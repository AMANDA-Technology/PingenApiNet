/*
MIT License

Copyright (c) 2022 Philip Näf <philip.naef@amanda-technology.ch>
Copyright (c) 2022 Manuel Gysin <manuel.gysin@amanda-technology.ch>
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

using System.Text.Json.Serialization;
// ReSharper disable InconsistentNaming

namespace PingenApiNet.Abstractions.Enums.Api;

/// <summary>
/// Pingen API data type to identify the kind of data transported in requests
/// </summary>
/// <remarks>
/// The numeric values are part of the public ABI of the <c>PingenApiNet.Abstractions</c> NuGet package:
/// C# inlines enum constants into consumer IL, so renumbering an existing member silently changes its
/// meaning for any consumer assembly that is not recompiled. Every member therefore carries an explicit
/// value, and new members must take the next free number — never insert into or reorder the existing ones.
/// The wire format is unaffected either way (<see cref="JsonStringEnumConverter{TEnum}" /> serializes by name).
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<PingenApiDataType>))]
public enum PingenApiDataType
{
    /// <summary>
    /// Data type letters
    /// </summary>
    letters = 0,

    /// <summary>
    /// Data type batches
    /// </summary>
    batches = 1,

    /// <summary>
    /// Data type organisations
    /// </summary>
    organisations = 2,

    /// <summary>
    /// Data type letter_price_calculator
    /// </summary>
    letter_price_calculator = 3,

    /// <summary>
    /// Data type letters_events
    /// </summary>
    letters_events = 4,

    /// <summary>
    /// Data type users
    /// </summary>
    users = 5,

    /// <summary>
    /// Data type associations
    /// </summary>
    associations = 6,

    /// <summary>
    /// Data type webhooks
    /// </summary>
    webhooks = 7,

    /// <summary>
    /// Data type file_uploads
    /// </summary>
    file_uploads = 8,

    /// <summary>
    /// Data type webhook_issues
    /// </summary>
    webhook_issues = 9,

    /// <summary>
    /// Data type webhook_sent
    /// </summary>
    webhook_sent = 10,

    /// <summary>
    /// Data type webhook_undeliverable
    /// </summary>
    webhook_undeliverable = 11,

    /// <summary>
    /// Data type delivery products
    /// </summary>
    delivery_products = 12,

    /// <summary>
    /// Data type presets. Used as the JSON:API <c>type</c> discriminator on relationship
    /// inputs (<c>LetterCreateRelationships</c>, <c>BatchCreateRelationships</c>) only.
    /// No <c>Preset</c> attributes model is currently bound, so this value is intentionally
    /// absent from <c>PingenSerialisationHelper.PingenApiDataTypeMapping</c>.
    /// </summary>
    presets = 13,

    /// <summary>
    /// Data type webhook_delivered. Sent for the <c>delivered</c> webhook event category
    /// ("Delivered Sent Documents": letters posted with delivery confirmation, e.g. registered mail).
    /// Its attributes are <c>url</c> + <c>created_at</c> only — identical to <c>webhook_sent</c> —
    /// so it binds to the shared <c>WebhookEvent</c> model with a null <c>Reason</c>.
    /// </summary>
    webhook_delivered = 14,

    /// <summary>
    /// Data type deliverables_events. Pingen's generalised name for a delivery event, introduced when the
    /// API grew a "deliverable" abstraction over letters, emails and ebills. Rolled out unannounced on
    /// <b>2026-07-27</b>: the webhook body's <c>data.relationships.event.data.type</c> changed from
    /// <see cref="letters_events"/> to this value for <i>every</i> event category.
    /// <para>
    /// That relationship binds to a non-nullable <see cref="PingenApiDataType"/>, so a library that does not
    /// know the value throws <c>JsonException</c> out of <c>PingenWebhookHelper.ValidateWebhookAndGetData</c>
    /// — <b>after</b> the HMAC signature has validated — and the consumer answers 4xx/5xx to every delivery
    /// until Pingen dead-letters the event. Enumerating it is the fix; see
    /// <c>doc/analysis/2026-05-01-api-docs-gap-audit.md</c> § Addendum (2026-07-29).
    /// </para>
    /// <para>
    /// Pingen kept the legacy shape alongside the new one: the body still carries a <c>letter</c>
    /// relationship (next to the new <c>deliverable</c>) and <c>included</c> now contains the <b>same event
    /// twice</b> — once typed <c>letters_events</c> and once typed <c>deliverables_events</c>, sharing one id.
    /// That duplication is why this value is deliberately absent from
    /// <c>PingenSerialisationHelper.PingenApiDataTypeMapping</c>: mapping it to <c>LetterEvent</c> as well
    /// would make <c>IncludedCollection.OfType&lt;LetterEvent&gt;()</c> yield two resources, which is exactly
    /// enough for <c>PingenSerialisationHelper.TryGetIncludedData</c>'s <c>SingleOrDefault()</c> to throw and
    /// re-break the very webhooks this value repairs. The <see cref="letters_events"/> entry stays the single
    /// binding for the event's attributes; revisit only if Pingen drops the legacy include.
    /// </para>
    /// </summary>
    deliverables_events = 15
}
