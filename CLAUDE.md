---
title: PingenApiNet — Claude Code Project Instructions
tags: [project, onboarding, dotnet]
---

# PingenApiNet

An unofficial .NET 9 API client library for the [Pingen v2 REST API](https://api.pingen.com/documentation) (used version 2.0.0). Pingen is a Swiss online postal service that accepts PDFs and dispatches physical letters. This library handles authentication (OAuth 2.0 client credentials), request construction, response deserialization, rate-limit tracking, auto-pagination, file upload/download, and webhook validation. It is published as three NuGet packages under the MIT license.

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 9 / C# 13 |
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
  PingenApiNet.Tests/           # Integration tests (NUnit). References Abstractions + PingenApiNet. NOT packaged.
build/
  GetBuildVersion.psm1          # PowerShell module for semver extraction from git ref
.github/workflows/
  main.yml                      # Build + pack + publish to NuGet on tag
  codeql-analysis.yml           # Security scanning
  sonar-analysis.yml            # SonarCloud quality gate
```

### Dependency Graph

```
Abstractions  <──  PingenApiNet  <──  AspNetCore
                                 <──  Tests
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

# Run integration tests (requires env vars — see Testing section)
dotnet test src/PingenApiNet.Tests/PingenApiNet.Tests.csproj
```

## Key Conventions

### Records and Immutability
All model types are C# `record` or `sealed record`. Use `init`-only properties. The `Data<TAttributes>` hierarchy is the core immutable data wrapper. New models must follow this pattern.

### JsonPropertyName on All Serialized Properties
Every property that crosses the JSON boundary must have `[JsonPropertyName("snake_case_name")]`. This is also the way `PingenAttributesPropertyHelper<T>.GetJsonPropertyName()` resolves filter/sort field names at runtime — it reads the attribute at runtime. Do not omit `[JsonPropertyName]` on attributes models.

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
`PingenConnectionHandler` manages the OAuth 2.0 access token internally with a `SemaphoreSlim(1,1)` to serialize concurrent re-auth. The static `_accessToken` field means one token per process. Token expiry has a 1-minute safety buffer (`ExpiresAt.AddMinutes(-1)`).

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
| Test base | `src/PingenApiNet.Tests/TestBase.cs` |
| CI pipeline | `.github/workflows/main.yml` |

## Testing

Tests are primarily **integration tests** that call the real Pingen staging API. `ApiRequestQueryParameters` is an offline unit test for query parameter construction. `IncludeHelpers` is an offline unit test that verifies all `*Includes` static helper classes have correct constant values matching their sibling `*Relationships` JSON property names. Required environment variables for integration tests:

```
PingenApiNet__BaseUri          # e.g. https://api-staging.pingen.com
PingenApiNet__IdentityUri      # e.g. https://identity-staging.pingen.com
PingenApiNet__ClientId
PingenApiNet__ClientSecret
PingenApiNet__OrganisationId
```

Tests construct `PingenConnectionHandler` and `PingenApiClient` directly without DI. The `Webhooks.DeserializeWebhookEventData` test is an offline test using `Assets/webhook_sample.json`. The `ApiRequestQueryParameters` tests are also offline — they verify `ApiRequest.Include` property behavior and query parameter formatting without any API calls. The `IncludeHelpers` tests are also offline — they verify all `*Includes` static helper constant values and their usability with `ApiRequest.Include`.

## Known Constraints and Gotchas

1. **`_accessToken` is static** — shared across all `PingenConnectionHandler` instances in the same process. This is intentional to avoid unnecessary re-authentication. Be careful if you need per-instance tokens (e.g., multi-tenant scenarios).
2. **`DistributionService` is undocumented** — `IPingenApiClient.Distributions` calls an unofficial endpoint. Use at your own risk.
3. **`LetterCreate.MetaData` caveat** — Only set `MetaData` for `PostAgRegistered` or `PostAgAPlus` delivery products. Setting it for cheap products causes address validation failures when postcodes exceed 4 characters.
4. **`Included` is `IList<object>`** — The JSON:API `included` array is partially typed. Use `PingenSerialisationHelper.TryGetIncludedData<T>()` to extract specific included resources by their `PingenApiDataType` mapping.
5. **Letter status polling** — Pingen validates uploaded letters asynchronously. After `Letters.Create()`, poll `Letters.Get()` until `Status == LetterStates.Valid` before calling `Letters.Send()`.
6. **SerializerOptions are created fresh per call** — `PingenSerialisationHelper.SerializerOptions()` allocates a new `JsonSerializerOptions` instance each call. This is a minor performance concern; do not cache options that contain custom converters without understanding thread-safety implications.
7. **Filtering serialization** — Filter expressions are serialized to JSON as nested `KeyValuePair<string, object>` using a custom converter (`PingenKeyValuePairStringObjectConverter`). The filter key for field names must be the JSON property name (use `PingenAttributesPropertyHelper<T>.GetJsonPropertyName()`).
