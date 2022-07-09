using System;
using System.Linq;

namespace ChemPoint.GP.PMDL.Utils
{
    public static class DataColumnMappings
    {
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

            new DataColumnMap() { ColumnName = "IsDuplicate", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "RequiredDistribution", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "StatusId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ErrorDescription", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CurrencyId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "UserId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Notes", Type = typeof(string) },
        };

        //CTSI
        public static DataColumnMap[] CTSIInvoiceDetailsNA =
        {
            new DataColumnMap() { ColumnName = "OriginialCTSIInvoiceId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "CTSIFileId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "CTSIId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "DocumentType", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "DocumentNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "DocumentDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "VendorId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "TotalApprovedDocumentAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "ApprovedAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "OverCharge", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "FreightAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "TaxAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "MiscellaneousAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "TradeDiscounts", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "PurchaseAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "CurrencyCode", Type = typeof(string) },

            new DataColumnMap() { ColumnName = "GlAccount", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "GLAccountDescription", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CptReference", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "AirwayInvoiceNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Notes", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "OtherDuplicates", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CurrencyDecimalPlaces", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "DebitDistributionType", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "CreditAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "CreditAccountNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "StatusId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ValidDocumentNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CtsiStatus", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ErrorDescription", Type = typeof(string) },
           
        };

        public static DataColumnMap[] CTSIInvoiceDetailsEU =
        {
            new DataColumnMap() { ColumnName = "OriginialCTSIInvoiceId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "CTSIFileId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "CTSIId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "DocumentType", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "DocumentNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "DocumentDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "VendorId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "TotalApprovedDocumentAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "ApprovedAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "OverCharge", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "FreightAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "TaxAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "MiscellaneousAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "TradeDiscounts", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "PurchaseAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "BaseLocalCharge", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "BaseZeroRatedCharge", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "BaseReverseCharge", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "BaseChargeType", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CurrencyCode", Type = typeof(string) },

            new DataColumnMap() { ColumnName = "GlAccount", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "GLAccountDescription", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CptReference", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "AirwayInvoiceNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Notes", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "OtherDuplicates", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CurrencyDecimalPlaces", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "DebitDistributionType", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "CreditAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "CreditAccountNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "StatusId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ValidDocumentNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "TaxScheduleId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CtsiStatus", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ErrorDescription", Type = typeof(string) },

        };

        public static DataColumnMap[] CTSIInvoiceDetailsTaxEU =
        {
            new DataColumnMap() { ColumnName = "BaseCharge", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "TaxScheduleId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "TaxDetailId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "TaxPercentage", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "TaxDetailIdDescription", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Currency", Type = typeof(string) },
        };
    }
}

