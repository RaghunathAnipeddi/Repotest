using System;
using System.Collections.Generic;
using System.Data;
using Chempoint.GP.Model.Interactions.Setup;
using System.Reflection;

namespace ChemPoint.GP.SetupDetailDL.Util
{
    public class DataTableMapper
    {
        public static DataTable SavePaymentTermsDataTable(SetupRequest paymentRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            var dataRow = dt.NewRow();
            dataRow["PaymentTermsID"] = paymentRequest.SetupEntity.PaymentTermsDetails.PaymentTermsID.ToString();
            dataRow["DueOfMonths"] = paymentRequest.SetupEntity.PaymentTermsDetails.DueOfMonths.ToString();
            dataRow["EOMEnabled"] = paymentRequest.SetupEntity.PaymentTermsDetails.EomEnabled;
            dataRow["OrderPrePaymentPct"] = paymentRequest.SetupEntity.PaymentTermsDetails.OrderPrePaymentPct.ToString();
            dataRow["TermsGracePeriod"] = paymentRequest.SetupEntity.PaymentTermsDetails.TermsGracePeriod.ToString();
            dataRow["Nested"] = paymentRequest.SetupEntity.PaymentTermsDetails.Nested.ToString();
            dataRow["DateWithin"] = paymentRequest.SetupEntity.PaymentTermsDetails.DateWithin;
            dataRow["TermsIfYes"] = paymentRequest.SetupEntity.PaymentTermsDetails.TermsIfYes.ToString();
            dataRow["TermsIfNo"] = paymentRequest.SetupEntity.PaymentTermsDetails.TermsIfNo.ToString();
            dataRow["CreatedBy"] = paymentRequest.SetupEntity.AuditInformation.CreatedBy;
            dataRow["CreatedDate"] = paymentRequest.SetupEntity.AuditInformation.CreatedDate;
            dataRow["ModifiedBy"] = paymentRequest.SetupEntity.AuditInformation.ModifiedBy;
            dataRow["ModifiedDate"] = paymentRequest.SetupEntity.AuditInformation.ModifiedDate;
            
            dt.Rows.Add(dataRow);
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
