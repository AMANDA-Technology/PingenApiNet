using System.Net;
using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Files;
using PingenApiNet.Services.Connectors;

namespace PingenApiNet.Tests.Tests.Unit.Services.Connectors;

/// <summary>
/// Unit tests for <see cref="FilesService"/>
/// </summary>
public class FilesServiceTests
{
    private IPingenConnectionHandler _mockConnectionHandler = null!;
    private FilesService _filesService = null!;

    /// <summary>
    /// Set up mocks before each test
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockConnectionHandler = Substitute.For<IPingenConnectionHandler>();
        _filesService = new FilesService(_mockConnectionHandler);
    }

    /// <summary>
    /// Verifies GetPath calls ConnectionHandler.GetAsync with correct endpoint
    /// </summary>
    [Test]
    public async Task GetPath_CallsConnectionHandlerWithCorrectPath()
    {
        _mockConnectionHandler
            .GetAsync<SingleResult<FileUploadData>>(
                "file-upload",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<FileUploadData>> { IsSuccess = true });

        await _filesService.GetPath();

        await _mockConnectionHandler.Received(1).GetAsync<SingleResult<FileUploadData>>(
            "file-upload",
            Arg.Any<ApiPagingRequest?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies UploadFile returns success result on 200 OK
    /// </summary>
    [Test]
    public async Task UploadFile_Success_ReturnsSuccessResult()
    {
        var fileUploadData = CreateFileUploadData();
        using var stream = new MemoryStream([0x25, 0x50, 0x44, 0x46]);

        _mockConnectionHandler
            .SendExternalRequestAsync(
                Arg.Any<HttpRequestMessage>(),
                Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.OK));

        var result = await _filesService.UploadFile(fileUploadData, stream);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.StatusCode.ShouldBe(HttpStatusCode.OK),
            () => result.ReasonPhrase.ShouldBe("OK")
        );
    }

    /// <summary>
    /// Verifies UploadFile returns failure result on 403 Forbidden
    /// </summary>
    [Test]
    public async Task UploadFile_Forbidden_ReturnsFailureResult()
    {
        var fileUploadData = CreateFileUploadData();
        using var stream = new MemoryStream([0x25, 0x50, 0x44, 0x46]);

        _mockConnectionHandler
            .SendExternalRequestAsync(
                Arg.Any<HttpRequestMessage>(),
                Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.Forbidden));

        var result = await _filesService.UploadFile(fileUploadData, stream);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.StatusCode.ShouldBe(HttpStatusCode.Forbidden),
            () => result.ReasonPhrase.ShouldBe("Forbidden")
        );
    }

    /// <summary>
    /// Verifies UploadFile returns failure result on 500 Internal Server Error
    /// </summary>
    [Test]
    public async Task UploadFile_InternalServerError_ReturnsFailureResult()
    {
        var fileUploadData = CreateFileUploadData();
        using var stream = new MemoryStream([0x25, 0x50, 0x44, 0x46]);

        _mockConnectionHandler
            .SendExternalRequestAsync(
                Arg.Any<HttpRequestMessage>(),
                Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var result = await _filesService.UploadFile(fileUploadData, stream);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.StatusCode.ShouldBe(HttpStatusCode.InternalServerError)
        );
    }

    /// <summary>
    /// Verifies UploadFile sends PUT request to the correct URL
    /// </summary>
    [Test]
    public async Task UploadFile_SendsPutRequestToCorrectUrl()
    {
        const string uploadUrl = "https://s3.example.com/bucket/file.pdf";
        var fileUploadData = CreateFileUploadData(uploadUrl);
        using var stream = new MemoryStream([0x25, 0x50, 0x44, 0x46]);

        _mockConnectionHandler
            .SendExternalRequestAsync(
                Arg.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Put &&
                    r.RequestUri!.ToString() == uploadUrl),
                Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.OK));

        await _filesService.UploadFile(fileUploadData, stream);

        await _mockConnectionHandler.Received(1).SendExternalRequestAsync(
            Arg.Is<HttpRequestMessage>(r =>
                r.Method == HttpMethod.Put &&
                r.RequestUri!.ToString() == uploadUrl),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies UploadFile sends StreamContent in the request
    /// </summary>
    [Test]
    public async Task UploadFile_SendsStreamContent()
    {
        var fileUploadData = CreateFileUploadData();
        using var stream = new MemoryStream([0x25, 0x50, 0x44, 0x46]);

        _mockConnectionHandler
            .SendExternalRequestAsync(
                Arg.Is<HttpRequestMessage>(r => r.Content is StreamContent),
                Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.OK));

        await _filesService.UploadFile(fileUploadData, stream);

        await _mockConnectionHandler.Received(1).SendExternalRequestAsync(
            Arg.Is<HttpRequestMessage>(r => r.Content is StreamContent),
            Arg.Any<CancellationToken>());
    }

    private static FileUploadData CreateFileUploadData(string url = "https://s3.example.com/test")
    {
        return new FileUploadData
        {
            Id = "test-file-id",
            Type = PingenApiDataType.file_uploads,
            Attributes = new FileUpload(
                Url: url,
                UrlSignature: "test-signature",
                ExpiresAt: DateTime.UtcNow.AddHours(1)
            )
        };
    }
}
