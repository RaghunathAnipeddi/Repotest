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

namespace Chempoint.GP.APIToGP
{
    /// <PayableManagement>
    /// Project Name        :   payable mangement migration
    /// Affected Module     :   Purchase
    /// Affected Windows    :   API to GP task scheduler.
    /// Developed on        :   April2017
    /// Developed by        :   Muthu
    ///-------------------------------------------------------------------------------------------
    ///     Date		Author          Description
    ///-------------------------------------------------------------------------------------------
    /// **Apr2017       Mthangaraj      Initial Creation.
    /// </PayableManagement>
    public class APIToGP
    {
        StringBuilder logMessage = new StringBuilder();
        public enum Company { Chmpt = 1, Cpeur = 2 };

        static string downloadedFilePath = ConfigurationManager.AppSettings["FTPFileDownloadFolderPath"];

        static void Main(string[] args)
        {
            APIToGP apOutService = new APIToGP();
            try
            {


                apOutService.logMessage.AppendLine("*******************************************************************************************************.");
                apOutService.logMessage.AppendLine(DateTime.Now + " Chempoint.APOut.APOutService.eConnect started.");

                // gets the list of files to process from the input folder path.
                IEnumerable<string> files = GetFilesFromDirectory(downloadedFilePath);
                if (files == null)
                    apOutService.logMessage.AppendLine(DateTime.Now + " No files found to process.");
                else
                {
                    apOutService.logMessage.AppendLine(DateTime.Now + " --------------------------------------------------------------");

                    // Fetch NA files
                    apOutService.logMessage.AppendLine(DateTime.Now + " Fetching list of NA files");
                    List<string> naFiles = (from fileName in files
                                            where Path.GetFileNameWithoutExtension(fileName).StartsWith(ConfigurationManager.AppSettings["CHMPTFileNameForPO"].Trim()) //NACHPAPOPOR
                                                   || Path.GetFileNameWithoutExtension(fileName).StartsWith(ConfigurationManager.AppSettings["CHMPTFileNameForNonPO"].Trim()) //NACHPAPOGLO
                                            orderby fileName descending
                                            select fileName).ToList();

                    // Fetch EU files
                    apOutService.logMessage.AppendLine(DateTime.Now + " Fetching list of EU files");
                    List<string> euFiles = (from fileName in files
                                            where Path.GetFileNameWithoutExtension(fileName).StartsWith(ConfigurationManager.AppSettings["CPEURFileNameForPO"].Trim()) //EUCHPAPOPOR
                                             || Path.GetFileNameWithoutExtension(fileName).StartsWith(ConfigurationManager.AppSettings["CPEURFileNameForNonPO"].Trim()) //EUCHPAPOGLO
                                            orderby fileName descending
                                            select fileName).ToList();

                    if (naFiles.Count() > 0)
                    {
                        apOutService.logMessage.AppendLine(DateTime.Now + " Started : NA file");

                        foreach (string fileName in naFiles.OrderByDescending(s => s))
                            apOutService.logMessage.AppendLine(DateTime.Now + " File Name:  " + fileName.ToString().Trim());

                        apOutService.CreatePayments(naFiles, Company.Chmpt);  // Pass NA files

                        apOutService.logMessage.AppendLine(DateTime.Now + " Ended : NA file");
                    }
                    if (euFiles.Count() > 0)
                    {
                        apOutService.logMessage.AppendLine(DateTime.Now + " Started : EU file");
                        foreach (string fileName in euFiles.OrderByDescending(s => s))
                            apOutService.logMessage.AppendLine(DateTime.Now + " File Name:  " + fileName.ToString().Trim());

                        apOutService.CreatePayments(euFiles, Company.Cpeur);  // Pass EU files

                        apOutService.logMessage.AppendLine(DateTime.Now + " Ended : EU file");
                    }
                    apOutService.logMessage.AppendLine("--------------------------------------------------.");
                }

            }
            catch (Exception ex)
            {
                apOutService.logMessage.AppendLine(ex.Message);
            }
            finally
            {
                apOutService.logMessage.AppendLine(DateTime.Now + " Chempoint.APOut.APOutService.eConnect ended.");
                apOutService.logMessage.AppendLine("*******************************************************************************************************.");
                //Log the details into file.
                LogInformationToFile(apOutService.logMessage.ToString().Trim(), ConfigurationManager.AppSettings["LogFileName"], 2);
                apOutService.logMessage = null;
            }
        }

        /// <summary>
        /// Log Information to File
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static int LogInformationToFile(string message, string logFileName, int type)
        {
            try
            {
                string fileName = logFileName + "_" + DateTime.Now.ToString("MM_dd_yy") + ".log";
                string path = ConfigurationManager.AppSettings["LoggingPath"];


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
        // <summary>
        /// Get the files starts with Freight from directory
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetFilesFromDirectory(String path)
        {
            string[] dirfiles = null;
            IEnumerable<string> files = null;
            try
            {
                dirfiles = Directory.GetFiles(path).ToArray();

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
        /// <param name="files">list of files</param>
        /// <param name="company">company Id</param>
        /// <returns></returns>
        public void CreatePayments(List<string> files, Company companyId)
        {
            PayableManagementRequest objRequest = null;
            PayableManagementResponse objResponse = null;
            string exactFileName = string.Empty;
            try
            {
                if ((companyId == Company.Chmpt && Convert.ToBoolean(ConfigurationManager.AppSettings["IsNAEnabled"])) || (companyId == Company.Cpeur && Convert.ToBoolean(ConfigurationManager.AppSettings["IsEUEnabled"])))
                {
                    if (files.Count() > 0)   // check file count
                    {
                        bool isStatus = false;
                        string gpServiceConfigurationUrl = ConfigurationManager.AppSettings["ServiceURL"].ToString();

                        objRequest = new PayableManagementRequest();
                        objResponse = new PayableManagementResponse();
                        PayableManagementEntity payableManagementDetails = new PayableManagementEntity();
                        payableManagementDetails.Files = files;
                        payableManagementDetails.UserId = ConfigurationManager.AppSettings["UserId"];
                        payableManagementDetails.Source = ConfigurationManager.AppSettings["SourceApi"];
                        payableManagementDetails.SourceFormName = string.Empty;
                        objRequest.PayableManagementDetails = payableManagementDetails;
                        objRequest.companyId = (int)companyId;

                        // Calling PushToGpForApi function where request object has been passed as a parameter.
                        using (HttpClient client = new HttpClient())
                        {
                            client.Timeout = TimeSpan.FromMinutes(10);
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/PayableManagement/ProcessApOutFile", objRequest); // we need to refer the web.api service url here.
                            if (response.Result.IsSuccessStatusCode)
                            {
                                objResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                                if (objResponse.Status == Response.Success)
                                {
                                    isStatus = true;
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
                else
                {
                    logMessage.AppendLine(DateTime.Now + "Either company Id is invalid or integrations not enabled for this company " + companyId);
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(objResponse.LogMessage.ToString().Trim());
                logMessage.AppendLine(ex.Message);
                logMessage.AppendLine(ex.StackTrace);
                logMessage.AppendLine(DateTime.Now + " -------------------------------------.");
            }
        }
    }
}
