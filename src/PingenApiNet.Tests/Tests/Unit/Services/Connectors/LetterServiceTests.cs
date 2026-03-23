using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults.Embedded;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Letters.Views;
using PingenApiNet.Services.Connectors;

namespace PingenApiNet.Tests.Tests.Unit.Services.Connectors;

/// <summary>
/// Unit tests for <see cref="LetterService"/>
/// </summary>
public class LetterServiceTests
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
    /// Verifies GetPage calls ConnectionHandler.GetAsync with correct endpoint
    /// </summary>
    [Test]
    public async Task GetPage_CallsConnectionHandlerWithCorrectPath()
    {
        _mockConnectionHandler
            .Setup(x => x.GetAsync<CollectionResult<LetterData>>(
                "letters",
                It.IsAny<ApiPagingRequest?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult<CollectionResult<LetterData>> { IsSuccess = true });

        await _letterService.GetPage();

        _mockConnectionHandler.Verify(x => x.GetAsync<CollectionResult<LetterData>>(
            "letters",
            It.IsAny<ApiPagingRequest?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies Get calls ConnectionHandler.GetAsync with correct path including letter ID
    /// </summary>
    [Test]
    public async Task Get_CallsConnectionHandlerWithCorrectPath()
    {
        const string letterId = "test-letter-id";

        _mockConnectionHandler
            .Setup(x => x.GetAsync<SingleResult<LetterDataDetailed>>(
                $"letters/{letterId}",
                It.IsAny<ApiPagingRequest?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult<SingleResult<LetterDataDetailed>> { IsSuccess = true });

        await _letterService.Get(letterId);

        _mockConnectionHandler.Verify(x => x.GetAsync<SingleResult<LetterDataDetailed>>(
            $"letters/{letterId}",
            It.IsAny<ApiPagingRequest?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies Delete calls ConnectionHandler.DeleteAsync with correct path
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerWithCorrectPath()
    {
        const string letterId = "delete-letter-id";

        _mockConnectionHandler
            .Setup(x => x.DeleteAsync(
                $"letters/{letterId}",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult { IsSuccess = true });

        await _letterService.Delete(letterId);

        _mockConnectionHandler.Verify(x => x.DeleteAsync(
            $"letters/{letterId}",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies Cancel calls ConnectionHandler.PatchAsync with correct cancel path
    /// </summary>
    [Test]
    public async Task Cancel_CallsConnectionHandlerWithCorrectPath()
    {
        const string letterId = "cancel-letter-id";

        _mockConnectionHandler
            .Setup(x => x.PatchAsync(
                $"letters/{letterId}/cancel",
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult { IsSuccess = true });

        await _letterService.Cancel(letterId);

        _mockConnectionHandler.Verify(x => x.PatchAsync(
            $"letters/{letterId}/cancel",
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies GetFileLocation calls ConnectionHandler.GetAsync with correct file path
    /// </summary>
    [Test]
    public async Task GetFileLocation_CallsConnectionHandlerWithCorrectPath()
    {
        const string letterId = "file-letter-id";

        _mockConnectionHandler
            .Setup(x => x.GetAsync(
                $"letters/{letterId}/file",
                It.IsAny<ApiPagingRequest?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult { IsSuccess = true });

        await _letterService.GetFileLocation(letterId);

        _mockConnectionHandler.Verify(x => x.GetAsync(
            $"letters/{letterId}/file",
            It.IsAny<ApiPagingRequest?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies Create calls ConnectionHandler.PostAsync with correct path
    /// </summary>
    [Test]
    public async Task Create_CallsConnectionHandlerWithCorrectPath()
    {
        var data = new DataPost<LetterCreate, LetterCreateRelationships>
        {
            Type = PingenApiDataType.letters,
            Attributes = new LetterCreate
            {
                FileOriginalName = "test.pdf",
                FileUrl = "https://example.com/test.pdf",
                FileUrlSignature = "sig",
                AddressPosition = Abstractions.Enums.Letters.LetterAddressPosition.left,
                AutoSend = false,
                DeliveryProduct = "cheap",
                PrintMode = Abstractions.Enums.Letters.LetterPrintMode.simplex,
                PrintSpectrum = Abstractions.Enums.Letters.LetterPrintSpectrum.grayscale
            }
        };

        _mockConnectionHandler
            .Setup(x => x.PostAsync<SingleResult<LetterDataDetailed>, DataPost<LetterCreate, LetterCreateRelationships>>(
                "letters",
                It.IsAny<DataPost<LetterCreate, LetterCreateRelationships>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult<SingleResult<LetterDataDetailed>> { IsSuccess = true });

        await _letterService.Create(data);

        _mockConnectionHandler.Verify(x => x.PostAsync<SingleResult<LetterDataDetailed>, DataPost<LetterCreate, LetterCreateRelationships>>(
            "letters",
            It.IsAny<DataPost<LetterCreate, LetterCreateRelationships>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies DownloadFileContent throws PingenFileDownloadException on failed download
    /// </summary>
    [Test]
    public void DownloadFileContent_FailedRequest_ThrowsPingenFileDownloadException()
    {
        var fileUrl = new Uri("https://example.com/file.pdf");

        _mockConnectionHandler
            .Setup(x => x.SendExternalRequestAsync(
                It.IsAny<HttpRequestMessage>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
            {
                Content = new StringContent("<Error><Code>AccessDenied</Code><Message>Access Denied</Message></Error>")
            });

        Assert.ThrowsAsync<Abstractions.Exceptions.PingenFileDownloadException>(
            async () => await _letterService.DownloadFileContent(fileUrl));
    }

    /// <summary>
    /// Verifies DownloadFileContent returns stream on success
    /// </summary>
    [Test]
    public async Task DownloadFileContent_Success_ReturnsStream()
    {
        var fileUrl = new Uri("https://example.com/file.pdf");
        var content = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF magic bytes

        _mockConnectionHandler
            .Setup(x => x.SendExternalRequestAsync(
                It.IsAny<HttpRequestMessage>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(content)
            });

        var result = await _letterService.DownloadFileContent(fileUrl);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
    }
}
