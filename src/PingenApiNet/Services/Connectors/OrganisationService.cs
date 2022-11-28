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
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Organisations;
using PingenApiNet.Interfaces;
using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Services.Connectors.Base;

namespace PingenApiNet.Services.Connectors;

/// <inheritdoc cref="PingenApiNet.Interfaces.Connectors.IOrganisationService" />
public sealed class OrganisationService : ConnectorService, IOrganisationService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrganisationService"/> class.
    /// </summary>
    /// <param name="connectionHandler"></param>
    public OrganisationService(IPingenConnectionHandler connectionHandler) : base(connectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<CollectionResult<OrganisationData>>> GetPage([Optional] ApiPagingRequest? apiRequest, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<CollectionResult<OrganisationData>>("organisations", apiRequest, cancellationToken);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IEnumerable<OrganisationData>> GetPageResultsAsync([EnumeratorCancellation] [Optional] CancellationToken cancellationToken)
    {
        await foreach (var page in AutoPage(async apiRequest => await GetPage(apiRequest, cancellationToken)).WithCancellation(cancellationToken))
            yield return page;
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<OrganisationDataDetailed>>> Get(string organisationId, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<SingleResult<OrganisationDataDetailed>>(requestPath: $"organisations/{organisationId}", cancellationToken: cancellationToken);
    }
}
