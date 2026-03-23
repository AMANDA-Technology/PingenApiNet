using System.Security.Cryptography;
using System.Text;
using PingenApiNet.Abstractions.Exceptions;
using PingenApiNet.Abstractions.Helpers;

namespace PingenApiNet.Tests.Tests.Unit.Helpers;

/// <summary>
/// Unit tests for <see cref="PingenWebhookHelper"/>
/// </summary>
public class PingenWebhookHelperTests
{
    private static readonly string SamplePayload = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "webhook_sample.json"));

    /// <summary>
    /// Verifies that ValidateWebhook returns true for a valid signature
    /// </summary>
    [Test]
    public async Task ValidateWebhook_ValidSignature_ReturnsTrue()
    {
        const string signingKey = "test-signing-key";
        var signature = ComputeHmacSha256(signingKey, SamplePayload);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SamplePayload));

        var result = await PingenWebhookHelper.ValidateWebhook(signingKey, signature, stream);

        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Verifies that ValidateWebhook returns false for an invalid signature
    /// </summary>
    [Test]
    public async Task ValidateWebhook_InvalidSignature_ReturnsFalse()
    {
        const string signingKey = "test-signing-key";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SamplePayload));

        var result = await PingenWebhookHelper.ValidateWebhook(signingKey, "invalid-signature", stream);

        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Verifies that ValidateWebhookAndGetData throws when signature is invalid
    /// </summary>
    [Test]
    public void ValidateWebhookAndGetData_InvalidSignature_Throws()
    {
        const string signingKey = "test-signing-key";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SamplePayload));

        Assert.That(async () =>
            await PingenWebhookHelper.ValidateWebhookAndGetData(signingKey, "invalid-sig", stream),
            Throws.Exception);
    }

    /// <summary>
    /// Verifies that ValidateWebhookAndGetData returns data when signature is valid
    /// </summary>
    [Test]
    public async Task ValidateWebhookAndGetData_ValidSignature_ReturnsData()
    {
        const string signingKey = "test-signing-key";
        var signature = ComputeHmacSha256(signingKey, SamplePayload);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SamplePayload));

        var (webhookEventData, organisationData, letterData, letterEventData) =
            await PingenWebhookHelper.ValidateWebhookAndGetData(signingKey, signature, stream);

        Assert.Multiple(() =>
        {
            Assert.That(webhookEventData, Is.Not.Null);
            Assert.That(organisationData, Is.Not.Null);
            Assert.That(letterData, Is.Not.Null);
            Assert.That(letterEventData, Is.Not.Null);
        });
    }

    /// <summary>
    /// Verifies that ValidateWebhook returns false for mismatched signing key
    /// </summary>
    [Test]
    public async Task ValidateWebhook_DifferentKey_ReturnsFalse()
    {
        const string signingKey = "correct-key";
        const string wrongKey = "wrong-key";
        var signature = ComputeHmacSha256(signingKey, SamplePayload);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SamplePayload));

        var result = await PingenWebhookHelper.ValidateWebhook(wrongKey, signature, stream);

        Assert.That(result, Is.False);
    }

    private static string ComputeHmacSha256(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return hash.Aggregate("", (current, t) => current + t.ToString("x2"));
    }
}
