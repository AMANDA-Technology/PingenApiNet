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

using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Exceptions;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Base;
using PingenApiNet.Abstractions.Models.LetterEvents;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Organisations;
using PingenApiNet.Abstractions.Models.Webhooks.WebhookEvents;

namespace PingenApiNet.Abstractions.Helpers;

/// <summary>
///
/// </summary>
public static class PingenWebhookHelper
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="signingKey"></param>
    /// <param name="signature"></param>
    /// <param name="requestStream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>webhook event, included organisation, included letter, included letter event</returns>
    /// <exception cref="PingenWebhookValidationErrorException"></exception>
    public static async Task<(
        WebhookEventData? webhookEventData,
        Data<Organisation>? organisationData,
        Data<Letter>? letterData,
        Data<LetterEvent>? letterEventData)>
        ValidateWebhookAndGetData(string signingKey, string signature, Stream requestStream, [Optional] CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(requestStream);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        requestStream.Position = 0;

        if (!await ValidateWebhook(signingKey, signature, requestStream, cancellationToken))
        {
            throw new PingenWebhookValidationErrorException(
                PingenSerialisationHelper.Deserialize<WebhookEventData>(payload)
                ?? new WebhookEventData
                {
                    Id = string.Empty,
                    Type = PingenApiDataType.letters,
                    Links = null!,
                    Attributes = null!,
                    Relationships = null!
                },
                "Validation of webhook signature failed");
        }

        var webhookEventData = PingenSerialisationHelper.Deserialize<SingleResult<WebhookEventData>>(payload);
        PingenSerialisationHelper.TryGetIncludedData(webhookEventData!, out Data<Organisation>? organisationData);
        PingenSerialisationHelper.TryGetIncludedData(webhookEventData!, out Data<Letter>? letterData);
        PingenSerialisationHelper.TryGetIncludedData(webhookEventData!, out Data<LetterEvent>? letterEventData);

        return (webhookEventData?.Data, organisationData, letterData, letterEventData);

    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="signingKey"></param>
    /// <param name="signature"></param>
    /// <param name="payload"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<bool> ValidateWebhook(string signingKey, string signature, Stream payload, [Optional] CancellationToken cancellationToken)
    {
        var keyByte = Encoding.UTF8.GetBytes(signingKey);
        using var hmacsha256 = new HMACSHA256(keyByte);
        await hmacsha256.ComputeHashAsync(payload, cancellationToken);

        return hmacsha256.Hash != null && signature == ByteToString(hmacsha256.Hash);
    }

    /// <summary>
    /// Create string vom byte array
    /// </summary>
    /// <param name="buff"></param>
    /// <returns></returns>
    private static string ByteToString(IEnumerable<byte> buff)
    {
        return buff.Aggregate("", (current, t) => current + t.ToString("x2"));
    }
}
