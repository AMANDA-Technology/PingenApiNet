/* Copyright (C) AMANDA Technology - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Manuel Gysin <manuel.gysin@amanda-technology.ch>
 * Written by Philip Näf <philip.naef@amanda-technology.ch>
 */

namespace PingenApiNet.Services.Connectors.Endpoints;

/// <summary>
/// Endpoints (API request paths) for letters service
/// </summary>
internal static class LettersEndpoints
{
    /// <summary>
    /// Root path of distributions
    /// </summary>
    internal const string Root = "letters";

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
    internal static string Events(string id, string language) => $"{Single(id)}/events?language={language}";

    /// <summary>
    /// Endpoint to get issues
    /// </summary>
    /// <param name="language"></param>
    /// <returns></returns>
    internal static string Issues(string language) => $"{Root}/issues?language={language}";
}
