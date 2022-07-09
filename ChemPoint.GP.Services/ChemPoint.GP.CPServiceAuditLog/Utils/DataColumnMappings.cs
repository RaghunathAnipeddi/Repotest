using System;
using System.Linq;

namespace ChemPoint.GP.CPServiceAuditLog.Utils
{
    public static class DataColumnMappings
    {
        public static readonly DataColumnMap[] SaveAuditLog = 
        {
            new DataColumnMap() { ColumnName = "Source", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Operation", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "SourceDocumentId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "HeaderGUID", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "LineGUID", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "HeaderStatus", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "LineStatus", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Company", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "Notes", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CreatedOn", Type = typeof(DateTime) },
        };
    }
}

