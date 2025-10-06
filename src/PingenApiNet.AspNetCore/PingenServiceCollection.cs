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

using Microsoft.Extensions.DependencyInjection;
using PingenApiNet.Interfaces;
using PingenApiNet.Interfaces.Connectors;
using PingenApiNet.Services;
using PingenApiNet.Services.Connectors;

namespace PingenApiNet.AspNetCore;

/// <summary>
/// Pingen service collection extension for dependency injection
/// </summary>
public static class PingenServiceCollection
{
    /// <summary>
    /// Adds the configuration, handler and rest service to the services
    /// </summary>
    /// <param name="services"></param>
    /// <param name="baseUri"></param>
    /// <param name="identityUri"></param>
    /// <param name="clientId"></param>
    /// <param name="clientSecret"></param>
    /// <param name="defaultOrganisationId"></param>
    /// <returns></returns>
    public static IServiceCollection AddPingenServices(this IServiceCollection services, string baseUri, string identityUri, string clientId, string clientSecret, string defaultOrganisationId)
    {
        return services.AddPingenServices(new PingenConfiguration
        {
            BaseUri = baseUri,
            IdentityUri = identityUri,
            ClientId = clientId,
            ClientSecret = clientSecret,
            DefaultOrganisationId = defaultOrganisationId
        });
    }

    /// <summary>
    /// Adds the configuration, handler and rest service to the services
    /// </summary>
    /// <param name="services"></param>
    /// <param name="pingenConfiguration"></param>
    /// <returns></returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IServiceCollection AddPingenServices(this IServiceCollection services, IPingenConfiguration pingenConfiguration)
    {
        services.AddSingleton(pingenConfiguration);
        services.AddSingleton<IPingenConnectionHandler, PingenConnectionHandler>();
        services.AddScoped<ILetterService, LetterService>();
        services.AddScoped<IBatchService, BatchService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IOrganisationService, OrganisationService>();
        services.AddScoped<IWebhookService, WebhookService>();
        services.AddScoped<IFilesService, FilesService>();
        services.AddScoped<IDistributionService, DistributionService>();
        services.AddScoped<IPingenApiClient, PingenApiClient>();

        return services;
    }
}
