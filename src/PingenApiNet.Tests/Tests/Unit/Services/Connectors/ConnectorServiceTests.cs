using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Enums.Letters;
using PingenApiNet.Abstractions.Exceptions;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.Relations;
using PingenApiNet.Abstractions.Models.Api.Embedded.Relations.Embedded;
using PingenApiNet.Abstractions.Models.Base.Embedded;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Services.Connectors;

namespace PingenApiNet.Tests.Tests.Unit.Services.Connectors;

/// <summary>
/// Unit tests for <see cref="PingenApiNet.Services.Connectors.Base.ConnectorService"/> via LetterService
/// </summary>
public class ConnectorServiceTests
{
    private Mock<IPingenConnectionHandler> _mockConnectionHandler = null!;
    private LetterService _letterService = null!;

    /// <summary>
    /// Set up mocks before each test
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockConnectionHandler = new Mock<IPingenConnectionHandler>();
        _letterService = new LetterService(_mockConnectionHandler.Object);
    }

    /// <summary>
    /// Verifies HandleResult returns data on success for collection result
    /// </summary>
    [Test]
    public void HandleResult_CollectionResult_Success_ReturnsData()
    {
        var letterData = CreateLetterData("letter-1");
        var collectionResult = new CollectionResult<LetterData>(
            [letterData],
            new CollectionResultLinks("", "", "", "", ""),
            new CollectionResultMeta(1, 1, 20, 1, 1, 1)
        );

        var apiResult = new ApiResult<CollectionResult<LetterData>>
        {
            IsSuccess = true,
            Data = collectionResult
        };

        var result = _letterService.HandleResult(apiResult);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo("letter-1"));
    }

    /// <summary>
    /// Verifies HandleResult throws PingenApiErrorException on failure for collection result
    /// </summary>
    [Test]
    public void HandleResult_CollectionResult_Failure_ThrowsPingenApiErrorException()
    {
        var apiResult = new ApiResult<CollectionResult<LetterData>>
        {
            IsSuccess = false
        };

        var ex = Assert.Throws<PingenApiErrorException>(() => _letterService.HandleResult(apiResult));
        Assert.That(ex!.ApiResult, Is.Not.Null);
    }

    /// <summary>
    /// Verifies HandleResult returns data on success for single result
    /// </summary>
    [Test]
    public void HandleResult_SingleResult_Success_ReturnsData()
    {
        var letterData = CreateLetterDataDetailed("letter-2");
        var singleResult = new SingleResult<LetterDataDetailed>(letterData);

        var apiResult = new ApiResult<SingleResult<LetterDataDetailed>>
        {
            IsSuccess = true,
            Data = singleResult
        };

        var result = _letterService.HandleResult(apiResult);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo("letter-2"));
    }

    /// <summary>
    /// Verifies HandleResult throws PingenApiErrorException on failure for single result
    /// </summary>
    [Test]
    public void HandleResult_SingleResult_Failure_ThrowsPingenApiErrorException()
    {
        var apiResult = new ApiResult<SingleResult<LetterDataDetailed>>
        {
            IsSuccess = false
        };

        Assert.Throws<PingenApiErrorException>(() => _letterService.HandleResult(apiResult));
    }

    /// <summary>
    /// Verifies HandleResult returns empty list when Data is null on success
    /// </summary>
    [Test]
    public void HandleResult_CollectionResult_NullData_ReturnsEmptyList()
    {
        var apiResult = new ApiResult<CollectionResult<LetterData>>
        {
            IsSuccess = true,
            Data = null
        };

        var result = _letterService.HandleResult(apiResult);

        Assert.That(result, Is.Empty);
    }

    /// <summary>
    /// Verifies HandleResult returns default when single result Data is null on success
    /// </summary>
    [Test]
    public void HandleResult_SingleResult_NullData_ReturnsDefault()
    {
        var apiResult = new ApiResult<SingleResult<LetterDataDetailed>>
        {
            IsSuccess = true,
            Data = null
        };

        var result = _letterService.HandleResult(apiResult);

        Assert.That(result, Is.Null);
    }

    private static LetterData CreateLetterData(string id) => new()
    {
        Id = id,
        Type = PingenApiDataType.letters,
        Attributes = CreateLetterAttributes(),
        Relationships = CreateLetterRelationships()
    };

    private static LetterDataDetailed CreateLetterDataDetailed(string id) => new()
    {
        Id = id,
        Type = PingenApiDataType.letters,
        Attributes = CreateLetterAttributes(),
        Relationships = CreateLetterRelationships(),
        Meta = new(new(CreateLetterAbilities()))
    };

    private static Letter CreateLetterAttributes() => new(
        Status: "draft",
        FileOriginalName: "test.pdf",
        FilePages: 1,
        Address: "Test Address",
        AddressPosition: LetterAddressPosition.left,
        Country: "CH",
        DeliveryProduct: "cheap",
        PrintMode: LetterPrintMode.simplex,
        PrintSpectrum: LetterPrintSpectrum.grayscale,
        PriceCurrency: "",
        PriceValue: null,
        PaperTypes: [],
        Fonts: [],
        TrackingNumber: "",
        SubmittedAt: null,
        CreatedAt: DateTime.UtcNow,
        UpdatedAt: DateTime.UtcNow
    );

    private static LetterRelationships CreateLetterRelationships() => new(
        Organisation: new(new(""), null!),
        Events: new(new(new("", new(0)))),
        Batch: new(new(""), null!)
    );

    private static LetterAbilities CreateLetterAbilities() => new(
        Cancel: PingenApiAbility.ok,
        Delete: PingenApiAbility.ok,
        Submit: PingenApiAbility.ok,
        SendSimplex: PingenApiAbility.ok,
        Edit: PingenApiAbility.ok,
        GetPdfRaw: PingenApiAbility.ok,
        GetPdfValidation: PingenApiAbility.ok,
        ChangePaperType: PingenApiAbility.ok,
        ChangeWindowPosition: PingenApiAbility.ok,
        CreateCoverPage: PingenApiAbility.ok,
        FixOverwriteRestrictedAreas: PingenApiAbility.ok,
        FixCoverPage: PingenApiAbility.ok,
        FixRegularPaper: PingenApiAbility.ok
    );
}
