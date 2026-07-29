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
using PingenApiNet.Abstractions.Models.Api.Embedded.Relations;

namespace PingenApiNet.Abstractions.Models.Webhooks.WebhookEvents;

/// <summary>
/// Webhook event relationships.
/// <para>
/// Since Pingen's <b>2026-07-27</b> deliverable generalisation the documented shape is
/// <c>organisation</c> + <c>deliverable</c> + <c>event</c> (all three spec-required on
/// <c>WebhookDeliverable*GET</c>); <c>letter</c> is no longer a declared property at all. Pingen still sends
/// it on the wire, so both are modelled and both are nullable — <see cref="Letter"/> because the contract has
/// already dropped it, <see cref="Deliverable"/> because bodies captured before the rollout do not carry it.
/// Prefer <see cref="Deliverable"/> and fall back to <see cref="Letter"/>.
/// </para>
/// </summary>
/// <param name="Organisation">The organisation the deliverable belongs to.</param>
/// <param name="Letter">
/// Legacy relationship to the letter. Undocumented since 2026-07-27 but still sent; will disappear.
/// Use <paramref name="Deliverable"/> instead.
/// </param>
/// <param name="Event">
/// The delivery event. Its <c>data.type</c> is <c>deliverables_events</c> since 2026-07-27
/// (<c>letters_events</c> before).
/// </param>
/// <param name="Deliverable">
/// The deliverable the event belongs to. Its <c>data.type</c> is one of <c>letters</c>, <c>emails</c> or
/// <c>ebills</c>. Null on bodies predating the 2026-07-27 rollout.
/// </param>
public sealed record WebhookEventRelationships(
    [property: JsonPropertyName("organisation")] RelatedSingleOutput Organisation,
    [property: JsonPropertyName("letter")] RelatedSingleOutput? Letter,
    [property: JsonPropertyName("event")] RelatedSingleOutput Event,
    [property: JsonPropertyName("deliverable")] RelatedSingleOutput? Deliverable = null
) : IRelationships;
