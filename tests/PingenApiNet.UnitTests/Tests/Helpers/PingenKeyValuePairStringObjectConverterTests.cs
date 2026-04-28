using System.Text.Json;
using System.Text.Json.Serialization;
using PingenApiNet.Abstractions.Helpers;

namespace PingenApiNet.UnitTests.Tests.Helpers;

/// <summary>
///     Unit tests for PingenKeyValuePairStringObjectConverter
/// </summary>
public class PingenKeyValuePairStringObjectConverterTests
{
    /// <summary>
    ///     Verifies that a KeyValuePair with a string value serializes as a JSON object property
    /// </summary>
    [Test]
    public void Write_StringValue_SerializesAsJsonObject()
    {
        var obj = new KvpHolder { Filter = new KeyValuePair<string, object>("name", "John") };

        string json = PingenSerialisationHelper.Serialize(obj);

        using var doc = JsonDocument.Parse(json);
        JsonElement filter = doc.RootElement.GetProperty("filter");
        filter.GetProperty("name").GetString().ShouldBe("John");
    }

    /// <summary>
    ///     Verifies that a KeyValuePair with a numeric value serializes the number correctly
    /// </summary>
    [Test]
    public void Write_NumericValue_SerializesNumber()
    {
        var obj = new KvpHolder { Filter = new KeyValuePair<string, object>("count", 42) };

        string json = PingenSerialisationHelper.Serialize(obj);

        using var doc = JsonDocument.Parse(json);
        JsonElement filter = doc.RootElement.GetProperty("filter");
        filter.GetProperty("count").GetInt32().ShouldBe(42);
    }

    /// <summary>
    ///     Verifies that a KeyValuePair with a boolean value serializes the boolean correctly
    /// </summary>
    [Test]
    public void Write_BooleanValue_SerializesBoolean()
    {
        var obj = new KvpHolder { Filter = new KeyValuePair<string, object>("active", true) };

        string json = PingenSerialisationHelper.Serialize(obj);

        using var doc = JsonDocument.Parse(json);
        JsonElement filter = doc.RootElement.GetProperty("filter");
        filter.GetProperty("active").GetBoolean().ShouldBeTrue();
    }

    /// <summary>
    ///     Verifies that a KeyValuePair with a nested dictionary value serializes as a nested object
    /// </summary>
    [Test]
    public void Write_NestedObject_SerializesNestedStructure()
    {
        var nested = new Dictionary<string, object> { ["city"] = "Zurich" };
        var obj = new KvpHolder { Filter = new KeyValuePair<string, object>("address", nested) };

        string json = PingenSerialisationHelper.Serialize(obj);

        using var doc = JsonDocument.Parse(json);
        JsonElement filter = doc.RootElement.GetProperty("filter");
        JsonElement address = filter.GetProperty("address");
        address.GetProperty("city").GetString().ShouldBe("Zurich");
    }

    /// <summary>
    ///     Verifies that a KeyValuePair with a null value serializes the key with a null JSON value
    /// </summary>
    [Test]
    public void Write_NullValue_SerializesKeyWithNullValue()
    {
        var obj = new KvpHolder { Filter = new KeyValuePair<string, object>("key", null!) };

        string json = PingenSerialisationHelper.Serialize(obj);

        using var doc = JsonDocument.Parse(json);
        JsonElement filter = doc.RootElement.GetProperty("filter");
        filter.GetProperty("key").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    /// <summary>
    ///     Verifies that a KeyValuePair with an empty string key serializes correctly
    /// </summary>
    [Test]
    public void Write_EmptyStringKey_SerializesWithEmptyKey()
    {
        var obj = new KvpHolder { Filter = new KeyValuePair<string, object>("", "value") };

        string json = PingenSerialisationHelper.Serialize(obj);

        using var doc = JsonDocument.Parse(json);
        JsonElement filter = doc.RootElement.GetProperty("filter");
        filter.GetProperty("").GetString().ShouldBe("value");
    }

    /// <summary>
    ///     Verifies that Read correctly deserializes a JSON string containing key-value data
    /// </summary>
    [Test]
    public void Read_ValidJsonString_ReturnsKeyValuePair()
    {
        string json = "{\"filter\":\"{\\\"name\\\":\\\"John\\\"}\"}";

        KvpHolder? result = PingenSerialisationHelper.Deserialize<KvpHolder>(json);

        result.ShouldNotBeNull();
        result!.Filter.Key.ShouldBe("name");
        JsonElement valueElement = result.Filter.Value.ShouldBeOfType<JsonElement>();
        valueElement.GetString().ShouldBe("John");
    }

    /// <summary>
    ///     Verifies that Read with an empty string returns the default KeyValuePair
    /// </summary>
    [Test]
    public void Read_EmptyString_ReturnsDefault()
    {
        string json = "{\"filter\":\"\"}";

        KvpHolder? result = PingenSerialisationHelper.Deserialize<KvpHolder>(json);

        result.ShouldNotBeNull();
        result!.Filter.Key.ShouldBeNull();
        result.Filter.Value.ShouldBeNull();
    }

    /// <summary>
    ///     Verifies that serializing a list of KeyValuePair filter expressions produces an array of single-key JSON objects
    /// </summary>
    [Test]
    public void Write_ListOfFilterExpressions_SerializesEachAsJsonObject()
    {
        var obj = new KvpListHolder
        {
            Filters =
            [
                new KeyValuePair<string, object>("name", "John"),
                new KeyValuePair<string, object>("status", "active"),
                new KeyValuePair<string, object>("count", 42)
            ]
        };

        string json = PingenSerialisationHelper.Serialize(obj);

        using var doc = JsonDocument.Parse(json);
        JsonElement filters = doc.RootElement.GetProperty("filters");
        filters.ValueKind.ShouldBe(JsonValueKind.Array);
        filters.GetArrayLength().ShouldBe(3);
        filters.ShouldSatisfyAllConditions(
            () => filters[0].GetProperty("name").GetString().ShouldBe("John"),
            () => filters[1].GetProperty("status").GetString().ShouldBe("active"),
            () => filters[2].GetProperty("count").GetInt32().ShouldBe(42)
        );
    }

    /// <summary>
    ///     Verifies that serializing an empty list of KeyValuePair filter expressions produces an empty JSON array
    /// </summary>
    [Test]
    public void Write_EmptyListOfFilterExpressions_SerializesAsEmptyArray()
    {
        var obj = new KvpListHolder { Filters = [] };

        string json = PingenSerialisationHelper.Serialize(obj);

        json.ShouldContain("\"filters\":[]");
    }

    /// <summary>
    ///     Verifies that a null list of KeyValuePair filter expressions causes the property to be omitted from JSON
    /// </summary>
    [Test]
    public void Write_NullListOfFilterExpressions_OmitsProperty()
    {
        var obj = new KvpListHolder { Filters = null };

        string json = PingenSerialisationHelper.Serialize(obj);

        json.ShouldNotContain("filters");
    }

    /// <summary>
    ///     Verifies that a complex KeyValuePair value with nested operator dictionaries serialises as a nested object
    /// </summary>
    [Test]
    public void Write_ComplexFilterExpression_SerializesNested()
    {
        var innerEq = new Dictionary<string, object> { ["city"] = "Zurich", ["postcode"] = "8000" };
        var inner = new Dictionary<string, object> { ["eq"] = innerEq };
        var obj = new KvpHolder { Filter = new KeyValuePair<string, object>("address", inner) };

        string json = PingenSerialisationHelper.Serialize(obj);

        using var doc = JsonDocument.Parse(json);
        JsonElement address = doc.RootElement.GetProperty("filter").GetProperty("address");
        JsonElement eq = address.GetProperty("eq");
        eq.ShouldSatisfyAllConditions(
            () => eq.GetProperty("city").GetString().ShouldBe("Zurich"),
            () => eq.GetProperty("postcode").GetString().ShouldBe("8000")
        );
    }

    /// <summary>
    ///     Verifies that a JSON null token deserializes to the default KeyValuePair
    /// </summary>
    [Test]
    public void Read_NullToken_ReturnsDefault()
    {
        string json = "{\"filter\":null}";

        KvpHolder? result = PingenSerialisationHelper.Deserialize<KvpHolder>(json);

        result.ShouldNotBeNull();
        result!.Filter.Key.ShouldBeNull();
        result.Filter.Value.ShouldBeNull();
    }

    /// <summary>
    ///     Verifies that a numeric value inside the quoted JSON string round-trips and is exposed as a JsonElement
    /// </summary>
    [Test]
    public void Read_NumericValueInsideJsonString_RoundTripsAsJsonElement()
    {
        string json = "{\"filter\":\"{\\\"count\\\":42}\"}";

        KvpHolder? result = PingenSerialisationHelper.Deserialize<KvpHolder>(json);

        result.ShouldNotBeNull();
        result!.Filter.Key.ShouldBe("count");
        JsonElement element = result.Filter.Value.ShouldBeOfType<JsonElement>();
        element.GetInt32().ShouldBe(42);
    }

    private sealed record KvpHolder
    {
        /// <summary>
        ///     Filter key-value pair for testing
        /// </summary>
        [JsonPropertyName("filter")]
        public KeyValuePair<string, object> Filter { get; init; }
    }

    private sealed record KvpListHolder
    {
        /// <summary>
        ///     Filter list for testing list-of-KeyValuePair serialization
        /// </summary>
        [JsonPropertyName("filters")]
        public IReadOnlyList<KeyValuePair<string, object>>? Filters { get; init; }
    }
}
