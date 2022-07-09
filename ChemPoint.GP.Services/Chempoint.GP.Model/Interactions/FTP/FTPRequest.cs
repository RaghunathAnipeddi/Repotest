using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.FTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Model.Interactions.FTP
{
    public class FTPRequest
    {
        public int CompanyId { get; set; }
        public StringBuilder logMessage { get; set; }
        public FTPEntity objFTPEntity { get; set; }
    }
}
