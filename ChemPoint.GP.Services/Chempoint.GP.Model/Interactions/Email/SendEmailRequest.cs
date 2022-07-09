using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChemPoint.GP.Entities;
using ChemPoint.GP.Entities.BaseEntities;

namespace Chempoint.GP.Model.Interactions.Email
{
    public class SendEmailRequest
    {
        public EMailInformation EmailInformation { get; set; }
        public string Report { get; set; }
        public string LogPath { get; set; }
        public string FileName { get; set; }
        public string ServiceFileName { get; set; }
        public string LoggingPath { get; set; }
        public string LogFileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public int EmailConfigID { get; set; }
        public string ConnectionString { get; set; }
        public string Source { get; set; }
    }
}
