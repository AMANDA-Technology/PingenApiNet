---
title: "ADR-003: IAsyncEnumerable for Auto-Pagination"
tags: [adr, pagination, async]
---

# ADR-003: IAsyncEnumerable for Auto-Pagination

## Context

The Pingen API uses cursor-based pagination. Collection responses include `links` (first, prev, next, last URLs) and `meta` with `currentPage` / `lastPage` counters. Consumers often need all pages of a collection (e.g., all letters), which requires looping until the last page.

The API also supports filter, sort, search, and manual page control via `ApiPagingRequest`.

## Decision

Each connector service exposes two variants for every collection endpoint:

1. `GetPage([ApiPagingRequest?])` — returns `Task<ApiResult<CollectionResult<TData>>>`. Returns a single page. For callers that want full control over paging and error handling.

2. `GetPageResultsAsync([ApiPagingRequest?])` — returns `IAsyncEnumerable<IEnumerable<TData>>`. Automatically iterates all pages starting from `PageNumber=1` (or the caller's requested starting page). Throws `PingenApiErrorException` on any page failure.

The `AutoPage<TData>` method in `ConnectorService` implements the shared looping logic using `yield return` inside an `async IAsyncEnumerable`. It increments `PageNumber` by 1 after each successful page fetch, stopping when `Meta.CurrentPage >= Meta.LastPage`.

## Consequences

**Good:**
- `IAsyncEnumerable` allows consumers to process results page-by-page without accumulating the full collection in memory.
- `await foreach (var page in client.Letters.GetPageResultsAsync(request))` is ergonomic and idiomatic for .NET.
- Cancellation token is propagated using `[EnumeratorCancellation]`.

**Bad / Watch out:**
- The `*PageResultsAsync` methods throw `PingenApiErrorException` rather than returning a failed `ApiResult`. Callers must catch this exception, unlike the `GetPage` variant which returns a result object.
- The auto-pagination increments `page[number]` linearly. This is not a cursor-based approach — if the collection changes (new items added/removed) between pages, results may be inconsistent. This is a limitation of the Pingen API's paging model, not this library.
- Sort and filter parameters from `ApiPagingRequest` are carried forward to all subsequent pages. The `PageNumber` and `PageLimit` from the caller's request are the starting point, not overridden.
