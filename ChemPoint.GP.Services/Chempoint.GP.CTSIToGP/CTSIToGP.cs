using Chempoint.GP.Model.Interactions.PayableManagement;
using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.APOut;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chempoint.GP.CTSIToGP
{
    /// <PayableManagement>
    /// Project Name        :   payable mangement migration
    /// Affected Module     :   Purchase
    /// Affected Windows    :   CTSI to GP task scheduler.
    /// Developed on        :   April2017
    /// Developed by        :   Muthu and Nagaraj
    ///-------------------------------------------------------------------------------------------
    ///     Date		Author          Description
    ///-------------------------------------------------------------------------------------------
    /// **Apr2017       Mthangaraj      Initial Creation.
    /// </PayableManagement>
    public class CTSIToGP
    {
        #region Variables declaration
        StringBuilder logMessage = new StringBuilder();
        string chmptFileName = ConfigurationManager.AppSettings["CHMPTFileName"];
        string cpeurFileName = ConfigurationManager.AppSettings["CPEURFileName"];
        string gbpFileName = ConfigurationManager.AppSettings["GBPFileName"];
        int naCompanyId = Convert.ToInt16(ConfigurationManager.AppSettings["NACompanyId"]);
        int euCompanyId = Convert.ToInt16(ConfigurationManager.AppSettings["EUCompanyId"]);
        static string downloadedFilePath = ConfigurationManager.AppSettings["ZipExtractedFilePath"];
        # endregion
        public static void Main(string[] args)
        {
            CTSIToGP processToGP = new CTSIToGP();
            try
            {
               processToGP.logMessage.AppendLine("*******************************************************************************************************.");
                processToGP.logMessage.AppendLine(DateTime.Now + " Chempoint.Ctsi.CreateTransactionInGP.eConnect started.");

                // gets the list of files to process from the input folder path.
                IEnumerable<string> files = GetFilesFromDirectory(downloadedFilePath);
                if (files == null)
                    processToGP.logMessage.AppendLine(DateTime.Now + " No files found to process.");
                else
                {
                    if (args.Length == 0)
                    {
                        foreach (string fileName in files.OrderBy(s => s))
                        {
                            processToGP.CreateTransactions(fileName);
                            processToGP.logMessage.AppendLine("--------------------------------------------------.");
                        }
                    }
                    else
                    {
                        for (int i = 0; i < args.Length; i++)
                        {
                            if (args[i] != string.Empty || args[i] != null)
                            {
                                foreach (string fileName in files)
                                {
                                    processToGP.CreateTransactions(fileName);
                                    processToGP.logMessage.AppendLine("--------------------------------------------------.");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                processToGP.logMessage.AppendLine(ex.Message);
                MessageBox.Show(ex.Message);
            }

            finally
            {
                processToGP.logMessage.AppendLine(DateTime.Now + " Chempoint.Ctsi.CreateTransactionInGP.eConnect ended.");
                processToGP.logMessage.AppendLine("*******************************************************************************************************.");
                //Log the details into file.
                LogInformationToFile(processToGP.logMessage.ToString().Trim(), ConfigurationManager.AppSettings["CtsiToGPLogFileName"], 2);
                processToGP.logMessage = null;
            }
        }

        /// <summary>
        /// Log Information to File
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static int LogInformationToFile(string message, string logFileName, int type)
        {
            try
            {
                string fileName = logFileName + "_" + DateTime.Now.ToString("MM_dd_yy") + ".log";
                string path = ConfigurationManager.AppSettings["CtsiToGPLoggingPath"];


                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path = path + fileName;


                if (!File.Exists(path))
                    File.Create(path).Close();

                //Log the details into the file.
                using (StreamWriter w = File.AppendText(path))
                {
                    w.Write(message.Trim());
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

        /// <summary>
        /// Get the files starts with Freight from directory
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetFilesFromDirectory(String path)
        {
            string[] dirfiles = null;
            IEnumerable<string> files = null;
            try
            {
                dirfiles = Directory.GetFiles(path, ConfigurationManager.AppSettings["FileSearchPattern"]).Where(s =>
                            s.ToLower().EndsWith(ConfigurationManager.AppSettings["XlsFile"]) || s.ToLower().EndsWith(ConfigurationManager.AppSettings["XlsxFile"])).ToArray();

                if (dirfiles.Length > 0)
                    files = dirfiles.OrderByDescending(s => s);//files order by descending   

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return files;
        }

        /// <summary>
        /// Create invoice in GP
        /// </summary>
        /// <param name="count"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        public void CreateTransactions(string fileName)
        {

            //CTSI EMEA implementation
            //Finding out company id from file name.
            int companyId = 0;
            string exactFileName = string.Empty;
            PayableManagementRequest objRequest = null;
            PayableManagementResponse objResponse = null;
            bool isStatus = false;
            string gpServiceConfigurationUrl = ConfigurationManager.AppSettings["ServiceURL"].ToString();
            try
            {

                logMessage.AppendLine(DateTime.Now + " --------------------------------------------------------------");
                logMessage.AppendLine(DateTime.Now + " Started : Validating file " + fileName);

                exactFileName = fileName.Substring(fileName.LastIndexOf("\\") + 1).Substring(0, 7);
                if (exactFileName.ToUpper() == chmptFileName)
                    companyId = naCompanyId;
                else if (exactFileName.ToUpper() == cpeurFileName || exactFileName.ToUpper() == gbpFileName)
                    companyId = euCompanyId;


                if (companyId == naCompanyId || (companyId == euCompanyId && Convert.ToBoolean(ConfigurationManager.AppSettings["IsEUEnabled"])))
                {

                    objRequest = new PayableManagementRequest();
                    objResponse = new PayableManagementResponse();
                    PayableManagementEntity payableManagementEntity = new PayableManagementEntity();
                    payableManagementEntity.FileName = fileName;
                    payableManagementEntity.UserId = ConfigurationManager.AppSettings["UserId"];
                    payableManagementEntity.Source = ConfigurationManager.AppSettings["Source"];
                    payableManagementEntity.SourceFormName = string.Empty;
                    objRequest.PayableManagementDetails = payableManagementEntity;
                    objRequest.companyId = companyId;

                    using (HttpClient client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromMinutes(10);
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/PayableManagement/ProcessCtsiFile", objRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            objResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                            if (objResponse.Status == Response.Success)
                            {
                                logMessage.AppendLine(DateTime.Now + " PushToGpForCtsi function called successfully");
                            }
                        }
                        else
                        {
                            isStatus = false;
                        }
                    }
                    if (objResponse != null)
                    {
                        logMessage.AppendLine(objResponse.LogMessage.ToString().Trim());
                    }
                }
                else
                {
                    logMessage.AppendLine(DateTime.Now + " Found no valid file name/files to process.");
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(objResponse.LogMessage.ToString());
                logMessage.AppendLine(ex.Message);
                logMessage.AppendLine(ex.StackTrace);
                logMessage.AppendLine(DateTime.Now + " -------------------------------------.");
            }
        }
    }
}
