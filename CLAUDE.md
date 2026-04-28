---
title: PingenApiNet — Claude Code Project Instructions
tags: [project, onboarding, dotnet]
---

# PingenApiNet

An unofficial .NET 10 API client library for the [Pingen v2 REST API](https://api.pingen.com/documentation) (used version 2.0.0). Pingen is a Swiss online postal service that accepts PDFs and dispatches physical letters. This library handles authentication (OAuth 2.0 client credentials), request construction, response deserialization, rate-limit tracking, auto-pagination, file upload/download, and webhook validation. It is published as three NuGet packages under the MIT license.

> **Before starting any task, read [`doc/ai-readiness.md`](doc/ai-readiness.md) and [`doc/architecture/README.md`](doc/architecture/README.md)**. The ai-readiness doc enumerates fragile areas (token lifecycle, org-id prefix logic, `PingenApiDataTypeMapping` completeness, `DefaultRequestHeaders.Authorization` sharing) with precautions. The architecture README is the entry point to the C4 diagrams + ADRs.

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 / C# 14 |
| Serialization | `System.Text.Json` with custom converters |
| HTTP | `System.Net.Http.HttpClient` via `IHttpClientFactory` (ASP.NET Core path) or direct construction (standalone path) |
| DI integration | `Microsoft.Extensions.DependencyInjection` |
| Testing | NUnit 4 + Shouldly + NSubstitute + WireMock.Net + coverlet |
| CI/CD | GitHub Actions — triggers on git tag push, builds and publishes to NuGet.org |
| Versioning | Semver derived from git tag via `build/GetBuildVersion.psm1` (PowerShell) |

## Solution / Project Structure

```
PingenApiNet.sln
src/
  PingenApiNet.Abstractions/       # No external NuGet dependencies — pure models, enums, interfaces, helpers
  PingenApiNet/                    # References Abstractions + Microsoft.Extensions.Http
  PingenApiNet.AspNetCore/         # References Abstractions + PingenApiNet + Microsoft.Extensions.DependencyInjection
tests/
  PingenApiNet.UnitTests/          # Offline unit tests (NUnit + Shouldly + NSubstitute).
                                   # References Abstractions + PingenApiNet + AspNetCore. NOT packaged.
  PingenApiNet.Tests.Integration/  # In-process integration tests (NUnit + Shouldly + WireMock.Net).
                                   # Exercises connectors/services against a local WireMock stub of the
                                   # Pingen API + Identity server. No network. NOT packaged.
  PingenApiNet.Tests.E2E/          # E2E integration tests (NUnit + Shouldly). Hits the real Pingen
                                   # staging API. Requires env vars — see Testing section. NOT packaged.
build/
  GetBuildVersion.psm1             # PowerShell module for semver extraction from git ref
.github/workflows/
  main.yml                         # Build + pack + publish to NuGet on tag
  codeql-analysis.yml              # Security scanning
  sonar-analysis.yml               # SonarCloud quality gate
```

### Dependency Graph

```
src/Abstractions  <──  src/PingenApiNet  <──  src/AspNetCore
                                        <──  tests/UnitTests            (refs Abstractions + PingenApiNet + AspNetCore)
                                        <──  tests/Tests.Integration    (refs Abstractions + PingenApiNet)
                                        <──  tests/Tests.E2E            (refs Abstractions + PingenApiNet + AspNetCore)
```

`Abstractions` has zero NuGet dependencies. It defines all data contracts.

## Build Commands

```bash
# Restore
dotnet restore PingenApiNet.sln

# Build (Debug)
dotnet build PingenApiNet.sln

# Build (Release with version)
dotnet build PingenApiNet.sln --configuration Release -p:Version=1.2.5

# Pack NuGet packages
dotnet pack PingenApiNet.sln --configuration Release -p:PackageVersion=1.2.5 --no-build

# Run unit tests (offline, no env vars needed)
dotnet test tests/PingenApiNet.UnitTests/PingenApiNet.UnitTests.csproj

# Run integration tests (offline, uses in-process WireMock.Net — no env vars needed)
dotnet test tests/PingenApiNet.Tests.Integration/PingenApiNet.Tests.Integration.csproj

# Run unit + integration tests together (fastest safe loop for local dev)
dotnet test PingenApiNet.sln --filter "FullyQualifiedName!~PingenApiNet.Tests.E2E"

# Run E2E tests (hits real Pingen staging API — requires env vars, see Testing section)
dotnet test tests/PingenApiNet.Tests.E2E/PingenApiNet.Tests.E2E.csproj

# Run everything in the solution
dotnet test PingenApiNet.sln
```

## Key Conventions

### Records and Immutability
All model types are C# `record` or `sealed record`. Use `init`-only properties. The `Data<TAttributes>` hierarchy is the core immutable data wrapper. New models must follow this pattern.

### JsonPropertyName on All Serialized Properties
Every property that crosses the JSON boundary must have `[JsonPropertyName("snake_case_name")]`. This is also the way `PingenAttributesPropertyHelper<T>.GetJsonPropertyName()` resolves filter/sort field names at runtime — it reads the attribute at runtime. Do not omit `[JsonPropertyName]` on attributes models. Attribute model records use the corresponding `*Fields` class constants as `[JsonPropertyName]` arguments, and all constructor parameters are nullable to support sparse fieldset responses.

### XML Documentation Comments
All public API members carry XML doc comments (`/// <summary>`). This is enforced by `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in every csproj. New public members must have doc comments.

### Nullable Reference Types
Enabled across all projects (`<Nullable>enable</Nullable>`). Treat all nullable annotation warnings as errors in spirit — do not suppress them without reason.

### Interface-First Design
Consumer code depends on interfaces (`IPingenApiClient`, `IPingenConnectionHandler`, `ILetterService`, etc.), never on concrete types. This enables mocking in future unit tests.

### Endpoint Constants
Every connector service has a sibling static class `*Endpoints` inside `Services/Connectors/Endpoints/`. This class holds all URL path fragments as `internal const string` or static methods. Never hardcode URL strings inside service methods.

### DataPost / DataPatch Envelope
All write payloads are wrapped in `DataPost<TAttributes>` or `DataPost<TAttributes, TRelationships>` (POST) and `DataPatch<TAttributes>` (PATCH). These serialize to the JSON:API `{ "data": { "type": "...", "attributes": {...} } }` envelope expected by Pingen.

### Error Handling
- `ConnectorService.HandleResult()` throws `PingenApiErrorException` if `ApiResult.IsSuccess` is false. Use this method when callers should not receive raw errors.
- Raw `ApiResult` is returned by `IPingenConnectionHandler` methods — callers can choose between checking `.IsSuccess` or calling `HandleResult`.
- `PingenFileDownloadException` is thrown when an S3 file download fails (the XML `<Error><Code>` is extracted).

### Authentication
`PingenConnectionHandler` manages the OAuth 2.0 access token internally with a `SemaphoreSlim(1,1)` to serialize concurrent re-auth. The `_accessToken` field is per-instance, so each handler maintains its own token (safe for multi-tenant scenarios). Token expiry has a 1-minute safety buffer (`ExpiresAt.AddMinutes(-1)`).

### Auto-Redirect Disabled
The API HTTP client has `AllowAutoRedirect = false` because the file-location endpoint returns `302 Found` with a `Location` header that must be followed manually via the `External` HTTP client (unauthenticated, arbitrary URLs).

## Architecture Patterns

- **Connector service pattern**: Each Pingen resource area (letters, batches, webhooks, etc.) has a dedicated `IXxxService` interface and `XxxService : ConnectorService` implementation. `ConnectorService` provides `HandleResult` and `AutoPage` utilities.
- **IPingenApiClient facade**: Consumer-facing entry point that aggregates all connector services as properties (`client.Letters.Create(...)`, `client.Files.UploadFile(...)`).
- **Auto-pagination via IAsyncEnumerable**: Collection endpoints expose both `GetPage()` (single page, raw `ApiResult`) and `GetPageResultsAsync()` (`IAsyncEnumerable<IEnumerable<TData>>`). The `AutoPage` helper in `ConnectorService` loops until `Meta.CurrentPage >= Meta.LastPage`.
- **Three HTTP clients**: `Pingen.Identity` (token endpoint), `Pingen.Api` (all resource API calls, no auto-redirect), `Pingen.Files` (external S3 URLs, anonymous).
- **JSON:API compliance**: All request/response shapes follow the JSON:API spec used by Pingen — `type`, `id`, `attributes`, `relationships`, `included`, `links`, `meta` fields.

## API Reference

The upstream Pingen API documentation (which this library wraps) is at:
**https://api.pingen.com/documentation**

Consult this when implementing new endpoints, verifying request/response shapes, or understanding business rules. The library targets API version 2.0.0.

## Important File Locations

| Purpose | Path |
|---|---|
| Main consumer interface | `src/PingenApiNet/Interfaces/IPingenApiClient.cs` |
| HTTP + auth plumbing | `src/PingenApiNet/Services/PingenConnectionHandler.cs` |
| DI registration | `src/PingenApiNet.AspNetCore/PingenServiceCollection.cs` |
| JSON serialization | `src/PingenApiNet.Abstractions/Helpers/PingenSerialisationHelper.cs` |
| Webhook validation | `src/PingenApiNet.Abstractions/Helpers/PingenWebhookHelper.cs` |
| Base data model | `src/PingenApiNet.Abstractions/Models/Base/Data.cs` |
| API result model | `src/PingenApiNet.Abstractions/Models/Api/ApiResult.cs` |
| Filter/sort helper | `src/PingenApiNet.Abstractions/Helpers/PingenAttributesPropertyHelper.cs` |
| E2E test base | `tests/PingenApiNet.Tests.E2E/E2eTestBase.cs` |
| Integration test base | `tests/PingenApiNet.Tests.Integration/IntegrationTestBase.cs` |
| WireMock JSON:API stub helper | `tests/PingenApiNet.Tests.Integration/Helpers/JsonApiStubHelper.cs` |
| Unit test HTTP handler stub | `tests/PingenApiNet.UnitTests/Helpers/MockHttpMessageHandler.cs` |
| CI pipeline | `.github/workflows/main.yml` |

## Testing

Tests are split into three projects. The first two run offline on every dev machine and in CI; the third requires Pingen staging credentials and is typically run manually.

### Unit Tests (`PingenApiNet.UnitTests`)
Offline unit tests that require no API credentials or network access. Uses **NUnit 4 + Shouldly + NSubstitute**. Located under `tests/PingenApiNet.UnitTests/Tests/`. Covers:

- `ApiRequestQueryParameters` / `SparseFieldsets` / `IncludeHelpers` / `FieldHelpers` — paging, filtering, sorting, include and sparse-fieldset query construction; verify `*Fields` / `*Includes` constants match `[JsonPropertyName]` values.
- `Webhooks` — offline deserialization of `Assets/webhook_sample.json`.
- `Helpers/` — `PingenSerialisationHelper`, `PingenWebhookHelper`, `PingenAttributesPropertyHelper`, `PingenDateTimeConverter(Nullable)`, `PingenKeyValuePairStringObjectConverter` (comprehensive coverage for edge cases, nullability, missing properties, invalid formats, and nested collections).
- `Models/` — `ApiResult`, `DataPost`/`DataPatch`, `ExternalRequestResult`, `IncludedCollection`, `PingenConfiguration`.
- `Services/` — `PingenApiClient` facade + `PingenConnectionHandler` (OAuth token lifecycle, re-auth, rate-limit header parsing, multi-tenant token isolation regression test for #22, concurrent-login double-check regression for #27).
- `Services/Connectors/` — per-connector unit tests using NSubstitute-mocked `IPingenConnectionHandler` (verifies endpoint path construction and error/edge-case handling for Batches, Distribution, Files, Letters, Organisations, Users, Webhooks, and the shared `ConnectorService`).
- `AspNetCore/` — `PingenServiceCollection.AddPingenServices()` DI registration.
- `Enums/` — `BatchIconSerializationTests`, `LetterEnumSerializationTests`, `AllEnumsSerializationTests` (enum and string-constant serialization round-trips for every value).
- `Exceptions/` — the three Pingen exception types.

`Helpers/MockHttpMessageHandler.cs` is a reusable handler stub for shaping HTTP responses in `PingenConnectionHandler` tests.

### Integration Tests (`PingenApiNet.Tests.Integration`)
Offline in-process integration tests using **NUnit 4 + Shouldly + WireMock.Net + Bogus**. Spins up a local WireMock HTTP server per test fixture, stubs both the Pingen API and the OAuth token endpoint, and exercises a real `PingenApiClient` wired to that server. Covers request/response round-trips, JSON:API envelope shaping, auto-pagination (`InScenario` state machines), and the three-HTTP-client routing (identity / api / external-files):

- `BatchServiceTests`, `DistributionServiceTests`, `FilesServiceTests`, `LetterServiceTests`, `OrganisationServiceTests`, `PingenApiClientTests`, `UserServiceTests`, `WebhookServiceTests`.
- `IntegrationTestBase.cs` — handles `[OneTimeSetUp]` WireMock startup, per-test reset, token-endpoint stub, client construction, and disposal.
- `Helpers/JsonApiStubHelper.cs` — low-level JSON:API envelope builder.
- `Helpers/PingenResponseFactory.cs` — centralised WireMock JSON:API response builder using Bogus for realistic test data generation.
- `Helpers/WireMockExtensions.cs` and `Helpers/WireMockAssertionExtensions.cs` — fluent helpers for stub setup and verification.

### E2E Tests (`PingenApiNet.Tests.E2E`)
Remote integration tests that call the **real Pingen staging API**. Uses **NUnit 4 + Shouldly + Bogus**. Require the following environment variables (the test base throws `InvalidOperationException` if any is missing):

```
PingenApiNet__BaseUri          # e.g. https://api-staging.pingen.com
PingenApiNet__IdentityUri      # e.g. https://identity-staging.pingen.com
PingenApiNet__ClientId
PingenApiNet__ClientSecret
PingenApiNet__OrganisationId
```

E2E tests inherit from `E2eTestBase.cs` which provides a LIFO cleanup queue (`RegisterCleanup(Func<Task>)`), orphan scavenging pattern, isolation prefix (`TestPrefix`), and standard `AssertSuccess` overloads. Current fixtures: `DistributionGetDeliveryProducts`, `FileUpload`, `LettersGetAll`, `RateLimit` (the last one deliberately exceeds rate limits to validate `Retry-After` behavior — run sparingly).

## Known Constraints and Gotchas

1. **`_accessToken` is per-instance** — each `PingenConnectionHandler` maintains its own access token and authentication semaphore. This is safe for multi-tenant scenarios where different handlers use different credentials. Note that in single-tenant DI setups with `Scoped` lifetime, each request scope gets a fresh handler and will re-authenticate if the previous token is not shared.
2. **`DistributionService` is undocumented** — `IPingenApiClient.Distributions` calls an unofficial endpoint. Use at your own risk.
3. **`LetterCreate.MetaData` caveat** — Only set `MetaData` for `PostAgRegistered` or `PostAgAPlus` delivery products. Setting it for cheap products causes address validation failures when postcodes exceed 4 characters.
4. **`Included` is `IncludedCollection`** — The JSON:API `included` array is wrapped in an `IncludedCollection` that stores raw `JsonElement` items and provides typed access via `OfType<T>()` (returns `IEnumerable<Data<T>>`) and `FindById<T>(string id)` (returns `Data<T>?`). `PingenSerialisationHelper.TryGetIncludedData<T>()` is still available for extracting a single included resource.
5. **Letter status polling** — Pingen validates uploaded letters asynchronously. After `Letters.Create()`, poll `Letters.Get()` until `Status == LetterStates.Valid` before calling `Letters.Send()`.
6. **SerializerOptions are cached** — `PingenSerialisationHelper.SerializerOptions()` returns a shared `static readonly` instance. `JsonSerializerOptions` is thread-safe once initialized. Do not mutate the returned instance.
7. **Filtering serialization** — Filter expressions are serialized to JSON as nested `KeyValuePair<string, object>` using a custom converter (`PingenKeyValuePairStringObjectConverter`). The filter key for field names must be the JSON property name (use `PingenAttributesPropertyHelper<T>.GetJsonPropertyName()`).
