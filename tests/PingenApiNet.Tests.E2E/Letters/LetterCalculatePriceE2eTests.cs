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
using PingenApiNet.Abstractions.Enums.Letters;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.LetterPrices;

namespace PingenApiNet.Tests.E2E.Letters;

/// <summary>
///     End-to-end tests for <c>Letters.CalculatePrice</c>. Calls the price-calculator endpoint
///     with two distinct delivery-product configurations (the Pingen-internal <c>cheap</c>
///     bucket and the Swiss Post B-mail product) and verifies that each call returns a
///     populated price, a non-null currency, and that the two products yield different prices —
///     proving the calculator actually differentiates by product instead of returning a stub.
/// </summary>
[TestFixture]
[Category("E2e")]
public sealed class LetterCalculatePriceE2eTests : E2eTestBase
{
    /// <summary>
    ///     Calls <c>Letters.CalculatePrice</c> with the <see cref="LetterCreateDeliveryProduct.Cheap" />
    ///     internal delivery product against a Swiss recipient and asserts that the response is
    ///     successful, the price is positive, and the currency is populated.
    /// </summary>
    [Test]
    public async Task CalculatePrice_WithCheapConfig_ShouldReturnPositivePrice()
    {
        PingenApiClient.ShouldNotBeNull();

        ApiResult<SingleResult<LetterPriceData>> result =
            await PingenApiClient!.Letters.CalculatePrice(BuildPricePost(LetterCreateDeliveryProduct.Cheap));

        AssertSuccess(result);
        LetterPrice price = result.Data!.Data.Attributes;
        price.Price.ShouldNotBeNull();
        price.Price!.Value.ShouldBeGreaterThan(0m);
        price.Currency.ShouldNotBeNull();
    }

    /// <summary>
    ///     Calls <c>Letters.CalculatePrice</c> with the Swiss Post B-mail
    ///     (<see cref="LetterSendDeliveryProduct.PostAgB" />) delivery product and asserts that
    ///     the response is successful, the price is positive, and the currency is populated.
    /// </summary>
    [Test]
    public async Task CalculatePrice_WithPostAgBConfig_ShouldReturnPositivePrice()
    {
        PingenApiClient.ShouldNotBeNull();

        ApiResult<SingleResult<LetterPriceData>> result =
            await PingenApiClient!.Letters.CalculatePrice(BuildPricePost(LetterSendDeliveryProduct.PostAgB));

        AssertSuccess(result);
        LetterPrice price = result.Data!.Data.Attributes;
        price.Price.ShouldNotBeNull();
        price.Price!.Value.ShouldBeGreaterThan(0m);
        price.Currency.ShouldNotBeNull();
    }

    /// <summary>
    ///     Compares the prices returned for the <see cref="LetterCreateDeliveryProduct.Cheap" />
    ///     and <see cref="LetterSendDeliveryProduct.PostAgB" /> configurations and asserts that
    ///     they differ. A matching price would indicate the calculator is not actually
    ///     respecting the <c>delivery_product</c> attribute.
    /// </summary>
    [Test]
    public async Task CalculatePrice_ForDifferentDeliveryProducts_ShouldReturnDifferentPrices()
    {
        PingenApiClient.ShouldNotBeNull();

        ApiResult<SingleResult<LetterPriceData>> cheap =
            await PingenApiClient!.Letters.CalculatePrice(BuildPricePost(LetterCreateDeliveryProduct.Cheap));
        ApiResult<SingleResult<LetterPriceData>> postAgB =
            await PingenApiClient.Letters.CalculatePrice(BuildPricePost(LetterSendDeliveryProduct.PostAgB));

        AssertSuccess(cheap);
        AssertSuccess(postAgB);

        decimal? cheapPrice = cheap.Data!.Data.Attributes.Price;
        decimal? postAgBPrice = postAgB.Data!.Data.Attributes.Price;

        cheapPrice.ShouldNotBeNull();
        postAgBPrice.ShouldNotBeNull();
        cheapPrice.ShouldNotBe(postAgBPrice);
    }

    private static DataPost<LetterPriceConfiguration> BuildPricePost(string deliveryProduct) => new()
    {
        Type = PingenApiDataType.letters,
        Attributes = new LetterPriceConfiguration
        {
            Country = "CH",
            PaperTypes = [LetterPaperTypes.Normal],
            PrintMode = LetterPrintMode.simplex,
            PrintSpectrum = LetterPrintSpectrum.grayscale,
            DeliveryProduct = deliveryProduct
        }
    };
}
