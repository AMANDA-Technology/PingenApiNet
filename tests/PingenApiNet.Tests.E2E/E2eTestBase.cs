using PingenApiNet.Abstractions.Interfaces.Data;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Services.Connectors;

namespace PingenApiNet.Tests.E2E;

/// <summary>
///     Base class for all Pingen E2E tests. Provides client construction, LIFO resource cleanup,
///     orphan scavenging, and assertion helpers.
/// </summary>
public abstract class E2eTestBase
{
    private readonly Stack<Func<Task>> _cleanupQueue = new();

    /// <summary>
    ///     Default Pingen API client wired to the staging environment.
    /// </summary>
    protected IPingenApiClient? PingenApiClient;

    private IPingenConfiguration? _pingenConfiguration;

    /// <summary>
    ///     Unique prefix for all test data created in this fixture; used to identify and scavenge orphans.
    ///     Format: <c>PG-E2E-{guid}</c>.
    /// </summary>
    protected string TestPrefix { get; } = $"PG-E2E-{Guid.NewGuid():N}";

    /// <summary>
    ///     Create client and read configuration from environment variables before each test.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when a required environment variable is missing.</exception>
    [SetUp]
    public void Setup()
    {
        _pingenConfiguration = new PingenConfiguration
        {
            BaseUri =
                Environment.GetEnvironmentVariable("PingenApiNet__BaseUri") ??
                throw new InvalidOperationException("Missing PingenApiNet__BaseUri"),
            IdentityUri =
                Environment.GetEnvironmentVariable("PingenApiNet__IdentityUri") ??
                throw new InvalidOperationException("Missing PingenApiNet__IdentityUri"),
            ClientId =
                Environment.GetEnvironmentVariable("PingenApiNet__ClientId") ??
                throw new InvalidOperationException("Missing PingenApiNet__ClientId"),
            ClientSecret =
                Environment.GetEnvironmentVariable("PingenApiNet__ClientSecret") ??
                throw new InvalidOperationException("Missing PingenApiNet__ClientSecret"),
            DefaultOrganisationId = Environment.GetEnvironmentVariable("PingenApiNet__OrganisationId") ??
                                    throw new InvalidOperationException("Missing PingenApiNet__OrganisationId")
        };

        PingenApiClient = CreateClient();
    }

    /// <summary>
    ///     Scavenge orphaned resources then run LIFO cleanup queue after all tests in the fixture complete.
    /// </summary>
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await ScavengeOrphans();
        await RunCleanup();
    }

    /// <summary>
    ///     Register a cleanup action to be executed in LIFO order at <see cref="OneTimeTearDown" />.
    /// </summary>
    /// <param name="cleanup">Async cleanup delegate.</param>
    protected void RegisterCleanup(Func<Task> cleanup) => _cleanupQueue.Push(cleanup);

    /// <summary>
    ///     Execute all registered cleanup actions in LIFO order.
    /// </summary>
    protected async Task RunCleanup()
    {
        while (_cleanupQueue.TryPop(out Func<Task>? cleanup))
            await cleanup();
    }

    /// <summary>
    ///     Override to scan the Pingen API for resources tagged with <see cref="TestPrefix" /> and
    ///     delete any that were not cleaned up by the normal flow.
    /// </summary>
    protected virtual Task ScavengeOrphans() => Task.CompletedTask;

    // ── Factory ──────────────────────────────────────────────────────────────

    /// <summary>
    ///     Create a new Pingen API client using the current configuration.
    /// </summary>
    /// <returns>A fully-wired <see cref="PingenApiNet.PingenApiClient" />.</returns>
    /// <exception cref="InvalidOperationException">Thrown when configuration has not been initialised.</exception>
    protected PingenApiClient CreateClient()
    {
        IPingenConfiguration cfg = _pingenConfiguration ??
                                   throw new InvalidOperationException(
                                       "Configuration not initialised — call Setup() first.");
        var connectionHandler = new PingenConnectionHandler(cfg, PingenHttpClients.Create(cfg));

        return new PingenApiClient(
            connectionHandler,
            new LetterService(connectionHandler),
            new BatchService(connectionHandler),
            new UserService(connectionHandler),
            new OrganisationService(connectionHandler),
            new WebhookService(connectionHandler),
            new FilesService(connectionHandler),
            new DistributionService(connectionHandler));
    }

    // ── Assertion Helpers ─────────────────────────────────────────────────────

    /// <summary>
    ///     Assert that a non-data API result succeeded with no error.
    /// </summary>
    /// <param name="result">The API result to verify.</param>
    protected static void AssertSuccess(ApiResult result)
    {
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.ApiError.ShouldBeNull();
    }

    /// <summary>
    ///     Assert that a data API result succeeded with non-null data and no error.
    /// </summary>
    /// <typeparam name="T">The data result type.</typeparam>
    /// <param name="result">The API result to verify.</param>
    protected static void AssertSuccess<T>(ApiResult<T> result) where T : class, IDataResult
    {
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.ApiError.ShouldBeNull();
        result.Data.ShouldNotBeNull();
    }
}
