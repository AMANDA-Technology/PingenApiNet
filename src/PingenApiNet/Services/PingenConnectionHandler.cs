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
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Web;
using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Interfaces.Data;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Interfaces;
using PingenApiNet.Models;
using PingenApiNet.Services.Connectors.Endpoints;

namespace PingenApiNet.Services;

/// <inheritdoc />
public sealed class PingenConnectionHandler : IPingenConnectionHandler
{
    /// <summary>
    /// Endpoints that do not use organisation ID in path
    /// </summary>
    private static readonly string[] NonOrganisationEndpoints = [FileUploadEndpoints.FileUpload, UsersEndpoints.Root, OrganisationsEndpoints.Root];

    /// <summary>
    /// Pingen configuration for connection handler
    /// </summary>
    private readonly IPingenConfiguration _configuration;

    /// <summary>
    /// Holds the http client with some basic settings, to be used for all connectors
    /// </summary>
    private readonly HttpClient _client;

    /// <summary>
    /// The organisation ID to use for all request at /organisations/{organisationId}/*
    /// </summary>
    private string? _organisationId;

    /// <summary>
    /// Access token for authenticated requests
    /// </summary>
    private static AccessToken? _accessToken;

    /// <summary>
    /// Semaphore to ensure that only one thread tries to re-/authenticate at a time
    /// </summary>
    private static readonly SemaphoreSlim AuthenticationSemaphore = new(1, 1);

    /// <summary>
    /// Indicates if the authentication semaphore has been entered or not
    /// </summary>
    private bool _isEnteredAuthenticationSemaphore;

    /// <summary>
    /// Initializes a new instance of the <see cref="PingenConnectionHandler"/> class.
    /// </summary>
    /// <param name="configuration"></param>
    public PingenConnectionHandler(IPingenConfiguration configuration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configuration.BaseUri);
        ArgumentException.ThrowIfNullOrWhiteSpace(configuration.IdentityUri);

        // Configure
        _configuration = configuration;
        _organisationId = configuration.DefaultOrganisationId;

        // Normalize base urls
        if (!_configuration.BaseUri.EndsWith('/'))
            _configuration.BaseUri += '/';

        if (!_configuration.IdentityUri.EndsWith('/'))
            _configuration.IdentityUri += '/';

        // Create a new http client for instance
        _client = new(new HttpClientHandler { AllowAutoRedirect = false })
        {
            BaseAddress = new(_configuration.BaseUri)
        };

        // Try authorize when static access token is set
        TryAuthorizeHttpClient();
    }

    /// <summary>
    /// Set or update access token to use for authenticated requests
    /// </summary>
    /// <exception cref="Exception"></exception>
    private async Task SetOrUpdateAccessToken()
    {
        // Only update if needed
        if (IsAuthorized())
            return;

        // Wait for semaphore entrance
        if (!await AuthenticationSemaphore.WaitAsync(TimeSpan.FromSeconds(10)))
        {
            throw new InvalidOperationException("Authentication semaphore entrance timeout");
        }
        try
        {
            _isEnteredAuthenticationSemaphore = true;
            await Login();
        }
        finally
        {
            AuthenticationSemaphore.Release();
            _isEnteredAuthenticationSemaphore = false;
        }
    }

    /// <summary>
    /// Check if access token is set and not expired
    /// </summary>
    /// <returns></returns>
    private static bool IsAuthorized() => _accessToken is not null && _accessToken.ExpiresAt.AddMinutes(-1) > DateTime.Now;

    /// <summary>
    /// Re-/Authenticate, save access token and authorize the http client
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task Login()
    {
        // Preconditions
        if (IsAuthorized())
            return;

        if (!_isEnteredAuthenticationSemaphore)
            throw new InvalidOperationException("Login is not allowed without entering the authentication semaphore");

        // Create client and set header
        using var identityClient = new HttpClient();
        identityClient.DefaultRequestHeaders.Accept.Clear();
        identityClient.DefaultRequestHeaders.Accept.Add(new("application/x-www-form-urlencoded"));

        // Send request and validate
        var response = await identityClient.PostAsync($"{_configuration.IdentityUri}auth/access-tokens", new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _configuration.ClientId),
            new KeyValuePair<string, string>("client_secret", _configuration.ClientSecret),
            //new KeyValuePair<string, string>("scope", "client-id-source"),
        }));

        if (!response.IsSuccessStatusCode)
        {
            var authenticationError = await PingenSerialisationHelper.DeserializeAsync<AuthenticationError>(await response.Content.ReadAsStreamAsync());

            if (authenticationError is null)
                throw new InvalidOperationException("Invalid authentication error received");

            throw new InvalidOperationException($"Failed to obtain token with error: {authenticationError.Message}");
        }

        // Try to get token
        _accessToken = await PingenSerialisationHelper.DeserializeAsync<AccessToken>(await response.Content.ReadAsStreamAsync());
        if (!IsAuthorized())
            throw new InvalidOperationException("Invalid access token object received");

        // Authorize http client
        if (!TryAuthorizeHttpClient())
            throw new InvalidOperationException("Failed to authorize http client");
    }

    /// <summary>
    /// Set authorization to http client
    /// </summary>
    private bool TryAuthorizeHttpClient()
    {
        if (!IsAuthorized())
            return false;

        _client.DefaultRequestHeaders.Authorization = new("Bearer", _accessToken!.Token);
        return true;
    }

    /// <inheritdoc />
    public void SetOrganisationId(string organisationId)
    {
        _organisationId = organisationId;
    }

    /// <inheritdoc />
    public async Task<ApiResult<TResult>> GetAsync<TResult>(string requestPath, [Optional] ApiPagingRequest? apiPagingRequest, [Optional] CancellationToken cancellationToken) where TResult : IDataResult
    {
        await SetOrUpdateAccessToken();
        return await GetApiResult<TResult>(await _client.SendAsync(GetHttpRequestMessage(HttpMethod.Get, requestPath, apiPagingRequest), cancellationToken));
    }

    /// <inheritdoc />
    public async Task<ApiResult> GetAsync(string requestPath, [Optional] ApiPagingRequest? apiPagingRequest, [Optional] CancellationToken cancellationToken)
    {
        await SetOrUpdateAccessToken();
        return await GetApiResult(await _client.SendAsync(GetHttpRequestMessage(HttpMethod.Get, requestPath, apiPagingRequest), cancellationToken));
    }

    /// <inheritdoc />
    public async Task<ApiResult<TResult>> PostAsync<TResult, TPost>(string requestPath, TPost dataPost, [Optional] string? idempotencyKey, [Optional] CancellationToken cancellationToken) where TResult : IDataResult where TPost : IDataPost
    {
        await SetOrUpdateAccessToken();
        var res = await _client.SendAsync(GetHttpRequestMessageWithBody(HttpMethod.Post, requestPath, dataPost, null, idempotencyKey), cancellationToken);
        return await GetApiResult<TResult>(res);
    }

    /// <inheritdoc />
    public async Task<ApiResult> DeleteAsync(string requestPath, [Optional] CancellationToken cancellationToken)
    {
        await SetOrUpdateAccessToken();
        return await GetApiResult(await _client.SendAsync(GetHttpRequestMessage(HttpMethod.Delete, requestPath), cancellationToken));
    }

    /// <inheritdoc cref="PatchAsync" />
    public async Task<ApiResult> PatchAsync(string requestPath, [Optional] string? idempotencyKey, [Optional] CancellationToken cancellationToken)
    {
        await SetOrUpdateAccessToken();
        return await GetApiResult(await _client.SendAsync(GetHttpRequestMessage(HttpMethod.Patch, requestPath, null, idempotencyKey), cancellationToken));
    }

    /// <inheritdoc cref="PatchAsync{TResult,TPost}" />
    public async Task<ApiResult<TResult>> PatchAsync<TResult, TPatch>(string requestPath, TPatch data, [Optional] string? idempotencyKey, [Optional] CancellationToken cancellationToken) where TResult : IDataResult where TPatch : IDataPatch
    {
        await SetOrUpdateAccessToken();
        return await GetApiResult<TResult>(await _client.SendAsync(GetHttpRequestMessageWithBody(HttpMethod.Patch, requestPath, data, null, idempotencyKey), cancellationToken));
    }

    /// <summary>
    /// Get http request message to send to the API (with body as json from data with type T)
    /// </summary>
    /// <param name="httpMethod"></param>
    /// <param name="requestPath"></param>
    /// <param name="payload"></param>
    /// <param name="apiRequest"></param>
    /// <param name="idempotencyKey"></param>
    /// <returns></returns>
    private HttpRequestMessage GetHttpRequestMessageWithBody<T>(HttpMethod httpMethod, string requestPath, T? payload, [Optional] ApiRequest? apiRequest, [Optional] string? idempotencyKey) where T : IDataPost
    {
        var httpRequestMessage = GetHttpRequestMessage(httpMethod, requestPath, apiRequest, idempotencyKey);

        httpRequestMessage.Content = payload is null
            ? null
            : new StringContent(PingenSerialisationHelper.Serialize(new{ data = payload }), new MediaTypeHeaderValue("application/vnd.api+json"));

        return httpRequestMessage;
    }

    /// <summary>
    /// Get http request message to send to the API (without body)
    /// </summary>
    /// <param name="requestPath"></param>
    /// <param name="apiRequest"></param>
    /// <param name="httpMethod"></param>
    /// <param name="idempotencyKey"></param>
    /// <returns></returns>
    private HttpRequestMessage GetHttpRequestMessage(HttpMethod httpMethod, string requestPath, [Optional] ApiRequest? apiRequest, [Optional] string? idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(_organisationId))
            throw new InvalidOperationException("Organisation ID has not been set");

        var httpRequestMessage = new HttpRequestMessage { Method = httpMethod };

        var uriBuilder = new UriBuilder(new Uri(_client.BaseAddress!,
            Array.Exists(NonOrganisationEndpoints, requestPath.StartsWith)
                ? requestPath
                : $"{OrganisationsEndpoints.Single(_organisationId)}/{requestPath}"));

        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        foreach (var (key, value) in GetRequestHeaders(apiRequest, idempotencyKey))
            httpRequestMessage.Headers.Add(key, value);

        foreach (var (key, value) in GetQueryParameters(apiRequest))
            query[key] = value;

        uriBuilder.Query = query.ToString();
        httpRequestMessage.RequestUri = uriBuilder.Uri;

        return httpRequestMessage;
    }

    /// <summary>
    /// Get API request headers
    /// </summary>
    /// <param name="apiRequest"></param>
    /// <param name="idempotencyKey"></param>
    /// <returns></returns>
    private static IEnumerable<KeyValuePair<string, string>> GetRequestHeaders(ApiRequest? apiRequest, [Optional] string? idempotencyKey)
    {
        if (!string.IsNullOrEmpty(idempotencyKey))
            yield return new(ApiHeaderNames.IdempotencyKey, idempotencyKey);

        if (apiRequest is not null)
        {
            // nothing
        }
    }

    /// <summary>
    /// Get API request query parameters
    /// </summary>
    /// <param name="apiRequest"></param>
    /// <returns></returns>
    private static IEnumerable<KeyValuePair<string, string>> GetQueryParameters(ApiRequest? apiRequest)
    {
        if (apiRequest is null)
            yield break;

        // TODO: Add Sparse fieldsets? https://api.pingen.com/documentation#section/Advanced/Sparse-fieldsets

        // TODO: Add Including relationships? https://api.pingen.com/documentation#section/Advanced/Including-relationships

        if (apiRequest is not ApiPagingRequest apiPagingRequest)
            yield break;

        foreach (var keyValuePair in GetQueryParameters(apiPagingRequest))
            yield return keyValuePair;
    }

    /// <summary>
    /// Get API request query parameters
    /// </summary>
    /// <param name="apiPagingRequest"></param>
    /// <returns></returns>
    private static IEnumerable<KeyValuePair<string, string>> GetQueryParameters(ApiPagingRequest? apiPagingRequest)
    {
        if (apiPagingRequest is null)
            yield break;

        if (apiPagingRequest.Sorting?.Any() is true)
            yield return new(ApiQueryParameterNames.Sorting, string.Join(',', apiPagingRequest.Sorting.Select(entry => $"{(entry.Value is CollectionSortDirection.DESC ? "-" : string.Empty)}{entry.Key[..1].ToLower() + entry.Key[1..]}")));

        if (apiPagingRequest.Filtering.HasValue)
            yield return new(ApiQueryParameterNames.Filtering, PingenSerialisationHelper.Serialize(apiPagingRequest.Filtering.Value));

        if (!string.IsNullOrEmpty(apiPagingRequest.Searching))
            yield return new(ApiQueryParameterNames.Searching, apiPagingRequest.Searching);

        if (apiPagingRequest.PageNumber.HasValue)
            yield return new(ApiQueryParameterNames.PageNumber, apiPagingRequest.PageNumber.Value.ToString());

        if (apiPagingRequest.PageLimit.HasValue)
            yield return new(ApiQueryParameterNames.PageLimit, apiPagingRequest.PageLimit.Value.ToString());
    }

    /// <summary>
    /// Get API result from response
    /// </summary>
    /// <param name="httpResponseMessage"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private static async Task<ApiResult<T>> GetApiResult<T>(HttpResponseMessage httpResponseMessage) where T : IDataResult
    {
        var isSuccess = httpResponseMessage.IsSuccessStatusCode || httpResponseMessage.StatusCode is HttpStatusCode.Found;
        var headers = GetResponseHeaders(httpResponseMessage);
        var content = await httpResponseMessage.Content.ReadAsStringAsync();

        return new()
        {
            IsSuccess = isSuccess,
            RequestId = (Guid) (headers[ApiHeaderNames.RequestId] ?? Guid.Empty),
            RateLimitLimit = (int) (headers[ApiHeaderNames.RateLimitLimit] ?? 0),
            RateLimitRemaining = (int) (headers[ApiHeaderNames.RateLimitRemaining] ?? 0),
            RateLimitReset = (DateTimeOffset?) headers[ApiHeaderNames.RateLimitReset],
            RetryAfter = (int?) headers[ApiHeaderNames.RetryAfter],
            IdempotentReplayed = (bool) (headers[ApiHeaderNames.IdempotentReplayed] ?? false),
            ApiError = isSuccess ? null : PingenSerialisationHelper.Deserialize<ApiError>(content),
            Location = (Uri?) headers[ApiHeaderNames.Location],
            Data = httpResponseMessage.IsSuccessStatusCode ? PingenSerialisationHelper.Deserialize<T>(content) : default
        };
    }

    /// <summary>
    /// Get API result from response
    /// </summary>
    /// <param name="httpResponseMessage"></param>
    /// <returns></returns>
    private static async Task<ApiResult> GetApiResult(HttpResponseMessage httpResponseMessage)
    {
        var isSuccess = httpResponseMessage.IsSuccessStatusCode || httpResponseMessage.StatusCode is HttpStatusCode.Found;
        var headers = GetResponseHeaders(httpResponseMessage);
        var content = await httpResponseMessage.Content.ReadAsStringAsync();

        return new()
        {
            IsSuccess = isSuccess,
            RequestId = (Guid) (headers[ApiHeaderNames.RequestId] ?? Guid.Empty),
            RateLimitLimit = (int) (headers[ApiHeaderNames.RateLimitLimit] ?? 0),
            RateLimitRemaining = (int) (headers[ApiHeaderNames.RateLimitRemaining] ?? 0),
            RateLimitReset = (DateTimeOffset?) headers[ApiHeaderNames.RateLimitReset],
            RetryAfter = (int?) headers[ApiHeaderNames.RetryAfter],
            IdempotentReplayed = (bool) (headers[ApiHeaderNames.IdempotentReplayed] ?? false),
            ApiError = isSuccess ? null : PingenSerialisationHelper.Deserialize<ApiError>(content),
            Location = (Uri?) headers[ApiHeaderNames.Location]
        };
    }

    /// <summary>
    /// Get API headers from response
    /// </summary>
    /// <param name="httpResponseMessage"></param>
    /// <returns></returns>
    private static Dictionary<string, object?> GetResponseHeaders(HttpResponseMessage httpResponseMessage)
    {
        return new()
        {
            [ApiHeaderNames.RequestId] = httpResponseMessage.Headers.TryGetValues(ApiHeaderNames.RequestId, out var values) && Guid.TryParse(values.First(), out var requestId)
                ? requestId
                : Guid.Empty,

            [ApiHeaderNames.RateLimitLimit] = httpResponseMessage.Headers.TryGetValues(ApiHeaderNames.RateLimitLimit, out values) && int.TryParse(values.First(), out var rateLimitLimit)
                ? rateLimitLimit
                : 0,

            [ApiHeaderNames.RateLimitRemaining] = httpResponseMessage.Headers.TryGetValues(ApiHeaderNames.RateLimitRemaining, out values) && int.TryParse(values.First(), out var rateLimitRemaining)
                ? rateLimitRemaining
                : 0,

            [ApiHeaderNames.RateLimitReset] = httpResponseMessage.Headers.TryGetValues(ApiHeaderNames.RateLimitReset, out values) && long.TryParse(values.First(), out var rateLimitResetUnix)
                ? DateTimeOffset.FromUnixTimeSeconds(rateLimitResetUnix)
                : null,

            [ApiHeaderNames.RetryAfter] = httpResponseMessage.Headers.TryGetValues(ApiHeaderNames.RetryAfter, out values) && int.TryParse(values.First(), out var retryAfter)
                ? retryAfter
                : null,

            [ApiHeaderNames.IdempotentReplayed] = httpResponseMessage.Headers.TryGetValues(ApiHeaderNames.IdempotentReplayed, out values)
                                                  && bool.TryParse(values.First(), out var idempotentReplayed)
                                                  && idempotentReplayed,

            [ApiHeaderNames.Location] = httpResponseMessage.Headers.TryGetValues(ApiHeaderNames.Location, out values) && Uri.TryCreate(values.First(), UriKind.Absolute, out var location)
                ? location
                : null,

            [ApiHeaderNames.Signature] = httpResponseMessage.Headers.TryGetValues(ApiHeaderNames.Signature, out values)
                ? values.First()
                : string.Empty
        };
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
    }
}
