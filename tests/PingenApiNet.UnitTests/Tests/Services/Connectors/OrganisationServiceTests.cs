using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Exceptions;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.Relations;
using PingenApiNet.Abstractions.Models.Api.Embedded.Relations.Embedded;
using PingenApiNet.Abstractions.Models.Base.Embedded;
using PingenApiNet.Abstractions.Models.Organisations;
using PingenApiNet.Abstractions.Models.UserAssociations;
using PingenApiNet.Services.Connectors;

namespace PingenApiNet.UnitTests.Tests.Services.Connectors;

/// <summary>
/// Unit tests for <see cref="OrganisationService"/>
/// </summary>
public class OrganisationServiceTests
{
    private IPingenConnectionHandler _mockConnectionHandler = null!;
    private OrganisationService _organisationService = null!;

    /// <summary>
    /// Set up mocks before each test
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockConnectionHandler = Substitute.For<IPingenConnectionHandler>();
        _organisationService = new OrganisationService(_mockConnectionHandler);
    }

    /// <summary>
    /// Verifies GetPage calls ConnectionHandler with correct endpoint
    /// </summary>
    [Test]
    public async Task GetPage_CallsConnectionHandlerWithCorrectPath()
    {
        _mockConnectionHandler
            .GetAsync<CollectionResult<OrganisationData>>(
                "organisations",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<OrganisationData>> { IsSuccess = true });

        await _organisationService.GetPage();

        await _mockConnectionHandler.Received(1).GetAsync<CollectionResult<OrganisationData>>(
            "organisations",
            Arg.Any<ApiPagingRequest?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies GetPageResultsAsync calls ConnectionHandler and returns pages via auto-pagination
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_CallsConnectionHandlerAndReturnsPages()
    {
        var organisationData = CreateOrganisationData("org-1");

        _mockConnectionHandler
            .GetAsync<CollectionResult<OrganisationData>>(
                "organisations",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<OrganisationData>>
            {
                IsSuccess = true,
                Data = new CollectionResult<OrganisationData>(
                    [organisationData],
                    new CollectionResultLinks("", "", "", "", ""),
                    new CollectionResultMeta(1, 1, 20, 1, 1, 1))
            });

        var pages = new List<IEnumerable<OrganisationData>>();
        await foreach (var page in _organisationService.GetPageResultsAsync())
        {
            pages.Add(page);
        }

        pages.Count.ShouldBe(1);
        pages[0].First().Id.ShouldBe("org-1");
    }

    /// <summary>
    /// Verifies Get calls ConnectionHandler with correct path including organisation ID
    /// </summary>
    [Test]
    public async Task Get_CallsConnectionHandlerWithCorrectPath()
    {
        const string organisationId = "test-org-id";

        _mockConnectionHandler
            .GetAsync<SingleResult<OrganisationDataDetailed>>(
                $"organisations/{organisationId}",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<OrganisationDataDetailed>> { IsSuccess = true });

        await _organisationService.Get(organisationId);

        await _mockConnectionHandler.Received(1).GetAsync<SingleResult<OrganisationDataDetailed>>(
            $"organisations/{organisationId}",
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
            .GetAsync<CollectionResult<OrganisationData>>(
                "organisations",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<OrganisationData>>
            {
                IsSuccess = false,
                ApiError = apiError
            });

        var result = await _organisationService.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldBe(apiError),
            () => result.Data.ShouldBeNull()
        );
    }

    /// <summary>
    /// Verifies GetPage forwards the supplied paging request through to the connection handler
    /// </summary>
    [Test]
    public async Task GetPage_WithPagingRequest_ForwardsRequestToConnectionHandler()
    {
        var pagingRequest = new ApiPagingRequest { PageNumber = 3, PageLimit = 25 };

        _mockConnectionHandler
            .GetAsync<CollectionResult<OrganisationData>>(
                "organisations",
                pagingRequest,
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<OrganisationData>> { IsSuccess = true });

        await _organisationService.GetPage(pagingRequest);

        await _mockConnectionHandler.Received(1).GetAsync<CollectionResult<OrganisationData>>(
            "organisations",
            pagingRequest,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies GetPageResultsAsync surfaces an API failure as a <see cref="PingenApiErrorException"/>
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_ApiError_ThrowsPingenApiErrorException()
    {
        _mockConnectionHandler
            .GetAsync<CollectionResult<OrganisationData>>(
                "organisations",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<OrganisationData>>
            {
                IsSuccess = false,
                ApiError = CreateApiError("server_error", "boom")
            });

        await Should.ThrowAsync<PingenApiErrorException>(async () =>
        {
            await foreach (var _ in _organisationService.GetPageResultsAsync())
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
        const string organisationId = "missing-org";
        var apiError = CreateApiError("not_found", "Organisation not found");
        _mockConnectionHandler
            .GetAsync<SingleResult<OrganisationDataDetailed>>(
                $"organisations/{organisationId}",
                Arg.Any<ApiRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<OrganisationDataDetailed>>
            {
                IsSuccess = false,
                ApiError = apiError
            });

        var result = await _organisationService.Get(organisationId);

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
            .GetAsync<SingleResult<OrganisationDataDetailed>>(
                "organisations/",
                Arg.Any<ApiRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<OrganisationDataDetailed>> { IsSuccess = true });

        await _organisationService.Get(string.Empty);

        await _mockConnectionHandler.Received(1).GetAsync<SingleResult<OrganisationDataDetailed>>(
            "organisations/",
            Arg.Any<ApiRequest?>(),
            Arg.Any<CancellationToken>());
    }

    private static OrganisationData CreateOrganisationData(string id) => new()
    {
        Id = id,
        Type = PingenApiDataType.organisations,
        Attributes = new Organisation(null, null, null, null, null, null, null, null, null, null, null, null, null),
        Relationships = new OrganisationRelationships(new RelatedManyOutput(new RelatedManyLinks(new RelatedManyLinkInfo("", new RelatedManyLinkMeta(0)))))
    };

    private static ApiError CreateApiError(string code, string detail) => new(
    [
        new ApiErrorData(code, "Error", detail, new ApiErrorSource(string.Empty, string.Empty))
    ]);
}
