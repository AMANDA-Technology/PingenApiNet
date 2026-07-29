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

using System.Text.Json;
using System.Text.Json.Serialization;
using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Interfaces.Data;
using PingenApiNet.Abstractions.Models.Base;

namespace PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;

/// <summary>
/// A strongly-typed wrapper around the JSON:API <c>included</c> array.
/// Stores the raw <see cref="JsonElement"/> items and provides typed access
/// via <see cref="OfType{T}"/> and <see cref="FindById{T}"/>.
/// </summary>
[JsonConverter(typeof(IncludedCollectionJsonConverter))]
public sealed class IncludedCollection
{
    /// <summary>
    /// A reusable empty <see cref="IncludedCollection"/> singleton.
    /// </summary>
    public static IncludedCollection Empty { get; } = new([]);

    /// <summary>
    /// The raw JSON elements from the <c>included</c> array.
    /// </summary>
    public IReadOnlyList<JsonElement> RawItems { get; }

    /// <summary>
    /// The number of raw items in the collection.
    /// </summary>
    public int Count => RawItems.Count;

    /// <summary>
    /// Initializes a new <see cref="IncludedCollection"/> from a list of raw JSON elements.
    /// </summary>
    /// <param name="rawItems">The JSON elements from the <c>included</c> array.</param>
    public IncludedCollection(IReadOnlyList<JsonElement> rawItems)
    {
        RawItems = rawItems;
    }

    /// <summary>
    /// Returns all included resources whose <c>type</c> maps to <typeparamref name="T"/>
    /// in <see cref="PingenSerialisationHelper.PingenApiDataTypeMapping"/>,
    /// deserialized as <see cref="Data{TAttributes}"/>.
    /// <para>
    /// When more than one <c>type</c> discriminator maps to <typeparamref name="T"/>, a single resource can
    /// arrive once under each. Since 2026-07-27 Pingen does exactly that for delivery events, emitting one
    /// event as both <c>letters_events</c> and <c>deliverables_events</c> with a shared <c>id</c> while it
    /// migrates off the old name. Those <b>cross-discriminator</b> repeats are collapsed to their first
    /// occurrence — without that, <see cref="PingenSerialisationHelper.TryGetIncludedData{T}"/>'s
    /// <c>SingleOrDefault()</c> throws and every webhook delivery fails.
    /// </para>
    /// <para>
    /// The collapse is deliberately narrow. Repeats of the same <c>id</c> under the <b>same</b> discriminator
    /// are still returned in full: that is a genuine duplicate in the API's own response rather than an
    /// artefact of the rename, and suppressing it would hide malformed data instead of surviving a migration.
    /// Resources without an <c>id</c> are never suppressed.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The attributes type to filter by (must implement <see cref="IAttributes"/>).</typeparam>
    /// <returns>An enumerable of <see cref="Data{TAttributes}"/> for each matching included resource.</returns>
    public IEnumerable<Data<T>> OfType<T>() where T : IAttributes
    {
        var mapping = PingenSerialisationHelper.PingenApiDataTypeMapping;
        var discriminatorById = new Dictionary<string, PingenApiDataType>(StringComparer.Ordinal);

        foreach (var element in RawItems)
        {
            if (!element.TryGetProperty("type", out var typeProperty))
                continue;

            if (typeProperty.GetString() is not { } typeString)
                continue;

            if (!Enum.TryParse<PingenApiDataType>(typeString, out var dataType))
                continue;

            if (!mapping.TryGetValue(dataType, out var mappedType) || mappedType != typeof(T))
                continue;

            if (element.TryGetProperty("id", out var idProperty) && idProperty.GetString() is { } idString)
            {
                if (discriminatorById.TryGetValue(idString, out var seenDataType) && seenDataType != dataType)
                    continue;

                discriminatorById[idString] = dataType;
            }

            var deserialized = PingenSerialisationHelper.Deserialize<Data<T>>(element.GetRawText());
            if (deserialized is not null)
                yield return deserialized;
        }
    }

    /// <summary>
    /// Finds the first included resource whose <c>type</c> maps to <typeparamref name="T"/>
    /// and whose <c>id</c> matches <paramref name="id"/>.
    /// </summary>
    /// <typeparam name="T">The attributes type to filter by (must implement <see cref="IAttributes"/>).</typeparam>
    /// <param name="id">The resource identifier to match.</param>
    /// <returns>The matching <see cref="Data{TAttributes}"/> or <c>null</c> if not found.</returns>
    public Data<T>? FindById<T>(string id) where T : IAttributes
    {
        var mapping = PingenSerialisationHelper.PingenApiDataTypeMapping;

        foreach (var element in RawItems)
        {
            if (!element.TryGetProperty("type", out var typeProperty))
                continue;

            if (typeProperty.GetString() is not { } typeString)
                continue;

            if (!Enum.TryParse<PingenApiDataType>(typeString, out var dataType))
                continue;

            if (!mapping.TryGetValue(dataType, out var mappedType) || mappedType != typeof(T))
                continue;

            if (!element.TryGetProperty("id", out var idProperty))
                continue;

            if (idProperty.GetString() != id)
                continue;

            return PingenSerialisationHelper.Deserialize<Data<T>>(element.GetRawText());
        }

        return null;
    }
}

/// <summary>
/// Custom JSON converter for <see cref="IncludedCollection"/>.
/// Reads a JSON array of objects into a list of <see cref="JsonElement"/> items,
/// and writes them back as a JSON array.
/// </summary>
public sealed class IncludedCollectionJsonConverter : JsonConverter<IncludedCollection>
{
    /// <inheritdoc />
    public override IncludedCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of JSON array for included collection.");

        var items = new List<JsonElement>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            var element = JsonElement.ParseValue(ref reader);
            items.Add(element);
        }

        return new IncludedCollection(items);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, IncludedCollection value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in value.RawItems)
        {
            item.WriteTo(writer);
        }
        writer.WriteEndArray();
    }
}
