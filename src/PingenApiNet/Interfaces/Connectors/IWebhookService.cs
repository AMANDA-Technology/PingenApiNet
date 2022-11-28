/*
MIT License

Copyright (c) 2022 Philip Näf <philip.naef@amanda-technology.ch>
Copyright (c) 2022 Manuel Gysin <manuel.gysin@amanda-technology.ch>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Runtime.InteropServices;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Webhooks;
using PingenApiNet.Abstractions.Models.Webhooks.Views;

namespace PingenApiNet.Interfaces.Connectors;

/// <summary>
/// Pingen webhook service endpoint. <see href="https://api.v2.pingen.com/documentation#tag/organisations.management.webhooks">API Doc - Webhooks</see>
/// </summary>
public interface IWebhookService
{
    /// <summary>
    /// Get a collection of webhooks. <see href="https://api.v2.pingen.com/documentation#tag/organisations.management.webhooks/operation/webhooks.index">API Doc - Webhooks list</see>
    /// </summary>>
    /// <param name="apiRequest">Optional, Request meta information to send to the API</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult<CollectionResult<WebhookData>>> GetPage([Optional] ApiPagingRequest? apiRequest, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Call <see cref="GetPage"/> and auto page until end of collection
    /// </summary>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public IAsyncEnumerable<IEnumerable<WebhookData>> GetPageResultsAsync([Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create new webhook. <see href="https://api.v2.pingen.com/documentation#tag/organisations.management.webhooks/operation/webhooks">API Doc - Webhooks create</see>
    /// </summary>
    /// <param name="data">Data for POST</param>
    /// <param name="idempotencyKey">Optional, unique request identifier for idempotency. To be able to safely retry these kind of API calls, you can set the HTTP Header Idempotency-Key with any unique 1-64 character string. <see href="https://api.v2.pingen.com/documentation#section/Advanced/Idempotency">API Doc - Idempotency</see></param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult<SingleResult<WebhookData>>> Create(DataPost<WebhookCreate> data, [Optional] Guid? idempotencyKey, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Get details of a webhook. <see href="https://api.v2.pingen.com/documentation#tag/organisations.management.webhooks/operation/webhooks.show">API Doc - Webhooks show</see>
    /// </summary>
    /// <param name="webhookId">ID of the webhook to get</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult<SingleResult<WebhookData>>> Get(string webhookId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete a webhook. <see href="https://api.v2.pingen.com/documentation#tag/organisations.management.webhooks/operation/webhooks.destroy">API Doc - Webhooks delete</see>
    /// </summary>
    /// <param name="webhookId">ID of the webhook to delete</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult> Delete(string webhookId, [Optional] CancellationToken cancellationToken);
}
