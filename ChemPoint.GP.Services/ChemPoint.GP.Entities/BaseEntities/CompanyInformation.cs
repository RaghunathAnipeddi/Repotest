using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class CompanyInformation
    {
        public int CompanyId { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public string BusinessUnitName { get; set; }

        public string Currency { get; set; }

        public string Url { get; set; }

        public string Region { get; set; }

        public string DunsNumber { get; set; }

        public string ContactInformation { get; set; }

        public string CompanyProfile { get; set; }

        public AddressInformation[] Addresses { get; set; }

        public string TaxInformation { get; set; }

        public AccountInformation AccountInformation { get; set; }

        public Status Status { get; set; }

        public AuditInformation AuditInformation { get; set; }

        public string Instructions { get; set; }
    }
}
