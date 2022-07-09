using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Model.Interactions.Sales
{
    public class ReceivablesRequest : IModelBase
    {

        public AuditInformation AuditInformation { get; set; }

        public CustomerInformation CustomerInformation { get; set; }

        public EMailInformation SalesOrderFailureEmail { get; set; }

        public EMailInformation SalesPriorityOrdersEmail { get; set; }

        public string EUEconnectConnectionString { get; set; }

        public string NAEconnectConnectionString { get; set; }

        public string LoggingPath { get; set; }

        public string LoggingFileName { get; set; }

        public string Source { get; set; }

        public string EFTPaymentStyleSheetPath { get; set; }

        public string EFTApplyStyleSheetPath { get; set; }

        public string EFTPaymentAndApplyStyleSheetPath { get; set; }

        public ReceivablesHeader ReceivablesHeader { get; set; }

        public List<ReceivableDetails> ReceivableEntity { get; set; }

        public EFTPayment EFTPayment { get; set; }

        public List<EFTPayment> EFTPaymentList { get; set; }

        public List<EFTCustomerPayment> EFTCustomerPaymentList { get; set; }

        public List<EFTCashReceipt> EFTCashReceiptList { get; set; }

        public List<EFTPayment> EFTOriginalList { get; set; }

        public List<EFTPayment> EFTFilteredPaymentList { get; set; }

        public List<EFTPayment> EFTMailList { get; set; }

        public List<EFTPayment> EFTTransactionList { get; set; }

        public List<EFTPayment> EFTTransactionMapList { get; set; }

        public Int16 Searchtype { get; set; }
        public string CustomerIdStart { get; set; }
        public string CustomerIdEnd { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string ReferenceNoStart { get; set; }
        public string ReferenceNoEnd { get; set; }
        public string DocNumbrStart { get; set; }
        public string DocNumbrend { get; set; }
        public Int16 Actiontype { get; set; }

        public string BatchId { get; set; }

        public string EFTPaymentFilePath { get; set; }

        public string EFTPaymentFileName { get; set; }

        public int CompanyId { get; set; }

        public string UserName { get; set; }

        public string ConnectionString { get; set; }

        public string LogMessage { get; set; }

        public decimal ControlAmount { get; set; }

        public int PaymentCount { get; set; }
    }
}
