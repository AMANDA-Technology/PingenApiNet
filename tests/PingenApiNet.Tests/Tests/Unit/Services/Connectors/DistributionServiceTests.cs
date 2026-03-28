using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults.Embedded;
using PingenApiNet.Abstractions.Models.DeliveryProducts;
using PingenApiNet.Services.Connectors;

namespace PingenApiNet.Tests.Tests.Unit.Services.Connectors;

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

    private static DeliveryProductData CreateDeliveryProductData(string id) => new()
    {
        Id = id,
        Type = PingenApiDataType.delivery_products,
        Attributes = new DeliveryProduct(null, null, null, null, null, null, null)
    };
}
