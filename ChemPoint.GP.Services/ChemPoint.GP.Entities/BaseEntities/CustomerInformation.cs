using System;
using System.Collections.Generic;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class CustomerInformation : IModelBase
    {
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerGuid { get; set; }

        public string CustomerTaxExemptCode { get; set; }

        public string CustomerTaxUseCode { get; set; }

        public List<string> VatNumber { get; set; }

        public CompanyInformation CompanyInformationInfo { get; set; }

        public CreditProfile CreditProfileInfo { get; set; }

        public string IndustryId { get; set; }

        public AuditInformation AuditInformation { get; set; }

        public string Instructions { get; set; }

        public List<string> EftCTXCustomerSourceList { get; set; }

        public string EftCTXCustomerSource { get; set; }

        public string ParentCustomerId { get; set; }

        public string XrmParentCustomerId { get; set; }

        public List<CustomerMappingDetails> CustomerMappingDetails { get; set; }

        public int MaxCustomerReferenceId { get; set; }

    }
}
