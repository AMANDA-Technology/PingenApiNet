/*
MIT License

Copyright (c) 2024 Dejan Appenzeller <dejan.appenzeller@swisspeers.ch>

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
using PingenApiNet.Abstractions.Models.Batches;
using PingenApiNet.Abstractions.Models.Batches.Views;
using PingenApiNet.Interfaces;
using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Services.Connectors.Base;
using PingenApiNet.Services.Connectors.Endpoints;

namespace PingenApiNet.Services.Connectors;

/// <inheritdoc cref="PingenApiNet.Interfaces.Connectors.IBatchService" />
public sealed class BatchService : ConnectorService, IBatchService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchService"/> class.
    /// </summary>
    /// <param name="connectionHandler"></param>
    public BatchService(IPingenConnectionHandler connectionHandler) : base(connectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<CollectionResult<BatchData>>> GetPage([Optional] ApiPagingRequest? apiPagingRequest, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<CollectionResult<BatchData>>(BatchesEndpoints.Root, apiPagingRequest, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<BatchDataDetailed>>> Create(DataPost<BatchCreate> data, [Optional] string? idempotencyKey, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<SingleResult<BatchDataDetailed>, DataPost<BatchCreate>>(requestPath: BatchesEndpoints.Root, data, idempotencyKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<BatchDataDetailed>>> Get(string batchId, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<SingleResult<BatchDataDetailed>>(requestPath: BatchesEndpoints.Single(batchId), cancellationToken: cancellationToken);
    }
}
