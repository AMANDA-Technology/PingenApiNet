using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults.Embedded;
using PingenApiNet.Abstractions.Models.LetterEvents;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Letters.Views;
using PingenApiNet.Services.Connectors;

namespace PingenApiNet.UnitTests.Tests.Services.Connectors;

/// <summary>
/// Unit tests for <see cref="LetterService"/>
/// </summary>
public class LetterServiceTests
{
    private IPingenConnectionHandler _mockConnectionHandler = null!;
    private LetterService _letterService = null!;

    /// <summary>
    /// Set up mocks before each test
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockConnectionHandler = Substitute.For<IPingenConnectionHandler>();
        _letterService = new LetterService(_mockConnectionHandler);
    }

    /// <summary>
    /// Verifies GetPage calls ConnectionHandler.GetAsync with correct endpoint
    /// </summary>
    [Test]
    public async Task GetPage_CallsConnectionHandlerWithCorrectPath()
    {
        _mockConnectionHandler
            .GetAsync<CollectionResult<LetterData>>(
                "letters",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<LetterData>> { IsSuccess = true });

        await _letterService.GetPage();

        await _mockConnectionHandler.Received(1).GetAsync<CollectionResult<LetterData>>(
            "letters",
            Arg.Any<ApiPagingRequest?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies Get calls ConnectionHandler.GetAsync with correct path including letter ID
    /// </summary>
    [Test]
    public async Task Get_CallsConnectionHandlerWithCorrectPath()
    {
        const string letterId = "test-letter-id";

        _mockConnectionHandler
            .GetAsync<SingleResult<LetterDataDetailed>>(
                $"letters/{letterId}",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<LetterDataDetailed>> { IsSuccess = true });

        await _letterService.Get(letterId);

        await _mockConnectionHandler.Received(1).GetAsync<SingleResult<LetterDataDetailed>>(
            $"letters/{letterId}",
            Arg.Any<ApiPagingRequest?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies Delete calls ConnectionHandler.DeleteAsync with correct path
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerWithCorrectPath()
    {
        const string letterId = "delete-letter-id";

        _mockConnectionHandler
            .DeleteAsync(
                $"letters/{letterId}",
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult { IsSuccess = true });

        await _letterService.Delete(letterId);

        await _mockConnectionHandler.Received(1).DeleteAsync(
            $"letters/{letterId}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies Cancel calls ConnectionHandler.PatchAsync with correct cancel path
    /// </summary>
    [Test]
    public async Task Cancel_CallsConnectionHandlerWithCorrectPath()
    {
        const string letterId = "cancel-letter-id";

        _mockConnectionHandler
            .PatchAsync(
                $"letters/{letterId}/cancel",
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult { IsSuccess = true });

        await _letterService.Cancel(letterId);

        await _mockConnectionHandler.Received(1).PatchAsync(
            $"letters/{letterId}/cancel",
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies GetFileLocation calls ConnectionHandler.GetAsync with correct file path
    /// </summary>
    [Test]
    public async Task GetFileLocation_CallsConnectionHandlerWithCorrectPath()
    {
        const string letterId = "file-letter-id";

        _mockConnectionHandler
            .GetAsync(
                $"letters/{letterId}/file",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult { IsSuccess = true });

        await _letterService.GetFileLocation(letterId);

        await _mockConnectionHandler.Received(1).GetAsync(
            $"letters/{letterId}/file",
            Arg.Any<ApiPagingRequest?>(),
            Arg.Any<CancellationToken>());
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
            .PostAsync<SingleResult<LetterDataDetailed>, DataPost<LetterCreate, LetterCreateRelationships>>(
                "letters",
                Arg.Any<DataPost<LetterCreate, LetterCreateRelationships>>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<LetterDataDetailed>> { IsSuccess = true });

        await _letterService.Create(data);

        await _mockConnectionHandler.Received(1).PostAsync<SingleResult<LetterDataDetailed>, DataPost<LetterCreate, LetterCreateRelationships>>(
            "letters",
            Arg.Any<DataPost<LetterCreate, LetterCreateRelationships>>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies DownloadFileContent throws PingenFileDownloadException on failed download
    /// </summary>
    [Test]
    public async Task DownloadFileContent_FailedRequest_ThrowsPingenFileDownloadException()
    {
        var fileUrl = new Uri("https://example.com/file.pdf");

        _mockConnectionHandler
            .SendExternalRequestAsync(
                Arg.Any<HttpRequestMessage>(),
                Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
            {
                Content = new StringContent("<Error><Code>AccessDenied</Code><Message>Access Denied</Message></Error>")
            });

        await Should.ThrowAsync<Abstractions.Exceptions.PingenFileDownloadException>(
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
            .SendExternalRequestAsync(
                Arg.Any<HttpRequestMessage>(),
                Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(content)
            });

        var result = await _letterService.DownloadFileContent(fileUrl);

        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0L);
    }

    /// <summary>
    /// Verifies GetEventsPage URL-encodes the language parameter value
    /// </summary>
    [Test]
    public async Task GetEventsPage_UrlEncodesLanguageParameter()
    {
        const string letterId = "test-letter-id";
        const string language = "en&foo=bar";
        var expectedPath = $"letters/{letterId}/events?language={Uri.EscapeDataString(language)}";

        _mockConnectionHandler
            .GetAsync<CollectionResult<LetterEventData>>(
                expectedPath,
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<LetterEventData>> { IsSuccess = true });

        await _letterService.GetEventsPage(letterId, language);

        await _mockConnectionHandler.Received(1).GetAsync<CollectionResult<LetterEventData>>(
            expectedPath,
            Arg.Any<ApiPagingRequest?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies GetIssuesPage URL-encodes the language parameter value
    /// </summary>
    [Test]
    public async Task GetIssuesPage_UrlEncodesLanguageParameter()
    {
        const string language = "en&foo=bar";
        var expectedPath = $"letters/issues?language={Uri.EscapeDataString(language)}";

        _mockConnectionHandler
            .GetAsync<CollectionResult<LetterEventData>>(
                expectedPath,
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<LetterEventData>> { IsSuccess = true });

        await _letterService.GetIssuesPage(language);

        await _mockConnectionHandler.Received(1).GetAsync<CollectionResult<LetterEventData>>(
            expectedPath,
            Arg.Any<ApiPagingRequest?>(),
            Arg.Any<CancellationToken>());
    }
}
