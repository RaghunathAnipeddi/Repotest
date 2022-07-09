using System;
using System.Collections.Generic;

namespace ChemPoint.GP.Entities.BaseEntities
{
    /// <summary>
    /// -------------------------------------------------------------------------------------------
    ///     Date		Author          Description
    ///-------------------------------------------------------------------------------------------
    /// **26Jan2017     RReddy      IsCustomerExcempted,CustomizeServiceSkus included AND Removed ExcludeServiceSkus field
    /// </summary>
    public class OrderHeader : IModelBase
    {
        public int SopType { get; set; }

        public string SopNumber { get; set; }

        public string InvoiceNumber { get; set; }

        public int InvoiceType { get; set; }

        public string[] TrackingNumber { get; set; }

        public string OrderCurrency { get; set; }

        public string OrderGuid { get; set; }

        public long MasterNumber { get; set; }

        public DateTime OrderDate { get; set; }

        public bool IsServiceSku { get; set; }

        public string ShipVia { get; set; }

        public string ShipViaType { get; set; }

        public string FreightTerm { get; set; }

        public string CarrierName { get; set; }

        public string CarrierAccountNumber { get; set; }

        public string CarrierPhone { get; set; }

        public string IncoTerm { get; set; }

        public string ServiceType { get; set; }

        public string ImporterofRecord { get; set; }

        public List<IncoTerm> Inco { get; set; }

        public AddressInformation Carrier3rdPartyAddress { get; set; }

        public AddressInformation CustomerPickupAddress { get; set; }

        public int ThirdPartyAddressId { get; set; }

        public string CustomBroker { get; set; }

        public string MainCustomerNumber { get; set; }

        public string CustomerGuid { get; set; }

        public List<AddressInformation> AddressDetails { get; set; }

        public AddressInformation FinalDestinationAddress { get; set; }

        public AddressInformation BillToAddress { get; set; }

        public AddressInformation ShipToAddress { get; set; }

        public decimal SubTotalAmount { get; set; }

        public decimal TradeDiscountAmount { get; set; }

        public decimal MiscAmount { get; set; }

        public decimal FreightAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal ExchangeRateAmount { get; set; }

        public decimal OrderTotalAmount { get; set; }

        public string OrderStatus { get; set; }

        public int ToBeCancelled { get; set; }

        //public int ExcludeServiceSkus { get; set; }

        public bool IsCustomerExcempted { get; set; }

        public int CustomizeServiceSkus { get; set; }

        public string LocationCode { get; set; }

        public string TaxScheduleId { get; set; }

        public string TaxRegNumber { get; set; }

        public int FreightDecimalPlaces { get; set; }

        public int ExtendedDecimalPlaces { get; set; }

        public string CarrierInstructionId { get; set; }

        public string WarehouseInstructionId { get; set; }

        public string InternalInstructionId { get; set; }

        public List<SalesInstruction> SalesHeaderInstructionEntity { get; set; }

        public DateTime OrderSubmittedDate { get; set; }

        public string PaymentTerm { get; set; }

        public string ShipToCountry	 { get; set; }
        
        public string ShipToCountryCode	{ get; set; }
        
        public string ShipFromCountry { get; set; }
        
        public string ShipFromCountryCode { get; set; }

        public string FinalDestinationCountry { get; set; }

        public string FinalDestinationCountryCode { get; set; }

        public string NewShipToCountry { get; set; }

        public string NewShipToCountryCode { get; set; }

        public string NewShipFromCountry { get; set; }

        public string NewShipFromCountryCode { get; set; }

        public string NewFinalDestinationCountry { get; set; }

        public string NewFinalDestinationCountryCode { get; set; }

        public bool IsLineAdded { get; set; }

    }
}
