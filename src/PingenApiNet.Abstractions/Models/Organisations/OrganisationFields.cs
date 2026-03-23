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

namespace PingenApiNet.Abstractions.Models.Organisations;

/// <summary>
/// Available sparse fieldset field names for the Organisation resource.
/// Pass one or more of these constants as field values in <see cref="Api.ApiRequest.SparseFieldsets"/>
/// to request only specific attributes in the response.
/// <see href="https://api.pingen.com/documentation#section/Advanced/Sparse-fieldsets">API Doc - Sparse fieldsets</see>
/// </summary>
public static class OrganisationFields
{
    /// <summary>Organisation name</summary>
    public const string Name = "name";

    /// <summary>Organisation status</summary>
    public const string Status = "status";

    /// <summary>Subscription plan name</summary>
    public const string Plan = "plan";

    /// <summary>Billing mode (prepaid or postpaid)</summary>
    public const string BillingMode = "billing_mode";

    /// <summary>Billing currency code</summary>
    public const string BillingCurrency = "billing_currency";

    /// <summary>Current billing balance</summary>
    public const string BillingBalance = "billing_balance";

    /// <summary>Default country code for letters</summary>
    public const string DefaultCountry = "default_country";

    /// <summary>Default address window position</summary>
    public const string DefaultAddressPosition = "default_address_position";

    /// <summary>Address data retention period in days</summary>
    public const string DataRetentionAddresses = "data_retention_addresses";

    /// <summary>PDF data retention period in days</summary>
    public const string DataRetentionPdf = "data_retention_pdf";

    /// <summary>Organisation brand colour</summary>
    public const string Color = "color";

    /// <summary>Timestamp when the organisation was created</summary>
    public const string CreatedAt = "created_at";

    /// <summary>Timestamp when the organisation was last updated</summary>
    public const string UpdatedAt = "updated_at";
}
