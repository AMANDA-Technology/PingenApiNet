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

using PingenApiNet.Abstractions.Models.Letters;
using PingenApiNet.Tests.Integration.Helpers;

namespace PingenApiNet.Tests.Integration.Tests.CrossCutting;

/// <summary>
///     Cross-cutting integration tests verifying <see cref="CancellationToken" /> propagation
///     through <see cref="PingenConnectionHandler" />.
/// </summary>
[TestFixture]
public sealed class CancellationTokenTests : IntegrationTestBase
{
    /// <summary>
    ///     Verifies that an already-cancelled <see cref="CancellationToken" /> aborts the request
    ///     without dispatching it to the API endpoint.
    /// </summary>
    [Test]
    public async Task GetPage_WithCancelledToken_ShouldThrowAndNotSendRequest()
    {
        Server.StubJsonGet(OrgPath("batches"), PingenResponseFactory.BatchCollection());

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await Client.Batches.GetPage(cancellationToken: cts.Token));

        Server.VerifyNotCalled(OrgPath("batches"));
    }

    /// <summary>
    ///     Verifies that an already-cancelled <see cref="CancellationToken" /> aborts auto-pagination
    ///     without dispatching any request to the API endpoint.
    /// </summary>
    [Test]
    public async Task GetPageResultsAsync_WithCancelledToken_ShouldThrowAndNotSendRequest()
    {
        Server.StubJsonGet(OrgPath("letters"), PingenResponseFactory.LetterCollection());

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await foreach (IEnumerable<LetterData> _ in
                           Client.Letters.GetPageResultsAsync(cancellationToken: cts.Token))
            {
                // Should never iterate
            }
        });

        Server.VerifyNotCalled(OrgPath("letters"));
    }

    /// <summary>
    ///     Verifies that a single-item connector method (Get) also honours an already-cancelled token.
    /// </summary>
    [Test]
    public async Task Get_WithCancelledToken_ShouldThrowAndNotSendRequest()
    {
        string batchId = Guid.NewGuid().ToString();
        Server.StubJsonGet(OrgPath($"batches/{batchId}"), PingenResponseFactory.SingleBatch(batchId));

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await Client.Batches.Get(batchId, cancellationToken: cts.Token));

        Server.VerifyNotCalled(OrgPath($"batches/{batchId}"));
    }
}
