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
using PingenApiNet.Abstractions.Models.API;
using PingenApiNet.Abstractions.Models.Data;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Interfaces;
using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Services.Connectors.Base;

namespace PingenApiNet.Services.Connectors;

/// <inheritdoc cref="PingenApiNet.Interfaces.Connectors.ILetterService" />
public sealed class LetterService : ConnectorService, ILetterService
{
    /// <summary>
    /// Pingen connection handler
    /// </summary>
    private readonly IPingenConnectionHandler _pingenConnectionHandler;

    /// <summary>
    /// Inject connection handler at construction
    /// </summary>
    /// <param name="pingenConnectionHandler"></param>
    public LetterService(IPingenConnectionHandler pingenConnectionHandler)
    {
        _pingenConnectionHandler = pingenConnectionHandler;
    }

    /// <inheritdoc />
    public async Task<ApiResult<CollectionResult<LetterData>>> GetPage([Optional] ApiRequest? apiRequest, [Optional] CancellationToken cancellationToken)
    {
        return await _pingenConnectionHandler.GetAsync<CollectionResult<LetterData>>("letters", apiRequest, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<LetterData>> GetPageResult([Optional] ApiRequest? apiRequest, [Optional] CancellationToken cancellationToken)
    {
        return HandleResult(await GetPage(apiRequest, cancellationToken));
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IEnumerable<LetterData>> GetPageResultsAsync([EnumeratorCancellation] [Optional] CancellationToken cancellationToken)
    {
        await foreach (var page in AutoPage(async apiRequest => await GetPage(apiRequest, cancellationToken)).WithCancellation(cancellationToken))
            yield return page;
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<LetterData>>> Create(DataPost<LetterCreate> data, [Optional] Guid? idempotencyKey, [Optional] CancellationToken cancellationToken)
    {
        return await _pingenConnectionHandler.PostAsync<SingleResult<LetterData>, DataPost<LetterCreate>>("letters", data, idempotencyKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<LetterData> CreateAndGetResult(DataPost<LetterCreate> data, [Optional] Guid? idempotencyKey, [Optional] CancellationToken cancellationToken)
    {
        return HandleResult(await Create(data, idempotencyKey, cancellationToken)) ?? throw new("Unexpected result. API returned no data after create.");
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<LetterData>>> Send(DataPatch<LetterSend> data, [Optional] Guid? idempotencyKey, [Optional] CancellationToken cancellationToken)
    {
        return await _pingenConnectionHandler.PatchAsync<SingleResult<LetterData>, DataPatch<LetterSend>>($"letters/{data.Id}/send", data, idempotencyKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult> Cancel(int letterId, [Optional] Guid? idempotencyKey, [Optional] CancellationToken cancellationToken)
    {
        return await _pingenConnectionHandler.PatchAsync($"letters/{letterId}/cancel", idempotencyKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<LetterData>>> Get(int letterId, [Optional] CancellationToken cancellationToken)
    {
        return await _pingenConnectionHandler.GetAsync<SingleResult<LetterData>>($"letters/{letterId}", null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult> Delete(int letterId, [Optional] CancellationToken cancellationToken)
    {
        return await _pingenConnectionHandler.DeleteAsync($"letters/{letterId}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<LetterData>>> Update(DataPatch<LetterUpdate> data, [Optional] Guid? idempotencyKey, [Optional] CancellationToken cancellationToken)
    {
        return await _pingenConnectionHandler.PatchAsync<SingleResult<LetterData>, DataPatch<LetterUpdate>>($"letters/{data.Id}", data, idempotencyKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult> GetFileLocation(int letterId, [Optional] CancellationToken cancellationToken)
    {
        return await _pingenConnectionHandler.GetAsync($"letters/{letterId}/file", null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<LetterPriceData>>> CalculatePrice(DataPost<LetterPriceConfiguration> data, [Optional] Guid? idempotencyKey, [Optional] CancellationToken cancellationToken)
    {
        return await _pingenConnectionHandler.PostAsync<SingleResult<LetterPriceData>, DataPost<LetterPriceConfiguration>>("letters/price-calculator", data, idempotencyKey, cancellationToken);
    }
}
