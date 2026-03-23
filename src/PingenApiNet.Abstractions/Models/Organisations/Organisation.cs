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
using PingenApiNet.Abstractions.Interfaces.Data;

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
    [property: JsonPropertyName(OrganisationFields.Name)] string? Name,
    [property: JsonPropertyName(OrganisationFields.Status)] string? Status,
    [property: JsonPropertyName(OrganisationFields.Plan)] string? Plan,
    [property: JsonPropertyName(OrganisationFields.BillingMode)] string? BillingMode,
    [property: JsonPropertyName(OrganisationFields.BillingCurrency)] string? BillingCurrency,
    [property: JsonPropertyName(OrganisationFields.BillingBalance)] double? BillingBalance,
    [property: JsonPropertyName(OrganisationFields.DefaultCountry)] string? DefaultCountry,
    [property: JsonPropertyName(OrganisationFields.DefaultAddressPosition)] string? DefaultAddressPosition,
    [property: JsonPropertyName(OrganisationFields.DataRetentionAddresses)] int? DataRetentionAddresses,
    [property: JsonPropertyName(OrganisationFields.DataRetentionPdf)] int? DataRetentionPdf,
    [property: JsonPropertyName(OrganisationFields.Color)] string? Color,
    [property: JsonPropertyName(OrganisationFields.CreatedAt)] DateTime? CreatedAt,
    [property: JsonPropertyName(OrganisationFields.UpdatedAt)] DateTime? UpdatedAt
 ) : IAttributes;
