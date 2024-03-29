﻿/*
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
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults.Embedded;

namespace PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;

/// <summary>
/// Generic response of collection endpoint
/// </summary>
/// <param name="Data">Collection of data objects requested from endpoint</param>
/// <param name="Links">Collection result links</param>
/// <param name="Meta">Collection result meta information</param>
/// <typeparam name="TData"></typeparam>
public sealed record CollectionResult<TData>(
    [property: JsonPropertyName("data")] IList<TData> Data,
    [property: JsonPropertyName("links")] CollectionResultLinks Links,
    [property: JsonPropertyName("meta")] CollectionResultMeta Meta
) : IDataResult<IList<TData>> where TData : IData
{
    /// <summary>
    /// Additionally requested includes
    /// </summary>
    [JsonPropertyName("included")]
    public IList<object>? Included { get; init; } // TODO: Implement type for Included?
}
