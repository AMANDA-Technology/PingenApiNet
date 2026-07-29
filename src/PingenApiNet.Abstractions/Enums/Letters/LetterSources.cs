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
/// Sources a letter can originate from, i.e. the values of <c>Letter.Source</c>.
/// <br/>Documented by the API as the <c>source</c> enum on <c>LetterAttributes</c>.
/// <para>
/// Bound as a plain <c>string</c> on the model rather than a C# enum, deliberately: an unknown value in a
/// strictly-bound enum makes the whole response unparseable, which is the failure mode that caused the
/// 2026-07-27 webhook outage. Pingen has added values here before (the integration sources) without notice.
/// Compare against these constants instead of switching exhaustively.
/// </para>
/// </summary>
public static class LetterSources
{
    /// <summary>
    /// Created through the Pingen web application.
    /// </summary>
    public const string App = "app";

    /// <summary>
    /// Created through the API — what this library produces.
    /// </summary>
    public const string Api = "api";

    /// <summary>
    /// Created as part of a batch.
    /// </summary>
    public const string Batch = "batch";

    /// <summary>
    /// Created by the email integration.
    /// </summary>
    public const string IntegrationEmail = "integration_email";

    /// <summary>
    /// Created by the Amazon S3 integration.
    /// </summary>
    public const string IntegrationS3 = "integration_s3";

    /// <summary>
    /// Created by the Dropbox integration.
    /// </summary>
    public const string IntegrationDropbox = "integration_dropbox";

    /// <summary>
    /// Created by the Google Drive integration.
    /// </summary>
    public const string IntegrationGoogleDrive = "integration_googledrive";

    /// <summary>
    /// Created by the OneDrive integration.
    /// </summary>
    public const string IntegrationOneDrive = "integration_onedrive";
}
