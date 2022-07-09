using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChemPoint.GP.BusinessContracts.TaskScheduler.FTP;
using Chempoint.GP.Model.Interactions.FTP;
using System.Data.SqlClient;
using System.Data;
using ChemPoint.GP.DataContracts.FTP;
using Chempoint.GP.FtpDL;
using System.Configuration;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Net;
using Chempoint.GP.Infrastructure.Logging;
using Chempoint.GP.Model.Interactions.Cryptography;
using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.FTP;
using Chempoint.GP.Model.Interactions.Cryptography;
using System.IO.Compression;

namespace Chempoint.GP.FtpBL
{
    public class FtpBL : IFtpBL
    {
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objFTPRequest"></param>
        /// <returns></returns>
        public FTPResponse SaveFtpLog(FTPRequest objFTPRequest)
        {
            
            StringBuilder logMessage = new StringBuilder();
            FTPResponse objFTPResponse = new FTPResponse();
            IFtpDL ftpDL = new FtpDAL(ConfigurationManager.ConnectionStrings["FtpGPTraceConnString"].ToString());
            try
            {
                // objFTPRequest.logMessage.AppendLine("****************************************************************");
                logMessage = objFTPRequest.logMessage;
                logMessage.AppendLine(DateTime.Now.ToString() + " - SaveFtpLog method in Chempoint.GP.FtpBL.FtpBL is started");

                if (objFTPRequest.objFTPEntity != null)
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Invoking the SaveFtpLog method in Chempoint.GP.FtpDL.FtpDAL");
                    objFTPRequest.logMessage = logMessage;
                    objFTPResponse = ftpDL.SaveFtpLog(objFTPRequest);
                    logMessage = objFTPResponse.logMessage;

                } 
                else

                    logMessage.AppendLine(DateTime.Now.ToString() + " - FTPRequest object is null");

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
                objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpFailure;
                objFTPResponse.ErrorMessage = ex.Message;
            }
            finally
            {              
                logMessage.AppendLine(DateTime.Now.ToString() + " - SaveFtpLog method in Chempoint.GP.FtpBL.FtpBL ended.");
                objFTPResponse.logMessage = logMessage;              
            }
            return objFTPResponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public FTPResponse FetchFtpDetails(FTPRequest objFTPRequest)
        {
             
            FTPResponse objFTPResponse = new FTPResponse();
            IFtpDL ftpDL = new FtpDAL(ConfigurationManager.ConnectionStrings["FtpGPCustomizationsConnString"].ToString());
            StringBuilder logMessage = new StringBuilder();
            try
            {

                if (objFTPRequest.objFTPEntity != null)
                {

                    objFTPRequest.logMessage.AppendLine(DateTime.Now.ToString() + " - Invoking the FetchFtpDetails method in Chempoint.GP.FtpDL.FtpDAL");

                    FTPEntity objFTPEntity = new FTPEntity();

                    objFTPResponse = ftpDL.FetchFtpDetails(objFTPRequest);

                    logMessage = objFTPResponse.logMessage;
                    if (objFTPEntity != null)
                    {

                        objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpSuccess;
                        logMessage.AppendLine(DateTime.Now.ToString() + " - FetchFtpDetails is successfully completed");
                    }

                }
                else
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " - FTPRequest object is null");
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
                objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpFailure;
                objFTPResponse.ErrorMessage = ex.Message;
            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - FetchFtpDetails method in Chempoint.GP.FtpBL.FtpBL ended.");
                objFTPResponse.logMessage = logMessage;
            }
            return objFTPResponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objFTPRequest"></param>
        /// <param name="objFilesList"></param>
        /// <returns></returns>
        public FTPResponse FilesUploadToFtpWithZip(FTPRequest objFTPRequest)
        {
            StringBuilder logMessage = new StringBuilder();
            FTPResponse objFTPResponse = new FTPResponse();
            List<string> FilesList = new List<string>();
            try
            {

                logMessage = objFTPRequest.logMessage;
                logMessage.AppendLine(DateTime.Now.ToString() + " - FilesUploadToFtpWithZip method in Chempoint.GP.FtpBL.FtpBL is started");
                if (objFTPRequest.objFTPEntity.AppName != null)
                {
                    objFTPRequest.logMessage = logMessage;
                    objFTPResponse = FetchFtpDetails(objFTPRequest);
                    logMessage = objFTPResponse.logMessage;
                    string date = DateTime.Now.ToString("yyyyMMddHHmmss").Trim();

                    FilesList = objFTPResponse.objFTPEntity.FilesPathList;
                    if (FilesList.Count > 0)
                    {

                        string strFtpUploadFolderPath = objFTPResponse.objFTPEntity.FtpUploadPath;
                        string strUserName = objFTPResponse.objFTPEntity.FtpUserName;

                        
                        objFTPRequest.objFTPEntity.IsFileUpload = true;
                        objFTPRequest.objFTPEntity.Status = false;

                        CryptoRequest objCryptoRequest = new CryptoRequest();
                        objCryptoRequest.CipherText = objFTPResponse.objFTPEntity.FtpPassword;

                        var obj = new Chempoint.GP.CryptoBL.CryptoBL().DecryptPassword(objCryptoRequest);
                        if (!string.IsNullOrEmpty(obj.DecryptedText))
                        {

                            string strPassword = obj.DecryptedText;
                            string strBatchFileName = string.Empty, strZipSourcePath = string.Empty, strFtpFullPath = string.Empty;

                            string strTempPath = objFTPResponse.objFTPEntity.ArchiveLocation;

                            if (!Directory.Exists(strTempPath))
                                Directory.CreateDirectory(strTempPath);

                            //Zipping the files
                            strZipSourcePath = strTempPath;
                            objFTPRequest.objFTPEntity.ClientUploadPath = strTempPath;
                            strBatchFileName = objFTPRequest.objFTPEntity.BatchFileName;
                            strFtpFullPath = strFtpUploadFolderPath + strBatchFileName;
                            #region ZippingFiles

                            // gets the Zip file
                            logMessage.AppendLine(DateTime.Now.ToString() + " - Zipping the files");
                            FileStream ostream = null;
                            byte[] obuffer;
                            ZipOutputStream oZipStream = null;
                            ZipEntry oZipEntry = null;
                            try
                            {


                                // create zip stream
                                strBatchFileName = strBatchFileName + date + ".zip";
                                strZipSourcePath = strZipSourcePath + strBatchFileName;
                                oZipStream = new ZipOutputStream(File.Create(strZipSourcePath));

                                // maximum compression
                                oZipStream.SetLevel(9);
                                // for each file, generate a zip entry
                                foreach (string strFile in FilesList)
                                {
                                    oZipEntry = new ZipEntry(strFile.Substring(strFile.LastIndexOf("\\") + 1));
                                    ostream = File.OpenRead(strFile);
                                    oZipEntry.Size = ostream.Length;
                                    oZipStream.PutNextEntry(oZipEntry);

                                    obuffer = new byte[ostream.Length];
                                    ostream.Read(obuffer, 0, obuffer.Length);
                                    oZipStream.Write(obuffer, 0, obuffer.Length);
                                    oZipStream.CloseEntry();
                                    oZipStream.Flush();
                                    ostream.Flush();
                                    ostream.Close();
                                    ostream.Dispose();
                                    //File.Delete(strFile);
                                }

                                logMessage.AppendLine(DateTime.Now.ToString() + " - Files Zipped Successfully");
                                logMessage.AppendLine(DateTime.Now.ToString() + " - File backup is availble in the following path: " + strZipSourcePath);


                            }
                            catch (Exception)
                            {
                                throw;
                            }
                            finally
                            {

                                if (oZipEntry != null)
                                    oZipEntry = null;

                                if (oZipStream != null)
                                {
                                    oZipStream.Finish();
                                    oZipStream.Flush();
                                    oZipStream.Close();
                                    oZipStream.Dispose();
                                }
                                if (ostream != null)
                                {
                                    ostream.Close();
                                    ostream.Dispose();
                                }

                            }
                            #endregion

                            // Uploads the files to FTP site
                            int intUploadFileCount = 0;
                            if (File.Exists(strZipSourcePath))
                            {

                                logMessage.AppendLine(DateTime.Now.ToString() + " - Total No.of Zip Files to be uploaded to FTP is 1");

                                logMessage.AppendLine(DateTime.Now.ToString() + " - File from this path " + strZipSourcePath + " is started uploading to FTP path" + strFtpFullPath);

                                FtpWebRequest ftpUploadRequest = (FtpWebRequest)FtpWebRequest.Create(strFtpFullPath);
                                ftpUploadRequest.Credentials = new NetworkCredential(strUserName, strPassword);
                                ftpUploadRequest.KeepAlive = true;
                                ftpUploadRequest.UseBinary = true;
                                ftpUploadRequest.Method = WebRequestMethods.Ftp.UploadFile;

                                FileStream uploadedFileStream = File.OpenRead(strZipSourcePath);
                                byte[] uploadedFileBuffer = new byte[uploadedFileStream.Length];
                                uploadedFileStream.Read(uploadedFileBuffer, 0, uploadedFileBuffer.Length);
                                uploadedFileStream.Close();

                                Stream ftpUploadStream = ftpUploadRequest.GetRequestStream();
                                ftpUploadStream.Write(uploadedFileBuffer, 0, uploadedFileBuffer.Length);
                                ftpUploadStream.Close();
                                intUploadFileCount++;
                                logMessage.AppendLine(DateTime.Now.ToString() + " - File is uploaded to " + strFtpFullPath);
                                logMessage.AppendLine(DateTime.Now.ToString() + " - Uploading process is completed");
                                logMessage.AppendLine(DateTime.Now.ToString() + " - Total number of files uploaded to FTP is " + intUploadFileCount);

                                //Status is true when files uploaded successfully
                                objFTPRequest.objFTPEntity.Status = true;
                                objFTPRequest.objFTPEntity.ClientUploadPath = strZipSourcePath;
                                objFTPRequest.objFTPEntity.FtpUploadPath = strFtpFullPath;
                                objFTPRequest.logMessage = logMessage;
                                objFTPResponse = SaveFtpLog(objFTPRequest);
                                logMessage = objFTPResponse.logMessage;
                               
                                objFTPRequest.logMessage.AppendLine(DateTime.Now.ToString() + " - Successfully uploaded the file to FTP site.");
                                objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpSuccess;
                            }
                        }
                        else
                            logMessage.AppendLine(DateTime.Now.ToString() + " - Decryption of ftp password is failed");

                    }
                    else
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Files are not found to upload to FTP");
                }
                else
                    logMessage.AppendLine(DateTime.Now.ToString() + " - FtpRequest object is null");

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
                objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpFailure;
                objFTPResponse.ErrorMessage = ex.Message;
            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - FilesUploadToFtpWithZip method in Chempoint.GP.FtpBL.FtpBL ended.");
                objFTPResponse.logMessage = logMessage;
            }

            return objFTPResponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objFTPRequest"></param>
        /// <param name="objFilesList"></param>
        /// <returns></returns>
        public FTPResponse FilesUploadToFtpWithOutZip(FTPRequest objFTPRequest)
        {
            FTPResponse objFTPResponse = new FTPResponse();
            List<string> FilesList = new List<string>();
            StringBuilder logMessage = new StringBuilder();
            try
            {
                logMessage = objFTPRequest.logMessage;
                objFTPRequest.logMessage.AppendLine(DateTime.Now.ToString() + " - FilesUploadToFtpWithOutZip method in Chempoint.GP.FtpBL.FtpBL is started");
                if (objFTPRequest.objFTPEntity.AppName != null)
                {

                    objFTPResponse = FetchFtpDetails(objFTPRequest);
                    string date = DateTime.Now.ToString("yyyyMMddHHmmss").Trim();

                    FilesList = objFTPRequest.objFTPEntity.FilesPathList;

                    if (FilesList.Count > 0)
                    {

                        string strFtpUploadFolderPath = objFTPResponse.objFTPEntity.FtpUploadPath;
                        string strUserName = objFTPResponse.objFTPEntity.FtpUserName;

                        
                        objFTPRequest.objFTPEntity.IsFileUpload = true;
                        objFTPRequest.objFTPEntity.Status = false;

                        CryptoRequest objCryptoRequest = new CryptoRequest();
                        objCryptoRequest.CipherText = objFTPResponse.objFTPEntity.FtpPassword;
                        objCryptoRequest.logMessage = objFTPRequest.logMessage;
                        var obj = new Chempoint.GP.CryptoBL.CryptoBL().DecryptPassword(objCryptoRequest);
                        if (!string.IsNullOrEmpty(obj.DecryptedText))
                        {

                            string strPassword = obj.DecryptedText;
                            string strBatchFileName = string.Empty, strFtpFullPath = string.Empty;
                            logMessage = obj.logMessage;
                            logMessage.AppendLine(DateTime.Now.ToString() + " - Total No.of Files to be uploaded to FTP is " + FilesList.Count);
                            int intUploadFileCount = 0;
                            foreach (string file in FilesList)
                            {
                                if (File.Exists(file))
                                {
                                    strFtpFullPath = strFtpUploadFolderPath + Path.GetFileName(file);
                                    logMessage.AppendLine(DateTime.Now.ToString() + " - File from this path " + file + " is started uploading to FTP path" + strFtpFullPath);

                                    FtpWebRequest ftpUploadRequest = (FtpWebRequest)FtpWebRequest.Create(strFtpFullPath);
                                    ftpUploadRequest.Credentials = new NetworkCredential(strUserName, strPassword);
                                    ftpUploadRequest.KeepAlive = true;
                                    ftpUploadRequest.UseBinary = true;
                                    ftpUploadRequest.Method = WebRequestMethods.Ftp.UploadFile;

                                    FileStream uploadedFileStream = File.OpenRead(file);
                                    byte[] uploadedFileBuffer = new byte[uploadedFileStream.Length];
                                    uploadedFileStream.Read(uploadedFileBuffer, 0, uploadedFileBuffer.Length);
                                    uploadedFileStream.Close();

                                    Stream ftpUploadStream = ftpUploadRequest.GetRequestStream();
                                    ftpUploadStream.Write(uploadedFileBuffer, 0, uploadedFileBuffer.Length);
                                    ftpUploadStream.Close();
                                    logMessage.AppendLine(DateTime.Now.ToString() + " - File is uploaded to " + strFtpFullPath);

                                    intUploadFileCount++;

                                    //Status is true when files uploaded successfully
                                    objFTPRequest.objFTPEntity.Status = true;
                                    objFTPRequest.objFTPEntity.ClientUploadPath = file;
                                    objFTPRequest.objFTPEntity.FtpUploadPath = strFtpFullPath;
                                    objFTPRequest.objFTPEntity.BatchFileName = Path.GetFileNameWithoutExtension(file);

                                    if (objFTPRequest.objFTPEntity.BatchFileName.Contains("CPUS"))
                                        objFTPRequest.CompanyId = Chempoint.GP.Infrastructure.Config.Configuration.ChmptId;
                                    else if (objFTPRequest.objFTPEntity.BatchFileName.Contains("CPEMEA"))
                                        objFTPRequest.CompanyId = Chempoint.GP.Infrastructure.Config.Configuration.CpeurId;

                                    objFTPRequest.logMessage = logMessage;

                                    objFTPResponse = SaveFtpLog(objFTPRequest);
                                    
                                    logMessage = objFTPResponse.logMessage;

                                    objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpSuccess;
                                }
                            }
                            logMessage.AppendLine(DateTime.Now.ToString() + " - Uploading process is completed");
                            logMessage.AppendLine(DateTime.Now.ToString() + " - Total number of files uploaded to FTP is " + intUploadFileCount);

                        }
                        else
                            logMessage.AppendLine(DateTime.Now.ToString() + " - Decryption of ftp password is failed");

                    }
                    else
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Files are not found to upload to FTP");
                }
                else
                    logMessage.AppendLine(DateTime.Now.ToString() + " - FtpRequest object is null");
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
                objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpFailure;
                objFTPResponse.ErrorMessage = ex.Message;
            }
            finally
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - FilesUploadToFtpWithOutZip method in Chempoint.GP.FtpBL.FtpBL ended.");
                objFTPResponse.logMessage = logMessage;
            }
            return objFTPResponse;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objFTPRequest"></param>
        /// <returns></returns>
        public FTPResponse DownloadFilesFromFtp(FTPRequest objFTPRequest)
        {
            FTPResponse objFTPResponse = new FTPResponse();
            StringBuilder logMessage = new StringBuilder();
             
            try
            {
                logMessage = objFTPRequest.logMessage;
                if (objFTPRequest.objFTPEntity != null)
                {
                    objFTPResponse = FetchFtpDetails(objFTPRequest);
                    if (!Directory.Exists(objFTPResponse.objFTPEntity.ClientDownloadPath))
                    {
                        Directory.CreateDirectory(objFTPResponse.objFTPEntity.ClientDownloadPath);
                    }
                    string strFtpSourcePath = objFTPResponse.objFTPEntity.FtpDownloadPath;
                    string strClientDownloadPath = objFTPResponse.objFTPEntity.ClientDownloadPath;
                    string strUserName = objFTPResponse.objFTPEntity.FtpUserName;
                    logMessage.AppendLine(DateTime.Now.ToString() + " - DownloadFilesFromFtp method in Chempoint.GP.FtpBL.FtpBL is started");

                    string date = DateTime.Now.ToString("yyyyMMddHHmmss").Trim();
                    // Downloads the files
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Downloading the details from FTP site");
                    logMessage.AppendLine(DateTime.Now.ToString() + " - FTP download path = " + strFtpSourcePath);
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Gets the file list from FTP site");


                    objFTPRequest.objFTPEntity.IsFileUpload = false;
                    objFTPRequest.objFTPEntity.Status = false;

                    CryptoRequest objCryptoRequest = new CryptoRequest();
                    objCryptoRequest.CipherText = objFTPResponse.objFTPEntity.FtpPassword;
                    logMessage.AppendLine(DateTime.Now.ToString() + " - DecryptPassword method in Chempoint.GP.CryptoBL.CryptoBL is started");

                    var obj = new Chempoint.GP.CryptoBL.CryptoBL().DecryptPassword(objCryptoRequest);
                    if (!string.IsNullOrEmpty(obj.DecryptedText))
                    {
                        string strPassword = obj.DecryptedText;
                        FtpWebRequest ftpGetFilesRequest = (FtpWebRequest)FtpWebRequest.Create(strFtpSourcePath);
                        ftpGetFilesRequest.Credentials = new NetworkCredential(strUserName, strPassword);
                        ftpGetFilesRequest.KeepAlive = true;
                        ftpGetFilesRequest.UseBinary = true;
                        ftpGetFilesRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                        FtpWebResponse ftpGetFilesResponse = (FtpWebResponse)ftpGetFilesRequest.GetResponse();
                        StreamReader ftpGetFilesReader = new StreamReader(ftpGetFilesResponse.GetResponseStream());

                        string line = ftpGetFilesReader.ReadLine();
                        StringBuilder ftpGetFilesResult = new StringBuilder();
                        while (line != null)
                        {
                            ftpGetFilesResult.Append(line);
                            ftpGetFilesResult.Append("|");
                            line = ftpGetFilesReader.ReadLine();
                        }
                        if (ftpGetFilesResult.Length > 0)
                        {
                            ftpGetFilesResult.Remove(ftpGetFilesResult.ToString().LastIndexOf("|"), 1);
                            ftpGetFilesReader.Close();
                            ftpGetFilesResponse.Close();
                            string[] listFiles = ftpGetFilesResult.ToString().Split('|');
                            logMessage.AppendLine(DateTime.Now.ToString() + " - Successfully retrieves the file list from FTP site.");
                            logMessage.AppendLine(DateTime.Now.ToString() + " - Total files fetched :  " + listFiles.Length);


                            foreach (string strfile in listFiles)
                            {

                                logMessage.AppendLine(DateTime.Now.ToString() + " - Started downloading the file : " + strfile);
                                logMessage.AppendLine(DateTime.Now.ToString() + " - Downloaded File Path : " + strFtpSourcePath);
                                FtpWebRequest ftpDownloadRequest = (FtpWebRequest)FtpWebRequest.Create(strFtpSourcePath + strfile);
                                ftpDownloadRequest.Credentials = new NetworkCredential(strUserName, strPassword);
                                ftpDownloadRequest.KeepAlive = true;
                                ftpDownloadRequest.UseBinary = true;
                                ftpDownloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                                FtpWebResponse ftpDownloadFileResponse = (FtpWebResponse)ftpDownloadRequest.GetResponse();
                                Stream ftpFileDownloadStream = ftpDownloadFileResponse.GetResponseStream();

                                long cl = ftpDownloadFileResponse.ContentLength;
                                int bufferSize = 2048;
                                int readCount;
                                FileStream outputDownloadFileStream = new FileStream(strClientDownloadPath + strfile, FileMode.Create);
                                byte[] fileBuffer = new byte[bufferSize];
                                readCount = ftpFileDownloadStream.Read(fileBuffer, 0, bufferSize);
                                while (readCount > 0)
                                {
                                    outputDownloadFileStream.Write(fileBuffer, 0, readCount);
                                    readCount = ftpFileDownloadStream.Read(fileBuffer, 0, bufferSize);
                                }

                                ftpFileDownloadStream.Close();
                                outputDownloadFileStream.Close();
                                ftpDownloadFileResponse.Close();

                                //Status will be true if files downloaded successfully from ftp
                                objFTPRequest.objFTPEntity.Status = true;
                                objFTPRequest.objFTPEntity.ClientDownloadPath = strClientDownloadPath + strfile;
                                objFTPRequest.objFTPEntity.FtpDownloadPath = strFtpSourcePath + strfile;
                                objFTPRequest.objFTPEntity.BatchFileName = Path.GetFileNameWithoutExtension(strfile);

                                objFTPResponse = SaveFtpLog(objFTPRequest);

                                logMessage.AppendLine(DateTime.Now.ToString() + " - Succussfully downloaded the file : " + strfile);
                                objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpSuccess;

                            }

                        }
                        else
                        {
                            objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpFailure;
                            objFTPResponse.ErrorMessage = "Files are not available in " + strFtpSourcePath;
                            logMessage.AppendLine(DateTime.Now.ToString() + " - Files are not available in " + strFtpSourcePath);
                        }
                    }
                    else
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Decryption of ftp password is failed");


                }
                else
                    logMessage.AppendLine(DateTime.Now.ToString() + " - FtpRequest object is null");
            }
            catch (Exception ex)
            {
                objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpFailure;
                objFTPResponse.ErrorMessage = ex.Message;
                if (ex.Message.ToUpper().Contains("FILE UNAVAILABLE"))
                    logMessage.AppendLine(DateTime.Now.ToString() + " - No files exists to download.");
                else
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Error while downloading files. Error : " + ex.Message);
            }
            finally
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - UploadFilesToFtp method in Chempoint.GP.FtpBL.FtpBL ended.");
                objFTPResponse.logMessage = logMessage;
            }
            return objFTPResponse;
        }

        /// <summary>
        /// To get the Report configuaration details using App Name
        /// </summary>
        /// <param name="objFTPRequest"></param>
        /// <returns></returns>
        public FTPResponse GetUploadReportConfigDetails(FTPRequest objFTPRequest)
        {
            FTPResponse objFTPResponse = new FTPResponse();
            IFtpDL ftpDL = new FtpDAL(ConfigurationManager.ConnectionStrings["FtpGPCustomizationsConnString"].ToString());
            StringBuilder logMessage = new StringBuilder();
            try
            {
                logMessage = objFTPRequest.logMessage;

                if (objFTPRequest.objFTPEntity != null)
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Invoking the GetUploadReportConfigDetails method in Chempoint.GP.FtpDL.FtpDAL");

                    FTPEntity objFTPEntity = new FTPEntity();

                    objFTPRequest.logMessage = logMessage;
                    objFTPResponse = ftpDL.GetUploadReportConfigDetails(objFTPRequest);

                    if (objFTPResponse != null)
                    {
                        objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpSuccess;
                    }
                }
                else
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " - FTPRequest object is null");
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
                objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpFailure;
                objFTPResponse.ErrorMessage = ex.Message;
            }
            finally
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - GetUploadReportConfigDetails method in Chempoint.GP.FtpBL.FtpBL ended.");
                logMessage = new StringBuilder();
                logMessage.Append(objFTPResponse.logMessage);
            }
            return objFTPResponse;
        }

        public FTPResponse GetResulSet(FTPRequest objFTPRequest)
        {
            FTPResponse objFTPResponse = new FTPResponse();
            IFtpDL ftpDL = new FtpDAL(ConfigurationManager.ConnectionStrings["FtpGPCustomizationsConnString"].ToString());
            StringBuilder logMessage = new StringBuilder();
            try
            {

                logMessage = objFTPRequest.logMessage;
                
                logMessage.AppendLine(DateTime.Now.ToString() + " - GetResulSet method in Chempoint.GP.FtpBL.FtpBL is started");


                if (objFTPRequest.objFTPEntity != null)
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Invoking the GetResulSet method in Chempoint.GP.FtpDL.FtpDAL");

                    FTPEntity objFTPEntity = new FTPEntity();

                    objFTPRequest.logMessage = logMessage;

                    objFTPResponse = ftpDL.GetResulSet(objFTPRequest);

                    logMessage = objFTPResponse.logMessage;
                    if (objFTPResponse != null)
                    {
                        objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpSuccess;
                    }
                }
                else
                {
                    objFTPRequest.logMessage.AppendLine(DateTime.Now.ToString() + " - FTPRequest object is null");
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
                objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpFailure;
                objFTPResponse.ErrorMessage = ex.Message;
            }
            finally
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - GetResulSet method in Chempoint.GP.FtpBL.FtpBL ended.");
                objFTPResponse.logMessage = logMessage;
            }
            return objFTPResponse;
        }

        public FTPResponse ZipTheFiles(FTPRequest objFTPRequest)
        {

            FileStream ostream = null;
            byte[] obuffer;
            ZipOutputStream oZipStream = null;
            ZipEntry oZipEntry = null;
            FTPResponse objFTPResponse = new FTPResponse();
            string strFileName = string.Empty, strZipSourcePath = string.Empty, strFtpFullPath = string.Empty;
            StringBuilder logMessage = new StringBuilder();
            try
            {

                logMessage = objFTPRequest.logMessage;

                logMessage.AppendLine(DateTime.Now.ToString() + " - ZipTheFiles method in Chempoint.GP.FtpBL.FtpBL is started");

                logMessage.AppendLine(DateTime.Now.ToString() + " - Started the Zipping the files");
                string date = DateTime.Now.ToString("yyyyMMddHHmmss").Trim();

                // create zip stream
                strFileName = objFTPRequest.objFTPEntity.FileName + date + ".zip";
                strZipSourcePath = objFTPRequest.objFTPEntity.ArchiveLocation + strFileName;
                oZipStream = new ZipOutputStream(File.Create(strZipSourcePath));

                // maximum compression
                oZipStream.SetLevel(9);
                // for each file, generate a zip entry
                foreach (string strFile in objFTPRequest.objFTPEntity.FilesPathList)
                {
                    oZipEntry = new ZipEntry(strFile.Substring(strFile.LastIndexOf("\\") + 1));
                    ostream = File.OpenRead(strFile);
                    oZipEntry.Size = ostream.Length;
                    oZipStream.PutNextEntry(oZipEntry);

                    obuffer = new byte[ostream.Length];
                    ostream.Read(obuffer, 0, obuffer.Length);
                    oZipStream.Write(obuffer, 0, obuffer.Length);
                    oZipStream.CloseEntry();
                    oZipStream.Flush();
                    ostream.Flush();
                    ostream.Close();
                    ostream.Dispose();
                }
                objFTPResponse.Output = strZipSourcePath;
                logMessage.AppendLine(DateTime.Now.ToString() + " - Files Zipped Successfully");
                logMessage.AppendLine(DateTime.Now.ToString() + " - File backup is availble in the following path: " + strZipSourcePath);
                objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpSuccess;

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
                objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpFailure;
                objFTPResponse.ErrorMessage = ex.Message;
            }
            finally
            {
                if (oZipEntry != null)
                    oZipEntry = null;

                if (oZipStream != null)
                {
                    oZipStream.Finish();
                    oZipStream.Flush();
                    oZipStream.Close();
                    oZipStream.Dispose();
                }
                if (ostream != null)
                {
                    ostream.Close();
                    ostream.Dispose();
                }
                logMessage.AppendLine(DateTime.Now.ToString() + " - GetUploadReportConfigDetails method in Chempoint.GP.FtpBL.FtpBL ended.");
                objFTPResponse.logMessage = logMessage;
            }
            return objFTPResponse;
        }

        #region PayableService

        public FTPResponse GenerateZipFile(FTPRequest objFTPRequest)
        {
            StringBuilder logMessage = new StringBuilder();
            FTPResponse objFTPResponse = null;
            try
            {
                objFTPResponse = new FTPResponse();

                List<string> list = new List<string>();
                string outputPathAndFile = objFTPRequest.objFTPEntity.LoggingFilePath + objFTPRequest.objFTPEntity.FileName;
                // add each file in directory
                using (ZipArchive zipfile = System.IO.Compression.ZipFile.Open(outputPathAndFile, ZipArchiveMode.Create))
                {
                    foreach (string file in Directory.GetFiles(objFTPRequest.objFTPEntity.LoggingFilePath))
                    {
                        if (file.EndsWith(".txt"))
                        {

                            zipfile.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Fastest);
                            File.Delete(file);
                        }
                    }
                }
                logMessage.AppendLine("File backup is availble in the following path: " + objFTPRequest.objFTPEntity.LoggingFilePath + objFTPRequest.objFTPEntity.FileName);


                logMessage.AppendLine(DateTime.Now.ToString() + " - Files Zipped Successfully");
                objFTPResponse.logMessage = logMessage;
                objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpSuccess;

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
                objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpFailure;
                objFTPResponse.logMessage = logMessage;
                objFTPResponse.ErrorMessage = ex.Message;
            }

            return objFTPResponse;
        }

        /// <summary>
        /// Moving files to the temp folder
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="zipFileName"></param>
        /// <returns></returns>
        public FTPResponse MoveZipFileToTargetFolder(FTPRequest objFTPRequest)
        {
            StringBuilder logMessage = new StringBuilder();
            FTPResponse objFTPResponse = null;
            try
            {
                objFTPResponse = new FTPResponse();

                // Use Path class to manipulate file and directory paths. 
                string sourceFile = Path.Combine(objFTPRequest.objFTPEntity.SourcePath, objFTPRequest.objFTPEntity.FileName);
                string destFile = Path.Combine(objFTPRequest.objFTPEntity.DestinationPath, objFTPRequest.objFTPEntity.FileName);

                // Create a new target folder, if necessary. 
                if (!Directory.Exists(objFTPRequest.objFTPEntity.DestinationPath))
                {
                    Directory.CreateDirectory(objFTPRequest.objFTPEntity.DestinationPath);
                }
                // To move a file or folder to a new location:
                //File.Move(sourceFile, destFile);
                File.Copy(sourceFile, destFile);
                logMessage.AppendLine(" Zip file backup is availble in the following path: " + objFTPRequest.objFTPEntity.DestinationPath + objFTPRequest.objFTPEntity.FileName);
                objFTPResponse.logMessage = logMessage;
                objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpSuccess;

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
                objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpFailure;
                objFTPResponse.logMessage = logMessage;
                objFTPResponse.ErrorMessage = ex.Message;
            }

            return objFTPResponse;
        }

        //#region CTSI
        public FTPResponse UploadCtsiZipFilesToFtp(FTPRequest objFTPRequest)
        {
            StringBuilder logMessage = new StringBuilder();
            FTPResponse objFTPResponse = null;
            try
            {
                objFTPResponse = new FTPResponse();

                string ftpInFolderPath = objFTPRequest.objFTPEntity.FtpUploadPath;
                logMessage.AppendLine(" Uploading the Zip files to FTP upload path " + objFTPRequest.objFTPEntity.ClientUploadPath + "/" + ftpInFolderPath + "/");
                Chempoint.GP.FtpBL.CtsiFTPLibrary.FtpClient cl = new Chempoint.GP.FtpBL.CtsiFTPLibrary.FtpClient();
                cl.Host = objFTPRequest.objFTPEntity.ClientUploadPath;
                cl.Credentials = new NetworkCredential(objFTPRequest.objFTPEntity.FtpUserName, objFTPRequest.objFTPEntity.FtpPassword);
                cl.Connect();

                string wd = cl.GetWorkingDirectory();
                cl.SetWorkingDirectory("/" + ftpInFolderPath);

                string fileName = objFTPRequest.objFTPEntity.SourcePath + objFTPRequest.objFTPEntity.FileName;
                FileInfo fileInf = new FileInfo(fileName);
                Stream requestStream = cl.OpenWrite(fileInf.Name, Chempoint.GP.FtpBL.CtsiFTPLibrary.FtpDataType.Binary);

                FileStream sourceStream = File.OpenRead(fileName);
                byte[] uploadedFileBuffer = new byte[sourceStream.Length];
                sourceStream.Read(uploadedFileBuffer, 0, uploadedFileBuffer.Length);
                sourceStream.Close();

                requestStream.Write(uploadedFileBuffer, 0, uploadedFileBuffer.Length);
                requestStream.Close();

                cl.Disconnect();
                logMessage.AppendLine(" Successfully uploaded the file '" + objFTPRequest.objFTPEntity.FileName + "' to FTP site");

                objFTPResponse.logMessage = logMessage;
                objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpSuccess;

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
                objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpFailure;
                objFTPResponse.logMessage = logMessage;
                objFTPResponse.ErrorMessage = ex.Message;
            }

            return objFTPResponse;
        }

        public FTPResponse DownloadCtsiZipFilesFromFtp(FTPRequest objFTPRequest)
        {
            StringBuilder logMessage = new StringBuilder();
            FTPResponse objFTPResponse = null;
            try
            {
                objFTPResponse = new FTPResponse();

                logMessage.AppendLine(" Scanning for the files available in FTP download path " + objFTPRequest.objFTPEntity.ClientDownloadPath + "/" + objFTPRequest.objFTPEntity.FtpUploadPath);

                Chempoint.GP.FtpBL.CtsiFTPLibrary.FtpClient ftpClient = new Chempoint.GP.FtpBL.CtsiFTPLibrary.FtpClient();
                ftpClient.Host = objFTPRequest.objFTPEntity.ClientDownloadPath;
                ftpClient.Credentials = new NetworkCredential(objFTPRequest.objFTPEntity.FtpUserName, objFTPRequest.objFTPEntity.FtpPassword);
                ftpClient.Connect();
                ftpClient.SetWorkingDirectory("/" + objFTPRequest.objFTPEntity.FtpUploadPath);
                string[] files = ftpClient.GetNameListing();
                if (files.Length > 0)
                {
                    foreach (string ef in files)
                    {
                        if (ef.ToLower().StartsWith(objFTPRequest.objFTPEntity.FileName.ToLower()))
                        // if(ef.ToLower().Remove(0, 5).StartsWith(fileNameStarts.ToLower())) // for aspire FTP
                        {
                            string fileName = ef.Trim();
                            //string fileName = ef.Remove(0, 5); // for aspire FTP
                            logMessage.AppendLine(" Started downloading the file : " + fileName);

                            string outputfile = objFTPRequest.objFTPEntity.UploadFilePath + fileName;
                            FileStream fsout = new FileStream(outputfile, FileMode.Create);
                            Stream ftpFileStream = ftpClient.OpenRead(fileName, Chempoint.GP.FtpBL.CtsiFTPLibrary.FtpDataType.Binary);
                            ftpFileStream.CopyTo(fsout);
                            ftpFileStream.Close();
                            fsout.Close();
                            logMessage.AppendLine(" Successfully downloaded and the file is now available in : " + objFTPRequest.objFTPEntity.UploadFilePath + fileName);

                            // Deletes the files in FTP Site
                            logMessage.AppendLine(" Started deleting the file from FTP : " + objFTPRequest.objFTPEntity.ClientDownloadPath + "/" + objFTPRequest.objFTPEntity.FtpUploadPath + "/" + fileName);
                            ftpClient.DeleteFile(fileName);
                            logMessage.AppendLine(" Successfully deleted the file : " + fileName);
                        }
                        else
                            logMessage.AppendLine(" Not a valid file name");
                    }
                }
                else
                {
                    logMessage.AppendLine(" No files exists to download.");
                }
                ftpClient.Disconnect();


                objFTPResponse.logMessage = logMessage;
                objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpSuccess;

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
                objFTPResponse.ErrorStatus = Chempoint.GP.Infrastructure.Config.Configuration.FtpFailure;
                objFTPResponse.logMessage = logMessage;
                objFTPResponse.ErrorMessage = ex.Message;
            }

            return objFTPResponse;
        }
        //#endregion

        #endregion



    }
}
