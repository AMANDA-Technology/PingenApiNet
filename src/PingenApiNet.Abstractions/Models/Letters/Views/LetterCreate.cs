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
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Letters.Embedded;

namespace PingenApiNet.Abstractions.Models.Letters.Views;

/// <summary>
/// Letter create object to send via <see cref="DataPost{TAttributes}"/> to the API
/// </summary>
public sealed record LetterCreate
{
    /// <summary>
    /// Filename [ 5 .. 255 ] characters
    /// </summary>
    [JsonPropertyName("file_original_name")]
    public required string FileOriginalName { get; init; }

    /// <summary>
    /// File URL [ 1 .. 1000 ] characters
    /// </summary>
    [JsonPropertyName("file_url")]
    public required string FileUrl { get; init; }

    /// <summary>
    /// File URL signature [ 1 .. 60 ] characters
    /// </summary>
    [JsonPropertyName("file_url_signature")]
    public required string FileUrlSignature { get; init; }

    /// <summary>
    /// Address position
    /// </summary>
    [JsonPropertyName("address_position")]
    public required LetterAddressPosition AddressPosition { get; init; }

    /// <summary>
    /// Auto send
    /// </summary>
    [JsonPropertyName("auto_send")]
    public required bool AutoSend { get; init; }

    /// <summary>
    /// Delivery product
    /// </summary>
    [JsonPropertyName("delivery_product")]
    public required LetterDeliveryProduct DeliveryProduct { get; init; }

    /// <summary>
    /// Print mode
    /// </summary>
    [JsonPropertyName("print_mode")]
    public required LetterPrintMode PrintMode { get; init; }

    /// <summary>
    /// Print spectrum
    /// </summary>
    [JsonPropertyName("print_spectrum")]
    public required LetterPrintSpectrum PrintSpectrum { get; init; }

    /// <summary>
    /// Meta data
    /// </summary>
    [JsonPropertyName("meta_data")]
    public required LetterMetaData MetaData { get; init; }
}
