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
using PingenApiNet.Abstractions.Enums.Batches;
using PingenApiNet.Abstractions.Enums.Letters;
using PingenApiNet.Abstractions.Interfaces.Data;
using PingenApiNet.Abstractions.Models.Api.Embedded;

namespace PingenApiNet.Abstractions.Models.Batches.Views;

/// <summary>
/// Batch create object to send via <see cref="DataPost{TAttributes}"/> to the API
/// </summary>
public sealed record BatchCreate : IAttributes
{
    /// <summary>
    /// Name of the batch [ 5 .. 100 ] characters
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Icon
    /// </summary>
    [JsonPropertyName("icon")]
    public required BatchIcon Icon { get; init; }

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
    /// Grouping type
    /// </summary>
    [JsonPropertyName("grouping_type")]
    public required BatchGroupingType GroupingType { get; init; }

    /// <summary>
    /// Grouping options type
    /// </summary>
    [JsonPropertyName("grouping_options_split_type")]
    public required BatchGroupingOptionsSplitType GroupingOptionsSplitType { get; init; }

    /// <summary>
    /// Grouping options split size [ 1 .. 10 ]
    /// </summary>
    [JsonPropertyName("grouping_options_split_size")]
    public int? GroupingOptionsSplitSize { get; init; }

    /// <summary>
    /// Grouping options split separator [ 1 .. 20 ] characters
    /// </summary>
    [JsonPropertyName("grouping_options_split_separator")]
    public string? GroupingOptionsSplitSeparator { get; init; }

    /// <summary>
    /// Grouping options split position
    /// </summary>
    [JsonPropertyName("grouping_options_split_position")]
    public BatchGroupingOptionsSplitPosition? GroupingOptionsSplitPosition { get; init; }
}
