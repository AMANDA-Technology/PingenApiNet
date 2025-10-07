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
using PingenApiNet.Abstractions.Interfaces.Data;
using PingenApiNet.Abstractions.Models.Api;

namespace PingenApiNet.Interfaces;

/// <summary>
/// Connection handler to call Pingen REST API
/// </summary>
public interface IPingenConnectionHandler
{
    /// <summary>
    /// Change the organisation ID to use for upcoming requests
    /// </summary>
    /// <param name="organisationId">Id to use for all requests at /organisations/{organisationId}/*</param>
    public void SetOrganisationId(string organisationId);

    /// <summary>
    /// Base GET request
    /// </summary>
    /// <param name="requestPath">Relative request path</param>
    /// <param name="apiPagingRequest">Optional, Request meta information to send to the API</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public Task<ApiResult<TResult>> GetAsync<TResult>(string requestPath, [Optional] ApiPagingRequest? apiPagingRequest, [Optional] CancellationToken cancellationToken) where TResult : IDataResult;

    /// <summary>
    /// Base GET request
    /// </summary>
    /// <param name="requestPath">Relative request path</param>
    /// <param name="apiPagingRequest">Optional, Request meta information to send to the API</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult> GetAsync(string requestPath, [Optional] ApiPagingRequest? apiPagingRequest, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Base POST request with payload
    /// </summary>
    /// <param name="requestPath">Relative request path</param>
    /// <param name="dataPost">Data POST object to send to the API</param>
    /// <param name="idempotencyKey">Optional, unique request identifier for idempotency. To be able to safely retry these kind of API calls, you can set the HTTP Header Idempotency-Key with any unique 1-64 character string. <see href="https://api.pingen.com/documentation#section/Advanced/Idempotency">API Doc - Idempotency</see></param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TPost"></typeparam>
    /// <returns></returns>
    public Task<ApiResult<TResult>> PostAsync<TResult, TPost>(string requestPath, TPost dataPost, [Optional] string? idempotencyKey, [Optional] CancellationToken cancellationToken) where TResult : IDataResult where TPost : IDataPost;

    /// <summary>
    /// Base DELETE request
    /// </summary>
    /// <param name="requestPath">Relative request path</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult> DeleteAsync(string requestPath, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Base PATCH request without payload
    /// </summary>
    /// <param name="requestPath">Relative request path</param>
    /// <param name="idempotencyKey">Optional, unique request identifier for idempotency. To be able to safely retry these kind of API calls, you can set the HTTP Header Idempotency-Key with any unique 1-64 character string. <see href="https://api.pingen.com/documentation#section/Advanced/Idempotency">API Doc - Idempotency</see></param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult> PatchAsync(string requestPath, [Optional] string? idempotencyKey, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Base PATCH request with payload
    /// </summary>
    /// <param name="requestPath">Relative request path</param>
    /// <param name="data">Data for PATCH</param>
    /// <param name="idempotencyKey">Optional, unique request identifier for idempotency. To be able to safely retry these kind of API calls, you can set the HTTP Header Idempotency-Key with any unique 1-64 character string. <see href="https://api.pingen.com/documentation#section/Advanced/Idempotency">API Doc - Idempotency</see></param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TPatch"></typeparam>
    /// <returns></returns>
    public Task<ApiResult<TResult>> PatchAsync<TResult, TPatch>(string requestPath, TPatch data, [Optional] string? idempotencyKey, [Optional] CancellationToken cancellationToken) where TResult : IDataResult where TPatch : IDataPatch;

    /// <summary>
    /// Send an external request. This is not an API call, but a request to an absolute URL without authentication or other pre-configured headers.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<HttpResponseMessage> SendExternalRequestAsync(HttpRequestMessage request, [Optional] CancellationToken cancellationToken);
}
