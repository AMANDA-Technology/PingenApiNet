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

using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Enums.Letters;
using PingenApiNet.Abstractions.Exceptions;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Letters.Embedded;
using PingenApiNet.Abstractions.Models.Letters.Views;

namespace PingenApiNet.Tests.E2E.Tests;

/// <summary>
///
/// </summary>
public class TestGetFileUploadData : E2eTestBase
{
    /// <summary>
    ///
    /// </summary>
    [Test]
    public async Task GetUploadData()
    {
        PingenApiClient.ShouldNotBeNull();

        var res = await PingenApiClient!.Files.GetPath();
        res.ShouldNotBeNull();
        res.ShouldSatisfyAllConditions(
            () => res.IsSuccess.ShouldBeTrue(),
            () => res.ApiError.ShouldBeNull(),
            () => res.Data?.Data.ShouldNotBeNull()
        );
    }

    [Test]
    public async Task GetUploadDataAndCreateLetter()
    {
        const string fileName = "sample.pdf";
        PingenApiClient.ShouldNotBeNull();

        var res = await PingenApiClient!.Files.GetPath();
        res.ShouldNotBeNull();
        res.ShouldSatisfyAllConditions(
            () => res.IsSuccess.ShouldBeTrue(),
            () => res.ApiError.ShouldBeNull(),
            () => res.Data?.Data.ShouldNotBeNull()
        );

        MemoryStream stream = new();
        await File.OpenRead($"Assets/{fileName}").CopyToAsync(stream);
        var uploadRes = await PingenApiClient.Files.UploadFile(res.Data!.Data, stream);

        uploadRes.ShouldSatisfyAllConditions(
            () => uploadRes.IsSuccess.ShouldBeTrue(),
            () => uploadRes.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK)
        );

        var letterMetaData = new LetterMetaData
        {
            Recipient = new()
            {
                Name = "manuel gysin",
                Street = "solecht",
                Number = "42",
                Zip = "3303",
                City = "jegenstorf",
                Country = "CH"
            },
            Sender = new()
            {
                Name = "Monika Muster",
                Street = "Musterstrasse ",
                Number = "12",
                Zip = "1212",
                City = "Musterhausen",
                Country = "CH"
            }
        };

        var resLetter = await PingenApiClient.Letters.Create(new()
        {
            Attributes = new()
            {
                FileOriginalName = fileName,
                FileUrl = res.Data.Data.Attributes.Url,
                FileUrlSignature = res.Data.Data.Attributes.UrlSignature,
                AddressPosition = LetterAddressPosition.left,
                AutoSend = false,
                DeliveryProduct = LetterCreateDeliveryProduct.Cheap,
                PrintMode = LetterPrintMode.simplex,
                PrintSpectrum = LetterPrintSpectrum.grayscale,
                MetaData = letterMetaData
            },
            Type = PingenApiDataType.letters,
            Relationships = LetterCreateRelationships.Create("1234567890")
        });

        resLetter.ShouldNotBeNull();
        resLetter.ShouldSatisfyAllConditions(
            () => resLetter.IsSuccess.ShouldBeTrue(),
            () => resLetter.ApiError.ShouldBeNull(),
            () => resLetter.Data?.Data.ShouldNotBeNull()
        );

        var letterId = resLetter.Data!.Data.Id;

        var letterFromRemote = await PingenApiClient.Letters.Get(letterId);
        letterFromRemote.ShouldNotBeNull();

        var letterEvents = await PingenApiClient.Letters.GetEventsPage(letterId, PingenApiLanguage.EnGB);
        letterEvents.ShouldNotBeNull();

        const int attempts = 300;
        const int delaySeconds = 1;
        LetterDataDetailed? letter = null;
        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            var resultGetLetter = await PingenApiClient.Letters.Get(letterId);
            if (resultGetLetter.IsSuccess)
            {
                var status = resultGetLetter.Data?.Data.Attributes.Status;
                if (status == LetterStates.Valid)
                {
                    letter = resultGetLetter.Data!.Data;
                    break;
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        }

        letter.ShouldNotBeNull();
        letter.Attributes.Status.ShouldBe(LetterStates.Valid);

        var resSendLetter = await PingenApiClient.Letters.Send(new()
        {
            Type = PingenApiDataType.letters,
            Attributes = new()
            {
                DeliveryProduct = LetterSendDeliveryProduct.PostAgA,
                PrintMode = LetterPrintMode.simplex,
                PrintSpectrum = LetterPrintSpectrum.color,
                MetaData = letterMetaData
            },
            Id = letterId
        });

        resSendLetter.ShouldNotBeNull();
        resSendLetter.IsSuccess.ShouldBeTrue();
    }

    [Test]
    public async Task GetLetterEvents()
    {
        PingenApiClient.ShouldNotBeNull();
        const string letterId = "1540e30d-84cd-4425-bcc1-c3aff196d4da";

        foreach (var language in new[]
                 {
                     PingenApiLanguage.EnGB,
                     PingenApiLanguage.DeDE,
                     PingenApiLanguage.DeCH,
                     PingenApiLanguage.NlNL,
                     PingenApiLanguage.FrFR
                 })
        {
            var res = await PingenApiClient!.Letters.GetEventsPage(letterId, language);
            res.ShouldNotBeNull();
            res.ShouldSatisfyAllConditions(
                () => res.IsSuccess.ShouldBeTrue(),
                () => res.ApiError.ShouldBeNull(),
                () => res.Data?.Data.ShouldNotBeNull()
            );
        }
    }

    [Test]
    public async Task GetFileDownloadError()
    {
        PingenApiClient.ShouldNotBeNull();
        const string url = "https://pingen2-staging.objects.rma.cloudscale.ch/letters/20c1e673-03fe-4e19-ad45-9cdd85c5a940?X-Amz-Content-Sha256=UNSIGNED-PAYLOAD&X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=Z3YMWIXX6Y1G0KHUQDZ7%2F20221128%2Fregion1%2Fs3%2Faws4_request&X-Amz-Date=20221128T222323Z&X-Amz-SignedHeaders=host&X-Amz-Expires=86400&X-Amz-Signature=02705e5180cd082c5907d93e77fdb2d8c5a77938bc2e91a6e7c014ef953db9dd";

        await Should.ThrowAsync<PingenFileDownloadException>(async () => await PingenApiClient!.Letters.DownloadFileContent(new(url)));
    }

    [Test]
    public async Task GetFileDownload()
    {
        PingenApiClient.ShouldNotBeNull();
        const string letterId = "1540e30d-84cd-4425-bcc1-c3aff196d4da";
        const string filePath = "filepath.pdf";

        var res = await PingenApiClient!.Letters.GetFileLocation(letterId);
        res.ShouldNotBeNull();
        res.ShouldSatisfyAllConditions(
            () => res.IsSuccess.ShouldBeTrue(),
            () => res.ApiError.ShouldBeNull(),
            () => res.Location.ShouldNotBeNull()
        );

        var stream = await PingenApiClient.Letters.DownloadFileContent(res.Location!);
        await using (var file = File.OpenWrite(filePath))
            await stream.CopyToAsync(file);

        File.Exists(filePath).ShouldBeTrue();
        File.Delete(filePath);
    }
}
