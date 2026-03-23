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

        Assert.That(json, Does.Contain("\"name\":\"test\""));
        Assert.That(json, Does.Contain("\"value\":42"));
    }

    /// <summary>
    /// Verifies that Deserialize can round-trip a serialized object
    /// </summary>
    [Test]
    public void Deserialize_ValidJson_ReturnsObject()
    {
        var json = "{\"id\":\"abc-123\",\"type\":\"letters\"}";

        var result = PingenSerialisationHelper.Deserialize<DataIdentity>(json);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Id, Is.EqualTo("abc-123"));
            Assert.That(result.Type, Is.EqualTo(PingenApiDataType.letters));
        });
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

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Id, Is.EqualTo("def-456"));
            Assert.That(result.Type, Is.EqualTo(PingenApiDataType.batches));
        });
    }

    /// <summary>
    /// Verifies that null properties are omitted during serialization
    /// </summary>
    [Test]
    public void Serialize_NullProperty_IsOmitted()
    {
        var obj = new { name = "test", extra = (string?)null };

        var json = PingenSerialisationHelper.Serialize(obj);

        Assert.That(json, Does.Not.Contain("extra"));
    }

    /// <summary>
    /// Verifies the PingenApiDataTypeMapping contains expected types
    /// </summary>
    [Test]
    public void PingenApiDataTypeMapping_ContainsExpectedTypes()
    {
        var mapping = PingenSerialisationHelper.PingenApiDataTypeMapping;

        Assert.Multiple(() =>
        {
            Assert.That(mapping, Does.ContainKey(PingenApiDataType.letters));
            Assert.That(mapping, Does.ContainKey(PingenApiDataType.batches));
            Assert.That(mapping, Does.ContainKey(PingenApiDataType.organisations));
            Assert.That(mapping, Does.ContainKey(PingenApiDataType.webhooks));
            Assert.That(mapping, Does.ContainKey(PingenApiDataType.users));
            Assert.That(mapping, Does.ContainKey(PingenApiDataType.file_uploads));
            Assert.That(mapping, Does.ContainKey(PingenApiDataType.delivery_products));
        });
    }

    /// <summary>
    /// Verifies that Deserialize throws for empty JSON when required properties are missing
    /// </summary>
    [Test]
    public void Deserialize_EmptyJsonObject_ThrowsForRequiredProperties()
    {
        var json = "{}";

        Assert.Throws<System.Text.Json.JsonException>(() =>
            PingenSerialisationHelper.Deserialize<DataIdentity>(json));
    }
}
