---
title: "ADR-002: Static Access Token Field in PingenConnectionHandler"
tags: [adr, auth, concurrency]
---

# ADR-002: Static Access Token Field in PingenConnectionHandler

## Context

`PingenConnectionHandler` is registered as `Scoped` in ASP.NET Core DI (new instance per HTTP request). Each instance needs to call the Pingen Identity service to obtain an OAuth2 access token before making its first API request. Access tokens from the client_credentials flow are valid for a period (minutes to hours). Re-authenticating on every request scope would be wasteful.

## Decision

The `_accessToken` field in `PingenConnectionHandler` is `static`:

```csharp
private static AccessToken? _accessToken;
private static readonly SemaphoreSlim AuthenticationSemaphore = new(1, 1);
```

This means one token is shared across all `PingenConnectionHandler` instances in the same process. The `SemaphoreSlim(1,1)` prevents concurrent re-authentication races. The token is refreshed only when expired (with a 1-minute safety buffer).

## Consequences

**Good:**
- A single OAuth2 token is reused across all scoped service instances — no unnecessary re-auth per request.
- Thread-safe: the semaphore serializes re-authentication; only one goroutine enters `Login()` at a time. Others wait up to 10 seconds.

**Bad / Watch out:**
- **Multi-tenant gotcha**: If an application manages multiple Pingen organisations with separate `ClientId`/`ClientSecret` pairs and creates multiple `PingenConnectionHandler` instances with different configurations, they will fight over the single static token. The current implementation is not safe for that use case. Work around it by using separate processes or separate AppDomains.
- **`DefaultOrganisationId` vs `SetOrganisationId`**: The org ID is instance-level (`_organisationId`), not static. Changing it via `SetOrganisationId()` on one scoped handler does not affect other handlers. This is correct for multi-org switching within a single request scope but requires care if the same scoped instance is reused across multiple logical operations.
- **Testing**: Since the token is static, tests that run in parallel within the same process will share authentication state. The test base constructs direct `PingenConnectionHandler` instances which can interact if run concurrently.
