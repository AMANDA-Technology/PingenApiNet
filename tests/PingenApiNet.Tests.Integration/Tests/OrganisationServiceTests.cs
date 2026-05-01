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

using PingenApiNet.Abstractions.Exceptions;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Organisations;
using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace PingenApiNet.Tests.Integration.Tests;

/// <summary>
///     Integration tests for <see cref="IOrganisationService" />.
/// </summary>
[TestFixture]
public sealed class OrganisationServiceTests : IntegrationTestBase
{
    /// <summary>
    ///     Verifies that GetPage returns a paginated list of organisations.
    /// </summary>
    [Test]
    public async Task GetPage_ShouldReturnOrganisations()
    {
        Server.StubJsonGet("/organisations", PingenResponseFactory.OrganisationCollection());

        ApiResult<CollectionResult<OrganisationData>> result = await Client.Organisations.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(3),
            () => result.Data!.Data[0].Attributes.Name.ShouldNotBeNullOrEmpty());
    }

    /// <summary>
    ///     Verifies that GetPage returns an empty collection when no organisations exist.
    /// </summary>
    [Test]
    public async Task GetPage_ShouldReturnEmptyWhenNoOrganisations()
    {
        Server.StubJsonGet("/organisations", PingenResponseFactory.OrganisationCollection(0));

        ApiResult<CollectionResult<OrganisationData>> result = await Client.Organisations.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(0));
    }

    /// <summary>
    ///     Verifies that GetPage exposes pagination meta on a multi-page response.
    /// </summary>
    [Test]
    public async Task GetPage_ShouldExposePaginationMeta_ForMultiPageResponse()
    {
        Server.StubJsonGet(
            "/organisations",
            PingenResponseFactory.OrganisationCollection(5, 2, 3));

        ApiResult<CollectionResult<OrganisationData>> result = await Client.Organisations.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Meta.CurrentPage.ShouldBe(2),
            () => result.Data!.Meta.LastPage.ShouldBe(3),
            () => result.Data!.Data.Count.ShouldBe(5));
    }

    /// <summary>
    ///     Verifies that GetPage returns an unsuccessful ApiResult on API errors.
    /// </summary>
    [TestCase(401)]
    [TestCase(403)]
    [TestCase(500)]
    [TestCase(503)]
    public async Task GetPage_OnApiError_ShouldReturnUnsuccessfulApiResult(int statusCode)
    {
        Server.StubError(
            "/organisations",
            "GET",
            PingenResponseFactory.ErrorResponse(status: statusCode.ToString()),
            statusCode);

        ApiResult<CollectionResult<OrganisationData>> result = await Client.Organisations.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors.Count.ShouldBeGreaterThan(0),
            () => result.Data.ShouldBeNull());
    }

    /// <summary>
    ///     Verifies that GetPageResultsAsync auto-paginates across two pages.
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_ShouldAutoPaginate()
    {
        Server
            .Given(Request.Create()
                .WithPath("/organisations")
                .UsingGet())
            .InScenario("org-paging")
            .WillSetStateTo("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(PingenResponseFactory.OrganisationCollection(1, 1, 2)));

        Server
            .Given(Request.Create()
                .WithPath("/organisations")
                .UsingGet())
            .InScenario("org-paging")
            .WhenStateIs("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(PingenResponseFactory.OrganisationCollection(1, 2, 2)));

        var allItems = new List<string>();
        await foreach (IEnumerable<OrganisationData> page in Client.Organisations.GetPageResultsAsync())
            allItems.AddRange(page.Select(item => item.Id));

        allItems.Count.ShouldBe(2);
    }

    /// <summary>
    ///     Verifies that GetPageResultsAsync stops after a single page when only one exists.
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_ShouldYieldSinglePage_WhenOnlyOneExists()
    {
        Server.StubJsonGet("/organisations", PingenResponseFactory.OrganisationCollection(2));

        var allItems = new List<string>();
        await foreach (IEnumerable<OrganisationData> page in Client.Organisations.GetPageResultsAsync())
            allItems.AddRange(page.Select(item => item.Id));

        allItems.Count.ShouldBe(2);
    }

    /// <summary>
    ///     Verifies that GetPageResultsAsync surfaces a <see cref="PingenApiErrorException" /> when the underlying call
    ///     fails.
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_OnApiError_ShouldThrowPingenApiErrorException()
    {
        Server.StubError(
            "/organisations",
            "GET",
            PingenResponseFactory.ErrorResponse("Server Error", "Service unavailable", "500"),
            500);

        PingenApiErrorException exception = await Should.ThrowAsync<PingenApiErrorException>(async () =>
        {
            await foreach (IEnumerable<OrganisationData> _ in Client.Organisations.GetPageResultsAsync())
            {
                // Should never iterate
            }
        });

        exception.ApiResult.ShouldNotBeNull();
        exception.ApiResult!.IsSuccess.ShouldBeFalse();
    }

    /// <summary>
    ///     Verifies that Get returns a single organisation with detailed attributes.
    /// </summary>
    [Test]
    public async Task Get_ShouldReturnSingleOrganisation()
    {
        string orgId = Guid.NewGuid().ToString();

        Server.StubJsonGet($"/organisations/{orgId}", PingenResponseFactory.SingleOrganisation(orgId));

        ApiResult<SingleResult<OrganisationDataDetailed>> result = await Client.Organisations.Get(orgId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(orgId),
            () => result.Data!.Data.Attributes.Name.ShouldNotBeNullOrEmpty(),
            () => result.Data!.Data.Attributes.BillingCurrency.ShouldBe("CHF"));
    }

    /// <summary>
    ///     Verifies that Get surfaces a 404 not-found error via ApiResult.
    /// </summary>
    [Test]
    public async Task Get_ShouldReturnErrorWhenNotFound()
    {
        string orgId = Guid.NewGuid().ToString();

        Server.StubError(
            $"/organisations/{orgId}",
            "GET",
            PingenResponseFactory.ErrorResponse("Not Found", "The requested organisation does not exist.", "404"),
            404);

        ApiResult<SingleResult<OrganisationDataDetailed>> result = await Client.Organisations.Get(orgId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors[0].Title.ShouldBe("Not Found"),
            () => result.Data.ShouldBeNull());
    }
}
