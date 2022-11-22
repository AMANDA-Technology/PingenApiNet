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

using System.Text;
using System.Text.Json;
using PingenApiNet.Abstractions.Models.Data;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Pagination;
using PingenApiNet.Interfaces;
using PingenApiNet.Interfaces.Connectors;

namespace PingenApiNet.Services.Connectors;

/// <summary>
/// Pingen case service endpoint
/// </summary>
public sealed class LetterService : ILetterService
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
    public async Task<GetListResult<LetterData>> GetAll()
    {
        await _pingenConnectionHandler.SetOrUpdateAccessToken();

        var response = await _pingenConnectionHandler.GetAsync("letters");

        if (!response.IsSuccessStatusCode)
        {
            // do something
            throw new();
        }

        var res = await JsonSerializer.DeserializeAsync<GetListResult<LetterData>>(await response.Content.ReadAsStreamAsync());
        if (res is null) throw new();

        return res;
    }

    /// <inheritdoc />
    public async Task<GetSingleResult<LetterData>> Create(DataPost<LetterCreate> data)
    {
        await _pingenConnectionHandler.SetOrUpdateAccessToken();

        var response = await _pingenConnectionHandler.PostAsync("letters", new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            // do something
            throw new();
        }

        var res = await JsonSerializer.DeserializeAsync<GetSingleResult<LetterData>>(await response.Content.ReadAsStreamAsync());
        if (res is null) throw new();

        return res;
    }
}
