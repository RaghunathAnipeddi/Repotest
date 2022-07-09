using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class CreditCardInformation
    {
        public int CreditCardId { get; set; }

        public string Name { get; set; }

        public long CreditCardNumber { get; set; }

        public string CreditCardProvider { get; set; }

        public DateTime CreditCardExpiryDate { get; set; }

        public string Instructions { get; set; }

        public string NameAsOnCard { get; set; }

        public int SecurityCode { get; set; }
    }
}
