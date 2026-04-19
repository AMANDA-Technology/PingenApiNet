---
title: "ADR-005: WireMock.Net for In-Process Integration Tests"
tags: [adr, testing, integration, wiremock]
---

# ADR-005: WireMock.Net for In-Process Integration Tests

## Status

**Accepted** — implemented in `tests/PingenApiNet.Tests.Integration/`.

## Context

The library has historically shipped with two test projects:

- `PingenApiNet.UnitTests` — offline unit tests. Services are exercised by mocking `IPingenConnectionHandler` with NSubstitute, which verifies the service-to-handler contract (correct endpoint path, HTTP verb, payload wrapping) but **cannot** catch issues in `PingenConnectionHandler` itself, in the OAuth token lifecycle, in query-string construction, or in JSON:API response envelope handling end-to-end.
- `PingenApiNet.Tests.E2E` — live tests against the real Pingen staging API. These require credentials, network access, and consume staging quota. They are valuable for final validation but too heavy and flaky for every commit and every local iteration.

A middle tier was missing: **integration tests that exercise the real `PingenApiClient` end-to-end (real `PingenConnectionHandler`, real OAuth flow, real HTTP, real JSON parsing) without a remote dependency**.

## Decision

A third test project, `PingenApiNet.Tests.Integration`, uses **WireMock.Net 1.7.x** to host an in-process HTTP server per test fixture. The server stubs:

- The OAuth 2.0 token endpoint (`POST /auth/access-tokens`) — stubbed once per `[SetUp]` so every fixture starts authenticated.
- Pingen resource endpoints (`GET /organisations/{orgId}/letters`, `POST /organisations/{orgId}/webhooks`, etc.) — stubbed per test with declarative `Request.Create()...RespondWith(...)` calls.
- Pagination scenarios via WireMock's `InScenario(...).WillSetStateTo(...).WhenStateIs(...)` state machine for auto-pagination tests.
- S3-style file up/download endpoints for `FilesService` and `LetterService.DownloadFileContent` coverage.

`IntegrationTestBase` creates a fresh `PingenHttpClients` whose three `HttpClient` instances (`Identity`, `Api`, `External`) all target the WireMock URL, then wires a real `PingenConnectionHandler` + `PingenApiClient` by hand (no DI container). `[OneTimeSetUp]` starts WireMock, `[SetUp]` resets stubs + re-stubs the token endpoint + rebuilds the client, `[TearDown]` disposes HTTP clients, `[OneTimeTearDown]` stops WireMock.

`Helpers/JsonApiStubHelper.cs` centralizes JSON:API body construction (single/collection/related/meta-abilities) so individual test files stay readable.

## Alternatives Considered

- **Custom `HttpMessageHandler`** (already used in `PingenApiNet.UnitTests/Helpers/MockHttpMessageHandler.cs`) — great for unit-testing `PingenConnectionHandler` but awkward for multi-endpoint flows and pagination; each test would hand-roll routing logic.
- **`HttpClient` interception via `IHttpClientFactory` test doubles** — more coupling between test code and factory internals; less declarative than WireMock's given/respond DSL.
- **Testcontainers + a mock API image** — overkill for a client library; introduces Docker as a prerequisite for `dotnet test` locally and in CI.

## Consequences

**Good:**
- Every connector (Batches, Distribution, Files, Letters, Organisations, Users, Webhooks) now has a round-trip test that exercises the real HTTP pipeline, real JSON serialisation, real pagination auto-loop, and real org-scoped URL prefixing.
- Tests run offline, in-process, with no credentials, no quota consumption, and no flake from network weather.
- Tests are fast — WireMock is embedded Kestrel; fixtures start in ~100 ms.
- WireMock's scenario/state DSL makes auto-pagination tests (two pages, flip state on first hit) easy to express.
- The stub helper (`JsonApiStubHelper`) documents the canonical JSON:API shapes Pingen returns — a valuable spec artifact in its own right.
- Regressions in `PingenConnectionHandler` URL construction (e.g., non-org-scoped endpoints, query parameter ordering, idempotency headers) are now catchable without staging access.

**Bad / Watch out:**
- WireMock stubs are hand-written from observation of the real Pingen API. They can drift from reality; they are not authoritative. The E2E test project remains the source of truth for actual API compatibility.
- The base class re-creates HTTP clients per `[SetUp]` (not per fixture), so every test pays the `HttpClient` construction cost. Acceptable given the overall speed, but do not assume fixture-scoped client reuse.
- `IntegrationTestBase` wires the client manually (no DI). If `PingenServiceCollection.AddPingenServices` diverges (e.g., new `Scoped` dependency), those integration tests will not catch the DI mis-wiring — `PingenServiceCollection` is covered separately by `PingenApiNet.UnitTests/Tests/AspNetCore/PingenServiceCollectionTests`.
- WireMock listens on a random ephemeral port per fixture. Tests are safe to parallelise across fixtures, but not across tests within a fixture (`Server.Reset()` in `[SetUp]` would wipe stubs from a sibling test running concurrently). NUnit's default is fixture-level parallelism, which is correct.
- Adding integration coverage for a new endpoint requires the developer to know the Pingen response shape. Keep `JsonApiStubHelper` honest by occasionally cross-checking against the [Pingen API docs](https://api.pingen.com/documentation) or real staging responses captured via the E2E project.

## Related

- [[001-json-api-records]] — serialisation decisions that these integration tests exercise end-to-end.
- [[002-static-access-token]] — the multi-tenant token regression (#22) lives in `PingenApiNet.UnitTests`, not here, because it needs fine-grained `HttpMessageHandler` control.
- [[003-iasyncenumerable-pagination]] — auto-pagination is now covered by integration tests as well as unit tests.
- [[004-three-http-clients]] — all three clients (Identity, Api, Files) are exercised by the integration suite.
