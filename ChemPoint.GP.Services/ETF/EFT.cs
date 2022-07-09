using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.ETF
{
    public class EFT
    {

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

        public int EftFileReferenceId { get; set; }

        public string ReferenceNo { get; set; }
    }

    public class FilTeredEFT
    {
        public string PaymentNumber { get; set; }

        public string ReferenceNumber { get; set; }

        public DateTime DateRecived { get; set; }

        public decimal PaymentAmount { get; set; }

        public string ItemReference { get; set; }

        public decimal ItemAmount { get; set; }

        public string CustomerID { get; set; }

        public string ReceivedOnCTS { get; set; }

        public string Notes { get; set; }

        public int Status { get; set; }

        public string Source { get; set; }

        public string BankOriginatingID { get; set; }

        public int EftFileReferenceId { get; set; }

        public string AccountNo { get; set; }
    }
}
