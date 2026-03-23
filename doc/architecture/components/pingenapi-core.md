---
title: C4 Level 3 — PingenApiNet (Core Package)
tags: [architecture, c4, components, core]
---

# C4 Level 3: PingenApiNet (Core Package)

`src/PingenApiNet/`

## Component Diagram

```mermaid
C4Component
    title Component Diagram — PingenApiNet

    Container_Boundary(core, "PingenApiNet") {
        Component(apiClient, "PingenApiClient", "C# sealed class", "Facade aggregating all connector service properties. Exposes Letters, Batches, Users, Organisations, Webhooks, Files, Distributions. Implements IPingenApiClient.")
        Component(connectionHandler, "PingenConnectionHandler", "C# sealed class", "Manages the OAuth2 token lifecycle and constructs/dispatches all HTTP requests to the Pingen API. Implements IPingenConnectionHandler.")
        Component(httpClients, "PingenHttpClients", "C# class", "Wraps three HttpClient instances: Identity (token requests), Api (resource API), External (S3 file up/download). Created via IHttpClientFactory or direct construction.")
        Component(configuration, "PingenConfiguration", "C# sealed record", "Holds BaseUri, IdentityUri, ClientId, ClientSecret, DefaultOrganisationId, WebhookSigningKeys. Implements IPingenConfiguration.")
        Component(connectorBase, "ConnectorService", "C# abstract class", "Base for all connector services. Provides HandleResult() (throws on error) and AutoPage() (IAsyncEnumerable pagination loop).")
        Component(letterService, "LetterService", "C# sealed class", "Connector for /letters/* endpoints: GetPage, GetPageResultsAsync, Create, Send, Cancel, Get, Delete, Update, GetFileLocation, DownloadFileContent, CalculatePrice, GetEventsPage, GetEventsPageResultsAsync, GetIssuesPage, GetIssuesPageResultsAsync.")
        Component(batchService, "BatchService", "C# sealed class", "Connector for /batches/* endpoints.")
        Component(userService, "UserService", "C# sealed class", "Connector for /users/* endpoints.")
        Component(orgService, "OrganisationService", "C# sealed class", "Connector for /organisations/* endpoints (non-org-scoped).")
        Component(webhookService, "WebhookService", "C# sealed class", "Connector for webhooks endpoints.")
        Component(filesService, "FilesService", "C# sealed class", "GetPath() to obtain pre-signed upload URL, UploadFile() to PUT stream to S3.")
        Component(distributionService, "DistributionService", "C# sealed class", "Connector for undocumented distribution/delivery-products endpoints.")
        Component(endpoints, "Endpoints (static classes)", "C# static classes", "One per resource domain: LettersEndpoints, BatchesEndpoints, FileUploadEndpoints, OrganisationsEndpoints, UsersEndpoints, WebhooksEndpoints, DistributionEndpoints. Define URL path fragments as constants/methods.")
    }

    Rel(apiClient, connectionHandler, "Delegates all HTTP ops to")
    Rel(apiClient, letterService, "Exposes as Letters property")
    Rel(apiClient, batchService, "Exposes as Batches property")
    Rel(apiClient, userService, "Exposes as Users property")
    Rel(apiClient, orgService, "Exposes as Organisations property")
    Rel(apiClient, webhookService, "Exposes as Webhooks property")
    Rel(apiClient, filesService, "Exposes as Files property")
    Rel(apiClient, distributionService, "Exposes as Distributions property")
    Rel(letterService, connectorBase, "Extends")
    Rel(batchService, connectorBase, "Extends")
    Rel(userService, connectorBase, "Extends")
    Rel(orgService, connectorBase, "Extends")
    Rel(webhookService, connectorBase, "Extends")
    Rel(filesService, connectorBase, "Extends")
    Rel(distributionService, connectorBase, "Extends")
    Rel(connectorBase, connectionHandler, "Uses via ConnectionHandler protected field")
    Rel(connectionHandler, httpClients, "Dispatches HTTP via")
    Rel(connectionHandler, configuration, "Reads base URLs, org ID, credentials from")
    Rel(letterService, endpoints, "Gets URL paths from LettersEndpoints")
    Rel(filesService, endpoints, "Gets URL paths from FileUploadEndpoints")
```

## Components in Detail

### PingenApiClient (`Services/PingenApiClient.cs`)

The consumer-facing entry point. Constructor-injected with all seven connector services and the connection handler. Exposes them as `IXxxService` properties. The only non-connector method is `SetOrganisationId(string)` which delegates to the connection handler to switch organisation context mid-session.

### PingenConnectionHandler (`Services/PingenConnectionHandler.cs`)

The most complex component. Responsibilities:
- Acquires and caches the OAuth2 bearer token (`SetOrUpdateAccessToken()` called before every request).
- Constructs `HttpRequestMessage` with correct URL (org-scoped or global), query parameters (paging, sorting, filtering, searching), and headers (`Idempotency-Key`).
- Detects which endpoints are NOT org-scoped (`NonOrganisationEndpoints` array: FileUpload, Users, Organisations root).
- Parses all rate-limit and request-ID response headers into `ApiResult`.
- Handles `302 Found` as success (file location endpoint).
- Exposes `SendExternalRequestAsync` for anonymous S3 requests.

Token field `_accessToken` is `static` — shared across all handler instances in the process (see [[decisions/002-static-access-token]]).

### PingenHttpClients (`Services/PingenHttpClients.cs`)

A thin wrapper grouping the three `HttpClient` instances. Created either via `IHttpClientFactory` (ASP.NET Core DI path) or `PingenHttpClients.Create(configuration)` (standalone path used by tests). The API client has `AllowAutoRedirect = false` (see [[decisions/004-three-http-clients]]).

### ConnectorService (`Services/Connectors/Base/ConnectorService.cs`)

Abstract base providing two reusable methods:
- `HandleResult<TData>(ApiResult<CollectionResult<TData>>)` — throws `PingenApiErrorException` on failure, returns `IList<TData>`.
- `HandleResult<TData>(ApiResult<SingleResult<TData>>)` — throws on failure, returns `TData?`.
- `AutoPage<TData>(apiPagingRequest, getPage)` — `protected async IAsyncEnumerable<IEnumerable<TData>>`. Loops from `PageNumber=1` (or caller's page) until `Meta.CurrentPage >= Meta.LastPage`.

### LetterService (`Services/Connectors/LetterService.cs`)

The most feature-rich connector. Key operations:
- `Create` — POST to `letters`, wraps `DataPost<LetterCreate, LetterCreateRelationships>`.
- `Send` — PATCH to `letters/{id}/send`, wraps `DataPatch<LetterSend>`.
- `GetFileLocation` — GET `letters/{id}/file`, returns `ApiResult` with `Location` URI (from `302 Found`).
- `DownloadFileContent` — calls `SendExternalRequestAsync` with the Location URI, parses XML error on failure.
- `CalculatePrice` — POST to `letters/price-calculator`.
- Events and Issues endpoints include a required `language` query parameter (appended directly in `LettersEndpoints`).

### Endpoints Static Classes (`Services/Connectors/Endpoints/`)

Each file is `internal static class`. Methods return `string` path fragments like `"letters/abc-123/send"`. These are concatenated by `PingenConnectionHandler` with the org-scoped prefix (`organisations/{orgId}/`) unless the path starts with one of the `NonOrganisationEndpoints`.
