using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Model.Interactions.Sales
{
    public  class ReceivablesResponse
    {
        public List<ReceivableDetails> ReceivableEntity { get; set; }

        public ResponseStatus Status { get; set; }

        public bool ValidationStatus { get; set; }

        public string ErrorMessage { get; set; }

        public CustomerInformation CustomerInformation { get; set; }

        public List<EFTPayment> EFTCustomerRemittancesList { get; set; }

        public List<EFTCustomerPayment> EFTCustomerPaymentList { get; set; }

        public List<EFTCashReceipt> EFTCashReceiptList { get; set; }

        public EFTPayment EFTPayment { get; set; }

        public string paymentRemitInputXml { get; set; }

        public DataSet ApplyPaymentDetail { get; set; }

        public List<string> LockedOrders { get; set; }

        public List<string> UnlockedOrders { get; set; }

        public string LogMessage { get; set; }

        public decimal BatchAmount { get; set; }

    }
}
