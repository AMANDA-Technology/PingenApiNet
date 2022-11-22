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

namespace PingenApiNet.Abstractions.Enums;

/// <summary>
/// Pingen API data type to identify the kind of data transported in requests
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PingenApiDataType
{
    /// <summary>
    /// Data type letters
    /// </summary>
    letters,

    /// <summary>
    /// Data type organisations
    /// </summary>
    organisations,

    /// <summary>
    /// Data type letter_price_calculator
    /// </summary>
    letter_price_calculator,

    /// <summary>
    /// Data type letters_events
    /// </summary>
    letters_events,

    /// <summary>
    /// Data type users
    /// </summary>
    users,

    /// <summary>
    /// Data type associations
    /// </summary>
    associations,

    /// <summary>
    /// Data type webhooks
    /// </summary>
    webhooks,

    /// <summary>
    /// Data type file_uploads
    /// </summary>
    file_uploads,

    /// <summary>
    /// Data type webhook_issues
    /// </summary>
    webhook_issues,

    /// <summary>
    /// Data type webhook_sent
    /// </summary>
    webhook_sent,

    /// <summary>
    /// Data type webhook_undeliverable
    /// </summary>
    webhook_undeliverable
}
