using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.FTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Model.Interactions.Cryptography
{
    public class CryptoResponse
    {
        public StringBuilder logMessage { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorStatus { get; set; }
        public string DecryptedText { get; set; }
        public FTPEntity objFTPEntity { get; set; }
    }
}
