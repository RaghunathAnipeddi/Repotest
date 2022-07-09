using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class CreditProfile
    {
        public int CreditType { get; set; }

        public string PaymentTerms { get; set; }

        public Amount CreditLimit { get; set; }

        public Amount OutstandingBalance { get; set; }

        public bool CreditHoldFlag { get; set; }

        public int CreditHealthIndicator { get; set; }

        public bool CreditReviewRequireFlag { get; set; }

        public bool TerritoryExceptionFlag { get; set; }

        public int CountOfAccounts { get; set; }

        public string Instructions { get; set; }
    }
}
