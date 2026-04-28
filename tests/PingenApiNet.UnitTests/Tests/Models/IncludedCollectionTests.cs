/*
MIT License

Copyright (c) 2022 Philip Näf <philip.naef@amanda-technology.ch>
Copyright (c) 2022 Manuel Gysin <manuel.gysin@amanda-technology.ch>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Text.Json;
using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Base;
using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Abstractions.Models.Organisations;

namespace PingenApiNet.UnitTests.Tests.Models;

/// <summary>
///     Unit tests for <see cref="IncludedCollection" />
/// </summary>
public class IncludedCollectionTests
{
    /// <summary>
    ///     Verifies that IncludedCollection.Empty has zero items
    /// </summary>
    [Test]
    public void Empty_HasZeroCount()
    {
        IncludedCollection empty = IncludedCollection.Empty;

        empty.Count.ShouldBe(0);
        empty.RawItems.ShouldBeEmpty();
    }

    /// <summary>
    ///     Verifies that IncludedCollection.Empty is a singleton
    /// </summary>
    [Test]
    public void Empty_ReturnsSameInstance()
    {
        IncludedCollection first = IncludedCollection.Empty;
        IncludedCollection second = IncludedCollection.Empty;

        ReferenceEquals(first, second).ShouldBeTrue();
    }

    /// <summary>
    ///     Verifies that deserialization of an empty included array produces a collection with count 0
    /// </summary>
    [Test]
    public void Deserialize_EmptyIncludedArray_ProducesEmptyCollection()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": []
                      }
                      """;

        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        result.Included.ShouldNotBeNull();
        result.Included!.Count.ShouldBe(0);
    }

    /// <summary>
    ///     Verifies that deserialization with no included key produces null
    /// </summary>
    [Test]
    public void Deserialize_MissingIncludedKey_ProducesNull()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } }
                      }
                      """;

        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        result.Included.ShouldBeNull();
    }

    /// <summary>
    ///     Verifies that deserialization with null included value produces null
    /// </summary>
    [Test]
    public void Deserialize_NullIncludedValue_ProducesNull()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": null
                      }
                      """;

        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        result.Included.ShouldBeNull();
    }

    /// <summary>
    ///     Verifies that OfType returns matching items deserialized to the correct type
    /// </summary>
    [Test]
    public void OfType_MatchingType_ReturnsDeserializedItems()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": [
                              { "id": "org-1", "type": "organisations", "attributes": { "name": "Test Org" } },
                              { "id": "letter-2", "type": "letters", "attributes": { "status": "sent" } }
                          ]
                      }
                      """;
        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        var orgs = result.Included!.OfType<Organisation>().ToList();

        orgs.Count.ShouldBe(1);
        orgs[0].Id.ShouldBe("org-1");
        orgs[0].Attributes.Name.ShouldBe("Test Org");
    }

    /// <summary>
    ///     Verifies that OfType returns empty enumerable when no items match
    /// </summary>
    [Test]
    public void OfType_NoMatchingType_ReturnsEmpty()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": [
                              { "id": "letter-2", "type": "letters", "attributes": { "status": "sent" } }
                          ]
                      }
                      """;
        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        var orgs = result.Included!.OfType<Organisation>().ToList();

        orgs.ShouldBeEmpty();
    }

    /// <summary>
    ///     Verifies that OfType skips items with unknown type discriminators
    /// </summary>
    [Test]
    public void OfType_UnknownTypeDiscriminator_SkipsElement()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": [
                              { "id": "unknown-1", "type": "unknown_type_xyz", "attributes": {} },
                              { "id": "org-1", "type": "organisations", "attributes": { "name": "Test Org" } }
                          ]
                      }
                      """;
        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        var orgs = result.Included!.OfType<Organisation>().ToList();

        orgs.Count.ShouldBe(1);
        orgs[0].Id.ShouldBe("org-1");
    }

    /// <summary>
    ///     Verifies that OfType returns multiple items of the same type
    /// </summary>
    [Test]
    public void OfType_MultipleMatchingItems_ReturnsAll()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": [
                              { "id": "letter-2", "type": "letters", "attributes": { "status": "sent" } },
                              { "id": "org-1", "type": "organisations", "attributes": { "name": "Test Org" } },
                              { "id": "letter-3", "type": "letters", "attributes": { "status": "draft" } }
                          ]
                      }
                      """;
        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        var letters = result.Included!.OfType<Letter>().ToList();

        letters.Count.ShouldBe(2);
        letters[0].Id.ShouldBe("letter-2");
        letters[1].Id.ShouldBe("letter-3");
    }

    /// <summary>
    ///     Verifies that FindById returns the matching item with the correct id and type
    /// </summary>
    [Test]
    public void FindById_MatchingIdAndType_ReturnsItem()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": [
                              { "id": "org-1", "type": "organisations", "attributes": { "name": "Org One" } },
                              { "id": "org-2", "type": "organisations", "attributes": { "name": "Org Two" } }
                          ]
                      }
                      """;
        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        Data<Organisation>? found = result.Included!.FindById<Organisation>("org-2");

        found.ShouldNotBeNull();
        found!.Id.ShouldBe("org-2");
        found.Attributes.Name.ShouldBe("Org Two");
    }

    /// <summary>
    ///     Verifies that FindById returns null when no item matches the requested id
    /// </summary>
    [Test]
    public void FindById_NoMatchingId_ReturnsNull()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": [
                              { "id": "org-1", "type": "organisations", "attributes": { "name": "Test Org" } }
                          ]
                      }
                      """;
        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        Data<Organisation>? found = result.Included!.FindById<Organisation>("org-999");

        found.ShouldBeNull();
    }

    /// <summary>
    ///     Verifies that FindById returns null when id matches but type does not
    /// </summary>
    [Test]
    public void FindById_MatchingIdWrongType_ReturnsNull()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": [
                              { "id": "org-1", "type": "organisations", "attributes": { "name": "Test Org" } }
                          ]
                      }
                      """;
        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        Data<Letter>? found = result.Included!.FindById<Letter>("org-1");

        found.ShouldBeNull();
    }

    /// <summary>
    ///     Verifies that Count reflects the number of raw JSON elements
    /// </summary>
    [Test]
    public void Count_ReflectsRawItemCount()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": [
                              { "id": "org-1", "type": "organisations", "attributes": { "name": "Org One" } },
                              { "id": "org-2", "type": "organisations", "attributes": { "name": "Org Two" } },
                              { "id": "letter-2", "type": "letters", "attributes": { "status": "sent" } }
                          ]
                      }
                      """;
        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        result.Included!.Count.ShouldBe(3);
    }

    /// <summary>
    ///     Verifies that RawItems is read-only
    /// </summary>
    [Test]
    public void RawItems_IsReadOnly()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": [
                              { "id": "org-1", "type": "organisations", "attributes": { "name": "Test Org" } }
                          ]
                      }
                      """;
        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        result.Included!.RawItems.ShouldBeAssignableTo<IReadOnlyList<JsonElement>>();
    }

    /// <summary>
    ///     Verifies that IncludedCollection works with CollectionResult as well
    /// </summary>
    [Test]
    public void CollectionResult_IncludedCollection_WorksCorrectly()
    {
        string json = """
                      {
                          "data": [
                              { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } }
                          ],
                          "links": { "self": "https://example.com", "first": "https://example.com", "last": "https://example.com" },
                          "meta": { "current_page": 1, "last_page": 1, "per_page": 100, "from": 1, "to": 1, "total": 1 },
                          "included": [
                              { "id": "org-1", "type": "organisations", "attributes": { "name": "Test Org" } }
                          ]
                      }
                      """;
        CollectionResult<Data<Letter>>? result =
            PingenSerialisationHelper.Deserialize<CollectionResult<Data<Letter>>>(json)!;

        result.Included.ShouldNotBeNull();
        var orgs = result.Included!.OfType<Organisation>().ToList();
        orgs.Count.ShouldBe(1);
        orgs[0].Attributes.Name.ShouldBe("Test Org");
    }

    /// <summary>
    ///     Verifies that FindById returns the first match when the included array contains duplicate ids
    /// </summary>
    [Test]
    public void FindById_DuplicateEntries_ReturnsFirstMatch()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": [
                              { "id": "org-1", "type": "organisations", "attributes": { "name": "First" } },
                              { "id": "org-1", "type": "organisations", "attributes": { "name": "Second" } }
                          ]
                      }
                      """;
        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        Data<Organisation>? found = result.Included!.FindById<Organisation>("org-1");

        found.ShouldNotBeNull();
        found!.Attributes.Name.ShouldBe("First");
    }

    /// <summary>
    ///     Verifies that OfType returns all duplicate entries in their original insertion order
    /// </summary>
    [Test]
    public void OfType_DuplicateEntries_ReturnsAllInsertionOrder()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": [
                              { "id": "org-1", "type": "organisations", "attributes": { "name": "First" } },
                              { "id": "org-1", "type": "organisations", "attributes": { "name": "Second" } }
                          ]
                      }
                      """;
        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        var orgs = result.Included!.OfType<Organisation>().ToList();

        orgs.ShouldSatisfyAllConditions(
            () => orgs.Count.ShouldBe(2),
            () => orgs[0].Attributes.Name.ShouldBe("First"),
            () => orgs[1].Attributes.Name.ShouldBe("Second")
        );
    }

    /// <summary>
    ///     Verifies that the IncludedCollection constructor accepts a list of raw JsonElement items and stores them
    /// </summary>
    [Test]
    public void Constructor_AcceptsRawItemsList_StoresThem()
    {
        var emptyCollection = new IncludedCollection(new List<JsonElement>());

        JsonElement element = JsonDocument.Parse("{\"id\":\"x\",\"type\":\"letters\"}").RootElement;
        var populatedCollection = new IncludedCollection([element]);

        emptyCollection.Count.ShouldBe(0);
        populatedCollection.ShouldSatisfyAllConditions(
            () => populatedCollection.Count.ShouldBe(1),
            () => populatedCollection.RawItems[0].GetProperty("id").GetString().ShouldBe("x")
        );
    }

    /// <summary>
    ///     Verifies that OfType skips items that lack a type property
    /// </summary>
    [Test]
    public void OfType_ItemMissingTypeProperty_SkipsElement()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": [
                              { "id": "x", "attributes": {} },
                              { "id": "org-1", "type": "organisations", "attributes": { "name": "Test" } }
                          ]
                      }
                      """;
        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        var orgs = result.Included!.OfType<Organisation>().ToList();

        orgs.Count.ShouldBe(1);
    }

    /// <summary>
    ///     Verifies that OfType skips items whose type property is JSON null
    /// </summary>
    [Test]
    public void OfType_ItemWithNullTypeProperty_SkipsElement()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": [
                              { "id": "x", "type": null, "attributes": {} },
                              { "id": "org-1", "type": "organisations", "attributes": { "name": "Test" } }
                          ]
                      }
                      """;
        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        var orgs = result.Included!.OfType<Organisation>().ToList();

        orgs.Count.ShouldBe(1);
    }

    /// <summary>
    ///     Verifies that FindById skips elements that lack an id property and continues searching
    /// </summary>
    [Test]
    public void FindById_ItemMissingIdProperty_SkipsElement()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": [
                              { "type": "organisations", "attributes": { "name": "X" } },
                              { "id": "org-2", "type": "organisations", "attributes": { "name": "Org Two" } }
                          ]
                      }
                      """;
        SingleResult<Data<Letter>>? result = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;

        Data<Organisation>? found = result.Included!.FindById<Organisation>("org-2");

        found.ShouldNotBeNull();
        found!.Id.ShouldBe("org-2");
    }

    /// <summary>
    ///     Verifies that the IncludedCollection JSON converter round-trip preserves the included array contents
    /// </summary>
    [Test]
    public void JsonConverter_RoundTrip_PreservesIncludedArray()
    {
        string json = """
                      {
                          "data": { "id": "letter-1", "type": "letters", "attributes": { "status": "valid" } },
                          "included": [
                              { "id": "org-1", "type": "organisations", "attributes": { "name": "Org One" } },
                              { "id": "org-2", "type": "organisations", "attributes": { "name": "Org Two" } }
                          ]
                      }
                      """;

        SingleResult<Data<Letter>>? first = PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(json)!;
        string serialized = PingenSerialisationHelper.Serialize(first);
        SingleResult<Data<Letter>>? second =
            PingenSerialisationHelper.Deserialize<SingleResult<Data<Letter>>>(serialized)!;

        var orgs = second.Included!.OfType<Organisation>().ToList();
        second.ShouldSatisfyAllConditions(
            () => second.Included!.Count.ShouldBe(2),
            () => orgs.Count.ShouldBe(2),
            () => orgs[0].Id.ShouldBe("org-1"),
            () => orgs[1].Id.ShouldBe("org-2")
        );
    }
}
