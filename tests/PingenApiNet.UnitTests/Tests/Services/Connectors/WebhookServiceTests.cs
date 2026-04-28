using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Exceptions;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.Relations;
using PingenApiNet.Abstractions.Models.Api.Embedded.Relations.Embedded;
using PingenApiNet.Abstractions.Models.Webhooks;
using PingenApiNet.Abstractions.Models.Webhooks.Views;
using PingenApiNet.Services.Connectors;

namespace PingenApiNet.UnitTests.Tests.Services.Connectors;

/// <summary>
/// Unit tests for <see cref="WebhookService"/>
/// </summary>
public class WebhookServiceTests
{
    private IPingenConnectionHandler _mockConnectionHandler = null!;
    private WebhookService _webhookService = null!;

    /// <summary>
    /// Set up mocks before each test
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockConnectionHandler = Substitute.For<IPingenConnectionHandler>();
        _webhookService = new WebhookService(_mockConnectionHandler);
    }

    /// <summary>
    /// Verifies WebhookService is sealed for consistency with other connector services
    /// </summary>
    [Test]
    public void WebhookService_IsSealed()
    {
        typeof(WebhookService).IsSealed.ShouldBeTrue();
    }

    /// <summary>
    /// Verifies GetPage calls ConnectionHandler with correct endpoint
    /// </summary>
    [Test]
    public async Task GetPage_CallsConnectionHandlerWithCorrectPath()
    {
        _mockConnectionHandler
            .GetAsync<CollectionResult<WebhookData>>(
                "webhooks",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<WebhookData>> { IsSuccess = true });

        await _webhookService.GetPage();

        await _mockConnectionHandler.Received(1).GetAsync<CollectionResult<WebhookData>>(
            "webhooks",
            Arg.Any<ApiPagingRequest?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies Get calls ConnectionHandler with correct path
    /// </summary>
    [Test]
    public async Task Get_CallsConnectionHandlerWithCorrectPath()
    {
        const string webhookId = "webhook-123";

        _mockConnectionHandler
            .GetAsync<SingleResult<WebhookData>>(
                $"webhooks/{webhookId}",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<WebhookData>> { IsSuccess = true });

        await _webhookService.Get(webhookId);

        await _mockConnectionHandler.Received(1).GetAsync<SingleResult<WebhookData>>(
            $"webhooks/{webhookId}",
            Arg.Any<ApiPagingRequest?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies Delete calls ConnectionHandler with correct path
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerWithCorrectPath()
    {
        const string webhookId = "webhook-delete";

        _mockConnectionHandler
            .DeleteAsync(
                $"webhooks/{webhookId}",
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult { IsSuccess = true });

        await _webhookService.Delete(webhookId);

        await _mockConnectionHandler.Received(1).DeleteAsync(
            $"webhooks/{webhookId}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies Create calls ConnectionHandler.PostAsync with correct endpoint and payload type
    /// </summary>
    [Test]
    public async Task Create_CallsConnectionHandlerWithCorrectPath()
    {
        var data = new DataPost<WebhookCreate>
        {
            Type = PingenApiDataType.webhooks,
            Attributes = new WebhookCreate
            {
                FileOriginalName = WebhookEventCategory.issues,
                Url = new Uri("https://example.com/webhook"),
                SigningKey = "test-signing-key"
            }
        };

        _mockConnectionHandler
            .PostAsync<SingleResult<WebhookData>, DataPost<WebhookCreate>>(
                "webhooks",
                Arg.Any<DataPost<WebhookCreate>>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<WebhookData>> { IsSuccess = true });

        await _webhookService.Create(data);

        await _mockConnectionHandler.Received(1).PostAsync<SingleResult<WebhookData>, DataPost<WebhookCreate>>(
            "webhooks",
            Arg.Any<DataPost<WebhookCreate>>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies GetPageResultsAsync calls ConnectionHandler and returns pages via auto-pagination
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_CallsConnectionHandlerAndReturnsPages()
    {
        var webhookData = CreateWebhookData("webhook-page-1");

        _mockConnectionHandler
            .GetAsync<CollectionResult<WebhookData>>(
                "webhooks",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<WebhookData>>
            {
                IsSuccess = true,
                Data = new CollectionResult<WebhookData>(
                    [webhookData],
                    new CollectionResultLinks("", "", "", "", ""),
                    new CollectionResultMeta(1, 1, 20, 1, 1, 1))
            });

        var pages = new List<IEnumerable<WebhookData>>();
        await foreach (var page in _webhookService.GetPageResultsAsync())
        {
            pages.Add(page);
        }

        pages.Count.ShouldBe(1);
        pages[0].First().Id.ShouldBe("webhook-page-1");
    }

    /// <summary>
    /// Verifies GetPage propagates failure ApiResult without throwing when the API returns an error
    /// </summary>
    [Test]
    public async Task GetPage_ApiError_ReturnsFailureResult()
    {
        var apiError = CreateApiError("server_error", "Service unavailable");
        _mockConnectionHandler
            .GetAsync<CollectionResult<WebhookData>>(
                "webhooks",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<WebhookData>>
            {
                IsSuccess = false,
                ApiError = apiError
            });

        var result = await _webhookService.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldBe(apiError),
            () => result.Data.ShouldBeNull()
        );
    }

    /// <summary>
    /// Verifies GetPageResultsAsync surfaces an API failure as a <see cref="PingenApiErrorException"/>
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_ApiError_ThrowsPingenApiErrorException()
    {
        _mockConnectionHandler
            .GetAsync<CollectionResult<WebhookData>>(
                "webhooks",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<WebhookData>>
            {
                IsSuccess = false,
                ApiError = CreateApiError("server_error", "boom")
            });

        await Should.ThrowAsync<PingenApiErrorException>(async () =>
        {
            await foreach (var _ in _webhookService.GetPageResultsAsync())
            {
                // consume pages
            }
        });
    }

    /// <summary>
    /// Verifies Get propagates failure ApiResult without throwing when the API returns an error
    /// </summary>
    [Test]
    public async Task Get_ApiError_ReturnsFailureResult()
    {
        const string webhookId = "missing-webhook";
        var apiError = CreateApiError("not_found", "Webhook not found");
        _mockConnectionHandler
            .GetAsync<SingleResult<WebhookData>>(
                $"webhooks/{webhookId}",
                Arg.Any<ApiRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<WebhookData>>
            {
                IsSuccess = false,
                ApiError = apiError
            });

        var result = await _webhookService.Get(webhookId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldBe(apiError),
            () => result.Data.ShouldBeNull()
        );
    }

    /// <summary>
    /// Verifies Get with an empty ID constructs an endpoint path with a trailing slash and does not throw
    /// </summary>
    [Test]
    public async Task Get_EmptyId_ConstructsTrailingSlashPath()
    {
        _mockConnectionHandler
            .GetAsync<SingleResult<WebhookData>>(
                "webhooks/",
                Arg.Any<ApiRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<WebhookData>> { IsSuccess = true });

        await _webhookService.Get(string.Empty);

        await _mockConnectionHandler.Received(1).GetAsync<SingleResult<WebhookData>>(
            "webhooks/",
            Arg.Any<ApiRequest?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies Delete propagates failure ApiResult without throwing when the API returns an error
    /// </summary>
    [Test]
    public async Task Delete_ApiError_ReturnsFailureResult()
    {
        const string webhookId = "delete-webhook";
        var apiError = CreateApiError("conflict", "Webhook cannot be deleted");
        _mockConnectionHandler
            .DeleteAsync(
                $"webhooks/{webhookId}",
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult
            {
                IsSuccess = false,
                ApiError = apiError
            });

        var result = await _webhookService.Delete(webhookId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldBe(apiError)
        );
    }

    /// <summary>
    /// Verifies Delete with an empty ID constructs an endpoint path with a trailing slash and does not throw
    /// </summary>
    [Test]
    public async Task Delete_EmptyId_ConstructsTrailingSlashPath()
    {
        _mockConnectionHandler
            .DeleteAsync(
                "webhooks/",
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult { IsSuccess = true });

        await _webhookService.Delete(string.Empty);

        await _mockConnectionHandler.Received(1).DeleteAsync(
            "webhooks/",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies Create propagates failure ApiResult without throwing when the API rejects the payload
    /// </summary>
    [Test]
    public async Task Create_ApiError_ReturnsFailureResult()
    {
        var data = CreateWebhookPost();
        var apiError = CreateApiError("unprocessable_entity", "Webhook URL is invalid");
        _mockConnectionHandler
            .PostAsync<SingleResult<WebhookData>, DataPost<WebhookCreate>>(
                "webhooks",
                Arg.Any<DataPost<WebhookCreate>>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<WebhookData>>
            {
                IsSuccess = false,
                ApiError = apiError
            });

        var result = await _webhookService.Create(data);

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
        const string idempotencyKey = "webhook-idem-1";
        var data = CreateWebhookPost();

        _mockConnectionHandler
            .PostAsync<SingleResult<WebhookData>, DataPost<WebhookCreate>>(
                "webhooks",
                Arg.Any<DataPost<WebhookCreate>>(),
                idempotencyKey,
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<WebhookData>> { IsSuccess = true });

        await _webhookService.Create(data, idempotencyKey);

        await _mockConnectionHandler.Received(1).PostAsync<SingleResult<WebhookData>, DataPost<WebhookCreate>>(
            "webhooks",
            Arg.Any<DataPost<WebhookCreate>>(),
            idempotencyKey,
            Arg.Any<CancellationToken>());
    }

    private static DataPost<WebhookCreate> CreateWebhookPost() => new()
    {
        Type = PingenApiDataType.webhooks,
        Attributes = new WebhookCreate
        {
            FileOriginalName = WebhookEventCategory.issues,
            Url = new Uri("https://example.com/webhook"),
            SigningKey = "test-signing-key"
        }
    };

    private static WebhookData CreateWebhookData(string id) => new()
    {
        Id = id,
        Type = PingenApiDataType.webhooks,
        Attributes = new Webhook(WebhookEventCategory.issues, new Uri("https://example.com/webhook"), "key"),
        Relationships = new WebhookRelationships(new RelatedSingleOutput(new RelatedSingleLinks(""), new Abstractions.Models.Base.DataIdentity { Id = "", Type = PingenApiDataType.organisations }))
    };

    private static ApiError CreateApiError(string code, string detail) => new(
    [
        new ApiErrorData(code, "Error", detail, new ApiErrorSource(string.Empty, string.Empty))
    ]);
}
