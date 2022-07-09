using System;
using System.Linq;

namespace ChemPoint.GP.SetupDetailDL.Util
{
    class DataColumnMappings
    {
        public static readonly DataColumnMap[] SavePaymentTermsDetails = 
        {
            new DataColumnMap() { ColumnName = "PaymentTermsID", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "DueOfMonths", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "EOMEnabled", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "OrderPrePaymentPct", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "TermsGracePeriod", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "Nested", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "DateWithin", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "TermsIfYes", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "TermsIfNo", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CreatedBy", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CreatedDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "ModifiedBy", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ModifiedDate", Type = typeof(DateTime) }
        };
    }
}
