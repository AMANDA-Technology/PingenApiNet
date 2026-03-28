using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Models.Letters;

namespace PingenApiNet.UnitTests.Tests.Helpers;

/// <summary>
/// Unit tests for <see cref="PingenAttributesPropertyHelper{T}"/>
/// </summary>
public class PingenAttributesPropertyHelperTests
{
    /// <summary>
    /// Verifies that GetJsonPropertyName returns the correct JSON property name for a Letter property
    /// </summary>
    [Test]
    public void GetJsonPropertyName_LetterStatus_ReturnsCorrectName()
    {
        var result = PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(l => l.Status);

        result.ShouldBe("status");
    }

    /// <summary>
    /// Verifies that GetJsonPropertyName returns the correct name for file_original_name
    /// </summary>
    [Test]
    public void GetJsonPropertyName_LetterFileOriginalName_ReturnsSnakeCase()
    {
        var result = PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(l => l.FileOriginalName);

        result.ShouldBe("file_original_name");
    }

    /// <summary>
    /// Verifies that GetJsonPropertyName returns the correct name for country
    /// </summary>
    [Test]
    public void GetJsonPropertyName_LetterCountry_ReturnsCorrectName()
    {
        var result = PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(l => l.Country);

        result.ShouldBe("country");
    }

    /// <summary>
    /// Verifies that GetJsonPropertyName throws for a non-member expression
    /// </summary>
    [Test]
    public void GetJsonPropertyName_NonMemberExpression_ThrowsInvalidOperationException()
    {
        Should.Throw<InvalidOperationException>(() =>
            PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(l => "constant"));
    }
}
