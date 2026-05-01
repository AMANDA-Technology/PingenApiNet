/*
MIT License

Copyright (c) 2022 Philip Näf <philip.naef@amanda-technology.ch>
Copyright (c) 2022 Manuel Gysin <manuel.gysin@amanda-technology.ch>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Net;
using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Enums.Letters;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Files;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Letters.Embedded;
using PingenApiNet.Abstractions.Models.Letters.Views;
using PingenApiNet.Services.Connectors;

namespace PingenApiNet.Tests.E2E.Letters;

/// <summary>
///     End-to-end tests for the full Pingen letter workflow. Uploads a sample PDF, creates a
///     letter that references the upload, polls for the letter to reach the <c>valid</c> state,
///     submits it via the send endpoint, and (when the API still allows it) cancels the letter.
///     Orphan letters left behind by crashed prior runs are scavenged once before the fixture
///     starts so they cannot pollute state across runs.
/// </summary>
[TestFixture]
[Category("E2e")]
public sealed class LetterWorkflowE2eTests : E2eTestBase
{
    /// <summary>
    ///     Removes any letter tagged with <see cref="TestPrefixIdentifier" /> in its file name that
    ///     was left behind by a crashed previous run. Runs once before the fixture begins; constructs
    ///     its own client because the base class only sets <see cref="E2eTestBase.PingenApiClient" />
    ///     in the per-test <c>[SetUp]</c> hook.
    /// </summary>
    [OneTimeSetUp]
    public async Task ScavengeOrphansBeforeRunning()
    {
        IPingenApiClient bootstrapClient = BuildBootstrapClient();
        await ScavengeLetterOrphans(bootstrapClient);
    }

    private const string SampleFileName = "sample.pdf";
    private const string TestPrefixIdentifier = "PG-E2E-";
    private const int ValidationPollAttempts = 120;
    private const int ValidationPollDelaySeconds = 1;

    private FileUploadData? _uploadData;
    private string? _createdLetterId;

    /// <summary>
    ///     Uploads the local sample PDF using the path returned by <c>Files.GetPath</c>. The resulting
    ///     <see cref="FileUploadData" /> is shared with subsequent ordered tests. Files are immutable on
    ///     the Pingen storage layer, so no cleanup is registered for the upload itself.
    /// </summary>
    [Test]
    [Order(1)]
    public async Task UploadFile_ShouldUploadPdf()
    {
        PingenApiClient.ShouldNotBeNull();

        ApiResult<SingleResult<FileUploadData>> uploadPath = await PingenApiClient!.Files.GetPath();
        AssertSuccess(uploadPath);

        await using var stream = new MemoryStream();
        await using (FileStream fileStream = File.OpenRead($"Assets/{SampleFileName}"))
        {
            await fileStream.CopyToAsync(stream);
        }

        ExternalRequestResult uploadResult = await PingenApiClient.Files.UploadFile(uploadPath.Data!.Data, stream);

        uploadResult.ShouldSatisfyAllConditions(
            () => uploadResult.IsSuccess.ShouldBeTrue(),
            () => uploadResult.StatusCode.ShouldBe(HttpStatusCode.OK)
        );

        _uploadData = uploadPath.Data.Data;
    }

    /// <summary>
    ///     Creates a letter that references the uploaded file with <c>auto_send=false</c>. The
    ///     recipient and file name are tagged with <see cref="E2eTestBase.TestPrefix" /> so orphans
    ///     can be identified later. A best-effort cleanup is registered to cancel or delete the
    ///     letter at fixture teardown.
    /// </summary>
    [Test]
    [Order(2)]
    public async Task Create_ShouldCreateLetterInDraft()
    {
        PingenApiClient.ShouldNotBeNull();
        _uploadData.ShouldNotBeNull();

        var metaData = new LetterMetaData
        {
            Recipient = new LetterMetaDataContact
            {
                Name = $"{TestPrefix}-recipient",
                Street = "Teststrasse",
                Number = "1",
                Zip = "3303",
                City = "Jegenstorf",
                Country = "CH"
            },
            Sender = new LetterMetaDataContact
            {
                Name = $"{TestPrefix}-sender",
                Street = "Musterstrasse",
                Number = "2",
                Zip = "1212",
                City = "Musterhausen",
                Country = "CH"
            }
        };

        var data = new DataPost<LetterCreate, LetterCreateRelationships>
        {
            Type = PingenApiDataType.letters,
            Attributes = new LetterCreate
            {
                FileOriginalName = $"{TestPrefix}-{SampleFileName}",
                FileUrl = _uploadData!.Attributes.Url,
                FileUrlSignature = _uploadData.Attributes.UrlSignature,
                AddressPosition = LetterAddressPosition.left,
                AutoSend = false,
                DeliveryProduct = LetterCreateDeliveryProduct.Cheap,
                PrintMode = LetterPrintMode.simplex,
                PrintSpectrum = LetterPrintSpectrum.grayscale,
                MetaData = metaData
            },
            Relationships = LetterCreateRelationships.Create("1234567890")
        };

        ApiResult<SingleResult<LetterDataDetailed>> result = await PingenApiClient!.Letters.Create(data);

        AssertSuccess(result);
        result.Data!.Data.Id.ShouldNotBeNullOrEmpty();
        result.Data.Data.Attributes.FileOriginalName.ShouldNotBeNull();
        result.Data.Data.Attributes.FileOriginalName!.ShouldContain(TestPrefix);

        _createdLetterId = result.Data.Data.Id;

        string idForCleanup = _createdLetterId;
        RegisterCleanup(async () => await TryCleanupLetter(PingenApiClient!, idForCleanup));
    }

    /// <summary>
    ///     Polls <c>Letters.Get</c> until the letter created by <see cref="Create_ShouldCreateLetterInDraft" />
    ///     reaches the <see cref="LetterStates.Valid" /> state. Pingen runs PDF validation asynchronously,
    ///     so this waits for the terminal validation state before downstream tests can send the letter.
    /// </summary>
    [Test]
    [Order(3)]
    public async Task Get_ShouldShowLetterValidAfterPolling()
    {
        PingenApiClient.ShouldNotBeNull();
        _createdLetterId.ShouldNotBeNullOrEmpty();

        LetterDataDetailed? letter = null;
        for (int attempt = 1; attempt <= ValidationPollAttempts; attempt++)
        {
            ApiResult<SingleResult<LetterDataDetailed>> getResult =
                await PingenApiClient!.Letters.Get(_createdLetterId!);

            if (getResult.IsSuccess && getResult.Data?.Data is { } current)
            {
                string? status = current.Attributes.Status;
                if (status is LetterStates.Valid or LetterStates.Invalid or LetterStates.Unprintable)
                {
                    letter = current;
                    break;
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(ValidationPollDelaySeconds));
        }

        letter.ShouldNotBeNull();
        letter.Attributes.Status.ShouldBe(LetterStates.Valid);
    }

    /// <summary>
    ///     Submits the validated letter via <c>Letters.Send</c> with a Swiss <c>postag_b</c> product and
    ///     verifies that the resulting status is no longer <see cref="LetterStates.Valid" />, proving
    ///     the state machine moved forward. Concrete next states (sending, submitted, processing, sent)
    ///     are timing-dependent on the staging API, so the assertion only checks that progress was made.
    /// </summary>
    [Test]
    [Order(4)]
    public async Task Send_ShouldTransitionLetterBeyondValid()
    {
        PingenApiClient.ShouldNotBeNull();
        _createdLetterId.ShouldNotBeNullOrEmpty();

        var sendData = new DataPatch<LetterSend>
        {
            Id = _createdLetterId!,
            Type = PingenApiDataType.letters,
            Attributes = new LetterSend
            {
                DeliveryProduct = LetterSendDeliveryProduct.PostAgB,
                PrintMode = LetterPrintMode.simplex,
                PrintSpectrum = LetterPrintSpectrum.grayscale
            }
        };

        ApiResult<SingleResult<LetterDataDetailed>> result = await PingenApiClient!.Letters.Send(sendData);

        AssertSuccess(result);
        result.Data!.Data.Id.ShouldBe(_createdLetterId);
        result.Data.Data.Attributes.Status.ShouldNotBe(LetterStates.Valid);
    }

    /// <summary>
    ///     Cancels the letter when the API reports cancel as <see cref="PingenApiAbility.ok" />. Once a
    ///     letter has progressed too far in the pipeline the cancel ability disappears; in that case the
    ///     test is skipped so the fixture remains green even on fast staging environments. The LIFO
    ///     cleanup queue still removes the letter at teardown.
    /// </summary>
    [Test]
    [Order(5)]
    public async Task Cancel_ShouldCancelLetterIfStillAllowed()
    {
        PingenApiClient.ShouldNotBeNull();
        _createdLetterId.ShouldNotBeNullOrEmpty();

        ApiResult<SingleResult<LetterDataDetailed>> current =
            await PingenApiClient!.Letters.Get(_createdLetterId!);
        AssertSuccess(current);

        PingenApiAbility cancelAbility = current.Data!.Data.Meta.Abilities.Self.Cancel;
        if (cancelAbility is not PingenApiAbility.ok)
        {
            Assert.Pass($"Cancel not currently allowed (ability={cancelAbility}). Cleanup queue will handle removal.");
            return;
        }

        ApiResult cancelResult = await PingenApiClient.Letters.Cancel(_createdLetterId!);
        AssertSuccess(cancelResult);
    }

    /// <summary>
    ///     End-of-fixture sweep that catches any letter still tagged with
    ///     <see cref="TestPrefixIdentifier" /> after the LIFO queue has run.
    /// </summary>
    protected override async Task ScavengeOrphans()
    {
        if (PingenApiClient is not null)
            await ScavengeLetterOrphans(PingenApiClient);
    }

    private static async Task ScavengeLetterOrphans(IPingenApiClient client)
    {
        // Walk recent pages only — a full org-wide sweep is too expensive on staging.
        const int maxPagesToScan = 5;
        var orphanIds = new List<string>();
        int pagesScanned = 0;

        await foreach (IEnumerable<LetterData> page in client.Letters.GetPageResultsAsync())
        {
            pagesScanned++;
            foreach (LetterData letter in page)
            {
                string fileOriginalName = letter.Attributes.FileOriginalName ?? string.Empty;
                string address = letter.Attributes.Address ?? string.Empty;
                if (fileOriginalName.Contains(TestPrefixIdentifier) || address.Contains(TestPrefixIdentifier))
                    orphanIds.Add(letter.Id);
            }

            if (pagesScanned >= maxPagesToScan)
                break;
        }

        foreach (string id in orphanIds)
            await TryCleanupLetter(client, id);
    }

    private static async Task TryCleanupLetter(IPingenApiClient client, string letterId)
    {
        try
        {
            ApiResult<SingleResult<LetterDataDetailed>> current = await client.Letters.Get(letterId);
            if (!current.IsSuccess || current.Data?.Data is not { } detail)
                return;

            LetterAbilities abilities = detail.Meta.Abilities.Self;
            if (abilities.Cancel is PingenApiAbility.ok)
                await client.Letters.Cancel(letterId);
            else if (abilities.Delete is PingenApiAbility.ok)
                await client.Letters.Delete(letterId);
        }
        catch (Exception)
        {
            // Best-effort cleanup: a single orphan that resists removal must not fail the fixture.
        }
    }

    private static PingenApiClient BuildBootstrapClient()
    {
        var configuration = new PingenConfiguration
        {
            BaseUri =
                Environment.GetEnvironmentVariable("PingenApiNet__BaseUri") ??
                throw new InvalidOperationException("Missing PingenApiNet__BaseUri"),
            IdentityUri =
                Environment.GetEnvironmentVariable("PingenApiNet__IdentityUri") ??
                throw new InvalidOperationException("Missing PingenApiNet__IdentityUri"),
            ClientId =
                Environment.GetEnvironmentVariable("PingenApiNet__ClientId") ??
                throw new InvalidOperationException("Missing PingenApiNet__ClientId"),
            ClientSecret =
                Environment.GetEnvironmentVariable("PingenApiNet__ClientSecret") ??
                throw new InvalidOperationException("Missing PingenApiNet__ClientSecret"),
            DefaultOrganisationId =
                Environment.GetEnvironmentVariable("PingenApiNet__OrganisationId") ??
                throw new InvalidOperationException("Missing PingenApiNet__OrganisationId")
        };

        var connectionHandler = new PingenConnectionHandler(configuration, PingenHttpClients.Create(configuration));

        return new PingenApiClient(
            connectionHandler,
            new LetterService(connectionHandler),
            new BatchService(connectionHandler),
            new UserService(connectionHandler),
            new OrganisationService(connectionHandler),
            new WebhookService(connectionHandler),
            new FilesService(connectionHandler),
            new DistributionService(connectionHandler));
    }
}
