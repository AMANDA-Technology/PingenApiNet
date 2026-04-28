using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Helpers.JsonConverters;

namespace PingenApiNet.UnitTests.Tests.Helpers;

/// <summary>
///     Unit tests for DateTime serialization via PingenSerialisationHelper custom converters
/// </summary>
public class PingenDateTimeConverterTests
{
    /// <summary>
    ///     Verifies DateTime serializes to Pingen format
    /// </summary>
    [Test]
    public void Serialize_DateTime_UsesPingenFormat()
    {
        var obj = new DateTimeHolder { Date = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Utc) };

        string json = PingenSerialisationHelper.Serialize(obj);

        json.ShouldContain("2024-06-15T14:30:00");
    }

    /// <summary>
    ///     Verifies DateTime can round-trip through serialization
    /// </summary>
    [Test]
    public void Deserialize_PingenDateTimeFormat_ReturnsCorrectDateTime()
    {
        string json = "{\"date\":\"2024-06-15T14:30:00+00:00\"}";

        DateTimeHolder? result = PingenSerialisationHelper.Deserialize<DateTimeHolder>(json);

        result.ShouldNotBeNull();
        result!.ShouldSatisfyAllConditions(
            () => result.Date.Year.ShouldBe(2024),
            () => result.Date.Month.ShouldBe(6),
            () => result.Date.Day.ShouldBe(15)
        );
    }

    /// <summary>
    ///     Verifies empty DateTime string deserializes to DateTime.MinValue
    /// </summary>
    [Test]
    public void Deserialize_EmptyDateTimeString_ReturnsMinValue()
    {
        string json = "{\"date\":\"\"}";

        DateTimeHolder? result = PingenSerialisationHelper.Deserialize<DateTimeHolder>(json);

        result.ShouldNotBeNull();
        result.Date.ShouldBe(DateTime.MinValue);
    }

    /// <summary>
    ///     Verifies nullable DateTime handles null properly
    /// </summary>
    [Test]
    public void Serialize_NullableDateTime_OmitsWhenNull()
    {
        var obj = new NullableDateTimeHolder { Date = null };

        string json = PingenSerialisationHelper.Serialize(obj);

        json.ShouldNotContain("date");
    }

    /// <summary>
    ///     Verifies that DateTime.MinValue round-trips through the helper at second precision
    /// </summary>
    [Test]
    public void Deserialize_DateTimeMinValue_RoundTripsThroughHelper()
    {
        var obj = new DateTimeHolder { Date = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc) };

        string json = PingenSerialisationHelper.Serialize(obj);
        DateTimeHolder? result = PingenSerialisationHelper.Deserialize<DateTimeHolder>(json);

        result.ShouldNotBeNull();
        result.Date.ShouldBe(DateTime.MinValue);
    }

    /// <summary>
    ///     Verifies that a DateTime near MaxValue round-trips with seconds precision preserved
    /// </summary>
    [Test]
    public void Deserialize_DateTimeMaxValue_RoundTripsApproximately()
    {
        var obj = new DateTimeHolder { Date = new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc) };

        string json = PingenSerialisationHelper.Serialize(obj);
        DateTimeHolder? result = PingenSerialisationHelper.Deserialize<DateTimeHolder>(json);

        result.ShouldNotBeNull();
        result!.ShouldSatisfyAllConditions(
            () => result.Date.Year.ShouldBe(9999),
            () => result.Date.Month.ShouldBe(12),
            () => result.Date.Day.ShouldBe(31),
            () => result.Date.Hour.ShouldBe(23),
            () => result.Date.Minute.ShouldBe(59),
            () => result.Date.Second.ShouldBe(59)
        );
    }

    /// <summary>
    ///     Verifies that an unparseable date string deserializes to DateTime.MinValue
    /// </summary>
    [Test]
    public void Deserialize_InvalidString_ReturnsMinValue()
    {
        string json = "{\"date\":\"not-a-date\"}";

        DateTimeHolder? result = PingenSerialisationHelper.Deserialize<DateTimeHolder>(json);

        result.ShouldNotBeNull();
        result.Date.ShouldBe(DateTime.MinValue);
    }

    /// <summary>
    ///     Verifies that a DateTime with a positive UTC offset round-trips at second precision
    /// </summary>
    [Test]
    public void Deserialize_TimezoneOffset_RoundTripsExact()
    {
        string json = "{\"date\":\"2024-06-15T14:30:00+02:00\"}";

        DateTimeHolder? result = PingenSerialisationHelper.Deserialize<DateTimeHolder>(json);

        result.ShouldNotBeNull();
        result!.ShouldSatisfyAllConditions(
            () => result.Date.Year.ShouldBe(2024),
            () => result.Date.Month.ShouldBe(6),
            () => result.Date.Day.ShouldBe(15),
            () => result.Date.Minute.ShouldBe(30),
            () => result.Date.Second.ShouldBe(0)
        );
    }

    /// <summary>
    ///     Verifies that a DateTime survives a round-trip with all components preserved at second precision
    /// </summary>
    [Test]
    public void RoundTrip_PreservesSecondPrecision()
    {
        var obj = new DateTimeHolder { Date = new DateTime(2024, 3, 10, 8, 45, 12, DateTimeKind.Utc) };

        string json = PingenSerialisationHelper.Serialize(obj);
        DateTimeHolder? result = PingenSerialisationHelper.Deserialize<DateTimeHolder>(json);

        result.ShouldNotBeNull();
        result!.ShouldSatisfyAllConditions(
            () => result.Date.Year.ShouldBe(2024),
            () => result.Date.Month.ShouldBe(3),
            () => result.Date.Day.ShouldBe(10),
            () => result.Date.Hour.ShouldBe(8),
            () => result.Date.Minute.ShouldBe(45),
            () => result.Date.Second.ShouldBe(12)
        );
    }

    /// <summary>
    ///     Verifies that a direct converter Read invocation of an empty JSON string returns DateTime.MinValue
    /// </summary>
    [Test]
    public void Read_DirectInvocation_EmptyJsonString_ReturnsMinValue()
    {
        DateTime result = InvokeRead("\"\""u8.ToArray());

        result.ShouldBe(DateTime.MinValue);
    }

    /// <summary>
    ///     Verifies that a direct converter Write invocation produces the Pingen date format
    /// </summary>
    [Test]
    public void Write_DirectInvocation_ProducesPingenFormat()
    {
        string output = InvokeWrite(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        output.ShouldStartWith("\"2024-01-01T00:00:00");
        output.ShouldEndWith("00:00\"");
    }

    private static DateTime InvokeRead(byte[] utf8Json)
    {
        var converter = new PingenDateTimeConverter();
        var opts = new JsonSerializerOptions();
        var reader = new Utf8JsonReader(utf8Json);
        reader.Read();
        return converter.Read(ref reader, typeof(DateTime), opts);
    }

    private static string InvokeWrite(DateTime value)
    {
        var converter = new PingenDateTimeConverter();
        var opts = new JsonSerializerOptions();
        using var ms = new MemoryStream();
        using (var writer = new Utf8JsonWriter(ms))
        {
            converter.Write(writer, value, opts);
        }

        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private sealed record DateTimeHolder
    {
        [JsonPropertyName("date")] public DateTime Date { get; init; }
    }

    private sealed record NullableDateTimeHolder
    {
        [JsonPropertyName("date")] public DateTime? Date { get; init; }
    }
}
