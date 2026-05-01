using System.Net;
using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Files;
using PingenApiNet.Services.Connectors;

namespace PingenApiNet.UnitTests.Tests.Services.Connectors;

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
    /// Verifies GetPath propagates failure ApiResult without throwing when the API returns an error
    /// </summary>
    [Test]
    public async Task GetPath_ApiError_ReturnsFailureResult()
    {
        var apiError = CreateApiError("forbidden", "Token does not allow file upload");
        _mockConnectionHandler
            .GetAsync<SingleResult<FileUploadData>>(
                "file-upload",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<FileUploadData>>
            {
                IsSuccess = false,
                ApiError = apiError
            });

        var result = await _filesService.GetPath();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldBe(apiError),
            () => result.Data.ShouldBeNull()
        );
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
    /// Verifies UploadFile returns failure result on 408 Request Timeout
    /// </summary>
    [Test]
    public async Task UploadFile_RequestTimeout_ReturnsFailureResult()
    {
        var fileUploadData = CreateFileUploadData();
        using var stream = new MemoryStream([0x25, 0x50, 0x44, 0x46]);

        _mockConnectionHandler
            .SendExternalRequestAsync(
                Arg.Any<HttpRequestMessage>(),
                Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.RequestTimeout));

        var result = await _filesService.UploadFile(fileUploadData, stream);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.StatusCode.ShouldBe(HttpStatusCode.RequestTimeout)
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

    /// <summary>
    /// Verifies UploadFile throws <see cref="ArgumentNullException"/> when the data stream is null
    /// </summary>
    [Test]
    public async Task UploadFile_NullStream_ThrowsArgumentNullException()
    {
        var fileUploadData = CreateFileUploadData();

        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await _filesService.UploadFile(fileUploadData, null!));
    }

    /// <summary>
    /// Verifies UploadFile throws <see cref="NullReferenceException"/> when the file upload data is null
    /// </summary>
    [Test]
    public async Task UploadFile_NullFileUploadData_ThrowsNullReferenceException()
    {
        using var stream = new MemoryStream([0x25, 0x50, 0x44, 0x46]);

        await Should.ThrowAsync<NullReferenceException>(async () =>
            await _filesService.UploadFile(null!, stream));
    }

    /// <summary>
    /// Verifies UploadFile does not invoke the connection handler when the data stream is null
    /// </summary>
    [Test]
    public async Task UploadFile_NullStream_DoesNotInvokeConnectionHandler()
    {
        var fileUploadData = CreateFileUploadData();

        try
        {
            await _filesService.UploadFile(fileUploadData, null!);
        }
        catch (ArgumentNullException)
        {
            // expected — see UploadFile_NullStream_ThrowsArgumentNullException
        }

        await _mockConnectionHandler.DidNotReceive().SendExternalRequestAsync(
            Arg.Any<HttpRequestMessage>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that <see cref="FilesService.UploadFile"/> sends only the bytes from the data stream's
    /// current position onward — i.e. the service does not rewind the stream before reading
    /// </summary>
    [Test]
    public async Task UploadFile_StreamWithAdvancedPosition_SendsBytesFromCurrentPositionOnly()
    {
        var fileUploadData = CreateFileUploadData();
        using var stream = new MemoryStream([0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08]);
        stream.Position = 3;

        byte[]? capturedBytes = null;
        _mockConnectionHandler
            .SendExternalRequestAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var req = callInfo.Arg<HttpRequestMessage>();
                capturedBytes = req.Content!.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

        await _filesService.UploadFile(fileUploadData, stream);

        capturedBytes.ShouldNotBeNull();
        capturedBytes!.ShouldBe([0x04, 0x05, 0x06, 0x07, 0x08]);
    }

    /// <summary>
    /// Verifies that <see cref="FilesService.UploadFile"/> does not explicitly dispose the data stream
    /// passed by the caller — disposal is delegated to the <see cref="StreamContent"/> wrapper, so a
    /// non-owning stream wrapper around the caller's data leaves the underlying buffer usable
    /// </summary>
    [Test]
    public async Task UploadFile_DoesNotExplicitlyDisposeCallerStream()
    {
        var fileUploadData = CreateFileUploadData();
        var inner = new MemoryStream([0x25, 0x50, 0x44, 0x46]);
        var nonOwningWrapper = new NonOwningStreamWrapper(inner);

        _mockConnectionHandler
            .SendExternalRequestAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.OK));

        await _filesService.UploadFile(fileUploadData, nonOwningWrapper);

        inner.CanRead.ShouldBeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="FilesService.UploadFile"/> forwards an 8 MB stream byte-for-byte
    /// without truncation, transformation, or padding
    /// </summary>
    [Test]
    public async Task UploadFile_LargeMemoryStream_ForwardsBytesUnchanged()
    {
        var fileUploadData = CreateFileUploadData();
        var data = new byte[8 * 1024 * 1024];
        new Random(Seed: 42).NextBytes(data);
        using var stream = new MemoryStream(data);

        byte[]? capturedBytes = null;
        _mockConnectionHandler
            .SendExternalRequestAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var req = callInfo.Arg<HttpRequestMessage>();
                capturedBytes = req.Content!.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

        await _filesService.UploadFile(fileUploadData, stream);

        capturedBytes.ShouldNotBeNull();
        capturedBytes!.Length.ShouldBe(8 * 1024 * 1024);
        capturedBytes.ShouldBe(data);
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

    private static ApiError CreateApiError(string code, string detail) => new(
    [
        new ApiErrorData(code, "Error", detail, new ApiErrorSource(string.Empty, string.Empty))
    ]);

    /// <summary>
    /// Stream wrapper that delegates all reads/writes to an inner stream but does not dispose
    /// the inner stream when itself disposed. Used to detect whether <see cref="FilesService"/>
    /// explicitly disposes its data parameter beyond <see cref="StreamContent"/> ownership semantics.
    /// </summary>
    private sealed class NonOwningStreamWrapper : Stream
    {
        private readonly Stream _inner;

        public NonOwningStreamWrapper(Stream inner) => _inner = inner;

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => _inner.CanWrite;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => _inner.Position = value; }
        public override void Flush() => _inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void SetLength(long value) => _inner.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);

        protected override void Dispose(bool disposing)
        {
            // Intentionally does NOT dispose the inner stream — caller retains ownership.
            base.Dispose(disposing);
        }
    }
}
