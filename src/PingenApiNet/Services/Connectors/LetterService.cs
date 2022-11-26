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

using PingenApiNet.Abstractions.Enums;
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
    public override ApiRequest<DataPost<TData>> GetDefaultApiRequest<TData>(TData data)
    {
        return new()
        {
            IdempotencyKey = null,
            Sorting = null,
            Filtering = null,
            Searching = null,
            PageNumber = null,
            PageLimit = null,
            Data = new()
            {
                Type = PingenApiDataType.letters,
                Attributes = data
            }
        };
    }

    /// <inheritdoc />
    public async Task<ApiResult<CollectionResult<LetterData>>> GetAll()
    {
        return await _pingenConnectionHandler.GetAsync<CollectionResult<LetterData>>("letters");
    }

    /// <inheritdoc />
    public async Task<List<LetterData>> GetAllAndHandleResult()
    {
        return HandleResult(await GetAll());
    }

    /// <inheritdoc />
    public async Task<ApiResult<SingleResult<LetterData>>> Create(ApiRequest<DataPost<LetterCreate>> data)
    {
        return await _pingenConnectionHandler.PostAsync<SingleResult<LetterData>, DataPost<LetterCreate>>("letters", data);
    }

    /// <inheritdoc />
    public async Task<LetterData> CreateWithDefaultAndHandleResult(LetterCreate data)
    {
        return HandleResult(await Create(GetDefaultApiRequest(data))) ?? throw new("Unexpected result. API returned no data after create.");
    }
}
