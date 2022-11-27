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

namespace PingenApiNet.Tests;

public class TestGetFileUploadData
{
    private readonly IPingenApiClient _pingenApiClient;

    public TestGetFileUploadData()
    {
        var baseUri = Environment.GetEnvironmentVariable("PingenApiNet__BaseUri") ?? throw new("Missing PingenApiNet__BaseUri");
        var identityUri = Environment.GetEnvironmentVariable("PingenApiNet__IdentityUri") ?? throw new("Missing PingenApiNet__IdentityUri");
        var clientId = Environment.GetEnvironmentVariable("PingenApiNet__ClientId") ?? throw new("Missing PingenApiNet__ClientId");
        var clientSecret = Environment.GetEnvironmentVariable("PingenApiNet__ClientSecret") ?? throw new("Missing PingenApiNet__ClientSecret");
        var organisationId = Environment.GetEnvironmentVariable("PingenApiNet__OrganisationId") ?? throw new("Missing PingenApiNet__OrganisationId");

        _pingenApiClient = new PingenApiClient(
            new PingenConnectionHandler(
                new PingenConfiguration
                {
                    BaseUri = baseUri,
                    IdentityUri = identityUri,
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    DefaultOrganisationId = organisationId
                }));
    }

    [SetUp]
    public void Setup()
    {
        // Nothing to do yet
    }

    [Test]
    public async Task GetUploadData()
    {
        Assert.That(_pingenApiClient, Is.Not.Null);

        var res = await _pingenApiClient.Files.GetPath();
        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data?.Data, Is.Not.Null);
        });
    }

    [Test]
    public async Task GetUploadDataAndCreateLetter()
    {
        Assert.That(_pingenApiClient, Is.Not.Null);

        var res = await _pingenApiClient.Files.GetPath();
        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data?.Data, Is.Not.Null);
        });

        MemoryStream stream = new();
        await File.OpenRead("Assets/sample.pdf").CopyToAsync(stream);
        var uploadRes = await _pingenApiClient.Files.UploadFile(res.Data!.Data, stream);

        Assert.That(uploadRes, Is.True);

        var resLetter = await _pingenApiClient.Letters.Create(new()
        {
            Attributes = new()
            {
                FileOriginalName = "sample.pdf",
                FileUrl = res.Data.Data.Attributes.Url,
                FileUrlSignature = res.Data.Data.Attributes.UrlSignature,
                AddressPosition = LetterAddressPosition.left,
                AutoSend = false,
                DeliveryProduct = LetterDeliveryProduct.cheap,
                PrintMode = LetterPrintMode.simplex,
                PrintSpectrum = LetterPrintSpectrum.grayscale,
                MetaData = new()
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
                }
            },
            Type = PingenApiDataType.letters
        });

        Assert.That(resLetter, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(resLetter.IsSuccess, Is.True);
            Assert.That(resLetter.ApiError, Is.Null);
            Assert.That(resLetter.Data?.Data, Is.Not.Null);
        });
    }
}
