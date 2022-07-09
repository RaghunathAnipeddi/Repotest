using System;

namespace Chempoint.GP.Model.Interactions.AuditLog
{
    public enum ResponseStatus
    {
        Success = 0,
        Error = 1,
        Unknown = 2,
        Custom = 3
    }

    public class XrmAuditLogResponse
    {
        public ResponseStatus Status { get; set; }

        public string ErrorMessage { get; set; }
    }
}
