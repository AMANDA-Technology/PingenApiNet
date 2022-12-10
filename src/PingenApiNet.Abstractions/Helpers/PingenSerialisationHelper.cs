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
using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Helpers.JsonConverters;
using PingenApiNet.Abstractions.Interfaces.Data;
using PingenApiNet.Abstractions.Models.Base;
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
    /// Json serializer options with default settings and custom converters
    /// </summary>
    /// <returns></returns>
    private static JsonSerializerOptions SerializerOptions()
    {
        var a = new JsonSerializerOptions();
        a.Converters.Add(new PingenDateTimeConverter());
        a.Converters.Add(new PingenDateTimeNullableConverter());
        a.Converters.Add(new PingenKeyValuePairStringObjectConverter());
        a.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        return a;
    }

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
    ///
    /// </summary>
    /// <param name="dataResult"></param>
    /// <param name="included"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static bool TryGetIncludedData<T>(IDataResult dataResult, out Data<T>? included) where T : IAttributes
    {
        if (dataResult.Included
            ?.SingleOrDefault(include => include is JsonElement jsonElement
                                         && Deserialize<DataIdentity>(jsonElement.GetRawText()) is { } dataIdentity
                                         && PingenApiDataTypeMapping[dataIdentity.Type] == typeof(T))
            is JsonElement includeJsonElement)
        {
            included = Deserialize<Data<T>>(includeJsonElement.GetRawText());
            return true;
        }

        included = default;
        return false;
    }
}
