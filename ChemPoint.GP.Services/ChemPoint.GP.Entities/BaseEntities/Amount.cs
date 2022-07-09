using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class Amount : IModelBase
    {
        public string Currency { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal UnappliedAmount { get; set; }

        public decimal AmountRemaining { get; set; }

        public decimal ApplyAmount { get; set; }

        public decimal DocumentAmount { get; set; }

        public decimal OriginatingCurrencyDocumentAmount { get; set; }

        public decimal OriginatingAmountRemaining { get; set; }
        public decimal ApplyAmountInOrignCurrency { get; set; }

        public decimal ExchangeRate { get; set; }

    }
}
