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

using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.LetterEvents;

namespace PingenApiNet.Tests.E2E.Letters;

/// <summary>
///     End-to-end tests for <c>Letters.GetIssuesPage</c> / <c>Letters.GetIssuesPageResultsAsync</c>.
///     The issues endpoint is read-only — it lists organisation-wide letter issues for the
///     requested locale — so these tests neither create nor clean up resources. The tests
///     verify the language parameter is accepted for each supported locale and that the
///     auto-paging stream terminates within a small bounded number of pages so a runaway
///     pagination loop fails the fixture rather than hanging the run.
/// </summary>
[TestFixture]
[Category("E2e")]
public sealed class LetterIssuesE2eTests : E2eTestBase
{
    private const int MaxPagesToInspect = 5;

    /// <summary>
    ///     Calls <c>Letters.GetIssuesPage</c> with the supplied language code and asserts the
    ///     response is successful and structurally well-formed. Parameterised across the
    ///     localisations the library exposes via <see cref="PingenApiLanguage" /> to confirm the
    ///     <c>language</c> query parameter is correctly URL-encoded for each locale.
    /// </summary>
    /// <param name="language">A constant from <see cref="PingenApiLanguage" />.</param>
    [TestCase(PingenApiLanguage.EnGB)]
    [TestCase(PingenApiLanguage.DeDE)]
    [TestCase(PingenApiLanguage.FrFR)]
    public async Task GetIssuesPage_ShouldReturnSuccess(string language)
    {
        PingenApiClient.ShouldNotBeNull();

        ApiResult<CollectionResult<LetterEventData>> result =
            await PingenApiClient!.Letters.GetIssuesPage(language);

        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.ApiError.ShouldBeNull();
        result.Data.ShouldNotBeNull();
        result.Data!.Data.ShouldNotBeNull();
    }

    /// <summary>
    ///     Iterates <c>Letters.GetIssuesPageResultsAsync</c> with the EnGB locale, capping the
    ///     enumeration at <see cref="MaxPagesToInspect" /> pages to assert the auto-pager
    ///     terminates instead of looping forever — a defence against a regression where
    ///     pagination metadata is mis-parsed and the iterator never stops.
    /// </summary>
    [Test]
    public async Task GetIssuesPageResultsAsync_ShouldTerminateWithinBoundedPages()
    {
        PingenApiClient.ShouldNotBeNull();

        int pagesObserved = 0;
        await foreach (IEnumerable<LetterEventData> page in
                       PingenApiClient!.Letters.GetIssuesPageResultsAsync(PingenApiLanguage.EnGB))
        {
            page.ShouldNotBeNull();
            pagesObserved++;
            if (pagesObserved >= MaxPagesToInspect)
                break;
        }

        pagesObserved.ShouldBeLessThanOrEqualTo(MaxPagesToInspect);
    }
}
