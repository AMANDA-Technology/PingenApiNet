﻿/*
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
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Web;
using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Interfaces.Api;
using PingenApiNet.Abstractions.Models.API;
using PingenApiNet.Interfaces;
using PingenApiNet.Records;

namespace PingenApiNet.Services;

/// <inheritdoc />
public sealed class PingenConnectionHandler : IPingenConnectionHandler
{
    /// <summary>
    /// Pingen configuration for connection handler
    /// </summary>
    private readonly IPingenConfiguration _configuration;

    /// <summary>
    /// Holds the http client with some basic settings, to be used for all connectors
    /// </summary>
    private readonly HttpClient _client;

    /// <summary>
    /// Access token for authenticated requests
    /// </summary>
    private AccessToken? _accessToken;

    /// <summary>
    /// The organisation ID to use for all request at /organisations/{organisationId}/*
    /// </summary>
    private string _organisationId;

    /// <summary>
    /// Create a new connection handler to call pingen REST API
    /// </summary>
    /// <param name="configuration"></param>
    public PingenConnectionHandler(IPingenConfiguration configuration)
    {
        _configuration = configuration;
        _organisationId = configuration.DefaultOrganisationId;

        if (!_configuration.BaseUri.EndsWith("/"))
            _configuration.BaseUri += "/";

        if (!_configuration.IdentityUri.EndsWith("/"))
            _configuration.IdentityUri += "/";

        _client = new()
        {
            BaseAddress = new(_configuration.BaseUri)
        };
    }

    /// <summary>
    /// Set or update access token to use for authenticated requests
    /// </summary>
    /// <exception cref="Exception"></exception>
    private async Task SetOrUpdateAccessToken()
    {
        // Only update if needed
        if (_accessToken?.ExpiresAt.AddSeconds(-60) > DateTime.Now)
            return;

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
            var authenticationError = await JsonSerializer.DeserializeAsync<AuthenticationError>(await response.Content.ReadAsStreamAsync());

            if (authenticationError is null)
                throw new("Invalid authentication error received");

            throw new($"Failed to obtain token with error: {authenticationError.Message}");
        }

        // Try to get token
        _accessToken = await JsonSerializer.DeserializeAsync<AccessToken>(await response.Content.ReadAsStreamAsync());
        if (_accessToken == null) throw new("Invalid access token object received");

        // Set to base client
        _client.DefaultRequestHeaders.Authorization = new("Bearer", _accessToken.Token);
    }

    /// <inheritdoc />
    public void SetOrganisationId(string organisationId)
    {
        _organisationId = organisationId;
    }

    /// <inheritdoc />
    public async Task<ApiResult<TResult>> GetAsync<TResult>(string requestPath, [Optional] ApiRequest? apiRequest, [Optional] CancellationToken cancellationToken) where TResult : IDataResult
    {
        await SetOrUpdateAccessToken();
        return await GetApiResult<TResult>(await _client.SendAsync(GetHttpRequestMessage(HttpMethod.Get, requestPath, apiRequest), cancellationToken));
    }

    /// <inheritdoc />
    public async Task<ApiResult<TResult>> PostAsync<TResult, TPost>(string requestPath, ApiRequest<TPost> apiRequest, [Optional] CancellationToken cancellationToken) where TResult : IDataResult where TPost : IDataPost
    {
        await SetOrUpdateAccessToken();
        return await GetApiResult<TResult>(await _client.SendAsync(GetHttpRequestMessageWithBody(HttpMethod.Post, requestPath, apiRequest), cancellationToken));
    }

    /// <inheritdoc />
    public async Task<ApiResult<TResult>> DeleteAsync<TResult>(string requestPath, [Optional] ApiRequest? apiRequest, [Optional] CancellationToken cancellationToken) where TResult : IDataResult
    {
        await SetOrUpdateAccessToken();
        return await GetApiResult<TResult>(await _client.SendAsync(GetHttpRequestMessage(HttpMethod.Delete, requestPath, apiRequest), cancellationToken));
    }

    /// <inheritdoc />
    public async Task<ApiResult<TResult>> PatchAsync<TResult, TPost>(string requestPath, ApiRequest<TPost> apiRequest, [Optional] CancellationToken cancellationToken) where TResult : IDataResult where TPost : IDataPost
    {
        await SetOrUpdateAccessToken();
        return await GetApiResult<TResult>(await _client.SendAsync(GetHttpRequestMessageWithBody(HttpMethod.Patch, requestPath, apiRequest), cancellationToken));
    }

    /// <summary>
    /// Get http request message to send to the API (with body as json from data with type T)
    /// </summary>
    /// <param name="httpMethod"></param>
    /// <param name="requestPath"></param>
    /// <param name="apiRequest"></param>
    /// <returns></returns>
    private HttpRequestMessage GetHttpRequestMessageWithBody<T>(HttpMethod httpMethod, string requestPath, ApiRequest<T> apiRequest) where T : IDataPost
    {
        var httpRequestMessage = GetHttpRequestMessage(httpMethod, requestPath, apiRequest);

        httpRequestMessage.Content = apiRequest.Data is null
            ? null
            : new StringContent(JsonSerializer.Serialize(apiRequest.Data), Encoding.UTF8, "application/json");

        return httpRequestMessage;
    }

    /// <summary>
    /// Get http request message to send to the API (without body)
    /// </summary>
    /// <param name="requestPath"></param>
    /// <param name="apiRequest"></param>
    /// <param name="httpMethod"></param>
    /// <returns></returns>
    private HttpRequestMessage GetHttpRequestMessage(HttpMethod httpMethod, string requestPath, ApiRequest? apiRequest)
    {
        var httpRequestMessage = new HttpRequestMessage
        {
            Content = null,
            Method = httpMethod,
            RequestUri = null,
            Version = HttpVersion.Version20, // TODO: HTTP2 OK?
            VersionPolicy = HttpVersionPolicy.RequestVersionOrLower
        };
        var uriBuilder = new UriBuilder(new Uri(_client.BaseAddress!, $"organisations/{_organisationId}/{requestPath}"));
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        if (apiRequest is not null)
        {
            foreach (var (key, value) in GetRequestHeaders(apiRequest))
                httpRequestMessage.Headers.Add(key, value);

            foreach (var (key, value) in GetQueryParameters(apiRequest))
                query[key] = value;
        }

        uriBuilder.Query = query.ToString();
        httpRequestMessage.RequestUri = uriBuilder.Uri;

        return httpRequestMessage;
    }

    /// <summary>
    /// Get API request headers
    /// </summary>
    /// <param name="apiRequest"></param>
    /// <returns></returns>
    private static IEnumerable<KeyValuePair<string, string>> GetRequestHeaders(ApiRequest apiRequest)
    {
        if (apiRequest.IdempotencyKey.HasValue)
            yield return new(ApiHeaderNames.IdempotencyKey, apiRequest.IdempotencyKey.Value.ToString());
    }

    /// <summary>
    /// Get API request query parameters
    /// </summary>
    /// <param name="apiRequest"></param>
    /// <returns></returns>
    private static IEnumerable<KeyValuePair<string, string>> GetQueryParameters(ApiRequest apiRequest)
    {
        if (apiRequest.Sorting?.Any() is true)
            yield return new(ApiQueryParameterNames.Sorting, string.Join(',', apiRequest.Sorting.Select(entry => $"{(entry.Value is CollectionSortDirection.DESC ? "-" : string.Empty)}{entry.Key}")));

        if (apiRequest.Filtering?.Any() is true)
            yield return new(ApiQueryParameterNames.Filtering, JsonSerializer.Serialize(apiRequest.Filtering));

        if (!string.IsNullOrEmpty(apiRequest.Searching))
            yield return new(ApiQueryParameterNames.Searching, apiRequest.Searching);

        if (apiRequest.PageNumber.HasValue)
            yield return new(ApiQueryParameterNames.PageNumber, apiRequest.PageNumber.Value.ToString());

        if (apiRequest.PageLimit.HasValue)
            yield return new(ApiQueryParameterNames.PageLimit, apiRequest.PageLimit.Value.ToString());

        // TODO: Add Sparse fieldsets? https://api.v2.pingen.com/documentation#section/Advanced/Sparse-fieldsets

        // TODO: Add Including relationships? https://api.v2.pingen.com/documentation#section/Advanced/Including-relationships
    }

    /// <summary>
    /// Get API result from response
    /// </summary>
    /// <param name="httpResponseMessage"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private async Task<ApiResult<T>> GetApiResult<T>(HttpResponseMessage httpResponseMessage) where T : IDataResult
    {
        var headers = GetResponseHeaders(httpResponseMessage);

        if (httpResponseMessage.IsSuccessStatusCode)
            return new()
            {
                IsSuccess = true,
                Data = JsonSerializer.Deserialize<T>(await httpResponseMessage.Content.ReadAsStringAsync()),
                RequestId = (Guid) (headers[ApiHeaderNames.RequestId] ?? Guid.Empty),
                RateLimitLimit = (int) (headers[ApiHeaderNames.RateLimitLimit] ?? 0),
                RateLimitRemaining = (int) (headers[ApiHeaderNames.RateLimitRemaining] ?? 0),
                RateLimitReset = (DateTime?) headers[ApiHeaderNames.RateLimitReset],
                RetryAfter = (int?) headers[ApiHeaderNames.RetryAfter],
                IdempotentReplayed = (bool) (headers[ApiHeaderNames.IdempotentReplayed] ?? false),
                ApiError = null
            };

        return new()
        {
            IsSuccess = false,
            Data = default,
            RequestId = (Guid) (headers[ApiHeaderNames.RequestId] ?? Guid.Empty),
            RateLimitLimit = (int) (headers[ApiHeaderNames.RateLimitLimit] ?? 0),
            RateLimitRemaining = (int) (headers[ApiHeaderNames.RateLimitRemaining] ?? 0),
            RateLimitReset = (DateTime?) headers[ApiHeaderNames.RateLimitReset],
            RetryAfter = (int?) headers[ApiHeaderNames.RetryAfter],
            IdempotentReplayed = (bool) (headers[ApiHeaderNames.IdempotentReplayed] ?? false),
            ApiError = JsonSerializer.Deserialize<ApiError>(await httpResponseMessage.Content.ReadAsStringAsync())
        };
    }

    /// <summary>
    /// Get API headers from response
    /// </summary>
    /// <param name="httpResponseMessage"></param>
    /// <returns></returns>
    private Dictionary<string, object?> GetResponseHeaders(HttpResponseMessage httpResponseMessage)
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

            [ApiHeaderNames.RateLimitReset] = httpResponseMessage.Headers.TryGetValues(ApiHeaderNames.RateLimitReset, out values) && DateTime.TryParse(values.First(), out var rateLimitReset)
                ? rateLimitReset
                : null,

            [ApiHeaderNames.RetryAfter] = httpResponseMessage.Headers.TryGetValues(ApiHeaderNames.RetryAfter, out values) && int.TryParse(values.First(), out var retryAfter)
                ? retryAfter
                : null,

            [ApiHeaderNames.IdempotentReplayed] = httpResponseMessage.Headers.TryGetValues(ApiHeaderNames.IdempotentReplayed, out values)
                                                  && bool.TryParse(values.First(), out var idempotentReplayed)
                                                  && idempotentReplayed
        };
    }
}
