using PingenApiNet.Abstractions.Enums.Batches;
using PingenApiNet.Abstractions.Helpers;

namespace PingenApiNet.UnitTests.Tests.Enums;

/// <summary>
/// Verifies that BatchIcon enum values with hyphens serialize correctly via System.Text.Json
/// </summary>
public class BatchIconSerializationTests
{
    /// <summary>
    /// Verifies that BatchIcon.waveHand serializes to "wave-hand"
    /// </summary>
    [Test]
    public void WaveHand_SerializesToHyphenatedForm()
    {
        var json = PingenSerialisationHelper.Serialize(BatchIcon.waveHand);

        json.ShouldBe("\"wave-hand\"");
    }

    /// <summary>
    /// Verifies that BatchIcon.percentTag serializes to "percent-tag"
    /// </summary>
    [Test]
    public void PercentTag_SerializesToHyphenatedForm()
    {
        var json = PingenSerialisationHelper.Serialize(BatchIcon.percentTag);

        json.ShouldBe("\"percent-tag\"");
    }

    /// <summary>
    /// Verifies that BatchIcon.percentBadge serializes to "percent-badge"
    /// </summary>
    [Test]
    public void PercentBadge_SerializesToHyphenatedForm()
    {
        var json = PingenSerialisationHelper.Serialize(BatchIcon.percentBadge);

        json.ShouldBe("\"percent-badge\"");
    }

    /// <summary>
    /// Verifies that "wave-hand" deserializes to BatchIcon.waveHand
    /// </summary>
    [Test]
    public void WaveHand_DeserializesFromHyphenatedForm()
    {
        var result = PingenSerialisationHelper.Deserialize<BatchIcon>("\"wave-hand\"");

        result.ShouldBe(BatchIcon.waveHand);
    }

    /// <summary>
    /// Verifies that "percent-tag" deserializes to BatchIcon.percentTag
    /// </summary>
    [Test]
    public void PercentTag_DeserializesFromHyphenatedForm()
    {
        var result = PingenSerialisationHelper.Deserialize<BatchIcon>("\"percent-tag\"");

        result.ShouldBe(BatchIcon.percentTag);
    }

    /// <summary>
    /// Verifies that "percent-badge" deserializes to BatchIcon.percentBadge
    /// </summary>
    [Test]
    public void PercentBadge_DeserializesFromHyphenatedForm()
    {
        var result = PingenSerialisationHelper.Deserialize<BatchIcon>("\"percent-badge\"");

        result.ShouldBe(BatchIcon.percentBadge);
    }
}
