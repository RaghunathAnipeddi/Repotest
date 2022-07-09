using ChemPoint.GP.BusinessContracts.AuditLog;
using System;
using Chempoint.GP.Model.Interactions.AuditLog;
using Chempoint.GP.Infrastructure.Utils;
using ChemPoint.GP.DataContracts.AuditLog;

namespace ChemPoint.GP.CPServiceAuditLogBL
{
    public class XrmAuditLogBusiness : IAuditLogBusiness
    {
        public XrmAuditLogResponse UpdateXrmIntegrationsAuditLog(XrmAuditLogRequest xrmAuditLogRequest)
        {
            XrmAuditLogResponse xrmAuditLogResponse = null;
            IXrmAuditLog auditLogDataAccess = null;

            xrmAuditLogRequest.ThrowIfNull("xrmAuditLogRequest");
            xrmAuditLogRequest.AuditLogEntity.ThrowIfNull("xrmAuditLogRequest.AuditLogEntity");
            try
            {
                xrmAuditLogResponse = new XrmAuditLogResponse();
                auditLogDataAccess = new ChemPoint.GP.CPServiceAuditLog.XrmAuditLog(xrmAuditLogRequest.ConnectionString);
                auditLogDataAccess.UpdateXrmIntegrationsAuditLog(xrmAuditLogRequest.AuditLogEntity);
                xrmAuditLogResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                xrmAuditLogResponse.Status = Chempoint.GP.Model.Interactions.AuditLog.ResponseStatus.Error;
                xrmAuditLogResponse.ErrorMessage = ex.Message.ToString().Trim();
                
            }
            return xrmAuditLogResponse;
        }
    }
}
