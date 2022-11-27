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
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Organisations;
using PingenApiNet.Interfaces.Connectors.Base;

namespace PingenApiNet.Interfaces.Connectors;

/// <summary>
/// Pingen organisation service endpoint. <see href="https://api.v2.pingen.com/documentation#tag/organisations.general">API Doc - Organisations General</see>
/// </summary>
public interface IOrganisationService : IConnectorService
{
    /// <summary>
    /// Get collection of organisations. <see href="https://api.v2.pingen.com/documentation#tag/organisations.general/operation/organisations.index">API Doc - Organisations list</see>
    /// </summary>>
    /// <param name="apiRequest">Optional, Request meta information to send to the API</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult<CollectionResult<OrganisationData>>> GetPage([Optional] ApiPagingRequest? apiRequest, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Call <see cref="GetPage"/> and auto page until end of collection
    /// </summary>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public IAsyncEnumerable<IEnumerable<OrganisationData>> GetPageResultsAsync([Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Get details of an organisation. <see href="https://api.v2.pingen.com/documentation#tag/organisations.general/operation/organisations.show">API Doc - Organisations show</see>
    /// </summary>
    /// <param name="organisationId">ID of the organisation to get</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult<SingleResult<OrganisationDataDetailed>>> Get(int organisationId, [Optional] CancellationToken cancellationToken);
}
