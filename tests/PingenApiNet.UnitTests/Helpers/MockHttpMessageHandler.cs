using System.Net;

namespace PingenApiNet.UnitTests.Helpers;

/// <summary>
/// Mock HTTP message handler for unit testing HTTP client behavior
/// </summary>
public sealed class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsyncFunc;

    /// <summary>
    /// Initializes a new instance with a custom handler function
    /// </summary>
    /// <param name="sendAsyncFunc"></param>
    public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsyncFunc)
    {
        _sendAsyncFunc = sendAsyncFunc;
    }

    /// <summary>
    /// Initializes a new instance that returns a fixed response
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="content"></param>
    public MockHttpMessageHandler(HttpStatusCode statusCode, string content = "")
    {
        _sendAsyncFunc = (_, _) => Task.FromResult(new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content)
        });
    }

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return _sendAsyncFunc(request, cancellationToken);
    }
}
