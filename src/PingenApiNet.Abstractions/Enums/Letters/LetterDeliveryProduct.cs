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

using System.Text.Json.Serialization;
// ReSharper disable InconsistentNaming

namespace PingenApiNet.Abstractions.Enums.Letters;

/// <summary>
/// Letter delivery product. <see href="https://api.v2.pingen.com/documentation#tag/letters.general/operation/letters.show">API Doc - Letter details</see>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LetterDeliveryProduct
{
    /// <summary>
    /// Delivery product fast
    /// </summary>
    fast,

    /// <summary>
    /// Delivery product cheap
    /// </summary>
    cheap,

    /// <summary>
    /// Delivery product bulk
    /// </summary>
    bulk,

    /// <summary>
    /// Delivery product premium
    /// </summary>
    premium,

    /// <summary>
    /// Delivery product registered
    /// </summary>
    registered,

    /// <summary>
    /// Delivery product AT Post economy
    /// </summary>
    atpost_economy,

    /// <summary>
    /// Delivery product AT Post priority
    /// </summary>
    atpost_priority,

    /// <summary>
    /// Delivery product Post AG A
    /// </summary>
    postag_a,

    /// <summary>
    /// Delivery product Post AG B
    /// </summary>
    postag_b,

    /// <summary>
    /// Delivery product Post AG B2
    /// </summary>
    postag_b2,

    /// <summary>
    /// Delivery product Post AG Registered
    /// </summary>
    postag_registered,

    /// <summary>
    /// Delivery product Post AG A-Plus
    /// </summary>
    postag_aplus,

    /// <summary>
    /// Delivery product DP AG standard
    /// </summary>
    dpag_standard,

    /// <summary>
    /// Delivery product DP AG economy
    /// </summary>
    dpag_economy,

    /// <summary>
    /// Delivery product Ind post mail
    /// </summary>
    indpost_mail,

    /// <summary>
    /// Delivery product Ind post speed mail
    /// </summary>
    indpost_speedmail,

    /// <summary>
    /// Delivery product NL post priority
    /// </summary>
    nlpost_priority,

    /// <summary>
    /// Delivery product DHL priority
    /// </summary>
    dhl_priority
}
