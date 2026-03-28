/*
MIT License

Copyright (c) 2022 Philip Näf <philip.naef@amanda-technology.ch>
Copyright (c) 2022 Manuel Gysin <manuel.gysin@amanda-technology.ch>
Copyright (c) 2024 Dejan Appenzeller <dejan.appenzeller@swisspeers.ch>

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
using PingenApiNet.Abstractions.Helpers.JsonConverters;
using PingenApiNet.Abstractions.Interfaces.Data;
using PingenApiNet.Abstractions.Models.Base;
using PingenApiNet.Abstractions.Models.Batches;
using PingenApiNet.Abstractions.Models.DeliveryProducts;
using PingenApiNet.Abstractions.Models.Files;
using PingenApiNet.Abstractions.Models.LetterEvents;
using PingenApiNet.Abstractions.Models.LetterPrices;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Organisations;
using PingenApiNet.Abstractions.Models.UserAssociations;
using PingenApiNet.Abstractions.Models.Users;
using PingenApiNet.Abstractions.Models.Webhooks;
using PingenApiNet.Abstractions.Models.Webhooks.WebhookEvents;

namespace PingenApiNet.Abstractions.Helpers;

/// <summary>
/// Helper for JSON serialisation of Pingen API data
/// </summary>
public static class PingenSerialisationHelper
{
    /// <summary>
    /// Cached JSON serializer options with default settings and custom converters.
    /// Thread-safe once initialized; do not mutate.
    /// </summary>
    private static readonly JsonSerializerOptions CachedSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new PingenDateTimeConverter(),
            new PingenDateTimeNullableConverter(),
            new PingenKeyValuePairStringObjectConverter()
        }
    };

    /// <summary>
    /// Json serializer options with default settings and custom converters
    /// </summary>
    /// <returns></returns>
    private static JsonSerializerOptions SerializerOptions() => CachedSerializerOptions;

    /// <summary>
    /// Parses the text representing a single JSON value into an instance of the type specified by a generic type parameter.
    /// </summary>
    /// <param name="json"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, options: SerializerOptions());
    }

    /// <summary>
    /// Asynchronously reads the UTF-8 encoded text representing a single JSON value into an instance of a type specified by a generic type parameter. The stream will be read to completion.
    /// </summary>
    /// <param name="utf8Json"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async Task<T?> DeserializeAsync<T>(Stream utf8Json)
    {
        return await JsonSerializer.DeserializeAsync<T>(utf8Json, options: SerializerOptions());
    }

    /// <summary>
    /// Converts the value of a type specified by a generic type parameter into a JSON string.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string Serialize(object data)
    {
        return JsonSerializer.Serialize(data, options: SerializerOptions());
    }

    /// <summary>
    ///
    /// </summary>
    public static Dictionary<PingenApiDataType, Type> PingenApiDataTypeMapping => new()
    {
        [PingenApiDataType.letters] = typeof(Letter),
        [PingenApiDataType.batches] = typeof(Batch),
        [PingenApiDataType.organisations] = typeof(Organisation),
        [PingenApiDataType.letter_price_calculator] = typeof(LetterPrice),
        [PingenApiDataType.letters_events] = typeof(LetterEvent),
        [PingenApiDataType.users] = typeof(User),
        [PingenApiDataType.associations] = typeof(UserAssociation),
        [PingenApiDataType.webhooks] = typeof(Webhook),
        [PingenApiDataType.file_uploads] = typeof(FileUpload),
        [PingenApiDataType.webhook_issues] = typeof(WebhookEvent),
        [PingenApiDataType.webhook_sent] = typeof(WebhookEvent),
        [PingenApiDataType.webhook_undeliverable] = typeof(WebhookEvent),
        [PingenApiDataType.delivery_products] = typeof(DeliveryProduct)
    };

    /// <summary>
    /// Attempts to extract a single included resource of type <typeparamref name="T"/> from the data result.
    /// Uses <see cref="PingenApiDataTypeMapping"/> to resolve the JSON:API <c>type</c> discriminator
    /// to the .NET attributes type.
    /// </summary>
    /// <param name="dataResult">The data result containing the <c>included</c> collection.</param>
    /// <param name="included">When this method returns, contains the deserialized <see cref="Data{T}"/> if found; otherwise <c>null</c>.</param>
    /// <typeparam name="T">The attributes type to search for (must implement <see cref="IAttributes"/>).</typeparam>
    /// <returns><c>true</c> if a matching included resource was found; otherwise <c>false</c>.</returns>
    public static bool TryGetIncludedData<T>(IDataResult dataResult, out Data<T>? included) where T : IAttributes
    {
        included = dataResult.Included?.OfType<T>().SingleOrDefault();
        return included is not null;
    }
}
