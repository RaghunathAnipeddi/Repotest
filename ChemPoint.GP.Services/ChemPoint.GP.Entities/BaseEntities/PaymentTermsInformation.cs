using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class PaymentTermsInformation
    {
        public string PaymentTermsID { get; set; }

        public string DueOfMonths { get; set; }

        public int EomEnabled { get; set; }

        public string OrderPrePaymentPct { get; set; }

        public int TermsGracePeriod { get; set; }

        public bool Nested { get; set; }

        public int DateWithin { get; set; }

        public string TermsIfYes { get; set; }

        public string TermsIfNo { get; set; }

        public DateTime DocDate { get; set; }

        public DateTime Duedate { get; set; }

        public AuditInformation AuditInformation { get; set; }
    }
}
