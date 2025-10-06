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

using System.Runtime.InteropServices;
using PingenApiNet.Abstractions.Models.Api;
using PingenApiNet.Abstractions.Models.Api.Embedded.DataResults;
using PingenApiNet.Abstractions.Models.Files;

namespace PingenApiNet.Interfaces.Connectors;

/// <summary>
/// Pingen letter service endpoint. <see href="https://api.pingen.com/documentation#tag/misc.files">API Doc - Files</see>
/// </summary>
public interface IFilesService
{
    /// <summary>
    /// Get details for file upload. <see href="https://api.pingen.com/documentation#tag/misc.files/operation/files.file-upload">API Doc - Files details</see>
    /// </summary>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<ApiResult<SingleResult<FileUploadData>>> GetPath([Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Upload a file to URL received in result from <see cref="GetPath"/>
    /// </summary>
    /// <param name="fileUploadData">Result from <see cref="GetPath"/></param>
    /// <param name="data">Binary file to upload as stream</param>
    /// <param name="cancellationToken">Optional, A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public Task<bool> UploadFile(FileUploadData fileUploadData, Stream data, [Optional] CancellationToken cancellationToken);
}
