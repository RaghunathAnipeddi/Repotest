using System;
using System.Collections;

namespace ChemPoint.TaxEngine
{
    /// <summary>
    /// Class which holds the tax details.
    /// </summary>
    public class TaxResult 
    {
        public TaxResult()
        {
        }

        #region Private_Properties
        
        /// <summary>
        /// Private variable for the public property TaxLineDetails
        /// </summary>
        private ArrayList taxLineDetails = new ArrayList();
        
        #endregion Private_Properties
        
        #region Public_Properties
        
        /// <summary>
        /// Represents the document status present in avalara database.
        /// </summary>
        public string DocStatus { get; set; }
        
        /// <summary>
        /// Represents the document type (Temporary, Saved, Posted, etc).
        /// </summary>
        public string DocType { get; set; }
        
        /// <summary>
        /// Last timestamp when the tax was calculated
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Total document amount
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// Total exemption amount
        /// </summary>
        public decimal TotalExemption { get; set; }
        
        /// <summary>
        /// Header frieght tax rate
        /// </summary>
        public double FrieghtTaxRate { get; set; }
        
        /// <summary>
        /// Header Misc tax rate
        /// </summary>
        public double MiscTaxRate { get; set; }
        
        /// <summary>
        /// Total document amount which is taxable
        /// </summary>
        public decimal TotalTaxable { get; set; }
        
        /// <summary>
        /// Tax line details, tax details splitup for every line
        /// </summary>
        public TaxLineDetail[] TaxLineDetails
        {
            get
            {
                TaxLineDetail[] lineArray = new TaxLineDetail[taxLineDetails.Count];
                for (int i = 0; i < taxLineDetails.Count; i++)
                    lineArray[i] = (TaxLineDetail)taxLineDetails[i];
                
                return lineArray;
            }
        }
        
        #endregion Public_Properties
        
        #region Public_Methods
        
        /// <summary>
        /// Method which adds a new TaxLineDetail object.
        /// </summary>
        /// <param name="taxLineDetail"></param>
        public void AddTaxLineDetail(TaxLineDetail taxLineDetail)
        {
            taxLineDetails.Add(taxLineDetail);
        }

        #endregion Public_Methods
    }
}
