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
    Api/             — ApiPagingRequest, ApiRequest, ApiResult / ApiResult<T>
      Embedded/      — ApiError, ApiErrorData, ApiErrorSource, DataPatch<T>, DataPost<T,R>
        DataResults/ — CollectionResult<T>, CollectionResultLinks, CollectionResultMeta, SingleResult<T>
        Relations/   — RelatedManyOutput, RelatedSingleInput, RelatedSingleOutput (+ embedded link types)
    Base/            — Data, DataIdentity (+ DataLinks, Meta, MetaAbility embedded)
    Batches/         — Batch, BatchAbilities, BatchData, BatchIncludes, BatchRelationships, BatchCreate, BatchCreateRelationships
    DeliveryProducts/ — DeliveryProduct, DeliveryProductData
    Files/           — FileUpload, FileUploadData
    LetterEvents/    — LetterEvent, LetterEventData, LetterEventIncludes, LetterEventRelationships
    LetterPrices/    — LetterPrice, LetterPriceConfiguration, LetterPriceData
    Letters/         — Letter, LetterAbilities, LetterData, LetterDataDetailed,
                       LetterIncludes, LetterRelationships (+ Batch), LetterFont, LetterMetaData, LetterMetaDataContact
      Views/         — LetterCreate, LetterCreateRelationships, LetterSend, LetterUpdate
    Organisations/   — Organisation, OrganisationData, OrganisationRelationships
    UserAssociations/ — UserAssociation, UserAssociationAbilities, UserAssociationData,
                        UserAssociationIncludes, UserAssociationRelationships, OrganisationAbilities
    Users/           — User, UserAbilities, UserData, UserRelationships
    Webhooks/        — Webhook, WebhookData, WebhookIncludes, WebhookRelationships
      Views/         — WebhookCreate
      WebhookEvents/ — WebhookEvent, WebhookEventData, WebhookEventRelationships
```

## Data Model Hierarchy

The core data model is a generic hierarchy that mirrors the JSON:API response shape:

```
IDataIdentity                  — { type: PingenApiDataType, id: string }
  DataIdentity                 — record implementing IDataIdentity
    Data                       — + links: DataLinks?
      Data<TAttributes>        — + attributes: TAttributes (where T : IAttributes)
        Data<TAttributes, TRelationships>  — + relationships: TRelationships

IDataResult                    — marker interface; also has Included: IList<object>?
  CollectionResult<TData>      — { data: IList<TData>, links, meta } where TData : IData
  SingleResult<TData>          — { data: TData } where TData : IData
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

- `Serialize(object)` / `Deserialize<T>(string)` / `DeserializeAsync<T>(Stream)` — all using a consistent `JsonSerializerOptions` with custom converters and `WhenWritingNull`.
- `TryGetIncludedData<T>(IDataResult, out Data<T>?)` — finds a specific type within the `Included` array by matching `PingenApiDataType` via `PingenApiDataTypeMapping`.
- **Important**: `SerializerOptions()` is called fresh per operation. There is no static cached instance.

### PingenWebhookHelper (`Helpers/PingenWebhookHelper.cs`)

- `ValidateWebhookAndGetData(signingKey, signature, requestStream)` — validates HMAC-SHA256 signature, then deserializes `SingleResult<WebhookEventData>` and extracts included `Organisation`, `Letter`, and `LetterEvent` via `TryGetIncludedData`.
- `ValidateWebhook(signingKey, signature, requestStream)` — signature validation only, returns `bool`.
- Throws `PingenWebhookValidationErrorException` on signature mismatch.

### PingenAttributesPropertyHelper\<T\> (`Helpers/PingenAttributesPropertyHelper.cs`)

Generic utility (where `T : IAttributes`) providing `GetJsonPropertyName<TValue>(Expression<Func<T, TValue>>)`. Uses reflection to read `[JsonPropertyName]` from the selected property. Used to build type-safe filter and sort keys for `ApiPagingRequest`.

## JSON Converters

| Converter | Purpose |
|---|---|
| `PingenDateTimeConverter` | Parses Pingen's non-standard datetime format |
| `PingenDateTimeNullableConverter` | Nullable variant of the above |
| `PingenKeyValuePairStringObjectConverter` | Serializes `KeyValuePair<string, object>` used for nested filter expressions |
