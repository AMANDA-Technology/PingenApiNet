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
using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Interfaces.Data;

namespace PingenApiNet.Abstractions.Models.Letters;

/// <summary>
/// Letter abilities
/// </summary>
/// <param name="Cancel"></param>
/// <param name="Delete"></param>
/// <param name="Submit"></param>
/// <param name="SendSimplex"></param>
/// <param name="Edit"></param>
/// <param name="GetPdfRaw"></param>
/// <param name="GetPdfValidation"></param>
/// <param name="ChangePaperType"></param>
/// <param name="ChangeWindowPosition"></param>
/// <param name="CreateCoverPage"></param>
/// <param name="FixOverwriteRestrictedAreas"></param>
/// <param name="FixCoverPage"></param>
/// <param name="FixRegularPaper"></param>
public sealed record LetterAbilities(
    [property: JsonPropertyName("cancel")] PingenApiAbility Cancel,
    [property: JsonPropertyName("delete")] PingenApiAbility Delete,
    [property: JsonPropertyName("submit")] PingenApiAbility Submit,
    [property: JsonPropertyName("send-simplex")] PingenApiAbility SendSimplex,
    [property: JsonPropertyName("edit")] PingenApiAbility Edit,
    // ReSharper disable once InconsistentNaming
    [property: JsonPropertyName("get-pdf-raw")] PingenApiAbility GetPdfRaw,
    // ReSharper disable once InconsistentNaming
    [property: JsonPropertyName("get-pdf-validation")] PingenApiAbility GetPdfValidation,
    [property: JsonPropertyName("change-paper-type")] PingenApiAbility ChangePaperType,
    [property: JsonPropertyName("change-window-position")] PingenApiAbility ChangeWindowPosition,
    [property: JsonPropertyName("create-coverpage")] PingenApiAbility CreateCoverPage,
    [property: JsonPropertyName("fix-overwrite-restricted-areas")] PingenApiAbility FixOverwriteRestrictedAreas,
    [property: JsonPropertyName("fix-coverpage")] PingenApiAbility FixCoverPage,
    [property: JsonPropertyName("fix-regular-paper")] PingenApiAbility FixRegularPaper
) : IAbilities;
