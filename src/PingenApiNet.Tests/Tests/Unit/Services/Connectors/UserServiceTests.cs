using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.Relations;
using PingenApiNet.Abstractions.Models.Api.Embedded.Relations.Embedded;
using PingenApiNet.Abstractions.Models.Base.Embedded;
using PingenApiNet.Abstractions.Models.UserAssociations;
using PingenApiNet.Abstractions.Models.Users;
using PingenApiNet.Services.Connectors;

namespace PingenApiNet.Tests.Tests.Unit.Services.Connectors;

/// <summary>
/// Unit tests for <see cref="UserService"/>
/// </summary>
public class UserServiceTests
{
    private IPingenConnectionHandler _mockConnectionHandler = null!;
    private UserService _userService = null!;

    /// <summary>
    /// Set up mocks before each test
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockConnectionHandler = Substitute.For<IPingenConnectionHandler>();
        _userService = new UserService(_mockConnectionHandler);
    }

    /// <summary>
    /// Verifies Get calls ConnectionHandler with correct endpoint (no ID parameter)
    /// </summary>
    [Test]
    public async Task Get_CallsConnectionHandlerWithCorrectPath()
    {
        _mockConnectionHandler
            .GetAsync<SingleResult<UserDataDetailed>>(
                "user",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<SingleResult<UserDataDetailed>> { IsSuccess = true });

        await _userService.Get();

        await _mockConnectionHandler.Received(1).GetAsync<SingleResult<UserDataDetailed>>(
            "user",
            Arg.Any<ApiPagingRequest?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies GetAssociationsPage calls ConnectionHandler with correct endpoint
    /// </summary>
    [Test]
    public async Task GetAssociationsPage_CallsConnectionHandlerWithCorrectPath()
    {
        _mockConnectionHandler
            .GetAsync<CollectionResult<UserAssociationDataDetailed>>(
                "user/associations",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<UserAssociationDataDetailed>> { IsSuccess = true });

        await _userService.GetAssociationsPage();

        await _mockConnectionHandler.Received(1).GetAsync<CollectionResult<UserAssociationDataDetailed>>(
            "user/associations",
            Arg.Any<ApiPagingRequest?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies GetAssociationsPageResultsAsync calls ConnectionHandler and returns pages via auto-pagination
    /// </summary>
    [Test]
    public async Task GetAssociationsPageResultsAsync_CallsConnectionHandlerAndReturnsPages()
    {
        var associationData = CreateUserAssociationDataDetailed("assoc-1");

        _mockConnectionHandler
            .GetAsync<CollectionResult<UserAssociationDataDetailed>>(
                "user/associations",
                Arg.Any<ApiPagingRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<CollectionResult<UserAssociationDataDetailed>>
            {
                IsSuccess = true,
                Data = new CollectionResult<UserAssociationDataDetailed>(
                    [associationData],
                    new CollectionResultLinks("", "", "", "", ""),
                    new CollectionResultMeta(1, 1, 20, 1, 1, 1))
            });

        var pages = new List<IEnumerable<UserAssociationDataDetailed>>();
        await foreach (var page in _userService.GetAssociationsPageResultsAsync())
        {
            pages.Add(page);
        }

        pages.Count.ShouldBe(1);
        pages[0].First().Id.ShouldBe("assoc-1");
    }

    private static UserAssociationDataDetailed CreateUserAssociationDataDetailed(string id) => new()
    {
        Id = id,
        Type = PingenApiDataType.associations,
        Attributes = new UserAssociation(null, null, null, null),
        Relationships = new UserAssociationRelationships(new RelatedSingleOutput(new RelatedSingleLinks(""), new Abstractions.Models.Base.DataIdentity { Id = "", Type = PingenApiDataType.organisations })),
        Meta = new(new(new UserAssociationAbilities(PingenApiAbility.ok, PingenApiAbility.ok, PingenApiAbility.ok), new OrganisationAbilities(PingenApiAbility.ok)))
    };
}
