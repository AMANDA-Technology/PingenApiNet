using System.Text.Json;
using System.Text.Json.Serialization;
using PingenApiNet.Abstractions.Helpers;

namespace PingenApiNet.Tests.Tests.Unit.Helpers;

/// <summary>
/// Unit tests for PingenKeyValuePairStringObjectConverter
/// </summary>
public class PingenKeyValuePairStringObjectConverterTests
{
    /// <summary>
    /// Verifies that a KeyValuePair with a string value serializes as a JSON object property
    /// </summary>
    [Test]
    public void Write_StringValue_SerializesAsJsonObject()
    {
        var obj = new KvpHolder { Filter = new KeyValuePair<string, object>("name", "John") };

        var json = PingenSerialisationHelper.Serialize(obj);

        using var doc = JsonDocument.Parse(json);
        var filter = doc.RootElement.GetProperty("filter");
        filter.GetProperty("name").GetString().ShouldBe("John");
    }

    /// <summary>
    /// Verifies that a KeyValuePair with a numeric value serializes the number correctly
    /// </summary>
    [Test]
    public void Write_NumericValue_SerializesNumber()
    {
        var obj = new KvpHolder { Filter = new KeyValuePair<string, object>("count", 42) };

        var json = PingenSerialisationHelper.Serialize(obj);

        using var doc = JsonDocument.Parse(json);
        var filter = doc.RootElement.GetProperty("filter");
        filter.GetProperty("count").GetInt32().ShouldBe(42);
    }

    /// <summary>
    /// Verifies that a KeyValuePair with a boolean value serializes the boolean correctly
    /// </summary>
    [Test]
    public void Write_BooleanValue_SerializesBoolean()
    {
        var obj = new KvpHolder { Filter = new KeyValuePair<string, object>("active", true) };

        var json = PingenSerialisationHelper.Serialize(obj);

        using var doc = JsonDocument.Parse(json);
        var filter = doc.RootElement.GetProperty("filter");
        filter.GetProperty("active").GetBoolean().ShouldBeTrue();
    }

    /// <summary>
    /// Verifies that a KeyValuePair with a nested dictionary value serializes as a nested object
    /// </summary>
    [Test]
    public void Write_NestedObject_SerializesNestedStructure()
    {
        var nested = new Dictionary<string, object> { ["city"] = "Zurich" };
        var obj = new KvpHolder { Filter = new KeyValuePair<string, object>("address", nested) };

        var json = PingenSerialisationHelper.Serialize(obj);

        using var doc = JsonDocument.Parse(json);
        var filter = doc.RootElement.GetProperty("filter");
        var address = filter.GetProperty("address");
        address.GetProperty("city").GetString().ShouldBe("Zurich");
    }

    /// <summary>
    /// Verifies that a KeyValuePair with a null value serializes the key with a null JSON value
    /// </summary>
    [Test]
    public void Write_NullValue_SerializesKeyWithNullValue()
    {
        var obj = new KvpHolder { Filter = new KeyValuePair<string, object>("key", null!) };

        var json = PingenSerialisationHelper.Serialize(obj);

        using var doc = JsonDocument.Parse(json);
        var filter = doc.RootElement.GetProperty("filter");
        filter.GetProperty("key").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    /// <summary>
    /// Verifies that a KeyValuePair with an empty string key serializes correctly
    /// </summary>
    [Test]
    public void Write_EmptyStringKey_SerializesWithEmptyKey()
    {
        var obj = new KvpHolder { Filter = new KeyValuePair<string, object>("", "value") };

        var json = PingenSerialisationHelper.Serialize(obj);

        using var doc = JsonDocument.Parse(json);
        var filter = doc.RootElement.GetProperty("filter");
        filter.GetProperty("").GetString().ShouldBe("value");
    }

    /// <summary>
    /// Verifies that Read correctly deserializes a JSON string containing key-value data
    /// </summary>
    [Test]
    public void Read_ValidJsonString_ReturnsKeyValuePair()
    {
        var json = "{\"filter\":\"{\\\"name\\\":\\\"John\\\"}\"}"; 

        var result = PingenSerialisationHelper.Deserialize<KvpHolder>(json);

        result.ShouldNotBeNull();
        result!.Filter.Key.ShouldBe("name");
        var valueElement = result.Filter.Value.ShouldBeOfType<JsonElement>();
        valueElement.GetString().ShouldBe("John");
    }

    /// <summary>
    /// Verifies that Read with an empty string returns the default KeyValuePair
    /// </summary>
    [Test]
    public void Read_EmptyString_ReturnsDefault()
    {
        var json = "{\"filter\":\"\"}";

        var result = PingenSerialisationHelper.Deserialize<KvpHolder>(json);

        result.ShouldNotBeNull();
        result!.Filter.Key.ShouldBeNull();
        result.Filter.Value.ShouldBeNull();
    }

    private sealed record KvpHolder
    {
        /// <summary>
        /// Filter key-value pair for testing
        /// </summary>
        [JsonPropertyName("filter")]
        public KeyValuePair<string, object> Filter { get; init; }
    }
}
