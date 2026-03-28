using PingenApiNet.Services.Connectors;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace PingenApiNet.Tests.Integration;

/// <summary>
/// Base class for integration tests. Starts a WireMock server, stubs the OAuth token endpoint,
/// and wires up a real <see cref="PingenApiClient"/> whose HTTP clients all point at WireMock.
/// </summary>
public abstract class IntegrationTestBase
{
    /// <summary>
    /// Organisation ID used in tests.
    /// </summary>
    protected const string TestOrganisationId = "test-org-id-001";

    /// <summary>
    /// WireMock server instance shared across tests in the fixture.
    /// </summary>
    protected WireMockServer Server = null!;

    /// <summary>
    /// Pingen API client wired to the WireMock server.
    /// </summary>
    protected IPingenApiClient Client = null!;

    private HttpClient _identityClient = null!;
    private HttpClient _apiClient = null!;
    private HttpClient _externalClient = null!;

    /// <summary>
    /// Start WireMock server once per test fixture.
    /// </summary>
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Server = WireMockServer.Start();
    }

    /// <summary>
    /// Reset stubs, re-stub the token endpoint, and create a fresh client before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        Server.Reset();
        StubTokenEndpoint();

        var wireMockUrl = Server.Url!;

        _identityClient = new HttpClient { BaseAddress = new Uri(wireMockUrl) };
        _identityClient.DefaultRequestHeaders.Accept.Clear();
        _identityClient.DefaultRequestHeaders.Accept.Add(new("application/x-www-form-urlencoded"));

        _apiClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
        {
            BaseAddress = new Uri(wireMockUrl)
        };

        _externalClient = new HttpClient { BaseAddress = new Uri(wireMockUrl) };

        var configuration = new PingenConfiguration
        {
            BaseUri = "https://api.test.pingen.com/",
            IdentityUri = "https://identity.test.pingen.com/",
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            DefaultOrganisationId = TestOrganisationId
        };

        var httpClients = new PingenHttpClients(_identityClient, _apiClient, _externalClient);
        var connectionHandler = new PingenConnectionHandler(configuration, httpClients);

        Client = new PingenApiClient(
            connectionHandler,
            new LetterService(connectionHandler),
            new BatchService(connectionHandler),
            new UserService(connectionHandler),
            new OrganisationService(connectionHandler),
            new WebhookService(connectionHandler),
            new FilesService(connectionHandler),
            new DistributionService(connectionHandler));
    }

    /// <summary>
    /// Dispose HTTP clients after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _identityClient.Dispose();
        _apiClient.Dispose();
        _externalClient.Dispose();
    }

    /// <summary>
    /// Stop WireMock server after the fixture completes.
    /// </summary>
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Server.Stop();
        Server.Dispose();
    }

    /// <summary>
    /// Stub the OAuth 2.0 token endpoint to return a valid access token.
    /// </summary>
    private void StubTokenEndpoint()
    {
        Server
            .Given(Request.Create()
                .WithPath("/auth/access-tokens")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonApiStubHelper.TokenResponse()));
    }

    /// <summary>
    /// Build the API path for an organisation-scoped endpoint.
    /// </summary>
    /// <param name="path">Endpoint path (e.g. "batches").</param>
    /// <returns>Full path including organisation prefix.</returns>
    protected static string OrgPath(string path) => $"/organisations/{TestOrganisationId}/{path}";
}
