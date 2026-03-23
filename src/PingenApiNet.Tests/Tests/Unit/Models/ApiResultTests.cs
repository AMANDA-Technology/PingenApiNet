using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults.Embedded;
using PingenApiNet.Abstractions.Models.Letters;

namespace PingenApiNet.Tests.Tests.Unit.Models;

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

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.RequestId, Is.EqualTo(Guid.Empty));
            Assert.That(result.RateLimitLimit, Is.EqualTo(0));
            Assert.That(result.RateLimitRemaining, Is.EqualTo(0));
            Assert.That(result.RateLimitReset, Is.Null);
            Assert.That(result.RetryAfter, Is.Null);
            Assert.That(result.IdempotentReplayed, Is.False);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Location, Is.Null);
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.RequestId, Is.EqualTo(requestId));
            Assert.That(result.RateLimitLimit, Is.EqualTo(100));
            Assert.That(result.RateLimitRemaining, Is.EqualTo(99));
            Assert.That(result.RateLimitReset, Is.EqualTo(resetTime));
            Assert.That(result.RetryAfter, Is.EqualTo(30));
            Assert.That(result.IdempotentReplayed, Is.True);
            Assert.That(result.Location, Is.EqualTo(location));
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Null);
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.RequestId, Is.EqualTo(requestId));
            Assert.That(result.RateLimitLimit, Is.EqualTo(50));
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ApiError, Is.Not.Null);
            Assert.That(result.ApiError!.Errors, Has.Count.EqualTo(1));
        });
    }

    /// <summary>
    /// Verifies CollectionResultMeta stores pagination values
    /// </summary>
    [Test]
    public void CollectionResultMeta_StoresPaginationValues()
    {
        var meta = new CollectionResultMeta(1, 5, 20, 1, 20, 100);

        Assert.Multiple(() =>
        {
            Assert.That(meta.CurrentPage, Is.EqualTo(1));
            Assert.That(meta.LastPage, Is.EqualTo(5));
            Assert.That(meta.PerPage, Is.EqualTo(20));
            Assert.That(meta.From, Is.EqualTo(1));
            Assert.That(meta.To, Is.EqualTo(20));
            Assert.That(meta.Total, Is.EqualTo(100));
        });
    }
}
