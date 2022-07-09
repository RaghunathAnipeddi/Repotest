using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.Entities.Business_Entities.TaskScheduler.APOut
{
    public class PayableManagementEntity
    {
        public string ManualPaymentNumber { get; set; }
        public string FileName { get; set; }
        public string UserId { get; set; }
        public int FromExpenseId { get; set; }
        public int ToExpenseId { get; set; }
        public DateTime FromDocumentDate { get; set; }
        public DateTime ToDocumentDate { get; set; }
        public string FromVendorId { get; set; }
        public string ToVendorId { get; set; }
        public string SearchType { get; set; }
        public string SourceLookup { get; set; }
        public string LookupValue { get; set; }
        public string SourceFormName { get; set; }
        public SqlConnection Connection { get; set; }
        public PayableDetailsEntity InvoiceDetail { get; set; }
        public StringBuilder LogMessage { get; set; }
        
        public List<string> Files { get; set; }

        #region CTSI
        public string FromCtsiId { get; set; }
        public string ToCtsiId { get; set; }
        public string Company { get; set; }
        public string Source { get; set; }
        #endregion 

        #region API
        public string FromApiId { get; set; }
        public string ToApiId { get; set; }
        public int InvoiceType { get; set; }
       

        #endregion
    }
}
