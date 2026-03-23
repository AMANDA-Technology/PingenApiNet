using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Enums.Letters;
using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Letters.Views;

namespace PingenApiNet.Tests.Tests.Unit.Models;

/// <summary>
/// Unit tests for <see cref="DataPost{TAttributes}"/> and <see cref="DataPatch{TAttributes}"/>
/// </summary>
public class DataPostPatchTests
{
    /// <summary>
    /// Verifies DataPost stores type and attributes
    /// </summary>
    [Test]
    public void DataPost_StoresTypeAndAttributes()
    {
        var attributes = CreateLetterCreateAttributes();

        var dataPost = new DataPost<LetterCreate>
        {
            Type = PingenApiDataType.letters,
            Attributes = attributes
        };

        Assert.Multiple(() =>
        {
            dataPost.Type.ShouldBe(PingenApiDataType.letters);
            dataPost.Attributes.ShouldBeSameAs(attributes);
        });
    }

    /// <summary>
    /// Verifies DataPost serializes to expected JSON:API structure
    /// </summary>
    [Test]
    public void DataPost_SerializesToExpectedJson()
    {
        var dataPost = new DataPost<LetterCreate>
        {
            Type = PingenApiDataType.letters,
            Attributes = CreateLetterCreateAttributes()
        };

        var json = PingenSerialisationHelper.Serialize(new { data = dataPost });

        Assert.Multiple(() =>
        {
            json.ShouldContain("\"type\":\"letters\"");
            json.ShouldContain("\"attributes\"");
            json.ShouldContain("\"file_original_name\":\"test.pdf\"");
        });
    }

    /// <summary>
    /// Verifies DataPatch stores id, type and attributes
    /// </summary>
    [Test]
    public void DataPatch_StoresIdTypeAndAttributes()
    {
        var attributes = new LetterUpdate { PaperTypes = ["normal"] };

        var dataPatch = new DataPatch<LetterUpdate>
        {
            Id = "letter-123",
            Type = PingenApiDataType.letters,
            Attributes = attributes
        };

        Assert.Multiple(() =>
        {
            dataPatch.Id.ShouldBe("letter-123");
            dataPatch.Type.ShouldBe(PingenApiDataType.letters);
            dataPatch.Attributes.ShouldBeSameAs(attributes);
        });
    }

    /// <summary>
    /// Verifies DataPatch serializes to expected JSON:API structure with id
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

        var json = PingenSerialisationHelper.Serialize(new { data = dataPatch });

        Assert.Multiple(() =>
        {
            json.ShouldContain("\"id\":\"letter-456\"");
            json.ShouldContain("\"type\":\"letters\"");
            json.ShouldContain("\"attributes\"");
        });
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
