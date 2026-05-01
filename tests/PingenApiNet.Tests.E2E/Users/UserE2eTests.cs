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
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.UserAssociations;
using PingenApiNet.Abstractions.Models.Users;

namespace PingenApiNet.Tests.E2E.Users;

/// <summary>
///     End-to-end tests for the Pingen user endpoint. Exercises a paginated association
///     list and a single-resource lookup of the authenticated user against the live staging API.
/// </summary>
[TestFixture]
[Category("E2e")]
public sealed class UserE2eTests : E2eTestBase
{
    /// <summary>
    ///     Verifies that a paginated list of user associations can be retrieved and exposes pagination meta.
    /// </summary>
    [Test]
    public async Task GetAssociationsPage_ShouldReturnPaginatedAssociations()
    {
        PingenApiClient.ShouldNotBeNull();

        ApiResult<CollectionResult<UserAssociationDataDetailed>> result =
            await PingenApiClient!.Users.GetAssociationsPage(
                new ApiPagingRequest { PageNumber = 1, PageLimit = 20 });

        AssertSuccess(result);
        result.Data!.Data.ShouldNotBeNull();
        result.Data.Meta.CurrentPage.ShouldNotBeNull();
        result.Data.Meta.CurrentPage!.Value.ShouldBeGreaterThanOrEqualTo(1);
        result.Data.Meta.LastPage.ShouldNotBeNull();
        result.Data.Meta.LastPage!.Value.ShouldBeGreaterThanOrEqualTo(1);
    }

    /// <summary>
    ///     Verifies that the authenticated user can be fetched and that core attributes are populated.
    ///     The Pingen user endpoint identifies the user from the bearer token rather than a path id.
    /// </summary>
    [Test]
    public async Task Get_ShouldReturnAuthenticatedUserById()
    {
        PingenApiClient.ShouldNotBeNull();

        ApiResult<SingleResult<UserDataDetailed>> result = await PingenApiClient!.Users.Get();

        AssertSuccess(result);
        result.Data!.Data.Id.ShouldNotBeNullOrEmpty();
        result.Data.Data.Attributes.Email.ShouldNotBeNullOrEmpty();
    }
}
