using System;

namespace ChemPoint.TaxEngine
{
    /// <summary>
    /// This class represents one line detail of TaxResult object.
    /// </summary>
    public class TaxLineDetail
    {
        public TaxLineDetail()
        {
        }

        public string BoundaryLevel { get; set; }

        public decimal Discount { get; set; }

        public int ExemptCertId { get; set; }

        public decimal Exemption { get; set; }

        public string LineSeq { get; set; }

        public double Rate { get; set; }

        public decimal Tax { get; set; }

        public bool Taxability { get; set; }

        public decimal TaxableAmt { get; set; }

        public string TaxCode { get; set; }
    }
}
