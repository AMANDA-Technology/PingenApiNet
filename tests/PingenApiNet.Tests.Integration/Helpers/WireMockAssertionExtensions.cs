using WireMock.Server;

namespace PingenApiNet.Tests.Integration.Helpers;

/// <summary>
///     Assertion extension methods on <see cref="WireMockServer" /> for verifying request activity
///     inside integration-test fixtures.
/// </summary>
internal static class WireMockAssertionExtensions
{
    /// <summary>
    ///     Assert that a specific endpoint was called exactly <paramref name="times" /> times.
    /// </summary>
    /// <param name="server">The WireMock server.</param>
    /// <param name="path">URL path to check.</param>
    /// <param name="method">HTTP method to check; defaults to <c>"GET"</c>.</param>
    /// <param name="times">Expected call count; defaults to 1.</param>
    internal static void VerifyCalled(this WireMockServer server, string path, string method = "GET", int times = 1)
    {
        int count = server.LogEntries
            .Count(e => e.RequestMessage?.Path == path &&
                        string.Equals(e.RequestMessage?.Method, method, StringComparison.OrdinalIgnoreCase));

        count.ShouldBe(times,
            $"Expected '{method} {path}' to be called {times} time(s) but was called {count} time(s).");
    }

    /// <summary>
    ///     Assert that a specific endpoint was called at least once.
    /// </summary>
    /// <param name="server">The WireMock server.</param>
    /// <param name="path">URL path to check.</param>
    /// <param name="method">HTTP method to check; defaults to <c>"GET"</c>.</param>
    internal static void VerifyCalledAtLeastOnce(this WireMockServer server, string path, string method = "GET")
    {
        int count = server.LogEntries
            .Count(e => e.RequestMessage?.Path == path &&
                        string.Equals(e.RequestMessage?.Method, method, StringComparison.OrdinalIgnoreCase));

        count.ShouldBeGreaterThanOrEqualTo(1,
            $"Expected '{method} {path}' to be called at least once but was never called.");
    }

    /// <summary>
    ///     Assert that a specific endpoint was never called.
    /// </summary>
    /// <param name="server">The WireMock server.</param>
    /// <param name="path">URL path to check.</param>
    /// <param name="method">HTTP method to check; defaults to <c>"GET"</c>.</param>
    internal static void VerifyNotCalled(this WireMockServer server, string path, string method = "GET")
    {
        int count = server.LogEntries
            .Count(e => e.RequestMessage?.Path == path &&
                        string.Equals(e.RequestMessage?.Method, method, StringComparison.OrdinalIgnoreCase));

        count.ShouldBe(0, $"Expected '{method} {path}' to never be called but was called {count} time(s).");
    }
}
