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

using PingenApiNet.Abstractions.Models.Data;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Pagination;

namespace PingenApiNet.Interfaces.Connectors;

/// <summary>
/// Pingen letter service endpoint. <see href="https://api.v2.pingen.com/documentation#tag/letters.general">API Doc - Letters General</see>
/// </summary>
public interface ILetterService
{
    /// <summary>
    /// Get a collection of letters. <see href="https://api.v2.pingen.com/documentation#tag/letters.general/operation/letters.list">API Doc - Letters list</see>
    /// </summary>
    /// <returns></returns>
    public Task<GetListResult<LetterData>> GetAll();

    /// <summary>
    /// Create a new letter. <see href="https://api.v2.pingen.com/documentation#tag/letters.general/operation/letters.create">API Doc - Letters create</see>
    /// </summary>
    /// <returns></returns>
    public Task<GetSingleResult<LetterData>> Create(DataPost<LetterCreate> data);
}
