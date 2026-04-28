using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Enums.Letters;
using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Letters.Views;

namespace PingenApiNet.UnitTests.Tests.Models;

/// <summary>
///     Unit tests for <see cref="DataPost{TAttributes}" /> and <see cref="DataPatch{TAttributes}" />
/// </summary>
public class DataPostPatchTests
{
    /// <summary>
    ///     Verifies DataPost stores type and attributes
    /// </summary>
    [Test]
    public void DataPost_StoresTypeAndAttributes()
    {
        LetterCreate attributes = CreateLetterCreateAttributes();

        var dataPost = new DataPost<LetterCreate> { Type = PingenApiDataType.letters, Attributes = attributes };

        dataPost.ShouldSatisfyAllConditions(
            () => dataPost.Type.ShouldBe(PingenApiDataType.letters),
            () => dataPost.Attributes.ShouldBeSameAs(attributes)
        );
    }

    /// <summary>
    ///     Verifies DataPost serializes to expected JSON:API structure
    /// </summary>
    [Test]
    public void DataPost_SerializesToExpectedJson()
    {
        var dataPost = new DataPost<LetterCreate>
        {
            Type = PingenApiDataType.letters, Attributes = CreateLetterCreateAttributes()
        };

        string json = PingenSerialisationHelper.Serialize(new { data = dataPost });

        json.ShouldSatisfyAllConditions(
            () => json.ShouldContain("\"type\":\"letters\""),
            () => json.ShouldContain("\"attributes\""),
            () => json.ShouldContain("\"file_original_name\":\"test.pdf\"")
        );
    }

    /// <summary>
    ///     Verifies DataPatch stores id, type and attributes
    /// </summary>
    [Test]
    public void DataPatch_StoresIdTypeAndAttributes()
    {
        var attributes = new LetterUpdate { PaperTypes = ["normal"] };

        var dataPatch = new DataPatch<LetterUpdate>
        {
            Id = "letter-123", Type = PingenApiDataType.letters, Attributes = attributes
        };

        dataPatch.ShouldSatisfyAllConditions(
            () => dataPatch.Id.ShouldBe("letter-123"),
            () => dataPatch.Type.ShouldBe(PingenApiDataType.letters),
            () => dataPatch.Attributes.ShouldBeSameAs(attributes)
        );
    }

    /// <summary>
    ///     Verifies DataPatch serializes to expected JSON:API structure with id
    /// </summary>
    [Test]
    public void DataPatch_SerializesToJsonWithId()
    {
        var dataPatch = new DataPatch<LetterUpdate>
        {
            Id = "letter-456",
            Type = PingenApiDataType.letters,
            Attributes = new LetterUpdate { PaperTypes = ["normal"] }
        };

        string json = PingenSerialisationHelper.Serialize(new { data = dataPatch });

        json.ShouldSatisfyAllConditions(
            () => json.ShouldContain("\"id\":\"letter-456\""),
            () => json.ShouldContain("\"type\":\"letters\""),
            () => json.ShouldContain("\"attributes\"")
        );
    }

    /// <summary>
    ///     Verifies that DataPost serializes the Type enum as the snake_case JSON string value
    /// </summary>
    [Test]
    public void DataPost_SerializesType_AsSnakeCaseEnumValue()
    {
        var dataPost = new DataPost<LetterCreate>
        {
            Type = PingenApiDataType.letters, Attributes = CreateLetterCreateAttributes()
        };

        string json = PingenSerialisationHelper.Serialize(dataPost);

        json.ShouldContain("\"type\":\"letters\"");
    }

    /// <summary>
    ///     Verifies that the single-arity DataPost does not include a relationships block in its JSON
    /// </summary>
    [Test]
    public void DataPost_OmitsRelationships_WhenUsingNonRelationshipsGenericArity()
    {
        var dataPost = new DataPost<LetterCreate>
        {
            Type = PingenApiDataType.letters, Attributes = CreateLetterCreateAttributes()
        };

        string json = PingenSerialisationHelper.Serialize(dataPost);

        json.ShouldNotContain("relationships");
    }

    /// <summary>
    ///     Verifies that DataPost with relationships serializes the relationships block correctly
    /// </summary>
    [Test]
    public void DataPostWithRelationships_SerializesRelationshipsBlock()
    {
        var dataPost = new DataPost<LetterCreate, LetterCreateRelationships>
        {
            Type = PingenApiDataType.letters,
            Attributes = CreateLetterCreateAttributes(),
            Relationships = LetterCreateRelationships.Create("preset-1")
        };

        string json = PingenSerialisationHelper.Serialize(dataPost);

        json.ShouldSatisfyAllConditions(
            () => json.ShouldContain("\"relationships\""),
            () => json.ShouldContain("\"preset\""),
            () => json.ShouldContain("\"id\":\"preset-1\""),
            () => json.ShouldContain("\"type\":\"presets\"")
        );
    }

    /// <summary>
    ///     Verifies that null Relationships are omitted from JSON when WhenWritingNull policy is in effect
    /// </summary>
    [Test]
    public void DataPostWithRelationships_NullRelationships_OmittedFromJson()
    {
        var dataPost = new DataPost<LetterCreate, LetterCreateRelationships>
        {
            Type = PingenApiDataType.letters, Attributes = CreateLetterCreateAttributes(), Relationships = null
        };

        string json = PingenSerialisationHelper.Serialize(dataPost);

        json.ShouldNotContain("\"relationships\"");
    }

    /// <summary>
    ///     Verifies that null optional attributes (for example MetaData on LetterCreate) are omitted from JSON
    /// </summary>
    [Test]
    public void DataPost_NullOptionalAttribute_OmittedFromJson()
    {
        LetterCreate attributes = CreateLetterCreateAttributes() with { MetaData = null };

        var dataPost = new DataPost<LetterCreate> { Type = PingenApiDataType.letters, Attributes = attributes };

        string json = PingenSerialisationHelper.Serialize(dataPost);

        json.ShouldNotContain("\"meta_data\"");
    }

    /// <summary>
    ///     Verifies that DataPatch is assignable to DataPost (inheritance relationship)
    /// </summary>
    [Test]
    public void DataPatch_InheritsFromDataPost_TypeAndAttributes()
    {
        var dataPatch = new DataPatch<LetterUpdate>
        {
            Id = "letter-789",
            Type = PingenApiDataType.letters,
            Attributes = new LetterUpdate { PaperTypes = ["normal"] }
        };

        dataPatch.ShouldBeAssignableTo<DataPost<LetterUpdate>>();
    }

    /// <summary>
    ///     Verifies that DataPatch serializes id, type, and attributes but not relationships
    /// </summary>
    [Test]
    public void DataPatch_SerializesIdTypeAttributes_NotRelationships()
    {
        var dataPatch = new DataPatch<LetterUpdate>
        {
            Id = "letter-x",
            Type = PingenApiDataType.letters,
            Attributes = new LetterUpdate { PaperTypes = ["normal"] }
        };

        string json = PingenSerialisationHelper.Serialize(dataPatch);

        json.ShouldSatisfyAllConditions(
            () => json.ShouldContain("\"id\":\"letter-x\""),
            () => json.ShouldContain("\"type\":\"letters\""),
            () => json.ShouldContain("\"attributes\""),
            () => json.ShouldNotContain("\"relationships\"")
        );
    }

    /// <summary>
    ///     Verifies that wrapping a DataPost in an anonymous { data = ... } envelope produces the expected JSON:API shape
    /// </summary>
    [Test]
    public void DataPost_SerializeThenWrapInDataEnvelope_ProducesJsonApiShape()
    {
        var dataPost = new DataPost<LetterCreate>
        {
            Type = PingenApiDataType.letters, Attributes = CreateLetterCreateAttributes()
        };

        string json = PingenSerialisationHelper.Serialize(new { data = dataPost });

        json.ShouldSatisfyAllConditions(
            () => json.ShouldStartWith("{\"data\":{"),
            () => json.ShouldContain("\"type\":\"letters\""),
            () => json.ShouldContain("\"attributes\""),
            () => json.ShouldContain("\"file_original_name\":\"test.pdf\"")
        );
    }

    /// <summary>
    ///     Verifies that two DataPost instances built from the same attributes serialize to identical JSON
    /// </summary>
    [Test]
    public void DataPost_ReferenceEquality_AttributesShareInstance()
    {
        LetterCreate attributes = CreateLetterCreateAttributes();

        var p1 = new DataPost<LetterCreate> { Type = PingenApiDataType.letters, Attributes = attributes };

        var p2 = new DataPost<LetterCreate> { Type = PingenApiDataType.letters, Attributes = attributes };

        PingenSerialisationHelper.Serialize(p1).ShouldBe(PingenSerialisationHelper.Serialize(p2));
    }

    private static LetterCreate CreateLetterCreateAttributes() => new()
    {
        FileOriginalName = "test.pdf",
        FileUrl = "https://example.com/test.pdf",
        FileUrlSignature = "sig123",
        AddressPosition = LetterAddressPosition.left,
        AutoSend = false,
        DeliveryProduct = "cheap",
        PrintMode = LetterPrintMode.simplex,
        PrintSpectrum = LetterPrintSpectrum.grayscale
    };
}
