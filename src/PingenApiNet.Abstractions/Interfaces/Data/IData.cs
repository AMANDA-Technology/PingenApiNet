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

using PingenApiNet.Abstractions.Models.Base.Embedded;

namespace PingenApiNet.Abstractions.Interfaces.Data;

/// <summary>
/// Base data object interface without the actual data, but can be used to ensure an object is a data object
/// </summary>
public interface IData
{
    /// <summary>
    /// Data links object
    /// </summary>
    public DataLinks? Links { get; init; }
}

/// <summary>
/// Base data object including type based data
/// </summary>
/// <typeparam name="TAttributes"></typeparam>
public interface IData<TAttributes> : IData
    where TAttributes : IAttributes
{
    /// <summary>
    /// Object data attributes based on type
    /// </summary>
    public TAttributes Attributes { get; init; }
}

/// <summary>
/// Base data object including type based data and relationships
/// </summary>
/// <typeparam name="TAttributes"></typeparam>
/// <typeparam name="TRelationships"></typeparam>
public interface IData<TAttributes, TRelationships> : IData<TAttributes>
    where TAttributes : IAttributes
    where TRelationships : IRelationships
{
    /// <summary>
    /// Relationships based on type
    /// </summary>
    public TRelationships Relationships { get; init; }
}

/// <summary>
/// Base data object including type based data, relationships and meta information
/// </summary>
/// <typeparam name="TAttributes"></typeparam>
/// <typeparam name="TRelationships"></typeparam>
/// <typeparam name="TMeta"></typeparam>
public interface IData<TAttributes, TRelationships, TMeta> : IData<TAttributes, TRelationships>
    where TAttributes : IAttributes
    where TRelationships : IRelationships
    where TMeta : IMeta
{
    /// <summary>
    /// Meta information based on type
    /// </summary>
    public TMeta Meta { get; init; }
}
