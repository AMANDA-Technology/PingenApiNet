using System.Text.Json.Serialization;
using PingenApiNet.Abstractions.Helpers;

namespace PingenApiNet.UnitTests.Tests.Helpers;

/// <summary>
/// Unit tests for DateTime serialization via PingenSerialisationHelper custom converters
/// </summary>
public class PingenDateTimeConverterTests
{
    /// <summary>
    /// Verifies DateTime serializes to Pingen format
    /// </summary>
    [Test]
    public void Serialize_DateTime_UsesPingenFormat()
    {
        var obj = new DateTimeHolder { Date = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Utc) };

        var json = PingenSerialisationHelper.Serialize(obj);

        json.ShouldContain("2024-06-15T14:30:00");
    }

    /// <summary>
    /// Verifies DateTime can round-trip through serialization
    /// </summary>
    [Test]
    public void Deserialize_PingenDateTimeFormat_ReturnsCorrectDateTime()
    {
        var json = "{\"date\":\"2024-06-15T14:30:00+00:00\"}";

        var result = PingenSerialisationHelper.Deserialize<DateTimeHolder>(json);

        result.ShouldNotBeNull();
        result!.ShouldSatisfyAllConditions(
            () => result.Date.Year.ShouldBe(2024),
            () => result.Date.Month.ShouldBe(6),
            () => result.Date.Day.ShouldBe(15)
        );
    }

    /// <summary>
    /// Verifies empty DateTime string deserializes to DateTime.MinValue
    /// </summary>
    [Test]
    public void Deserialize_EmptyDateTimeString_ReturnsMinValue()
    {
        var json = "{\"date\":\"\"}";

        var result = PingenSerialisationHelper.Deserialize<DateTimeHolder>(json);

        result.ShouldNotBeNull();
        result.Date.ShouldBe(DateTime.MinValue);
    }

    /// <summary>
    /// Verifies nullable DateTime handles null properly
    /// </summary>
    [Test]
    public void Serialize_NullableDateTime_OmitsWhenNull()
    {
        var obj = new NullableDateTimeHolder { Date = null };

        var json = PingenSerialisationHelper.Serialize(obj);

        json.ShouldNotContain("date");
    }

    private sealed record DateTimeHolder
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; init; }
    }

    private sealed record NullableDateTimeHolder
    {
        [JsonPropertyName("date")]
        public DateTime? Date { get; init; }
    }
}
