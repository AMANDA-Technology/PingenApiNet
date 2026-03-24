---
title: Architecture Overview — PingenApiNet
tags: [architecture, overview]
---

# Architecture Overview

PingenApiNet is a .NET 10 API client library published as three NuGet packages. It provides a typed, async-first, JSON:API-compliant interface to the [Pingen v2 API](https://api.pingen.com/documentation) — an online postal service that accepts PDF files and sends physical letters.

## Key Constraints

- **Library, not application** — there is no executable entry point. All packages are consumed by downstream ASP.NET Core applications or other .NET services.
- **Zero-dependency abstractions** — `PingenApiNet.Abstractions` has no NuGet dependencies, making it safe to reference in domain projects without pulling in HTTP or DI concerns.
- **Integration and offline unit tests** — offline tests verify model correctness and helper constants without API calls; integration tests call the real staging API and require environment variables with valid credentials.
- **OAuth 2.0 client credentials flow** — token is acquired and refreshed automatically; callers never handle bearer tokens directly.

## Tech Stack

| Concern | Technology |
|---|---|
| Language | C# 14 |
| Runtime | .NET 10 |
| Serialization | System.Text.Json |
| HTTP | System.Net.Http (HttpClient / IHttpClientFactory) |
| DI | Microsoft.Extensions.DependencyInjection |
| Testing | NUnit 4, coverlet |
| Packaging | NuGet (via `dotnet pack`, published by GitHub Actions on tag) |
| Code quality | CodeQL (GitHub Actions), SonarCloud |

## Architecture Documents

- [[context]] — C4 Level 1: System context, actors, and external systems
- [[containers]] — C4 Level 2: NuGet packages and their relationships
- [[components/pingenapi-core]] — C4 Level 3: Internal components of `PingenApiNet`
- [[components/pingenapi-abstractions]] — C4 Level 3: Internal components of `PingenApiNet.Abstractions`
- [[decisions/001-json-api-records]] — ADR: Records and System.Text.Json for domain models
- [[decisions/002-static-access-token]] — ADR: Static access token field in connection handler
- [[decisions/003-iasyncenumerable-pagination]] — ADR: IAsyncEnumerable for auto-pagination
- [[decisions/004-three-http-clients]] — ADR: Three named HttpClient instances
- [[glossary]] — Domain terminology
