using Chempoint.GP.Model.Interactions.Purchases;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Chempoint.GP.Model.Interactions.PayableManagement;

namespace ChemPoint.GP.Podl.Utils
{
    public class DataTableMapper
    {
        /// <summary>
        /// LogActivity..
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable SaveXRMActivityLOG(PurchaseOrderRequest poActivityrRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            DataRow dataRow = null;
            foreach (var xrmIntegration in poActivityrRequest.POActivityLogList)
            {
                dataRow = dt.NewRow();
                dataRow["PONumber"] = xrmIntegration.PONumber.ToString().Trim();
                dataRow["POActivityTypeMasterID"] = xrmIntegration.POActivityTypeMasterID;
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        public static DataTable SaveEstimatedShipmentCostEntry(PurchaseOrderRequest purchaseOrderRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            DataRow dataRow = null;
            foreach (var ShipmentEstimate in purchaseOrderRequest.PurchaseShipmentEstimateList)
            {
                dataRow = dt.NewRow();
                dataRow["PoNumber"] = ShipmentEstimate.PoNumber.ToString().Trim();
                dataRow["PoOrdNumber"] = ShipmentEstimate.PoOrdNumber;
                dataRow["PoLineNumber"] = ShipmentEstimate.PoLineNumber;
                dataRow["ItemNumber"] = ShipmentEstimate.ItemNumber.ToString().Trim();
                dataRow["UoM"] = ShipmentEstimate.UoM.ToString().Trim();
                dataRow["QtyOrdered"] = ShipmentEstimate.QtyOrdered;
                dataRow["QtyCancelled"] = ShipmentEstimate.QtyCancelled;
                dataRow["QtyPrevShipped"] = ShipmentEstimate.QtyPrevShipped;
                dataRow["QtyRemaining"] = ShipmentEstimate.QtyRemaining;
                dataRow["EstimatedQtyShipped"] = ShipmentEstimate.EstimatedQtyShipped;
                dataRow["EstimatedQtyNetWeight"] = ShipmentEstimate.EstimatedQtyNetWeight;
                dataRow["EstimatedShipmentCost"] = ShipmentEstimate.EstimatedShipmentCost;
                dataRow["ReceiptLineNumber"] = ShipmentEstimate.ReceiptLineNumber;                              
                dataRow["IsLineMatched"] = ShipmentEstimate.IsLineMatched;
                dataRow["ShipmentEstimateMatchType"] = ShipmentEstimate.ShipmentEstimateMatchType;
                dataRow["QtyVariance"] = ShipmentEstimate.QtyVariance;                
                dt.Rows.Add(dataRow);
            }
            return dt;
        }


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
                dataRow["IsDuplicate"] = POInvoice.IsDuplicated;
                dataRow["RequiredDistribution"] = POInvoice.RequiredDistribution;
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
                dataRow["IsDuplicate"] = POInvoice.IsDuplicated;
                dataRow["RequiredDistribution"] = POInvoice.RequiredDistribution;
                dataRow["StatusId"] = POInvoice.StatusId;
                dataRow["ErrorDescription"] = POInvoice.ErrorDescription;
                dataRow["CurrencyId"] = POInvoice.CurrencyId;
                dataRow["UserId"] = POInvoice.UserId;
                dataRow["Notes"] = POInvoice.Notes;
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        public static DataTable ValidatePOCostMgtDetails(PurchaseOrderRequest request, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            DataRow dataRow = null;
            foreach (var POInvoice in request.PurchaseCostMgt)
            {
                dataRow = dt.NewRow();
                dataRow["PoNumber"] = POInvoice.PoNumber;
                //dataRow["VendorId"] = POInvoice.VendorId;
                dataRow["Ord"] = POInvoice.Ord;
                //dataRow["LineNumber"] = POInvoice.LineNumber;
                //dataRow["ItemNmber"] = POInvoice.ItemNumber;
                //dataRow["UOfM"] = POInvoice.UOfM;
                //dataRow["UnitCost"] = POInvoice.UnitCost;
                //dataRow["CostStatus"] = POInvoice.CostStatus;
                //dataRow["NoteIndex"] = POInvoice.NoteIndex;
                dataRow["UserId"] = POInvoice.UserId;
                //dataRow["PoReasonCode"] = POInvoice.Reason;
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        public static DataTable SavePoCostMgtChangestoAudit(PurchaseOrderRequest request, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            DataRow dataRow = null;
            foreach (var POInvoice in request.PurchaseCostMgt)
            {
                dataRow = dt.NewRow();
                dataRow["PoNumber"] = POInvoice.PoNumber;                
                dataRow["Ord"] = POInvoice.Ord;
                dataRow["LineNumber"] = POInvoice.LineNumber;
                dataRow["ItemNmber"] = POInvoice.ItemNumber;
                dataRow["UnitCost"] = POInvoice.UnitCost;                
                dataRow["UserId"] = POInvoice.UserId;
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        public static DataTable UpdateHasCostVariance(PurchaseOrderRequest request, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            DataRow dataRow = null;
            foreach (var POInvoice in request.PurchaseCostMgt)
            {
                dataRow = dt.NewRow();
                dataRow["PoNumber"] = POInvoice.PoNumber;
                dataRow["Ord"] = POInvoice.Ord;
                dataRow["ItemNmber"] = POInvoice.ItemNumber;
                dataRow["HasCostVariance"] = POInvoice.HasCostVariance;
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        public static DataTable SavePoUnitCostDetails(PurchaseOrderRequest request, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            DataRow dataRow = null;
            foreach (var POInvoice in request.PurchaseCostMgt)
            {
                dataRow = dt.NewRow();
                dataRow["PoNumber"] = POInvoice.PoNumber;
                dataRow["VendorId"] = POInvoice.VendorId;
                dataRow["PoType"] = POInvoice.PoType;
                dataRow["PoLineStatus"] = POInvoice.PoLineStatus;
                dataRow["Ord"] = POInvoice.Ord;
                dataRow["LineNumber"] = POInvoice.LineNumber;
                dataRow["ItemNmber"] = POInvoice.ItemNumber;
                dataRow["UOfM"] = POInvoice.UOfM;
                dataRow["UnitCost"] = POInvoice.UnitCost;
                dataRow["ProposedUnitCost"] = POInvoice.ProposedUnitCost;
                dataRow["CostStatus"] = POInvoice.CostStatus;
                dataRow["CostSupportId"] = POInvoice.CostSupportId;
                dataRow["CostBookCost"] = POInvoice.CostBookCost;
                dataRow["CostNotes"] = POInvoice.CostNotes;
                dataRow["LineCostNotes"] = POInvoice.LineCostNotes;
                dataRow["NoteIndex"] = POInvoice.NoteIndex;
                dataRow["CurrencyIndex"] = POInvoice.CurrencyIndex;
                dataRow["ItemDescription"] = POInvoice.ItemDescription;
                dataRow["CostSupportCost"] = POInvoice.CostSupportCost;
                dataRow["POVariance"] = POInvoice.PoCostVariance;
                dataRow["PoReasonCode"] = POInvoice.Reason;
                dataRow["PoLineCostSource"] = POInvoice.PoLineCostSource;
                
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

