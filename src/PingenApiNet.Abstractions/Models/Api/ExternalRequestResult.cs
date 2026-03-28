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

using System.Net;

namespace PingenApiNet.Abstractions.Models.Api;

/// <summary>
/// Result of an external HTTP request (e.g., S3 file upload). Unlike <see cref="ApiResult"/>,
/// this does not carry Pingen API-specific metadata such as request IDs or rate limit headers.
/// </summary>
public sealed record ExternalRequestResult
{
    /// <summary>
    /// Indicates whether the HTTP request completed successfully (2xx status code)
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// The HTTP status code returned by the external server
    /// </summary>
    public HttpStatusCode StatusCode { get; init; }

    /// <summary>
    /// The reason phrase returned by the external server (e.g., "OK", "Forbidden", "Not Found").
    /// May be null if the server did not include a reason phrase.
    /// </summary>
    public string? ReasonPhrase { get; init; }
}
