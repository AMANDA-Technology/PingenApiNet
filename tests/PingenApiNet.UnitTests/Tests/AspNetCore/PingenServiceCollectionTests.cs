using Microsoft.Extensions.DependencyInjection;
using PingenApiNet.AspNetCore;
using PingenApiNet.Interfaces.Connectors;

namespace PingenApiNet.UnitTests.Tests.AspNetCore;

/// <summary>
/// Unit tests for <see cref="PingenServiceCollection"/> DI registration
/// </summary>
public class PingenServiceCollectionTests
{
    private const string HeaderName = "X-Amanda-Client";

    private const string HeaderValue = "AMANDA.discountfit/2f3a9c1 (Backend; env=int)";

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

    /// <summary>
    /// Verifies that the configured default request headers are applied to the identity and the API http client
    /// </summary>
    [Test]
    public void AddPingenServices_WithDefaultRequestHeaders_AppliesThemToIdentityAndApiClients()
    {
        var services = new ServiceCollection();

        services.AddPingenServices(BuildConfiguration(new Dictionary<string, string> { [HeaderName] = HeaderValue }));

        var factory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
        using var identityClient = factory.CreateClient(PingenHttpClients.Names.Identity);
        using var apiClient = factory.CreateClient(PingenHttpClients.Names.Api);

        services.ShouldSatisfyAllConditions(
            () => GetHeaderValues(identityClient, HeaderName).ShouldBe([HeaderValue]),
            () => GetHeaderValues(apiClient, HeaderName).ShouldBe([HeaderValue]));
    }

    /// <summary>
    /// Verifies that the configured default request headers are NOT applied to the files http client. That client
    /// targets pre signed third party storage URLs, which must not receive any pre-configured header (see ADR-004).
    /// </summary>
    [Test]
    public void AddPingenServices_WithDefaultRequestHeaders_DoesNotApplyToFilesClient()
    {
        var services = new ServiceCollection();

        services.AddPingenServices(BuildConfiguration(new Dictionary<string, string> { [HeaderName] = HeaderValue }));

        var factory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
        using var filesClient = factory.CreateClient(PingenHttpClients.Names.Files);

        filesClient.DefaultRequestHeaders.ShouldBeEmpty();
    }

    /// <summary>
    /// Verifies that the default request headers are applied after the headers configured by this library,
    /// without dropping them
    /// </summary>
    [Test]
    public void AddPingenServices_WithDefaultRequestHeaders_KeepsAcceptHeaderOnIdentityClient()
    {
        var services = new ServiceCollection();

        services.AddPingenServices(BuildConfiguration(new Dictionary<string, string> { [HeaderName] = HeaderValue }));

        var factory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
        using var identityClient = factory.CreateClient(PingenHttpClients.Names.Identity);

        identityClient.DefaultRequestHeaders.Accept.ShouldHaveSingleItem()
            .MediaType.ShouldBe("application/x-www-form-urlencoded");
    }

    /// <summary>
    /// Verifies that reserved header names are silently skipped, so they can never duplicate or overwrite a header
    /// owned by this library
    /// </summary>
    [TestCase("Authorization")]
    [TestCase("Accept")]
    [TestCase("Host")]
    [TestCase("Idempotency-Key")]
    public void AddPingenServices_WithReservedDefaultRequestHeader_SkipsIt(string name)
    {
        var services = new ServiceCollection();

        services.AddPingenServices(BuildConfiguration(new Dictionary<string, string> { [name] = "caller-value" }));

        var factory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
        using var apiClient = factory.CreateClient(PingenHttpClients.Names.Api);

        GetHeaderValues(apiClient, name).ShouldBeEmpty();
    }

    /// <summary>
    /// Verifies that invalid default request headers are skipped instead of failing the client configuration
    /// </summary>
    [Test]
    public void AddPingenServices_WithInvalidDefaultRequestHeaders_SkipsThemWithoutThrowing()
    {
        var services = new ServiceCollection();

        services.AddPingenServices(BuildConfiguration(new Dictionary<string, string>
        {
            ["X Invalid Name"] = "value",
            ["X-Invalid-Value"] = "value\r\nX-Injected: evil",
            ["X-Blank-Value"] = " ",
            [HeaderName] = HeaderValue
        }));

        var factory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
        HttpClient apiClient = null!;

        Should.NotThrow(() => apiClient = factory.CreateClient(PingenHttpClients.Names.Api));

        using (apiClient)
        {
            apiClient.ShouldSatisfyAllConditions(
                () => GetHeaderValues(apiClient, HeaderName).ShouldBe([HeaderValue]),
                () => apiClient.DefaultRequestHeaders.Count().ShouldBe(1));
        }
    }

    /// <summary>
    /// Verifies that every client created from the factory carries the header exactly once
    /// </summary>
    [Test]
    public void AddPingenServices_WithDefaultRequestHeaders_AppliesThemOncePerCreatedClient()
    {
        var services = new ServiceCollection();

        services.AddPingenServices(BuildConfiguration(new Dictionary<string, string> { [HeaderName] = HeaderValue }));

        var factory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
        using var firstApiClient = factory.CreateClient(PingenHttpClients.Names.Api);
        using var secondApiClient = factory.CreateClient(PingenHttpClients.Names.Api);

        services.ShouldSatisfyAllConditions(
            () => GetHeaderValues(firstApiClient, HeaderName).ShouldBe([HeaderValue]),
            () => GetHeaderValues(secondApiClient, HeaderName).ShouldBe([HeaderValue]));
    }

    /// <summary>
    /// Verifies that the http clients carry no additional headers when none are configured, e.g. when registered
    /// through the overload taking the single configuration values
    /// </summary>
    [Test]
    public void AddPingenServices_WithoutDefaultRequestHeaders_AppliesNoAdditionalHeaders()
    {
        var services = new ServiceCollection();

        services.AddPingenServices(
            "https://api.example.com/",
            "https://identity.example.com/",
            "test-client-id",
            "test-client-secret",
            "test-org-id");

        var factory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
        using var identityClient = factory.CreateClient(PingenHttpClients.Names.Identity);
        using var apiClient = factory.CreateClient(PingenHttpClients.Names.Api);
        using var filesClient = factory.CreateClient(PingenHttpClients.Names.Files);

        services.ShouldSatisfyAllConditions(
            () => identityClient.DefaultRequestHeaders.ShouldHaveSingleItem().Key.ShouldBe("Accept"),
            () => apiClient.DefaultRequestHeaders.ShouldBeEmpty(),
            () => filesClient.DefaultRequestHeaders.ShouldBeEmpty());
    }

    /// <summary>
    /// Build a configuration for the tests, optionally with additional default request headers.
    /// </summary>
    /// <param name="defaultRequestHeaders">Optional, additional static default request headers.</param>
    /// <returns>The configuration to register.</returns>
    private static PingenConfiguration BuildConfiguration(IReadOnlyDictionary<string, string>? defaultRequestHeaders = null)
    {
        return new PingenConfiguration
        {
            BaseUri = "https://api.example.com/",
            IdentityUri = "https://identity.example.com/",
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            DefaultOrganisationId = "test-org-id",
            DefaultRequestHeaders = defaultRequestHeaders
        };
    }

    /// <summary>
    /// Get all values of the given header, or an empty array if the header is not set.
    /// </summary>
    /// <param name="client">The client to read the header from.</param>
    /// <param name="name">The header name to read.</param>
    /// <returns>All values of the given header.</returns>
    private static string[] GetHeaderValues(HttpClient client, string name)
    {
        return client.DefaultRequestHeaders.TryGetValues(name, out var values) ? values.ToArray() : [];
    }
}
