using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class EMailInformation : IModelBase
    {
        public string EmailFrom { get; set; }

        public string EmailTo { get; set; }

        public string EmailBcc { get; set; }

        public string EmailCc { get; set; }

        public string Body { get; set; }

        public string Subject { get; set; }

        public bool HasAttachment { get; set; }

        public string SmtpAddress { get; set; }

        public string UserId { get; set; }

        public string Password { get; set; }

        public string Signature { get; set; }

        public bool IsDataTableBodyRequired { get; set; }
    }
}
