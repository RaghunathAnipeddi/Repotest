using Chempoint.GP.Model.Interactions.Email;
using ChemPoint.GP.DataContracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.DataContracts.GPUtilities
{
    public interface ISendEmailRepository : IRepository
    {
        SendEmailResponse GetMailConfiguration(SendEmailRequest sendEmailRequest);
    }
}
