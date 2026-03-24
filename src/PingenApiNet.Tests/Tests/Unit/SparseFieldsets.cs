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

using System.Reflection;
using PingenApiNet.Abstractions.Enums.Api;
using PingenApiNet.Abstractions.Models.Api;

namespace PingenApiNet.Tests.Tests.Unit;

/// <summary>
/// Tests for sparse fieldset query parameter support
/// </summary>
public class SparseFieldsets
{
    /// <summary>
    /// Verify that ApiRequest has a SparseFieldsets property of the correct type
    /// </summary>
    [Test]
    public void ApiRequest_HasSparseFieldsetsProperty()
    {
        var request = new ApiRequest
        {
            SparseFieldsets = new[]
            {
                new KeyValuePair<PingenApiDataType, IEnumerable<string>>(PingenApiDataType.letters, new[] { "name", "status" })
            }
        };

        request.SparseFieldsets.ShouldNotBeNull();
        request.SparseFieldsets!.Count().ShouldBe(1);
    }

    /// <summary>
    /// Verify that SparseFieldsets defaults to null
    /// </summary>
    [Test]
    public void ApiRequest_SparseFieldsetsDefaultsToNull()
    {
        var request = new ApiRequest();
        request.SparseFieldsets.ShouldBeNull();
    }

    /// <summary>
    /// Verify that ApiPagingRequest inherits SparseFieldsets from ApiRequest
    /// </summary>
    [Test]
    public void ApiPagingRequest_InheritsSparseFieldsetsFromApiRequest()
    {
        var request = new ApiPagingRequest
        {
            SparseFieldsets = new[]
            {
                new KeyValuePair<PingenApiDataType, IEnumerable<string>>(PingenApiDataType.letters, new[] { "name", "status" }),
                new KeyValuePair<PingenApiDataType, IEnumerable<string>>(PingenApiDataType.organisations, new[] { "name" })
            },
            PageLimit = 10
        };

        request.ShouldSatisfyAllConditions(
            () => request.SparseFieldsets.ShouldNotBeNull(),
            () => request.SparseFieldsets!.Count().ShouldBe(2),
            () => request.PageLimit.ShouldBe(10)
        );
    }

    /// <summary>
    /// Verify that ApiQueryParameterNames.SparseFields generates correct format
    /// </summary>
    [Test]
    public void ApiQueryParameterNames_SparseFields_GeneratesCorrectFormat()
    {
        "ApiQueryParameterNames".ShouldSatisfyAllConditions(
            () => ApiQueryParameterNames.SparseFields(PingenApiDataType.letters).ShouldBe("fields[letters]"),
            () => ApiQueryParameterNames.SparseFields(PingenApiDataType.organisations).ShouldBe("fields[organisations]"),
            () => ApiQueryParameterNames.SparseFields(PingenApiDataType.webhooks).ShouldBe("fields[webhooks]"),
            () => ApiQueryParameterNames.SparseFields(PingenApiDataType.batches).ShouldBe("fields[batches]"),
            () => ApiQueryParameterNames.SparseFields(PingenApiDataType.users).ShouldBe("fields[users]")
        );
    }

    /// <summary>
    /// Verify that GetQueryParameters serializes sparse fieldsets correctly using reflection
    /// </summary>
    [Test]
    public void GetQueryParameters_SerializesSparseFieldsets()
    {
        var apiRequest = new ApiRequest
        {
            SparseFieldsets = new[]
            {
                new KeyValuePair<PingenApiDataType, IEnumerable<string>>(PingenApiDataType.letters, new[] { "name", "status" })
            }
        };

        var result = InvokeGetQueryParameters(apiRequest);

        result.ShouldNotBeNull();
        var parameters = result!.ToList();
        parameters.Count.ShouldBe(1);
        parameters.ShouldSatisfyAllConditions(
            () => parameters[0].Key.ShouldBe("fields[letters]"),
            () => parameters[0].Value.ShouldBe("name,status")
        );
    }

    /// <summary>
    /// Verify that GetQueryParameters serializes multiple sparse fieldsets
    /// </summary>
    [Test]
    public void GetQueryParameters_SerializesMultipleSparseFieldsets()
    {
        var apiRequest = new ApiRequest
        {
            SparseFieldsets = new[]
            {
                new KeyValuePair<PingenApiDataType, IEnumerable<string>>(PingenApiDataType.letters, new[] { "name", "status" }),
                new KeyValuePair<PingenApiDataType, IEnumerable<string>>(PingenApiDataType.organisations, new[] { "name" })
            }
        };

        var result = InvokeGetQueryParameters(apiRequest);

        result.ShouldNotBeNull();
        var parameters = result!.ToList();
        parameters.Count.ShouldBe(2);
        parameters.ShouldSatisfyAllConditions(
            () => parameters[0].Key.ShouldBe("fields[letters]"),
            () => parameters[0].Value.ShouldBe("name,status"),
            () => parameters[1].Key.ShouldBe("fields[organisations]"),
            () => parameters[1].Value.ShouldBe("name")
        );
    }

    /// <summary>
    /// Verify that GetQueryParameters handles null SparseFieldsets
    /// </summary>
    [Test]
    public void GetQueryParameters_HandlesNullSparseFieldsets()
    {
        var apiRequest = new ApiRequest();

        var result = InvokeGetQueryParameters(apiRequest);

        result.ShouldNotBeNull();
        var parameters = result!.ToList();
        parameters.Count.ShouldBe(0);
    }

    /// <summary>
    /// Verify that GetQueryParameters handles empty SparseFieldsets
    /// </summary>
    [Test]
    public void GetQueryParameters_HandlesEmptySparseFieldsets()
    {
        var apiRequest = new ApiRequest
        {
            SparseFieldsets = []
        };

        var result = InvokeGetQueryParameters(apiRequest);

        result.ShouldNotBeNull();
        var parameters = result!.ToList();
        parameters.Count.ShouldBe(0);
    }

    /// <summary>
    /// Verify that sparse fieldsets are serialized before paging-specific parameters
    /// when ApiPagingRequest is used
    /// </summary>
    [Test]
    public void GetQueryParameters_SparseFieldsetsWithPagingRequest()
    {
        var apiPagingRequest = new ApiPagingRequest
        {
            SparseFieldsets = new[]
            {
                new KeyValuePair<PingenApiDataType, IEnumerable<string>>(PingenApiDataType.letters, new[] { "name" })
            },
            PageLimit = 10,
            PageNumber = 1
        };

        var result = InvokeGetQueryParameters(apiPagingRequest);

        result.ShouldNotBeNull();
        var parameters = result!.ToList();
        parameters.Count.ShouldBeGreaterThanOrEqualTo(3);

        var fieldsParam = parameters.First(p => p.Key.StartsWith("fields["));
        fieldsParam.ShouldSatisfyAllConditions(
            () => fieldsParam.Key.ShouldBe("fields[letters]"),
            () => fieldsParam.Value.ShouldBe("name")
        );
    }

    /// <summary>
    /// Verify that IPingenConnectionHandler has GetAsync overloads that accept ApiRequest
    /// </summary>
    [Test]
    public void IPingenConnectionHandler_HasGetAsyncOverloadsWithApiRequest()
    {
        var interfaceType = typeof(IPingenConnectionHandler);

        // GetAsync<TResult>(string, ApiRequest?, CancellationToken) should exist
        var genericMethod = interfaceType.GetMethods()
            .Where(m => m.Name == "GetAsync" && m.IsGenericMethod)
            .FirstOrDefault(m =>
            {
                var parameters = m.GetParameters();
                return parameters.Length == 3
                       && parameters[0].ParameterType == typeof(string)
                       && parameters[1].ParameterType == typeof(ApiRequest);
            });
        genericMethod.ShouldNotBeNull("GetAsync<TResult>(string, ApiRequest?, CancellationToken) should exist on IPingenConnectionHandler");

        // GetAsync(string, ApiRequest?, CancellationToken) should exist
        var nonGenericMethod = interfaceType.GetMethods()
            .Where(m => m.Name == "GetAsync" && !m.IsGenericMethod)
            .FirstOrDefault(m =>
            {
                var parameters = m.GetParameters();
                return parameters.Length == 3
                       && parameters[0].ParameterType == typeof(string)
                       && parameters[1].ParameterType == typeof(ApiRequest);
            });
        nonGenericMethod.ShouldNotBeNull("GetAsync(string, ApiRequest?, CancellationToken) should exist on IPingenConnectionHandler");
    }

    /// <summary>
    /// Verify that connector service Get methods accept optional ApiRequest
    /// </summary>
    [Test]
    public void ConnectorServiceInterfaces_GetMethodsAcceptApiRequest()
    {
        // ILetterService.Get should have overload with ApiRequest
        var letterGet = typeof(PingenApiNet.Interfaces.Connectors.ILetterService)
            .GetMethod("Get", [typeof(string), typeof(ApiRequest), typeof(CancellationToken)]);
        letterGet.ShouldNotBeNull("ILetterService.Get should accept ApiRequest");

        // IBatchService.Get should have overload with ApiRequest
        var batchGet = typeof(PingenApiNet.Interfaces.Connectors.IBatchService)
            .GetMethod("Get", [typeof(string), typeof(ApiRequest), typeof(CancellationToken)]);
        batchGet.ShouldNotBeNull("IBatchService.Get should accept ApiRequest");

        // IOrganisationService.Get should have overload with ApiRequest
        var orgGet = typeof(PingenApiNet.Interfaces.Connectors.IOrganisationService)
            .GetMethod("Get", [typeof(string), typeof(ApiRequest), typeof(CancellationToken)]);
        orgGet.ShouldNotBeNull("IOrganisationService.Get should accept ApiRequest");

        // IWebhookService.Get should have overload with ApiRequest
        var webhookGet = typeof(PingenApiNet.Interfaces.Connectors.IWebhookService)
            .GetMethod("Get", [typeof(string), typeof(ApiRequest), typeof(CancellationToken)]);
        webhookGet.ShouldNotBeNull("IWebhookService.Get should accept ApiRequest");

        // IUserService.Get should have overload with ApiRequest
        var userGet = typeof(PingenApiNet.Interfaces.Connectors.IUserService)
            .GetMethod("Get", [typeof(ApiRequest), typeof(CancellationToken)]);
        userGet.ShouldNotBeNull("IUserService.Get should accept ApiRequest");
    }

    /// <summary>
    /// Invoke the private static GetQueryParameters method via reflection
    /// </summary>
    private static IEnumerable<KeyValuePair<string, string>>? InvokeGetQueryParameters(ApiRequest? apiRequest)
    {
        var method = typeof(PingenConnectionHandler)
            .GetMethod("GetQueryParameters", BindingFlags.NonPublic | BindingFlags.Static, [typeof(ApiRequest)]);

        return method?.Invoke(null, [apiRequest]) as IEnumerable<KeyValuePair<string, string>>;
    }
}
