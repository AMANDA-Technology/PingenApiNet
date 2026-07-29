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

namespace PingenApiNet.Abstractions.Models.LetterEvents;

/// <summary>
/// Letter event relationships.
/// <para>
/// The letter-events endpoint (<c>GET .../deliveries/letters/{letterId}/events</c>) still returns a
/// <c>letter</c> relationship, which is what the spec documents for <c>LetterEventEloquentGETListItem</c>.
/// Its deliverable-generalised counterpart, <c>DeliverableEventLuceneGETListItem</c> — already used by the
/// emails and ebills event endpoints — declares <c>deliverable</c> instead. Both are modelled and both are
/// nullable so this record survives the letter endpoint being switched over the way the webhook body was on
/// 2026-07-27. Prefer <see cref="Deliverable"/> and fall back to <see cref="Letter"/>.
/// </para>
/// </summary>
/// <param name="Letter">The letter this event belongs to. Null once Pingen switches this endpoint over.</param>
/// <param name="Deliverable">
/// The deliverable this event belongs to (<c>letters</c>, <c>emails</c> or <c>ebills</c>).
/// Null while the endpoint still returns the letter-shaped payload.
/// </param>
public sealed record LetterEventRelationships(
    [property: JsonPropertyName("letter")] RelatedSingleOutput? Letter,
    [property: JsonPropertyName("deliverable")] RelatedSingleOutput? Deliverable = null
) : IRelationships;
