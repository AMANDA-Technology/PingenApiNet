---
title: C4 Level 3 — PingenApiNet.Abstractions
tags: [architecture, c4, components, abstractions]
---

# C4 Level 3: PingenApiNet.Abstractions

`src/PingenApiNet.Abstractions/`

This package contains all data contracts, domain interfaces, enums, helpers, and exceptions. It has no NuGet dependencies, making it suitable for reference from any project layer (domain, application, infrastructure) without pulling in HTTP or DI concerns.

## Internal Structure

```
PingenApiNet.Abstractions/
  Enums/
    Api/             — ApiHeaderNames, ApiQueryParameterNames, CollectionFilterOperator,
                       CollectionSortDirection, PingenApiAbility, PingenApiCurrency,
                       PingenApiDataType, PingenApiLanguage, WebhookEventCategory
    Batches/         — BatchGroupingOptions*, BatchGroupingType, BatchIcon
    LetterEvents/    — LetterEventCodes
    Letters/         — LetterAddressPosition, LetterCreateDeliveryProduct, LetterPaperTypes,
                       LetterPrintMode, LetterPrintSpectrum, LetterSendDeliveryProduct, LetterStates
    Users/           — UserAssociationStatus, UserRole
  Exceptions/
    PingenApiErrorException
    PingenFileDownloadException
    PingenWebhookValidationErrorException
  Helpers/
    JsonConverters/  — PingenDateTimeConverter, PingenDateTimeNullableConverter,
                       PingenKeyValuePairStringObjectConverter
    PingenAttributesPropertyHelper<T>
    PingenSerialisationHelper
    PingenWebhookHelper
  Interfaces/
    Data/            — IAbilities, IAttributes, IData, IDataIdentity, IDataPatch,
                       IDataPost, IDataResult, IMeta, IMetaAbility, IRelationships
  Models/
    Api/             — ApiPagingRequest, ApiRequest, ApiResult / ApiResult<T>, ExternalRequestResult
      Embedded/      — ApiError, ApiErrorData, ApiErrorSource, DataPatch<T>, DataPost<T,R>
        DataResults/ — CollectionResult<T>, IncludedCollection, SingleResult<T>
          Embedded/  — CollectionResultLinks, CollectionResultMeta
        Relations/   — RelatedManyOutput, RelatedSingleInput, RelatedSingleOutput (+ embedded link types)
    Base/            — Data, DataIdentity (+ DataLinks, Meta, MetaAbility embedded)
    Batches/         — Batch, BatchAbilities, BatchData, BatchFields, BatchIncludes,
                       BatchRelationships, BatchCreate, BatchCreateRelationships
    DeliveryProducts/ — DeliveryProduct, DeliveryProductData, DeliveryProductFields
    Files/           — FileUpload, FileUploadData
    LetterEvents/    — LetterEvent, LetterEventData, LetterEventFields, LetterEventIncludes,
                       LetterEventRelationships
    LetterPrices/    — LetterPrice, LetterPriceConfiguration, LetterPriceData, LetterPriceFields
    Letters/         — Letter, LetterAbilities, LetterData, LetterDataDetailed,
                       LetterFields, LetterIncludes, LetterRelationships,
                       LetterFont, LetterMetaData, LetterMetaDataContact
      Views/         — LetterCreate, LetterCreateRelationships, LetterSend, LetterUpdate
    Organisations/   — Organisation, OrganisationData, OrganisationFields, OrganisationRelationships
    UserAssociations/ — UserAssociation, UserAssociationAbilities, UserAssociationData,
                        UserAssociationFields, UserAssociationIncludes,
                        UserAssociationRelationships, OrganisationAbilities
    Users/           — User, UserAbilities, UserData, UserFields, UserRelationships
    Webhooks/        — Webhook, WebhookData, WebhookFields, WebhookIncludes, WebhookRelationships
      Views/         — WebhookCreate
      WebhookEvents/ — WebhookEvent, WebhookEventData, WebhookEventFields, WebhookEventRelationships
```

## Data Model Hierarchy

The core data model is a generic hierarchy that mirrors the JSON:API response shape:

```
IDataIdentity                  — { type: PingenApiDataType, id: string }
  DataIdentity                 — record implementing IDataIdentity
    Data                       — + links: DataLinks?
      Data<TAttributes>        — + attributes: TAttributes (where T : IAttributes)
        Data<TAttributes, TRelationships>  — + relationships: TRelationships

IDataResult                    — carrier interface; defines Included: IncludedCollection?
  CollectionResult<TData>      — { data: IList<TData>, links, meta, included? } where TData : IData
  SingleResult<TData>          — { data: TData, included? } where TData : IData

IncludedCollection             — strongly-typed wrapper around the JSON:API `included` array;
                                 stores raw JsonElement items and exposes
                                   • OfType<T>() where T : IAttributes  → IEnumerable<Data<T>>
                                   • FindById<T>(string id) where T : IAttributes  → Data<T>?
                                 Type discriminator → CLR type lookup comes from
                                 `PingenSerialisationHelper.PingenApiDataTypeMapping`.
                                 Has a `[JsonConverter(typeof(IncludedCollectionJsonConverter))]`
                                 that reads/writes each element as a raw JsonElement so that
                                 heterogeneous resource types can coexist in one array.
```

All attributes types (e.g., `Letter`, `Batch`, `Organisation`) implement `IAttributes` and are C# positional records with `[JsonPropertyName]` on every property.

"Data" types (e.g., `LetterData`, `BatchData`) are simple type aliases — `public record LetterData : Data<Letter, LetterRelationships>`. The "Detailed" variants add a `Meta` property (e.g., `LetterDataDetailed` adds `Meta<MetaAbility<LetterAbilities>>`).

## Write Payloads

```
IDataPost                — marker interface
  DataPost<TAttributes>  — { type, attributes }
    DataPost<TAttributes, TRelationships>  — + relationships?

IDataPatch               — extends IDataPost
  DataPatch<TAttributes> — extends DataPost<TAttributes> + { id }
```

`PingenConnectionHandler` wraps these in a `{ "data": <payload> }` envelope before serialization.

## Key Helpers

### PingenSerialisationHelper (`Helpers/PingenSerialisationHelper.cs`)

- `Serialize(object)` / `Deserialize<T>(string)` / `DeserializeAsync<T>(Stream)` — all using a consistent `JsonSerializerOptions` with `DefaultIgnoreCondition = WhenWritingNull`, `DictionaryKeyPolicy = CamelCase`, and three custom converters (`PingenDateTimeConverter`, `PingenDateTimeNullableConverter`, `PingenKeyValuePairStringObjectConverter`).
- `TryGetIncludedData<T>(IDataResult, out Data<T>?)` — finds a single included resource of type `T` by delegating to `IncludedCollection.OfType<T>().SingleOrDefault()`; uses `PingenApiDataTypeMapping` for the type-string → CLR-type lookup.
- `PingenApiDataTypeMapping` — static dictionary mapping `PingenApiDataType` enum values to their corresponding `IAttributes` CLR type (`letters → Letter`, `batches → Batch`, `webhook_issues / webhook_sent / webhook_undeliverable / webhook_delivered → WebhookEvent`, `letters_events / deliverables_events → LetterEvent`). New resource types must be registered here, otherwise `IncludedCollection.OfType<T>()` / `FindById<T>()` and `TryGetIncludedData<T>()` will silently skip them. A handful of values are deliberately unmapped because no attributes model fits them yet (`presets`, `emails`, `ebills`); `PingenApiDataTypeMappingTests` enforces that the set of exceptions is explicit rather than accidental. Note the mapping is many-to-one in both directions of risk: two discriminators may share a CLR type (the delivery-event pair), which is why `OfType<T>()` collapses one resource arriving under both.
- **Caching**: `SerializerOptions()` returns a single `static readonly` `CachedSerializerOptions` instance. `JsonSerializerOptions` is thread-safe once initialized; do not mutate it.

### PingenWebhookHelper (`Helpers/PingenWebhookHelper.cs`)

- `ValidateWebhookAndGetData(signingKey, signature, requestStream, cancellationToken)` — reads the stream once into a payload string, rewinds it, validates the HMAC-SHA256 signature (via `ValidateWebhook`), deserializes `SingleResult<WebhookEventData>`, and extracts included `Organisation`, `Letter`, and `LetterEvent` via `TryGetIncludedData`. Returns a 4-tuple. Deserialization happens *after* signature validation, so an unknown `type` discriminator anywhere in the body — top level or on any relationship — surfaces as a `JsonException` on an otherwise authentic request; see `doc/analysis/2026-05-01-api-docs-gap-audit.md` for the two outages this has caused.
- `ValidateWebhook(signingKey, signature, requestStream, cancellationToken)` — signature-only check. Computes HMAC-SHA256 of the stream, converts the caller-supplied signature from hex, and compares with `CryptographicOperations.FixedTimeEquals` (constant-time to prevent timing attacks). Returns `bool`. Catches `FormatException` from non-hex signatures and returns `false`.
- Throws `PingenWebhookValidationErrorException` from `ValidateWebhookAndGetData` on signature mismatch; the exception carries the deserialized `WebhookEventData` (best-effort) for diagnostic logging.
- **Caller responsibility**: ASP.NET callers must call `Request.EnableBuffering()` before passing `Request.Body` because the helper reads the stream twice (once for payload extraction, once via HMAC).

### PingenAttributesPropertyHelper\<T\> (`Helpers/PingenAttributesPropertyHelper.cs`)

Generic utility (where `T : IAttributes`) providing `GetJsonPropertyName<TValue>(Expression<Func<T, TValue>>)`. Uses reflection to read `[JsonPropertyName]` from the selected property. Used to build type-safe filter and sort keys for `ApiPagingRequest`.

## JSON Converters

| Converter | Purpose |
|---|---|
| `PingenDateTimeConverter` | Parses Pingen's non-standard datetime format |
| `PingenDateTimeNullableConverter` | Nullable variant of the above |
| `PingenKeyValuePairStringObjectConverter` | Serializes `KeyValuePair<string, object>` used for nested filter expressions |
