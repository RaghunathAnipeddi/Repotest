using System;
using System.Linq;

namespace ChemPoint.GP.InventoryDL.Utils
{
    public static class DataColumnMappings
    {
        public static readonly DataColumnMap[] SaveSalesOrder = 
        {
            new DataColumnMap() { ColumnName = "SopType", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "SopNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "OrigType", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "OrigNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "MasterNumber", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "ActualShipDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "IsOrderToCancel", Type = typeof(bool) },
            new DataColumnMap() { ColumnName = "BillToAddressID", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ShipToAddressID", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "WarehouseInstructionID", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CarrierInstructionID", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CurCustReqShipDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "CurCustReqDelDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "CurSchedDelDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "OrigCustReqDelDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "OrigSchedDelDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "CurSchedShipDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "OrigSchedShipDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "FreightTerm", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "IncoTerm", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CustomerPickupAddressId", Type = typeof(int) },
            new DataColumnMap() { ColumnName = "CarrierName", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CarrierAccountNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CarrierPhone", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ThirdPartyAddressId", Type = typeof(int) },
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
            new DataColumnMap() { ColumnName = "FreightPrice", Type = typeof(Decimal) },
            new DataColumnMap() { ColumnName = "QuoteNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ActualShipDate", Type = typeof(DateTime) },
            new DataColumnMap() { ColumnName = "ShipWeight", Type = typeof(Decimal) },
            new DataColumnMap() { ColumnName = "NetWeight", Type = typeof(Decimal) },
            new DataColumnMap() { ColumnName = "UNNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "CountryofOrigin", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "ECCNNumber", Type = typeof(string) },
            new DataColumnMap() { ColumnName = "HTSNumber", Type = typeof(string) },
        };
    }
}

