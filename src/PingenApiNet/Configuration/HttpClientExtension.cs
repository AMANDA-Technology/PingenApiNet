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

namespace PingenApiNet.Configuration;

/// <summary>
/// Extensions to configure the <see cref="HttpClient"/> instances used by this library.
/// </summary>
public static class HttpClientExtension
{
    /// <summary>
    /// Header names owned by this library, which are therefore never taken from the caller supplied headers.
    /// The header collections append on a duplicate name instead of throwing, so an unguarded caller entry
    /// would transmit two values and break authentication, content negotiation or idempotency.
    /// </summary>
    private static readonly HashSet<string> ReservedHeaderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Accept",
        "Host",
        "Idempotency-Key"
    };

    /// <summary>
    /// Valid header name token characters in addition to ALPHA and DIGIT, as defined by RFC 9110.
    /// </summary>
    private const string AdditionalHeaderNameTokenCharacters = "!#$%&'*+-.^_`|~";

    /// <summary>
    /// Applies additional static default request headers to the given client, e.g. to identify the calling
    /// application at the Pingen API. Intended to be applied once per <see cref="HttpClient"/>, on
    /// <see cref="HttpClient.DefaultRequestHeaders"/>, with static values only. Per request values must be
    /// set on the <see cref="HttpRequestMessage"/> instead.
    /// <br/><br/>
    /// Semantics:
    /// <list type="bullet">
    /// <item>Applied after the headers this library configures itself. Each header is removed before it is
    /// added, so applying the same headers twice replaces instead of appends.</item>
    /// <item>The reserved header names <c>Authorization</c>, <c>Accept</c>, <c>Host</c> and
    /// <c>Idempotency-Key</c> are silently skipped, case insensitive. The header collections append on a
    /// duplicate name instead of throwing, so an unguarded caller entry would transmit two values and break
    /// authentication or idempotency.</item>
    /// <item>Never throws. A <c>null</c> or empty dictionary is a no-op. A blank name or value, a name
    /// containing an invalid header name token character, or a value containing a control character
    /// (below 0x20 or 0x7F, notably CR/LF) is skipped. This is not cosmetic:
    /// <c>TryAddWithoutValidation</c> accepts a value containing CR/LF and the underlying handler then
    /// throws when the request is sent.</item>
    /// </list>
    /// </summary>
    /// <param name="client">The client to apply the headers to.</param>
    /// <param name="headers">Optional, additional static default request headers to apply.</param>
    /// <returns>The same <paramref name="client"/>, to allow chaining.</returns>
    public static HttpClient ApplyDefaultRequestHeaders(this HttpClient client, IReadOnlyDictionary<string, string>? headers)
    {
        if (headers is null || headers.Count == 0)
            return client;

        foreach (var (name, value) in headers)
        {
            if (!IsApplicableHeaderName(name) || !IsApplicableHeaderValue(value))
                continue;

            client.DefaultRequestHeaders.Remove(name);
            client.DefaultRequestHeaders.TryAddWithoutValidation(name, value);
        }

        return client;
    }

    /// <summary>
    /// Checks whether the given header name is neither blank nor reserved and consists of valid token characters only.
    /// </summary>
    /// <param name="name">The header name to check.</param>
    /// <returns>True if the header name may be applied.</returns>
    private static bool IsApplicableHeaderName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name)
               && !ReservedHeaderNames.Contains(name)
               && name.All(character => char.IsAsciiLetterOrDigit(character) || AdditionalHeaderNameTokenCharacters.Contains(character));
    }

    /// <summary>
    /// Checks whether the given header value is neither blank nor contains a control character.
    /// </summary>
    /// <param name="value">The header value to check.</param>
    /// <returns>True if the header value may be applied.</returns>
    private static bool IsApplicableHeaderValue(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
               && !value.Any(character => character < ' ' || character == (char)0x7F);
    }
}
