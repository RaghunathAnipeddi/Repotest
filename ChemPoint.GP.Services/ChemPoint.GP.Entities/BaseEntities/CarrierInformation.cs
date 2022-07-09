using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class CarrierInformation
    {
        public int CarrierId { get; set; }

        public string CarrierCode { get; set; }

        public CompanyInformation Company { get; set; }

        public Status Status { get; set; }

        public AuditInformation AuditInformation { get; set; }

        public string Instructions { get; set; }
    }
}
