/*
MIT License

Copyright (c) 2024 Dejan Appenzeller <dejan.appenzeller@swisspeers.ch>

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

using System.Runtime.Serialization;
using System.Text.Json.Serialization;
// ReSharper disable InconsistentNaming

namespace PingenApiNet.Abstractions.Enums.Batches;

/// <summary>
/// Batch icon
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BatchIcon
{
    /// <summary>
    /// Campaign
    /// </summary>
    campaign,

    /// <summary>
    /// Megaphone
    /// </summary>
    megaphone,

    /// <summary>
    /// Wave-hand
    /// </summary>
    [EnumMember(Value = "wave-hand")]
    waveHand,

    /// <summary>
    /// Flash
    /// </summary>
    flash,

    /// <summary>
    /// Rocket
    /// </summary>
    rocket,

    /// <summary>
    /// Bell
    /// </summary>
    bell,

    /// <summary>
    /// Percent-tag
    /// </summary>
    [EnumMember(Value = "percent-tag")]
    percentTag,

    /// <summary>
    /// Percent-badge
    /// </summary>
    [EnumMember(Value = "percent-badge")]
    percentBadge,

    /// <summary>
    /// Present
    /// </summary>
    present,

    /// <summary>
    /// Receipt
    /// </summary>
    receipt,

    /// <summary>
    /// Document
    /// </summary>
    document,

    /// <summary>
    /// Information
    /// </summary>
    information,

    /// <summary>
    /// Calendar
    /// </summary>
    calendar,

    /// <summary>
    /// Newspaper
    /// </summary>
    newspaper,

    /// <summary>
    /// Crown
    /// </summary>
    crown,

    /// <summary>
    /// Virus
    /// </summary>
    virus
}
