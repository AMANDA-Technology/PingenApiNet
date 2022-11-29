using PingenApiNet.Services.Connectors;

namespace PingenApiNet.Tests;

/// <summary>
///
/// </summary>
public class TestBase
{
    protected IPingenApiClient? PingenApiClient;

    /// <summary>
    ///
    /// </summary>
    /// <exception cref="Exception"></exception>
    [SetUp]
    public void Setup()
    {
        var baseUri = Environment.GetEnvironmentVariable("PingenApiNet__BaseUri") ?? throw new("Missing PingenApiNet__BaseUri");
        var identityUri = Environment.GetEnvironmentVariable("PingenApiNet__IdentityUri") ?? throw new("Missing PingenApiNet__IdentityUri");
        var clientId = Environment.GetEnvironmentVariable("PingenApiNet__ClientId") ?? throw new("Missing PingenApiNet__ClientId");
        var clientSecret = Environment.GetEnvironmentVariable("PingenApiNet__ClientSecret") ?? throw new("Missing PingenApiNet__ClientSecret");
        var organisationId = Environment.GetEnvironmentVariable("PingenApiNet__OrganisationId") ?? throw new("Missing PingenApiNet__OrganisationId");
        var connectionHandler = new PingenConnectionHandler(
            new PingenConfiguration
            {
                BaseUri = baseUri,
                IdentityUri = identityUri,
                ClientId = clientId,
                ClientSecret = clientSecret,
                DefaultOrganisationId = organisationId
            });

        PingenApiClient = new PingenApiClient(
            connectionHandler,
            new LetterService(connectionHandler),
            new UserService(connectionHandler),
            new OrganisationService(connectionHandler),
            new WebhookService(connectionHandler),
            new FilesService(connectionHandler),
            new DistributionService(connectionHandler));
    }
}
