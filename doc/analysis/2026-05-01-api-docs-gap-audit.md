---
title: API Docs Gap Analysis & Model/Mapping Audit
date: 2026-05-01
issue: 105
parent_epic: 102
phase: 1
sub_issue_handoffs: [106, 107, 108, 110]
---

# API Docs Gap Analysis & Model/Mapping Audit

**Date:** 2026-05-01
**Issue:** [#105 — API-Docs Gap Analysis & Model/Mapping Audit](https://github.com/AMANDA-Technology/PingenApiNet/issues/105)
**Parent Epic:** [#102](https://github.com/AMANDA-Technology/PingenApiNet/issues/102)
**Reference:** Pingen API documentation root — <https://api.pingen.com/documentation>
**Scope:** Read-only inventory of library endpoints, sparse-fieldset / include constants, and enums against the upstream Pingen API documentation, plus a single new reflection test (`PingenApiDataTypeMappingTests`).

---

## How to read this document

This audit is a **point-in-time inventory** of the library's public surface measured against the Pingen API specification (v2.0.0). It is intentionally read-only: no `src/**` files were modified. Findings are grouped into four tables and then re-grouped into per-sub-issue handoff lists at the bottom for follow-up implementation work.

**`Match?` column legend**

| Value | Meaning |
|---|---|
| ✅ Verified | Library URL / constant matches a stable Pingen-documented operation that the library currently exercises through unit + integration tests. |
| ⚠️ Verify upstream | Library shape is internally consistent (constants align with `[JsonPropertyName]`, integration tests round-trip), but the upstream operation/field/include must be re-checked against `https://api.pingen.com/documentation` during the follow-up sub-issue. The audit cannot reach the live docs. |
| ❌ Gap | A defect surfaced by the audit. Documented with severity and a target sub-issue. |
| 🟡 Undocumented | Library exposes an endpoint or surface that is not part of the public Pingen documentation (e.g., `DistributionService`). Tracked but not "fixable" from the library side. |

---

## 1. Endpoint URL audit

Effective request paths are constructed by `PingenConnectionHandler` at `src/PingenApiNet/Services/PingenConnectionHandler.cs:275-296`. Any request path that begins with one of `NonOrganisationEndpoints` (`file-upload`, `user`, `organisations`) is sent verbatim. All other paths are prefixed with `organisations/{organisationId}/`. The `Pingen.Api` `HttpClient.BaseAddress` adds the version segment (`/v1/` in production / staging URIs configured by the consumer), so the full URL becomes `{BaseAddress}/{prefix?}/{requestPath}`.

| Service / Method | HTTP | Library request path | Effective URL (after handler) | Pingen Doc anchor (relative to <https://api.pingen.com/documentation>) | Match? | Notes |
|---|---|---|---|---|---|---|
| `BatchService.GetPage` | GET | `batches` | `organisations/{orgId}/batches` | `#tag/Batches/operation/batches.list` | ⚠️ Verify upstream | Round-trip exercised by `BatchServiceTests` (integration); no contradiction. |
| `BatchService.Create` | POST | `batches` | `organisations/{orgId}/batches` | `#tag/Batches/operation/batches.create` | ⚠️ Verify upstream | Sends `DataPost<BatchCreate, BatchCreateRelationships>`; relationship `preset` is sent as `PingenApiDataType.presets` (see `BatchCreateRelationships.cs:49-57`) — see `presets` mapping gap in §4. |
| `BatchService.Get` | GET | `batches/{id}` | `organisations/{orgId}/batches/{id}` | `#tag/Batches/operation/batches.show` | ⚠️ Verify upstream | |
| `DistributionService.GetDeliveryProductsPage` | GET | `distribution/delivery-products` | `organisations/{orgId}/distribution/delivery-products` | n/a — undocumented in public API | 🟡 Undocumented | Documented internally as `unofficial` (AGENTS.md § Known Constraints #2 and `ai-readiness.md § 3.1`). E2E `DistributionGetDeliveryProducts` is the only behavioural source of truth. The library's implicit org-scoping (path is *not* in `NonOrganisationEndpoints`) is currently consistent with WireMock + E2E behaviour. |
| `FilesService.GetPath` | GET | `file-upload` | `file-upload` *(no org prefix)* | `#tag/File-Upload/operation/file-upload.show` | ⚠️ Verify upstream | Skips org prefix via `NonOrganisationEndpoints[0]`. Returns `302`-style location data via the standard JSON:API envelope (response carries `attributes.url` + `attributes.url_signature`). |
| `FilesService.UploadFile` | PUT | (external S3 URL from `GetPath`) | external `https://...amazonaws.com/...` | (external storage; outside Pingen API spec) | ✅ Verified | Goes through `External` `HttpClient` (anonymous, redirects allowed). |
| `LetterService.GetPage` | GET | `letters` | `organisations/{orgId}/letters` | `#tag/Letters/operation/letters.list` | ⚠️ Verify upstream | |
| `LetterService.Create` | POST | `letters` | `organisations/{orgId}/letters` | `#tag/Letters/operation/letters.create` | ⚠️ Verify upstream | Sends `DataPost<LetterCreate, LetterCreateRelationships>`; relationship `preset` is sent as `PingenApiDataType.presets` (see `LetterCreateRelationships.cs:49-57`) — see `presets` mapping gap in §4. |
| `LetterService.Send` | PATCH | `letters/{id}/send` | `organisations/{orgId}/letters/{id}/send` | `#tag/Letters/operation/letters.send` | ⚠️ Verify upstream | Body is `DataPatch<LetterSend>` — letter id is on the data envelope, must match path id. |
| `LetterService.Cancel` | PATCH | `letters/{id}/cancel` | `organisations/{orgId}/letters/{id}/cancel` | `#tag/Letters/operation/letters.cancel` | ⚠️ Verify upstream | Empty body. Returns no payload. |
| `LetterService.Get` | GET | `letters/{id}` | `organisations/{orgId}/letters/{id}` | `#tag/Letters/operation/letters.show` | ⚠️ Verify upstream | |
| `LetterService.Delete` | DELETE | `letters/{id}` | `organisations/{orgId}/letters/{id}` | `#tag/Letters/operation/letters.delete` | ⚠️ Verify upstream | |
| `LetterService.Update` | PATCH | `letters/{id}` | `organisations/{orgId}/letters/{id}` | `#tag/Letters/operation/letters.update` | ⚠️ Verify upstream | |
| `LetterService.GetFileLocation` | GET | `letters/{id}/file` | `organisations/{orgId}/letters/{id}/file` | `#tag/Letters/operation/letters.file` | ⚠️ Verify upstream | Returns `302 Found` — handled via `AllowAutoRedirect=false` + `IsSuccess` short-circuit (`PingenConnectionHandler.cs:374-418`). |
| `LetterService.DownloadFileContent` | GET | (external S3 URL from `GetFileLocation`) | external `https://...amazonaws.com/...` | (external storage; outside Pingen API spec) | ✅ Verified | XML S3 errors decoded into `PingenFileDownloadException`. |
| `LetterService.CalculatePrice` | POST | `letters/price-calculator` | `organisations/{orgId}/letters/price-calculator` | `#tag/Letters/operation/letters.priceCalculator` | ⚠️ Verify upstream | |
| `LetterService.GetEventsPage` | GET | `letters/{id}/events?language={lang}` | `organisations/{orgId}/letters/{id}/events?language={lang}` | `#tag/Letters/operation/letters.events.list` | ❌ Gap (low) | Hard-codes `?language=` into the *path constant* (`LettersEndpoints.Events`) instead of using the standard `apiPagingRequest` query-parameter machinery. Mixing with paging may produce `?language=de&page[number]=1` which is parseable but inconsistent with the rest of the library. Track in `#107` (Fields/Includes/query-string consistency). |
| `LetterService.GetIssuesPage` | GET | `letters/issues?language={lang}` | `organisations/{orgId}/letters/issues?language={lang}` | `#tag/Letters/operation/letters.issues.list` | ❌ Gap (low) | Same hard-coded `?language=` concern as `GetEventsPage`. Track in `#107`. |
| `OrganisationService.GetPage` | GET | `organisations` | `organisations` *(no org prefix)* | `#tag/Organisations/operation/organisations.list` | ⚠️ Verify upstream | Skips org prefix via `NonOrganisationEndpoints[2]`. |
| `OrganisationService.Get` | GET | `organisations/{id}` | `organisations/{id}` *(no org prefix)* | `#tag/Organisations/operation/organisations.show` | ⚠️ Verify upstream | The `requestPath` literally starts with `organisations`, so the `NonOrganisationEndpoints` check skips the auto-prefix and the path is sent verbatim. This is the *intended* behaviour — see `ai-readiness.md § 3.1` "Org-id prefix logic". A single test for `Single("foo")` constructing `organisations/foo` would harden the contract; track in `#106`. |
| `UserService.Get` | GET | `user` | `user` *(no org prefix)* | `#tag/User/operation/user.show` | ⚠️ Verify upstream | |
| `UserService.GetAssociationsPage` | GET | `user/associations` | `user/associations` *(no org prefix)* | `#tag/User-Associations/operation/user.associations.list` | ⚠️ Verify upstream | |
| `WebhookService.GetPage` | GET | `webhooks` | `organisations/{orgId}/webhooks` | `#tag/Webhooks/operation/webhooks.list` | ⚠️ Verify upstream | |
| `WebhookService.Create` | POST | `webhooks` | `organisations/{orgId}/webhooks` | `#tag/Webhooks/operation/webhooks.create` | ⚠️ Verify upstream | |
| `WebhookService.Get` | GET | `webhooks/{id}` | `organisations/{orgId}/webhooks/{id}` | `#tag/Webhooks/operation/webhooks.show` | ⚠️ Verify upstream | |
| `WebhookService.Delete` | DELETE | `webhooks/{id}` | `organisations/{orgId}/webhooks/{id}` | `#tag/Webhooks/operation/webhooks.delete` | ⚠️ Verify upstream | |

### Endpoint coverage gaps observed

The library does **not** currently expose:

- **Webhook update / partial update** (`PATCH webhooks/{id}`) — Pingen documents this; library has no `WebhookService.Update`.
- **Batch update / delete / cancel** workflow — `BatchService` has only `GetPage`, `Get`, `Create`. Pingen documents batch lifecycle operations (`PATCH batches/{id}/send`, `PATCH batches/{id}/cancel`, etc. depending on doc version).
- **Presets endpoints** (`GET organisations/{orgId}/presets`, `GET organisations/{orgId}/presets/{id}`) — `PingenApiDataType.presets` is referenced in relationships but no `IPresetService` exists. See §4 and the `#106` handoff.
- **Letter file events stream / additional `letters/...` sub-resources** — out of scope for this audit beyond the existing `events` and `issues`.

These are coverage gaps, not defects in existing code. They are tracked in `#106` (endpoint coverage extension).

---

## 2. Field constants (`*Fields`) audit

These constants drive sparse-fieldset query construction (`ApiRequest.SparseFieldsets`). Each constant must equal the corresponding `[JsonPropertyName]` on the attributes record. The existing `FieldHelpers` test class (`tests/PingenApiNet.UnitTests/Tests/FieldHelpers.cs`) enforces the constant-to-property correspondence; this audit verifies the inventory is **complete** and notes any drift from the upstream `attributes` payload.

| `*Fields` class | File | Constants count | Internally consistent? | Match against API docs? | Notes / Gaps |
|---|---|---:|---|---|---|
| `BatchFields` | `src/PingenApiNet.Abstractions/Models/Batches/BatchFields.cs` | 13 | ✅ (covered by `FieldHelpers.BatchFields_ConstantsMatchJsonPropertyNames`) | ⚠️ Verify upstream | All 13 properties of `Batch` record have a matching constant. Re-verify the per-attribute set against the published `Batch` schema. |
| `LetterFields` | `src/PingenApiNet.Abstractions/Models/Letters/LetterFields.cs` | 17 | ✅ (covered) | ⚠️ Verify upstream | Comprehensive. Re-verify the upstream `Letter` schema has no additional sparseable fields not yet exposed (e.g., language hints, recipient analytics). |
| `OrganisationFields` | `src/PingenApiNet.Abstractions/Models/Organisations/OrganisationFields.cs` | 13 | ✅ (covered) | ⚠️ Verify upstream | |
| `UserFields` | `src/PingenApiNet.Abstractions/Models/Users/UserFields.cs` | 7 | ✅ (covered) | ⚠️ Verify upstream | |
| `UserAssociationFields` | `src/PingenApiNet.Abstractions/Models/UserAssociations/UserAssociationFields.cs` | 4 | ✅ (covered) | ⚠️ Verify upstream | Light surface — only `role`, `status`, `created_at`, `updated_at`. Confirm the `UserAssociation` resource exposes nothing else sparseable (e.g., a denormalised `organisation_name`). |
| `WebhookFields` | `src/PingenApiNet.Abstractions/Models/Webhooks/WebhookFields.cs` | 3 | ✅ (covered) | ⚠️ Verify upstream | Only `event_category`, `url`, `signing_key`. Confirm whether `secret`, `enabled`, or status fields are also sparseable. |
| `LetterEventFields` | `src/PingenApiNet.Abstractions/Models/LetterEvents/LetterEventFields.cs` | 9 | ✅ (covered) | ⚠️ Verify upstream | |
| `WebhookEventFields` | `src/PingenApiNet.Abstractions/Models/Webhooks/WebhookEvents/WebhookEventFields.cs` | 3 | ✅ (covered) | ⚠️ Verify upstream | |
| `DeliveryProductFields` | `src/PingenApiNet.Abstractions/Models/DeliveryProducts/DeliveryProductFields.cs` | 7 | ✅ (covered) | 🟡 Undocumented | Distribution endpoint is undocumented; field set is "best-known". Re-derive from a live response if Pingen ever publishes the schema. |
| `LetterPriceFields` | `src/PingenApiNet.Abstractions/Models/LetterPrices/LetterPriceFields.cs` | 2 | ✅ (covered) | ⚠️ Verify upstream | Only `currency` and `price`. Confirm whether the price-calculator response exposes a breakdown (line items, taxes) that should also be sparseable. |

### Missing `*Fields` classes

- **No `PresetFields`** — see `presets` mapping gap (§4) and the `#107` handoff.

---

## 3. Include relationship constants (`*Includes`) audit

These constants drive `ApiRequest.Include`. Coverage is enforced by `IncludeHelpers` (`tests/PingenApiNet.UnitTests/Tests/IncludeHelpers.cs`).

| `*Includes` class | File | Constants | Internally consistent? | Match against API docs? | Notes / Gaps |
|---|---|---|---|---|---|
| `BatchIncludes` | `src/PingenApiNet.Abstractions/Models/Batches/BatchIncludes.cs` | `Organisation = "organisation"` | ✅ | ⚠️ Verify upstream | If batches expose `letters` as an include relation (Pingen often does), it is missing here. |
| `LetterIncludes` | `src/PingenApiNet.Abstractions/Models/Letters/LetterIncludes.cs` | `Organisation = "organisation"`, `Batch = "batch"` | ✅ | ⚠️ Verify upstream | Pingen letters are also documented as having `events` as an include relation in some doc versions — confirm and add if applicable. |
| `LetterEventIncludes` | `src/PingenApiNet.Abstractions/Models/LetterEvents/LetterEventIncludes.cs` | `Letter = "letter"` | ✅ | ⚠️ Verify upstream | |
| `UserAssociationIncludes` | `src/PingenApiNet.Abstractions/Models/UserAssociations/UserAssociationIncludes.cs` | `Organisation = "organisation"` | ✅ | ⚠️ Verify upstream | If associations expose `user` as an include, it is missing. |
| `WebhookIncludes` | `src/PingenApiNet.Abstractions/Models/Webhooks/WebhookIncludes.cs` | `Organisation = "organisation"` | ✅ | ⚠️ Verify upstream | |

### Missing `*Includes` classes

- **No `OrganisationIncludes`, `UserIncludes`, `WebhookEventIncludes`, `DeliveryProductIncludes`, `LetterPriceIncludes`, `PresetIncludes`.** Some of these resource types may not have any documented include relations (e.g., `LetterPrice`); others (e.g., `Organisation` exposing `users`, `WebhookEvent` exposing `letter`) likely do. Track in `#107`.

---

## 4. Enum completeness audit

| Enum | File | Values | Status | Notes / Gaps |
|---|---|---|---|---|
| `PingenApiCurrency` | `src/PingenApiNet.Abstractions/Enums/Api/PingenApiCurrency.cs` | `EUR`, `CHF` | ❌ Gap (medium) | Line 46 carries `// TODO: Missing API Doc about currencies` — list is acknowledged as incomplete. The Pingen public API may accept additional currencies (typically `USD`, `GBP` as common payment currencies); add only the values Pingen actually documents to avoid divergence. Track in `#108`. |
| `PingenApiDataType` | `src/PingenApiNet.Abstractions/Enums/Api/PingenApiDataType.cs` | 18 values: `letters`, `batches`, `organisations`, `letter_price_calculator`, `letters_events`, `users`, `associations`, `webhooks`, `file_uploads`, `webhook_issues`, `webhook_sent`, `webhook_undeliverable`, `delivery_products`, `presets`, `webhook_delivered`, `deliverables_events`, `emails`, `ebills` | ❌ Gap (high) | `presets` is enumerated and used in `LetterCreateRelationships.cs:56` and `BatchCreateRelationships.cs:56` to send a preset id, but no `Preset` model exists and the value is **not** registered in `PingenSerialisationHelper.PingenApiDataTypeMapping`. As a result, any Pingen response with `included.[].type == "presets"` will be **silently skipped** by `IncludedCollection.OfType<Preset>()` / `FindById<Preset>()`. The `PingenApiDataTypeMappingTests` regression test surfaces this gap explicitly via the `KnownUnmappedDataTypes` allow-list. Track in `#106` (model + service) and `#108` (mapping wiring once `Preset` exists). `webhook_channel_subscriptions` is **not** enumerated — see the addendum. `deliverables_events` is enumerated **and** mapped to `LetterEvent` alongside `letters_events`; `emails` / `ebills` are enumerated but unmapped until those channels are modelled — see the 2026-07-29 addendum. Members carry explicit numeric values and are append-only (public ABI). |
| `PingenApiDataTypeMapping` | `src/PingenApiNet.Abstractions/Helpers/PingenSerialisationHelper.cs:116-132` | 15 entries | ❌ Gap (high, related) | Per-call allocation (`=> new { … }` getter) — flagged previously in `ai-readiness.md § 3.3` "allocates on every access". Trivial fix to `static readonly`. Track in `#108`. The completeness regression is covered by `PingenApiDataTypeMappingTests`. |
| `WebhookEventCategory` | `src/PingenApiNet.Abstractions/Enums/Api/WebhookEventCategory.cs` | 4 values: `issues`, `undeliverable`, `sent`, `delivered` | ⚠️ Deliberately incomplete (1 of 5) | The spec's `event_category` enum (`WebhookCreatePOST`) has five values, confirmed by the live API's own validation error: `Possible values: issues, sent, undeliverable, delivered, channel_subscriptions`. `delivered` is added here because its webhook body binds losslessly to the existing `WebhookEvent` model. `channel_subscriptions` is **omitted on purpose** — the library cannot yet represent its body, and enumerating the category would let a consumer subscribe to events it would then silently mis-parse. See the addendum. |

### Reflection test coverage

`tests/PingenApiNet.UnitTests/Tests/Helpers/PingenApiDataTypeMappingTests.cs` asserts:

1. Every `PingenApiDataType` enum value is either registered in `PingenApiDataTypeMapping` **or** explicitly listed in a `KnownUnmappedDataTypes` allow-list. The allow-list contains `PingenApiDataType.presets` (tracking `#106` / `#108`) and `PingenApiDataType.emails` / `PingenApiDataType.ebills` (tracking `#125` — enumerated so the `deliverable` relationship binds, unmapped until the email and ebill channels are modelled).
2. Every mapped CLR `Type` is non-null and implements `IAttributes` (so `IncludedCollection.OfType<T>` works).
3. The allow-list does not drift from the enum: every entry must be a real enum value, and any value that gains a mapping must be removed from the allow-list (preventing the audit from going stale).

Test confirmed RED-then-GREEN: with the allow-list emptied, the first assertion fails with `unmapped should be empty but had 1 item and was [PingenApiDataType.presets]`.

### Addendum (2026-07-12) — an unknown top-level `data.type` is fatal, not skippable

`IncludedCollection.OfType<T>()` is tolerant of an unknown `included[].type` (it `Enum.TryParse`s and `continue`s, so the resource is silently skipped — that is the "silent skip" gap above). **The top-level `data.type` is not.** It binds to `DataIdentity.Type`, a non-nullable `PingenApiDataType` carrying `[JsonConverter(typeof(JsonStringEnumConverter<PingenApiDataType>))]`, so an unrecognised discriminator **throws** `JsonException` out of `PingenSerialisationHelper.Deserialize<SingleResult<WebhookEventData>>` inside `PingenWebhookHelper.ValidateWebhookAndGetData` — *after* the HMAC signature has already validated.

Reproduced against the published 1.2.5 with a spec-shaped `webhook_delivered` body:

```
JsonException: The JSON value could not be converted to
PingenApiNet.Abstractions.Enums.Api.PingenApiDataType. Path: $.data.type
```

Consequence for consumers: a webhook subscription for a category whose discriminator the library does not know returns HTTP 5xx/422 for **every** delivery, Pingen retries and then permanently drops the event. That is the bug `webhook_delivered` fixes.

**Enumerating the type is necessary but not sufficient.** `ValidateWebhookAndGetData` binds every body to `WebhookEventData` (= `Data<WebhookEvent, WebhookEventRelationships>`) *unconditionally* — it does not branch on the discriminator, and `PingenApiDataTypeMapping` governs only the `included[]` array, never the top-level `data`. So adding an enum value without a model that fits the body does not make the category "tolerated"; it converts a loud `JsonException` into silent corruption:

- attributes with no counterpart on `WebhookEvent` are dropped without warning;
- `WebhookEventRelationships.Letter` / `.Event` are declared **non-nullable** but `RespectNullableAnnotations` is not set on the cached `JsonSerializerOptions`, so a body without those relationships writes `null` into them and the consumer NREs on first dereference;
- a body with no `relationships` key at all still throws, because `Data.Relationships` is `required` — so the failure mode is not even consistent.

This is why **`webhook_delivered` is enumerated and `webhook_channel_subscriptions` is not**:

| | `webhook_delivered` | `webhook_channel_subscriptions` |
|---|---|---|
| Attributes (spec) | `url`, `created_at` | `identifier`, `email`, `name`, `address`, `status`, `approved_at`, `url`, `created_at` |
| Relationships (spec) | `organisation`, `letter`, `event` | `organisation`, `channel_ebill` (**no letter**) |
| Fits `WebhookEvent` / `WebhookEventRelationships`? | **Yes** — identical to `webhook_sent`, binds losslessly with a null `Reason` | **No** — 6 attributes dropped, `Letter`/`Event` null in non-nullable properties |
| Shipped? | ✅ enumerated + mapped | ❌ deliberately absent, in both `PingenApiDataType` and `WebhookEventCategory` |

Follow-up (needs a tracking issue): add `WebhookChannelSubscription : IAttributes` + a `WebhookChannelSubscriptionsRelationships` type, have `ValidateWebhookAndGetData` dispatch on `data.type` instead of blind-binding, then enumerate `channel_subscriptions` / `webhook_channel_subscriptions` and register the mapping. Until then, a consumer that creates a `channel_subscriptions` subscription out-of-band (e.g. in the Pingen web app) still gets the `JsonException` above — loud and retried, rather than silently wrong.

Regression coverage: `PingenWebhookHelperTests.ValidateWebhookAndGetData_WebhookDeliveredPayload_DeserializesWithoutLoss` (against `Assets/webhook_delivered_sample.json`, the real body shape — attributes `url` + `created_at`, no `reason`), plus `webhook_delivered` in the `ForAllEventTypes` matrix and the `webhook_delivered → WebhookEvent` assertion in `PingenSerialisationHelperTests.PingenApiDataTypeMapping_AllWebhookCategoriesMapToWebhookEvent`.

### Addendum (2026-07-29) — the deliverable rollout: outage fix + full spec re-alignment

On **2026-07-27** Pingen rolled out a "deliverable" abstraction over letters, emails and ebills (the spec's `Webhook*GET` schemas were renamed `WebhookDeliverable*GET`, and `LetterEventRelatedSingleOutput` became `DeliverableEventLuceneRelatedSingleOutput`). The wire consequence, unannounced and with no version gate:

```
data.relationships.event.data.type:  "letters_events"  →  "deliverables_events"
```

for **every** event category. `RelatedSingleOutput.Data.Type` is the same non-nullable `PingenApiDataType` as the top-level discriminator, so the addendum above applies verbatim one level deeper — `JsonException` after the HMAC has validated, the consumer answers 4xx/5xx, Pingen dead-letters the event after 7 attempts over ~7 h and mails the organisation a "Webhook Zustellung Fehlgeschlagen" digest:

```
JsonException: The JSON value could not be converted to
PingenApiNet.Abstractions.Enums.Api.PingenApiDataType.
Path: $.data.relationships.event.data.type
```

Observed in production on 1.2.5 and reproduced on 1.3.0-rc-1: last successful delivery 2026-07-27 13:42 CEST, first failure 17:02 CEST, then 100 % failure (626 deliveries in the following 48 h on one consumer alone). Both the `sent` and `undeliverable` categories were affected; the fix is `deliverables_events = 15`.

**Pingen kept the legacy shape alongside the new one**, which is what makes the fix a one-liner *and* sets the trap:

| | before 2026-07-27 | after |
|---|---|---|
| `relationships.letter` | present | **still present** (unchanged) |
| `relationships.deliverable` | — | added (`data.type` = `letters`) |
| `relationships.event.data.type` | `letters_events` | **`deliverables_events`** ← the only breaking change |
| `included[]` event resource | one, typed `letters_events` | **two**, typed `letters_events` *and* `deliverables_events`, sharing one id |

Both the legacy relationship and the legacy include survived the rollout, which is what kept the blast radius to a single unknown discriminator. **They are not part of the contract any more, though**: `WebhookDeliverableSentGET` and its three siblings declare `required: [organisation, deliverable, event]` and no longer list `letter` as a property at all. The wire is running ahead of the spec in one direction and behind it in the other, so the library is fixed for both shapes rather than for the one currently on the wire.

### What this PR changes

**Discriminators.** `deliverables_events = 15`, plus `emails = 16` and `ebills = 17` — the latter two because `DeliverableRelatedSingleOutput.data.type` is `enum: [letters, emails, ebills]`, so binding the new `deliverable` relationship without them would reproduce this outage verbatim for any organisation that sends on those channels.

**Event resolution.** `deliverables_events` maps to `LetterEvent` *alongside* `letters_events` — the spec's `DeliverableEventLuceneAttributes` and `LetterEventEloquentAttributes` are field-for-field identical, so one CLR type backs both. The duplicate that produces meanwhile is handled where it belongs, in `IncludedCollection.OfType<T>()`, which now collapses a resource appearing under two *different* discriminators with one shared id. The collapse is deliberately narrow: repeats under the *same* discriminator are still returned in full (`OfType_DuplicateEntries_ReturnsAllInsertionOrder` pins that), because those are malformed API responses rather than migration artefacts.

The alternative — enumerating `deliverables_events` but leaving it unmapped, so `letters_events` stays the single binding — was rejected. It works only for as long as Pingen keeps sending the legacy copy, and it fails *silently* when that stops: `TryGetIncludedData` returns `false`, the letter event comes back `null`, and consumers get an `NullReferenceException` somewhere downstream instead of an error at the parse boundary. Verified by sabotage: removing the mapping entry fails `ValidateWebhookAndGetData_LegacyLetterShapeDropped_StillResolvesEverything`.

**Relationships.** `WebhookEventRelationships` gains a nullable `Deliverable` and `Letter` becomes nullable; `LetterEventRelationships` gets the same treatment, because `DeliverableEventLuceneGETListItem` — already backing the emails and ebills event endpoints — declares `deliverable` where `LetterEventEloquentGETListItem` declares `letter`. Both new parameters are appended with `= null` defaults, so existing positional construction still compiles. Prefer `Deliverable`, fall back to `Letter`.

**Attributes that changed with the rollout.** `BatchAttributes` gained `channel_type` (`post` | `ebill` | `email`) and `deliverable_count`, the channel-agnostic counterpart to `letter_count`. Both were being dropped on the floor.

**Attributes that were already being dropped** (pre-dating the rollout, previously tracked as non-urgent in #125, closed here so the client is aligned end-to-end): `WebhookEvent.corrected_address` — spec-required on every `undeliverable` webhook and the most damaging of the set, since it is the address Pingen worked out for a failed delivery — plus `Letter.source`, `Batch.source`, `Organisation.{edition, flags, missing_credits, limits_monthly_letters_count, limits_monthly_emails_count, limits_monthly_ebills_count}` and `User.{edition, flags}`.

New string-constant classes `LetterSources`, `BatchSources` and `BatchChannelTypes` accompany the `source` / `channel_type` fields. They are `const string` sets rather than C# enums **on purpose**: a strictly-bound enum turns an unannounced new value into an unparseable response, which is the failure this whole addendum is about. `Letter.Status` already follows that pattern.

Every attributes model in the library now matches its spec schema exactly. The only intentional divergence is `WebhookEvent`, which is a superset: one model backs all four event categories, so `reason` and `corrected_address` are present-but-null on the categories that do not send them.

### Scope check — the polling path

`GET /organisations/{org}/deliveries/letters/{letterId}/events` returns `data[].type == "letters_events"` live, and the spec agrees: it resolves to `LetterEventEloquentGETList`, whose items are typed `letters_events`. (An earlier draft of this addendum claimed the spec had already flipped that endpoint to `deliverables_events` — it has not. The `deliverables_events` examples belong to `DeliverableEventLucene*` and to the emails/ebills event endpoints.) Enumerating the value and mapping both discriminators covers the flip in advance either way.

### Regression coverage

| Test | Asset | Pins |
|---|---|---|
| `ValidateWebhookAndGetData_DeliverablesEventRelationship_DeserializesAndResolvesSingleEvent` | `webhook_sent_deliverables_sample.json` — verbatim anonymised capture from `GET /organisations/{org}/webhooks/{id}/requests` | today's wire shape: new discriminator, both relationships, duplicated include |
| `ValidateWebhookAndGetData_LegacyLetterShapeDropped_StillResolvesEverything` | `webhook_undeliverable_deliverable_only_sample.json` — **constructed from the spec, not captured** | the end state: no `letter`, no legacy include, `corrected_address` present |
| `OfType_SameResourceUnderTwoDiscriminators_CollapsesToOne` / `OfType_DistinctResourcesUnderTwoDiscriminators_ReturnsBoth` | inline | the collapse, and that it does not swallow genuinely distinct events |

Verified RED-then-GREEN on all three failure modes:

| Sabotage | Result |
|---|---|
| Make the discriminator unknown (simulates the pre-fix library) | `JsonException … Path: $.data.relationships.event.data.type` — the exact production error |
| Drop `deliverables_events` from the mapping | `LegacyLetterShapeDropped` fails — the silent-null future outage |
| Remove the cross-discriminator collapse | `InvalidOperationException: Sequence contains more than one element` on today's shape |

### Still open

`channel_subscriptions` remains unenumerated in `WebhookEventCategory`, and `webhook_channel_subscriptions` unenumerated in `PingenApiDataType`, for the reason given in the 2026-07-12 addendum: its body has a different attribute surface and different relationships (`organisation` + `channel_ebill`, no `deliverable`, no `event`), so it needs its own model rather than a discriminator entry. A webhook of that category created through the Pingen web app will still fail to deserialize. Tracked in #125.


---

## 5. Sub-issue handoff lists

These are concrete, actionable items grouped for follow-up sub-issues. Issue numbers are taken from the parent epic plan — confirm with the epic owner before opening PRs against each.

> The audit cannot reach the live `https://api.pingen.com/documentation` site. Every "⚠️ Verify upstream" row above lands in **#106** unless it more naturally fits **#107** (constants), **#108** (enums/mapping), or **#110** (cross-cutting/test-coverage).

### Handoff to `#106` — Endpoint coverage & verification

- **Live-doc verification pass**: walk every "⚠️ Verify upstream" row in §1 against the published Pingen v2.0.0 docs and either upgrade to ✅ or open a new issue.
- **Webhook update endpoint**: add `WebhookService.Update(DataPatch<WebhookUpdate> data, …)` mapping to `PATCH webhooks/{id}` if Pingen exposes it.
- **Batch lifecycle endpoints**: extend `IBatchService` with `Send`, `Cancel`, `Update`, `Delete` (subject to upstream availability).
- **Preset endpoints + service**: introduce `IPresetService` with `GetPage`, `Get`, `Create`, `Update`, `Delete` against `organisations/{orgId}/presets`. Requires the new `Preset` model (handed to `#108`).
- **Org-prefix unit tests**: add a `PingenConnectionHandler`-level unit test asserting `file-upload`, `user`, `organisations` (and any future `NonOrganisationEndpoints` entry) construct un-prefixed URLs, and that all other endpoints prepend `organisations/{orgId}/`. Closes the gap noted in `ai-readiness.md § 2.3`.

### Handoff to `#107` — `*Fields` / `*Includes` / query-string consistency

- **Verify each `*Fields` class exposes the full sparseable attribute set** of its resource (cross-check with live `attributes` response shape). Likely additions: `Webhook` may expose `enabled`, `secret`, `created_at`, `updated_at`; `LetterPrice` response may include line-item breakdown.
- **Add missing `*Includes` classes** for resources that have documented include relations: `OrganisationIncludes`, `UserIncludes` (probably `organisations`), `WebhookEventIncludes`, possibly `DeliveryProductIncludes` and `BatchIncludes.Letters`.
- **Add `PresetFields` / `PresetIncludes`** once the `Preset` attributes record exists (depends on `#108`).
- **Refactor `LettersEndpoints.Events` and `LettersEndpoints.Issues`** to stop hard-coding `?language={lang}` in the path constant. Move `language` to an additional query parameter on `ApiPagingRequest` (or a per-method override) so paging interaction is explicit and the standard `ApiQueryParameterNames` machinery applies. Maintain backwards compatibility on the public service signatures.

### Handoff to `#108` — Enum completeness & mapping wiring

- **`PingenApiCurrency`**: resolve the line-46 TODO by adding only the currencies Pingen documents. If documentation is silent, leave as-is and update the comment to point at this audit document.
- **`PingenApiDataTypeMapping` static-cache**: convert from expression-bodied property getter (`=> new { … }`) to `public static readonly Dictionary<PingenApiDataType, Type>`. Trivial perf win flagged in `ai-readiness.md § 3.3`.
- **Add `Preset` attributes record + `[PingenApiDataType.presets] = typeof(Preset)`** in `PingenSerialisationHelper.PingenApiDataTypeMapping`. Once added, **delete** the `PingenApiDataType.presets` entry from `KnownUnmappedDataTypes` in `PingenApiDataTypeMappingTests` — the third test (`KnownUnmappedDataTypes_StaysConsistentWithEnumAndMapping`) is the watchdog that ensures this cleanup is not forgotten.

### Handoff to `#110` — Cross-cutting test/doc tasks

- **Live-doc spot-check follow-up**: turn the §1 / §2 / §3 "⚠️ Verify upstream" rows into a checklist that one human + AI pair can clear in a single session against the published docs.
- **Optional: add an integration test that captures the full outbound query string** for a `Letters.GetPage` call combining sparse fieldsets + `Include` + filter + sort. Catches future drift in `LettersEndpoints.Events` / `Issues` once they are fixed under `#107`.
- **Refresh `ai-readiness.md § 3.3`** to reflect the now-closed coverage gaps (notably the `PingenApiDataTypeMapping` reflection test).
- **Distribution endpoint posture**: write a short ADR or note describing the policy for un-documented endpoints (graceful failure, no critical-path dependency) so future endpoints follow the same pattern.

---

## Methodology note

This audit was produced by an autonomous developer agent operating in a sandboxed environment without internet access. Every "Verified" claim is supported by source code or existing test coverage in this repository; every "Verify upstream" row requires a manual or AI-assisted pass against the live Pingen API documentation before being upgraded. No `src/**` files were modified; the only behavioural change is the addition of `PingenApiDataTypeMappingTests.cs`, which formalises the `PingenApiDataTypeMapping` completeness invariant called out in `ai-readiness.md § 3.1`.
