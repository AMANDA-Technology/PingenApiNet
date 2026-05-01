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
using PingenApiNet.Abstractions.Models.Organisations;

namespace PingenApiNet.Tests.E2E.Organisations;

/// <summary>
///     End-to-end tests for the Pingen organisations endpoint. Exercises a paginated list
///     and a single-resource lookup against the live staging API.
/// </summary>
[TestFixture]
[Category("E2e")]
public sealed class OrganisationE2eTests : E2eTestBase
{
    /// <summary>
    ///     Verifies that a paginated list of organisations can be retrieved and exposes pagination meta.
    /// </summary>
    [Test]
    public async Task GetPage_ShouldReturnPaginatedOrganisations()
    {
        PingenApiClient.ShouldNotBeNull();

        ApiResult<CollectionResult<OrganisationData>> result =
            await PingenApiClient!.Organisations.GetPage(new ApiPagingRequest { PageNumber = 1, PageLimit = 20 });

        AssertSuccess(result);
        result.Data!.Data.ShouldNotBeNull();
        result.Data.Meta.CurrentPage.ShouldNotBeNull();
        result.Data.Meta.CurrentPage!.Value.ShouldBeGreaterThanOrEqualTo(1);
        result.Data.Meta.LastPage.ShouldNotBeNull();
        result.Data.Meta.LastPage!.Value.ShouldBeGreaterThanOrEqualTo(1);
    }

    /// <summary>
    ///     Verifies that the configured default organisation can be fetched by id.
    /// </summary>
    [Test]
    public async Task Get_ShouldReturnOrganisationById()
    {
        PingenApiClient.ShouldNotBeNull();

        string organisationId =
            Environment.GetEnvironmentVariable("PingenApiNet__OrganisationId") ??
            throw new InvalidOperationException("Missing PingenApiNet__OrganisationId");

        ApiResult<SingleResult<OrganisationDataDetailed>> result =
            await PingenApiClient!.Organisations.Get(organisationId);

        AssertSuccess(result);
        result.Data!.Data.Id.ShouldBe(organisationId);
        result.Data.Data.Attributes.Name.ShouldNotBeNullOrEmpty();
    }
}
