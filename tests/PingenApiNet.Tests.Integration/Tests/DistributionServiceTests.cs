using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace PingenApiNet.Tests.Integration.Tests;

/// <summary>
/// Integration tests for <see cref="IDistributionService"/>.
/// </summary>
[TestFixture]
public sealed class DistributionServiceTests : IntegrationTestBase
{
    [Test]
    public async Task GetDeliveryProductsPage_ShouldReturnProducts()
    {
        var productId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath("distribution/delivery-products"))
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [
                        (productId,
                            new
                            {
                                countries = new[] { "CH", "DE" },
                                name = "postag_a",
                                full_name = "PostAG A-Post",
                                delivery_time_days = new[] { 1, 2 },
                                features = new[] { "color", "duplex" },
                                price_currency = "CHF",
                                price_starting_from = 1.25
                            },
                            null,
                            null)
                    ],
                    "delivery_products")));

        var result = await Client.Distributions.GetDeliveryProductsPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(1),
            () => result.Data!.Data[0].Id.ShouldBe(productId),
            () => result.Data!.Data[0].Attributes.Name.ShouldBe("postag_a"),
            () => result.Data!.Data[0].Attributes.PriceCurrency.ShouldBe("CHF"));
    }

    [Test]
    public async Task GetDeliveryProductsPageResultsAsync_ShouldAutoPaginate()
    {
        var id1 = Guid.NewGuid().ToString();
        var id2 = Guid.NewGuid().ToString();

        // Page 1 — first call returns page 1 and transitions to "page2" state
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
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [
                        (id1,
                            new { name = "postag_a", full_name = "PostAG A-Post", price_currency = "CHF", price_starting_from = 1.25 },
                            null,
                            null)
                    ],
                    "delivery_products",
                    currentPage: 1,
                    lastPage: 2,
                    perPage: 1,
                    total: 2)));

        // Page 2 — second call (when state is "page2") returns page 2
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
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [
                        (id2,
                            new { name = "postag_b", full_name = "PostAG B-Post", price_currency = "CHF", price_starting_from = 0.95 },
                            null,
                            null)
                    ],
                    "delivery_products",
                    currentPage: 2,
                    lastPage: 2,
                    perPage: 1,
                    total: 2)));

        var allItems = new List<string>();
        await foreach (var page in Client.Distributions.GetDeliveryProductsPageResultsAsync())
        {
            allItems.AddRange(page.Select(item => item.Id));
        }

        allItems.ShouldSatisfyAllConditions(
            () => allItems.Count.ShouldBe(2),
            () => allItems.ShouldContain(id1),
            () => allItems.ShouldContain(id2));
    }
}
