using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class TaxSetupInformation
    {
        //Tax Detail Setup
        public string TaxDetailId { get; set; }

        public string TaxDetailReference { get; set; }

        public string TaxDetailUnivarNvTaxCode { get; set; }

        public string TaxDetailUnivarNvTaxCodeDescription { get; set; }

        //Tax Schedule Setup
        public string TaxScheduleId { get; set; }

        public string TaxScheduleChempointVatNumber { get; set; }

        public bool TaxScheduleIsActive { get; set; }
    }
}
