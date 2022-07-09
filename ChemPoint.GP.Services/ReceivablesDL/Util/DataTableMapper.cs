using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReceivablesDL.Util
{
    public class DataTableMapper
    {

        /// <summary>
        /// To import EFT summary 
        /// </summary>
        /// <param name="receivablesRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable ImportOriginalEFTSummaryReport(ReceivablesRequest receivablesRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);

            foreach (var lineItem in receivablesRequest.EFTOriginalList)
            {
                var dataRow = dt.NewRow();

                dataRow["AsOf"] = lineItem.AsOf;
                dataRow["Currency"] = lineItem.Currency;
                dataRow["BankIDType"] = lineItem.BankIDType;
                dataRow["BankID"] = lineItem.BankID;
                dataRow["Account"] = lineItem.Account;
                dataRow["DataType"] = lineItem.DataType;
                dataRow["BAICode"] = lineItem.BAICode;
                dataRow["Description"] = lineItem.Description;
                dataRow["Amount"] = lineItem.Amount;
                dataRow["BalanceOrValueDate"] = lineItem.BalanceOrValueDate;
                dataRow["CustomerReference"] = lineItem.CustomerReference;
                dataRow["ImmediateAvailability"] = lineItem.ImmediateAvailability;
                dataRow["OneDayFloat"] = lineItem.OneDayFloat;
                dataRow["TwoPlusDayFloat"] = lineItem.TwoPlusDayFloat;
                dataRow["BankReference"] = lineItem.BankReference;
                dataRow["NoOfItems"] = lineItem.NoOfItems;
                dataRow["Text"] = lineItem.Text;
                dataRow["CTXId"] = lineItem.CTXId;
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        /// <summary>
        /// Filetered EFT 
        /// </summary>
        /// <param name="receivablesRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable ImportFilteredEFTSummaryReport(ReceivablesRequest receivablesRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);

            foreach (var lineItem in receivablesRequest.EFTFilteredPaymentList)
            {
                var dataRow = dt.NewRow();

                dataRow["PaymentNumber"] = lineItem.PaymentNumber;
                dataRow["ReferenceNumber"] = lineItem.ReferenceNumber;
                dataRow["DateReceived"] = lineItem.DateReceived;
                dataRow["PaymentAmount"] = lineItem.PaymentAmount;
                dataRow["ItemAmount"] = lineItem.ItemAmount;
                dataRow["CustomerID"] = lineItem.CustomerID;
                dataRow["Source"] = lineItem.Source;
                dataRow["BankOriginatingID"] = lineItem.BankOriginatingID;
                dataRow["CTXId"] = lineItem.CTXId;
                dataRow["AccountNo"] = lineItem.AccountNo;
                dataRow["CurrencyID"] = lineItem.CurrencyID;
                dataRow["ItemReference"] = lineItem.ItemReference;
                dataRow["Notes"] = lineItem.Notes;
                dataRow["EftStatusId"] = lineItem.EftStatusId;

                dt.Rows.Add(dataRow);

            }

            return dt;
        }


        /// <summary>
        /// Filetered EFT 
        /// </summary>
        /// <param name="receivablesRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable PushToGPPayments(ReceivablesRequest receivablesRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);

            foreach (var lineItem in receivablesRequest.EFTCustomerPaymentList)
            {
                var dataRow = dt.NewRow();

                dataRow["EftId"] = lineItem.EftId;
                dataRow["EftAppId"] = lineItem.EftAppId;
                dataRow["PaymentNumber"] = lineItem.PaymentNumber;
                dataRow["ReferenceNumber"] = lineItem.ReferenceNumber;
                dataRow["DateReceived"] = lineItem.DateReceived;
                dataRow["PaymentAmount"] = lineItem.PaymentAmount;
                dataRow["CustomerID"] = lineItem.CustomerID;
                dataRow["CurrencyId"] = lineItem.CurrencyId;
                dataRow["IsFullyApplied"] = lineItem.IsFullyApplied;
                dataRow["Source"] = lineItem.Source;
                dataRow["BankOriginating"] = lineItem.BankOriginating;
                dataRow["ItemReferenceNumber"] = lineItem.ItemReferenceNumber;
                dataRow["ItemAmount"] = lineItem.ItemAmount;
                dataRow["CreatedBy"] = receivablesRequest.AuditInformation.CreatedBy;
                dataRow["IsUpdated"] = lineItem.IsUpdated;
                dataRow["Status"] = lineItem.Status;
                dataRow["StatusReason"] = lineItem.Status;
                dataRow["AccountName"] = lineItem.AccountName;
                dt.Rows.Add(dataRow);
            }

            return dt;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="receivablesRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable EFTMailReport(ReceivablesRequest receivablesRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);

            foreach (var lineItem in receivablesRequest.EFTMailList)
            {
                var dataRow = dt.NewRow();

                dataRow["AsOf"] = lineItem.AsOf;

                dataRow["Account"] = lineItem.Account;

                dataRow["Amount"] = lineItem.Amount;

                dataRow["Text"] = lineItem.Text;

                dt.Rows.Add(dataRow);
            }
            return dt;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="receivablesRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable EFTTransaction(ReceivablesRequest receivablesRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);

            foreach (var lineItem in receivablesRequest.EFTTransactionList)
            {
                var dataRow = dt.NewRow();

                dataRow["NAEFTTransactionId"] = lineItem.NAEFTTransactionId;
                dataRow["NACTXSummaryDetailId"] = lineItem.NACTXSummaryDetailId;
                dataRow["GPPaymentNumber"] = lineItem.PaymentNumber;
                dataRow["ReferenceNumber"] = lineItem.ReferenceNumber;
                dataRow["DateReceived"] = lineItem.DateReceived;
                dataRow["PaymentAmount"] = lineItem.PaymentAmount;
                dataRow["CustomerID"] = lineItem.CustomerID;
                dataRow["CurrencyID"] = lineItem.CurrencyID;
                dataRow["EftStatusId"] = lineItem.EftStatusId;
                dataRow["ISFullyApplied"] = lineItem.IsFullyApplied;
                dataRow["Source"] = lineItem.Source;
                dataRow["BankOriginating"] = lineItem.BankOriginatingID;
                dataRow["Notes"] = lineItem.Notes;
                dataRow["TraceNumber"] = lineItem.TraceNumber;
                dataRow["ACHTraceNumber"] = lineItem.ACHTraceNumber;
                dataRow["AccountName"] = lineItem.AccountName;
                dataRow["BankAccountNumber"] = lineItem.BankAccountNumber;
                dataRow["BankRoutingNumber"] = lineItem.BankRoutingNumber;
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        /// <summary>
        /// EFTTransactionMap
        /// </summary>
        /// <param name="receivablesRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable EFTTransactionMap(ReceivablesRequest receivablesRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);

            foreach (var lineItem in receivablesRequest.EFTTransactionMapList)
            {
                var dataRow = dt.NewRow();

                dataRow["NAEFTTransactionApplyId"] = lineItem.NAEFTTransactionApplyId;
                dataRow["NAEFTTransactionId"] = lineItem.NAEFTTransactionId;
                dataRow["ItemReferenceNumber"] = lineItem.ItemReferenceNumber;
                dataRow["ItemAmount"] = lineItem.ItemAmount;

                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="receivablesRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable EFTEmailLineDetails(ReceivablesRequest receivablesRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);

            foreach (var eftEmailLine in receivablesRequest.EFTPaymentList)
            {
                var dataRow = dt.NewRow();

                dataRow["EftId"] = eftEmailLine.EftId;
                dataRow["EftRowId"] = eftEmailLine.EFTRowId;
                dataRow["EftApplyId"] = eftEmailLine.EftAppId;
                dataRow["ItemReferenceNumber"] = eftEmailLine.ItemReferenceNumber;
                dataRow["ItemAmount"] = eftEmailLine.ItemAmount;
                dataRow["CreatedBy"] = receivablesRequest.AuditInformation.CreatedBy.ToString().Trim();

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

            if (value.GetType() == typeof(string) && string.IsNullOrEmpty(value.ToString()))
                return "";

            else if (value == null || string.IsNullOrEmpty(value.ToString()))
                return DBNull.Value;

            else if (value.GetType() == typeof(DateTime) && ((DateTime)value) == DateTime.MinValue)
                return DBNull.Value;

            //else if (value.GetType().BaseType == typeof(TriggerSource) && propName.ToUpper() == "TRIGGERSOURCE")
            //    return ((TriggerSource)value).Name.ToString();

            //else if (value.GetType().BaseType == typeof(TriggerSource))
            //    return ((TriggerSource)value).EmployeeStatus.ToString();

            else
                return value;
        }
    }
}