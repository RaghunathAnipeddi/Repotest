using Chempoint.GP.Model.Interactions.AuditLog;

namespace ChemPoint.GP.BusinessContracts.AuditLog
{
    public interface IAuditLogBusiness
    {
        XrmAuditLogResponse UpdateXrmIntegrationsAuditLog(XrmAuditLogRequest xrmAuditLogRequest);
    }
}
