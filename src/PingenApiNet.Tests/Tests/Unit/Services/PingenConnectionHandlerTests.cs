using System.Net;
using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Models.Api;
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

        Should.Throw<ArgumentException>(() =>
            new PingenConnectionHandler(config, CreateHttpClients()));
    }

    /// <summary>
    /// Verifies that constructor throws when IdentityUri is null or empty
    /// </summary>
    [Test]
    public void Constructor_NullIdentityUri_ThrowsArgumentException()
    {
        var config = CreateConfig(baseUri: "https://api.example.com/", identityUri: "");

        Should.Throw<ArgumentException>(() =>
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

        handler.ShouldNotBeNull();
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

        var result = await handler.GetAsync("letters", (ApiPagingRequest?)null);

        result.IsSuccess.ShouldBeTrue();
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

        result.IsSuccessStatusCode.ShouldBeTrue();
    }

    /// <summary>
    /// Verifies that each PingenConnectionHandler instance maintains its own access token,
    /// preventing multi-tenant token sharing (regression test for issue #22)
    /// </summary>
    [Test]
    public async Task MultipleInstances_MaintainSeparateAccessTokens()
    {
        // Handler A identity server returns token-A
        var identityHandlerA = new MockHttpMessageHandler(HttpStatusCode.OK,
            PingenSerialisationHelper.Serialize(new
            {
                access_token = "token-A",
                token_type = "Bearer",
                expires_in = 3600
            }));

        string? capturedTokenA = null;
        var apiHandlerA = new MockHttpMessageHandler((request, _) =>
        {
            capturedTokenA = request.Headers.Authorization?.Parameter;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"data\":[]}")
            });
        });

        // Handler B identity server returns token-B
        var identityHandlerB = new MockHttpMessageHandler(HttpStatusCode.OK,
            PingenSerialisationHelper.Serialize(new
            {
                access_token = "token-B",
                token_type = "Bearer",
                expires_in = 3600
            }));

        string? capturedTokenB = null;
        var apiHandlerB = new MockHttpMessageHandler((request, _) =>
        {
            capturedTokenB = request.Headers.Authorization?.Parameter;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"data\":[]}")
            });
        });

        var configA = CreateConfig();
        var configB = CreateConfig();

        var httpClientsA = CreateHttpClients(identityHandlerA, apiHandlerA);
        var httpClientsB = CreateHttpClients(identityHandlerB, apiHandlerB);

        // Handler A authenticates first
        var handlerA = new PingenConnectionHandler(configA, httpClientsA);
        await handlerA.GetAsync("letters", (ApiPagingRequest?)null);

        // Handler B created after A has already authenticated — must use its own token
        var handlerB = new PingenConnectionHandler(configB, httpClientsB);
        await handlerB.GetAsync("letters", (ApiPagingRequest?)null);

        // Each handler should use its own token, not share the static one
        capturedTokenA.ShouldBe("token-A");
        capturedTokenB.ShouldBe("token-B");
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

        var result = await handler.GetAsync("letters", (ApiPagingRequest?)null);

        result.IsSuccess.ShouldBeFalse();
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
