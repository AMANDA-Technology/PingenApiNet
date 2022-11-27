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
using PingenApiNet.Abstractions.Enums.Api;
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
    public async Task<ApiResult<CollectionResult<LetterData>>> GetPage([Optional] ApiPagingRequest? apiRequest, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<CollectionResult<LetterData>>("letters", apiRequest, cancellationToken);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IEnumerable<LetterData>> GetPageResultsAsync([EnumeratorCancellation] [Optional] CancellationToken cancellationToken)
    {
        await foreach (var page in AutoPage(async apiRequest => await GetPage(apiRequest, cancellationToken)).WithCancellation(cancellationToken))
            yield return page;
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<LetterDataDetailed>>> Create(DataPost<LetterCreate> data, [Optional] Guid? idempotencyKey, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<SingleResult<LetterDataDetailed>, DataPost<LetterCreate>>("letters", data, idempotencyKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<LetterDataDetailed>>> Send(DataPatch<LetterSend> data, [Optional] Guid? idempotencyKey, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PatchAsync<SingleResult<LetterDataDetailed>, DataPatch<LetterSend>>($"letters/{data.Id}/send", data, idempotencyKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult> Cancel(int letterId, [Optional] Guid? idempotencyKey, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PatchAsync($"letters/{letterId}/cancel", idempotencyKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<LetterDataDetailed>>> Get(int letterId, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<SingleResult<LetterDataDetailed>>(requestPath: $"letters/{letterId}", cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult> Delete(int letterId, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.DeleteAsync($"letters/{letterId}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<LetterDataDetailed>>> Update(DataPatch<LetterUpdate> data, [Optional] Guid? idempotencyKey, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PatchAsync<SingleResult<LetterDataDetailed>, DataPatch<LetterUpdate>>($"letters/{data.Id}", data, idempotencyKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult> GetFileLocation(int letterId, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync(requestPath: $"letters/{letterId}/file", cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<LetterPriceData>>> CalculatePrice(DataPost<LetterPriceConfiguration> data, [Optional] Guid? idempotencyKey, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<SingleResult<LetterPriceData>, DataPost<LetterPriceConfiguration>>("letters/price-calculator", data, idempotencyKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<CollectionResult<LetterEventData>>> GetEventsPage(string letterId, PingenApiLanguage language, [Optional] ApiPagingRequest? apiRequest, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<CollectionResult<LetterEventData>>($"letters/{letterId}/events?language={Enum.GetName(language)}", apiRequest, cancellationToken);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IEnumerable<LetterEventData>> GetEventsPageResultsAsync(string letterId, PingenApiLanguage language, [EnumeratorCancellation] [Optional] CancellationToken cancellationToken)
    {
        await foreach (var page in AutoPage(async apiRequest => await GetEventsPage(letterId, language, apiRequest, cancellationToken)).WithCancellation(cancellationToken))
            yield return page;
    }

    /// <inheritdoc />
    public async Task<ApiResult<CollectionResult<LetterEventData>>> GetIssuesPage(PingenApiLanguage language, [Optional] ApiPagingRequest? apiRequest, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<CollectionResult<LetterEventData>>($"letters/issues?language={Enum.GetName(language)}", apiRequest, cancellationToken);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IEnumerable<LetterEventData>> GetIssuesPageResultsAsync(PingenApiLanguage language, [EnumeratorCancellation] [Optional] CancellationToken cancellationToken)
    {
        await foreach (var page in AutoPage(async apiRequest => await GetIssuesPage(language, apiRequest, cancellationToken)).WithCancellation(cancellationToken))
            yield return page;
    }
}
