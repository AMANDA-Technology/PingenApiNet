---
title: AI Readiness Assessment — PingenApiNet
tags: [ai, readiness, assessment, onboarding]
date: 2026-04-19
---

# AI Readiness Assessment

This document scores how well-prepared the PingenApiNet codebase is for autonomous AI-team work. It covers documentation quality, test coverage, fragile or dangerous areas, and a backlog of proposed improvements discovered during exploration.

See also:
- [[CLAUDE|../CLAUDE]] — AI-agent project instructions at the repo root
- [[architecture/README]] — C4 architecture overview
- [[architecture/glossary]] — Domain terminology

---

## 1. Documentation Quality

### 1.1 What Exists and Is Useful

| Artifact | Path | Usefulness for AI |
|---|---|---|
| Project instructions | [[../CLAUDE\|CLAUDE.md]] | **High** — names tech stack, solution layout, dependency graph, build/test commands, key conventions (records, `[JsonPropertyName]`, XML doc comments, nullable enabled), important file locations, and documented gotchas. Reading this alone is enough to orient a competent worker agent. |
| C4 Level 1 context | [[architecture/context]] | **High** — actors, external systems (Pingen API, Identity, S3 storage, NuGet.org), staging vs production URIs. Mermaid `C4Context` renders in most renderers. |
| C4 Level 2 containers | [[architecture/containers]] | **High** — the three NuGet packages + three test projects (Unit, Integration, E2E) with responsibilities, references, and a `C4Container` mermaid diagram. Now covers the WireMock.Net integration tier. |
| C4 Level 3 components (Core) | [[architecture/components/pingenapi-core]] | **High** — component diagram + per-component narrative covering `PingenApiClient`, `PingenConnectionHandler`, `PingenHttpClients`, `ConnectorService`, and the connector services with representative endpoints. |
| C4 Level 3 components (Abstractions) | [[architecture/components/pingenapi-abstractions]] | **High** — full package tree, data-model hierarchy, `IncludedCollection` / `OfType<T>` / `FindById<T>` semantics, helper responsibilities. |
| ADR-001 | [[architecture/decisions/001-json-api-records]] | **High** — records + `System.Text.Json` rationale, `[JsonPropertyName]` enforcement, nullable reference types, reflection usage. Includes post-original addendum for the now-cached `SerializerOptions` and the `IncludedCollection` evolution. |
| ADR-002 | [[architecture/decisions/002-static-access-token]] | **High** — the multi-tenant security fix (issue #22): `_accessToken` + `_authenticationSemaphore` per instance. Flags the filename-vs-content drift (file still named `002-static-access-token.md`) and cross-links the regression tests. |
| ADR-003 | [[architecture/decisions/003-iasyncenumerable-pagination]] | **High** — two-variant API (`GetPage` vs `GetPageResultsAsync`), `[EnumeratorCancellation]` on the async-enumerable, linear `page[number]` increment caveat. |
| ADR-004 | [[architecture/decisions/004-three-http-clients]] | **High** — named clients `Pingen.Identity` / `Pingen.Api` / `Pingen.Files` with distinct configurations. Now flags the shared `DefaultRequestHeaders.Authorization` hazard for in-process multi-tenancy. |
| ADR-005 | [[architecture/decisions/005-wiremock-integration-tests]] | **High** — rationale, alternatives, and consequences for the WireMock.Net integration-test tier introduced in the refresh. |
| Glossary | [[architecture/glossary]] | **High** — Pingen domain terms (Organisation, Letter, Batch, DeliveryProduct, FileUrlSignature, Preset, Webhook, etc.), JSON:API protocol terms, library-specific terms (`ApiResult`, `ConnectorService`, `NonOrganisationEndpoints`, `IncludedCollection`). |
| README.md | `README.md` | **Medium-high** — 8 usage samples (create/send letter, download file, delivery products, sort+filter, DI, webhooks, include parameter, sparse fieldsets). Good onboarding reading for humans; slightly noisy for AI because some samples mix Newtonsoft (`JsonConvert.SerializeObject`) with System.Text.Json. |
| In-code XML doc comments | `src/**/*.cs` | **High** — enforced project-wide via `<GenerateDocumentationFile>true</GenerateDocumentationFile>`. Public API surface has `/// <summary>` comments; most private members also. Parameter names are consistently descriptive. |

### 1.2 What Is Missing or Thin

| Gap | Impact | Suggested Remediation |
|---|---|---|
| No top-level `CONTRIBUTING.md` | Low — small project, conventions are implicit in `CLAUDE.md`. | Not required. |
| No versioning / release notes | Medium — CI publishes on tag, but there is no `CHANGELOG.md` capturing per-version changes. | Adopt Keep-a-Changelog; generate entries at release time. |
| No explicit architecture **deployment** doc | Low — the library is consumed as NuGet packages; there is no runtime deployment per se. | Not required. |
| No dedicated "how to add a new connector endpoint" runbook | Medium — adding an endpoint touches `Abstractions` (model + `*Fields` + `*Includes`), `PingenApiNet` (service + `*Endpoints`), `PingenApiNet.AspNetCore` (DI), `PingenSerialisationHelper.PingenApiDataTypeMapping`, and the three test projects. An AI agent can infer the pattern from existing endpoints, but an explicit checklist would eliminate drift. | See **Backlog** item "Runbook: adding a connector endpoint". |
| No documented Pingen API version pinning strategy | Medium — README mentions API 2.0.0 but there is no explicit compatibility matrix or deprecation plan. | See **Backlog** item "Pingen API version policy". |
| Sample tests for `IncludedCollection.FindById<T>` with `UserAssociation` / ambiguous types | Low — tests cover `Organisation` / `Letter`, but `UserAssociation` is mapped in `PingenApiDataTypeMapping` without test coverage for `FindById`. | Minor — covered by `OfType<T>` tests indirectly. |

### 1.3 Rating: **Ready**

The documentation is complete enough for an AI agent to reason about the system, pick a task, and deliver without asking clarification questions. The C4 docs + ADRs + `CLAUDE.md` triangulate each other well. Minor drift (the superseded "static" in ADR-002 filename, some edge-case cross-references) is annotated, not hidden.

---

## 2. Test Coverage

### 2.1 Frameworks and Commands

**Testing stack**

- **NUnit 4** — test runner, `[Test]` / `[TestFixture]` / `[SetUp]` / `[TearDown]`.
- **Shouldly 4.3** — assertions (`ShouldBe`, `ShouldSatisfyAllConditions`, `Should.Throw`, `Should.ThrowAsync`).
- **NSubstitute 5.3** — mocking (unit-test connectors, mock `IPingenConnectionHandler`).
- **WireMock.Net 1.7** — in-process HTTP stub server (integration tests only).
- **Bogus 35.5** — realistic test data generation (Integration and E2E tests).
- **coverlet.collector 8.0** — coverage collection.
- **NUnit.Analyzers 4.12** — static analysis for NUnit patterns.

**Commands** (copy-paste ready from repo root):

```bash
# Fast local loop — unit tests only (~secs). No network, no env vars.
dotnet test tests/PingenApiNet.UnitTests/PingenApiNet.UnitTests.csproj

# Offline integration tests (WireMock in-process, no network).
dotnet test tests/PingenApiNet.Tests.Integration/PingenApiNet.Tests.Integration.csproj

# Unit + integration combined (recommended default for AI workers).
dotnet test PingenApiNet.sln --filter "FullyQualifiedName!~PingenApiNet.Tests.E2E"

# Live E2E tests (requires staging credentials — see § 2.4).
dotnet test tests/PingenApiNet.Tests.E2E/PingenApiNet.Tests.E2E.csproj

# Everything (unit + integration + E2E).
dotnet test PingenApiNet.sln

# With coverage (XPlat Code Coverage via coverlet.collector).
dotnet test PingenApiNet.sln --collect:"XPlat Code Coverage"
```

### 2.2 What IS Covered

**`PingenApiNet.UnitTests`** (~240 tests across 21 test files, ~3,400 LOC)

| Area | Coverage |
|---|---|
| `PingenConnectionHandler` (Services/PingenConnectionHandlerTests.cs — 572 LOC, 17 tests) | Constructor guards, `SetOrganisationId`, `GetAsync` happy-path, authentication failure (with and without deserialisable error body), rate-limit header parsing, missing-headers-default-values, `Retry-After` header, 429 response, 401 response, multi-tenant token isolation regression (#22), concurrent-login double-check regression (#27), expired-token re-auth. Uses `MockHttpMessageHandler` for deterministic responses. |
| `PingenApiClient` facade (Services/PingenApiClientTests.cs) | Delegates to connector services; `SetOrganisationId` forwarding. |
| Connector services (Services/Connectors/*) | One test file per connector (Batches, Distribution, Files, Letters, Organisations, Users, Webhooks, `ConnectorService` base). Tests use `Substitute.For<IPingenConnectionHandler>()` to verify each service calls `GetAsync` / `PostAsync` / `PatchAsync` / `DeleteAsync` with the correct URL path, HTTP verb, and payload type. |
| `IncludedCollection` (Models/IncludedCollectionTests.cs — 341 LOC, 15 tests) | `Empty` singleton, deserialisation of empty / missing / null `included`, `OfType<T>` with matching / non-matching / unknown-discriminator / multiple-match cases, `FindById` with matching / wrong-id / wrong-type cases, `Count`, `RawItems.IsReadOnly`, round-trip via `CollectionResult`. |
| Serialization helpers (Helpers/*) | `PingenSerialisationHelper`, `PingenWebhookHelper` (valid / invalid signature / invalid hex / throws on data mismatch), `PingenAttributesPropertyHelper`, both `PingenDateTimeConverter` variants, `PingenKeyValuePairStringObjectConverter` (nested filter expressions). |
| Models (Models/*) | `ApiResult` shape, `DataPost` / `DataPatch` envelope, `ExternalRequestResult`, `PingenConfiguration` normalization. |
| Exceptions (Exceptions/PingenExceptionTests.cs) | All three Pingen exception types — constructor variants, message preservation, `ApiResult` preservation. |
| Enums (Enums/BatchIconSerializationTests.cs) | Hyphenated enum-member serialisation (e.g., `BatchIcon.waveHand` → `"wave-hand"`). |
| `ApiRequest` / paging (ApiRequestQueryParameters.cs + SparseFieldsets.cs + IncludeHelpers.cs + FieldHelpers.cs) | Query-parameter formatting, `*Includes` / `*Fields` constant-vs-JsonPropertyName consistency (catches drift when a model gains/renames a property without updating the sibling helpers). |
| DI (AspNetCore/PingenServiceCollectionTests.cs) | `AddPingenServices` registers all nine public interfaces. |
| Webhooks (Webhooks.cs) | Offline deserialization of `Assets/webhook_sample.json` + `TryGetIncludedData` for Organisation / Letter / LetterEvent. |

**`PingenApiNet.Tests.Integration`** (8 test files, all use real `PingenApiClient` + WireMock)

| Fixture | Coverage |
|---|---|
| `PingenApiClientTests` | Facade-level: all connector properties are non-null; `SetOrganisationId` routes subsequent requests to the new org path. |
| `LetterServiceTests` | `GetPage`, `GetPageResultsAsync` with two-page scenario, `Create`, `Send`, `Cancel`, `Get`, `Delete`, `Update`, `GetFileLocation` (302 + Location header), `DownloadFileContent` (external URL to WireMock), `CalculatePrice`, `GetEventsPage` + `GetEventsPageResultsAsync`, `GetIssuesPage` + `GetIssuesPageResultsAsync`. |
| `WebhookServiceTests` | `GetPage`, paginated `GetPageResultsAsync`, `Create`, `Get`, `Delete`. |
| `BatchServiceTests`, `DistributionServiceTests`, `FilesServiceTests`, `OrganisationServiceTests`, `UserServiceTests` | Per-connector round-trips: list / get / (create) / (delete) as the connector supports. |

**`PingenApiNet.Tests.E2E`** (4 fixtures, live staging)

| Fixture | Coverage |
|---|---|
| `LettersGetAll` | Iterates all letters via auto-pagination against staging. |
| `FileUpload` | Full `Files.GetPath` → `UploadFile` → `Letters.Create` → `Letters.Get` → `Letters.Send` sequence. |
| `DistributionGetDeliveryProducts` | Hits the undocumented `/distribution/delivery-products` endpoint. |
| `RateLimit` | Deliberately exceeds Pingen rate limits to verify `Retry-After` handling end-to-end (parallel load). |

### 2.3 What Is NOT Covered

| Gap | Impact | Notes |
|---|---|---|
| **`IncludedCollection.FindById<T>` with `UserAssociation` / heterogeneous-type arrays** | Low | `OfType<T>` covers the mapping logic, but `FindById` has no test specifically for resource types beyond `Organisation` and `Letter`. |
| **`FilesService.UploadFile` stream-handling in unit tests** | Low | Integration test exists. No offline unit test for stream-disposal / large-stream semantics. |
| **`PingenConnectionHandler` path construction for `NonOrganisationEndpoints` edge cases** | Medium | Unit tests cover auth + rate-limit + error paths but do not directly assert that `file_uploads`, `users`, and `organisations` skip the org-id prefix. The integration tests cover the happy path for these endpoints implicitly. A focused unit test would catch a future regression where someone adds a new `NonOrganisationEndpoints` entry incorrectly. |
| **Idempotency-Key header round-trip** | Medium | `PingenConnectionHandler` sets the `Idempotency-Key` header when provided, but there is no test asserting a provided key arrives in the outbound request. `IdempotentReplayed` response-header parsing is implicitly covered by shape tests. |
| **Filter-and-sort query string integration** | Medium | `PingenKeyValuePairStringObjectConverter` is unit-tested and `ApiPagingRequest` has serialisation tests, but there is no integration test that sends a filter+sort request to WireMock and asserts the exact query string that arrives. |
| **`SparseFieldsets` with multiple types + `Include` combined** | Low | README example shows combining them. Offline tests cover each independently; no integration test exercises both together end-to-end. |
| **Distribution service (undocumented endpoint)** | Low | Integration test exists with stubbed response shape. The shape itself is unverified against live Pingen — the E2E test (`DistributionGetDeliveryProducts`) is the only source of truth. |
| **Webhook signing-key rotation** | Low | `PingenConfiguration.WebhookSigningKeys` is a `Dictionary<string, string>?` but there is no test or sample showing how multiple keys are used (rotation scenario). `PingenWebhookHelper.ValidateWebhook` takes a single key. |
| **Retry / back-off strategy for transient 5xx** | Medium (product decision) | There is currently no built-in retry. Rate-limit (429) exposure is surfaced via `ApiResult.RetryAfter`, and the E2E `RateLimit` fixture validates the header. Polly or a manual retry loop is left to consumers. |
| **Telemetry / logging** | Medium (product decision) | `PingenConnectionHandler` has no `ILogger` dependency. No request/response diagnostics. Consumers wrap with their own observability. |
| **Thread-safety of `PingenConfiguration.Normalize()`** | Low | `Normalize()` is called once in the constructor; mutation of `WebhookSigningKeys` after construction is not guarded. In practice the DI registration is `Singleton` (configuration) + `Scoped` (handler), so this is fine. Worth documenting. |

### 2.4 Quality Assessment

Tests are **meaningful, not superficial**:

- Unit tests use real serialisers (`PingenSerialisationHelper.Deserialize<T>`) against real JSON bodies — they exercise the converter chain end-to-end. They do not assert getter/setter round-trips in isolation.
- Integration tests stand up the real `PingenApiClient` and verify full HTTP round-trips through WireMock. Auto-pagination tests use WireMock's state machine to return different payloads on page 1 vs page 2, proving the loop terminates correctly.
- Regression tests for issues #22 (multi-tenant token leak) and #27 (double-check concurrent login) live in `PingenConnectionHandlerTests` and explicitly exercise the racy paths.
- `FieldHelpers` / `IncludeHelpers` tests use reflection to assert that every `*Fields` / `*Includes` constant matches the `[JsonPropertyName]` on the corresponding model property — this catches drift when a model gains a field without the helpers being updated.
- Shouldly's `ShouldSatisfyAllConditions` is used liberally so failed assertions report the complete failure set instead of stopping at the first.

### 2.5 Rating: **Good Coverage**

Across all three tiers, the library exercises its public surface area, its OAuth + HTTP plumbing, its JSON:API envelope handling, and its auto-pagination loop. The two known multi-threading regressions have dedicated tests. Gaps are in secondary concerns (header round-trips, observability, retry policy) rather than core correctness.

---

## 3. Technical Debt and Danger Zones

### 3.1 Fragile Code Areas

| Area | File:Line (approx) | Why It Is Dangerous | Precautions for AI Agents |
|---|---|---|---|
| **OAuth token lifecycle + concurrency** | `src/PingenApiNet/Services/PingenConnectionHandler.cs:98-167` | `SetOrUpdateAccessToken` → `_authenticationSemaphore` → `Login` with double-check on `IsAuthorized()`. Past regressions: #22 (cross-tenant token leak), #27 (concurrent re-authentication). The 10-second semaphore timeout is a hard deadline — adding latency inside `Login` could push callers into timeout territory. | Do not "simplify" the double-check pattern. Do not make `_accessToken` or `_authenticationSemaphore` static again. Do not remove the per-instance field. Run both regression tests after any change: `PingenConnectionHandlerTests.MultipleInstances_MaintainSeparateAccessTokens` and `SetOrUpdateAccessToken_ConcurrentCalls_OnlyAuthenticatesOnce`. |
| **Shared `HttpClient.DefaultRequestHeaders.Authorization`** | `src/PingenApiNet/Services/PingenConnectionHandler.cs:172-179` (`TryAuthorizeHttpClient`) | The bearer token is written to the `Pingen.Api` `HttpClient.DefaultRequestHeaders`, but that `HttpClient` is pooled by `IHttpClientFactory`. In a multi-tenant in-process scenario two scopes can last-writer-win each other's header. Documented in ADR-004. | Do not assume `DefaultRequestHeaders.Authorization` is per-scope. If adding multi-tenant support, switch to per-request headers on `HttpRequestMessage`. |
| **Org-id prefix logic** | `src/PingenApiNet/Services/PingenConnectionHandler.cs:47, 275-296` (`NonOrganisationEndpoints`, `GetHttpRequestMessage`) | The logic is "if the path starts with any `NonOrganisationEndpoints` entry, do not prepend `organisations/{orgId}/`". Adding a new non-org-scoped endpoint without updating this array would silently attempt to hit `organisations/{orgId}/new-endpoint` and 404. Adding a new org-scoped endpoint whose path prefix happens to match an entry would wrongly skip the prefix. | When adding an endpoint, explicitly check `NonOrganisationEndpoints` membership. Add a unit test asserting the constructed URL. Prefer endpoint constants (`*Endpoints.Root`) to avoid typos. |
| **JSON filter nested `KeyValuePair`** | `src/PingenApiNet.Abstractions/Helpers/JsonConverters/PingenKeyValuePairStringObjectConverter.cs` + `ApiPagingRequest.Filtering` | Filter keys must be JSON property names (not C# property names). Use `PingenAttributesPropertyHelper<T>.GetJsonPropertyName(...)`. Miswriting the key produces a silently-wrong filter — the API returns unfiltered results, not an error. | Always use the attribute helper for filter / sort keys. Do not hardcode strings. Prefer the existing `LetterFields.*` / `BatchFields.*` constants when possible. |
| **File-location redirect (`302 Found` as success)** | `src/PingenApiNet/Services/PingenConnectionHandler.cs:374-418` (`GetApiResult`) | `AllowAutoRedirect = false` on `Pingen.Api` plus "`isSuccess = httpResponseMessage.IsSuccessStatusCode \|\| httpResponseMessage.StatusCode is HttpStatusCode.Found`". If Pingen ever changes the file-location response to a different 3xx (e.g., 307, 308), the library will silently treat it as an error. | Do not remove the `302` special case without auditing every endpoint. Adding a new `3xx`-returning endpoint requires extending this check. |
| **`PingenApiDataTypeMapping` coverage** | `src/PingenApiNet.Abstractions/Helpers/PingenSerialisationHelper.cs:109-124` | `IncludedCollection.OfType<T>()`, `FindById<T>()`, and `TryGetIncludedData<T>()` all rely on this dictionary. Adding a new resource type (new `PingenApiDataType` enum value + new attributes type) requires a corresponding mapping entry, otherwise included resources of that type are silently skipped with no warning. | Every new resource type **must** be added to `PingenApiDataTypeMapping`. Consider writing a reflection-based test that iterates `PingenApiDataType` values and asserts each is present (currently missing from the suite — see Backlog). |
| **`DistributionService` undocumented endpoint** | `src/PingenApiNet/Services/Connectors/DistributionService.cs` + `DistributionEndpoints.cs` | The `/distribution/delivery-products` endpoint is not in Pingen's public documentation. The response shape is our best guess from live observation. Pingen may change it without notice. | Do not make `Distribution` a hard dependency of new code paths. Fail gracefully. Flag any deserialisation error as a likely upstream change, not a library bug. |
| **`LetterCreate.MetaData`** | `src/PingenApiNet.Abstractions/Models/Letters/LetterMetaData.cs` + `README.md` | Setting `MetaData` on cheap delivery products triggers address-validation failures when postcodes exceed 4 characters. Only `PostAgRegistered` / `PostAgAPlus` tolerate full metadata. | Read `CLAUDE.md` § Known Constraints #3 before adding any sample that sets `MetaData`. Do not set it for `Cheap` or `PostAgEconomy` products. |
| **Letter-status polling timing** | README § 1 + `CLAUDE.md` § Known Constraints #5 | After `Letters.Create()`, Pingen validates the PDF asynchronously. Calling `Letters.Send()` immediately returns an error. Callers must poll `Letters.Get()` until `Status == LetterStates.Valid`. | Any new "create + send" sample or helper must include polling with a timeout. Do not assume a single `await` is enough. |
| **`PingenConnectionHandler.GetRequestHeaders` dead branch** | `src/PingenApiNet/Services/PingenConnectionHandler.cs:307-316` | Contains `if (apiRequest is not null) { /* nothing */ }` — a placeholder for future header logic. Harmless but misleading; suggests header handling is incomplete. | Do not delete without understanding what was intended. Likely a stub for `Accept-Language` / `User-Agent` / custom headers. |

### 3.2 Critical Business Logic Requiring Special Care

| Concern | Path | Notes |
|---|---|---|
| **Authentication** | `PingenConnectionHandler` § token lifecycle. | Bearer tokens are per-instance, per-credentials. Never cache tokens outside the handler. Never log the token. |
| **Rate limiting** | `ApiResult.RateLimit*` + `RetryAfter` + `PingenConnectionHandler.GetResponseHeaders`. | Pingen enforces per-route limits. The library surfaces `ApiResult.RateLimitRemaining` / `RetryAfter` / `RateLimitReset` on every call, but does **not** auto-retry. Consumer code is responsible for back-off. The E2E `RateLimit` fixture exercises this end-to-end. |
| **Pagination** | `ConnectorService.AutoPage` + `*PageResultsAsync`. | Increments `page[number]` linearly. If the underlying collection mutates between pages, results can be inconsistent (duplicates or skips). Document this in user-facing docs for anyone iterating large collections. |
| **Webhook signature validation** | `PingenWebhookHelper.ValidateWebhook`. | Uses `CryptographicOperations.FixedTimeEquals` (constant-time). Do not "optimise" to `SequenceEqual` / `==`. Callers must call `Request.EnableBuffering()` before passing the body because the stream is read twice. |
| **File download (S3)** | `LetterService.DownloadFileContent` + `PingenConnectionHandler.SendExternalRequestAsync`. | Uses `External` `HttpClient` (no auth, allows redirect). An S3 error returns XML; the library surfaces it via `PingenFileDownloadException`. Do not route external downloads through `Pingen.Api` — it would leak the bearer token to S3. |
| **Idempotency keys** | `ApiHeaderNames.IdempotencyKey` + `PingenConnectionHandler.GetRequestHeaders`. | 1–64 character opaque strings. Replayed-response detection via `Idempotent-Replayed` header → `ApiResult.IdempotentReplayed`. Callers that retry writes should supply a stable key; do not regenerate on each attempt. |

### 3.3 Known Technical Debt

| Debt Item | Severity | Notes |
|---|---|---|
| `TODO` on missing API docs | Low | `src/PingenApiNet.Abstractions/Enums/Api/PingenApiCurrency.cs:46` — `// TODO: Missing API Doc about currencies`. Currencies list may be incomplete. |
| `PingenApiDataTypeMapping` allocates on every access | Low | Property expression body (`=> new { ... }`) builds the dictionary per call. Should be `public static readonly Dictionary<...> PingenApiDataTypeMapping = new() { ... };`. Trivial fix. |
| `PingenConnectionHandler.GetRequestHeaders` placeholder branch | Low | See § 3.1 last row. |
| ADR-002 filename says "static" but status says "Accepted (instance-scoped)" | Low | The filename is preserved because README and other docs link to it; the file content now annotates this in the Status block. Acceptable. |
| `ApiQueryParameterNames.Sorting` lowercases the first character manually | Low | `src/PingenApiNet/Services/PingenConnectionHandler.cs:353` — `entry.Key[..1].ToLower() + entry.Key[1..]`. Fragile against locale + non-ASCII property names. A `char.ToLowerInvariant(...)` would be safer. No known incident. |
| `PingenConfiguration.WebhookSigningKeys` is mutable `Dictionary<string, string>?` after construction | Low | Not thread-safe if mutated at runtime. Acceptable for Singleton DI registration; document as "immutable after `AddPingenServices`". |
| No logging / telemetry hooks | Medium | Consumers have no way to observe outbound requests or rate-limit pressure without wrapping the client. A `DelegatingHandler` + `ILogger` integration point would be a mild improvement. |
| No retry / resilience policy | Medium | `Polly` integration is not in-scope by design, but documenting the integration pattern (DelegatingHandler on the `Pingen.Api` named client) would help consumers. |
| No `CHANGELOG.md` | Low | Releases are git-tag-driven. Tag annotations are the de-facto changelog. |

### 3.4 Deprecated Patterns / Outdated Dependencies

- None observed. All packages are on recent versions:
  - `Microsoft.NET.Test.Sdk 18.3.0`
  - `NUnit 4.5.1`, `NUnit3TestAdapter 6.2.0`, `NUnit.Analyzers 4.12.0`
  - `Shouldly 4.3.0`, `NSubstitute 5.3.0`
  - `WireMock.Net 1.7.0`
  - `coverlet.collector 8.0.1`
  - `Microsoft.Extensions.DependencyInjection 10.0.5`
- `Microsoft.Extensions.Http` version is resolved transitively by targeting `net10.0`.
- `.NET 10` + `C# 14` — current at time of writing.

---

## 4. Backlog Ideas

Improvement proposals discovered during the review. Estimated on a gut-feel S/M/L scale. Priority reflects AI-readiness impact, not business value.

| Title | Description | Complexity | Priority |
|---|---|---|---|
| Reflection test for `PingenApiDataTypeMapping` completeness | Enumerate `PingenApiDataType` values at test-time and assert each maps to a non-null CLR type in `PingenSerialisationHelper.PingenApiDataTypeMapping`. Catches the "forgot to add the mapping" regression that would otherwise silently skip included resources. | S | High |
| Static-cache `PingenApiDataTypeMapping` | Convert from expression-bodied property getter to `static readonly Dictionary`. Removes per-call allocation on hot `IncludedCollection.OfType<T>` / `FindById<T>` paths. | S | Medium |
| Integration test: filter + sort query string round-trip | Add a WireMock test that captures the outbound query string for an `ApiPagingRequest` with a nested `CollectionFilterOperator.And(Or(...))` filter, asserting the serialised filter matches the Pingen spec. | S | Medium |
| Integration test: Idempotency-Key header round-trip | WireMock assertion that a `Letters.Create` call with an idempotency key produces an outbound `Idempotency-Key` header with the expected value, and that `Idempotent-Replayed: true` populates `ApiResult.IdempotentReplayed`. | S | Medium |
| Unit test: `NonOrganisationEndpoints` URL construction | Directly assert `PingenConnectionHandler` builds `file_uploads`, `users`, `organisations` URLs without the `organisations/{orgId}/` prefix, and confirms all other endpoints include it. | S | Medium |
| `ILogger<PingenConnectionHandler>` integration | Optional `ILogger` dependency (constructor-injected). Log outbound method + URL + request-id + rate-limit-remaining at Debug. Never log bearer tokens or full response bodies. | M | Medium |
| Runbook: adding a new connector endpoint | New doc `doc/architecture/runbooks/add-connector-endpoint.md` with a numbered checklist: (1) add model + `*Fields` + `*Includes`, (2) register in `PingenApiDataType` enum + `PingenApiDataTypeMapping`, (3) add `*Endpoints` URL constants, (4) add service method + interface entry, (5) register in `PingenServiceCollection`, (6) add unit + integration tests. | S | High |
| Webhook signing-key rotation support | Accept multiple active keys; try each until one validates. `PingenWebhookHelper.ValidateWebhook(IReadOnlyCollection<string> signingKeys, ...)`. Useful during key rotation without downtime. | M | Low |
| Polly-compatible retry DelegatingHandler sample | Documentation note + sample code showing consumers how to wrap `Pingen.Api` with `services.AddHttpClient("Pingen.Api").AddPolicyHandler(...)`. Clarifies the intended extension point. | S | Medium |
| Explicit `Accept-Language` support on `ApiRequest` | Currently language is a query parameter on LetterEvents / LetterIssues. Per-request `Accept-Language` header would align with more idiomatic localisation. Backward-compatible addition. | M | Low |
| `CHANGELOG.md` + `release-notes/` per-version | Keep-a-Changelog format. Populate from git tags. CI could auto-generate at release time. | S | Medium |
| Obsolete the inline `PingenSerialisationHelper.TryGetIncludedData<T>` | Now that `IncludedCollection.OfType<T>().SingleOrDefault()` is the canonical call, consider adding `[Obsolete("Use result.Included?.OfType<T>().SingleOrDefault() instead.")]` in a future major version. Keep the behaviour for N major versions. | S | Low |
| Per-request Bearer token (remove shared `DefaultRequestHeaders` state) | Move the `Authorization` header from `HttpClient.DefaultRequestHeaders` to `HttpRequestMessage.Headers` per request. Enables safe in-process multi-tenancy. See ADR-004 § Watch out. | M | Low |
| Source-generator-based JSON (AOT friendliness) | Opt-in `JsonSerializerContext` to generate serialisation metadata at compile time. Enables trimming + AOT. Requires refactor of the `PingenSerialisationHelper` single-options object. | L | Low |
| `PingenApiNet.TestUtilities` NuGet package | Export `JsonApiStubHelper` + `IntegrationTestBase` scaffolding as an optional `-TestUtilities` package so consumer applications can easily WireMock-test their own usage of `PingenApiClient`. | L | Low |

---

## Appendix — Key Files for Rapid Onboarding

If a task is scoped narrowly, an AI agent should start here:

| Purpose | Path |
|---|---|
| Project instructions (read this first) | [[../CLAUDE\|CLAUDE.md]] |
| Public consumer entry point | `src/PingenApiNet/Interfaces/IPingenApiClient.cs` |
| HTTP + auth core | `src/PingenApiNet/Services/PingenConnectionHandler.cs` |
| Three-client HTTP wrapper | `src/PingenApiNet/Services/PingenHttpClients.cs` |
| DI registration (the one-stop API for consumers) | `src/PingenApiNet.AspNetCore/PingenServiceCollection.cs` |
| JSON serialisation core | `src/PingenApiNet.Abstractions/Helpers/PingenSerialisationHelper.cs` |
| Webhook validation | `src/PingenApiNet.Abstractions/Helpers/PingenWebhookHelper.cs` |
| JSON:API envelope | `src/PingenApiNet.Abstractions/Models/Api/Embedded/DataResults/` |
| Typed Included wrapper | `src/PingenApiNet.Abstractions/Models/Api/Embedded/DataResults/IncludedCollection.cs` |
| Base data model | `src/PingenApiNet.Abstractions/Models/Base/Data.cs` |
| Unit test HTTP-handler stub | `tests/PingenApiNet.UnitTests/Helpers/MockHttpMessageHandler.cs` |
| Integration test base + stub helpers | `tests/PingenApiNet.Tests.Integration/IntegrationTestBase.cs`, `tests/PingenApiNet.Tests.Integration/Helpers/` (`JsonApiStubHelper`, `PingenResponseFactory`, `WireMockExtensions`) |
| E2E test base | `tests/PingenApiNet.Tests.E2E/E2eTestBase.cs` |
| CI pipeline | `.github/workflows/main.yml` |
