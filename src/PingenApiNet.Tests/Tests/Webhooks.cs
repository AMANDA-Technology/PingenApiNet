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

using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Base;
using PingenApiNet.Abstractions.Models.LetterEvents;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Organisations;
using PingenApiNet.Abstractions.Models.Webhooks.WebhookEvents;

namespace PingenApiNet.Tests.Tests;

/// <summary>
///
/// </summary>
public class Webhooks : TestBase
{
    /// <summary>
    ///
    /// </summary>
    [Test]
    public async Task DeserializeWebhookEventData()
    {
        var webhookBody = await File.ReadAllTextAsync("Assets/webhook_sample.json");
        Assert.That(webhookBody, Is.Not.Empty);

        var webhookEventData = PingenSerialisationHelper.Deserialize<SingleResult<WebhookEventData>>(webhookBody);
        Assert.That(webhookEventData?.Data.Attributes, Is.Not.Null);

        var includedOrganisationFound = PingenSerialisationHelper.TryGetIncludedData(webhookEventData!, out Data<Organisation>? organisationData);
        Assert.Multiple(() =>
        {
            Assert.That(includedOrganisationFound, Is.True);
            Assert.That(organisationData, Is.Not.Null);
        });

        var letterFound = PingenSerialisationHelper.TryGetIncludedData<Letter>(webhookEventData!, out var letterData);
        Assert.Multiple(() =>
        {
            Assert.That(letterFound, Is.True);
            Assert.That(letterData, Is.Not.Null);
        });

        Data<LetterEvent>? letterEventData;
        var letterEventFound = PingenSerialisationHelper.TryGetIncludedData(webhookEventData!, out letterEventData);
        Assert.Multiple(() =>
        {
            Assert.That(letterEventFound, Is.True);
            Assert.That(letterEventData, Is.Not.Null);
        });
    }
}
