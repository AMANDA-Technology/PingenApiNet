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
using PingenApiNet.Abstractions.Models.LetterEvents;

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
    ///     Enum values that are intentionally absent from <see cref="PingenSerialisationHelper.PingenApiDataTypeMapping" />.
    ///     Each entry must be tracked in <c>doc/analysis/2026-05-01-api-docs-gap-audit.md</c> so the decision is not
    ///     forgotten. Adding to this allow-list is a deliberate policy decision; do not extend it without updating
    ///     the audit document.
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="PingenApiDataType.presets" /> — pending follow-up work (#106 model + service,
    ///             #108 mapping wiring): the value is only ever sent on relationship inputs, and no <c>Preset</c>
    ///             attributes type exists to bind a response to yet.
    ///         </item>
    ///         <item>
    ///             <see cref="PingenApiDataType.deliverables_events" /> — <b>permanently unmapped by design</b>, not
    ///             a gap. Since 2026-07-27 Pingen puts the same delivery event into <c>included</c> twice, typed
    ///             <c>letters_events</c> and <c>deliverables_events</c> with one shared id. Mapping both to
    ///             <see cref="LetterEvent" /> would give <c>IncludedCollection.OfType&lt;LetterEvent&gt;()</c> two
    ///             matches and make <see cref="PingenSerialisationHelper.TryGetIncludedData{T}" /> throw — the
    ///             behaviour pinned by <c>PingenSerialisationHelperTests.TryGetIncludedData_MultipleMatches_Throws</c>
    ///             — re-breaking every webhook. Do not "close" this entry by adding a mapping.
    ///         </item>
    ///     </list>
    /// </summary>
    private static readonly HashSet<PingenApiDataType> KnownUnmappedDataTypes =
        [PingenApiDataType.presets, PingenApiDataType.deliverables_events];

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
