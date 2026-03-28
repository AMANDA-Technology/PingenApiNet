using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Enums.Batches;
using PingenApiNet.Abstractions.Enums.Letters;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Batches.Views;
using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace PingenApiNet.Tests.Integration.Tests;

/// <summary>
/// Integration tests for <see cref="IBatchService"/>.
/// </summary>
[TestFixture]
public sealed class BatchServiceTests : IntegrationTestBase
{
    [Test]
    public async Task GetPage_ShouldReturnBatches()
    {
        var batchId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath("batches"))
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.CollectionResponse(
                    [
                        (batchId,
                            new { name = "Test Batch", icon = "flat", status = "valid", file_original_name = "letters.pdf", letter_count = 5, price_currency = "CHF", price_value = 12.50 },
                            (object)new
                            {
                                organisation = JsonApiStubHelper.RelatedSingle(TestOrganisationId, "organisations"),
                                events = JsonApiStubHelper.RelatedMany()
                            },
                            null)
                    ],
                    "batches")));

        var result = await Client.Batches.GetPage();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Count.ShouldBe(1),
            () => result.Data!.Data[0].Id.ShouldBe(batchId),
            () => result.Data!.Data[0].Attributes.Name.ShouldBe("Test Batch"));
    }

    [Test]
    public async Task Get_ShouldReturnSingleBatch()
    {
        var batchId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath($"batches/{batchId}"))
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.SingleResponse(
                    batchId,
                    "batches",
                    new { name = "My Batch", icon = "flat", status = "valid", file_original_name = "test.pdf", letter_count = 3, price_currency = "CHF", price_value = 8.75 },
                    relationships: new
                    {
                        organisation = JsonApiStubHelper.RelatedSingle(TestOrganisationId, "organisations"),
                        events = JsonApiStubHelper.RelatedMany()
                    },
                    meta: JsonApiStubHelper.MetaWithAbilities(new { cancel = "ok", delete = "ok", submit = "ok", edit = "ok", change_window_position = "ok", add_attachment = "ok" }))));

        var result = await Client.Batches.Get(batchId);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(batchId),
            () => result.Data!.Data.Attributes.Name.ShouldBe("My Batch"),
            () => result.Data!.Data.Attributes.LetterCount.ShouldBe(3));
    }

    [Test]
    public async Task Create_ShouldPostBatchAndReturnResult()
    {
        var batchId = Guid.NewGuid().ToString();

        Server
            .Given(Request.Create()
                .WithPath(OrgPath("batches"))
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(201)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.SingleResponse(
                    batchId,
                    "batches",
                    new { name = "Created Batch", icon = "flat", status = "processing", file_original_name = "new.pdf", letter_count = 0, price_currency = "CHF", price_value = 0.0 },
                    relationships: new
                    {
                        organisation = JsonApiStubHelper.RelatedSingle(TestOrganisationId, "organisations"),
                        events = JsonApiStubHelper.RelatedMany()
                    },
                    meta: JsonApiStubHelper.MetaWithAbilities(new { cancel = "ok", delete = "ok", submit = "state", edit = "ok", change_window_position = "ok", add_attachment = "ok" }))));

        var data = new DataPost<BatchCreate, BatchCreateRelationships>
        {
            Type = PingenApiDataType.batches,
            Attributes = new BatchCreate
            {
                Name = "Created Batch",
                Icon = BatchIcon.campaign,
                FileOriginalName = "new.pdf",
                FileUrl = "https://example.com/files/new.pdf",
                FileUrlSignature = "sig-123",
                AddressPosition = LetterAddressPosition.left,
                GroupingType = BatchGroupingType.merge,
                GroupingOptionsSplitType = BatchGroupingOptionsSplitType.file
            },
            Relationships = BatchCreateRelationships.Create("preset-id-001")
        };

        var result = await Client.Batches.Create(data);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(batchId),
            () => result.Data!.Data.Attributes.Name.ShouldBe("Created Batch"),
            () => result.Data!.Data.Attributes.Status.ShouldBe("processing"));
    }
}
