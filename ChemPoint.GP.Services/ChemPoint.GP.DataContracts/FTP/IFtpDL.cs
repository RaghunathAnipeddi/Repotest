using Chempoint.GP.Model.Interactions.FTP;
using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.FTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.DataContracts.FTP
{
    public interface IFtpDL
    {
        FTPResponse SaveFtpLog(FTPRequest objFTPRequest);
        FTPResponse FetchFtpDetails(FTPRequest objFTPRequest);
        FTPResponse GetUploadReportConfigDetails(FTPRequest objFTPRequest);
        FTPResponse GetResulSet(FTPRequest objFTPRequest);
    }
}
