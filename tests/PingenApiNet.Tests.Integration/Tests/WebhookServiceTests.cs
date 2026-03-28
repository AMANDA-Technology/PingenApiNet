using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Webhooks.Views;
using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace PingenApiNet.Tests.Integration.Tests;

/// <summary>
/// Integration tests for <see cref="IWebhookService"/>.
/// </summary>
[TestFixture]
public sealed class WebhookServiceTests : IntegrationTestBase
{
    /// <summary>
    /// Builds a valid webhook item tuple for collection stubs.
    /// </summary>
    private static (string Id, object Attributes, object? Relationships, object? Meta) WebhookItem(string id)
    {
        return (id,
            new
            {
                event_category = "issues",
                url = "https://example.com/webhook",
                signing_key = "test-signing-key"
            },
            (object)new
            {
                organisation = JsonApiStubHelper.RelatedSingle(TestOrganisationId, "organisations")
            },
            null);
    }

    /// <summary>
    /// Verifies that GetPage returns a paginated list of webhooks.
    /// </summary>
    [Test]
    public async Task GetPage_ShouldReturnWebhooks()
    {
        var webhookId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath("webhooks"))
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [WebhookItem(webhookId)],
                    "webhooks")));

        var result = await Client.Webhooks.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(1),
            () => result.Data!.Data[0].Id.ShouldBe(webhookId),
            () => result.Data!.Data[0].Attributes.Url!.ToString().ShouldBe("https://example.com/webhook"));
    }

    /// <summary>
    /// Verifies that GetPageResultsAsync auto-paginates across multiple pages.
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_ShouldAutoPaginate()
    {
        var id1 = Guid.NewGuid().ToString();
        var id2 = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath("webhooks"))
                .UsingGet())
            .InScenario("webhooks-paging")
            .WillSetStateTo("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [WebhookItem(id1)],
                    "webhooks",
                    currentPage: 1, lastPage: 2, perPage: 1, total: 2)));

        Server
            .Given(Request.Create()
                .WithPath(OrgPath("webhooks"))
                .UsingGet())
            .InScenario("webhooks-paging")
            .WhenStateIs("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [WebhookItem(id2)],
                    "webhooks",
                    currentPage: 2, lastPage: 2, perPage: 1, total: 2)));

        var allItems = new List<string>();
        await foreach (var page in Client.Webhooks.GetPageResultsAsync())
        {
            allItems.AddRange(page.Select(item => item.Id));
        }

        allItems.ShouldSatisfyAllConditions(
            () => allItems.Count.ShouldBe(2),
            () => allItems.ShouldContain(id1),
            () => allItems.ShouldContain(id2));
    }

    /// <summary>
    /// Verifies that Create posts a new webhook and returns the created resource.
    /// </summary>
    [Test]
    public async Task Create_ShouldPostWebhookAndReturnResult()
    {
        var webhookId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath("webhooks"))
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(201)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.SingleResponse(
                    webhookId,
                    "webhooks",
                    new
                    {
                        event_category = "sent",
                        url = "https://example.com/webhook/sent",
                        signing_key = "new-signing-key"
                    },
                    relationships: new
                    {
                        organisation = JsonApiStubHelper.RelatedSingle(TestOrganisationId, "organisations")
                    })));

        var data = new DataPost<WebhookCreate>
        {
            Type = PingenApiDataType.webhooks,
            Attributes = new WebhookCreate
            {
                FileOriginalName = WebhookEventCategory.sent,
                Url = new Uri("https://example.com/webhook/sent"),
                SigningKey = "new-signing-key"
            }
        };

        var result = await Client.Webhooks.Create(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(webhookId),
            () => result.Data!.Data.Attributes.Url!.ToString().ShouldBe("https://example.com/webhook/sent"));
    }

    /// <summary>
    /// Verifies that Get returns a single webhook by ID.
    /// </summary>
    [Test]
    public async Task Get_ShouldReturnSingleWebhook()
    {
        var webhookId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath($"webhooks/{webhookId}"))
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.SingleResponse(
                    webhookId,
                    "webhooks",
                    new
                    {
                        event_category = "undeliverable",
                        url = "https://example.com/webhook/undeliverable",
                        signing_key = "get-signing-key"
                    },
                    relationships: new
                    {
                        organisation = JsonApiStubHelper.RelatedSingle(TestOrganisationId, "organisations")
                    })));

        var result = await Client.Webhooks.Get(webhookId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(webhookId),
            () => result.Data!.Data.Attributes.EventCategory.ShouldBe(WebhookEventCategory.undeliverable));
    }

    /// <summary>
    /// Verifies that Delete removes a webhook and returns success.
    /// </summary>
    [Test]
    public async Task Delete_ShouldReturnSuccess()
    {
        var webhookId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath($"webhooks/{webhookId}"))
                .UsingDelete())
            .RespondWith(Response.Create()
                .WithStatusCode(204)
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString()));

        var result = await Client.Webhooks.Delete(webhookId);

        result.IsSuccess.ShouldBeTrue();
    }
}
