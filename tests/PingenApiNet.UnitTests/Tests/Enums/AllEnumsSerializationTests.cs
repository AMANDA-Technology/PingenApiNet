/*
MIT License

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

using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Enums.Batches;
using PingenApiNet.Abstractions.Enums.Users;
using PingenApiNet.Abstractions.Helpers;

namespace PingenApiNet.UnitTests.Tests.Enums;

/// <summary>
///     Verifies serialization and deserialization of all cross-cutting enums and remaining
///     string-constant lookups exposed by the Abstractions library:
///     <see cref="BatchGroupingType" />, <see cref="BatchGroupingOptionsSplitType" />,
///     <see cref="BatchGroupingOptionsSplitPosition" />, <see cref="UserRole" />,
///     <see cref="UserAssociationStatus" />, <see cref="CollectionSortDirection" />,
///     <see cref="WebhookEventCategory" />, <see cref="PingenApiAbility" />,
///     <see cref="PingenApiCurrency" />, <see cref="PingenApiDataType" />,
///     <see cref="CollectionFilterOperator" />, and <see cref="PingenApiLanguage" />.
/// </summary>
public class AllEnumsSerializationTests
{
    // -------------------------------------------------------------------
    // Section A: BatchGroupingType (real enum, 2 values)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Verifies that every <see cref="BatchGroupingType" /> value serializes to its lowercase JSON identifier.
    /// </summary>
    /// <param name="value">The enum value under test.</param>
    /// <param name="expectedJson">The expected JSON representation.</param>
    [TestCase(BatchGroupingType.zip, "\"zip\"")]
    [TestCase(BatchGroupingType.merge, "\"merge\"")]
    public void BatchGroupingType_Serializes(BatchGroupingType value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);

    /// <summary>
    ///     Verifies that every <see cref="BatchGroupingType" /> JSON string deserializes back to the expected value.
    /// </summary>
    /// <param name="json">The JSON string under test.</param>
    /// <param name="expected">The expected enum value.</param>
    [TestCase("\"zip\"", BatchGroupingType.zip)]
    [TestCase("\"merge\"", BatchGroupingType.merge)]
    public void BatchGroupingType_Deserializes(string json, BatchGroupingType expected) =>
        PingenSerialisationHelper.Deserialize<BatchGroupingType>(json).ShouldBe(expected);

    /// <summary>
    ///     Sentinel: fails if a new <see cref="BatchGroupingType" /> value is added without updating the cases above.
    /// </summary>
    [Test]
    public void BatchGroupingType_AllValuesAreCovered() => Enum.GetValues<BatchGroupingType>().Length.ShouldBe(2);

    // -------------------------------------------------------------------
    // Section B: BatchGroupingOptionsSplitType (real enum, 4 values)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Verifies that every <see cref="BatchGroupingOptionsSplitType" /> value serializes to its JSON identifier,
    ///     preserving the underscore in <c>qr_invoice</c>.
    /// </summary>
    /// <param name="value">The enum value under test.</param>
    /// <param name="expectedJson">The expected JSON representation.</param>
    [TestCase(BatchGroupingOptionsSplitType.file, "\"file\"")]
    [TestCase(BatchGroupingOptionsSplitType.page, "\"page\"")]
    [TestCase(BatchGroupingOptionsSplitType.custom, "\"custom\"")]
    [TestCase(BatchGroupingOptionsSplitType.qr_invoice, "\"qr_invoice\"")]
    public void BatchGroupingOptionsSplitType_Serializes(BatchGroupingOptionsSplitType value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);

    /// <summary>
    ///     Verifies that every <see cref="BatchGroupingOptionsSplitType" /> JSON string deserializes back to the expected
    ///     value.
    /// </summary>
    /// <param name="json">The JSON string under test.</param>
    /// <param name="expected">The expected enum value.</param>
    [TestCase("\"file\"", BatchGroupingOptionsSplitType.file)]
    [TestCase("\"page\"", BatchGroupingOptionsSplitType.page)]
    [TestCase("\"custom\"", BatchGroupingOptionsSplitType.custom)]
    [TestCase("\"qr_invoice\"", BatchGroupingOptionsSplitType.qr_invoice)]
    public void BatchGroupingOptionsSplitType_Deserializes(string json, BatchGroupingOptionsSplitType expected) =>
        PingenSerialisationHelper.Deserialize<BatchGroupingOptionsSplitType>(json).ShouldBe(expected);

    /// <summary>
    ///     Sentinel: fails if a new <see cref="BatchGroupingOptionsSplitType" /> value is added without updating the cases
    ///     above.
    /// </summary>
    [Test]
    public void BatchGroupingOptionsSplitType_AllValuesAreCovered() =>
        Enum.GetValues<BatchGroupingOptionsSplitType>().Length.ShouldBe(4);

    // -------------------------------------------------------------------
    // Section C: BatchGroupingOptionsSplitPosition (real enum, 2 values)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Verifies that every <see cref="BatchGroupingOptionsSplitPosition" /> value serializes to its snake_case JSON
    ///     identifier.
    /// </summary>
    /// <param name="value">The enum value under test.</param>
    /// <param name="expectedJson">The expected JSON representation.</param>
    [TestCase(BatchGroupingOptionsSplitPosition.first_page, "\"first_page\"")]
    [TestCase(BatchGroupingOptionsSplitPosition.last_page, "\"last_page\"")]
    public void BatchGroupingOptionsSplitPosition_Serializes(BatchGroupingOptionsSplitPosition value,
        string expectedJson) => PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);

    /// <summary>
    ///     Verifies that every <see cref="BatchGroupingOptionsSplitPosition" /> JSON string deserializes back to the expected
    ///     value.
    /// </summary>
    /// <param name="json">The JSON string under test.</param>
    /// <param name="expected">The expected enum value.</param>
    [TestCase("\"first_page\"", BatchGroupingOptionsSplitPosition.first_page)]
    [TestCase("\"last_page\"", BatchGroupingOptionsSplitPosition.last_page)]
    public void
        BatchGroupingOptionsSplitPosition_Deserializes(string json, BatchGroupingOptionsSplitPosition expected) =>
        PingenSerialisationHelper.Deserialize<BatchGroupingOptionsSplitPosition>(json).ShouldBe(expected);

    /// <summary>
    ///     Sentinel: fails if a new <see cref="BatchGroupingOptionsSplitPosition" /> value is added without updating the cases
    ///     above.
    /// </summary>
    [Test]
    public void BatchGroupingOptionsSplitPosition_AllValuesAreCovered() =>
        Enum.GetValues<BatchGroupingOptionsSplitPosition>().Length.ShouldBe(2);

    // -------------------------------------------------------------------
    // Section D: UserRole (real enum, 2 values)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Verifies that every <see cref="UserRole" /> value serializes to its lowercase JSON identifier.
    /// </summary>
    /// <param name="value">The enum value under test.</param>
    /// <param name="expectedJson">The expected JSON representation.</param>
    [TestCase(UserRole.owner, "\"owner\"")]
    [TestCase(UserRole.manager, "\"manager\"")]
    public void UserRole_Serializes(UserRole value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);

    /// <summary>
    ///     Verifies that every <see cref="UserRole" /> JSON string deserializes back to the expected value.
    /// </summary>
    /// <param name="json">The JSON string under test.</param>
    /// <param name="expected">The expected enum value.</param>
    [TestCase("\"owner\"", UserRole.owner)]
    [TestCase("\"manager\"", UserRole.manager)]
    public void UserRole_Deserializes(string json, UserRole expected) =>
        PingenSerialisationHelper.Deserialize<UserRole>(json).ShouldBe(expected);

    /// <summary>
    ///     Sentinel: fails if a new <see cref="UserRole" /> value is added without updating the cases above.
    /// </summary>
    [Test]
    public void UserRole_AllValuesAreCovered() => Enum.GetValues<UserRole>().Length.ShouldBe(2);

    // -------------------------------------------------------------------
    // Section E: UserAssociationStatus (real enum, 3 values)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Verifies that every <see cref="UserAssociationStatus" /> value serializes to its lowercase JSON identifier.
    /// </summary>
    /// <param name="value">The enum value under test.</param>
    /// <param name="expectedJson">The expected JSON representation.</param>
    [TestCase(UserAssociationStatus.pending, "\"pending\"")]
    [TestCase(UserAssociationStatus.active, "\"active\"")]
    [TestCase(UserAssociationStatus.blocked, "\"blocked\"")]
    public void UserAssociationStatus_Serializes(UserAssociationStatus value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);

    /// <summary>
    ///     Verifies that every <see cref="UserAssociationStatus" /> JSON string deserializes back to the expected value.
    /// </summary>
    /// <param name="json">The JSON string under test.</param>
    /// <param name="expected">The expected enum value.</param>
    [TestCase("\"pending\"", UserAssociationStatus.pending)]
    [TestCase("\"active\"", UserAssociationStatus.active)]
    [TestCase("\"blocked\"", UserAssociationStatus.blocked)]
    public void UserAssociationStatus_Deserializes(string json, UserAssociationStatus expected) =>
        PingenSerialisationHelper.Deserialize<UserAssociationStatus>(json).ShouldBe(expected);

    /// <summary>
    ///     Sentinel: fails if a new <see cref="UserAssociationStatus" /> value is added without updating the cases above.
    /// </summary>
    [Test]
    public void UserAssociationStatus_AllValuesAreCovered() =>
        Enum.GetValues<UserAssociationStatus>().Length.ShouldBe(3);

    // -------------------------------------------------------------------
    // Section F: CollectionSortDirection (real enum, 2 values, uppercase identifiers)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Verifies that every <see cref="CollectionSortDirection" /> value serializes to its uppercase JSON identifier
    ///     (no naming policy is applied, so <c>ASC</c>/<c>DESC</c> are preserved verbatim).
    /// </summary>
    /// <param name="value">The enum value under test.</param>
    /// <param name="expectedJson">The expected JSON representation.</param>
    [TestCase(CollectionSortDirection.ASC, "\"ASC\"")]
    [TestCase(CollectionSortDirection.DESC, "\"DESC\"")]
    public void CollectionSortDirection_Serializes(CollectionSortDirection value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);

    /// <summary>
    ///     Verifies that every <see cref="CollectionSortDirection" /> JSON string deserializes back to the expected value.
    /// </summary>
    /// <param name="json">The JSON string under test.</param>
    /// <param name="expected">The expected enum value.</param>
    [TestCase("\"ASC\"", CollectionSortDirection.ASC)]
    [TestCase("\"DESC\"", CollectionSortDirection.DESC)]
    public void CollectionSortDirection_Deserializes(string json, CollectionSortDirection expected) =>
        PingenSerialisationHelper.Deserialize<CollectionSortDirection>(json).ShouldBe(expected);

    /// <summary>
    ///     Sentinel: fails if a new <see cref="CollectionSortDirection" /> value is added without updating the cases above.
    /// </summary>
    [Test]
    public void CollectionSortDirection_AllValuesAreCovered() =>
        Enum.GetValues<CollectionSortDirection>().Length.ShouldBe(2);

    // -------------------------------------------------------------------
    // Section G: WebhookEventCategory (real enum, 3 values)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Verifies that every <see cref="WebhookEventCategory" /> value serializes to its lowercase JSON identifier.
    /// </summary>
    /// <param name="value">The enum value under test.</param>
    /// <param name="expectedJson">The expected JSON representation.</param>
    [TestCase(WebhookEventCategory.issues, "\"issues\"")]
    [TestCase(WebhookEventCategory.undeliverable, "\"undeliverable\"")]
    [TestCase(WebhookEventCategory.sent, "\"sent\"")]
    public void WebhookEventCategory_Serializes(WebhookEventCategory value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);

    /// <summary>
    ///     Verifies that every <see cref="WebhookEventCategory" /> JSON string deserializes back to the expected value.
    /// </summary>
    /// <param name="json">The JSON string under test.</param>
    /// <param name="expected">The expected enum value.</param>
    [TestCase("\"issues\"", WebhookEventCategory.issues)]
    [TestCase("\"undeliverable\"", WebhookEventCategory.undeliverable)]
    [TestCase("\"sent\"", WebhookEventCategory.sent)]
    public void WebhookEventCategory_Deserializes(string json, WebhookEventCategory expected) =>
        PingenSerialisationHelper.Deserialize<WebhookEventCategory>(json).ShouldBe(expected);

    /// <summary>
    ///     Sentinel: fails if a new <see cref="WebhookEventCategory" /> value is added without updating the cases above.
    /// </summary>
    [Test]
    public void WebhookEventCategory_AllValuesAreCovered() => Enum.GetValues<WebhookEventCategory>().Length.ShouldBe(3);

    // -------------------------------------------------------------------
    // Section H: PingenApiAbility (real enum, 3 values)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Verifies that every <see cref="PingenApiAbility" /> value serializes to its lowercase JSON identifier.
    /// </summary>
    /// <param name="value">The enum value under test.</param>
    /// <param name="expectedJson">The expected JSON representation.</param>
    [TestCase(PingenApiAbility.ok, "\"ok\"")]
    [TestCase(PingenApiAbility.state, "\"state\"")]
    [TestCase(PingenApiAbility.permission, "\"permission\"")]
    public void PingenApiAbility_Serializes(PingenApiAbility value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);

    /// <summary>
    ///     Verifies that every <see cref="PingenApiAbility" /> JSON string deserializes back to the expected value.
    /// </summary>
    /// <param name="json">The JSON string under test.</param>
    /// <param name="expected">The expected enum value.</param>
    [TestCase("\"ok\"", PingenApiAbility.ok)]
    [TestCase("\"state\"", PingenApiAbility.state)]
    [TestCase("\"permission\"", PingenApiAbility.permission)]
    public void PingenApiAbility_Deserializes(string json, PingenApiAbility expected) =>
        PingenSerialisationHelper.Deserialize<PingenApiAbility>(json).ShouldBe(expected);

    /// <summary>
    ///     Sentinel: fails if a new <see cref="PingenApiAbility" /> value is added without updating the cases above.
    /// </summary>
    [Test]
    public void PingenApiAbility_AllValuesAreCovered() => Enum.GetValues<PingenApiAbility>().Length.ShouldBe(3);

    // -------------------------------------------------------------------
    // Section I: PingenApiCurrency (real enum, 4 values, uppercase identifiers)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Verifies that every <see cref="PingenApiCurrency" /> value serializes to its uppercase JSON identifier.
    /// </summary>
    /// <param name="value">The enum value under test.</param>
    /// <param name="expectedJson">The expected JSON representation.</param>
    [TestCase(PingenApiCurrency.EUR, "\"EUR\"")]
    [TestCase(PingenApiCurrency.CHF, "\"CHF\"")]
    [TestCase(PingenApiCurrency.USD, "\"USD\"")]
    [TestCase(PingenApiCurrency.GBP, "\"GBP\"")]
    public void PingenApiCurrency_Serializes(PingenApiCurrency value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);

    /// <summary>
    ///     Verifies that every <see cref="PingenApiCurrency" /> JSON string deserializes back to the expected value.
    /// </summary>
    /// <param name="json">The JSON string under test.</param>
    /// <param name="expected">The expected enum value.</param>
    [TestCase("\"EUR\"", PingenApiCurrency.EUR)]
    [TestCase("\"CHF\"", PingenApiCurrency.CHF)]
    [TestCase("\"USD\"", PingenApiCurrency.USD)]
    [TestCase("\"GBP\"", PingenApiCurrency.GBP)]
    public void PingenApiCurrency_Deserializes(string json, PingenApiCurrency expected) =>
        PingenSerialisationHelper.Deserialize<PingenApiCurrency>(json).ShouldBe(expected);

    /// <summary>
    ///     Sentinel: fails if a new <see cref="PingenApiCurrency" /> value is added without updating the cases above.
    ///     The four values mirror the upstream <c>OrganisationAttributes.billing_currency</c> enum (CHF, EUR, USD, GBP).
    /// </summary>
    [Test]
    public void PingenApiCurrency_AllValuesAreCovered() => Enum.GetValues<PingenApiCurrency>().Length.ShouldBe(4);

    // -------------------------------------------------------------------
    // Section J: PingenApiDataType (real enum, 14 values, snake_case identifiers)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Verifies that every <see cref="PingenApiDataType" /> value serializes to its snake_case JSON identifier.
    /// </summary>
    /// <param name="value">The enum value under test.</param>
    /// <param name="expectedJson">The expected JSON representation.</param>
    [TestCase(PingenApiDataType.letters, "\"letters\"")]
    [TestCase(PingenApiDataType.batches, "\"batches\"")]
    [TestCase(PingenApiDataType.organisations, "\"organisations\"")]
    [TestCase(PingenApiDataType.letter_price_calculator, "\"letter_price_calculator\"")]
    [TestCase(PingenApiDataType.letters_events, "\"letters_events\"")]
    [TestCase(PingenApiDataType.users, "\"users\"")]
    [TestCase(PingenApiDataType.associations, "\"associations\"")]
    [TestCase(PingenApiDataType.webhooks, "\"webhooks\"")]
    [TestCase(PingenApiDataType.file_uploads, "\"file_uploads\"")]
    [TestCase(PingenApiDataType.webhook_issues, "\"webhook_issues\"")]
    [TestCase(PingenApiDataType.webhook_sent, "\"webhook_sent\"")]
    [TestCase(PingenApiDataType.webhook_undeliverable, "\"webhook_undeliverable\"")]
    [TestCase(PingenApiDataType.delivery_products, "\"delivery_products\"")]
    [TestCase(PingenApiDataType.presets, "\"presets\"")]
    public void PingenApiDataType_Serializes(PingenApiDataType value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);

    /// <summary>
    ///     Verifies that every <see cref="PingenApiDataType" /> JSON string deserializes back to the expected value.
    /// </summary>
    /// <param name="json">The JSON string under test.</param>
    /// <param name="expected">The expected enum value.</param>
    [TestCase("\"letters\"", PingenApiDataType.letters)]
    [TestCase("\"batches\"", PingenApiDataType.batches)]
    [TestCase("\"organisations\"", PingenApiDataType.organisations)]
    [TestCase("\"letter_price_calculator\"", PingenApiDataType.letter_price_calculator)]
    [TestCase("\"letters_events\"", PingenApiDataType.letters_events)]
    [TestCase("\"users\"", PingenApiDataType.users)]
    [TestCase("\"associations\"", PingenApiDataType.associations)]
    [TestCase("\"webhooks\"", PingenApiDataType.webhooks)]
    [TestCase("\"file_uploads\"", PingenApiDataType.file_uploads)]
    [TestCase("\"webhook_issues\"", PingenApiDataType.webhook_issues)]
    [TestCase("\"webhook_sent\"", PingenApiDataType.webhook_sent)]
    [TestCase("\"webhook_undeliverable\"", PingenApiDataType.webhook_undeliverable)]
    [TestCase("\"delivery_products\"", PingenApiDataType.delivery_products)]
    [TestCase("\"presets\"", PingenApiDataType.presets)]
    public void PingenApiDataType_Deserializes(string json, PingenApiDataType expected) =>
        PingenSerialisationHelper.Deserialize<PingenApiDataType>(json).ShouldBe(expected);

    /// <summary>
    ///     Sentinel: fails if a new <see cref="PingenApiDataType" /> value is added without updating the cases above.
    /// </summary>
    [Test]
    public void PingenApiDataType_AllValuesAreCovered() => Enum.GetValues<PingenApiDataType>().Length.ShouldBe(14);

    // -------------------------------------------------------------------
    // Section K: CollectionFilterOperator (static class of const string, 2 constants)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Regression-pin: verifies that every <see cref="CollectionFilterOperator" /> constant has the exact expected string
    ///     value.
    /// </summary>
    /// <param name="expected">The expected string value.</param>
    /// <param name="actual">The constant value declared by the source.</param>
    [TestCase("and", CollectionFilterOperator.And)]
    [TestCase("or", CollectionFilterOperator.Or)]
    public void CollectionFilterOperator_HasExpectedConstantValue(string expected, string actual) =>
        actual.ShouldBe(expected);

    /// <summary>
    ///     Verifies that every <see cref="CollectionFilterOperator" /> constant serializes as a JSON string payload.
    /// </summary>
    /// <param name="value">The constant value under test.</param>
    /// <param name="expectedJson">The expected JSON string.</param>
    [TestCase(CollectionFilterOperator.And, "\"and\"")]
    [TestCase(CollectionFilterOperator.Or, "\"or\"")]
    public void CollectionFilterOperator_SerializesAsJsonString(string value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);

    // -------------------------------------------------------------------
    // Section L: PingenApiLanguage (static class of const string, 5 constants)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Regression-pin: verifies that every <see cref="PingenApiLanguage" /> constant has the exact expected string value,
    ///     preserving the hyphen between language and region codes.
    /// </summary>
    /// <param name="expected">The expected string value.</param>
    /// <param name="actual">The constant value declared by the source.</param>
    [TestCase("en-GB", PingenApiLanguage.EnGB)]
    [TestCase("de-DE", PingenApiLanguage.DeDE)]
    [TestCase("de-CH", PingenApiLanguage.DeCH)]
    [TestCase("nl-NL", PingenApiLanguage.NlNL)]
    [TestCase("fr-FR", PingenApiLanguage.FrFR)]
    public void PingenApiLanguage_HasExpectedConstantValue(string expected, string actual) => actual.ShouldBe(expected);

    /// <summary>
    ///     Verifies that every <see cref="PingenApiLanguage" /> constant serializes as a JSON string payload.
    /// </summary>
    /// <param name="value">The constant value under test.</param>
    /// <param name="expectedJson">The expected JSON string.</param>
    [TestCase(PingenApiLanguage.EnGB, "\"en-GB\"")]
    [TestCase(PingenApiLanguage.DeDE, "\"de-DE\"")]
    [TestCase(PingenApiLanguage.DeCH, "\"de-CH\"")]
    [TestCase(PingenApiLanguage.NlNL, "\"nl-NL\"")]
    [TestCase(PingenApiLanguage.FrFR, "\"fr-FR\"")]
    public void PingenApiLanguage_SerializesAsJsonString(string value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);
}
