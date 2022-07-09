using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace ChemPoint.GP.HoldEngineDL.Utils
{
    public class DataTableMapper
    {
        /// <summary>
        /// SOP Customer Detail transaction details
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable GetCustomerDataTable(List<CustomerInformation> customerlist, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            DataRow dataRow = null;
            foreach (var customer in customerlist)
            {
                dataRow = dt.NewRow();
                dataRow["CustNmbr"] = customer.CustomerId;
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


            else
                return value;
        }
    }
}

