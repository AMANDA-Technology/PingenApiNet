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

using PingenApiNet.Interfaces.Connectors;

namespace PingenApiNet.Interfaces;

/// <summary>
/// Connector service to call Pingen REST API. <see href="https://api.pingen.com/documentation">API Doc</see>
/// </summary>
public interface IPingenApiClient
{
    /// <summary>
    /// Change the organisation ID to use for upcoming requests
    /// </summary>
    /// <param name="organisationId">Id to use for all requests at /organisations/{organisationId}/*</param>
    public void SetOrganisationId(string organisationId);

    /// <summary>
    /// Pingen letters connector. <see href="https://api.pingen.com/documentation#tag/letters.general">API Doc - Letters General</see>
    /// </summary>
    public ILetterService Letters { get; set; }

    /// <summary>
    /// Pingen users connector. <see href="https://api.pingen.com/documentation#tag/user.general">API Doc - Users General</see>
    /// </summary>
    public IUserService Users { get; set; }

    /// <summary>
    /// Pingen organisations connector. <see href="https://api.pingen.com/documentation#tag/organisations.general">API Doc - Organisations General</see>
    /// </summary>
    public IOrganisationService Organisations { get; set; }

    /// <summary>
    /// Pingen webhooks connector. <see href="https://api.pingen.com/documentation#tag/organisations.management.webhooks">API Doc - Webhooks</see>
    /// </summary>
    public IWebhookService Webhooks { get; set; }

    /// <summary>
    /// Pingen files connector. <see href="https://api.pingen.com/documentation#tag/misc.files">API Doc - Files</see>
    /// </summary>
    public IFilesService Files { get; set; }

    /// <summary>
    /// Pingen distribution connector.
    /// Undocumented endpoint, use at own risk.
    /// </summary>
    public IDistributionService Distributions { get; set; }
}
