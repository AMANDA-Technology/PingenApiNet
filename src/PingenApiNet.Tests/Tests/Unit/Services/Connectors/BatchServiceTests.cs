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
    private IPingenConnectionHandler _mockConnectionHandler = null!;
    private BatchService _batchService = null!;

    /// <summary>
    /// Set up mocks before each test
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockConnectionHandler = Substitute.For<IPingenConnectionHandler>();
        _batchService = new BatchService(_mockConnectionHandler);
    }

    /// <summary>
    /// Verifies GetPage calls ConnectionHandler with correct endpoint
    /// </summary>
    [Test]
    public async Task GetPage_CallsConnectionHandlerWithCorrectPath()
    {
        _mockConnectionHandler
            .GetAsync<CollectionResult<BatchData>>(
                "batches",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<BatchData>> { IsSuccess = true });

        await _batchService.GetPage();

        await _mockConnectionHandler.Received(1).GetAsync<CollectionResult<BatchData>>(
            "batches",
            Arg.Any<ApiPagingRequest?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies Get calls ConnectionHandler with correct path including batch ID
    /// </summary>
    [Test]
    public async Task Get_CallsConnectionHandlerWithCorrectPath()
    {
        const string batchId = "test-batch-id";

        _mockConnectionHandler
            .GetAsync<SingleResult<BatchDataDetailed>>(
                $"batches/{batchId}",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<BatchDataDetailed>> { IsSuccess = true });

        await _batchService.Get(batchId);

        await _mockConnectionHandler.Received(1).GetAsync<SingleResult<BatchDataDetailed>>(
            $"batches/{batchId}",
            Arg.Any<ApiPagingRequest?>(),
            Arg.Any<CancellationToken>());
    }
}
