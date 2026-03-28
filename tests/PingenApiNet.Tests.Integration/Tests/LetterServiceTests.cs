using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Enums.Letters;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.LetterPrices;
using PingenApiNet.Abstractions.Models.Letters.Views;
using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace PingenApiNet.Tests.Integration.Tests;

/// <summary>
/// Integration tests for <see cref="ILetterService"/>.
/// </summary>
[TestFixture]
public sealed class LetterServiceTests : IntegrationTestBase
{
    /// <summary>
    /// Builds a valid letter item tuple for collection stubs.
    /// </summary>
    private static (string Id, object Attributes, object? Relationships, object? Meta) LetterItem(string id)
    {
        return (id,
            new
            {
                status = "valid",
                file_original_name = "test.pdf",
                file_pages = 2,
                address = "Test Address",
                address_position = "left",
                country = "CH",
                delivery_product = "cheap",
                print_mode = "simplex",
                print_spectrum = "grayscale",
                price_currency = "CHF",
                price_value = 1.50
            },
            (object)new
            {
                organisation = JsonApiStubHelper.RelatedSingle(TestOrganisationId, "organisations"),
                events = JsonApiStubHelper.RelatedMany(),
                batch = JsonApiStubHelper.RelatedSingle("batch-001", "batches")
            },
            null);
    }

    /// <summary>
    /// Builds a valid detailed letter response body with meta abilities.
    /// </summary>
    private static string DetailedLetterResponse(string id)
    {
        return JsonApiStubHelper.SingleResponse(
            id,
            "letters",
            new
            {
                status = "valid",
                file_original_name = "detailed.pdf",
                file_pages = 3,
                address = "Detailed Address",
                address_position = "left",
                country = "CH",
                delivery_product = "cheap",
                print_mode = "simplex",
                print_spectrum = "grayscale",
                price_currency = "CHF",
                price_value = 2.50
            },
            relationships: new
            {
                organisation = JsonApiStubHelper.RelatedSingle(TestOrganisationId, "organisations"),
                events = JsonApiStubHelper.RelatedMany(),
                batch = JsonApiStubHelper.RelatedSingle("batch-001", "batches")
            },
            meta: JsonApiStubHelper.MetaWithAbilities(new
            {
                cancel = "ok",
                delete = "ok",
                submit = "ok",
                send_simplex = "ok",
                edit = "ok",
                get_pdf_raw = "ok",
                get_pdf_validation = "ok",
                change_paper_type = "ok",
                change_window_position = "ok",
                create_coverpage = "ok",
                fix_overwrite_restricted_areas = "ok",
                fix_coverpage = "ok",
                fix_regular_paper = "ok"
            }));
    }

    /// <summary>
    /// Builds a valid letter event item tuple for collection stubs.
    /// </summary>
    private static (string Id, object Attributes, object? Relationships, object? Meta) EventItem(string id, string code)
    {
        return (id,
            new
            {
                code,
                name = $"Event {code}",
                producer = "PostAG",
                location = "Zurich",
                has_image = false,
                data = Array.Empty<string>(),
                emitted_at = "2026-03-01T10:00:00Z",
                created_at = "2026-03-01T10:00:00Z",
                updated_at = "2026-03-01T10:00:00Z"
            },
            (object)new
            {
                letter = JsonApiStubHelper.RelatedSingle("letter-001", "letters")
            },
            null);
    }

    /// <summary>
    /// Verifies that GetPage returns a paginated list of letters.
    /// </summary>
    [Test]
    public async Task GetPage_ShouldReturnLetters()
    {
        var letterId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath("letters"))
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [LetterItem(letterId)],
                    "letters")));

        var result = await Client.Letters.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(1),
            () => result.Data!.Data[0].Id.ShouldBe(letterId),
            () => result.Data!.Data[0].Attributes.FileOriginalName.ShouldBe("test.pdf"));
    }

    /// <summary>
    /// Verifies that GetPageResultsAsync auto-paginates across multiple pages.
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_ShouldAutoPaginate()
    {
        var id1 = Guid.NewGuid().ToString();
        var id2 = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath("letters"))
                .UsingGet())
            .InScenario("letters-paging")
            .WillSetStateTo("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [LetterItem(id1)],
                    "letters",
                    currentPage: 1, lastPage: 2, perPage: 1, total: 2)));

        Server
            .Given(Request.Create()
                .WithPath(OrgPath("letters"))
                .UsingGet())
            .InScenario("letters-paging")
            .WhenStateIs("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [LetterItem(id2)],
                    "letters",
                    currentPage: 2, lastPage: 2, perPage: 1, total: 2)));

        var allItems = new List<string>();
        await foreach (var page in Client.Letters.GetPageResultsAsync())
        {
            allItems.AddRange(page.Select(item => item.Id));
        }

        allItems.ShouldSatisfyAllConditions(
            () => allItems.Count.ShouldBe(2),
            () => allItems.ShouldContain(id1),
            () => allItems.ShouldContain(id2));
    }

    /// <summary>
    /// Verifies that Create posts a new letter and returns the created resource.
    /// </summary>
    [Test]
    public async Task Create_ShouldPostLetterAndReturnResult()
    {
        var letterId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath("letters"))
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(201)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(DetailedLetterResponse(letterId)));

        var data = new DataPost<LetterCreate, LetterCreateRelationships>
        {
            Type = PingenApiDataType.letters,
            Attributes = new LetterCreate
            {
                FileOriginalName = "new-letter.pdf",
                FileUrl = "https://example.com/files/new-letter.pdf",
                FileUrlSignature = "sig-abc-123",
                AddressPosition = LetterAddressPosition.left,
                AutoSend = false,
                DeliveryProduct = "cheap",
                PrintMode = LetterPrintMode.simplex,
                PrintSpectrum = LetterPrintSpectrum.grayscale
            },
            Relationships = LetterCreateRelationships.Create("preset-id-001")
        };

        var result = await Client.Letters.Create(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(letterId),
            () => result.Data!.Data.Attributes.FileOriginalName.ShouldBe("detailed.pdf"));
    }

    /// <summary>
    /// Verifies that Send patches a letter with delivery options.
    /// </summary>
    [Test]
    public async Task Send_ShouldPatchLetterAndReturnResult()
    {
        var letterId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath($"letters/{letterId}/send"))
                .UsingPatch())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(DetailedLetterResponse(letterId)));

        var data = new DataPatch<LetterSend>
        {
            Id = letterId,
            Type = PingenApiDataType.letters,
            Attributes = new LetterSend
            {
                DeliveryProduct = "cheap",
                PrintMode = LetterPrintMode.simplex,
                PrintSpectrum = LetterPrintSpectrum.grayscale
            }
        };

        var result = await Client.Letters.Send(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(letterId));
    }

    /// <summary>
    /// Verifies that Cancel patches a letter to cancel it.
    /// </summary>
    [Test]
    public async Task Cancel_ShouldPatchLetterCancel()
    {
        var letterId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath($"letters/{letterId}/cancel"))
                .UsingPatch())
            .RespondWith(Response.Create()
                .WithStatusCode(204)
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString()));

        var result = await Client.Letters.Cancel(letterId);

        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// Verifies that Get returns a single letter with detailed attributes.
    /// </summary>
    [Test]
    public async Task Get_ShouldReturnSingleLetter()
    {
        var letterId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath($"letters/{letterId}"))
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(DetailedLetterResponse(letterId)));

        var result = await Client.Letters.Get(letterId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(letterId),
            () => result.Data!.Data.Attributes.FileOriginalName.ShouldBe("detailed.pdf"),
            () => result.Data!.Data.Attributes.FilePages.ShouldBe(3));
    }

    /// <summary>
    /// Verifies that Delete removes a letter and returns success.
    /// </summary>
    [Test]
    public async Task Delete_ShouldReturnSuccess()
    {
        var letterId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath($"letters/{letterId}"))
                .UsingDelete())
            .RespondWith(Response.Create()
                .WithStatusCode(204)
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString()));

        var result = await Client.Letters.Delete(letterId);

        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// Verifies that Update patches letter attributes and returns the updated resource.
    /// </summary>
    [Test]
    public async Task Update_ShouldPatchLetterAndReturnResult()
    {
        var letterId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath($"letters/{letterId}"))
                .UsingPatch())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(DetailedLetterResponse(letterId)));

        var data = new DataPatch<LetterUpdate>
        {
            Id = letterId,
            Type = PingenApiDataType.letters,
            Attributes = new LetterUpdate
            {
                PaperTypes = ["normal"]
            }
        };

        var result = await Client.Letters.Update(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(letterId));
    }

    /// <summary>
    /// Verifies that GetFileLocation returns a 302 redirect with a Location header.
    /// </summary>
    [Test]
    public async Task GetFileLocation_ShouldReturn302WithLocation()
    {
        var letterId = Guid.NewGuid().ToString();
        var fileUrl = "https://s3.example.com/files/letter.pdf";

        Server
            .Given(Request.Create()
                .WithPath(OrgPath($"letters/{letterId}/file"))
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(302)
                .WithHeader("Location", fileUrl)
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString()));

        var result = await Client.Letters.GetFileLocation(letterId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Location.ShouldNotBeNull(),
            () => result.Location!.ToString().ShouldBe(fileUrl));
    }

    /// <summary>
    /// Verifies that DownloadFileContent retrieves file content from an external URL.
    /// </summary>
    [Test]
    public async Task DownloadFileContent_ShouldReturnFileStream()
    {
        var downloadPath = "/files/download/test-letter.pdf";
        var fileContent = "fake-pdf-binary-content"u8.ToArray();

        Server
            .Given(Request.Create()
                .WithPath(downloadPath)
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/pdf")
                .WithBody(fileContent));

        var fileUrl = new Uri($"{Server.Url}{downloadPath}");
        using var stream = await Client.Letters.DownloadFileContent(fileUrl);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var downloadedContent = memoryStream.ToArray();

        downloadedContent.ShouldBe(fileContent);
    }

    /// <summary>
    /// Verifies that CalculatePrice posts a price configuration and returns the price.
    /// </summary>
    [Test]
    public async Task CalculatePrice_ShouldReturnLetterPrice()
    {
        var priceId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath("letters/price-calculator"))
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.SingleResponse(
                    priceId,
                    "letter_price_calculator",
                    new { currency = "CHF", price = 1.50 })));

        var data = new DataPost<LetterPriceConfiguration>
        {
            Type = PingenApiDataType.letter_price_calculator,
            Attributes = new LetterPriceConfiguration
            {
                Country = "CH",
                PaperTypes = ["normal"],
                PrintMode = LetterPrintMode.simplex,
                PrintSpectrum = LetterPrintSpectrum.grayscale,
                DeliveryProduct = "cheap"
            }
        };

        var result = await Client.Letters.CalculatePrice(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(priceId),
            () => result.Data!.Data.Attributes.Price.ShouldBe(1.50m));
    }

    /// <summary>
    /// Verifies that GetEventsPage returns letter events with language parameter.
    /// </summary>
    [Test]
    public async Task GetEventsPage_ShouldReturnLetterEvents()
    {
        var letterId = Guid.NewGuid().ToString();
        var eventId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath($"letters/{letterId}/events"))
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [EventItem(eventId, "submitted")],
                    "letters_events")));

        var result = await Client.Letters.GetEventsPage(letterId, "en-GB");

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(1),
            () => result.Data!.Data[0].Id.ShouldBe(eventId),
            () => result.Data!.Data[0].Attributes.Code.ShouldBe("submitted"));
    }

    /// <summary>
    /// Verifies that GetEventsPageResultsAsync auto-paginates letter events.
    /// </summary>
    [Test]
    public async Task GetEventsPageResultsAsync_ShouldAutoPaginate()
    {
        var letterId = Guid.NewGuid().ToString();
        var eventId1 = Guid.NewGuid().ToString();
        var eventId2 = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath($"letters/{letterId}/events"))
                .UsingGet())
            .InScenario("letter-events-paging")
            .WillSetStateTo("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [EventItem(eventId1, "submitted")],
                    "letters_events",
                    currentPage: 1, lastPage: 2, perPage: 1, total: 2)));

        Server
            .Given(Request.Create()
                .WithPath(OrgPath($"letters/{letterId}/events"))
                .UsingGet())
            .InScenario("letter-events-paging")
            .WhenStateIs("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [EventItem(eventId2, "delivered")],
                    "letters_events",
                    currentPage: 2, lastPage: 2, perPage: 1, total: 2)));

        var allItems = new List<string>();
        await foreach (var page in Client.Letters.GetEventsPageResultsAsync(letterId, "en-GB"))
        {
            allItems.AddRange(page.Select(item => item.Id));
        }

        allItems.ShouldSatisfyAllConditions(
            () => allItems.Count.ShouldBe(2),
            () => allItems.ShouldContain(eventId1),
            () => allItems.ShouldContain(eventId2));
    }

    /// <summary>
    /// Verifies that GetIssuesPage returns letter issues with language parameter.
    /// </summary>
    [Test]
    public async Task GetIssuesPage_ShouldReturnLetterIssues()
    {
        var issueId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath("letters/issues"))
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [EventItem(issueId, "issue_detected")],
                    "letters_events")));

        var result = await Client.Letters.GetIssuesPage("en-GB");

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(1),
            () => result.Data!.Data[0].Id.ShouldBe(issueId),
            () => result.Data!.Data[0].Attributes.Code.ShouldBe("issue_detected"));
    }

    /// <summary>
    /// Verifies that GetIssuesPageResultsAsync auto-paginates letter issues.
    /// </summary>
    [Test]
    public async Task GetIssuesPageResultsAsync_ShouldAutoPaginate()
    {
        var issueId1 = Guid.NewGuid().ToString();
        var issueId2 = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath("letters/issues"))
                .UsingGet())
            .InScenario("letter-issues-paging")
            .WillSetStateTo("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [EventItem(issueId1, "issue_detected")],
                    "letters_events",
                    currentPage: 1, lastPage: 2, perPage: 1, total: 2)));

        Server
            .Given(Request.Create()
                .WithPath(OrgPath("letters/issues"))
                .UsingGet())
            .InScenario("letter-issues-paging")
            .WhenStateIs("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [EventItem(issueId2, "issue_resolved")],
                    "letters_events",
                    currentPage: 2, lastPage: 2, perPage: 1, total: 2)));

        var allItems = new List<string>();
        await foreach (var page in Client.Letters.GetIssuesPageResultsAsync("en-GB"))
        {
            allItems.AddRange(page.Select(item => item.Id));
        }

        allItems.ShouldSatisfyAllConditions(
            () => allItems.Count.ShouldBe(2),
            () => allItems.ShouldContain(issueId1),
            () => allItems.ShouldContain(issueId2));
    }
}
