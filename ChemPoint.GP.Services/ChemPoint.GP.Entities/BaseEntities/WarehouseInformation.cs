using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class WarehouseInformation
    {
        public string WarehouseId { get; set; }

        public string WarehouseType { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string Address3 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string ZipCode { get; set; }

        public string WarehouseProfile { get; set; }

        public Status Status { get; set; }

        public AuditInformation AuditInformation { get; set; }

        public string Instructions { get; set; }
    }
}
