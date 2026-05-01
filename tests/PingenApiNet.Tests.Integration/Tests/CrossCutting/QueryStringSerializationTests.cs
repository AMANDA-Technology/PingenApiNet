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

using System.Web;
using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.Logging;
using WireMock.Types;

namespace PingenApiNet.Tests.Integration.Tests.CrossCutting;

/// <summary>
///     Cross-cutting integration tests verifying that <see cref="ApiPagingRequest" /> and
///     <see cref="ApiRequest" /> properties are serialized to the correct JSON:API query
///     parameters by <c>PingenConnectionHandler</c>.
/// </summary>
[TestFixture]
public sealed class QueryStringSerializationTests : IntegrationTestBase
{
    /// <summary>
    ///     Verifies that nested <c>and</c>/<c>or</c> filter expressions are serialized to a
    ///     URL-encoded JSON document under the <c>filter</c> query parameter, preserving the
    ///     nested operator structure that callers build via <see cref="CollectionFilterOperator" />.
    /// </summary>
    [Test]
    public async Task GetPage_WithNestedAndOrFilter_ShouldSerializeAsUrlEncodedJson()
    {
        Server.StubJsonGet(OrgPath("letters"), PingenResponseFactory.LetterCollection(0));

        var apiPagingRequest = new ApiPagingRequest
        {
            Filtering = new KeyValuePair<string, object>(
                CollectionFilterOperator.And,
                new KeyValuePair<string, object>[]
                {
                    new(CollectionFilterOperator.Or,
                        new KeyValuePair<string, object>[]
                        {
                            new(LetterFields.Country, "CH"), new(LetterFields.Country, "DE")
                        }),
                    new(LetterFields.Status, "valid")
                })
        };

        await Client.Letters.GetPage(apiPagingRequest);

        ILogEntry entry = Server.LogEntries.Single(e => e.RequestMessage?.Path == OrgPath("letters"));
        IDictionary<string, WireMockList<string>>? query = entry.RequestMessage!.Query!;
        query.ShouldContainKey("filter");

        string filterValue = query["filter"][0];
        filterValue.ShouldBe("""{"and":[{"or":[{"country":"CH"},{"country":"DE"}]},{"status":"valid"}]}""");

        string rawQuery = entry.RequestMessage!.RawQuery!;
        rawQuery.ShouldContain("filter=");
        HttpUtility.UrlDecode(rawQuery).ShouldContain(filterValue);
    }

    /// <summary>
    ///     Verifies that multi-field sort instructions are serialized as a comma-separated list under
    ///     the <c>sort</c> query parameter, prefixing fields with <c>-</c> for descending direction.
    /// </summary>
    [Test]
    public async Task GetPage_WithMultiFieldSort_ShouldSerializeAsCommaSeparatedList()
    {
        Server.StubJsonGet(OrgPath("letters"), PingenResponseFactory.LetterCollection(0));

        var apiPagingRequest = new ApiPagingRequest
        {
            Sorting = new[]
            {
                new KeyValuePair<string, CollectionSortDirection>(LetterFields.CreatedAt,
                    CollectionSortDirection.ASC),
                new KeyValuePair<string, CollectionSortDirection>(LetterFields.Status,
                    CollectionSortDirection.DESC)
            }
        };

        await Client.Letters.GetPage(apiPagingRequest);

        ILogEntry entry = Server.LogEntries.Single(e => e.RequestMessage?.Path == OrgPath("letters"));
        IDictionary<string, WireMockList<string>>? query = entry.RequestMessage!.Query!;
        query.ShouldContainKey("sort");
        query["sort"][0].ShouldBe("created_at,-status");
    }

    /// <summary>
    ///     Verifies that the <see cref="ApiPagingRequest.Searching" /> property is serialized to
    ///     the JSON:API <c>q</c> query parameter for free-text search.
    /// </summary>
    [Test]
    public async Task GetPage_WithSearchString_ShouldSerializeAsQParameter()
    {
        Server.StubJsonGet(OrgPath("letters"), PingenResponseFactory.LetterCollection(0));

        const string searchTerm = "important-customer";
        var apiPagingRequest = new ApiPagingRequest { Searching = searchTerm };

        await Client.Letters.GetPage(apiPagingRequest);

        ILogEntry entry = Server.LogEntries.Single(e => e.RequestMessage?.Path == OrgPath("letters"));
        IDictionary<string, WireMockList<string>>? query = entry.RequestMessage!.Query!;
        query.ShouldContainKey("q");
        query["q"][0].ShouldBe(searchTerm);
    }

    /// <summary>
    ///     Verifies that <see cref="ApiPagingRequest.PageNumber" /> and
    ///     <see cref="ApiPagingRequest.PageLimit" /> are serialized to the JSON:API
    ///     <c>page[number]</c> and <c>page[limit]</c> query parameters with brackets
    ///     URL-encoded as <c>%5B</c> / <c>%5D</c> on the wire.
    /// </summary>
    [Test]
    public async Task GetPage_WithPageNumberAndLimit_ShouldSerializePageParameters()
    {
        Server.StubJsonGet(OrgPath("letters"), PingenResponseFactory.LetterCollection(0));

        var apiPagingRequest = new ApiPagingRequest { PageNumber = 3, PageLimit = 50 };

        await Client.Letters.GetPage(apiPagingRequest);

        ILogEntry entry = Server.LogEntries.Single(e => e.RequestMessage?.Path == OrgPath("letters"));
        string decodedQuery = HttpUtility.UrlDecode(entry.RequestMessage!.RawQuery!);

        decodedQuery.ShouldSatisfyAllConditions(
            () => decodedQuery.ShouldContain("page[number]=3"),
            () => decodedQuery.ShouldContain("page[limit]=50"));
    }

    /// <summary>
    ///     Verifies that sparse fieldsets and includes are simultaneously serialized to the
    ///     <c>fields[type]</c> and <c>include</c> query parameters when both are provided on
    ///     a single request, with the brackets in <c>fields[letters]</c> URL-encoded on the wire.
    /// </summary>
    [Test]
    public async Task Get_WithSparseFieldsetsAndInclude_ShouldSerializeBothParameters()
    {
        string letterId = Guid.NewGuid().ToString();
        Server.StubJsonGet(OrgPath($"letters/{letterId}"), PingenResponseFactory.SingleLetter(letterId));

        var apiRequest = new ApiRequest
        {
            SparseFieldsets = new[]
            {
                new KeyValuePair<PingenApiDataType, IEnumerable<string>>(
                    PingenApiDataType.letters,
                    new[] { LetterFields.Status, LetterFields.FileOriginalName })
            },
            Include = new[] { LetterIncludes.Organisation, LetterIncludes.Batch }
        };

        await Client.Letters.Get(letterId, apiRequest);

        ILogEntry entry = Server.LogEntries.Single(e => e.RequestMessage?.Path == OrgPath($"letters/{letterId}"));
        string decodedQuery = HttpUtility.UrlDecode(entry.RequestMessage!.RawQuery!);

        decodedQuery.ShouldSatisfyAllConditions(
            () => decodedQuery.ShouldContain("fields[letters]=status,file_original_name"),
            () => decodedQuery.ShouldContain("include=organisation,batch"));
    }
}
