using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Batches;
using PingenApiNet.Services.Connectors;

namespace PingenApiNet.Tests.Tests.Unit.Services.Connectors;

/// <summary>
/// Unit tests for <see cref="BatchService"/>
/// </summary>
public class BatchServiceTests
{
    private Mock<IPingenConnectionHandler> _mockConnectionHandler = null!;
    private BatchService _batchService = null!;

    /// <summary>
    /// Set up mocks before each test
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockConnectionHandler = new Mock<IPingenConnectionHandler>();
        _batchService = new BatchService(_mockConnectionHandler.Object);
    }

    /// <summary>
    /// Verifies GetPage calls ConnectionHandler with correct endpoint
    /// </summary>
    [Test]
    public async Task GetPage_CallsConnectionHandlerWithCorrectPath()
    {
        _mockConnectionHandler
            .Setup(x => x.GetAsync<CollectionResult<BatchData>>(
                "batches",
                It.IsAny<ApiPagingRequest?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult<CollectionResult<BatchData>> { IsSuccess = true });

        await _batchService.GetPage();

        _mockConnectionHandler.Verify(x => x.GetAsync<CollectionResult<BatchData>>(
            "batches",
            It.IsAny<ApiPagingRequest?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies Get calls ConnectionHandler with correct path including batch ID
    /// </summary>
    [Test]
    public async Task Get_CallsConnectionHandlerWithCorrectPath()
    {
        const string batchId = "test-batch-id";

        _mockConnectionHandler
            .Setup(x => x.GetAsync<SingleResult<BatchDataDetailed>>(
                $"batches/{batchId}",
                It.IsAny<ApiPagingRequest?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult<SingleResult<BatchDataDetailed>> { IsSuccess = true });

        await _batchService.Get(batchId);

        _mockConnectionHandler.Verify(x => x.GetAsync<SingleResult<BatchDataDetailed>>(
            $"batches/{batchId}",
            It.IsAny<ApiPagingRequest?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
