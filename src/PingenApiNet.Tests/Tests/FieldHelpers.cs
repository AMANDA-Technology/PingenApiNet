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

using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Batches;
using PingenApiNet.Abstractions.Models.DeliveryProducts;
using PingenApiNet.Abstractions.Models.LetterEvents;
using PingenApiNet.Abstractions.Models.LetterPrices;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Organisations;
using PingenApiNet.Abstractions.Models.UserAssociations;
using PingenApiNet.Abstractions.Models.Users;
using PingenApiNet.Abstractions.Models.Webhooks;
using PingenApiNet.Abstractions.Models.Webhooks.WebhookEvents;

namespace PingenApiNet.Tests.Tests;

/// <summary>
/// Offline unit tests for sparse fieldset field helper static classes
/// </summary>
public class FieldHelpers
{
    /// <summary>
    /// Verifies LetterFields constant values match JsonPropertyName values in Letter record
    /// </summary>
    [Test]
    public void LetterFields_ConstantsMatchJsonPropertyNames()
    {
        Assert.Multiple(() =>
        {
            Assert.That(LetterFields.Status, Is.EqualTo(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.Status)));
            Assert.That(LetterFields.FileOriginalName, Is.EqualTo(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.FileOriginalName)));
            Assert.That(LetterFields.FilePages, Is.EqualTo(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.FilePages)));
            Assert.That(LetterFields.Address, Is.EqualTo(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.Address)));
            Assert.That(LetterFields.AddressPosition, Is.EqualTo(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.AddressPosition)));
            Assert.That(LetterFields.Country, Is.EqualTo(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.Country)));
            Assert.That(LetterFields.DeliveryProduct, Is.EqualTo(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.DeliveryProduct)));
            Assert.That(LetterFields.PrintMode, Is.EqualTo(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.PrintMode)));
            Assert.That(LetterFields.PrintSpectrum, Is.EqualTo(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.PrintSpectrum)));
            Assert.That(LetterFields.PriceCurrency, Is.EqualTo(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.PriceCurrency)));
            Assert.That(LetterFields.PriceValue, Is.EqualTo(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.PriceValue)));
            Assert.That(LetterFields.PaperTypes, Is.EqualTo(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.PaperTypes)));
            Assert.That(LetterFields.Fonts, Is.EqualTo(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.Fonts)));
            Assert.That(LetterFields.TrackingNumber, Is.EqualTo(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.TrackingNumber)));
            Assert.That(LetterFields.SubmittedAt, Is.EqualTo(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.SubmittedAt)));
            Assert.That(LetterFields.CreatedAt, Is.EqualTo(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.CreatedAt)));
            Assert.That(LetterFields.UpdatedAt, Is.EqualTo(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.UpdatedAt)));
        });
    }

    /// <summary>
    /// Verifies BatchFields constant values match JsonPropertyName values in Batch record
    /// </summary>
    [Test]
    public void BatchFields_ConstantsMatchJsonPropertyNames()
    {
        Assert.Multiple(() =>
        {
            Assert.That(BatchFields.Name, Is.EqualTo(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.Name)));
            Assert.That(BatchFields.Icon, Is.EqualTo(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.Icon)));
            Assert.That(BatchFields.Status, Is.EqualTo(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.Status)));
            Assert.That(BatchFields.FileOriginalName, Is.EqualTo(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.FileOriginalName)));
            Assert.That(BatchFields.LetterCount, Is.EqualTo(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.LetterCount)));
            Assert.That(BatchFields.AddressPosition, Is.EqualTo(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.AddressPosition)));
            Assert.That(BatchFields.PrintMode, Is.EqualTo(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.PrintMode)));
            Assert.That(BatchFields.PrintSpectrum, Is.EqualTo(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.PrintSpectrum)));
            Assert.That(BatchFields.PriceCurrency, Is.EqualTo(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.PriceCurrency)));
            Assert.That(BatchFields.PriceValue, Is.EqualTo(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.PriceValue)));
            Assert.That(BatchFields.SubmittedAt, Is.EqualTo(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.SubmittedAt)));
            Assert.That(BatchFields.CreatedAt, Is.EqualTo(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.CreatedAt)));
            Assert.That(BatchFields.UpdatedAt, Is.EqualTo(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.UpdatedAt)));
        });
    }

    /// <summary>
    /// Verifies OrganisationFields constant values match JsonPropertyName values in Organisation record
    /// </summary>
    [Test]
    public void OrganisationFields_ConstantsMatchJsonPropertyNames()
    {
        Assert.Multiple(() =>
        {
            Assert.That(OrganisationFields.Name, Is.EqualTo(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.Name)));
            Assert.That(OrganisationFields.Status, Is.EqualTo(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.Status)));
            Assert.That(OrganisationFields.Plan, Is.EqualTo(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.Plan)));
            Assert.That(OrganisationFields.BillingMode, Is.EqualTo(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.BillingMode)));
            Assert.That(OrganisationFields.BillingCurrency, Is.EqualTo(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.BillingCurrency)));
            Assert.That(OrganisationFields.BillingBalance, Is.EqualTo(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.BillingBalance)));
            Assert.That(OrganisationFields.DefaultCountry, Is.EqualTo(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.DefaultCountry)));
            Assert.That(OrganisationFields.DefaultAddressPosition, Is.EqualTo(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.DefaultAddressPosition)));
            Assert.That(OrganisationFields.DataRetentionAddresses, Is.EqualTo(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.DataRetentionAddresses)));
            Assert.That(OrganisationFields.DataRetentionPdf, Is.EqualTo(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.DataRetentionPdf)));
            Assert.That(OrganisationFields.Color, Is.EqualTo(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.Color)));
            Assert.That(OrganisationFields.CreatedAt, Is.EqualTo(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.CreatedAt)));
            Assert.That(OrganisationFields.UpdatedAt, Is.EqualTo(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.UpdatedAt)));
        });
    }

    /// <summary>
    /// Verifies UserFields constant values match JsonPropertyName values in User record
    /// </summary>
    [Test]
    public void UserFields_ConstantsMatchJsonPropertyNames()
    {
        Assert.Multiple(() =>
        {
            Assert.That(UserFields.Email, Is.EqualTo(PingenAttributesPropertyHelper<User>.GetJsonPropertyName(x => x.Email)));
            Assert.That(UserFields.FirstName, Is.EqualTo(PingenAttributesPropertyHelper<User>.GetJsonPropertyName(x => x.FirstName)));
            Assert.That(UserFields.LastName, Is.EqualTo(PingenAttributesPropertyHelper<User>.GetJsonPropertyName(x => x.LastName)));
            Assert.That(UserFields.Status, Is.EqualTo(PingenAttributesPropertyHelper<User>.GetJsonPropertyName(x => x.Status)));
            Assert.That(UserFields.Language, Is.EqualTo(PingenAttributesPropertyHelper<User>.GetJsonPropertyName(x => x.Language)));
            Assert.That(UserFields.CreatedAt, Is.EqualTo(PingenAttributesPropertyHelper<User>.GetJsonPropertyName(x => x.CreatedAt)));
            Assert.That(UserFields.UpdatedAt, Is.EqualTo(PingenAttributesPropertyHelper<User>.GetJsonPropertyName(x => x.UpdatedAt)));
        });
    }

    /// <summary>
    /// Verifies UserAssociationFields constant values match JsonPropertyName values in UserAssociation record
    /// </summary>
    [Test]
    public void UserAssociationFields_ConstantsMatchJsonPropertyNames()
    {
        Assert.Multiple(() =>
        {
            Assert.That(UserAssociationFields.Role, Is.EqualTo(PingenAttributesPropertyHelper<UserAssociation>.GetJsonPropertyName(x => x.Role)));
            Assert.That(UserAssociationFields.Status, Is.EqualTo(PingenAttributesPropertyHelper<UserAssociation>.GetJsonPropertyName(x => x.Status)));
            Assert.That(UserAssociationFields.CreatedAt, Is.EqualTo(PingenAttributesPropertyHelper<UserAssociation>.GetJsonPropertyName(x => x.CreatedAt)));
            Assert.That(UserAssociationFields.UpdatedAt, Is.EqualTo(PingenAttributesPropertyHelper<UserAssociation>.GetJsonPropertyName(x => x.UpdatedAt)));
        });
    }

    /// <summary>
    /// Verifies WebhookFields constant values match JsonPropertyName values in Webhook record
    /// </summary>
    [Test]
    public void WebhookFields_ConstantsMatchJsonPropertyNames()
    {
        Assert.Multiple(() =>
        {
            Assert.That(WebhookFields.EventCategory, Is.EqualTo(PingenAttributesPropertyHelper<Webhook>.GetJsonPropertyName(x => x.EventCategory)));
            Assert.That(WebhookFields.Url, Is.EqualTo(PingenAttributesPropertyHelper<Webhook>.GetJsonPropertyName(x => x.Url)));
            Assert.That(WebhookFields.SigningKey, Is.EqualTo(PingenAttributesPropertyHelper<Webhook>.GetJsonPropertyName(x => x.SigningKey)));
        });
    }

    /// <summary>
    /// Verifies LetterEventFields constant values match JsonPropertyName values in LetterEvent record
    /// </summary>
    [Test]
    public void LetterEventFields_ConstantsMatchJsonPropertyNames()
    {
        Assert.Multiple(() =>
        {
            Assert.That(LetterEventFields.Code, Is.EqualTo(PingenAttributesPropertyHelper<LetterEvent>.GetJsonPropertyName(x => x.Code)));
            Assert.That(LetterEventFields.Name, Is.EqualTo(PingenAttributesPropertyHelper<LetterEvent>.GetJsonPropertyName(x => x.Name)));
            Assert.That(LetterEventFields.Producer, Is.EqualTo(PingenAttributesPropertyHelper<LetterEvent>.GetJsonPropertyName(x => x.Producer)));
            Assert.That(LetterEventFields.Location, Is.EqualTo(PingenAttributesPropertyHelper<LetterEvent>.GetJsonPropertyName(x => x.Location)));
            Assert.That(LetterEventFields.HasImage, Is.EqualTo(PingenAttributesPropertyHelper<LetterEvent>.GetJsonPropertyName(x => x.HasImage)));
            Assert.That(LetterEventFields.Data, Is.EqualTo(PingenAttributesPropertyHelper<LetterEvent>.GetJsonPropertyName(x => x.Data)));
            Assert.That(LetterEventFields.EmittedAt, Is.EqualTo(PingenAttributesPropertyHelper<LetterEvent>.GetJsonPropertyName(x => x.EmittedAt)));
            Assert.That(LetterEventFields.CreatedAt, Is.EqualTo(PingenAttributesPropertyHelper<LetterEvent>.GetJsonPropertyName(x => x.CreatedAt)));
            Assert.That(LetterEventFields.UpdatedAt, Is.EqualTo(PingenAttributesPropertyHelper<LetterEvent>.GetJsonPropertyName(x => x.UpdatedAt)));
        });
    }

    /// <summary>
    /// Verifies WebhookEventFields constant values match JsonPropertyName values in WebhookEvent record
    /// </summary>
    [Test]
    public void WebhookEventFields_ConstantsMatchJsonPropertyNames()
    {
        Assert.Multiple(() =>
        {
            Assert.That(WebhookEventFields.Reason, Is.EqualTo(PingenAttributesPropertyHelper<WebhookEvent>.GetJsonPropertyName(x => x.Reason)));
            Assert.That(WebhookEventFields.Url, Is.EqualTo(PingenAttributesPropertyHelper<WebhookEvent>.GetJsonPropertyName(x => x.Url)));
            Assert.That(WebhookEventFields.CreatedAt, Is.EqualTo(PingenAttributesPropertyHelper<WebhookEvent>.GetJsonPropertyName(x => x.CreatedAt)));
        });
    }

    /// <summary>
    /// Verifies DeliveryProductFields constant values match JsonPropertyName values in DeliveryProduct record
    /// </summary>
    [Test]
    public void DeliveryProductFields_ConstantsMatchJsonPropertyNames()
    {
        Assert.Multiple(() =>
        {
            Assert.That(DeliveryProductFields.Countries, Is.EqualTo(PingenAttributesPropertyHelper<DeliveryProduct>.GetJsonPropertyName(x => x.Countries)));
            Assert.That(DeliveryProductFields.Name, Is.EqualTo(PingenAttributesPropertyHelper<DeliveryProduct>.GetJsonPropertyName(x => x.Name)));
            Assert.That(DeliveryProductFields.FullName, Is.EqualTo(PingenAttributesPropertyHelper<DeliveryProduct>.GetJsonPropertyName(x => x.FullName)));
            Assert.That(DeliveryProductFields.DeliveryTimeDays, Is.EqualTo(PingenAttributesPropertyHelper<DeliveryProduct>.GetJsonPropertyName(x => x.DeliveryTimeDays)));
            Assert.That(DeliveryProductFields.Features, Is.EqualTo(PingenAttributesPropertyHelper<DeliveryProduct>.GetJsonPropertyName(x => x.Features)));
            Assert.That(DeliveryProductFields.PriceCurrency, Is.EqualTo(PingenAttributesPropertyHelper<DeliveryProduct>.GetJsonPropertyName(x => x.PriceCurrency)));
            Assert.That(DeliveryProductFields.PriceStartingFrom, Is.EqualTo(PingenAttributesPropertyHelper<DeliveryProduct>.GetJsonPropertyName(x => x.PriceStartingFrom)));
        });
    }

    /// <summary>
    /// Verifies LetterPriceFields constant values match JsonPropertyName values in LetterPrice record
    /// </summary>
    [Test]
    public void LetterPriceFields_ConstantsMatchJsonPropertyNames()
    {
        Assert.Multiple(() =>
        {
            Assert.That(LetterPriceFields.Currency, Is.EqualTo(PingenAttributesPropertyHelper<LetterPrice>.GetJsonPropertyName(x => x.Currency)));
            Assert.That(LetterPriceFields.Price, Is.EqualTo(PingenAttributesPropertyHelper<LetterPrice>.GetJsonPropertyName(x => x.Price)));
        });
    }

    /// <summary>
    /// Verifies that field constants can be used with ApiRequest.SparseFieldsets
    /// </summary>
    [Test]
    public void FieldConstants_CanBeUsedInApiRequest()
    {
        var request = new ApiRequest
        {
            SparseFieldsets =
            [
                new(PingenApiDataType.letters, [LetterFields.Status, LetterFields.CreatedAt]),
                new(PingenApiDataType.organisations, [OrganisationFields.Name])
            ]
        };

        Assert.Multiple(() =>
        {
            Assert.That(request.SparseFieldsets, Is.Not.Null);
            Assert.That(request.SparseFieldsets!.Count(), Is.EqualTo(2));
            var letterEntry = request.SparseFieldsets!.First(e => e.Key == PingenApiDataType.letters);
            Assert.That(letterEntry.Value, Is.EquivalentTo(new[] { "status", "created_at" }));
        });
    }

    /// <summary>
    /// Verifies that Letter can be deserialized from sparse JSON with only a subset of fields
    /// </summary>
    [Test]
    public void Letter_SparseDeserialization_ReturnsNullForMissingFields()
    {
        const string sparseJson = """{"status": "valid"}""";

        var letter = PingenSerialisationHelper.Deserialize<Letter>(sparseJson);

        Assert.Multiple(() =>
        {
            Assert.That(letter, Is.Not.Null);
            Assert.That(letter!.Status, Is.EqualTo("valid"));
            Assert.That(letter.FileOriginalName, Is.Null);
            Assert.That(letter.Address, Is.Null);
            Assert.That(letter.AddressPosition, Is.Null);
            Assert.That(letter.Country, Is.Null);
            Assert.That(letter.DeliveryProduct, Is.Null);
            Assert.That(letter.PrintMode, Is.Null);
            Assert.That(letter.PrintSpectrum, Is.Null);
            Assert.That(letter.PriceCurrency, Is.Null);
            Assert.That(letter.PaperTypes, Is.Null);
            Assert.That(letter.Fonts, Is.Null);
            Assert.That(letter.TrackingNumber, Is.Null);
        });
    }

    /// <summary>
    /// Verifies that Batch can be deserialized from sparse JSON with only a subset of fields
    /// </summary>
    [Test]
    public void Batch_SparseDeserialization_ReturnsNullForMissingFields()
    {
        const string sparseJson = """{"name": "test-batch", "status": "created"}""";

        var batch = PingenSerialisationHelper.Deserialize<Batch>(sparseJson);

        Assert.Multiple(() =>
        {
            Assert.That(batch, Is.Not.Null);
            Assert.That(batch!.Name, Is.EqualTo("test-batch"));
            Assert.That(batch.Status, Is.EqualTo("created"));
            Assert.That(batch.Icon, Is.Null);
            Assert.That(batch.FileOriginalName, Is.Null);
            Assert.That(batch.AddressPosition, Is.Null);
        });
    }
}
