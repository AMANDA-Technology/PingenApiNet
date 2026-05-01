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
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Services.Connectors;

namespace PingenApiNet.Tests.E2E.Helpers;

/// <summary>
///     Shared cleanup utilities for letter E2E fixtures. Provides best-effort cancel-or-delete
///     for a single letter and a paged orphan-scavenge that walks recent letters and removes
///     anything tagged with the well-known E2E test prefix. Also exposes a bootstrap client
///     factory for fixtures that need to scavenge during <c>[OneTimeSetUp]</c>, before
///     <c>E2eTestBase.Setup()</c> has wired up <c>PingenApiClient</c>.
/// </summary>
internal static class LetterCleanupHelper
{
    /// <summary>
    ///     Common identifier prefix used by all E2E fixtures (matches <c>E2eTestBase.TestPrefix</c>
    ///     format <c>PG-E2E-{guid}</c>). Used by <see cref="ScavengeLetterOrphans" /> to recognise
    ///     letters created by any prior or current E2E run.
    /// </summary>
    internal const string TestPrefixIdentifier = "PG-E2E-";

    /// <summary>
    ///     Best-effort cancel-or-delete for a single letter. Reads current abilities and prefers
    ///     <c>Cancel</c> when available, falling back to <c>Delete</c>. Swallows all exceptions so
    ///     a single resistant orphan cannot break the fixture teardown.
    /// </summary>
    /// <param name="client">Pingen API client used to perform the cleanup.</param>
    /// <param name="letterId">Identifier of the letter to remove.</param>
    internal static async Task TryCleanupLetter(IPingenApiClient client, string letterId)
    {
        try
        {
            ApiResult<SingleResult<LetterDataDetailed>> current = await client.Letters.Get(letterId);
            if (!current.IsSuccess || current.Data?.Data is not { } detail)
                return;

            LetterAbilities abilities = detail.Meta.Abilities.Self;
            if (abilities.Cancel is PingenApiAbility.ok)
                await client.Letters.Cancel(letterId);
            else if (abilities.Delete is PingenApiAbility.ok)
                await client.Letters.Delete(letterId);
        }
        catch (Exception)
        {
            // Best-effort cleanup: a single orphan that resists removal must not fail the fixture.
        }
    }

    /// <summary>
    ///     Walks the most recent letter pages (capped to limit cost on staging) and removes every
    ///     letter whose file name or address contains <see cref="TestPrefixIdentifier" />. Used at
    ///     fixture <c>[OneTimeSetUp]</c> and <c>[OneTimeTearDown]</c> to recover from crashed runs.
    /// </summary>
    /// <param name="client">Pingen API client used to enumerate and delete letters.</param>
    /// <param name="maxPagesToScan">Upper bound on pages scanned; defaults to 5 to avoid org-wide sweeps.</param>
    internal static async Task ScavengeLetterOrphans(IPingenApiClient client, int maxPagesToScan = 5)
    {
        var orphanIds = new List<string>();
        int pagesScanned = 0;

        await foreach (IEnumerable<LetterData> page in client.Letters.GetPageResultsAsync())
        {
            pagesScanned++;
            foreach (LetterData letter in page)
            {
                string fileOriginalName = letter.Attributes.FileOriginalName ?? string.Empty;
                string address = letter.Attributes.Address ?? string.Empty;
                if (fileOriginalName.Contains(TestPrefixIdentifier) || address.Contains(TestPrefixIdentifier))
                    orphanIds.Add(letter.Id);
            }

            if (pagesScanned >= maxPagesToScan)
                break;
        }

        foreach (string id in orphanIds)
            await TryCleanupLetter(client, id);
    }

    /// <summary>
    ///     Construct a fully-wired <see cref="PingenApiClient" /> from environment variables.
    ///     Used by fixtures that need a client during <c>[OneTimeSetUp]</c>, before the per-test
    ///     <c>[SetUp]</c> on <c>E2eTestBase</c> has run.
    /// </summary>
    /// <returns>A new <see cref="PingenApiClient" /> wired to the staging environment.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a required environment variable is missing.</exception>
    internal static PingenApiClient BuildBootstrapClient()
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
