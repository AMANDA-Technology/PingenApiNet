namespace PingenApiNet.Tests.Tests.Unit.Models;

/// <summary>
/// Unit tests for <see cref="PingenConfiguration"/> and <see cref="PingenConfigurationExtension"/>
/// </summary>
public class PingenConfigurationTests
{
    /// <summary>
    /// Verifies Normalize appends trailing slash to BaseUri
    /// </summary>
    [Test]
    public void Normalize_BaseUriWithoutTrailingSlash_AppendsSlash()
    {
        var config = new PingenConfiguration
        {
            BaseUri = "https://api.example.com",
            IdentityUri = "https://identity.example.com",
            ClientId = "test",
            ClientSecret = "test",
            DefaultOrganisationId = "org"
        };

        var normalized = config.Normalize();

        normalized.ShouldSatisfyAllConditions(
            () => normalized.BaseUri.ShouldBe("https://api.example.com/"),
            () => normalized.IdentityUri.ShouldBe("https://identity.example.com/")
        );
    }

    /// <summary>
    /// Verifies Normalize does not double-append trailing slash
    /// </summary>
    [Test]
    public void Normalize_BaseUriWithTrailingSlash_NoChange()
    {
        var config = new PingenConfiguration
        {
            BaseUri = "https://api.example.com/",
            IdentityUri = "https://identity.example.com/",
            ClientId = "test",
            ClientSecret = "test",
            DefaultOrganisationId = "org"
        };

        var normalized = config.Normalize();

        normalized.ShouldSatisfyAllConditions(
            () => normalized.BaseUri.ShouldBe("https://api.example.com/"),
            () => normalized.IdentityUri.ShouldBe("https://identity.example.com/")
        );
    }

    /// <summary>
    /// Verifies Normalize rejects BaseUri without HTTPS scheme
    /// </summary>
    [Test]
    public void Normalize_HttpBaseUri_ThrowsArgumentException()
    {
        var config = new PingenConfiguration
        {
            BaseUri = "http://api.example.com",
            IdentityUri = "https://identity.example.com",
            ClientId = "test",
            ClientSecret = "test",
            DefaultOrganisationId = "org"
        };

        Should.Throw<ArgumentException>(() => config.Normalize())
            .Message.ShouldContain("BaseUri");
    }

    /// <summary>
    /// Verifies Normalize rejects IdentityUri without HTTPS scheme
    /// </summary>
    [Test]
    public void Normalize_HttpIdentityUri_ThrowsArgumentException()
    {
        var config = new PingenConfiguration
        {
            BaseUri = "https://api.example.com",
            IdentityUri = "http://identity.example.com",
            ClientId = "test",
            ClientSecret = "test",
            DefaultOrganisationId = "org"
        };

        Should.Throw<ArgumentException>(() => config.Normalize())
            .Message.ShouldContain("IdentityUri");
    }

    /// <summary>
    /// Verifies Normalize accepts valid HTTPS URIs
    /// </summary>
    [Test]
    public void Normalize_HttpsUris_DoesNotThrow()
    {
        var config = new PingenConfiguration
        {
            BaseUri = "https://api.example.com",
            IdentityUri = "https://identity.example.com",
            ClientId = "test",
            ClientSecret = "test",
            DefaultOrganisationId = "org"
        };

        Should.NotThrow(() => config.Normalize());
    }

    /// <summary>
    /// Verifies PingenConfiguration stores all properties
    /// </summary>
    [Test]
    public void PingenConfiguration_StoresAllProperties()
    {
        var config = new PingenConfiguration
        {
            BaseUri = "https://api.example.com/",
            IdentityUri = "https://identity.example.com/",
            ClientId = "my-client-id",
            ClientSecret = "my-client-secret",
            DefaultOrganisationId = "my-org-id",
            WebhookSigningKeys = new Dictionary<string, string> { ["key1"] = "secret1" }
        };

        config.ShouldSatisfyAllConditions(
            () => config.BaseUri.ShouldBe("https://api.example.com/"),
            () => config.IdentityUri.ShouldBe("https://identity.example.com/"),
            () => config.ClientId.ShouldBe("my-client-id"),
            () => config.ClientSecret.ShouldBe("my-client-secret"),
            () => config.DefaultOrganisationId.ShouldBe("my-org-id"),
            () => config.WebhookSigningKeys.Count.ShouldBe(1)
        );
    }
}
