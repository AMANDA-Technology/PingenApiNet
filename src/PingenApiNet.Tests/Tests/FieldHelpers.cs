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
            Assert.That(LetterFields.Status, Is.EqualTo("status"));
            Assert.That(LetterFields.FileOriginalName, Is.EqualTo("file_original_name"));
            Assert.That(LetterFields.FilePages, Is.EqualTo("file_pages"));
            Assert.That(LetterFields.Address, Is.EqualTo("address"));
            Assert.That(LetterFields.AddressPosition, Is.EqualTo("address_position"));
            Assert.That(LetterFields.Country, Is.EqualTo("country"));
            Assert.That(LetterFields.DeliveryProduct, Is.EqualTo("delivery_product"));
            Assert.That(LetterFields.PrintMode, Is.EqualTo("print_mode"));
            Assert.That(LetterFields.PrintSpectrum, Is.EqualTo("print_spectrum"));
            Assert.That(LetterFields.PriceCurrency, Is.EqualTo("price_currency"));
            Assert.That(LetterFields.PriceValue, Is.EqualTo("price_value"));
            Assert.That(LetterFields.PaperTypes, Is.EqualTo("paper_types"));
            Assert.That(LetterFields.Fonts, Is.EqualTo("fonts"));
            Assert.That(LetterFields.TrackingNumber, Is.EqualTo("tracking_number"));
            Assert.That(LetterFields.SubmittedAt, Is.EqualTo("submitted_at"));
            Assert.That(LetterFields.CreatedAt, Is.EqualTo("created_at"));
            Assert.That(LetterFields.UpdatedAt, Is.EqualTo("updated_at"));
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
            Assert.That(BatchFields.Name, Is.EqualTo("name"));
            Assert.That(BatchFields.Icon, Is.EqualTo("icon"));
            Assert.That(BatchFields.Status, Is.EqualTo("status"));
            Assert.That(BatchFields.FileOriginalName, Is.EqualTo("file_original_name"));
            Assert.That(BatchFields.LetterCount, Is.EqualTo("letter_count"));
            Assert.That(BatchFields.AddressPosition, Is.EqualTo("address_position"));
            Assert.That(BatchFields.PrintMode, Is.EqualTo("print_mode"));
            Assert.That(BatchFields.PrintSpectrum, Is.EqualTo("print_spectrum"));
            Assert.That(BatchFields.PriceCurrency, Is.EqualTo("price_currency"));
            Assert.That(BatchFields.PriceValue, Is.EqualTo("price_value"));
            Assert.That(BatchFields.SubmittedAt, Is.EqualTo("submitted_at"));
            Assert.That(BatchFields.CreatedAt, Is.EqualTo("created_at"));
            Assert.That(BatchFields.UpdatedAt, Is.EqualTo("updated_at"));
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
            Assert.That(OrganisationFields.Name, Is.EqualTo("name"));
            Assert.That(OrganisationFields.Status, Is.EqualTo("status"));
            Assert.That(OrganisationFields.Plan, Is.EqualTo("plan"));
            Assert.That(OrganisationFields.BillingMode, Is.EqualTo("billing_mode"));
            Assert.That(OrganisationFields.BillingCurrency, Is.EqualTo("billing_currency"));
            Assert.That(OrganisationFields.BillingBalance, Is.EqualTo("billing_balance"));
            Assert.That(OrganisationFields.DefaultCountry, Is.EqualTo("default_country"));
            Assert.That(OrganisationFields.DefaultAddressPosition, Is.EqualTo("default_address_position"));
            Assert.That(OrganisationFields.DataRetentionAddresses, Is.EqualTo("data_retention_addresses"));
            Assert.That(OrganisationFields.DataRetentionPdf, Is.EqualTo("data_retention_pdf"));
            Assert.That(OrganisationFields.Color, Is.EqualTo("color"));
            Assert.That(OrganisationFields.CreatedAt, Is.EqualTo("created_at"));
            Assert.That(OrganisationFields.UpdatedAt, Is.EqualTo("updated_at"));
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
            Assert.That(UserFields.Email, Is.EqualTo("email"));
            Assert.That(UserFields.FirstName, Is.EqualTo("first_name"));
            Assert.That(UserFields.LastName, Is.EqualTo("last_name"));
            Assert.That(UserFields.Status, Is.EqualTo("status"));
            Assert.That(UserFields.Language, Is.EqualTo("language"));
            Assert.That(UserFields.CreatedAt, Is.EqualTo("created_at"));
            Assert.That(UserFields.UpdatedAt, Is.EqualTo("updated_at"));
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
            Assert.That(UserAssociationFields.Role, Is.EqualTo("role"));
            Assert.That(UserAssociationFields.Status, Is.EqualTo("status"));
            Assert.That(UserAssociationFields.CreatedAt, Is.EqualTo("created_at"));
            Assert.That(UserAssociationFields.UpdatedAt, Is.EqualTo("updated_at"));
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
            Assert.That(WebhookFields.EventCategory, Is.EqualTo("event_category"));
            Assert.That(WebhookFields.Url, Is.EqualTo("url"));
            Assert.That(WebhookFields.SigningKey, Is.EqualTo("signing_key"));
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
            Assert.That(LetterEventFields.Code, Is.EqualTo("code"));
            Assert.That(LetterEventFields.Name, Is.EqualTo("name"));
            Assert.That(LetterEventFields.Producer, Is.EqualTo("producer"));
            Assert.That(LetterEventFields.Location, Is.EqualTo("location"));
            Assert.That(LetterEventFields.HasImage, Is.EqualTo("has_image"));
            Assert.That(LetterEventFields.Data, Is.EqualTo("data"));
            Assert.That(LetterEventFields.EmittedAt, Is.EqualTo("emitted_at"));
            Assert.That(LetterEventFields.CreatedAt, Is.EqualTo("created_at"));
            Assert.That(LetterEventFields.UpdatedAt, Is.EqualTo("updated_at"));
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
            Assert.That(WebhookEventFields.Reason, Is.EqualTo("reason"));
            Assert.That(WebhookEventFields.Url, Is.EqualTo("url"));
            Assert.That(WebhookEventFields.CreatedAt, Is.EqualTo("created_at"));
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
            Assert.That(DeliveryProductFields.Countries, Is.EqualTo("countries"));
            Assert.That(DeliveryProductFields.Name, Is.EqualTo("name"));
            Assert.That(DeliveryProductFields.FullName, Is.EqualTo("full_name"));
            Assert.That(DeliveryProductFields.DeliveryTimeDays, Is.EqualTo("delivery_time_days"));
            Assert.That(DeliveryProductFields.Features, Is.EqualTo("features"));
            Assert.That(DeliveryProductFields.PriceCurrency, Is.EqualTo("price_currency"));
            Assert.That(DeliveryProductFields.PriceStartingFrom, Is.EqualTo("price_starting_from"));
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
            Assert.That(LetterPriceFields.Currency, Is.EqualTo("currency"));
            Assert.That(LetterPriceFields.Price, Is.EqualTo("price"));
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
}
