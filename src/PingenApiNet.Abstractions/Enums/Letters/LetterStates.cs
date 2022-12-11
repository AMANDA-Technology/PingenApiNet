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

namespace PingenApiNet.Abstractions.Enums.Letters;

/// <summary>
/// Letter States
/// <br/>WARNING: This is a copied list from js of pingen web interface. There is nothing about this in the API Doc. Please be careful, this list might be incomplete or wrong.
/// </summary>
public static class LetterStates
{
    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Validating = "validating";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Cancelled = "cancelled";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Cancelling = "cancelling";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Unprintable = "unprintable";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Fixing = "fixing";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Invalid = "invalid";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Submitted = "submitted";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Accepted = "accepted";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Printing = "printing";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Processing = "processing";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Sent = "sent";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string ActionRequired = "action_required";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Undeliverable = "undeliverable";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Valid = "valid";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string AwaitingCredits = "awaiting_credits";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Expired = "expired";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Transferring = "transferring";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Inspection = "inspection";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Rejected = "rejected";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Delivered = "delivered";
}
