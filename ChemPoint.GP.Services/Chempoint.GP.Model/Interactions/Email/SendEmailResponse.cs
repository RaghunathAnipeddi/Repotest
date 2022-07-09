using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Model.Interactions.Email
{
    public enum ResponseStatusForEmail
    {
        Success = 0,
        Error = 1,
        Unknown = 2,
        Custom = 3
    }

    public class SendEmailResponse
    {
        public bool IsMailSent = false;

        public string LogMessage { get; set; }

        public EMailInformation EmailInformation { get; set; }

        public ResponseStatusForEmail Status { get; set; }
    }
}



       
