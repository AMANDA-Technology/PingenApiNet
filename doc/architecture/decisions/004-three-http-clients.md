---
title: "ADR-004: Three Named HttpClient Instances"
tags: [adr, http, architecture]
---

# ADR-004: Three Named HttpClient Instances

## Context

The Pingen integration requires calling three distinct HTTP endpoints with different configuration requirements:

1. **Pingen Identity service** — OAuth2 token endpoint. Accepts `application/x-www-form-urlencoded` POST. `BaseAddress` set to `IdentityUri`.
2. **Pingen API** — Resource REST API with JSON:API bodies. Must have `AllowAutoRedirect = false` because the file location endpoint (`/letters/{id}/file`) returns `302 Found` with a `Location` header that must be captured as a URI, not silently followed.
3. **External file storage** — S3-compatible pre-signed URLs for PDF upload (PUT) and download (GET). URLs are fully qualified absolute URLs provided by the API. Must NOT carry the Pingen bearer token or any other pre-configured headers. Auto-redirect must be allowed (default).

Using a single `HttpClient` for all three would require header and configuration manipulation per request, which is error-prone and would pollute the shared `DefaultRequestHeaders`.

## Decision

Three named `HttpClient` instances are defined in `PingenHttpClients.Names`:

- `Pingen.Identity` — base address set to `IdentityUri`, accept header `application/x-www-form-urlencoded`.
- `Pingen.Api` — base address set to `BaseUri`, `AllowAutoRedirect = false`, receives `Authorization: Bearer <token>` on `DefaultRequestHeaders`.
- `Pingen.Files` — no base address, no pre-configured headers, default configuration (allow auto-redirect).

In the ASP.NET Core path (`PingenServiceCollection.AddPingenServices`), these are registered via `services.AddHttpClient(name, configure)` and resolved via `IHttpClientFactory`. In the standalone path (used by tests), `PingenHttpClients.Create(configuration)` constructs `HttpClient` instances directly.

## Consequences

**Good:**
- Each client is independently configured for its purpose — no per-request header manipulation.
- Bearer token is set once on `Pingen.Api.DefaultRequestHeaders.Authorization` after each token refresh, applying to all subsequent requests.
- `Pingen.Files` client ensures no accidental auth header leakage to external S3 URLs.
- Compatible with `IHttpClientFactory` lifetime management (pooled handlers, DNS rotation).

**Bad / Watch out:**
- `Pingen.Api` has `AllowAutoRedirect = false`. Any new endpoint that returns a redirect other than `302 Found` for file location will require explicit handling in `PingenConnectionHandler.GetApiResult` (currently treats `302` as success).
- **Shared `DefaultRequestHeaders.Authorization` on the `Pingen.Api` `HttpClient`** — this is a multi-tenant hazard. `PingenConnectionHandler` is registered `Scoped`, but the `HttpClient` produced by `IHttpClientFactory` is keyed by its name (`Pingen.Api`) and its underlying handler pool is process-scoped. That means two `Scoped` handler instances in two concurrent request scopes can race on the same `HttpClient.DefaultRequestHeaders.Authorization` slot. Each handler does hold its own `_accessToken` (fixed in [[002-static-access-token]]), but the Authorization header on the shared `HttpClient` is last-writer-wins. For current usage (single tenant per process, or re-auth per scope) this is fine; for true in-process multi-tenant use, callers should set the `Authorization` header per-request (via `HttpRequestMessage.Headers.Authorization`) rather than once on `DefaultRequestHeaders`. This is a known limitation — consider filing an issue if you need in-process multi-tenancy.
