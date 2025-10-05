using PingenApiNet.Services.Connectors;

namespace PingenApiNet.Tests;

/// <summary>
/// Base class for all pingen API tests
/// </summary>
public abstract class TestBase
{
    /// <summary>
    /// Pingen configuration
    /// </summary>
    private IPingenConfiguration? _pingenConfiguration;

    /// <summary>
    /// Default instance of pingen API client
    /// </summary>
    protected IPingenApiClient? PingenApiClient;

    /// <summary>
    /// Setup
    /// </summary>
    /// <exception cref="Exception"></exception>
    [SetUp]
    public void Setup()
    {
        _pingenConfiguration = new PingenConfiguration
        {
            BaseUri = Environment.GetEnvironmentVariable("PingenApiNet__BaseUri") ?? throw new InvalidOperationException("Missing PingenApiNet__BaseUri"),
            IdentityUri = Environment.GetEnvironmentVariable("PingenApiNet__IdentityUri") ?? throw new InvalidOperationException("Missing PingenApiNet__IdentityUri"),
            ClientId = Environment.GetEnvironmentVariable("PingenApiNet__ClientId") ?? throw new InvalidOperationException("Missing PingenApiNet__ClientId"),
            ClientSecret = Environment.GetEnvironmentVariable("PingenApiNet__ClientSecret") ?? throw new InvalidOperationException("Missing PingenApiNet__ClientSecret"),
            DefaultOrganisationId = Environment.GetEnvironmentVariable("PingenApiNet__OrganisationId") ?? throw new InvalidOperationException("Missing PingenApiNet__OrganisationId")
        };

        PingenApiClient = CreateClient();
    }

    /// <summary>
    /// Teardown
    /// </summary>
    [TearDown]
    public void Teardown()
    {
        PingenApiClient?.Dispose();
    }

    /// <summary>
    /// Create a new pingen API client
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    protected PingenApiClient CreateClient()
    {
        var connectionHandler = new PingenConnectionHandler(_pingenConfiguration!);

        return new(
            connectionHandler,
            new LetterService(connectionHandler),
            new UserService(connectionHandler),
            new OrganisationService(connectionHandler),
            new WebhookService(connectionHandler),
            new FilesService(connectionHandler),
            new DistributionService(connectionHandler));
    }
}
