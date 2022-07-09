using Chempoint.GP.Model.Interactions.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.BusinessContracts.Email
{
    public interface ISendEmail
    {
        SendEmailResponse SendEmail(SendEmailRequest request);
    }
}
