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

namespace PingenApiNet.Abstractions.Models.Letters;

/// <summary>
/// Available sparse fieldset field names for the Letter resource.
/// Pass one or more of these constants as field values in <see cref="Api.ApiRequest.SparseFieldsets"/>
/// to request only specific attributes in the response.
/// <see href="https://api.pingen.com/documentation#section/Advanced/Sparse-fieldsets">API Doc - Sparse fieldsets</see>
/// </summary>
public static class LetterFields
{
    /// <summary>Letter status (e.g. valid, sending, sent)</summary>
    public const string Status = "status";

    /// <summary>Original file name of the uploaded PDF</summary>
    public const string FileOriginalName = "file_original_name";

    /// <summary>Number of pages in the uploaded PDF</summary>
    public const string FilePages = "file_pages";

    /// <summary>Full address string extracted from the letter</summary>
    public const string Address = "address";

    /// <summary>Position of the address window on the letter</summary>
    public const string AddressPosition = "address_position";

    /// <summary>Destination country code (ISO 3166-1 alpha-2)</summary>
    public const string Country = "country";

    /// <summary>Delivery product identifier</summary>
    public const string DeliveryProduct = "delivery_product";

    /// <summary>Print mode (simplex or duplex)</summary>
    public const string PrintMode = "print_mode";

    /// <summary>Print spectrum (color or grayscale)</summary>
    public const string PrintSpectrum = "print_spectrum";

    /// <summary>Currency code for the price</summary>
    public const string PriceCurrency = "price_currency";

    /// <summary>Price value for sending this letter</summary>
    public const string PriceValue = "price_value";

    /// <summary>List of paper types used</summary>
    public const string PaperTypes = "paper_types";

    /// <summary>List of fonts detected in the letter</summary>
    public const string Fonts = "fonts";

    /// <summary>Tracking number assigned to the letter</summary>
    public const string TrackingNumber = "tracking_number";

    /// <summary>Timestamp when the letter was submitted for sending</summary>
    public const string SubmittedAt = "submitted_at";

    /// <summary>Timestamp when the letter was created</summary>
    public const string CreatedAt = "created_at";

    /// <summary>Timestamp when the letter was last updated</summary>
    public const string UpdatedAt = "updated_at";
}
