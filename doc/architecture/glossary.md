---
title: Domain Glossary — PingenApiNet
tags: [architecture, glossary, domain]
---

# Domain Glossary

Pingen-specific and library-specific terms used throughout the codebase.

## Pingen Domain Terms

| Term | Definition |
|---|---|
| **Organisation** | A Pingen account entity. Most API resources are scoped under an organisation. The URL path is `/organisations/{organisationId}/letters`, etc. A client can switch organisation context via `IPingenApiClient.SetOrganisationId()`. |
| **Letter** | The core Pingen resource. Represents a PDF file that will be printed and physically mailed. Goes through states: created → validated → sent → delivered. |
| **LetterState** | The lifecycle state of a letter. Key values: `Valid` (ready to send), `Invalid` (validation failed), `Sent`, etc. Defined in `LetterStates` enum. |
| **Batch** | A grouping of letters for bulk processing. Batches can be configured with grouping options, icons, and split settings. |
| **DeliveryProduct** | The postal service product used to deliver a letter. Pingen supports multiple products (e.g., `PostAgA`, `PostAgRegistered`, `Cheap`). Product is selected at creation and can be overridden at send time. |
| **AddressPosition** | Whether the recipient address window is on the `left` or `right` of the printed letter. |
| **PrintMode** | `simplex` (single-sided) or `duplex` (double-sided) printing. |
| **PrintSpectrum** | `color` or `grayscale` printing. |
| **MetaData** | Optional sender/recipient address structured data embedded in the letter request. Required only for registered/tracked delivery products where address validation is strict. For standard products, address is extracted from the PDF content itself. |
| **FileUpload** | A two-step process: (1) call `Files.GetPath()` to receive a pre-signed S3 PUT URL with a signature, (2) PUT the file bytes directly to that URL. The resulting `FileUrl` and `FileUrlSignature` are then referenced in `LetterCreate`. |
| **FileUrlSignature** | A short string (up to 60 chars) returned by the file upload path endpoint. Must be submitted alongside `FileUrl` when creating a letter to prove the upload was authorized. |
| **Preset** | A saved configuration template for letters (referenced in `LetterCreateRelationships`). Presets allow reusing print settings across letters. The `PingenApiDataType` is `presets`. |
| **Webhook** | An HTTP callback registered in Pingen to receive push notifications for letter events. Each webhook has a `signingKey` (also called "secret") used to validate HMAC-SHA256 signatures on incoming payloads. |
| **WebhookEvent** | An individual push notification sent by Pingen to a registered webhook URL. Contains included resources (organisation, letter, letter event). |
| **LetterEvent** | A record of something that happened to a letter (e.g., submitted, printed, delivered). Has a `language`-parameterized description. |
| **UserAssociation** | The relationship between a Pingen user account and an organisation, including role and status. |
| **Distribution / DeliveryProduct** | An unofficial endpoint (`/distribution/delivery-products`) not documented in the Pingen API docs. Returns available delivery products. Use via `IDistributionService`. |

## API Protocol Terms

| Term | Definition |
|---|---|
| **JSON:API** | The specification followed by the Pingen API. Resources have `type`, `id`, `attributes`, `relationships`, `links`, and `meta` fields. Collections have `data` (array), `links`, and `meta` with pagination info. See https://jsonapi.org |
| **Idempotency Key** | An optional `Idempotency-Key` header (1–64 characters) that can be sent with POST/PATCH requests. If the same key is replayed, Pingen returns the original response without re-processing. Detected via `Idempotent-Replayed` response header, exposed on `ApiResult.IdempotentReplayed`. |
| **Rate Limiting** | Pingen enforces request rate limits. Response headers `x-ratelimit-limit`, `x-ratelimit-remaining`, `x-ratelimit-reset`, and `Retry-After` are captured in every `ApiResult`. |
| **X-Request-ID** | A UUID assigned by Pingen to every request. Returned in the `X-Request-ID` response header and exposed as `ApiResult.RequestId`. Used for support inquiries. |
| **client_credentials** | The OAuth2 grant type used by this library. A `client_id` and `client_secret` are exchanged for an access token at `POST /auth/access-tokens` on the Identity service. |

## Library-Specific Terms

| Term | Definition |
|---|---|
| **ApiResult** | The return type for all connection handler calls. Contains `IsSuccess`, rate-limit headers, `ApiError` (on failure), `Location` (for redirects), and `Data` (on success, generic `T`). |
| **ConnectorService** | Abstract base class for domain-specific service implementations. Provides `HandleResult()` (exception-throwing) and `AutoPage()` (async enumerable pagination). |
| **DataPost / DataPatch** | Generic request payload wrappers. `DataPost<TAttributes>` is used for create operations, `DataPatch<TAttributes>` (adds `id`) for update/action operations. Both serialize into the JSON:API `{ "data": {...} }` envelope. |
| **Included** | The JSON:API `included` array in a response. In this library it is typed as `IList<object>?` and accessed via `PingenSerialisationHelper.TryGetIncludedData<T>()` which finds included resources by matching `PingenApiDataType`. |
| **NonOrganisationEndpoints** | URL paths that do NOT get prefixed with `/organisations/{orgId}/`. Currently: file upload, users root, organisations root. |
| **PingenHttpClients** | Wrapper grouping the three `HttpClient` instances: `Identity`, `Api`, `External`. |
