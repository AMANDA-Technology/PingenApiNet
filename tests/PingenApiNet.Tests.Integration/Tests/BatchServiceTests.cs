/*
MIT License

Copyright (c) 2024 Dejan Appenzeller <dejan.appenzeller@swisspeers.ch>

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

using Bogus;
using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Enums.Batches;
using PingenApiNet.Abstractions.Enums.Letters;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Batches;
using PingenApiNet.Abstractions.Models.Batches.Views;
using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Tests.Integration.Helpers;

namespace PingenApiNet.Tests.Integration.Tests;

/// <summary>
///     Integration tests for <see cref="IBatchService" />.
/// </summary>
[TestFixture]
public sealed class BatchServiceTests : IntegrationTestBase
{
    /// <summary>
    ///     Builds a Bogus faker for batch create payloads.
    /// </summary>
    private static Faker<DataPost<BatchCreate, BatchCreateRelationships>> BatchCreateFaker()
    {
        return new Faker<DataPost<BatchCreate, BatchCreateRelationships>>()
            .RuleFor(x => x.Type, PingenApiDataType.batches)
            .RuleFor(x => x.Attributes,
                f => new BatchCreate
                {
                    Name = f.Commerce.ProductName(),
                    Icon = f.PickRandom<BatchIcon>(),
                    FileOriginalName = f.System.FileName("pdf"),
                    FileUrl = f.Internet.Url(),
                    FileUrlSignature = f.Random.Hash(),
                    AddressPosition = f.PickRandom<LetterAddressPosition>(),
                    GroupingType = f.PickRandom<BatchGroupingType>(),
                    GroupingOptionsSplitType = f.PickRandom<BatchGroupingOptionsSplitType>()
                })
            .RuleFor(x => x.Relationships, f => BatchCreateRelationships.Create(f.Random.Guid().ToString()));
    }

    /// <summary>
    ///     Verifies that GetPage returns a paginated list of batches.
    /// </summary>
    [Test]
    public async Task GetPage_ShouldReturnBatches()
    {
        Server.StubJsonGet(OrgPath("batches"), PingenResponseFactory.BatchCollection());

        ApiResult<CollectionResult<BatchData>> result = await Client.Batches.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(3),
            () => result.Data!.Data[0].Attributes.Name.ShouldNotBeNullOrEmpty());
    }

    /// <summary>
    ///     Verifies that GetPage returns an empty collection when no batches exist.
    /// </summary>
    [Test]
    public async Task GetPage_ShouldReturnEmptyWhenNoBatches()
    {
        Server.StubJsonGet(OrgPath("batches"), PingenResponseFactory.BatchCollection(0));

        ApiResult<CollectionResult<BatchData>> result = await Client.Batches.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(0));
    }

    /// <summary>
    ///     Verifies that GetPage returns an unsuccessful ApiResult when the API responds with an error.
    /// </summary>
    [TestCase(401)]
    [TestCase(403)]
    [TestCase(500)]
    [TestCase(503)]
    public async Task GetPage_OnApiError_ShouldReturnUnsuccessfulApiResult(int statusCode)
    {
        Server.StubError(
            OrgPath("batches"),
            "GET",
            PingenResponseFactory.ErrorResponse(status: statusCode.ToString()),
            statusCode);

        ApiResult<CollectionResult<BatchData>> result = await Client.Batches.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors.Count.ShouldBeGreaterThan(0),
            () => result.Data.ShouldBeNull());
    }

    /// <summary>
    ///     Verifies that Get returns a single batch with detailed attributes.
    /// </summary>
    [Test]
    public async Task Get_ShouldReturnSingleBatch()
    {
        string batchId = Guid.NewGuid().ToString();

        Server.StubJsonGet(OrgPath($"batches/{batchId}"), PingenResponseFactory.SingleBatch(batchId));

        ApiResult<SingleResult<BatchDataDetailed>> result = await Client.Batches.Get(batchId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(batchId),
            () => result.Data!.Data.Attributes.Name.ShouldNotBeNullOrEmpty());
    }

    /// <summary>
    ///     Verifies that Get surfaces a 404 not-found error via ApiResult.
    /// </summary>
    [Test]
    public async Task Get_ShouldReturnErrorWhenNotFound()
    {
        string batchId = Guid.NewGuid().ToString();

        Server.StubError(
            OrgPath($"batches/{batchId}"),
            "GET",
            PingenResponseFactory.ErrorResponse("Not Found", "The requested batch does not exist.", "404"),
            404);

        ApiResult<SingleResult<BatchDataDetailed>> result = await Client.Batches.Get(batchId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors[0].Title.ShouldBe("Not Found"),
            () => result.Data.ShouldBeNull());
    }

    /// <summary>
    ///     Verifies that Create posts a new batch and returns the created resource.
    /// </summary>
    [Test]
    public async Task Create_ShouldPostBatchAndReturnResult()
    {
        string batchId = Guid.NewGuid().ToString();

        Server.StubJsonPost(OrgPath("batches"), PingenResponseFactory.SingleBatch(batchId));

        DataPost<BatchCreate, BatchCreateRelationships>? data = BatchCreateFaker().Generate();

        ApiResult<SingleResult<BatchDataDetailed>> result = await Client.Batches.Create(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(batchId),
            () => result.Data!.Data.Attributes.Name.ShouldNotBeNullOrEmpty());
    }

    /// <summary>
    ///     Verifies that Create surfaces a validation error returned by the API.
    /// </summary>
    [Test]
    public async Task Create_ShouldReturnErrorWhenValidationFails()
    {
        Server.StubError(
            OrgPath("batches"),
            "POST",
            PingenResponseFactory.ErrorResponse("Validation Failed", "Name is required."));

        DataPost<BatchCreate, BatchCreateRelationships>? data = BatchCreateFaker().Generate();

        ApiResult<SingleResult<BatchDataDetailed>> result = await Client.Batches.Create(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors[0].Title.ShouldBe("Validation Failed"),
            () => result.Data.ShouldBeNull());
    }

    /// <summary>
    ///     Verifies that GetPage handles paged collection metadata for a multi-page response.
    /// </summary>
    [Test]
    public async Task GetPage_ShouldExposePaginationMeta_ForMultiPageResponse()
    {
        Server.StubJsonGet(
            OrgPath("batches"),
            PingenResponseFactory.BatchCollection(5, currentPage: 2, lastPage: 3));

        ApiResult<CollectionResult<BatchData>> result = await Client.Batches.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Meta.CurrentPage.ShouldBe(2),
            () => result.Data!.Meta.LastPage.ShouldBe(3),
            () => result.Data!.Data.Count.ShouldBe(5));
    }
}
