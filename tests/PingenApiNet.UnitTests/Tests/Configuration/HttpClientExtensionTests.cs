using System.Net.Http.Headers;
using PingenApiNet.Configuration;

namespace PingenApiNet.UnitTests.Tests.Configuration;

/// <summary>
/// Unit tests for <see cref="HttpClientExtension.ApplyDefaultRequestHeaders"/>
/// </summary>
public class HttpClientExtensionTests
{
    private const string HeaderName = "X-Amanda-Client";

    private const string HeaderValue = "AMANDA.discountfit/2f3a9c1 (Backend; env=int)";

    /// <summary>
    /// Verifies that valid headers are applied to the default request headers
    /// </summary>
    [Test]
    public void ApplyDefaultRequestHeaders_WithValidHeaders_AppliesThem()
    {
        using var client = new HttpClient();

        client.ApplyDefaultRequestHeaders(new Dictionary<string, string>
        {
            [HeaderName] = HeaderValue,
            ["X-Correlation-Source"] = "unit-test"
        });

        client.ShouldSatisfyAllConditions(
            () => GetHeaderValues(client, HeaderName).ShouldBe([HeaderValue]),
            () => GetHeaderValues(client, "X-Correlation-Source").ShouldBe(["unit-test"]));
    }

    /// <summary>
    /// Verifies that the client itself is returned to allow chaining
    /// </summary>
    [Test]
    public void ApplyDefaultRequestHeaders_ReturnsSameClientForChaining()
    {
        using var client = new HttpClient();

        var returned = client.ApplyDefaultRequestHeaders(new Dictionary<string, string> { [HeaderName] = HeaderValue });

        returned.ShouldBeSameAs(client);
    }

    /// <summary>
    /// Verifies that null headers are a no-op
    /// </summary>
    [Test]
    public void ApplyDefaultRequestHeaders_WithNullHeaders_IsNoOp()
    {
        using var client = new HttpClient();

        Should.NotThrow(() => client.ApplyDefaultRequestHeaders(null));

        client.DefaultRequestHeaders.ShouldBeEmpty();
    }

    /// <summary>
    /// Verifies that empty headers are a no-op
    /// </summary>
    [Test]
    public void ApplyDefaultRequestHeaders_WithEmptyHeaders_IsNoOp()
    {
        using var client = new HttpClient();

        Should.NotThrow(() => client.ApplyDefaultRequestHeaders(new Dictionary<string, string>()));

        client.DefaultRequestHeaders.ShouldBeEmpty();
    }

    /// <summary>
    /// Verifies that a blank header name is skipped without throwing
    /// </summary>
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("\t")]
    public void ApplyDefaultRequestHeaders_WithBlankName_SkipsHeader(string name)
    {
        using var client = new HttpClient();

        Should.NotThrow(() => client.ApplyDefaultRequestHeaders(new Dictionary<string, string> { [name] = HeaderValue }));

        client.DefaultRequestHeaders.ShouldBeEmpty();
    }

    /// <summary>
    /// Verifies that a blank header value is skipped without throwing
    /// </summary>
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("\t")]
    public void ApplyDefaultRequestHeaders_WithBlankValue_SkipsHeader(string value)
    {
        using var client = new HttpClient();

        Should.NotThrow(() => client.ApplyDefaultRequestHeaders(new Dictionary<string, string> { [HeaderName] = value }));

        client.DefaultRequestHeaders.ShouldBeEmpty();
    }

    /// <summary>
    /// Verifies that a value containing a control character is skipped. TryAddWithoutValidation would accept it,
    /// and the request would then fail when it is sent.
    /// </summary>
    [TestCase("value\r\nX-Injected: evil")]
    [TestCase("value\rmore")]
    [TestCase("value\nmore")]
    [TestCase("value\u007Fmore")]
    [TestCase("value\u0000more")]
    public void ApplyDefaultRequestHeaders_WithControlCharacterInValue_SkipsHeader(string value)
    {
        using var client = new HttpClient();

        Should.NotThrow(() => client.ApplyDefaultRequestHeaders(new Dictionary<string, string> { [HeaderName] = value }));

        client.DefaultRequestHeaders.ShouldBeEmpty();
    }

    /// <summary>
    /// Verifies that a name containing an invalid header name token character is skipped
    /// </summary>
    [TestCase("X Amanda Client")]
    [TestCase("X-Amanda:Client")]
    [TestCase("X-Amanda\r\nClient")]
    [TestCase("X-Amanda/Client")]
    [TestCase("X-Amändä-Client")]
    public void ApplyDefaultRequestHeaders_WithInvalidNameToken_SkipsHeader(string name)
    {
        using var client = new HttpClient();

        Should.NotThrow(() => client.ApplyDefaultRequestHeaders(new Dictionary<string, string> { [name] = HeaderValue }));

        client.DefaultRequestHeaders.ShouldBeEmpty();
    }

    /// <summary>
    /// Verifies that reserved header names are silently skipped, case insensitive. The header collections append on a
    /// duplicate name instead of throwing, so an unguarded entry would transmit two values.
    /// </summary>
    [TestCase("Authorization")]
    [TestCase("authorization")]
    [TestCase("Accept")]
    [TestCase("ACCEPT")]
    [TestCase("Host")]
    [TestCase("host")]
    [TestCase("Idempotency-Key")]
    [TestCase("idempotency-key")]
    public void ApplyDefaultRequestHeaders_WithReservedName_SkipsHeader(string name)
    {
        using var client = new HttpClient();

        client.ApplyDefaultRequestHeaders(new Dictionary<string, string> { [name] = HeaderValue });

        client.DefaultRequestHeaders.ShouldBeEmpty();
    }

    /// <summary>
    /// Verifies that a reserved header name neither replaces nor duplicates the value this library set itself
    /// </summary>
    [Test]
    public void ApplyDefaultRequestHeaders_WithReservedName_KeepsHeadersOwnedByTheLibrary()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "library-token");
        client.DefaultRequestHeaders.Accept.Add(new("application/x-www-form-urlencoded"));

        client.ApplyDefaultRequestHeaders(new Dictionary<string, string>
        {
            ["Authorization"] = "Bearer caller-token",
            ["Accept"] = "application/json",
            [HeaderName] = HeaderValue
        });

        client.ShouldSatisfyAllConditions(
            () => GetHeaderValues(client, "Authorization").ShouldBe(["Bearer library-token"]),
            () => GetHeaderValues(client, "Accept").ShouldBe(["application/x-www-form-urlencoded"]),
            () => GetHeaderValues(client, HeaderName).ShouldBe([HeaderValue]));
    }

    /// <summary>
    /// Verifies that applying headers twice replaces instead of appends, so the header is present exactly once
    /// </summary>
    [Test]
    public void ApplyDefaultRequestHeaders_AppliedTwice_KeepsHeaderExactlyOnce()
    {
        using var client = new HttpClient();
        var headers = new Dictionary<string, string> { [HeaderName] = HeaderValue };

        client.ApplyDefaultRequestHeaders(headers);
        client.ApplyDefaultRequestHeaders(headers);

        GetHeaderValues(client, HeaderName).ShouldBe([HeaderValue]);
    }

    /// <summary>
    /// Verifies that re-applying with a changed value replaces the previous value
    /// </summary>
    [Test]
    public void ApplyDefaultRequestHeaders_ReApplied_ReplacesPreviousValue()
    {
        using var client = new HttpClient();

        client.ApplyDefaultRequestHeaders(new Dictionary<string, string> { [HeaderName] = "first" });
        client.ApplyDefaultRequestHeaders(new Dictionary<string, string> { [HeaderName] = "second" });

        GetHeaderValues(client, HeaderName).ShouldBe(["second"]);
    }

    /// <summary>
    /// Verifies that an invalid entry does not prevent the remaining valid entries from being applied
    /// </summary>
    [Test]
    public void ApplyDefaultRequestHeaders_WithMixedEntries_AppliesOnlyValidOnes()
    {
        using var client = new HttpClient();

        client.ApplyDefaultRequestHeaders(new Dictionary<string, string>
        {
            ["X Invalid Name"] = "value",
            ["X-Invalid-Value"] = "value\r\ninjected",
            ["Authorization"] = "Bearer caller-token",
            [HeaderName] = HeaderValue
        });

        client.ShouldSatisfyAllConditions(
            () => GetHeaderValues(client, HeaderName).ShouldBe([HeaderValue]),
            () => client.DefaultRequestHeaders.Count().ShouldBe(1));
    }

    /// <summary>
    /// Get all values of the given header, or an empty array if the header is not set.
    /// </summary>
    /// <param name="client">The client to read the header from.</param>
    /// <param name="name">The header name to read.</param>
    /// <returns>All values of the given header.</returns>
    private static string[] GetHeaderValues(HttpClient client, string name)
    {
        return client.DefaultRequestHeaders.TryGetValues(name, out var values) ? values.ToArray() : [];
    }
}
