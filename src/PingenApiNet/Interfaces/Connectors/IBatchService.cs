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
using PingenApiNet.Abstractions.Models.Batches.Views;
using PingenApiNet.Abstractions.Models.Batches;

namespace PingenApiNet.Interfaces.Connectors;

/// <summary>
/// Pingen batches service endpoint. <see href="https://api.pingen.com/documentation#tag/batches.general">API Doc - Batches General</see>
/// </summary>
public interface IBatchService
{
    /// <summary>
    /// Get a collection of batches. <see href="https://api.pingen.com/documentation#tag/batches.general/operation/batches.list">API Doc - Batches list</see>
    /// </summary>>
    /// <param name="apiPagingRequest">Optional, Request meta information to send to the API</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult<CollectionResult<BatchData>>> GetPage([Optional] ApiPagingRequest? apiPagingRequest, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a new batch. <see href="https://api.pingen.com/documentation#tag/batches.general/operation/batches.create">API Doc - Batches create</see>
    /// <br/>Important: The 3-Step Process to Create a new batch
    /// <br/>1. Make a GET Request to the (File upload) endpoint to request an upload url. Use <see cref="IFilesService.GetPath"/> on <see cref="IPingenApiClient.Files"/>
    /// <br/>2. Send the raw PDF or ZIP Binary file via PUT Request (NOT Form-Post) to the url received in Step 1. Use <see cref="IFilesService.UploadFile"/> on <see cref="IPingenApiClient.Files"/>
    /// <br/>3. Make a POST Request to the Create Batch Endpoint passing the file url and file signature you received in Step 1. <see cref="Create"/>
    /// </summary>
    /// <param name="data">Data for POST</param>
    /// <param name="idempotencyKey">Optional, unique request identifier for idempotency. To be able to safely retry these kind of API calls, you can set the HTTP Header Idempotency-Key with any unique 1-64 character string. <see href="https://api.pingen.com/documentation#section/Advanced/Idempotency">API Doc - Idempotency</see></param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult<SingleResult<BatchDataDetailed>>> Create(DataPost<BatchCreate> data, [Optional] string? idempotencyKey, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Get details of a batch. <see href="https://api.pingen.com/documentation#tag/batches.general/operation/batches.show">API Doc - Get details of a batch</see>
    /// </summary>
    /// <param name="batchId">ID of the batch to get</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult<SingleResult<BatchDataDetailed>>> Get(string batchId, [Optional] CancellationToken cancellationToken);
}
