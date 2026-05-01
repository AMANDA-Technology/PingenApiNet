---
title: "ADR-006: Three-Tier Test Pyramid and Coverage Strategy"
tags: [adr, testing, unit, integration, e2e, strategy]
---

# ADR-006: Three-Tier Test Pyramid and Coverage Strategy

## Status

**Accepted** — fully implemented across all three test projects as of Epic #102 (2026-05-01).

## Context

The library has three test projects (Unit, Integration, E2E) that collectively exercise the public API surface, the HTTP/OAuth plumbing, the JSON:API envelope handling, and live Pingen staging behaviour. Before Epic #102, coverage was uneven: the unit and integration tiers were well-established, but several targeted gaps existed in header round-trip verification, stream-handling semantics, and E2E letter workflow coverage.

Epic #102 ran a systematic review (sub-issue #105 — gap audit) and closed all identified gaps through targeted additions (sub-issues #106, #107, #108, #110, #109).

This ADR documents the rationale behind the three-tier structure, the boundaries between each tier, and the rules that keep the pyramid healthy going forward.

## Decision

The library uses a **three-tier test pyramid**:

### Tier 1 — Unit Tests (`PingenApiNet.UnitTests`)

**What:** Offline tests using NSubstitute mocks and `MockHttpMessageHandler` stubs. No network, no WireMock, no credentials.

**Scope:**
- Connector services: verify endpoint path, HTTP verb, and payload wrapping via `Substitute.For<IPingenConnectionHandler>()`.
- `PingenConnectionHandler`: OAuth token lifecycle, rate-limit header parsing, multi-tenant isolation, concurrent re-authentication. Uses `MockHttpMessageHandler` for deterministic HTTP responses.
- Helpers, serializers, converters, models, enums: real deserialization through `PingenSerialisationHelper` against known JSON strings.
- Regression tests for issues #22 (cross-tenant token leak) and #27 (concurrent re-auth double-check): run these after any change to `PingenConnectionHandler`.

**Run command:** `dotnet test tests/PingenApiNet.UnitTests/PingenApiNet.UnitTests.csproj`

**When to add a unit test:**
- Any new public method in `src/`.
- Any behavior edge case or error path that can be expressed with a mock/stub without a running HTTP server.
- Any regression for a specific bug — pin it here permanently.

### Tier 2 — Integration Tests (`PingenApiNet.Tests.Integration`)

**What:** In-process tests using WireMock.Net. A real `PingenApiClient` runs against a real `PingenConnectionHandler` which communicates with a WireMock stub server. No network, no credentials.

**Scope:**
- Full HTTP pipeline: JSON serialization, header construction, query-string encoding, OAuth handshake, pagination loop.
- Cross-cutting: cancellation, concurrency, error handling (4xx/5xx), pagination boundaries, edge cases.
- Per-connector round-trips: list, get, create, delete, paginated `GetPage` / `GetPageResultsAsync`.
- `QueryStringSerializationTests`: filter + sort + search + paging + sparse-fieldsets query string shapes.
- `IdempotencyTests`: `Idempotency-Key` header presence and `Idempotent-Replayed` flag surfacing.

**Run command:** `dotnet test tests/PingenApiNet.Tests.Integration/PingenApiNet.Tests.Integration.csproj`

**When to add an integration test:**
- Any new HTTP feature that involves header construction, query-string encoding, or response-header parsing — these cannot be verified by connector-service unit tests alone (which mock the handler).
- New pagination variants or state-machine flows (use WireMock's `InScenario` DSL).
- Error paths that involve real HTTP status codes and JSON:API error bodies.

### Tier 3 — E2E Tests (`PingenApiNet.Tests.E2E`)

**What:** Remote tests against the real Pingen staging API. Requires environment variables (`PingenApiNet__BaseUri`, `PingenApiNet__IdentityUri`, `PingenApiNet__ClientId`, `PingenApiNet__ClientSecret`, `PingenApiNet__OrganisationId`). Consumes staging quota.

**Scope:**
- Full letter workflow: upload file → create → poll until valid → send / cancel / update / delete.
- Price calculation, issues retrieval (parameterized by locale).
- Batch, Organisation, User, Webhook CRUD.
- `RateLimit` — deliberately triggers 429 to verify `Retry-After` end-to-end (run sparingly).

All fixtures inherit `E2eTestBase`, which provides:
- LIFO cleanup queue (`RegisterCleanup(Func<Task>)`).
- Orphan scavenging via `TestPrefix` isolation.
- Standardized `AssertSuccess` overloads.

**Run command:** `dotnet test tests/PingenApiNet.Tests.E2E/PingenApiNet.Tests.E2E.csproj`

**When to add an E2E test:**
- New endpoint where the response shape is unverified by integration tests (integration stubs are hand-written; staging is the source of truth).
- Any workflow that spans multiple API calls with real async state transitions (letter validation, file signing).
- Rate-limit or authentication behavior that cannot be simulated reliably in WireMock.

## Boundary Rules

1. **Unit tests must not require a running server.** If a test needs a real HTTP round-trip, it belongs in Tier 2.
2. **Integration tests must not require credentials or network.** If a test needs real Pingen behavior (e.g., async letter validation), it belongs in Tier 3.
3. **WireMock stubs are not authoritative.** They model the Pingen API as observed. Whenever a stub shape differs from the Pingen docs, the docs win; update the stub and add an E2E counterpart.
4. **Every new `NonOrganisationEndpoints` entry must have a unit test** asserting the URL is built without the `organisations/{orgId}/` prefix. This prevents silent 404s from incorrect endpoint registration.
5. **Every new `PingenApiDataType` enum value must have a `PingenApiDataTypeMapping` entry.** This is enforced by `PingenApiDataTypeMappingTests` which fails if an unmapped enum value is not explicitly allow-listed.

## Testing Stack

| Library | Version | Tier |
|---|---|---|
| NUnit 4.x | test runner | All |
| Shouldly 4.3 | assertions | All |
| NSubstitute 5.3 | mocking | Tier 1 |
| WireMock.Net 1.7.x | HTTP stubbing | Tier 2 |
| Bogus 35.5 | test data generation | Tier 2 + 3 |
| coverlet.collector 8.0 | coverage | All |

Rejections: xUnit, Moq, FluentAssertions, AutoFixture, NUnit `Assert.That` assertions. Use Shouldly (`ShouldBe`, `ShouldSatisfyAllConditions`, `Should.Throw`) throughout.

## Alternatives Considered

- **Single test project with all tiers** — rejected; offline and live tests would be entangled, making `--filter` gymnastics mandatory everywhere.
- **Testcontainers for a mock API** — rejected; adds Docker as a `dotnet test` prerequisite for a client library. WireMock in-process is simpler and faster.
- **Contract testing (Pact)** — deferred; useful if Pingen ever publishes a consumer-driven contract endpoint. The audit doc (`doc/analysis/2026-05-01-api-docs-gap-audit.md`) tracks schema drift manually for now.

## Consequences

**Good:**
- Offline tiers (1 + 2) run in CI on every commit without credentials.
- Tier 3 is opt-in (manual or on-demand) and guards against real API drift.
- The coverage pyramid is explicit: new behavior lands first in Tier 1 (fast), then gains a Tier 2 stub round-trip, and finally a Tier 3 smoke test for production-critical paths.
- `doc/analysis/2026-05-01-api-docs-gap-audit.md` provides a checklist for future waves.

**Watch out:**
- WireMock stubs can drift from the real Pingen API. Periodically cross-check stubs against [Pingen API docs](https://api.pingen.com/documentation) or captured E2E responses.
- The `RateLimit` E2E test deliberately hammers staging. Do not run it in automated CI without rate-limit budget to spare.
- `E2eTestBase.RegisterCleanup` runs in LIFO order during `[OneTimeTearDown]`. If `[OneTimeSetUp]` fails before cleanup is registered, orphans are scavenged on the next run via `TestPrefix`. Do not skip orphan scavenging in new E2E fixtures.

## Related

- [[005-wiremock-integration-tests]] — rationale for the WireMock in-process tier (Tier 2).
- [[001-json-api-records]] — serialization decisions exercised by Tiers 1 and 2.
- [[002-static-access-token]] — multi-tenant regression tests live in Tier 1.
- [[003-iasyncenumerable-pagination]] — auto-pagination covered in both Tier 2 and Tier 3.
- `doc/analysis/2026-04-20-test-coverage-roadmap.md` — wave-by-wave implementation history.
- `doc/analysis/2026-05-01-api-docs-gap-audit.md` — API coverage audit driving Epic #102.
