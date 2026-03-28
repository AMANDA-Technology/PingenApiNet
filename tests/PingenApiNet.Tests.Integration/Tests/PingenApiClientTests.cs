using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace PingenApiNet.Tests.Integration.Tests;

/// <summary>
/// Integration tests for <see cref="IPingenApiClient"/> facade.
/// </summary>
[TestFixture]
public sealed class PingenApiClientTests : IntegrationTestBase
{
    [Test]
    public void ServiceProperties_ShouldExposeAllConnectors()
    {
        Client.ShouldSatisfyAllConditions(
            () => Client.Letters.ShouldNotBeNull(),
            () => Client.Batches.ShouldNotBeNull(),
            () => Client.Users.ShouldNotBeNull(),
            () => Client.Organisations.ShouldNotBeNull(),
            () => Client.Webhooks.ShouldNotBeNull(),
            () => Client.Files.ShouldNotBeNull(),
            () => Client.Distributions.ShouldNotBeNull());
    }

    [Test]
    public async Task SetOrganisationId_ShouldRouteSubsequentRequestsToNewOrg()
    {
        const string newOrgId = "new-org-id-999";

        // Stub batches endpoint for the new organisation ID
        Server
            .Given(Request.Create()
                .WithPath($"/organisations/{newOrgId}/batches")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [],
                    "batches")));

        Client.SetOrganisationId(newOrgId);

        var result = await Client.Batches.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.ShouldBeEmpty());
    }
}
