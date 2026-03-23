using System.Text.Json.Serialization;
using PingenApiNet.Abstractions.Helpers;

namespace PingenApiNet.Tests.Tests.Unit.Helpers;

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

        Assert.That(json, Does.Contain("2024-06-15T14:30:00"));
    }

    /// <summary>
    /// Verifies DateTime can round-trip through serialization
    /// </summary>
    [Test]
    public void Deserialize_PingenDateTimeFormat_ReturnsCorrectDateTime()
    {
        var json = "{\"date\":\"2024-06-15T14:30:00+00:00\"}";

        var result = PingenSerialisationHelper.Deserialize<DateTimeHolder>(json);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Date.Year, Is.EqualTo(2024));
            Assert.That(result.Date.Month, Is.EqualTo(6));
            Assert.That(result.Date.Day, Is.EqualTo(15));
        });
    }

    /// <summary>
    /// Verifies empty DateTime string deserializes to DateTime.MinValue
    /// </summary>
    [Test]
    public void Deserialize_EmptyDateTimeString_ReturnsMinValue()
    {
        var json = "{\"date\":\"\"}";

        var result = PingenSerialisationHelper.Deserialize<DateTimeHolder>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Date, Is.EqualTo(DateTime.MinValue));
    }

    /// <summary>
    /// Verifies nullable DateTime handles null properly
    /// </summary>
    [Test]
    public void Serialize_NullableDateTime_OmitsWhenNull()
    {
        var obj = new NullableDateTimeHolder { Date = null };

        var json = PingenSerialisationHelper.Serialize(obj);

        Assert.That(json, Does.Not.Contain("date"));
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
