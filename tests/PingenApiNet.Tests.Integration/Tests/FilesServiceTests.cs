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

using System.Net;
using Bogus;
using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Files;
using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Tests.Integration.Helpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace PingenApiNet.Tests.Integration.Tests;

/// <summary>
///     Integration tests for <see cref="IFilesService" />.
/// </summary>
[TestFixture]
public sealed class FilesServiceTests : IntegrationTestBase
{
    private static readonly Faker _faker = new();

    /// <summary>
    ///     Builds a <see cref="FileUploadData" /> pointing at a path on the local WireMock server.
    /// </summary>
    private FileUploadData BuildUploadData(string path)
    {
        return new FileUploadData
        {
            Id = Guid.NewGuid().ToString(),
            Type = PingenApiDataType.file_uploads,
            Attributes = new FileUpload(
                $"{Server.Url}{path}",
                _faker.Random.AlphaNumeric(32),
                DateTime.UtcNow.AddHours(1))
        };
    }

    /// <summary>
    ///     Verifies that GetPath returns the upload URL and signature.
    /// </summary>
    [Test]
    public async Task GetPath_ShouldReturnUploadUrlAndSignature()
    {
        string fileUploadId = Guid.NewGuid().ToString();

        Server.StubJsonGet("/file-upload", PingenResponseFactory.FileUploadPath(fileUploadId));

        ApiResult<SingleResult<FileUploadData>> result = await Client.Files.GetPath();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldNotBeNull(),
            () => result.Data!.Data.Id.ShouldBe(fileUploadId),
            () => result.Data!.Data.Attributes.Url.ShouldNotBeNullOrEmpty(),
            () => result.Data!.Data.Attributes.UrlSignature.ShouldNotBeNullOrEmpty());
    }

    /// <summary>
    ///     Verifies that GetPath surfaces JSON:API errors as an unsuccessful ApiResult.
    /// </summary>
    [TestCase(401)]
    [TestCase(403)]
    [TestCase(500)]
    [TestCase(503)]
    public async Task GetPath_OnApiError_ShouldReturnUnsuccessfulApiResult(int statusCode)
    {
        Server.StubError(
            "/file-upload",
            "GET",
            PingenResponseFactory.ErrorResponse(status: statusCode.ToString()),
            statusCode);

        ApiResult<SingleResult<FileUploadData>> result = await Client.Files.GetPath();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.Data.ShouldBeNull());
    }

    /// <summary>
    ///     Verifies that UploadFile PUTs a file to the upload URL and returns success.
    /// </summary>
    [Test]
    public async Task UploadFile_ShouldPutFileToUploadUrl()
    {
        string uploadPath = $"/upload/{Guid.NewGuid()}";

        Server
            .Given(Request.Create()
                .WithPath(uploadPath)
                .UsingPut())
            .RespondWith(Response.Create().WithStatusCode(200));

        FileUploadData fileUploadData = BuildUploadData(uploadPath);
        using var stream = new MemoryStream(_faker.Random.Bytes(128));

        ExternalRequestResult result = await Client.Files.UploadFile(fileUploadData, stream);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.StatusCode.ShouldBe(HttpStatusCode.OK));
    }

    /// <summary>
    ///     Verifies that UploadFile reports failure for S3 4xx/5xx responses without throwing.
    /// </summary>
    [TestCase(400, HttpStatusCode.BadRequest)]
    [TestCase(403, HttpStatusCode.Forbidden)]
    [TestCase(404, HttpStatusCode.NotFound)]
    [TestCase(500, HttpStatusCode.InternalServerError)]
    [TestCase(503, HttpStatusCode.ServiceUnavailable)]
    public async Task UploadFile_ShouldReturnErrorWhenS3Fails(int statusCode, HttpStatusCode expected)
    {
        string uploadPath = $"/upload/{Guid.NewGuid()}";

        Server
            .Given(Request.Create()
                .WithPath(uploadPath)
                .UsingPut())
            .RespondWith(Response.Create().WithStatusCode(statusCode));

        FileUploadData fileUploadData = BuildUploadData(uploadPath);
        using var stream = new MemoryStream(_faker.Random.Bytes(128));

        ExternalRequestResult result = await Client.Files.UploadFile(fileUploadData, stream);

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.StatusCode.ShouldBe(expected));
    }
}
