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

namespace PingenApiNet.Abstractions.Enums.Api;

/// <summary>
/// API query parameters for requests
/// </summary>
// TODO: use another namespaces for static values?
public static class ApiQueryParameterNames
{
    /// <summary>
    /// Query parameter for Sorting
    /// </summary>
    public const string Sorting = "sort";

    /// <summary>
    /// Query parameter for Filtering
    /// </summary>
    public const string Filtering = "filter";

    /// <summary>
    /// Query parameter for Searching
    /// </summary>
    public const string Searching = "q";

    /// <summary>
    /// Query parameter for page number
    /// </summary>
    public const string PageNumber = "page[number]";

    /// <summary>
    /// Query parameter for page limit
    /// </summary>
    public const string PageLimit = "page[limit]";

    /// <summary>
    /// Query parameter for sparse fieldsets
    /// </summary>
    public static string SparseFields(PingenApiDataType type) => $"fields[{Enum.GetName(type)}]";

    /// <summary>
    /// Query parameter for Include
    /// </summary>
    public const string Include = "include";

    /// <summary>
    /// Query parameter for language
    /// </summary>
    public const string Language = "language";
}
