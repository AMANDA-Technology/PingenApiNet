using PingenApiNet.Interfaces.Connectors;

namespace PingenApiNet.Tests.Tests.Unit.Services;

/// <summary>
/// Unit tests for <see cref="PingenApiClient"/>
/// </summary>
public class PingenApiClientTests
{
    /// <summary>
    /// Verifies PingenApiClient exposes all connector services
    /// </summary>
    [Test]
    public void PingenApiClient_ExposesAllServices()
    {
        var mockConnectionHandler = Substitute.For<IPingenConnectionHandler>();
        var mockLetters = Substitute.For<ILetterService>();
        var mockBatches = Substitute.For<IBatchService>();
        var mockUsers = Substitute.For<IUserService>();
        var mockOrganisations = Substitute.For<IOrganisationService>();
        var mockWebhooks = Substitute.For<IWebhookService>();
        var mockFiles = Substitute.For<IFilesService>();
        var mockDistributions = Substitute.For<IDistributionService>();

        var client = new PingenApiClient(
            mockConnectionHandler,
            mockLetters,
            mockBatches,
            mockUsers,
            mockOrganisations,
            mockWebhooks,
            mockFiles,
            mockDistributions);

        client.ShouldSatisfyAllConditions(
            () => client.Letters.ShouldBeSameAs(mockLetters),
            () => client.Batches.ShouldBeSameAs(mockBatches),
            () => client.Users.ShouldBeSameAs(mockUsers),
            () => client.Organisations.ShouldBeSameAs(mockOrganisations),
            () => client.Webhooks.ShouldBeSameAs(mockWebhooks),
            () => client.Files.ShouldBeSameAs(mockFiles),
            () => client.Distributions.ShouldBeSameAs(mockDistributions)
        );
    }

    /// <summary>
    /// Verifies SetOrganisationId delegates to connection handler
    /// </summary>
    [Test]
    public void SetOrganisationId_DelegatesToConnectionHandler()
    {
        var mockConnectionHandler = Substitute.For<IPingenConnectionHandler>();
        var client = new PingenApiClient(
            mockConnectionHandler,
            Substitute.For<ILetterService>(),
            Substitute.For<IBatchService>(),
            Substitute.For<IUserService>(),
            Substitute.For<IOrganisationService>(),
            Substitute.For<IWebhookService>(),
            Substitute.For<IFilesService>(),
            Substitute.For<IDistributionService>());

        const string newOrgId = "new-org-id";
        client.SetOrganisationId(newOrgId);

        mockConnectionHandler.Received(1).SetOrganisationId(newOrgId);
    }

    /// <summary>
    /// Verifies services can be replaced via property setters
    /// </summary>
    [Test]
    public void Services_CanBeReplacedViaSetters()
    {
        var mockConnectionHandler = Substitute.For<IPingenConnectionHandler>();
        var client = new PingenApiClient(
            mockConnectionHandler,
            Substitute.For<ILetterService>(),
            Substitute.For<IBatchService>(),
            Substitute.For<IUserService>(),
            Substitute.For<IOrganisationService>(),
            Substitute.For<IWebhookService>(),
            Substitute.For<IFilesService>(),
            Substitute.For<IDistributionService>());

        var newLetterService = Substitute.For<ILetterService>();
        client.Letters = newLetterService;

        client.Letters.ShouldBeSameAs(newLetterService);
    }
}
