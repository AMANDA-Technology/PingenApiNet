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
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Letters.Views;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.Logging;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Types;

namespace PingenApiNet.Tests.Integration.Tests.CrossCutting;

/// <summary>
///     Cross-cutting integration tests verifying that the optional idempotency key flows through
///     <c>PingenConnectionHandler</c> as the <c>Idempotency-Key</c> request header, and that the
///     <c>Idempotent-Replayed</c> response header is surfaced via <see cref="ApiResult.IdempotentReplayed" />.
/// </summary>
[TestFixture]
public sealed class IdempotencyTests : IntegrationTestBase
{
    private const string IdempotencyKeyHeader = "Idempotency-Key";

    /// <summary>
    ///     Builds a Bogus faker for letter create payloads, mirroring the fixture used in
    ///     <c>LetterServiceTests</c> so request shaping stays realistic.
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
    ///     Verifies that <c>Letters.Create</c> forwards the supplied idempotency key as the
    ///     <c>Idempotency-Key</c> request header so retries can be safely deduplicated server-side.
    /// </summary>
    [Test]
    public async Task Create_WithIdempotencyKey_ShouldSendIdempotencyKeyHeader()
    {
        string letterId = Guid.NewGuid().ToString();
        string idempotencyKey = Guid.NewGuid().ToString();

        Server.StubJsonPost(OrgPath("letters"), PingenResponseFactory.SingleLetter(letterId));

        DataPost<LetterCreate, LetterCreateRelationships> data = LetterCreateFaker().Generate();

        await Client.Letters.Create(data, idempotencyKey);

        ILogEntry entry = Server.LogEntries.Single(e =>
            e.RequestMessage?.Path == OrgPath("letters") && e.RequestMessage?.Method == "POST");
        IDictionary<string, WireMockList<string>>? headers = entry.RequestMessage!.Headers!;
        headers.ShouldContainKey(IdempotencyKeyHeader);
        headers[IdempotencyKeyHeader][0].ShouldBe(idempotencyKey);
    }

    /// <summary>
    ///     Verifies that <c>Letters.Send</c> forwards the supplied idempotency key as the
    ///     <c>Idempotency-Key</c> request header on the PATCH /send endpoint.
    /// </summary>
    [Test]
    public async Task Send_WithIdempotencyKey_ShouldSendIdempotencyKeyHeader()
    {
        string letterId = Guid.NewGuid().ToString();
        string idempotencyKey = Guid.NewGuid().ToString();

        Server.StubJsonPatch(OrgPath($"letters/{letterId}/send"), PingenResponseFactory.SingleLetter(letterId));

        DataPatch<LetterSend> data = LetterSendFaker(letterId).Generate();

        await Client.Letters.Send(data, idempotencyKey);

        ILogEntry entry = Server.LogEntries.Single(e =>
            e.RequestMessage?.Path == OrgPath($"letters/{letterId}/send") && e.RequestMessage?.Method == "PATCH");
        IDictionary<string, WireMockList<string>>? headers = entry.RequestMessage!.Headers!;
        headers.ShouldContainKey(IdempotencyKeyHeader);
        headers[IdempotencyKeyHeader][0].ShouldBe(idempotencyKey);
    }

    /// <summary>
    ///     Verifies that <c>Letters.Cancel</c> forwards the supplied idempotency key as the
    ///     <c>Idempotency-Key</c> request header on the PATCH /cancel endpoint.
    /// </summary>
    [Test]
    public async Task Cancel_WithIdempotencyKey_ShouldSendIdempotencyKeyHeader()
    {
        string letterId = Guid.NewGuid().ToString();
        string idempotencyKey = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath($"letters/{letterId}/cancel"))
                .UsingPatch())
            .RespondWith(Response.Create()
                .WithStatusCode(204)
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString()));

        await Client.Letters.Cancel(letterId, idempotencyKey);

        ILogEntry entry = Server.LogEntries.Single(e =>
            e.RequestMessage?.Path == OrgPath($"letters/{letterId}/cancel") && e.RequestMessage?.Method == "PATCH");
        IDictionary<string, WireMockList<string>>? headers = entry.RequestMessage!.Headers!;
        headers.ShouldContainKey(IdempotencyKeyHeader);
        headers[IdempotencyKeyHeader][0].ShouldBe(idempotencyKey);
    }

    /// <summary>
    ///     Verifies that an <c>Idempotent-Replayed: true</c> response header is surfaced as
    ///     <see cref="ApiResult.IdempotentReplayed" /> on the resulting <see cref="ApiResult" />,
    ///     so callers can distinguish a fresh request from a replayed one.
    /// </summary>
    [Test]
    public async Task Create_WhenServerReplaysIdempotentResponse_ShouldSetIdempotentReplayedTrue()
    {
        string letterId = Guid.NewGuid().ToString();
        string idempotencyKey = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath("letters"))
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(201)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithHeader(ApiHeaderNames.IdempotentReplayed, "true")
                .WithBody(PingenResponseFactory.SingleLetter(letterId)));

        DataPost<LetterCreate, LetterCreateRelationships> data = LetterCreateFaker().Generate();

        ApiResult<SingleResult<LetterDataDetailed>> result = await Client.Letters.Create(data, idempotencyKey);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.IdempotentReplayed.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(letterId));
    }

    /// <summary>
    ///     Verifies that omitting the idempotency key results in no <c>Idempotency-Key</c> header on
    ///     the outgoing request, so the absence of opt-in does not pollute the wire.
    /// </summary>
    [Test]
    public async Task Create_WithoutIdempotencyKey_ShouldNotSendIdempotencyKeyHeader()
    {
        string letterId = Guid.NewGuid().ToString();

        Server.StubJsonPost(OrgPath("letters"), PingenResponseFactory.SingleLetter(letterId));

        DataPost<LetterCreate, LetterCreateRelationships> data = LetterCreateFaker().Generate();

        await Client.Letters.Create(data);

        ILogEntry entry = Server.LogEntries.Single(e =>
            e.RequestMessage?.Path == OrgPath("letters") && e.RequestMessage?.Method == "POST");
        entry.RequestMessage!.Headers!.ShouldNotContainKey(IdempotencyKeyHeader);
    }
}
