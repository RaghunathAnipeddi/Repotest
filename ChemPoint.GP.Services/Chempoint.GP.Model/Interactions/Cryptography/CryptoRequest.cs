using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.FTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Model.Interactions.Cryptography
{
    public class CryptoRequest
    {
        public StringBuilder logMessage { get; set; }
        public FTPEntity objFTPEntity { get; set; }
        public string CipherText { get; set; }
    }
}
