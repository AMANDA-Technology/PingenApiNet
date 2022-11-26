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

using PingenApiNet.Abstractions.Interfaces.Data;

namespace PingenApiNet.Abstractions.Models.API;

/// <summary>
/// An API result with meta information received from the Pingen API
/// </summary>
public record ApiResult
{
    /// <summary>
    /// Indicates if the API call was successfully or not
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// UUID. Can be used for support inquiries.
    /// <see href="https://api.v2.pingen.com/documentation#section/Basics/HTTP-methods-and-headers">API Doc - HTTP</see>
    /// </summary>
    public Guid RequestId { get; init; }

    /// <summary>
    /// The number of allowed requests in the current period.
    /// <see href="https://api.v2.pingen.com/documentation#section/Basics/HTTP-methods-and-headers">API Doc - HTTP</see>
    /// </summary>
    public int RateLimitLimit { get; init; }

    /// <summary>
    /// The number of remaining requests in the current period.
    /// <see href="https://api.v2.pingen.com/documentation#section/Basics/HTTP-methods-and-headers">API Doc - HTTP</see>
    /// </summary>
    public int RateLimitRemaining { get; init; }

    /// <summary>
    /// The timestamp when the current period expires and the request can be retried.
    /// <see href="https://api.v2.pingen.com/documentation#section/Basics/Throttling-Rate-limiting">API Doc - Throttling</see>
    /// </summary>
    public DateTime? RateLimitReset { get; init; }

    /// <summary>
    /// The number of seconds you have to wait until you can retry (and the current period expires).
    /// <see href="https://api.v2.pingen.com/documentation#section/Basics/Throttling-Rate-limiting">API Doc - Throttling</see>
    /// </summary>
    public int? RetryAfter { get; init; }

    /// <summary>
    /// You can determine if an idempotent API call was replayed by inspecting the Idempotent-Replayed header for the value true.
    /// <see href="https://api.v2.pingen.com/documentation#section/Advanced/Idempotency">API Doc - Idempotency</see>
    /// </summary>
    public bool IdempotentReplayed { get; init; }

    /// <summary>
    /// API error, set when failed. <see cref="Data"/> might be empty in that case.
    /// </summary>
    public ApiError? ApiError { get; init; }

    /// <summary>
    /// Location Url, for result 302 Found
    /// </summary>
    public Uri? Location { get; init; }
}

/// <summary>
/// An API result with meta information and data received from the Pingen API
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed record ApiResult<T> : ApiResult where T : IDataResult
{
    /// <summary>
    /// Data received from the API. Check <see cref="ApiResult.IsSuccess"/> and <see cref="ApiResult.ApiError"/>, especially when Data is null.
    /// </summary>
    public T? Data { get; init; }
}
