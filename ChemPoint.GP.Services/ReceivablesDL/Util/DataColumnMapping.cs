using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivablesDL.Util
{
    public class DataColumnMapping
    {

        public static readonly DataColumnMap[] ImportOriginalEFTSummary = 
        {
           
            new DataColumnMap() { ColumnName = "AsOf", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "Currency", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "BankIDType", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "BankID", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Account", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "DataType", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "BAICode", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Description", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Amount", Type = typeof(Decimal),IsDefaultValue = true, DefaultValue = 0.00M },
            new DataColumnMap() { ColumnName = "BalanceOrValueDate", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CustomerReference", Type = typeof(string),IsDefaultValue = true, DefaultValue = 0.00M },
            new DataColumnMap() { ColumnName = "ImmediateAvailability", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "OneDayFloat", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "TwoPlusDayFloat", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "BankReference", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "NoOfItems", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "Text", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CTXId", Type = typeof(int) }

        };

        public static readonly DataColumnMap[] ImportEFTMailSummary = 
        {
           
            new DataColumnMap() { ColumnName = "AsOf", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "Account", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Amount", Type = typeof(Decimal),IsDefaultValue = true, DefaultValue = 0.00M },
            new DataColumnMap() { ColumnName = "Text", Type = typeof(string) }

        };

        public static readonly DataColumnMap[] ImportFilteredEFT = 
        {
           new DataColumnMap() { ColumnName =  "PaymentNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ReferenceNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "DateReceived", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "PaymentAmount", Type = typeof(Decimal),IsDefaultValue = true, DefaultValue = 0.00M  },            
            new DataColumnMap() { ColumnName = "ItemReference", Type = typeof(string) },            
            new DataColumnMap() { ColumnName = "ItemAmount",  Type = typeof(Decimal),IsDefaultValue = true, DefaultValue = 0.00M  },
            new DataColumnMap() { ColumnName = "CustomerID", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Notes", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "EftStatusId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "Source", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "BankOriginatingID", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CTXId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "AccountNo", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CurrencyID", Type = typeof(string) }

      };

        public static readonly DataColumnMap[] PushToGPTable = 
        {
           new DataColumnMap() { ColumnName =  "EftId", Type = typeof(int) },
           new DataColumnMap() { ColumnName = "EftAppId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "PaymentNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ReferenceNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "DateReceived", Type = typeof(DateTime)  },            
            new DataColumnMap() { ColumnName = "PaymentAmount", Type = typeof(Decimal),IsDefaultValue = true, DefaultValue = 0.00M  },            
            new DataColumnMap() { ColumnName = "CustomerID",  Type = typeof(string)  },
            new DataColumnMap() { ColumnName = "CurrencyId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "IsFullyApplied", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "Source", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "BankOriginating", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ItemReferenceNumber", Type = typeof(string)},
            new DataColumnMap() { ColumnName = "ItemAmount", Type = typeof(Decimal),IsDefaultValue = true, DefaultValue = 0.00M },
            new DataColumnMap() { ColumnName = "CreatedBy",  Type = typeof(string)  },
            new DataColumnMap() { ColumnName = "IsUpdated", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "Status", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "StatusReason", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "AccountName", Type = typeof(string) }
      };
  

        public static readonly DataColumnMap[] EFTTransactionDetails = 
        {
           
            new DataColumnMap() { ColumnName = "NAEFTTransactionId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "NACTXSummaryDetailId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "GPPaymentNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ReferenceNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "DateReceived", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "PaymentAmount", Type = typeof(Decimal),IsDefaultValue = true, DefaultValue = 0.00M },
            new DataColumnMap() { ColumnName = "CustomerID", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CurrencyID", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "EftStatusId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ISFullyApplied", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "Source", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "BankOriginating", Type = typeof(string)},
            new DataColumnMap() { ColumnName = "Notes", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "TraceNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ACHTraceNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "AccountName", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "BankAccountNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "BankRoutingNumber", Type = typeof(string) },


        };

        public static readonly DataColumnMap[] EFTTransactionMapDetails = 
        {
           
            new DataColumnMap() { ColumnName = "NAEFTTransactionApplyId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "NAEFTTransactionId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ItemReferenceNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ItemAmount", Type = typeof(Decimal),IsDefaultValue = true, DefaultValue = 0.00M }
        };

        /// <summary>
        /// EFT Email Remittance Line DT
        /// </summary>
        public static readonly DataColumnMap[] EFTEmailLineDetails = 
        {
            new DataColumnMap() { ColumnName = "EftId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "EftRowId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "EftApplyId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ItemReferenceNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ItemAmount", Type = typeof(Decimal),IsDefaultValue = true, DefaultValue = 0.00M },
            new DataColumnMap() { ColumnName = "CreatedBy", Type = typeof(string) }
        };
    }
}
