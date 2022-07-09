using System;
using System.Linq;

namespace ChemPoint.GP.HoldEngineDL.Utils
{
    public static class DataColumnMappings
    {
        public static readonly DataColumnMap[] GetCustomerList = 
        {
            new DataColumnMap() { ColumnName = "CustNmbr", Type = typeof(string) }
        };
    }
}

