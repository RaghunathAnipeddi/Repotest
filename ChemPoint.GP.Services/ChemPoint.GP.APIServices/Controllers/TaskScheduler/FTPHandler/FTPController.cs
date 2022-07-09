using Chempoint.GP.FtpBL;
using Chempoint.GP.Model.Interactions.FTP;
using ChemPoint.GP.BusinessContracts.TaskScheduler.FTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;
using Chempoint.GP.Infrastructure.Logging;
namespace ChemPoint.GP.APIServices.Controllers.TaskScheduler.FTPHandler
{
    public class FTPController : ApiController
    {
        private IFtpBL ftpService = null;
        private StringBuilder logMessage = new StringBuilder();
        public FTPController()
        {
            ftpService = new FtpBL();
        }
       
        /// <summary>
        /// Uploading Files to FTP in zip format
        /// </summary>
        /// <param name="objFTPRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage FilesUploadToFtpWithZip(FTPRequest objFTPRequest)
        {
           
            HttpStatusCode objHttpStatusCode = HttpStatusCode.NotFound;
            FTPResponse objFTPResponse = new FTPResponse();
            try
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - FilesUploadToFtpWithZip API method in Chempoint.GP.APIServices is started");
                objFTPRequest.logMessage = logMessage;
                objFTPResponse = ftpService.FilesUploadToFtpWithZip(objFTPRequest);
                logMessage = objFTPResponse.logMessage;
              
                if (objFTPResponse.ErrorStatus == Chempoint.GP.Infrastructure.Config.Configuration.FtpSuccess)
                {
                    objHttpStatusCode = HttpStatusCode.OK;
                    logMessage.AppendLine(DateTime.Now.ToString() + " - FilesUploadToFtpWithZip methods Response is Success");

                }
                else
                {
                    objHttpStatusCode = HttpStatusCode.NotFound;
                    logMessage.AppendLine(DateTime.Now.ToString() + " - FilesUploadToFtpWithZip methods Response is Failure");
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Error Description " + objFTPResponse.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - FilesUploadToFtpWithZip API method in Chempoint.GP.APIServices ended.");
                objFTPResponse.TempString = logMessage.ToString();
            }
            return Request.CreateResponse(objHttpStatusCode, objFTPResponse);
        }


        /// <summary>
        /// Uploading Files individually to FTP 
        /// </summary>
        /// <param name="objFTPRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage FilesUploadToFtpWithOutZip(FTPRequest objFTPRequest)
        {

            HttpStatusCode objHttpStatusCode = HttpStatusCode.NotFound;
            FTPResponse objFTPResponse = new FTPResponse();
            try
            {
                logMessage = objFTPRequest.logMessage;
                logMessage.AppendLine(DateTime.Now.ToString() + " - FilesUploadToFtpWithOutZip API method in Chempoint.GP.APIServices is started");
                objFTPRequest.logMessage = logMessage;
                objFTPResponse = ftpService.FilesUploadToFtpWithOutZip(objFTPRequest);
                logMessage = objFTPResponse.logMessage;
                
                if (objFTPResponse.ErrorStatus == Chempoint.GP.Infrastructure.Config.Configuration.FtpSuccess)
                {
                    objHttpStatusCode = HttpStatusCode.OK;
                    logMessage.AppendLine(DateTime.Now.ToString() + " - FilesUploadToFtpWithOutZip methods Response is Success");
                }
                else
                {
                    objHttpStatusCode = HttpStatusCode.NotFound;
                    logMessage.AppendLine(DateTime.Now.ToString() + " - FilesUploadToFtpWithOutZip methods Response is Failure");
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Error Description " + objFTPResponse.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
            }
            finally
            {
                
                logMessage.AppendLine(DateTime.Now.ToString() + " - FilesUploadToFtpWithOutZip API method in Chempoint.GP.APIServices ended.");
                objFTPResponse.TempString = logMessage.ToString();
            }
            return Request.CreateResponse(objHttpStatusCode, objFTPResponse);

        }


        /// <summary>
        /// Download files from FTP
        /// </summary>
        /// <param name="objFTPRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage DownloadFilesFromFtp(FTPRequest objFTPRequest)
        {
            HttpStatusCode objHttpStatusCode = HttpStatusCode.NotFound;
            FTPResponse objFTPResponse = new FTPResponse();
            try
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - DownloadFilesFromFtp API method in Chempoint.GP.APIServices is started");
                objFTPRequest.logMessage = logMessage;
                objFTPResponse = ftpService.DownloadFilesFromFtp(objFTPRequest);
                logMessage = objFTPResponse.logMessage;
              
                if (objFTPResponse.ErrorStatus == Chempoint.GP.Infrastructure.Config.Configuration.FtpSuccess)
                {
                    objHttpStatusCode = HttpStatusCode.OK;
                    logMessage.AppendLine(DateTime.Now.ToString() + " - FilesUploadToFtpWithZip methods Response is Success");
                }
                else
                {
                    objHttpStatusCode = HttpStatusCode.NotFound;
                    logMessage.AppendLine(DateTime.Now.ToString() + " - FilesUploadToFtpWithZip methods Response is Failure");
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Error Description " + objFTPResponse.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - DownloadFilesFromFtp API method in Chempoint.GP.APIServices ended.");
                objFTPResponse.TempString = logMessage.ToString();
            }
            return Request.CreateResponse(objHttpStatusCode, objFTPResponse);
        }

        /// <summary>
        /// Uploading Files individually to FTP 
        /// </summary>
        /// <param name="objFTPRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage GetUploadReportConfigDetails(FTPRequest objFTPRequest)
        {

            HttpStatusCode objHttpStatusCode = HttpStatusCode.NotFound;
            FTPResponse objFTPResponse = new FTPResponse();
            try
            {
                logMessage = objFTPRequest.logMessage;
                logMessage.AppendLine(DateTime.Now.ToString() + " - GetUploadReportConfigDetails API method in Chempoint.GP.APIServices is started");
                objFTPRequest.logMessage = logMessage;
                objFTPResponse = ftpService.GetUploadReportConfigDetails(objFTPRequest);
                logMessage = objFTPResponse.logMessage;
               
                if (objFTPResponse.ErrorStatus == Chempoint.GP.Infrastructure.Config.Configuration.FtpSuccess)
                {
                    objHttpStatusCode = HttpStatusCode.OK;
                    logMessage.AppendLine(DateTime.Now.ToString() + " - GetUploadReportConfigDetails methods Response is Success");
                }
                else
                {
                    objHttpStatusCode = HttpStatusCode.NotFound;
                    logMessage.AppendLine(DateTime.Now.ToString() + " - GetUploadReportConfigDetails methods Response is Failure");
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Error Description " + objFTPResponse.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
            }
            finally
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - GetUploadReportConfigDetails API method in Chempoint.GP.APIServices ended.");
                objFTPResponse.TempString = logMessage.ToString();
            }
            return Request.CreateResponse(objHttpStatusCode, objFTPResponse);

        }

        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage GetResulSet(FTPRequest objFTPRequest)
        {

            HttpStatusCode objHttpStatusCode = HttpStatusCode.NotFound;
            FTPResponse objFTPResponse = new FTPResponse();
            try
            {
                logMessage = objFTPRequest.logMessage;
                logMessage.AppendLine(DateTime.Now.ToString() + " - GetResulSet API method in Chempoint.GP.APIServices is started");
                objFTPRequest.logMessage = logMessage;
                objFTPResponse = ftpService.GetResulSet(objFTPRequest);
                logMessage = objFTPResponse.logMessage;
             
                if (objFTPResponse.ErrorStatus == Chempoint.GP.Infrastructure.Config.Configuration.FtpSuccess)
                {
                    objHttpStatusCode = HttpStatusCode.OK;
                    logMessage.AppendLine(DateTime.Now.ToString() + " - GetResulSet methods Response is Success");

                }
                else
                {
                    objHttpStatusCode = HttpStatusCode.NotFound;
                    logMessage.AppendLine(DateTime.Now.ToString() + " - GetResulSet methods Response is Failure");
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Error Description " + objFTPResponse.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - GetResulSet API method in Chempoint.GP.APIServices ended.");
                objFTPResponse.TempString = logMessage.ToString();
            }
            return Request.CreateResponse(objHttpStatusCode, objFTPResponse);
        }

        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage ZipTheFiles(FTPRequest objFTPRequest)
        {
            HttpStatusCode objHttpStatusCode = HttpStatusCode.NotFound;
            FTPResponse objFTPResponse = new FTPResponse();
            try
            {
                logMessage = objFTPRequest.logMessage;
                logMessage.AppendLine(DateTime.Now.ToString() + " - ZipTheFiles API method in Chempoint.GP.APIServices is started");
                objFTPRequest.logMessage = logMessage;
                objFTPResponse = ftpService.ZipTheFiles(objFTPRequest);
                logMessage = objFTPResponse.logMessage;
               
                if (objFTPResponse.ErrorStatus == Chempoint.GP.Infrastructure.Config.Configuration.FtpSuccess)
                {
                    objHttpStatusCode = HttpStatusCode.OK;
                    logMessage.AppendLine(DateTime.Now.ToString() + " - ZipTheFiles methods Response is Success");

                }
                else
                {
                    objHttpStatusCode = HttpStatusCode.NotFound;
                    logMessage.AppendLine(DateTime.Now.ToString() + " - GetResulSet methods Response is Failure");
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Error Description " + objFTPResponse.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - ZipTheFiles API method in Chempoint.GP.APIServices ended.");
                objFTPResponse.TempString = logMessage.ToString();
            }
            return Request.CreateResponse(objHttpStatusCode, objFTPResponse);
        }
    }
}