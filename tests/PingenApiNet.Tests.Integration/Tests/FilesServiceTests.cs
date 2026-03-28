using PingenApiNet.Abstractions.Models.Files;
using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace PingenApiNet.Tests.Integration.Tests;

/// <summary>
/// Integration tests for <see cref="IFilesService"/>.
/// </summary>
[TestFixture]
public sealed class FilesServiceTests : IntegrationTestBase
{
    [Test]
    public async Task GetPath_ShouldReturnUploadUrlAndSignature()
    {
        var fileUploadId = Guid.NewGuid().ToString();
        var uploadUrl = $"{Server.Url}/upload/test-file";

        Server
            .Given(Request.Create()
                .WithPath("/file-upload")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(JsonApiStubHelper.SingleResponse(
                    fileUploadId,
                    "file_uploads",
                    new { url = uploadUrl, url_signature = "sig-abc-123", expires_at = "2026-12-31T23:59:59Z" })));

        var result = await Client.Files.GetPath();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(fileUploadId),
            () => result.Data!.Data.Attributes.Url.ShouldBe(uploadUrl),
            () => result.Data!.Data.Attributes.UrlSignature.ShouldBe("sig-abc-123"));
    }

    [Test]
    public async Task UploadFile_ShouldPutFileToUploadUrl()
    {
        var fileUploadId = Guid.NewGuid().ToString();
        var uploadPath = "/upload/test-file-put";
        var uploadUrl = $"{Server.Url}{uploadPath}";

        // Stub the PUT endpoint for file upload
        Server
            .Given(Request.Create()
                .WithPath(uploadPath)
                .UsingPut())
            .RespondWith(Response.Create()
                .WithStatusCode(200));

        var fileUploadData = new FileUploadData
        {
            Id = fileUploadId,
            Type = Abstractions.Enums.Api.PingenApiDataType.file_uploads,
            Attributes = new FileUpload(uploadUrl, "sig-abc-123", DateTime.UtcNow.AddHours(1))
        };

        using var stream = new MemoryStream("test file content"u8.ToArray());
        var result = await Client.Files.UploadFile(fileUploadData, stream);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK));
    }
}
