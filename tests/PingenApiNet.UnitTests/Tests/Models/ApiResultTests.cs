using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults.Embedded;
using PingenApiNet.Abstractions.Models.Letters;

namespace PingenApiNet.UnitTests.Tests.Models;

/// <summary>
/// Unit tests for <see cref="ApiResult"/> and <see cref="ApiResult{T}"/>
/// </summary>
public class ApiResultTests
{
    /// <summary>
    /// Verifies default ApiResult properties
    /// </summary>
    [Test]
    public void ApiResult_DefaultValues_AreCorrect()
    {
        var result = new ApiResult();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.RequestId.ShouldBe(Guid.Empty),
            () => result.RateLimitLimit.ShouldBe(0),
            () => result.RateLimitRemaining.ShouldBe(0),
            () => result.RateLimitReset.ShouldBeNull(),
            () => result.RetryAfter.ShouldBeNull(),
            () => result.IdempotentReplayed.ShouldBeFalse(),
            () => result.ApiError.ShouldBeNull(),
            () => result.Location.ShouldBeNull()
        );
    }

    /// <summary>
    /// Verifies that ApiResult properties can be set via init
    /// </summary>
    [Test]
    public void ApiResult_WithValues_StoresCorrectly()
    {
        var requestId = Guid.NewGuid();
        var resetTime = DateTimeOffset.UtcNow;
        var location = new Uri("https://example.com/redirect");

        var result = new ApiResult
        {
            IsSuccess = true,
            RequestId = requestId,
            RateLimitLimit = 100,
            RateLimitRemaining = 99,
            RateLimitReset = resetTime,
            RetryAfter = 30,
            IdempotentReplayed = true,
            Location = location
        };

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.RequestId.ShouldBe(requestId),
            () => result.RateLimitLimit.ShouldBe(100),
            () => result.RateLimitRemaining.ShouldBe(99),
            () => result.RateLimitReset.ShouldBe(resetTime),
            () => result.RetryAfter.ShouldBe(30),
            () => result.IdempotentReplayed.ShouldBeTrue(),
            () => result.Location.ShouldBe(location)
        );
    }

    /// <summary>
    /// Verifies that ApiResult{T} can hold SingleResult data
    /// </summary>
    [Test]
    public void ApiResultGeneric_WithData_StoresData()
    {
        var result = new ApiResult<SingleResult<LetterData>>
        {
            IsSuccess = true,
            Data = null
        };

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.Data.ShouldBeNull()
        );
    }

    /// <summary>
    /// Verifies that ApiResult{T} inherits base ApiResult properties
    /// </summary>
    [Test]
    public void ApiResultGeneric_InheritsBaseProperties()
    {
        var requestId = Guid.NewGuid();

        var result = new ApiResult<CollectionResult<LetterData>>
        {
            IsSuccess = true,
            RequestId = requestId,
            RateLimitLimit = 50,
            Data = null
        };

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.RequestId.ShouldBe(requestId),
            () => result.RateLimitLimit.ShouldBe(50)
        );
    }

    /// <summary>
    /// Verifies that ApiResult with ApiError stores error details
    /// </summary>
    [Test]
    public void ApiResult_WithApiError_StoresError()
    {
        var error = new ApiError([new("422", "Validation Error", "Field is invalid", new("/data/attributes/name", ""))]);

        var result = new ApiResult
        {
            IsSuccess = false,
            ApiError = error
        };

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.ApiError.ShouldNotBeNull(),
            () => result.ApiError!.Errors.Count.ShouldBe(1)
        );
    }

    /// <summary>
    /// Verifies CollectionResultMeta stores pagination values
    /// </summary>
    [Test]
    public void CollectionResultMeta_StoresPaginationValues()
    {
        var meta = new CollectionResultMeta(1, 5, 20, 1, 20, 100);

        meta.ShouldSatisfyAllConditions(
            () => meta.CurrentPage.ShouldBe(1),
            () => meta.LastPage.ShouldBe(5),
            () => meta.PerPage.ShouldBe(20),
            () => meta.From.ShouldBe(1),
            () => meta.To.ShouldBe(20),
            () => meta.Total.ShouldBe(100)
        );
    }
}
