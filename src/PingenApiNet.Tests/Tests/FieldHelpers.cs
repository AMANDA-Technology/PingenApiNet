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
        Should.SatisfyAllConditions(
            () => LetterFields.Status.ShouldBe(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.Status)),
            () => LetterFields.FileOriginalName.ShouldBe(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.FileOriginalName)),
            () => LetterFields.FilePages.ShouldBe(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.FilePages)),
            () => LetterFields.Address.ShouldBe(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.Address)),
            () => LetterFields.AddressPosition.ShouldBe(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.AddressPosition)),
            () => LetterFields.Country.ShouldBe(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.Country)),
            () => LetterFields.DeliveryProduct.ShouldBe(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.DeliveryProduct)),
            () => LetterFields.PrintMode.ShouldBe(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.PrintMode)),
            () => LetterFields.PrintSpectrum.ShouldBe(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.PrintSpectrum)),
            () => LetterFields.PriceCurrency.ShouldBe(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.PriceCurrency)),
            () => LetterFields.PriceValue.ShouldBe(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.PriceValue)),
            () => LetterFields.PaperTypes.ShouldBe(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.PaperTypes)),
            () => LetterFields.Fonts.ShouldBe(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.Fonts)),
            () => LetterFields.TrackingNumber.ShouldBe(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.TrackingNumber)),
            () => LetterFields.SubmittedAt.ShouldBe(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.SubmittedAt)),
            () => LetterFields.CreatedAt.ShouldBe(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.CreatedAt)),
            () => LetterFields.UpdatedAt.ShouldBe(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(x => x.UpdatedAt))
        );
    }

    /// <summary>
    /// Verifies BatchFields constant values match JsonPropertyName values in Batch record
    /// </summary>
    [Test]
    public void BatchFields_ConstantsMatchJsonPropertyNames()
    {
        Should.SatisfyAllConditions(
            () => BatchFields.Name.ShouldBe(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.Name)),
            () => BatchFields.Icon.ShouldBe(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.Icon)),
            () => BatchFields.Status.ShouldBe(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.Status)),
            () => BatchFields.FileOriginalName.ShouldBe(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.FileOriginalName)),
            () => BatchFields.LetterCount.ShouldBe(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.LetterCount)),
            () => BatchFields.AddressPosition.ShouldBe(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.AddressPosition)),
            () => BatchFields.PrintMode.ShouldBe(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.PrintMode)),
            () => BatchFields.PrintSpectrum.ShouldBe(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.PrintSpectrum)),
            () => BatchFields.PriceCurrency.ShouldBe(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.PriceCurrency)),
            () => BatchFields.PriceValue.ShouldBe(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.PriceValue)),
            () => BatchFields.SubmittedAt.ShouldBe(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.SubmittedAt)),
            () => BatchFields.CreatedAt.ShouldBe(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.CreatedAt)),
            () => BatchFields.UpdatedAt.ShouldBe(PingenAttributesPropertyHelper<Batch>.GetJsonPropertyName(x => x.UpdatedAt))
        );
    }

    /// <summary>
    /// Verifies OrganisationFields constant values match JsonPropertyName values in Organisation record
    /// </summary>
    [Test]
    public void OrganisationFields_ConstantsMatchJsonPropertyNames()
    {
        Should.SatisfyAllConditions(
            () => OrganisationFields.Name.ShouldBe(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.Name)),
            () => OrganisationFields.Status.ShouldBe(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.Status)),
            () => OrganisationFields.Plan.ShouldBe(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.Plan)),
            () => OrganisationFields.BillingMode.ShouldBe(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.BillingMode)),
            () => OrganisationFields.BillingCurrency.ShouldBe(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.BillingCurrency)),
            () => OrganisationFields.BillingBalance.ShouldBe(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.BillingBalance)),
            () => OrganisationFields.DefaultCountry.ShouldBe(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.DefaultCountry)),
            () => OrganisationFields.DefaultAddressPosition.ShouldBe(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.DefaultAddressPosition)),
            () => OrganisationFields.DataRetentionAddresses.ShouldBe(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.DataRetentionAddresses)),
            () => OrganisationFields.DataRetentionPdf.ShouldBe(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.DataRetentionPdf)),
            () => OrganisationFields.Color.ShouldBe(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.Color)),
            () => OrganisationFields.CreatedAt.ShouldBe(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.CreatedAt)),
            () => OrganisationFields.UpdatedAt.ShouldBe(PingenAttributesPropertyHelper<Organisation>.GetJsonPropertyName(x => x.UpdatedAt))
        );
    }

    /// <summary>
    /// Verifies UserFields constant values match JsonPropertyName values in User record
    /// </summary>
    [Test]
    public void UserFields_ConstantsMatchJsonPropertyNames()
    {
        Should.SatisfyAllConditions(
            () => UserFields.Email.ShouldBe(PingenAttributesPropertyHelper<User>.GetJsonPropertyName(x => x.Email)),
            () => UserFields.FirstName.ShouldBe(PingenAttributesPropertyHelper<User>.GetJsonPropertyName(x => x.FirstName)),
            () => UserFields.LastName.ShouldBe(PingenAttributesPropertyHelper<User>.GetJsonPropertyName(x => x.LastName)),
            () => UserFields.Status.ShouldBe(PingenAttributesPropertyHelper<User>.GetJsonPropertyName(x => x.Status)),
            () => UserFields.Language.ShouldBe(PingenAttributesPropertyHelper<User>.GetJsonPropertyName(x => x.Language)),
            () => UserFields.CreatedAt.ShouldBe(PingenAttributesPropertyHelper<User>.GetJsonPropertyName(x => x.CreatedAt)),
            () => UserFields.UpdatedAt.ShouldBe(PingenAttributesPropertyHelper<User>.GetJsonPropertyName(x => x.UpdatedAt))
        );
    }

    /// <summary>
    /// Verifies UserAssociationFields constant values match JsonPropertyName values in UserAssociation record
    /// </summary>
    [Test]
    public void UserAssociationFields_ConstantsMatchJsonPropertyNames()
    {
        Should.SatisfyAllConditions(
            () => UserAssociationFields.Role.ShouldBe(PingenAttributesPropertyHelper<UserAssociation>.GetJsonPropertyName(x => x.Role)),
            () => UserAssociationFields.Status.ShouldBe(PingenAttributesPropertyHelper<UserAssociation>.GetJsonPropertyName(x => x.Status)),
            () => UserAssociationFields.CreatedAt.ShouldBe(PingenAttributesPropertyHelper<UserAssociation>.GetJsonPropertyName(x => x.CreatedAt)),
            () => UserAssociationFields.UpdatedAt.ShouldBe(PingenAttributesPropertyHelper<UserAssociation>.GetJsonPropertyName(x => x.UpdatedAt))
        );
    }

    /// <summary>
    /// Verifies WebhookFields constant values match JsonPropertyName values in Webhook record
    /// </summary>
    [Test]
    public void WebhookFields_ConstantsMatchJsonPropertyNames()
    {
        Should.SatisfyAllConditions(
            () => WebhookFields.EventCategory.ShouldBe(PingenAttributesPropertyHelper<Webhook>.GetJsonPropertyName(x => x.EventCategory)),
            () => WebhookFields.Url.ShouldBe(PingenAttributesPropertyHelper<Webhook>.GetJsonPropertyName(x => x.Url)),
            () => WebhookFields.SigningKey.ShouldBe(PingenAttributesPropertyHelper<Webhook>.GetJsonPropertyName(x => x.SigningKey))
        );
    }

    /// <summary>
    /// Verifies LetterEventFields constant values match JsonPropertyName values in LetterEvent record
    /// </summary>
    [Test]
    public void LetterEventFields_ConstantsMatchJsonPropertyNames()
    {
        Should.SatisfyAllConditions(
            () => LetterEventFields.Code.ShouldBe(PingenAttributesPropertyHelper<LetterEvent>.GetJsonPropertyName(x => x.Code)),
            () => LetterEventFields.Name.ShouldBe(PingenAttributesPropertyHelper<LetterEvent>.GetJsonPropertyName(x => x.Name)),
            () => LetterEventFields.Producer.ShouldBe(PingenAttributesPropertyHelper<LetterEvent>.GetJsonPropertyName(x => x.Producer)),
            () => LetterEventFields.Location.ShouldBe(PingenAttributesPropertyHelper<LetterEvent>.GetJsonPropertyName(x => x.Location)),
            () => LetterEventFields.HasImage.ShouldBe(PingenAttributesPropertyHelper<LetterEvent>.GetJsonPropertyName(x => x.HasImage)),
            () => LetterEventFields.Data.ShouldBe(PingenAttributesPropertyHelper<LetterEvent>.GetJsonPropertyName(x => x.Data)),
            () => LetterEventFields.EmittedAt.ShouldBe(PingenAttributesPropertyHelper<LetterEvent>.GetJsonPropertyName(x => x.EmittedAt)),
            () => LetterEventFields.CreatedAt.ShouldBe(PingenAttributesPropertyHelper<LetterEvent>.GetJsonPropertyName(x => x.CreatedAt)),
            () => LetterEventFields.UpdatedAt.ShouldBe(PingenAttributesPropertyHelper<LetterEvent>.GetJsonPropertyName(x => x.UpdatedAt))
        );
    }

    /// <summary>
    /// Verifies WebhookEventFields constant values match JsonPropertyName values in WebhookEvent record
    /// </summary>
    [Test]
    public void WebhookEventFields_ConstantsMatchJsonPropertyNames()
    {
        Should.SatisfyAllConditions(
            () => WebhookEventFields.Reason.ShouldBe(PingenAttributesPropertyHelper<WebhookEvent>.GetJsonPropertyName(x => x.Reason)),
            () => WebhookEventFields.Url.ShouldBe(PingenAttributesPropertyHelper<WebhookEvent>.GetJsonPropertyName(x => x.Url)),
            () => WebhookEventFields.CreatedAt.ShouldBe(PingenAttributesPropertyHelper<WebhookEvent>.GetJsonPropertyName(x => x.CreatedAt))
        );
    }

    /// <summary>
    /// Verifies DeliveryProductFields constant values match JsonPropertyName values in DeliveryProduct record
    /// </summary>
    [Test]
    public void DeliveryProductFields_ConstantsMatchJsonPropertyNames()
    {
        Should.SatisfyAllConditions(
            () => DeliveryProductFields.Countries.ShouldBe(PingenAttributesPropertyHelper<DeliveryProduct>.GetJsonPropertyName(x => x.Countries)),
            () => DeliveryProductFields.Name.ShouldBe(PingenAttributesPropertyHelper<DeliveryProduct>.GetJsonPropertyName(x => x.Name)),
            () => DeliveryProductFields.FullName.ShouldBe(PingenAttributesPropertyHelper<DeliveryProduct>.GetJsonPropertyName(x => x.FullName)),
            () => DeliveryProductFields.DeliveryTimeDays.ShouldBe(PingenAttributesPropertyHelper<DeliveryProduct>.GetJsonPropertyName(x => x.DeliveryTimeDays)),
            () => DeliveryProductFields.Features.ShouldBe(PingenAttributesPropertyHelper<DeliveryProduct>.GetJsonPropertyName(x => x.Features)),
            () => DeliveryProductFields.PriceCurrency.ShouldBe(PingenAttributesPropertyHelper<DeliveryProduct>.GetJsonPropertyName(x => x.PriceCurrency)),
            () => DeliveryProductFields.PriceStartingFrom.ShouldBe(PingenAttributesPropertyHelper<DeliveryProduct>.GetJsonPropertyName(x => x.PriceStartingFrom))
        );
    }

    /// <summary>
    /// Verifies LetterPriceFields constant values match JsonPropertyName values in LetterPrice record
    /// </summary>
    [Test]
    public void LetterPriceFields_ConstantsMatchJsonPropertyNames()
    {
        Should.SatisfyAllConditions(
            () => LetterPriceFields.Currency.ShouldBe(PingenAttributesPropertyHelper<LetterPrice>.GetJsonPropertyName(x => x.Currency)),
            () => LetterPriceFields.Price.ShouldBe(PingenAttributesPropertyHelper<LetterPrice>.GetJsonPropertyName(x => x.Price))
        );
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

        var letterEntry = request.SparseFieldsets!.First(e => e.Key == PingenApiDataType.letters);
        Should.SatisfyAllConditions(
            () => request.SparseFieldsets.ShouldNotBeNull(),
            () => request.SparseFieldsets!.Count().ShouldBe(2),
            () => letterEntry.Value.ShouldBe(new[] { "status", "created_at" }, ignoreOrder: true)
        );
    }

    /// <summary>
    /// Verifies that Letter can be deserialized from sparse JSON with only a subset of fields
    /// </summary>
    [Test]
    public void Letter_SparseDeserialization_ReturnsNullForMissingFields()
    {
        const string sparseJson = """{"status": "valid"}""";

        var letter = PingenSerialisationHelper.Deserialize<Letter>(sparseJson);

        Should.SatisfyAllConditions(
            () => letter.ShouldNotBeNull(),
            () => letter!.Status.ShouldBe("valid"),
            () => letter.FileOriginalName.ShouldBeNull(),
            () => letter.Address.ShouldBeNull(),
            () => letter.AddressPosition.ShouldBeNull(),
            () => letter.Country.ShouldBeNull(),
            () => letter.DeliveryProduct.ShouldBeNull(),
            () => letter.PrintMode.ShouldBeNull(),
            () => letter.PrintSpectrum.ShouldBeNull(),
            () => letter.PriceCurrency.ShouldBeNull(),
            () => letter.PaperTypes.ShouldBeNull(),
            () => letter.Fonts.ShouldBeNull(),
            () => letter.TrackingNumber.ShouldBeNull()
        );
    }

    /// <summary>
    /// Verifies that Batch can be deserialized from sparse JSON with only a subset of fields
    /// </summary>
    [Test]
    public void Batch_SparseDeserialization_ReturnsNullForMissingFields()
    {
        const string sparseJson = """{"name": "test-batch", "status": "created"}""";

        var batch = PingenSerialisationHelper.Deserialize<Batch>(sparseJson);

        Should.SatisfyAllConditions(
            () => batch.ShouldNotBeNull(),
            () => batch!.Name.ShouldBe("test-batch"),
            () => batch.Status.ShouldBe("created"),
            () => batch.Icon.ShouldBeNull(),
            () => batch.FileOriginalName.ShouldBeNull(),
            () => batch.AddressPosition.ShouldBeNull()
        );
    }
}
