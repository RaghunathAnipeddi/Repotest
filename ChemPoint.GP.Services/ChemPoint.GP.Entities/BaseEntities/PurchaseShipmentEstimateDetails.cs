using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class PurchaseShipmentEstimateDetails
    {
        public int EstimateID { get; set; }
        public string Warehouse { get; set; }
        public string Vendor { get; set; }
        public string CarrierReference { get; set; }
        public decimal EstimatedCost { get; set; }
        public DateTime EstimatedShipDate { get; set; }
        public string CurrencyId { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime ExchangeExpirationDate { get; set; }
        public decimal TotalNetWeight { get; set; }
        public int IsMatchedToReceipt { get; set; }
        public string POReceipt { get; set; }
        public string EstimatedShipmentNotes { get; set; }
        public string UserId { get; set; }

        public string PoNumber { get; set; }
        public int PoLineNumber { get; set; }
        public string ItemNumber { get; set; }
        public string ItemDesc { get; set; }
        public string UoM { get; set; }
        public decimal QtyOrdered { get; set; }
        public decimal QtyCancelled { get; set; }
        public decimal QtyPrevShipped { get; set; }
        public decimal QtyRemaining { get; set; }
        public decimal EstimatedQtyShipped { get; set; }
        public decimal EstimatedQtyNetWeight { get; set; }
        public decimal EstimatedShipmentCost { get; set; }
        public int ReceiptLineNumber { get; set; }
        public bool IsLineMatched { get; set; }
        public int ShipmentEstimateMatchType { get; set; }
        public decimal QtyVariance { get; set; }
        public int IsAvailable { get; set; }
        public int PoOrdNumber { get; set; }
        public string LocationDesc { get; set; }
        public string VendorName { get; set; }
        public int CurrencyIndex { get; set; }
        public int DecimalPlaces { get; set; }
        public int WindowValue { get; set; }
    }
    public class PurchaseSelectPoLines
    {
        public string PoNumber { get; set; }
        public int PoLineNumber { get; set; }
        public string VendorId { get; set; }
        public string Warehouse { get; set; }
        public string HasCostVariance { get; set; }
        public DateTime CurSchedShip { get; set; }
        public DateTime CurSchedDel { get; set; }
        public DateTime CurAvailDate { get; set; }
        public DateTime ActualShipDate { get; set; }
        public DateTime AcknowledgementDate { get; set; }
        public DateTime ConfirmedDate { get; set; }
        public string POStatus { get; set; }
        public string UoM { get; set; }
        public decimal NetWeight { get; set; }
        public string ItemNumber { get; set; }
        public string ItemDescription { get; set; }
        public decimal QtyOrdered { get; set; }
        public decimal QtyCancelled { get; set; }
        public decimal QtyPrevShipped { get; set; }
        public decimal QtyRemaining { get; set; }
        public string PoIndicatorStatus { get; set; }
        public int Ord { get; set; }
    }

    public class PoEstimateId
    {
        public int EstimateID { get; set; }
    }

    public class PoList
    {
        public string PoNumber { get; set; }
        public string LocationCode { get; set; }
        public string VendorId { get; set; }
    }

    public class CurrencyDetails
    {
        public int CurrencyIndex { get; set; }
        public int DecimalPlaces { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime ExchangeExpirationDate { get; set; }

    }
}
