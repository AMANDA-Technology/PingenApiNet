using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Webhooks;
using PingenApiNet.Services.Connectors;

namespace PingenApiNet.Tests.Tests.Unit.Services.Connectors;

/// <summary>
/// Unit tests for <see cref="WebhookService"/>
/// </summary>
public class WebhookServiceTests
{
    private Mock<IPingenConnectionHandler> _mockConnectionHandler = null!;
    private WebhookService _webhookService = null!;

    /// <summary>
    /// Set up mocks before each test
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockConnectionHandler = new Mock<IPingenConnectionHandler>();
        _webhookService = new WebhookService(_mockConnectionHandler.Object);
    }

    /// <summary>
    /// Verifies GetPage calls ConnectionHandler with correct endpoint
    /// </summary>
    [Test]
    public async Task GetPage_CallsConnectionHandlerWithCorrectPath()
    {
        _mockConnectionHandler
            .Setup(x => x.GetAsync<CollectionResult<WebhookData>>(
                "webhooks",
                It.IsAny<ApiPagingRequest?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult<CollectionResult<WebhookData>> { IsSuccess = true });

        await _webhookService.GetPage();

        _mockConnectionHandler.Verify(x => x.GetAsync<CollectionResult<WebhookData>>(
            "webhooks",
            It.IsAny<ApiPagingRequest?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies Get calls ConnectionHandler with correct path
    /// </summary>
    [Test]
    public async Task Get_CallsConnectionHandlerWithCorrectPath()
    {
        const string webhookId = "webhook-123";

        _mockConnectionHandler
            .Setup(x => x.GetAsync<SingleResult<WebhookData>>(
                $"webhooks/{webhookId}",
                It.IsAny<ApiPagingRequest?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult<SingleResult<WebhookData>> { IsSuccess = true });

        await _webhookService.Get(webhookId);

        _mockConnectionHandler.Verify(x => x.GetAsync<SingleResult<WebhookData>>(
            $"webhooks/{webhookId}",
            It.IsAny<ApiPagingRequest?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies Delete calls ConnectionHandler with correct path
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerWithCorrectPath()
    {
        const string webhookId = "webhook-delete";

        _mockConnectionHandler
            .Setup(x => x.DeleteAsync(
                $"webhooks/{webhookId}",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult { IsSuccess = true });

        await _webhookService.Delete(webhookId);

        _mockConnectionHandler.Verify(x => x.DeleteAsync(
            $"webhooks/{webhookId}",
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
