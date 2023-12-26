/* Copyright (C) AMANDA Technology - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Manuel Gysin <manuel.gysin@amanda-technology.ch>
 * Written by Philip Näf <philip.naef@amanda-technology.ch>
 */

namespace PingenApiNet.Services.Connectors.Endpoints;

/// <summary>
/// Endpoints (API request paths) for user service
/// </summary>
internal static class UsersEndpoints
{
    /// <summary>
    /// Root path of user
    /// </summary>
    internal const string Root = "user";

    /// <summary>
    /// Endpoint to get associations (page)
    /// </summary>
    internal const string Associations = $"{Root}/associations";
}
