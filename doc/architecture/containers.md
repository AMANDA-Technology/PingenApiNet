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
        Container(tests, "PingenApiNet.Tests", ".NET 10 / NUnit 4", "Integration test suite. Not packaged. Calls real Pingen staging API.")
    }

    System_Ext(pingenApi, "Pingen API", "JSON:API REST")
    System_Ext(pingenIdentity, "Pingen Identity", "OAuth 2.0")
    System_Ext(pingenStorage, "Pingen Storage", "S3 pre-signed URLs")

    Rel(developer, aspnetcore, "Calls AddPingenServices()", "C# method call")
    Rel(developer, core, "Injects IPingenApiClient", "DI / constructor")
    Rel(developer, abstractions, "References for model types", "C# types")
    Rel(aspnetcore, core, "References", "Project/package reference")
    Rel(aspnetcore, abstractions, "References", "Project/package reference")
    Rel(core, abstractions, "References", "Project/package reference")
    Rel(tests, core, "Directly constructs", "Project reference")
    Rel(tests, abstractions, "References for model types", "Project reference")
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

### PingenApiNet.Tests (not packaged)

| Property | Value |
|---|---|
| Repo path | `src/PingenApiNet.Tests/` |
| Framework | NUnit 4, coverlet |
| Test type | Integration tests against Pingen staging API |
| Configuration | Environment variables (`PingenApiNet__BaseUri`, `PingenApiNet__IdentityUri`, `PingenApiNet__ClientId`, `PingenApiNet__ClientSecret`, `PingenApiNet__OrganisationId`) |
| Offline test | `Webhooks.DeserializeWebhookEventData` uses local `Assets/webhook_sample.json` |
| Test assets | `Assets/` — sample PDFs and a webhook JSON payload |
