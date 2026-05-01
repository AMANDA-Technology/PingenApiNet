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

using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Enums.Batches;
using PingenApiNet.Abstractions.Enums.Letters;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Batches;
using PingenApiNet.Abstractions.Models.Batches.Views;
using PingenApiNet.Abstractions.Models.Files;

namespace PingenApiNet.Tests.E2E.Batch;

/// <summary>
///     End-to-end tests for the Pingen batches endpoint. Exercises the 3-step batch creation
///     flow (file upload → batch create), single-resource lookup, and paginated listing
///     against the live staging API.
/// </summary>
[TestFixture]
[Category("E2e")]
public sealed class BatchE2eTests : E2eTestBase
{
    private const string SampleFileName = "sample.pdf";

    private string? _createdBatchId;

    /// <summary>
    ///     Creates a new batch by first uploading a sample PDF and then posting the batch payload.
    ///     The created id is shared with subsequent ordered tests for downstream lookups.
    /// </summary>
    [Test]
    [Order(1)]
    public async Task Create_ShouldCreateBatch()
    {
        PingenApiClient.ShouldNotBeNull();

        ApiResult<SingleResult<FileUploadData>> uploadPath =
            await PingenApiClient!.Files.GetPath();
        AssertSuccess(uploadPath);

        await using var stream = new MemoryStream();
        await using (FileStream fileStream = File.OpenRead($"Assets/{SampleFileName}"))
        {
            await fileStream.CopyToAsync(stream);
        }

        ExternalRequestResult uploadResult = await PingenApiClient.Files.UploadFile(uploadPath.Data!.Data, stream);
        uploadResult.IsSuccess.ShouldBeTrue();

        var data = new DataPost<BatchCreate, BatchCreateRelationships>
        {
            Type = PingenApiDataType.batches,
            Attributes = new BatchCreate
            {
                Name = $"{TestPrefix}-batch",
                Icon = BatchIcon.document,
                FileOriginalName = SampleFileName,
                FileUrl = uploadPath.Data.Data.Attributes.Url,
                FileUrlSignature = uploadPath.Data.Data.Attributes.UrlSignature,
                AddressPosition = LetterAddressPosition.left,
                GroupingType = BatchGroupingType.zip,
                GroupingOptionsSplitType = BatchGroupingOptionsSplitType.file
            },
            Relationships = BatchCreateRelationships.Create("1234567890")
        };

        ApiResult<SingleResult<BatchDataDetailed>> result = await PingenApiClient.Batches.Create(data);

        AssertSuccess(result);
        result.Data!.Data.Id.ShouldNotBeNullOrEmpty();
        result.Data.Data.Attributes.Name.ShouldBe($"{TestPrefix}-batch");

        _createdBatchId = result.Data.Data.Id;

        // IBatchService does not expose a Delete operation, so cleanup is recorded for tracking
        // and registers a no-op so the LIFO queue still reflects the creation.
        RegisterCleanup(() => Task.CompletedTask);
    }

    /// <summary>
    ///     Verifies that the batch created by <see cref="Create_ShouldCreateBatch" /> can be fetched by id.
    /// </summary>
    [Test]
    [Order(2)]
    public async Task Get_ShouldReturnBatchById()
    {
        PingenApiClient.ShouldNotBeNull();
        _createdBatchId.ShouldNotBeNullOrEmpty();

        ApiResult<SingleResult<BatchDataDetailed>> result = await PingenApiClient!.Batches.Get(_createdBatchId!);

        AssertSuccess(result);
        result.Data!.Data.Id.ShouldBe(_createdBatchId);
        result.Data.Data.Attributes.Name.ShouldBe($"{TestPrefix}-batch");
    }

    /// <summary>
    ///     Verifies that a paginated list of batches can be retrieved and exposes pagination meta.
    /// </summary>
    [Test]
    [Order(3)]
    public async Task GetPage_ShouldReturnPaginatedBatches()
    {
        PingenApiClient.ShouldNotBeNull();

        ApiResult<CollectionResult<BatchData>> result =
            await PingenApiClient!.Batches.GetPage(new ApiPagingRequest { PageNumber = 1, PageLimit = 20 });

        AssertSuccess(result);
        result.Data!.Data.ShouldNotBeNull();
        result.Data.Meta.CurrentPage.ShouldNotBeNull();
        result.Data.Meta.CurrentPage!.Value.ShouldBeGreaterThanOrEqualTo(1);
        result.Data.Meta.LastPage.ShouldNotBeNull();
        result.Data.Meta.LastPage!.Value.ShouldBeGreaterThanOrEqualTo(1);
    }
}
