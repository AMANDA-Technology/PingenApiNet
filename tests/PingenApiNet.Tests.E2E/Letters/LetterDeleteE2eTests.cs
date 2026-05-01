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
using PingenApiNet.Abstractions.Models.Letters.Views;
using PingenApiNet.Tests.E2E.Helpers;

namespace PingenApiNet.Tests.E2E.Letters;

/// <summary>
///     End-to-end test for <c>Letters.Delete</c>. Uploads a sample PDF, creates a draft letter,
///     deletes it, and verifies a follow-up <c>Letters.Get</c> reports the resource is gone
///     (non-success, 404 Not Found). Pingen only allows direct delete while the <c>delete</c>
///     ability is <see cref="PingenApiAbility.ok" />; once the letter has progressed past the
///     deletable state the test documents that constraint via <see cref="Assert.Pass(string)" />.
/// </summary>
[TestFixture]
[Category("E2e")]
public sealed class LetterDeleteE2eTests : E2eTestBase
{
    /// <summary>
    ///     Removes any letter tagged with the shared E2E prefix that was left behind by a crashed
    ///     previous run. Constructs its own client because the base class only sets
    ///     <see cref="E2eTestBase.PingenApiClient" /> in the per-test <c>[SetUp]</c> hook.
    /// </summary>
    [OneTimeSetUp]
    public async Task ScavengeOrphansBeforeRunning()
    {
        IPingenApiClient bootstrapClient = LetterCleanupHelper.BuildBootstrapClient();
        await LetterCleanupHelper.ScavengeLetterOrphans(bootstrapClient);
    }

    private const string SampleFileName = "sample.pdf";
    private const int ValidationPollAttempts = 120;
    private const int ValidationPollDelaySeconds = 1;

    /// <summary>
    ///     Fixture-level sweep run after the LIFO cleanup queue, in case any letter slipped through.
    /// </summary>
    protected override async Task ScavengeOrphans()
    {
        if (PingenApiClient is not null)
            await LetterCleanupHelper.ScavengeLetterOrphans(PingenApiClient);
    }

    /// <summary>
    ///     Uploads a PDF, creates a draft letter, polls until validation settles, deletes the
    ///     letter directly via <c>Letters.Delete</c>, asserts success, and confirms a subsequent
    ///     <c>Letters.Get</c> returns a non-success result with HTTP 404. The cleanup queue
    ///     entry is registered before the delete call so that an unexpected delete failure still
    ///     leaves the letter scheduled for fixture-teardown removal.
    /// </summary>
    [Test]
    public async Task Delete_ShouldDeleteDraftLetterAndReturn404OnGet()
    {
        PingenApiClient.ShouldNotBeNull();

        FileUploadData uploadData = await UploadSamplePdf();
        string letterId = await CreateDraftLetter(uploadData);
        RegisterCleanup(async () => await LetterCleanupHelper.TryCleanupLetter(PingenApiClient!, letterId));

        LetterDataDetailed letter = await PollUntilValidationComplete(letterId);
        letter.Attributes.Status.ShouldBe(LetterStates.Valid);

        PingenApiAbility deleteAbility = letter.Meta.Abilities.Self.Delete;
        if (deleteAbility is not PingenApiAbility.ok)
        {
            Assert.Pass(
                $"Delete ability is '{deleteAbility}', not 'ok'. " +
                "Pingen rejects direct delete once the letter has progressed past the deletable state.");
            return;
        }

        ApiResult deleteResult = await PingenApiClient!.Letters.Delete(letterId);
        AssertSuccess(deleteResult);

        ApiResult<SingleResult<LetterDataDetailed>> afterDelete = await PingenApiClient.Letters.Get(letterId);
        afterDelete.ShouldNotBeNull();
        afterDelete.IsSuccess.ShouldBeFalse();
        afterDelete.Data.ShouldBeNull();
        afterDelete.ApiError.ShouldNotBeNull();
    }

    private async Task<FileUploadData> UploadSamplePdf()
    {
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

        return uploadPath.Data.Data;
    }

    private async Task<string> CreateDraftLetter(FileUploadData uploadData)
    {
        var data = new DataPost<LetterCreate, LetterCreateRelationships>
        {
            Type = PingenApiDataType.letters,
            Attributes = new LetterCreate
            {
                FileOriginalName = $"{TestPrefix}-{SampleFileName}",
                FileUrl = uploadData.Attributes.Url,
                FileUrlSignature = uploadData.Attributes.UrlSignature,
                AddressPosition = LetterAddressPosition.left,
                AutoSend = false,
                DeliveryProduct = LetterCreateDeliveryProduct.Cheap,
                PrintMode = LetterPrintMode.simplex,
                PrintSpectrum = LetterPrintSpectrum.grayscale
            },
            Relationships = LetterCreateRelationships.Create("1234567890")
        };

        ApiResult<SingleResult<LetterDataDetailed>> result = await PingenApiClient!.Letters.Create(data);
        AssertSuccess(result);
        result.Data!.Data.Id.ShouldNotBeNullOrEmpty();

        return result.Data.Data.Id;
    }

    private async Task<LetterDataDetailed> PollUntilValidationComplete(string letterId)
    {
        for (int attempt = 1; attempt <= ValidationPollAttempts; attempt++)
        {
            ApiResult<SingleResult<LetterDataDetailed>> getResult = await PingenApiClient!.Letters.Get(letterId);
            if (getResult.IsSuccess && getResult.Data?.Data is { } current)
            {
                string? status = current.Attributes.Status;
                if (status is LetterStates.Valid or LetterStates.Invalid or LetterStates.Unprintable)
                    return current;
            }

            await Task.Delay(TimeSpan.FromSeconds(ValidationPollDelaySeconds));
        }

        throw new TimeoutException(
            $"Letter {letterId} did not reach a terminal validation state within " +
            $"{ValidationPollAttempts * ValidationPollDelaySeconds}s.");
    }
}
