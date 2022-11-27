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
/// API header names for requests or from responses
/// </summary>
// TODO: use another namespaces for static values?
public static class ApiHeaderNames
{
    #region HeadersToSend

    /// <summary>
    /// Header name for Idempotency key
    /// </summary>
    public const string IdempotencyKey = "Idempotency-Key";

    #endregion HeadersToSend

    #region HeadersToReceive

    /// <summary>
    /// Header name for request ID
    /// </summary>
    public const string RequestId = "X-Request-ID"; // API-DOC: X-Request-ID

    /// <summary>
    /// Header name for rate limit limit
    /// </summary>
    public const string RateLimitLimit = "x-ratelimit-limit"; // API-DOC: X-Rate-Limit-Limit

    /// <summary>
    /// Header name for rate limit remaining
    /// </summary>
    public const string RateLimitRemaining = "x-ratelimit-remaining"; // API-DOC: X-Rate-Limit-Remaining

    /// <summary>
    /// Header name for rate limit reset
    /// </summary>
    public const string RateLimitReset = "x-ratelimit-reset"; // API-DOC: X-Rate-Limit-Reset

    /// <summary>
    /// Header name for retry after
    /// </summary>
    public const string RetryAfter = "Retry-After";

    /// <summary>
    /// Header name for idempotent replayed
    /// </summary>
    public const string IdempotentReplayed = "Idempotent-Replayed";

    /// <summary>
    /// Header name for location
    /// </summary>
    public const string Location = "Location";

    #endregion HeadersToReceive
}
