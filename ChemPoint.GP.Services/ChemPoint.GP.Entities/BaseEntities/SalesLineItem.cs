using System;
using System.Collections.Generic;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class SalesLineItem : IModelBase
    {
        public string SopNumber { get; set; }

        public int SopType { get; set; }

        public int OrderLineId { get; set; }

        public string ItemNumber { get; set; }

        public string ItemDescription { get; set; }

        public string  LocationCode { get; set; }

        public string CarrierDescription { get; set; }

        public string FreightService { get; set; }

        public int ComponentSequenceNumber { get; set; }

        public string QuoteNumber { get; set; }

        public string QuoteGuid { get; set; }

        public int QuoteNumberVersion { get; set; }

        public bool IsFip { get; set; }

        public bool IsServiceSKU { get; set; }

        public decimal FreightIncludedPrice { get; set; }

        public decimal OrginatingFreightPrice { get; set; }

        public decimal FunctionalFreightPrice { get; set; }

        public string ItemUofM { get; set; }

        public int CurrencyDecimalPlaces { get; set; }

        public string OrderLineExemptionNo { get; set; }

        public string HtsNumber { get; set; }

        public string CountryOfOrigin { get; set; }

        public string UnNumber { get; set; }

        public string EccnNumber { get; set; }

        public decimal NetWeight { get; set; }

        public decimal ShipWeight { get; set; }

        public decimal OrderedQuantity { get; set; }

        public decimal CancelledQuantity { get; set; }

        public decimal AllocatedQuantity { get; set; }

        public decimal QuantityRemaining { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPriceAmount { get; set; }

        public decimal UnitCostAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal LineTotalAmount { get; set; }

        public decimal LineFreightAmount { get; set; }

        public LotInformation[] LotsArry { get; set; }

        public LotInformation Lots { get; set; }

        public List<LotInformation> LotsList { get; set; }

        public DateTime ActualShipDate { get; set; }

        public string CarrierInstructionId { get; set; }

        public string WarehouseInstructionId { get; set; }

        public string InternalInstructionId { get; set; }

        public string LineStatus { get; set; }

        public List<SalesInstruction> SalesLineInstruction { get; set; }

        public List<TrackingInformation> SalesLineTrackingNumber { get; set; }

    }
}
