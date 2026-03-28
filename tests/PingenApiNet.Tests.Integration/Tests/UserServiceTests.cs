using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace PingenApiNet.Tests.Integration.Tests;

/// <summary>
/// Integration tests for <see cref="IUserService"/>.
/// </summary>
[TestFixture]
public sealed class UserServiceTests : IntegrationTestBase
{
    /// <summary>
    /// Builds a valid user association item tuple for stubs.
    /// </summary>
    private static (string Id, object Attributes, object? Relationships, object? Meta) AssociationItem(string id, string role)
    {
        return (id,
            new { role, status = "active" },
            (object)new { organisation = JsonApiStubHelper.RelatedSingle($"org-for-{id}", "organisations") },
            JsonApiStubHelper.MetaWithOrganisationAbilities(
                new { reach = "ok", act = "ok" },
                new { manage = "ok" }));
    }

    [Test]
    public async Task Get_ShouldReturnAuthenticatedUser()
    {
        var userId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath("/user")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.SingleResponse(
                    userId,
                    "users",
                    new { email = "test@example.com", first_name = "Test", last_name = "User", status = "active", language = "en" },
                    relationships: new
                    {
                        associations = JsonApiStubHelper.RelatedMany(),
                        notifications = JsonApiStubHelper.RelatedMany()
                    },
                    meta: JsonApiStubHelper.MetaWithAbilities(new { reach = "ok", act = "ok" }))));

        var result = await Client.Users.Get();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(userId),
            () => result.Data!.Data.Attributes.Email.ShouldBe("test@example.com"),
            () => result.Data!.Data.Attributes.FirstName.ShouldBe("Test"),
            () => result.Data!.Data.Attributes.LastName.ShouldBe("User"));
    }

    [Test]
    public async Task GetAssociationsPage_ShouldReturnAssociations()
    {
        var associationId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath("/user/associations")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [AssociationItem(associationId, "owner")],
                    "associations")));

        var result = await Client.Users.GetAssociationsPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(1),
            () => result.Data!.Data[0].Id.ShouldBe(associationId));
    }

    [Test]
    public async Task GetAssociationsPageResultsAsync_ShouldAutoPaginate()
    {
        var id1 = Guid.NewGuid().ToString();
        var id2 = Guid.NewGuid().ToString();

        // Use WireMock scenarios to return page 1 on first call, page 2 on second
        Server
            .Given(Request.Create()
                .WithPath("/user/associations")
                .UsingGet())
            .InScenario("user-assoc-paging")
            .WillSetStateTo("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [AssociationItem(id1, "owner")],
                    "associations",
                    currentPage: 1, lastPage: 2, perPage: 1, total: 2)));

        Server
            .Given(Request.Create()
                .WithPath("/user/associations")
                .UsingGet())
            .InScenario("user-assoc-paging")
            .WhenStateIs("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [AssociationItem(id2, "manager")],
                    "associations",
                    currentPage: 2, lastPage: 2, perPage: 1, total: 2)));

        var allItems = new List<string>();
        await foreach (var page in Client.Users.GetAssociationsPageResultsAsync())
        {
            allItems.AddRange(page.Select(item => item.Id));
        }

        allItems.ShouldSatisfyAllConditions(
            () => allItems.Count.ShouldBe(2),
            () => allItems.ShouldContain(id1),
            () => allItems.ShouldContain(id2));
    }
}
