/* Copyright (C) AMANDA Technology - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Manuel Gysin <manuel.gysin@amanda-technology.ch>
 * Written by Philip Näf <philip.naef@amanda-technology.ch>
 */

namespace PingenApiNet.Services.Connectors.Endpoints;

/// <summary>
/// Endpoints (API request paths) for letters service.
/// <para>
/// Paths sit under <c>deliveries/</c>, the container Pingen introduced when it generalised "letter" into a
/// deliverable (letters, emails and ebills each get a sibling there). This is what the API documents, and what
/// its own <c>links.related</c> values point at. The short <c>letters/…</c> form the client used until
/// 2026-07-29 still resolves, but it is not in the spec and can be withdrawn without notice — the same
/// exposure that the <c>letter</c> webhook relationship represents. See
/// <c>doc/analysis/2026-05-01-api-docs-gap-audit.md</c> § Addendum (2026-07-29b).
/// </para>
/// <para>
/// Note <see cref="Issues"/> is not a plain prefix change: the documented route is
/// <c>deliveries/letters/events/issues</c>, not <c>deliveries/letters/issues</c>. It is one of four
/// event-category collections (<c>issues</c>, <c>sent</c>, <c>undeliverable</c>, <c>delivered</c>); only
/// <c>issues</c> is currently exposed by this client.
/// </para>
/// </summary>
internal static class LettersEndpoints
{
    /// <summary>
    /// Root path of letter deliveries
    /// </summary>
    internal const string Root = "deliveries/letters";

    /// <summary>
    /// Endpoint to calculate price
    /// </summary>
    internal const string PriceCalculator = $"{Root}/price-calculator";

    /// <summary>
    /// Endpoint to access a specific letter
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    internal static string Single(string id) => $"{Root}/{id}";

    /// <summary>
    /// Endpoint to send a specific letter
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    internal static string Send(string id) => $"{Single(id)}/send";

    /// <summary>
    /// Endpoint to cancel a specific letter
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    internal static string Cancel(string id) => $"{Single(id)}/cancel";

    /// <summary>
    /// Endpoint to get file of a specific letter
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    internal static string File(string id) => $"{Single(id)}/file";

    /// <summary>
    /// Endpoint to get events of a specific letter
    /// </summary>
    /// <param name="id"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    internal static string Events(string id, string language) => $"{Single(id)}/events?language={Uri.EscapeDataString(language)}";

    /// <summary>
    /// Endpoint to get issues across all letters. One of four event-category collections under
    /// <c>{Root}/events/</c>; note the <c>events/</c> segment, which the pre-2026-07-29 path lacked.
    /// </summary>
    /// <param name="language"></param>
    /// <returns></returns>
    internal static string Issues(string language) => $"{Root}/events/issues?language={Uri.EscapeDataString(language)}";
}
