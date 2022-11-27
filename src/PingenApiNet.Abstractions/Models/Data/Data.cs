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

using System.Text.Json.Serialization;
using PingenApiNet.Abstractions.Interfaces.Data;

namespace PingenApiNet.Abstractions.Models.Data;

/// <inheritdoc cref="IData" />
public abstract record Data : DataIdentity, IData
{
    /// <inheritdoc />
    [JsonPropertyName("links")]
    public required DataLinks Links { get; init; }
}

/// <inheritdoc cref="IData{TAttributes}" />
public abstract record Data<TAttributes> : Data, IData<TAttributes>
    where TAttributes : IAttributes
{
    /// <inheritdoc />
    [JsonPropertyName("attributes")]
    public required TAttributes Attributes { get; init; }
}

/// <inheritdoc cref="IData{TAttributes,TRelationships}" />
public abstract record Data<TAttributes, TRelationships> : Data<TAttributes>, IData<TAttributes, TRelationships>
    where TAttributes : IAttributes
    where TRelationships : IRelationships
{
    /// <inheritdoc />
    [JsonPropertyName("relationships")]
    public required TRelationships Relationships { get; init; }
}
