using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class SupplierInfomation
    {
        public string SupplierId { get; set; }

        public string SupplierType { get; set; }

        public bool CommercialSupplierFlag { get; set; }

        public bool PayableSupplierFlag { get; set; }

        public CompanyInformation CompanyInformation { get; set; }

        public string SupplierWarehouseProperties { get; set; }

        public Status Status { get; set; }

        public AuditInformation AuditInformation { get; set; }

        public string Instructions { get; set; }
    }
}
