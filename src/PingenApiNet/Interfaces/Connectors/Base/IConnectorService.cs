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

using PingenApiNet.Abstractions.Exceptions;
using PingenApiNet.Abstractions.Interfaces.Data;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;

namespace PingenApiNet.Interfaces.Connectors.Base;

/// <summary>
/// Base interface for all connector services for the API
/// </summary>
public interface IConnectorService
{
    /// <summary>
    /// Handle API result, throw on error, return data from <see cref="CollectionResult{TData}"/>
    /// </summary>
    /// <param name="apiResult"></param>
    /// <typeparam name="TData"></typeparam>
    /// <returns>List of data from collection result</returns>
    /// <exception cref="PingenApiErrorException"></exception>
    public IList<TData> HandleResult<TData>(ApiResult<CollectionResult<TData>> apiResult) where TData : IData;

    /// <summary>
    /// Handle API result, throw on error, return data from <see cref="SingleResult{TData}"/>
    /// </summary>
    /// <param name="apiResult"></param>
    /// <typeparam name="TData"></typeparam>
    /// <returns></returns>
    /// <returns>List of data from collection result</returns>
    /// <exception cref="PingenApiErrorException"></exception>
    public TData? HandleResult<TData>(ApiResult<SingleResult<TData>> apiResult) where TData : IData;
}
