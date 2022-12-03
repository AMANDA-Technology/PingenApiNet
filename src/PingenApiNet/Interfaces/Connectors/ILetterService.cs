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

using System.Runtime.InteropServices;
using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Exceptions;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.LetterEvents;
using PingenApiNet.Abstractions.Models.LetterPrices;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Letters.Views;
using PingenApiNet.Interfaces.Connectors.Base;

namespace PingenApiNet.Interfaces.Connectors;

/// <summary>
/// Pingen letter service endpoint. <see href="https://api.v2.pingen.com/documentation#tag/letters.general">API Doc - Letters General</see>
/// </summary>
public interface ILetterService : IConnectorService
{
    /// <summary>
    /// Get a collection of letters. <see href="https://api.v2.pingen.com/documentation#tag/letters.general/operation/letters.list">API Doc - Letters list</see>
    /// </summary>>
    /// <param name="apiPagingRequest">Optional, Request meta information to send to the API</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult<CollectionResult<LetterData>>> GetPage([Optional] ApiPagingRequest? apiPagingRequest, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Call <see cref="GetPage"/> and auto page until end of collection
    /// </summary>
    /// <param name="apiPagingRequest">Optional, Request meta information to send to the API (where page number is the first page to start auto paging until end of collection)</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>Pages from <see cref="GetPage"/> asynchronously</returns>
    /// <exception cref="PingenApiErrorException"></exception>
    public IAsyncEnumerable<IEnumerable<LetterData>> GetPageResultsAsync([Optional] ApiPagingRequest? apiPagingRequest, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a new letter. <see href="https://api.v2.pingen.com/documentation#tag/letters.general/operation/letters.create">API Doc - Letters create</see>
    /// <br/>Important: The 3-Step Process to Create a new letter
    /// <br/>1. Make a GET Request to the (File upload) endpoint to request an upload url. Use <see cref="IFilesService.GetPath"/> on <see cref="IPingenApiClient.Files"/>
    /// <br/>2. Send the raw PDF Binary file via PUT Request (NOT Form-Post) to the url received in Step 1. Use <see cref="IFilesService.UploadFile"/> on <see cref="IPingenApiClient.Files"/>
    /// <br/>3. Make a POST Request to the Create Letter Endpoint passing the file url and file signature you received in Step 1. <see cref="Create"/>
    /// </summary>
    /// <param name="data">Data for POST</param>
    /// <param name="idempotencyKey">Optional, unique request identifier for idempotency. To be able to safely retry these kind of API calls, you can set the HTTP Header Idempotency-Key with any unique 1-64 character string. <see href="https://api.v2.pingen.com/documentation#section/Advanced/Idempotency">API Doc - Idempotency</see></param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult<SingleResult<LetterDataDetailed>>> Create(DataPost<LetterCreate> data, [Optional] string? idempotencyKey, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Send a letter. <see href="https://api.v2.pingen.com/documentation#tag/letters.general/operation/letters.send">API Doc - Letters send</see>
    /// </summary>
    /// <param name="data">Data for PATCH</param>
    /// <param name="idempotencyKey">Optional, unique request identifier for idempotency. To be able to safely retry these kind of API calls, you can set the HTTP Header Idempotency-Key with any unique 1-64 character string. <see href="https://api.v2.pingen.com/documentation#section/Advanced/Idempotency">API Doc - Idempotency</see></param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult<SingleResult<LetterDataDetailed>>> Send(DataPatch<LetterSend> data, [Optional] string? idempotencyKey, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Send a letter. <see href="https://api.v2.pingen.com/documentation#tag/letters.general/operation/letters.send">API Doc - Letters send</see>
    /// </summary>
    /// <param name="letterId">ID of the letter to cancel</param>
    /// <param name="idempotencyKey">Optional, unique request identifier for idempotency. To be able to safely retry these kind of API calls, you can set the HTTP Header Idempotency-Key with any unique 1-64 character string. <see href="https://api.v2.pingen.com/documentation#section/Advanced/Idempotency">API Doc - Idempotency</see></param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult> Cancel(string letterId, [Optional] string? idempotencyKey, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Get details of a letter. <see href="https://api.v2.pingen.com/documentation#tag/letters.general/operation/letters.show">API Doc - Letters show</see>
    /// </summary>
    /// <param name="letterId">ID of the letter to get</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult<SingleResult<LetterDataDetailed>>> Get(string letterId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete a letter. <see href="https://api.v2.pingen.com/documentation#tag/letters.general/operation/letters.delete">API Doc - Letters delete</see>
    /// </summary>
    /// <param name="letterId">ID of the letter to delete</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult> Delete(string letterId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Edit a letter. <see href="https://api.v2.pingen.com/documentation#tag/letters.general/operation/letters.edit">API Doc - Letters edit</see>
    /// </summary>
    /// <param name="data">API request with data for PATCH</param>
    /// <param name="idempotencyKey">Optional, unique request identifier for idempotency. To be able to safely retry these kind of API calls, you can set the HTTP Header Idempotency-Key with any unique 1-64 character string. <see href="https://api.v2.pingen.com/documentation#section/Advanced/Idempotency">API Doc - Idempotency</see></param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult<SingleResult<LetterDataDetailed>>> Update(DataPatch<LetterUpdate> data, [Optional] string? idempotencyKey, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Get file of letter. (Redirect URL only) <see href="https://api.v2.pingen.com/documentation#tag/letters.general/operation/letters.file">API Doc - Letters file</see>
    /// <br/>API returns a 302 Found with the file URL in Location header. Get url from <see cref="ApiResult.Location"/>.
    /// </summary>
    /// <param name="letterId">ID of the letter to get the file from</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns><see cref="ApiResult"/> with <see cref="ApiResult.Location"/></returns>
    public Task<ApiResult> GetFileLocation(string letterId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Download file from URL received by <see cref="GetFileLocation"/> <see href="https://api.v2.pingen.com/documentation#tag/letters.general/operation/letters.file">API Doc - Letters file</see>
    /// </summary>
    /// <param name="fileUrl"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="PingenFileDownloadException"></exception>
    public Task<MemoryStream> DownloadFileContent(Uri fileUrl, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Calculate price for given letter configuration. <see href="https://api.v2.pingen.com/documentation#tag/letters.general/operation/letters.price-calculator">API Doc - Letters price calculator</see>
    /// </summary>
    /// <param name="data">Data for POST</param>
    /// <param name="idempotencyKey">Optional, unique request identifier for idempotency. To be able to safely retry these kind of API calls, you can set the HTTP Header Idempotency-Key with any unique 1-64 character string. <see href="https://api.v2.pingen.com/documentation#section/Advanced/Idempotency">API Doc - Idempotency</see></param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult<SingleResult<LetterPriceData>>> CalculatePrice(DataPost<LetterPriceConfiguration> data, [Optional] string? idempotencyKey, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Get a collection of letter events. <see href="https://api.v2.pingen.com/documentation#tag/letters.events/operation/letters.events">API Doc - Letters events</see>
    /// </summary>>
    /// <param name="letterId">ID of letter to get events</param>
    /// <param name="language">Language for events. Default: "en". Nope, API Doc is wrong!! Culture names required, e.g. 'en-GB'. Use any of <see cref="PingenApiLanguage"/></param>
    /// <param name="apiPagingRequest">Optional, Request meta information to send to the API</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult<CollectionResult<LetterEventData>>> GetEventsPage(string letterId, string language, [Optional] ApiPagingRequest? apiPagingRequest, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Call <see cref="GetEventsPage"/> and auto page until end of collection
    /// </summary>
    /// <param name="letterId">ID of letter to get events</param>
    /// <param name="language">Language for events. Default: "en"</param>
    /// <param name="apiPagingRequest">Optional, Request meta information to send to the API (where page number is the first page to start auto paging until end of collection)</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>Pages from <see cref="GetEventsPage"/> asynchronously</returns>
    /// <exception cref="PingenApiErrorException"></exception>
    public IAsyncEnumerable<IEnumerable<LetterEventData>> GetEventsPageResultsAsync(string letterId, string language, [Optional] ApiPagingRequest? apiPagingRequest, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Get a collection of issues of all letters. <see href="https://api.v2.pingen.com/documentation#tag/letters.events/operation/letters.issues">API Doc - Letters issues</see>
    /// </summary>>
    /// <param name="language">Language for events. Default: "en"</param>
    /// <param name="apiPagingRequest">Optional, Request meta information to send to the API</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult<CollectionResult<LetterEventData>>> GetIssuesPage(string language, [Optional] ApiPagingRequest? apiPagingRequest, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Call <see cref="GetIssuesPage"/> and auto page until end of collection
    /// </summary>
    /// <param name="language">Language for events. Default: "en"</param>
    /// <param name="apiPagingRequest">Optional, Request meta information to send to the API (where page number is the first page to start auto paging until end of collection)</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>Pages from <see cref="GetIssuesPage"/> asynchronously</returns>
    /// <exception cref="PingenApiErrorException"></exception>
    public IAsyncEnumerable<IEnumerable<LetterEventData>> GetIssuesPageResultsAsync(string language, [Optional] ApiPagingRequest? apiPagingRequest, [Optional] CancellationToken cancellationToken);
}
