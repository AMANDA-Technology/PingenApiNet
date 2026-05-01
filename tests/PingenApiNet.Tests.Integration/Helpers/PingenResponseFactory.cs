using System.Text.Json;
using Bogus;

namespace PingenApiNet.Tests.Integration.Helpers;

/// <summary>
///     Centralised factory for building JSON:API response strings used in WireMock stubs.
///     Uses Bogus for realistic test-data generation and delegates to <see cref="JsonApiStubHelper" />
///     for envelope construction.
/// </summary>
internal static class PingenResponseFactory
{
    private const string DefaultOrganisationId = "test-org-id-001";
    private static readonly Faker _faker = new();

    // ── Letters ─────────────────────────────────────────────────────────────

    /// <summary>
    ///     Build a JSON:API single-letter response.
    /// </summary>
    /// <param name="id">Letter ID; auto-generated when <c>null</c>.</param>
    /// <param name="organisationId">Organisation ID; defaults to <c>test-org-id-001</c>.</param>
    /// <returns>JSON string.</returns>
    internal static string SingleLetter(string? id = null, string? organisationId = null)
    {
        id ??= Guid.NewGuid().ToString();
        organisationId ??= DefaultOrganisationId;

        return JsonApiStubHelper.SingleResponse(
            id,
            "letters",
            LetterAttributes(),
            LetterRelationships(organisationId),
            JsonApiStubHelper.MetaWithAbilities(LetterAbilitiesObject()));
    }

    /// <summary>
    ///     Build a JSON:API letter collection response.
    /// </summary>
    /// <param name="count">Number of items to generate.</param>
    /// <param name="organisationId">Organisation ID; defaults to <c>test-org-id-001</c>.</param>
    /// <param name="currentPage">Current page number.</param>
    /// <param name="lastPage">Last page number.</param>
    /// <returns>JSON string.</returns>
    internal static string LetterCollection(int count = 3, string? organisationId = null, int currentPage = 1,
        int lastPage = 1)
    {
        organisationId ??= DefaultOrganisationId;

        IEnumerable<(string, object, object?, object?)> items = Enumerable.Range(0, count).Select(_ =>
            (Guid.NewGuid().ToString(),
                LetterAttributes(),
                (object?)LetterRelationships(organisationId),
                (object?)null));

        return JsonApiStubHelper.CollectionResponse(items, "letters", currentPage, lastPage, total: count);
    }

    /// <summary>
    ///     Build a JSON:API letter event collection response.
    /// </summary>
    /// <param name="count">Number of items to generate.</param>
    /// <param name="letterId">Parent letter ID; auto-generated when <c>null</c>.</param>
    /// <param name="currentPage">Current page number.</param>
    /// <param name="lastPage">Last page number.</param>
    /// <returns>JSON string.</returns>
    internal static string LetterEventCollection(int count = 3, string? letterId = null, int currentPage = 1,
        int lastPage = 1)
    {
        string parentLetterId = letterId ?? Guid.NewGuid().ToString();

        IEnumerable<(string, object, object?, object?)> items = Enumerable.Range(0, count).Select(_ =>
            (Guid.NewGuid().ToString(),
                LetterEventAttributes(),
                (object?)new { letter = JsonApiStubHelper.RelatedSingle(parentLetterId, "letters") },
                (object?)null));

        return JsonApiStubHelper.CollectionResponse(items, "letters_events", currentPage, lastPage, total: count);
    }

    /// <summary>
    ///     Build a JSON:API single letter-price-calculator response.
    /// </summary>
    /// <param name="id">Resource ID; auto-generated when <c>null</c>.</param>
    /// <param name="price">Calculated price; defaults to <c>1.50</c>.</param>
    /// <returns>JSON string.</returns>
    internal static string SingleLetterPriceCalculator(string? id = null, decimal price = 1.50m)
    {
        id ??= Guid.NewGuid().ToString();

        return JsonApiStubHelper.SingleResponse(
            id,
            "letter_price_calculator",
            new { currency = "CHF", price });
    }

    // ── Batches ──────────────────────────────────────────────────────────────

    /// <summary>
    ///     Build a JSON:API single-batch response.
    /// </summary>
    /// <param name="id">Batch ID; auto-generated when <c>null</c>.</param>
    /// <param name="organisationId">Organisation ID; defaults to <c>test-org-id-001</c>.</param>
    /// <returns>JSON string.</returns>
    internal static string SingleBatch(string? id = null, string? organisationId = null)
    {
        id ??= Guid.NewGuid().ToString();
        organisationId ??= DefaultOrganisationId;

        return JsonApiStubHelper.SingleResponse(
            id,
            "batches",
            BatchAttributes(),
            new
            {
                organisation = JsonApiStubHelper.RelatedSingle(organisationId, "organisations"),
                events = JsonApiStubHelper.RelatedMany()
            },
            JsonApiStubHelper.MetaWithAbilities(new { cancel = "ok", delete = "ok", submit = "ok", edit = "ok" }));
    }

    /// <summary>
    ///     Build a JSON:API batch collection response.
    /// </summary>
    /// <param name="count">Number of items to generate.</param>
    /// <param name="organisationId">Organisation ID; defaults to <c>test-org-id-001</c>.</param>
    /// <param name="currentPage">Current page number.</param>
    /// <param name="lastPage">Last page number.</param>
    /// <returns>JSON string.</returns>
    internal static string BatchCollection(int count = 3, string? organisationId = null, int currentPage = 1,
        int lastPage = 1)
    {
        organisationId ??= DefaultOrganisationId;

        IEnumerable<(string, object, object?, object?)> items = Enumerable.Range(0, count).Select(_ =>
            (Guid.NewGuid().ToString(),
                BatchAttributes(),
                (object?)new
                {
                    organisation = JsonApiStubHelper.RelatedSingle(organisationId, "organisations"),
                    events = JsonApiStubHelper.RelatedMany()
                },
                (object?)null));

        return JsonApiStubHelper.CollectionResponse(items, "batches", currentPage, lastPage, total: count);
    }

    // ── Files ────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Build a JSON:API file-upload-path response.
    /// </summary>
    /// <param name="id">File ID; auto-generated when <c>null</c>.</param>
    /// <returns>JSON string.</returns>
    internal static string FileUploadPath(string? id = null)
    {
        id ??= Guid.NewGuid().ToString();
        string url = $"https://s3.example.com/upload/{id}";

        return JsonApiStubHelper.SingleResponse(
            id,
            "file_uploads",
            new
            {
                url,
                url_signature = _faker.Random.AlphaNumeric(64),
                expires_at = DateTimeOffset.UtcNow.AddMinutes(30).ToString("o")
            });
    }

    // ── Delivery Products ────────────────────────────────────────────────────

    /// <summary>
    ///     Build a JSON:API delivery-product collection response.
    /// </summary>
    /// <param name="count">Number of items to generate.</param>
    /// <param name="currentPage">Current page number.</param>
    /// <param name="lastPage">Last page number.</param>
    /// <returns>JSON string.</returns>
    internal static string DeliveryProductCollection(int count = 3, int currentPage = 1, int lastPage = 1)
    {
        IEnumerable<(string, object, object?, object?)> items = Enumerable.Range(0, count).Select(_ =>
            (Guid.NewGuid().ToString(),
                DeliveryProductAttributes(),
                (object?)null,
                (object?)null));

        return JsonApiStubHelper.CollectionResponse(items, "delivery_products", currentPage, lastPage, total: count);
    }

    // ── Webhooks ─────────────────────────────────────────────────────────────

    /// <summary>
    ///     Build a JSON:API single-webhook response.
    /// </summary>
    /// <param name="id">Webhook ID; auto-generated when <c>null</c>.</param>
    /// <param name="organisationId">Organisation ID; defaults to <c>test-org-id-001</c>.</param>
    /// <returns>JSON string.</returns>
    internal static string SingleWebhook(string? id = null, string? organisationId = null)
    {
        id ??= Guid.NewGuid().ToString();
        organisationId ??= DefaultOrganisationId;

        return JsonApiStubHelper.SingleResponse(
            id,
            "webhooks",
            WebhookAttributes(),
            new { organisation = JsonApiStubHelper.RelatedSingle(organisationId, "organisations") });
    }

    /// <summary>
    ///     Build a JSON:API webhook collection response.
    /// </summary>
    /// <param name="count">Number of items to generate.</param>
    /// <param name="organisationId">Organisation ID; defaults to <c>test-org-id-001</c>.</param>
    /// <param name="currentPage">Current page number.</param>
    /// <param name="lastPage">Last page number.</param>
    /// <returns>JSON string.</returns>
    internal static string WebhookCollection(int count = 3, string? organisationId = null, int currentPage = 1,
        int lastPage = 1)
    {
        organisationId ??= DefaultOrganisationId;

        IEnumerable<(string, object, object?, object?)> items = Enumerable.Range(0, count).Select(_ =>
            (Guid.NewGuid().ToString(),
                WebhookAttributes(),
                (object?)new { organisation = JsonApiStubHelper.RelatedSingle(organisationId, "organisations") },
                (object?)null));

        return JsonApiStubHelper.CollectionResponse(items, "webhooks", currentPage, lastPage, total: count);
    }

    // ── Organisations ────────────────────────────────────────────────────────

    /// <summary>
    ///     Build a JSON:API single-organisation response.
    /// </summary>
    /// <param name="id">Organisation ID; auto-generated when <c>null</c>.</param>
    /// <returns>JSON string.</returns>
    internal static string SingleOrganisation(string? id = null)
    {
        id ??= Guid.NewGuid().ToString();

        return JsonApiStubHelper.SingleResponse(
            id,
            "organisations",
            OrganisationAttributes(),
            new { associations = JsonApiStubHelper.RelatedMany() },
            JsonApiStubHelper.MetaWithAbilities(new { manage = "ok" }));
    }

    /// <summary>
    ///     Build a JSON:API organisation collection response.
    /// </summary>
    /// <param name="count">Number of items to generate.</param>
    /// <param name="currentPage">Current page number.</param>
    /// <param name="lastPage">Last page number.</param>
    /// <returns>JSON string.</returns>
    internal static string OrganisationCollection(int count = 3, int currentPage = 1, int lastPage = 1)
    {
        IEnumerable<(string, object, object?, object?)> items = Enumerable.Range(0, count).Select(_ =>
            (Guid.NewGuid().ToString(),
                OrganisationAttributes(),
                (object?)new { associations = JsonApiStubHelper.RelatedMany() },
                (object?)null));

        return JsonApiStubHelper.CollectionResponse(items, "organisations", currentPage, lastPage, total: count);
    }

    // ── Users ────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Build a JSON:API single-user response (authenticated user).
    /// </summary>
    /// <param name="id">User ID; auto-generated when <c>null</c>.</param>
    /// <returns>JSON string.</returns>
    internal static string SingleUser(string? id = null)
    {
        id ??= Guid.NewGuid().ToString();

        return JsonApiStubHelper.SingleResponse(
            id,
            "users",
            UserAttributes(),
            new { associations = JsonApiStubHelper.RelatedMany(), notifications = JsonApiStubHelper.RelatedMany() },
            JsonApiStubHelper.MetaWithAbilities(new { reach = "ok", act = "ok" }));
    }

    /// <summary>
    ///     Build a JSON:API user-association collection response.
    /// </summary>
    /// <param name="count">Number of items to generate.</param>
    /// <param name="currentPage">Current page number.</param>
    /// <param name="lastPage">Last page number.</param>
    /// <returns>JSON string.</returns>
    internal static string UserAssociationCollection(int count = 3, int currentPage = 1, int lastPage = 1)
    {
        IEnumerable<(string, object, object?, object?)> items = Enumerable.Range(0, count).Select(i =>
            (Guid.NewGuid().ToString(),
                UserAssociationAttributes(),
                (object?)new { organisation = JsonApiStubHelper.RelatedSingle($"org-for-assoc-{i}", "organisations") },
                (object?)JsonApiStubHelper.MetaWithOrganisationAbilities(
                    new { reach = "ok", act = "ok" },
                    new { manage = "ok" })));

        return JsonApiStubHelper.CollectionResponse(items, "associations", currentPage, lastPage, total: count);
    }

    // ── Error Responses ──────────────────────────────────────────────────────

    /// <summary>
    ///     Build a JSON:API error response.
    /// </summary>
    /// <param name="title">Error title; defaults to a generic message.</param>
    /// <param name="detail">Error detail; defaults to a generic message.</param>
    /// <param name="status">HTTP status code string; defaults to <c>"422"</c>.</param>
    /// <returns>JSON string.</returns>
    internal static string ErrorResponse(string? title = null, string? detail = null, string status = "422")
    {
        return JsonSerializer.Serialize(new
        {
            errors = new[]
            {
                new
                {
                    status,
                    title = title ?? "Unprocessable Entity",
                    detail = detail ?? "The request could not be processed."
                }
            }
        });
    }

    // ── Private Attribute Builders ───────────────────────────────────────────

    private static object LetterAttributes() => new
    {
        status = _faker.PickRandom("valid", "invalid", "pending"),
        file_original_name = _faker.System.FileName("pdf"),
        file_pages = _faker.Random.Int(1, 10),
        address = _faker.Address.StreetAddress(),
        address_position = _faker.PickRandom("left", "right"),
        country = _faker.PickRandom("CH", "DE", "AT"),
        delivery_product = _faker.PickRandom("cheap", "priority"),
        print_mode = _faker.PickRandom("simplex", "duplex"),
        print_spectrum = _faker.PickRandom("grayscale", "color"),
        price_currency = "CHF",
        price_value = Math.Round(_faker.Random.Double(0.5, 5.0), 2),
        created_at = _faker.Date.Past().ToString("o"),
        updated_at = _faker.Date.Recent(30).ToString("o")
    };

    private static object LetterEventAttributes() => new
    {
        code = _faker.PickRandom("submitted", "delivered", "issue_detected", "issue_resolved", "sent"),
        name = _faker.Lorem.Sentence(3),
        producer = _faker.PickRandom("PostAG", "DPAG", "AustrianPost"),
        location = _faker.Address.City(),
        has_image = _faker.Random.Bool(),
        data = Array.Empty<string>(),
        emitted_at = _faker.Date.Recent(7).ToString("o"),
        created_at = _faker.Date.Recent(7).ToString("o"),
        updated_at = _faker.Date.Recent(7).ToString("o")
    };

    private static object BatchAttributes() => new
    {
        name = _faker.Commerce.ProductName(),
        icon = _faker.PickRandom("flat", "priority"),
        status = _faker.PickRandom("valid", "invalid", "pending"),
        file_original_name = _faker.System.FileName("pdf"),
        letter_count = _faker.Random.Int(1, 100),
        address_position = _faker.PickRandom("left", "right"),
        print_mode = _faker.PickRandom("simplex", "duplex"),
        print_spectrum = _faker.PickRandom("grayscale", "color"),
        price_currency = "CHF",
        price_value = Math.Round(_faker.Random.Double(1.0, 50.0), 2),
        created_at = _faker.Date.Past().ToString("o"),
        updated_at = _faker.Date.Recent(30).ToString("o")
    };

    private static object DeliveryProductAttributes() => new
    {
        name = $"PostAg {_faker.Commerce.ProductAdjective()}",
        full_name = $"PostAG {_faker.Commerce.ProductName()}",
        price_currency = "CHF",
        price_starting_from = Math.Round(_faker.Random.Double(0.5, 5.0), 2),
        delivery_time_days = new[] { _faker.Random.Int(1, 3), _faker.Random.Int(3, 7) },
        features = new[] { _faker.PickRandom("color", "grayscale"), _faker.PickRandom("simplex", "duplex") },
        countries = new[] { _faker.PickRandom("CH", "DE", "AT") }
    };

    private static object WebhookAttributes() => new
    {
        event_category = _faker.PickRandom("issues", "undeliverable", "sent"),
        url = _faker.Internet.UrlWithPath(),
        signing_key = _faker.Random.AlphaNumeric(32)
    };

    private static object OrganisationAttributes() => new
    {
        name = _faker.Company.CompanyName(),
        status = _faker.PickRandom("active", "inactive"),
        plan = _faker.PickRandom("free", "professional", "enterprise"),
        billing_mode = _faker.PickRandom("prepaid", "postpaid"),
        billing_currency = "CHF",
        billing_balance = Math.Round(_faker.Random.Double(0, 1000), 2),
        default_country = _faker.PickRandom("CH", "DE", "AT"),
        default_address_position = _faker.PickRandom("left", "right"),
        data_retention_addresses = _faker.Random.Int(30, 365),
        data_retention_pdf = _faker.Random.Int(30, 365),
        color = _faker.Internet.Color(),
        created_at = _faker.Date.Past().ToString("o"),
        updated_at = _faker.Date.Recent(30).ToString("o")
    };

    private static object UserAttributes() => new
    {
        email = _faker.Internet.Email(),
        first_name = _faker.Name.FirstName(),
        last_name = _faker.Name.LastName(),
        status = _faker.PickRandom("active", "pending", "blocked"),
        language = _faker.PickRandom("en", "de", "fr", "it"),
        created_at = _faker.Date.Past().ToString("o"),
        updated_at = _faker.Date.Recent(30).ToString("o")
    };

    private static object UserAssociationAttributes() => new
    {
        role = _faker.PickRandom("owner", "manager"),
        status = _faker.PickRandom("active", "pending", "blocked"),
        created_at = _faker.Date.Past().ToString("o"),
        updated_at = _faker.Date.Recent(30).ToString("o")
    };

    private static object LetterRelationships(string organisationId) => new
    {
        organisation = JsonApiStubHelper.RelatedSingle(organisationId, "organisations"),
        events = JsonApiStubHelper.RelatedMany(),
        batch = JsonApiStubHelper.RelatedSingle(Guid.NewGuid().ToString(), "batches")
    };

    private static object LetterAbilitiesObject() => new
    {
        cancel = "ok",
        delete = "ok",
        submit = "ok",
        send_simplex = "ok",
        edit = "ok",
        get_pdf_raw = "ok",
        get_pdf_validation = "ok",
        change_paper_type = "ok",
        change_window_position = "ok",
        create_coverpage = "ok",
        fix_overwrite_restricted_areas = "ok",
        fix_coverpage = "ok",
        fix_regular_paper = "ok"
    };
}
