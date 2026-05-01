/*
MIT License

Copyright (c) 2022 Philip Näf <philip.naef@amanda-technology.ch>
Copyright (c) 2022 Manuel Gysin <manuel.gysin@amanda-technology.ch>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Bogus;
using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Enums.Letters;
using PingenApiNet.Abstractions.Exceptions;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.LetterEvents;
using PingenApiNet.Abstractions.Models.LetterPrices;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Letters.Views;
using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace PingenApiNet.Tests.Integration.Tests;

/// <summary>
///     Integration tests for <see cref="ILetterService" />.
/// </summary>
[TestFixture]
public sealed class LetterServiceTests : IntegrationTestBase
{
    /// <summary>
    ///     Builds a Bogus faker for letter create payloads.
    /// </summary>
    private static Faker<DataPost<LetterCreate, LetterCreateRelationships>> LetterCreateFaker()
    {
        return new Faker<DataPost<LetterCreate, LetterCreateRelationships>>()
            .RuleFor(x => x.Type, PingenApiDataType.letters)
            .RuleFor(x => x.Attributes,
                f => new LetterCreate
                {
                    FileOriginalName = f.System.FileName("pdf"),
                    FileUrl = f.Internet.Url(),
                    FileUrlSignature = f.Random.Hash(60),
                    AddressPosition = f.PickRandom<LetterAddressPosition>(),
                    AutoSend = f.Random.Bool(),
                    DeliveryProduct = f.PickRandom(
                        LetterCreateDeliveryProduct.Cheap,
                        LetterCreateDeliveryProduct.Fast,
                        LetterCreateDeliveryProduct.Bulk,
                        LetterCreateDeliveryProduct.Premium),
                    PrintMode = f.PickRandom<LetterPrintMode>(),
                    PrintSpectrum = f.PickRandom<LetterPrintSpectrum>()
                })
            .RuleFor(x => x.Relationships, f => LetterCreateRelationships.Create(f.Random.Guid().ToString()));
    }

    /// <summary>
    ///     Builds a Bogus faker for letter send payloads.
    /// </summary>
    private static Faker<DataPatch<LetterSend>> LetterSendFaker(string letterId)
    {
        return new Faker<DataPatch<LetterSend>>()
            .RuleFor(x => x.Id, letterId)
            .RuleFor(x => x.Type, PingenApiDataType.letters)
            .RuleFor(x => x.Attributes,
                f => new LetterSend
                {
                    DeliveryProduct = f.PickRandom("cheap", "fast", "bulk", "premium"),
                    PrintMode = f.PickRandom<LetterPrintMode>(),
                    PrintSpectrum = f.PickRandom<LetterPrintSpectrum>()
                });
    }

    /// <summary>
    ///     Builds a Bogus faker for letter update payloads.
    /// </summary>
    private static Faker<DataPatch<LetterUpdate>> LetterUpdateFaker(string letterId)
    {
        return new Faker<DataPatch<LetterUpdate>>()
            .RuleFor(x => x.Id, letterId)
            .RuleFor(x => x.Type, PingenApiDataType.letters)
            .RuleFor(x => x.Attributes,
                _ => new LetterUpdate { PaperTypes = [LetterPaperTypes.Normal] });
    }

    /// <summary>
    ///     Builds a Bogus faker for letter price-configuration payloads.
    /// </summary>
    private static Faker<DataPost<LetterPriceConfiguration>> LetterPriceConfigurationFaker()
    {
        return new Faker<DataPost<LetterPriceConfiguration>>()
            .RuleFor(x => x.Type, PingenApiDataType.letter_price_calculator)
            .RuleFor(x => x.Attributes,
                f => new LetterPriceConfiguration
                {
                    Country = f.PickRandom("CH", "DE", "AT"),
                    PaperTypes = [LetterPaperTypes.Normal],
                    PrintMode = f.PickRandom<LetterPrintMode>(),
                    PrintSpectrum = f.PickRandom<LetterPrintSpectrum>(),
                    DeliveryProduct = f.PickRandom("cheap", "fast", "bulk", "premium")
                });
    }

    // ── GetPage ──────────────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that GetPage returns a paginated list of letters.
    /// </summary>
    [Test]
    public async Task GetPage_ShouldReturnLetters()
    {
        Server.StubJsonGet(OrgPath("letters"), PingenResponseFactory.LetterCollection());

        ApiResult<CollectionResult<LetterData>> result = await Client.Letters.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(3),
            () => result.Data!.Data[0].Attributes.FileOriginalName.ShouldNotBeNullOrEmpty());
    }

    /// <summary>
    ///     Verifies that GetPage returns an empty collection when no letters exist.
    /// </summary>
    [Test]
    public async Task GetPage_ShouldReturnEmptyWhenNoLetters()
    {
        Server.StubJsonGet(OrgPath("letters"), PingenResponseFactory.LetterCollection(0));

        ApiResult<CollectionResult<LetterData>> result = await Client.Letters.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(0));
    }

    /// <summary>
    ///     Verifies that GetPage exposes pagination meta on a multi-page response.
    /// </summary>
    [Test]
    public async Task GetPage_ShouldExposePaginationMeta_ForMultiPageResponse()
    {
        Server.StubJsonGet(
            OrgPath("letters"),
            PingenResponseFactory.LetterCollection(5, currentPage: 2, lastPage: 3));

        ApiResult<CollectionResult<LetterData>> result = await Client.Letters.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Meta.CurrentPage.ShouldBe(2),
            () => result.Data!.Meta.LastPage.ShouldBe(3),
            () => result.Data!.Data.Count.ShouldBe(5));
    }

    /// <summary>
    ///     Verifies that GetPage returns an unsuccessful ApiResult on API errors.
    /// </summary>
    [TestCase(401)]
    [TestCase(403)]
    [TestCase(500)]
    [TestCase(503)]
    public async Task GetPage_OnApiError_ShouldReturnUnsuccessfulApiResult(int statusCode)
    {
        Server.StubError(
            OrgPath("letters"),
            "GET",
            PingenResponseFactory.ErrorResponse(status: statusCode.ToString()),
            statusCode);

        ApiResult<CollectionResult<LetterData>> result = await Client.Letters.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors.Count.ShouldBeGreaterThan(0),
            () => result.Data.ShouldBeNull());
    }

    // ── GetPageResultsAsync ──────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that GetPageResultsAsync auto-paginates across two pages.
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_ShouldAutoPaginate()
    {
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
                .WithBody(PingenResponseFactory.LetterCollection(1, currentPage: 1, lastPage: 2)));

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
                .WithBody(PingenResponseFactory.LetterCollection(1, currentPage: 2, lastPage: 2)));

        var allItems = new List<string>();
        await foreach (IEnumerable<LetterData> page in Client.Letters.GetPageResultsAsync())
            allItems.AddRange(page.Select(item => item.Id));

        allItems.Count.ShouldBe(2);
    }

    /// <summary>
    ///     Verifies that GetPageResultsAsync stops after a single page when only one exists.
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_ShouldYieldSinglePage_WhenOnlyOneExists()
    {
        Server.StubJsonGet(OrgPath("letters"), PingenResponseFactory.LetterCollection(2));

        var allItems = new List<string>();
        await foreach (IEnumerable<LetterData> page in Client.Letters.GetPageResultsAsync())
            allItems.AddRange(page.Select(item => item.Id));

        allItems.Count.ShouldBe(2);
    }

    /// <summary>
    ///     Verifies that GetPageResultsAsync surfaces a <see cref="PingenApiErrorException" /> when the underlying call
    ///     fails.
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_OnApiError_ShouldThrowPingenApiErrorException()
    {
        Server.StubError(
            OrgPath("letters"),
            "GET",
            PingenResponseFactory.ErrorResponse("Server Error", "Service unavailable", "500"),
            500);

        PingenApiErrorException exception = await Should.ThrowAsync<PingenApiErrorException>(async () =>
        {
            await foreach (IEnumerable<LetterData> _ in Client.Letters.GetPageResultsAsync())
            {
                // Should never iterate
            }
        });

        exception.ApiResult.ShouldNotBeNull();
        exception.ApiResult!.IsSuccess.ShouldBeFalse();
    }

    // ── Get ──────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that Get returns a single letter with detailed attributes.
    /// </summary>
    [Test]
    public async Task Get_ShouldReturnSingleLetter()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubJsonGet(OrgPath($"letters/{letterId}"), PingenResponseFactory.SingleLetter(letterId));

        ApiResult<SingleResult<LetterDataDetailed>> result = await Client.Letters.Get(letterId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(letterId),
            () => result.Data!.Data.Attributes.FileOriginalName.ShouldNotBeNullOrEmpty());
    }

    /// <summary>
    ///     Verifies that Get surfaces a 404 not-found error via ApiResult.
    /// </summary>
    [Test]
    public async Task Get_ShouldReturnErrorWhenNotFound()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubError(
            OrgPath($"letters/{letterId}"),
            "GET",
            PingenResponseFactory.ErrorResponse("Not Found", "The letter does not exist.", "404"),
            404);

        ApiResult<SingleResult<LetterDataDetailed>> result = await Client.Letters.Get(letterId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors[0].Title.ShouldBe("Not Found"),
            () => result.Data.ShouldBeNull());
    }

    // ── Create ───────────────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that Create posts a new letter and returns the created resource.
    /// </summary>
    [Test]
    public async Task Create_ShouldPostLetterAndReturnResult()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubJsonPost(OrgPath("letters"), PingenResponseFactory.SingleLetter(letterId));

        DataPost<LetterCreate, LetterCreateRelationships>? data = LetterCreateFaker().Generate();

        ApiResult<SingleResult<LetterDataDetailed>> result = await Client.Letters.Create(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(letterId),
            () => result.Data!.Data.Attributes.FileOriginalName.ShouldNotBeNullOrEmpty());
    }

    /// <summary>
    ///     Verifies that Create surfaces a validation error returned by the API.
    /// </summary>
    [Test]
    public async Task Create_ShouldReturnErrorWhenValidationFails()
    {
        Server.StubError(
            OrgPath("letters"),
            "POST",
            PingenResponseFactory.ErrorResponse("Validation Failed", "FileUrl is required."));

        DataPost<LetterCreate, LetterCreateRelationships>? data = LetterCreateFaker().Generate();

        ApiResult<SingleResult<LetterDataDetailed>> result = await Client.Letters.Create(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors[0].Title.ShouldBe("Validation Failed"),
            () => result.Data.ShouldBeNull());
    }

    // ── Send ─────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that Send patches a letter with delivery options and returns the updated resource.
    /// </summary>
    [Test]
    public async Task Send_ShouldPatchLetterAndReturnResult()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubJsonPatch(OrgPath($"letters/{letterId}/send"), PingenResponseFactory.SingleLetter(letterId));

        DataPatch<LetterSend>? data = LetterSendFaker(letterId).Generate();

        ApiResult<SingleResult<LetterDataDetailed>> result = await Client.Letters.Send(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(letterId));
    }

    /// <summary>
    ///     Verifies that Send surfaces a 422 already-sent error via ApiResult.
    /// </summary>
    [Test]
    public async Task Send_ShouldReturnErrorWhenAlreadySent()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubError(
            OrgPath($"letters/{letterId}/send"),
            "PATCH",
            PingenResponseFactory.ErrorResponse("Conflict", "Letter has already been sent."));

        DataPatch<LetterSend>? data = LetterSendFaker(letterId).Generate();

        ApiResult<SingleResult<LetterDataDetailed>> result = await Client.Letters.Send(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors[0].Title.ShouldBe("Conflict"),
            () => result.Data.ShouldBeNull());
    }

    /// <summary>
    ///     Verifies that Send surfaces a 404 not-found error via ApiResult.
    /// </summary>
    [Test]
    public async Task Send_ShouldReturnErrorWhenNotFound()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubError(
            OrgPath($"letters/{letterId}/send"),
            "PATCH",
            PingenResponseFactory.ErrorResponse("Not Found", "The letter does not exist.", "404"),
            404);

        DataPatch<LetterSend>? data = LetterSendFaker(letterId).Generate();

        ApiResult<SingleResult<LetterDataDetailed>> result = await Client.Letters.Send(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors[0].Title.ShouldBe("Not Found"),
            () => result.Data.ShouldBeNull());
    }

    // ── Cancel ───────────────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that Cancel patches a letter to cancel it.
    /// </summary>
    [Test]
    public async Task Cancel_ShouldPatchLetterCancel()
    {
        string letterId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath($"letters/{letterId}/cancel"))
                .UsingPatch())
            .RespondWith(Response.Create()
                .WithStatusCode(204)
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString()));

        ApiResult result = await Client.Letters.Cancel(letterId);

        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    ///     Verifies that Cancel surfaces a 422 already-cancelled error via ApiResult.
    /// </summary>
    [Test]
    public async Task Cancel_ShouldReturnErrorWhenAlreadyCancelled()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubError(
            OrgPath($"letters/{letterId}/cancel"),
            "PATCH",
            PingenResponseFactory.ErrorResponse("Conflict", "Letter has already been cancelled."));

        ApiResult result = await Client.Letters.Cancel(letterId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors[0].Title.ShouldBe("Conflict"));
    }

    /// <summary>
    ///     Verifies that Cancel surfaces a 404 not-found error via ApiResult.
    /// </summary>
    [Test]
    public async Task Cancel_ShouldReturnErrorWhenNotFound()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubError(
            OrgPath($"letters/{letterId}/cancel"),
            "PATCH",
            PingenResponseFactory.ErrorResponse("Not Found", "The letter does not exist.", "404"),
            404);

        ApiResult result = await Client.Letters.Cancel(letterId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors[0].Title.ShouldBe("Not Found"));
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that Delete removes a letter and returns success on 204 No Content.
    /// </summary>
    [Test]
    public async Task Delete_ShouldReturnSuccess()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubDelete(OrgPath($"letters/{letterId}"));

        ApiResult result = await Client.Letters.Delete(letterId);

        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    ///     Verifies that Delete surfaces a 404 not-found error via ApiResult.
    /// </summary>
    [Test]
    public async Task Delete_ShouldReturnErrorWhenNotFound()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubError(
            OrgPath($"letters/{letterId}"),
            "DELETE",
            PingenResponseFactory.ErrorResponse("Not Found", "The letter does not exist.", "404"),
            404);

        ApiResult result = await Client.Letters.Delete(letterId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors[0].Title.ShouldBe("Not Found"));
    }

    // ── Update ───────────────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that Update patches letter attributes and returns the updated resource.
    /// </summary>
    [Test]
    public async Task Update_ShouldPatchLetterAndReturnResult()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubJsonPatch(OrgPath($"letters/{letterId}"), PingenResponseFactory.SingleLetter(letterId));

        DataPatch<LetterUpdate>? data = LetterUpdateFaker(letterId).Generate();

        ApiResult<SingleResult<LetterDataDetailed>> result = await Client.Letters.Update(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(letterId));
    }

    /// <summary>
    ///     Verifies that Update surfaces a validation error via ApiResult.
    /// </summary>
    [Test]
    public async Task Update_ShouldReturnErrorWhenValidationFails()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubError(
            OrgPath($"letters/{letterId}"),
            "PATCH",
            PingenResponseFactory.ErrorResponse("Validation Failed", "PaperTypes contains an invalid value."));

        DataPatch<LetterUpdate>? data = LetterUpdateFaker(letterId).Generate();

        ApiResult<SingleResult<LetterDataDetailed>> result = await Client.Letters.Update(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors[0].Title.ShouldBe("Validation Failed"),
            () => result.Data.ShouldBeNull());
    }

    /// <summary>
    ///     Verifies that Update surfaces a 404 not-found error via ApiResult.
    /// </summary>
    [Test]
    public async Task Update_ShouldReturnErrorWhenNotFound()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubError(
            OrgPath($"letters/{letterId}"),
            "PATCH",
            PingenResponseFactory.ErrorResponse("Not Found", "The letter does not exist.", "404"),
            404);

        DataPatch<LetterUpdate>? data = LetterUpdateFaker(letterId).Generate();

        ApiResult<SingleResult<LetterDataDetailed>> result = await Client.Letters.Update(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors[0].Title.ShouldBe("Not Found"),
            () => result.Data.ShouldBeNull());
    }

    // ── GetFileLocation ──────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that GetFileLocation returns a 302 redirect with a Location header.
    /// </summary>
    [Test]
    public async Task GetFileLocation_ShouldReturn302WithLocation()
    {
        string letterId = Guid.NewGuid().ToString();
        const string fileUrl = "https://s3.example.com/files/letter.pdf";

        Server
            .Given(Request.Create()
                .WithPath(OrgPath($"letters/{letterId}/file"))
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(302)
                .WithHeader("Location", fileUrl)
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString()));

        ApiResult result = await Client.Letters.GetFileLocation(letterId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Location.ShouldNotBeNull(),
            () => result.Location!.ToString().ShouldBe(fileUrl));
    }

    /// <summary>
    ///     Verifies that GetFileLocation surfaces a 404 not-found error via ApiResult.
    /// </summary>
    [Test]
    public async Task GetFileLocation_ShouldReturnErrorWhenNotReady()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubError(
            OrgPath($"letters/{letterId}/file"),
            "GET",
            PingenResponseFactory.ErrorResponse("Not Found", "Letter file is not ready yet.", "404"),
            404);

        ApiResult result = await Client.Letters.GetFileLocation(letterId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors[0].Title.ShouldBe("Not Found"));
    }

    // ── DownloadFileContent ──────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that DownloadFileContent retrieves file content from an external URL.
    /// </summary>
    [Test]
    public async Task DownloadFileContent_ShouldReturnFileStream()
    {
        const string downloadPath = "/files/download/test-letter.pdf";
        byte[] fileContent = "fake-pdf-binary-content"u8.ToArray();

        Server
            .Given(Request.Create()
                .WithPath(downloadPath)
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/pdf")
                .WithBody(fileContent));

        var fileUrl = new Uri($"{Server.Url}{downloadPath}");
        await using Stream stream = await Client.Letters.DownloadFileContent(fileUrl);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        byte[] downloadedContent = memoryStream.ToArray();

        downloadedContent.ShouldBe(fileContent);
    }

    /// <summary>
    ///     Verifies that DownloadFileContent throws <see cref="PingenFileDownloadException" /> when the S3 download
    ///     fails with an XML-formatted error.
    /// </summary>
    [Test]
    public async Task DownloadFileContent_ShouldThrowWhenDownloadFails()
    {
        const string downloadPath = "/files/download/missing-letter.pdf";
        const string s3ErrorXml = """
                                  <?xml version="1.0" encoding="UTF-8"?>
                                  <Error>
                                      <Code>NoSuchKey</Code>
                                      <Message>The specified key does not exist.</Message>
                                  </Error>
                                  """;

        Server
            .Given(Request.Create()
                .WithPath(downloadPath)
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(404)
                .WithHeader("Content-Type", "application/xml")
                .WithBody(s3ErrorXml));

        var fileUrl = new Uri($"{Server.Url}{downloadPath}");

        PingenFileDownloadException exception =
            await Should.ThrowAsync<PingenFileDownloadException>(async () =>
                await Client.Letters.DownloadFileContent(fileUrl));

        exception.ShouldNotBeNull();
    }

    // ── CalculatePrice ───────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that CalculatePrice posts a price configuration and returns the price.
    /// </summary>
    [Test]
    public async Task CalculatePrice_ShouldReturnLetterPrice()
    {
        string priceId = Guid.NewGuid().ToString();

        Server.StubJsonPost(
            OrgPath("letters/price-calculator"),
            PingenResponseFactory.SingleLetterPriceCalculator(priceId, 2.75m),
            200);

        DataPost<LetterPriceConfiguration>? data = LetterPriceConfigurationFaker().Generate();

        ApiResult<SingleResult<LetterPriceData>> result = await Client.Letters.CalculatePrice(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(priceId),
            () => result.Data!.Data.Attributes.Price.ShouldBe(2.75m),
            () => result.Data!.Data.Attributes.Currency.ShouldBe(PingenApiCurrency.CHF));
    }

    /// <summary>
    ///     Verifies that CalculatePrice surfaces a validation error via ApiResult.
    /// </summary>
    [Test]
    public async Task CalculatePrice_ShouldReturnErrorWhenValidationFails()
    {
        Server.StubError(
            OrgPath("letters/price-calculator"),
            "POST",
            PingenResponseFactory.ErrorResponse("Validation Failed", "Country is required."));

        DataPost<LetterPriceConfiguration>? data = LetterPriceConfigurationFaker().Generate();

        ApiResult<SingleResult<LetterPriceData>> result = await Client.Letters.CalculatePrice(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors[0].Title.ShouldBe("Validation Failed"),
            () => result.Data.ShouldBeNull());
    }

    // ── GetEventsPage ────────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that GetEventsPage returns letter events.
    /// </summary>
    [Test]
    public async Task GetEventsPage_ShouldReturnLetterEvents()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubJsonGet(
            OrgPath($"letters/{letterId}/events"),
            PingenResponseFactory.LetterEventCollection(letterId: letterId));

        ApiResult<CollectionResult<LetterEventData>> result = await Client.Letters.GetEventsPage(letterId, "en-GB");

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(3),
            () => result.Data!.Data[0].Attributes.Code.ShouldNotBeNullOrEmpty());
    }

    /// <summary>
    ///     Verifies that GetEventsPage returns an empty collection when no events exist.
    /// </summary>
    [Test]
    public async Task GetEventsPage_ShouldReturnEmptyWhenNoEvents()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubJsonGet(
            OrgPath($"letters/{letterId}/events"),
            PingenResponseFactory.LetterEventCollection(0, letterId));

        ApiResult<CollectionResult<LetterEventData>> result = await Client.Letters.GetEventsPage(letterId, "en-GB");

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(0));
    }

    /// <summary>
    ///     Verifies that GetEventsPage exposes pagination meta on a multi-page response.
    /// </summary>
    [Test]
    public async Task GetEventsPage_ShouldExposePaginationMeta_ForMultiPageResponse()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubJsonGet(
            OrgPath($"letters/{letterId}/events"),
            PingenResponseFactory.LetterEventCollection(4, letterId, 1, 3));

        ApiResult<CollectionResult<LetterEventData>> result = await Client.Letters.GetEventsPage(letterId, "en-GB");

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data!.Meta.CurrentPage.ShouldBe(1),
            () => result.Data!.Meta.LastPage.ShouldBe(3));
    }

    /// <summary>
    ///     Verifies that GetEventsPage returns an unsuccessful ApiResult on API errors.
    /// </summary>
    [Test]
    public async Task GetEventsPage_OnApiError_ShouldReturnUnsuccessfulApiResult()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubError(
            OrgPath($"letters/{letterId}/events"),
            "GET",
            PingenResponseFactory.ErrorResponse("Server Error", "Service unavailable", "500"),
            500);

        ApiResult<CollectionResult<LetterEventData>> result = await Client.Letters.GetEventsPage(letterId, "en-GB");

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.Data.ShouldBeNull());
    }

    // ── GetEventsPageResultsAsync ────────────────────────────────────────────

    /// <summary>
    ///     Verifies that GetEventsPageResultsAsync auto-paginates letter events.
    /// </summary>
    [Test]
    public async Task GetEventsPageResultsAsync_ShouldAutoPaginate()
    {
        string letterId = Guid.NewGuid().ToString();

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
                .WithBody(PingenResponseFactory.LetterEventCollection(1, letterId, 1, 2)));

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
                .WithBody(PingenResponseFactory.LetterEventCollection(1, letterId, 2, 2)));

        var allItems = new List<string>();
        await foreach (IEnumerable<LetterEventData> page in Client.Letters.GetEventsPageResultsAsync(letterId, "en-GB"))
            allItems.AddRange(page.Select(item => item.Id));

        allItems.Count.ShouldBe(2);
    }

    /// <summary>
    ///     Verifies that GetEventsPageResultsAsync stops after a single page when only one exists.
    /// </summary>
    [Test]
    public async Task GetEventsPageResultsAsync_ShouldYieldSinglePage_WhenOnlyOneExists()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubJsonGet(
            OrgPath($"letters/{letterId}/events"),
            PingenResponseFactory.LetterEventCollection(2, letterId));

        var allItems = new List<string>();
        await foreach (IEnumerable<LetterEventData> page in Client.Letters.GetEventsPageResultsAsync(letterId, "en-GB"))
            allItems.AddRange(page.Select(item => item.Id));

        allItems.Count.ShouldBe(2);
    }

    /// <summary>
    ///     Verifies that GetEventsPageResultsAsync surfaces a <see cref="PingenApiErrorException" /> when the underlying
    ///     call fails.
    /// </summary>
    [Test]
    public async Task GetEventsPageResultsAsync_OnApiError_ShouldThrowPingenApiErrorException()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubError(
            OrgPath($"letters/{letterId}/events"),
            "GET",
            PingenResponseFactory.ErrorResponse("Forbidden", "Access denied", "403"),
            403);

        PingenApiErrorException exception = await Should.ThrowAsync<PingenApiErrorException>(async () =>
        {
            await foreach (IEnumerable<LetterEventData> _ in
                           Client.Letters.GetEventsPageResultsAsync(letterId, "en-GB"))
            {
                // Should never iterate
            }
        });

        exception.ApiResult.ShouldNotBeNull();
        exception.ApiResult!.IsSuccess.ShouldBeFalse();
    }

    // ── GetIssuesPage ────────────────────────────────────────────────────────

    /// <summary>
    ///     Verifies that GetIssuesPage returns letter issues.
    /// </summary>
    [Test]
    public async Task GetIssuesPage_ShouldReturnLetterIssues()
    {
        Server.StubJsonGet(
            OrgPath("letters/issues"),
            PingenResponseFactory.LetterEventCollection());

        ApiResult<CollectionResult<LetterEventData>> result = await Client.Letters.GetIssuesPage("en-GB");

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(3),
            () => result.Data!.Data[0].Attributes.Code.ShouldNotBeNullOrEmpty());
    }

    /// <summary>
    ///     Verifies that GetIssuesPage returns an empty collection when no issues exist.
    /// </summary>
    [Test]
    public async Task GetIssuesPage_ShouldReturnEmptyWhenNoIssues()
    {
        Server.StubJsonGet(
            OrgPath("letters/issues"),
            PingenResponseFactory.LetterEventCollection(0));

        ApiResult<CollectionResult<LetterEventData>> result = await Client.Letters.GetIssuesPage("en-GB");

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(0));
    }

    /// <summary>
    ///     Verifies that GetIssuesPage exposes pagination meta on a multi-page response.
    /// </summary>
    [Test]
    public async Task GetIssuesPage_ShouldExposePaginationMeta_ForMultiPageResponse()
    {
        Server.StubJsonGet(
            OrgPath("letters/issues"),
            PingenResponseFactory.LetterEventCollection(4, currentPage: 1, lastPage: 2));

        ApiResult<CollectionResult<LetterEventData>> result = await Client.Letters.GetIssuesPage("en-GB");

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data!.Meta.CurrentPage.ShouldBe(1),
            () => result.Data!.Meta.LastPage.ShouldBe(2));
    }

    /// <summary>
    ///     Verifies that GetIssuesPage returns an unsuccessful ApiResult on API errors.
    /// </summary>
    [Test]
    public async Task GetIssuesPage_OnApiError_ShouldReturnUnsuccessfulApiResult()
    {
        Server.StubError(
            OrgPath("letters/issues"),
            "GET",
            PingenResponseFactory.ErrorResponse("Server Error", "Service unavailable", "500"),
            500);

        ApiResult<CollectionResult<LetterEventData>> result = await Client.Letters.GetIssuesPage("en-GB");

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.Data.ShouldBeNull());
    }

    // ── GetIssuesPageResultsAsync ────────────────────────────────────────────

    /// <summary>
    ///     Verifies that GetIssuesPageResultsAsync auto-paginates letter issues.
    /// </summary>
    [Test]
    public async Task GetIssuesPageResultsAsync_ShouldAutoPaginate()
    {
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
                .WithBody(PingenResponseFactory.LetterEventCollection(1, currentPage: 1, lastPage: 2)));

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
                .WithBody(PingenResponseFactory.LetterEventCollection(1, currentPage: 2, lastPage: 2)));

        var allItems = new List<string>();
        await foreach (IEnumerable<LetterEventData> page in Client.Letters.GetIssuesPageResultsAsync("en-GB"))
            allItems.AddRange(page.Select(item => item.Id));

        allItems.Count.ShouldBe(2);
    }

    /// <summary>
    ///     Verifies that GetIssuesPageResultsAsync stops after a single page when only one exists.
    /// </summary>
    [Test]
    public async Task GetIssuesPageResultsAsync_ShouldYieldSinglePage_WhenOnlyOneExists()
    {
        Server.StubJsonGet(
            OrgPath("letters/issues"),
            PingenResponseFactory.LetterEventCollection(2));

        var allItems = new List<string>();
        await foreach (IEnumerable<LetterEventData> page in Client.Letters.GetIssuesPageResultsAsync("en-GB"))
            allItems.AddRange(page.Select(item => item.Id));

        allItems.Count.ShouldBe(2);
    }

    /// <summary>
    ///     Verifies that GetIssuesPageResultsAsync surfaces a <see cref="PingenApiErrorException" /> when the underlying
    ///     call fails.
    /// </summary>
    [Test]
    public async Task GetIssuesPageResultsAsync_OnApiError_ShouldThrowPingenApiErrorException()
    {
        Server.StubError(
            OrgPath("letters/issues"),
            "GET",
            PingenResponseFactory.ErrorResponse("Forbidden", "Access denied", "403"),
            403);

        PingenApiErrorException exception = await Should.ThrowAsync<PingenApiErrorException>(async () =>
        {
            await foreach (IEnumerable<LetterEventData> _ in Client.Letters.GetIssuesPageResultsAsync("en-GB"))
            {
                // Should never iterate
            }
        });

        exception.ApiResult.ShouldNotBeNull();
        exception.ApiResult!.IsSuccess.ShouldBeFalse();
    }
}
