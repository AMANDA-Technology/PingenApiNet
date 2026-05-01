# Test Coverage Implementation Roadmap

**Date:** 2026-04-20  
**Issue:** [#82 — create implementation roadmap for 100% test coverage](https://github.com/AMANDA-Technology/PingenApiNet/issues/82)  
**Goal:** 100% test coverage across unit, integration, and E2E tiers  
**Reference:** [AMANDA-Technology/CashCtrlApiNet](https://github.com/AMANDA-Technology/CashCtrlApiNet)

---

## Current State

### Projects
| Project | Framework | Purpose |
|---|---|---|
| `PingenApiNet.Abstractions` | net10.0 | Models, enums, helpers — zero external deps |
| `PingenApiNet` | net10.0 | Main API client, 7 connector services |
| `PingenApiNet.AspNetCore` | net10.0 | DI registration |
| `PingenApiNet.UnitTests` | net10.0 | Offline unit tests |
| `PingenApiNet.Tests.Integration` | net10.0 | WireMock-based integration tests |
| `PingenApiNet.Tests.E2E` | net10.0 | Live Pingen staging API tests |

### Services Under Test
1. **BatchService** — `GetPage`, `GetPageResultsAsync`, `Get`, `Create`
2. **DistributionService** — `GetDeliveryProducts`, `GetDeliveryProductsPageResultsAsync`
3. **FilesService** — `GetPath`, `UploadFile`, `DownloadFileContent`
4. **LetterService** — `Create`, `Get`, `GetPage`, `GetPageResultsAsync`, `Send`, `Cancel`, `GetFileLocation`
5. **OrganisationService** — `GetPage`, `GetPageResultsAsync`, `Get`
6. **UserService** — `GetPage`, `GetPageResultsAsync`, `Get`
7. **WebhookService** — `Create`, `Get`, `GetPage`, `GetPageResultsAsync`, `Delete`

### Current Coverage Baseline
| Tier | Status |
|---|---|
| Unit | Test classes exist for all 7 services + helpers; error/edge cases covered for Batch, Distribution, Files, and Letter services (Wave 1 Phase 1 complete) |
| Integration | Test classes exist for all 7 services; cross-cutting scenarios complete (Wave 3); 100% coverage for Batch, Distribution, Files, Webhook (Wave 4 complete) |
| E2E | Batch, Organisation, User, Webhook covered (Wave 6 complete); other scenarios: Distribution, FileUpload, LettersGetAll, RateLimit |

### Stack Alignment (vs CashCtrlApiNet reference)
| Library | Status |
|---|---|
| NUnit 4.5.1 | ✅ Aligned |
| Shouldly 4.3.0 | ✅ Aligned |
| NSubstitute 5.3.0 | ✅ Aligned (unit tests) |
| WireMock.Net 2.2.0 | ✅ Aligned (integration tests) |
| Bogus | ✅ Done — installed in Integration and E2E tests |
| `PingenResponseFactory` | ✅ Done — WireMock stub helpers created |
| `WireMockExtensions` | ✅ Done — fluent WireMock setup helpers created |
| `E2eTestBase` with LIFO cleanup | ✅ Done — upgraded and renamed from `TestBase.cs` |

**No framework migration required.** Only infrastructure gaps need to be filled (Wave 0).

---

## Implementation Waves

| Wave | Title | Tier | Persona | Dependencies |
|---|---|---|---|---|
| **0** | Pre-implementation Alignment & Test Infrastructure | All | `developer-dotnet` | None |
| **1** | Unit Tests: Service Edge Cases & Error Scenarios | Unit | `developer-dotnet` | Wave 0 |
| **2** | Unit Tests: Core Models, Enums & Helpers | Unit | `developer-dotnet` | Wave 0 |
| **3** | Integration Tests: Cross-cutting Concerns | Integration | `developer-dotnet` | Wave 0 |
| **4** | Integration Tests: Services Group 1 (Batch, Distribution, Files, Webhook) | Integration | `developer-dotnet` | Wave 0 |
| **5** | Integration Tests: Services Group 2 (Letter, Organisation, User) | Integration | `developer-dotnet` | Wave 0 |
| **6** | E2E Tests: Services Group 1 (Batch, Organisation, User, Webhook) | E2E | `developer-dotnet` | Wave 0 |
| **7** | E2E Tests: Services Group 2 (Letter workflow, Distribution, Files) | E2E | `developer-dotnet` | Wave 0 |

---

## Wave Details

### Wave 0: Pre-implementation Alignment & Test Infrastructure

**Scope:**
- Add `Bogus` NuGet package to `PingenApiNet.Tests.Integration` and `PingenApiNet.Tests.E2E`
- Create `PingenResponseFactory` — centralised WireMock JSON:API response builder (single item, collection, error responses)
- Create `WireMockExtensions` and `WireMockAssertionExtensions` for cleaner test arrangement and verification
- Upgrade `TestBase.cs` → `E2eTestBase.cs` with LIFO cleanup queue, orphan scavenging, `"PG-E2E-{guid}"` isolation prefix, standardised `AssertSuccess()` overloads

**Acceptance Criteria:**
- `Bogus` installed and usable from both test projects
- `PingenResponseFactory` generates valid JSON:API envelopes for collection and single-item responses
- `E2eTestBase` tracks resources and cleans up in LIFO order during `[OneTimeTearDown]`
- All existing E2E tests updated to inherit `E2eTestBase`
- Files changed: ≤10

---

### Wave 1: Unit Tests — Service Edge Cases & Error Scenarios

**Phase 1 of 2 (Complete):** Batch, Distribution, Files, Letter services.
**Phase 2 of 2 (Complete):** Organisation, User, Webhook, and `PingenConnectionHandler`.

**Scope:** `tests/PingenApiNet.UnitTests/Tests/Services/Connectors/`

- Add error scenario tests to all 7 service test classes (invalid IDs, API-level errors, 422 validation failures)
- Verify correct HTTP method, endpoint path, and query parameters for each operation
- Cover `PingenConnectionHandler` rate-limit parsing, OAuth token refresh, multi-tenant isolation edge cases

**Acceptance Criteria:**
- All 7 service test classes cover success, failure, and argument-validation paths
- `PingenConnectionHandler` edge cases (token expiry, 429 `Retry-After`, concurrent re-auth) are fully covered
- Files changed: ~7–8

---

### Wave 2: Unit Tests — Core Models, Enums & Helpers

**Phase 1 of 3 (Complete):** Model tests (`ApiResult`, `DataPost` / `DataPatch`, `IncludedCollection`, `ExternalRequestResult`).
**Phase 2 of 3 (Complete):** Enum serialization tests (`BatchIconSerializationTests`, `LetterEnumSerializationTests`, `AllEnumsSerializationTests`).
**Phase 3 of 3 (Complete):** Helper tests (`PingenAttributesPropertyHelper`, `PingenDateTimeConverter`, `PingenKeyValuePairStringObjectConverter`, `PingenSerialisationHelper`, `PingenWebhookHelper`).

**Scope:** `tests/PingenApiNet.UnitTests/Tests/Models/`, `Enums/`, `Helpers/`

- Enum serialization round-trips for all enum types (including hyphenated values)
- `ApiResult<T>` boundary conditions (null data, error payloads, empty included arrays)
- `DataPost<T>` / `DataPatch<T>` serialization edge cases
- `PingenDateTimeConverter` / nullable variant edge cases (min/max DateTime, null)
- `PingenWebhookHelper` with invalid signatures and malformed payloads
- `PingenAttributesPropertyHelper` for missing/duplicate attributes
- `PingenSerialisationHelper` with missing/extra properties

**Acceptance Criteria:**
- 100% unit coverage for all model, enum, and helper classes
- All JSON converter edge cases tested
- Files changed: ≤10

---

### Wave 3: Integration Tests — Cross-cutting Concerns

**Phase 1 of 1 (Complete):** Create all 5 cross-cutting test classes.

**Scope:** `tests/PingenApiNet.Tests.Integration/Tests/CrossCutting/`

New test classes:
- `CancellationTokenTests` — `CancellationToken` propagation through HTTP pipeline
- `ConcurrencyTests` — parallel requests from multiple clients, token isolation
- `ErrorHandlingTests` — 401, 403, 404, 422, 500, 502, 503, 504 responses + JSON:API error parsing
- `PaginationTests` — `IAsyncEnumerable` auto-pagination, boundary conditions (empty page, single page, multi-page)
- `EdgeCaseTests` — empty collections, null optional fields, large payloads, malformed responses

**Acceptance Criteria:**
- 5 new cross-cutting test classes created
- All cross-cutting code paths in `PingenConnectionHandler` covered via WireMock
- Files changed: 5

---

### Wave 4: Integration Tests — Services Group 1

**Phase 1 of 1 (Complete):** Batch, Distribution, Files, Webhook services.

**Scope:** `tests/PingenApiNet.Tests.Integration/Tests/` — Batch, Distribution, Files, Webhook

- Refactor existing test classes to use `PingenResponseFactory` and `Bogus`
- Add missing endpoint variations and error responses
- Cover paginated responses, file upload/download via WireMock stubs, webhook CRUD

**Acceptance Criteria:**
- `BatchServiceTests`, `DistributionServiceTests`, `FilesServiceTests`, `WebhookServiceTests` achieve 100% integration coverage
- All tests use `Bogus` for payloads and `WireMockExtensions` for stubs
- Files changed: 4

---

### Wave 5: Integration Tests — Services Group 2

**Phase 1 of 1 (Complete):** Letter, Organisation, User services.

**Scope:** `tests/PingenApiNet.Tests.Integration/Tests/` — Letter, Organisation, User

- Refactor existing test classes to use `PingenResponseFactory` and `Bogus`
- Cover letter state transitions: create → send → cancel → get file location
- Organisation and User paginated retrieval scenarios

**Acceptance Criteria:**
- `LetterServiceTests`, `OrganisationServiceTests`, `UserServiceTests` achieve 100% integration coverage
- Letter workflow state transitions are stubbed and verified
- Files changed: 3

---

### Wave 6: E2E Tests — Services Group 1

**Phase 1 of 1 (Complete):** Batch, Organisation, User, Webhook.

**Scope:** `tests/PingenApiNet.Tests.E2E/` — Batch, Organisation, User, Webhook

New/updated test fixtures inheriting `E2eTestBase`:
- `BatchE2eTests` — create batch, get batch, paginated list
- `OrganisationE2eTests` — list organisations, get by ID
- `UserE2eTests` — list users, get by ID
- `WebhookE2eTests` — full CRUD (create → get → list → delete), LIFO cleanup

**Acceptance Criteria:**
- All 4 fixtures interact with real Pingen staging API
- LIFO cleanup queue purges all test data (especially webhooks) during teardown
- Orphan scavenging removes leftover data from previously failed runs
- Files changed: 4

---

### Wave 7: E2E Tests — Services Group 2

**Phase 1 of 1 (Complete):** Letter workflow, Distribution, Files.

**Scope:** `tests/PingenApiNet.Tests.E2E/` — Letter workflow, Distribution, Files

- Create `LetterWorkflowE2eTests` — full live scenario: upload file → create letter → send → cancel
- Update `DistributionGetDeliveryProducts.cs` and `FileUpload.cs` to inherit `E2eTestBase`
- All test data registered in LIFO cleanup queue

**Acceptance Criteria:**
- Letter full workflow verified end-to-end on Pingen staging API
- All created artefacts (files, letters) are cleaned up in teardown
- Files changed: 3

---

### Wave 8: Test Completeness Review & Spec-Drift Fixes

**Phase 1 of 1 (Complete — Epic #102, 2026-05-01):** API docs audit + targeted gap closure.

**Scope:** All test projects + `src/PingenApiNet.Abstractions/Enums/`

Sub-issues completed as part of Epic #102:

- **#105** — API documentation gap audit. Produced `doc/analysis/2026-05-01-api-docs-gap-audit.md` comparing every Pingen v2 endpoint and model against the library. Identified coverage gaps, spec drift, and documentation updates.
- **#106** — Unit test gap closure: 19 new unit tests across `PingenConnectionHandlerTests` (NonOrg URL matrix, idempotency round-trip with `PostAsync`/`PatchAsync`), `FilesServiceTests` (stream position semantics, non-disposal, 8 MB large stream), and `IncludedCollectionTests` (`FindById<T>` for `UserAssociation`, heterogeneous arrays, same-id different-types).
- **#107** — Integration test refinement: 12 new integration tests. New `CrossCutting/QueryStringSerializationTests.cs` (nested And/Or filter, multi-field sort, search param, page params, sparse-fieldsets+include) and `CrossCutting/IdempotencyTests.cs` (Idempotency-Key header presence on Create/Send/Cancel, `Idempotent-Replayed` flag). Extended `DistributionServiceTests` with HTTP 500 and malformed JSON error paths.
- **#108** — E2E expansion: 5 new E2E files. `Helpers/LetterCleanupHelper.cs` (reusable cleanup/scavenging), `Letters/LetterUpdateE2eTests.cs` (upload+draft+poll+patch paper type), `Letters/LetterDeleteE2eTests.cs` (upload+draft+delete+verify 404), `Letters/LetterCalculatePriceE2eTests.cs` (cheap vs PostAgB price comparison), `Letters/LetterIssuesE2eTests.cs` (parameterized EN-GB/DE-DE/FR-FR, bounded auto-page).
- **#110** — Spec drift fixes: `USD` and `GBP` added to `PingenApiCurrency` from Pingen OpenAPI spec (`billing_currency` field); `presets` enum value XML doc expanded to clarify relationship-input-only nature; `PingenApiDataTypeMapping` XML doc updated to document deliberate `presets` omission. `AllEnumsSerializationTests` sentinel bumped to 4 currency values.
- **#109** — Documentation refresh: `doc/ai-readiness.md` updated to reflect closed gaps and new test counts; `doc/analysis/2026-04-20-test-coverage-roadmap.md` updated (this section); `doc/architecture/components/pingenapi-core.md` corrected `/user/*` path; `doc/architecture/decisions/006-test-pyramid-coverage-strategy.md` added.

**Acceptance Criteria:**
- `dotnet build PingenApiNet.sln --configuration Debug` → 0 errors, 0 CS warnings
- `dotnet test PingenApiNet.sln --filter "FullyQualifiedName!~PingenApiNet.Tests.E2E"` → all pass (~685+ unit, ~162+ integration)
- All 7 §2.3 gap rows in `doc/ai-readiness.md` closed or annotated
- `PingenApiCurrency` includes `CHF`, `EUR`, `USD`, `GBP`
- ADR-006 created

---

## Execution Sequence

```
Wave 0 (sequential — must complete first)
    │
    ├─── Wave 1 ─── (parallel)
    ├─── Wave 2 ─── (parallel)
    ├─── Wave 3 ─── (parallel)
    ├─── Wave 4 ─── (parallel)
    ├─── Wave 5 ─── (parallel)
    ├─── Wave 6 ─── (parallel)
    └─── Wave 7 ─── (parallel)
```

After Wave 0 merges, Waves 1–7 can be implemented concurrently by independent agents.

---

## Environment Variables (E2E)

```bash
PingenApiNet__BaseUri=https://api-staging.pingen.com
PingenApiNet__IdentityUri=https://identity-staging.pingen.com
PingenApiNet__ClientId=<client-id>
PingenApiNet__ClientSecret=<client-secret>
PingenApiNet__OrganisationId=<organisation-id>
```
