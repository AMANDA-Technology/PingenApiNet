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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Webhooks;
using PingenApiNet.Abstractions.Models.Webhooks.Views;
using PingenApiNet.Interfaces;
using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Services.Connectors.Base;

namespace PingenApiNet.Services.Connectors;

/// <inheritdoc cref="PingenApiNet.Interfaces.Connectors.IWebhookService" />
public class WebhookService : ConnectorService, IWebhookService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookService"/> class.
    /// </summary>
    /// <param name="connectionHandler"></param>
    public WebhookService(IPingenConnectionHandler connectionHandler) : base(connectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<CollectionResult<WebhookData>>> GetPage([Optional] ApiPagingRequest? apiRequest, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<CollectionResult<WebhookData>>("webhooks", apiRequest, cancellationToken);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IEnumerable<WebhookData>> GetPageResultsAsync([EnumeratorCancellation] [Optional] CancellationToken cancellationToken)
    {
        await foreach (var page in AutoPage(async apiRequest => await GetPage(apiRequest, cancellationToken)).WithCancellation(cancellationToken))
            yield return page;
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<WebhookData>>> Create(DataPost<WebhookCreate> data, [Optional] Guid? idempotencyKey, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<SingleResult<WebhookData>, DataPost<WebhookCreate>>("webhooks", data, idempotencyKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<WebhookData>>> Get(int webhookId, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<SingleResult<WebhookData>>(requestPath: $"webhooks/{webhookId}", cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult> Delete(int webhookId, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.DeleteAsync($"webhooks/{webhookId}", cancellationToken);
    }
}
