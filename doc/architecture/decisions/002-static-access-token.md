---
title: "ADR-002: Instance-Scoped Access Token Field in PingenConnectionHandler"
tags: [adr, auth, concurrency]
---

# ADR-002: Instance-Scoped Access Token Field in PingenConnectionHandler

> **Filename note**: the file is named `002-static-access-token.md` for historical reasons (links in `README.md` point at it). The current decision is **instance-scoped**, not static. The original static design is described as prior context; this ADR supersedes it.

## Status

**Accepted** — supersedes a prior design that used a `static` `_accessToken` field. The change to an instance field fixes a multi-tenant security vulnerability (see issue #22). The instance-scoped design is verified by `PingenConnectionHandlerTests.MultipleInstances_MaintainSeparateAccessTokens` (regression test for #22) and `PingenConnectionHandlerTests.SetOrUpdateAccessToken_ConcurrentCalls_OnlyAuthenticatesOnce` (double-check locking regression for #27).

## Context

`PingenConnectionHandler` is registered as `Scoped` in ASP.NET Core DI (new instance per HTTP request). Each instance needs to call the Pingen Identity service to obtain an OAuth2 access token before making its first API request. Access tokens from the client_credentials flow are valid for a period (minutes to hours).

The original design used a `static` `_accessToken` field so that one token was shared across all handler instances in the same process, avoiding unnecessary re-authentication. However, this created a critical security vulnerability in multi-tenant scenarios: if multiple `PingenConnectionHandler` instances were created with different `ClientId`/`ClientSecret` pairs, they would share the same token — a token obtained for Organisation X would be reused by a handler configured for Organisation Y.

## Decision

The `_accessToken` and `_authenticationSemaphore` fields are now **instance-scoped**:

```csharp
private AccessToken? _accessToken;
private readonly SemaphoreSlim _authenticationSemaphore = new(1, 1);
```

Each `PingenConnectionHandler` instance maintains its own token and its own authentication semaphore. The `IsAuthorized()` method is an instance method.

## Consequences

**Good:**
- **Multi-tenant safe**: Multiple handler instances with different credentials maintain independent tokens. No cross-tenant token leakage.
- **Thread-safe per instance**: The per-instance semaphore serializes re-authentication within a single handler; only one thread enters `Login()` at a time per handler. Others wait up to 10 seconds.
- **Simpler reasoning**: No process-global mutable state to worry about.

**Trade-off:**
- **More frequent authentication in single-tenant Scoped DI**: Since each `Scoped` handler instance starts with no token, a new request scope will re-authenticate even if a previous scope's token is still valid. For most applications calling the Pingen API infrequently, this overhead is negligible. If token reuse across scopes is needed, consumers can register `PingenConnectionHandler` as `Singleton` (with appropriate thread-safety considerations for `_organisationId`) or implement external token caching.

**Bad / Watch out:**
- **`DefaultOrganisationId` vs `SetOrganisationId`**: The org ID is instance-level (`_organisationId`). Changing it via `SetOrganisationId()` on one handler does not affect other handlers. This is correct for multi-org switching within a single request scope but requires care if the same instance is reused across multiple logical operations.
