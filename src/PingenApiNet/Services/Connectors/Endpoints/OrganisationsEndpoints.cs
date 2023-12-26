﻿/* Copyright (C) AMANDA Technology - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Manuel Gysin <manuel.gysin@amanda-technology.ch>
 * Written by Philip Näf <philip.naef@amanda-technology.ch>
 */

namespace PingenApiNet.Services.Connectors.Endpoints;

/// <summary>
/// Endpoints (API request paths) for organisations service
/// </summary>
internal static class OrganisationsEndpoints
{
    /// <summary>
    /// Root path of organisations
    /// </summary>
    internal const string Root = "organisations";

    /// <summary>
    /// Endpoint to access a single organisation
    /// </summary>
    internal static string Single(string id) => $"{Root}/{id}";
}
