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
/// <param name="DeliveryProduct">Should be any of <see cref="LetterSendDeliveryProduct"/></param>
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
    [property: JsonPropertyName(LetterFields.Status)] string? Status,
    [property: JsonPropertyName(LetterFields.FileOriginalName)] string? FileOriginalName,
    [property: JsonPropertyName(LetterFields.FilePages)] int? FilePages,
    [property: JsonPropertyName(LetterFields.Address)] string? Address,
    [property: JsonPropertyName(LetterFields.AddressPosition)] LetterAddressPosition? AddressPosition,
    [property: JsonPropertyName(LetterFields.Country)] string? Country,
    [property: JsonPropertyName(LetterFields.DeliveryProduct)] string? DeliveryProduct,
    [property: JsonPropertyName(LetterFields.PrintMode)] LetterPrintMode? PrintMode,
    [property: JsonPropertyName(LetterFields.PrintSpectrum)] LetterPrintSpectrum? PrintSpectrum,
    [property: JsonPropertyName(LetterFields.PriceCurrency)] string? PriceCurrency,
    [property: JsonPropertyName(LetterFields.PriceValue)] double? PriceValue,
    [property: JsonPropertyName(LetterFields.PaperTypes)] IReadOnlyList<string>? PaperTypes,
    [property: JsonPropertyName(LetterFields.Fonts)] IReadOnlyList<LetterFont>? Fonts,
    [property: JsonPropertyName(LetterFields.TrackingNumber)] string? TrackingNumber,
    [property: JsonPropertyName(LetterFields.SubmittedAt)] DateTime? SubmittedAt,
    [property: JsonPropertyName(LetterFields.CreatedAt)] DateTime? CreatedAt,
    [property: JsonPropertyName(LetterFields.UpdatedAt)] DateTime? UpdatedAt
) : IAttributes;
