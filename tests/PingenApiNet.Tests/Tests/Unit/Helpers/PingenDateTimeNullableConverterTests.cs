using System.Text.Json.Serialization;
using PingenApiNet.Abstractions.Helpers;

namespace PingenApiNet.Tests.Tests.Unit.Helpers;

/// <summary>
/// Unit tests for PingenDateTimeNullableConverter
/// </summary>
public class PingenDateTimeNullableConverterTests
{
    /// <summary>
    /// Verifies that a non-null DateTime? serializes using Pingen date format
    /// </summary>
    [Test]
    public void Serialize_WithValue_UsesPingenFormat()
    {
        var obj = new NullableDateTimeHolder { Date = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Utc) };

        var json = PingenSerialisationHelper.Serialize(obj);

        json.ShouldContain("2024-06-15T14:30:00");
    }

    /// <summary>
    /// Verifies that a valid Pingen date string deserializes to the correct DateTime?
    /// </summary>
    [Test]
    public void Deserialize_ValidDateString_ReturnsDateTime()
    {
        var json = "{\"date\":\"2024-06-15T14:30:00+00:00\"}";

        var result = PingenSerialisationHelper.Deserialize<NullableDateTimeHolder>(json);

        result.ShouldNotBeNull();
        result!.Date.ShouldNotBeNull();
        result.Date!.Value.ShouldSatisfyAllConditions(
            () => result.Date!.Value.Year.ShouldBe(2024),
            () => result.Date!.Value.Month.ShouldBe(6),
            () => result.Date!.Value.Day.ShouldBe(15)
        );
    }

    /// <summary>
    /// Verifies that a null JSON token deserializes to null DateTime?
    /// </summary>
    [Test]
    public void Deserialize_NullToken_ReturnsNull()
    {
        var json = "{\"date\":null}";

        var result = PingenSerialisationHelper.Deserialize<NullableDateTimeHolder>(json);

        result.ShouldNotBeNull();
        result!.Date.ShouldBeNull();
    }

    /// <summary>
    /// Verifies that an empty string deserializes to null DateTime?
    /// </summary>
    [Test]
    public void Deserialize_EmptyString_ReturnsNull()
    {
        var json = "{\"date\":\"\"}";

        var result = PingenSerialisationHelper.Deserialize<NullableDateTimeHolder>(json);

        result.ShouldNotBeNull();
        result!.Date.ShouldBeNull();
    }

    /// <summary>
    /// Verifies that a null DateTime? causes the property to be omitted from JSON
    /// </summary>
    [Test]
    public void Serialize_NullValue_OmitsProperty()
    {
        var obj = new NullableDateTimeHolder { Date = null };

        var json = PingenSerialisationHelper.Serialize(obj);

        json.ShouldNotContain("date");
    }

    /// <summary>
    /// Verifies that a DateTime? value survives a round-trip through serialization
    /// </summary>
    [Test]
    public void RoundTrip_SerializeThenDeserialize_PreservesDate()
    {
        var original = new NullableDateTimeHolder { Date = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc) };

        var json = PingenSerialisationHelper.Serialize(original);
        var result = PingenSerialisationHelper.Deserialize<NullableDateTimeHolder>(json);

        result.ShouldNotBeNull();
        result!.Date.ShouldNotBeNull();
        result.Date!.Value.ShouldSatisfyAllConditions(
            () => result.Date!.Value.Year.ShouldBe(2024),
            () => result.Date!.Value.Month.ShouldBe(6),
            () => result.Date!.Value.Day.ShouldBe(15)
        );
    }

    private sealed record NullableDateTimeHolder
    {
        /// <summary>
        /// Nullable date for testing
        /// </summary>
        [JsonPropertyName("date")]
        public DateTime? Date { get; init; }
    }
}
