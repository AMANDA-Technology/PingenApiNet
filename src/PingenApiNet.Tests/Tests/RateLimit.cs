/* Copyright (C) AMANDA Technology - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Manuel Gysin <manuel.gysin@amanda-technology.ch>
 * Written by Philip Näf <philip.naef@amanda-technology.ch>
 */

using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Helpers;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Letters;

namespace PingenApiNet.Tests.Tests;

public class RateLimit : TestBase
{
    private static readonly ApiPagingRequest ApiPagingRequest = new()
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

    [Test]
    public async Task Some()
    {
        var hasRateLimitReached = false;

        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        await Parallel.ForEachAsync(
            ParallelDelays(10, 2), cts.Token, async (delay, cancellationToken) =>
            {
                var lastRemaining = int.MaxValue;
                var client = CreateClient();

                while (!cancellationToken.IsCancellationRequested)
                {
                    var res = await client.Letters.GetPage(ApiPagingRequest, cancellationToken);

                    // Break when rate limit has been reset
                    if (res.RateLimitRemaining > lastRemaining)
                    {
                        await Console.Out.WriteLineAsync($"Cancel loop because rate limit has been reset from {lastRemaining} to {res.RateLimitRemaining}");
                        break;
                    }

                    // Assert rate limit reached and retry after is set
                    if (!res.IsSuccess)
                    {
                        hasRateLimitReached = res.RateLimitRemaining <= 0;
                        await Console.Out.WriteLineAsync($"Call has been failed due to rate limit and can be repeated in {res.RetryAfter} seconds");

                        Assert.Multiple(() =>
                        {
                            Assert.That(res.RateLimitRemaining, Is.LessThanOrEqualTo(0));
                            Assert.That(res.RateLimitReset, Is.Not.Null);
                            Assert.That(res.RetryAfter, Is.Not.Null);
                        });

                        // Assert that call is success when repeated after given time
                        while (DateTimeOffset.UtcNow < res.RateLimitReset && !cancellationToken.IsCancellationRequested)
                            await Task.Delay(100, cancellationToken);

                        var resRepeat = await client.Letters.GetPage(ApiPagingRequest, cancellationToken);
                        Assert.That(resRepeat.IsSuccess, Is.True);
                        await Console.Out.WriteLineAsync("Call has been succeeded at retry");
                        break;
                    }

                    lastRemaining = res.RateLimitRemaining;
                    await Task.Delay(delay, cancellationToken);
                }
            });

        Assert.Multiple(() =>
        {
            Assert.That(hasRateLimitReached, Is.True);
            Assert.That(cts.IsCancellationRequested, Is.False);
        });
        return;

        IEnumerable<int> ParallelDelays(int parallelCount, int delayMilliseconds)
        {
            for (var i = 0; i < parallelCount; i++)
                yield return delayMilliseconds;
        }
    }
}
