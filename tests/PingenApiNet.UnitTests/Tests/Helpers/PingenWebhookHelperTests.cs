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
    ///     A real <c>webhook_delivered</c> body as Pingen sends it: attributes are <c>url</c> + <c>created_at</c>
    ///     only (no <c>reason</c>), relationships are organisation + letter + event. Kept as its own asset rather
    ///     than derived from <see cref="SamplePayload" /> by swapping the type string, so the test exercises the
    ///     payload shape the API actually produces.
    /// </summary>
    private static readonly string DeliveredPayload =
        File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "webhook_delivered_sample.json"));

    /// <summary>
    ///     A real webhook body in the shape Pingen has sent since <b>2026-07-27</b>, when it generalised
    ///     "letter" to "deliverable" (letter | email | ebill). Captured verbatim from
    ///     <c>GET /organisations/{org}/webhooks/{id}/requests</c> and only anonymised. Three differences
    ///     against <see cref="SamplePayload" />, all present at once:
    ///     <list type="number">
    ///         <item><c>data.relationships.event.data.type</c> is <c>deliverables_events</c>, not <c>letters_events</c>;</item>
    ///         <item>a new <c>deliverable</c> relationship sits alongside the retained <c>letter</c> one;</item>
    ///         <item><c>included</c> carries the same event <b>twice</b>, once under each type, sharing one id.</item>
    ///     </list>
    /// </summary>
    private static readonly string DeliverablesEventPayload =
        File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "webhook_sent_deliverables_sample.json"));

    /// <summary>
    ///     The end state of the 2026-07-27 rollout, <b>constructed from the spec rather than captured</b>:
    ///     <c>WebhookDeliverableUndeliverableGET</c> declares <c>organisation</c> + <c>deliverable</c> +
    ///     <c>event</c> as its required relationships and no longer declares <c>letter</c> at all, so this body
    ///     drops the legacy relationship and the duplicated <c>letters_events</c> include with it. Pingen has
    ///     not sent this shape yet — it is what the published contract already describes, and what the library
    ///     must survive being switched to without notice. Also carries <c>corrected_address</c>, spec-required
    ///     on this category and previously dropped on the floor.
    /// </summary>
    private static readonly string DeliverableOnlyPayload =
        File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "webhook_undeliverable_deliverable_only_sample.json"));

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
    ///     Verifies that a real, spec-shaped webhook_delivered payload deserializes end to end.
    ///     Regression test for the outage this fixes: before <c>webhook_delivered</c> was added to
    ///     <see cref="PingenApiDataType" />, the unknown discriminator made <c>JsonStringEnumConverter</c> throw a
    ///     <see cref="JsonException" /> inside ValidateWebhookAndGetData — <em>after</em> signature validation had
    ///     already passed — so every "delivered" webhook failed and Pingen eventually dead-lettered it.
    ///     This uses <c>Assets/webhook_delivered_sample.json</c> (the real body shape) rather than a
    ///     type-swapped webhook_issues body, so it also pins the attribute and relationship surface:
    ///     per the Pingen OpenAPI spec, WebhookDeliveredAttributes is <c>url</c> + <c>created_at</c> only
    ///     (identical to WebhookSentAttributes — only <c>issues</c> and <c>undeliverable</c> carry
    ///     <c>reason</c>), and the relationships are organisation + letter + event. The shared
    ///     <see cref="WebhookEvent" /> model therefore binds it without loss, with a null <c>Reason</c>.
    /// </summary>
    [Test]
    public async Task ValidateWebhookAndGetData_WebhookDeliveredPayload_DeserializesWithoutLoss()
    {
        const string signingKey = "test-signing-key";
        string signature = ComputeHmacSha256(signingKey, DeliveredPayload);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(DeliveredPayload));

        (WebhookEventData? webhookEventData, Data<Organisation>? organisationData, Data<Letter>? letterData,
                Data<LetterEvent>? letterEventData) =
            await PingenWebhookHelper.ValidateWebhookAndGetData(signingKey, signature, stream);

        webhookEventData.ShouldSatisfyAllConditions(
            () => webhookEventData.ShouldNotBeNull(),
            () => webhookEventData!.Type.ShouldBe(PingenApiDataType.webhook_delivered),
            () => webhookEventData!.Attributes.Reason.ShouldBeNull(),
            () => webhookEventData!.Attributes.Url.ShouldNotBeNull(),
            () => webhookEventData!.Attributes.CreatedAt.ShouldNotBeNull(),
            () => webhookEventData!.Relationships.Organisation.Data.Type.ShouldBe(PingenApiDataType.organisations),
            () => webhookEventData!.Relationships.Letter.ShouldNotBeNull(),
            () => webhookEventData!.Relationships.Letter!.Data.Type.ShouldBe(PingenApiDataType.letters),
            () => webhookEventData!.Relationships.Deliverable.ShouldBeNull(),
            () => webhookEventData!.Relationships.Event.Data.Type.ShouldBe(PingenApiDataType.letters_events),
            () => organisationData.ShouldNotBeNull(),
            () => letterData.ShouldNotBeNull(),
            () => letterEventData.ShouldNotBeNull()
        );
    }

    /// <summary>
    ///     Regression test for the 2026-07-27 outage: Pingen switched the webhook body's
    ///     <c>data.relationships.event.data.type</c> from <c>letters_events</c> to <c>deliverables_events</c>
    ///     for every event category, without notice. That relationship binds to a non-nullable
    ///     <see cref="PingenApiDataType" />, so the unknown discriminator threw
    ///     <see cref="JsonException" /> out of ValidateWebhookAndGetData — <em>after</em> the HMAC signature
    ///     had already validated — and every consumer answered 4xx/5xx until Pingen dead-lettered the event.
    ///     Same failure class as the earlier <c>webhook_delivered</c> outage, one level deeper in the body.
    ///     <para>
    ///     The assertions on the resolved included resources are the load-bearing half. Pingen emits the
    ///     <b>same</b> event in <c>included</c> twice (typed <c>letters_events</c> and
    ///     <c>deliverables_events</c>, sharing an id) and both discriminators map to
    ///     <see cref="LetterEvent" />, so this test fails if <c>IncludedCollection.OfType&lt;T&gt;()</c> ever
    ///     stops collapsing matches by resource id — two matches make
    ///     <c>TryGetIncludedData</c>'s <c>SingleOrDefault()</c> throw, re-breaking every webhook.
    ///     </para>
    /// </summary>
    [Test]
    public async Task ValidateWebhookAndGetData_DeliverablesEventRelationship_DeserializesAndResolvesSingleEvent()
    {
        const string signingKey = "test-signing-key";
        string signature = ComputeHmacSha256(signingKey, DeliverablesEventPayload);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(DeliverablesEventPayload));

        (WebhookEventData? webhookEventData, Data<Organisation>? organisationData, Data<Letter>? letterData,
                Data<LetterEvent>? letterEventData) =
            await PingenWebhookHelper.ValidateWebhookAndGetData(signingKey, signature, stream);

        webhookEventData.ShouldSatisfyAllConditions(
            () => webhookEventData.ShouldNotBeNull(),
            () => webhookEventData!.Type.ShouldBe(PingenApiDataType.webhook_sent),
            () => webhookEventData!.Relationships.Organisation.Data.Type.ShouldBe(PingenApiDataType.organisations),
            () => webhookEventData!.Relationships.Letter.ShouldNotBeNull(),
            () => webhookEventData!.Relationships.Letter!.Data.Type.ShouldBe(PingenApiDataType.letters),
            () => webhookEventData!.Relationships.Deliverable.ShouldNotBeNull(),
            () => webhookEventData!.Relationships.Deliverable!.Data.Type.ShouldBe(PingenApiDataType.letters),
            () => webhookEventData!.Relationships.Deliverable!.Data.Id.ShouldBe(webhookEventData!.Relationships.Letter!.Data.Id),
            () => webhookEventData!.Relationships.Event.Data.Type.ShouldBe(PingenApiDataType.deliverables_events),
            () => organisationData.ShouldNotBeNull(),
            () => letterData.ShouldNotBeNull(),
            () => letterEventData.ShouldNotBeNull(),
            () => letterEventData!.Attributes.Code.ShouldBe("transferred_to_distributor")
        );
    }

    /// <summary>
    ///     Forward-compatibility pin for the end state of the 2026-07-27 rollout: the body Pingen's own spec
    ///     describes today, where the legacy shape is gone. <c>WebhookDeliverable*GET</c> declares
    ///     <c>organisation</c> + <c>deliverable</c> + <c>event</c> as the required relationships and no longer
    ///     lists <c>letter</c> at all, so the wire is expected to drop it — and with it the duplicated
    ///     <c>letters_events</c> copy in <c>included</c>.
    ///     <para>
    ///     Everything the library exposes must still resolve from that body alone: the event include is found
    ///     via <c>deliverables_events</c> (not the legacy type), and the letter include via the
    ///     <c>deliverable</c> relationship's <c>letters</c> resource. If this test regresses, the library will
    ///     start returning a silently null letter event the day Pingen finishes the migration — a quieter and
    ///     worse failure than the <see cref="JsonException" /> that started the outage, because it surfaces as
    ///     a <see cref="NullReferenceException" /> in consumer code rather than at the parse boundary.
    ///     </para>
    /// </summary>
    [Test]
    public async Task ValidateWebhookAndGetData_LegacyLetterShapeDropped_StillResolvesEverything()
    {
        const string signingKey = "test-signing-key";
        string signature = ComputeHmacSha256(signingKey, DeliverableOnlyPayload);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(DeliverableOnlyPayload));

        (WebhookEventData? webhookEventData, Data<Organisation>? organisationData, Data<Letter>? letterData,
                Data<LetterEvent>? letterEventData) =
            await PingenWebhookHelper.ValidateWebhookAndGetData(signingKey, signature, stream);

        webhookEventData.ShouldSatisfyAllConditions(
            () => webhookEventData.ShouldNotBeNull(),
            () => webhookEventData!.Type.ShouldBe(PingenApiDataType.webhook_undeliverable),
            () => webhookEventData!.Relationships.Letter.ShouldBeNull(),
            () => webhookEventData!.Relationships.Deliverable.ShouldNotBeNull(),
            () => webhookEventData!.Relationships.Deliverable!.Data.Type.ShouldBe(PingenApiDataType.letters),
            () => webhookEventData!.Relationships.Event.Data.Type.ShouldBe(PingenApiDataType.deliverables_events),
            () => organisationData.ShouldNotBeNull(),
            () => letterData.ShouldNotBeNull(),
            () => letterEventData.ShouldNotBeNull(),
            () => letterEventData!.Attributes.Code.ShouldBe("undeliverable"),
            () => webhookEventData!.Attributes.CorrectedAddress.ShouldNotBeNull(),
            () => webhookEventData!.Attributes.CorrectedAddress!.Zip.ShouldBe("8051"),
            () => webhookEventData!.Attributes.CorrectedAddress!.Number.ShouldBe("50A")
        );
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
