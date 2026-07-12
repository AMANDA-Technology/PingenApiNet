using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Models.Base;
using PingenApiNet.Abstractions.Models.LetterEvents;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Organisations;
using PingenApiNet.Abstractions.Models.Webhooks.WebhookEvents;

namespace PingenApiNet.UnitTests.Tests.Helpers;

/// <summary>
///     Unit tests for <see cref="PingenWebhookHelper" />
/// </summary>
public class PingenWebhookHelperTests
{
    private static readonly string SamplePayload =
        File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "webhook_sample.json"));

    /// <summary>
    ///     Verifies that ValidateWebhook returns true for a valid signature
    /// </summary>
    [Test]
    public async Task ValidateWebhook_ValidSignature_ReturnsTrue()
    {
        const string signingKey = "test-signing-key";
        string signature = ComputeHmacSha256(signingKey, SamplePayload);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SamplePayload));

        bool result = await PingenWebhookHelper.ValidateWebhook(signingKey, signature, stream);

        result.ShouldBeTrue();
    }

    /// <summary>
    ///     Verifies that ValidateWebhook returns false for an invalid signature
    /// </summary>
    [Test]
    public async Task ValidateWebhook_InvalidSignature_ReturnsFalse()
    {
        const string signingKey = "test-signing-key";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SamplePayload));

        bool result = await PingenWebhookHelper.ValidateWebhook(signingKey, "invalid-signature", stream);

        result.ShouldBeFalse();
    }

    /// <summary>
    ///     Verifies that ValidateWebhookAndGetData throws when signature is invalid
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
    ///     Verifies that ValidateWebhookAndGetData returns data when signature is valid
    /// </summary>
    [Test]
    public async Task ValidateWebhookAndGetData_ValidSignature_ReturnsData()
    {
        const string signingKey = "test-signing-key";
        string signature = ComputeHmacSha256(signingKey, SamplePayload);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SamplePayload));

        (WebhookEventData? webhookEventData, Data<Organisation>? organisationData, Data<Letter>? letterData,
                Data<LetterEvent>? letterEventData) =
            await PingenWebhookHelper.ValidateWebhookAndGetData(signingKey, signature, stream);

        webhookEventData.ShouldSatisfyAllConditions(
            () => webhookEventData.ShouldNotBeNull(),
            () => organisationData.ShouldNotBeNull(),
            () => letterData.ShouldNotBeNull(),
            () => letterEventData.ShouldNotBeNull()
        );
    }

    /// <summary>
    ///     Verifies that ValidateWebhook returns false for mismatched signing key
    /// </summary>
    [Test]
    public async Task ValidateWebhook_DifferentKey_ReturnsFalse()
    {
        const string signingKey = "correct-key";
        const string wrongKey = "wrong-key";
        string signature = ComputeHmacSha256(signingKey, SamplePayload);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SamplePayload));

        bool result = await PingenWebhookHelper.ValidateWebhook(wrongKey, signature, stream);

        result.ShouldBeFalse();
    }

    /// <summary>
    ///     Verifies that an empty signing key still produces a valid HMAC and validates correctly
    /// </summary>
    [Test]
    public async Task ValidateWebhook_EmptySigningKey_StillComputesHash()
    {
        const string signingKey = "";
        string signature = ComputeHmacSha256(signingKey, SamplePayload);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SamplePayload));

        bool result = await PingenWebhookHelper.ValidateWebhook(signingKey, signature, stream);

        result.ShouldBeTrue();
    }

    /// <summary>
    ///     Verifies that a non-hex signature returns false via the FormatException catch path
    /// </summary>
    [Test]
    public async Task ValidateWebhook_NonHexSignature_ReturnsFalse()
    {
        const string signingKey = "test-signing-key";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SamplePayload));

        bool result = await PingenWebhookHelper.ValidateWebhook(signingKey, "not-hex-zzz", stream);

        result.ShouldBeFalse();
    }

    /// <summary>
    ///     Verifies that an odd-length hex signature returns false via the FormatException catch path
    /// </summary>
    [Test]
    public async Task ValidateWebhook_OddLengthHexSignature_ReturnsFalse()
    {
        const string signingKey = "test-signing-key";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SamplePayload));

        bool result = await PingenWebhookHelper.ValidateWebhook(signingKey, "abc", stream);

        result.ShouldBeFalse();
    }

    /// <summary>
    ///     Verifies that an uppercase hex signature is accepted because Convert.FromHexString is case-insensitive
    /// </summary>
    [Test]
    public async Task ValidateWebhook_UppercaseHexSignature_ReturnsTrue()
    {
        const string signingKey = "test-signing-key";
        string signature = ComputeHmacSha256(signingKey, SamplePayload).ToUpperInvariant();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SamplePayload));

        bool result = await PingenWebhookHelper.ValidateWebhook(signingKey, signature, stream);

        result.ShouldBeTrue();
    }

    /// <summary>
    ///     Verifies that an empty payload validates correctly when the signature matches the empty-stream HMAC
    /// </summary>
    [Test]
    public async Task ValidateWebhook_EmptyPayload_ComputesHashOverEmptyStream()
    {
        const string signingKey = "test-signing-key";
        string signature = ComputeHmacSha256(signingKey, string.Empty);
        using var stream = new MemoryStream();

        bool result = await PingenWebhookHelper.ValidateWebhook(signingKey, signature, stream);

        result.ShouldBeTrue();
    }

    /// <summary>
    ///     Verifies that ValidateWebhookAndGetData throws JsonException when the payload is malformed JSON
    /// </summary>
    [Test]
    public async Task ValidateWebhookAndGetData_MalformedJson_ThrowsJsonException()
    {
        const string signingKey = "test-signing-key";
        const string malformed = "{not-valid-json";
        string signature = ComputeHmacSha256(signingKey, malformed);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(malformed));

        await Should.ThrowAsync<JsonException>(async () =>
            await PingenWebhookHelper.ValidateWebhookAndGetData(signingKey, signature, stream));
    }

    /// <summary>
    ///     Verifies that all included relationships resolve for each supported webhook event type
    /// </summary>
    [TestCase("webhook_issues")]
    [TestCase("webhook_sent")]
    [TestCase("webhook_undeliverable")]
    [TestCase("webhook_delivered")]
    public async Task ValidateWebhookAndGetData_AllIncludedRelationshipsResolved_ForAllEventTypes(string apiDataType)
    {
        const string signingKey = "test-signing-key";
        string payload = BuildPayloadForType(apiDataType);
        string signature = ComputeHmacSha256(signingKey, payload);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));

        (WebhookEventData? webhookEventData, Data<Organisation>? organisationData, Data<Letter>? letterData,
                Data<LetterEvent>? letterEventData) =
            await PingenWebhookHelper.ValidateWebhookAndGetData(signingKey, signature, stream);

        webhookEventData.ShouldSatisfyAllConditions(
            () => webhookEventData.ShouldNotBeNull(),
            () => organisationData.ShouldNotBeNull(),
            () => letterData.ShouldNotBeNull(),
            () => letterEventData.ShouldNotBeNull()
        );
    }

    /// <summary>
    ///     Verifies that a webhook_sent payload deserializes with the correct type discriminator
    /// </summary>
    [Test]
    public async Task ValidateWebhookAndGetData_WebhookSentType_DeserializesCorrectly()
    {
        const string signingKey = "test-signing-key";
        string payload = BuildPayloadForType("webhook_sent");
        string signature = ComputeHmacSha256(signingKey, payload);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));

        (WebhookEventData? webhookEventData, _, _, _) =
            await PingenWebhookHelper.ValidateWebhookAndGetData(signingKey, signature, stream);

        webhookEventData.ShouldNotBeNull();
        webhookEventData!.Type.ShouldBe(PingenApiDataType.webhook_sent);
    }

    /// <summary>
    ///     Verifies that a webhook_undeliverable payload deserializes with the correct type discriminator
    /// </summary>
    [Test]
    public async Task ValidateWebhookAndGetData_WebhookUndeliverableType_DeserializesCorrectly()
    {
        const string signingKey = "test-signing-key";
        string payload = BuildPayloadForType("webhook_undeliverable");
        string signature = ComputeHmacSha256(signingKey, payload);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));

        (WebhookEventData? webhookEventData, _, _, _) =
            await PingenWebhookHelper.ValidateWebhookAndGetData(signingKey, signature, stream);

        webhookEventData.ShouldNotBeNull();
        webhookEventData!.Type.ShouldBe(PingenApiDataType.webhook_undeliverable);
    }

    /// <summary>
    ///     Verifies that a webhook_delivered payload deserializes with the correct type discriminator.
    ///     Regression test: before <c>webhook_delivered</c> was added to <see cref="PingenApiDataType" />,
    ///     the unknown discriminator made <c>JsonStringEnumConverter</c> throw a
    ///     <see cref="JsonException" /> inside ValidateWebhookAndGetData — after signature validation had
    ///     already passed — so every "delivered" webhook failed and Pingen eventually dead-lettered it.
    /// </summary>
    [Test]
    public async Task ValidateWebhookAndGetData_WebhookDeliveredType_DeserializesCorrectly()
    {
        const string signingKey = "test-signing-key";
        string payload = BuildPayloadForType("webhook_delivered");
        string signature = ComputeHmacSha256(signingKey, payload);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));

        (WebhookEventData? webhookEventData, _, _, _) =
            await PingenWebhookHelper.ValidateWebhookAndGetData(signingKey, signature, stream);

        webhookEventData.ShouldNotBeNull();
        webhookEventData!.Type.ShouldBe(PingenApiDataType.webhook_delivered);
    }

    /// <summary>
    ///     Verifies that a webhook_delivered payload carrying no <c>reason</c> attribute deserializes with a
    ///     null <see cref="WebhookEvent.Reason" />. Per the Pingen OpenAPI spec, WebhookDeliveredAttributes is
    ///     <c>url</c> + <c>created_at</c> only (identical to WebhookSentAttributes); only the <c>issues</c> and
    ///     <c>undeliverable</c> categories carry <c>reason</c>. This pins that the shared
    ///     <see cref="WebhookEvent" /> model binds a delivered payload without loss.
    /// </summary>
    [Test]
    public async Task ValidateWebhookAndGetData_WebhookDelivered_HasNoReason()
    {
        const string signingKey = "test-signing-key";
        string payload = BuildPayloadForType("webhook_delivered")
            .Replace("\"reason\": \"Printing failed\",", string.Empty);
        string signature = ComputeHmacSha256(signingKey, payload);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));

        (WebhookEventData? webhookEventData, Data<Organisation>? organisationData, Data<Letter>? letterData,
                Data<LetterEvent>? letterEventData) =
            await PingenWebhookHelper.ValidateWebhookAndGetData(signingKey, signature, stream);

        webhookEventData.ShouldSatisfyAllConditions(
            () => webhookEventData.ShouldNotBeNull(),
            () => webhookEventData!.Type.ShouldBe(PingenApiDataType.webhook_delivered),
            () => webhookEventData!.Attributes.Reason.ShouldBeNull(),
            () => webhookEventData!.Attributes.CreatedAt.ShouldNotBeNull(),
            () => organisationData.ShouldNotBeNull(),
            () => letterData.ShouldNotBeNull(),
            () => letterEventData.ShouldNotBeNull()
        );
    }

    /// <summary>
    ///     Verifies that a webhook_channel_subscriptions payload no longer throws on the type discriminator.
    ///     The category has a different attribute surface and therefore no dedicated attributes model yet
    ///     (it is allow-listed in <c>PingenApiDataTypeMappingTests.KnownUnmappedDataTypes</c>), but the enum
    ///     value must exist so the JSON:API <c>type</c> deserializes rather than throwing.
    /// </summary>
    [Test]
    public async Task ValidateWebhookAndGetData_WebhookChannelSubscriptionsType_DoesNotThrowOnDiscriminator()
    {
        const string signingKey = "test-signing-key";
        string payload = BuildPayloadForType("webhook_channel_subscriptions");
        string signature = ComputeHmacSha256(signingKey, payload);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));

        (WebhookEventData? webhookEventData, _, _, _) =
            await PingenWebhookHelper.ValidateWebhookAndGetData(signingKey, signature, stream);

        webhookEventData.ShouldNotBeNull();
        webhookEventData!.Type.ShouldBe(PingenApiDataType.webhook_channel_subscriptions);
    }

    private static string ComputeHmacSha256(string key, string data)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        using var hmac = new HMACSHA256(keyBytes);
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string BuildPayloadForType(string apiDataType)
        => SamplePayload.Replace("\"type\": \"webhook_issues\"", $"\"type\": \"{apiDataType}\"");
}
