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

namespace PingenApiNet.Abstractions.Models.Organisations;

/// <summary>
/// Organisation
/// </summary>
/// <param name="Name"></param>
/// <param name="Status"></param>
/// <param name="Plan"></param>
/// <param name="BillingMode"></param>
/// <param name="BillingCurrency"></param>
/// <param name="BillingBalance"></param>
/// <param name="DefaultCountry"></param>
/// <param name="DefaultAddressPosition"></param>
/// <param name="DataRetentionAddresses"></param>
/// <param name="DataRetentionPdf"></param>
/// <param name="Color"></param>
/// <param name="CreatedAt"></param>
/// <param name="UpdatedAt"></param>
public sealed record Organisation(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("plan")] string Plan,
    [property: JsonPropertyName("billing_mode")] string BillingMode,
    [property: JsonPropertyName("billing_currency")] string BillingCurrency,
    [property: JsonPropertyName("billing_balance")] double? BillingBalance,
    [property: JsonPropertyName("default_country")] string DefaultCountry,
    [property: JsonPropertyName("default_address_position")] string DefaultAddressPosition,
    [property: JsonPropertyName("data_retention_addresses")] int? DataRetentionAddresses,
    [property: JsonPropertyName("data_retention_pdf")] int? DataRetentionPdf,
    [property: JsonPropertyName("color")] string Color,
    [property: JsonPropertyName("created_at")] DateTime? CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTime? UpdatedAt
 );
