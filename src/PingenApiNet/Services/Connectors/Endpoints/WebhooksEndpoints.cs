/* Copyright (C) AMANDA Technology - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Manuel Gysin <manuel.gysin@amanda-technology.ch>
 * Written by Philip Näf <philip.naef@amanda-technology.ch>
 */

namespace PingenApiNet.Services.Connectors.Endpoints;

/// <summary>
/// Endpoints (API request paths) for webhooks service
/// </summary>
internal static class WebhooksEndpoints
{
    /// <summary>
    /// Root path of webhooks
    /// </summary>
    internal const string Root = "webhooks";

    /// <summary>
    /// Endpoint to access a single webhook
    /// </summary>
    internal static string Single(string id) => $"{Root}/{id}";
}
