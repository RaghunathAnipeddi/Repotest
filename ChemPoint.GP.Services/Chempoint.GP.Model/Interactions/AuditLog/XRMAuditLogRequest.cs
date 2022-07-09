using ChemPoint.GP.Entities.Business_Entities.AuditLog;

namespace Chempoint.GP.Model.Interactions.AuditLog
{
    public class XrmAuditLogRequest
    {
        public string ConnectionString { get; set; }

        public AuditLogEntity AuditLogEntity { get; set; }
    }
}