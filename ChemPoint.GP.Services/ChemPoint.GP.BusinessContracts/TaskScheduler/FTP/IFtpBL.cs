using Chempoint.GP.Model.Interactions.FTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.BusinessContracts.TaskScheduler.FTP
{
    public interface IFtpBL
    {

        FTPResponse FilesUploadToFtpWithZip(FTPRequest objFTPRequest);

        FTPResponse FilesUploadToFtpWithOutZip(FTPRequest objFTPRequest);

        FTPResponse DownloadFilesFromFtp(FTPRequest objFTPRequest);

        FTPResponse GetUploadReportConfigDetails(FTPRequest objFTPRequest);

        FTPResponse GetResulSet(FTPRequest objFTPRequest);

        FTPResponse ZipTheFiles(FTPRequest objFTPRequest);

        #region PaybaleService
        FTPResponse GenerateZipFile(FTPRequest objFTPRequest);
        //FTPResponse UploadCtsiZipFilesToFtp(FTPRequest objFTPRequest);
        #endregion
    }
}
