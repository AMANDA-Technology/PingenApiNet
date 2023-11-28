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

namespace PingenApiNet.Abstractions.Enums.Letters;

/// <summary>
/// Letter paper type. <see href="https://api.pingen.com/documentation#tag/letters.general/operation/letters.price-calculator">API Doc - Letters price calculator</see>
/// </summary>
public static class LetterPaperTypes
{
    /// <summary>
    /// Normal paper
    /// </summary>
    public const string Normal = "normal";

    /// <summary>
    /// QR paper
    /// </summary>
    public const string Qr = "qr";

    /// <summary>
    /// IS paper
    /// </summary>
    public const string Is = "is";

    /// <summary>
    /// ISR paper
    /// </summary>
    public const string Isr = "isr";

    /// <summary>
    /// ISR+ paper
    /// </summary>
    public const string IsrPlus = "isr+";

    /// <summary>
    /// Sepa AT paper
    /// </summary>
    public const string SepaAt = "sepa_at";

    /// <summary>
    /// Sepa DE paper
    /// </summary>
    public const string SepaDe = "sepa_de";
}
