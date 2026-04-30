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

using PingenApiNet.Abstractions.Exceptions;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Batches;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace PingenApiNet.Tests.Integration.Tests.CrossCutting;

/// <summary>
///     Cross-cutting integration tests verifying that HTTP error status codes returned by the
///     Pingen API are translated into the correct exception types and that the JSON:API error
///     envelope is parsed into <see cref="ApiResult.ApiError" />.
/// </summary>
[TestFixture]
public sealed class ErrorHandlingTests : IntegrationTestBase
{
    /// <summary>
    ///     Verifies that auto-pagination across the letters endpoint surfaces a
    ///     <see cref="PingenApiErrorException" /> for every documented HTTP error status code.
    /// </summary>
    /// <param name="statusCode">HTTP status code returned by the stub.</param>
    [TestCase(401)]
    [TestCase(403)]
    [TestCase(404)]
    [TestCase(422)]
    [TestCase(500)]
    [TestCase(502)]
    [TestCase(503)]
    [TestCase(504)]
    public async Task GetPageResultsAsync_OnApiError_ShouldThrowPingenApiErrorException(int statusCode)
    {
        Server.StubError(
            OrgPath("letters"),
            "GET",
            PingenResponseFactory.ErrorResponse(
                $"Error {statusCode}",
                $"HTTP {statusCode} returned by stub",
                statusCode.ToString()),
            statusCode);

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

    /// <summary>
    ///     Verifies that the JSON:API error envelope is deserialized into
    ///     <see cref="ApiResult.ApiError" /> with title and detail fields preserved.
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_OnApiError_ShouldParseJsonApiErrorEnvelope()
    {
        Server.StubError(
            OrgPath("letters"),
            "GET",
            PingenResponseFactory.ErrorResponse(
                "Validation failed",
                "The request payload is invalid"));

        PingenApiErrorException exception = await Should.ThrowAsync<PingenApiErrorException>(async () =>
        {
            await foreach (IEnumerable<LetterData> _ in Client.Letters.GetPageResultsAsync())
            {
                // Should never iterate
            }
        });

        exception.ApiResult.ShouldNotBeNull();
        exception.ApiResult!.ApiError.ShouldNotBeNull();
        exception.ApiResult.ApiError!.Errors.Count.ShouldBeGreaterThan(0);
        exception.ApiResult.ApiError.Errors[0].Title.ShouldBe("Validation failed");
        exception.ApiResult.ApiError.Errors[0].Detail.ShouldBe("The request payload is invalid");
    }

    /// <summary>
    ///     Verifies that a single GET returning an HTTP error sets <see cref="ApiResult.IsSuccess" />
    ///     to false without throwing, since callers can opt to handle the raw <see cref="ApiResult" />.
    /// </summary>
    [TestCase(401)]
    [TestCase(403)]
    [TestCase(404)]
    [TestCase(422)]
    [TestCase(500)]
    [TestCase(502)]
    [TestCase(503)]
    [TestCase(504)]
    public async Task GetPage_OnApiError_ShouldReturnUnsuccessfulApiResult(int statusCode)
    {
        Server.StubError(
            OrgPath("batches"),
            "GET",
            PingenResponseFactory.ErrorResponse(status: statusCode.ToString()),
            statusCode);

        ApiResult<CollectionResult<BatchData>> result = await Client.Batches.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors.Count.ShouldBeGreaterThan(0),
            () => result.Data.ShouldBeNull());
    }

    /// <summary>
    ///     Verifies that a 429 Too Many Requests response surfaces the rate-limit
    ///     <see cref="ApiResult.RetryAfter" /> header alongside the failure indication.
    /// </summary>
    [Test]
    public async Task GetPage_On429TooManyRequests_ShouldExposeRetryAfter()
    {
        Server
            .Given(Request.Create()
                .WithPath(OrgPath("batches"))
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(429)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Retry-After", "30")
                .WithBody(PingenResponseFactory.ErrorResponse(
                    "Too Many Requests",
                    "Rate limit exceeded",
                    "429")));

        ApiResult<CollectionResult<BatchData>> result = await Client.Batches.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.RetryAfter.ShouldBe(30),
            () => result.ApiError.ShouldNotBeNull());
    }
}
