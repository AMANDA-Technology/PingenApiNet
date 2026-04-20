using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace PingenApiNet.Tests.Integration.Helpers;

/// <summary>
///     Fluent extension methods on <see cref="WireMockServer" /> for concise stub setup.
/// </summary>
internal static class WireMockExtensions
{
    /// <summary>
    ///     Stub a GET endpoint to return a JSON body.
    /// </summary>
    /// <param name="server">The WireMock server.</param>
    /// <param name="path">URL path to match.</param>
    /// <param name="body">JSON response body.</param>
    /// <param name="statusCode">HTTP status code; defaults to 200.</param>
    internal static void StubJsonGet(this WireMockServer server, string path, string body, int statusCode = 200)
    {
        server
            .Given(Request.Create()
                .WithPath(path)
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(statusCode)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(body));
    }

    /// <summary>
    ///     Stub a POST endpoint to return a JSON body.
    /// </summary>
    /// <param name="server">The WireMock server.</param>
    /// <param name="path">URL path to match.</param>
    /// <param name="body">JSON response body.</param>
    /// <param name="statusCode">HTTP status code; defaults to 201.</param>
    internal static void StubJsonPost(this WireMockServer server, string path, string body, int statusCode = 201)
    {
        server
            .Given(Request.Create()
                .WithPath(path)
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(statusCode)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(body));
    }

    /// <summary>
    ///     Stub a PATCH endpoint to return a JSON body.
    /// </summary>
    /// <param name="server">The WireMock server.</param>
    /// <param name="path">URL path to match.</param>
    /// <param name="body">JSON response body.</param>
    /// <param name="statusCode">HTTP status code; defaults to 200.</param>
    internal static void StubJsonPatch(this WireMockServer server, string path, string body, int statusCode = 200)
    {
        server
            .Given(Request.Create()
                .WithPath(path)
                .UsingPatch())
            .RespondWith(Response.Create()
                .WithStatusCode(statusCode)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString())
                .WithBody(body));
    }

    /// <summary>
    ///     Stub a DELETE endpoint.
    /// </summary>
    /// <param name="server">The WireMock server.</param>
    /// <param name="path">URL path to match.</param>
    /// <param name="statusCode">HTTP status code; defaults to 204.</param>
    internal static void StubDelete(this WireMockServer server, string path, int statusCode = 204)
    {
        server
            .Given(Request.Create()
                .WithPath(path)
                .UsingDelete())
            .RespondWith(Response.Create()
                .WithStatusCode(statusCode)
                .WithHeader("X-Request-ID", Guid.NewGuid().ToString()));
    }

    /// <summary>
    ///     Stub an endpoint to return a JSON error response.
    /// </summary>
    /// <param name="server">The WireMock server.</param>
    /// <param name="path">URL path to match.</param>
    /// <param name="method">HTTP method to match (e.g. "GET", "POST").</param>
    /// <param name="body">JSON error response body.</param>
    /// <param name="statusCode">HTTP status code; defaults to 422.</param>
    internal static void StubError(this WireMockServer server, string path, string method, string body,
        int statusCode = 422)
    {
        server
            .Given(Request.Create()
                .WithPath(path)
                .UsingMethod(method))
            .RespondWith(Response.Create()
                .WithStatusCode(statusCode)
                .WithHeader("Content-Type", "application/json")
                .WithBody(body));
    }
}
