using Chempoint.GP.Model.Interactions.Sales;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace ChemPoint.GP.OrderPostingDL.Util
{
    public class DataTableMapper
    {
        public static DataTable LockedUserDataTable(OrderBatchPostRequest salesOrderRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            foreach (User usr in salesOrderRequest.LockedUsers)
            {
                var dataRow = dt.NewRow();
                dataRow["UserID"] = usr.UserId;
                dataRow["UserName"] = usr.UserName;
                dataRow["Remove"] = 0;

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
