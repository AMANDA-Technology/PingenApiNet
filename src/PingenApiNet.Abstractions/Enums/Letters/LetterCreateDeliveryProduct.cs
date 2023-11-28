/* Copyright (C) AMANDA Technology - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Manuel Gysin <manuel.gysin@amanda-technology.ch>
 * Written by Philip Näf <philip.naef@amanda-technology.ch>
 */

namespace PingenApiNet.Abstractions.Enums.Letters;

/// <summary>
/// Letter delivery product to be used at Letters/Create endpoint. <see href="https://api.v2.pingen.com/documentation#tag/letters.general/operation/letters.show">API Doc - Letter details</see>
/// <br/>NOTE: This seems to be confusing and different delivery products will be used at Letters/Send endpoint.
/// </summary>
public static class LetterCreateDeliveryProduct
{
    /// <summary>
    /// Some 'fast' product?
    /// </summary>
    public const string Fast = "fast";

    /// <summary>
    /// Some 'cheap' product?
    /// </summary>
    public const string Cheap = "cheap";

    /// <summary>
    /// Some 'bulk' product?
    /// </summary>
    public const string Bulk = "bulk";

    /// <summary>
    /// Some 'premium' product?
    /// </summary>
    public const string Premium = "premium";

    /// <summary>
    /// Some 'registered' product?
    /// </summary>
    public const string Registered = "registered";
}
