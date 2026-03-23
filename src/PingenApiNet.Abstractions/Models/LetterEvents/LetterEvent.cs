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
using PingenApiNet.Abstractions.Enums.LetterEvents;
using PingenApiNet.Abstractions.Interfaces.Data;

namespace PingenApiNet.Abstractions.Models.LetterEvents;

/// <summary>
/// Letter event
/// </summary>
/// <param name="Code">Probably any of <see cref="LetterEventCodes"/>, but there is nothing about it in API Doc...</param>
/// <param name="Name"></param>
/// <param name="Producer"></param>
/// <param name="Location"></param>
/// <param name="HasImage"></param>
/// <param name="Data"></param>
/// <param name="EmittedAt"></param>
/// <param name="CreatedAt"></param>
/// <param name="UpdatedAt"></param>
public sealed record LetterEvent(
    [property: JsonPropertyName(LetterEventFields.Code)] string? Code,
    [property: JsonPropertyName(LetterEventFields.Name)] string? Name,
    [property: JsonPropertyName(LetterEventFields.Producer)] string? Producer,
    [property: JsonPropertyName(LetterEventFields.Location)] string? Location,
    [property: JsonPropertyName(LetterEventFields.HasImage)] bool? HasImage,
    [property: JsonPropertyName(LetterEventFields.Data)] IReadOnlyList<string>? Data,
    [property: JsonPropertyName(LetterEventFields.EmittedAt)] DateTime? EmittedAt,
    [property: JsonPropertyName(LetterEventFields.CreatedAt)] DateTime? CreatedAt,
    [property: JsonPropertyName(LetterEventFields.UpdatedAt)] DateTime? UpdatedAt
) : IAttributes;
