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

using PingenApiNet.Abstractions.Enums.LetterEvents;
using PingenApiNet.Abstractions.Enums.Letters;
using PingenApiNet.Abstractions.Helpers;

namespace PingenApiNet.UnitTests.Tests.Enums;

/// <summary>
///     Verifies serialization and deserialization of all letter-area enums and string-constant
///     lookups exposed by the Abstractions library: <see cref="LetterPrintMode" />,
///     <see cref="LetterAddressPosition" />, <see cref="LetterPrintSpectrum" />,
///     <see cref="LetterStates" />, <see cref="LetterCreateDeliveryProduct" />,
///     <see cref="LetterSendDeliveryProduct" />, <see cref="LetterPaperTypes" />, and
///     <see cref="LetterEventCodes" />.
/// </summary>
public class LetterEnumSerializationTests
{
    // -------------------------------------------------------------------
    // Section A: LetterPrintMode (real enum, 2 values)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Verifies that every <see cref="LetterPrintMode" /> value serializes to its lowercase JSON identifier.
    /// </summary>
    /// <param name="value">The enum value under test.</param>
    /// <param name="expectedJson">The expected JSON representation.</param>
    [TestCase(LetterPrintMode.simplex, "\"simplex\"")]
    [TestCase(LetterPrintMode.duplex, "\"duplex\"")]
    public void LetterPrintMode_Serializes(LetterPrintMode value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);

    /// <summary>
    ///     Verifies that every <see cref="LetterPrintMode" /> JSON string deserializes back to the expected value.
    /// </summary>
    /// <param name="json">The JSON string under test.</param>
    /// <param name="expected">The expected enum value.</param>
    [TestCase("\"simplex\"", LetterPrintMode.simplex)]
    [TestCase("\"duplex\"", LetterPrintMode.duplex)]
    public void LetterPrintMode_Deserializes(string json, LetterPrintMode expected) =>
        PingenSerialisationHelper.Deserialize<LetterPrintMode>(json).ShouldBe(expected);

    /// <summary>
    ///     Sentinel: fails if a new <see cref="LetterPrintMode" /> value is added without updating the cases above.
    /// </summary>
    [Test]
    public void LetterPrintMode_AllValuesAreCovered() => Enum.GetValues<LetterPrintMode>().Length.ShouldBe(2);

    // -------------------------------------------------------------------
    // Section B: LetterAddressPosition (real enum, 2 values)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Verifies that every <see cref="LetterAddressPosition" /> value serializes to its lowercase JSON identifier.
    /// </summary>
    /// <param name="value">The enum value under test.</param>
    /// <param name="expectedJson">The expected JSON representation.</param>
    [TestCase(LetterAddressPosition.left, "\"left\"")]
    [TestCase(LetterAddressPosition.right, "\"right\"")]
    public void LetterAddressPosition_Serializes(LetterAddressPosition value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);

    /// <summary>
    ///     Verifies that every <see cref="LetterAddressPosition" /> JSON string deserializes back to the expected value.
    /// </summary>
    /// <param name="json">The JSON string under test.</param>
    /// <param name="expected">The expected enum value.</param>
    [TestCase("\"left\"", LetterAddressPosition.left)]
    [TestCase("\"right\"", LetterAddressPosition.right)]
    public void LetterAddressPosition_Deserializes(string json, LetterAddressPosition expected) =>
        PingenSerialisationHelper.Deserialize<LetterAddressPosition>(json).ShouldBe(expected);

    /// <summary>
    ///     Sentinel: fails if a new <see cref="LetterAddressPosition" /> value is added without updating the cases above.
    /// </summary>
    [Test]
    public void LetterAddressPosition_AllValuesAreCovered() =>
        Enum.GetValues<LetterAddressPosition>().Length.ShouldBe(2);

    // -------------------------------------------------------------------
    // Section C: LetterPrintSpectrum (real enum, 2 values)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Verifies that every <see cref="LetterPrintSpectrum" /> value serializes to its lowercase JSON identifier.
    /// </summary>
    /// <param name="value">The enum value under test.</param>
    /// <param name="expectedJson">The expected JSON representation.</param>
    [TestCase(LetterPrintSpectrum.grayscale, "\"grayscale\"")]
    [TestCase(LetterPrintSpectrum.color, "\"color\"")]
    public void LetterPrintSpectrum_Serializes(LetterPrintSpectrum value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);

    /// <summary>
    ///     Verifies that every <see cref="LetterPrintSpectrum" /> JSON string deserializes back to the expected value.
    /// </summary>
    /// <param name="json">The JSON string under test.</param>
    /// <param name="expected">The expected enum value.</param>
    [TestCase("\"grayscale\"", LetterPrintSpectrum.grayscale)]
    [TestCase("\"color\"", LetterPrintSpectrum.color)]
    public void LetterPrintSpectrum_Deserializes(string json, LetterPrintSpectrum expected) =>
        PingenSerialisationHelper.Deserialize<LetterPrintSpectrum>(json).ShouldBe(expected);

    /// <summary>
    ///     Sentinel: fails if a new <see cref="LetterPrintSpectrum" /> value is added without updating the cases above.
    /// </summary>
    [Test]
    public void LetterPrintSpectrum_AllValuesAreCovered() => Enum.GetValues<LetterPrintSpectrum>().Length.ShouldBe(2);

    // -------------------------------------------------------------------
    // Section D: LetterStates (static class of const string, 20 constants)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Regression-pin: verifies that every <see cref="LetterStates" /> constant has the exact expected string value.
    /// </summary>
    /// <param name="expected">The expected string value.</param>
    /// <param name="actual">The constant value declared by the source.</param>
    [TestCase("validating", LetterStates.Validating)]
    [TestCase("cancelled", LetterStates.Cancelled)]
    [TestCase("cancelling", LetterStates.Cancelling)]
    [TestCase("unprintable", LetterStates.Unprintable)]
    [TestCase("fixing", LetterStates.Fixing)]
    [TestCase("invalid", LetterStates.Invalid)]
    [TestCase("submitted", LetterStates.Submitted)]
    [TestCase("accepted", LetterStates.Accepted)]
    [TestCase("printing", LetterStates.Printing)]
    [TestCase("processing", LetterStates.Processing)]
    [TestCase("sent", LetterStates.Sent)]
    [TestCase("action_required", LetterStates.ActionRequired)]
    [TestCase("undeliverable", LetterStates.Undeliverable)]
    [TestCase("valid", LetterStates.Valid)]
    [TestCase("awaiting_credits", LetterStates.AwaitingCredits)]
    [TestCase("expired", LetterStates.Expired)]
    [TestCase("transferring", LetterStates.Transferring)]
    [TestCase("inspection", LetterStates.Inspection)]
    [TestCase("rejected", LetterStates.Rejected)]
    [TestCase("delivered", LetterStates.Delivered)]
    public void LetterStates_HasExpectedConstantValue(string expected, string actual) => actual.ShouldBe(expected);

    /// <summary>
    ///     Verifies that every <see cref="LetterStates" /> constant serializes as a JSON string payload.
    /// </summary>
    /// <param name="value">The constant value under test.</param>
    /// <param name="expectedJson">The expected JSON string.</param>
    [TestCase(LetterStates.Validating, "\"validating\"")]
    [TestCase(LetterStates.Cancelled, "\"cancelled\"")]
    [TestCase(LetterStates.Cancelling, "\"cancelling\"")]
    [TestCase(LetterStates.Unprintable, "\"unprintable\"")]
    [TestCase(LetterStates.Fixing, "\"fixing\"")]
    [TestCase(LetterStates.Invalid, "\"invalid\"")]
    [TestCase(LetterStates.Submitted, "\"submitted\"")]
    [TestCase(LetterStates.Accepted, "\"accepted\"")]
    [TestCase(LetterStates.Printing, "\"printing\"")]
    [TestCase(LetterStates.Processing, "\"processing\"")]
    [TestCase(LetterStates.Sent, "\"sent\"")]
    [TestCase(LetterStates.ActionRequired, "\"action_required\"")]
    [TestCase(LetterStates.Undeliverable, "\"undeliverable\"")]
    [TestCase(LetterStates.Valid, "\"valid\"")]
    [TestCase(LetterStates.AwaitingCredits, "\"awaiting_credits\"")]
    [TestCase(LetterStates.Expired, "\"expired\"")]
    [TestCase(LetterStates.Transferring, "\"transferring\"")]
    [TestCase(LetterStates.Inspection, "\"inspection\"")]
    [TestCase(LetterStates.Rejected, "\"rejected\"")]
    [TestCase(LetterStates.Delivered, "\"delivered\"")]
    public void LetterStates_SerializesAsJsonString(string value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);

    // -------------------------------------------------------------------
    // Section E: LetterCreateDeliveryProduct (static class of const string, 5 constants)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Regression-pin: verifies that every <see cref="LetterCreateDeliveryProduct" /> constant has the exact expected
    ///     string value.
    /// </summary>
    /// <param name="expected">The expected string value.</param>
    /// <param name="actual">The constant value declared by the source.</param>
    [TestCase("fast", LetterCreateDeliveryProduct.Fast)]
    [TestCase("cheap", LetterCreateDeliveryProduct.Cheap)]
    [TestCase("bulk", LetterCreateDeliveryProduct.Bulk)]
    [TestCase("premium", LetterCreateDeliveryProduct.Premium)]
    [TestCase("registered", LetterCreateDeliveryProduct.Registered)]
    public void LetterCreateDeliveryProduct_HasExpectedConstantValue(string expected, string actual) =>
        actual.ShouldBe(expected);

    /// <summary>
    ///     Verifies that every <see cref="LetterCreateDeliveryProduct" /> constant serializes as a JSON string payload.
    /// </summary>
    /// <param name="value">The constant value under test.</param>
    /// <param name="expectedJson">The expected JSON string.</param>
    [TestCase(LetterCreateDeliveryProduct.Fast, "\"fast\"")]
    [TestCase(LetterCreateDeliveryProduct.Cheap, "\"cheap\"")]
    [TestCase(LetterCreateDeliveryProduct.Bulk, "\"bulk\"")]
    [TestCase(LetterCreateDeliveryProduct.Premium, "\"premium\"")]
    [TestCase(LetterCreateDeliveryProduct.Registered, "\"registered\"")]
    public void LetterCreateDeliveryProduct_SerializesAsJsonString(string value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);

    // -------------------------------------------------------------------
    // Section F: LetterSendDeliveryProduct (static class of const string, 14 constants)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Regression-pin: verifies that every <see cref="LetterSendDeliveryProduct" /> constant has the exact expected string
    ///     value.
    /// </summary>
    /// <param name="expected">The expected string value.</param>
    /// <param name="actual">The constant value declared by the source.</param>
    [TestCase("atpost_economy", LetterSendDeliveryProduct.AtPostEconomy)]
    [TestCase("atpost_priority", LetterSendDeliveryProduct.AtPostPriority)]
    [TestCase("postag_a", LetterSendDeliveryProduct.PostAgA)]
    [TestCase("postag_b", LetterSendDeliveryProduct.PostAgB)]
    [TestCase("postag_b2", LetterSendDeliveryProduct.PostAgB2)]
    [TestCase("postag_registered", LetterSendDeliveryProduct.PostAgRegistered)]
    [TestCase("postag_aplus", LetterSendDeliveryProduct.PostAgAPlus)]
    [TestCase("dpag_standard", LetterSendDeliveryProduct.DpAgStandard)]
    [TestCase("dpag_economy", LetterSendDeliveryProduct.DpAgEconomy)]
    [TestCase("indpost_mail", LetterSendDeliveryProduct.IndPostMail)]
    [TestCase("indpost_speedmail", LetterSendDeliveryProduct.IndPostSpeedMail)]
    [TestCase("nlpost_priority", LetterSendDeliveryProduct.NlPostPriority)]
    [TestCase("dhl_europe_priority", LetterSendDeliveryProduct.DhlEuropePriority)]
    [TestCase("dhl_world_priority", LetterSendDeliveryProduct.DhlWorldPriority)]
    public void LetterSendDeliveryProduct_HasExpectedConstantValue(string expected, string actual) =>
        actual.ShouldBe(expected);

    /// <summary>
    ///     Verifies that every <see cref="LetterSendDeliveryProduct" /> constant serializes as a JSON string payload.
    /// </summary>
    /// <param name="value">The constant value under test.</param>
    /// <param name="expectedJson">The expected JSON string.</param>
    [TestCase(LetterSendDeliveryProduct.AtPostEconomy, "\"atpost_economy\"")]
    [TestCase(LetterSendDeliveryProduct.AtPostPriority, "\"atpost_priority\"")]
    [TestCase(LetterSendDeliveryProduct.PostAgA, "\"postag_a\"")]
    [TestCase(LetterSendDeliveryProduct.PostAgB, "\"postag_b\"")]
    [TestCase(LetterSendDeliveryProduct.PostAgB2, "\"postag_b2\"")]
    [TestCase(LetterSendDeliveryProduct.PostAgRegistered, "\"postag_registered\"")]
    [TestCase(LetterSendDeliveryProduct.PostAgAPlus, "\"postag_aplus\"")]
    [TestCase(LetterSendDeliveryProduct.DpAgStandard, "\"dpag_standard\"")]
    [TestCase(LetterSendDeliveryProduct.DpAgEconomy, "\"dpag_economy\"")]
    [TestCase(LetterSendDeliveryProduct.IndPostMail, "\"indpost_mail\"")]
    [TestCase(LetterSendDeliveryProduct.IndPostSpeedMail, "\"indpost_speedmail\"")]
    [TestCase(LetterSendDeliveryProduct.NlPostPriority, "\"nlpost_priority\"")]
    [TestCase(LetterSendDeliveryProduct.DhlEuropePriority, "\"dhl_europe_priority\"")]
    [TestCase(LetterSendDeliveryProduct.DhlWorldPriority, "\"dhl_world_priority\"")]
    public void LetterSendDeliveryProduct_SerializesAsJsonString(string value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);

    // -------------------------------------------------------------------
    // Section G: LetterPaperTypes (static class of const string, 7 constants)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Regression-pin: verifies that every <see cref="LetterPaperTypes" /> constant has the exact expected string value.
    /// </summary>
    /// <param name="expected">The expected string value.</param>
    /// <param name="actual">The constant value declared by the source.</param>
    [TestCase("normal", LetterPaperTypes.Normal)]
    [TestCase("qr", LetterPaperTypes.Qr)]
    [TestCase("is", LetterPaperTypes.Is)]
    [TestCase("isr", LetterPaperTypes.Isr)]
    [TestCase("isr+", LetterPaperTypes.IsrPlus)]
    [TestCase("sepa_at", LetterPaperTypes.SepaAt)]
    [TestCase("sepa_de", LetterPaperTypes.SepaDe)]
    public void LetterPaperTypes_HasExpectedConstantValue(string expected, string actual) => actual.ShouldBe(expected);

    /// <summary>
    ///     Verifies that every <see cref="LetterPaperTypes" /> constant serializes as a JSON string payload.
    ///     Note: <see cref="LetterPaperTypes.IsrPlus" /> contains a <c>+</c> which the default
    ///     <c>System.Text.Json</c> encoder escapes to the <c>+</c> sequence — this is the documented
    ///     safe-set behaviour and is preserved here as the expected wire format.
    /// </summary>
    /// <param name="value">The constant value under test.</param>
    /// <param name="expectedJson">The expected JSON string.</param>
    [TestCase(LetterPaperTypes.Normal, "\"normal\"")]
    [TestCase(LetterPaperTypes.Qr, "\"qr\"")]
    [TestCase(LetterPaperTypes.Is, "\"is\"")]
    [TestCase(LetterPaperTypes.Isr, "\"isr\"")]
    [TestCase(LetterPaperTypes.IsrPlus, "\"isr\\u002B\"")]
    [TestCase(LetterPaperTypes.SepaAt, "\"sepa_at\"")]
    [TestCase(LetterPaperTypes.SepaDe, "\"sepa_de\"")]
    public void LetterPaperTypes_SerializesAsJsonString(string value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);

    // -------------------------------------------------------------------
    // Section H: LetterEventCodes (static class of const string, 42 constants)
    // -------------------------------------------------------------------

    /// <summary>
    ///     Regression-pin: verifies that every <see cref="LetterEventCodes" /> constant has the exact expected string value.
    /// </summary>
    /// <param name="expected">The expected string value.</param>
    /// <param name="actual">The constant value declared by the source.</param>
    [TestCase("auto_initiated_sending", LetterEventCodes.AutoInitiatedSending)]
    [TestCase("change_address_position", LetterEventCodes.ChangeAddressPosition)]
    [TestCase("initiated_sending", LetterEventCodes.InitiatedSending)]
    [TestCase("queued_for_transfer", LetterEventCodes.QueuedForTransfer)]
    [TestCase("transferred_to_pool", LetterEventCodes.TransferredToPool)]
    [TestCase("accepted", LetterEventCodes.Accepted)]
    [TestCase("address_position_changed", LetterEventCodes.AddressPositionChanged)]
    [TestCase("address_purged", LetterEventCodes.AddressPurged)]
    [TestCase("auto_submitted", LetterEventCodes.AutoSubmitted)]
    [TestCase("cancellation_completed", LetterEventCodes.CancellationCompleted)]
    [TestCase("cancellation_rejected", LetterEventCodes.CancellationRejected)]
    [TestCase("cancellation_requested", LetterEventCodes.CancellationRequested)]
    [TestCase("content_failed_inspection", LetterEventCodes.ContentFailedInspection)]
    [TestCase("content_inspection_error", LetterEventCodes.ContentInspectionError)]
    [TestCase("content_passed_inspection", LetterEventCodes.ContentPassedInspection)]
    [TestCase("created", LetterEventCodes.Created)]
    [TestCase("delivered", LetterEventCodes.Delivered)]
    [TestCase("file_not_a4_portrait", LetterEventCodes.FileNotA4Portrait)]
    [TestCase("file_not_found", LetterEventCodes.FileNotFound)]
    [TestCase("file_not_readable", LetterEventCodes.FileNotReadable)]
    [TestCase("file_removed", LetterEventCodes.FileRemoved)]
    [TestCase("file_size_over_limit", LetterEventCodes.FileSizeOverLimit)]
    [TestCase("file_too_many_pages", LetterEventCodes.FileTooManyPages)]
    [TestCase("invalid_pdf_binary_content", LetterEventCodes.InvalidPdfBinaryContent)]
    [TestCase("meta_data_failed_requirements", LetterEventCodes.MetaDataFailedRequirements)]
    [TestCase("processing", LetterEventCodes.Processing)]
    [TestCase("receival_confirmed_by_distributor", LetterEventCodes.ReceivalConfirmedByDistributor)]
    [TestCase("rejected_from_printcenter", LetterEventCodes.RejectedFromPrintcenter)]
    [TestCase("rerouted", LetterEventCodes.Rerouted)]
    [TestCase("retracted_from_printcenter", LetterEventCodes.RetractedFromPrintcenter)]
    [TestCase("shipping_country_blocked", LetterEventCodes.ShippingCountryBlocked)]
    [TestCase("shipping_organization_country_blocked", LetterEventCodes.ShippingOrganizationCountryBlocked)]
    [TestCase("submitted", LetterEventCodes.Submitted)]
    [TestCase("track_and_trace", LetterEventCodes.TrackAndTrace)]
    [TestCase("transferred_to_distributor", LetterEventCodes.TransferredToDistributor)]
    [TestCase("transferred_to_printcenter", LetterEventCodes.TransferredToPrintcenter)]
    [TestCase("unable_to_find_delivery_product", LetterEventCodes.UnableToFindDeliveryProduct)]
    [TestCase("undeliverable", LetterEventCodes.Undeliverable)]
    [TestCase("waiting_for_funds", LetterEventCodes.WaitingForFunds)]
    [TestCase("waiting_for_funds_expired", LetterEventCodes.WaitingForFundsExpired)]
    [TestCase("waiting_for_legitimacy", LetterEventCodes.WaitingForLegitimacy)]
    [TestCase("waiting_for_legitimacy_expired", LetterEventCodes.WaitingForLegitimacyExpired)]
    public void LetterEventCodes_HasExpectedConstantValue(string expected, string actual) => actual.ShouldBe(expected);

    /// <summary>
    ///     Verifies that every <see cref="LetterEventCodes" /> constant serializes as a JSON string payload.
    /// </summary>
    /// <param name="value">The constant value under test.</param>
    /// <param name="expectedJson">The expected JSON string.</param>
    [TestCase(LetterEventCodes.AutoInitiatedSending, "\"auto_initiated_sending\"")]
    [TestCase(LetterEventCodes.ChangeAddressPosition, "\"change_address_position\"")]
    [TestCase(LetterEventCodes.InitiatedSending, "\"initiated_sending\"")]
    [TestCase(LetterEventCodes.QueuedForTransfer, "\"queued_for_transfer\"")]
    [TestCase(LetterEventCodes.TransferredToPool, "\"transferred_to_pool\"")]
    [TestCase(LetterEventCodes.Accepted, "\"accepted\"")]
    [TestCase(LetterEventCodes.AddressPositionChanged, "\"address_position_changed\"")]
    [TestCase(LetterEventCodes.AddressPurged, "\"address_purged\"")]
    [TestCase(LetterEventCodes.AutoSubmitted, "\"auto_submitted\"")]
    [TestCase(LetterEventCodes.CancellationCompleted, "\"cancellation_completed\"")]
    [TestCase(LetterEventCodes.CancellationRejected, "\"cancellation_rejected\"")]
    [TestCase(LetterEventCodes.CancellationRequested, "\"cancellation_requested\"")]
    [TestCase(LetterEventCodes.ContentFailedInspection, "\"content_failed_inspection\"")]
    [TestCase(LetterEventCodes.ContentInspectionError, "\"content_inspection_error\"")]
    [TestCase(LetterEventCodes.ContentPassedInspection, "\"content_passed_inspection\"")]
    [TestCase(LetterEventCodes.Created, "\"created\"")]
    [TestCase(LetterEventCodes.Delivered, "\"delivered\"")]
    [TestCase(LetterEventCodes.FileNotA4Portrait, "\"file_not_a4_portrait\"")]
    [TestCase(LetterEventCodes.FileNotFound, "\"file_not_found\"")]
    [TestCase(LetterEventCodes.FileNotReadable, "\"file_not_readable\"")]
    [TestCase(LetterEventCodes.FileRemoved, "\"file_removed\"")]
    [TestCase(LetterEventCodes.FileSizeOverLimit, "\"file_size_over_limit\"")]
    [TestCase(LetterEventCodes.FileTooManyPages, "\"file_too_many_pages\"")]
    [TestCase(LetterEventCodes.InvalidPdfBinaryContent, "\"invalid_pdf_binary_content\"")]
    [TestCase(LetterEventCodes.MetaDataFailedRequirements, "\"meta_data_failed_requirements\"")]
    [TestCase(LetterEventCodes.Processing, "\"processing\"")]
    [TestCase(LetterEventCodes.ReceivalConfirmedByDistributor, "\"receival_confirmed_by_distributor\"")]
    [TestCase(LetterEventCodes.RejectedFromPrintcenter, "\"rejected_from_printcenter\"")]
    [TestCase(LetterEventCodes.Rerouted, "\"rerouted\"")]
    [TestCase(LetterEventCodes.RetractedFromPrintcenter, "\"retracted_from_printcenter\"")]
    [TestCase(LetterEventCodes.ShippingCountryBlocked, "\"shipping_country_blocked\"")]
    [TestCase(LetterEventCodes.ShippingOrganizationCountryBlocked, "\"shipping_organization_country_blocked\"")]
    [TestCase(LetterEventCodes.Submitted, "\"submitted\"")]
    [TestCase(LetterEventCodes.TrackAndTrace, "\"track_and_trace\"")]
    [TestCase(LetterEventCodes.TransferredToDistributor, "\"transferred_to_distributor\"")]
    [TestCase(LetterEventCodes.TransferredToPrintcenter, "\"transferred_to_printcenter\"")]
    [TestCase(LetterEventCodes.UnableToFindDeliveryProduct, "\"unable_to_find_delivery_product\"")]
    [TestCase(LetterEventCodes.Undeliverable, "\"undeliverable\"")]
    [TestCase(LetterEventCodes.WaitingForFunds, "\"waiting_for_funds\"")]
    [TestCase(LetterEventCodes.WaitingForFundsExpired, "\"waiting_for_funds_expired\"")]
    [TestCase(LetterEventCodes.WaitingForLegitimacy, "\"waiting_for_legitimacy\"")]
    [TestCase(LetterEventCodes.WaitingForLegitimacyExpired, "\"waiting_for_legitimacy_expired\"")]
    public void LetterEventCodes_SerializesAsJsonString(string value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);
}
