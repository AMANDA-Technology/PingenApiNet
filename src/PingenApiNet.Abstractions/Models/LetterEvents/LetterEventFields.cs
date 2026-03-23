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

namespace PingenApiNet.Abstractions.Models.LetterEvents;

/// <summary>
/// Available sparse fieldset field names for the LetterEvent resource.
/// Pass one or more of these constants as field values in <see cref="Api.ApiRequest.SparseFieldsets"/>
/// to request only specific attributes in the response.
/// <see href="https://api.pingen.com/documentation#section/Advanced/Sparse-fieldsets">API Doc - Sparse fieldsets</see>
/// </summary>
public static class LetterEventFields
{
    /// <summary>Event code (see LetterEventCodes)</summary>
    public const string Code = "code";

    /// <summary>Human-readable event name</summary>
    public const string Name = "name";

    /// <summary>Event producer (postal carrier etc.)</summary>
    public const string Producer = "producer";

    /// <summary>Physical location associated with the event</summary>
    public const string Location = "location";

    /// <summary>Whether the event has an associated image</summary>
    public const string HasImage = "has_image";

    /// <summary>Additional event data payload</summary>
    public const string Data = "data";

    /// <summary>Timestamp when the event was emitted by the carrier</summary>
    public const string EmittedAt = "emitted_at";

    /// <summary>Timestamp when the event was created in Pingen</summary>
    public const string CreatedAt = "created_at";

    /// <summary>Timestamp when the event was last updated</summary>
    public const string UpdatedAt = "updated_at";
}
