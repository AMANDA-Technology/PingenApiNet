using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Exceptions;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults.Embedded;
using PingenApiNet.Abstractions.Models.DeliveryProducts;
using PingenApiNet.Services.Connectors;

namespace PingenApiNet.UnitTests.Tests.Services.Connectors;

/// <summary>
/// Unit tests for <see cref="DistributionService"/>
/// </summary>
public class DistributionServiceTests
{
    private IPingenConnectionHandler _mockConnectionHandler = null!;
    private DistributionService _distributionService = null!;

    /// <summary>
    /// Set up mocks before each test
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockConnectionHandler = Substitute.For<IPingenConnectionHandler>();
        _distributionService = new DistributionService(_mockConnectionHandler);
    }

    /// <summary>
    /// Verifies GetDeliveryProductsPage calls ConnectionHandler with correct endpoint
    /// </summary>
    [Test]
    public async Task GetDeliveryProductsPage_CallsConnectionHandlerWithCorrectPath()
    {
        _mockConnectionHandler
            .GetAsync<CollectionResult<DeliveryProductData>>(
                "distribution/delivery-products",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<DeliveryProductData>> { IsSuccess = true });

        await _distributionService.GetDeliveryProductsPage();

        await _mockConnectionHandler.Received(1).GetAsync<CollectionResult<DeliveryProductData>>(
            "distribution/delivery-products",
            Arg.Any<ApiPagingRequest?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies GetDeliveryProductsPage propagates failure ApiResult without throwing when the API returns an error
    /// </summary>
    [Test]
    public async Task GetDeliveryProductsPage_ApiError_ReturnsFailureResult()
    {
        var apiError = CreateApiError("server_error", "Service unavailable");
        _mockConnectionHandler
            .GetAsync<CollectionResult<DeliveryProductData>>(
                "distribution/delivery-products",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<DeliveryProductData>>
            {
                IsSuccess = false,
                ApiError = apiError
            });

        var result = await _distributionService.GetDeliveryProductsPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldBe(apiError),
            () => result.Data.ShouldBeNull()
        );
    }

    /// <summary>
    /// Verifies GetDeliveryProductsPage forwards the supplied paging request through to the connection handler
    /// </summary>
    [Test]
    public async Task GetDeliveryProductsPage_WithPagingRequest_ForwardsRequestToConnectionHandler()
    {
        var pagingRequest = new ApiPagingRequest { PageNumber = 2, PageLimit = 50 };

        _mockConnectionHandler
            .GetAsync<CollectionResult<DeliveryProductData>>(
                "distribution/delivery-products",
                pagingRequest,
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<DeliveryProductData>> { IsSuccess = true });

        await _distributionService.GetDeliveryProductsPage(pagingRequest);

        await _mockConnectionHandler.Received(1).GetAsync<CollectionResult<DeliveryProductData>>(
            "distribution/delivery-products",
            pagingRequest,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies GetDeliveryProductsPageResultsAsync calls ConnectionHandler and returns pages via auto-pagination
    /// </summary>
    [Test]
    public async Task GetDeliveryProductsPageResultsAsync_CallsConnectionHandlerAndReturnsPages()
    {
        var deliveryProductData = CreateDeliveryProductData("product-1");

        _mockConnectionHandler
            .GetAsync<CollectionResult<DeliveryProductData>>(
                "distribution/delivery-products",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<DeliveryProductData>>
            {
                IsSuccess = true,
                Data = new CollectionResult<DeliveryProductData>(
                    [deliveryProductData],
                    new CollectionResultLinks("", "", "", "", ""),
                    new CollectionResultMeta(1, 1, 20, 1, 1, 1))
            });

        var pages = new List<IEnumerable<DeliveryProductData>>();
        await foreach (var page in _distributionService.GetDeliveryProductsPageResultsAsync())
        {
            pages.Add(page);
        }

        pages.Count.ShouldBe(1);
        pages[0].First().Id.ShouldBe("product-1");
    }

    /// <summary>
    /// Verifies GetDeliveryProductsPageResultsAsync surfaces an API failure as a <see cref="PingenApiErrorException"/>
    /// </summary>
    [Test]
    public async Task GetDeliveryProductsPageResultsAsync_ApiError_ThrowsPingenApiErrorException()
    {
        _mockConnectionHandler
            .GetAsync<CollectionResult<DeliveryProductData>>(
                "distribution/delivery-products",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<DeliveryProductData>>
            {
                IsSuccess = false,
                ApiError = CreateApiError("server_error", "boom")
            });

        await Should.ThrowAsync<PingenApiErrorException>(async () =>
        {
            await foreach (var _ in _distributionService.GetDeliveryProductsPageResultsAsync())
            {
                // consume pages
            }
        });
    }

    private static DeliveryProductData CreateDeliveryProductData(string id) => new()
    {
        Id = id,
        Type = PingenApiDataType.delivery_products,
        Attributes = new DeliveryProduct(null, null, null, null, null, null, null)
    };

    private static ApiError CreateApiError(string code, string detail) => new(
    [
        new ApiErrorData(code, "Error", detail, new ApiErrorSource(string.Empty, string.Empty))
    ]);
}
