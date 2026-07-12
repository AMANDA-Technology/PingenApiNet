/*
MIT License

Copyright (c) 2026 AMANDA Technology

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

using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Interfaces.Data;

namespace PingenApiNet.UnitTests.Tests.Helpers;

/// <summary>
///     Reflection-based regression tests for <see cref="PingenSerialisationHelper.PingenApiDataTypeMapping" />.
///     The mapping resolves the JSON:API <c>type</c> discriminator to a CLR attributes type and is
///     the single point of failure for <c>IncludedCollection.OfType&lt;T&gt;</c>,
///     <c>IncludedCollection.FindById&lt;T&gt;</c>, and <see cref="PingenSerialisationHelper.TryGetIncludedData{T}" />.
///     A missing entry causes included resources of that type to be silently skipped.
/// </summary>
public class PingenApiDataTypeMappingTests
{
    /// <summary>
    ///     Enum values that are intentionally absent from <see cref="PingenSerialisationHelper.PingenApiDataTypeMapping" />
    ///     pending follow-up work. Each entry must be tracked in <c>doc/analysis/2026-05-01-api-docs-gap-audit.md</c>
    ///     and a corresponding sub-issue (#106 / #107 / #108 / #110) so the gap is not forgotten.
    ///     Adding to this allow-list is a deliberate policy decision; do not extend it without updating the audit document.
    /// </summary>
    private static readonly HashSet<PingenApiDataType> KnownUnmappedDataTypes = [PingenApiDataType.presets];

    /// <summary>
    ///     Asserts every <see cref="PingenApiDataType" /> enum value is either registered in the mapping
    ///     or explicitly recorded in <see cref="KnownUnmappedDataTypes" />. A new enum value that ships
    ///     without either a mapping or an allow-list entry will fail this test, preventing the
    ///     silent-skip regression described in <c>doc/ai-readiness.md § 3.1</c>.
    /// </summary>
    [Test]
    public void PingenApiDataTypeMapping_HasEntryOrIsKnownUnmapped_ForEveryEnumValue()
    {
        Dictionary<PingenApiDataType, Type> mapping = PingenSerialisationHelper.PingenApiDataTypeMapping;
        PingenApiDataType[] enumValues = Enum.GetValues<PingenApiDataType>();

        var unaccounted = enumValues
            .Where(v => !mapping.ContainsKey(v) && !KnownUnmappedDataTypes.Contains(v))
            .ToList();

        unaccounted.ShouldBeEmpty(
            "Every PingenApiDataType value must either be present in PingenSerialisationHelper.PingenApiDataTypeMapping "
            + "or be listed in KnownUnmappedDataTypes (with a corresponding entry in the audit document). "
            + $"Missing: {string.Join(", ", unaccounted)}");
    }

    /// <summary>
    ///     Asserts every CLR <see cref="Type" /> value in the mapping is non-null and implements
    ///     <see cref="IAttributes" />. <c>IncludedCollection.OfType&lt;T&gt;</c> constrains <c>T</c> to
    ///     <see cref="IAttributes" />; a non-conforming entry would compile but fail at runtime.
    /// </summary>
    [Test]
    public void PingenApiDataTypeMapping_AllMappedTypes_AreNonNullAndImplementIAttributes()
    {
        Dictionary<PingenApiDataType, Type> mapping = PingenSerialisationHelper.PingenApiDataTypeMapping;

        mapping.ShouldSatisfyAllConditions(
            mapping.Select<KeyValuePair<PingenApiDataType, Type>, Action>(kvp =>
                () =>
                {
                    kvp.Value.ShouldNotBeNull($"Mapping value for {kvp.Key} must not be null.");
                    typeof(IAttributes).IsAssignableFrom(kvp.Value).ShouldBeTrue(
                        $"Mapping for {kvp.Key} resolves to {kvp.Value.FullName} which does not implement IAttributes.");
                }).ToArray()
        );
    }

    /// <summary>
    ///     Asserts the <see cref="KnownUnmappedDataTypes" /> allow-list does not drift from the enum.
    ///     Every entry must reference a real <see cref="PingenApiDataType" /> value, and any value
    ///     that gains a mapping must be removed from the allow-list (otherwise the audit becomes stale).
    /// </summary>
    [Test]
    public void KnownUnmappedDataTypes_StaysConsistentWithEnumAndMapping()
    {
        var enumValues = Enum.GetValues<PingenApiDataType>().ToHashSet();
        Dictionary<PingenApiDataType, Type> mapping = PingenSerialisationHelper.PingenApiDataTypeMapping;

        var nonExistent = KnownUnmappedDataTypes.Where(v => !enumValues.Contains(v)).ToList();
        var alreadyMapped = KnownUnmappedDataTypes.Where(v => mapping.ContainsKey(v)).ToList();

        KnownUnmappedDataTypes.ShouldSatisfyAllConditions(
            () => nonExistent.ShouldBeEmpty(
                $"KnownUnmappedDataTypes references values that are not in the PingenApiDataType enum: {string.Join(", ", nonExistent)}."),
            () => alreadyMapped.ShouldBeEmpty(
                $"KnownUnmappedDataTypes contains values that ARE present in the mapping: {string.Join(", ", alreadyMapped)}. "
                + "Remove them from the allow-list now that the gap is closed.")
        );
    }
}
