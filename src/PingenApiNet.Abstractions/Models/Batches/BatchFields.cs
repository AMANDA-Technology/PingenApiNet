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

namespace PingenApiNet.Abstractions.Models.Batches;

/// <summary>
/// Available sparse fieldset field names for the Batch resource.
/// Pass one or more of these constants as field values in <see cref="Api.ApiRequest.SparseFieldsets"/>
/// to request only specific attributes in the response.
/// <see href="https://api.pingen.com/documentation#section/Advanced/Sparse-fieldsets">API Doc - Sparse fieldsets</see>
/// </summary>
public static class BatchFields
{
    /// <summary>Batch name</summary>
    public const string Name = "name";

    /// <summary>Batch icon identifier</summary>
    public const string Icon = "icon";

    /// <summary>Batch status</summary>
    public const string Status = "status";

    /// <summary>Original file name of the batch PDF</summary>
    public const string FileOriginalName = "file_original_name";

    /// <summary>Number of letters in this batch</summary>
    public const string LetterCount = "letter_count";

    /// <summary>Address position for letters in this batch</summary>
    public const string AddressPosition = "address_position";

    /// <summary>Print mode for letters in this batch</summary>
    public const string PrintMode = "print_mode";

    /// <summary>Print spectrum for letters in this batch</summary>
    public const string PrintSpectrum = "print_spectrum";

    /// <summary>Currency code for the batch price</summary>
    public const string PriceCurrency = "price_currency";

    /// <summary>Total price value for the batch</summary>
    public const string PriceValue = "price_value";

    /// <summary>Timestamp when the batch was submitted</summary>
    public const string SubmittedAt = "submitted_at";

    /// <summary>Timestamp when the batch was created</summary>
    public const string CreatedAt = "created_at";

    /// <summary>Timestamp when the batch was last updated</summary>
    public const string UpdatedAt = "updated_at";
}
