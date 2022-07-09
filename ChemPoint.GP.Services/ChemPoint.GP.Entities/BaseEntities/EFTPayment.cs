using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class EFTPayment : IModelBase
    {
        public int EFTRowId { get; set; }

        public string PaymentNumber { get; set; }

        public string ReferenceNumber { get; set; }

        public DateTime DateReceived { get; set; }

        public decimal PaymentAmount { get; set; }

        public decimal TotalPaymentAmount { get; set; }

        public string ItemReference { get; set; }

        public decimal ItemAmount { get; set; }

        public string CustomerID { get; set; }

        public string ReceivedOnCTS { get; set; }
        
        public int IsFullyApplied { get; set; }

        public int IsSelected { get; set; }

        public int EftId { get; set; }

        public int EftAppId { get; set; }

        public string Source { get; set; }

        public string BankOriginatingID { get; set; }

        public int CTXId { get; set; }

        public string AccountNo { get; set; }

        public string Notes { get; set; }

        public List<string> EFTCustomerId { get; set; }

        public List<string> EFTItemReference { get; set; }

        public List<string> EFTReferenceNumber { get; set; }

        public List<string> EFTBatchId { get; set; }

        public int NAEFTTransactionId { get; set; }

        public int NAEFTTransactionApplyId { get; set; }

        public string ItemReferenceNumber { get; set; }

        public DateTime AsOf { get; set; }

        public string Currency { get; set; }

        public string BankIDType { get; set; }

        public string BankID { get; set; }

        public string Account { get; set; }

        public string DataType { get; set; }

        public int BAICode { get; set; }

        public string Description { get; set; }

        public decimal Amount { get; set; }

        public string BalanceOrValueDate { get; set; }

        public string CustomerReference { get; set; }

        public int ImmediateAvailability { get; set; }

        public int OneDayFloat { get; set; }

        public int TwoPlusDayFloat { get; set; }

        public string BankReference { get; set; }

        public int NoOfItems { get; set; }

        public string Text { get; set; }

        // public string ReceivedOnCTS { get; set; }

        public string CurrencyID { get; set; }

        public int EftStatusId { get; set; }

        public int NACTXSummaryDetailId { get; set; }

        public string TraceNumber { get; set; }

        public string ACHTraceNumber { get; set; }

        public string AccountName { get; set; }

        public string BankAccountNumber { get; set; }

        public string BankRoutingNumber { get; set; }

        public string CreatedBy { get; set; }
        public int IsUpdated { get; set; }
        public int EftFileId { get; set; }

        public decimal ControlAmount { get; set; }
        public int PaymentCount { get; set; }
        public decimal RemainingAmount { get; set; }
        public int RemainingCount { get; set; } 
    }

 

    public class EFTCashReceipt : IModelBase
    {
        public int EFTTransactionId { get; set; }
        public int EFTTransactionApplyId { get; set; }
        public string GPPaymentNumber { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime DateReceived { get; set; }
        public string CurrencyId { get; set; }
        public decimal PaymentAmount { get; set; }
        public string CustomerID { get; set; }
        public string ItemReferenceNumber { get; set; }
        public decimal ItemAmount { get; set; }
        public int EftStatusId { get; set; }
        public bool ISFullyApplied { get; set; }
        public string Source { get; set; }
        public string BankOriginating { get; set; }
        public string Notes { get; set; } 

    }

    public class EFTCustomerPayment : IModelBase
    {
        public int EftId { get; set; }
        public int EftAppId { get; set; }
        public string PaymentNumber { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime DateReceived { get; set; }
        public decimal PaymentAmount { get; set; }
        public string CustomerID { get; set; }
        public string CurrencyId { get; set; }
        public bool IsFullyApplied { get; set; }
        public string Source { get; set; }
        public string BankOriginating { get; set; }
        public string ItemReferenceNumber { get; set; }
        public decimal ItemAmount { get; set; }
        public string CreatedBy { get; set; }
        public int IsUpdated { get; set; }
        public int Status { get; set; }
        public string StatusReason { get; set; }
        public string AccountName { get; set; }
    }

}

