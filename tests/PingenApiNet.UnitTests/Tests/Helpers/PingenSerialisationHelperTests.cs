using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Base;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Organisations;
using PingenApiNet.Abstractions.Models.Webhooks.WebhookEvents;

namespace PingenApiNet.UnitTests.Tests.Helpers;

/// <summary>
///     Unit tests for <see cref="PingenSerialisationHelper" />
/// </summary>
public class PingenSerialisationHelperTests
{
    /// <summary>
    ///     Verifies that Serialize produces valid JSON for a simple object
    /// </summary>
    [Test]
    public void Serialize_SimpleObject_ReturnsValidJson()
    {
        var obj = new { name = "test", value = 42 };

        string json = PingenSerialisationHelper.Serialize(obj);

        json.ShouldContain("\"name\":\"test\"");
        json.ShouldContain("\"value\":42");
    }

    /// <summary>
    ///     Verifies that Deserialize can round-trip a serialized object
    /// </summary>
    [Test]
    public void Deserialize_ValidJson_ReturnsObject()
    {
        string json = "{\"id\":\"abc-123\",\"type\":\"letters\"}";

        DataIdentity? result = PingenSerialisationHelper.Deserialize<DataIdentity>(json);

        result.ShouldNotBeNull();
        result!.ShouldSatisfyAllConditions(
            () => result.Id.ShouldBe("abc-123"),
            () => result.Type.ShouldBe(PingenApiDataType.letters)
        );
    }

    /// <summary>
    ///     Verifies that DeserializeAsync can parse a stream
    /// </summary>
    [Test]
    public async Task DeserializeAsync_ValidStream_ReturnsObject()
    {
        string json = "{\"id\":\"def-456\",\"type\":\"batches\"}";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        DataIdentity? result = await PingenSerialisationHelper.DeserializeAsync<DataIdentity>(stream);

        result.ShouldNotBeNull();
        result!.ShouldSatisfyAllConditions(
            () => result.Id.ShouldBe("def-456"),
            () => result.Type.ShouldBe(PingenApiDataType.batches)
        );
    }

    /// <summary>
    ///     Verifies that null properties are omitted during serialization
    /// </summary>
    [Test]
    public void Serialize_NullProperty_IsOmitted()
    {
        var obj = new { name = "test", extra = (string?)null };

        string json = PingenSerialisationHelper.Serialize(obj);

        json.ShouldNotContain("extra");
    }

    /// <summary>
    ///     Verifies the PingenApiDataTypeMapping contains expected types
    /// </summary>
    [Test]
    public void PingenApiDataTypeMapping_ContainsExpectedTypes()
    {
        Dictionary<PingenApiDataType, Type> mapping = PingenSerialisationHelper.PingenApiDataTypeMapping;

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
    ///     Verifies that TryGetIncludedData returns true and deserializes the matching included element
    /// </summary>
    [Test]
    public void TryGetIncludedData_MatchingType_ReturnsTrueWithData()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": [
                              { "id": "org-1", "type": "organisations", "attributes": { "name": "Test Org" } },
                              { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } }
                          ]
                      }
                      """;
        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        bool found =
            PingenSerialisationHelper.TryGetIncludedData<Organisation>(result, out Data<Organisation>? included);

        found.ShouldBeTrue();
        included.ShouldNotBeNull();
        included!.Id.ShouldBe("org-1");
        included.Type.ShouldBe(PingenApiDataType.organisations);
    }

    /// <summary>
    ///     Verifies that TryGetIncludedData returns false when no element matches the requested type
    /// </summary>
    [Test]
    public void TryGetIncludedData_NoMatchingType_ReturnsFalse()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": [
                              { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } }
                          ]
                      }
                      """;
        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        bool found =
            PingenSerialisationHelper.TryGetIncludedData<Organisation>(result, out Data<Organisation>? included);

        found.ShouldBeFalse();
        included.ShouldBeNull();
    }

    /// <summary>
    ///     Verifies that TryGetIncludedData returns false when included is null
    /// </summary>
    [Test]
    public void TryGetIncludedData_NullIncluded_ReturnsFalse()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } }
                      }
                      """;
        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        bool found =
            PingenSerialisationHelper.TryGetIncludedData<Organisation>(result, out Data<Organisation>? included);

        found.ShouldBeFalse();
        included.ShouldBeNull();
    }

    /// <summary>
    ///     Verifies that TryGetIncludedData gracefully skips elements with unrecognized type discriminators
    /// </summary>
    [Test]
    public void TryGetIncludedData_UnknownTypeDiscriminator_SkipsElement()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": [
                              { "id": "unknown-1", "type": "unknown_type_xyz", "attributes": {} },
                              { "id": "org-1", "type": "organisations", "attributes": { "name": "Test Org" } }
                          ]
                      }
                      """;
        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        bool found =
            PingenSerialisationHelper.TryGetIncludedData<Organisation>(result, out Data<Organisation>? included);

        found.ShouldBeTrue();
        included.ShouldNotBeNull();
        included!.Id.ShouldBe("org-1");
    }

    /// <summary>
    ///     Verifies that Deserialize throws for empty JSON when required properties are missing
    /// </summary>
    [Test]
    public void Deserialize_EmptyJsonObject_ThrowsForRequiredProperties()
    {
        string json = "{}";

        Should.Throw<JsonException>(() =>
            PingenSerialisationHelper.Deserialize<DataIdentity>(json));
    }

    /// <summary>
    ///     Verifies that SerializerOptions returns the same cached instance on every call
    /// </summary>
    [Test]
    public void SerializerOptions_ReturnsSameCachedInstance()
    {
        MethodInfo method = typeof(PingenSerialisationHelper)
            .GetMethod("SerializerOptions", BindingFlags.NonPublic | BindingFlags.Static)!;

        object? first = method.Invoke(null, null);
        object? second = method.Invoke(null, null);

        ReferenceEquals(first, second).ShouldBeTrue();
    }

    /// <summary>
    ///     Verifies that missing optional properties deserialize to default values without throwing
    /// </summary>
    [Test]
    public void Deserialize_MissingOptionalProperties_FillsWithNull()
    {
        string json = "{\"id\":\"x\"}";

        PartialModel? result = PingenSerialisationHelper.Deserialize<PartialModel>(json);

        result.ShouldNotBeNull();
        result!.ShouldSatisfyAllConditions(
            () => result.Id.ShouldBe("x"),
            () => result.Name.ShouldBeNull()
        );
    }

    /// <summary>
    ///     Verifies that extra unknown properties are silently ignored during deserialization
    /// </summary>
    [Test]
    public void Deserialize_ExtraProperties_AreIgnored()
    {
        string json = "{\"id\":\"abc-123\",\"type\":\"letters\",\"unknown_field\":\"skipped\",\"another\":42}";

        DataIdentity? result = PingenSerialisationHelper.Deserialize<DataIdentity>(json);

        result.ShouldNotBeNull();
        result!.ShouldSatisfyAllConditions(
            () => result.Id.ShouldBe("abc-123"),
            () => result.Type.ShouldBe(PingenApiDataType.letters)
        );
    }

    /// <summary>
    ///     Verifies that a nested record with a custom-converter property round-trips with shape preserved
    /// </summary>
    [Test]
    public void Deserialize_NestedObject_PreservesShape()
    {
        var inner = new InnerHolder { Date = new DateTime(2024, 5, 6, 7, 8, 9, DateTimeKind.Utc) };
        var outer = new OuterHolder { Inner = inner };

        string json = PingenSerialisationHelper.Serialize(outer);
        OuterHolder? result = PingenSerialisationHelper.Deserialize<OuterHolder>(json);

        result.ShouldNotBeNull();
        result!.Inner.ShouldNotBeNull();
        result.Inner!.ShouldSatisfyAllConditions(
            () => result.Inner.Date.Year.ShouldBe(2024),
            () => result.Inner.Date.Month.ShouldBe(5),
            () => result.Inner.Date.Day.ShouldBe(6)
        );
    }

    /// <summary>
    ///     Verifies that a record holding both DateTime converter types round-trips through serialization
    ///     and that the asymmetric KeyValuePair converter still emits a JSON object on the write path
    /// </summary>
    [Test]
    public void Serialize_RecordWithCustomConverters_RoundTripsAllThree()
    {
        var holder = new TripleConverterHolder
        {
            Date = new DateTime(2024, 8, 1, 9, 30, 0, DateTimeKind.Utc),
            OptionalDate = new DateTime(2024, 8, 2, 10, 0, 0, DateTimeKind.Utc),
            Filter = new KeyValuePair<string, object>("name", "Jane")
        };

        string json = PingenSerialisationHelper.Serialize(holder);

        json.ShouldSatisfyAllConditions(
            () => json.ShouldContain("2024-08-01T09:30:00"),
            () => json.ShouldContain("2024-08-02T10:00:00"),
            () => json.ShouldContain("\"name\":\"Jane\"")
        );
    }

    /// <summary>
    ///     Verifies that DeserializeAsync throws JsonException for an empty stream
    /// </summary>
    [Test]
    public async Task DeserializeAsync_EmptyStream_Throws()
    {
        using var stream = new MemoryStream();

        await Should.ThrowAsync<JsonException>(async () =>
            await PingenSerialisationHelper.DeserializeAsync<DataIdentity>(stream));
    }

    /// <summary>
    ///     Verifies that PingenApiDataTypeMapping has the expected number of entries
    /// </summary>
    [Test]
    public void PingenApiDataTypeMapping_HasExpectedCount()
    {
        Dictionary<PingenApiDataType, Type> mapping = PingenSerialisationHelper.PingenApiDataTypeMapping;

        mapping.Count.ShouldBe(13);
    }

    /// <summary>
    ///     Verifies that all webhook categories map to the WebhookEvent attributes type
    /// </summary>
    [Test]
    public void PingenApiDataTypeMapping_AllWebhookCategoriesMapToWebhookEvent()
    {
        Dictionary<PingenApiDataType, Type> mapping = PingenSerialisationHelper.PingenApiDataTypeMapping;

        mapping.ShouldSatisfyAllConditions(
            () => mapping[PingenApiDataType.webhook_issues].ShouldBe(typeof(WebhookEvent)),
            () => mapping[PingenApiDataType.webhook_sent].ShouldBe(typeof(WebhookEvent)),
            () => mapping[PingenApiDataType.webhook_undeliverable].ShouldBe(typeof(WebhookEvent))
        );
    }

    /// <summary>
    ///     Verifies that the presets enum value is deliberately not mapped to a CLR type
    /// </summary>
    [Test]
    public void PingenApiDataTypeMapping_PresetsNotMapped()
    {
        Dictionary<PingenApiDataType, Type> mapping = PingenSerialisationHelper.PingenApiDataTypeMapping;

        mapping.ContainsKey(PingenApiDataType.presets).ShouldBeFalse();
    }

    /// <summary>
    ///     Verifies that TryGetIncludedData throws when multiple included resources match the requested type
    /// </summary>
    [Test]
    public void TryGetIncludedData_MultipleMatches_Throws()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": [
                              { "id": "org-1", "type": "organisations", "attributes": { "name": "First Org" } },
                              { "id": "org-2", "type": "organisations", "attributes": { "name": "Second Org" } }
                          ]
                      }
                      """;
        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        Should.Throw<InvalidOperationException>(() =>
            PingenSerialisationHelper.TryGetIncludedData<Organisation>(result, out _));
    }

    private sealed record PartialModel
    {
        /// <summary>
        ///     Required identifier for partial-model deserialization tests
        /// </summary>
        [JsonPropertyName("id")]
        public required string Id { get; init; }

        /// <summary>
        ///     Optional name field that must remain null when missing in JSON
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }

    private sealed record InnerHolder
    {
        /// <summary>
        ///     Inner date field exercising the DateTime converter inside a nested record
        /// </summary>
        [JsonPropertyName("date")]
        public DateTime Date { get; init; }
    }

    private sealed record OuterHolder
    {
        /// <summary>
        ///     Nested inner record for round-trip testing
        /// </summary>
        [JsonPropertyName("inner")]
        public InnerHolder? Inner { get; init; }
    }

    private sealed record TripleConverterHolder
    {
        /// <summary>
        ///     Required DateTime field
        /// </summary>
        [JsonPropertyName("date")]
        public DateTime Date { get; init; }

        /// <summary>
        ///     Optional DateTime field
        /// </summary>
        [JsonPropertyName("optional_date")]
        public DateTime? OptionalDate { get; init; }

        /// <summary>
        ///     Filter KeyValuePair field
        /// </summary>
        [JsonPropertyName("filter")]
        public KeyValuePair<string, object> Filter { get; init; }
    }
}
