using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Serialization;
using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Interfaces.Data;
using PingenApiNet.Abstractions.Models.Letters;

namespace PingenApiNet.UnitTests.Tests.Helpers;

/// <summary>
///     Unit tests for <see cref="PingenAttributesPropertyHelper{T}" />
/// </summary>
public class PingenAttributesPropertyHelperTests
{
    /// <summary>
    ///     Verifies that GetJsonPropertyName returns the correct JSON property name for a Letter property
    /// </summary>
    [Test]
    public void GetJsonPropertyName_LetterStatus_ReturnsCorrectName()
    {
        string result = PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(l => l.Status);

        result.ShouldBe("status");
    }

    /// <summary>
    ///     Verifies that GetJsonPropertyName returns the correct name for file_original_name
    /// </summary>
    [Test]
    public void GetJsonPropertyName_LetterFileOriginalName_ReturnsSnakeCase()
    {
        string result = PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(l => l.FileOriginalName);

        result.ShouldBe("file_original_name");
    }

    /// <summary>
    ///     Verifies that GetJsonPropertyName returns the correct name for country
    /// </summary>
    [Test]
    public void GetJsonPropertyName_LetterCountry_ReturnsCorrectName()
    {
        string result = PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(l => l.Country);

        result.ShouldBe("country");
    }

    /// <summary>
    ///     Verifies that GetJsonPropertyName throws for a non-member expression
    /// </summary>
    [Test]
    public void GetJsonPropertyName_NonMemberExpression_ThrowsInvalidOperationException()
    {
        Should.Throw<InvalidOperationException>(() =>
            PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(l => "constant"));
    }

    /// <summary>
    ///     Verifies that GetJsonPropertyName throws when the property has no JsonPropertyName attribute
    /// </summary>
    [Test]
    public void GetJsonPropertyName_PropertyMissingJsonPropertyName_ThrowsInvalidOperationException()
    {
        Should.Throw<InvalidOperationException>(() =>
            PingenAttributesPropertyHelper<AttrFixtureMissingJsonName>.GetJsonPropertyName(x => x.PlainProperty));
    }

    /// <summary>
    ///     Verifies that GetJsonPropertyName resolves correctly when the property carries multiple unrelated attributes
    /// </summary>
    [Test]
    public void GetJsonPropertyName_PropertyWithMultipleUnrelatedAttributes_ReturnsJsonPropertyName()
    {
        string result = PingenAttributesPropertyHelper<AttrFixtureWithMixedAttributes>
            .GetJsonPropertyName(x => x.Name);

        result.ShouldBe("real_name");
    }

    /// <summary>
    ///     Verifies that GetJsonPropertyName throws for a method-call expression body
    /// </summary>
    [Test]
    public void GetJsonPropertyName_MethodCallExpression_ThrowsInvalidOperationException()
    {
        Should.Throw<InvalidOperationException>(() =>
            PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(l => l.Status!.ToUpper()));
    }

    /// <summary>
    ///     Verifies that GetJsonPropertyName throws for a binary expression body
    /// </summary>
    [Test]
    public void GetJsonPropertyName_BinaryExpression_ThrowsInvalidOperationException()
    {
        Should.Throw<InvalidOperationException>(() =>
            PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(l => l.Status + "suffix"));
    }

    /// <summary>
    ///     Verifies the compile-time guarantee that JsonPropertyNameAttribute disallows multiple instances on a single
    ///     property
    /// </summary>
    [Test]
    public void JsonPropertyNameAttribute_DoesNotAllowMultipleOnSingleProperty()
    {
        AttributeUsageAttribute?
            usage = typeof(JsonPropertyNameAttribute).GetCustomAttribute<AttributeUsageAttribute>();

        usage.ShouldNotBeNull();
        usage!.AllowMultiple.ShouldBeFalse();
    }

    /// <summary>
    ///     Verifies that GetJsonPropertyName returns the snake_case name for Letter.CreatedAt
    /// </summary>
    [Test]
    public void GetJsonPropertyName_LetterCreatedAt_ReturnsSnakeCase()
    {
        string result = PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(l => l.CreatedAt);

        result.ShouldBe("created_at");
    }

    /// <summary>
    ///     Verifies that GetJsonPropertyName returns the snake_case name for Letter.PriceCurrency
    /// </summary>
    [Test]
    public void GetJsonPropertyName_LetterPriceCurrency_ReturnsSnakeCase()
    {
        string result = PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(l => l.PriceCurrency);

        result.ShouldBe("price_currency");
    }

    private sealed record AttrFixtureMissingJsonName(string PlainProperty) : IAttributes;

    private sealed record AttrFixtureWithMixedAttributes(
        [property: JsonPropertyName("real_name")]
        [property: Required]
        [property: System.ComponentModel.Description("a property carrying multiple unrelated attributes")]
        string Name) : IAttributes;
}
