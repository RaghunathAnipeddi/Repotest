using System;
using System.Collections.Generic;

namespace Chempoint.GP.Model.Interactions.Sales
{
    public class PickTicketResponse
    {
        public ResponseStatus Status { get; set; }

        public string Message { get; set; }

        public bool IsXmlIntegratedWarehouse { get; set; }

        public bool IsEdiIntegratedWarehouse { get; set; }

        public bool IsIntegratedCarrier { get; set; }
     
        public string EdiPTSendOperation { get; set; }
       
        public string EdiPTSendErrorMessage { get; set; }

        public bool IsXmlPTSendBefore { get; set; }

        public string WarehouseId { get; set; }

        public string WarehouseName { get; set; }

        public string CarrierId { get; set; }

        public List<SalesDocumentStatus> SalesDocumentStatusList { get; set; }
    }

    public class SalesDocumentStatus
    {
        public string SopNumber { get; set; }

        public int SopType { get; set; }

        public bool IsIntegrated { get; set; }

        public bool CanSubmit { get; set; }

        public bool CanEdit { get; set; }

        public bool CanDelete { get; set; }

        public string WarehouseId { get; set; }

        public string WarehouseName { get; set; }

        public string ValidationMessage { get; set; }

        public DocumentStatus DocumentStatus { get; set; }

        public DocumentOperationStatus DocumentOperationStatus { get; set; }

        public bool IsActiveCarrier { get; set; }

        public string CarrierId { get; set; }
    }

    public enum DocumentStatus
    {
        XmlGenerated = 1,
        EdiGenerated = 2,
        AwaitingMdn = 3,
        MdnReceived = 4,
        AwaitingAcknowledgement = 5,
        AcknowledgementReceived = 6,
        AcknowledgementCancelled = 7,
    }

    public enum DocumentOperationStatus
    {
        Print = 1,
        ReprintAndCancel = 2,
        Reprint = 3,
        Cancel = 4,
        AwaitingAcknowledgement = 5,
        ShippingConfirmation = 6
    }

    public class Error
    {
        public string Status { get; set; }

        public string Message { get; set; }

        public string ErrorReferenceCode { get; set; }

        public string ExceptionMessage { get; set; }

        public string StackTrace { get; set; }
    }
}
