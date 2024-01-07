# PingenApiNet

Unofficial API client implementation for the Pingen v2 API. See [API Doc](https://api.pingen.com/documentation). (used version 2.0.0)

With a special thanks to Pingen GmbH for the handy Online Postal Service for sending letters digitally. See [Pingen website](https://www.pingen.ch/).

### Packages

- PingenApiNet: Client service to interact with the Pingen v2 API
- PingenApiNet.Abstractions: Models, Views and Enums used for the API
- PingenApiNet.AspNetCore: Dependency injection in ASP.NET Core

[![BuildNuGetAndPublish](https://github.com/AMANDA-Technology/PingenApiNet/actions/workflows/main.yml/badge.svg)](https://github.com/AMANDA-Technology/PingenApiNet/actions/workflows/main.yml)

[![CodeQL](https://github.com/AMANDA-Technology/PingenApiNet/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/AMANDA-Technology/PingenApiNet/actions/workflows/codeql-analysis.yml)

[![CodeQL](https://github.com/AMANDA-Technology/PingenApiNet/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/AMANDA-Technology/PingenApiNet/actions/workflows/codeql-analysis.yml)

### Usage

#### 1. Create and send a letter

Get a file path and upload file.
```c#
var resultFilePath = await _pingenApiClient.Files.GetPath();

if (!resultFilePath.IsSuccess)
    return $"Failed to get file upload path from pingen: {JsonConvert.SerializeObject(resultFilePath.ApiError)}";

if (resultFilePath.Data?.Data is null)
    return "Failed to get file upload path from pingen, request was OK but return null";

if (!await _pingenApiClient.Files.UploadFile(resultFilePath.Data.Data, contentStream))
    return "Failed to upload file to pingen storage";
```

Create the letter.
```c#
var resultCreateLetter = await _pingenApiClient.Letters.Create(new()
{
    Attributes = new()
    {
        FileOriginalName = filename,
        FileUrl = resultFilePath.Data.Data.Attributes.Url,
        FileUrlSignature = resultFilePath.Data.Data.Attributes.UrlSignature,
        AddressPosition = LetterAddressPosition.left,
        AutoSend = false,
        DeliveryProduct = LetterCreateDeliveryProduct.Cheap, // A more specific product can be used later at Letters/Send endpoint
        PrintMode = LetterPrintMode.simplex,
        PrintSpectrum = LetterPrintSpectrum.color,
        MetaData = new() // This must only be set when "postag_registered" or "postag_registered" product used. Otherwise the API can fail at address validation when Zip code has more than 4 characters.
        {
            Recipient = new()
            {
                Name = recipientName,
                Street = recipientAddress.Street,
                Number = recipientAddress.HouseNumber,
                Zip = recipientAddress.Postcode,
                City = recipientAddress.City,
                Country = recipientAddress.Country
            },
            Sender = new()
            {
                Name = senderName,
                //...
                Country = senderAddress.Country
            }
        }
    },
    Type = PingenApiDataType.letters
}, $"Letters.Create.{outgoing.Id}"); // Optionally, idempotency key

if (!resultCreateLetter.IsSuccess)
    return $"Failed create letter on pingen: {JsonConvert.SerializeObject(resultCreateLetter.ApiError)}";

if (resultCreateLetter.Data?.Data is null)
    return "Failed create letter on pingen, request was OK but return null";
```

Check the letter state and send it.
```c#
// NOTE: Consider using a loop with delay to poll the letter state (Pingen needs some seconds to validate the letter)
var resultGetLetter = await _pingenApiClient.Letters.Get(letter.Id);

if (resultGetLetter.IsSuccess
    && resultGetLetter.Data?.Data.Attributes.Status == LetterStates.Valid)
{
    var resultSendLetter = await _pingenApiClient.Letters.Send(new()
    {
        Type = PingenApiDataType.letters,
        Attributes = new()
        {
            DeliveryProduct = LetterSendDeliveryProduct.PostAgA
            PrintMode = LetterPrintMode.simplex,
            PrintSpectrum = LetterPrintSpectrum.color,
            MetaData = // Use from sample above at Letters.Create
        },
        Id = resultGetLetter.Data!.Data.Id
    });

    if (!resultSendLetter.IsSuccess)
        return $"Failed to send letter on pingen: {JsonConvert.SerializeObject(resultSendLetter.ApiError)}";
}
```

#### 2. Download file from a letter

Get letter and download file.
```c#
var resultFileLocation = await _pingenApiClient.Letters.GetFileLocation(letterId);
if (!resultFileLocation.IsSuccess)
    return new(null, $"Failed to get file location from letter on pingen: {JsonConvert.SerializeObject(resultFileLocation.ApiError)}");

if (resultFileLocation.Location is null)
    return new(null, "Failed to get file location from letter on pingen, request was OK but location null");

try
{
    var memoryStrem = await _pingenApiClient.Letters.DownloadFileContent(resultFileLocation.Location);
}
catch (PingenFileDownloadException e)
{
    return $"Failed to download file from pingen: {e.ErrorCode}";
}
```

#### 3. Get delivery products

Get all delivery products with auto paging.
```c#
await foreach (var page in _pingenApiClient.Distributions.GetDeliveryProductsPageResultsAsync(apiPagingRequest))
{
    deliveryProducts.AddRange(page);
}
```

#### 4. Sample for sorting & filtering

This is barely tested but should work as follows:
```c#
var apiPagingRequest = new ApiPagingRequest
{
    Sorting = new Dictionary<string, CollectionSortDirection>
    {
        [PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(letter => letter.CreatedAt)] = CollectionSortDirection.DESC
    },
    Filtering = new(
        CollectionFilterOperator.And,
        new KeyValuePair<string, object>[]
        {
            new(CollectionFilterOperator.Or, new KeyValuePair<string, object>[]
            {
                new(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(letter => letter.Country), "CH"),
                new(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(letter => letter.Country), "LI")
            }),
            new(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(letter => letter.Status), "valid")
        })
};
```

#### 5. Dependency Injection

Register pingen services. You can inject the client by interface IPingenApiClient.
```c#
services.AddPingenServices(new PingenConfiguration
{
    BaseUri = "https://api-staging.pingen.com",
    IdentityUri = "https://identity-staging.pingen.com",
    ClientId = /****/,
    ClientSecret = /****/,
    DefaultOrganisationId = /****/
});
```

#### 6. Receive Webhooks

Validate webhook and extract data.
```c#
[NonAction]
private async Task<(WebhookEventData WebhookEventData, Data<Organisation> OrganisationData, Data<Letter> LetterData, Data<LetterEvent> EventData)> ValidateAndGetWebhookEvent()
{
    // Enable request buffering for multiple body stream reads
    Request.EnableBuffering();

    // Ensure signature header received
    if (!Request.Headers.TryGetValue("Signature", out var signature))
        throw new InvalidOperationException("Missing 'Signature' header");

    // Validate webhook body and get data
    var (webhookEventData, organisationData, letterData, eventData) = await PingenWebhookHelper.ValidateWebhookAndGetData(signingKey, /*your signature*/, Request.Body);
    if (webhookEventData is null)
        throw new InvalidOperationException("Failed to parse WebhookEventData from body, resulted as NULL");

    return (webhookEventData, organisationData, letterData, eventData);
}
```
