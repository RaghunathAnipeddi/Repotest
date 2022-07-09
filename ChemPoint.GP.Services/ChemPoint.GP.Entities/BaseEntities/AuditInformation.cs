using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class AuditInformation
    {
        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string ModifiedBy { get; set; }

        public string Instructions { get; set; }

        public int CompanyId { get; set; }
    }
}
