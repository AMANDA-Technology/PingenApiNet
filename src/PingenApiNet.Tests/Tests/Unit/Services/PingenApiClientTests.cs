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
        var mockConnectionHandler = new Mock<IPingenConnectionHandler>();
        var mockLetters = new Mock<ILetterService>();
        var mockBatches = new Mock<IBatchService>();
        var mockUsers = new Mock<IUserService>();
        var mockOrganisations = new Mock<IOrganisationService>();
        var mockWebhooks = new Mock<IWebhookService>();
        var mockFiles = new Mock<IFilesService>();
        var mockDistributions = new Mock<IDistributionService>();

        var client = new PingenApiClient(
            mockConnectionHandler.Object,
            mockLetters.Object,
            mockBatches.Object,
            mockUsers.Object,
            mockOrganisations.Object,
            mockWebhooks.Object,
            mockFiles.Object,
            mockDistributions.Object);

        Assert.Multiple(() =>
        {
            Assert.That(client.Letters, Is.SameAs(mockLetters.Object));
            Assert.That(client.Batches, Is.SameAs(mockBatches.Object));
            Assert.That(client.Users, Is.SameAs(mockUsers.Object));
            Assert.That(client.Organisations, Is.SameAs(mockOrganisations.Object));
            Assert.That(client.Webhooks, Is.SameAs(mockWebhooks.Object));
            Assert.That(client.Files, Is.SameAs(mockFiles.Object));
            Assert.That(client.Distributions, Is.SameAs(mockDistributions.Object));
        });
    }

    /// <summary>
    /// Verifies SetOrganisationId delegates to connection handler
    /// </summary>
    [Test]
    public void SetOrganisationId_DelegatesToConnectionHandler()
    {
        var mockConnectionHandler = new Mock<IPingenConnectionHandler>();
        var client = new PingenApiClient(
            mockConnectionHandler.Object,
            new Mock<ILetterService>().Object,
            new Mock<IBatchService>().Object,
            new Mock<IUserService>().Object,
            new Mock<IOrganisationService>().Object,
            new Mock<IWebhookService>().Object,
            new Mock<IFilesService>().Object,
            new Mock<IDistributionService>().Object);

        const string newOrgId = "new-org-id";
        client.SetOrganisationId(newOrgId);

        mockConnectionHandler.Verify(x => x.SetOrganisationId(newOrgId), Times.Once);
    }

    /// <summary>
    /// Verifies services can be replaced via property setters
    /// </summary>
    [Test]
    public void Services_CanBeReplacedViaSetters()
    {
        var mockConnectionHandler = new Mock<IPingenConnectionHandler>();
        var client = new PingenApiClient(
            mockConnectionHandler.Object,
            new Mock<ILetterService>().Object,
            new Mock<IBatchService>().Object,
            new Mock<IUserService>().Object,
            new Mock<IOrganisationService>().Object,
            new Mock<IWebhookService>().Object,
            new Mock<IFilesService>().Object,
            new Mock<IDistributionService>().Object);

        var newLetterService = new Mock<ILetterService>().Object;
        client.Letters = newLetterService;

        Assert.That(client.Letters, Is.SameAs(newLetterService));
    }
}
