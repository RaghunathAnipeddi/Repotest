using Chempoint.GP.Model.Interactions.FTP;
using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.FTP;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Automation;
using System.Threading;
using System.Collections;
using Chempoint.GP.Infrastructure.Logging;
using System.Reflection;
using System.Text.RegularExpressions;
using Chempoint.GP.Model.Interactions.Email;
using ChemPoint.GP.Entities.BaseEntities;
using Chempoint.GP.Model.Interactions.Purchases;

namespace Chempoint.ReportUploader
{
    public class ReportUpldr
    {

        static string strGenReportFileName = string.Empty, strLogPath = string.Empty, strLogFileName = string.Empty;
        static List<string> objFTPUploadFilesList = new List<string>(); //GLobal field to store the paths for uploading in FTP server
        static StringBuilder logMessage = new StringBuilder();
        static DataTable dtResultSet = new DataTable();
        static int HttpClientTimeOut = 100;
        static bool IsMailSent = false;

        /// <summary>
        /// Convert datatable to Notepad and save in path
        /// </summary>
        /// <param name="strOutFilePath"></param>
        /// <param name="dt"></param>
        private static void ConvertDataTableToNotepad(string strOutFilePath, DataTable dt, bool bIsHeaderReq)
        {

            StringBuilder objStringBuilder = new StringBuilder();
            try
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - ConvertDataTableToNotepad method in Chempoint.ReportUploader is started");

                if (!File.Exists(strOutFilePath))
                {
                    var myFile = File.Create(strOutFilePath);
                    myFile.Close();
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Notepad file is created at " + strOutFilePath);
                }

                StreamWriter sw = new StreamWriter(strOutFilePath);
                int intColCount = dt.Columns.Count;

                int i, j;

                if (bIsHeaderReq)
                {
                    for (j = 0; j < dt.Columns.Count - 1; j++)
                    {
                        sw.Write(dt.Columns[j].ColumnName + "\t");
                    }
                    sw.Write(Environment.NewLine);
                }
                foreach (DataRow row in dt.Rows)
                {
                    object[] array = row.ItemArray;
                    for (i = 0; i < array.Length; i++)
                    {
                        sw.Write((string.IsNullOrEmpty(array[i].ToString()) ? " " : array[i].ToString().Trim()) + (i == array.Length - 1 ? "" : "\t"));
                    }
                    sw.Write(Environment.NewLine);
                }

                sw.Flush();
                sw.Close();

                //Copying file to Secured Path
                File.Copy(strOutFilePath, ConfigurationManager.AppSettings["SecuredFTP"] + Path.GetFileName(strOutFilePath));


                logMessage.AppendLine(DateTime.Now.ToString() + " - Report content is written in " + strOutFilePath);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());

            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - ConvertDataTableToNotepad method in Chempoint.ReportUploader ended.");

            }
        }

        /// <summary>
        /// Upload files to FTP and delete the temporary files and save the backup of it
        /// </summary>
        private static void UploadFilesToFtp()
        {

            try
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - UploadFilesToFtp method in Chempoint.ReportUploader is started");

                #region Upload files


                FTPRequest objFTPRequest = new FTPRequest();
                objFTPRequest.objFTPEntity = new FTPEntity();
                objFTPRequest.objFTPEntity.AppName = ConfigurationManager.AppSettings["AppName"];

                objFTPRequest.objFTPEntity.FilesPathList = objFTPUploadFilesList;
                HttpClient client = new HttpClient();
                string strBaseUrl = ConfigurationManager.AppSettings["ServiceUrl"];
                client.BaseAddress = new Uri(strBaseUrl);
                client.Timeout = new TimeSpan(1, 0, 0);
                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string strMethodName = "api/FTP/FilesUploadToFtpWithOutZip";


                logMessage.AppendLine(DateTime.Now.ToString() + " - Invoking the " + strBaseUrl + strMethodName);
                objFTPRequest.logMessage = logMessage;

                HttpResponseMessage response = client.PostAsJsonAsync(strMethodName, objFTPRequest).Result;
                if (response.IsSuccessStatusCode)
                {
                    var obj = response.Content.ReadAsAsync<FTPResponse>();
                    logMessage = new StringBuilder().AppendLine(obj.Result.TempString);
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Files are uploaded successfully");
                    Console.WriteLine("Files are uploaded successfully");
                }
                else
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Files are not uploaded");
                }


                #endregion
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());

            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - UploadFilesToFtp method in Chempoint.ReportUploader ended.");

            }
        }

        /// <summary>
        /// Get the Url of Internet shortcut file from Browser
        /// </summary>

        private static void GetReportUrlFromBrowser()
        {
            string[] strSourceDir = null;
            int intFileCount = 0;
            string strArchievedShortcutFile = string.Empty, strGenRptFilePath = string.Empty,
                    strFetchedUrl = string.Empty;

            try
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - GetReportUrlFromBrowser method in Chempoint.ReportUploader is started");

                strSourceDir = Directory.GetFiles(ConfigurationManager.AppSettings["SourceDir"].ToString());
                if (strSourceDir.Length > 0)
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Getting the Generated URL link files from " + ConfigurationManager.AppSettings["SourceDir"]);
                    intFileCount = strSourceDir.Length;

                    foreach (string strFilePath in strSourceDir)
                    {
                        strGenRptFilePath = strFilePath;
                        strGenReportFileName = Path.GetFileNameWithoutExtension(strFilePath);
                        if (!strGenReportFileName.Contains("Group") || !strGenReportFileName.Contains("group"))
                        {

                            logMessage.AppendLine(DateTime.Now.ToString() + " - Closing the IE Browser if opened");

                        FetchURLFromIE:

                            CloseIEBrowser();

                            Process.Start(strFilePath);
                            Thread.Sleep(5000);

                            foreach (Process process in Process.GetProcessesByName("iexplore"))
                            {
                                strFetchedUrl = GetInternetExplorerUrl(process);
                                if (strFetchedUrl == null)
                                {
                                    //If url value is not able to fetch, then go to "FetchURLFromIE label"
                                    goto FetchURLFromIE;
                                }
                                else
                                    break;
                            }

                            long lngDate = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));

                            if (File.Exists(strGenRptFilePath))
                            {
                                strArchievedShortcutFile = ConfigurationManager.AppSettings["ArchievedURLDir"].ToString();

                                File.Copy(strGenRptFilePath, strArchievedShortcutFile + strGenReportFileName + "_" + lngDate.ToString() + ".url");
                                logMessage.AppendLine(DateTime.Now.ToString() + " - File copied to " + strArchievedShortcutFile + strGenReportFileName + "_" + lngDate.ToString() + ".url");
                                File.Delete(strGenRptFilePath);
                                logMessage.AppendLine(DateTime.Now.ToString() + " - " + strGenRptFilePath + " is deleted");
                            }
                            if (!string.IsNullOrEmpty(strFetchedUrl))
                            {
                                if (strFetchedUrl.Contains("viewershell?reportId="))
                                {
                                    strFetchedUrl = strFetchedUrl.Replace("viewershell?reportId=", "part/");

                                    if (strFetchedUrl.Contains("&"))
                                        strFetchedUrl = strFetchedUrl.Replace("&", "/");

                                    if (strFetchedUrl.Contains("=0"))
                                        strFetchedUrl = strFetchedUrl.Replace("=0", "/0");

                                    logMessage.AppendLine(DateTime.Now.ToString() + " - Fetched URL converted to " + strFetchedUrl);

                                }

                                ReportUpldr.DownloadReportFromUrl(strFetchedUrl);
                            }

                        }
                    }

                    //Close IE browser
                    CloseIEBrowser();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());

            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - GetReportUrlFromBrowser method in Chempoint.ReportUploader ended.");

            }
        }

        /// <summary>
        /// Download data from URL
        /// </summary>
        /// <param name="objUrlList"></param>
        private static void DownloadReportFromUrl(string strUrl)
        {

            try
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - DownloadReportFromUrl method in Chempoint.ReportUploader is started");

                HtmlAgilityPack.HtmlDocument objHtmlDocument = null;
                string strPage = string.Empty;
                WebClient objWebClient = null;
                DataTable dtTable = null;


                objWebClient = new WebClient();
                objWebClient.UseDefaultCredentials = true;
                objWebClient.Credentials = CredentialCache.DefaultCredentials;


                strPage = objWebClient.DownloadString(strUrl).Trim();

                logMessage.AppendLine(DateTime.Now.ToString() + " - Downloaded the Html string from URL http://" + strUrl);

                objHtmlDocument = new HtmlAgilityPack.HtmlDocument();
                objHtmlDocument.LoadHtml(strPage);

                var headers = objHtmlDocument.DocumentNode.SelectNodes("//tr/th");

                dtTable = new DataTable();

                foreach (HtmlNode header in headers)
                    dtTable.Columns.Add(header.InnerText);

                foreach (var row in objHtmlDocument.DocumentNode.SelectNodes("//tr[td]"))
                    dtTable.Rows.Add(row.SelectNodes("td").Select(td => td.InnerText).ToArray());

                logMessage.AppendLine(DateTime.Now.ToString() + " - Converted the HTML string to Datatable");

                dtTable.Columns.Remove("Column1");
                dtTable.Columns.Remove("Column2");
                dtTable.Columns.Remove("Column3");
                dtTable.Columns.Remove("Column4");

                dtTable.Columns[0].ColumnName = "Account Description";
                dtTable.Columns[1].ColumnName = "Account Number";
                dtTable.Columns[2].ColumnName = "YTD";



                for (int i = dtTable.Rows.Count - 1; i >= 0; i--)
                {
                    if (dtTable.Rows[i]["Account Number"].ToString().Contains("&nbsp;"))
                        dtTable.Rows[i].Delete();
                }

                string strAccNo = dtTable.Rows[4]["Account Number"].ToString().Trim();
                string strCurrency = string.Empty;

                if (strAccNo.Split('-')[0].StartsWith("4") || strAccNo.Split('-')[0].StartsWith("1"))
                {
                    strCurrency = "USD";
                    strGenReportFileName = ConfigurationManager.AppSettings["NAReportName"];
                }

                else if (strAccNo.Split('-')[0].StartsWith("5") || strAccNo.Split('-')[0].StartsWith("2"))
                {
                    strCurrency = "EUR";
                    strGenReportFileName = ConfigurationManager.AppSettings["EUReportName"];
                }

                long lngDate = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));

                string strNewFilePath = ConfigurationManager.AppSettings["ArchievedExceldDir"] + strGenReportFileName + "_" + lngDate.ToString() + ".csv";


                for (int i = 0; i <= dtTable.Rows.Count - 1; i++)
                {
                    if (dtTable.Rows[i][0].ToString().Contains("&amp;"))
                    {
                        string str = dtTable.Rows[i]["Account Description"].ToString().Replace("&amp;", "&");
                        dtTable.Rows[i]["Account Description"] = str;
                    }


                    string amtStr = dtTable.Rows[i]["YTD"].ToString();
                    NumberStyles styles = NumberStyles.AllowParentheses | NumberStyles.AllowTrailingSign | NumberStyles.Float | NumberStyles.AllowThousands;
                    string outputStr = double.Parse(amtStr, styles).ToString("#,0.00;-#,0.00");
                    double outputDbl = double.Parse(outputStr);
                    dtTable.Rows[i]["YTD"] = outputDbl.ToString();

                }
                dtTable.AcceptChanges();


                ReportUpldr.WriteToExcelSpreadsheet(dtTable, strNewFilePath);

                dtTable.Columns.Add("Account Currency");
                for (int i = 0; i <= dtTable.Rows.Count - 1; i++)
                {
                    dtTable.Rows[i]["Account Currency"] = strCurrency;
                }


                ReportUpldr.AppendURLDataToTemplateDatatable(dtTable, strGenReportFileName);

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());

            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - DownloadReportFromUrl method in Chempoint.ReportUploader ended.");

            }
        }

        /// <summary>
        /// Appending the data got from Report url to predefind template datatable
        /// </summary>
        /// <param name="dataTable"></param>
        private static void AppendURLDataToTemplateDatatable(DataTable dataTable, string strFileName)
        {

            try
            {
                DateTime PeriodEndDate = new DateTime();
                DateTime now = DateTime.Now;

                if (Convert.ToInt16(now.Date.Day) < 20)
                    PeriodEndDate = DateTime.Today.AddDays(0 - DateTime.Today.Day);
                else if (Convert.ToInt16(now.Date.Day) >= 20)
                {
                    var Date = new DateTime(now.Year, now.Month, 1);
                    PeriodEndDate = Date.AddMonths(1).AddDays(-1);
                }


                logMessage.AppendLine(DateTime.Now.ToString() + " - AppendURLDataToTemplateDatatable method in Chempoint.ReportUploader is started");

                DataTable dt = new DataTable();

                dt.Columns.Add("Entity Unique Identifier");
                dt.Columns.Add("Account Number");
                dt.Columns.Add("Key3");
                dt.Columns.Add("Key4");
                dt.Columns.Add("Key5");
                dt.Columns.Add("Key6");
                dt.Columns.Add("Key7");
                dt.Columns.Add("Key8");
                dt.Columns.Add("Key9");
                dt.Columns.Add("Key10");
                dt.Columns.Add("Account Description");
                dt.Columns.Add("Account Reference");
                dt.Columns.Add("Financial Statement");
                dt.Columns.Add("Account Type");
                dt.Columns.Add("Active Account");
                dt.Columns.Add("Activity in Period");
                dt.Columns.Add("Functional Currency");
                dt.Columns.Add("Account Currency");
                dt.Columns.Add("Period End Date");
                dt.Columns.Add("Reporting Balance");
                dt.Columns.Add("GL Functional Balance");
                dt.Columns.Add("GL Account Balance");


                for (int rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
                {

                    string str = dataTable.Rows[rowIndex]["Account Number"].ToString().Trim();

                    string[] strArr = str.Split('-');

                    if (strArr[2].StartsWith("1") || strArr[2].StartsWith("2") || strArr[2].StartsWith("3"))
                    {
                        DataRow dr = dt.NewRow();

                        string strAccountType = string.Empty;
                        if (strArr[2].StartsWith("1"))
                            strAccountType = "Asset";
                        else if (strArr[2].StartsWith("2"))
                            strAccountType = "Liability";
                        else if (strArr[2].StartsWith("3"))
                            strAccountType = "Equity";

                        dr["Entity Unique Identifier"] = strArr[0].Trim();
                        dr["Account Number"] = strArr[1] + "-" + strArr[2] + "-" + strArr[3] + "-" + strArr[4];
                        dr["Key3"] = string.Empty;
                        dr["Key4"] = string.Empty;
                        dr["Key5"] = string.Empty;
                        dr["Key6"] = string.Empty;
                        dr["Key7"] = string.Empty;
                        dr["Key8"] = string.Empty;
                        dr["Key9"] = string.Empty;
                        dr["Key10"] = string.Empty;
                        dr["Account Description"] = dataTable.Rows[rowIndex][0].ToString().Trim();
                        dr["Account Reference"] = string.Empty;
                        dr["Financial Statement"] = "A";
                        dr["Account Type"] = strAccountType;
                        dr["Active Account"] = "TRUE";
                        dr["Activity in Period"] = "FALSE";
                        dr["Functional Currency"] = string.Empty;
                        dr["Account Currency"] = dataTable.Rows[rowIndex][3].ToString().Trim();
                        dr["Period End Date"] = PeriodEndDate.ToString("MM/dd/yyyy");
                        dr["Reporting Balance"] = string.Empty;
                        dr["GL Functional Balance"] = string.Empty;
                        dr["GL Account Balance"] = dataTable.Rows[rowIndex][2].ToString().Trim();

                        dt.Rows.Add(dr);
                    }
                }
                dt.AcceptChanges();

                if (dt.Rows.Count > 0)
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " - All filters are applied to Datatable");
                    string strNotepadReportPath = ConfigurationManager.AppSettings["ArchievedNotepdDir"];

                    long lngDate = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));

                    string strPath = strNotepadReportPath + strFileName + "_" + lngDate.ToString() + ".txt";


                    ReportUpldr.ConvertDataTableToNotepad(strPath, dt, false);

                }
            }

            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());

            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - AppendURLDataToTemplateDatatable method in Chempoint.ReportUploader ended.");

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="strFilePath"></param>
        private static void WriteToExcelSpreadsheet(DataTable dt, string strFilePath)
        {

            try
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - WriteToExcelSpreadsheet method in Chempoint.ReportUploader is started");


                if (!File.Exists(strFilePath))
                {
                    var myFile = File.Create(strFilePath);
                    myFile.Close();
                }

                //open file
                StreamWriter wr = new StreamWriter(strFilePath);

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    wr.Write(dt.Columns[i].ToString().ToUpper() + ",");
                }

                wr.WriteLine();

                //write rows to excel file
                for (int i = 0; i < (dt.Rows.Count); i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        wr.Write(dt.Rows[i][j].ToString().Trim() + ",");
                    }
                    //go to next line
                    wr.WriteLine();
                }
                //close file
                wr.Close();
                wr.Dispose();

                logMessage.AppendLine(DateTime.Now.ToString() + " - Report content is written to " + strFilePath);



            }
            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());

            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - WriteToExcelSpreadsheet method in Chempoint.ReportUploader ended.");

            }
        }

        /// <summary>
        /// Getting the Upload report config details using AppName
        /// Generating the Report based on Filters
        /// Uploading the Report to FTP based on IsFtp filter
        /// </summary>
        /// <param name="strAppName"></param>
        private static async void GetUploadReportConfigDetails(string strAppName)
        {

            FTPRequest objFTPRequest = null;
            HttpClient client = null;
            string strBaseUrl = string.Empty, strMethodName = string.Empty, jsonContent = string.Empty;
            HttpResponseMessage response = null;
            FTPResponse objFTPResponse = null;
            string strFileType = string.Empty;
            try
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - GetUploadReportConfigDetails method in Chempoint.ReportUploader is started");

                objFTPRequest = new FTPRequest();
                objFTPRequest.objFTPEntity = new FTPEntity();
                objFTPRequest.objFTPEntity.AppName = strAppName;
                client = new HttpClient();
                strBaseUrl = ConfigurationManager.AppSettings["ServiceUrl"];
                client.BaseAddress = new Uri(strBaseUrl);
                client.Timeout = new TimeSpan(0, HttpClientTimeOut, 0);
                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                strMethodName = "api/FTP/GetUploadReportConfigDetails";

                logMessage.AppendLine(DateTime.Now.ToString() + " - Invoking the " + strBaseUrl + strMethodName);
                objFTPRequest.logMessage = logMessage;

                response = client.PostAsJsonAsync(strMethodName, objFTPRequest).Result;


                if (response.IsSuccessStatusCode)
                {
                    //Getting the string Json Response
                    jsonContent = await response.Content.ReadAsStringAsync();

                    //Deserialize the json response to FTPResponse object
                    objFTPResponse = new JavaScriptSerializer().Deserialize<FTPResponse>(jsonContent);
                    logMessage = new StringBuilder().AppendLine(objFTPResponse.TempString);

                    if (objFTPResponse.objFTPEntityList != null)
                    {
                        foreach (var objFTPEntity in objFTPResponse.objFTPEntityList)
                        {

                            //Getting the Datatable using Stored procedure obtained from objFTPEntity.ScripName
                            GetResultSet(objFTPEntity);

                            if (dtResultSet != null)
                            {
                                if (objFTPEntity.FormatType == ".txt")
                                {
                                    logMessage.AppendLine(DateTime.Now.ToString() + " - FileType of AppId: " + objFTPEntity.AppId + " is .txt");

                                    //objFTPUploadFilesList is global field. Therefore removing the values if exists
                                    if (objFTPUploadFilesList.Count > 0)
                                        objFTPUploadFilesList.RemoveAt(0);

                                    string strArchivPath = ConfigurationManager.AppSettings["ArchievedNotepdDir"];
                                    string date = DateTime.Now.ToString("yyyyMMddHHmmss").Trim();
                                    string strPath = strArchivPath + objFTPEntity.FileName + "_" + date + ".txt";


                                    //Converting the Datatable to Notepad
                                    ConvertDataTableToNotepad(strPath, dtResultSet, objFTPEntity.IsHeaderRequired);

                                    if (objFTPEntity.IsZip)
                                    {
                                        logMessage.AppendLine(DateTime.Now.ToString() + " - IsZip property of AppId: " + objFTPEntity.AppId + " is true");
                                        objFTPRequest = new FTPRequest();
                                        objFTPRequest.objFTPEntity = objFTPEntity;
                                        objFTPRequest.objFTPEntity.ArchiveLocation = strArchivPath;
                                        objFTPRequest.objFTPEntity.FilesPathList = objFTPUploadFilesList;

                                        strMethodName = "api/FTP/ZipTheFiles";
                                        logMessage.AppendLine(DateTime.Now.ToString() + " - Invoking the " + strBaseUrl + strMethodName);
                                        objFTPRequest.logMessage = logMessage;
                                        response = client.PostAsJsonAsync(strMethodName, objFTPRequest).Result;
                                        logMessage = new StringBuilder().AppendLine(objFTPResponse.TempString);

                                        if (response.IsSuccessStatusCode)
                                        {
                                            //objFTPUploadFilesList is global field. Therefore removing the values if exists
                                            objFTPUploadFilesList.RemoveAt(0);

                                            jsonContent = await response.Content.ReadAsStringAsync();
                                            objFTPResponse = new JavaScriptSerializer().Deserialize<FTPResponse>(jsonContent);

                                            var FileName = objFTPResponse.Output;

                                            if (File.Exists(FileName))
                                                objFTPUploadFilesList.Add(FileName);

                                        }
                                    }

                                }


                                if (objFTPEntity.FormatType == ".csv")
                                {

                                    logMessage.AppendLine(DateTime.Now.ToString() + " - FileType of AppId: " + objFTPEntity.AppId + " is .csv");

                                    //objFTPUploadFilesList is global field. Therefore removing the values if exists
                                    if (objFTPUploadFilesList.Count > 0)
                                        objFTPUploadFilesList.RemoveAt(0);

                                    string strArchivPath = ConfigurationManager.AppSettings["ArchievedExceldDir"];
                                    string date = DateTime.Now.ToString("yyyyMMddHHmmss").Trim();
                                    string strPath = strArchivPath + objFTPEntity.FileName + "_" + date + ".csv";

                                    //Converting the datatable to CSV
                                    WriteToExcelSpreadsheet(dtResultSet, strPath);

                                    if (File.Exists(strPath))
                                        objFTPUploadFilesList.Add(strPath);

                                    if (objFTPEntity.IsZip)
                                    {
                                        logMessage.AppendLine(DateTime.Now.ToString() + " - IsZip property of AppId: " + objFTPEntity.AppId + " is true");

                                        objFTPRequest = new FTPRequest();
                                        objFTPRequest.objFTPEntity = objFTPEntity;
                                        objFTPRequest.objFTPEntity.ArchiveLocation = strArchivPath;
                                        objFTPRequest.objFTPEntity.FilesPathList = objFTPUploadFilesList;

                                        strMethodName = "api/FTP/ZipTheFiles";
                                        logMessage.AppendLine(DateTime.Now.ToString() + " - Invoking the " + strBaseUrl + strMethodName);
                                        objFTPRequest.logMessage = logMessage;
                                        response = client.PostAsJsonAsync(strMethodName, objFTPRequest).Result;
                                        logMessage = new StringBuilder().AppendLine(objFTPResponse.TempString);

                                        if (response.IsSuccessStatusCode)
                                        {
                                            //objFTPUploadFilesList is global field. Therefore removing the values if exists
                                            if (objFTPUploadFilesList.Count > 0)
                                                objFTPUploadFilesList.RemoveAt(0);

                                            jsonContent = await response.Content.ReadAsStringAsync();
                                            objFTPResponse = new JavaScriptSerializer().Deserialize<FTPResponse>(jsonContent);

                                            var FileName = objFTPResponse.Output;

                                            if (File.Exists(FileName))
                                                objFTPUploadFilesList.Add(FileName);
                                        }
                                    }

                                }

                                if (objFTPEntity.IsMail)
                                {
                                    logMessage.AppendLine(DateTime.Now.ToString() + " - IsMail property of AppId: " + objFTPEntity.AppId + " is true");

                                    if (dtResultSet != null)
                                    {
                                        if ((objFTPEntity.ScriptName == Chempoint.GP.Infrastructure.Config.Configuration.SPGetUpdatedPOCostDetailsNA ||
                                            objFTPEntity.ScriptName == Chempoint.GP.Infrastructure.Config.Configuration.SPGetUpdatedPOCostDetailsEU))
                                        {
                                            DataTable dtClone = new DataTable();
                                            dtClone = dtResultSet.Copy();
                                            dtClone.Columns.Remove("TraceId");

                                            SendReport(dtClone, objFTPEntity);

                                            if (IsMailSent)
                                            {
                                                PurchaseOrderRequest purchaseOrderRequest = new PurchaseOrderRequest();

                                                if (objFTPEntity.ScriptName == Chempoint.GP.Infrastructure.Config.Configuration.SPGetUpdatedPOCostDetailsNA)
                                                    purchaseOrderRequest.CompanyID = 1;
                                                else
                                                    purchaseOrderRequest.CompanyID = 2;


                                                //Getting Unique Trace Id from dtResultSet

                                                DataTable dtUnique = dtResultSet.DefaultView.ToTable(true, "TraceId");

                                                purchaseOrderRequest.UpdatePoCostDataTableString = ConvertDataTableToString(dtUnique);

                                                strMethodName = "api/PurchaseOrder/UpdatePoCostProactiveMailStatus";

                                                logMessage.AppendLine(DateTime.Now.ToString() + " - Invoking the " + strBaseUrl + strMethodName);

                                                response = client.PostAsJsonAsync(strMethodName, purchaseOrderRequest).Result;

                                                if (response.IsSuccessStatusCode)
                                                {

                                                    logMessage.AppendLine(DateTime.Now.ToString() + " - Successfully updated the POcost process state ");

                                                }
                                                else
                                                    logMessage.AppendLine(DateTime.Now.ToString() + " - Updating the POcost process state is failed");

                                            }
                                        }

                                        else
                                            SendReport(dtResultSet, objFTPEntity);
                                    }
                                }
                            }

                            if (objFTPEntity.IsFtp)
                            {
                                logMessage.AppendLine(DateTime.Now.ToString() + " - IsFtp property of AppId: " + objFTPEntity.AppId + " is true");
                                if (objFTPUploadFilesList.Count > 0)
                                    UploadFilesToFtp();
                            }
                        }
                    }
                }
                else
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Files are not uploaded");
                }

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());

            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - GetUploadReportConfigDetails method in Chempoint.ReportUploader ended.");

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objFTPEntity"></param>
        /// <returns></returns>
        private static async void GetResultSet(FTPEntity objFTPEntity)
        {

            try
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - GetResultSet method in Chempoint.ReportUploader is started");

                FTPRequest objFTPRequest = new FTPRequest();
                objFTPRequest.objFTPEntity = objFTPEntity;

                HttpClient client = new HttpClient();
                string strBaseUrl = ConfigurationManager.AppSettings["ServiceUrl"];
                client.BaseAddress = new Uri(strBaseUrl);
                client.Timeout = new TimeSpan(0, HttpClientTimeOut, 0);
                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string strMethodName = "api/FTP/GetResulSet";

                logMessage.AppendLine(DateTime.Now.ToString() + " - Invoking the " + strBaseUrl + strMethodName);
                objFTPRequest.logMessage = logMessage;

                HttpResponseMessage response = client.PostAsJsonAsync(strMethodName, objFTPRequest).Result;

                if (response.IsSuccessStatusCode)
                {
                    string jsonContent = await response.Content.ReadAsStringAsync();

                    var obj = new JavaScriptSerializer().Deserialize<FTPResponse>(jsonContent);

                    logMessage = new StringBuilder().AppendLine(obj.TempString);

                    if (!string.IsNullOrEmpty(obj.ResultSet))
                        dtResultSet = ConvertStringToDataTable(obj.ResultSet);
                    else
                    {
                        dtResultSet = null;
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Result Set from stored procedure " + objFTPEntity.ScriptName + " is empty");
                    }

                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());

            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - GetResultSet method in Chempoint.ReportUploader ended.");

            }

        }

        /// <summary>
        /// Convert string Datatable to Datatable object
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static DataTable ConvertStringToDataTable(string data)
        {
            DataTable dataTable = new DataTable();
            bool columnsAdded = false;

            try
            {
                if (!string.IsNullOrEmpty(data))
                {

                    foreach (string row in data.Split('#'))
                    {
                        DataRow dataRow = dataTable.NewRow();

                        foreach (string cell in row.Split('|'))
                        {

                            string[] keyValue = cell.Split('~');

                            if (!columnsAdded)
                            {
                                DataColumn dataColumn = new DataColumn(keyValue[0]);
                                dataTable.Columns.Add(dataColumn);
                            }

                            dataRow[keyValue[0]] = keyValue[1];

                        }

                        columnsAdded = true;
                        dataTable.Rows.Add(dataRow);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dataTable;
        }

        private static void CloseIEBrowser()
        {
            Process[] ps = Process.GetProcessesByName("IEXPLORE");

            foreach (Process p in ps)
            {
                try
                {
                    if (!p.HasExited)
                    {
                        p.Kill();
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        private static string GetInternetExplorerUrl(Process process)
        {
            try
            {
                if (process == null)
                    throw new ArgumentNullException("process");

                if (process.MainWindowHandle == IntPtr.Zero)
                    return null;

                AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);
                if (element == null)
                    return null;

                AutomationElement rebar = element.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "ReBarWindow32"));
                if (rebar == null)
                    return null;

                AutomationElement edit = rebar.FindFirst(TreeScope.Subtree, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));

                return ((ValuePattern)edit.GetCurrentPattern(ValuePattern.Pattern)).Current.Value as string;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Passing datatable for sending the mail with html body contained with datatable
        /// </summary>
        /// <param name="dt"></param>
        private static async void SendReport(DataTable dt, FTPEntity objFTPEntity)
        {

            try
            {
                logMessage.AppendLine("****************************************************************");
                logMessage.AppendLine(DateTime.Now.ToString() + " - SendReport method in Chempoint.ReportUploader is started");

                SendEmailRequest objSendEmailRequest = new SendEmailRequest();
                objSendEmailRequest.EmailInformation = new EMailInformation();
                objSendEmailRequest.FileName = objFTPEntity.FileName;
                objSendEmailRequest.Report = ConvertDataTableToString(dt);

                objSendEmailRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
                objSendEmailRequest.EmailInformation.SmtpAddress = ConfigurationManager.AppSettings["SMTPServer"];

                objSendEmailRequest.EmailConfigID = objFTPEntity.EmailConfigID;
                objSendEmailRequest.EmailInformation.EmailFrom = objFTPEntity.EmailFrom;

                objSendEmailRequest.EmailInformation.Signature = objFTPEntity.Signature;
                objSendEmailRequest.EmailInformation.IsDataTableBodyRequired = objFTPEntity.IsDataTableBodyRequired;
                objSendEmailRequest.FileType = objFTPEntity.FormatType;



                string strBaseUrl = ConfigurationManager.AppSettings["ServiceUrl"];

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(strBaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.Timeout = new TimeSpan(0, HttpClientTimeOut, 0);

                    HttpResponseMessage response = client.PostAsJsonAsync("api/SendEmail/SendEmail", objSendEmailRequest).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        IsMailSent = true;

                        string jsonContent = await response.Content.ReadAsStringAsync();

                        var obj = new JavaScriptSerializer().Deserialize<SendEmailResponse>(jsonContent);

                        logMessage = new StringBuilder().AppendLine(obj.LogMessage);
                    }
                    else
                        IsMailSent = false;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());

            }
            finally
            {

                logMessage.AppendLine(DateTime.Now.ToString() + " - SendReport method in Chempoint.ReportUploader ended.");

            }
        }

        private static string ConvertDataTableToString(DataTable dataTable)
        {
            string data = string.Empty;
            int rowsCount = dataTable.Rows.Count;
            for (int rowValue = 0; rowValue < rowsCount; rowValue++)
            {
                DataRow row = dataTable.Rows[rowValue];
                int columnsCount = dataTable.Columns.Count;
                for (int colValue = 0; colValue < columnsCount; colValue++)
                {
                    data += dataTable.Columns[colValue].ColumnName + "~" + row[colValue];
                    if (colValue == columnsCount - 1)
                    {
                        if (rowValue != (rowsCount - 1))
                            data += "#";
                    }
                    else
                        data += "|";
                }
            }
            return data;
        }

        [STAThread()]
        static void Main(string[] args)
        {
            
            try
            {
                logMessage.AppendLine("****************************************************************");
                logMessage.AppendLine(DateTime.Now.ToString() + " - Report Uploader scheduler in Chempoint.ReportUploader is started");
                if (args.Length > 0)
                {

                    Console.WriteLine("Process is started");
                    Console.WriteLine("Process is running");
                    strLogPath = ConfigurationManager.AppSettings["FtpLogPath"];
                    strLogFileName = ConfigurationManager.AppSettings["FtpLogFileName"];

                    logMessage.AppendLine(DateTime.Now.ToString() + " - App Name: " + args[0]);

                    if (args[0] == ConfigurationManager.AppSettings["ManagementReport"])
                    {

                        ReportUpldr.GetReportUrlFromBrowser();

                    }
                    else
                    {
                        GetUploadReportConfigDetails(args[0]);
                    }

                }
                else
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Exe does not contain Arguments");
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());

            }
            finally
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Report Uploader scheduler in Chempoint.ReportUploader ended.");
                logMessage.AppendLine("****************************************************************");
                new TextLogger().LogInformationIntoFile(logMessage.ToString(), strLogPath, strLogFileName);
            }
        }


        #region old code


        #endregion
    }
}

