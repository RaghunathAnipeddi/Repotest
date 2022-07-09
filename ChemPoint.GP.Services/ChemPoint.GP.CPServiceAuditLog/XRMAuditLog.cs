using System.Collections.Generic;
using System.Data;
using Chempoint.GP.Infrastructure.Utils;
using Chempoint.GP.Infrastructure.Config;
using ChemPoint.GP.DataContracts.AuditLog;
using ChemPoint.GP.Entities.Business_Entities.AuditLog;
using ChemPoint.GP.CPServiceAuditLog.Utils;
using Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider.Context;

namespace ChemPoint.GP.CPServiceAuditLog
{
    public class XrmAuditLog : RepositoryBase, IXrmAuditLog
    {
        public XrmAuditLog(string connectionString) : base(new GpAddInDbContext(connectionString))
        {
        }

        public object UpdateXrmIntegrationsAuditLog(AuditLogEntity auditLogEntity)
        {
            DataTable auditLogDT = DataTableMapper.GetDataTable(new List<AuditLogEntity>() { auditLogEntity },
                DataColumnMappings.SaveAuditLog);

            var cmd = CreateStoredProcCommand(Configuration.SPUpdateXrmIntegrationsAuditLog);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateXrmIntegrationsAuditLogParam1, SqlDbType.Structured, auditLogDT, 21);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateXrmIntegrationsAuditLogParam2, SqlDbType.Int, auditLogEntity.Company);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateXrmIntegrationsAuditLogParam3, SqlDbType.VarChar, auditLogEntity.UserID, 21);
            return base.Insert(cmd);
        }
    }
}
