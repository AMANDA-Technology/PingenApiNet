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
using System.Xml;
using PingenApiNet.Abstractions.Exceptions;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.LetterEvents;
using PingenApiNet.Abstractions.Models.LetterPrices;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Letters.Views;
using PingenApiNet.Interfaces;
using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Services.Connectors.Base;
using PingenApiNet.Services.Connectors.Endpoints;

namespace PingenApiNet.Services.Connectors;

/// <inheritdoc cref="PingenApiNet.Interfaces.Connectors.ILetterService" />
public sealed class LetterService : ConnectorService, ILetterService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LetterService"/> class.
    /// </summary>
    /// <param name="connectionHandler"></param>
    public LetterService(IPingenConnectionHandler connectionHandler) : base(connectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<CollectionResult<LetterData>>> GetPage([Optional] ApiPagingRequest? apiPagingRequest, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<CollectionResult<LetterData>>(LettersEndpoints.Root, apiPagingRequest, cancellationToken);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IEnumerable<LetterData>> GetPageResultsAsync([Optional] ApiPagingRequest? apiPagingRequest, [EnumeratorCancellation] [Optional] CancellationToken cancellationToken)
    {
        await foreach (var page in AutoPage(apiPagingRequest, async apiRequest => await GetPage(apiRequest, cancellationToken)).WithCancellation(cancellationToken))
            yield return page;
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<LetterDataDetailed>>> Create(DataPost<LetterCreate, LetterCreateRelationships> data, [Optional] string? idempotencyKey, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<SingleResult<LetterDataDetailed>, DataPost<LetterCreate, LetterCreateRelationships>>(LettersEndpoints.Root, data, idempotencyKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<LetterDataDetailed>>> Send(DataPatch<LetterSend> data, [Optional] string? idempotencyKey, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PatchAsync<SingleResult<LetterDataDetailed>, DataPatch<LetterSend>>(LettersEndpoints.Send(data.Id), data, idempotencyKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult> Cancel(string letterId, [Optional] string? idempotencyKey, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PatchAsync(LettersEndpoints.Cancel(letterId), idempotencyKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<LetterDataDetailed>>> Get(string letterId, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<SingleResult<LetterDataDetailed>>(requestPath: LettersEndpoints.Single(letterId), cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult> Delete(string letterId, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.DeleteAsync(LettersEndpoints.Single(letterId), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<LetterDataDetailed>>> Update(DataPatch<LetterUpdate> data, [Optional] string? idempotencyKey, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PatchAsync<SingleResult<LetterDataDetailed>, DataPatch<LetterUpdate>>(LettersEndpoints.Single(data.Id), data, idempotencyKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult> GetFileLocation(string letterId, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync(requestPath: LettersEndpoints.File(letterId), cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MemoryStream> DownloadFileContent(Uri fileUrl, [Optional] CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();
        var fileContentResponse = await httpClient.GetAsync(fileUrl, cancellationToken);
        if (!fileContentResponse.IsSuccessStatusCode)
        {
            var errorContent = await fileContentResponse.Content.ReadAsStringAsync(cancellationToken);
            var xml = new XmlDocument();
            xml.LoadXml(errorContent);
            throw new PingenFileDownloadException(xml.SelectSingleNode("/Error/Code/text()")?.Value ?? string.Empty);
        }

        await fileContentResponse.Content.LoadIntoBufferAsync();
        return (MemoryStream) await fileContentResponse.Content.ReadAsStreamAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<LetterPriceData>>> CalculatePrice(DataPost<LetterPriceConfiguration> data, [Optional] string? idempotencyKey, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<SingleResult<LetterPriceData>, DataPost<LetterPriceConfiguration>>(LettersEndpoints.PriceCalculator, data, idempotencyKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<CollectionResult<LetterEventData>>> GetEventsPage(string letterId, string language, [Optional] ApiPagingRequest? apiPagingRequest, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<CollectionResult<LetterEventData>>(LettersEndpoints.Events(letterId, language), apiPagingRequest, cancellationToken);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IEnumerable<LetterEventData>> GetEventsPageResultsAsync(string letterId, string language, [Optional] ApiPagingRequest? apiPagingRequest, [EnumeratorCancellation] [Optional] CancellationToken cancellationToken)
    {
        await foreach (var page in AutoPage(apiPagingRequest, async apiRequest => await GetEventsPage(letterId, language, apiRequest, cancellationToken)).WithCancellation(cancellationToken))
            yield return page;
    }

    /// <inheritdoc />
    public async Task<ApiResult<CollectionResult<LetterEventData>>> GetIssuesPage(string language, [Optional] ApiPagingRequest? apiPagingRequest, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<CollectionResult<LetterEventData>>(LettersEndpoints.Issues(language), apiPagingRequest, cancellationToken);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IEnumerable<LetterEventData>> GetIssuesPageResultsAsync(string language, [Optional] ApiPagingRequest? apiPagingRequest, [EnumeratorCancellation] [Optional] CancellationToken cancellationToken)
    {
        await foreach (var page in AutoPage(apiPagingRequest, async apiRequest => await GetIssuesPage(language, apiRequest, cancellationToken)).WithCancellation(cancellationToken))
            yield return page;
    }
}
