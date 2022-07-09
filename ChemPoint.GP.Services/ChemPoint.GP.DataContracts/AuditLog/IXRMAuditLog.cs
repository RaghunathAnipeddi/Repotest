using ChemPoint.GP.DataContracts.Base;
using ChemPoint.GP.Entities.Business_Entities.AuditLog;

namespace ChemPoint.GP.DataContracts.AuditLog
{
    public interface IXrmAuditLog : IRepository
    {
        object UpdateXrmIntegrationsAuditLog(AuditLogEntity auditLogEntity);
    }
}
