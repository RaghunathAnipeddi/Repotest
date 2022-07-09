using System;
using System.Collections;

namespace ChemPoint.TaxEngine
{
    /// <summary>
    /// Company Code Enum
    /// </summary>
    public enum CompanyCodeEnum
    {
        Chmpt = 0,
        Cpeur = 1,
    }

    /// <summary>
    /// Currency code enum
    /// </summary>
    public enum CurrencyCodeEnum
    {
        USD = 0,
        CAD = 1,
        EUR = 2,
        GBP = 3,
    }

    /// <summary>
    /// Customer Shipping method type
    /// </summary>
    public enum LineShippingType
    {
        Delivery = 0,
        Pickup = 1       
    }

    /// <summary>
    /// Tax which holds details used to calculate tax
    /// </summary>
    public class TaxRequest 
    {
        public TaxRequest() 
        {
            CmpyCode = CompanyCodeEnum.Chmpt;
            OrderCurrencyCode = CurrencyCodeEnum.USD;
            CustomerNumber = string.Empty;
            DocumentNumber = string.Empty;
            CustomerExemptionNo = string.Empty;
            CustomerUseCode = string.Empty;
        }

        #region Private_Variables
        
        /// <summary>
        /// private varaible for the public property LineDetails.
        /// </summary>
        private ArrayList lineDetails = new ArrayList();
        
        #endregion Private_Variables
        
        /// <summary>
        /// Company code property
        /// </summary>
        public CompanyCodeEnum CmpyCode { get; set; }
        
        /// <summary>
        /// Currency Code property
        /// </summary>
        public CurrencyCodeEnum OrderCurrencyCode { get; set; }
        
        /// <summary>
        /// customer number property
        /// </summary>
        public string CustomerNumber { get; set; }
        
        /// <summary>
        /// Order number 
        /// </summary>
        public string DocumentNumber { get; set; }
        
        /// <summary>
        /// Order date
        /// </summary>
        public DateTime DocumentDate { get; set; }
        
        /// <summary>
        /// Customer tax exemption number if any
        /// </summary>
        public string CustomerExemptionNo { get; set; }
        
        /// <summary>
        /// Customer Avatax Use Code if any
        /// </summary>
        public string CustomerUseCode { get; set; }
        
        /// <summary>
        /// Line details which holds order line details
        /// </summary>
        public LineDetail[] LineDetails 
        { 
            get 
            { 
                LineDetail[] lineArray = new LineDetail[lineDetails.Count];
                for (int i = 0; i < lineDetails.Count; i++)
                    lineArray[i] = (LineDetail)lineDetails[i];
                
                return lineArray;
            }
        }
        
        /// <summary>
        /// Addds a new lineDetail
        /// </summary>
        /// <param name="lineDetail"></param>
        public void AddLineDetail(LineDetail lineDetail)
        {
            lineDetails.Add(lineDetail);
        }
    }
}
