---
title: "ADR-001: C# Records and System.Text.Json for Domain Models"
tags: [adr, serialization, models]
---

# ADR-001: C# Records and System.Text.Json for Domain Models

## Context

The Pingen v2 API is JSON:API compliant. All resources have a predictable shape: `type`, `id`, `attributes`, `relationships`, `links`, `meta`, and `included`. The library needs to serialize request payloads and deserialize response bodies with high fidelity. It also needs to expose these types to consumer applications.

Two serializer options were available: `Newtonsoft.Json` (used in README code samples as `JsonConvert.SerializeObject`) and `System.Text.Json` (inbox with .NET, higher performance).

## Decision

Use C# `record` types (immutable by default, structural equality, concise syntax) for all models and `System.Text.Json` for serialization.

All attribute types (e.g., `Letter`, `Batch`) are positional records. Data wrapper types (e.g., `LetterData`, `BatchData`) are non-positional records that extend the generic `Data<TAttributes, TRelationships>` hierarchy. Request payloads (`DataPost<T>`, `DataPatch<T>`) are records with `required` init-only properties.

Custom JSON converters handle Pingen's non-standard date format and the nested `KeyValuePair<string, object>` filter expression structure.

Enums that cross the JSON boundary use `[JsonConverter(typeof(JsonStringEnumConverter))]` and lowercase enum member names matching the API's string literals (e.g., `LetterPrintMode.simplex`).

## Consequences

**Good:**
- Records enforce immutability, matching the "response data should not be mutated" principle.
- `System.Text.Json` has no external dependency — keeps `PingenApiNet.Abstractions` dependency-free.
- `[JsonPropertyName]` on every property enables `PingenAttributesPropertyHelper<T>` to safely resolve sort/filter field names at runtime via reflection.
- Positional records for attributes types produce concise, readable code.

**Bad / Watch out:**
- `SerializerOptions()` is instantiated fresh per call in `PingenSerialisationHelper` — there is no static cached `JsonSerializerOptions`. This is a minor allocation overhead. If performance becomes a concern, caching a static instance should be considered (but requires care with the custom converters).
- README examples use `JsonConvert.SerializeObject` (Newtonsoft) for error formatting in sample code. Consumer applications using Newtonsoft will need to add it separately; the library itself does not pull it in.
- `System.Text.Json` does not support `[Optional]` attributes from `System.Runtime.InteropServices` in the same way Newtonsoft does — but this library uses `[Optional]` only on method parameters (for C# 13 optional parameter syntax), not on serialized types, so there is no conflict.
