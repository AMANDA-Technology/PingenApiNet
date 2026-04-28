using PingenApiNet.Abstractions.Enums.Batches;
using PingenApiNet.Abstractions.Helpers;

namespace PingenApiNet.UnitTests.Tests.Enums;

/// <summary>
///     Verifies that BatchIcon enum values serialize and deserialize correctly via System.Text.Json,
///     including the three hyphenated <c>JsonStringEnumMemberName</c> overrides.
/// </summary>
public class BatchIconSerializationTests
{
    /// <summary>
    ///     Verifies that every BatchIcon value serializes to the expected JSON string,
    ///     including the three hyphenated overrides via <c>JsonStringEnumMemberName</c>.
    /// </summary>
    /// <param name="value">The BatchIcon value under test.</param>
    /// <param name="expectedJson">The expected JSON string representation.</param>
    [TestCase(BatchIcon.campaign, "\"campaign\"")]
    [TestCase(BatchIcon.megaphone, "\"megaphone\"")]
    [TestCase(BatchIcon.waveHand, "\"wave-hand\"")]
    [TestCase(BatchIcon.flash, "\"flash\"")]
    [TestCase(BatchIcon.rocket, "\"rocket\"")]
    [TestCase(BatchIcon.bell, "\"bell\"")]
    [TestCase(BatchIcon.percentTag, "\"percent-tag\"")]
    [TestCase(BatchIcon.percentBadge, "\"percent-badge\"")]
    [TestCase(BatchIcon.present, "\"present\"")]
    [TestCase(BatchIcon.receipt, "\"receipt\"")]
    [TestCase(BatchIcon.document, "\"document\"")]
    [TestCase(BatchIcon.information, "\"information\"")]
    [TestCase(BatchIcon.calendar, "\"calendar\"")]
    [TestCase(BatchIcon.newspaper, "\"newspaper\"")]
    [TestCase(BatchIcon.crown, "\"crown\"")]
    [TestCase(BatchIcon.virus, "\"virus\"")]
    public void BatchIcon_Serializes(BatchIcon value, string expectedJson) =>
        PingenSerialisationHelper.Serialize(value).ShouldBe(expectedJson);

    /// <summary>
    ///     Verifies that every BatchIcon JSON string deserializes back to the expected enum value.
    /// </summary>
    /// <param name="json">The JSON string under test.</param>
    /// <param name="expected">The expected BatchIcon value.</param>
    [TestCase("\"campaign\"", BatchIcon.campaign)]
    [TestCase("\"megaphone\"", BatchIcon.megaphone)]
    [TestCase("\"wave-hand\"", BatchIcon.waveHand)]
    [TestCase("\"flash\"", BatchIcon.flash)]
    [TestCase("\"rocket\"", BatchIcon.rocket)]
    [TestCase("\"bell\"", BatchIcon.bell)]
    [TestCase("\"percent-tag\"", BatchIcon.percentTag)]
    [TestCase("\"percent-badge\"", BatchIcon.percentBadge)]
    [TestCase("\"present\"", BatchIcon.present)]
    [TestCase("\"receipt\"", BatchIcon.receipt)]
    [TestCase("\"document\"", BatchIcon.document)]
    [TestCase("\"information\"", BatchIcon.information)]
    [TestCase("\"calendar\"", BatchIcon.calendar)]
    [TestCase("\"newspaper\"", BatchIcon.newspaper)]
    [TestCase("\"crown\"", BatchIcon.crown)]
    [TestCase("\"virus\"", BatchIcon.virus)]
    public void BatchIcon_Deserializes(string json, BatchIcon expected) =>
        PingenSerialisationHelper.Deserialize<BatchIcon>(json).ShouldBe(expected);

    /// <summary>
    ///     Regression: verifies the three hyphenated <c>JsonStringEnumMemberName</c> overrides round-trip
    ///     without loss.
    /// </summary>
    /// <param name="value">The hyphenated BatchIcon value under test.</param>
    [TestCase(BatchIcon.waveHand)]
    [TestCase(BatchIcon.percentTag)]
    [TestCase(BatchIcon.percentBadge)]
    public void BatchIcon_HyphenatedValues_RoundTrip(BatchIcon value)
    {
        string json = PingenSerialisationHelper.Serialize(value);

        PingenSerialisationHelper.Deserialize<BatchIcon>(json).ShouldBe(value);
    }

    /// <summary>
    ///     Sentinel: fails if a new BatchIcon value is added without updating the parametrized cases above.
    /// </summary>
    [Test]
    public void BatchIcon_AllValuesAreCovered() => Enum.GetValues<BatchIcon>().Length.ShouldBe(16);
}
