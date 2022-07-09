using System;
using System.Linq;

namespace ChemPoint.GP.OrderPostingDL.Util
{
    public static class DataColumnMappings
    {
        public static readonly DataColumnMap[] LockedUserColumns = 
        {
            new DataColumnMap() { ColumnName = "UserID", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "UserName", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Remove", Type = typeof(int) }
        };
    }
}
