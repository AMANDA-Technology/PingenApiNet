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

using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace PingenApiNet.Tests.Integration.Tests.CrossCutting;

/// <summary>
///     Cross-cutting integration tests verifying boundary conditions for the
///     <c>IAsyncEnumerable</c> auto-pagination implemented by
///     <c>ConnectorService.AutoPage</c> and surfaced via
///     <c>GetPageResultsAsync</c>.
/// </summary>
[TestFixture]
public sealed class PaginationTests : IntegrationTestBase
{
    /// <summary>
    ///     Verifies that an empty first page (count=0, currentPage=1, lastPage=1) yields no items
    ///     and triggers exactly one API call.
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_EmptyFirstPage_ShouldYieldNoItemsAndStopAfterOneCall()
    {
        Server.StubJsonGet(
            OrgPath("letters"),
            PingenResponseFactory.LetterCollection(0, currentPage: 1, lastPage: 1));

        var allItems = new List<LetterData>();
        await foreach (IEnumerable<LetterData> page in Client.Letters.GetPageResultsAsync()) allItems.AddRange(page);

        allItems.ShouldBeEmpty();
        Server.VerifyCalled(OrgPath("letters"));
    }

    /// <summary>
    ///     Verifies that a single populated page (count=3, currentPage=1, lastPage=1) yields all
    ///     items and triggers exactly one API call.
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_SinglePage_ShouldYieldAllItemsAndStopAfterOneCall()
    {
        Server.StubJsonGet(
            OrgPath("letters"),
            PingenResponseFactory.LetterCollection());

        var allItems = new List<LetterData>();
        await foreach (IEnumerable<LetterData> page in Client.Letters.GetPageResultsAsync()) allItems.AddRange(page);

        allItems.Count.ShouldBe(3);
        Server.VerifyCalled(OrgPath("letters"));
    }

    /// <summary>
    ///     Verifies that across three pages the auto-pager fetches every page until
    ///     <c>currentPage &gt;= lastPage</c>, yielding the union of all items.
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_MultiPage_ShouldFetchAllPages()
    {
        const string scenario = "letters-multi-page";

        Server
            .Given(Request.Create().WithPath(OrgPath("letters")).UsingGet())
            .InScenario(scenario)
            .WillSetStateTo("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(PingenResponseFactory.LetterCollection(2, currentPage: 1, lastPage: 3)));

        Server
            .Given(Request.Create().WithPath(OrgPath("letters")).UsingGet())
            .InScenario(scenario)
            .WhenStateIs("page2")
            .WillSetStateTo("page3")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(PingenResponseFactory.LetterCollection(2, currentPage: 2, lastPage: 3)));

        Server
            .Given(Request.Create().WithPath(OrgPath("letters")).UsingGet())
            .InScenario(scenario)
            .WhenStateIs("page3")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(PingenResponseFactory.LetterCollection(1, currentPage: 3, lastPage: 3)));

        var allItems = new List<LetterData>();
        int pageCount = 0;
        await foreach (IEnumerable<LetterData> page in Client.Letters.GetPageResultsAsync())
        {
            pageCount++;
            allItems.AddRange(page);
        }

        pageCount.ShouldBe(3);
        allItems.Count.ShouldBe(5);
        Server.VerifyCalled(OrgPath("letters"), times: 3);
    }

    /// <summary>
    ///     Verifies that when the last page returns exactly the per-page limit of items, the
    ///     auto-pager stops cleanly without requesting a phantom <c>N+1</c> page.
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_LastPageBoundary_ShouldNotRequestExtraPage()
    {
        const string scenario = "letters-boundary";
        const int perPage = 5;

        Server
            .Given(Request.Create().WithPath(OrgPath("letters")).UsingGet())
            .InScenario(scenario)
            .WillSetStateTo("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(PingenResponseFactory.LetterCollection(
                    perPage,
                    currentPage: 1,
                    lastPage: 2)));

        Server
            .Given(Request.Create().WithPath(OrgPath("letters")).UsingGet())
            .InScenario(scenario)
            .WhenStateIs("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(PingenResponseFactory.LetterCollection(
                    perPage,
                    currentPage: 2,
                    lastPage: 2)));

        var allItems = new List<LetterData>();
        await foreach (IEnumerable<LetterData> page in Client.Letters.GetPageResultsAsync()) allItems.AddRange(page);

        allItems.Count.ShouldBe(perPage * 2);
        Server.VerifyCalled(OrgPath("letters"), times: 2);
    }

    /// <summary>
    ///     Verifies that auto-pagination starts from a non-default initial page number
    ///     and continues until <c>lastPage</c>.
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_StartingFromMiddlePage_ShouldFetchRemainingPages()
    {
        const string scenario = "letters-from-middle";

        Server
            .Given(Request.Create().WithPath(OrgPath("letters")).UsingGet())
            .InScenario(scenario)
            .WillSetStateTo("nextPage")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(PingenResponseFactory.LetterCollection(2, currentPage: 2, lastPage: 3)));

        Server
            .Given(Request.Create().WithPath(OrgPath("letters")).UsingGet())
            .InScenario(scenario)
            .WhenStateIs("nextPage")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(PingenResponseFactory.LetterCollection(2, currentPage: 3, lastPage: 3)));

        var apiPagingRequest = new ApiPagingRequest { PageNumber = 2 };
        var allItems = new List<LetterData>();
        await foreach (IEnumerable<LetterData> page in Client.Letters.GetPageResultsAsync(apiPagingRequest))
            allItems.AddRange(page);

        allItems.Count.ShouldBe(4);
        Server.VerifyCalled(OrgPath("letters"), times: 2);
    }
}
