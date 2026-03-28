using System.Security.Cryptography;
using System.Text;
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

        result.ShouldBeTrue();
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

        result.ShouldBeFalse();
    }

    /// <summary>
    /// Verifies that ValidateWebhookAndGetData throws when signature is invalid
    /// </summary>
    [Test]
    public async Task ValidateWebhookAndGetData_InvalidSignature_Throws()
    {
        const string signingKey = "test-signing-key";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SamplePayload));

        await Should.ThrowAsync<Exception>(async () =>
            await PingenWebhookHelper.ValidateWebhookAndGetData(signingKey, "invalid-sig", stream));
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

        webhookEventData.ShouldSatisfyAllConditions(
            () => webhookEventData.ShouldNotBeNull(),
            () => organisationData.ShouldNotBeNull(),
            () => letterData.ShouldNotBeNull(),
            () => letterEventData.ShouldNotBeNull()
        );
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

        result.ShouldBeFalse();
    }

    private static string ComputeHmacSha256(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
