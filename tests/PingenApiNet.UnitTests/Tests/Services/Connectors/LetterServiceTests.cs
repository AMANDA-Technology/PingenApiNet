using System.Net;
using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Enums.Letters;
using PingenApiNet.Abstractions.Exceptions;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.LetterEvents;
using PingenApiNet.Abstractions.Models.LetterPrices;
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
    /// Verifies GetPage propagates failure ApiResult without throwing when the API returns an error
    /// </summary>
    [Test]
    public async Task GetPage_ApiError_ReturnsFailureResult()
    {
        var apiError = CreateApiError("server_error", "Service unavailable");
        _mockConnectionHandler
            .GetAsync<CollectionResult<LetterData>>(
                "letters",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<LetterData>>
            {
                IsSuccess = false,
                ApiError = apiError
            });

        var result = await _letterService.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldBe(apiError),
            () => result.Data.ShouldBeNull()
        );
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
    /// Verifies Get propagates failure ApiResult without throwing when the API returns an error
    /// </summary>
    [Test]
    public async Task Get_ApiError_ReturnsFailureResult()
    {
        const string letterId = "missing-letter";
        var apiError = CreateApiError("not_found", "Letter not found");
        _mockConnectionHandler
            .GetAsync<SingleResult<LetterDataDetailed>>(
                $"letters/{letterId}",
                Arg.Any<ApiRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<LetterDataDetailed>>
            {
                IsSuccess = false,
                ApiError = apiError
            });

        var result = await _letterService.Get(letterId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldBe(apiError)
        );
    }

    /// <summary>
    /// Verifies Get with an empty ID constructs an endpoint path with a trailing slash and does not throw
    /// </summary>
    [Test]
    public async Task Get_EmptyId_ConstructsTrailingSlashPath()
    {
        _mockConnectionHandler
            .GetAsync<SingleResult<LetterDataDetailed>>(
                "letters/",
                Arg.Any<ApiRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<LetterDataDetailed>> { IsSuccess = true });

        await _letterService.Get(string.Empty);

        await _mockConnectionHandler.Received(1).GetAsync<SingleResult<LetterDataDetailed>>(
            "letters/",
            Arg.Any<ApiRequest?>(),
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
    /// Verifies Delete propagates failure ApiResult without throwing when the API returns an error
    /// </summary>
    [Test]
    public async Task Delete_ApiError_ReturnsFailureResult()
    {
        const string letterId = "delete-letter-id";
        var apiError = CreateApiError("conflict", "Letter cannot be deleted in current state");
        _mockConnectionHandler
            .DeleteAsync(
                $"letters/{letterId}",
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult
            {
                IsSuccess = false,
                ApiError = apiError
            });

        var result = await _letterService.Delete(letterId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldBe(apiError)
        );
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
    /// Verifies Cancel propagates failure ApiResult without throwing when the API returns an error
    /// </summary>
    [Test]
    public async Task Cancel_ApiError_ReturnsFailureResult()
    {
        const string letterId = "cancel-letter-id";
        var apiError = CreateApiError("conflict", "Letter cannot be cancelled in current state");
        _mockConnectionHandler
            .PatchAsync(
                $"letters/{letterId}/cancel",
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult
            {
                IsSuccess = false,
                ApiError = apiError
            });

        var result = await _letterService.Cancel(letterId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldBe(apiError)
        );
    }

    /// <summary>
    /// Verifies Cancel forwards the supplied idempotency key through to the connection handler
    /// </summary>
    [Test]
    public async Task Cancel_WithIdempotencyKey_ForwardsKeyToConnectionHandler()
    {
        const string letterId = "cancel-letter-id";
        const string idempotencyKey = "cancel-idem-1";

        _mockConnectionHandler
            .PatchAsync(
                $"letters/{letterId}/cancel",
                idempotencyKey,
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult { IsSuccess = true });

        await _letterService.Cancel(letterId, idempotencyKey);

        await _mockConnectionHandler.Received(1).PatchAsync(
            $"letters/{letterId}/cancel",
            idempotencyKey,
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
    /// Verifies GetFileLocation propagates failure ApiResult without throwing when the API returns an error
    /// </summary>
    [Test]
    public async Task GetFileLocation_ApiError_ReturnsFailureResult()
    {
        const string letterId = "file-letter-id";
        var apiError = CreateApiError("not_found", "Letter file not available");
        _mockConnectionHandler
            .GetAsync(
                $"letters/{letterId}/file",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult
            {
                IsSuccess = false,
                ApiError = apiError
            });

        var result = await _letterService.GetFileLocation(letterId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldBe(apiError)
        );
    }

    /// <summary>
    /// Verifies Create calls ConnectionHandler.PostAsync with correct path
    /// </summary>
    [Test]
    public async Task Create_CallsConnectionHandlerWithCorrectPath()
    {
        var data = CreateLetterPost();

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
    /// Verifies Create propagates failure ApiResult without throwing when the API rejects the payload
    /// </summary>
    [Test]
    public async Task Create_ApiError_ReturnsFailureResult()
    {
        var data = CreateLetterPost();
        var apiError = CreateApiError("unprocessable_entity", "File URL signature invalid");
        _mockConnectionHandler
            .PostAsync<SingleResult<LetterDataDetailed>, DataPost<LetterCreate, LetterCreateRelationships>>(
                "letters",
                Arg.Any<DataPost<LetterCreate, LetterCreateRelationships>>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<LetterDataDetailed>>
            {
                IsSuccess = false,
                ApiError = apiError
            });

        var result = await _letterService.Create(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldBe(apiError),
            () => result.Data.ShouldBeNull()
        );
    }

    /// <summary>
    /// Verifies Create forwards the supplied idempotency key through to the connection handler
    /// </summary>
    [Test]
    public async Task Create_WithIdempotencyKey_ForwardsKeyToConnectionHandler()
    {
        const string idempotencyKey = "create-idem-1";
        var data = CreateLetterPost();

        _mockConnectionHandler
            .PostAsync<SingleResult<LetterDataDetailed>, DataPost<LetterCreate, LetterCreateRelationships>>(
                "letters",
                Arg.Any<DataPost<LetterCreate, LetterCreateRelationships>>(),
                idempotencyKey,
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<LetterDataDetailed>> { IsSuccess = true });

        await _letterService.Create(data, idempotencyKey);

        await _mockConnectionHandler.Received(1).PostAsync<SingleResult<LetterDataDetailed>, DataPost<LetterCreate, LetterCreateRelationships>>(
            "letters",
            Arg.Any<DataPost<LetterCreate, LetterCreateRelationships>>(),
            idempotencyKey,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies Send calls ConnectionHandler.PatchAsync with correct send path including the letter ID
    /// </summary>
    [Test]
    public async Task Send_CallsConnectionHandlerWithCorrectPath()
    {
        const string letterId = "send-letter-id";
        var data = CreateLetterSendPatch(letterId);

        _mockConnectionHandler
            .PatchAsync<SingleResult<LetterDataDetailed>, DataPatch<LetterSend>>(
                $"letters/{letterId}/send",
                Arg.Any<DataPatch<LetterSend>>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<LetterDataDetailed>> { IsSuccess = true });

        await _letterService.Send(data);

        await _mockConnectionHandler.Received(1).PatchAsync<SingleResult<LetterDataDetailed>, DataPatch<LetterSend>>(
            $"letters/{letterId}/send",
            Arg.Any<DataPatch<LetterSend>>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies Send propagates failure ApiResult without throwing when the API returns an error
    /// </summary>
    [Test]
    public async Task Send_ApiError_ReturnsFailureResult()
    {
        const string letterId = "send-letter-id";
        var data = CreateLetterSendPatch(letterId);
        var apiError = CreateApiError("conflict", "Letter is not in a valid state to send");
        _mockConnectionHandler
            .PatchAsync<SingleResult<LetterDataDetailed>, DataPatch<LetterSend>>(
                $"letters/{letterId}/send",
                Arg.Any<DataPatch<LetterSend>>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<LetterDataDetailed>>
            {
                IsSuccess = false,
                ApiError = apiError
            });

        var result = await _letterService.Send(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldBe(apiError)
        );
    }

    /// <summary>
    /// Verifies Send throws <see cref="NullReferenceException"/> when the patch data is null because the service dereferences <c>data.Id</c> to build the endpoint path
    /// </summary>
    [Test]
    public async Task Send_NullData_ThrowsNullReferenceException()
    {
        await Should.ThrowAsync<NullReferenceException>(async () =>
            await _letterService.Send(null!));
    }

    /// <summary>
    /// Verifies Update calls ConnectionHandler.PatchAsync with correct path including the letter ID
    /// </summary>
    [Test]
    public async Task Update_CallsConnectionHandlerWithCorrectPath()
    {
        const string letterId = "update-letter-id";
        var data = CreateLetterUpdatePatch(letterId);

        _mockConnectionHandler
            .PatchAsync<SingleResult<LetterDataDetailed>, DataPatch<LetterUpdate>>(
                $"letters/{letterId}",
                Arg.Any<DataPatch<LetterUpdate>>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<LetterDataDetailed>> { IsSuccess = true });

        await _letterService.Update(data);

        await _mockConnectionHandler.Received(1).PatchAsync<SingleResult<LetterDataDetailed>, DataPatch<LetterUpdate>>(
            $"letters/{letterId}",
            Arg.Any<DataPatch<LetterUpdate>>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies Update propagates failure ApiResult without throwing when the API returns an error
    /// </summary>
    [Test]
    public async Task Update_ApiError_ReturnsFailureResult()
    {
        const string letterId = "update-letter-id";
        var data = CreateLetterUpdatePatch(letterId);
        var apiError = CreateApiError("unprocessable_entity", "Paper type is invalid");
        _mockConnectionHandler
            .PatchAsync<SingleResult<LetterDataDetailed>, DataPatch<LetterUpdate>>(
                $"letters/{letterId}",
                Arg.Any<DataPatch<LetterUpdate>>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<LetterDataDetailed>>
            {
                IsSuccess = false,
                ApiError = apiError
            });

        var result = await _letterService.Update(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldBe(apiError)
        );
    }

    /// <summary>
    /// Verifies Update throws <see cref="NullReferenceException"/> when the patch data is null because the service dereferences <c>data.Id</c> to build the endpoint path
    /// </summary>
    [Test]
    public async Task Update_NullData_ThrowsNullReferenceException()
    {
        await Should.ThrowAsync<NullReferenceException>(async () =>
            await _letterService.Update(null!));
    }

    /// <summary>
    /// Verifies CalculatePrice calls ConnectionHandler.PostAsync with correct price-calculator path
    /// </summary>
    [Test]
    public async Task CalculatePrice_CallsConnectionHandlerWithCorrectPath()
    {
        var data = CreateLetterPriceConfigurationPost();

        _mockConnectionHandler
            .PostAsync<SingleResult<LetterPriceData>, DataPost<LetterPriceConfiguration>>(
                "letters/price-calculator",
                Arg.Any<DataPost<LetterPriceConfiguration>>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<LetterPriceData>> { IsSuccess = true });

        await _letterService.CalculatePrice(data);

        await _mockConnectionHandler.Received(1).PostAsync<SingleResult<LetterPriceData>, DataPost<LetterPriceConfiguration>>(
            "letters/price-calculator",
            Arg.Any<DataPost<LetterPriceConfiguration>>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies CalculatePrice propagates failure ApiResult without throwing when the API returns an error
    /// </summary>
    [Test]
    public async Task CalculatePrice_ApiError_ReturnsFailureResult()
    {
        var data = CreateLetterPriceConfigurationPost();
        var apiError = CreateApiError("unprocessable_entity", "Country code is invalid");
        _mockConnectionHandler
            .PostAsync<SingleResult<LetterPriceData>, DataPost<LetterPriceConfiguration>>(
                "letters/price-calculator",
                Arg.Any<DataPost<LetterPriceConfiguration>>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<LetterPriceData>>
            {
                IsSuccess = false,
                ApiError = apiError
            });

        var result = await _letterService.CalculatePrice(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldBe(apiError)
        );
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
            .Returns(new HttpResponseMessage(HttpStatusCode.Forbidden)
            {
                Content = new StringContent("<Error><Code>AccessDenied</Code><Message>Access Denied</Message></Error>")
            });

        await Should.ThrowAsync<PingenFileDownloadException>(
            async () => await _letterService.DownloadFileContent(fileUrl));
    }

    /// <summary>
    /// Verifies DownloadFileContent extracts the AWS-style error code from the response body and exposes it on the thrown exception
    /// </summary>
    [Test]
    public async Task DownloadFileContent_FailedRequest_ExtractsErrorCodeOntoException()
    {
        var fileUrl = new Uri("https://example.com/file.pdf");

        _mockConnectionHandler
            .SendExternalRequestAsync(
                Arg.Any<HttpRequestMessage>(),
                Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.Forbidden)
            {
                Content = new StringContent("<Error><Code>AccessDenied</Code><Message>Access Denied</Message></Error>")
            });

        var ex = await Should.ThrowAsync<PingenFileDownloadException>(
            async () => await _letterService.DownloadFileContent(fileUrl));
        ex.ErrorCode.ShouldBe("AccessDenied");
    }

    /// <summary>
    /// Verifies DownloadFileContent throws <see cref="PingenFileDownloadException"/> with an empty error code when the AWS response body has no <c>Code</c> element
    /// </summary>
    [Test]
    public async Task DownloadFileContent_FailedRequestMissingErrorCode_ThrowsPingenFileDownloadExceptionWithEmptyCode()
    {
        var fileUrl = new Uri("https://example.com/file.pdf");

        _mockConnectionHandler
            .SendExternalRequestAsync(
                Arg.Any<HttpRequestMessage>(),
                Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.Forbidden)
            {
                Content = new StringContent("<Error><Message>Access Denied</Message></Error>")
            });

        var ex = await Should.ThrowAsync<PingenFileDownloadException>(
            async () => await _letterService.DownloadFileContent(fileUrl));
        ex.ErrorCode.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Verifies DownloadFileContent issues an HTTP GET request against the supplied file URL
    /// </summary>
    [Test]
    public async Task DownloadFileContent_SendsGetRequestToCorrectUrl()
    {
        var fileUrl = new Uri("https://files.example.com/letter.pdf");
        var content = new byte[] { 0x25, 0x50, 0x44, 0x46 };

        _mockConnectionHandler
            .SendExternalRequestAsync(
                Arg.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get &&
                    r.RequestUri == fileUrl),
                Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(content)
            });

        await _letterService.DownloadFileContent(fileUrl);

        await _mockConnectionHandler.Received(1).SendExternalRequestAsync(
            Arg.Is<HttpRequestMessage>(r =>
                r.Method == HttpMethod.Get &&
                r.RequestUri == fileUrl),
            Arg.Any<CancellationToken>());
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
            .Returns(new HttpResponseMessage(HttpStatusCode.OK)
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
    /// Verifies GetEventsPage propagates failure ApiResult without throwing when the API returns an error
    /// </summary>
    [Test]
    public async Task GetEventsPage_ApiError_ReturnsFailureResult()
    {
        const string letterId = "test-letter-id";
        const string language = "en";
        var expectedPath = $"letters/{letterId}/events?language={Uri.EscapeDataString(language)}";
        var apiError = CreateApiError("not_found", "Letter not found");

        _mockConnectionHandler
            .GetAsync<CollectionResult<LetterEventData>>(
                expectedPath,
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<LetterEventData>>
            {
                IsSuccess = false,
                ApiError = apiError
            });

        var result = await _letterService.GetEventsPage(letterId, language);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldBe(apiError)
        );
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

    /// <summary>
    /// Verifies GetIssuesPage propagates failure ApiResult without throwing when the API returns an error
    /// </summary>
    [Test]
    public async Task GetIssuesPage_ApiError_ReturnsFailureResult()
    {
        const string language = "en";
        var expectedPath = $"letters/issues?language={Uri.EscapeDataString(language)}";
        var apiError = CreateApiError("server_error", "Service unavailable");

        _mockConnectionHandler
            .GetAsync<CollectionResult<LetterEventData>>(
                expectedPath,
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<LetterEventData>>
            {
                IsSuccess = false,
                ApiError = apiError
            });

        var result = await _letterService.GetIssuesPage(language);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldBe(apiError)
        );
    }

    private static DataPost<LetterCreate, LetterCreateRelationships> CreateLetterPost() => new()
    {
        Type = PingenApiDataType.letters,
        Attributes = new LetterCreate
        {
            FileOriginalName = "test.pdf",
            FileUrl = "https://example.com/test.pdf",
            FileUrlSignature = "sig",
            AddressPosition = LetterAddressPosition.left,
            AutoSend = false,
            DeliveryProduct = "cheap",
            PrintMode = LetterPrintMode.simplex,
            PrintSpectrum = LetterPrintSpectrum.grayscale
        }
    };

    private static DataPatch<LetterSend> CreateLetterSendPatch(string id) => new()
    {
        Id = id,
        Type = PingenApiDataType.letters,
        Attributes = new LetterSend
        {
            DeliveryProduct = "cheap",
            PrintMode = LetterPrintMode.simplex,
            PrintSpectrum = LetterPrintSpectrum.grayscale
        }
    };

    private static DataPatch<LetterUpdate> CreateLetterUpdatePatch(string id) => new()
    {
        Id = id,
        Type = PingenApiDataType.letters,
        Attributes = new LetterUpdate
        {
            PaperTypes = ["normal"]
        }
    };

    private static DataPost<LetterPriceConfiguration> CreateLetterPriceConfigurationPost() => new()
    {
        Type = PingenApiDataType.letters,
        Attributes = new LetterPriceConfiguration
        {
            Country = "CH",
            PaperTypes = ["normal"],
            PrintMode = LetterPrintMode.simplex,
            PrintSpectrum = LetterPrintSpectrum.grayscale,
            DeliveryProduct = "cheap"
        }
    };

    private static ApiError CreateApiError(string code, string detail) => new(
    [
        new ApiErrorData(code, "Error", detail, new ApiErrorSource(string.Empty, string.Empty))
    ]);
}
