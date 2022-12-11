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

using System.Text.Json.Serialization;
using PingenApiNet.Abstractions.Enums.Letters;
using PingenApiNet.Abstractions.Interfaces.Data;
using PingenApiNet.Abstractions.Models.Letters.Embedded;

namespace PingenApiNet.Abstractions.Models.Letters;

/// <summary>
/// Letter
/// </summary>
/// <param name="Status">Probably any of <see cref="LetterStates"/>, but there is nothing about it in API Doc...</param>
/// <param name="FileOriginalName"></param>
/// <param name="FilePages"></param>
/// <param name="Address"></param>
/// <param name="AddressPosition"></param>
/// <param name="Country"></param>
/// <param name="DeliveryProduct"></param>
/// <param name="PrintMode"></param>
/// <param name="PrintSpectrum"></param>
/// <param name="PriceCurrency"></param>
/// <param name="PriceValue"></param>
/// <param name="PaperTypes"></param>
/// <param name="Fonts"></param>
/// <param name="TrackingNumber"></param>
/// <param name="SubmittedAt"></param>
/// <param name="CreatedAt"></param>
/// <param name="UpdatedAt"></param>
public sealed record Letter(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("file_original_name")] string FileOriginalName,
    [property: JsonPropertyName("file_pages")] int? FilePages,
    [property: JsonPropertyName("address")] string Address,
    [property: JsonPropertyName("address_position")] LetterAddressPosition AddressPosition,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("delivery_product")] LetterDeliveryProduct DeliveryProduct,
    [property: JsonPropertyName("print_mode")] LetterPrintMode PrintMode,
    [property: JsonPropertyName("print_spectrum")] LetterPrintSpectrum PrintSpectrum,
    [property: JsonPropertyName("price_currency")] string PriceCurrency,
    [property: JsonPropertyName("price_value")] double? PriceValue,
    [property: JsonPropertyName("paper_types")] IReadOnlyList<string> PaperTypes,
    [property: JsonPropertyName("fonts")] IReadOnlyList<LetterFont> Fonts,
    [property: JsonPropertyName("tracking_number")] string TrackingNumber,
    [property: JsonPropertyName("submitted_at")] DateTime? SubmittedAt,
    [property: JsonPropertyName("created_at")] DateTime? CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTime? UpdatedAt
) : IAttributes;
