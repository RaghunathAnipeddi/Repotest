using Chempoint.GP.Model.Interactions.Email;
using Chempoint.GP.Model.Interactions.PayableManagement;
using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ChemPoint.GP.APIOutFileGenerator
{
    /// <PayableManagement>
    /// Project Name        :   payable mangement migration
    /// Affected Module     :   Purchase
    /// Affected Windows    :   GP to API task scheduler.
    /// Developed on        :   April2017
    /// Developed by        :   Muthu and Nagaraj
    ///-------------------------------------------------------------------------------------------
    ///     Date		Author          Description
    ///-------------------------------------------------------------------------------------------
    /// **Apr2017       Mthangaraj      Initial Creation.
    /// </PayableManagement>
    public class APIOutFileGenerator
    {
        static StringBuilder logMessage = new StringBuilder();
        static int companyId;
        static void Main(string[] args)
        {
            //args = new string[] { "EU,Daily" };
            string cmdLineApiArgs = string.Join("", args);
            string[] apiFileParams = cmdLineApiArgs.Split(',');
            if (apiFileParams[0] == "NA")
                companyId = 1;
            else
                companyId = 2;
            string strCutOfMonth = apiFileParams[1].ToString().Trim();
            APIOutFileGenerator apiFileGenerator = new APIOutFileGenerator();
            apiFileGenerator.CallGetAPIFiles(companyId, strCutOfMonth);
        }
        public void AppendLogMessage(String strMessage)
        {
            logMessage.Append(System.DateTime.Now.ToString() + "-- " + strMessage.ToString());
            logMessage.AppendLine();
        }



        /// <summary>
        /// Call API Zip File Creation Method
        /// </summary>
        public void CallGetAPIFiles(int companyId, string strCutOfMonth)
        {
            string Company = string.Empty;
            int count = 0;
            PayableManagementRequest request = null;
            SendEmailRequest sendEmailRequest = new SendEmailRequest();
            EMailInformation eMailInformation = new EMailInformation();
            try
            {
                AppendLogMessage("******************************************************************************************");
                if (companyId == 1)
                    AppendLogMessage(string.Concat(companyId == 1 && strCutOfMonth == "Daily" ? "NA - Daily" : "NA - Quarterly", " ", "Chempoint API File Generator Started."));
                else if (companyId == 2)
                    AppendLogMessage(string.Concat(companyId == 2 && strCutOfMonth == "Daily" ? "EU - Daily" : "EU - Quarterly", " ", "Chempoint API File Generator Started."));
                bool isSuccessfullyCreated = false;
                int retryCount = Convert.ToInt32(ConfigurationManager.AppSettings["RetryCount"].Trim());

                for (count = 1; count <= retryCount; count++)
                {
                    if (!isSuccessfullyCreated)
                    {
                        AppendLogMessage("------------------------------------------------------------------------------------------");
                        AppendLogMessage(count + " Retry call started.");
                        try
                        {

                            request = new PayableManagementRequest();
                            request.companyId = companyId;
                            request.CutofMonth = strCutOfMonth;
                            request.OutputFilePath = ConfigurationManager.AppSettings["OutputFilePath"].ToString();
                            request.VendorFileName = ConfigurationManager.AppSettings["VendorFileName"].ToString();
                            request.LocFileName = ConfigurationManager.AppSettings["LocFileName"].ToString();
                            request.GlFileName = ConfigurationManager.AppSettings["GlFileName"].ToString();
                            request.PoFileName = ConfigurationManager.AppSettings["PoFileName"].ToString();
                            request.RcvFileName = ConfigurationManager.AppSettings["RcvFileName"].ToString();
                            request.PayFileName = ConfigurationManager.AppSettings["PayFileName"].ToString();
                            request.GPToAPITempFolderPath = ConfigurationManager.AppSettings["GPToAPITempFolderPath"].ToString();

                            request.OutputFilePath = ConfigurationManager.AppSettings["OutputFilePath"].Trim();
                            request.FTPInFolderPath = ConfigurationManager.AppSettings["FTPInFolderPath"].Trim();
                            request.FTPOutFolderPath = ConfigurationManager.AppSettings["FTPOutFolderPath"].Trim();
                            request.ZipExtractedFilePath = ConfigurationManager.AppSettings["ZipExtractedFilePath"].Trim();
                            request.FTPPath = ConfigurationManager.AppSettings["FTPPath"].Trim();
                            request.FTPUserName = ConfigurationManager.AppSettings["FTPUserName"].Trim();
                            request.FTPPassword = ConfigurationManager.AppSettings["FTPPassword"].Trim();
                            request.FTPDownloadFile1 = ConfigurationManager.AppSettings["FTPDownloadFile1"].Trim();
                            request.FTPDownloadFile2 = ConfigurationManager.AppSettings["FTPDownloadFile2"].Trim();
                            request.TextFileExtension = ConfigurationManager.AppSettings["TextFileExtension"].Trim();
                            request.executionCount = count;
                            string gpServiceConfigurationUrl = ConfigurationManager.AppSettings["ServiceUrl"];

                            eMailInformation.EmailFrom = ConfigurationManager.AppSettings["MailFrom"].Trim();
                            eMailInformation.SmtpAddress = ConfigurationManager.AppSettings["SMTP"].Trim();
                            eMailInformation.Signature = ConfigurationManager.AppSettings["Signature"].Trim();
                            eMailInformation.Subject = ConfigurationManager.AppSettings["MailGPToAPISubject"].Trim();
                            sendEmailRequest.EmailInformation = eMailInformation;
                            request.AppConfigID = Convert.ToInt16(ConfigurationManager.AppSettings["EmailConfigForFileGenerator"].Trim());
                            request.EmailRequest = sendEmailRequest;

                            using (HttpClient client = new HttpClient())
                            {
                                client.Timeout = TimeSpan.FromMinutes(10);
                                client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                                client.Timeout = new TimeSpan(1, 0, 0);
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                var response = client.PostAsJsonAsync("api/PayableManagement/GetAPIFiles", request); // we need to refer the web.api service url here.
                                if (response.Result.IsSuccessStatusCode)
                                {
                                    PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                                    if (payableResponse != null)
                                    {
                                        logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                    }
                                }
                            }

                            //GetAPIFiles(count, companyId);
                            isSuccessfullyCreated = true;
                            break;
                        }
                        catch
                        {
                            isSuccessfullyCreated = false;
                        }


                    }
                }
                AppendLogMessage(count + " Retry call ended.");
                AppendLogMessage("------------------------------------------------------------------------------------------");


                bool isVensuccessfullyCreated = false;
                for (int count1 = 1; count1 <= retryCount; count1++)
                {
                    if (!isVensuccessfullyCreated)
                    {
                        AppendLogMessage("------------------------------------------------------------------------------------------");
                        AppendLogMessage(count1 + " Retry call started.");
                        try
                        {
                            request = new PayableManagementRequest();
                            request.companyId = companyId;
                            request.GPToAPILoggingPath = ConfigurationManager.AppSettings["MailGPToAPISubject"].Trim();
                            request.GPToAPILogFileName = ConfigurationManager.AppSettings["GPToAPILogFileName"].Trim();

                            request.OutputFilePath = ConfigurationManager.AppSettings["OutputFilePath"].Trim();
                            request.VendorFileName = ConfigurationManager.AppSettings["VendorFileName"].ToString();
                            request.VenGLFileName = ConfigurationManager.AppSettings["VenGLFileName"].ToString();
                            request.LocFileName = ConfigurationManager.AppSettings["LocFileName"].ToString();
                            request.GlFileName = ConfigurationManager.AppSettings["GlFileName"].ToString();
                            request.PoFileName = ConfigurationManager.AppSettings["PoFileName"].ToString();
                            request.RcvFileName = ConfigurationManager.AppSettings["RcvFileName"].ToString();
                            request.PayFileName = ConfigurationManager.AppSettings["PayFileName"].ToString();
                            request.GPToAPITempFolderPath = ConfigurationManager.AppSettings["GPToAPITempFolderPath"].ToString();
                            request.executionCount = count1;
                            string gpServiceConfigurationUrl = ConfigurationManager.AppSettings["ServiceUrl"];

                            eMailInformation.EmailFrom = ConfigurationManager.AppSettings["MailFrom"].Trim();
                            eMailInformation.SmtpAddress = ConfigurationManager.AppSettings["SMTP"].Trim();
                            eMailInformation.Signature = ConfigurationManager.AppSettings["Signature"].Trim();
                            eMailInformation.Subject = ConfigurationManager.AppSettings["MailGPToAPISubject"].Trim();
                            sendEmailRequest.EmailInformation = eMailInformation;
                            request.AppConfigID = Convert.ToInt16(ConfigurationManager.AppSettings["EmailConfigForFileGenerator"].Trim());
                            request.EmailRequest = sendEmailRequest;

                            using (HttpClient client = new HttpClient())
                            {
                                client.Timeout = TimeSpan.FromMinutes(10);
                                client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                var response = client.PostAsJsonAsync("api/PurchaseOrder/GetVenGLDetails", request); // we need to refer the web.api service url here.
                                if (response.Result.IsSuccessStatusCode)
                                {
                                    PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                                    if (payableResponse != null)
                                    {
                                        logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                    }
                                }
                            }

                            //GetVenGLDetails(count1, companyId);
                            isVensuccessfullyCreated = true;
                            break;
                        }
                        catch
                        {
                            isVensuccessfullyCreated = false;

                        }

                    }
                }
                AppendLogMessage(count + " Retry call ended.");
                AppendLogMessage("------------------------------------------------------------------------------------------");
                //Company = company;
            }
            catch (Exception ex)
            {
                AppendLogMessage(ex.Message);
                AppendLogMessage(ex.StackTrace);
                AppendLogMessage("------------------------------------------------------------------------------------------");
            }
            finally
            {
                AppendLogMessage("Chempoint API File Generator Ended.");
                AppendLogMessage("******************************************************************************************");

                // Log the details into file. TODO
                this.LogInformationToFile(ConfigurationManager.AppSettings["GPToAPILoggingPath"].Trim(), logMessage.ToString().Trim(), ConfigurationManager.AppSettings["GPToAPILogFileName"].Trim(), Company, 2);
                logMessage = new StringBuilder();
            }
        }
        internal int LogInformationToFile(string strPath, string strMessage, string strLogFileName, string comnpany, int type)
        {
            try
            {

                if (type == 2)
                {
                    strLogFileName = strLogFileName + DateTime.Now.ToString("MM_dd_yy") + ".log";
                }


                if (!Directory.Exists(strPath))
                    Directory.CreateDirectory(strPath);

                strPath = strPath + strLogFileName;

                if (!File.Exists(strPath))
                    File.Create(strPath).Close();

                //Log the details into the file.
                using (StreamWriter w = File.AppendText(strPath))
                {
                    if (type == 2)
                        w.WriteLine(strMessage.Trim());
                    else
                        w.Write(strMessage.Trim());
                    w.Flush();
                    w.Close();
                }

                // Return success to the caller.                
                return 0;
            }
            catch (Exception ex)
            {
                return -1;
                throw ex;
            }
        }
    }
}
