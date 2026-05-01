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

using System.Text.Json;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Batches;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Organisations;
using PingenApiNet.Tests.Integration.Helpers;

namespace PingenApiNet.Tests.Integration.Tests.CrossCutting;

/// <summary>
///     Cross-cutting integration tests verifying deserialization behavior on sparse, complex,
///     or unusually large JSON:API responses.
/// </summary>
[TestFixture]
public sealed class EdgeCaseTests : IntegrationTestBase
{
    /// <summary>
    ///     Verifies that an empty <c>data: []</c> payload deserialises to an empty list rather
    ///     than null, so callers can iterate without null guards.
    /// </summary>
    [Test]
    public async Task GetPage_EmptyCollection_ShouldReturnEmptyList()
    {
        Server.StubJsonGet(OrgPath("batches"),
            PingenResponseFactory.BatchCollection(0));

        ApiResult<CollectionResult<BatchData>> result = await Client.Batches.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.ShouldNotBeNull(),
            () => result.Data!.Data.ShouldBeEmpty());
    }

    /// <summary>
    ///     Verifies that a single resource with all-null optional attribute fields deserialises
    ///     without throwing, so sparse fieldset responses are tolerated.
    /// </summary>
    [Test]
    public async Task Get_WithAllNullOptionalFields_ShouldDeserializeSuccessfully()
    {
        string batchId = Guid.NewGuid().ToString();
        string body = JsonSerializer.Serialize(new
        {
            data = new
            {
                id = batchId,
                type = "batches",
                attributes =
                    new
                    {
                        name = (string?)null,
                        icon = (string?)null,
                        status = (string?)null,
                        file_original_name = (string?)null,
                        letter_count = (int?)null,
                        address_position = (string?)null,
                        print_mode = (string?)null,
                        print_spectrum = (string?)null,
                        price_currency = (string?)null,
                        price_value = (double?)null,
                        submitted_at = (string?)null,
                        created_at = (string?)null,
                        updated_at = (string?)null
                    },
                relationships =
                    new
                    {
                        organisation = JsonApiStubHelper.RelatedSingle(TestOrganisationId, "organisations"),
                        events = JsonApiStubHelper.RelatedMany()
                    },
                meta = JsonApiStubHelper.MetaWithAbilities(new
                {
                    cancel = "ok",
                    delete = "ok",
                    submit = "ok",
                    edit = "ok",
                    change_window_position = "ok",
                    add_attachment = "ok"
                })
            }
        });

        Server.StubJsonGet(OrgPath($"batches/{batchId}"), body);

        ApiResult<SingleResult<BatchDataDetailed>> result = await Client.Batches.Get(batchId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(batchId),
            () => result.Data!.Data.Attributes.Name.ShouldBeNull(),
            () => result.Data!.Data.Attributes.Status.ShouldBeNull(),
            () => result.Data!.Data.Attributes.LetterCount.ShouldBeNull());
    }

    /// <summary>
    ///     Verifies that a JSON:API response containing an <c>included</c> array is deserialised
    ///     into <see cref="CollectionResult{T}.Included" /> with the included items accessible via
    ///     <c>OfType&lt;T&gt;</c>.
    /// </summary>
    [Test]
    public async Task GetPage_WithIncludedArray_ShouldDeserializeIncluded()
    {
        string orgId = TestOrganisationId;
        string letterId = Guid.NewGuid().ToString();

        string body = JsonSerializer.Serialize(new
        {
            data =
                new[]
                {
                    new
                    {
                        id = letterId,
                        type = "letters",
                        attributes =
                            new { status = "valid", file_original_name = "test.pdf", file_pages = 1 },
                        relationships =
                            new
                            {
                                organisation = JsonApiStubHelper.RelatedSingle(orgId, "organisations"),
                                events = JsonApiStubHelper.RelatedMany()
                            }
                    }
                },
            included = new object[]
            {
                new
                {
                    id = orgId,
                    type = "organisations",
                    attributes =
                        new
                        {
                            name = "Included Org",
                            status = "active",
                            plan = "professional",
                            billing_mode = "prepaid",
                            billing_currency = "CHF",
                            billing_balance = 100.0,
                            default_country = "CH"
                        }
                }
            },
            links = new
            {
                first = "https://api.test.pingen.com/?page[number]=1",
                last = "https://api.test.pingen.com/?page[number]=1",
                prev = (string?)null,
                next = (string?)null,
                self = "https://api.test.pingen.com/?page[number]=1"
            },
            meta = new
            {
                current_page = 1,
                last_page = 1,
                per_page = 20,
                from = 1,
                to = 1,
                total = 1
            }
        });

        Server.StubJsonGet(OrgPath("letters"), body);

        ApiResult<CollectionResult<LetterData>> result = await Client.Letters.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Included.ShouldNotBeNull(),
            () => result.Data!.Included!.Count.ShouldBe(1));

        var includedOrgs = result.Data!.Included!.OfType<Organisation>().ToList();
        includedOrgs.Count.ShouldBe(1);
        includedOrgs[0].Id.ShouldBe(orgId);
        includedOrgs[0].Attributes.Name.ShouldBe("Included Org");
    }

    /// <summary>
    ///     Verifies that a large page (100 items) deserialises every item without truncation.
    /// </summary>
    [Test]
    public async Task GetPage_With100Items_ShouldDeserializeAll()
    {
        const int itemCount = 100;

        Server.StubJsonGet(OrgPath("batches"),
            PingenResponseFactory.BatchCollection(itemCount));

        ApiResult<CollectionResult<BatchData>> result = await Client.Batches.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(itemCount));

        result.Data!.Data.Select(d => d.Id).Distinct().Count().ShouldBe(itemCount);
    }

    /// <summary>
    ///     Verifies that a response with a missing <c>included</c> key (not just null) deserialises
    ///     successfully and surfaces the included collection as null on the result.
    /// </summary>
    [Test]
    public async Task GetPage_WithoutIncludedKey_ShouldHaveNullIncluded()
    {
        Server.StubJsonGet(OrgPath("batches"),
            PingenResponseFactory.BatchCollection(2));

        ApiResult<CollectionResult<BatchData>> result = await Client.Batches.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Included.ShouldBeNull());
    }
}
