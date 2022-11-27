/* Copyright (C) AMANDA Technology - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Manuel Gysin <manuel.gysin@amanda-technology.ch>
 * Written by Philip Näf <philip.naef@amanda-technology.ch>
 */

namespace PingenApiNet.Abstractions.Models.Api;

/// <summary>
/// An API request object to sent to the API with meta information to send as headers or query parameters
/// </summary>
public abstract record ApiRequest
{
    // TODO: Add Sparse fieldsets? https://api.v2.pingen.com/documentation#section/Advanced/Sparse-fieldsets
    // TODO: Add Including relationships? https://api.v2.pingen.com/documentation#section/Advanced/Including-relationships
    // NOTE: When implementing, every request on all connector services should accept this one as optional argument, or implement a 'raw' request method. And make it non abstract.
}
