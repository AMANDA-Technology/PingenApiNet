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

using PingenApiNet.Abstractions.Exceptions;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.DeliveryProducts;
using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace PingenApiNet.Tests.Integration.Tests;

/// <summary>
///     Integration tests for <see cref="IDistributionService" />.
/// </summary>
[TestFixture]
public sealed class DistributionServiceTests : IntegrationTestBase
{
    /// <summary>
    ///     Verifies that GetDeliveryProductsPage returns a single page of products.
    /// </summary>
    [Test]
    public async Task GetDeliveryProductsPage_ShouldReturnProducts()
    {
        Server.StubJsonGet(
            OrgPath("distribution/delivery-products"),
            PingenResponseFactory.DeliveryProductCollection(3));

        ApiResult<CollectionResult<DeliveryProductData>> result = await Client.Distributions.GetDeliveryProductsPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(3),
            () => result.Data!.Data[0].Attributes.PriceCurrency.ShouldBe("CHF"));
    }

    /// <summary>
    ///     Verifies that GetDeliveryProductsPage returns an empty collection when none exist.
    /// </summary>
    [Test]
    public async Task GetDeliveryProductsPage_ShouldReturnEmptyWhenNoProducts()
    {
        Server.StubJsonGet(
            OrgPath("distribution/delivery-products"),
            PingenResponseFactory.DeliveryProductCollection(0));

        ApiResult<CollectionResult<DeliveryProductData>> result = await Client.Distributions.GetDeliveryProductsPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(0));
    }

    /// <summary>
    ///     Verifies that GetDeliveryProductsPage exposes pagination meta on a multi-page response.
    /// </summary>
    [Test]
    public async Task GetDeliveryProductsPage_ShouldExposePaginationMeta_ForMultiPageResponse()
    {
        Server.StubJsonGet(
            OrgPath("distribution/delivery-products"),
            PingenResponseFactory.DeliveryProductCollection(4, 1, 2));

        ApiResult<CollectionResult<DeliveryProductData>> result = await Client.Distributions.GetDeliveryProductsPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Meta.CurrentPage.ShouldBe(1),
            () => result.Data!.Meta.LastPage.ShouldBe(2));
    }

    /// <summary>
    ///     Verifies that GetDeliveryProductsPage returns an unsuccessful ApiResult on API errors.
    /// </summary>
    [TestCase(401)]
    [TestCase(403)]
    [TestCase(500)]
    [TestCase(503)]
    public async Task GetDeliveryProductsPage_OnApiError_ShouldReturnUnsuccessfulApiResult(int statusCode)
    {
        Server.StubError(
            OrgPath("distribution/delivery-products"),
            "GET",
            PingenResponseFactory.ErrorResponse(status: statusCode.ToString()),
            statusCode);

        ApiResult<CollectionResult<DeliveryProductData>> result = await Client.Distributions.GetDeliveryProductsPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors.Count.ShouldBeGreaterThan(0),
            () => result.Data.ShouldBeNull());
    }

    /// <summary>
    ///     Verifies that GetDeliveryProductsPageResultsAsync auto-paginates across two pages.
    /// </summary>
    [Test]
    public async Task GetDeliveryProductsPageResultsAsync_ShouldAutoPaginate()
    {
        Server
            .Given(Request.Create()
                .WithPath(OrgPath("distribution/delivery-products"))
                .UsingGet())
            .InScenario("dist-paging")
            .WillSetStateTo("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(PingenResponseFactory.DeliveryProductCollection(
                    1, 1, 2)));

        Server
            .Given(Request.Create()
                .WithPath(OrgPath("distribution/delivery-products"))
                .UsingGet())
            .InScenario("dist-paging")
            .WhenStateIs("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(PingenResponseFactory.DeliveryProductCollection(
                    1, 2, 2)));

        var allItems = new List<string>();
        await foreach (IEnumerable<DeliveryProductData> page in
                       Client.Distributions.GetDeliveryProductsPageResultsAsync())
            allItems.AddRange(page.Select(item => item.Id));

        allItems.Count.ShouldBe(2);
    }

    /// <summary>
    ///     Verifies that GetDeliveryProductsPageResultsAsync stops after a single page when only
    ///     one page is available.
    /// </summary>
    [Test]
    public async Task GetDeliveryProductsPageResultsAsync_ShouldYieldSinglePage_WhenOnlyOneExists()
    {
        Server.StubJsonGet(
            OrgPath("distribution/delivery-products"),
            PingenResponseFactory.DeliveryProductCollection(2));

        var allItems = new List<string>();
        await foreach (IEnumerable<DeliveryProductData> page in
                       Client.Distributions.GetDeliveryProductsPageResultsAsync())
            allItems.AddRange(page.Select(item => item.Id));

        allItems.Count.ShouldBe(2);
    }

    /// <summary>
    ///     Verifies that GetDeliveryProductsPageResultsAsync surfaces a <see cref="PingenApiErrorException" />
    ///     when the underlying call fails.
    /// </summary>
    [Test]
    public async Task GetDeliveryProductsPageResultsAsync_OnApiError_ShouldThrowPingenApiErrorException()
    {
        Server.StubError(
            OrgPath("distribution/delivery-products"),
            "GET",
            PingenResponseFactory.ErrorResponse("Server Error", "Service unavailable", "500"),
            500);

        PingenApiErrorException exception = await Should.ThrowAsync<PingenApiErrorException>(async () =>
        {
            await foreach (IEnumerable<DeliveryProductData> _ in Client.Distributions
                               .GetDeliveryProductsPageResultsAsync())
            {
                // Should never iterate
            }
        });

        exception.ApiResult.ShouldNotBeNull();
        exception.ApiResult!.IsSuccess.ShouldBeFalse();
    }
}
