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
using PingenApiNet.Abstractions.Interfaces.Api;
using PingenApiNet.Abstractions.Models.API;

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
    /// <param name="apiRequest">Optional, Request meta information to send to the API</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public Task<ApiResult<TResult>> GetAsync<TResult>(string requestPath, [Optional] ApiRequest? apiRequest, [Optional] CancellationToken cancellationToken) where TResult : IDataResult;

    /// <summary>
    /// Base POST request
    /// </summary>
    /// <param name="requestPath">Relative request path</param>
    /// <param name="apiRequest">Request object to send to the API</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TPost"></typeparam>
    /// <returns></returns>
    public Task<ApiResult<TResult>> PostAsync<TResult, TPost>(string requestPath, ApiRequest<TPost> apiRequest, [Optional] CancellationToken cancellationToken) where TResult : IDataResult where TPost : IDataPost;

    /// <summary>
    /// Base DELETE request
    /// </summary>
    /// <param name="requestPath">Relative request path</param>
    /// <param name="apiRequest">Optional, Request meta information to send to the API</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public Task<ApiResult<TResult>> DeleteAsync<TResult>(string requestPath, [Optional] ApiRequest? apiRequest, [Optional] CancellationToken cancellationToken) where TResult : IDataResult;

    /// <summary>
    /// Base PATCH request
    /// </summary>
    /// <param name="requestPath">Relative request path</param>
    /// <param name="apiRequest">Request object to send to the API</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TPost"></typeparam>
    /// <returns></returns>
    public Task<ApiResult<TResult>> PatchAsync<TResult, TPost>(string requestPath, ApiRequest<TPost> apiRequest, [Optional] CancellationToken cancellationToken) where TResult : IDataResult where TPost : IDataPost;
}
