using PingenApiNet.Abstractions.Exceptions;
using PingenApiNet.Abstractions.Models.Api;

namespace PingenApiNet.Tests.Tests.Unit.Exceptions;

/// <summary>
/// Unit tests for Pingen exception types
/// </summary>
public class PingenExceptionTests
{
    /// <summary>
    /// Verifies PingenApiErrorException stores the ApiResult
    /// </summary>
    [Test]
    public void PingenApiErrorException_WithApiResult_StoresApiResult()
    {
        var apiResult = new ApiResult { IsSuccess = false, RequestId = Guid.NewGuid() };

        var exception = new PingenApiErrorException(apiResult);

        Assert.That(exception.ApiResult, Is.SameAs(apiResult));
    }

    /// <summary>
    /// Verifies PingenApiErrorException stores message and ApiResult
    /// </summary>
    [Test]
    public void PingenApiErrorException_WithMessage_StoresMessage()
    {
        var apiResult = new ApiResult { IsSuccess = false };
        const string message = "Test error message";

        var exception = new PingenApiErrorException(apiResult, message);

        Assert.Multiple(() =>
        {
            Assert.That(exception.Message, Is.EqualTo(message));
            Assert.That(exception.ApiResult, Is.SameAs(apiResult));
        });
    }

    /// <summary>
    /// Verifies PingenApiErrorException stores inner exception
    /// </summary>
    [Test]
    public void PingenApiErrorException_WithInnerException_StoresInnerException()
    {
        var apiResult = new ApiResult { IsSuccess = false };
        var inner = new InvalidOperationException("inner");

        var exception = new PingenApiErrorException(apiResult, "msg", inner);

        Assert.Multiple(() =>
        {
            Assert.That(exception.InnerException, Is.SameAs(inner));
            Assert.That(exception.ApiResult, Is.SameAs(apiResult));
        });
    }

    /// <summary>
    /// Verifies PingenFileDownloadException stores error code
    /// </summary>
    [Test]
    public void PingenFileDownloadException_WithErrorCode_StoresErrorCode()
    {
        const string errorCode = "AccessDenied";

        var exception = new PingenFileDownloadException(errorCode);

        Assert.That(exception.ErrorCode, Is.EqualTo(errorCode));
    }

    /// <summary>
    /// Verifies PingenFileDownloadException with message
    /// </summary>
    [Test]
    public void PingenFileDownloadException_WithMessage_StoresMessageAndCode()
    {
        const string errorCode = "NoSuchKey";
        const string message = "File not found";

        var exception = new PingenFileDownloadException(errorCode, message);

        Assert.Multiple(() =>
        {
            Assert.That(exception.ErrorCode, Is.EqualTo(errorCode));
            Assert.That(exception.Message, Is.EqualTo(message));
        });
    }

    /// <summary>
    /// Verifies PingenFileDownloadException with inner exception
    /// </summary>
    [Test]
    public void PingenFileDownloadException_WithInnerException_StoresAll()
    {
        var inner = new Exception("inner");

        var exception = new PingenFileDownloadException("err", "msg", inner);

        Assert.That(exception.InnerException, Is.SameAs(inner));
    }

    /// <summary>
    /// Verifies PingenFileDownloadException accepts null error code
    /// </summary>
    [Test]
    public void PingenFileDownloadException_NullErrorCode_Allowed()
    {
        var exception = new PingenFileDownloadException(null);

        Assert.That(exception.ErrorCode, Is.Null);
    }

    /// <summary>
    /// Verifies PingenWebhookValidationErrorException stores webhook data
    /// </summary>
    [Test]
    public void PingenWebhookValidationErrorException_WithNullData_Allowed()
    {
        var exception = new PingenWebhookValidationErrorException(null);

        Assert.That(exception.WebhookEventData, Is.Null);
    }

    /// <summary>
    /// Verifies PingenWebhookValidationErrorException with message
    /// </summary>
    [Test]
    public void PingenWebhookValidationErrorException_WithMessage_StoresMessage()
    {
        const string message = "Validation failed";

        var exception = new PingenWebhookValidationErrorException(null, message);

        Assert.That(exception.Message, Is.EqualTo(message));
    }

    /// <summary>
    /// Verifies PingenWebhookValidationErrorException with inner exception
    /// </summary>
    [Test]
    public void PingenWebhookValidationErrorException_WithInnerException_StoresAll()
    {
        var inner = new Exception("inner");

        var exception = new PingenWebhookValidationErrorException(null, "msg", inner);

        Assert.That(exception.InnerException, Is.SameAs(inner));
    }
}
