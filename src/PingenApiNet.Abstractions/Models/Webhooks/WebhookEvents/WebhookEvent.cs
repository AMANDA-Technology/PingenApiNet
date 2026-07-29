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
using PingenApiNet.Abstractions.Interfaces.Data;
using PingenApiNet.Abstractions.Models.Webhooks.WebhookEvents.Embedded;

namespace PingenApiNet.Abstractions.Models.Webhooks.WebhookEvents;

/// <summary>
/// Webhook event. Shared by all four event categories (<c>issues</c>, <c>sent</c>, <c>undeliverable</c>,
/// <c>delivered</c>), whose attribute surfaces are supersets of one another — hence every property being
/// nullable rather than one record per category.
/// </summary>
/// <param name="Reason">Why the item failed. Sent on <c>issues</c> and <c>undeliverable</c> only.</param>
/// <param name="Url"></param>
/// <param name="CreatedAt"></param>
/// <param name="CorrectedAddress">
/// The address Pingen worked out for an undeliverable item. Sent on <c>undeliverable</c> only, where the
/// spec marks it required; null for every other category.
/// </param>
public sealed record WebhookEvent(
    [property: JsonPropertyName(WebhookEventFields.Reason)] string? Reason,
    [property: JsonPropertyName(WebhookEventFields.Url)] Uri? Url,
    [property: JsonPropertyName(WebhookEventFields.CreatedAt)] DateTime? CreatedAt,
    [property: JsonPropertyName(WebhookEventFields.CorrectedAddress)] WebhookEventCorrectedAddress? CorrectedAddress = null
) : IAttributes;
