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
using PingenApiNet.Abstractions.Exceptions;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Webhooks;
using PingenApiNet.Abstractions.Models.Webhooks.Views;
using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace PingenApiNet.Tests.Integration.Tests;

/// <summary>
///     Integration tests for <see cref="IWebhookService" />.
/// </summary>
[TestFixture]
public sealed class WebhookServiceTests : IntegrationTestBase
{
    /// <summary>
    ///     Builds a Bogus faker for webhook create payloads.
    /// </summary>
    private static Faker<DataPost<WebhookCreate>> WebhookCreateFaker()
    {
        return new Faker<DataPost<WebhookCreate>>()
            .RuleFor(x => x.Type, PingenApiDataType.webhooks)
            .RuleFor(x => x.Attributes,
                f => new WebhookCreate
                {
                    FileOriginalName = f.PickRandom<WebhookEventCategory>(),
                    Url = new Uri(f.Internet.UrlWithPath()),
                    SigningKey = f.Random.AlphaNumeric(32)
                });
    }

    /// <summary>
    ///     Verifies that GetPage returns a paginated list of webhooks.
    /// </summary>
    [Test]
    public async Task GetPage_ShouldReturnWebhooks()
    {
        Server.StubJsonGet(OrgPath("webhooks"), PingenResponseFactory.WebhookCollection());

        ApiResult<CollectionResult<WebhookData>> result = await Client.Webhooks.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(3),
            () => result.Data!.Data[0].Attributes.Url.ShouldNotBeNull());
    }

    /// <summary>
    ///     Verifies that GetPage returns an empty collection when no webhooks exist.
    /// </summary>
    [Test]
    public async Task GetPage_ShouldReturnEmptyWhenNoWebhooks()
    {
        Server.StubJsonGet(OrgPath("webhooks"), PingenResponseFactory.WebhookCollection(0));

        ApiResult<CollectionResult<WebhookData>> result = await Client.Webhooks.GetPage();

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
            OrgPath("webhooks"),
            PingenResponseFactory.WebhookCollection(4, currentPage: 1, lastPage: 3));

        ApiResult<CollectionResult<WebhookData>> result = await Client.Webhooks.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data!.Meta.CurrentPage.ShouldBe(1),
            () => result.Data!.Meta.LastPage.ShouldBe(3));
    }

    /// <summary>
    ///     Verifies that GetPageResultsAsync auto-paginates across two pages.
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_ShouldAutoPaginate()
    {
        Server
            .Given(Request.Create()
                .WithPath(OrgPath("webhooks"))
                .UsingGet())
            .InScenario("webhooks-paging")
            .WillSetStateTo("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(PingenResponseFactory.WebhookCollection(
                    1, currentPage: 1, lastPage: 2)));

        Server
            .Given(Request.Create()
                .WithPath(OrgPath("webhooks"))
                .UsingGet())
            .InScenario("webhooks-paging")
            .WhenStateIs("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(PingenResponseFactory.WebhookCollection(
                    1, currentPage: 2, lastPage: 2)));

        var allItems = new List<string>();
        await foreach (IEnumerable<WebhookData> page in Client.Webhooks.GetPageResultsAsync())
            allItems.AddRange(page.Select(item => item.Id));

        allItems.Count.ShouldBe(2);
    }

    /// <summary>
    ///     Verifies that GetPageResultsAsync surfaces a <see cref="PingenApiErrorException" /> when
    ///     the underlying call fails.
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_OnApiError_ShouldThrowPingenApiErrorException()
    {
        Server.StubError(
            OrgPath("webhooks"),
            "GET",
            PingenResponseFactory.ErrorResponse("Forbidden", "Access denied", "403"),
            403);

        PingenApiErrorException exception = await Should.ThrowAsync<PingenApiErrorException>(async () =>
        {
            await foreach (IEnumerable<WebhookData> _ in Client.Webhooks.GetPageResultsAsync())
            {
                // Should never iterate
            }
        });

        exception.ApiResult.ShouldNotBeNull();
        exception.ApiResult!.IsSuccess.ShouldBeFalse();
    }

    /// <summary>
    ///     Verifies that Create posts a new webhook and returns the created resource.
    /// </summary>
    [Test]
    public async Task Create_ShouldPostWebhookAndReturnResult()
    {
        string webhookId = Guid.NewGuid().ToString();

        Server.StubJsonPost(OrgPath("webhooks"), PingenResponseFactory.SingleWebhook(webhookId));

        DataPost<WebhookCreate>? data = WebhookCreateFaker().Generate();

        ApiResult<SingleResult<WebhookData>> result = await Client.Webhooks.Create(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(webhookId),
            () => result.Data!.Data.Attributes.Url.ShouldNotBeNull());
    }

    /// <summary>
    ///     Verifies that Create surfaces a validation error returned by the API.
    /// </summary>
    [Test]
    public async Task Create_ShouldReturnErrorWhenValidationFails()
    {
        Server.StubError(
            OrgPath("webhooks"),
            "POST",
            PingenResponseFactory.ErrorResponse("Validation Failed", "URL is required."));

        DataPost<WebhookCreate>? data = WebhookCreateFaker().Generate();

        ApiResult<SingleResult<WebhookData>> result = await Client.Webhooks.Create(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors[0].Title.ShouldBe("Validation Failed"),
            () => result.Data.ShouldBeNull());
    }

    /// <summary>
    ///     Verifies that Get returns a single webhook by ID.
    /// </summary>
    [Test]
    public async Task Get_ShouldReturnSingleWebhook()
    {
        string webhookId = Guid.NewGuid().ToString();

        Server.StubJsonGet(OrgPath($"webhooks/{webhookId}"), PingenResponseFactory.SingleWebhook(webhookId));

        ApiResult<SingleResult<WebhookData>> result = await Client.Webhooks.Get(webhookId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(webhookId),
            () => result.Data!.Data.Attributes.Url.ShouldNotBeNull());
    }

    /// <summary>
    ///     Verifies that Get surfaces a 404 not-found error via ApiResult.
    /// </summary>
    [Test]
    public async Task Get_ShouldReturnErrorWhenNotFound()
    {
        string webhookId = Guid.NewGuid().ToString();

        Server.StubError(
            OrgPath($"webhooks/{webhookId}"),
            "GET",
            PingenResponseFactory.ErrorResponse("Not Found", "The webhook does not exist.", "404"),
            404);

        ApiResult<SingleResult<WebhookData>> result = await Client.Webhooks.Get(webhookId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors[0].Title.ShouldBe("Not Found"),
            () => result.Data.ShouldBeNull());
    }

    /// <summary>
    ///     Verifies that Delete removes a webhook and returns success on 204 No Content.
    /// </summary>
    [Test]
    public async Task Delete_ShouldReturnSuccess()
    {
        string webhookId = Guid.NewGuid().ToString();

        Server.StubDelete(OrgPath($"webhooks/{webhookId}"));

        ApiResult result = await Client.Webhooks.Delete(webhookId);

        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    ///     Verifies that Delete surfaces a 404 not-found error via ApiResult.
    /// </summary>
    [Test]
    public async Task Delete_ShouldReturnErrorWhenNotFound()
    {
        string webhookId = Guid.NewGuid().ToString();

        Server.StubError(
            OrgPath($"webhooks/{webhookId}"),
            "DELETE",
            PingenResponseFactory.ErrorResponse("Not Found", "The webhook does not exist.", "404"),
            404);

        ApiResult result = await Client.Webhooks.Delete(webhookId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors[0].Title.ShouldBe("Not Found"));
    }
}
