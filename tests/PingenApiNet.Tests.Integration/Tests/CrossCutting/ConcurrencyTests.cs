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

using System.Net.Http.Headers;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Batches;
using PingenApiNet.Services.Connectors;
using PingenApiNet.Tests.Integration.Helpers;

namespace PingenApiNet.Tests.Integration.Tests.CrossCutting;

/// <summary>
///     Cross-cutting integration tests verifying concurrent request safety and per-handler
///     token isolation when multiple <see cref="PingenApiClient" /> instances target distinct
///     organisations against a shared WireMock server.
/// </summary>
[TestFixture]
public sealed class ConcurrencyTests : IntegrationTestBase
{
    /// <summary>
    ///     Dispose any HttpClient instances created by <see cref="CreateClient" /> after each test.
    /// </summary>
    [TearDown]
    public void TearDownClients()
    {
        foreach (HttpClient c in _ownedClients) c.Dispose();

        _ownedClients.Clear();
    }

    private readonly List<HttpClient> _ownedClients = [];

    /// <summary>
    ///     Verifies that two clients with different organisation IDs can run requests in parallel
    ///     and each receive the data scoped to their own organisation, without token cross-contamination.
    /// </summary>
    [Test]
    public async Task MultipleClients_DifferentOrganisations_ShouldIsolateRequests()
    {
        const string orgA = "org-A";
        const string orgB = "org-B";

        Server.StubJsonGet($"/organisations/{orgA}/batches",
            PingenResponseFactory.BatchCollection(2, orgA));
        Server.StubJsonGet($"/organisations/{orgB}/batches",
            PingenResponseFactory.BatchCollection(5, orgB));

        IPingenApiClient clientA = CreateClient(orgA);
        IPingenApiClient clientB = CreateClient(orgB);

        Task<ApiResult<CollectionResult<BatchData>>> resultATask = clientA.Batches.GetPage();
        Task<ApiResult<CollectionResult<BatchData>>> resultBTask = clientB.Batches.GetPage();

        await Task.WhenAll(resultATask, resultBTask);

        ApiResult<CollectionResult<BatchData>> resultA = await resultATask;
        ApiResult<CollectionResult<BatchData>> resultB = await resultBTask;

        resultA.ShouldSatisfyAllConditions(
            () => resultA.IsSuccess.ShouldBeTrue(),
            () => resultA.Data.ShouldNotBeNull(),
            () => resultA.Data!.Data.Count.ShouldBe(2));

        resultB.ShouldSatisfyAllConditions(
            () => resultB.IsSuccess.ShouldBeTrue(),
            () => resultB.Data.ShouldNotBeNull(),
            () => resultB.Data!.Data.Count.ShouldBe(5));

        Server.VerifyCalled($"/organisations/{orgA}/batches");
        Server.VerifyCalled($"/organisations/{orgB}/batches");
    }

    /// <summary>
    ///     Verifies that many parallel requests against the same client all complete successfully,
    ///     exercising the authentication semaphore's double-check guard for concurrent first-request
    ///     authentication.
    /// </summary>
    [Test]
    public async Task SingleClient_ManyParallelRequests_ShouldAllSucceed()
    {
        IPingenApiClient freshClient = CreateClient(TestOrganisationId);

        Server.StubJsonGet(OrgPath("batches"), PingenResponseFactory.BatchCollection(1));

        Task<ApiResult<CollectionResult<BatchData>>>[] tasks = Enumerable.Range(0, 10)
            .Select(_ => freshClient.Batches.GetPage())
            .ToArray();

        ApiResult<CollectionResult<BatchData>>[] results = await Task.WhenAll(tasks);

        results.Length.ShouldBe(10);
        foreach (ApiResult<CollectionResult<BatchData>> r in results)
        {
            r.IsSuccess.ShouldBeTrue();
            r.Data.ShouldNotBeNull();
        }

        Server.VerifyCalledAtLeastOnce(OrgPath("batches"));
    }

    /// <summary>
    ///     Build a fresh <see cref="PingenApiClient" /> wired to the shared WireMock server with
    ///     its own isolated set of <see cref="HttpClient" /> instances and an explicit organisation ID.
    /// </summary>
    private IPingenApiClient CreateClient(string organisationId)
    {
        string url = Server.Url!;

        var identityClient = new HttpClient { BaseAddress = new Uri(url) };
        identityClient.DefaultRequestHeaders.Accept.Clear();
        identityClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

        var apiClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
        {
            BaseAddress = new Uri(url)
        };

        var externalClient = new HttpClient { BaseAddress = new Uri(url) };

        _ownedClients.Add(identityClient);
        _ownedClients.Add(apiClient);
        _ownedClients.Add(externalClient);

        var configuration = new PingenConfiguration
        {
            BaseUri = "https://api.test.pingen.com/",
            IdentityUri = "https://identity.test.pingen.com/",
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            DefaultOrganisationId = organisationId
        };

        var httpClients = new PingenHttpClients(identityClient, apiClient, externalClient);
        var connectionHandler = new PingenConnectionHandler(configuration, httpClients);

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
