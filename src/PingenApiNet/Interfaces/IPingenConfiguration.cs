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

namespace PingenApiNet.Interfaces;

/// <summary>
/// Configuration for accessing Pingen API
/// </summary>
public interface IPingenConfiguration
{
    /// <summary>
    /// Base URI for accessing the service. <see href="https://api.v2.pingen.com/documentation#section/Basics/Environments">API Doc - Environments</see>
    /// </summary>
    public string BaseUri { get; set; }

    /// <summary>
    /// Identity URI to obtain access token. <see href="https://api.v2.pingen.com/documentation#section/Basics/Environments">API Doc - Environments</see>
    /// </summary>
    public string IdentityUri { get; set; }

    /// <summary>
    /// Generated client id. <see href="https://api.v2.pingen.com/documentation#section/Authentication">API Doc - Authentication</see>
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Generated client secret. <see href="https://api.v2.pingen.com/documentation#section/Authentication">API Doc - Authentication</see>
    /// </summary>
    public string ClientSecret { get; set; }

    /// <summary>
    /// Default organisation ID to use. Can be changed later using <see cref="IPingenApiClient.SetOrganisationId"/>. <see href="https://api.v2.pingen.com/documentation#section/Quickstart">API Doc - Quickstart</see>
    /// </summary>
    public string DefaultOrganisationId { get; set; }

    /// <summary>
    /// Dictionary with webhook guid and the assigned signing key.
    /// The signing key is also stated as secret sometimes in the pingen documentation.
    /// </summary>
    public Dictionary<string, string>? WebhookSigningKeys { get; set; }
}
