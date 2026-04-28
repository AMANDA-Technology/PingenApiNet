using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Helpers.JsonConverters;

namespace PingenApiNet.UnitTests.Tests.Helpers;

/// <summary>
///     Unit tests for PingenDateTimeNullableConverter
/// </summary>
public class PingenDateTimeNullableConverterTests
{
    /// <summary>
    ///     Verifies that a non-null DateTime? serializes using Pingen date format
    /// </summary>
    [Test]
    public void Serialize_WithValue_UsesPingenFormat()
    {
        var obj = new NullableDateTimeHolder { Date = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Utc) };

        string json = PingenSerialisationHelper.Serialize(obj);

        json.ShouldContain("2024-06-15T14:30:00");
    }

    /// <summary>
    ///     Verifies that a valid Pingen date string deserializes to the correct DateTime?
    /// </summary>
    [Test]
    public void Deserialize_ValidDateString_ReturnsDateTime()
    {
        string json = "{\"date\":\"2024-06-15T14:30:00+00:00\"}";

        NullableDateTimeHolder? result = PingenSerialisationHelper.Deserialize<NullableDateTimeHolder>(json);

        result.ShouldNotBeNull();
        result!.Date.ShouldNotBeNull();
        result.Date!.Value.ShouldSatisfyAllConditions(
            () => result.Date!.Value.Year.ShouldBe(2024),
            () => result.Date!.Value.Month.ShouldBe(6),
            () => result.Date!.Value.Day.ShouldBe(15)
        );
    }

    /// <summary>
    ///     Verifies that a null JSON token deserializes to null DateTime?
    /// </summary>
    [Test]
    public void Deserialize_NullToken_ReturnsNull()
    {
        string json = "{\"date\":null}";

        NullableDateTimeHolder? result = PingenSerialisationHelper.Deserialize<NullableDateTimeHolder>(json);

        result.ShouldNotBeNull();
        result!.Date.ShouldBeNull();
    }

    /// <summary>
    ///     Verifies that an empty string deserializes to null DateTime?
    /// </summary>
    [Test]
    public void Deserialize_EmptyString_ReturnsNull()
    {
        string json = "{\"date\":\"\"}";

        NullableDateTimeHolder? result = PingenSerialisationHelper.Deserialize<NullableDateTimeHolder>(json);

        result.ShouldNotBeNull();
        result!.Date.ShouldBeNull();
    }

    /// <summary>
    ///     Verifies that a null DateTime? causes the property to be omitted from JSON
    /// </summary>
    [Test]
    public void Serialize_NullValue_OmitsProperty()
    {
        var obj = new NullableDateTimeHolder { Date = null };

        string json = PingenSerialisationHelper.Serialize(obj);

        json.ShouldNotContain("date");
    }

    /// <summary>
    ///     Verifies that a DateTime? value survives a round-trip through serialization
    /// </summary>
    [Test]
    public void RoundTrip_SerializeThenDeserialize_PreservesDate()
    {
        var original = new NullableDateTimeHolder { Date = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc) };

        string json = PingenSerialisationHelper.Serialize(original);
        NullableDateTimeHolder? result = PingenSerialisationHelper.Deserialize<NullableDateTimeHolder>(json);

        result.ShouldNotBeNull();
        result!.Date.ShouldNotBeNull();
        result.Date!.Value.ShouldSatisfyAllConditions(
            () => result.Date!.Value.Year.ShouldBe(2024),
            () => result.Date!.Value.Month.ShouldBe(6),
            () => result.Date!.Value.Day.ShouldBe(15)
        );
    }

    /// <summary>
    ///     Verifies that a DateTime.MinValue formatted string deserializes to a non-null DateTime equal to MinValue
    /// </summary>
    [Test]
    public void Deserialize_NullableMinValue_ReturnsMinValue()
    {
        string json = "{\"date\":\"0001-01-01T00:00:00+00:00\"}";

        NullableDateTimeHolder? result = PingenSerialisationHelper.Deserialize<NullableDateTimeHolder>(json);

        result.ShouldNotBeNull();
        result!.Date.ShouldNotBeNull();
        result.Date!.Value.ShouldBe(DateTime.MinValue);
    }

    /// <summary>
    ///     Verifies that DateTime.MaxValue serialised by the helper round-trips with seconds precision preserved
    /// </summary>
    [Test]
    public void Deserialize_NullableMaxValueFormatted_RoundTripsApproximately()
    {
        var obj = new NullableDateTimeHolder { Date = new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc) };

        string json = PingenSerialisationHelper.Serialize(obj);
        NullableDateTimeHolder? result = PingenSerialisationHelper.Deserialize<NullableDateTimeHolder>(json);

        result.ShouldNotBeNull();
        result!.Date.ShouldNotBeNull();
        result.Date!.Value.ShouldSatisfyAllConditions(
            () => result.Date!.Value.Year.ShouldBe(9999),
            () => result.Date!.Value.Month.ShouldBe(12),
            () => result.Date!.Value.Day.ShouldBe(31),
            () => result.Date!.Value.Hour.ShouldBe(23),
            () => result.Date!.Value.Minute.ShouldBe(59),
            () => result.Date!.Value.Second.ShouldBe(59)
        );
    }

    /// <summary>
    ///     Verifies that an unparseable date string deserializes to a null DateTime?
    /// </summary>
    [Test]
    public void Deserialize_InvalidString_ReturnsNull()
    {
        string json = "{\"date\":\"garbage\"}";

        NullableDateTimeHolder? result = PingenSerialisationHelper.Deserialize<NullableDateTimeHolder>(json);

        result.ShouldNotBeNull();
        result!.Date.ShouldBeNull();
    }

    /// <summary>
    ///     Verifies that a direct converter Read invocation of a JSON null token returns null
    /// </summary>
    [Test]
    public void Read_DirectInvocation_NullToken_ReturnsNull()
    {
        var converter = new PingenDateTimeNullableConverter();
        var opts = new JsonSerializerOptions();
        byte[] bytes = "null"u8.ToArray();
        var reader = new Utf8JsonReader(bytes);
        reader.Read();

        DateTime? result = converter.Read(ref reader, typeof(DateTime?), opts);

        result.ShouldBeNull();
    }

    /// <summary>
    ///     Verifies that a direct converter Write invocation of a null value writes a JSON null token
    /// </summary>
    [Test]
    public void Write_DirectInvocation_NullValue_WritesJsonNull()
    {
        var converter = new PingenDateTimeNullableConverter();
        var opts = new JsonSerializerOptions();
        using var ms = new MemoryStream();
        using (var writer = new Utf8JsonWriter(ms))
        {
            converter.Write(writer, null, opts);
        }

        Encoding.UTF8.GetString(ms.ToArray()).ShouldBe("null");
    }

    /// <summary>
    ///     Verifies that a non-null DateTime? round-trips with all date components preserved
    /// </summary>
    [Test]
    public void RoundTrip_PreservesNullableValue()
    {
        var obj = new NullableDateTimeHolder { Date = new DateTime(2024, 7, 1, 12, 0, 0, DateTimeKind.Utc) };

        string json = PingenSerialisationHelper.Serialize(obj);
        NullableDateTimeHolder? result = PingenSerialisationHelper.Deserialize<NullableDateTimeHolder>(json);

        result.ShouldNotBeNull();
        result!.Date.ShouldNotBeNull();
        result.Date!.Value.ShouldSatisfyAllConditions(
            () => result.Date!.Value.Year.ShouldBe(2024),
            () => result.Date!.Value.Month.ShouldBe(7),
            () => result.Date!.Value.Day.ShouldBe(1)
        );
    }

    private sealed record NullableDateTimeHolder
    {
        /// <summary>
        ///     Nullable date for testing
        /// </summary>
        [JsonPropertyName("date")]
        public DateTime? Date { get; init; }
    }
}
