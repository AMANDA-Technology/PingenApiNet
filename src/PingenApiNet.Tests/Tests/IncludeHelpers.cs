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

using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Batches;
using PingenApiNet.Abstractions.Models.LetterEvents;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Organisations;
using PingenApiNet.Abstractions.Models.UserAssociations;
using PingenApiNet.Abstractions.Models.Users;
using PingenApiNet.Abstractions.Models.Webhooks;
using PingenApiNet.Abstractions.Models.Webhooks.WebhookEvents;

namespace PingenApiNet.Tests.Tests;

/// <summary>
/// Offline unit tests for include helper static classes
/// </summary>
public class IncludeHelpers
{
    /// <summary>
    /// Verifies LetterIncludes constant values match relationship JSON property names
    /// </summary>
    [Test]
    public void LetterIncludes_ConstantsMatchRelationshipNames()
    {
        Assert.Multiple(() =>
        {
            Assert.That(LetterIncludes.Organisation, Is.EqualTo("organisation"));
            Assert.That(LetterIncludes.Events, Is.EqualTo("events"));
        });
    }

    /// <summary>
    /// Verifies BatchIncludes constant values match relationship JSON property names
    /// </summary>
    [Test]
    public void BatchIncludes_ConstantsMatchRelationshipNames()
    {
        Assert.Multiple(() =>
        {
            Assert.That(BatchIncludes.Organisation, Is.EqualTo("organisation"));
            Assert.That(BatchIncludes.Events, Is.EqualTo("events"));
        });
    }

    /// <summary>
    /// Verifies OrganisationIncludes constant values match relationship JSON property names
    /// </summary>
    [Test]
    public void OrganisationIncludes_ConstantsMatchRelationshipNames()
    {
        Assert.That(OrganisationIncludes.Associations, Is.EqualTo("associations"));
    }

    /// <summary>
    /// Verifies UserIncludes constant values match relationship JSON property names
    /// </summary>
    [Test]
    public void UserIncludes_ConstantsMatchRelationshipNames()
    {
        Assert.Multiple(() =>
        {
            Assert.That(UserIncludes.Associations, Is.EqualTo("associations"));
            Assert.That(UserIncludes.Notifications, Is.EqualTo("notifications"));
        });
    }

    /// <summary>
    /// Verifies UserAssociationIncludes constant values match relationship JSON property names
    /// </summary>
    [Test]
    public void UserAssociationIncludes_ConstantsMatchRelationshipNames()
    {
        Assert.That(UserAssociationIncludes.Organisation, Is.EqualTo("organisation"));
    }

    /// <summary>
    /// Verifies LetterEventIncludes constant values match relationship JSON property names
    /// </summary>
    [Test]
    public void LetterEventIncludes_ConstantsMatchRelationshipNames()
    {
        Assert.That(LetterEventIncludes.Letter, Is.EqualTo("letter"));
    }

    /// <summary>
    /// Verifies WebhookIncludes constant values match relationship JSON property names
    /// </summary>
    [Test]
    public void WebhookIncludes_ConstantsMatchRelationshipNames()
    {
        Assert.That(WebhookIncludes.Organisation, Is.EqualTo("organisation"));
    }

    /// <summary>
    /// Verifies WebhookEventIncludes constant values match relationship JSON property names
    /// </summary>
    [Test]
    public void WebhookEventIncludes_ConstantsMatchRelationshipNames()
    {
        Assert.Multiple(() =>
        {
            Assert.That(WebhookEventIncludes.Organisation, Is.EqualTo("organisation"));
            Assert.That(WebhookEventIncludes.Letter, Is.EqualTo("letter"));
            Assert.That(WebhookEventIncludes.Event, Is.EqualTo("event"));
        });
    }

    /// <summary>
    /// Verifies that include helper constants can be used with ApiRequest.Include
    /// </summary>
    [Test]
    public void IncludeConstants_CanBeUsedInApiRequest()
    {
        var request = new ApiRequest
        {
            Include = [LetterIncludes.Organisation, LetterIncludes.Events]
        };

        Assert.Multiple(() =>
        {
            Assert.That(request.Include, Is.EquivalentTo(new[] { "organisation", "events" }));
            Assert.That(string.Join(',', request.Include!), Is.EqualTo("organisation,events"));
        });
    }
}
