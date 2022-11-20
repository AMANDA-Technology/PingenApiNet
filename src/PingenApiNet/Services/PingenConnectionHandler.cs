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

using System.Text.Json;
using PingenApiNet.Interfaces;
using PingenApiNet.Records;

namespace PingenApiNet.Services;

/// <summary>
/// Connection handler to call pingen REST API
/// </summary>
public sealed class PingenConnectionHandler : IPingenConnectionHandler
{
    /// <summary>
    /// Holds the http client with some basic settings, to be used for all connectors
    /// </summary>
    public HttpClient Client { get; }

    /// <summary>
    ///
    /// </summary>
    private AccessToken? _accessToken;

    /// <summary>
    ///
    /// </summary>
    private readonly IPingenConfiguration _configuration;

    /// <summary>
    ///
    /// </summary>
    /// <param name="configuration"></param>
    public PingenConnectionHandler(IPingenConfiguration configuration)
    {
        _configuration = configuration;

        if (!_configuration.BaseUri.EndsWith("/"))
            _configuration.BaseUri += "/";

        if (!_configuration.IdentityUri.EndsWith("/"))
            _configuration.IdentityUri += "/";

        Client = new()
        {
            BaseAddress = new(_configuration.BaseUri)
        };
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public async Task SetOrUpdateAccessToken()
    {
        // Only update if needed
        if (_accessToken?.ExpiresAt.AddSeconds(-60) > DateTime.Now)
        {
            return;
        }

        // Create client and set header
        using var identityClient = new HttpClient();
        identityClient.DefaultRequestHeaders.Accept.Clear();
        identityClient.DefaultRequestHeaders.Accept.Add(new("application/x-www-form-urlencoded"));

        // Set content
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _configuration.ClientId),
            new KeyValuePair<string, string>("client_secret", _configuration.ClientSecret),
            //new KeyValuePair<string, string>("scope", "client-id-source"),
        });

        // Send request and validate
        var response = await identityClient.PostAsync($"{_configuration.IdentityUri}auth/access-tokens", content);

        if (!response.IsSuccessStatusCode)
        {
            var authenticationError = await JsonSerializer
                .DeserializeAsync<AuthenticationError>(await response.Content.ReadAsStreamAsync());

            if (authenticationError is null)
            {
                throw new("Invalid authentication error received");
            }

            throw new ($"Failed to obtain token with error: {authenticationError.Message}");
        }

        // Try to get token
        _accessToken = await JsonSerializer.DeserializeAsync<AccessToken>(await response.Content.ReadAsStreamAsync());
        if (_accessToken == null) throw new("Invalid access token object received");

        // Set to base client
        Client.DefaultRequestHeaders.Authorization = new("Bearer", _accessToken.Token);
    }
}
