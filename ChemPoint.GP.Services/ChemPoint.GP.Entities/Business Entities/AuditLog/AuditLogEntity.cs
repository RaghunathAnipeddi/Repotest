using ChemPoint.GP.Entities.BaseEntities;
using System;

namespace ChemPoint.GP.Entities.Business_Entities.AuditLog
{
    public class AuditLogEntity : IModelBase
    {
        public string Source { get; set; }

        public string Operation { get; set; }

        public string SourceDocumentId { get; set; }

        public string HeaderGuid { get; set; }

        public string LineGuid { get; set; }

        public string HeaderStatus { get; set; }

        public string LineStatus { get; set; }

        public int Company { get; set; }

        public string Notes { get; set; }

        public DateTime CreatedOn { get; set; }

        public string UserID { get; set; }
    }
}
