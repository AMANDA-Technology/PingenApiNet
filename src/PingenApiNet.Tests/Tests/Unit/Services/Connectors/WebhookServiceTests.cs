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
}
