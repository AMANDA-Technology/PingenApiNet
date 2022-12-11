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

namespace PingenApiNet.Abstractions.Enums.LetterEvents;

/// <summary>
/// Letter event codes
/// <br/>WARNING: This is a copied list from js of pingen web interface. There is nothing about this in the API Doc. Please be careful, this list might be incomplete or wrong.
/// </summary>
public static class LetterEventCodes
{
    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string AutoInitiatedSending = "auto_initiated_sending";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string ChangeAddressPosition = "change_address_position";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string InitiatedSending = "initiated_sending";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string QueuedForTransfer = "queued_for_transfer";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string TransferredToPool = "transferred_to_pool";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Accepted = "accepted";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string AddressPositionChanged = "address_position_changed";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string AddressPurged = "address_purged";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string AutoSubmitted = "auto_submitted";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string CancellationCompleted = "cancellation_completed";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string CancellationRejected = "cancellation_rejected";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string CancellationRequested = "cancellation_requested";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string ContentFailedInspection = "content_failed_inspection";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string ContentInspectionError = "content_inspection_error";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string ContentPassedInspection = "content_passed_inspection";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Created = "created";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Delivered = "delivered";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string FileNotA4Portrait = "file_not_a4_portrait";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string FileNotFound = "file_not_found";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string FileNotReadable = "file_not_readable";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string FileRemoved = "file_removed";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string FileSizeOverLimit = "file_size_over_limit";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string FileTooManyPages = "file_too_many_pages";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string InvalidPdfBinaryContent = "invalid_pdf_binary_content";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string MetaDataFailedRequirements = "meta_data_failed_requirements";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Processing = "processing";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string ReceivalConfirmedByDistributor = "receival_confirmed_by_distributor";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string RejectedFromPrintcenter = "rejected_from_printcenter";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Rerouted = "rerouted";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string RetractedFromPrintcenter = "retracted_from_printcenter";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string ShippingCountryBlocked = "shipping_country_blocked";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string ShippingOrganizationCountryBlocked = "shipping_organization_country_blocked";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Submitted = "submitted";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string TrackAndTrace = "track_and_trace";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string TransferredToDistributor = "transferred_to_distributor";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string TransferredToPrintcenter = "transferred_to_printcenter";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string UnableToFindDeliveryProduct = "unable_to_find_delivery_product";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string Undeliverable = "undeliverable";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string WaitingForFunds = "waiting_for_funds";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string WaitingForFundsExpired = "waiting_for_funds_expired";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string WaitingForLegitimacy = "waiting_for_legitimacy";

    /// <summary>
    /// NO API DOC available
    /// </summary>
    public const string WaitingForLegitimacyExpired = "waiting_for_legitimacy_expired";
}
