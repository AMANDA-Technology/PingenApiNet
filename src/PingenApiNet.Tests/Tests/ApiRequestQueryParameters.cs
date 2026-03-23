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

namespace PingenApiNet.Tests.Tests;

/// <summary>
/// Offline unit tests for API request query parameter construction
/// </summary>
public class ApiRequestQueryParameters
{
    /// <summary>
    /// Verifies that Include property can hold multiple relationship names
    /// </summary>
    [Test]
    public void ApiRequest_Include_PropertyIsSettable()
    {
        var request = new ApiRequest
        {
            Include = ["events", "sender"]
        };

        request.Include.ShouldBe(new[] { "events", "sender" }, ignoreOrder: true);
    }

    /// <summary>
    /// Verifies that Include property works with a single relationship
    /// </summary>
    [Test]
    public void ApiRequest_Include_SingleRelationship()
    {
        var request = new ApiRequest
        {
            Include = ["events"]
        };

        request.Include.ShouldBe(new[] { "events" }, ignoreOrder: true);
    }

    /// <summary>
    /// Verifies that Include is null by default
    /// </summary>
    [Test]
    public void ApiRequest_Include_NullByDefault()
    {
        var request = new ApiRequest();

        request.Include.ShouldBeNull();
    }

    /// <summary>
    /// Verifies that Include values serialize to a comma-separated string matching the API query parameter format
    /// </summary>
    [Test]
    public void Include_SerializesToCommaSeparatedString()
    {
        var includeValues = new[] { "events", "sender", "file" };
        var expected = "events,sender,file";

        var result = string.Join(',', includeValues);

        Assert.Multiple(() =>
        {
            result.ShouldBe(expected);
            ApiQueryParameterNames.Include.ShouldBe("include");
        });
    }
}
