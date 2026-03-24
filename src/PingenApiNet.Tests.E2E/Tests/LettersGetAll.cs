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
using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Letters;

namespace PingenApiNet.Tests.E2E.Tests;

/// <summary>
///
/// </summary>
public class TestLetters : TestBase
{
    /// <summary>
    ///
    /// </summary>
    [Test]
    public async Task GetAllLetters()
    {
        var apiPagingRequest = new ApiPagingRequest
        {
            Sorting = new Dictionary<string, CollectionSortDirection>
            {
                [PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(letter => letter.CreatedAt)] = CollectionSortDirection.DESC
            },
            Filtering = new(
                CollectionFilterOperator.And,
                new KeyValuePair<string, object>[]
                {
                    new(CollectionFilterOperator.Or, new KeyValuePair<string, object>[]
                    {
                        new(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(letter => letter.Country), "CH"),
                        new(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(letter => letter.Country), "LI")
                    }),
                    new(PingenAttributesPropertyHelper<Letter>.GetJsonPropertyName(letter => letter.Status), "valid")
                })
        };

        PingenApiClient.ShouldNotBeNull();

        var res = await PingenApiClient!.Letters.GetPage(apiPagingRequest);
        res.ShouldNotBeNull();
        res.ShouldSatisfyAllConditions(
            () => res.IsSuccess.ShouldBeTrue(),
            () => res.ApiError.ShouldBeNull(),
            () => res.Data?.Data.ShouldNotBeNull()
        );

        var letters = new List<LetterData>();
        await foreach (var page in PingenApiClient.Letters.GetPageResultsAsync(apiPagingRequest))
        {
            letters.AddRange(page);
        }
        letters.ShouldNotBeNull();
        letters.ShouldNotBeEmpty();
    }

    /// <summary>
    ///
    /// </summary>
    [Test]
    public async Task GetLetter()
    {
        const string letterId = "0e738359-799e-4e23-9666-0a7dfe6096b4";

        PingenApiClient.ShouldNotBeNull();

        var res = await PingenApiClient!.Letters.Get(letterId);
        res.ShouldNotBeNull();
        res.ShouldSatisfyAllConditions(
            () => res.IsSuccess.ShouldBeTrue(),
            () => res.ApiError.ShouldBeNull(),
            () => res.Data?.Data.ShouldNotBeNull()
        );
    }
}
