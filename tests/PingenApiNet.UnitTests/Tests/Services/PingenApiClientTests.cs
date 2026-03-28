using PingenApiNet.Interfaces.Connectors;

namespace PingenApiNet.UnitTests.Tests.Services;

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
    /// Verifies service properties use init-only setters (immutable after construction)
    /// </summary>
    [Test]
    public void Services_UseInitOnlySetters()
    {
        var serviceProperties = new[]
        {
            nameof(PingenApiClient.Letters),
            nameof(PingenApiClient.Batches),
            nameof(PingenApiClient.Users),
            nameof(PingenApiClient.Organisations),
            nameof(PingenApiClient.Webhooks),
            nameof(PingenApiClient.Files),
            nameof(PingenApiClient.Distributions)
        };

        foreach (var propertyName in serviceProperties)
        {
            var property = typeof(PingenApiClient).GetProperty(propertyName)!;
            var setMethod = property.GetSetMethod()!;
            var modifiers = setMethod.ReturnParameter.GetRequiredCustomModifiers();
            modifiers.ShouldContain(typeof(System.Runtime.CompilerServices.IsExternalInit),
                $"{propertyName} should be init-only");
        }
    }
}
