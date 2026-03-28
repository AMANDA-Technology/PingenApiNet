using System.Reflection;
using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Base;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Organisations;

namespace PingenApiNet.UnitTests.Tests.Helpers;

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
    /// Verifies that TryGetIncludedData returns true and deserializes the matching included element
    /// </summary>
    [Test]
    public void TryGetIncludedData_MatchingType_ReturnsTrueWithData()
    {
        var json = """
        {
            "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
            "included": [
                { "id": "org-1", "type": "organisations", "attributes": { "name": "Test Org" } },
                { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } }
            ]
        }
        """;
        var result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        var found = PingenSerialisationHelper.TryGetIncludedData<Organisation>(result, out var included);

        found.ShouldBeTrue();
        included.ShouldNotBeNull();
        included!.Id.ShouldBe("org-1");
        included.Type.ShouldBe(PingenApiDataType.organisations);
    }

    /// <summary>
    /// Verifies that TryGetIncludedData returns false when no element matches the requested type
    /// </summary>
    [Test]
    public void TryGetIncludedData_NoMatchingType_ReturnsFalse()
    {
        var json = """
        {
            "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
            "included": [
                { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } }
            ]
        }
        """;
        var result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        var found = PingenSerialisationHelper.TryGetIncludedData<Organisation>(result, out var included);

        found.ShouldBeFalse();
        included.ShouldBeNull();
    }

    /// <summary>
    /// Verifies that TryGetIncludedData returns false when included is null
    /// </summary>
    [Test]
    public void TryGetIncludedData_NullIncluded_ReturnsFalse()
    {
        var json = """
        {
            "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } }
        }
        """;
        var result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        var found = PingenSerialisationHelper.TryGetIncludedData<Organisation>(result, out var included);

        found.ShouldBeFalse();
        included.ShouldBeNull();
    }

    /// <summary>
    /// Verifies that TryGetIncludedData gracefully skips elements with unrecognized type discriminators
    /// </summary>
    [Test]
    public void TryGetIncludedData_UnknownTypeDiscriminator_SkipsElement()
    {
        var json = """
        {
            "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
            "included": [
                { "id": "unknown-1", "type": "unknown_type_xyz", "attributes": {} },
                { "id": "org-1", "type": "organisations", "attributes": { "name": "Test Org" } }
            ]
        }
        """;
        var result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        var found = PingenSerialisationHelper.TryGetIncludedData<Organisation>(result, out var included);

        found.ShouldBeTrue();
        included.ShouldNotBeNull();
        included!.Id.ShouldBe("org-1");
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

    /// <summary>
    /// Verifies that SerializerOptions returns the same cached instance on every call
    /// </summary>
    [Test]
    public void SerializerOptions_ReturnsSameCachedInstance()
    {
        var method = typeof(PingenSerialisationHelper)
            .GetMethod("SerializerOptions", BindingFlags.NonPublic | BindingFlags.Static)!;

        var first = method.Invoke(null, null);
        var second = method.Invoke(null, null);

        ReferenceEquals(first, second).ShouldBeTrue();
    }
}
