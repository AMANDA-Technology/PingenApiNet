/*
MIT License

Copyright (c) 2022 Philip Näf <philip.naef@amanda-technology.ch>
Copyright (c) 2022 Manuel Gysin <manuel.gysin@amanda-technology.ch>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Webhooks;
using PingenApiNet.Abstractions.Models.Webhooks.Views;
using PingenApiNet.Services.Connectors;

namespace PingenApiNet.Tests.E2E.Webhooks;

/// <summary>
///     End-to-end tests for the Pingen webhooks endpoint. Exercises create, get, list,
///     and delete operations against the live staging API. Orphan webhooks from crashed
///     prior runs are scavenged once before the fixture starts so they cannot pollute
///     state across runs.
/// </summary>
[TestFixture]
[Category("E2e")]
public sealed class WebhookE2eTests : E2eTestBase
{
    private const string TestPrefixIdentifier = "PG-E2E-";

    private string? _createdWebhookId;

    /// <summary>
    ///     Removes any webhook tagged with <see cref="TestPrefixIdentifier" /> that was left behind
    ///     by a crashed previous run. Runs once before the fixture begins; constructs its own
    ///     client because the base class only sets <see cref="E2eTestBase.PingenApiClient" /> in
    ///     the per-test <c>[SetUp]</c> hook.
    /// </summary>
    [OneTimeSetUp]
    public async Task ScavengeOrphansBeforeRunning()
    {
        IPingenApiClient bootstrapClient = BuildBootstrapClient();
        await ScavengeWebhookOrphans(bootstrapClient);
    }

    /// <summary>
    ///     Creates a new webhook tagged with <see cref="E2eTestBase.TestPrefix" /> in the URL,
    ///     remembers its id for downstream tests, and registers an idempotent delete in the
    ///     LIFO cleanup queue.
    /// </summary>
    [Test]
    [Order(1)]
    public async Task Create_ShouldCreateWebhook()
    {
        PingenApiClient.ShouldNotBeNull();

        var data = new DataPost<WebhookCreate>
        {
            Type = PingenApiDataType.webhooks,
            Attributes = new WebhookCreate
            {
                FileOriginalName = WebhookEventCategory.issues,
                Url = new Uri($"https://example.com/{TestPrefix}/webhook"),
                SigningKey = Guid.NewGuid().ToString("N")
            }
        };

        ApiResult<SingleResult<WebhookData>> result = await PingenApiClient!.Webhooks.Create(data);

        AssertSuccess(result);
        result.Data!.Data.Id.ShouldNotBeNullOrEmpty();
        result.Data.Data.Attributes.Url.ShouldNotBeNull();
        result.Data.Data.Attributes.Url!.ToString().ShouldContain(TestPrefix);

        _createdWebhookId = result.Data.Data.Id;

        string idForCleanup = _createdWebhookId;
        RegisterCleanup(async () =>
        {
            try
            {
                await PingenApiClient!.Webhooks.Delete(idForCleanup);
            }
            catch (Exception)
            {
                // Cleanup is best-effort: the explicit Delete in test Order(4) may have already removed it.
            }
        });
    }

    /// <summary>
    ///     Verifies that the webhook created in <see cref="Create_ShouldCreateWebhook" /> can be fetched by id.
    /// </summary>
    [Test]
    [Order(2)]
    public async Task Get_ShouldReturnWebhookById()
    {
        PingenApiClient.ShouldNotBeNull();
        _createdWebhookId.ShouldNotBeNullOrEmpty();

        ApiResult<SingleResult<WebhookData>> result = await PingenApiClient!.Webhooks.Get(_createdWebhookId!);

        AssertSuccess(result);
        result.Data!.Data.Id.ShouldBe(_createdWebhookId);
        result.Data.Data.Attributes.Url.ShouldNotBeNull();
        result.Data.Data.Attributes.Url!.ToString().ShouldContain(TestPrefix);
    }

    /// <summary>
    ///     Verifies that paginated webhook listing surfaces the webhook created in
    ///     <see cref="Create_ShouldCreateWebhook" />.
    /// </summary>
    [Test]
    [Order(3)]
    public async Task GetPage_ShouldReturnWebhooks_IncludingCreated()
    {
        PingenApiClient.ShouldNotBeNull();
        _createdWebhookId.ShouldNotBeNullOrEmpty();

        var allItems = new List<WebhookData>();
        await foreach (IEnumerable<WebhookData> page in PingenApiClient!.Webhooks.GetPageResultsAsync())
            allItems.AddRange(page);

        allItems.ShouldNotBeEmpty();
        allItems.ShouldContain(item => item.Id == _createdWebhookId);
    }

    /// <summary>
    ///     Explicitly deletes the webhook created in <see cref="Create_ShouldCreateWebhook" /> before
    ///     the LIFO cleanup queue runs. This proves the live Delete path independently of teardown.
    /// </summary>
    [Test]
    [Order(4)]
    public async Task Delete_ShouldRemoveWebhook()
    {
        PingenApiClient.ShouldNotBeNull();
        _createdWebhookId.ShouldNotBeNullOrEmpty();

        ApiResult result = await PingenApiClient!.Webhooks.Delete(_createdWebhookId!);

        AssertSuccess(result);
    }

    /// <summary>
    ///     End-of-fixture sweep that catches any webhook still tagged with
    ///     <see cref="TestPrefixIdentifier" /> after the LIFO queue has run.
    /// </summary>
    protected override async Task ScavengeOrphans()
    {
        if (PingenApiClient is not null)
            await ScavengeWebhookOrphans(PingenApiClient);
    }

    private static async Task ScavengeWebhookOrphans(IPingenApiClient client)
    {
        var orphanIds = new List<string>();
        await foreach (IEnumerable<WebhookData> page in client.Webhooks.GetPageResultsAsync())
        foreach (WebhookData webhook in page)
        {
            string url = webhook.Attributes.Url?.ToString() ?? string.Empty;
            if (url.Contains(TestPrefixIdentifier))
                orphanIds.Add(webhook.Id);
        }

        foreach (string id in orphanIds)
            try
            {
                await client.Webhooks.Delete(id);
            }
            catch (Exception)
            {
                // Best-effort cleanup: do not fail the fixture if a single orphan cannot be deleted.
            }
    }

    private static PingenApiClient BuildBootstrapClient()
    {
        var configuration = new PingenConfiguration
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
            DefaultOrganisationId =
                Environment.GetEnvironmentVariable("PingenApiNet__OrganisationId") ??
                throw new InvalidOperationException("Missing PingenApiNet__OrganisationId")
        };

        var connectionHandler = new PingenConnectionHandler(configuration, PingenHttpClients.Create(configuration));

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
}
