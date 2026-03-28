using PingenApiNet.Abstractions.Enums.Api;
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

    private static WebhookData CreateWebhookData(string id) => new()
    {
        Id = id,
        Type = PingenApiDataType.webhooks,
        Attributes = new Webhook(WebhookEventCategory.issues, new Uri("https://example.com/webhook"), "key"),
        Relationships = new WebhookRelationships(new RelatedSingleOutput(new RelatedSingleLinks(""), new Abstractions.Models.Base.DataIdentity { Id = "", Type = PingenApiDataType.organisations }))
    };
}
