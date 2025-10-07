using PingenApiNet.Interfaces;

namespace PingenApiNet.Services;

/// <summary>
/// Pingen http clients
/// </summary>
/// <param name="identityClient"></param>
/// <param name="apiClient"></param>
/// <param name="externalClient"></param>
public class PingenHttpClients(HttpClient identityClient, HttpClient apiClient, HttpClient externalClient)
{
    /// <summary>
    /// HttpClient names
    /// </summary>
    public static class Names
    {
        /// <summary>
        /// Name of pingen identity http client
        /// </summary>
        public const string Identity = "Pingen.Identity";

        /// <summary>
        /// Name of pingen api http client
        /// </summary>
        public const string Api = "Pingen.Api";

        /// <summary>
        /// Name of pingen files http client
        /// </summary>
        public const string Files = "Pingen.Files";
    }

    /// <summary>
    /// Instance of pingen identity http client
    /// </summary>
    public HttpClient Identity { get; } = identityClient;

    /// <summary>
    /// Instance of pingen api http client
    /// </summary>
    public HttpClient Api { get; } = apiClient;

    /// <summary>
    /// Instance of pingen external http client (not pre-configured). This client is used for file up/download.
    /// </summary>
    public HttpClient External { get; } = externalClient;

    /// <summary>
    /// Creates a new instance of <see cref="PingenHttpClients"/> with http clients registered at factory.
    /// </summary>
    /// <param name="factory"></param>
    public PingenHttpClients(IHttpClientFactory factory)
        : this(factory.CreateClient(Names.Identity), factory.CreateClient(Names.Api), factory.CreateClient(Names.Files))
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="PingenHttpClients"/> with http clients based on configuration.
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static PingenHttpClients Create(IPingenConfiguration configuration)
    {
        var identityClient = new HttpClient
        {
            BaseAddress = new(configuration.IdentityUri)
        };
        identityClient.DefaultRequestHeaders.Accept.Clear();
        identityClient.DefaultRequestHeaders.Accept.Add(new("application/x-www-form-urlencoded"));

        var apiClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
        {
            BaseAddress = new(configuration.BaseUri)
        };

        var externalClient = new HttpClient();

        return new(identityClient, apiClient, externalClient);
    }
}
