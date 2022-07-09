using Chempoint.GP.Infrastructure.Config;
using Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider.Context;
using Chempoint.GP.Infrastructure.Maps.Sales;
using Chempoint.GP.Infrastructure.Utils;
using Chempoint.GP.Model.Interactions.Email;
using ChemPoint.GP.DataContracts.GPUtilities;
using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.EmailDL
{
    public class SendEmailDL :RepositoryBase, ISendEmailRepository
    {
        public SendEmailDL(string connectionString)
            : base(new GpAddInDbContext(connectionString))
        {
        }



        public SendEmailResponse GetMailConfiguration(SendEmailRequest sendEmailRequest)
        {
            SendEmailResponse response = new SendEmailResponse();
            EMailInformation emailInfo= new EMailInformation();
            var cmd = CreateStoredProcCommand(Configuration.SPGetEmailConfiguration);
            cmd.Parameters.AddInputParams(Configuration.SPGetEmailConfiguration_param1, SqlDbType.VarChar, sendEmailRequest.EmailConfigID, 21);
            var ds = base.GetDataSet(cmd);
            emailInfo =base.GetEntity<EMailInformation, EmailConfigurationMap>(ds.Tables[0]);
            response.EmailInformation = emailInfo;
            response.Status = ResponseStatusForEmail.Success;
            return response;
        }
    }
}
