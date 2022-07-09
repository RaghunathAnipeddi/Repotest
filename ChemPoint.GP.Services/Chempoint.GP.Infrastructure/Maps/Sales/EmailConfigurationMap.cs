using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.BaseEntities;
using ChemPoint.GP.Entities.Business_Entities.Sales;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Infrastructure.Maps.Sales
{
    public class EmailConfigurationMap : BaseDataTableMap<EMailInformation>
    {
        public override EMailInformation Map(DataRow dr)
        {
            EMailInformation emailInfo = new EMailInformation();
            emailInfo.EmailTo = dr["Recipients"].ToString();
            emailInfo.EmailCc = dr["CopyRecipients"].ToString();
            emailInfo.EmailBcc = dr["BlindCopyRecipients"].ToString();
            emailInfo.Subject = dr["Subject"].ToString();
            //emailInfo.HasAttachment = false;
            emailInfo.HasAttachment = Convert.ToBoolean(dr["AttachQueryResultAsFile"]);
            emailInfo.Subject = dr["Subject"].ToString();
            emailInfo.Body = dr["Body"].ToString();
            return emailInfo;
        }
    }
}
 