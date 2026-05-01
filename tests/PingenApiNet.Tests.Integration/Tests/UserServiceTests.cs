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
using PingenApiNet.Abstractions.Models.UserAssociations;
using PingenApiNet.Abstractions.Models.Users;
using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace PingenApiNet.Tests.Integration.Tests;

/// <summary>
///     Integration tests for <see cref="IUserService" />.
/// </summary>
[TestFixture]
public sealed class UserServiceTests : IntegrationTestBase
{
    /// <summary>
    ///     Verifies that Get returns the authenticated user.
    /// </summary>
    [Test]
    public async Task Get_ShouldReturnAuthenticatedUser()
    {
        string userId = Guid.NewGuid().ToString();

        Server.StubJsonGet("/user", PingenResponseFactory.SingleUser(userId));

        ApiResult<SingleResult<UserDataDetailed>> result = await Client.Users.Get();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(userId),
            () => result.Data!.Data.Attributes.Email.ShouldNotBeNullOrEmpty(),
            () => result.Data!.Data.Attributes.FirstName.ShouldNotBeNullOrEmpty(),
            () => result.Data!.Data.Attributes.LastName.ShouldNotBeNullOrEmpty());
    }

    /// <summary>
    ///     Verifies that Get surfaces JSON:API errors as an unsuccessful ApiResult.
    /// </summary>
    [TestCase(401)]
    [TestCase(403)]
    [TestCase(500)]
    [TestCase(503)]
    public async Task Get_OnApiError_ShouldReturnUnsuccessfulApiResult(int statusCode)
    {
        Server.StubError(
            "/user",
            "GET",
            PingenResponseFactory.ErrorResponse(status: statusCode.ToString()),
            statusCode);

        ApiResult<SingleResult<UserDataDetailed>> result = await Client.Users.Get();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors.Count.ShouldBeGreaterThan(0),
            () => result.Data.ShouldBeNull());
    }

    /// <summary>
    ///     Verifies that GetAssociationsPage returns a paginated list of associations.
    /// </summary>
    [Test]
    public async Task GetAssociationsPage_ShouldReturnAssociations()
    {
        Server.StubJsonGet("/user/associations", PingenResponseFactory.UserAssociationCollection());

        ApiResult<CollectionResult<UserAssociationDataDetailed>> result = await Client.Users.GetAssociationsPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(3),
            () => result.Data!.Data[0].Attributes.Role.ShouldNotBeNull());
    }

    /// <summary>
    ///     Verifies that GetAssociationsPage returns an empty collection when no associations exist.
    /// </summary>
    [Test]
    public async Task GetAssociationsPage_ShouldReturnEmptyWhenNoAssociations()
    {
        Server.StubJsonGet("/user/associations", PingenResponseFactory.UserAssociationCollection(0));

        ApiResult<CollectionResult<UserAssociationDataDetailed>> result = await Client.Users.GetAssociationsPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(0));
    }

    /// <summary>
    ///     Verifies that GetAssociationsPage exposes pagination meta on a multi-page response.
    /// </summary>
    [Test]
    public async Task GetAssociationsPage_ShouldExposePaginationMeta_ForMultiPageResponse()
    {
        Server.StubJsonGet(
            "/user/associations",
            PingenResponseFactory.UserAssociationCollection(4, 1, 3));

        ApiResult<CollectionResult<UserAssociationDataDetailed>> result = await Client.Users.GetAssociationsPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Meta.CurrentPage.ShouldBe(1),
            () => result.Data!.Meta.LastPage.ShouldBe(3),
            () => result.Data!.Data.Count.ShouldBe(4));
    }

    /// <summary>
    ///     Verifies that GetAssociationsPage returns an unsuccessful ApiResult on API errors.
    /// </summary>
    [TestCase(401)]
    [TestCase(403)]
    [TestCase(500)]
    [TestCase(503)]
    public async Task GetAssociationsPage_OnApiError_ShouldReturnUnsuccessfulApiResult(int statusCode)
    {
        Server.StubError(
            "/user/associations",
            "GET",
            PingenResponseFactory.ErrorResponse(status: statusCode.ToString()),
            statusCode);

        ApiResult<CollectionResult<UserAssociationDataDetailed>> result = await Client.Users.GetAssociationsPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors.Count.ShouldBeGreaterThan(0),
            () => result.Data.ShouldBeNull());
    }

    /// <summary>
    ///     Verifies that GetAssociationsPageResultsAsync auto-paginates across two pages.
    /// </summary>
    [Test]
    public async Task GetAssociationsPageResultsAsync_ShouldAutoPaginate()
    {
        Server
            .Given(Request.Create()
                .WithPath("/user/associations")
                .UsingGet())
            .InScenario("user-assoc-paging")
            .WillSetStateTo("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(PingenResponseFactory.UserAssociationCollection(1, 1, 2)));

        Server
            .Given(Request.Create()
                .WithPath("/user/associations")
                .UsingGet())
            .InScenario("user-assoc-paging")
            .WhenStateIs("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(PingenResponseFactory.UserAssociationCollection(1, 2, 2)));

        var allItems = new List<string>();
        await foreach (IEnumerable<UserAssociationDataDetailed> page in Client.Users.GetAssociationsPageResultsAsync())
            allItems.AddRange(page.Select(item => item.Id));

        allItems.Count.ShouldBe(2);
    }

    /// <summary>
    ///     Verifies that GetAssociationsPageResultsAsync stops after a single page when only one exists.
    /// </summary>
    [Test]
    public async Task GetAssociationsPageResultsAsync_ShouldYieldSinglePage_WhenOnlyOneExists()
    {
        Server.StubJsonGet("/user/associations", PingenResponseFactory.UserAssociationCollection(2));

        var allItems = new List<string>();
        await foreach (IEnumerable<UserAssociationDataDetailed> page in Client.Users.GetAssociationsPageResultsAsync())
            allItems.AddRange(page.Select(item => item.Id));

        allItems.Count.ShouldBe(2);
    }

    /// <summary>
    ///     Verifies that GetAssociationsPageResultsAsync surfaces a <see cref="PingenApiErrorException" /> when the
    ///     underlying call fails.
    /// </summary>
    [Test]
    public async Task GetAssociationsPageResultsAsync_OnApiError_ShouldThrowPingenApiErrorException()
    {
        Server.StubError(
            "/user/associations",
            "GET",
            PingenResponseFactory.ErrorResponse("Forbidden", "Access denied", "403"),
            403);

        PingenApiErrorException exception = await Should.ThrowAsync<PingenApiErrorException>(async () =>
        {
            await foreach (IEnumerable<UserAssociationDataDetailed> _ in Client.Users.GetAssociationsPageResultsAsync())
            {
                // Should never iterate
            }
        });

        exception.ApiResult.ShouldNotBeNull();
        exception.ApiResult!.IsSuccess.ShouldBeFalse();
    }
}
