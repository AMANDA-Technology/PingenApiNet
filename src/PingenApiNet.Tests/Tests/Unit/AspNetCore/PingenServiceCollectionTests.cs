using Microsoft.Extensions.DependencyInjection;
using PingenApiNet.AspNetCore;
using PingenApiNet.Interfaces.Connectors;

namespace PingenApiNet.Tests.Tests.Unit.AspNetCore;

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

        Assert.Multiple(() =>
        {
            Assert.That(services.Any(s => s.ServiceType == typeof(IPingenConfiguration)), Is.True, "IPingenConfiguration not registered");
            Assert.That(services.Any(s => s.ServiceType == typeof(IPingenConnectionHandler)), Is.True, "IPingenConnectionHandler not registered");
            Assert.That(services.Any(s => s.ServiceType == typeof(IPingenApiClient)), Is.True, "IPingenApiClient not registered");
            Assert.That(services.Any(s => s.ServiceType == typeof(ILetterService)), Is.True, "ILetterService not registered");
            Assert.That(services.Any(s => s.ServiceType == typeof(IBatchService)), Is.True, "IBatchService not registered");
            Assert.That(services.Any(s => s.ServiceType == typeof(IUserService)), Is.True, "IUserService not registered");
            Assert.That(services.Any(s => s.ServiceType == typeof(IOrganisationService)), Is.True, "IOrganisationService not registered");
            Assert.That(services.Any(s => s.ServiceType == typeof(IWebhookService)), Is.True, "IWebhookService not registered");
            Assert.That(services.Any(s => s.ServiceType == typeof(IFilesService)), Is.True, "IFilesService not registered");
            Assert.That(services.Any(s => s.ServiceType == typeof(IDistributionService)), Is.True, "IDistributionService not registered");
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(services.First(s => s.ServiceType == typeof(IPingenConfiguration)).Lifetime,
                Is.EqualTo(ServiceLifetime.Singleton), "IPingenConfiguration should be Singleton");

            Assert.That(services.First(s => s.ServiceType == typeof(IPingenConnectionHandler)).Lifetime,
                Is.EqualTo(ServiceLifetime.Scoped), "IPingenConnectionHandler should be Scoped");

            Assert.That(services.First(s => s.ServiceType == typeof(IPingenApiClient)).Lifetime,
                Is.EqualTo(ServiceLifetime.Scoped), "IPingenApiClient should be Scoped");

            Assert.That(services.First(s => s.ServiceType == typeof(ILetterService)).Lifetime,
                Is.EqualTo(ServiceLifetime.Scoped), "ILetterService should be Scoped");
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(services.Any(s => s.ServiceType == typeof(IPingenConfiguration)), Is.True);
            Assert.That(services.Any(s => s.ServiceType == typeof(IPingenApiClient)), Is.True);
        });
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

        Assert.That(client, Is.Not.Null);
    }
}
