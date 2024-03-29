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

using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Helpers;

namespace PingenApiNet.Abstractions.Models.Api;

/// <summary>
/// Extends the <see cref="ApiRequest"/> with sorting, filtering, searching and paging for collection endpoints
/// </summary>
public sealed record ApiPagingRequest : ApiRequest
{
    /// <summary>
    /// Enumerable of sort instructions with property name and sort direction, where the order is relevant.
    /// <see href="https://api.pingen.com/documentation#section/Advanced/Sorting-collections">API Doc - Sorting</see>
    /// </summary>
    public IEnumerable<KeyValuePair<string, CollectionSortDirection>>? Sorting { get; init; }

    /// <summary>
    /// Possibly nested filtering instruction with operator name (<see cref="CollectionFilterOperator"/>) and collection of conditions (Array of KeyValuePair{string, object}) or key / value filter comparator (KeyValuePair{string, string}).
    /// <br/>Use <see cref="PingenAttributesPropertyHelper{T}"/>.<see cref="PingenAttributesPropertyHelper{T}.GetJsonPropertyName{TValue}"/> for correct key names, where T is the type of the attributes object in response data
    /// <see href="https://api.pingen.com/documentation#section/Advanced/Filtering-collections">API Doc - Filtering</see>
    /// </summary>
    public KeyValuePair<string, object>? Filtering { get; init; }

    /// <summary>
    /// Plain blind searches can be done by passing the string to be searched in the parameter q.
    /// <see href="https://api.pingen.com/documentation#section/Advanced/Searching-collections">API Doc - Searching</see>
    /// </summary>
    public string? Searching { get; init; }

    /// <summary>
    /// Every collection endpoint accepts the two parameters page[number] and page[limit].
    /// The default limit 20, the maximum is 100 objects per page. Using them together enables you to do simple paging.
    /// In every collection response you will furthermore receive the full URLs of the previous, next and last page (if available).
    /// <see href="https://api.pingen.com/documentation#section/Advanced/Paginating-collections">API Doc - Paginating</see>
    /// </summary>
    public int? PageNumber { get; init; }

    /// <summary>
    /// Every collection endpoint accepts the two parameters page[number] and page[limit].
    /// The default limit 20, the maximum is 100 objects per page. Using them together enables you to do simple paging.
    /// In every collection response you will furthermore receive the full URLs of the previous, next and last page (if available).
    /// <see href="https://api.pingen.com/documentation#section/Advanced/Paginating-collections">API Doc - Paginating</see>
    /// </summary>
    public int? PageLimit { get; init; }
}
