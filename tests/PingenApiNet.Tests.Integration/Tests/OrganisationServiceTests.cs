using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace PingenApiNet.Tests.Integration.Tests;

/// <summary>
/// Integration tests for <see cref="IOrganisationService"/>.
/// </summary>
[TestFixture]
public sealed class OrganisationServiceTests : IntegrationTestBase
{
    [Test]
    public async Task GetPage_ShouldReturnOrganisations()
    {
        var orgId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath("/organisations")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [
                        (orgId,
                            new { name = "Test Org", status = "active", plan = "professional", billing_mode = "prepaid", billing_currency = "CHF", billing_balance = 100.0, default_country = "CH" },
                            (object)new { associations = JsonApiStubHelper.RelatedMany() },
                            null)
                    ],
                    "organisations")));

        var result = await Client.Organisations.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(1),
            () => result.Data!.Data[0].Id.ShouldBe(orgId),
            () => result.Data!.Data[0].Attributes.Name.ShouldBe("Test Org"));
    }

    [Test]
    public async Task GetPageResultsAsync_ShouldAutoPaginate()
    {
        var id1 = Guid.NewGuid().ToString();
        var id2 = Guid.NewGuid().ToString();

        // Page 1 — first call returns page 1 and transitions to "page2" state
        Server
            .Given(Request.Create()
                .WithPath("/organisations")
                .UsingGet())
            .InScenario("org-paging")
            .WillSetStateTo("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [
                        (id1,
                            new { name = "Org A", status = "active" },
                            (object)new { associations = JsonApiStubHelper.RelatedMany() },
                            null)
                    ],
                    "organisations",
                    currentPage: 1,
                    lastPage: 2,
                    perPage: 1,
                    total: 2)));

        // Page 2 — second call (when state is "page2") returns page 2
        Server
            .Given(Request.Create()
                .WithPath("/organisations")
                .UsingGet())
            .InScenario("org-paging")
            .WhenStateIs("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [
                        (id2,
                            new { name = "Org B", status = "active" },
                            (object)new { associations = JsonApiStubHelper.RelatedMany() },
                            null)
                    ],
                    "organisations",
                    currentPage: 2,
                    lastPage: 2,
                    perPage: 1,
                    total: 2)));

        var allItems = new List<string>();
        await foreach (var page in Client.Organisations.GetPageResultsAsync())
        {
            allItems.AddRange(page.Select(item => item.Id));
        }

        allItems.ShouldSatisfyAllConditions(
            () => allItems.Count.ShouldBe(2),
            () => allItems.ShouldContain(id1),
            () => allItems.ShouldContain(id2));
    }

    [Test]
    public async Task Get_ShouldReturnSingleOrganisation()
    {
        var orgId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath($"/organisations/{orgId}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.SingleResponse(
                    orgId,
                    "organisations",
                    new { name = "My Organisation", status = "active", plan = "professional", billing_mode = "prepaid", billing_currency = "CHF", billing_balance = 250.50, default_country = "CH" },
                    relationships: new { associations = JsonApiStubHelper.RelatedMany() },
                    meta: JsonApiStubHelper.MetaWithAbilities(new { manage = "ok" }))));

        var result = await Client.Organisations.Get(orgId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(orgId),
            () => result.Data!.Data.Attributes.Name.ShouldBe("My Organisation"),
            () => result.Data!.Data.Attributes.BillingCurrency.ShouldBe("CHF"));
    }
}
