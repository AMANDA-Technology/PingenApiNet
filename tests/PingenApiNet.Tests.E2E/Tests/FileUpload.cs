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
using PingenApiNet.Abstractions.Exceptions;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Files;
using PingenApiNet.Abstractions.Models.LetterEvents;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Letters.Embedded;
using PingenApiNet.Abstractions.Models.Letters.Views;

namespace PingenApiNet.Tests.E2E.Tests;

/// <summary>
///     End-to-end tests for the Pingen file-upload endpoint. Exercises the upload-URL handshake,
///     the upload + create-letter combination, the letter events listing, and file download paths
///     against the live staging API. Each letter created during the fixture is registered in the
///     LIFO cleanup queue so it is cancelled or deleted at fixture teardown.
/// </summary>
[TestFixture]
[Category("E2e")]
public sealed class TestGetFileUploadData : E2eTestBase
{
    /// <summary>
    ///     Verifies that the file-upload signed URL handshake returns a non-null payload.
    /// </summary>
    [Test]
    public async Task GetUploadData()
    {
        PingenApiClient.ShouldNotBeNull();

        ApiResult<SingleResult<FileUploadData>> res = await PingenApiClient!.Files.GetPath();
        AssertSuccess(res);
    }

    /// <summary>
    ///     Uploads the local sample PDF to the signed URL, creates a letter that references the
    ///     upload, polls for the letter to reach <see cref="LetterStates.Valid" />, and submits it
    ///     via the send endpoint. The created letter is registered in the LIFO cleanup queue and
    ///     will be cancelled or deleted at fixture teardown.
    /// </summary>
    [Test]
    public async Task GetUploadDataAndCreateLetter()
    {
        const string fileName = "sample.pdf";
        PingenApiClient.ShouldNotBeNull();

        ApiResult<SingleResult<FileUploadData>> res = await PingenApiClient!.Files.GetPath();
        AssertSuccess(res);

        MemoryStream stream = new();
        await File.OpenRead($"Assets/{fileName}").CopyToAsync(stream);
        ExternalRequestResult uploadRes = await PingenApiClient.Files.UploadFile(res.Data!.Data, stream);

        uploadRes.ShouldSatisfyAllConditions(
            () => uploadRes.IsSuccess.ShouldBeTrue(),
            () => uploadRes.StatusCode.ShouldBe(HttpStatusCode.OK)
        );

        var letterMetaData = new LetterMetaData
        {
            Recipient = new LetterMetaDataContact
            {
                Name = $"{TestPrefix}-recipient",
                Street = "solecht",
                Number = "42",
                Zip = "3303",
                City = "jegenstorf",
                Country = "CH"
            },
            Sender = new LetterMetaDataContact
            {
                Name = $"{TestPrefix}-sender",
                Street = "Musterstrasse ",
                Number = "12",
                Zip = "1212",
                City = "Musterhausen",
                Country = "CH"
            }
        };

        ApiResult<SingleResult<LetterDataDetailed>> resLetter = await PingenApiClient.Letters.Create(
            new DataPost<LetterCreate, LetterCreateRelationships>
            {
                Attributes = new LetterCreate
                {
                    FileOriginalName = $"{TestPrefix}-{fileName}",
                    FileUrl = res.Data.Data.Attributes.Url,
                    FileUrlSignature = res.Data.Data.Attributes.UrlSignature,
                    AddressPosition = LetterAddressPosition.left,
                    AutoSend = false,
                    DeliveryProduct = LetterCreateDeliveryProduct.Cheap,
                    PrintMode = LetterPrintMode.simplex,
                    PrintSpectrum = LetterPrintSpectrum.grayscale,
                    MetaData = letterMetaData
                },
                Type = PingenApiDataType.letters,
                Relationships = LetterCreateRelationships.Create("1234567890")
            });

        AssertSuccess(resLetter);

        string letterId = resLetter.Data!.Data.Id;
        IPingenApiClient cleanupClient = PingenApiClient;
        RegisterCleanup(async () => await TryCleanupLetter(cleanupClient, letterId));

        ApiResult<SingleResult<LetterDataDetailed>> letterFromRemote = await PingenApiClient.Letters.Get(letterId);
        letterFromRemote.ShouldNotBeNull();

        ApiResult<CollectionResult<LetterEventData>> letterEvents =
            await PingenApiClient.Letters.GetEventsPage(letterId, PingenApiLanguage.EnGB);
        letterEvents.ShouldNotBeNull();

        const int attempts = 300;
        const int delaySeconds = 1;
        LetterDataDetailed? letter = null;
        for (int attempt = 1; attempt <= attempts; attempt++)
        {
            ApiResult<SingleResult<LetterDataDetailed>> resultGetLetter = await PingenApiClient.Letters.Get(letterId);
            if (resultGetLetter.IsSuccess)
            {
                string? status = resultGetLetter.Data?.Data.Attributes.Status;
                if (status is LetterStates.Valid)
                {
                    letter = resultGetLetter.Data!.Data;
                    break;
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        }

        letter.ShouldNotBeNull();
        letter.Attributes.Status.ShouldBe(LetterStates.Valid);

        ApiResult<SingleResult<LetterDataDetailed>> resSendLetter = await PingenApiClient.Letters.Send(
            new DataPatch<LetterSend>
            {
                Type = PingenApiDataType.letters,
                Attributes = new LetterSend
                {
                    DeliveryProduct = LetterSendDeliveryProduct.PostAgA,
                    PrintMode = LetterPrintMode.simplex,
                    PrintSpectrum = LetterPrintSpectrum.color,
                    MetaData = letterMetaData
                },
                Id = letterId
            });

        AssertSuccess(resSendLetter);
    }

    /// <summary>
    ///     Verifies that letter events can be retrieved in every supported language.
    /// </summary>
    [Test]
    public async Task GetLetterEvents()
    {
        PingenApiClient.ShouldNotBeNull();
        const string letterId = "1540e30d-84cd-4425-bcc1-c3aff196d4da";

        foreach (string language in new[]
                 {
                     PingenApiLanguage.EnGB, PingenApiLanguage.DeDE, PingenApiLanguage.DeCH, PingenApiLanguage.NlNL,
                     PingenApiLanguage.FrFR
                 })
        {
            ApiResult<CollectionResult<LetterEventData>> res =
                await PingenApiClient!.Letters.GetEventsPage(letterId, language);
            AssertSuccess(res);
        }
    }

    /// <summary>
    ///     Verifies that an expired download URL produces a <see cref="PingenFileDownloadException" />.
    /// </summary>
    [Test]
    public async Task GetFileDownloadError()
    {
        PingenApiClient.ShouldNotBeNull();
        const string url =
            "https://pingen2-staging.objects.rma.cloudscale.ch/letters/20c1e673-03fe-4e19-ad45-9cdd85c5a940?X-Amz-Content-Sha256=UNSIGNED-PAYLOAD&X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=Z3YMWIXX6Y1G0KHUQDZ7%2F20221128%2Fregion1%2Fs3%2Faws4_request&X-Amz-Date=20221128T222323Z&X-Amz-SignedHeaders=host&X-Amz-Expires=86400&X-Amz-Signature=02705e5180cd082c5907d93e77fdb2d8c5a77938bc2e91a6e7c014ef953db9dd";

        await Should.ThrowAsync<PingenFileDownloadException>(async () =>
            await PingenApiClient!.Letters.DownloadFileContent(new Uri(url)));
    }

    /// <summary>
    ///     Resolves the signed download URL for a known letter, downloads the resulting file content,
    ///     and verifies the content can be persisted to disk.
    /// </summary>
    [Test]
    public async Task GetFileDownload()
    {
        PingenApiClient.ShouldNotBeNull();
        const string letterId = "1540e30d-84cd-4425-bcc1-c3aff196d4da";
        const string filePath = "filepath.pdf";

        ApiResult res = await PingenApiClient!.Letters.GetFileLocation(letterId);
        res.ShouldNotBeNull();
        res.ShouldSatisfyAllConditions(
            () => res.IsSuccess.ShouldBeTrue(),
            () => res.ApiError.ShouldBeNull(),
            () => res.Location.ShouldNotBeNull()
        );

        Stream stream = await PingenApiClient.Letters.DownloadFileContent(res.Location!);
        await using (FileStream file = File.OpenWrite(filePath))
        {
            await stream.CopyToAsync(file);
        }

        File.Exists(filePath).ShouldBeTrue();
        File.Delete(filePath);
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
            // Best-effort cleanup: a single residual letter must not fail the fixture.
        }
    }
}
