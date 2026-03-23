/*
MIT License

Copyright (c) 2024 Dejan Appenzeller <dejan.appenzeller@swisspeers.ch>

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

namespace PingenApiNet.Abstractions.Models.Batches;

/// <summary>
/// Batch
/// </summary>
/// <param name="Name"></param>
/// <param name="Icon"></param>
/// <param name="Status"></param>
/// <param name="FileOriginalName"></param>
/// <param name="LetterCount"></param>
/// <param name="AddressPosition"></param>
/// <param name="PrintMode"></param>
/// <param name="PrintSpectrum"></param>
/// <param name="PriceCurrency"></param>
/// <param name="PriceValue"></param>
/// <param name="SubmittedAt"></param>
/// <param name="CreatedAt"></param>
/// <param name="UpdatedAt"></param>
public sealed record Batch(
    [property: JsonPropertyName(BatchFields.Name)] string? Name,
    [property: JsonPropertyName(BatchFields.Icon)] string? Icon,
    [property: JsonPropertyName(BatchFields.Status)] string? Status,
    [property: JsonPropertyName(BatchFields.FileOriginalName)] string? FileOriginalName,
    [property: JsonPropertyName(BatchFields.LetterCount)] int? LetterCount,
    [property: JsonPropertyName(BatchFields.AddressPosition)] LetterAddressPosition? AddressPosition,
    [property: JsonPropertyName(BatchFields.PrintMode)] LetterPrintMode? PrintMode,
    [property: JsonPropertyName(BatchFields.PrintSpectrum)] LetterPrintSpectrum? PrintSpectrum,
    [property: JsonPropertyName(BatchFields.PriceCurrency)] string? PriceCurrency,
    [property: JsonPropertyName(BatchFields.PriceValue)] double? PriceValue,
    [property: JsonPropertyName(BatchFields.SubmittedAt)] DateTime? SubmittedAt,
    [property: JsonPropertyName(BatchFields.CreatedAt)] DateTime? CreatedAt,
    [property: JsonPropertyName(BatchFields.UpdatedAt)] DateTime? UpdatedAt
) : IAttributes;
