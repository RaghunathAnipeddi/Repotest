using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class SalesOrderInformation
    {
        public string OrderId { get; set; }

        public string SalesOrderGuid { get; set; }
        
        public string InvoiceId { get; set; }
        
        public CustomerInformation Customer { get; set; }
        
        public WarehouseInformation Warehouse { get; set; }

        public SalesOrderDetails SalesOrderDetails { get; set; }
    }
}
