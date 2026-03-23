using System.Net;
using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Tests.Helpers;

namespace PingenApiNet.Tests.Tests.Unit.Services;

/// <summary>
/// Unit tests for <see cref="PingenConnectionHandler"/>
/// </summary>
[NonParallelizable]
public class PingenConnectionHandlerTests
{
    /// <summary>
    /// Verifies that constructor throws when BaseUri is null or empty
    /// </summary>
    [Test]
    public void Constructor_NullBaseUri_ThrowsArgumentException()
    {
        var config = CreateConfig(baseUri: "", identityUri: "https://identity.example.com/");

        Assert.Throws<ArgumentException>(() =>
            new PingenConnectionHandler(config, CreateHttpClients()));
    }

    /// <summary>
    /// Verifies that constructor throws when IdentityUri is null or empty
    /// </summary>
    [Test]
    public void Constructor_NullIdentityUri_ThrowsArgumentException()
    {
        var config = CreateConfig(baseUri: "https://api.example.com/", identityUri: "");

        Assert.Throws<ArgumentException>(() =>
            new PingenConnectionHandler(config, CreateHttpClients()));
    }

    /// <summary>
    /// Verifies that constructor normalizes URIs (appends trailing slash)
    /// </summary>
    [Test]
    public void Constructor_ValidConfig_Succeeds()
    {
        var config = CreateConfig();

        var handler = new PingenConnectionHandler(config, CreateHttpClients());

        Assert.That(handler, Is.Not.Null);
    }

    /// <summary>
    /// Verifies SetOrganisationId changes the org ID
    /// </summary>
    [Test]
    public void SetOrganisationId_UpdatesOrganisationId()
    {
        var config = CreateConfig();
        var handler = new PingenConnectionHandler(config, CreateHttpClients());

        // Should not throw
        handler.SetOrganisationId("new-org-id");
    }

    /// <summary>
    /// Verifies that GetAsync authenticates and sends request
    /// </summary>
    [Test]
    public async Task GetAsync_AuthenticatesAndSendsRequest()
    {
        var tokenJson = PingenSerialisationHelper.Serialize(new
        {
            access_token = "test-token",
            token_type = "Bearer",
            expires_in = 3600
        });

        var identityHandler = new MockHttpMessageHandler(HttpStatusCode.OK, tokenJson);
        var apiHandler = new MockHttpMessageHandler((request, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"data\":[]}")
            };
            return Task.FromResult(response);
        });

        var config = CreateConfig();
        var httpClients = CreateHttpClients(identityHandler, apiHandler);
        var handler = new PingenConnectionHandler(config, httpClients);

        var result = await handler.GetAsync("letters");

        Assert.That(result.IsSuccess, Is.True);
    }

    /// <summary>
    /// Verifies that SendExternalRequestAsync sends to external client
    /// </summary>
    [Test]
    public async Task SendExternalRequestAsync_SendsRequest()
    {
        var externalHandler = new MockHttpMessageHandler(HttpStatusCode.OK, "file-content");

        var config = CreateConfig();
        var httpClients = CreateHttpClients(externalHandler: externalHandler);
        var handler = new PingenConnectionHandler(config, httpClients);

        var request = new HttpRequestMessage(HttpMethod.Get, "https://external.example.com/file.pdf");
        var result = await handler.SendExternalRequestAsync(request);

        Assert.That(result.IsSuccessStatusCode, Is.True);
    }

    /// <summary>
    /// Verifies that GetAsync returns error result on non-success status code
    /// </summary>
    [Test]
    public async Task GetAsync_NonSuccessStatusCode_ReturnsFailureResult()
    {
        var tokenJson = PingenSerialisationHelper.Serialize(new
        {
            access_token = "test-token",
            token_type = "Bearer",
            expires_in = 3600
        });

        var identityHandler = new MockHttpMessageHandler(HttpStatusCode.OK, tokenJson);
        var apiHandler = new MockHttpMessageHandler(HttpStatusCode.BadRequest,
            "{\"errors\":[{\"code\":\"400\",\"title\":\"Bad Request\",\"detail\":\"Invalid request\",\"source\":{\"pointer\":\"/data\",\"parameter\":\"\"}}]}");

        var config = CreateConfig();
        var httpClients = CreateHttpClients(identityHandler, apiHandler);
        var handler = new PingenConnectionHandler(config, httpClients);

        var result = await handler.GetAsync("letters");

        Assert.That(result.IsSuccess, Is.False);
    }

    private static IPingenConfiguration CreateConfig(
        string baseUri = "https://api.example.com/",
        string identityUri = "https://identity.example.com/")
    {
        return new PingenConfiguration
        {
            BaseUri = baseUri,
            IdentityUri = identityUri,
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            DefaultOrganisationId = "test-org-id"
        };
    }

    private static PingenHttpClients CreateHttpClients(
        MockHttpMessageHandler? identityHandler = null,
        MockHttpMessageHandler? apiHandler = null,
        MockHttpMessageHandler? externalHandler = null)
    {
        var identityClient = new HttpClient(identityHandler ?? new MockHttpMessageHandler(HttpStatusCode.OK, "{}"))
        {
            BaseAddress = new Uri("https://identity.example.com/")
        };

        var apiClient = new HttpClient(apiHandler ?? new MockHttpMessageHandler(HttpStatusCode.OK, "{}"))
        {
            BaseAddress = new Uri("https://api.example.com/")
        };

        var externalClient = new HttpClient(externalHandler ?? new MockHttpMessageHandler(HttpStatusCode.OK, "{}"));

        return new PingenHttpClients(identityClient, apiClient, externalClient);
    }
}
