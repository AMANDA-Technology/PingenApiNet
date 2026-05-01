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
using PingenApiNet.Abstractions.Exceptions;
using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.DeliveryProducts;

namespace PingenApiNet.Tests.E2E.Tests;

/// <summary>
///     End-to-end tests for the Pingen distributions endpoint. Exercises the delivery-products
///     listing — both single-page retrieval and the auto-paginated <c>IAsyncEnumerable</c> path —
///     against the live staging API. Sorting and filtering parameters are sent and accepted by the
///     server, but the upstream endpoint is documented to ignore them, so this fixture verifies
///     the request shape and pagination contract rather than filter semantics.
/// </summary>
[TestFixture]
[Category("E2e")]
public sealed class DistributionGetDeliveryProducts : E2eTestBase
{
    /// <summary>
    ///     Verifies that delivery products can be retrieved both via a single page request and via
    ///     the auto-paginated <c>IAsyncEnumerable</c> helper. A non-empty product set across all
    ///     pages confirms the server returned valid data and that auto-pagination terminates.
    /// </summary>
    [Test]
    public async Task GetProducts()
    {
        // The distributions/delivery-products endpoint accepts sorting and filtering parameters but
        // does not appear to honour them. The request below verifies serialization is accepted by
        // the API even though the server-side filter is a no-op.
        var apiPagingRequest = new ApiPagingRequest
        {
            Sorting = new Dictionary<string, CollectionSortDirection>
            {
                [PingenAttributesPropertyHelper<DeliveryProduct>.GetJsonPropertyName(product => product.PriceStartingFrom)] =
                    CollectionSortDirection.DESC
            },
            Filtering = new KeyValuePair<string, object>(
                CollectionFilterOperator.And,
                new KeyValuePair<string, object>[]
                {
                    new(
                        PingenAttributesPropertyHelper<DeliveryProduct>.GetJsonPropertyName(product =>
                            product.PriceCurrency), "CHF"),
                    new(
                        PingenAttributesPropertyHelper<DeliveryProduct>.GetJsonPropertyName(product =>
                            product.PriceStartingFrom), "<=1")
                })
        };

        PingenApiClient.ShouldNotBeNull();

        ApiResult<CollectionResult<DeliveryProductData>> res =
            await PingenApiClient!.Distributions.GetDeliveryProductsPage(apiPagingRequest);
        AssertSuccess(res);

        var deliveryProducts = new List<DeliveryProductData>();

        ApiError? error = null;
        try
        {
            await foreach (IEnumerable<DeliveryProductData> page in
                           PingenApiClient.Distributions.GetDeliveryProductsPageResultsAsync(apiPagingRequest))
                deliveryProducts.AddRange(page);
        }
        catch (PingenApiErrorException e)
        {
            error = e.ApiResult?.ApiError;
        }

        deliveryProducts.ShouldSatisfyAllConditions(
            () => deliveryProducts.ShouldNotBeEmpty(),
            () => error.ShouldBeNull()
        );
    }
}
