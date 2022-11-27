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

using PingenApiNet.Interfaces;
using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Services.Connectors;

namespace PingenApiNet.Services;

/// <inheritdoc />
public sealed class PingenApiClient : IPingenApiClient
{
    /// <summary>
    /// Private instance of connection handler used for all Services
    /// </summary>
    private readonly IPingenConnectionHandler _pingenConnectionHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="PingenApiClient"/> class.
    /// </summary>
    public PingenApiClient(IPingenConnectionHandler pingenConnectionHandler)
    {
        _pingenConnectionHandler = pingenConnectionHandler;

        // TODO: Shouldn't these be injected?
        Letters = new LetterService(_pingenConnectionHandler);
        Users = new UserService(_pingenConnectionHandler);
        Organisations = new OrganisationService(_pingenConnectionHandler);
        Webhooks = new WebhookService(_pingenConnectionHandler);
        Files = new FilesService(_pingenConnectionHandler);
    }

    /// <inheritdoc />
    public void SetOrganisationId(string organisationId)
    {
        _pingenConnectionHandler.SetOrganisationId(organisationId);
    }

    /// <inheritdoc />
    public ILetterService Letters { get; set; }

    /// <inheritdoc />
    public IUserService Users { get; set; }

    /// <inheritdoc />
    public IOrganisationService Organisations { get; set; }

    /// <inheritdoc />
    public IWebhookService Webhooks { get; set; }

    /// <inheritdoc />
    public IFilesService Files { get; set; }
}
