---
title: C4 Level 2 — Containers (NuGet Packages)
tags: [architecture, c4, containers]
---

# C4 Level 2: Containers (NuGet Packages)

## Diagram

```mermaid
C4Container
    title Container Diagram — PingenApiNet NuGet Packages

    Person(developer, "Application Developer")

    Container_Boundary(library, "PingenApiNet Library") {
        Container(aspnetcore, "PingenApiNet.AspNetCore", ".NET 10 / C#", "IServiceCollection extension for registering all Pingen services and HTTP clients in ASP.NET Core DI containers")
        Container(core, "PingenApiNet", ".NET 10 / C#", "Core API client: connection handler, authentication, HTTP request construction, response parsing, and connector services per resource type")
        Container(abstractions, "PingenApiNet.Abstractions", ".NET 10 / C#", "All models, enums, interfaces, exceptions, and helpers. Zero NuGet dependencies. Safe for reference in domain layers.")
        Container(unitTests, "PingenApiNet.UnitTests", ".NET 10 / NUnit 4", "Offline unit tests (NUnit + Shouldly + NSubstitute). Not packaged. No network.")
        Container(integrationTests, "PingenApiNet.Tests.Integration", ".NET 10 / NUnit 4 + WireMock.Net", "Offline integration tests (NUnit + Shouldly + WireMock.Net). Exercises real PingenApiClient against in-process stubbed HTTP server. Not packaged.")
        Container(e2eTests, "PingenApiNet.Tests.E2E", ".NET 10 / NUnit 4", "End-to-end tests against the real Pingen staging API. Requires credential env vars. Not packaged.")
    }

    System_Ext(pingenApi, "Pingen API", "JSON:API REST")
    System_Ext(pingenIdentity, "Pingen Identity", "OAuth 2.0")
    System_Ext(pingenStorage, "Pingen Storage", "S3 pre-signed URLs")
    System_Ext(wiremock, "WireMock.Net", "In-process HTTP stub server (Integration tests only)")

    Rel(developer, aspnetcore, "Calls AddPingenServices()", "C# method call")
    Rel(developer, core, "Injects IPingenApiClient", "DI / constructor")
    Rel(developer, abstractions, "References for model types", "C# types")
    Rel(aspnetcore, core, "References", "Project/package reference")
    Rel(aspnetcore, abstractions, "References", "Project/package reference")
    Rel(core, abstractions, "References", "Project/package reference")
    Rel(unitTests, core, "Mocks IPingenConnectionHandler / constructs connectors", "Project reference + NSubstitute")
    Rel(unitTests, abstractions, "References for model types", "Project reference")
    Rel(integrationTests, core, "Constructs real PingenApiClient", "Project reference")
    Rel(integrationTests, abstractions, "References for model types", "Project reference")
    Rel(integrationTests, wiremock, "HTTP requests routed to WireMock server", "HTTP (localhost)")
    Rel(e2eTests, core, "Constructs real PingenApiClient", "Project reference")
    Rel(e2eTests, pingenApi, "Real HTTP calls (staging)", "HTTPS JSON:API")
    Rel(core, pingenIdentity, "POST auth/access-tokens", "HTTPS")
    Rel(core, pingenApi, "GET / POST / PATCH / DELETE", "HTTPS JSON:API")
    Rel(core, pingenStorage, "PUT (upload) / GET (download)", "HTTPS pre-signed URL")
```

## Package Details

### PingenApiNet.Abstractions

| Property | Value |
|---|---|
| Repo path | `src/PingenApiNet.Abstractions/` |
| Namespace root | `PingenApiNet.Abstractions` |
| NuGet dependencies | None |
| Responsibility | All data contracts, domain interfaces, enums, custom JSON converters, helper utilities |
| Key interfaces | `IPingenConfiguration`, `IData`, `IAttributes`, `IRelationships`, `IDataResult`, `IDataPost`, `IDataPatch` |
| Key models | `ApiResult<T>`, `CollectionResult<T>`, `SingleResult<T>`, `Data<TAttributes>`, `DataPost<TAttributes>`, `DataPatch<TAttributes>` |
| Key helpers | `PingenSerialisationHelper`, `PingenWebhookHelper`, `PingenAttributesPropertyHelper<T>` |
| Exceptions | `PingenApiErrorException`, `PingenFileDownloadException`, `PingenWebhookValidationErrorException` |

### PingenApiNet

| Property | Value |
|---|---|
| Repo path | `src/PingenApiNet/` |
| Namespace root | `PingenApiNet` |
| NuGet dependencies | `Microsoft.Extensions.Http` 10.0.x |
| Responsibility | HTTP client management, OAuth 2.0 token lifecycle, URL construction, request dispatch, response parsing, connector services |
| Main entry points | `IPingenApiClient` / `PingenApiClient`, `IPingenConnectionHandler` / `PingenConnectionHandler` |
| Connector services | `LetterService`, `BatchService`, `UserService`, `OrganisationService`, `WebhookService`, `FilesService`, `DistributionService` |
| HTTP client factory | `PingenHttpClients` — wraps three `HttpClient` instances (Identity, Api, External/Files) |

### PingenApiNet.AspNetCore

| Property | Value |
|---|---|
| Repo path | `src/PingenApiNet.AspNetCore/` |
| Namespace root | `PingenApiNet.AspNetCore` |
| NuGet dependencies | `Microsoft.Extensions.DependencyInjection` 10.0.x |
| Responsibility | Single static class `PingenServiceCollection` with `AddPingenServices()` extension method. Registers all named HTTP clients and scoped services. |
| Consumer API | `services.AddPingenServices(configuration)` or `services.AddPingenServices(baseUri, identityUri, clientId, clientSecret, orgId)` |

### PingenApiNet.UnitTests (not packaged)

| Property | Value |
|---|---|
| Repo path | `tests/PingenApiNet.UnitTests/` |
| Framework | NUnit 4, Shouldly, NSubstitute, coverlet |
| Test type | Offline unit tests. No API credentials, no network. |
| References | `Abstractions` + `PingenApiNet` + `PingenApiNet.AspNetCore` |
| Test assets | `Assets/webhook_sample.json` — for offline deserialization tests |
| Test helpers | `Helpers/MockHttpMessageHandler.cs` — scriptable `HttpMessageHandler` stub used by `PingenConnectionHandler` unit tests |

### PingenApiNet.Tests.Integration (not packaged)

| Property | Value |
|---|---|
| Repo path | `tests/PingenApiNet.Tests.Integration/` |
| Framework | NUnit 4, Shouldly, **WireMock.Net 1.7.x**, coverlet |
| Test type | Offline integration tests. Each fixture spins up a local WireMock HTTP server, stubs the OAuth token endpoint + resource endpoints, and drives a real `PingenApiClient` whose three HTTP clients are rewired to the WireMock URL. |
| References | `Abstractions` + `PingenApiNet` only — does not depend on `AspNetCore` DI (constructs the client by hand). |
| Base | `IntegrationTestBase.cs` — `[OneTimeSetUp]` starts WireMock, `[SetUp]` resets stubs + stubs the token endpoint + creates a fresh `PingenApiClient`, `[TearDown]` disposes HTTP clients, `[OneTimeTearDown]` stops WireMock. |
| Helpers | `Helpers/JsonApiStubHelper.cs` — builds JSON:API `data`/`included`/`links`/`meta` response bodies, single + collection shapes, relationship wrappers, meta-abilities payloads. |
| Coverage | Per-connector round-trip tests: `BatchServiceTests`, `DistributionServiceTests`, `FilesServiceTests`, `LetterServiceTests`, `OrganisationServiceTests`, `PingenApiClientTests`, `UserServiceTests`, `WebhookServiceTests`. |

### PingenApiNet.Tests.E2E (not packaged)

| Property | Value |
|---|---|
| Repo path | `tests/PingenApiNet.Tests.E2E/` |
| Framework | NUnit 4, Shouldly, coverlet |
| Test type | Live end-to-end tests. Calls the real Pingen staging API. |
| References | `Abstractions` + `PingenApiNet` + `PingenApiNet.AspNetCore` |
| Required env vars | `PingenApiNet__BaseUri`, `PingenApiNet__IdentityUri`, `PingenApiNet__ClientId`, `PingenApiNet__ClientSecret`, `PingenApiNet__OrganisationId` (base class throws if any is missing). |
| Fixtures | `DistributionGetDeliveryProducts`, `FileUpload`, `LettersGetAll`, `RateLimit`. |
