using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.Entities.Business_Entities.TaskScheduler.FTP
{
    public class FTPEntity
    {
        public int AppId { get; set; }

        public string AppName { get; set; }

        public string Company { get; set; }

        public string BatchFileName { get; set; }

        public string FileName { get; set; }

        public bool IsFileUpload { get; set; }

        public bool Status { get; set; }

        public string FtpAddress { get; set; }

        public string FtpUserName { get; set; }

        public string FtpPassword { get; set; }

        public string FtpUploadPath { get; set; }

        public string FtpDownloadPath { get; set; }

        public string ClientUploadPath { get; set; }

        public string ClientDownloadPath { get; set; }

        //public DateTime CreatedOn { get; set; }

        public long CreatedBy { get; set; }

        public string UserName { get; set; }

        public string LoggingFilePath { get; set; }

        public string LoggingFileName { get; set; }

        public string ArchiveLocation { get; set; }

        public List<string> FilesPathList { get; set; }

        public string UploadFilePath { get; set; }

        public string ScriptName { get; set; }

        public string FormatType { get; set; }

        public bool IsHeaderRequired { get; set; }

        public bool IsFtp { get; set; }

        public bool IsMail { get; set; }

        public bool IsZip { get; set; }

        public int EmailConfigID { get; set; }
        
        public string EmailFrom { get; set; }
       
        public string Signature { get; set; }

        public bool IsDataTableBodyRequired { get; set; }

        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }

    }
}
