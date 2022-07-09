using System;
using System.Linq;

namespace ChemPoint.GP.SalesOrderDL.Utils
{
    public static class DataColumnMappings
    {
        public static readonly DataColumnMap[] SaveSalesOrder = 
        {
            new DataColumnMap() { ColumnName = "SopType", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "SopNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "FreightTerm", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "IncoTerm", Type = typeof(string) },
            //new DataColumnMap() { ColumnName= "CustomerPickupAddress", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CarrierName", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CarrierAccountNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CarrierPhone", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ThirdPartyAddressId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ServiceType", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ImporterofRecord", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CustomBroker", Type = typeof(string) }
        };
        public static readonly DataColumnMap[] SaveItemDetail = 
        {
            new DataColumnMap() { ColumnName = "SopType", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "SopNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "LineItemSequence", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ComponentLineSeqNumber", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ItemNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "FreightPrice", Type = typeof(Decimal), IsDefaultValue = true, DefaultValue = 0.00M },
            new DataColumnMap() { ColumnName = "QuoteNumber", Type = typeof(string), IsDefaultValue = true, DefaultValue = "" },
            new DataColumnMap() { ColumnName = "ActualShipDate", Type = typeof(DateTime), IsDefaultValue = true, DefaultValue = DateTime.Parse("1900/01/01", System.Globalization.CultureInfo.CreateSpecificCulture("en-CA")) },
            new DataColumnMap() { ColumnName = "ShipWeight", Type = typeof(Decimal), IsDefaultValue = true, DefaultValue = "0.00000" },
            new DataColumnMap() { ColumnName = "NetWeight", Type = typeof(Decimal), IsDefaultValue = true, DefaultValue = "0.00000" },
            new DataColumnMap() { ColumnName = "UNNumber", Type = typeof(string), IsDefaultValue = true, DefaultValue = "0" },
            new DataColumnMap() { ColumnName = "CountryofOrigin", Type = typeof(string), IsDefaultValue = true, DefaultValue = "" },
            new DataColumnMap() { ColumnName = "ECCNNumber", Type = typeof(string), IsDefaultValue = true, DefaultValue = "0" },
            new DataColumnMap() { ColumnName = "HTSNumber", Type = typeof(string), IsDefaultValue = true, DefaultValue = "0" },
            //new DataColumnMap() { ColumnName = "OriginatingFreightPrice", Type = typeof(Decimal), IsDefaultValue = true, DefaultValue = 0.00M },
        };

        public static readonly DataColumnMap[] SaveSalesOrderType = 
        {
            new DataColumnMap() { ColumnName = "SopType", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "SopNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "IsInternational", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "IsNAFTA", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "IsDropship", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "IsBulk", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "IsI3Order", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "IsCreditCard", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "IsCorrective", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "IsFullTruckLoad", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "IsConsignment", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "IsCorrectiveBoo", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "IsSampleOrder", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "IsTempControl", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "IsHazmat", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "IsFreezeProtect", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "IsAutoPTEligible", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "IsCreditEnginePassed", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "IsTaxEnginePassed", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "ModifiedBy", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CompanyId", Type = typeof(int) },
        };

        public static readonly DataColumnMap[] SaveTransactionAddressCodes = 
        {
            new DataColumnMap() { ColumnName = "SopType", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "SopNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CustomerID", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CustomerName", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "AddressTypeID", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "AddressCode", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ShipToName", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ContactPerson", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Address1", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Address2", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Address3", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Country", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "City", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "State", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Zip", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CCode", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Phone1", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Phone2", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Phone3", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Fax", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ModifiedBy", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CompanyId", Type = typeof(int) },
        };

        public static readonly DataColumnMap[] SaveThirdPartyAddress = 
        {
            new DataColumnMap() { ColumnName = "SalesThirdPartyAddressID", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "SopType", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "SopNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "AddressId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ContactName", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "AddressLine1", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "AddressLine2", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "AddressLine3", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "City", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "State", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ZipCode", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Country", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ModifiedBy", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CompanyId", Type = typeof(int) },
        };
        public static readonly DataColumnMap[] SaveScheduleDate = 
        {
            new DataColumnMap() { ColumnName = "SopType", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "SopNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CurCustReqShipDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "CurCustReqDelDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "CurSchedDelDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "CurSchedShipDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "OrigCustReqDelDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "OrigSchedDelDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "OrigSchedShipDate", Type = typeof(DateTime) },
        };

        public static readonly DataColumnMap[] SaveHeaderComment = 
        {
            new DataColumnMap() { ColumnName = "SopType", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "SopNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "InstructionTypeID", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "CommentID", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CommentText", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ModifiedBy", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CompanyId", Type = typeof(int) },
        };

        public static readonly DataColumnMap[] SaveLineComment =
        {
            new DataColumnMap() { ColumnName = "SopType", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "SopNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "LineItemSequence", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ComponentSeqNumber", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "InstructionTypeID", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "CommentID", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CommentText", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ModifiedBy", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CompanyId", Type = typeof(int) },
        };

        public static readonly DataColumnMap[] ValidateServiceSKU = 
        {
            new DataColumnMap() { ColumnName = "SopNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "SopType", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "FreightTerm", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CurrencyId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CustomerNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "MiceAmount", Type =typeof(Decimal), IsDefaultValue = true, DefaultValue = 0.00M },
            new DataColumnMap() { ColumnName = "ItemNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Quantity", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "WarehouseID", Type = typeof(string) },
             new DataColumnMap() { ColumnName = "CreatedOn", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "RequestedShipDate", Type = typeof(DateTime) }, 
        };

        public static readonly DataColumnMap[] SaveAllocatedQty = 
        {
            new DataColumnMap() { ColumnName = "SopType", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "SopNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "LineItemSequence", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ComponentLineSeqNumber", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ItemNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "LocationCode", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "AllocatedQty", Type = typeof(Decimal), IsDefaultValue = true, DefaultValue = 0.00M  },
            new DataColumnMap() { ColumnName = "Quantity", Type = typeof(Decimal), IsDefaultValue = true, DefaultValue = 0.00M  },
            new DataColumnMap() { ColumnName = "CancelledQuantity", Type = typeof(Decimal), IsDefaultValue = true, DefaultValue = 0.00M  },
            new DataColumnMap() { ColumnName = "QuantityRemaining", Type = typeof(Decimal), IsDefaultValue = true, DefaultValue = 0.00M  },
            new DataColumnMap() { ColumnName = "ModifiedBy", Type = typeof(string) },
        };

         public static readonly DataColumnMap[] SaveApplyToOpenOrder = 
        {
            new DataColumnMap() { ColumnName = "DocumentNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "DocumentType", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "CustomerId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CurrencyId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ApplyToDocumentNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ApplyToDocumentType", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ApplyToCustomerId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "OriginatingAmountRemaining", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "OriginatingDocumentAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "OriginatingApplyAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "FunctionalAmountRemaining", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "FunctionalDocumentAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "FunctionalApplyAmount", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "DocumentCurrencyId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ExchangeRate", Type = typeof(decimal) },
            new DataColumnMap() { ColumnName = "ApplyDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "DocumentStatus", Type = typeof(string) }, 
            new DataColumnMap() { ColumnName = "JobStatusId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "IsSelected", Type = typeof(bool) },
        };


        public static readonly DataColumnMap[] ReceivablesItemDetail = 
        {
            new DataColumnMap() { ColumnName = "InvoiceNumber", Type = typeof(string), IsDefaultValue = true, DefaultValue = "" },
            new DataColumnMap() { ColumnName = "SupportId", Type = typeof(int), IsDefaultValue = true, DefaultValue = 0 },
            new DataColumnMap() { ColumnName = "IncidentId", Type = typeof(int), IsDefaultValue = true, DefaultValue = 0 },
            new DataColumnMap() { ColumnName = "ItemNumber", Type = typeof(string), IsDefaultValue = true, DefaultValue = "" },
            new DataColumnMap() { ColumnName = "ItemDescription", Type = typeof(string), IsDefaultValue = true, DefaultValue = "" },
            new DataColumnMap() { ColumnName = "LineItemSequence", Type = typeof(int), IsDefaultValue = true, DefaultValue = 0 },
            new DataColumnMap() { ColumnName = "IsSelected", Type = typeof(bool), IsDefaultValue = true, DefaultValue = false },
            new DataColumnMap() { ColumnName = "Volume", Type = typeof(Decimal), IsDefaultValue = true, DefaultValue = 0.00M },
            new DataColumnMap() { ColumnName = "Quantity", Type = typeof(Decimal), IsDefaultValue = true, DefaultValue = 0.00M },
            new DataColumnMap() { ColumnName = "NetWeight", Type = typeof(Decimal), IsDefaultValue = true, DefaultValue = 0.00M },
            new DataColumnMap() { ColumnName = "ExtendedPrice", Type = typeof(Decimal), IsDefaultValue = true, DefaultValue = 0.00M },
            new DataColumnMap() { ColumnName = "AppliedAmount", Type = typeof(Decimal), IsDefaultValue = true, DefaultValue = 0.00M },
        };

        public static readonly DataColumnMap[] SaveEFTCustomerSource = 
        {
            new DataColumnMap() { ColumnName = "NAEFTCustomerMappingId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "CustomerId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "EftCustomerDetail", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "Type", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CreatedBy", Type = typeof(string) },
        };

        public static readonly DataColumnMap[] SaveEFTCustomerRemittance = 
        {
            new DataColumnMap() { ColumnName = "RowId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "EftId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "EftAppId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "PaymentNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ReferenceNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "DateReceived", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "PaymentAmount", Type = typeof(Decimal),IsDefaultValue = true, DefaultValue = 0.00M },
            new DataColumnMap() { ColumnName = "CustomerID", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CurrencyId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "IsFullyApplied", Type = typeof(Int16) },
            new DataColumnMap() { ColumnName = "Source", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "BankOriginating", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ItemReferenceNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ItemAmount", Type = typeof(Decimal),IsDefaultValue = true, DefaultValue = 0.00M },
            new DataColumnMap() { ColumnName = "CreatedBy", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "IsUpdated", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "EftStatusId", Type = typeof(Int16) },
            new DataColumnMap() { ColumnName = "EftNotes", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "FileId", Type = typeof(Int16) },

        };


        public static readonly DataColumnMap[] SaveEFTEmailRemittance = 
        {
            new DataColumnMap() { ColumnName = "RowId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "EftId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ReferenceNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "DateReceived", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "PaymentAmount", Type = typeof(Decimal),IsDefaultValue = true, DefaultValue = 0.00M },
            new DataColumnMap() { ColumnName = "CustomerID", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CurrencyId", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ItemReferenceNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ItemAmount", Type = typeof(Decimal),IsDefaultValue = true, DefaultValue = 0.00M },
            new DataColumnMap() { ColumnName = "CreatedBy", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "IsUpdated", Type = typeof(string) },
        };


    }
}

