using System.Net;
using PingenApiNet.Abstractions.Models.Api;

namespace PingenApiNet.UnitTests.Tests.Models;

/// <summary>
/// Unit tests for <see cref="ExternalRequestResult"/>
/// </summary>
public class ExternalRequestResultTests
{
    /// <summary>
    /// Verifies default ExternalRequestResult properties
    /// </summary>
    [Test]
    public void ExternalRequestResult_DefaultValues_AreCorrect()
    {
        var result = new ExternalRequestResult();

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.StatusCode.ShouldBe(default(HttpStatusCode)),
            () => result.ReasonPhrase.ShouldBeNull()
        );
    }

    /// <summary>
    /// Verifies that ExternalRequestResult properties can be set via init
    /// </summary>
    [Test]
    public void ExternalRequestResult_WithValues_StoresCorrectly()
    {
        var result = new ExternalRequestResult
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            ReasonPhrase = "OK"
        };

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeTrue(),
            () => result.StatusCode.ShouldBe(HttpStatusCode.OK),
            () => result.ReasonPhrase.ShouldBe("OK")
        );
    }

    /// <summary>
    /// Verifies record equality for ExternalRequestResult
    /// </summary>
    [Test]
    public void ExternalRequestResult_RecordEquality_Works()
    {
        var result1 = new ExternalRequestResult
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            ReasonPhrase = "OK"
        };

        var result2 = new ExternalRequestResult
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            ReasonPhrase = "OK"
        };

        result1.ShouldBe(result2);
    }

    /// <summary>
    /// Verifies record inequality for ExternalRequestResult with different status codes
    /// </summary>
    [Test]
    public void ExternalRequestResult_DifferentStatusCodes_AreNotEqual()
    {
        var success = new ExternalRequestResult
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            ReasonPhrase = "OK"
        };

        var failure = new ExternalRequestResult
        {
            IsSuccess = false,
            StatusCode = HttpStatusCode.Forbidden,
            ReasonPhrase = "Forbidden"
        };

        success.ShouldNotBe(failure);
    }

    /// <summary>
    /// Verifies ExternalRequestResult with error status codes
    /// </summary>
    [Test]
    public void ExternalRequestResult_ErrorStatusCodes_StoredCorrectly()
    {
        var result = new ExternalRequestResult
        {
            IsSuccess = false,
            StatusCode = HttpStatusCode.ServiceUnavailable,
            ReasonPhrase = "Service Unavailable"
        };

        result.ShouldSatisfyAllConditions(
            () => result.IsSuccess.ShouldBeFalse(),
            () => result.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable),
            () => result.ReasonPhrase.ShouldBe("Service Unavailable")
        );
    }
}
