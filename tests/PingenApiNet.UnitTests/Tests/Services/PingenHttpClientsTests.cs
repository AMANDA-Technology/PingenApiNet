namespace PingenApiNet.UnitTests.Tests.Services;

/// <summary>
/// Unit tests for <see cref="PingenHttpClients"/>
/// </summary>
public class PingenHttpClientsTests
{
    private const string HeaderName = "X-Amanda-Client";

    private const string HeaderValue = "AMANDA.discountfit/2f3a9c1 (Backend; env=int)";

    /// <summary>
    /// Verifies that the configured default request headers are applied to the identity and the API http client
    /// </summary>
    [Test]
    public void Create_WithDefaultRequestHeaders_AppliesThemToIdentityAndApiClients()
    {
        var httpClients = PingenHttpClients.Create(BuildConfiguration(new Dictionary<string, string> { [HeaderName] = HeaderValue }));

        using (httpClients.Identity)
        using (httpClients.Api)
        using (httpClients.External)
        {
            httpClients.ShouldSatisfyAllConditions(
                () => GetHeaderValues(httpClients.Identity, HeaderName).ShouldBe([HeaderValue]),
                () => GetHeaderValues(httpClients.Api, HeaderName).ShouldBe([HeaderValue]));
        }
    }

    /// <summary>
    /// Verifies that the configured default request headers are NOT applied to the external http client. That client
    /// targets pre signed third party storage URLs, which must not receive any pre-configured header (see ADR-004).
    /// </summary>
    [Test]
    public void Create_WithDefaultRequestHeaders_DoesNotApplyToExternalClient()
    {
        var httpClients = PingenHttpClients.Create(BuildConfiguration(new Dictionary<string, string> { [HeaderName] = HeaderValue }));

        using (httpClients.Identity)
        using (httpClients.Api)
        using (httpClients.External)
        {
            httpClients.External.DefaultRequestHeaders.ShouldBeEmpty();
        }
    }

    /// <summary>
    /// Verifies that the default request headers are applied after the headers configured by this library,
    /// without dropping them
    /// </summary>
    [Test]
    public void Create_WithDefaultRequestHeaders_KeepsAcceptHeaderOnIdentityClient()
    {
        var httpClients = PingenHttpClients.Create(BuildConfiguration(new Dictionary<string, string> { [HeaderName] = HeaderValue }));

        using (httpClients.Identity)
        using (httpClients.Api)
        using (httpClients.External)
        {
            httpClients.Identity.DefaultRequestHeaders.Accept.ShouldHaveSingleItem()
                .MediaType.ShouldBe("application/x-www-form-urlencoded");
        }
    }

    /// <summary>
    /// Verifies that reserved header names are silently skipped, so they can never duplicate or overwrite a header
    /// owned by this library
    /// </summary>
    [TestCase("Authorization")]
    [TestCase("Accept")]
    [TestCase("Host")]
    [TestCase("Idempotency-Key")]
    public void Create_WithReservedDefaultRequestHeader_SkipsIt(string name)
    {
        var httpClients = PingenHttpClients.Create(BuildConfiguration(new Dictionary<string, string> { [name] = "caller-value" }));

        using (httpClients.Identity)
        using (httpClients.Api)
        using (httpClients.External)
        {
            GetHeaderValues(httpClients.Api, name).ShouldBeEmpty();
        }
    }

    /// <summary>
    /// Verifies that invalid default request headers are skipped instead of failing the client construction
    /// </summary>
    [Test]
    public void Create_WithInvalidDefaultRequestHeaders_SkipsThemWithoutThrowing()
    {
        IPingenConfiguration configuration = BuildConfiguration(new Dictionary<string, string>
        {
            ["X Invalid Name"] = "value",
            ["X-Invalid-Value"] = "value\r\nX-Injected: evil",
            ["X-Blank-Value"] = " ",
            [HeaderName] = HeaderValue
        });

        PingenHttpClients httpClients = null!;

        Should.NotThrow(() => httpClients = PingenHttpClients.Create(configuration));

        using (httpClients.Identity)
        using (httpClients.Api)
        using (httpClients.External)
        {
            httpClients.ShouldSatisfyAllConditions(
                () => GetHeaderValues(httpClients.Api, HeaderName).ShouldBe([HeaderValue]),
                () => httpClients.Api.DefaultRequestHeaders.Count().ShouldBe(1));
        }
    }

    /// <summary>
    /// Verifies that the http clients carry no additional headers when none are configured
    /// </summary>
    [Test]
    public void Create_WithoutDefaultRequestHeaders_AppliesNoAdditionalHeaders()
    {
        var httpClients = PingenHttpClients.Create(BuildConfiguration());

        using (httpClients.Identity)
        using (httpClients.Api)
        using (httpClients.External)
        {
            httpClients.ShouldSatisfyAllConditions(
                () => httpClients.Identity.DefaultRequestHeaders.ShouldHaveSingleItem().Key.ShouldBe("Accept"),
                () => httpClients.Api.DefaultRequestHeaders.ShouldBeEmpty(),
                () => httpClients.External.DefaultRequestHeaders.ShouldBeEmpty());
        }
    }

    /// <summary>
    /// Build a configuration for the tests, optionally with additional default request headers.
    /// </summary>
    /// <param name="defaultRequestHeaders">Optional, additional static default request headers.</param>
    /// <returns>The configuration to create the http clients from.</returns>
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
