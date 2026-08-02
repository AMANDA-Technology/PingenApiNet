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
using PingenApiNet.Abstractions.Models.Files;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.Logging;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Types;

namespace PingenApiNet.Tests.Integration.Tests.CrossCutting;

/// <summary>
///     Cross-cutting integration tests verifying that <see cref="IPingenConfiguration.DefaultRequestHeaders" />
///     reaches the wire on the identity and API HTTP clients, and never on the external files client, which
///     targets pre signed third party storage URLs (see ADR-004).
/// </summary>
[TestFixture]
public sealed class DefaultRequestHeadersTests : IntegrationTestBase
{
    private const string HeaderName = "X-Amanda-Client";

    private const string HeaderValue = "AMANDA.discountfit/2f3a9c1 (Backend; env=int)";

    private const string IdempotencyKeyHeader = "Idempotency-Key";

    private static readonly Faker _faker = new();

    /// <inheritdoc />
    protected override IReadOnlyDictionary<string, string>? DefaultRequestHeaders => new Dictionary<string, string>
    {
        [HeaderName] = HeaderValue,
        // Reserved, must never reach the wire from here
        [IdempotencyKeyHeader] = "caller-supplied-idempotency-key"
    };

    /// <summary>
    ///     Verifies that the configured default request headers are sent to the Pingen API.
    /// </summary>
    [Test]
    public async Task ApiRequest_ShouldSendConfiguredDefaultRequestHeader()
    {
        Server.StubJsonGet(OrgPath("deliveries/letters"), PingenResponseFactory.LetterCollection());

        await Client.Letters.GetPage();

        GetRequestHeaderValues(OrgPath("deliveries/letters"), "GET", HeaderName).ShouldBe([HeaderValue]);
    }

    /// <summary>
    ///     Verifies that the configured default request headers are sent to the Pingen identity service.
    /// </summary>
    [Test]
    public async Task IdentityRequest_ShouldSendConfiguredDefaultRequestHeader()
    {
        Server.StubJsonGet(OrgPath("deliveries/letters"), PingenResponseFactory.LetterCollection());

        await Client.Letters.GetPage();

        GetRequestHeaderValues("/auth/access-tokens", "POST", HeaderName).ShouldBe([HeaderValue]);
    }

    /// <summary>
    ///     Verifies that the configured default request headers are NOT sent to the external file storage. That
    ///     request targets a pre signed third party URL, an additional header risks a signature rejection.
    /// </summary>
    [Test]
    public async Task ExternalFileUpload_ShouldNotSendConfiguredDefaultRequestHeader()
    {
        string uploadPath = $"/upload/{Guid.NewGuid()}";

        Server
            .Given(Request.Create()
                .WithPath(uploadPath)
                .UsingPut())
            .RespondWith(Response.Create().WithStatusCode(200));

        var fileUploadData = new FileUploadData
        {
            Id = Guid.NewGuid().ToString(),
            Type = PingenApiDataType.file_uploads,
            Attributes = new FileUpload(
                $"{Server.Url}{uploadPath}",
                _faker.Random.AlphaNumeric(32),
                DateTime.UtcNow.AddHours(1))
        };

        using var stream = new MemoryStream(_faker.Random.Bytes(128));

        await Client.Files.UploadFile(fileUploadData, stream);

        GetRequestHeaderValues(uploadPath, "PUT", HeaderName).ShouldBeEmpty();
    }

    /// <summary>
    ///     Verifies that a reserved header name configured as a default request header never reaches the wire, so it
    ///     can neither duplicate nor overwrite the idempotency key this library sends itself.
    /// </summary>
    [Test]
    public async Task ApiRequest_WithIdempotencyKey_ShouldSendOnlyTheIdempotencyKeyOfThisLibrary()
    {
        string idempotencyKey = Guid.NewGuid().ToString();
        string letterId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath($"deliveries/letters/{letterId}/cancel"))
                .UsingPatch())
            .RespondWith(Response.Create()
                .WithStatusCode(204)
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString()));

        await Client.Letters.Cancel(letterId, idempotencyKey);

        GetRequestHeaderValues(OrgPath($"deliveries/letters/{letterId}/cancel"), "PATCH", IdempotencyKeyHeader)
            .ShouldBe([idempotencyKey]);
    }

    /// <summary>
    ///     Get all values of the given header on the recorded request, or an empty array if the header is not set.
    /// </summary>
    /// <param name="path">URL path of the recorded request.</param>
    /// <param name="method">HTTP method of the recorded request.</param>
    /// <param name="name">The header name to read.</param>
    /// <returns>All values of the given header.</returns>
    private string[] GetRequestHeaderValues(string path, string method, string name)
    {
        ILogEntry entry = Server.LogEntries.Single(e =>
            e.RequestMessage?.Path == path &&
            string.Equals(e.RequestMessage?.Method, method, StringComparison.OrdinalIgnoreCase));

        IDictionary<string, WireMockList<string>>? headers = entry.RequestMessage!.Headers;

        return headers is not null && headers.TryGetValue(name, out WireMockList<string>? values)
            ? [.. values]
            : [];
    }
}
