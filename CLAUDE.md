---
title: PingenApiNet — Claude Code Project Instructions
tags: [project, onboarding, dotnet]
---

# PingenApiNet

An unofficial .NET 10 API client library for the [Pingen v2 REST API](https://api.pingen.com/documentation) (used version 2.0.0). Pingen is a Swiss online postal service that accepts PDFs and dispatches physical letters. This library handles authentication (OAuth 2.0 client credentials), request construction, response deserialization, rate-limit tracking, auto-pagination, file upload/download, and webhook validation. It is published as three NuGet packages under the MIT license.

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 / C# 14 |
| Serialization | `System.Text.Json` with custom converters |
| HTTP | `System.Net.Http.HttpClient` via `IHttpClientFactory` (ASP.NET Core path) or direct construction (standalone path) |
| DI integration | `Microsoft.Extensions.DependencyInjection` |
| Testing | NUnit 4 + coverlet |
| CI/CD | GitHub Actions — triggers on git tag push, builds and publishes to NuGet.org |
| Versioning | Semver derived from git tag via `build/GetBuildVersion.psm1` (PowerShell) |

## Solution / Project Structure

```
PingenApiNet.sln
src/
  PingenApiNet.Abstractions/    # No external NuGet dependencies — pure models, enums, interfaces, helpers
  PingenApiNet/                 # References Abstractions + Microsoft.Extensions.Http
  PingenApiNet.AspNetCore/      # References Abstractions + PingenApiNet + Microsoft.Extensions.DependencyInjection
tests/
  PingenApiNet.UnitTests/       # Offline unit tests (NUnit). References Abstractions + PingenApiNet + AspNetCore. NOT packaged.
  PingenApiNet.Tests.E2E/       # E2E integration tests (NUnit). Requires API env vars. NOT packaged.
build/
  GetBuildVersion.psm1          # PowerShell module for semver extraction from git ref
.github/workflows/
  main.yml                      # Build + pack + publish to NuGet on tag
  codeql-analysis.yml           # Security scanning
  sonar-analysis.yml            # SonarCloud quality gate
```

### Dependency Graph

```
src/Abstractions  <──  src/PingenApiNet  <──  src/AspNetCore
                                        <──  tests/UnitTests
                                        <──  tests/Tests.E2E
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

# Run E2E tests (requires env vars — see Testing section)
dotnet test tests/PingenApiNet.Tests.E2E/PingenApiNet.Tests.E2E.csproj
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
| E2E test base | `tests/PingenApiNet.Tests.E2E/TestBase.cs` |
| CI pipeline | `.github/workflows/main.yml` |

## Testing

Tests are split into two projects:

### Unit Tests (`PingenApiNet.UnitTests`)
Offline tests that require no API credentials or network access. Located under `Tests/`. Includes:
- `ApiRequestQueryParameters` — verifies `ApiRequest.Include` property behavior and query parameter formatting.
- `FieldHelpers` — verifies `*Fields` constant values match `[JsonPropertyName]` attributes on model records.
- `IncludeHelpers` — verifies `*Includes` static helper constant values match relationship JSON property names.
- `SparseFieldsets` — verifies sparse fieldset query parameter construction and serialization.
- `Webhooks` — offline deserialization test using `Assets/webhook_sample.json`.
- Additional unit tests under `Tests/` subdirectories (Helpers, Services, Models, Exceptions, AspNetCore).

### E2E Tests (`PingenApiNet.Tests.E2E`)
Integration tests that call the real Pingen staging API. Require environment variables:

```
PingenApiNet__BaseUri          # e.g. https://api-staging.pingen.com
PingenApiNet__IdentityUri      # e.g. https://identity-staging.pingen.com
PingenApiNet__ClientId
PingenApiNet__ClientSecret
PingenApiNet__OrganisationId
```

E2E tests construct `PingenConnectionHandler` and `PingenApiClient` directly without DI. Includes: `DistributionGetDeliveryProducts`, `FileUpload`, `LettersGetAll`, `RateLimit`.

## Known Constraints and Gotchas

1. **`_accessToken` is per-instance** — each `PingenConnectionHandler` maintains its own access token and authentication semaphore. This is safe for multi-tenant scenarios where different handlers use different credentials. Note that in single-tenant DI setups with `Scoped` lifetime, each request scope gets a fresh handler and will re-authenticate if the previous token is not shared.
2. **`DistributionService` is undocumented** — `IPingenApiClient.Distributions` calls an unofficial endpoint. Use at your own risk.
3. **`LetterCreate.MetaData` caveat** — Only set `MetaData` for `PostAgRegistered` or `PostAgAPlus` delivery products. Setting it for cheap products causes address validation failures when postcodes exceed 4 characters.
4. **`Included` is `IncludedCollection`** — The JSON:API `included` array is wrapped in an `IncludedCollection` that stores raw `JsonElement` items and provides typed access via `OfType<T>()` (returns `IEnumerable<Data<T>>`) and `FindById<T>(string id)` (returns `Data<T>?`). `PingenSerialisationHelper.TryGetIncludedData<T>()` is still available for extracting a single included resource.
5. **Letter status polling** — Pingen validates uploaded letters asynchronously. After `Letters.Create()`, poll `Letters.Get()` until `Status == LetterStates.Valid` before calling `Letters.Send()`.
6. **SerializerOptions are cached** — `PingenSerialisationHelper.SerializerOptions()` returns a shared `static readonly` instance. `JsonSerializerOptions` is thread-safe once initialized. Do not mutate the returned instance.
7. **Filtering serialization** — Filter expressions are serialized to JSON as nested `KeyValuePair<string, object>` using a custom converter (`PingenKeyValuePairStringObjectConverter`). The filter key for field names must be the JSON property name (use `PingenAttributesPropertyHelper<T>.GetJsonPropertyName()`).
