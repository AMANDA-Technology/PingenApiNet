using System.Text.Json;

namespace PingenApiNet.Tests.Integration.Helpers;

/// <summary>
/// Helper methods for building JSON:API response strings used in WireMock stubs.
/// </summary>
internal static class JsonApiStubHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    /// <summary>
    /// Build an OAuth 2.0 access token response body.
    /// </summary>
    /// <param name="token">Bearer token value.</param>
    /// <param name="expiresIn">Token lifetime in seconds.</param>
    /// <returns>JSON string.</returns>
    internal static string TokenResponse(string token = "test-access-token", long expiresIn = 3600)
    {
        return JsonSerializer.Serialize(new
        {
            access_token = token,
            token_type = "Bearer",
            expires_in = expiresIn
        }, JsonOptions);
    }

    /// <summary>
    /// Build a JSON:API single-resource response.
    /// </summary>
    /// <param name="id">Resource ID.</param>
    /// <param name="type">Resource type string.</param>
    /// <param name="attributes">Attributes object (will be serialized).</param>
    /// <param name="relationships">Optional relationships object.</param>
    /// <param name="meta">Optional meta object.</param>
    /// <returns>JSON string.</returns>
    internal static string SingleResponse(string id, string type, object attributes, object? relationships = null, object? meta = null)
    {
        var data = BuildDataObject(id, type, attributes, relationships, meta);
        return JsonSerializer.Serialize(new { data }, JsonOptions);
    }

    /// <summary>
    /// Build a JSON:API collection response with pagination meta.
    /// </summary>
    /// <param name="items">List of (id, attributes, relationships, meta) tuples.</param>
    /// <param name="type">Resource type string.</param>
    /// <param name="currentPage">Current page number.</param>
    /// <param name="lastPage">Last page number.</param>
    /// <param name="perPage">Items per page.</param>
    /// <param name="total">Total items count.</param>
    /// <returns>JSON string.</returns>
    internal static string CollectionResponse(
        IEnumerable<(string Id, object Attributes, object? Relationships, object? Meta)> items,
        string type,
        int currentPage = 1,
        int lastPage = 1,
        int perPage = 20,
        int? total = null)
    {
        var itemList = items.ToList();
        var actualTotal = total ?? itemList.Count;

        var data = itemList.Select(item => BuildDataObject(item.Id, type, item.Attributes, item.Relationships, item.Meta)).ToList();

        return JsonSerializer.Serialize(new
        {
            data,
            links = new
            {
                first = "https://api.test.pingen.com/?page[number]=1",
                last = $"https://api.test.pingen.com/?page[number]={lastPage}",
                prev = (string?)null,
                next = currentPage < lastPage ? $"https://api.test.pingen.com/?page[number]={currentPage + 1}" : null,
                self = $"https://api.test.pingen.com/?page[number]={currentPage}"
            },
            meta = new
            {
                current_page = currentPage,
                last_page = lastPage,
                per_page = perPage,
                from = itemList.Count > 0 ? (currentPage - 1) * perPage + 1 : 0,
                to = itemList.Count > 0 ? (currentPage - 1) * perPage + itemList.Count : 0,
                total = actualTotal
            }
        }, JsonOptions);
    }

    /// <summary>
    /// Build a single data object with optional relationships and meta.
    /// </summary>
    private static Dictionary<string, object?> BuildDataObject(string id, string type, object attributes, object? relationships, object? meta)
    {
        var obj = new Dictionary<string, object?>
        {
            ["id"] = id,
            ["type"] = type,
            ["attributes"] = attributes
        };

        if (relationships is not null)
            obj["relationships"] = relationships;

        if (meta is not null)
            obj["meta"] = meta;

        return obj;
    }

    /// <summary>
    /// Build a related-single-output relationship object with links.
    /// </summary>
    /// <param name="relatedId">Related resource ID.</param>
    /// <param name="relatedType">Related resource type.</param>
    /// <param name="relatedUrl">Related link URL.</param>
    /// <returns>Anonymous object suitable for serialization.</returns>
    internal static object RelatedSingle(string relatedId, string relatedType, string relatedUrl = "https://api.test.pingen.com/related")
    {
        return new
        {
            links = new { related = relatedUrl },
            data = new { id = relatedId, type = relatedType }
        };
    }

    /// <summary>
    /// Build a related-many-output relationship object with links.
    /// </summary>
    /// <param name="relatedUrl">Related link URL.</param>
    /// <param name="href">Link href.</param>
    /// <param name="metaCount">Count metadata.</param>
    /// <returns>Anonymous object suitable for serialization.</returns>
    internal static object RelatedMany(string relatedUrl = "https://api.test.pingen.com/related", string href = "https://api.test.pingen.com/related", int metaCount = 0)
    {
        return new
        {
            links = new
            {
                related = new
                {
                    href,
                    meta = new { count = metaCount }
                }
            }
        };
    }

    /// <summary>
    /// Build a meta object with self abilities.
    /// </summary>
    /// <param name="abilities">Abilities object.</param>
    /// <returns>Anonymous object.</returns>
    internal static object MetaWithAbilities(object abilities)
    {
        return new
        {
            abilities = new { self = abilities }
        };
    }

    /// <summary>
    /// Build a meta object with self abilities and organisation abilities.
    /// </summary>
    /// <param name="selfAbilities">Self abilities object.</param>
    /// <param name="organisationAbilities">Organisation abilities object.</param>
    /// <returns>Anonymous object.</returns>
    internal static object MetaWithOrganisationAbilities(object selfAbilities, object organisationAbilities)
    {
        return new
        {
            abilities = new
            {
                self = selfAbilities,
                organisation = organisationAbilities
            }
        };
    }
}
