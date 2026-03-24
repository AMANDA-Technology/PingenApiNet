using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Models.Base;

namespace PingenApiNet.Tests.Tests.Unit.Helpers;

/// <summary>
/// Unit tests for <see cref="PingenSerialisationHelper"/>
/// </summary>
public class PingenSerialisationHelperTests
{
    /// <summary>
    /// Verifies that Serialize produces valid JSON for a simple object
    /// </summary>
    [Test]
    public void Serialize_SimpleObject_ReturnsValidJson()
    {
        var obj = new { name = "test", value = 42 };

        var json = PingenSerialisationHelper.Serialize(obj);

        json.ShouldContain("\"name\":\"test\"");
        json.ShouldContain("\"value\":42");
    }

    /// <summary>
    /// Verifies that Deserialize can round-trip a serialized object
    /// </summary>
    [Test]
    public void Deserialize_ValidJson_ReturnsObject()
    {
        var json = "{\"id\":\"abc-123\",\"type\":\"letters\"}";

        var result = PingenSerialisationHelper.Deserialize<DataIdentity>(json);

        result.ShouldNotBeNull();
        result!.ShouldSatisfyAllConditions(
            () => result.Id.ShouldBe("abc-123"),
            () => result.Type.ShouldBe(PingenApiDataType.letters)
        );
    }

    /// <summary>
    /// Verifies that DeserializeAsync can parse a stream
    /// </summary>
    [Test]
    public async Task DeserializeAsync_ValidStream_ReturnsObject()
    {
        var json = "{\"id\":\"def-456\",\"type\":\"batches\"}";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

        var result = await PingenSerialisationHelper.DeserializeAsync<DataIdentity>(stream);

        result.ShouldNotBeNull();
        result!.ShouldSatisfyAllConditions(
            () => result.Id.ShouldBe("def-456"),
            () => result.Type.ShouldBe(PingenApiDataType.batches)
        );
    }

    /// <summary>
    /// Verifies that null properties are omitted during serialization
    /// </summary>
    [Test]
    public void Serialize_NullProperty_IsOmitted()
    {
        var obj = new { name = "test", extra = (string?)null };

        var json = PingenSerialisationHelper.Serialize(obj);

        json.ShouldNotContain("extra");
    }

    /// <summary>
    /// Verifies the PingenApiDataTypeMapping contains expected types
    /// </summary>
    [Test]
    public void PingenApiDataTypeMapping_ContainsExpectedTypes()
    {
        var mapping = PingenSerialisationHelper.PingenApiDataTypeMapping;

        mapping.ShouldSatisfyAllConditions(
            () => mapping.ContainsKey(PingenApiDataType.letters).ShouldBeTrue(),
            () => mapping.ContainsKey(PingenApiDataType.batches).ShouldBeTrue(),
            () => mapping.ContainsKey(PingenApiDataType.organisations).ShouldBeTrue(),
            () => mapping.ContainsKey(PingenApiDataType.webhooks).ShouldBeTrue(),
            () => mapping.ContainsKey(PingenApiDataType.users).ShouldBeTrue(),
            () => mapping.ContainsKey(PingenApiDataType.file_uploads).ShouldBeTrue(),
            () => mapping.ContainsKey(PingenApiDataType.delivery_products).ShouldBeTrue()
        );
    }

    /// <summary>
    /// Verifies that Deserialize throws for empty JSON when required properties are missing
    /// </summary>
    [Test]
    public void Deserialize_EmptyJsonObject_ThrowsForRequiredProperties()
    {
        var json = "{}";

        Should.Throw<System.Text.Json.JsonException>(() =>
            PingenSerialisationHelper.Deserialize<DataIdentity>(json));
    }
}
