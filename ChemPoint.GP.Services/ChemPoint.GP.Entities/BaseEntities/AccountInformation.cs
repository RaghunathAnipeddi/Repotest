using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class AccountInformation
    {
        public int AccountHolderId { get; set; }

        public string Name { get; set; }

        public int AccountType { get; set; }

        public string AccountNumber { get; set; }

        public string BankName { get; set; }

        public string BankIdentificationNumber { get; set; }

        public CreditCardInformation CreditCardInformation { get; set; }

        public string Instructions { get; set; }

        public Status Status { get; set; }

        public AuditInformation AuditInformation { get; set; }
    }
}
