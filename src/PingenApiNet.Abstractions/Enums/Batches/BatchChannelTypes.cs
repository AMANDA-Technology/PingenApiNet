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

namespace PingenApiNet.Abstractions.Enums.Batches;

/// <summary>
/// Delivery channels a batch can dispatch through, i.e. the values of <c>Batch.ChannelType</c>.
/// <br/>Added by Pingen's 2026-07-27 deliverable rollout, which generalised "letter" into a deliverable that
/// can travel by post, ebill or email. Before that a batch was implicitly always postal.
/// <para>
/// Bound as a plain <c>string</c> on the model rather than a C# enum — see
/// <see cref="Letters.LetterSources"/> for why. This library only sends postal batches
/// (<see cref="Post"/>); the other two channels are not implemented (issue #125), so treat them as read-only
/// information about batches created elsewhere.
/// </para>
/// </summary>
public static class BatchChannelTypes
{
    /// <summary>
    /// Physical mail — the only channel this library can create batches for.
    /// </summary>
    public const string Post = "post";

    /// <summary>
    /// Swiss eBill delivery. Not implemented by this library.
    /// </summary>
    public const string Ebill = "ebill";

    /// <summary>
    /// Email delivery. Not implemented by this library.
    /// </summary>
    public const string Email = "email";
}
