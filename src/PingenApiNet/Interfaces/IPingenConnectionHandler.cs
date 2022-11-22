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

namespace PingenApiNet.Interfaces;

/// <summary>
/// Connection handler to call pingen REST API
/// </summary>
public interface IPingenConnectionHandler
{
    /// <summary>
    /// Set or update access token to use for authenticated requests
    /// </summary>
    /// <returns></returns>
    public Task SetOrUpdateAccessToken();

    /// <summary>
    /// Change the organisation ID to use for upcoming requests
    /// </summary>
    /// <param name="organisationId">Id to use for all requests at /organisations/{organisationId}/*</param>
    public void SetOrganisationId(string organisationId);

    /// <summary>
    /// Base GET request
    /// </summary>
    /// <param name="requestPath">Relative request path</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<HttpResponseMessage> GetAsync(string requestPath, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Base POST request
    /// </summary>
    /// <param name="requestPath">Relative request path</param>
    /// <param name="content">The HTTP request content sent to the server</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<HttpResponseMessage> PostAsync(string requestPath, HttpContent? content, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Base DELETE request
    /// </summary>
    /// <param name="requestPath">Relative request path</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<HttpResponseMessage> DeleteAsync(string requestPath, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Base PATCH request
    /// </summary>
    /// <param name="requestPath">Relative request path</param>
    /// <param name="content">The HTTP request content sent to the server</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<HttpResponseMessage> PatchAsync(string requestPath, HttpContent? content, [Optional] CancellationToken cancellationToken);
}
