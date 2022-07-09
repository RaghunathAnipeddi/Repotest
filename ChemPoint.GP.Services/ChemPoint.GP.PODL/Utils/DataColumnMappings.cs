using System;
using System.Linq;

namespace ChemPoint.GP.Podl.Utils
{
    public static class DataColumnMappings
    {
        public static readonly DataColumnMap[] SavePurchaseIndicator = 
        {
            new DataColumnMap() { ColumnName = "PoNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "POLineNumber", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ItemNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "POIndicatorStatusId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "BackOrderReason", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "InitialBackOrderDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "CancelledReason", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "InitialCancelledDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "IsCostVariance", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "AcknowledgementDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "ConfirmedDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "ActualShipDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "CreatedBy", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "CreatedOn", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "ModifiedBy", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ModifiedOn", Type = typeof(DateTime) }
        };

        public static readonly DataColumnMap[] SaveXRMActivityLOG = 
        {
            new DataColumnMap() { ColumnName = "PONumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "POActivityTypeMasterID", Type = typeof(int) }
           
        };

        public static readonly DataColumnMap[] SaveEstimatedShipmentCostEntry = 
        {
            new DataColumnMap() { ColumnName = "PoNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "PoOrdNumber", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "PoLineNumber", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ItemNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "UoM", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "QtyOrdered", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "QtyCancelled", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "QtyPrevShipped", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "QtyRemaining", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "EstimatedQtyShipped", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "EstimatedQtyNetWeight", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "EstimatedShipmentCost", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "ReceiptLineNumber", Type = typeof(int) },            
            new DataColumnMap() { ColumnName = "IsLineMatched", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "ShipmentEstimateMatchType", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "QtyVariance", Type = typeof(decimal) }            
           
        };

        public static DataColumnMap[] POInvoiceDetails =
        {
            new DataColumnMap() { ColumnName = "ApiInvoiceId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "OriginalApiInvoiceId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "DocumentType", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "DocumentRowID", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "DocumentNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "DocumentDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "VendorNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "DocumentAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "ApprovedAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "FreightAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "SalesTaxAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "TradeDiscountAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "MiscellaneousAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "PurchasingAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "POAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "PONumber", Type = typeof(string) },

            new DataColumnMap() { ColumnName = "PODateOpened", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "POLineNumber", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ReceiptNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ReceiptLineNumber", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ItemNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ItemUnitQty", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "ItemUnitPrice", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "ItemExtendedAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "AdjustedItemUnitQty", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "AdjustedItemUnitPrice", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "ShipToState", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ShippedDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "LocationKey", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "GLIndex", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "TaxScheduleId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "FormTypeCode", Type = typeof(int) },

            new DataColumnMap() { ColumnName = "IsDuplicate", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "RequiredDistribution", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "StatusId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ErrorDescription", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CurrencyId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "UserId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Notes", Type = typeof(string) },
        };

        public static DataColumnMap[] ValidatePOCostMgt =
        {
            new DataColumnMap() { ColumnName = "PoNumber", Type = typeof(string) },
           // new DataColumnMap() { ColumnName = "VendorId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Ord", Type = typeof(int) },
            //new DataColumnMap() { ColumnName = "LineNumber", Type = typeof(int) },
            //new DataColumnMap() { ColumnName = "ItemNmber", Type = typeof(string) },
            //new DataColumnMap() { ColumnName = "UOfM", Type = typeof(string) },
            //new DataColumnMap() { ColumnName = "UnitCost", Type = typeof(decimal) },
            //new DataColumnMap() { ColumnName = "CostStatus", Type = typeof(int) },
            //new DataColumnMap() { ColumnName = "NoteIndex", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "UserId", Type = typeof(string) },
            //new DataColumnMap() { ColumnName = "PoReasonCode", Type = typeof(string) },

        };

        public static DataColumnMap[] SavePoCostMgtChangestoAudit =
        {
            new DataColumnMap() { ColumnName = "PoNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Ord", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "LineNumber", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ItemNmber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "UnitCost", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "UserId", Type = typeof(string) },
        };

        public static DataColumnMap[] UpdateHasCostVariance =
        {
            new DataColumnMap() { ColumnName = "PoNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Ord", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ItemNmber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "HasCostVariance", Type = typeof(int) },
        };

        public static DataColumnMap[] SavePoUnitCostDetails =
        {
            new DataColumnMap() { ColumnName = "PoNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "VendorId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "PoType", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "PoLineStatus", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Ord", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "LineNumber", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ItemNmber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "UOfM", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "UnitCost", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "ProposedUnitCost", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "CostStatus", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "CostSupportId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CostBookCost", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CostNotes", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "LineCostNotes", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "NoteIndex", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "CurrencyIndex", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ItemDescription", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CostSupportCost", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "POVariance", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "PoReasonCode", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "PoLineCostSource", Type = typeof(string) },            
        };
    }
}

