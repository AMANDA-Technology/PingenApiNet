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

        Assert.Multiple(() =>
        {
            normalized.BaseUri.ShouldBe("https://api.example.com/");
            normalized.IdentityUri.ShouldBe("https://identity.example.com/");
        });
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

        Assert.Multiple(() =>
        {
            normalized.BaseUri.ShouldBe("https://api.example.com/");
            normalized.IdentityUri.ShouldBe("https://identity.example.com/");
        });
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

        Assert.Multiple(() =>
        {
            config.BaseUri.ShouldBe("https://api.example.com/");
            config.IdentityUri.ShouldBe("https://identity.example.com/");
            config.ClientId.ShouldBe("my-client-id");
            config.ClientSecret.ShouldBe("my-client-secret");
            config.DefaultOrganisationId.ShouldBe("my-org-id");
            config.WebhookSigningKeys.Count.ShouldBe(1);
        });
    }
}
