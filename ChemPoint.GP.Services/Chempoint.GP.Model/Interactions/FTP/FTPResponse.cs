using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.FTP;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Model.Interactions.FTP
{
    public class FTPResponse
    {
        public int CompanyId { get; set; }
        public StringBuilder logMessage { get; set; }
        public string TempString { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorStatus { get; set; }
        public FTPEntity objFTPEntity { get; set; }
        public List<FTPEntity> objFTPEntityList { get; set; }
        public string ResultSet { get; set; }
        public string Output { get; set; }
    }
}
