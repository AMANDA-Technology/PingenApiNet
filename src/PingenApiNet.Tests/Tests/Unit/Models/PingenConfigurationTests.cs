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
            Assert.That(normalized.BaseUri, Is.EqualTo("https://api.example.com/"));
            Assert.That(normalized.IdentityUri, Is.EqualTo("https://identity.example.com/"));
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
            Assert.That(normalized.BaseUri, Is.EqualTo("https://api.example.com/"));
            Assert.That(normalized.IdentityUri, Is.EqualTo("https://identity.example.com/"));
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
            Assert.That(config.BaseUri, Is.EqualTo("https://api.example.com/"));
            Assert.That(config.IdentityUri, Is.EqualTo("https://identity.example.com/"));
            Assert.That(config.ClientId, Is.EqualTo("my-client-id"));
            Assert.That(config.ClientSecret, Is.EqualTo("my-client-secret"));
            Assert.That(config.DefaultOrganisationId, Is.EqualTo("my-org-id"));
            Assert.That(config.WebhookSigningKeys, Has.Count.EqualTo(1));
        });
    }
}
