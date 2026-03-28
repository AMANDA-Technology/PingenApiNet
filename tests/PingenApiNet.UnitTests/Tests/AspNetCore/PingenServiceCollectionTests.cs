using Microsoft.Extensions.DependencyInjection;
using PingenApiNet.AspNetCore;
using PingenApiNet.Interfaces.Connectors;

namespace PingenApiNet.UnitTests.Tests.AspNetCore;

/// <summary>
/// Unit tests for <see cref="PingenServiceCollection"/> DI registration
/// </summary>
public class PingenServiceCollectionTests
{
    /// <summary>
    /// Verifies that AddPingenServices registers all expected services
    /// </summary>
    [Test]
    public void AddPingenServices_RegistersAllServices()
    {
        var services = new ServiceCollection();

        services.AddPingenServices(
            "https://api.example.com/",
            "https://identity.example.com/",
            "test-client-id",
            "test-client-secret",
            "test-org-id");

        services.ShouldSatisfyAllConditions(
            () => services.Any(s => s.ServiceType == typeof(IPingenConfiguration)).ShouldBeTrue("IPingenConfiguration not registered"),
            () => services.Any(s => s.ServiceType == typeof(IPingenConnectionHandler)).ShouldBeTrue("IPingenConnectionHandler not registered"),
            () => services.Any(s => s.ServiceType == typeof(IPingenApiClient)).ShouldBeTrue("IPingenApiClient not registered"),
            () => services.Any(s => s.ServiceType == typeof(ILetterService)).ShouldBeTrue("ILetterService not registered"),
            () => services.Any(s => s.ServiceType == typeof(IBatchService)).ShouldBeTrue("IBatchService not registered"),
            () => services.Any(s => s.ServiceType == typeof(IUserService)).ShouldBeTrue("IUserService not registered"),
            () => services.Any(s => s.ServiceType == typeof(IOrganisationService)).ShouldBeTrue("IOrganisationService not registered"),
            () => services.Any(s => s.ServiceType == typeof(IWebhookService)).ShouldBeTrue("IWebhookService not registered"),
            () => services.Any(s => s.ServiceType == typeof(IFilesService)).ShouldBeTrue("IFilesService not registered"),
            () => services.Any(s => s.ServiceType == typeof(IDistributionService)).ShouldBeTrue("IDistributionService not registered")
        );
    }

    /// <summary>
    /// Verifies that AddPingenServices registers services with correct lifetimes
    /// </summary>
    [Test]
    public void AddPingenServices_RegistersCorrectLifetimes()
    {
        var services = new ServiceCollection();

        services.AddPingenServices(
            "https://api.example.com/",
            "https://identity.example.com/",
            "test-client-id",
            "test-client-secret",
            "test-org-id");

        services.ShouldSatisfyAllConditions(
            () => services.First(s => s.ServiceType == typeof(IPingenConfiguration)).Lifetime
                .ShouldBe(ServiceLifetime.Singleton, "IPingenConfiguration should be Singleton"),
            () => services.First(s => s.ServiceType == typeof(IPingenConnectionHandler)).Lifetime
                .ShouldBe(ServiceLifetime.Scoped, "IPingenConnectionHandler should be Scoped"),
            () => services.First(s => s.ServiceType == typeof(IPingenApiClient)).Lifetime
                .ShouldBe(ServiceLifetime.Scoped, "IPingenApiClient should be Scoped"),
            () => services.First(s => s.ServiceType == typeof(ILetterService)).Lifetime
                .ShouldBe(ServiceLifetime.Scoped, "ILetterService should be Scoped")
        );
    }

    /// <summary>
    /// Verifies that AddPingenServices with configuration object works
    /// </summary>
    [Test]
    public void AddPingenServices_WithConfigObject_RegistersAllServices()
    {
        var services = new ServiceCollection();
        var config = new PingenConfiguration
        {
            BaseUri = "https://api.example.com/",
            IdentityUri = "https://identity.example.com/",
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            DefaultOrganisationId = "test-org-id"
        };

        services.AddPingenServices(config);

        services.ShouldSatisfyAllConditions(
            () => services.Any(s => s.ServiceType == typeof(IPingenConfiguration)).ShouldBeTrue(),
            () => services.Any(s => s.ServiceType == typeof(IPingenApiClient)).ShouldBeTrue()
        );
    }

    /// <summary>
    /// Verifies that AddPingenServices can resolve IPingenApiClient
    /// </summary>
    [Test]
    public void AddPingenServices_CanResolveApiClient()
    {
        var services = new ServiceCollection();

        services.AddPingenServices(
            "https://api.example.com/",
            "https://identity.example.com/",
            "test-client-id",
            "test-client-secret",
            "test-org-id");

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var client = scope.ServiceProvider.GetService<IPingenApiClient>();

        client.ShouldNotBeNull();
    }
}
