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

namespace PingenApiNet.Abstractions.Enums.Letters;

/// <summary>
/// Letter delivery product to be used at Letters/Send endpoint. <see href="https://api.pingen.com/documentation#tag/letters.general/operation/letters.show">API Doc - Letter details</see>
/// <br/>NOTE: This list is adjusted from what delivery products are available on pingen. API Doc seems to be wrong.
/// <para>
/// Re-verified 2026-07-29 and the note still holds. The spec constrains <c>delivery_product</c> to the five
/// generic tiers on both <c>LetterAttributes</c> and <c>LetterSendPATCH</c>
/// (see <see cref="LetterCreateDeliveryProduct"/>), and none of the carrier-specific ids below appear
/// anywhere in it. The live API nonetheless <b>returns</b> them — a captured webhook body carries
/// <c>"delivery_product": "postag_b"</c>. The values here are observed from the product, like
/// <see cref="LetterStates"/>.
/// </para>
/// <para>
/// This is why <c>Letter.DeliveryProduct</c>, <c>LetterCreate.DeliveryProduct</c> and
/// <c>LetterSend.DeliveryProduct</c> are all plain <c>string</c> and these are <c>const string</c> helpers
/// rather than a C# enum. Binding a strict enum here would make every response carrying a carrier id
/// unparseable — the exact failure that took webhooks down on 2026-07-27. Do not "tighten" it.
/// </para>
/// </summary>
public static class LetterSendDeliveryProduct
{
    /// <summary>
    /// Delivery product AT Post economy
    /// </summary>
    public const string AtPostEconomy = "atpost_economy";

    /// <summary>
    /// Delivery product AT Post priority
    /// </summary>
    public const string AtPostPriority = "atpost_priority";

    /// <summary>
    /// Delivery product Post AG A
    /// </summary>
    public const string PostAgA = "postag_a";

    /// <summary>
    /// Delivery product Post AG B
    /// </summary>
    public const string PostAgB = "postag_b";

    /// <summary>
    /// Delivery product Post AG B2
    /// </summary>
    public const string PostAgB2 = "postag_b2";

    /// <summary>
    /// Delivery product Post AG Registered
    /// </summary>
    public const string PostAgRegistered = "postag_registered";

    /// <summary>
    /// Delivery product Post AG A-Plus
    /// </summary>
    public const string PostAgAPlus = "postag_aplus";

    /// <summary>
    /// Delivery product DP AG standard
    /// </summary>
    public const string DpAgStandard = "dpag_standard";

    /// <summary>
    /// Delivery product DP AG economy
    /// </summary>
    public const string DpAgEconomy = "dpag_economy";

    /// <summary>
    /// Delivery product Ind post mail
    /// </summary>
    public const string IndPostMail = "indpost_mail";

    /// <summary>
    /// Delivery product Ind post speed mail
    /// </summary>
    public const string IndPostSpeedMail = "indpost_speedmail";

    /// <summary>
    /// Delivery product NL post priority
    /// </summary>
    public const string NlPostPriority = "nlpost_priority";

    /// <summary>
    /// Delivery product DHL Europe priority
    /// </summary>
    public const string DhlEuropePriority = "dhl_europe_priority";

    /// <summary>
    /// Delivery product DHL World priority
    /// </summary>
    public const string DhlWorldPriority = "dhl_world_priority";
}
