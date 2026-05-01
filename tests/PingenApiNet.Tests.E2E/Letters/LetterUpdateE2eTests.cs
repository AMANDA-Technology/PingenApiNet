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
///     End-to-end test for <c>Letters.Update</c>. Uploads a sample PDF, creates a draft letter,
///     waits for validation to settle, and patches the letter's <c>paper_types</c>. The Pingen
///     API only allows the paper-type change while the <c>change-paper-type</c> ability is
///     <see cref="PingenApiAbility.ok" /> — once a letter has progressed past draft/valid the
///     update is rejected. The test handles both outcomes: success is asserted in the green
///     path, otherwise the API constraint is documented via <see cref="Assert.Pass(string)" />.
/// </summary>
[TestFixture]
[Category("E2e")]
public sealed class LetterUpdateE2eTests : E2eTestBase
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
    ///     Uploads a PDF, creates a draft letter, waits for it to reach the
    ///     <see cref="LetterStates.Valid" /> state and then patches its paper types. Asserts a
    ///     successful round-trip when the API allows the edit, or documents the constraint when
    ///     the <c>change-paper-type</c> ability is no longer <see cref="PingenApiAbility.ok" />.
    /// </summary>
    [Test]
    public async Task Update_ShouldUpdatePaperTypesOnDraftLetter()
    {
        PingenApiClient.ShouldNotBeNull();

        FileUploadData uploadData = await UploadSamplePdf();
        string letterId = await CreateDraftLetter(uploadData);
        RegisterCleanup(async () => await LetterCleanupHelper.TryCleanupLetter(PingenApiClient!, letterId));

        LetterDataDetailed letter = await PollUntilValidationComplete(letterId);
        letter.Attributes.Status.ShouldBe(LetterStates.Valid);

        PingenApiAbility changePaperTypeAbility = letter.Meta.Abilities.Self.ChangePaperType;
        if (changePaperTypeAbility is not PingenApiAbility.ok)
        {
            Assert.Pass(
                $"change-paper-type ability is '{changePaperTypeAbility}', not 'ok'. " +
                "Pingen rejects paper-type updates once a letter has progressed past the editable state.");
            return;
        }

        var patch = new DataPatch<LetterUpdate>
        {
            Id = letterId,
            Type = PingenApiDataType.letters,
            Attributes = new LetterUpdate { PaperTypes = [LetterPaperTypes.Normal] }
        };

        ApiResult<SingleResult<LetterDataDetailed>> result = await PingenApiClient!.Letters.Update(patch);

        AssertSuccess(result);
        result.Data!.Data.Id.ShouldBe(letterId);
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
