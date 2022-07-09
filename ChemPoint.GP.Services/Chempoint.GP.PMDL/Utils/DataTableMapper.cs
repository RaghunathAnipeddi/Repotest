using Chempoint.GP.Model.Interactions.Purchases;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Chempoint.GP.Model.Interactions.PayableManagement;

namespace ChemPoint.GP.PMDL.Utils
{
    public class DataTableMapper
    {
        public static DataTable POInvoiceDetails(PayableManagementRequest request, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            DataRow dataRow = null;
            foreach (var POInvoice in request.PoValidationList)
            {
                dataRow = dt.NewRow();
                dataRow["ApiInvoiceId"] = POInvoice.APIInvoiceId;
                dataRow["OriginalApiInvoiceId"] = POInvoice.OriginalApiInvoiceId;
                dataRow["DocumentType"] = POInvoice.DocumentType;
                dataRow["DocumentRowID"] = POInvoice.DocumentRowId;
                dataRow["DocumentNumber"] = POInvoice.DocumentNumber;
                dataRow["DocumentDate"] = POInvoice.DocumentDate;
                dataRow["VendorNumber"] = POInvoice.VendorName;
                dataRow["DocumentAmount"] = POInvoice.DocumentAmount;
                dataRow["ApprovedAmount"] = POInvoice.ApprovedAmount;
                dataRow["FreightAmount"] = POInvoice.FreightAmount;
                dataRow["SalesTaxAmount"] = POInvoice.TaxAmount;
                dataRow["TradeDiscountAmount"] = POInvoice.TradeDiscounts;
                dataRow["MiscellaneousAmount"] = POInvoice.MiscellaneousAmount;
                dataRow["PurchasingAmount"] = POInvoice.PurchaseAmount;
                dataRow["POAmount"] = POInvoice.POAmount;
                dataRow["PONumber"] = POInvoice.PurchaseOrderNumber;
                dataRow["PODateOpened"] = POInvoice.PODateOpened;
                dataRow["POLineNumber"] = POInvoice.POLineNumber;
                dataRow["ReceiptNumber"] = POInvoice.ReceiptNumber;
                dataRow["ReceiptLineNumber"] = POInvoice.ReceiptLineNumber;
                dataRow["ItemNumber"] = POInvoice.ItemNumber;
                dataRow["ItemUnitQty"] = POInvoice.ItemUnitQty;
                dataRow["ItemUnitPrice"] = POInvoice.ItemUnitPrice;
                dataRow["ItemExtendedAmount"] = POInvoice.ExtendedCost;
                dataRow["AdjustedItemUnitQty"] = POInvoice.AdjustedItemUnitQuantity;
                dataRow["AdjustedItemUnitPrice"] = POInvoice.AdjustedItemUnitPrice;
                dataRow["ShipToState"] = POInvoice.ShipToState;
                dataRow["ShippedDate"] = POInvoice.ShippedDate;
                dataRow["LocationKey"] = POInvoice.LocationName;
                dataRow["GLIndex"] = POInvoice.GLIndex;
                dataRow["TaxScheduleId"] = POInvoice.TaxScheduleId;
                dataRow["FormTypeCode"] = POInvoice.FormTypeCode;
                dataRow["IsDuplicate"] = Convert.ToBoolean(POInvoice.IsDuplicated);
                dataRow["RequiredDistribution"] = Convert.ToBoolean(POInvoice.RequiredDistribution);
                dataRow["StatusId"] = POInvoice.StatusId;
                dataRow["ErrorDescription"] = POInvoice.ErrorDescription;
                dataRow["CurrencyId"] = POInvoice.CurrencyId;
                dataRow["UserId"] = POInvoice.UserId;
                dataRow["Notes"] = POInvoice.Notes;
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        public static DataTable DistributedDetails(PayableManagementRequest request, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            DataRow dataRow = null;
            foreach (var POInvoice in request.DuplicationValidationList)
            {
                dataRow = dt.NewRow();
                dataRow["ApiInvoiceId"] = POInvoice.APIInvoiceId;
                dataRow["OriginalApiInvoiceId"] = POInvoice.OriginalApiInvoiceId;
                dataRow["DocumentType"] = POInvoice.DocumentType;
                dataRow["DocumentRowID"] = POInvoice.DocumentRowId;
                dataRow["DocumentNumber"] = POInvoice.DocumentNumber;
                dataRow["DocumentDate"] = POInvoice.DocumentDate;
                dataRow["VendorNumber"] = POInvoice.VendorName;
                dataRow["DocumentAmount"] = POInvoice.DocumentAmount;
                dataRow["ApprovedAmount"] = POInvoice.ApprovedAmount;
                dataRow["FreightAmount"] = POInvoice.FreightAmount;
                dataRow["SalesTaxAmount"] = POInvoice.TaxAmount;
                dataRow["TradeDiscountAmount"] = POInvoice.TradeDiscounts;
                dataRow["MiscellaneousAmount"] = POInvoice.MiscellaneousAmount;
                dataRow["PurchasingAmount"] = POInvoice.PurchaseAmount;
                dataRow["POAmount"] = POInvoice.POAmount;
                dataRow["PONumber"] = POInvoice.PurchaseOrderNumber;
                dataRow["PODateOpened"] = POInvoice.PODateOpened;
                dataRow["POLineNumber"] = POInvoice.POLineNumber;
                dataRow["ReceiptNumber"] = POInvoice.ReceiptNumber;
                dataRow["ReceiptLineNumber"] = POInvoice.ReceiptLineNumber;
                dataRow["ItemNumber"] = POInvoice.ItemNumber;
                dataRow["ItemUnitQty"] = POInvoice.ItemUnitQty;
                dataRow["ItemUnitPrice"] = POInvoice.ItemUnitPrice;
                dataRow["ItemExtendedAmount"] = POInvoice.ExtendedCost;
                dataRow["AdjustedItemUnitQty"] = POInvoice.AdjustedItemUnitQuantity;
                dataRow["AdjustedItemUnitPrice"] = POInvoice.AdjustedItemUnitPrice;
                dataRow["ShipToState"] = POInvoice.ShipToState;
                dataRow["ShippedDate"] = POInvoice.ShippedDate;
                dataRow["LocationKey"] = POInvoice.LocationName;
                dataRow["GLIndex"] = POInvoice.GLIndex;
                dataRow["TaxScheduleId"] = POInvoice.TaxScheduleId;
                dataRow["FormTypeCode"] = POInvoice.FormTypeCode;
                dataRow["IsDuplicate"] = Convert.ToBoolean(POInvoice.IsDuplicated);
                dataRow["RequiredDistribution"] = Convert.ToBoolean(POInvoice.RequiredDistribution);
                dataRow["StatusId"] = POInvoice.StatusId;
                dataRow["ErrorDescription"] = POInvoice.ErrorDescription;
                dataRow["CurrencyId"] = POInvoice.CurrencyId;
                dataRow["UserId"] = POInvoice.UserId;
                dataRow["Notes"] = POInvoice.Notes;
                dt.Rows.Add(dataRow);
            }
            return dt;
        }


        public static DataTable CTSIInvoiceDetailsNA(PayableManagementRequest request, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            DataRow dataRow = null;
            foreach (var POInvoice in request.CTSIValidationList)
            {
                dataRow = dt.NewRow();
                dataRow["OriginialCTSIInvoiceId"] = POInvoice.OriginalCTSIInvoiceId;
                dataRow["CTSIId"] = POInvoice.CtsiId;
                dataRow["CTSIFileId"] = POInvoice.CtsiFileId;
                dataRow["DocumentType"] = POInvoice.DocumentType;
                dataRow["DocumentNumber"] = POInvoice.DocumentNumber;
                dataRow["DocumentDate"] = POInvoice.DocumentDate;
                dataRow["VendorId"] = POInvoice.VendorName;
                dataRow["TotalApprovedDocumentAmount"] = POInvoice.TotalApprovedDocumentAmount;
                dataRow["ApprovedAmount"] = POInvoice.ApprovedAmount;
                dataRow["OverCharge"] = POInvoice.OverCharge;
                dataRow["FreightAmount"] = POInvoice.FreightAmount;
                dataRow["TaxAmount"] = POInvoice.TaxAmount;
                dataRow["MiscellaneousAmount"] = POInvoice.MiscellaneousAmount;
                dataRow["TradeDiscounts"] = POInvoice.TradeDiscounts;
                dataRow["PurchaseAmount"] = POInvoice.PurchaseAmount;
                dataRow["CurrencyCode"] = POInvoice.CurrencyCode;

                dataRow["GlAccount"] = POInvoice.GLAccount;
                dataRow["GLAccountDescription"] = POInvoice.GLAccountDescription;
                dataRow["CptReference"] = POInvoice.CPTReference;
                dataRow["AirwayInvoiceNumber"] = POInvoice.AirwayInvoiceNumber;
                dataRow["Notes"] = POInvoice.Notes;
                dataRow["OtherDuplicates"] = POInvoice.OtherDuplicates;
                dataRow["CurrencyDecimalPlaces"] = POInvoice.CurrencyDecimal;
                dataRow["DebitDistributionType"] = POInvoice.DebitDistributionType;
                dataRow["CreditAmount"] = POInvoice.CreditAmount;
                dataRow["CreditAccountNumber"] = POInvoice.CreditAccountNumber;
                dataRow["StatusId"] = POInvoice.StatusId;
                dataRow["ValidDocumentNumber"] = POInvoice.ValidDocumentNumber;
                dataRow["CtsiStatus"] = POInvoice.ShipToState;
                dataRow["ErrorDescription"] = POInvoice.ErrorDescription;
                dt.Rows.Add(dataRow);
            }
            return dt;
        }


        public static DataTable CTSIInvoiceDetailsEU(PayableManagementRequest request, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            DataRow dataRow = null;
            foreach (var POInvoice in request.CTSIValidationList)
            {
                dataRow = dt.NewRow();
                dataRow["OriginialCTSIInvoiceId"] = POInvoice.OriginalCTSIInvoiceId;
                dataRow["CTSIId"] = POInvoice.CtsiId;
                dataRow["CTSIFileId"] = POInvoice.CtsiFileId;
                dataRow["DocumentType"] = POInvoice.DocumentType;
                dataRow["DocumentNumber"] = POInvoice.DocumentNumber;
                dataRow["DocumentDate"] = POInvoice.DocumentDate;
                dataRow["VendorId"] = POInvoice.VendorName;
                dataRow["TotalApprovedDocumentAmount"] = POInvoice.TotalApprovedDocumentAmount;
                dataRow["ApprovedAmount"] = POInvoice.ApprovedAmount;
                dataRow["OverCharge"] = POInvoice.OverCharge;
                dataRow["FreightAmount"] = POInvoice.FreightAmount;
                dataRow["TaxAmount"] = POInvoice.TaxAmount;
                dataRow["MiscellaneousAmount"] = POInvoice.MiscellaneousAmount;
                dataRow["TradeDiscounts"] = POInvoice.TradeDiscounts;
                dataRow["PurchaseAmount"] = POInvoice.PurchaseAmount;
                dataRow["BaseLocalCharge"] = POInvoice.BaseLocalCharge;
                dataRow["BaseZeroRatedCharge"] = POInvoice.BaseZeroRatedCharge;
                dataRow["BaseReverseCharge"] = POInvoice.BaseReverseCharge;
                dataRow["BaseChargeType"] = POInvoice.BaseChargeType;

                dataRow["CurrencyCode"] = POInvoice.CurrencyCode;

                dataRow["GlAccount"] = POInvoice.GLAccount;
                dataRow["GLAccountDescription"] = POInvoice.GLAccountDescription;
                dataRow["CptReference"] = POInvoice.CPTReference;
                dataRow["AirwayInvoiceNumber"] = POInvoice.AirwayInvoiceNumber;
                dataRow["Notes"] = POInvoice.Notes;
                dataRow["OtherDuplicates"] = POInvoice.OtherDuplicates;
                dataRow["CurrencyDecimalPlaces"] = POInvoice.CurrencyDecimal;
                dataRow["DebitDistributionType"] = POInvoice.DebitDistributionType;
                dataRow["CreditAmount"] = POInvoice.CreditAmount;
                dataRow["CreditAccountNumber"] = POInvoice.CreditAccountNumber;
                dataRow["StatusId"] = POInvoice.StatusId;

                dataRow["ValidDocumentNumber"] = POInvoice.ValidDocumentNumber;
                dataRow["TaxScheduleId"] = POInvoice.TaxScheduleId;
                dataRow["CtsiStatus"] = POInvoice.ShipToState;
                dataRow["ErrorDescription"] = POInvoice.ErrorDescription;
                dt.Rows.Add(dataRow);
            }
            return dt;
        }


        public static DataTable CTSIInvoiceDetailsTaxEU(PayableManagementRequest request, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            DataRow dataRow = null;
            foreach (var POInvoice in request.CTSITaxValidationList)
            {
                dataRow = dt.NewRow();
                dataRow["BaseCharge"] = POInvoice.BaseCharge;
                dataRow["TaxScheduleId"] = POInvoice.TaxScheduleId;
                dataRow["TaxDetailId"] = POInvoice.TaxDetailId;
                dataRow["TaxPercentage"] = POInvoice.TaxPercentage;
                dataRow["TaxDetailIdDescription"] = POInvoice.TaxDetailIdDescription;
                dataRow["Currency"] = POInvoice.CurrencyCode;
                dt.Rows.Add(dataRow);
            }
            return dt;
        }
        public static DataTable GetDataTable<T>(List<T> lst, DataColumnMap[] columns)
        {
            var dt = GetDataTable(columns);
            FillData<T>(lst, dt, columns);
            return dt;
        }

        public static DataTable GetDataTable<T>(T t, DataColumnMap[] columns)
        {
            var dt = GetDataTable(columns);
            FillData<T>(t, dt, columns);
            return dt;
        }

        private static DataTable GetDataTable(DataColumnMap[] columns)
        {
            var dataTable = new DataTable();

            foreach (var column in columns)
            {
                var dataColumn = new DataColumn();
                dataColumn.ColumnName = column.ColumnName;
                dataColumn.DataType = column.Type;

                if (column.IsDefaultValue)
                    dataColumn.DefaultValue = column.DefaultValue;

                dataTable.Columns.Add(dataColumn);
            }
            return dataTable;
        }

        private static void FillData<T>(List<T> lstData, DataTable dataTable, DataColumnMap[] columns)
        {
            foreach (var data in lstData)
            {
                FillData<T>(data, dataTable, columns);
            }
        }

        private static void FillData<T>(T data, DataTable dataTable, DataColumnMap[] columns)
        {
            var dataRow = dataTable.NewRow();

            foreach (var column in columns)
                dataRow[column.ColumnName] = GetPropValue(data, column.ColumnName);

            dataTable.Rows.Add(dataRow);
        }

        private static object GetPropValue(object src, string propName)
        {
            object value = src.GetType().GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(src, null);

            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return DBNull.Value;


            else if (value.GetType() == typeof(DateTime) && ((DateTime)value) == DateTime.MinValue)
                return DBNull.Value;

            else
                return value;
        }

    }
}
