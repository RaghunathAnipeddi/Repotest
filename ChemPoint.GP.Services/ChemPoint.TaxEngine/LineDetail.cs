using System;

namespace ChemPoint.TaxEngine
{
    /// <summary>
    /// This class holds the LineDetails for the TaxRequest object.
    /// </summary>
    public class LineDetail
    {
        public LineDetail() 
        {
            ItemNumber = string.Empty;
            LineSeq = 0;
            LineExtendedPrice = 0;
            LineFrieghtPrice = 0;
            LineExemptionNo = string.Empty;
            LineQty = 0;

            ShipToAddressLine1 = string.Empty;
            ShipToAddressLine2 = string.Empty;
            ShipToAddressLine3 = string.Empty;
            ShipToAddressCity = string.Empty;
            ShipToAddressState = string.Empty;
            ShipToAddressCountry = string.Empty;
            ShipToAddressZip = string.Empty;

            ShipFromAddressLine1 = string.Empty;
            ShipFromAddressLine2 = string.Empty;
            ShipFromAddressLine3 = string.Empty;
            ShipFromAddressCity = string.Empty;
            ShipFromAddressState = string.Empty;
            ShipFromAddressCountry = string.Empty;
            ShipFromAddressZip = string.Empty;

            LineShipMethod = string.Empty;
            LineShippingType = LineShippingType.Pickup;
        }

        /// <summary>
        /// Item number
        /// </summary>
        public string ItemNumber { get; set; }

        /// <summary>
        /// Line sequence number
        /// </summary>
        public int LineSeq { get; set; }

        /// <summary>
        /// Line extended price
        /// </summary>
        public decimal LineExtendedPrice { get; set; }

        /// <summary>
        /// Line frieght price
        /// </summary>
        public decimal LineFrieghtPrice { get; set; }

        /// <summary>
        /// Line exempt number
        /// </summary>
        public string LineExemptionNo { get; set; }

        /// <summary>
        /// Line Quantity
        /// </summary>
        public double LineQty { get; set; }

        /// <summary>
        /// Shipping To address property
        /// </summary>
        public string ShipToAddressLine1 { get; set; }

        public string ShipToAddressLine2 { get; set; }

        public string ShipToAddressLine3 { get; set; }

        public string ShipToAddressCity { get; set; }

        public string ShipToAddressState { get; set; }

        public string ShipToAddressCountry { get; set; }

        public string ShipToAddressZip { get; set; }

        public string ShipFromAddressLine1 { get; set; }

        public string ShipFromAddressLine2 { get; set; }

        public string ShipFromAddressLine3 { get; set; }

        public string ShipFromAddressCity { get; set; }

        public string ShipFromAddressState { get; set; }

        public string ShipFromAddressCountry { get; set; }

        public string ShipFromAddressZip { get; set; }

        public string LineShipMethod { get; set; }

        public LineShippingType LineShippingType { get; set; }
    }
}
