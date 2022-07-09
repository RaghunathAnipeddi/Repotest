using Chempoint.GP.Model.Interactions.Email;
using Chempoint.GP.Model.Interactions.PayableManagement;
using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace ChemPoint.GP.CTSIFileGenerator
{
    /// <PayableManagement>
    /// Project Name        :   payable mangement migration
    /// Affected Module     :   Purchase
    /// Affected Windows    :   GP to CTSI task scheduler.
    /// Developed on        :   April2017
    /// Developed by        :   Muthu and Nagaraj
    ///-------------------------------------------------------------------------------------------
    ///     Date		Author          Description
    ///-------------------------------------------------------------------------------------------
    /// **Apr2017       Mthangaraj      Initial Creation.
    /// </PayableManagement>
    public class CTSIFileGenerator
    {
        private string zipFileName = string.Empty;
        private bool isZipFileGenerated = false;
        private bool isZipFileMoved = false;
        private int fileCount = 0;
        private string path = string.Empty;
        private const string APP_NAME = "CTSI";
        public static StringBuilder strLogMsg = new StringBuilder();
        private static StringBuilder _logMessage = new StringBuilder();
        static string date = DateTime.Now.ToString("MMddyyyyHHmmss").Trim();
        public static StringBuilder LogMessage
        {
            get
            {
                return _logMessage;
            }
            set
            {
                _logMessage = value;
            }
        }
        private string ftpPath = ConfigurationManager.AppSettings["FTPPath"].ToString().Trim();
        private string userName = ConfigurationManager.AppSettings["FTPUserName"].ToString().Trim();
        private string password = ConfigurationManager.AppSettings["FTPPassword"].ToString().Trim();
        private static int retryCount = 0;
        
        private static void Main(string[] args)
        {
            CTSIFileGenerator processtoCTSI = new CTSIFileGenerator();
            try
            {
                

                int.TryParse(ConfigurationManager.AppSettings["RetryCount"].ToString().Trim(), out retryCount);
                // Getting company name from App.config
                processtoCTSI.ProcessTheFilesToCtsi(args, processtoCTSI, ConfigurationManager.AppSettings["NA"].ToUpper().Trim());
                processtoCTSI.ProcessTheFilesToCtsi(args, processtoCTSI, ConfigurationManager.AppSettings["EU"].ToUpper().Trim());
                // Log the details into file.
                LogInformationToFile(ConfigurationManager.AppSettings["GPToCTSILoggingPath"],
                    LogMessage.ToString().Trim(), ConfigurationManager.AppSettings["GPToCTSILogFileName"]
                    , 2);
                LogMessage = new StringBuilder();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                //new TextLogger().LogInformationIntoFile(logMessage.ToString(), strLogPath, strLogFileName);
            }
        }



        /// <summary>
        /// This Method is used to upload the files from GP to CTSI for both NA and EU.
        /// </summary>
        /// <param name="args">This is a String Array Field will hold Date values.</param>
        /// <param name="processtoCTSI">This is an instance of ProcesstoCtsi class.</param>
        /// <param name="objHelper">This is an instance of Helper class.</param>
        /// <param name="company">This is a string field will hold the company name.</param>
        private void ProcessTheFilesToCtsi(string[] args, CTSIFileGenerator processtoCTSI, string company)
        {
            AppendLogMessageWithoutDateTime("================================================================================================================================");

            AppendLogMessage(string.Concat(company, " ", " Execution Process Started."));

            AppendLogMessage(string.Concat(company, " ", " Retry Count - " + retryCount.ToString().Trim()));
            if (args.Length == 0)
            {
                DateTime date = DateTime.Now;
                int daysCount = 0;

                //Gets the number of days to Get CTSI files   
                int.TryParse(ConfigurationManager.AppSettings["DaysCount"], out daysCount);
                for (int rowindex = 0; rowindex < daysCount; rowindex++)
                {
                    processtoCTSI.GetCtsiFiles(company, date.AddDays(-rowindex));
                }
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(args[i]) == false) processtoCTSI.GetCtsiFiles(company, Convert.ToDateTime(args[i]));
                }
            }

            AppendLogMessage(string.Concat(company, " ", " Execution Process Completed."));

            AppendLogMessage("================================================================================================================================");
        }


        /// <summary>
        /// Call API Zip File Creation Method
        /// </summary>
        //public void GetCtsiFiles(string company, DateTime processingDate)
        //{
        //    bool isSuccessfullyCreated = false;
        //    int fileUploadCount = 0;
        //    int fileGenerateCount = 0;
        //    path = ConfigurationManager.AppSettings["TempFolderPath"];
        //    PayableManagementResponse objResponse = null;
        //    try
        //    {
        //        string gpServiceConfigurationUrl = ConfigurationManager.AppSettings["ServiceURL"].ToString();
        //        objResponse = new PayableManagementResponse();

        //        //Fetch transaction details and generate zip files
        //        while (fileUploadCount < retryCount)
        //        {
        //            try
        //            {
        //                if (!isSuccessfullyCreated)
        //                {
        //                    //isSuccessfullyCreated = GenerateFilesToBeUploaded(company, processingDate, objHelper);
        //                    using (HttpClient client = new HttpClient())
        //                    {
        //                        client.Timeout = TimeSpan.FromMinutes(10);
        //                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
        //                        client.DefaultRequestHeaders.Accept.Clear();
        //                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //                        PayableManagementRequest objRequest = new PayableManagementRequest();
        //                        objRequest.Company = company;
        //                        if (company == "CHMPT")
        //                            objRequest.companyId = 1;
        //                        else
        //                            objRequest.companyId = 2;
        //                        objRequest.processingDate = processingDate;

        //                        var response = client.PostAsJsonAsync("api/PayableManagement/GenerateCtsiFilesToBeUploaded", objRequest); // we need to refer the web.api service url here.
        //                        if (response.Result.IsSuccessStatusCode)
        //                        {
        //                            objResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
        //                            if (objResponse.Status == Response.Success)
        //                            {
        //                                //isStatus = true;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            //isStatus = false;
        //                        }
        //                    }
        //                    if (objResponse != null)
        //                    {
        //                        AppendLogMessage(string.Concat(company, " ", objResponse.LogFileMessage.ToString().Trim()));
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                AppendLogMessage("Error while generating zip files" + "\n" + "Error : " + ex.Message);
        //                AppendLogMessage(ex.StackTrace);

        //                if (fileGenerateCount > 0)
        //                {
        //                    AppendLogMessage(" " + fileGenerateCount + " Retry call ended to retrieve documents and generate zip files");
        //                }
        //                AppendLogMessage("----------------------------------------------------------------------------------------------------------------------------------------------------------------------");

        //                if (fileGenerateCount == retryCount - 1)
        //                {
        //                    AppendLogMessage(" Sending failure mail.");
        //                    SendReport((" <b>Error occurred while generating zip files for the date " + processingDate.ToShortDateString() + "</b>" + "<br><br>" + " <b>Error Description:</b> " + "<br>" + ex.Message.Trim() + "<br>" + " <b>Error Details:</b> " + "<br>" + ex.StackTrace));
        //                    {
        //                        objHelper.AppendLogMessage(" Mail has been sent successfully");
        //                        objHelper.AppendLogMessage("----------------------------------------------------------------------------------------------------------------------------------------------------------------------");
        //                    }
        //                else
        //                {
        //                        objHelper.AppendLogMessage(" Mail is not sent successfully");
        //                        objHelper.AppendLogMessage("----------------------------------------------------------------------------------------------------------------------------------------------------------------------");
        //                    }
        //                }

        //                AppendLogMessage(" Sending failure mail.");
        //                LogMessage.AppendLine(strLogMsg.ToString());

        //                fileGenerateCount++;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        AppendLogMessage(ex.Message);
        //        AppendLogMessage(ex.StackTrace);
        //        AppendLogMessage("----------------------------------------------------------------------------------------------------------------------------------------------------------------------");
        //    }
        //}


        public void GetCtsiFiles(string company, DateTime processingDate)
        {
            bool isSuccessfullyCreated = false;
            int fileUploadCount = 0;
            int fileGenerateCount = 0;
            path = ConfigurationManager.AppSettings["TempFolderPath"];
            PayableManagementResponse objResponse = null;
            try
            {
                AppendLogMessage("*************************************************************************************************************************************");
                AppendLogMessage(" Chempoint Ctsi File Generator Started For " + company + " - Processing Date: " + processingDate.ToShortDateString());
                string gpServiceConfigurationUrl = ConfigurationManager.AppSettings["ServiceURL"].ToString();
                objResponse = new PayableManagementResponse();

                //Fetch transaction details and generate zip files
                //while (fileGenerateCount < retryCount)
                //{
                    try
                    {
                        if (!isSuccessfullyCreated)
                        {
                            if (fileGenerateCount > 0)
                                AppendLogMessage(" " + fileGenerateCount + " Retry call started to retrieve documents and generate zip files");
                            //isSuccessfullyCreated = GenerateFilesToBeUploaded(company, processingDate, objHelper);
                            using (HttpClient client = new HttpClient())
                            {
                                client.Timeout = TimeSpan.FromMinutes(10);
                                client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                PayableManagementRequest objRequest = new PayableManagementRequest();
                                objRequest.Company = company;
                                if (company == "CHMPT")
                                    objRequest.companyId = 1;
                                else
                                    objRequest.companyId = 2;


                            objRequest.processingDate = processingDate;

                                var response = client.PostAsJsonAsync("api/PayableManagement/GenerateCtsiFilesToBeUploaded", objRequest); // we need to refer the web.api service url here.
                                if (response.Result.IsSuccessStatusCode)
                                {
                                    objResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                                    if (objResponse.Status == Response.Success)
                                    {
                                        if (fileGenerateCount > 0)
                                        {
                                            AppendLogMessage(" " + fileGenerateCount + " Retry call ended to retrieve documents and generate zip files");
                                        }
                                        AppendLogMessage("----------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                                    }
                                }
                                else
                                {
                                    //isStatus = false;
                                }
                                if (fileGenerateCount > 0)
                                {
                                    AppendLogMessage(" " + fileGenerateCount + " Retry call ended to retrieve documents and generate zip files");
                                }
                                AppendLogMessage("----------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                            }
                            if (objResponse != null)
                            {
                                AppendLogMessage(string.Concat(company, " ", objResponse.LogMessage.ToString().Trim()));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendLogMessage(string.Concat(company, " ", objResponse.LogFileMessage.ToString().Trim()));
                        AppendLogMessage("Error while generating zip files" + "\n" + "Error : " + ex.Message);
                        AppendLogMessage(ex.StackTrace);

                        if (fileGenerateCount > 0)
                        {
                            AppendLogMessage(" " + fileGenerateCount + " Retry call ended to retrieve documents and generate zip files");
                        }
                        AppendLogMessage("----------------------------------------------------------------------------------------------------------------------------------------------------------------------");

                        if (fileGenerateCount == retryCount - 1)
                        {
                            AppendLogMessage(" Sending failure mail.");
                            SendReport((" <b>Error occurred while generating zip files for the date " + processingDate.ToShortDateString() + "</b>" + "<br><br>" + " <b>Error Description:</b> " + "<br>" + ex.Message.Trim() + "<br>" + " <b>Error Details:</b> " + "<br>" + ex.StackTrace));
                        }

                        fileGenerateCount++;
                    }
                //}
                //upload the zipfiles to FTP
                if (isSuccessfullyCreated && fileCount > 0 && isZipFileGenerated && isZipFileMoved)
                {
                    bool isZipFileUploaded = false;
                    while (fileUploadCount < retryCount)
                    {
                        try
                        {
                            if (fileUploadCount > 0)
                            {
                                AppendLogMessage(" " + fileUploadCount + " Retry call started for uploading zip files to FTP");
                            }
                            isZipFileUploaded = UploadZipFilesToFtp(path, zipFileName, ftpPath, userName, password);
                            if (isZipFileUploaded)
                            {
                                if (fileUploadCount > 0)
                                {
                                    AppendLogMessage(" " + fileUploadCount + " Retry call ended for uploading zip files to FTP");
                                }
                                AppendLogMessage("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                                break;
                            }
                            if (fileUploadCount > 0)
                            {
                                AppendLogMessage(" " + fileUploadCount + " Retry call ended for uploading zip files to FTP");
                            }
                            AppendLogMessage("----------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                        }
                        catch (Exception ex)
                        {
                            AppendLogMessage(" Error while uploading files to FTP " + "\n" + "Error : " + ex.Message);
                            AppendLogMessage(ex.StackTrace);
                            if (fileUploadCount > 0)
                            {
                                AppendLogMessage(" " + fileUploadCount + " Retry call ended for uploading zip files to FTP");
                            }
                            AppendLogMessage("----------------------------------------------------------------------------------------------------------------------------------------------------------------------");

                            if (fileUploadCount == retryCount - 1)
                            {
                                AppendLogMessage(" Sending failure mail.");
                                SendReport((" <b>Error occurred while generating zip files for the date " + processingDate.ToShortDateString() + "</b>" + "<br><br>" + " <b>Error Description:</b> " + "<br>" + ex.Message.Trim() + "<br>" + " <b>Error Details:</b> " + "<br>" + ex.StackTrace));
                            }
                            fileUploadCount++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppendLogMessage(ex.Message);
                AppendLogMessage(ex.StackTrace);
                AppendLogMessage("----------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            }
            finally
            {
                AppendLogMessage(" Chempoint Ctsi File Generator Ended For " + company + " - Processing Date: " + processingDate.ToShortDateString());
                AppendLogMessage("*************************************************************************************************************************************");

                isZipFileGenerated = false;
                isZipFileMoved = false;
                fileCount = 0;
            }
        }

        public bool UploadZipFilesToFtp(string outPath, string zipFileName, string ftpPath, string userName, string password)
        {
            bool isUploaded = false;

            try
            {

                string ftpInFolderPath = ConfigurationManager.AppSettings["FTPInFolderPath"].ToString().Trim();
                AppendLogMessage(" Uploading the Zip files to FTP upload path " + ftpPath + "/" + ftpInFolderPath + "/");
                Chempoint.GP.FtpBL.CtsiFTPLibrary.FtpClient cl = new Chempoint.GP.FtpBL.CtsiFTPLibrary.FtpClient();
                cl.Host = ftpPath;
                cl.Credentials = new NetworkCredential(userName, password);
                cl.Connect();

                string wd = cl.GetWorkingDirectory();
                cl.SetWorkingDirectory("/" + ftpInFolderPath);

                string fileName = outPath + zipFileName;
                FileInfo fileInf = new FileInfo(fileName);
                Stream requestStream = cl.OpenWrite(fileInf.Name, Chempoint.GP.FtpBL.CtsiFTPLibrary.FtpDataType.Binary);

                FileStream sourceStream = File.OpenRead(fileName);
                byte[] uploadedFileBuffer = new byte[sourceStream.Length];
                sourceStream.Read(uploadedFileBuffer, 0, uploadedFileBuffer.Length);
                sourceStream.Close();

                requestStream.Write(uploadedFileBuffer, 0, uploadedFileBuffer.Length);
                requestStream.Close();

                cl.Disconnect();
                AppendLogMessage(" Successfully uploaded the file '" + zipFileName + "' to FTP site");

                isUploaded = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return isUploaded;
        }

        /// <summary>
        /// Send Mail
        /// </summary>
        /// <param name="messaage"></param>
        private static async void SendReport(string messaage)
        {
            SendEmailRequest objSendEmailRequest = new SendEmailRequest();
            objSendEmailRequest.EmailInformation = new EMailInformation();

            objSendEmailRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            objSendEmailRequest.EmailInformation.SmtpAddress = ConfigurationManager.AppSettings["SMTPServer"];

            objSendEmailRequest.EmailConfigID = Convert.ToInt16(ConfigurationManager.AppSettings["EmailConfigForFileGenerator"]);
            objSendEmailRequest.EmailInformation.EmailFrom = ConfigurationManager.AppSettings["MailFrom"];
            objSendEmailRequest.EmailInformation.Body = messaage;

            //objSendEmailRequest.EmailInformation.Signature = objFTPEntity.Signature;
            objSendEmailRequest.EmailInformation.IsDataTableBodyRequired = false;
            string strBaseUrl = ConfigurationManager.AppSettings["ServiceUrl"];

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(strBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.PostAsJsonAsync("api/SendEmail/SendEmail", objSendEmailRequest).Result;

                if (response.IsSuccessStatusCode)
                {
                    string jsonContent = await response.Content.ReadAsStringAsync();

                    var obj = new JavaScriptSerializer().Deserialize<SendEmailResponse>(jsonContent);

                    strLogMsg.AppendLine(obj.LogMessage.ToString());
                    strLogMsg.AppendLine("Mail has been sent successfully.");
                }
                else
                {
                    strLogMsg.AppendLine("Mail is not sent successfully.");
                }
            }
        }



        /// <summary>
        /// Appends the log details into the log file without date time.
        /// </summary>
        /// <param name="message"></param>
        public void AppendLogMessageWithoutDateTime(string message)
        {
            LogMessage.Append(message.ToString());
            LogMessage.AppendLine();
        }

        /// <summary>
        /// Appends the log details into the LogMessage string 
        /// </summary>
        /// <param name="message">string to be logged</param>        
        public void AppendLogMessage(String message)
        {
            LogMessage.Append(System.DateTime.Now.ToString() + "-- " + message.ToString());
            LogMessage.AppendLine();
        }

        /// <summary>
        /// Log Information to File
        /// </summary>
        /// <param name="path"></param>
        /// <param name="message"></param>
        /// <param name="logFileName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool LogInformationToFile(string path, string message, string logFileName, int type)
        {
            try
            {
                // Get the file name from the configuration xml.
                if (type == 1)
                    logFileName = logFileName + date + ".txt";
                if (type == 2)
                    logFileName = logFileName + DateTime.Now.ToString("MM_dd_yy") + ".log";

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path = path + logFileName;

                if (!File.Exists(path))
                    File.Create(path).Close();

                //Log the details into the file.
                using (StreamWriter w = File.AppendText(path))
                {
                    if (type == 2)
                        w.WriteLine(message.Trim());
                    else
                        w.Write(message.Trim());
                    w.Flush();
                    w.Close();
                }
                // Return success to the caller.                
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw ex;
            }
        }
    }
}
