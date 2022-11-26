﻿/*
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

using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Interfaces.Data;
using PingenApiNet.Abstractions.Models.API;
using PingenApiNet.Abstractions.Models.Data;
using PingenApiNet.Interfaces.Connectors.Base;

namespace PingenApiNet.Services.Connectors.Base;

/// <inheritdoc />
public abstract class ConnectorService : IConnectorService
{
    /// <inheritdoc />
    public abstract ApiRequest<DataPost<TData>> GetDefaultApiRequest<TData>(TData data);

    /// <inheritdoc />
    public List<TData> HandleResult<TData>(ApiResult<CollectionResult<TData>> apiResult) where TData : IData
    {
        if (!apiResult.IsSuccess)
            throw new PingenApiErrorException(apiResult);

        return apiResult.Data is null
            ? new()
            : apiResult.Data.Data.ToList();
    }

    /// <inheritdoc />
    public TData? HandleResult<TData>(ApiResult<SingleResult<TData>> apiResult) where TData : IData
    {
        if (!apiResult.IsSuccess)
            throw new PingenApiErrorException(apiResult);

        return apiResult.Data is null
            ? default
            : apiResult.Data.Data;
    }

    /// <summary>
    /// Generic auto page async
    /// </summary>
    /// <param name="getPage">Function to get page</param>
    /// <typeparam name="TData"></typeparam>
    /// <returns></returns>
    protected async IAsyncEnumerable<IEnumerable<TData>> AutoPage<TData>(Func<ApiRequest, Task<ApiResult<CollectionResult<TData>>>> getPage) where TData : IData
    {
        var apiReRequest = new ApiRequest
        {
            IdempotencyKey = Guid.NewGuid(),
            PageNumber = 1
        };

        ApiResult<CollectionResult<TData>> result;
        do
        {
            result = await getPage.Invoke(apiReRequest);
            HandleResult(result);
            yield return result.Data?.Data ?? Enumerable.Empty<TData>();

            apiReRequest = apiReRequest with
            {
                IdempotencyKey = Guid.NewGuid(),
                PageNumber = apiReRequest.PageNumber + 1
            };
        } while (result.Data?.Meta is not null && result.Data.Meta.CurrentPage < result.Data.Meta.LastPage);
    }
}