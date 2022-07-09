using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Dexterity.Bridge;
using Microsoft.Dexterity.Applications;
using Dic1311 = Microsoft.Dexterity.Applications.ChempointCustomizationsDictionary;
using System.Data;
using System.IO;
using Microsoft.Dexterity.Applications.DynamicsDictionary;
using System.Windows.Forms;
using Chempoint.GP.Model.Interactions.PayableManagement;
using System.Net.Http;
using System.Net.Http.Headers;
using ChemPoint.GP.PM.Properties;
using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.APOut;
using System.Linq;
using Chempoint.GP.Infrastructure.Logging;
using System.Runtime.InteropServices;
using ChemPoint.GP.Entities.BaseEntities;
using Chempoint.GP.Infrastructure.Maps.Base;
using Chempoint.GP.Infrastructure.DataAccessEngine.Extensions;
using Chempoint.GP.Infrastructure.Maps.Purchase;
using System.Threading;

namespace ChemPoint.GP.PM
{
    /// <PayableManagement>
    /// Project Name        :   payable mangement migration
    /// Affected Module     :   Purchase
    /// Affected Windows    :   API and CTSI Windows
    /// Developed on        :   April2017
    /// Developed by        :   Muthu and Nagaraj
    ///-------------------------------------------------------------------------------------------
    ///     Date		Author          Description
    ///-------------------------------------------------------------------------------------------
    /// **Apr2017       Mthangaraj      Initial Creation.
    /// </PayableManagement>
    public class GPAddIn : IDexterityAddIn
    {
        // IDexterityAddIn interface

        static string gpServiceConfigurationUrl = null;
        bool isPmLoggingEnabled = false;
        string pmLogFileName = null;
        string pmLogFilePath = null;

        #region PayableService

        private static Dic1311.BusinessGroupLookupForm businessGroupLookupForm;
        private static Dic1311.BusinessUnitLookupForm businessUnitLookupForm;

        private static Dic1311.CtsiDocumentsWithManualPaymentsForm ctsiDocumentsWithManualPaymentsForm;
        private static Dic1311.CtsiReUploadFailureDocumentsForm ctsiReUploadFailureDocumentsForm;
        private static Dic1311.CtsiIdLookupForm ctsiIdLookupForm;
        static VendorLookupForm vendorLookupForm;

        //API EMEA
        private static Dic1311.ApiReUploadFailureDocumentsForm apiReUploadFailureDocumentsForm;
        private static Dic1311.ApiDocumentsWithManualPaymentsForm apiDocumentsWithManualPaymentsForm;
        private static Dic1311.ApiIdLookupForm apiIdLookupForm;


        static string userId;
        bool isPayablesLoggingEnabled = false;
        string payablesLogFileName = string.Empty;
        string payablesLogFilePath = string.Empty;
        //int decimalPlaces = 0;
        DataTable taxDetails = null;

        
        Boolean RegisterReUploadCtsi = false;
        Boolean RegisterCTSILinkedManualDocuments = false;
        Boolean RegisterCtsiVendorLookup1Select = false;
        Boolean RegisterCtsiVendorLookup2Select = false;
        Boolean RegisterCtsiLookup2Select = false;
        Boolean RegisterCtsiLookup1Select = false;
        bool isCtsiLoggingEnabled = false;
        string ctsiLogFileName = string.Empty;
        //DataTable taxDetails = null;

        //API EMEA variables
        Boolean RegisterAPILinkedManualDocuments = false;
        bool isApiLoggingEnabled = false;
        string apiLogFileName = string.Empty;
        string apiLogFilePath = string.Empty;
        int documentType = 0;
        //int documentTypeManualPymt = 0;
        Boolean RegisterReUploadApi = false;
        Boolean RegisterApiLookupSelect = false;
        Boolean RegisterApiVendorLookupSelect = false;
        string lookupWindowName = string.Empty;

        #endregion

        [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileStringW",
            SetLastError = true,
            CharSet = CharSet.Unicode, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, string lpReturnString, int nSize, string lpFilename);

        public void Initialize()
        {

            #region PayableService

            vendorLookupForm = Dynamics.Forms.VendorLookup;
            businessGroupLookupForm = ChempointCustomizations.Forms.BusinessGroupLookup;
            businessUnitLookupForm = ChempointCustomizations.Forms.BusinessUnitLookup;

            ctsiDocumentsWithManualPaymentsForm = ChempointCustomizations.Forms.CtsiDocumentsWithManualPayments;
            ctsiReUploadFailureDocumentsForm = ChempointCustomizations.Forms.CtsiReUploadFailureDocuments;
            ctsiIdLookupForm = ChempointCustomizations.Forms.CtsiIdLookup;

            //API EMEA
            apiReUploadFailureDocumentsForm = ChempointCustomizations.Forms.ApiReUploadFailureDocuments;
            apiDocumentsWithManualPaymentsForm = ChempointCustomizations.Forms.ApiDocumentsWithManualPayments;
            apiIdLookupForm = ChempointCustomizations.Forms.ApiIdLookup;

            //CTSI
            ctsiDocumentsWithManualPaymentsForm.OpenBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiDocumentsWithManualPaymentsForm_OpenBeforeOriginal);
            ctsiReUploadFailureDocumentsForm.OpenBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormOpenBeforeOriginal);
            ctsiIdLookupForm.CtsiIdLookup.RedisplayButton.ClickAfterOriginal += new EventHandler(ctsiIdLookupRedisplayButtonClickAfterOriginal);
            ctsiIdLookupForm.CtsiIdLookup.CancelButton.ClickAfterOriginal += new EventHandler(ctsiIdLookupFormCancelButtonClickAfterOriginal);

            //API EMEA - reUploadFailure Form
            apiReUploadFailureDocumentsForm.OpenBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormOpenBeforeOriginal);
            apiDocumentsWithManualPaymentsForm.OpenBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiDocumentsWithManualPaymentsForm_OpenBeforeOriginal);
            apiIdLookupForm.ApiidLookup.RedisplayButton.ClickAfterOriginal += new EventHandler(apiIdLookupFormRedisplayButton_ClickAfterOriginal);
            apiIdLookupForm.ApiidLookup.CancelButton.ClickAfterOriginal += new EventHandler(apiIdLookupFormCancelButton_ClickAfterOriginal);
            apiIdLookupForm.ApiidLookup.CloseAfterOriginal += new EventHandler(apiIdLookupCloseAfterOriginal);
            vendorLookupForm.VendorLookup.CloseAfterOriginal += new EventHandler(vendorLookupCloseAfterOriginal);

            userId = Dynamics.Globals.UserId.Value.ToString();

            #endregion

            string getCurrentDirectoryPath = @Directory.GetCurrentDirectory();
            string iniFilePath = getCurrentDirectoryPath + "\\Data\\Dex.ini";
            List<string> categories = GetCategories(iniFilePath);
            foreach (string category in categories)
            {
                //Get the key values
                gpServiceConfigurationUrl = GetIniFileString(iniFilePath, category, "GPSERVICE", "");
                isPmLoggingEnabled = Convert.ToBoolean(GetIniFileString(iniFilePath, category, "ISPMLOGENABLED", ""));
                pmLogFileName = GetIniFileString(iniFilePath, category, "PMLOGFILENAME", "");
                pmLogFilePath = GetIniFileString(iniFilePath, category, "PMLOGFILEPATH", "");

            }
        }

        /// <summary>
        /// Method to get the catagory for the ini file.
        /// </summary>
        /// <param name="iniFile"></param>
        /// <returns></returns>
        private static List<string> GetCategories(string iniFile)
        {

            string returnString = new string(' ', 65536);

            GetPrivateProfileString(null, null, null, returnString, 65536, iniFile);

            List<string> result = new List<string>(returnString.Split('\0'));

            result.RemoveRange(result.Count - 2, 2);

            return result;

        }



        /// <summary>

        /// Method to read the data from dex.ini file

        /// </summary>

        /// <param name="iniFile"></param>

        /// <param name="category"></param>

        /// <param name="key"></param>

        /// <param name="defaultValue"></param>

        /// <returns></returns>

        private static string GetIniFileString(string iniFile, string category, string key, string defaultValue)
        {

            string returnString = new string(' ', 1024);

            GetPrivateProfileString(category, key, defaultValue, returnString, 1024, iniFile);

            return returnString.Split('\0')[0];

        }

        #region PayableService

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiIdLookupRedisplayButtonClickAfterOriginal(object sender, EventArgs e)
        {
            PayableManagementRequest requestObj = null;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                requestObj = new PayableManagementRequest();
                requestObj.userId = Dynamics.Globals.UserId.Value.ToString();
                requestObj.companyId = Dynamics.Globals.CompanyId.Value;

                // Service call ...
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.PostAsJsonAsync("api/PayableManagement/GetFailedCTSIIdsList", requestObj);
                    if (response.Result.IsSuccessStatusCode)
                    {
                        PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                        if (payableResponse.Status == Response.Error)
                        {
                            logMessage.AppendLine(payableResponse.LogMessage.ToString());
                            MessageBox.Show(Resources.STR_ErrorLoadingLookup);
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ctsiIdLookupRedisplayButtonClickAfterOriginal Method : " + payableResponse.ErrorMessage.ToString());
                            MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                        }
                        else
                        {
                            logMessage.AppendLine(payableResponse.LogMessage.ToString());
                            //if the line details exists then fill scroll
                            if (payableResponse.LookupDetails != null && payableResponse.LookupDetails.GetLookupDetails.Count > 0)
                            {
                                for (int i = 0; i < payableResponse.LookupDetails.GetLookupDetails.Count; i++)
                                {
                                    ctsiIdLookupForm.Tables.CtsiIdTemp.Close();
                                    ctsiIdLookupForm.Tables.CtsiIdTemp.Release();
                                    ctsiIdLookupForm.Tables.CtsiIdTemp.Ctsiid.Value = payableResponse.LookupDetails.GetLookupDetails[i].CtsiId;
                                    ctsiIdLookupForm.Tables.CtsiIdTemp.Save();
                                }

                                ctsiIdLookupForm.Tables.CtsiIdTemp.Release();
                                ctsiIdLookupForm.Tables.CtsiIdTemp.Close();
                                //Fill details to scrolling window.Call dex procedure to fill the scrolling window.
                                ctsiIdLookupForm.Procedures.CtsiIdLookupFormScrollFill.Invoke();
                            }
                        }
                    }
                    else
                    {
                        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: CtsiId Lookup RedisplayButton ClickAfterOriginal");
                        MessageBox.Show("Error: CtsiId Lookup RedisplayButton ClickAfterOriginal", Resources.STR_MESSAGE_TITLE);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// Event method called to close the expense report lookup window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiIdLookupFormCancelButtonClickAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                ctsiIdLookupForm.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
        }

        #region ExpenseReportUploadForm

        /// <summary>
        ///Method to move the create  the log file and for todays date and write to log file.
        /// </summary>
        /// <param name="message">log message</param>
        /// <param name="logFileName">Name of the log file</param>
        /// <param name="logFilePath">Path for the log file</param>
        public static void LogDetailsToFile(string message, string logFileName, string logFilePath)
        {
            try
            {
                logFileName = logFileName + DateTime.Now.ToString("MM_dd_yy") + ".log";
                if (!Directory.Exists(logFilePath))
                    Directory.CreateDirectory(logFilePath);
                logFilePath = logFilePath + logFileName;

                if (!File.Exists(logFilePath))
                    File.Create(logFilePath).Close();

                int count = 0;
                int maxRetryCount = 0;

                int.TryParse(Resources.RetryCount, out maxRetryCount);

                if (maxRetryCount == 0)
                {
                    maxRetryCount = 3;
                }
                while (count < maxRetryCount)
                {
                    try
                    {
                        using (StreamWriter w = File.AppendText(logFilePath))
                        {
                            w.WriteLine(message);
                            w.Flush();
                            w.Close();
                            count = maxRetryCount;
                        }
                    }
                    catch
                    {
                        count++;
                    }
                }
            }
            catch
            {
                throw;
            }

        }


        #endregion ReloadFailedExpensesEventDefination

        #region PrivateMethodsReLoadAndLinkedPayments

        /// <summary>
        /// Method to initinize the window and corresponsing fields based on the form name passed as parameter.
        /// </summary>
        /// <param name="formName"></param>
        private void InitilizeSortBySelection(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_ReUploadFailureCtsiDocuments)
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.AllOrRange.Value = 0;
                    HideAllSortByReUploadCtsiFields(Resources.STR_ReUploadFailureCtsiDocuments);
                    ClearReUploadCtsiFields(Resources.STR_ReUploadFailureCtsiDocuments);
                    ClearReUploadDocumentDateFields(Resources.STR_ReUploadFailureCtsiDocuments);
                    ClearReUploadVendorIdFields(Resources.STR_ReUploadFailureCtsiDocuments);
                    ClearScrollingWindows(Resources.STR_ReUploadFailureCtsiDocuments, Resources.STR_AllScroll);

                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalDocumentBy.Value == 1)
                    {
                        ShowReUploadCtsiIdFields(Resources.STR_ReUploadFailureCtsiDocuments);
                        ShowHeaderLookupButtons(Resources.STR_ReUploadFailureCtsiDocuments);
                    }
                    else if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalDocumentBy.Value == 2)
                    {
                        ShowReUploadDocumentDateFields(Resources.STR_ReUploadFailureCtsiDocuments);
                    }
                    else if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalDocumentBy.Value == 3)
                    {
                        ShowReUploadVendorIdFields(Resources.STR_ReUploadFailureCtsiDocuments);
                        ShowHeaderLookupButtons(Resources.STR_ReUploadFailureCtsiDocuments);
                    }
                    if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_CPEURCompany)
                    {
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalHideSelectTaxDetails.Hide();
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalHideValidTaxDetails.Hide();
                    }
                    else
                    {
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalHideSelectTaxDetails.Show();
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalHideValidTaxDetails.Show();
                    }
                }
                else if (formName == Resources.STR_CTSIDocumentsWithManualPayments)
                {
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.AllOrRange.Value = 0;
                    HideAllSortByReUploadCtsiFields(Resources.STR_CTSIDocumentsWithManualPayments);
                    ClearReUploadCtsiFields(Resources.STR_CTSIDocumentsWithManualPayments);
                    ClearReUploadDocumentDateFields(Resources.STR_CTSIDocumentsWithManualPayments);
                    ClearReUploadVendorIdFields(Resources.STR_CTSIDocumentsWithManualPayments);
                    ClearScrollingWindows(Resources.STR_CTSIDocumentsWithManualPayments, "");

                    if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalDocumentBy.Value == 1)
                    {
                        ShowReUploadCtsiIdFields(Resources.STR_CTSIDocumentsWithManualPayments);
                        ShowHeaderLookupButtons(Resources.STR_CTSIDocumentsWithManualPayments);
                    }
                    else if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalDocumentBy.Value == 2)
                    {
                        ShowReUploadDocumentDateFields(Resources.STR_CTSIDocumentsWithManualPayments);
                    }
                    else if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalDocumentBy.Value == 3)
                    {
                        ShowReUploadVendorIdFields(Resources.STR_CTSIDocumentsWithManualPayments);
                        ShowHeaderLookupButtons(Resources.STR_CTSIDocumentsWithManualPayments);
                    }
                }
                //API EMEA
                else if (formName == Resources.STR_ReUploadFailureApiDocuments)
                {
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.AllOrRange.Value = 0;
                    HideAllSortByReUploadApiFields(Resources.STR_ReUploadFailureApiDocuments);
                    ClearApiIdFields(Resources.STR_ReUploadFailureApiDocuments);
                    ClearReUploadDocumentDateFields(Resources.STR_ReUploadFailureApiDocuments);
                    ClearReUploadVendorIdFields(Resources.STR_ReUploadFailureApiDocuments);
                    ClearScrollingWindows(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_AllScroll);

                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalDocumentBy.Value == 1)
                    {
                        ShowReUploadApiFields(Resources.STR_ReUploadFailureApiDocuments);
                        ShowHeaderLookupButtons(Resources.STR_ReUploadFailureApiDocuments);
                    }
                    else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalDocumentBy.Value == 2)
                    {
                        ShowReUploadDocumentDateFields(Resources.STR_ReUploadFailureApiDocuments);
                    }
                    else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalDocumentBy.Value == 3)
                    {
                        ShowReUploadVendorIdFields(Resources.STR_ReUploadFailureApiDocuments);
                        ShowHeaderLookupButtons(Resources.STR_ReUploadFailureApiDocuments);
                    }
                }
                else if (formName == Resources.STR_APIDocumentsWithManualPayments)
                {
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.AllOrRange.Value = 0;
                    HideAllSortByReUploadApiFields(Resources.STR_APIDocumentsWithManualPayments);
                    ClearApiIdFields(Resources.STR_APIDocumentsWithManualPayments);
                    ClearReUploadVendorIdFields(Resources.STR_APIDocumentsWithManualPayments);
                    ClearScrollingWindows(Resources.STR_APIDocumentsWithManualPayments, "");

                    if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalDocumentBy.Value == 1) //Document id
                    {
                        ShowReUploadApiFields(Resources.STR_APIDocumentsWithManualPayments);
                        ShowHeaderLookupButtons(Resources.STR_APIDocumentsWithManualPayments);
                    }
                    else if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalDocumentBy.Value == 2) //Vendor 
                    {
                        ShowReUploadVendorIdFields(Resources.STR_APIDocumentsWithManualPayments);
                        ShowHeaderLookupButtons(Resources.STR_APIDocumentsWithManualPayments);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }


        private void ShowReUploadDocumentDateFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_ReUploadFailureCtsiDocuments)
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromDocumentDate.Show();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToDocumentDate.Show();
                }
                else if (formName == Resources.STR_CTSIDocumentsWithManualPayments)
                {
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromDocumentDate.Show();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToDocumentDate.Show();
                }
                else if (formName == Resources.STR_ReUploadFailureApiDocuments)
                {
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromDocumentDate.Show();
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToDocumentDate.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        private void ClearReUploadDocumentDateFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_ReUploadFailureCtsiDocuments)
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromDocumentDate.Clear();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToDocumentDate.Clear();
                }
                else if (formName == Resources.STR_CTSIDocumentsWithManualPayments)
                {
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromDocumentDate.Clear();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToDocumentDate.Clear();
                }
                else if (formName == Resources.STR_ReUploadFailureApiDocuments)
                {
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromDocumentDate.Clear();
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToDocumentDate.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        private void ShowReUploadVendorIdFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_ReUploadFailureCtsiDocuments)
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromVendorId.Show();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToVendorId.Show();
                }
                else if (formName == Resources.STR_CTSIDocumentsWithManualPayments)
                {
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromVendorId.Show();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToVendorId.Show();
                }
                else if (formName == Resources.STR_ReUploadFailureApiDocuments)
                {
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromVendorId.Show();
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToVendorId.Show();
                }
                else if (formName == Resources.STR_APIDocumentsWithManualPayments)
                {
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromVendorId.Show();
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToVendorId.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        private void ClearReUploadVendorIdFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_ReUploadFailureCtsiDocuments)
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromVendorId.Clear();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToVendorId.Clear();
                }
                else if (formName == Resources.STR_CTSIDocumentsWithManualPayments)
                {
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromVendorId.Clear();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToVendorId.Clear();
                }
                else if (formName == Resources.STR_ReUploadFailureApiDocuments)
                {
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromVendorId.Clear();
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToVendorId.Clear();
                }
                else if (formName == Resources.STR_APIDocumentsWithManualPayments)
                {
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromVendorId.Clear();
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToVendorId.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        private void ShowHeaderLookupButtons(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_ReUploadFailureCtsiDocuments)
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LookupButton1.Show();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LookupButton2.Show();
                }
                else if (formName == Resources.STR_CTSIDocumentsWithManualPayments)
                {
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LookupButton1.Show();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LookupButton2.Show();
                }
                else if (formName == Resources.STR_ReUploadFailureApiDocuments)
                {
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LookupButton1.Show();
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LookupButton2.Show();
                }
                else if (formName == Resources.STR_APIDocumentsWithManualPayments)
                {
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LookupButton1.Show();
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LookupButton2.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        private void ClearScrollingWindows(string formName, string scrollingWindowName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_ReUploadFailureCtsiDocuments)
                {
                    if (scrollingWindowName.ToLower() == Resources.STR_AllScroll.ToLower())
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                        TableError errorFailureTemp = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.ChangeFirst();
                        while (errorFailureTemp != TableError.EndOfTable && errorFailureTemp == TableError.NoError)
                        {
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Remove();
                            errorFailureTemp = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.ChangeNext();
                        }
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                        ctsiReUploadFailureDocumentsForm.Procedures.ReUploadFailureCtsiDocumentsFormScrollFill.Invoke();

                        ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Close();
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Release();
                        TableError errorValidTemp = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.ChangeFirst();
                        while (errorValidTemp != TableError.EndOfTable && errorValidTemp == TableError.NoError)
                        {
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Remove();
                            errorValidTemp = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.ChangeNext();
                        }
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Close();
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Release();
                        ctsiReUploadFailureDocumentsForm.Procedures.ReUploadValidatedCtsiRecordsFormScroll.Invoke();
                    }
                    else if (scrollingWindowName.ToLower() == Resources.STR_CTSIReUploadSelectDocumentsScroll.ToLower())
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                        TableError errorFailureTemp = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.ChangeFirst();
                        while (errorFailureTemp != TableError.EndOfTable && errorFailureTemp == TableError.NoError)
                        {
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Remove();
                            errorFailureTemp = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.ChangeNext();
                        }
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                        ctsiReUploadFailureDocumentsForm.Procedures.ReUploadFailureCtsiDocumentsFormScrollFill.Invoke();
                    }

                    else if (scrollingWindowName.ToLower() == Resources.STR_CTSIReUploadValidDocumentsScroll.ToLower())
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Close();
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Release();
                        TableError errorValidTemp = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.ChangeFirst();
                        while (errorValidTemp != TableError.EndOfTable && errorValidTemp == TableError.NoError)
                        {
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Remove();
                            errorValidTemp = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.ChangeNext();
                        }
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Close();
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Release();
                        ctsiReUploadFailureDocumentsForm.Procedures.ReUploadValidatedCtsiRecordsFormScroll.Invoke();
                    }
                }
                else if (formName == Resources.STR_ReUploadFailureApiDocuments)
                {
                    if (scrollingWindowName.ToLower() == Resources.STR_AllScroll.ToLower())
                    {
                        // Failure Non-PO Scrolling Window
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                        TableError errorNonPOFailureTemp = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.ChangeFirst();
                        while (errorNonPOFailureTemp != TableError.EndOfTable && errorNonPOFailureTemp == TableError.NoError)
                        {
                            apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Remove();
                            errorNonPOFailureTemp = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.ChangeNext();
                        }
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                        apiReUploadFailureDocumentsForm.Procedures.ApiNonPoReUploadDocumentsSelectScrollFill.Invoke();


                        // Valid Non-PO Scrolling Window
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Close();
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Release();
                        TableError errorNonPOValidTemp = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.ChangeFirst();
                        while (errorNonPOValidTemp != TableError.EndOfTable && errorNonPOValidTemp == TableError.NoError)
                        {
                            apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Remove();
                            errorNonPOValidTemp = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.ChangeNext();
                        }
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Close();
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Release();
                        apiReUploadFailureDocumentsForm.Procedures.ApiNonPoValidDocumentsScrollFill.Invoke();


                        // Failure PO Scrolling Window
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                        TableError errorPOFailureTemp = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ChangeFirst();
                        while (errorPOFailureTemp != TableError.EndOfTable && errorPOFailureTemp == TableError.NoError)
                        {
                            apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Remove();
                            errorPOFailureTemp = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ChangeNext();
                        }
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                        apiReUploadFailureDocumentsForm.Procedures.ApipoReUploadDocumentsSelectScrollFill.Invoke();


                        // Valid PO Scrolling Window
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Close();
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Release();
                        TableError errorPOValidTemp = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ChangeFirst();
                        while (errorPOValidTemp != TableError.EndOfTable && errorPOValidTemp == TableError.NoError)
                        {
                            apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Remove();
                            errorPOValidTemp = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ChangeNext();
                        }
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Close();
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Release();
                        apiReUploadFailureDocumentsForm.Procedures.ApipoValidDocumentsScrollFill.Invoke();

                    }
                    else if (scrollingWindowName.ToLower() == Resources.STR_APINonPOReUploadDocumentsSelectScroll.ToLower())
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                        TableError errorNonPOFailureTemp = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.ChangeFirst();
                        while (errorNonPOFailureTemp != TableError.EndOfTable && errorNonPOFailureTemp == TableError.NoError)
                        {
                            apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Remove();
                            errorNonPOFailureTemp = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.ChangeNext();
                        }
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                        apiReUploadFailureDocumentsForm.Procedures.ApiNonPoReUploadDocumentsSelectScrollFill.Invoke();
                    }

                    else if (scrollingWindowName.ToLower() == Resources.STR_APINonPOValidDocumentsScroll.ToLower())
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Close();
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Release();
                        TableError errorNonPOValidTemp = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.ChangeFirst();
                        while (errorNonPOValidTemp != TableError.EndOfTable && errorNonPOValidTemp == TableError.NoError)
                        {
                            apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Remove();
                            errorNonPOValidTemp = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.ChangeNext();
                        }
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Close();
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Release();
                        apiReUploadFailureDocumentsForm.Procedures.ApiNonPoValidDocumentsScrollFill.Invoke();
                    }

                    else if (scrollingWindowName.ToLower() == Resources.STR_APIPOReUploadDocumentsSelectScroll.ToLower())
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                        TableError errorPOFailureTemp = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ChangeFirst();
                        while (errorPOFailureTemp != TableError.EndOfTable && errorPOFailureTemp == TableError.NoError)
                        {
                            apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Remove();
                            errorPOFailureTemp = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ChangeNext();
                        }
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                        apiReUploadFailureDocumentsForm.Procedures.ApipoReUploadDocumentsSelectScrollFill.Invoke();
                    }

                    else if (scrollingWindowName.ToLower() == Resources.STR_APIPOValidDocumentsScroll.ToLower())
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Close();
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Release();
                        TableError errorPOValidTemp = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ChangeFirst();
                        while (errorPOValidTemp != TableError.EndOfTable && errorPOValidTemp == TableError.NoError)
                        {
                            apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Remove();
                            errorPOValidTemp = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ChangeNext();
                        }
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Close();
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Release();
                        apiReUploadFailureDocumentsForm.Procedures.ApipoValidDocumentsScrollFill.Invoke();
                    }
                }
                else if (formName == Resources.STR_CTSIDocumentsWithManualPayments)
                {
                    ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.RangeClear();
                    ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Ctsiid.Clear();
                    ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.RangeStart();
                    ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Ctsiid.Fill();
                    ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.RangeEnd();
                    ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.RangeRemove();
                    ctsiDocumentsWithManualPaymentsForm.Procedures.CtsiDocumentsWithManualPaymentsFillScroll.Invoke();

                }

                else if (formName == Resources.STR_APIDocumentsWithManualPayments)
                {
                    ////PO Invoice
                    apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.RangeClear();
                    apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.DocumentId.Clear();
                    apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.RangeStart();
                    apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.DocumentId.Fill();
                    apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.RangeEnd();
                    apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.RangeRemove();
                    apiDocumentsWithManualPaymentsForm.Procedures.ApiDocumentsWithManualPaymentsPoFillScroll.Invoke();

                    ////Non-PO Invoice
                    apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.RangeClear();
                    apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.DocumentId.Clear();
                    apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.RangeStart();
                    apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.DocumentId.Fill();
                    apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.RangeEnd();
                    apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.RangeRemove();
                    apiDocumentsWithManualPaymentsForm.Procedures.ApiDocumentsWithManualPaymentsNonPoFillScroll.Invoke();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }


        #endregion PrivateMethodsReLoadAndLinkedPayments

        #region CTSIReuploadfailedDocuments
        /// <summary>
        /// CTSI ReUpload Failure Documents
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormOpenBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!RegisterReUploadCtsi)
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.RedisplayButton.ClickAfterOriginal += new EventHandler(ctsiReUploadFailureDocumentsFormRedisplayButtonClickAfterOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalDocumentBy.Change += new EventHandler(ReUploadFailureCtsiFormLocalDocumentByChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ReValidate.ClickAfterOriginal += new EventHandler(ctsiReUploadFailureDocumentsFormReValidateClickAfterOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LookupButton1.ClickAfterOriginal += new EventHandler(ctsiReUploadFailureDocumentsFormLookupButton1ClickAfterOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LookupButton2.ClickAfterOriginal += new EventHandler(ctsiReUploadFailureDocumentsFormLookupButton2ClickAfterOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ClearButton.ClickAfterOriginal += new EventHandler(ctsiReUploadFailureDocumentsFormClearButtonClickAfterOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.AllOrRange.Change += new EventHandler(ctsiReUploadFailureDocumentsFormAllOrRangeChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromCtsiId.Change += new EventHandler(ctsiReUploadFailureDocumentsFormLocalFromCtsiIdChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToCtsiId.Change += new EventHandler(ctsiReUploadFailureDocumentsFormLocalToCtsiIdChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromVendorId.Change += new EventHandler(ctsiReUploadFailureDocumentsFormLocalFromVendorIdChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToVendorId.Change += new EventHandler(ctsiReUploadFailureDocumentsFormLocalToVendorIdChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.PushToGp.ClickAfterOriginal += new EventHandler(ctsiReUploadFailureDocumentsFormPushToGpClickAfterOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.OkButton.ClickAfterOriginal += new EventHandler(ctsiReUploadFailureDocumentsFormOkButtonClickAfterOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.SelectCheckBox.Change += new EventHandler(ctsiReUploadFailureDocumentsFormSelectCheckBoxChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.DocumentDate.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormDocumentDateEnterBeforeOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.DocumentDate.Change += new EventHandler(ctsiReUploadFailureDocumentsFormDocumentDateChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.ApprovedAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormApprovedAmountEnterBeforeOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.ApprovedAmount.Change += new EventHandler(ctsiReUploadFailureDocumentsFormApprovedAmountChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.TotalApprovedDocumentAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormTotalApprovedDocumentAmountEnterBeforeOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.TotalApprovedDocumentAmount.Change += new EventHandler(ctsiReUploadFailureDocumentsFormTotalApprovedDocumentAmountChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.VendorId.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormVendorIdEnterBeforeOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.VendorId.Change += new EventHandler(ctsiReUploadFailureDocumentsFormVendorIdChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.TotalOverChargeAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormTotalOverChargeAmountEnterBeforeOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.TotalOverChargeAmount.Change += new EventHandler(ctsiReUploadFailureDocumentsFormTotalOverChargeAmountChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.FreightAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormFreightAmountEnterBeforeOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.FreightAmount.Change += new EventHandler(ctsiReUploadFailureDocumentsFormFreightAmountChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.MiscAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormMiscAmountEnterBeforeOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.MiscAmount.Change += new EventHandler(ctsiReUploadFailureDocumentsFormMiscAmountChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.TradeDiscountAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormTradeDiscountAmountEnterBeforeOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.TradeDiscountAmount.Change += new EventHandler(ctsiReUploadFailureDocumentsFormTradeDiscountAmountChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.PurchasesAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormPurchasesAmountEnterBeforeOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.PurchasesAmount.Change += new EventHandler(ctsiReUploadFailureDocumentsFormPurchasesAmountChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.TaxAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormTaxAmountEnterBeforeOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.TaxAmount.Change += new EventHandler(ctsiReUploadFailureDocumentsFormTaxAmountChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.CpReference.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormCpReferenceEnterBeforeOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.CpReference.Change += new EventHandler(ctsiReUploadFailureDocumentsFormCpReferenceChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.CtsiAirwayInvoiceNumber.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormCtsiAirwayInvoiceNumberEnterBeforeOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.CtsiAirwayInvoiceNumber.Change += new EventHandler(ctsiReUploadFailureDocumentsFormCtsiAirwayInvoiceNumberChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.GlAccountNumber.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormGlAccountNumberEnterBeforeOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.GlAccountNumber.Change += new EventHandler(ctsiReUploadFailureDocumentsFormGlAccountNumberChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.VoucherNumber.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormVoucherNumberEnterBeforeOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.VoucherNumber.Change += new EventHandler(ctsiReUploadFailureDocumentsFormVoucherNumberChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.GlAccountDescription.Change += new EventHandler(ctsiReUploadFailureDocumentsFormGlAccountDescriptionChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.GlAccountDescription.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormGlAccountDescriptionEnterBeforeOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.CurrencyCode.Change += new EventHandler(ctsiReUploadFailureDocumentsFormCurrencyCodeChange);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.CurrencyCode.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormCurrencyCodeEnterBeforeOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.LineFillAfterOriginal += new EventHandler(CtsiReUploadDocumentsSelectScrollLineFillAfterOriginal);
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiValidDocumentsScroll.LineFillAfterOriginal += new EventHandler(CtsiValidDocumentsScroll_LineFillAfterOriginal);
                    if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_CPEURCompany)
                    {
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.BaseLocalCharge.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormBaseLocalChargeEnterBeforeOriginal);
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.BaseLocalCharge.Change += new EventHandler(ctsiReUploadFailureDocumentsFormBaseLocalChargeChange);
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.BaseReverseCharge.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormBaseReverseChargeEnterBeforeOriginal);
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.BaseReverseCharge.Change += new EventHandler(ctsiReUploadFailureDocumentsFormBaseReverseChargeChange);
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.BaseZeroRatedCharge.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormBaseZeroRatedChargeEnterBeforeOriginal);
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.BaseZeroRatedCharge.Change += new EventHandler(ctsiReUploadFailureDocumentsFormBaseZeroRatedChargeChange);
                    }
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CloseAfterOriginal += new EventHandler(ctsiReUploadFailureDocumentsFormCtsiReUploadFailureDocumentsCloseAfterOriginal);
                    RegisterReUploadCtsi = true;
                }
                ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = false;
                ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ScrollShrinkSwitch.Value = 1;
                ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CpScrollShrinkSwitch.Value = 1;
                ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalShrinkButton.RunValidate();
                ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalShrinkButton1.RunValidate();
                ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalDocumentBy.Value = 1;
                InitilizeSortBySelection(Resources.STR_ReUploadFailureCtsiDocuments);
                ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.PushToGp.Lock();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CtsiValidDocumentsScroll_LineFillAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_CPEURCompany)
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiValidDocumentsScroll.LocalHideValidTaxDetailsScroll.Hide();
                }
                else
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiValidDocumentsScroll.LocalHideValidTaxDetailsScroll.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CtsiReUploadDocumentsSelectScrollLineFillAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_CPEURCompany)
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.LocalHideSelectTaxDetailsScroll.Hide();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.BaseLocalCharge.Show();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.BaseReverseCharge.Show();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.BaseZeroRatedCharge.Show();

                }
                else
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.LocalHideSelectTaxDetailsScroll.Show();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.BaseLocalCharge.Hide();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.BaseReverseCharge.Hide();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.BaseZeroRatedCharge.Hide();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
        }
        /// <summary>
        /// CTSI Re Upload Failure Documents Form Re-display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormRedisplayButtonClickAfterOriginal(object sender, EventArgs e)
        {
            PayableManagementRequest requestObj = null;
            StringBuilder logMessage = new StringBuilder();
            bool isValid = false;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                logMessage.AppendLine(DateTime.Now + " Started : Filling data to Re-Upload CTSI Documents: "
                    + " by user : " + Dynamics.Globals.UserId.Value.ToString()
                    + " in company : " + Dynamics.Globals.IntercompanyId.Value.ToString());
                requestObj = new PayableManagementRequest();

                if ((ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalDocumentBy.Value == 1)
                       && (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.AllOrRange.Value == 1)
                         && (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromCtsiId.Value != string.Empty)
                               && (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToCtsiId.Value != string.Empty))
                {
                    requestObj.SearchType = Resources.STR_SearchType_CtsiId;
                    requestObj.FromCtsiId = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromCtsiId.Value;
                    requestObj.ToCtsiId = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToCtsiId.Value;
                    requestObj.FromVendorId = string.Empty;
                    requestObj.ToVendorId = string.Empty;
                    requestObj.FromDocumentDate = Convert.ToDateTime("01/01/1900");
                    requestObj.ToDocumentDate = Convert.ToDateTime("01/01/1900");
                    isValid = true;

                }
                else if ((ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalDocumentBy.Value == 2)
                       && (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.AllOrRange.Value == 1)
                         && (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromDocumentDate.Value != DateTime.MinValue)
                               && (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToDocumentDate.Value != DateTime.MinValue))
                {
                    requestObj.SearchType = Resources.STR_SearchType_DocumentDate;
                    requestObj.FromDocumentDate = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromDocumentDate.Value;
                    requestObj.ToDocumentDate = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToDocumentDate.Value;
                    requestObj.FromVendorId = string.Empty;
                    requestObj.ToVendorId = string.Empty;
                    requestObj.FromCtsiId = string.Empty;
                    requestObj.ToCtsiId = string.Empty;
                    isValid = true;

                }
                else if ((ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalDocumentBy.Value == 3)
                       && (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.AllOrRange.Value == 1)
                         && (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromVendorId.Value != string.Empty)
                               && (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToVendorId.Value != string.Empty))
                {
                    requestObj.SearchType = Resources.STR_SearchType_VendorId;
                    requestObj.FromVendorId = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromVendorId.Value;
                    requestObj.ToVendorId = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToVendorId.Value;
                    requestObj.FromDocumentDate = Convert.ToDateTime("01/01/1900");
                    requestObj.ToDocumentDate = Convert.ToDateTime("01/01/1900");
                    requestObj.FromCtsiId = string.Empty;
                    requestObj.ToCtsiId = string.Empty;
                    isValid = true;

                }
                else if (((ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalDocumentBy.Value == 1)
                            || (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalDocumentBy.Value == 2)
                                || (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalDocumentBy.Value == 3))
                                    && (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.AllOrRange.Value == 0))
                {
                    requestObj.SearchType = Resources.STR_SearchType_All;
                    requestObj.FromVendorId = string.Empty;
                    requestObj.ToVendorId = string.Empty;
                    requestObj.FromDocumentDate = Convert.ToDateTime("01/01/1900");
                    requestObj.ToDocumentDate = Convert.ToDateTime("01/01/1900");
                    requestObj.FromCtsiId = string.Empty;
                    requestObj.ToCtsiId = string.Empty;
                    isValid = true;
                }
                else
                {
                    MessageBox.Show(Resources.STR_RequiredFieldsMissing, Resources.STR_MESSAGE_TITLE);
                }
                if (isValid)
                {
                    requestObj.userId = Dynamics.Globals.UserId.Value.ToString();
                    requestObj.Company = Dynamics.Globals.IntercompanyId.Value.ToString();
                    requestObj.companyId = Dynamics.Globals.CompanyId.Value;

                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/PayableManagement/GetFailedCtsiDocuments", requestObj);
                        if (response.Result.IsSuccessStatusCode)
                        {
                            PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                            if (payableResponse.Status == Response.Error)
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                MessageBox.Show(Resources.STR_ErrorPopulatingFailedCtsiDocuments);
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ctsiReUploadFailureDocumentsFormRedisplayButtonClickAfterOriginal: " + payableResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                if (payableResponse != null)
                                {
                                    ReloadFailedCtsiDocumentscrollContents(payableResponse);
                                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = false;
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: ctsiReUploadFailureDocumentsFormRedisplayButtonClickAfterOriginal");
                            MessageBox.Show("Error: ctsiReUploadFailureDocumentsFormRedisplayButtonClickAfterOriginal", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
                requestObj = null;
            }
        }
        /// <summary>
        /// CTSI Filter methods
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ReUploadFailureCtsiFormLocalDocumentByChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                InitilizeSortBySelection(Resources.STR_ReUploadFailureCtsiDocuments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormCtsiReUploadFailureDocumentsCloseAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiIdLookupForm.IsOpen)
                {
                    ctsiIdLookupForm.Close();
                }
                if (vendorLookupForm.IsOpen)
                {
                    vendorLookupForm.Close();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormVendorIdChange(object sender, EventArgs e)
        {
            string oldVendorId = "";
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.OriginialCtsiInvoiceId.Value;
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Key = 1;
                TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldVendorId = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.VendorId.Value;
                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.VendorId.Value != string.Empty
                        && ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.VendorId.Value != oldVendorId)
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Close();
                        ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Release();
                        ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.VendorId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.VendorId.Value;
                        ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Key = 1;
                        TableError Vendorerror = ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Get();

                        if (Vendorerror != TableError.NoError)
                        {
                            // if not valid displays a message
                            MessageBox.Show(Resources.STR_VendorIdDoesNotExists, Resources.STR_MESSAGE_TITLE);
                            ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.VendorId.Value = oldVendorId;

                        }
                        else
                        {
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.VendorId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.VendorId.Value;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                            ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;

                        }
                        ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Close();
                        ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Release();
                    }
                    else if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.VendorId.Value == string.Empty)
                    {
                        MessageBox.Show(Resources.STR_EmptyVendorId, Resources.STR_MESSAGE_TITLE);
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.VendorId.Value = oldVendorId;
                    }
                }
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormVendorIdEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingFields(Resources.STR_ReUploadFailureCtsiDocuments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormTotalApprovedDocumentAmountChange(object sender, EventArgs e)
        {
            decimal oldTotApprovedAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.OriginialCtsiInvoiceId.Value;
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Key = 1;
                TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldTotApprovedAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.TotalApprovedDocumentAmount.Value;
                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.TotalApprovedDocumentAmount.Value != 0
                            && ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.TotalApprovedDocumentAmount.Value != oldTotApprovedAmount)
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.TotalApprovedDocumentAmount.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.TotalApprovedDocumentAmount.Value;
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                    else if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.TotalApprovedDocumentAmount.Value == 0)
                    {
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.TotalApprovedDocumentAmount.Value = oldTotApprovedAmount;
                        MessageBox.Show(Resources.STR_ZeroApprovedAmount, Resources.STR_MESSAGE_TITLE);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormTotalApprovedDocumentAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingFields(Resources.STR_ReUploadFailureCtsiDocuments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormApprovedAmountChange(object sender, EventArgs e)
        {

            decimal oldApprovedAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.OriginialCtsiInvoiceId.Value;
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Key = 1;
                TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldApprovedAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.ApprovedAmount.Value;
                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.ApprovedAmount.Value != 0
                            && ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.ApprovedAmount.Value != oldApprovedAmount)
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.ApprovedAmount.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.ApprovedAmount.Value;
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                    else if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.ApprovedAmount.Value == 0)
                    {
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.ApprovedAmount.Value = oldApprovedAmount;
                        MessageBox.Show(Resources.STR_ZeroApprovedAmount, Resources.STR_MESSAGE_TITLE);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormApprovedAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingFields(Resources.STR_ReUploadFailureCtsiDocuments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormDocumentDateChange(object sender, EventArgs e)
        {
            DateTime oldDocdate;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.OriginialCtsiInvoiceId.Value;
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Key = 1;
                TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldDocdate = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.DocumentDate.Value;
                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.DocumentDate.Value.ToString() != "01/01/1900"
                            && ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.DocumentDate.Value != oldDocdate)
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.DocumentDate.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.DocumentDate.Value;
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                    else if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.DocumentDate.Value.ToString() == "01/01/1900")
                    {
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.DocumentDate.Value = oldDocdate;
                        MessageBox.Show(Resources.STR_EmptyDocDate, Resources.STR_MESSAGE_TITLE);
                    }
                }
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormDocumentDateEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingFields(Resources.STR_ReUploadFailureCtsiDocuments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormSelectCheckBoxChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormOkButtonClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiReUploadFailureDocumentsForm.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormPushToGpClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            PayableManagementRequest reqObj = null;
            try
            {
                if (!ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value)
                {
                    reqObj = new PayableManagementRequest();
                    PayableManagementEntity payableManagementEntity = new PayableManagementEntity();

                    List<PayableLineEntity> ctsiValidationList = new List<PayableLineEntity>();
                    List<PayableLineEntity> ctsiValidationTaxList = new List<PayableLineEntity>();
                    payableManagementEntity.SourceFormName = Resources.STR_ReUploadFailureCtsiDocuments;
                    payableManagementEntity.FileName = string.Empty;
                    reqObj.PayableManagementDetails = payableManagementEntity;
                    reqObj.Company = Dynamics.Globals.IntercompanyId.Value;
                    reqObj.UserId = Dynamics.Globals.UserName.ToString().Trim();

                    if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_ChmptCompany)
                    {
                        reqObj.companyId = Convert.ToInt16(Resources.NACompanyId);
                    }
                    else
                    {
                        reqObj.companyId = Convert.ToInt16(Resources.EUCompanyId);
                    }
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Close();
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Release();
                    TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.GetFirst();

                    //DataTable invoiceDetailsDT = GetInvoiceDetails(reqObj, tableError);
                    //DataSet invoiceDetailsDT = new DataSet("@LineDetails");
                    //invoiceDetailsDT.Tables.Add(new DataTable());
                    //invoiceDetailsDT.Tables[0].Columns.Add("CTSIId", typeof(string));
                    //invoiceDetailsDT.Tables[0].Columns.Add("OriginialCTSIInvoiceId", typeof(int));
                    //invoiceDetailsDT.Tables[0].Columns.Add("CtsiFileId", typeof(int));
                    //invoiceDetailsDT.Tables[0].Columns.Add("DocumentNumber", typeof(string));
                    //invoiceDetailsDT.Tables[0].Columns.Add("DocumentDate", typeof(DateTime));
                    //invoiceDetailsDT.Tables[0].Columns.Add("VendorId", typeof(string));
                    //invoiceDetailsDT.Tables[0].Columns.Add("TotalApprovedDocumentAmount", typeof(decimal));
                    //invoiceDetailsDT.Tables[0].Columns.Add("ApprovedAmount", typeof(decimal));
                    //invoiceDetailsDT.Tables[0].Columns.Add("OverCharge", typeof(decimal));
                    //invoiceDetailsDT.Tables[0].Columns.Add("FreightAmount", typeof(decimal));
                    //invoiceDetailsDT.Tables[0].Columns.Add("TaxAmount", typeof(decimal));
                    //invoiceDetailsDT.Tables[0].Columns.Add("MiscellaneousAmount", typeof(decimal));
                    //invoiceDetailsDT.Tables[0].Columns.Add("TradeDiscounts", typeof(decimal));
                    //invoiceDetailsDT.Tables[0].Columns.Add("PurchaseAmount", typeof(decimal));
                    //invoiceDetailsDT.Tables[0].Columns.Add("CurrencyCode", typeof(string));
                    //invoiceDetailsDT.Tables[0].Columns.Add("GlAccount", typeof(string));
                    //invoiceDetailsDT.Tables[0].Columns.Add("GLAccountDescription", typeof(string));
                    //invoiceDetailsDT.Tables[0].Columns.Add("CptReference", typeof(string));
                    //invoiceDetailsDT.Tables[0].Columns.Add("AirWayInvoiceNumber", typeof(string));
                    //invoiceDetailsDT.Tables[0].Columns.Add("StatusId", typeof(int));
                    //invoiceDetailsDT.Tables[0].Columns.Add("ValidDocumentNumber", typeof(string));
                    //invoiceDetailsDT.Tables[0].Columns.Add("UserId", typeof(string));
                    //invoiceDetailsDT.Tables[0].Columns.Add("Notes", typeof(string));
                    //invoiceDetailsDT.Tables[0].Columns.Add("CurrencyDecimalPlaces", typeof(int));
                    //invoiceDetailsDT.Tables[0].Columns.Add("CreditAccountNumber", typeof(string));
                    //invoiceDetailsDT.Tables[0].Columns.Add("DebitDistributionType", typeof(int));
                    //invoiceDetailsDT.Tables[0].Columns.Add("CreditAmount", typeof(decimal));
                    //invoiceDetailsDT.Tables[0].Columns.Add("ErrorDescription", typeof(string));
                    //if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_CPEURCompany)
                    //{
                    //    invoiceDetailsDT.Tables[0].Columns.Add("BaseLocalCharge", typeof(decimal));
                    //    invoiceDetailsDT.Tables[0].Columns.Add("BaseZeroRatedCharge", typeof(decimal));
                    //    invoiceDetailsDT.Tables[0].Columns.Add("BaseReverseCharge", typeof(decimal));
                    //}

                    while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        //DataRow newRow = invoiceDetailsDT.Tables[0].NewRow();
                        //newRow["CTSIId"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Ctsiid.Value;
                        //newRow["OriginialCTSIInvoiceId"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value;
                        //newRow["CtsiFileId"] = 0;
                        //newRow["DocumentNumber"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.VoucherNumber.Value;
                        //newRow["DocumentDate"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.DocumentDate.Value;
                        //newRow["VendorId"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.VendorId.Value;
                        //newRow["TotalApprovedDocumentAmount"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.TotalApprovedDocumentAmount.Value;
                        //newRow["ApprovedAmount"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.ApprovedAmount.Value;
                        //newRow["OverCharge"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.TotalOverChargeAmount.Value;
                        //newRow["FreightAmount"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.FreightAmount.Value;
                        //newRow["TaxAmount"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.TaxAmount.Value;
                        //newRow["MiscellaneousAmount"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.MiscAmount.Value;
                        //newRow["TradeDiscounts"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.TradeDiscountAmount.Value;
                        //newRow["PurchaseAmount"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.PurchasesAmount.Value;
                        //newRow["CurrencyCode"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.CurrencyCode.Value;
                        //newRow["GlAccount"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.GlAccountNumber.Value;
                        //newRow["GLAccountDescription"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.GlAccountDescription.Value;
                        //newRow["CptReference"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.CpReference.Value;
                        //newRow["AirWayInvoiceNumber"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.CtsiAirwayInvoiceNumber.Value;
                        //newRow["StatusId"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Status.Value;
                        //newRow["ValidDocumentNumber"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.ValidDocumentNumber.Value;
                        //newRow["UserId"] = Dynamics.Globals.UserId.Value.ToString();
                        //newRow["CreditAccountNumber"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.CpCreditAccountNumber.Value;
                        //newRow["DebitDistributionType"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.DebitDistributionType.Value;
                        //newRow["CurrencyDecimalPlaces"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.CurrencyDecimalPlaces.Value;
                        //newRow["CreditAmount"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.CreditAmount.Value;
                        //newRow["ErrorDescription"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.ErrorMessageText.Value;
                        //newRow["Notes"] = string.Empty;
                        //if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_CPEURCompany)
                        //{
                        //    newRow["BaseLocalCharge"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.BaseLocalCharge.Value;
                        //    newRow["BaseZeroRatedCharge"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.BaseZeroRatedCharge.Value;
                        //    newRow["BaseReverseCharge"] = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.BaseReverseCharge.Value;
                        //}

                        //invoiceDetailsDT.Tables[0].Rows.Add(newRow);

                        
                        PayableLineEntity ctsiValidation = new PayableLineEntity();

                        ctsiValidation.CtsiId = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Ctsiid.Value;
                        ctsiValidation.OriginalCTSIInvoiceId= ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value;
                        ctsiValidation.CtsiFileId = 0;
                        ctsiValidation.DocumentNumber = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.VoucherNumber.Value;
                        ctsiValidation.DocumentDate = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.DocumentDate.Value;
                        ctsiValidation.VendorName= ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.VendorId.Value;
                        ctsiValidation.TotalApprovedDocumentAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.TotalApprovedDocumentAmount.Value;
                        ctsiValidation.ApprovedAmount= ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.ApprovedAmount.Value;
                        ctsiValidation.OverCharge = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.TotalOverChargeAmount.Value;
                        ctsiValidation.FreightAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.FreightAmount.Value;
                        ctsiValidation.TaxAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.TaxAmount.Value;
                        ctsiValidation.MiscellaneousAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.MiscAmount.Value;
                        ctsiValidation.TradeDiscounts = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.TradeDiscountAmount.Value;
                        ctsiValidation.PurchaseAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.PurchasesAmount.Value;
                        ctsiValidation.CurrencyCode = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.CurrencyCode.Value;
                        ctsiValidation.GLAccount = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.GlAccountNumber.Value;
                        ctsiValidation.GLAccountDescription= ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.GlAccountDescription.Value;
                        ctsiValidation.CPTReference = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.CpReference.Value;
                        ctsiValidation.AirwayInvoiceNumber = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.CtsiAirwayInvoiceNumber.Value;
                        ctsiValidation.StatusId = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Status.Value;
                        ctsiValidation.ValidDocumentNumber = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.ValidDocumentNumber.Value;
                        ctsiValidation.UserId = Dynamics.Globals.UserId.Value.ToString();
                        ctsiValidation.CreditAccountNumber = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.CpCreditAccountNumber.Value;
                        ctsiValidation.DebitDistributionType = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.DebitDistributionType.Value;
                        ctsiValidation.CurrencyDecimal = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.CurrencyDecimalPlaces.Value;
                        ctsiValidation.CreditAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.CreditAmount.Value;
                        ctsiValidation.ErrorDescription = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.ErrorMessageText.Value;
                        ctsiValidation.Notes = string.Empty;
                        ctsiValidation.BaseLocalCharge = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.BaseLocalCharge.Value;
                        ctsiValidation.BaseZeroRatedCharge = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.BaseZeroRatedCharge.Value;
                        ctsiValidation.BaseReverseCharge = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.BaseReverseCharge.Value;

                        ctsiValidationList.Add(ctsiValidation);
                        ctsiValidation = null;
                        tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.GetNext();
                    }

                    if (ctsiValidationList.Count > 0 && ctsiValidationList != null)
                    {
                        if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_CPEURCompany && taxDetails != null && taxDetails.Rows.Count > 0)
                        {
                            ctsiValidationTaxList = GetAllEntities<PayableLineEntity, PayableManagementCTSITaxMap>(taxDetails).ToList();
                            reqObj.CTSITaxValidationList = ctsiValidationTaxList;
                        }
                        // reqObj.CtsiDtValue = ConvertDataSetToString(invoiceDetailsDT);
                        reqObj.CTSIValidationList = ctsiValidationList;


                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/PayableManagement/UploadPayableDetailsIntoGpForCtsi", reqObj);
                            if (response.Result.IsSuccessStatusCode)
                            {
                                PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                                if (payableResponse.Status == Response.Error)
                                {
                                    logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxDetailMaintenance_DeleteButtonClickAfterOriginal Method (DeleteTaxDetailCustomRecord): " + payableResponse.ErrorMessage.ToString());
                                    MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                    InitilizeSortBySelection(Resources.STR_ReUploadFailureCtsiDocuments);
                                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.PushToGp.Lock();
                                }
                                MessageBox.Show(payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Expense Report Upload PushToGp ClickAfterOriginal");
                                MessageBox.Show("Error: Expense Report Upload PushToGp ClickAfterOriginal", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show(Resources.STR_FieldsUpdated, Resources.STR_MESSAGE_TITLE);
                }
            }
            catch (Exception ex)
            {
                if (ex is TimeoutException)
                {
                    MessageBox.Show(Resources.STR_UnknownException, Resources.STR_MESSAGE_TITLE);
                }
                else
                {
                    MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                }
                logMessage.AppendLine(DateTime.Now + " Error: " + ex.Message);
                InitilizeSortBySelection(Resources.STR_ReUploadFailureExpenses);
            }
            finally
            {
                ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Release();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Close();
                if (isCtsiLoggingEnabled && logMessage.ToString() != string.Empty)
                {
                    logMessage.AppendLine(DateTime.Now + "--------------------------------------------------------------");
                    LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                }
                reqObj = null;
            }
        }

        protected IEnumerable<TModel> GetAllEntities<TModel, TMap>(DataTable dt)
            where TModel : class, IModelBase, new()
            where TMap : class, IDataRowMap<TModel>, new()
        {
            var lst = new List<TModel>();

            foreach (DataRow dataRow in dt.Rows)
                lst.Add(dataRow.SelectRow(MapperFactory<TModel, TMap>.Mapper().Map));
            return lst;
        }

        private static string ConvertDataSetToString(DataSet dataSet)
        {
            string data = string.Empty;
            int totCount = dataSet.Tables.Count;

            for (int count = 0; count < totCount; count++)
            {
                int rowsCount = dataSet.Tables[count].Rows.Count;
                for (int rowValue = 0; rowValue < rowsCount; rowValue++)
                {
                    DataRow row = dataSet.Tables[count].Rows[rowValue];
                    int columnsCount = dataSet.Tables[count].Columns.Count;
                    for (int colValue = 0; colValue < columnsCount; colValue++)
                    {
                        data += dataSet.Tables[count].Columns[colValue].ColumnName + "~" + row[colValue];
                        if (colValue == columnsCount - 1)
                        {
                            if (rowValue != (rowsCount - 1))
                                data += "#";
                        }
                        else
                            data += "|";
                    }
                }
            }
            return data;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormLocalToVendorIdChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToVendorId.Value != string.Empty)
                {
                    ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Close();
                    ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Release();
                    ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.VendorId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToVendorId.Value;
                    ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Key = 1;
                    TableError Vendorerror = ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Get();

                    if (Vendorerror != TableError.NoError)
                    {
                        // if not valid displays a message
                        MessageBox.Show(Resources.STR_VendorIdDoesNotExists, Resources.STR_MESSAGE_TITLE);
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToVendorId.Clear();

                    }
                    else
                    {
                        if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToVendorId.Value == string.Empty)
                        {
                            if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromVendorId.Value == string.Empty)
                            {
                                ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.AllOrRange.Value = 0;
                            }
                        }
                        else
                        {
                            ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.AllOrRange.Value = 1;
                        }
                    }
                    ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Close();
                    ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Release();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormLocalFromVendorIdChange(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromVendorId.Value != string.Empty)
                {
                    ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Close();
                    ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Release();
                    ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.VendorId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromVendorId.Value;
                    ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Key = 1;
                    TableError Vendorerror = ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Get();

                    if (Vendorerror != TableError.NoError)
                    {
                        // if not valid displays a message
                        MessageBox.Show(Resources.STR_VendorIdDoesNotExists, Resources.STR_MESSAGE_TITLE);
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromVendorId.Clear();
                    }
                    else
                    {
                        if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromVendorId.Value == string.Empty)
                        {
                            if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToVendorId.Value == string.Empty)
                            {
                                ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.AllOrRange.Value = 0;
                            }
                        }
                        else
                        {
                            ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.AllOrRange.Value = 1;
                            if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToVendorId.Value == string.Empty)
                            {
                                ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToVendorId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromVendorId.Value;
                            }
                        }
                    }

                    ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Close();
                    ctsiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Release();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormLocalToCtsiIdChange(object sender, EventArgs e)
        {
            PayableManagementRequest requestObj = null;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToCtsiId.Value != string.Empty)
                {
                    requestObj = new PayableManagementRequest();
                    requestObj.SourceLookup = Resources.STR_CTSIId;
                    requestObj.LookupValue = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToCtsiId.Value.ToString();
                    requestObj.companyId = Dynamics.Globals.CompanyId.Value;

                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/PayableManagement/VerifyLookupValueExists", requestObj);
                        if (response.Result.IsSuccessStatusCode)
                        {
                            PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                            if (payableResponse.Status == Response.Error)
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ctsiReUploadFailureDocumentsFormLocalToCtsiIdChange: " + payableResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                if (!payableResponse.LookupValueExists)
                                {
                                    MessageBox.Show(Resources.STR_CTSIIdDoesNotExists, Resources.STR_MESSAGE_TITLE);
                                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToCtsiId.Value = string.Empty;
                                }
                                else
                                {
                                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToCtsiId.Value == string.Empty)
                                    {
                                        if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToCtsiId.Value == string.Empty)
                                        {
                                            ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.AllOrRange.Value = 0;
                                        }
                                    }
                                    else
                                    {
                                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.AllOrRange.Value = 1;
                                    }
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: ctsiReUploadFailureDocumentsFormLocalToCtsiIdChange");
                            MessageBox.Show("Error: ctsiReUploadFailureDocumentsFormLocalToCtsiIdChange", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
                requestObj = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormLocalFromCtsiIdChange(object sender, EventArgs e)
        {
            PayableManagementRequest requestObj = null;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromCtsiId.Value != string.Empty)
                {
                    requestObj = new PayableManagementRequest();
                    requestObj.SourceLookup = Resources.STR_CTSIId;
                    requestObj.LookupValue = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromCtsiId.Value.ToString();
                    requestObj.companyId = Dynamics.Globals.CompanyId.Value;

                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/PayableManagement/VerifyLookupValueExists", requestObj);
                        if (response.Result.IsSuccessStatusCode)
                        {
                            PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                            if (payableResponse.Status == Response.Error)
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ctsiReUploadFailureDocumentsFormLocalFromCtsiIdChange: " + payableResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                if (!payableResponse.LookupValueExists)
                                {
                                    MessageBox.Show(Resources.STR_CTSIIdDoesNotExists, Resources.STR_MESSAGE_TITLE);
                                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromCtsiId.Clear();
                                }
                                else
                                {
                                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromCtsiId.Value == string.Empty)
                                    {
                                        if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToCtsiId.Value == string.Empty)
                                        {
                                            ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.AllOrRange.Value = 0;
                                        }
                                    }
                                    else
                                    {
                                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.AllOrRange.Value = 1;
                                        if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToCtsiId.Value == string.Empty)
                                        {
                                            ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToCtsiId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromCtsiId.Value;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: ctsiReUploadFailureDocumentsFormLocalFromCtsiIdChange");
                            MessageBox.Show("Error: ctsiReUploadFailureDocumentsFormLocalFromCtsiIdChange", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
                requestObj = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormAllOrRangeChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalDocumentBy.Value == 1
                        && ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.AllOrRange.Value == 0)
                {
                    ClearReUploadCtsiFields(Resources.STR_ReUploadFailureCtsiDocuments);
                }
                else if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalDocumentBy.Value == 2
                            && ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.AllOrRange.Value == 0)
                {
                    ClearReUploadDocumentDateFields(Resources.STR_ReUploadFailureCtsiDocuments);
                }
                else if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalDocumentBy.Value == 3
                            && ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.AllOrRange.Value == 0)
                {
                    ClearReUploadVendorIdFields(Resources.STR_ReUploadFailureCtsiDocuments);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormClearButtonClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ScrollShrinkSwitch.Value = 1;
                ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CpScrollShrinkSwitch.Value = 1;
                ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalShrinkButton.RunValidate();
                ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalShrinkButton1.RunValidate();
                ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalDocumentBy.Value = 1;
                InitilizeSortBySelection(Resources.STR_ReUploadFailureCtsiDocuments);
                ClearScrollingWindows(Resources.STR_ReUploadFailureCtsiDocuments, Resources.STR_AllScroll);
                ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.PushToGp.Lock();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormLookupButton2ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalDocumentBy.Value == 1)
                {
                    if (ctsiIdLookupForm.IsOpen)
                    {
                        ctsiIdLookupForm.Close();
                    }
                    ctsiIdLookupForm.Open();
                    ctsiIdLookupForm.CtsiIdLookup.RedisplayButton.RunValidate();
                    ctsiIdLookupForm.CtsiIdLookup.LocalCalledBy.Value = 2;
                    if (!RegisterCtsiLookup2Select)
                    {
                        ctsiIdLookupForm.CtsiIdLookup.SelectButtonMnemonic.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.ctsiReUploadFailureDocumentsFormCtsiIdLookupSelectButtonToClickBeforeOriginal);
                        RegisterCtsiLookup2Select = true;
                    }
                }
                else if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalDocumentBy.Value == 3)
                {
                    vendorLookupForm.Open();
                    // Assigning the customer number to lookup form
                    vendorLookupForm.VendorLookup.VendorSortBy.Value = 1;
                    // select the record
                    vendorLookupForm.VendorLookup.VendorId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToVendorId.Value;
                    vendorLookupForm.VendorLookup.RedisplayButton.RunValidate();
                    // event which calls before the lookup window got closed.
                    vendorLookupForm.VendorLookup.LocalCalledBy.Value = 2;
                    if (!RegisterCtsiVendorLookup2Select)
                    {
                        vendorLookupForm.VendorLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(ctsiReUploadFailureDocumentsFormVendorLookupSelectButtonToClickBeforeOriginal);
                        RegisterCtsiVendorLookup2Select = true;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// Event method called on selecting the ctsiid in ctsiId lookup window and value returned to from ctsi Id field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void ctsiReUploadFailureDocumentsFormCtsiIdLookupSelectButtonToClickBeforeOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiIdLookupForm.CtsiIdLookup.LocalCalledBy.Value == 2)
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToCtsiId.Focus();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToCtsiId.Value = ctsiIdLookupForm.CtsiIdLookup.CtsiIdScroll.Ctsiid.Value;
                    ctsiIdLookupForm.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// Method to return the Vendor to to vendor Id of CTSI Reupload failed documents window.
        /// </summary>
        /// <param name="sender">Sender param</param>
        /// <param name="e">Event Arguments param</param>
        internal void ctsiReUploadFailureDocumentsFormVendorLookupSelectButtonToClickBeforeOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (vendorLookupForm.VendorLookup.LocalCalledBy.Value == 2)
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToVendorId.Focus();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToVendorId.Value = vendorLookupForm.VendorLookup.VendorLookupScroll.VendorId.Value.ToString();
                    vendorLookupForm.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormLookupButton1ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalDocumentBy.Value == 1)
                {
                    if (ctsiIdLookupForm.IsOpen)
                    {
                        ctsiIdLookupForm.Close();
                    }
                    ctsiIdLookupForm.Open();
                    ctsiIdLookupForm.CtsiIdLookup.RedisplayButton.RunValidate();
                    ctsiIdLookupForm.CtsiIdLookup.LocalCalledBy.Value = 1;
                    if (!RegisterCtsiLookup1Select)
                    {
                        ctsiIdLookupForm.CtsiIdLookup.SelectButtonMnemonic.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.ctsiReUploadFailureDocumentsFormCtsiIdLookupSelectButtonFromClickBeforeOriginal);
                        RegisterCtsiLookup1Select = true;
                    }
                }
                else if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalDocumentBy.Value == 3)
                {
                    try
                    {
                        if (vendorLookupForm.IsOpen)
                        {
                            vendorLookupForm.Close();
                        }
                        vendorLookupForm.Open();
                        // Assigning the customer number to lookup form
                        vendorLookupForm.VendorLookup.VendorSortBy.Value = 1;
                        // select the record
                        vendorLookupForm.VendorLookup.VendorId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromVendorId.Value;
                        vendorLookupForm.VendorLookup.RedisplayButton.RunValidate();
                        // event which calls before the lookup window got closed.
                        vendorLookupForm.VendorLookup.LocalCalledBy.Value = 1;
                        if (!RegisterCtsiVendorLookup1Select)
                        {
                            vendorLookupForm.VendorLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.ctsiReUploadFailureDocumentsFormVendorLookupSelectButtonFromClickBeforeOriginal);
                            RegisterCtsiVendorLookup1Select = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        // if any eror displays the message to user.
                        MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// /// Event method called on selecting the expense record id in expense record Id lookup window and value returned to to expense report Id field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void ctsiReUploadFailureDocumentsFormCtsiIdLookupSelectButtonFromClickBeforeOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiIdLookupForm.CtsiIdLookup.LocalCalledBy.Value == 1)
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromCtsiId.Focus();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromCtsiId.Value = ctsiIdLookupForm.CtsiIdLookup.CtsiIdScroll.Ctsiid.Value;
                    ctsiIdLookupForm.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// Method to return the Vendor to Customer Vendor Lookup window. 
        /// </summary>
        /// <param name="sender">Sender param</param>
        /// <param name="e">Event Arguments param</param>
        internal void ctsiReUploadFailureDocumentsFormVendorLookupSelectButtonFromClickBeforeOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (vendorLookupForm.VendorLookup.LocalCalledBy.Value == 1)
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromVendorId.Focus();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromVendorId.Value = vendorLookupForm.VendorLookup.VendorLookupScroll.VendorId.Value.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormReValidateClickAfterOriginal(object sender, EventArgs e)
        {
            PayableManagementRequest reqObj = null;
            StringBuilder logMessage = new StringBuilder();
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                logMessage.AppendLine(DateTime.Now + " Started : ReValidating Ctsi reupload scroll details "
                  + " by user : " + userId);

                reqObj = new PayableManagementRequest();
                reqObj.PayableDetailsEntity = new PayableDetailsEntity();
                reqObj.userId = Dynamics.Globals.UserId.Value;
                reqObj.Company = Dynamics.Globals.IntercompanyId.Value;
                if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_ChmptCompany)
                    reqObj.companyId = Convert.ToInt16(Resources.NACompanyId);
                else
                    reqObj.companyId = Convert.ToInt16(Resources.EUCompanyId);
                reqObj.SourceFormName = Resources.STR_ReUploadFailureCtsiDocuments;
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.GetFirst();
                while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    if (ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.SelectCheckBox.Value)
                    {
                        PayableLineEntity line = new PayableLineEntity()
                        {
                            OriginalCTSIInvoiceId = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value,
                            CtsiId = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Ctsiid.Value,
                            DocumentType = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.CtsiDocType.Value,
                            DocumentDate = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.DocumentDate.Value,
                            ApprovedAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.ApprovedAmount.Value,
                            TotalApprovedDocumentAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.TotalApprovedDocumentAmount.Value,
                            OverCharge = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.TotalOverChargeAmount.Value,
                            FreightAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.FreightAmount.Value,
                            MiscellaneousAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.MiscAmount.Value,
                            TradeDiscounts = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.TradeDiscountAmount.Value,
                            PurchaseAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.PurchasesAmount.Value,
                            VoucherNumber = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.VoucherNumber.Value,
                            VendorId = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.VendorId.Value,
                            CPTReference = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.CpReference.Value,
                            AirwayInvoiceNumber = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.CtsiAirwayInvoiceNumber.Value,
                            GLAccount = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.GlAccountNumber.Value,
                            GLAccountDescription = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.GlAccountDescription.Value,
                            CurrencyCode = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.CurrencyCode.Value,
                            TaxAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.TaxAmount.Value,
                        };
                        if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_CPEURCompany)
                        {
                            line.BaseLocalCharge = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.BaseLocalCharge.Value;
                            line.BaseZeroRatedCharge = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.BaseZeroRatedCharge.Value;
                            line.BaseReverseCharge = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.BaseReverseCharge.Value;
                        }
                        reqObj.PayableDetailsEntity.AddInvoiceLine(line);
                    }
                    tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.GetNext();
                }

                if (reqObj.PayableDetailsEntity.GetInvoiceLineDetails.Count > 0)
                {

                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/PayableManagement/ValidateAndGetCtsiTransactions", reqObj);
                        if (response.Result.IsSuccessStatusCode)
                        {
                            PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                            if (payableResponse.Status == Response.Error)
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                MessageBox.Show(Resources.STR_ErrorLoadingLookup);
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ctsiReUploadFailureDocumentsFormReValidateClickAfterOriginal: " + payableResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                if (payableResponse != null && payableResponse.PayableDetailsEntity != null && payableResponse.PayableDetailsEntity.GetInvoiceLineDetails.Count > 0)
                                {
                                    logMessage.Append(payableResponse.LogMessage.ToString());
                                    logMessage.AppendLine("Filling the scroll validated Ctsi documents scroll");
                                    LoadReuploadValidCtsiScrollContents(payableResponse);
                                    // decimalPlaces = responseObj.CurrencyDecimal;
                                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = false;
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: ctsiReUploadFailureDocumentsFormReValidateClickAfterOriginal");
                            MessageBox.Show("Error: ctsiReUploadFailureDocumentsFormReValidateClickAfterOriginal", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(Resources.STR_SelectCtsiId, Resources.STR_MESSAGE_TITLE);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormVoucherNumberChange(object sender, EventArgs e)
        {
            string oldDocNumber;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.OriginialCtsiInvoiceId.Value;
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Key = 1;
                TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldDocNumber = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.VoucherNumber.Value;
                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.VoucherNumber.Value != string.Empty
                            && ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.VoucherNumber.Value != oldDocNumber)
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.VoucherNumber.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.VoucherNumber.Value;
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                    else if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.VoucherNumber.Value == string.Empty)
                    {
                        MessageBox.Show(Resources.STR_EmptyDocumentNumber, Resources.STR_MESSAGE_TITLE);
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.VoucherNumber.Value = oldDocNumber;
                    }
                }
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormVoucherNumberEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingFields(Resources.STR_ReUploadFailureCtsiDocuments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormGlAccountNumberChange(object sender, EventArgs e)
        {
            string oldGlAccount;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.OriginialCtsiInvoiceId.Value;
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Key = 1;
                TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldGlAccount = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.GlAccountNumber.Value;
                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.GlAccountNumber.Value != string.Empty
                            && ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.GlAccountNumber.Value != oldGlAccount)
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.GlAccountNumber.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.GlAccountNumber.Value;
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                    else if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.GlAccountNumber.Value == string.Empty)
                    {
                        MessageBox.Show(Resources.STR_EmptyGLAccountNumber, Resources.STR_MESSAGE_TITLE);
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.GlAccountNumber.Value = oldGlAccount;
                    }
                }
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormGlAccountNumberEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingFields(Resources.STR_ReUploadFailureCtsiDocuments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormCurrencyCodeEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingFields(Resources.STR_ReUploadFailureCtsiDocuments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormCurrencyCodeChange(object sender, EventArgs e)
        {
            string oldCurrencyCode;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.OriginialCtsiInvoiceId.Value;
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Key = 1;
                TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldCurrencyCode = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.CurrencyCode.Value;
                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.CurrencyCode.Value != string.Empty
                            && ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.CurrencyCode.Value != oldCurrencyCode)
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.CurrencyCode.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.CurrencyCode.Value;
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                    else if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.GlAccountDescription.Value == string.Empty)
                    {
                        MessageBox.Show(Resources.STR_EmptyCurrencyCode, Resources.STR_MESSAGE_TITLE);
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.CurrencyCode.Value = oldCurrencyCode;
                    }
                }
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormGlAccountDescriptionEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingFields(Resources.STR_ReUploadFailureCtsiDocuments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormGlAccountDescriptionChange(object sender, EventArgs e)
        {
            string oldGlAccountDesc;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.OriginialCtsiInvoiceId.Value;
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Key = 1;
                TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldGlAccountDesc = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.GlAccountDescription.Value;
                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.GlAccountDescription.Value != string.Empty
                            && ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.GlAccountDescription.Value != oldGlAccountDesc)
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.GlAccountDescription.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.GlAccountDescription.Value;
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                    else if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.GlAccountDescription.Value == string.Empty)
                    {
                        MessageBox.Show(Resources.STR_GLAccountDescription, Resources.STR_MESSAGE_TITLE);
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.GlAccountDescription.Value = oldGlAccountDesc;
                    }
                }
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormCtsiAirwayInvoiceNumberChange(object sender, EventArgs e)
        {
            string oldAirwayNumber;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.OriginialCtsiInvoiceId.Value;
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Key = 1;
                TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldAirwayNumber = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.CtsiAirwayInvoiceNumber.Value;
                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.CtsiAirwayInvoiceNumber.Value != string.Empty
                            && ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.CtsiAirwayInvoiceNumber.Value != oldAirwayNumber)
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.CtsiAirwayInvoiceNumber.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.CtsiAirwayInvoiceNumber.Value;
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                }
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormCtsiAirwayInvoiceNumberEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingFields(Resources.STR_ReUploadFailureCtsiDocuments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormCpReferenceChange(object sender, EventArgs e)
        {
            string cpRefrenceNumber;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.OriginialCtsiInvoiceId.Value;
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Key = 1;
                TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    cpRefrenceNumber = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.CpReference.Value;
                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.CpReference.Value != string.Empty
                            && ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.CpReference.Value != cpRefrenceNumber)
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.CpReference.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.CpReference.Value;
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                }
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormCpReferenceEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingFields(Resources.STR_ReUploadFailureCtsiDocuments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormTaxAmountChange(object sender, EventArgs e)
        {
            decimal oldTaxAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.OriginialCtsiInvoiceId.Value;
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Key = 1;
                TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldTaxAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.TaxAmount.Value;
                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.TaxAmount.Value != oldTaxAmount)
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.TaxAmount.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.TaxAmount.Value;
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormTaxAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingFields(Resources.STR_ReUploadFailureCtsiDocuments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormPurchasesAmountChange(object sender, EventArgs e)
        {
            decimal oldPurchaseAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.OriginialCtsiInvoiceId.Value;
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Key = 1;
                TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldPurchaseAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.PurchasesAmount.Value;
                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.PurchasesAmount.Value != oldPurchaseAmount)
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.PurchasesAmount.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.PurchasesAmount.Value;
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormPurchasesAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingFields(Resources.STR_ReUploadFailureCtsiDocuments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormMiscAmountChange(object sender, EventArgs e)
        {
            decimal oldMiscAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.OriginialCtsiInvoiceId.Value;
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Key = 1;
                TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldMiscAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.MiscAmount.Value;
                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.MiscAmount.Value != oldMiscAmount)
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.MiscAmount.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.MiscAmount.Value;
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormMiscAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingFields(Resources.STR_ReUploadFailureCtsiDocuments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormTradeDiscountAmountChange(object sender, EventArgs e)
        {
            decimal oldTradeDiscAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.OriginialCtsiInvoiceId.Value;
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Key = 1;
                TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldTradeDiscAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.TradeDiscountAmount.Value;
                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.TradeDiscountAmount.Value != oldTradeDiscAmount)
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.TradeDiscountAmount.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.TradeDiscountAmount.Value;
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormTradeDiscountAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingFields(Resources.STR_ReUploadFailureCtsiDocuments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormFreightAmountChange(object sender, EventArgs e)
        {
            decimal oldFreightAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.OriginialCtsiInvoiceId.Value;
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Key = 1;
                TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldFreightAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.FreightAmount.Value;
                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.FreightAmount.Value != oldFreightAmount)
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.FreightAmount.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.FreightAmount.Value;
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormFreightAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingFields(Resources.STR_ReUploadFailureCtsiDocuments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormTotalOverChargeAmountChange(object sender, EventArgs e)
        {
            decimal oldOverChargeAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.OriginialCtsiInvoiceId.Value;
                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Key = 1;
                TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldOverChargeAmount = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.TotalOverChargeAmount.Value;
                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.TotalOverChargeAmount.Value != oldOverChargeAmount)
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.TotalOverChargeAmount.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.TotalOverChargeAmount.Value;
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormTotalOverChargeAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingFields(Resources.STR_ReUploadFailureCtsiDocuments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormBaseZeroRatedChargeChange(object sender, EventArgs e)
        {
            decimal oldBaseZeroRatedCharge = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_CPEURCompany)
                {
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.OriginialCtsiInvoiceId.Value;
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Key = 1;
                    TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Change();
                    if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        oldBaseZeroRatedCharge = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.BaseZeroRatedCharge.Value;
                        if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.BaseZeroRatedCharge.Value != oldBaseZeroRatedCharge)
                        {
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.BaseZeroRatedCharge.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.BaseZeroRatedCharge.Value;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                            ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormBaseReverseChargeChange(object sender, EventArgs e)
        {
            decimal oldBaseReverseCharge = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_CPEURCompany)
                {
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.OriginialCtsiInvoiceId.Value;
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Key = 1;
                    TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Change();
                    if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        oldBaseReverseCharge = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.BaseReverseCharge.Value;
                        if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.BaseReverseCharge.Value != oldBaseReverseCharge)
                        {
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.BaseReverseCharge.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.BaseReverseCharge.Value;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                            ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormBaseLocalChargeChange(object sender, EventArgs e)
        {
            decimal oldBaseLocalCharge = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_CPEURCompany)
                {
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.OriginialCtsiInvoiceId.Value;
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Key = 1;
                    TableError tableError = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Change();
                    if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        oldBaseLocalCharge = ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.BaseLocalCharge.Value;
                        if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.BaseLocalCharge.Value != oldBaseLocalCharge)
                        {
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.BaseLocalCharge.Value = ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.BaseLocalCharge.Value;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                            ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.ChangeFlag.Value = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormBaseZeroRatedChargeEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_CPEURCompany)
                {
                    RestrictUpdatingFields(Resources.STR_ReUploadFailureCtsiDocuments);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormBaseReverseChargeEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_CPEURCompany)
                {
                    RestrictUpdatingFields(Resources.STR_ReUploadFailureCtsiDocuments);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiReUploadFailureDocumentsFormBaseLocalChargeEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_CPEURCompany)
                {
                    RestrictUpdatingFields(Resources.STR_ReUploadFailureCtsiDocuments);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        #endregion

        #region CTSILinkManualPayment

        /// <summary>
        ///CTSI "Manual Payment Upload form" Open before original event. To
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctsiDocumentsWithManualPaymentsForm_OpenBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (!RegisterCTSILinkedManualDocuments)
                {
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalDocumentBy.Change += new EventHandler(CTSIDocumentWithManualLocalDocumentBy_Change);
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LookupButton1.ClickAfterOriginal += new EventHandler(CTSIDocumentWithManaualLookupButton1_ClickAfterOriginal);
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LookupButton2.ClickAfterOriginal += new EventHandler(CTSIDocumentWithManaualLookupButton2_ClickAfterOriginal);
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalDocumentBy.Change += new EventHandler(CTSIDocumentManualLocalDocumentBy_Change);
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromCtsiid.Change += new EventHandler(CTSIDocumentWithManualLocalFromCtsiid_Change);
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToCtsiid.Change += new EventHandler(CTSIDocumentWithManualLocalToCtsiid_Change);
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromVendorId.Change += new EventHandler(CTSIDocumentWithManualLocalFromVendorId_Change);
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToVendorId.Change += new EventHandler(CTSIDocumentWithManualLocalToVendorId_Change);
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.RedisplayButton.ClickAfterOriginal += new EventHandler(CTSIDocumentManualRedisplayButton_ClickAfterOriginal);
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.OkButton.ClickAfterOriginal += new EventHandler(CTSIDocumentManualOkButton_ClickAfterOriginal);
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.UpdateAsManuallyCreated.ClickAfterOriginal += new EventHandler(CTSIDocumentManualUpdateAsManuallyCreated_ClickAfterOriginal);
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.CloseAfterOriginal += new EventHandler(CtsiDocumentsWithManualPayments_CloseAfterOriginal);
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.AllOrRange.Change += new EventHandler(CtsiDocumentsWithManualPaymentsAllOrRange_Change);
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.CtsiDocumentsWithManualPaymentsScroll.ManualPaymentNumber.Change += new EventHandler(CTSIDocumentsManualPaymentNumber_Change);
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.CtsiDocumentsWithManualPaymentsScroll.SelectCheckBox.Change += new EventHandler(CTSIDocumentsManualSelectCheckBox_Change);
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.CtsiDocumentsWithManualPaymentsScroll.ManualPaymentNumber.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(CTSIDocumentManualPaymentNumber_EnterBeforeOriginal);

                    RegisterCTSILinkedManualDocuments = true;
                }
                ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalDocumentBy.Value = 1;
                ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.ChangeFlag.Value = false;
                InitilizeSortBySelection(Resources.STR_CTSIDocumentsWithManualPayments);
                ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.UpdateAsManuallyCreated.Lock();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CTSIDocumentManualPaymentNumber_EnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.CtsiDocumentsWithManualPaymentsScroll.SelectCheckBox.Value == false)
                {
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.CtsiDocumentsWithManualPaymentsScroll.SelectCheckBox.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CTSIDocumentsManualSelectCheckBox_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.ChangeFlag.Value = true;
                ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Close();
                ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Release();
                ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Ctsiid.Value = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.CtsiDocumentsWithManualPaymentsScroll.Ctsiid.Value;
                ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Key = 1;
                TableError tableError = ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Change();
                if (tableError == TableError.NoError
                            && tableError != TableError.EndOfTable)
                {
                    if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.CtsiDocumentsWithManualPaymentsScroll.SelectCheckBox.Value == true)
                    {
                        ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.SelectCheckBox.Value = true;
                        ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Save();
                    }
                    else
                    {
                        ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.SelectCheckBox.Value = false;
                        ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.ManualPaymentNumber.Value = string.Empty;
                        ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Save();
                        ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.CtsiDocumentsWithManualPaymentsScroll.ManualPaymentNumber.Value = string.Empty;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CTSIDocumentsManualPaymentNumber_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            PayableManagementRequest requestObj = null;
            logMessage.AppendLine(DateTime.Now + " Validate manual payment number already exists or is processed by some other user:  "
                        + " by user : " + Dynamics.Globals.UserId.Value.ToString()
                        + " in company : " + Dynamics.Globals.IntercompanyId.Value.ToString());
            try
            {
                ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Close();

                ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Release();
                ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Ctsiid.Value = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.CtsiDocumentsWithManualPaymentsScroll.Ctsiid.Value;
                ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Key = 1;
                TableError tableError = ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Change();
                if (tableError == TableError.NoError
                            && tableError != TableError.EndOfTable)
                {
                    if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.CtsiDocumentsWithManualPaymentsScroll.ManualPaymentNumber.Value != string.Empty)
                    {

                        requestObj = new PayableManagementRequest();
                        requestObj.ManualPaymentNumber = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.CtsiDocumentsWithManualPaymentsScroll.ManualPaymentNumber.Value;
                        requestObj.FromVendorId = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.CtsiDocumentsWithManualPaymentsScroll.VendorId.Value;
                        requestObj.companyId = Dynamics.Globals.CompanyId.Value;

                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/PayableManagement/ValidateVoucherExistsForVendor", requestObj);
                            if (response.Result.IsSuccessStatusCode)
                            {
                                PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                                if (payableResponse.Status == Response.Error)
                                {
                                    logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CTSIDocumentsManualPaymentNumber_Change: " + payableResponse.ErrorMessage.ToString());
                                }
                                else
                                {
                                    logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                    if (!payableResponse.IsValid)
                                    {
                                        MessageBox.Show(Resources.STR_InvoiceDoesNotExists, Resources.STR_MESSAGE_TITLE);
                                        ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.CtsiDocumentsWithManualPaymentsScroll.ManualPaymentNumber.Clear();
                                        ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.CtsiDocumentsWithManualPaymentsScroll.ManualPaymentNumber.Focus();
                                    }
                                    else
                                    {
                                        ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.ManualPaymentNumber.Value = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.CtsiDocumentsWithManualPaymentsScroll.ManualPaymentNumber.Value;
                                        ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Save();
                                        ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.ChangeFlag.Value = true;
                                    }
                                }
                            }
                            else
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: CTSIDocumentsManualPaymentNumber_Change");
                                MessageBox.Show("Error: CTSIDocumentsManualPaymentNumber_Change", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CtsiDocumentsWithManualPaymentsAllOrRange_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalDocumentBy.Value == 1
                        && ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.AllOrRange.Value == 0)
                {
                    ClearReUploadCtsiFields(Resources.STR_CTSIDocumentsWithManualPayments);
                }
                else if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalDocumentBy.Value == 2
                            && ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.AllOrRange.Value == 0)
                {
                    ClearReUploadDocumentDateFields(Resources.STR_CTSIDocumentsWithManualPayments);
                }
                else if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalDocumentBy.Value == 3
                            && ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.AllOrRange.Value == 0)
                {
                    ClearReUploadVendorIdFields(Resources.STR_CTSIDocumentsWithManualPayments);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CtsiDocumentsWithManualPayments_CloseAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiIdLookupForm.IsOpen)
                {
                    ctsiIdLookupForm.Close();
                }
                if (vendorLookupForm.IsOpen)
                {
                    vendorLookupForm.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CTSIDocumentManualUpdateAsManuallyCreated_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            logMessage.AppendLine(DateTime.Now + "   Started : Updating manual payment number: "
                   + " by user : " + Dynamics.Globals.UserId.Value.ToString()
                   + " in company : " + Dynamics.Globals.IntercompanyId.Value.ToString());
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                PayableManagementRequest reqObj = this.AssignCtsiGridValuetoRequestObject();
                reqObj.userId = Dynamics.Globals.UserId.Value.ToString();
                logMessage.AppendLine("Compnay Id: " + Dynamics.Globals.CompanyId.Value);
                if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_ChmptCompany)
                {
                    reqObj.companyId = Convert.ToInt16(Resources.NACompanyId);
                }
                else
                {
                    reqObj.companyId = Convert.ToInt16(Resources.EUCompanyId);
                }
                if (reqObj.PayableDetailsEntity.GetInvoiceLineDetails.Count > 0)
                {
                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/PayableManagement/UpdateManualPaymentNumberForCTSIDocuments", reqObj);
                        if (response.Result.IsSuccessStatusCode)
                        {
                            PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                            if (payableResponse.Status == Response.Error && payableResponse.IsValidStatus == 1)
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                logMessage.AppendLine(DateTime.Now + "One or more selected documents does not contain manual payment number. ");
                                MessageBox.Show(Resources.STR_EmptyManualPaymentNumber, Resources.STR_MESSAGE_TITLE);
                            }
                            else if (payableResponse.Status == Response.Error && payableResponse.IsValidStatus == 2)
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                logMessage.AppendLine(DateTime.Now + " One or more documents have already been created by some other user.");
                                MessageBox.Show(Resources.STR_DocumentAlreadyCreated, Resources.STR_MESSAGE_TITLE);
                                ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.RedisplayButton.RunValidate();
                            }
                            else
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                MessageBox.Show(Resources.STR_CtsiManualPaymentUpdateSuccess, Resources.STR_MESSAGE_TITLE);
                                logMessage.AppendLine(DateTime.Now + " Successfully linked the manual payment numbers to failed ctsi Id's.");
                                InitilizeSortBySelection(Resources.STR_CTSIDocumentsWithManualPayments);
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: CTSIDocumentManualUpdateAsManuallyCreated_ClickAfterOriginal");
                            MessageBox.Show("Error: CTSIDocumentManualUpdateAsManuallyCreated_ClickAfterOriginal", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(Resources.STR_SelectAtLeastOneCtsiDoc, Resources.STR_MESSAGE_TITLE);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CTSIDocumentManualOkButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            logMessage.AppendLine(DateTime.Now + " Completed : Closed Link failed expense record Id to manual payments:  "
                      + " by user : " + Dynamics.Globals.UserId.Value.ToString()
                      + " in company : " + Dynamics.Globals.IntercompanyId.Value.ToString());
            try
            {
                ctsiDocumentsWithManualPaymentsForm.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CTSIDocumentManualRedisplayButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            PayableManagementRequest reqObj = null;
            StringBuilder logMessage = new StringBuilder();
            bool isValid = false;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                logMessage.AppendLine(DateTime.Now + " Started : Filling data to Re-Upload Expense Documents: "
                    + " by user : " + Dynamics.Globals.UserId.Value.ToString()
                    + " in company : " + Dynamics.Globals.IntercompanyId.Value.ToString());
                reqObj = new PayableManagementRequest();

                if ((ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalDocumentBy.Value == 1)
                       && (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.AllOrRange.Value == 1)
                         && (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromCtsiid.Value != string.Empty)
                               && (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToCtsiid.Value != string.Empty))
                {
                    reqObj.SearchType = Resources.STR_SearchType_CtsiId;
                    reqObj.FromCtsiId = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromCtsiid.Value;
                    reqObj.ToCtsiId = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToCtsiid.Value;
                    reqObj.FromVendorId = string.Empty;
                    reqObj.ToVendorId = string.Empty;
                    reqObj.FromDocumentDate = Convert.ToDateTime("01/01/1900");
                    reqObj.ToDocumentDate = Convert.ToDateTime("01/01/1900");
                    isValid = true;
                }
                else if ((ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalDocumentBy.Value == 2)
                       && (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.AllOrRange.Value == 1)
                         && (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromDocumentDate.Value != DateTime.MinValue)
                               && (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToDocumentDate.Value != DateTime.MinValue))
                {
                    reqObj.SearchType = Resources.STR_SearchType_DocumentDate;
                    reqObj.FromDocumentDate = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromDocumentDate.Value;
                    reqObj.ToDocumentDate = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToDocumentDate.Value;
                    reqObj.FromVendorId = string.Empty;
                    reqObj.ToVendorId = string.Empty;
                    reqObj.FromCtsiId = string.Empty;
                    reqObj.ToCtsiId = string.Empty;
                    isValid = true;

                }
                else if ((ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalDocumentBy.Value == 3)
                       && (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.AllOrRange.Value == 1)
                         && (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromVendorId.Value != string.Empty)
                               && (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToVendorId.Value != string.Empty))
                {
                    reqObj.SearchType = Resources.STR_SearchType_VendorId; ;
                    reqObj.FromVendorId = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromVendorId.Value;
                    reqObj.ToVendorId = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToVendorId.Value;
                    reqObj.FromDocumentDate = Convert.ToDateTime("01/01/1900");
                    reqObj.ToDocumentDate = Convert.ToDateTime("01/01/1900");
                    reqObj.FromCtsiId = string.Empty;
                    reqObj.ToCtsiId = string.Empty;
                    isValid = true;
                }
                else if (((ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalDocumentBy.Value == 1)
                            || (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalDocumentBy.Value == 2)
                                || (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalDocumentBy.Value == 3))
                                    && (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.AllOrRange.Value == 0))
                {
                    reqObj.SearchType = Resources.STR_SearchType_All;
                    reqObj.FromVendorId = string.Empty;
                    reqObj.ToVendorId = string.Empty;
                    reqObj.FromDocumentDate = Convert.ToDateTime("01/01/1900");
                    reqObj.ToDocumentDate = Convert.ToDateTime("01/01/1900");
                    reqObj.FromCtsiId = string.Empty;
                    reqObj.ToCtsiId = string.Empty;
                    isValid = true;

                }
                else
                {
                    MessageBox.Show(Resources.STR_RequiredFieldsMissing, Resources.STR_MESSAGE_TITLE);
                }
                if (isValid)
                {
                    reqObj.userId = Dynamics.Globals.UserId.Value.ToString();
                    reqObj.companyId = Dynamics.Globals.CompanyId.Value;
                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/PayableManagement/GetFailedCtsiIdsToLinkManualPayments", reqObj);
                        if (response.Result.IsSuccessStatusCode)
                        {
                            PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                            if (payableResponse.Status == Response.Error)
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                logMessage.AppendLine(DateTime.Now + " Error in fetching records to populate in Link failed CTSI to manual payments window.");
                                MessageBox.Show(Resources.STR_ErrorPopulatingFailedCtsiDocuments);
                            }
                            else
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                if (payableResponse != null)
                                {
                                    logMessage.AppendLine(DateTime.Now + " Started loading the failed CTSI details to scrolling window.");
                                    LinkFailedCtsiRecordsScrollContents(payableResponse);
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: CTSIDocumentManualRedisplayButton_ClickAfterOriginal");
                            MessageBox.Show("Error: CTSIDocumentManualRedisplayButton_ClickAfterOriginal", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
                reqObj = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CTSIDocumentWithManualLocalToVendorId_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToVendorId.Value != string.Empty)
                {
                    ctsiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Close();
                    ctsiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Release();
                    ctsiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.VendorId.Value = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToVendorId.Value;
                    ctsiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Key = 1;
                    TableError Vendorerror = ctsiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Get();

                    if (Vendorerror != TableError.NoError)
                    {
                        // if not valid displays a message
                        MessageBox.Show(Resources.STR_VendorIdDoesNotExists, Resources.STR_MESSAGE_TITLE);
                        ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToVendorId.Clear();

                    }
                    else
                    {
                        if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToVendorId.Value == string.Empty)
                        {
                            if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromVendorId.Value == string.Empty)
                            {
                                ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.AllOrRange.Value = 0;
                            }
                        }
                        else
                        {
                            ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.AllOrRange.Value = 1;
                        }
                    }
                    ctsiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Close();
                    ctsiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Release();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CTSIDocumentWithManualLocalFromVendorId_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromVendorId.Value != string.Empty)
                {
                    ctsiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Close();
                    ctsiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Release();
                    ctsiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.VendorId.Value = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromVendorId.Value;
                    ctsiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Key = 1;
                    TableError Vendorerror = ctsiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Get();

                    if (Vendorerror != TableError.NoError)
                    {
                        // if not valid displays a message
                        MessageBox.Show(Resources.STR_VendorIdDoesNotExists, Resources.STR_MESSAGE_TITLE);
                        ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromVendorId.Clear();
                        ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromVendorId.Focus();

                    }
                    else if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToVendorId.Value != string.Empty)
                    {
                        ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToVendorId.Value = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromVendorId.Value;
                        ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.AllOrRange.Value = 1;
                    }
                    else
                    {
                        if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromVendorId.Value == string.Empty)
                        {
                            if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToVendorId.Value == string.Empty)
                            {
                                ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.AllOrRange.Value = 0;
                            }
                        }
                        else
                        {
                            ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.AllOrRange.Value = 1;
                            if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToVendorId.Value == string.Empty)
                            {
                                ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToVendorId.Value = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromVendorId.Value;
                            }
                        }
                    }

                    ctsiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Close();
                    ctsiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Release();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CTSIDocumentWithManualLocalToCtsiid_Change(object sender, EventArgs e)
        {
            PayableManagementRequest requestObj = null;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToCtsiid.Value != string.Empty)
                {
                    requestObj = new PayableManagementRequest();
                    requestObj.SourceLookup = Resources.STR_CTSIId;
                    requestObj.LookupValue = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToCtsiid.Value.ToString();

                    requestObj.userId = Dynamics.Globals.UserId.Value.ToString();
                    requestObj.companyId = Dynamics.Globals.CompanyId.Value;
                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/PayableManagement/VerifyCtsiLookupValueExists", requestObj);
                        if (response.Result.IsSuccessStatusCode)
                        {
                            PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                            if (payableResponse.Status == Response.Error)
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CTSIDocumentWithManualLocalToCtsiid_Change: " + payableResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                if (!payableResponse.LookupValueExists)
                                {
                                    MessageBox.Show(Resources.STR_CTSIIdDoesNotExists, Resources.STR_MESSAGE_TITLE);
                                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToCtsiid.Value = string.Empty;
                                }
                                else
                                {
                                    if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToCtsiid.Value == string.Empty)
                                    {
                                        if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromCtsiid.Value == string.Empty)
                                        {
                                            ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.AllOrRange.Value = 0;
                                        }
                                    }
                                    else
                                    {
                                        ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.AllOrRange.Value = 1;
                                    }
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: CTSIDocumentWithManualLocalToCtsiid_Change");
                            MessageBox.Show("Error: CTSIDocumentWithManualLocalToCtsiid_Change", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CTSIDocumentWithManualLocalFromCtsiid_Change(object sender, EventArgs e)
        {
            PayableManagementRequest requestObj = null;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromCtsiid.Value != string.Empty)
                {
                    requestObj = new PayableManagementRequest();
                    requestObj.SourceLookup = Resources.STR_CTSIId;
                    requestObj.LookupValue = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromCtsiid.Value;

                    requestObj.userId = Dynamics.Globals.UserId.Value.ToString();
                    requestObj.companyId = Dynamics.Globals.CompanyId.Value;
                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/PayableManagement/VerifyCtsiLookupValueExists", requestObj);
                        if (response.Result.IsSuccessStatusCode)
                        {
                            PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                            if (payableResponse.Status == Response.Error)
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TCTSIDocumentWithManualLocalFromCtsiid_Change: " + payableResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                if (!payableResponse.LookupValueExists)
                                {
                                    MessageBox.Show(Resources.STR_CTSIIdDoesNotExists, Resources.STR_MESSAGE_TITLE);
                                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromCtsiid.Clear();
                                }
                                else
                                {
                                    if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromCtsiid.Value == string.Empty)
                                    {
                                        if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToCtsiid.Value == string.Empty)
                                        {
                                            ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.AllOrRange.Value = 0;
                                        }
                                    }
                                    else
                                    {
                                        ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.AllOrRange.Value = 1;
                                        if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToCtsiid.Value == string.Empty)
                                        {
                                            ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToCtsiid.Value = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromCtsiid.Value;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: CTSIDocumentWithManualLocalFromCtsiid_Change");
                            MessageBox.Show("Error: CTSIDocumentWithManualLocalFromCtsiid_Change", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CTSIDocumentManualLocalDocumentBy_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                InitilizeSortBySelection(Resources.STR_CTSIDocumentsWithManualPayments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CTSIDocumentWithManaualLookupButton2_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalDocumentBy.Value == 1)
                {
                    if (ctsiIdLookupForm.IsOpen)
                    {
                        ctsiIdLookupForm.Close();
                    }
                    ctsiIdLookupForm.Open();
                    ctsiIdLookupForm.CtsiIdLookup.RedisplayButton.RunValidate();
                    ctsiIdLookupForm.CtsiIdLookup.LocalCalledBy.Value = 2;
                    ctsiIdLookupForm.CtsiIdLookup.SelectButtonMnemonic.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(CtsiManualPaymentCtsiIdToSelectButtonMnemonic_ClickBeforeOriginal);
                }
                else
                    if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalDocumentBy.Value == 3)
                {
                    vendorLookupForm.Open();
                    // Assigning the vendor number to lookup form
                    vendorLookupForm.VendorLookup.VendorSortBy.Value = 1;
                    // select the record
                    vendorLookupForm.VendorLookup.VendorId.Value = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToVendorId.Value;
                    vendorLookupForm.VendorLookup.RedisplayButton.RunValidate();
                    // event which calls before the lookup window got closed.
                    vendorLookupForm.VendorLookup.LocalCalledBy.Value = 2;
                    vendorLookupForm.VendorLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.CTSIDocumentsWithManualPaymentsSelectButtonToVendorIDSelectButton_ClickBeforeOriginal);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// Event method called on selecting the expense record id in expense record Id lookup window and value returned to from expense report Id field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void CtsiManualPaymentCtsiIdToSelectButtonMnemonic_ClickBeforeOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiIdLookupForm.CtsiIdLookup.LocalCalledBy.Value == 2)
                {
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToCtsiid.Focus();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToCtsiid.Value = ctsiIdLookupForm.CtsiIdLookup.CtsiIdScroll.Ctsiid.Value;
                    ctsiIdLookupForm.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CTSIDocumentWithManaualLookupButton1_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            logMessage.AppendLine(DateTime.Now + " Started : Open Failed Expenses report Id's to scrollign window. "
                   + " by user : " + Dynamics.Globals.UserId.Value.ToString()
                   + " in company : " + Dynamics.Globals.IntercompanyId.Value.ToString());
            try
            {
                if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalDocumentBy.Value == 1)
                {
                    if (ctsiIdLookupForm.IsOpen)
                    {
                        ctsiIdLookupForm.Close();
                    }
                    ctsiIdLookupForm.Open();
                    ctsiIdLookupForm.CtsiIdLookup.RedisplayButton.RunValidate();
                    ctsiIdLookupForm.CtsiIdLookup.LocalCalledBy.Value = 1;
                    ctsiIdLookupForm.CtsiIdLookup.SelectButtonMnemonic.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(CstiManualPaymentsFromCtsiIdSelectButtonMnemonic_ClickBeforeOriginal);
                }
                else if (ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalDocumentBy.Value == 3)
                {
                    logMessage.AppendLine(DateTime.Now + " Started : Open list of all vendor Id's to scrolling window. "
                   + " by user : " + Dynamics.Globals.UserId.Value.ToString()
                   + " in company : " + Dynamics.Globals.IntercompanyId.Value.ToString());
                    if (vendorLookupForm.IsOpen)
                    {
                        vendorLookupForm.Close();
                    }
                    vendorLookupForm.Open();
                    // Assigning the customer number to lookup form
                    vendorLookupForm.VendorLookup.VendorSortBy.Value = 1;
                    // select the record
                    vendorLookupForm.VendorLookup.VendorId.Value = ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromVendorId.Value;
                    vendorLookupForm.VendorLookup.RedisplayButton.RunValidate();
                    // event which calls before the lookup window got closed.
                    vendorLookupForm.VendorLookup.LocalCalledBy.Value = 1;
                    vendorLookupForm.VendorLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.CTSIDocumentsWithManualPaymentsSelectButton_ClickBeforeOriginal);
                }
                logMessage.AppendLine(DateTime.Now + " Completed returning the from search field.");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CstiManualPaymentsFromCtsiIdSelectButtonMnemonic_ClickBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ctsiIdLookupForm.CtsiIdLookup.LocalCalledBy.Value == 1)
                {
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromCtsiid.Focus();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromCtsiid.Value = ctsiIdLookupForm.CtsiIdLookup.CtsiIdScroll.Ctsiid.Value;
                    ctsiIdLookupForm.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CTSIDocumentsWithManualPaymentsSelectButtonToVendorIDSelectButton_ClickBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (vendorLookupForm.VendorLookup.LocalCalledBy.Value == 2)
                {
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToVendorId.Focus();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToVendorId.Value = vendorLookupForm.VendorLookup.VendorLookupScroll.VendorId.Value.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CTSIDocumentsWithManualPaymentsSelectButton_ClickBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (vendorLookupForm.VendorLookup.LocalCalledBy.Value == 1)
                {
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromVendorId.Focus();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromVendorId.Value = vendorLookupForm.VendorLookup.VendorLookupScroll.VendorId.Value.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CTSIDocumentWithManualLocalDocumentBy_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                InitilizeSortBySelection(Resources.STR_CTSIDocumentsWithManualPayments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        #endregion

        #region PrivateCTSIReuploadfailedDocuments
        private void ReloadFailedCtsiDocumentscrollContents(PayableManagementResponse responseObj)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                //load the details into window
                if (responseObj.Status == Response.Success)
                {
                    this.ClearScrollingWindows(Resources.STR_ReUploadFailureCtsiDocuments, Resources.STR_AllScroll);

                    //if the line details exists then fill scroll
                    if (responseObj.PayableDetailsEntity != null && responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Count > 0)
                    {
                        for (int i = 0; i < responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Count; i++)
                        {
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.SelectCheckBox.Value = false;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].OriginalCTSIInvoiceId;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Ctsiid.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].CtsiId;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.DocumentDate.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentDate;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.ApprovedAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ApprovedAmount;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.TotalApprovedDocumentAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].TotalApprovedDocumentAmount;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.TotalOverChargeAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].OverCharge;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.FreightAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].FreightAmount;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.MiscAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].MiscellaneousAmount;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.TradeDiscountAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].TradeDiscounts;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.PurchasesAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].PurchaseAmount;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.VoucherNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentNumber;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.VendorId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].VendorId;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.CpReference.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].CPTReference;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.CtsiAirwayInvoiceNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].AirwayInvoiceNumber;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.GlAccountNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GLAccount;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.GlAccountDescription.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GLAccountDescription;
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.CurrencyCode.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].CurrencyCode;
                            if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_CPEURCompany)
                            {
                                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.BaseLocalCharge.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].BaseLocalCharge;
                                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.BaseZeroRatedCharge.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].BaseZeroRatedCharge;
                                ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.BaseReverseCharge.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].BaseReverseCharge;
                            }
                            ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Save();
                        }

                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Release();
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiReUploadDocumentsTemp.Close();

                        //Fill details to scrolling window.Call dex procedure to fill the scrolling window.
                        ctsiReUploadFailureDocumentsForm.Procedures.ReUploadFailureCtsiDocumentsFormScrollFill.Invoke();

                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }

        }

        /// <summary>
        /// Method to hide all the fields of the windows based on the passed parameter form name.
        /// </summary>
        /// <param name="formName"></param>
        private void HideAllSortByReUploadCtsiFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_ReUploadFailureCtsiDocuments)
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromCtsiId.Hide();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToCtsiId.Hide();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromDocumentDate.Hide();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToDocumentDate.Hide();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromVendorId.Hide();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToVendorId.Hide();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LookupButton1.Hide();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LookupButton2.Hide();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.PushToGp.Lock();
                }
                else if (formName == Resources.STR_CTSIDocumentsWithManualPayments)
                {
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromCtsiid.Hide();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToCtsiid.Hide();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromDocumentDate.Hide();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToDocumentDate.Hide();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromVendorId.Hide();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToVendorId.Hide();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LookupButton1.Hide();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LookupButton2.Hide();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.UpdateAsManuallyCreated.Lock();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="formName"></param>
        private void ShowReUploadCtsiIdFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_ReUploadFailureCtsiDocuments)
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromCtsiId.Show();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToCtsiId.Show();
                }
                else if (formName == Resources.STR_CTSIDocumentsWithManualPayments)
                {
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromCtsiid.Show();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToCtsiid.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// Method to hide expense fields of the windows based on the passed parameter form name.
        /// </summary>
        /// <param name="formName"></param>
        private void HideReUploadCtsiIdFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_ReUploadFailureCtsiDocuments)
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromCtsiId.Hide();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToCtsiId.Hide();
                }
                else if (formName == Resources.STR_CTSIDocumentsWithManualPayments)
                {
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromCtsiid.Hide();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToCtsiid.Hide();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// Method to clear expense fields of the windows based on the passed parameter form name.
        /// </summary>
        /// <param name="formName"></param>
        private void ClearReUploadCtsiFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_ReUploadFailureCtsiDocuments)
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalFromCtsiId.Clear();
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.LocalToCtsiId.Clear();
                }
                else if (formName == Resources.STR_CTSIDocumentsWithManualPayments)
                {
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalFromCtsiid.Clear();
                    ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.LocalToCtsiid.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="formName"></param>
        private void RestrictUpdatingFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();

            try
            {
                if (formName == Resources.STR_ReUploadFailureCtsiDocuments)
                {
                    if (ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.SelectCheckBox.Value == false)
                        ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.CtsiReUploadDocumentsSelectScroll.SelectCheckBox.Focus();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private PayableManagementRequest AssignCtsiGridValuetoRequestObject()
        {
            PayableManagementRequest reqObj = null;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                reqObj = new PayableManagementRequest();
                reqObj.PayableDetailsEntity = new PayableDetailsEntity();
                reqObj.userId = Dynamics.Globals.UserId.Value;
                ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Close();
                ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Release();

                TableError tableError = ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.GetFirst();
                while (tableError == TableError.NoError
                            && tableError != TableError.EndOfTable)
                {
                    if (ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.SelectCheckBox.Value)
                    {
                        reqObj.PayableDetailsEntity.AddInvoiceLine(new PayableLineEntity()
                        {
                            CtsiId = ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Ctsiid.Value,
                            VoucherNumber = ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.ManualPaymentNumber.Value,
                            DocumentDate = ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.DocumentDate.Value,
                            VendorId = ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.VendorId.Value,
                            TotalApprovedDocumentAmount = ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.TotalApprovedDocumentAmount.Value
                        });
                    }
                    tableError = ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.GetNext();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Release();
                ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Close();
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
            return reqObj;
        }
        /// <summary>
        /// Fill the scroll values
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="responseObj"></param>
        private void LinkFailedCtsiRecordsScrollContents(PayableManagementResponse responseObj)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                //load the details into window
                if (responseObj.Status == Response.Success)
                {
                    this.ClearScrollingWindows(Resources.STR_CTSIDocumentsWithManualPayments, "");

                    //if the line details exists then fill scroll
                    if (responseObj.PayableDetailsEntity != null && responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Count > 0)
                    {
                        ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Close();
                        ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Release();
                        for (int i = 0; i < responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Count; i++)
                        {

                            ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.SelectCheckBox.Value = false;
                            ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Ctsiid.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].CtsiId;
                            ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.DocumentDate.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentDate;
                            ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.VendorId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].VendorId;
                            ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.TotalApprovedDocumentAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].TotalApprovedDocumentAmount;
                            ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.ManualPaymentNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GPVoucherNumber;
                            ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.VoucherNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].VoucherNumber;
                            ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Save();
                        }

                        ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Release();
                        ctsiDocumentsWithManualPaymentsForm.Tables.CtsiDocumentsManualPaymentsTemp.Close();

                        //Fill details to scrolling window.Call dex procedure to fill the scrolling window.
                        ctsiDocumentsWithManualPaymentsForm.Procedures.CtsiDocumentsWithManualPaymentsFillScroll.Invoke();
                        ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.UpdateAsManuallyCreated.Unlock();
                        logMessage.AppendLine(DateTime.Now + " Successfully completed loading the scroll contents to link failed CTSI to manual payments scrolling window..");

                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="responseObj"></param>
        private void LoadReuploadValidCtsiScrollContents(PayableManagementResponse responseObj)
        {
            try
            {
                //clear scroll
                ClearScrollingWindows(Resources.STR_ReUploadFailureCtsiDocuments, Resources.STR_CTSIReUploadValidDocumentsScroll);
                ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Close();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Release();
                for (int i = 0; i < responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Count; i++)
                {
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.CtsiStatus.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].CtsiStatus.ToString().Trim();
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.OriginialCtsiInvoiceId.Value = Convert.ToInt32(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].OriginalCTSIInvoiceId.ToString().Trim());
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Ctsiid.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].CtsiId.ToString().Trim();
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.DocumentDate.Value = Convert.ToDateTime(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentDate);
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.VendorId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].VendorId.ToString().Trim();
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.VoucherNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentNumber.ToString().Trim();
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.TotalApprovedDocumentAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].TotalApprovedDocumentAmount);
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.ApprovedAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ApprovedAmount);
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.FreightAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].FreightAmount);
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.TotalOverChargeAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].OverCharge);
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.MiscAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].MiscellaneousAmount);
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.TradeDiscountAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].TradeDiscounts);
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.TaxAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].TaxAmount);
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.CurrencyCode.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].CurrencyCode.ToString().Trim();
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.CpReference.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].CPTReference.ToString().Trim();
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.CtsiAirwayInvoiceNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].AirwayInvoiceNumber.ToString().Trim();
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.GlAccountNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GLAccountNumber.ToString().Trim();
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.GlAccountDescription.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GLAccountDescription.ToString().Trim();
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.ErrorMessageText.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ErrorDescription.ToString().Trim();
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.ValidDocumentNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ValidDocumentNumber.ToString().Trim();
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.CpCreditAccountNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].CreditAccountNumber.Trim();
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.CurrencyDecimalPlaces.Value = Convert.ToInt32(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].CurrencyDecimal.ToString());
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.DebitDistributionType.Value = Convert.ToInt16(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DebitDistributionType.ToString());
                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.CreditAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].CreditAmount.ToString());

                    if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_CPEURCompany)
                    {
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.BaseLocalCharge.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].BaseLocalCharge;
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.BaseZeroRatedCharge.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].BaseZeroRatedCharge;
                        ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.BaseReverseCharge.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].BaseReverseCharge;
                    }

                    ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Save();
                }
                //if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_CPEURCompany && responseObj.PayableDetailsEntity.TaxDetails != null)
                //{
                //    taxDetails = responseObj.PayableDetailsEntity.TaxDetails;
                //}

                ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Release();
                ctsiReUploadFailureDocumentsForm.Tables.CtsiValidReUploadDocumentsTemp.Close();

                ctsiReUploadFailureDocumentsForm.Procedures.ReUploadValidatedCtsiRecordsFormScroll.Invoke();

                if ((responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Where(o => o.CtsiStatus.ToString().ToUpper() == Resources.STR_CTSIStatusFail)).Count() == 0)
                {
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.PushToGp.Enable();
                }
                else
                    ctsiReUploadFailureDocumentsForm.CtsiReUploadFailureDocuments.PushToGp.Disable();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
        #endregion

        #region PrivateMethodsExpenseReportUploadForm
        /// <summary>
        /// Method to read the data from dex.ini file
        /// </summary>
        /// <param name="iniFile"></param>
        /// <param name="category"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        //private static string GetIniFileString(string iniFile, string category, string key, string defaultValue)
        //{
        //    string returnString = new string(' ', 1024);
        //    NativeMethods.GetPrivateProfileString(category, key, defaultValue, returnString, 1024, iniFile);
        //    return returnString.Split('\0')[0];
        //}
        /// <summary>
        /// Method to get the catagory for the ini file.
        /// </summary>
        /// <param name="iniFile"></param>
        /// <returns></returns>
        //private static List<string> GetCategories(string iniFile)
        //{
        //    string returnString = new string(' ', 65536);
        //    NativeMethods.GetPrivateProfileString(null, null, null, returnString, 65536, iniFile);
        //    List<string> result = new List<string>(returnString.Split('\0'));
        //    result.RemoveRange(result.Count - 2, 2);
        //    return result;
        //}
        /// <summary>
        /// Cleare scroll temp table
        /// </summary>
        //private void ClearExpenseReportUploadScroll()
        //{
        //    try
        //    {
        //        pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.Close();
        //        pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.Release();
        //        TableError tableRemove = pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.ChangeFirst();
        //        while (tableRemove == TableError.NoError && tableRemove != TableError.EndOfTable)
        //        {
        //            tableRemove = pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.Remove();
        //            tableRemove = pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.ChangeNext();
        //        }
        //        pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.Release();
        //        pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.Close();

        //        pmExpenseReportUploadForm.Procedures.PmExpenseReportUploadFormScrollFill.Invoke();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, Resources.STR_MESSAGE_TITLE);
        //    }
        //    finally
        //    {
        //        pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.Release();
        //        pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.Close();
        //    }
        //}
        ///// <summary>
        ///// Method to load the expense details.
        ///// </summary>
        //private void LoadExpenseReportUploadScrollContents(PayableManagementResponse responseObj)
        //{
        //    try
        //    {
        //        //clear temp table
        //        this.ClearExpenseReportUploadScroll();
        //        pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.Close();
        //        pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.Release();
        //        for (int i = 0; i < responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Count; i++)
        //        {
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.Release();
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.ExpenseRowId.Value = i;
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.ExpenseErrorId.Value = Convert.ToInt16(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ErrorId.ToString().Trim());
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.OriginalExpenseFileDetailId.Value = Convert.ToInt32(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].OriginalExpenseFileDetailId.ToString().Trim());
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.ExpenseFileId.Value = Convert.ToInt32(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ExpenseFileId.ToString().Trim());
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.ExpenseReportId.Value = Convert.ToInt32(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ExpenseRecordId.ToString().Trim());
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.DocumentTypeName.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentType.ToString().Trim();
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.DocumentNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentNumber.ToString().Trim();
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.DocumentDate.Value = Convert.ToDateTime(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentDate);
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.WeekEndingDate.Value = Convert.ToDateTime(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].WeekendingDate);
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.VendorId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].VendorId.ToString().Trim();
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.TotalApprovedDocumentAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].TotalApprovedDocumentAmount);
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.ApprovedAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ApprovedAmount);
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.CurrencyCode.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].CurrencyId.ToString().Trim();
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.ExpenseTypeName.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ExpenseTypeName;
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.Account.Value = Convert.ToInt32(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].AccountNumber);
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.DepartmentCode.Value = Convert.ToInt32(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DepartmentCode);
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.BusinessGroupName.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].BusinessGroupName.ToString().Trim();
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.BusinessUnitName.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].BusinessUnitName.ToString().Trim();
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.VendorName.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].VendorName.ToString().Trim();
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.City.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].VendorCity.ToString().Trim();
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.ErrorMessageText.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ErrorDescription.ToString().Trim();
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.ExpenseStatus.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ExpenseStatus.ToString().Trim();
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.GlAccountDescription.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GLAccountDescription.ToString().Trim();
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.GlAccountNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GLAccountNumber.ToString().Trim();
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.GlAccountShortDescription.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GLAccountShortDescription.ToString().Trim();
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.ExpenseDescription.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ExpenseDescription.ToString().Trim();
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.CpCreditAccountNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].CreditAccountNumber.ToString().Trim();
        //            pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.Save();
        //        }

        //        pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.Release();
        //        pmExpenseReportUploadForm.Tables.PmExpenseReportUploadTemp.Close();

        //        pmExpenseReportUploadForm.Procedures.PmExpenseReportUploadFormScrollFill.Invoke();

        //        if (responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Count > 0)
        //        {
        //            pmExpenseReportUploadForm.PmExpenseReportUpload.PushToGp.Unlock();
        //        }
        //        if (responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Where(o => o.ExpenseStatus == Resources.STR_ExpenseStatusFail).Count() > 0)
        //        {
        //            MessageBox.Show(Resources.STR_ExpenseUploadContainsFailedRecords);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message.ToString());
        //    }
        //}

        #endregion PrivateMethodsExpenseReportUploadForm

        #region ApiReuploadfailedDocuments
        /// <summary>
        /// CTSI ReUpload Failure Documents
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormOpenBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!RegisterReUploadApi)
                {
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalInvoiceType.Change += new EventHandler(LocalInvoiceType_Change);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.RedisplayButton.ClickAfterOriginal += new EventHandler(apiReUploadFailureDocumentsFormRedisplayButtonClickAfterOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalDocumentBy.Change += new EventHandler(apiReUploadFailureDocumentsFormLocalDocumentByChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ReValidate.ClickAfterOriginal += new EventHandler(apiReUploadFailureDocumentsFormReValidateClickAfterOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LookupButton1.ClickAfterOriginal += new EventHandler(apiReUploadFailureDocumentsFormLookupButton1ClickAfterOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LookupButton2.ClickAfterOriginal += new EventHandler(apiReUploadFailureDocumentsFormLookupButton2ClickAfterOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ClearButton.ClickAfterOriginal += new EventHandler(apiReUploadFailureDocumentsFormClearButtonClickAfterOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.AllOrRange.Change += new EventHandler(apiReUploadFailureDocumentsFormAllOrRangeChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromApiId.Change += new EventHandler(apiReUploadFailureDocumentsFormLocalFromCtsiIdChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToApiId.Change += new EventHandler(apiReUploadFailureDocumentsFormLocalToCtsiIdChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromVendorId.Change += new EventHandler(apiReUploadFailureDocumentsFormLocalFromVendorIdChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToVendorId.Change += new EventHandler(apiReUploadFailureDocumentsFormLocalToVendorIdChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.PushToGp.ClickAfterOriginal += new EventHandler(apiReUploadFailureDocumentsFormPushToGpClickAfterOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.OkButton.ClickAfterOriginal += new EventHandler(apiReUploadFailureDocumentsFormOkButtonClickAfterOriginal);

                    //non po scrolling window
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.SelectCheckBox.Change += new EventHandler(apiReUploadFailureDocumentsFormNonPOSelectCheckBoxChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.VoucherNumber.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormNonPOVoucherNumberEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.VoucherNumber.Change += new EventHandler(apiReUploadFailureDocumentsFormNonPOVoucherNumberChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.DocumentDate.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormNonPODocumentDateEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.DocumentDate.Change += new EventHandler(apiReUploadFailureDocumentsFormNonPODocumentDateChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.VendorId.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormNonPOVendorIdEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.VendorId.Change += new EventHandler(apiReUploadFailureDocumentsFormNonPOVendorIdChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.DocumentAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormNonPODocumentAmountEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.DocumentAmount.Change += new EventHandler(apiReUploadFailureDocumentsFormNonPODocumentAmountChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.ApprovedAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormNonPOApprovedAmountEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.ApprovedAmount.Change += new EventHandler(apiReUploadFailureDocumentsFormNonPOApprovedAmountChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.FreightAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormNonPOFreightAmountEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.FreightAmount.Change += new EventHandler(apiReUploadFailureDocumentsFormNonPOFreightAmountChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.TaxAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormNonPOTaxAmountEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.TaxAmount.Change += new EventHandler(apiReUploadFailureDocumentsFormNonPOTaxAmountChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.PurchasesAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormNonPOPurchasesAmountEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.PurchasesAmount.Change += new EventHandler(apiReUploadFailureDocumentsFormNonPOPurchasesAmountChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountNumber.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormNonPOGlAccountNumberEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountNumber.Change += new EventHandler(apiReUploadFailureDocumentsFormNonPOGlAccountNumberChange);
                    //apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountDescription.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormNonPOGlAccountDescriptionEnterBeforeOriginal);
                    //apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountDescription.Change += new EventHandler(apiReUploadFailureDocumentsFormNonPOGlAccountDescriptionChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.TradeDiscountAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormNonPOTradeDiscountAmountEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.TradeDiscountAmount.Change += new EventHandler(apiReUploadFailureDocumentsFormNonPOTradeDiscountAmountChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.MiscAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormNonPOMiscAmountEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.MiscAmount.Change += new EventHandler(apiReUploadFailureDocumentsFormNonPOMiscAmountChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.TaxScheduleId.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormNonPOTaxScheduleIdEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.TaxScheduleId.Change += new EventHandler(apiReUploadFailureDocumentsFormNonPOTaxScheduleIdChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.LocationName.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormNonPOLocationNameEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.LocationName.Change += new EventHandler(apiReUploadFailureDocumentsFormNonPOLocationNameChange);

                    //po scrolling window
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.SelectCheckBox.Change += new EventHandler(apiReUploadFailureDocumentsFormPOSelectCheckBoxChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.DocumentNumber.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPODocumentNumberEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.DocumentNumber.Change += new EventHandler(apiReUploadFailureDocumentsFormPODocumentNumberChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ReceiptDate.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPOReceiptDateEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ReceiptDate.Change += new EventHandler(apiReUploadFailureDocumentsFormPOReceiptDateChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.VendorId.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPOVendorIdEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.VendorId.Change += new EventHandler(apiReUploadFailureDocumentsFormPOVendorIdChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.DocumentAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPODocumentAmountEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.DocumentAmount.Change += new EventHandler(apiReUploadFailureDocumentsFormPODocumentAmountChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ApprovedAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPOApprovedAmountEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ApprovedAmount.Change += new EventHandler(apiReUploadFailureDocumentsFormPOApprovedAmountChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.FreightAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPOFreightAmountEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.FreightAmount.Change += new EventHandler(apiReUploadFailureDocumentsFormPOFreightAmountChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.TaxAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPOTaxAmountEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.TaxAmount.Change += new EventHandler(apiReUploadFailureDocumentsFormPOTaxAmountChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.MiscAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPOMiscAmountEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.MiscAmount.Change += new EventHandler(apiReUploadFailureDocumentsFormPOMiscAmountChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.PurchasesAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPOPurchasesAmountEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.PurchasesAmount.Change += new EventHandler(apiReUploadFailureDocumentsFormPOPurchasesAmountChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.PoNumber.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPOPoNumberEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.PoNumber.Change += new EventHandler(apiReUploadFailureDocumentsFormPOPoNumberChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.PoLineNumber.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPOPoLineNumberEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.PoLineNumber.Change += new EventHandler(apiReUploadFailureDocumentsFormPOPoLineNumberChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ItemNumber.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPOItemNumberEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ItemNumber.Change += new EventHandler(apiReUploadFailureDocumentsFormPOItemNumberChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.PopReceiptNumber.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPOPopReceiptNumberEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.PopReceiptNumber.Change += new EventHandler(apiReUploadFailureDocumentsFormPOPopReceiptNumberChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ReceiptLineNumber.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPOReceiptLineNumberEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ReceiptLineNumber.Change += new EventHandler(apiReUploadFailureDocumentsFormPOReceiptLineNumberChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.QtyShipped.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPOQtyShippedEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.QtyShipped.Change += new EventHandler(apiReUploadFailureDocumentsFormPOQtyShippedChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.QtyToInvoice.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPOAdjustedUnitsEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.QtyToInvoice.Change += new EventHandler(apiReUploadFailureDocumentsFormPOAdjustedUnitsChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.UnitCost.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPOUnitCostEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.UnitCost.Change += new EventHandler(apiReUploadFailureDocumentsFormPOUnitCostChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ExtendedCost.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPOExtendedCostEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ExtendedCost.Change += new EventHandler(apiReUploadFailureDocumentsFormPOExtendedCostChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.AdjustedUnitCost.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPOAdjustedUnitCostEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.AdjustedUnitCost.Change += new EventHandler(apiReUploadFailureDocumentsFormPOAdjustedUnitCostChange);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.LocationName.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(apiReUploadFailureDocumentsFormPOLocationNameEnterBeforeOriginal);
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.LocationName.Change += new EventHandler(apiReUploadFailureDocumentsFormPOLocationNameChange);

                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.CloseAfterOriginal += new EventHandler(apiReUploadFailureDocumentsFormApiReUploadFailureDocumentsCloseAfterOriginal);
                    RegisterReUploadApi = true;
                }

                apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalInvoiceType.Value = 2;
                apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalInvoiceType.RunValidate();

                apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value = false;
                apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = false;
                /*Commented now for api reuplaod fix
                //apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ScrollShrinkSwitch.Value = 1;
                //apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.CpScrollShrinkSwitch.Value = 1;
                //apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalShrinkButton.RunValidate();
                Commented now for api reuplaod fix*/
                // apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalShrinkButton1.RunValidate();
                apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalDocumentBy.Value = 1;
                documentType = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalInvoiceType.Value;
                InitilizeSortBySelection(Resources.STR_ReUploadFailureApiDocuments);
                apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.PushToGp.Lock();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the invoice type is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LocalInvoiceType_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                documentType = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalInvoiceType.Value;
                ClearHeaderFieds();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        void ClearHeaderFieds()
        {
            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromApiId.Clear();
            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToApiId.Clear();
            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromVendorId.Clear();
            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToVendorId.Clear();
            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToDocumentDate.Clear();
            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromDocumentDate.Clear();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormApiReUploadFailureDocumentsCloseAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (apiIdLookupForm.IsOpen)
                {
                    apiIdLookupForm.Close();
                }
                if (vendorLookupForm.IsOpen)
                {
                    vendorLookupForm.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// Api Re Upload Failure Documents Form Re-display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormRedisplayButtonClickAfterOriginal(object sender, EventArgs e)
        {
            PayableManagementRequest requestObj = null;
            StringBuilder logMessage = new StringBuilder();
            bool isValid = false;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                logMessage.AppendLine(DateTime.Now + " Started : Filling data to Re-Upload ApOut Documents: "
                    + " by user : " + Dynamics.Globals.UserId.Value.ToString()
                    + " in company : " + Dynamics.Globals.IntercompanyId.Value.ToString());
                requestObj = new PayableManagementRequest();

                if ((apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalDocumentBy.Value == 1)
                       && (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.AllOrRange.Value == 1)
                         && (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromApiId.Value != string.Empty)
                               && (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToApiId.Value != string.Empty))
                {
                    requestObj.SearchType = Resources.STR_SearchType_ApiInvoiceId;
                    requestObj.FromApiId = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromApiId.Value;
                    requestObj.ToApiId = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToApiId.Value;
                    requestObj.FromVendorId = string.Empty;
                    requestObj.ToVendorId = string.Empty;
                    requestObj.FromDocumentDate = Convert.ToDateTime("01/01/1900");
                    requestObj.ToDocumentDate = Convert.ToDateTime("01/01/1900");
                    isValid = true;

                }
                else if ((apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalDocumentBy.Value == 2)
                       && (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.AllOrRange.Value == 1)
                         && (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromDocumentDate.Value != DateTime.MinValue)
                               && (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToDocumentDate.Value != DateTime.MinValue))
                {
                    requestObj.SearchType = Resources.STR_SearchType_DocumentDate;
                    requestObj.FromDocumentDate = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromDocumentDate.Value;
                    requestObj.ToDocumentDate = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToDocumentDate.Value;
                    requestObj.FromVendorId = string.Empty;
                    requestObj.ToVendorId = string.Empty;
                    requestObj.FromApiId = string.Empty;
                    requestObj.ToApiId = string.Empty;
                    isValid = true;

                }
                else if ((apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalDocumentBy.Value == 3)
                       && (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.AllOrRange.Value == 1)
                         && (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromVendorId.Value != string.Empty)
                               && (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToVendorId.Value != string.Empty))
                {
                    requestObj.SearchType = Resources.STR_SearchType_VendorId;
                    requestObj.FromVendorId = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromVendorId.Value;
                    requestObj.ToVendorId = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToVendorId.Value;
                    requestObj.FromDocumentDate = Convert.ToDateTime("01/01/1900");
                    requestObj.ToDocumentDate = Convert.ToDateTime("01/01/1900");
                    requestObj.FromApiId = string.Empty;
                    requestObj.ToApiId = string.Empty;
                    isValid = true;

                }
                else if (((apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalDocumentBy.Value == 1)
                            || (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalDocumentBy.Value == 2)
                                || (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalDocumentBy.Value == 3))
                                    && (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.AllOrRange.Value == 0))
                {
                    requestObj.SearchType = Resources.STR_SearchType_All;
                    requestObj.FromVendorId = string.Empty;
                    requestObj.ToVendorId = string.Empty;
                    requestObj.FromDocumentDate = Convert.ToDateTime("01/01/1900");
                    requestObj.ToDocumentDate = Convert.ToDateTime("01/01/1900");
                    requestObj.FromApiId = string.Empty;
                    requestObj.ToApiId = string.Empty;
                    isValid = true;
                }
                else
                {
                    MessageBox.Show(Resources.STR_RequiredFieldsMissing, Resources.STR_MESSAGE_TITLE);
                }
                if (isValid)
                {
                    requestObj.userId = Dynamics.Globals.UserId.Value.ToString();
                    requestObj.Company = Dynamics.Globals.IntercompanyId.Value.ToString();
                    requestObj.companyId = Dynamics.Globals.CompanyId.Value;

                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalInvoiceType.Value == 1)
                        requestObj.InvoiceType = 1;
                    else
                        requestObj.InvoiceType = 2;


                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/PayableManagement/GetFailedApiDocuments", requestObj);
                        if (response.Result.IsSuccessStatusCode)
                        {
                            PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                            if (payableResponse.Status == Response.Error)
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                MessageBox.Show(Resources.STR_ErrorPopulatingFailedApiDocuments);
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In apiReUploadFailureDocumentsFormRedisplayButtonClickAfterOriginal: " + payableResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                if (payableResponse != null)
                                {
                                    ReloadFailedApiDocumentscrollContents(payableResponse);
                                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value = false;
                                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = false;
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: apiReUploadFailureDocumentsFormRedisplayButtonClickAfterOriginal");
                            MessageBox.Show("Error: apiReUploadFailureDocumentsFormRedisplayButtonClickAfterOriginal", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
                requestObj = null;
            }
        }

        /// <summary>
        /// CTSI Filter methods
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormLocalDocumentByChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                InitilizeSortBySelection(Resources.STR_ReUploadFailureApiDocuments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormLookupButton1ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalDocumentBy.Value == 1)
                {
                    if (apiIdLookupForm.IsOpen)
                    {
                        apiIdLookupForm.Close();
                    }
                    apiIdLookupForm.Open();
                    apiIdLookupForm.ApiidLookup.RedisplayButton.RunValidate();
                    apiIdLookupForm.ApiidLookup.LocalCalledBy.Value = 1;

                    lookupWindowName = Resources.STR_ReUploadFailureApiDocuments;
                    if (!RegisterApiLookupSelect)
                    {
                        apiIdLookupForm.ApiidLookup.SelectButtonMnemonic.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.apiIdLookupFormApiidLookupSelectButtonClickBeforeOriginal);
                        RegisterApiLookupSelect = true;
                    }
                }
                else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalDocumentBy.Value == 3)
                {
                    if (vendorLookupForm.IsOpen)
                    {
                        vendorLookupForm.Close();
                    }
                    vendorLookupForm.Open();
                    // Assigning the customer number to lookup form
                    vendorLookupForm.VendorLookup.VendorSortBy.Value = 1;
                    // select the record
                    vendorLookupForm.VendorLookup.VendorId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromVendorId.Value;
                    vendorLookupForm.VendorLookup.RedisplayButton.RunValidate();
                    // event which calls before the lookup window got closed.
                    vendorLookupForm.VendorLookup.LocalCalledBy.Value = 1;

                    lookupWindowName = Resources.STR_ReUploadFailureApiDocuments;
                    if (!RegisterApiVendorLookupSelect)
                    {
                        vendorLookupForm.VendorLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.vendorLookupFormVendorLookupSelectButtonClickBeforeOriginal);
                        RegisterApiVendorLookupSelect = true;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }


        /// <summary>
        /// Event method called on selecting the ctsiid in ctsiId lookup window and value returned to from ctsi Id field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void apiIdLookupFormApiidLookupSelectButtonClickBeforeOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!String.IsNullOrEmpty(lookupWindowName))
                {
                    if (lookupWindowName == Resources.STR_ReUploadFailureApiDocuments && apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.IsOpen)
                    {
                        if (apiIdLookupForm.ApiidLookup.LocalCalledBy.Value == 1)
                        {
                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromApiId.Focus();
                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromApiId.Value = apiIdLookupForm.ApiidLookup.ApiIdScroll.DocumentId.Value;
                            apiIdLookupForm.Close();
                        }
                        else if (apiIdLookupForm.ApiidLookup.LocalCalledBy.Value == 2)
                        {
                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToApiId.Focus();
                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToApiId.Value = apiIdLookupForm.ApiidLookup.ApiIdScroll.DocumentId.Value;
                            apiIdLookupForm.Close();
                        }
                    }
                    else if (lookupWindowName == Resources.STR_APIDocumentsWithManualPayments && apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.IsOpen)
                    {
                        if (apiIdLookupForm.ApiidLookup.LocalCalledBy.Value == 1)
                        {

                            apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromDocId.Focus();
                            apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromDocId.Value = apiIdLookupForm.ApiidLookup.ApiIdScroll.DocumentId.Value;
                            apiIdLookupForm.Close();
                        }
                        else if (apiIdLookupForm.ApiidLookup.LocalCalledBy.Value == 2)
                        {
                            apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToDocId.Focus();
                            apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToDocId.Value = apiIdLookupForm.ApiidLookup.ApiIdScroll.DocumentId.Value;
                            apiIdLookupForm.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// Event method called on selecting the ctsiid in ctsiId lookup window and value returned to from ctsi Id field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void vendorLookupFormVendorLookupSelectButtonClickBeforeOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!String.IsNullOrEmpty(lookupWindowName))
                {
                    if (lookupWindowName == Resources.STR_ReUploadFailureApiDocuments && apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.IsOpen)
                    {
                        if (vendorLookupForm.VendorLookup.LocalCalledBy.Value == 1)
                        {
                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromVendorId.Focus();
                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromVendorId.Value = vendorLookupForm.VendorLookup.VendorLookupScroll.VendorId.Value.ToString();
                            vendorLookupForm.Close();
                        }
                        else if (vendorLookupForm.VendorLookup.LocalCalledBy.Value == 2)
                        {
                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToVendorId.Focus();
                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToVendorId.Value = vendorLookupForm.VendorLookup.VendorLookupScroll.VendorId.Value.ToString();
                            vendorLookupForm.Close();
                        }
                    }
                    else if (lookupWindowName == Resources.STR_APIDocumentsWithManualPayments && apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.IsOpen)
                    {
                        if (vendorLookupForm.VendorLookup.LocalCalledBy.Value == 1)
                        {
                            apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromVendorId.Focus();
                            apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromVendorId.Value = vendorLookupForm.VendorLookup.VendorLookupScroll.VendorId.Value.ToString();
                            vendorLookupForm.Close();
                        }
                        else if (vendorLookupForm.VendorLookup.LocalCalledBy.Value == 2)
                        {
                            apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToVendorId.Focus();
                            apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToVendorId.Value = vendorLookupForm.VendorLookup.VendorLookupScroll.VendorId.Value.ToString();
                            vendorLookupForm.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormLookupButton2ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalDocumentBy.Value == 1)
                {
                    if (apiIdLookupForm.IsOpen)
                    {
                        apiIdLookupForm.Close();
                    }
                    apiIdLookupForm.Open();
                    apiIdLookupForm.ApiidLookup.RedisplayButton.RunValidate();
                    apiIdLookupForm.ApiidLookup.LocalCalledBy.Value = 2;

                    lookupWindowName = Resources.STR_ReUploadFailureApiDocuments;
                    if (!RegisterApiLookupSelect)
                    {
                        apiIdLookupForm.ApiidLookup.SelectButtonMnemonic.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.apiIdLookupFormApiidLookupSelectButtonClickBeforeOriginal);
                        RegisterApiLookupSelect = true;
                    }
                }
                else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalDocumentBy.Value == 3)
                {
                    vendorLookupForm.Open();
                    // Assigning the customer number to lookup form
                    vendorLookupForm.VendorLookup.VendorSortBy.Value = 1;
                    // select the record
                    vendorLookupForm.VendorLookup.VendorId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToVendorId.Value;
                    vendorLookupForm.VendorLookup.RedisplayButton.RunValidate();
                    // event which calls before the lookup window got closed.
                    vendorLookupForm.VendorLookup.LocalCalledBy.Value = 2;

                    lookupWindowName = Resources.STR_ReUploadFailureApiDocuments;
                    if (!RegisterApiVendorLookupSelect)
                    {
                        vendorLookupForm.VendorLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.vendorLookupFormVendorLookupSelectButtonClickBeforeOriginal);
                        RegisterApiVendorLookupSelect = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormClearButtonClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {

                /*Commented out //apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ScrollShrinkSwitch.Value = 1;
                //apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.CpScrollShrinkSwitch.Value = 1;
                //apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalShrinkButton.RunValidate();
                Commented out */
                //apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalShrinkButton1.RunValidate();
                apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalDocumentBy.Value = 1;
                InitilizeSortBySelection(Resources.STR_ReUploadFailureApiDocuments);
                ClearScrollingWindows(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_AllScroll);
                apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.PushToGp.Lock();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormAllOrRangeChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalDocumentBy.Value == 1
                        && apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.AllOrRange.Value == 0)
                {
                    ClearApiIdFields(Resources.STR_ReUploadFailureApiDocuments);
                }
                else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalDocumentBy.Value == 2
                            && apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.AllOrRange.Value == 0)
                {
                    ClearReUploadDocumentDateFields(Resources.STR_ReUploadFailureApiDocuments);
                }
                else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalDocumentBy.Value == 3
                            && apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.AllOrRange.Value == 0)
                {
                    ClearReUploadVendorIdFields(Resources.STR_ReUploadFailureApiDocuments);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// When the API FROM document id from changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormLocalFromCtsiIdChange(object sender, EventArgs e)
        {
            PayableManagementRequest requestObj = null;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromApiId.Value != string.Empty)
                {
                    requestObj = new PayableManagementRequest();
                    requestObj.SourceLookup = Resources.STR_APIId;
                    requestObj.LookupValue = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromApiId.Value;
                    requestObj.userId = Dynamics.Globals.UserId.Value.ToString();
                    requestObj.companyId = Dynamics.Globals.CompanyId.Value;

                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/PayableManagement/VerifyApiLookupValueExists", requestObj);
                        if (response.Result.IsSuccessStatusCode)
                        {
                            PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                            if (payableResponse.Status == Response.Error)
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In apiReUploadFailureDocumentsFormLocalFromCtsiIdChange: " + payableResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                if (!payableResponse.LookupValueExists)
                                {
                                    MessageBox.Show(Resources.STR_APIIdDoesNotExists, Resources.STR_MESSAGE_TITLE);
                                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromApiId.Clear();
                                }
                                else
                                {
                                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromApiId.Value == string.Empty)
                                    {
                                        if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToApiId.Value == string.Empty)
                                        {
                                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.AllOrRange.Value = 0;
                                        }
                                    }
                                    else
                                    {
                                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.AllOrRange.Value = 1;
                                        if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToApiId.Value == string.Empty)
                                        {
                                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToApiId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromApiId.Value;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: apiReUploadFailureDocumentsFormLocalFromCtsiIdChange");
                            MessageBox.Show("Error: apiReUploadFailureDocumentsFormLocalFromCtsiIdChange", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormLocalToCtsiIdChange(object sender, EventArgs e)
        {
            PayableManagementRequest requestObj = null;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToApiId.Value != string.Empty)
                {
                    requestObj = new PayableManagementRequest();
                    requestObj.SourceLookup = Resources.STR_APIId;
                    requestObj.LookupValue = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToApiId.Value.ToString();
                    requestObj.companyId = Dynamics.Globals.CompanyId.Value;


                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/PayableManagement/VerifyLookupValueExists", requestObj);
                        if (response.Result.IsSuccessStatusCode)
                        {
                            PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                            if (payableResponse.Status == Response.Error)
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In apiReUploadFailureDocumentsFormLocalToCtsiIdChange: " + payableResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                if (!payableResponse.LookupValueExists)
                                {
                                    MessageBox.Show(Resources.STR_APIIdDoesNotExists, Resources.STR_MESSAGE_TITLE);
                                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToApiId.Value = string.Empty;
                                }
                                else
                                {
                                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToApiId.Value == string.Empty)
                                    {
                                        if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromApiId.Value == string.Empty)
                                        {
                                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.AllOrRange.Value = 0;
                                        }
                                    }
                                    else
                                    {
                                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.AllOrRange.Value = 1;
                                    }
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: apiReUploadFailureDocumentsFormLocalToCtsiIdChange");
                            MessageBox.Show("Error: apiReUploadFailureDocumentsFormLocalToCtsiIdChange", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
                requestObj = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormLocalFromVendorIdChange(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromVendorId.Value != string.Empty)
                {
                    apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Close();
                    apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Release();
                    apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.VendorId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromVendorId.Value;
                    apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Key = 1;
                    TableError Vendorerror = apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Get();

                    if (Vendorerror != TableError.NoError)
                    {
                        // if not valid displays a message
                        MessageBox.Show(Resources.STR_VendorIdDoesNotExists, Resources.STR_MESSAGE_TITLE);
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromVendorId.Clear();
                    }
                    else
                    {
                        if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromVendorId.Value == string.Empty)
                        {
                            if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToVendorId.Value == string.Empty)
                            {
                                apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.AllOrRange.Value = 0;
                            }
                        }
                        else
                        {
                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.AllOrRange.Value = 1;
                            if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToVendorId.Value == string.Empty)
                            {
                                apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToVendorId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromVendorId.Value;
                            }
                        }
                    }

                    apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Close();
                    apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Release();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormLocalToVendorIdChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToVendorId.Value != string.Empty)
                {
                    apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Close();
                    apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Release();
                    apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.VendorId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToVendorId.Value;
                    apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Key = 1;
                    TableError Vendorerror = apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Get();

                    if (Vendorerror != TableError.NoError)
                    {
                        // if not valid displays a message
                        MessageBox.Show(Resources.STR_VendorIdDoesNotExists, Resources.STR_MESSAGE_TITLE);
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToVendorId.Clear();

                    }
                    else
                    {
                        if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToVendorId.Value == string.Empty)
                        {
                            if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromVendorId.Value == string.Empty)
                            {
                                apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.AllOrRange.Value = 0;
                            }
                        }
                        else
                        {
                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.AllOrRange.Value = 1;
                        }
                    }
                    apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Close();
                    apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Release();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormOkButtonClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormReValidateClickAfterOriginal(object sender, EventArgs e)
        {
            PayableManagementRequest reqObj = null;
            StringBuilder logMessage = new StringBuilder();
            Cursor.Current = Cursors.WaitCursor;
            try
            {

                if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalInvoiceType.Value == 1)
                {

                    logMessage.AppendLine(DateTime.Now + " Started : ReValidating Api Non reupload scroll details "
                      + " by user : " + userId);

                    reqObj = new PayableManagementRequest();
                    reqObj.PayableDetailsEntity = new PayableDetailsEntity();
                    reqObj.userId = Dynamics.Globals.UserId.Value;
                    reqObj.Company = Dynamics.Globals.IntercompanyId.Value;

                    if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_ChmptCompany)
                        reqObj.companyId = Convert.ToInt16(Resources.NACompanyId);
                    else
                        reqObj.companyId = Convert.ToInt16(Resources.EUCompanyId);
                    reqObj.SourceFormName = Resources.STR_ReUploadFailureApiDocuments;
                    reqObj.InvoiceType = 1;

                    apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                    apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();

                    TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.GetFirst();
                    while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        if (apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.SelectCheckBox.Value)
                        {
                            PayableLineEntity line = new PayableLineEntity()
                            {
                                OriginalApiInvoiceId = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.OriginalApiInvoiceId.Value,
                                DocumentId = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.DocumentId.Value,
                                DocumentTypeName = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.DocumentTypeName.Value,
                                VoucherNumber = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.VoucherNumber.Value,
                                DocumentDate = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.DocumentDate.Value,
                                VendorId = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.VendorId.Value,
                                DocumentAmount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.DocumentAmount.Value,
                                ApprovedAmount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.ApprovedAmount.Value,
                                FreightAmount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.FreightAmount.Value,
                                TaxAmount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.TaxAmount.Value,
                                PurchaseAmount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.PurchasesAmount.Value,
                                CurrencyCode = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.CurrencyId.Value,
                                GLAccount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.GlAccountNumber.Value,
                                GLAccountDescription = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.GlAccountDescription.Value,
                                TradeDiscounts = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.TradeDiscountAmount.Value,
                                MiscellaneousAmount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.MiscAmount.Value,
                                TaxScheduleId = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.TaxScheduleId.Value,
                                LocationName = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.LocationName.Value,
                                GLIndex = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.AccountIndex.Value,

                            };

                            reqObj.PayableDetailsEntity.AddInvoiceLine(line);
                        }
                        tableError = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.GetNext();
                    }

                    if (reqObj.PayableDetailsEntity.GetInvoiceLineDetails.Count > 0)
                    {
                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/PayableManagement/ValidateAndGetApiTransactions", reqObj);
                            if (response.Result.IsSuccessStatusCode)
                            {
                                PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                                if (payableResponse.Status == Response.Error)
                                {
                                    logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxDetailMaintenance_DeleteButtonClickAfterOriginal Method (DeleteTaxDetailCustomRecord): " + payableResponse.ErrorMessage.ToString());
                                    MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                    if (payableResponse != null && payableResponse.PayableDetailsEntity != null && payableResponse.PayableDetailsEntity.GetInvoiceLineDetails.Count > 0)
                                    {
                                        logMessage.Append(payableResponse.LogMessage.ToString());
                                        logMessage.AppendLine("Filling the scroll validated Api documents scroll");
                                        LoadReuploadValidApiScrollContents(payableResponse);
                                        // decimalPlaces = responseObj.CurrencyDecimal;
                                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value = false;
                                    }
                                }
                            }
                            else
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Expense Report Upload PushToGp ClickAfterOriginal");
                                MessageBox.Show("Error: Expense Report Upload PushToGp ClickAfterOriginal", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(Resources.STR_SelectApiInvoiceId, Resources.STR_MESSAGE_TITLE);
                    }

                }
                else
                {
                    logMessage.AppendLine(DateTime.Now + " Started : ReValidating Api PO reupload scroll details "
                       + " by user : " + userId);

                    reqObj = new PayableManagementRequest();
                    reqObj.PayableDetailsEntity = new PayableDetailsEntity();
                    reqObj.userId = Dynamics.Globals.UserId.Value;
                    reqObj.Company = Dynamics.Globals.IntercompanyId.Value;

                    if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_ChmptCompany)
                        reqObj.companyId = Convert.ToInt16(Resources.NACompanyId);
                    else
                        reqObj.companyId = Convert.ToInt16(Resources.EUCompanyId);
                    reqObj.SourceFormName = Resources.STR_ReUploadFailureApiDocuments;
                    reqObj.InvoiceType = 2;

                    apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                    apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                    TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.GetFirst();
                    while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        if (apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.SelectCheckBox.Value)
                        {
                            PayableLineEntity line = new PayableLineEntity()
                            {
                                OriginalApiInvoiceId = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value,
                                DocumentId = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.DocumentId.Value,
                                DocumentTypeName = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.DocumentTypeName.Value,
                                DocumentNumber = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.DocumentNumber.Value,
                                ReceiptDate = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ReceiptDate.Value,
                                VendorId = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.VendorId.Value,
                                DocumentAmount = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.DocumentAmount.Value,
                                ApprovedAmount = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ApprovedAmount.Value,
                                FreightAmount = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.FreightAmount.Value,
                                TaxAmount = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.TaxAmount.Value,
                                MiscellaneousAmount = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.MiscAmount.Value,
                                PurchaseAmount = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.PurchasesAmount.Value,
                                PurchaseOrderNumber = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.PoNumber.Value,
                                CurrencyCode = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.CurrencyId.Value,
                                POLineNumber = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.PoLineNumber.Value,
                                ItemNumber = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ItemNumber.Value,
                                ReceiptNumber = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.PopReceiptNumber.Value,
                                ReceiptLineNumber = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ReceiptLineNumber.Value,
                                QuantityShipped = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.QtyShipped.Value,
                                AdjustedItemUnitQuantity = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.QtyToInvoice.Value,
                                UnitCost = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.UnitCost.Value,
                                ExtendedCost = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ExtendedCost.Value,
                                AdjustedItemUnitPrice = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.AdjustedUnitCost.Value,
                                GLAccount = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.GlAccountNumber.Value,
                                TaxScheduleId = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.TaxScheduleId.Value,
                                LocationName = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.LocationName.Value,
                                GLIndex = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.AccountIndex.Value,
                            };

                            reqObj.PayableDetailsEntity.AddInvoiceLine(line);
                        }
                        tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.GetNext();
                    }

                    if (reqObj.PayableDetailsEntity.GetInvoiceLineDetails.Count > 0)
                    {
                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/PayableManagement/ValidateAndGetApiTransactions", reqObj);
                            if (response.Result.IsSuccessStatusCode)
                            {
                                PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                                if (payableResponse.Status == Response.Error)
                                {
                                    logMessage.Append(payableResponse.LogMessage.ToString());
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxDetailMaintenance_DeleteButtonClickAfterOriginal Method (DeleteTaxDetailCustomRecord): " + payableResponse.ErrorMessage.ToString());
                                    MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                    if (payableResponse != null && payableResponse.PayableDetailsEntity != null && payableResponse.PayableDetailsEntity.GetInvoiceLineDetails.Count > 0)
                                    {
                                        logMessage.AppendLine("Filling the scroll validated Api documents scroll");
                                        LoadReuploadValidApiScrollContents(payableResponse);
                                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = false;
                                    }
                                }
                            }
                            else
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Expense Report Upload PushToGp ClickAfterOriginal");
                                MessageBox.Show("Error: Expense Report Upload PushToGp ClickAfterOriginal", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(Resources.STR_SelectApiInvoiceId, Resources.STR_MESSAGE_TITLE);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPushToGpClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            PayableManagementRequest reqObj = null;
            List<PayableLineEntity> payableLine = new List<PayableLineEntity>();
            try
            {
                // non-po
                if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalInvoiceType.Value == 1)
                {
                    if (!apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value)
                    {
                        reqObj = new PayableManagementRequest();
                        reqObj.PayableDetailsEntity = new PayableDetailsEntity();
                        
                        

                        if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_ChmptCompany)
                        {
                            reqObj.companyId = Convert.ToInt16(Resources.NACompanyId);
                        }
                        else
                        {
                            reqObj.companyId = Convert.ToInt16(Resources.EUCompanyId);
                        }
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Close();
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Release();
                        TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.GetFirst();

                        //DataSet invoiceDetailsDT = new DataSet("@LineDetails");
                        //invoiceDetailsDT.Tables.Add(new DataTable());
                        //invoiceDetailsDT.Tables[0].Columns.Add("OriginalApiInvoiceId", typeof(int));
                        //invoiceDetailsDT.Tables[0].Columns.Add("DocumentType", typeof(int));
                        //invoiceDetailsDT.Tables[0].Columns.Add("DocumentRowID", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("DocumentNumber", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("DocumentDate", typeof(DateTime));
                        //invoiceDetailsDT.Tables[0].Columns.Add("VendorNumber", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("DocumentAmount", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ApprovedAmount", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("FreightAmount", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("SalesTaxAmount", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("TradeDiscountAmount", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("MiscellaneousAmount", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("PurchasingAmount", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("POAmount", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("PONumber", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("PODateOpened", typeof(DateTime));
                        //invoiceDetailsDT.Tables[0].Columns.Add("POLineNumber", typeof(int));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ReceiptNumber", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ReceiptLineNumber", typeof(int));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ItemNumber", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ItemUnitQty", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ItemUnitPrice", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ItemExtendedAmount", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("AdjustedItemUnitQty", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("AdjustedItemUnitPrice", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ShipToState", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ShippedDate", typeof(DateTime));
                        //invoiceDetailsDT.Tables[0].Columns.Add("LocationKey", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("GLIndex", typeof(int));
                        //invoiceDetailsDT.Tables[0].Columns.Add("TaxScheduleId", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("FormTypeCode", typeof(int));
                        //invoiceDetailsDT.Tables[0].Columns.Add("IsDuplicate", typeof(int));
                        //invoiceDetailsDT.Tables[0].Columns.Add("RequiredDistribution", typeof(int));
                        //invoiceDetailsDT.Tables[0].Columns.Add("StatusId", typeof(int));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ErrorDescription", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("CurrencyId", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("UserId", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("Notes", typeof(string));

                        while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                        {
                            //DataRow newRow = invoiceDetailsDT.Tables[0].NewRow();
                            ////newRow["ApiFileId"] = 0;
                            //newRow["OriginalApiInvoiceId"] = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.OriginalApiInvoiceId.Value;
                            //newRow["DocumentType"] = ConvertDocTypeValueByInvoiceType(apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalInvoiceType.Value, apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.DocumentTypeName.Value);
                            //newRow["DocumentRowID"] = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.DocumentId.Value;
                            //newRow["DocumentNumber"] = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.VoucherNumber.Value;
                            //newRow["DocumentDate"] = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.DocumentDate.Value;
                            //newRow["VendorNumber"] = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.VendorId.Value;
                            //newRow["DocumentAmount"] = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.DocumentAmount.Value;
                            //newRow["ApprovedAmount"] = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.ApprovedAmount.Value;
                            //newRow["FreightAmount"] = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.FreightAmount.Value;
                            //newRow["SalesTaxAmount"] = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.TaxAmount.Value;
                            //newRow["TradeDiscountAmount"] = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.TradeDiscountAmount.Value;
                            //newRow["MiscellaneousAmount"] = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.MiscAmount.Value;
                            //newRow["PurchasingAmount"] = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.PurchasesAmount.Value;
                            //newRow["POAmount"] = 0;
                            //newRow["PONumber"] = "";
                            //newRow["PODateOpened"] = DateTime.Today;
                            //newRow["POLineNumber"] = 0;
                            //newRow["ReceiptNumber"] = "";
                            //newRow["ReceiptLineNumber"] = 0;
                            //newRow["ItemNumber"] = "";
                            //newRow["ItemUnitQty"] = 0;
                            //newRow["ItemUnitPrice"] = 0;
                            //newRow["ItemExtendedAmount"] = 0;
                            //newRow["AdjustedItemUnitQty"] = 0;
                            //newRow["AdjustedItemUnitPrice"] = 0;
                            //newRow["ShipToState"] = ""; //
                            //newRow["ShippedDate"] = DateTime.Today;
                            //newRow["LocationKey"] = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.LocationName.Value;
                            //newRow["GLIndex"] = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.AccountIndex.Value;
                            //newRow["TaxScheduleId"] = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.TaxScheduleId.Value;
                            //newRow["FormTypeCode"] = ConvertDocTypeValueByInvoiceType(apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalInvoiceType.Value, apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.DocumentTypeName.Value);
                            //newRow["IsDuplicate"] = 0;
                            //newRow["RequiredDistribution"] = 0;
                            //newRow["StatusId"] = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Status.Value;
                            //newRow["ErrorDescription"] = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.ErrorDescription.Value;
                            //newRow["CurrencyId"] = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.CurrencyId.Value;
                            //newRow["UserId"] = Dynamics.Globals.UserId.Value.ToString();
                            //newRow["Notes"] = "";

                            //invoiceDetailsDT.Tables[0].Rows.Add(newRow);
                            PayableLineEntity payable = new PayableLineEntity();
                            payable.OriginalApiInvoiceId = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.OriginalApiInvoiceId.Value;
                            payable.DocumentType = ConvertDocTypeValueByInvoiceType(apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalInvoiceType.Value, apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.DocumentTypeName.Value).ToString();
                            payable.DocumentRowId = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.DocumentId.Value;
                            payable.DocumentNumber = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.VoucherNumber.Value;
                            payable.DocumentDate = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.DocumentDate.Value;
                            payable.VendorName = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.VendorId.Value;
                            payable.DocumentAmount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.DocumentAmount.Value;
                            payable.ApprovedAmount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.ApprovedAmount.Value;
                            payable.FreightAmount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.FreightAmount.Value;
                            payable.TaxAmount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.TaxAmount.Value;
                            payable.TradeDiscounts = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.TradeDiscountAmount.Value;
                            payable.MiscellaneousAmount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.MiscAmount.Value;
                            payable.PurchaseAmount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.PurchasesAmount.Value;
                            payable.POAmount = 0;
                            payable.PurchaseOrderNumber = "";
                            payable.PODateOpened = DateTime.Today;
                            payable.POLineNumber = 0;
                            payable.ReceiptNumber = "";
                            payable.ReceiptLineNumber = 0;
                            payable.ItemNumber = "";
                            payable.ItemUnitQty = 0;
                            payable.ItemUnitPrice = 0;
                            payable.ExtendedCost = 0;
                            payable.AdjustedItemUnitQuantity = 0;
                            payable.AdjustedItemUnitPrice = 0;
                            payable.ShipToState = ""; //
                            payable.ShippedDate = DateTime.Today;
                            payable.LocationName = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.LocationName.Value;
                            payable.GLIndex = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.AccountIndex.Value;
                            payable.TaxScheduleId = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.TaxScheduleId.Value;
                            payable.FormTypeCode = ConvertDocTypeValueByInvoiceType(apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalInvoiceType.Value, apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.DocumentTypeName.Value);
                            payable.IsDuplicated = 0;
                            payable.RequiredDistribution = 0;
                            payable.StatusId = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Status.Value;
                            payable.ErrorDescription = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.ErrorDescription.Value;
                            payable.CurrencyId = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.CurrencyId.Value;
                            payable.UserId = Dynamics.Globals.UserId.Value.ToString();
                            payable.Notes = "";

                            payableLine.Add(payable);

                            tableError = apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.GetNext();
                        }

                        // if (invoiceDetailsDT.Tables[0].Rows.Count > 0)
                        if (payableLine != null && payableLine.Count > 0)
                        {
                            //reqObj.ApiTransactionDtValue = ConvertDataTableToString(invoiceDetailsDT.Tables[0]);
                            reqObj.PoValidationList = payableLine;

                            reqObj.UserId = Dynamics.Globals.UserId.Value;
                            reqObj.SourceFormName = Resources.STR_ReUploadFailureApiDocuments;
                            reqObj.FileName = string.Empty;
                            reqObj.Company = Dynamics.Globals.IntercompanyId.Value;
                            reqObj.InvoiceType = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalInvoiceType.Value;


                            // Service call ...
                            using (HttpClient client = new HttpClient())
                            {
                                client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                var response = client.PostAsJsonAsync("api/PayableManagement/UploadPayableDetailsIntoGpForApi", reqObj);
                                if (response.Result.IsSuccessStatusCode)
                                {
                                    PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                                    if (payableResponse.Status == Response.Error)
                                    {
                                        logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                        MessageBox.Show(Resources.STR_ErrorLoadingLookup);
                                        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In apiReUploadFailureDocumentsFormPushToGpClickAfterOriginal: " + payableResponse.ErrorMessage.ToString());
                                        MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                    }
                                    else
                                    {
                                        logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                        InitilizeSortBySelection(Resources.STR_ReUploadFailureApiDocuments);
                                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.PushToGp.Lock();
                                        MessageBox.Show(payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                    }
                                }
                                else
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: apiReUploadFailureDocumentsFormPushToGpClickAfterOriginal");
                                    MessageBox.Show("Error: apiReUploadFailureDocumentsFormPushToGpClickAfterOriginal", Resources.STR_MESSAGE_TITLE);
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(Resources.STR_FieldsUpdated, Resources.STR_MESSAGE_TITLE);
                    }

                    apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Release();
                    apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Close();
                }
                else
                {
                    if (!apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value)
                    {
                        reqObj = new PayableManagementRequest();
                        reqObj.PayableDetailsEntity = new PayableDetailsEntity();
                        reqObj.userId = Dynamics.Globals.UserId.Value;
                        reqObj.SourceFormName = Resources.STR_ReUploadFailureApiDocuments;
                        reqObj.FileName = string.Empty;
                        reqObj.Company = Dynamics.Globals.IntercompanyId.Value;
                        reqObj.InvoiceType = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalInvoiceType.Value;

                        if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_ChmptCompany)
                        {
                            reqObj.companyId = Convert.ToInt16(Resources.NACompanyId);
                        }
                        else
                        {
                            reqObj.companyId = Convert.ToInt16(Resources.EUCompanyId);
                        }
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Close();
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Release();
                        TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.GetFirst();

                        //DataSet invoiceDetailsDT = new DataSet("@LineDetails");
                        //invoiceDetailsDT.Tables.Add(new DataTable());
                        //invoiceDetailsDT.Tables[0].Columns.Add("OriginalApiInvoiceId", typeof(int));
                        //invoiceDetailsDT.Tables[0].Columns.Add("DocumentType", typeof(int));
                        //invoiceDetailsDT.Tables[0].Columns.Add("DocumentRowID", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("DocumentNumber", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("DocumentDate", typeof(DateTime));
                        //invoiceDetailsDT.Tables[0].Columns.Add("VendorNumber", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("DocumentAmount", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ApprovedAmount", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("FreightAmount", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("SalesTaxAmount", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("TradeDiscountAmount", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("MiscellaneousAmount", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("PurchasingAmount", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("POAmount", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("PONumber", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("PODateOpened", typeof(DateTime));
                        //invoiceDetailsDT.Tables[0].Columns.Add("POLineNumber", typeof(int));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ReceiptNumber", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ReceiptLineNumber", typeof(int));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ItemNumber", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ItemUnitQty", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ItemUnitPrice", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ItemExtendedAmount", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("AdjustedItemUnitQty", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("AdjustedItemUnitPrice", typeof(decimal));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ShipToState", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ShippedDate", typeof(DateTime));
                        //invoiceDetailsDT.Tables[0].Columns.Add("LocationKey", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("GLIndex", typeof(int));
                        //invoiceDetailsDT.Tables[0].Columns.Add("TaxScheduleId", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("FormTypeCode", typeof(int));
                        //invoiceDetailsDT.Tables[0].Columns.Add("IsDuplicate", typeof(int));
                        //invoiceDetailsDT.Tables[0].Columns.Add("RequiredDistribution", typeof(int));
                        //invoiceDetailsDT.Tables[0].Columns.Add("StatusId", typeof(int));
                        //invoiceDetailsDT.Tables[0].Columns.Add("ErrorDescription", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("CurrencyId", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("UserId", typeof(string));
                        //invoiceDetailsDT.Tables[0].Columns.Add("Notes", typeof(string));

                        while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                        {
                            //DataRow newRow = invoiceDetailsDT.Tables[0].NewRow();
                            //newRow["OriginalApiInvoiceId"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.OriginalApiInvoiceId.Value;
                            //newRow["DocumentType"] = ConvertDocTypeValueByInvoiceType(apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalInvoiceType.Value, apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.DocumentTypeName.Value);
                            //newRow["DocumentRowID"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.DocumentId.Value;
                            //newRow["DocumentNumber"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.DocumentNumber.Value;
                            //newRow["DocumentDate"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ReceiptDate.Value;
                            //newRow["VendorNumber"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.VendorId.Value;
                            //newRow["DocumentAmount"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ApprovedAmount.Value;
                            //newRow["ApprovedAmount"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ApprovedAmount.Value;
                            //newRow["FreightAmount"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.FreightAmount.Value;
                            //newRow["SalesTaxAmount"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.TaxAmount.Value;
                            //newRow["TradeDiscountAmount"] = 0;
                            //newRow["MiscellaneousAmount"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.MiscAmount.Value;
                            //newRow["PurchasingAmount"] = 0;
                            //newRow["POAmount"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.PurchasesAmount.Value;
                            //newRow["PONumber"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.PoNumber.Value;
                            //newRow["PODateOpened"] = DateTime.Today;
                            //newRow["POLineNumber"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.PoLineNumber.Value;
                            //newRow["ReceiptNumber"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.PopReceiptNumber.Value;
                            //newRow["ReceiptLineNumber"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ReceiptLineNumber.Value;
                            //newRow["ItemNumber"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ItemNumber.Value;
                            //newRow["ItemUnitQty"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.QtyShipped.Value;
                            //newRow["ItemUnitPrice"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.UnitCost.Value;
                            //newRow["ItemExtendedAmount"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ExtendedCost.Value;
                            //newRow["AdjustedItemUnitQty"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.QtyToInvoice.Value;
                            //newRow["AdjustedItemUnitPrice"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.AdjustedUnitCost.Value;
                            //newRow["ShipToState"] = "";
                            //newRow["ShippedDate"] = DateTime.Today;
                            //newRow["LocationKey"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.LocationName.Value;
                            //newRow["GLIndex"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.AccountIndex.Value;
                            //newRow["TaxScheduleId"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.TaxScheduleId.Value;  //required
                            //newRow["FormTypeCode"] = reqObj.InvoiceType;
                            //newRow["IsDuplicate"] = 0;
                            //newRow["RequiredDistribution"] = 0;
                            //newRow["StatusId"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Status.Value;
                            //newRow["ErrorDescription"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ErrorDescription.Value;
                            //newRow["CurrencyId"] = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.CurrencyId.Value;
                            //newRow["UserId"] = Dynamics.Globals.UserId.Value.ToString();
                            //newRow["Notes"] = "";
                            //invoiceDetailsDT.Tables[0].Rows.Add(newRow);

                            PayableLineEntity payable = new PayableLineEntity();

                            payable.OriginalApiInvoiceId = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.OriginalApiInvoiceId.Value;
                            payable.DocumentType = ConvertDocTypeValueByInvoiceType(apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalInvoiceType.Value, apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.DocumentTypeName.Value).ToString();
                            payable.DocumentRowId = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.DocumentId.Value;
                            payable.DocumentNumber = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.DocumentNumber.Value;
                            payable.DocumentDate = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ReceiptDate.Value;
                            payable.VendorName = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.VendorId.Value;
                            payable.DocumentAmount = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ApprovedAmount.Value;
                            payable.ApprovedAmount = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ApprovedAmount.Value;
                            payable.FreightAmount = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.FreightAmount.Value;
                            payable.TaxAmount = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.TaxAmount.Value;
                            payable.TradeDiscounts = 0;
                            payable.MiscellaneousAmount = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.MiscAmount.Value;
                            payable.PurchaseAmount = 0;
                            payable.POAmount = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.PurchasesAmount.Value;
                            payable.PurchaseOrderNumber = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.PoNumber.Value;
                            payable.PODateOpened = DateTime.Today;
                            payable.POLineNumber = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.PoLineNumber.Value;
                            payable.ReceiptNumber = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.PopReceiptNumber.Value;
                            payable.ReceiptLineNumber = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ReceiptLineNumber.Value;
                            payable.ItemNumber = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ItemNumber.Value;
                            payable.ItemUnitQty = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.QtyShipped.Value;
                            payable.ItemUnitPrice = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.UnitCost.Value;
                            payable.ExtendedCost = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ExtendedCost.Value;
                            payable.AdjustedItemUnitQuantity = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.QtyToInvoice.Value;
                            payable.AdjustedItemUnitPrice = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.AdjustedUnitCost.Value;
                            payable.ShipToState = "";
                            payable.ShippedDate = DateTime.Today;
                            payable.LocationName = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.LocationName.Value;
                            payable.GLIndex = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.AccountIndex.Value;
                            payable.TaxScheduleId = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.TaxScheduleId.Value;  //required
                            payable.FormTypeCode = reqObj.InvoiceType;
                            payable.IsDuplicated = 0;
                            payable.RequiredDistribution = 0;
                            payable.StatusId = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Status.Value;
                            payable.ErrorDescription = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ErrorDescription.Value;
                            payable.CurrencyId = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.CurrencyId.Value;
                            payable.UserId = Dynamics.Globals.UserId.Value.ToString();
                            payable.Notes = "";

                            payableLine.Add(payable);
                            tableError = apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.GetNext();
                        }

                        //if (invoiceDetailsDT.Tables[0].Rows.Count > 0)
                        if (payableLine != null && payableLine.Count > 0)
                        {
                            #region DistributedDetails


                            //reqObj.ApiTransactionDtValue = ConvertDataTableToString(invoiceDetailsDT.Tables[0]);
                            reqObj.PoValidationList = payableLine;

                            // Service call ...
                            using (HttpClient client = new HttpClient())
                            {
                                client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                var response = client.PostAsJsonAsync("api/PayableManagement/GetApiDuplicateDocumentRowDetails", reqObj);
                                if (response.Result.IsSuccessStatusCode)
                                {
                                    PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                                    if (payableResponse.Status == Response.Error)
                                    {
                                        logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In GetApiDuplicateDocumentRowDetails: " + payableResponse.ErrorMessage.ToString());
                                        MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                    }
                                    else
                                    {
                                        logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                        List<PayableLineEntity> distributionDT = new List<PayableLineEntity>();
                                        distributionDT = ApiDeuplicateRowDocumentDetails(payableResponse);
                                        //reqObj.ApiDistributionDtValue = distributionDT;
                                        reqObj.DuplicationValidationList = distributionDT;
                                        // Service call ...
                                        using (HttpClient clientInside = new HttpClient())
                                        {
                                            clientInside.BaseAddress = new Uri(gpServiceConfigurationUrl);
                                            clientInside.DefaultRequestHeaders.Accept.Clear();
                                            clientInside.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                            var responseToGP = clientInside.PostAsJsonAsync("api/PayableManagement/UploadPayableDetailsIntoGpForApi", reqObj);
                                            if (responseToGP.Result.IsSuccessStatusCode)
                                            {
                                                PayableManagementResponse payableResponseDel = responseToGP.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                                                if (payableResponseDel.Status == Response.Error)
                                                {
                                                    MessageBox.Show(Resources.STR_ErrorLoadingLookup);
                                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In UploadPayableDetailsIntoGpForApi - PushTOGP: " + payableResponse.ErrorMessage.ToString());
                                                    MessageBox.Show("Error: " + payableResponseDel.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                                }
                                                else
                                                {
                                                    InitilizeSortBySelection(Resources.STR_ReUploadFailureApiDocuments);
                                                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.PushToGp.Lock();
                                                    MessageBox.Show(payableResponseDel.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                                }
                                            }
                                            else
                                            {
                                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: UploadPayableDetailsIntoGpForApi - PushTOGP");
                                                MessageBox.Show("Error: UploadPayableDetailsIntoGpForApi - PushTOGP", Resources.STR_MESSAGE_TITLE);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: GetApiDuplicateDocumentRowDetails");
                                    MessageBox.Show("Error: GetApiDuplicateDocumentRowDetails", Resources.STR_MESSAGE_TITLE);
                                }
                            }

                            #endregion
                        }
                    }
                    else
                    {
                        MessageBox.Show(Resources.STR_FieldsUpdated, Resources.STR_MESSAGE_TITLE);
                    }

                    apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Release();
                    apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Close();
                }
            }
            catch (Exception ex)
            {
                if (ex is TimeoutException)
                {
                    MessageBox.Show(Resources.STR_UnknownException, Resources.STR_MESSAGE_TITLE);
                }
                else
                {
                    MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                }
                logMessage.AppendLine(DateTime.Now + " Error: " + ex.Message);
                InitilizeSortBySelection(Resources.STR_ReUploadFailureApiDocuments);
            }
            finally
            {
                if (isApiLoggingEnabled && logMessage.ToString() != string.Empty)
                {
                    logMessage.AppendLine(DateTime.Now + "--------------------------------------------------------------");
                    LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                }
                reqObj = null;
            }

        }
        private DataTable ConvertStringToDataTable(string data)
        {
            DataTable dataTable = new DataTable();
            bool columnsAdded = false;
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
            return dataTable;
        }

        private List<PayableLineEntity> ApiDeuplicateRowDocumentDetails(PayableManagementResponse responseObj)
        {
            //DataTable DistributeDT = new DataTable();
            //if (!string.IsNullOrEmpty(responseObj.ApiDistributionDtValue))
            // DistributeDT = ConvertStringToDataTable(responseObj.ApiDistributionDtValue);
            //string DistributionDT = string.Empty;
            List<PayableLineEntity> payableLine = new List<PayableLineEntity>();

            //if (responseObj != null && responseObj.ApiDistributionDtValue != null && DistributeDT != null && DistributeDT.Rows.Count > 0)
            if (responseObj.DuplicationValidationList != null && responseObj.DuplicationValidationList.Count > 0)
            {
                payableLine = responseObj.DuplicationValidationList;
            }
            else
            {
                //PayableLineEntity payble = new PayableLineEntity();
                //DataTable DistributedDetails = new DataTable("DistributedDetails");
                //DistributedDetails.Columns.Add("OriginalApiInvoiceId", typeof(int));
                //DistributedDetails.Columns.Add("DocumentType", typeof(int));
                //DistributedDetails.Columns.Add("DocumentRowID", typeof(string));
                //DistributedDetails.Columns.Add("DocumentNumber", typeof(string));
                //DistributedDetails.Columns.Add("DocumentDate", typeof(DateTime));
                //DistributedDetails.Columns.Add("VendorNumber", typeof(string));
                //DistributedDetails.Columns.Add("DocumentAmount", typeof(decimal));
                //DistributedDetails.Columns.Add("ApprovedAmount", typeof(decimal));
                //DistributedDetails.Columns.Add("FreightAmount", typeof(decimal));
                //DistributedDetails.Columns.Add("SalesTaxAmount", typeof(decimal));
                //DistributedDetails.Columns.Add("TradeDiscountAmount", typeof(decimal));
                //DistributedDetails.Columns.Add("MiscellaneousAmount", typeof(decimal));
                //DistributedDetails.Columns.Add("PurchasingAmount", typeof(decimal));
                //DistributedDetails.Columns.Add("POAmount", typeof(decimal));
                //DistributedDetails.Columns.Add("PONumber", typeof(string));
                //DistributedDetails.Columns.Add("PODateOpened", typeof(DateTime));
                //DistributedDetails.Columns.Add("POLineNumber", typeof(int));
                //DistributedDetails.Columns.Add("ReceiptNumber", typeof(string));
                //DistributedDetails.Columns.Add("ReceiptLineNumber", typeof(int));
                //DistributedDetails.Columns.Add("ItemNumber", typeof(string));
                //DistributedDetails.Columns.Add("ItemUnitQty", typeof(decimal));
                //DistributedDetails.Columns.Add("ItemUnitPrice", typeof(decimal));
                //DistributedDetails.Columns.Add("ItemExtendedAmount", typeof(decimal));
                //DistributedDetails.Columns.Add("AdjustedItemUnitQty", typeof(decimal));
                //DistributedDetails.Columns.Add("AdjustedItemUnitPrice", typeof(decimal));
                //DistributedDetails.Columns.Add("ShipToState", typeof(string));
                //DistributedDetails.Columns.Add("ShippedDate", typeof(DateTime));
                //DistributedDetails.Columns.Add("LocationKey", typeof(string));
                //DistributedDetails.Columns.Add("GLIndex", typeof(int));
                //DistributedDetails.Columns.Add("TaxScheduleId", typeof(string));
                //DistributedDetails.Columns.Add("FormTypeCode", typeof(int));
                //DistributedDetails.Columns.Add("IsDuplicate", typeof(int));
                //DistributedDetails.Columns.Add("RequiredDistribution", typeof(int));
                //DistributedDetails.Columns.Add("StatusId", typeof(int));
                ////DistributedDetails.Columns.Add("ErrorDescription", typeof(string));
                //DistributedDetails.Columns.Add("CurrencyId", typeof(string));
                ////DistributedDetails.Columns.Add("UserId", typeof(string));
                ////DistributedDetails.Columns.Add("Notes", typeof(string));

                //DistributionDT = ConvertDataTableToString(DistributedDetails);
                payableLine = null;
            }
            return payableLine;
        }
        public static int ConvertDocTypeValueByInvoiceType(int invType, string docTypeName)
        {
            int docType = 1;
            if (invType == 1)
            {
                if (docTypeName == "INV")
                {
                    docType = 1;
                }
                else if (docTypeName == "CRM")
                {
                    docType = 3;
                }
            }
            else
            {
                docType = 2;
            }
            return docType;
        }

        #region Non-PO

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOSelectCheckBoxChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOVoucherNumberEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APINonPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPODocumentDateEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APINonPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOVendorIdEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APINonPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPODocumentAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APINonPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOApprovedAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APINonPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOFreightAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APINonPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOTaxAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APINonPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOPurchasesAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APINonPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOGlAccountNumberEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APINonPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOGlAccountDescriptionEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APINonPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOTradeDiscountAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APINonPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOMiscAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APINonPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOTaxScheduleIdEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APINonPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOLocationNameEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APINonPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOVoucherNumberChange(object sender, EventArgs e)
        {
            string oldVoucherNumber;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldVoucherNumber = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.VoucherNumber.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.VoucherNumber.Value != string.Empty
                            && apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.VoucherNumber.Value != oldVoucherNumber)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.VoucherNumber.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.VoucherNumber.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                    else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.VoucherNumber.Value == string.Empty)
                    {
                        MessageBox.Show(Resources.STR_EmptyDocumentNumber, Resources.STR_MESSAGE_TITLE);
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.VoucherNumber.Value = oldVoucherNumber;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPODocumentDateChange(object sender, EventArgs e)
        {
            DateTime oldDocdate;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldDocdate = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.DocumentDate.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.DocumentDate.Value.ToString() != "01/01/1900"
                            && apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.DocumentDate.Value != oldDocdate)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.DocumentDate.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.DocumentDate.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                    else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.DocumentDate.Value.ToString() == "01/01/1900")
                    {
                        MessageBox.Show(Resources.STR_EmptyDocDate, Resources.STR_MESSAGE_TITLE);
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.DocumentDate.Value = oldDocdate;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOVendorIdChange(object sender, EventArgs e)
        {
            string oldVendorId = "";
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldVendorId = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.VendorId.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.VendorId.Value != string.Empty
                        && apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.VendorId.Value != oldVendorId)
                    {
                        apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Close();
                        apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Release();
                        apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.VendorId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.VendorId.Value;
                        apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Key = 1;
                        TableError Vendorerror = apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Get();
                        if (Vendorerror == TableError.NotFound)
                        {
                            // if not valid displays a message
                            MessageBox.Show(Resources.STR_VendorIdDoesNotExists, Resources.STR_MESSAGE_TITLE);
                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.VendorId.Value = oldVendorId;
                        }
                        else
                        {
                            apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.VendorId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.VendorId.Value;
                            apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Save();
                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value = true;
                        }
                        apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Close();
                        apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Release();
                    }
                    else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.VendorId.Value == string.Empty)
                    {
                        MessageBox.Show(Resources.STR_EmptyVendorId, Resources.STR_MESSAGE_TITLE);
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.VendorId.Value = oldVendorId;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPODocumentAmountChange(object sender, EventArgs e)
        {
            decimal oldDocumentAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldDocumentAmount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.DocumentAmount.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.DocumentAmount.Value != 0
                            && apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.DocumentAmount.Value != oldDocumentAmount)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.DocumentAmount.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.DocumentAmount.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                    else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.DocumentAmount.Value == 0)
                    {
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.DocumentAmount.Value = oldDocumentAmount;
                        MessageBox.Show(Resources.STR_ZeroDocumentAmount, Resources.STR_MESSAGE_TITLE);
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOApprovedAmountChange(object sender, EventArgs e)
        {

            decimal oldApprovedAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldApprovedAmount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.ApprovedAmount.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.ApprovedAmount.Value != 0
                            && apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.ApprovedAmount.Value != oldApprovedAmount)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.ApprovedAmount.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.ApprovedAmount.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                    else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.ApprovedAmount.Value == 0)
                    {
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.ApprovedAmount.Value = oldApprovedAmount;
                        MessageBox.Show(Resources.STR_ZeroApprovedAmount, Resources.STR_MESSAGE_TITLE);
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOFreightAmountChange(object sender, EventArgs e)
        {
            decimal oldFreightAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldFreightAmount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.FreightAmount.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.FreightAmount.Value != oldFreightAmount)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.FreightAmount.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.FreightAmount.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOTaxAmountChange(object sender, EventArgs e)
        {
            decimal oldTaxAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldTaxAmount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.TaxAmount.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.TaxAmount.Value != oldTaxAmount)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.TaxAmount.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.TaxAmount.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOPurchasesAmountChange(object sender, EventArgs e)
        {
            decimal oldPurchaseAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldPurchaseAmount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.PurchasesAmount.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.PurchasesAmount.Value != oldPurchaseAmount)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.PurchasesAmount.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.PurchasesAmount.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOGlAccountNumberChange(object sender, EventArgs e)
        {
            string oldGlAccount;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldGlAccount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.GlAccountNumber.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountNumber.Value != string.Empty
                        && apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountNumber.Value != oldGlAccount
                        && apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountNumber.Value.ToLower().Trim() != "po-auto"
                        && apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountNumber.Value.ToLower().Trim() != "vat-auto")
                    {
                        apiReUploadFailureDocumentsForm.Tables.GlAccountIndexMstr.Close();
                        apiReUploadFailureDocumentsForm.Tables.GlAccountIndexMstr.Release();
                        apiReUploadFailureDocumentsForm.Tables.GlAccountIndexMstr.AccountNumberString.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountNumber.Value;
                        apiReUploadFailureDocumentsForm.Tables.GlAccountIndexMstr.Key = 3;
                        TableError glIndexError = apiReUploadFailureDocumentsForm.Tables.GlAccountIndexMstr.Get();
                        if (glIndexError == TableError.NotFound)
                        {
                            // if not valid displays a message
                            MessageBox.Show(Resources.STR_GLAccountDoesNotExists, Resources.STR_MESSAGE_TITLE);
                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountNumber.Value = oldGlAccount;
                        }
                        else
                        {
                            apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.GlAccountNumber.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountNumber.Value;
                            apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.AccountIndex.Value = apiReUploadFailureDocumentsForm.Tables.GlAccountIndexMstr.AccountIndex.Value;

                            apiReUploadFailureDocumentsForm.Tables.GlAccountMstr.Close();
                            apiReUploadFailureDocumentsForm.Tables.GlAccountMstr.Release();
                            apiReUploadFailureDocumentsForm.Tables.GlAccountMstr.AccountIndex.Value = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.AccountIndex.Value;
                            apiReUploadFailureDocumentsForm.Tables.GlAccountMstr.Key = 1;
                            TableError glMasterError = apiReUploadFailureDocumentsForm.Tables.GlAccountMstr.Get();
                            if (glMasterError == TableError.NoError)
                            {
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.GlAccountDescription.Value = apiReUploadFailureDocumentsForm.Tables.GlAccountMstr.AccountDescription.Value;
                                apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.AccountIndex.Value = apiReUploadFailureDocumentsForm.Tables.GlAccountMstr.AccountIndex.Value;
                                apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountDescription.Value = apiReUploadFailureDocumentsForm.Tables.GlAccountMstr.AccountDescription.Value;

                            }
                            apiReUploadFailureDocumentsForm.Tables.GlAccountMstr.Close();
                            apiReUploadFailureDocumentsForm.Tables.GlAccountMstr.Release();

                            apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Save();
                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value = true;
                        }
                        apiReUploadFailureDocumentsForm.Tables.GlAccountIndexMstr.Close();
                        apiReUploadFailureDocumentsForm.Tables.GlAccountIndexMstr.Release();
                    }
                    else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountNumber.Value == string.Empty)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.GlAccountNumber.Value = "PO-Auto";
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.AccountIndex.Value = 0;
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.GlAccountDescription.Value = "";

                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value = true;

                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.AccountIndex.Value = 0;
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountDescription.Value = "";
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountNumber.Value = "PO-Auto";
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOGlAccountDescriptionChange(object sender, EventArgs e)
        {
            string oldGlAccountDesc;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldGlAccountDesc = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.GlAccountDescription.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountDescription.Value != string.Empty
                            && apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountDescription.Value != oldGlAccountDesc)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.GlAccountDescription.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountDescription.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                    else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountDescription.Value == string.Empty)
                    {
                        MessageBox.Show(Resources.STR_GLAccountDescription, Resources.STR_MESSAGE_TITLE);
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.GlAccountDescription.Value = oldGlAccountDesc;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOTradeDiscountAmountChange(object sender, EventArgs e)
        {
            decimal oldTradeDiscAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldTradeDiscAmount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.TradeDiscountAmount.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.TradeDiscountAmount.Value != oldTradeDiscAmount)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.TradeDiscountAmount.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.TradeDiscountAmount.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOMiscAmountChange(object sender, EventArgs e)
        {
            decimal oldMiscAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldMiscAmount = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.MiscAmount.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.MiscAmount.Value != oldMiscAmount)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.MiscAmount.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.MiscAmount.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOTaxScheduleIdChange(object sender, EventArgs e)
        {
            string oldTaxScheduleId = "";
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldTaxScheduleId = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.TaxScheduleId.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.TaxScheduleId.Value != oldTaxScheduleId)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.TaxScheduleId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.TaxScheduleId.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormNonPOLocationNameChange(object sender, EventArgs e)
        {
            string oldLocationName = "";
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldLocationName = apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.LocationName.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.LocationName.Value != oldLocationName)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.LocationName.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.LocationName.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag.Value = true;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        #endregion

        #region PO

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOSelectCheckBoxChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPODocumentNumberEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOReceiptDateEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOVendorIdEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPODocumentAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOApprovedAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOFreightAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOTaxAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOMiscAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOPurchasesAmountEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOPoNumberEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOPoLineNumberEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOItemNumberEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOPopReceiptNumberEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOReceiptLineNumberEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOQtyShippedEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOAdjustedUnitsEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOUnitCostEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOExtendedCostEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOAdjustedUnitCostEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOLocationNameEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RestrictUpdatingApiFields(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOReUploadDocumentsSelectScroll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPODocumentNumberChange(object sender, EventArgs e)
        {
            string oldDocumentNumber;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldDocumentNumber = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.DocumentNumber.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.DocumentNumber.Value != string.Empty
                            && apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.DocumentNumber.Value != oldDocumentNumber)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.DocumentNumber.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.DocumentNumber.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
                    }
                    else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.DocumentNumber.Value == string.Empty)
                    {
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.DocumentNumber.Value = oldDocumentNumber;
                        MessageBox.Show(Resources.STR_EmptyDocumentNumber, Resources.STR_MESSAGE_TITLE);
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOReceiptDateChange(object sender, EventArgs e)
        {
            DateTime oldReceiptDate;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldReceiptDate = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ReceiptDate.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ReceiptDate.Value.ToString() != "01/01/1900"
                            && apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ReceiptDate.Value != oldReceiptDate)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ReceiptDate.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ReceiptDate.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
                    }
                    else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ReceiptDate.Value.ToString() == "01/01/1900")
                    {
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ReceiptDate.Value = oldReceiptDate;
                        MessageBox.Show(Resources.STR_EmptyDocDate, Resources.STR_MESSAGE_TITLE);
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOVendorIdChange(object sender, EventArgs e)
        {
            string oldVendorId = "";
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldVendorId = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.VendorId.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.VendorId.Value != string.Empty
                        && apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.VendorId.Value != oldVendorId)
                    {
                        apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Close();
                        apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Release();
                        apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.VendorId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.VendorId.Value;
                        apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Key = 1;
                        TableError Vendorerror = apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Get();
                        if (tableError == TableError.NotFound)
                        {
                            // if not valid displays a message
                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.VendorId.Value = oldVendorId;
                            MessageBox.Show(Resources.STR_VendorIdDoesNotExists, Resources.STR_MESSAGE_TITLE);
                        }
                        else
                        {
                            apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.VendorId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.VendorId.Value;
                            apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;

                        }
                        apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Close();
                        apiReUploadFailureDocumentsForm.Tables.PmVendorMstr.Release();
                    }
                    else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.VendorId.Value == string.Empty)
                    {
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.VendorId.Value = oldVendorId;
                        MessageBox.Show(Resources.STR_EmptyVendorId, Resources.STR_MESSAGE_TITLE);
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPODocumentAmountChange(object sender, EventArgs e)
        {
            decimal oldDocumentAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldDocumentAmount = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.DocumentAmount.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.DocumentAmount.Value != 0
                            && apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.DocumentAmount.Value != oldDocumentAmount)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.DocumentAmount.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.DocumentAmount.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
                    }
                    else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.DocumentAmount.Value == 0)
                    {
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.DocumentAmount.Value = oldDocumentAmount;
                        MessageBox.Show(Resources.STR_ZeroDocumentAmount, Resources.STR_MESSAGE_TITLE);
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOApprovedAmountChange(object sender, EventArgs e)
        {

            decimal oldApprovedAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldApprovedAmount = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ApprovedAmount.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ApprovedAmount.Value != 0
                            && apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ApprovedAmount.Value != oldApprovedAmount)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ApprovedAmount.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ApprovedAmount.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
                    }
                    else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ApprovedAmount.Value == 0)
                    {
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ApprovedAmount.Value = oldApprovedAmount;
                        MessageBox.Show(Resources.STR_ZeroApprovedAmount, Resources.STR_MESSAGE_TITLE);
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOFreightAmountChange(object sender, EventArgs e)
        {
            decimal oldFreightAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldFreightAmount = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.FreightAmount.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.FreightAmount.Value != oldFreightAmount)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.FreightAmount.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.FreightAmount.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOTaxAmountChange(object sender, EventArgs e)
        {
            decimal oldTaxAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldTaxAmount = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.TaxAmount.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.TaxAmount.Value != oldTaxAmount)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.TaxAmount.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.TaxAmount.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOMiscAmountChange(object sender, EventArgs e)
        {
            decimal oldMiscAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldMiscAmount = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.MiscAmount.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.MiscAmount.Value != oldMiscAmount)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.MiscAmount.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.MiscAmount.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOPurchasesAmountChange(object sender, EventArgs e)
        {
            decimal oldPurchaseAmount = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldPurchaseAmount = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.PurchasesAmount.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.PurchasesAmount.Value != oldPurchaseAmount)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.PurchasesAmount.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.PurchasesAmount.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOPoNumberChange(object sender, EventArgs e)
        {
            string oldPoNumber;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldPoNumber = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.PoNumber.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.PoNumber.Value != oldPoNumber)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.PoNumber.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.PoNumber.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOPoLineNumberChange(object sender, EventArgs e)
        {
            int oldPoLineNumber;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldPoLineNumber = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.PoLineNumber.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.PoLineNumber.Value != 0 &&
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.PoLineNumber.Value != oldPoLineNumber)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.PoLineNumber.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.PoLineNumber.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
                    }
                    //else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.PoLineNumber.Value == 0)
                    //{
                    //    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.PoLineNumber.Value = oldPoLineNumber;
                    //    MessageBox.Show(Resources.STR_POLineNumber, Resources.STR_MESSAGE_TITLE);
                    //}
                }
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOItemNumberChange(object sender, EventArgs e)
        {
            string oldItemNumber;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldItemNumber = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ItemNumber.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ItemNumber.Value != oldItemNumber)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ItemNumber.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ItemNumber.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOPopReceiptNumberChange(object sender, EventArgs e)
        {
            string oldPopReceiptNumber;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldPopReceiptNumber = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.PopReceiptNumber.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.PopReceiptNumber.Value != oldPopReceiptNumber)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.PopReceiptNumber.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.PopReceiptNumber.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOReceiptLineNumberChange(object sender, EventArgs e)
        {
            int oldReceiptLineNumber;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldReceiptLineNumber = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ReceiptLineNumber.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ReceiptLineNumber.Value != 0 &&
                        //apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ReceiptLineNumber.Value >= 16384 &&
                        //apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ReceiptLineNumber.Value % 16384 == 0 &&
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ReceiptLineNumber.Value != oldReceiptLineNumber)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ReceiptLineNumber.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ReceiptLineNumber.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
                    }
                    //else if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ReceiptLineNumber.Value == 0)
                    //{
                    //    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ReceiptLineNumber.Value = oldReceiptLineNumber;
                    //    MessageBox.Show(Resources.STR_ReceiptLineNumber, Resources.STR_MESSAGE_TITLE);
                    //}
                }
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOQtyShippedChange(object sender, EventArgs e)
        {
            decimal oldQtyShipped = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldQtyShipped = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.QtyShipped.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.QtyShipped.Value != oldQtyShipped)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.QtyShipped.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.QtyShipped.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOAdjustedUnitsChange(object sender, EventArgs e)
        {
            decimal oldAdjustedUnits = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldAdjustedUnits = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.QtyToInvoice.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.QtyToInvoice.Value != oldAdjustedUnits)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.QtyToInvoice.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.QtyToInvoice.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOUnitCostChange(object sender, EventArgs e)
        {
            decimal oldUnitCost = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldUnitCost = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.UnitCost.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.UnitCost.Value != oldUnitCost)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.UnitCost.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.UnitCost.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOExtendedCostChange(object sender, EventArgs e)
        {
            decimal oldExtendedCost = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldExtendedCost = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ExtendedCost.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ExtendedCost.Value != oldExtendedCost)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ExtendedCost.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.ExtendedCost.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOAdjustedUnitCostChange(object sender, EventArgs e)
        {
            decimal oldAdjustedUnitCost = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldAdjustedUnitCost = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.AdjustedUnitCost.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.AdjustedUnitCost.Value != oldAdjustedUnitCost)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.AdjustedUnitCost.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.AdjustedUnitCost.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiReUploadFailureDocumentsFormPOLocationNameChange(object sender, EventArgs e)
        {
            string oldLocationName = "";
            StringBuilder logMessage = new StringBuilder();
            try
            {
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.OriginalApiInvoiceId.Value;
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Key = 1;
                TableError tableError = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    oldLocationName = apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.LocationName.Value;
                    if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.LocationName.Value != oldLocationName)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.LocationName.Value = apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.LocationName.Value;
                        apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ChangeFlag2.Value = true;
                    }
                }
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        #endregion

        #endregion

        #region APILinkManualPayments
        void apiDocumentsWithManualPaymentsForm_OpenBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (!RegisterAPILinkedManualDocuments)
                {
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalInvoiceType.Change += new EventHandler(ManualPaymentsLocalInvoiceType_Change);
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalDocumentBy.Change += new EventHandler(apiDocumentsWithManualLocalDocumentBy_Change);
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LookupButton1.ClickAfterOriginal += new EventHandler(apiDocumentsWithManualLookupButton1_ClickAfterOriginal);
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LookupButton2.ClickAfterOriginal += new EventHandler(apiDocumentsWithManualLookupButton2_ClickAfterOriginal);
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromDocId.Change += new EventHandler(apiDocumentsWithManualLocalFromDocId_Change);
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToDocId.Change += new EventHandler(apiDocumentsWithManualLocalToDocId_Change);
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromVendorId.Change += new EventHandler(apiDocumentsWithManualLocalFromVendorId_Change);
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToVendorId.Change += new EventHandler(apiDocumentsWithManualLocalToVendorId_Change);
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.CloseAfterOriginal += new EventHandler(ApiDocumentsWithManualPayments_CloseAfterOriginal);
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.AllOrRange.Change += new EventHandler(apiDocumentsWithManualAllOrRange_Change);
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApiNonPoManualPaymentScroll.SelectCheckBox.Change += new EventHandler(ApiNonPoManualPaymentScrollSelectCheckBox_Change);
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApipoDocumentsWithManualPaymentsScroll.SelectCheckBox.Change += new EventHandler(ApipoDocumentsWithManualPaymentsScrollSelectCheckBox_Change);
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApiNonPoManualPaymentScroll.ManualPaymentNumber.Change += new EventHandler(ApiNonPoManualPaymentScrollManualPaymentNumber_Change);
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApipoDocumentsWithManualPaymentsScroll.ManualPaymentNumber.Change += new EventHandler(ApiPoManualPaymentScrollManualPaymentNumber_Change);
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.RedisplayButton.ClickAfterOriginal += new EventHandler(apiDocumentsWithManualRedisplayButton_ClickAfterOriginal);
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApiNonPoManualPaymentScroll.ManualPaymentNumber.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ApiNonPoManualPaymentScrollManualPaymentNumber_EnterBeforeOriginal);
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApipoDocumentsWithManualPaymentsScroll.ManualPaymentNumber.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ApipoDocumentsWithManualPaymentsScrollManualPaymentNumber_EnterBeforeOriginal);
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.OkButton.ClickAfterOriginal += new EventHandler(ApiDocumentsWithManualOkButton_ClickAfterOriginal);
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.UpdateAsManuallyCreated.ClickAfterOriginal += new EventHandler(apiDocumentsWithManualUpdateAsManuallyCreated_ClickAfterOriginal);
                    RegisterAPILinkedManualDocuments = true;

                }
                apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalInvoiceType.Value = 2;
                apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalInvoiceType.RunValidate();
                apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalDocumentBy.Value = 1;
                documentType = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalInvoiceType.Value;
                //ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.ChangeFlag.Value = false;
                InitilizeSortBySelection(Resources.STR_APIDocumentsWithManualPayments);
                apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.UpdateAsManuallyCreated.Lock();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
        }
        void ManualPaymentsLocalInvoiceType_Change(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {
                documentType = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalInvoiceType.Value;
                apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromVendorId.Clear();
                apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToVendorId.Clear();
                apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromDocId.Clear();
                apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToDocId.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;

            }
        }
        /// <summary>
        /// When the redisplay button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiIdLookupFormRedisplayButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            PayableManagementRequest requestObj = null;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                requestObj = new PayableManagementRequest();
                requestObj.userId = Dynamics.Globals.UserId.Value.ToString();
                requestObj.InvoiceType = documentType;
                requestObj.companyId = Dynamics.Globals.CompanyId.Value;

                // Service call ...
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.PostAsJsonAsync("api/PayableManagement/GetFailedAPIIdsList", requestObj);
                    if (response.Result.IsSuccessStatusCode)
                    {
                        PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                        if (payableResponse.Status == Response.Error)
                        {
                            logMessage.AppendLine(payableResponse.LogMessage.ToString());
                            MessageBox.Show(Resources.STR_ErrorLoadingLookup);
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In apiIdLookupFormRedisplayButton_ClickAfterOriginal: " + payableResponse.ErrorMessage.ToString());
                            MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                        }
                        else
                        {
                            logMessage.AppendLine(payableResponse.LogMessage.ToString());
                            //if the line details exists then fill scroll
                            if (payableResponse.LookupDetails != null && payableResponse.LookupDetails.GetLookupDetails.Count > 0)
                            {
                                for (int i = 0; i < payableResponse.LookupDetails.GetLookupDetails.Count; i++)
                                {
                                    apiIdLookupForm.Tables.ApiIdTemp.Close();
                                    apiIdLookupForm.Tables.ApiIdTemp.Release();
                                    apiIdLookupForm.Tables.ApiIdTemp.DocumentId.Value = payableResponse.LookupDetails.GetLookupDetails[i].ApiId;
                                    apiIdLookupForm.Tables.ApiIdTemp.Save();
                                }

                                apiIdLookupForm.Tables.ApiIdTemp.Close();
                                apiIdLookupForm.Tables.ApiIdTemp.Release();
                                //Fill details to scrolling window.Call dex procedure to fill the scrolling window.
                                apiIdLookupForm.Procedures.ApiIdLookupFormScrollFill.Invoke();
                            }
                        }
                    }
                    else
                    {
                        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: apiIdLookupFormRedisplayButton_ClickAfterOriginal");
                        MessageBox.Show("Error: apiIdLookupFormRedisplayButton_ClickAfterOriginal", Resources.STR_MESSAGE_TITLE);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
        }
        /// <summary>
        /// When the cancel button is clicked in the lookup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiIdLookupFormCancelButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                apiIdLookupForm.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
        }

        /// <summary>
        /// Event method called to close the ctsi lookup window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiIdLookupCloseAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                lookupWindowName = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
        }

        /// <summary>
        /// Event method called to close the ctsi lookup window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void vendorLookupCloseAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                lookupWindowName = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
        }
        /// <summary>
        /// When the update manually created button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiDocumentsWithManualUpdateAsManuallyCreated_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            logMessage.AppendLine(DateTime.Now + "   Started : Updating manual payment number: "
                   + " by user : " + Dynamics.Globals.UserId.Value.ToString()
                   + " in company : " + Dynamics.Globals.IntercompanyId.Value.ToString());
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                PayableManagementRequest reqObj = this.AssignApiGridValuetoRequestObject(apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalInvoiceType.Value);
                reqObj.userId = Dynamics.Globals.UserId.Value.ToString();
                logMessage.AppendLine("Compnay Id: " + Dynamics.Globals.CompanyId.Value);
                if (Dynamics.Globals.IntercompanyId.Value == Resources.STR_ChmptCompany)
                {
                    reqObj.companyId = Convert.ToInt16(Resources.NACompanyId);
                }
                else
                {
                    reqObj.companyId = Convert.ToInt16(Resources.EUCompanyId);
                }
                if (reqObj.PayableDetailsEntity.GetInvoiceLineDetails.Count > 0)
                {
                    //PayableDetails payableDetails = new PayableDetails();

                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/PayableManagement/UpdateManualPaymentNumberForAPIDocuments", reqObj);
                        if (response.Result.IsSuccessStatusCode)
                        {
                            PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                            if (payableResponse.Status == Response.Error && payableResponse.IsValidStatus == 1)
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                logMessage.AppendLine(DateTime.Now + "One or more selected documents does not contain manual payment number. ");
                                MessageBox.Show(Resources.STR_EmptyManualPaymentNumber, Resources.STR_MESSAGE_TITLE);
                            }
                            else if (payableResponse.Status == Response.Error && payableResponse.IsValidStatus == 2)
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                logMessage.AppendLine(DateTime.Now + " One or more documents have already been created by some other user.");
                                MessageBox.Show(Resources.STR_DocumentAlreadyCreated, Resources.STR_MESSAGE_TITLE);
                                apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.RedisplayButton.RunValidate();
                            }
                            else
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                MessageBox.Show(Resources.STR_ApiManualPaymentUpdateSuccess, Resources.STR_MESSAGE_TITLE);
                                logMessage.AppendLine(DateTime.Now + " Successfully linked the manual payment numbers to failed api Id's.");
                                InitilizeSortBySelection(Resources.STR_APIDocumentsWithManualPayments);
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Expense Report Upload PushToGp ClickAfterOriginal");
                            MessageBox.Show("Error: Expense Report Upload PushToGp ClickAfterOriginal", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(Resources.STR_SelectAtLeastOneApiDoc, Resources.STR_MESSAGE_TITLE);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the API TO document id changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiDocumentsWithManualLocalToDocId_Change(object sender, EventArgs e)
        {
            PayableManagementRequest requestObj = null;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToDocId.Value != string.Empty)
                {
                    requestObj = new PayableManagementRequest();
                    requestObj.SourceLookup = Resources.STR_APIId;
                    requestObj.LookupValue = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToDocId.Value.ToString();
                    requestObj.userId = Dynamics.Globals.UserId.Value.ToString();
                    requestObj.companyId = Dynamics.Globals.CompanyId.Value;

                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/PayableManagement/VerifyApiLookupValueExists", requestObj);
                        if (response.Result.IsSuccessStatusCode)
                        {
                            PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                            if (payableResponse.Status == Response.Error)
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In apiDocumentsWithManualLocalToDocId_Change: " + payableResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                if (!payableResponse.LookupValueExists)
                                {
                                    MessageBox.Show(Resources.STR_APIIdDoesNotExists, Resources.STR_MESSAGE_TITLE);
                                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToDocId.Value = string.Empty;
                                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToDocId.Focus();

                                }
                                else
                                {
                                    if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToDocId.Value == string.Empty)
                                    {
                                        if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromDocId.Value == string.Empty)
                                        {
                                            apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.AllOrRange.Value = 0;
                                        }
                                    }
                                    else
                                    {
                                        apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.AllOrRange.Value = 1;
                                    }
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: apiDocumentsWithManualLocalToDocId_Change");
                            MessageBox.Show("Error: apiDocumentsWithManualLocalToDocId_Change", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the API FROM document id from changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiDocumentsWithManualLocalFromDocId_Change(object sender, EventArgs e)
        {
            PayableManagementRequest requestObj = null;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromDocId.Value != string.Empty)
                {
                    requestObj = new PayableManagementRequest();
                    requestObj.SourceLookup = Resources.STR_APIId;
                    requestObj.LookupValue = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromDocId.Value;
                    requestObj.userId = Dynamics.Globals.UserId.Value.ToString();
                    requestObj.companyId = Dynamics.Globals.CompanyId.Value;

                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/PayableManagement/VerifyApiLookupValueExists", requestObj);
                        if (response.Result.IsSuccessStatusCode)
                        {
                            PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                            if (payableResponse.Status == Response.Error)
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In apiDocumentsWithManualLocalFromDocId_Change: " + payableResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                if (!payableResponse.LookupValueExists)
                                {
                                    MessageBox.Show(Resources.STR_APIIdDoesNotExists, Resources.STR_MESSAGE_TITLE);
                                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromDocId.Clear();
                                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromDocId.Focus();
                                }
                                else
                                {
                                    if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromDocId.Value == string.Empty)
                                    {
                                        if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToDocId.Value == string.Empty)
                                        {
                                            apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.AllOrRange.Value = 0;
                                        }
                                    }
                                    else
                                    {
                                        apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.AllOrRange.Value = 1;
                                        if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToDocId.Value == string.Empty)
                                        {
                                            apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToDocId.Value = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromDocId.Value;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: apiDocumentsWithManualLocalFromDocId_Change");
                            MessageBox.Show("Error: apiDocumentsWithManualLocalFromDocId_Change", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// When ok button of the manual payment is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ApiDocumentsWithManualOkButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            logMessage.AppendLine(DateTime.Now + " Completed : Closing manual payments:  "
                      + " by user : " + Dynamics.Globals.UserId.Value.ToString()
                      + " in company : " + Dynamics.Globals.IntercompanyId.Value.ToString());
            try
            {
                apiDocumentsWithManualPaymentsForm.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the manual payment number is focussed in PO scroll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ApipoDocumentsWithManualPaymentsScrollManualPaymentNumber_EnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApipoDocumentsWithManualPaymentsScroll.SelectCheckBox.Value == false)
                {
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApipoDocumentsWithManualPaymentsScroll.SelectCheckBox.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// Wehn the focus enters manual payment number of Non PO scroll
        /// </summary>
        void ApiNonPoManualPaymentScrollManualPaymentNumber_EnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApiNonPoManualPaymentScroll.SelectCheckBox.Value == false)
                {
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApiNonPoManualPaymentScroll.SelectCheckBox.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// Method to dsiplay the details to scrollign window based on conditions and type.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiDocumentsWithManualRedisplayButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            PayableManagementRequest reqObj = null;
            StringBuilder logMessage = new StringBuilder();
            bool isValid = false;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                logMessage.AppendLine(DateTime.Now + " Started : Filling data to manual payments: "
                    + " by user : " + Dynamics.Globals.UserId.Value.ToString()
                    + " in company : " + Dynamics.Globals.IntercompanyId.Value.ToString());
                reqObj = new PayableManagementRequest();
                if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalInvoiceType == 1) //non po invoice
                    reqObj.InvoiceType = 1;
                else if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalInvoiceType == 2)// po
                    reqObj.InvoiceType = 2;

                if ((apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalDocumentBy.Value == 1)
                       && (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.AllOrRange.Value == 1)
                         && (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromDocId.Value != string.Empty)
                               && (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToDocId.Value != string.Empty))
                {
                    reqObj.SearchType = Resources.STR_SearchType_ApiInvoiceId;
                    reqObj.FromApiId = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromDocId.Value;
                    reqObj.ToApiId = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToDocId.Value;
                    reqObj.FromVendorId = string.Empty;
                    reqObj.ToVendorId = string.Empty;
                    isValid = true;
                }
                else if ((apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalDocumentBy.Value == 2)
                       && (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.AllOrRange.Value == 1)
                         && (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromVendorId.Value != string.Empty)
                               && (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToVendorId.Value != string.Empty))
                {
                    reqObj.SearchType = Resources.STR_SearchType_VendorId; ;
                    reqObj.FromVendorId = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromVendorId.Value;
                    reqObj.ToVendorId = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToVendorId.Value;
                    reqObj.FromApiId = string.Empty;
                    reqObj.ToApiId = string.Empty;
                    isValid = true;
                }
                else if (((apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalDocumentBy.Value == 1)
                                || (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalDocumentBy.Value == 2))
                                    && (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.AllOrRange.Value == 0))
                {
                    reqObj.SearchType = Resources.STR_SearchType_All;
                    reqObj.FromVendorId = string.Empty;
                    reqObj.ToVendorId = string.Empty;
                    reqObj.FromApiId = string.Empty;
                    reqObj.ToApiId = string.Empty;
                    isValid = true;

                }
                else
                {
                    MessageBox.Show(Resources.STR_RequiredFieldsMissing, Resources.STR_MESSAGE_TITLE);
                }
                if (isValid)
                {
                    if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalInvoiceType == 1)// Non PO invoice
                    {
                        reqObj.userId = Dynamics.Globals.UserId.Value.ToString();
                        reqObj.companyId = Dynamics.Globals.CompanyId.Value;

                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/PayableManagement/GetFailedApiIdsToLinkManualPayments", reqObj);
                            if (response.Result.IsSuccessStatusCode)
                            {
                                PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                                if (payableResponse.Status == Response.Error)
                                {
                                    logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                    logMessage.AppendLine(DateTime.Now + " Error in fetching records to populate in Link failed Non PO API to manual payments window.");
                                    MessageBox.Show(Resources.STR_ErrorPopulatingFailedApiDocuments);
                                }
                                else
                                {
                                    logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                    if (payableResponse != null)
                                    {
                                        logMessage.AppendLine(DateTime.Now + " Started loading the failed API details to scrolling window.");
                                        LinkFailedApiRecordsScrollContents(payableResponse, "NonPOInvoice");
                                    }
                                }
                            }
                            else
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: apiDocumentsWithManualRedisplayButton_ClickAfterOriginal-NonPO");
                                MessageBox.Show("Error: apiDocumentsWithManualRedisplayButton_ClickAfterOriginal-NonPO", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                    else if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalInvoiceType == 2)//  PO invoice
                    {
                        reqObj.userId = Dynamics.Globals.UserId.Value.ToString();
                        reqObj.companyId = Dynamics.Globals.CompanyId.Value;

                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/PayableManagement/GetFailedApiIdsToLinkManualPayments", reqObj);
                            if (response.Result.IsSuccessStatusCode)
                            {
                                PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                                if (payableResponse.Status == Response.Error)
                                {
                                    logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                    logMessage.AppendLine(DateTime.Now + " Error in fetching records to populate in Link failed PO API to manual payments window.");
                                    MessageBox.Show(Resources.STR_ErrorPopulatingFailedApiDocuments);
                                }
                                else
                                {
                                    logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                    if (payableResponse != null)
                                    {
                                        logMessage.AppendLine(DateTime.Now + " Started loading the failed API details to scrolling window.");
                                        LinkFailedApiRecordsScrollContents(payableResponse, "POInvoice");
                                    }
                                }
                            }
                            else
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: apiDocumentsWithManualRedisplayButton_ClickAfterOriginal-PO");
                                MessageBox.Show("Error: apiDocumentsWithManualRedisplayButton_ClickAfterOriginal-PO", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
                reqObj = null;
            }

        }
        /// <summary>
        /// When the manual invoice number is changed for PO SCroll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ApiPoManualPaymentScrollManualPaymentNumber_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            PayableManagementRequest requestObj = null;
            logMessage.AppendLine(DateTime.Now + " Validate manual payment number already exists or is processed by some other user:  "
                        + " by user : " + Dynamics.Globals.UserId.Value.ToString()
                        + " in company : " + Dynamics.Globals.IntercompanyId.Value.ToString());
            try
            {

                apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Close();
                apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Release();
                apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.DocumentId.Value = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApipoDocumentsWithManualPaymentsScroll.DocumentId.Value;
                // apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.ManualPaymentNumber.Value = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApipoDocumentsWithManualPaymentsScroll.ManualPaymentNumber.Value;
                apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Key = 1;
                TableError tableError = apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Change();
                if (tableError == TableError.NoError
                            && tableError != TableError.EndOfTable)
                {
                    if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApipoDocumentsWithManualPaymentsScroll.ManualPaymentNumber.Value != string.Empty)
                    {

                        requestObj = new PayableManagementRequest();
                        requestObj.ManualPaymentNumber = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApipoDocumentsWithManualPaymentsScroll.ManualPaymentNumber.Value;
                        requestObj.FromVendorId = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApipoDocumentsWithManualPaymentsScroll.VendorId.Value;
                        requestObj.companyId = Dynamics.Globals.CompanyId.Value;

                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/PayableManagement/ValidatePODocumentExistsForVendor", requestObj);
                            if (response.Result.IsSuccessStatusCode)
                            {
                                PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                                if (payableResponse.Status == Response.Error)
                                {
                                    logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                    MessageBox.Show(Resources.STR_ErrorLoadingLookup);
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxDetailMaintenance_DeleteButtonClickAfterOriginal Method (DeleteTaxDetailCustomRecord): " + payableResponse.ErrorMessage.ToString());
                                    MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                    if (!payableResponse.IsValid)
                                    {
                                        MessageBox.Show(Resources.STR_POInvoiceDoesNotExists, Resources.STR_MESSAGE_TITLE);
                                        apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApipoDocumentsWithManualPaymentsScroll.ManualPaymentNumber.Clear();
                                        apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApipoDocumentsWithManualPaymentsScroll.ManualPaymentNumber.Focus();
                                    }
                                    else
                                    {
                                        apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.ManualPaymentNumber.Value = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApipoDocumentsWithManualPaymentsScroll.ManualPaymentNumber.Value;
                                        apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Save();
                                        // ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.ChangeFlag.Value = true;
                                    }
                                }
                            }
                            else
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Expense Report Upload PushToGp ClickAfterOriginal");
                                MessageBox.Show("Error: Expense Report Upload PushToGp ClickAfterOriginal", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the Manual payment number change in NON PO scroll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ApiNonPoManualPaymentScrollManualPaymentNumber_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            PayableManagementRequest requestObj = null;
            logMessage.AppendLine(DateTime.Now + " Validate manual payment number already exists or is processed by some other user:  "
                        + " by user : " + Dynamics.Globals.UserId.Value.ToString()
                        + " in company : " + Dynamics.Globals.IntercompanyId.Value.ToString());
            try
            {

                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Close();
                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Release();
                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.DocumentId.Value = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApiNonPoManualPaymentScroll.DocumentId.Value;
                // apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.ManualPaymentNumber.Value = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApiNonPoManualPaymentScroll.ManualPaymentNumber.Value;
                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Key = 1;
                TableError tableError = apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApiNonPoManualPaymentScroll.ManualPaymentNumber.Value != string.Empty)
                    {
                        requestObj = new PayableManagementRequest();
                        requestObj.ManualPaymentNumber = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApiNonPoManualPaymentScroll.ManualPaymentNumber.Value;
                        requestObj.FromVendorId = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApiNonPoManualPaymentScroll.VendorId.Value;
                        requestObj.companyId = Dynamics.Globals.CompanyId.Value;

                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/PayableManagement/ValidateNonPODocumentExistsForVendor", requestObj);
                            if (response.Result.IsSuccessStatusCode)
                            {
                                PayableManagementResponse payableResponse = response.Result.Content.ReadAsAsync<PayableManagementResponse>().Result;
                                if (payableResponse.Status == Response.Error)
                                {
                                    logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxDetailMaintenance_DeleteButtonClickAfterOriginal Method (DeleteTaxDetailCustomRecord): " + payableResponse.ErrorMessage.ToString());
                                    MessageBox.Show("Error: " + payableResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    logMessage.AppendLine(payableResponse.LogMessage.ToString());
                                    if (!payableResponse.IsValid)
                                    {
                                        MessageBox.Show(Resources.STR_InvoiceDoesNotExists, Resources.STR_MESSAGE_TITLE);
                                        apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApiNonPoManualPaymentScroll.ManualPaymentNumber.Clear();
                                        apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApiNonPoManualPaymentScroll.ManualPaymentNumber.Focus();
                                    }
                                    else
                                    {
                                        apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.ManualPaymentNumber.Value = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApiNonPoManualPaymentScroll.ManualPaymentNumber.Value;
                                        apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Save();
                                        // ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.ChangeFlag.Value = true;
                                    }
                                }
                            }
                            else
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Expense Report Upload PushToGp ClickAfterOriginal");
                                MessageBox.Show("Error: Expense Report Upload PushToGp ClickAfterOriginal", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                }
                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Close();
                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the check box of non po invoice scroll is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ApiNonPoManualPaymentScrollSelectCheckBox_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {

                //ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.ChangeFlag.Value = true;
                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Close();
                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Release();
                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.DocumentId.Value = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApiNonPoManualPaymentScroll.DocumentId.Value;
                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Key = 1;
                TableError tableError = apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Change();
                if (tableError == TableError.NoError
                            && tableError != TableError.EndOfTable)
                {
                    if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApiNonPoManualPaymentScroll.SelectCheckBox.Value == true)
                    {
                        apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.SelectCheckBox.Value = true;
                        apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Save();
                    }
                    else
                    {
                        apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.SelectCheckBox.Value = false;
                        apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.ManualPaymentNumber.Value = string.Empty;
                        apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Save();
                        apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApiNonPoManualPaymentScroll.ManualPaymentNumber.Value = string.Empty;
                    }
                }
                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Close();
                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Release();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the select check box of manual payment  PO scroll is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ApipoDocumentsWithManualPaymentsScrollSelectCheckBox_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                // ctsiDocumentsWithManualPaymentsForm.CtsiDocumentsWithManualPayments.ChangeFlag.Value = true;
                apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Close();
                apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Release();
                apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.DocumentId.Value = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApipoDocumentsWithManualPaymentsScroll.DocumentId.Value;
                apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Key = 1;
                TableError tableError = apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Change();
                if (tableError == TableError.NoError
                            && tableError != TableError.EndOfTable)
                {
                    if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApipoDocumentsWithManualPaymentsScroll.SelectCheckBox.Value == true)
                    {
                        apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.SelectCheckBox.Value = true;
                        apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Save();
                    }
                    else
                    {
                        apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.SelectCheckBox.Value = false;
                        apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.ManualPaymentNumber.Value = string.Empty;
                        apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Save();
                        apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.ApipoDocumentsWithManualPaymentsScroll.ManualPaymentNumber.Value = string.Empty;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the range radio button is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiDocumentsWithManualAllOrRange_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalDocumentBy.Value == 1
                        && apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.AllOrRange.Value == 0)
                {
                    ClearApiIdFields(Resources.STR_APIDocumentsWithManualPayments);
                }
                else if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalDocumentBy.Value == 2
                            && apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.AllOrRange.Value == 0)
                {
                    ClearReUploadVendorIdFields(Resources.STR_APIDocumentsWithManualPayments);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the manual payment API window is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ApiDocumentsWithManualPayments_CloseAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                //if (ctsiIdLookupForm.IsOpen)
                //{
                //    ctsiIdLookupForm.Close();
                //}
                if (vendorLookupForm.IsOpen)
                {
                    vendorLookupForm.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the vendor id in to field changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiDocumentsWithManualLocalToVendorId_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToVendorId.Value != string.Empty)
                {
                    apiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Close();
                    apiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Release();
                    apiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.VendorId.Value = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToVendorId.Value;
                    apiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Key = 1;
                    TableError Vendorerror = apiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Get();

                    if (Vendorerror != TableError.NoError)
                    {
                        // if not valid displays a message
                        MessageBox.Show(Resources.STR_VendorIdDoesNotExists, Resources.STR_MESSAGE_TITLE);
                        apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToVendorId.Clear();
                        apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToVendorId.Focus();
                    }
                    else
                    {
                        if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToVendorId.Value == string.Empty)
                        {
                            if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromVendorId.Value == string.Empty)
                            {
                                apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.AllOrRange.Value = 0;
                            }
                        }
                        else
                        {
                            apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.AllOrRange.Value = 1;
                        }
                    }
                    apiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Close();
                    apiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Release();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the vendor id entered in window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiDocumentsWithManualLocalFromVendorId_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromVendorId.Value != string.Empty)
                {
                    apiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Close();
                    apiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Release();
                    apiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.VendorId.Value = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromVendorId.Value;
                    apiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Key = 1;
                    TableError Vendorerror = apiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Get();

                    if (Vendorerror != TableError.NoError)
                    {
                        // if not valid displays a message
                        MessageBox.Show(Resources.STR_VendorIdDoesNotExists, Resources.STR_MESSAGE_TITLE);
                        apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromVendorId.Clear();
                        apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromVendorId.Focus();

                    }
                    else if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToVendorId.Value != string.Empty)
                    {
                        apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToVendorId.Value = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromVendorId.Value;
                        apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.AllOrRange.Value = 1;
                    }
                    else
                    {
                        if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromVendorId.Value == string.Empty)
                        {
                            if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToVendorId.Value == string.Empty)
                            {
                                apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.AllOrRange.Value = 0;
                            }
                        }
                        else
                        {
                            apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.AllOrRange.Value = 1;
                            if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToVendorId.Value == string.Empty)
                            {
                                apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToVendorId.Value = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromVendorId.Value;
                            }
                        }
                    }

                    apiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Close();
                    apiDocumentsWithManualPaymentsForm.Tables.PmVendorMstr.Release();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the selection is made in the search list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiDocumentsWithManualLocalDocumentBy_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                InitilizeSortBySelection(Resources.STR_APIDocumentsWithManualPayments);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the lookup button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiDocumentsWithManualLookupButton1_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            logMessage.AppendLine(DateTime.Now + " Started : Open Failed document Id's to scrolling window. "
                   + " by user : " + Dynamics.Globals.UserId.Value.ToString()
                   + " in company : " + Dynamics.Globals.IntercompanyId.Value.ToString());
            try
            {
                if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalDocumentBy.Value == 1)
                {
                    if (apiIdLookupForm.IsOpen)
                    {
                        apiIdLookupForm.Close();
                    }
                    apiIdLookupForm.Open();
                    documentType = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalInvoiceType.Value;
                    apiIdLookupForm.ApiidLookup.RedisplayButton.RunValidate();
                    apiIdLookupForm.ApiidLookup.LocalCalledBy.Value = 1;

                    lookupWindowName = Resources.STR_APIDocumentsWithManualPayments;
                    if (!RegisterApiLookupSelect)
                    {
                        apiIdLookupForm.ApiidLookup.SelectButtonMnemonic.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.apiIdLookupFormApiidLookupSelectButtonClickBeforeOriginal);
                        RegisterApiLookupSelect = true;
                    }
                }
                else if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalDocumentBy.Value == 2)
                {
                    logMessage.AppendLine(DateTime.Now + " Started : Open list of all vendor Id's to scrolling window. "
                   + " by user : " + Dynamics.Globals.UserId.Value.ToString()
                   + " in company : " + Dynamics.Globals.IntercompanyId.Value.ToString());
                    if (vendorLookupForm.IsOpen)
                    {
                        vendorLookupForm.Close();
                    }
                    vendorLookupForm.Open();
                    // Assigning the customer number to lookup form
                    vendorLookupForm.VendorLookup.VendorSortBy.Value = 1;
                    // select the record
                    vendorLookupForm.VendorLookup.VendorId.Value = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromVendorId.Value;
                    vendorLookupForm.VendorLookup.RedisplayButton.RunValidate();
                    // event which calls before the lookup window got closed.
                    vendorLookupForm.VendorLookup.LocalCalledBy.Value = 1;

                    lookupWindowName = Resources.STR_APIDocumentsWithManualPayments;
                    if (!RegisterApiVendorLookupSelect)
                    {
                        vendorLookupForm.VendorLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.vendorLookupFormVendorLookupSelectButtonClickBeforeOriginal);
                        RegisterApiVendorLookupSelect = true;
                    }
                }
                logMessage.AppendLine(DateTime.Now + " Completed returning the from search field.");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// When the to lookup is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void apiDocumentsWithManualLookupButton2_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            logMessage.AppendLine(DateTime.Now + " Started : Open Failed Document Id's to scrolling window. "
                   + " by user : " + Dynamics.Globals.UserId.Value.ToString()
                   + " in company : " + Dynamics.Globals.IntercompanyId.Value.ToString());
            try
            {
                if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalDocumentBy.Value == 1)
                {
                    if (apiIdLookupForm.IsOpen)
                    {
                        apiIdLookupForm.Close();
                    }
                    apiIdLookupForm.Open();
                    documentType = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalInvoiceType.Value;
                    //ManualPymt
                    apiIdLookupForm.ApiidLookup.RedisplayButton.RunValidate();
                    apiIdLookupForm.ApiidLookup.LocalCalledBy.Value = 2;

                    lookupWindowName = Resources.STR_APIDocumentsWithManualPayments;
                    if (!RegisterApiLookupSelect)
                    {
                        apiIdLookupForm.ApiidLookup.SelectButtonMnemonic.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.apiIdLookupFormApiidLookupSelectButtonClickBeforeOriginal);
                        RegisterApiLookupSelect = true;
                    }
                }
                else if (apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalDocumentBy.Value == 2)
                {
                    logMessage.AppendLine(DateTime.Now + " Started : Open list of all vendor Id's to scrolling window. "
                   + " by user : " + Dynamics.Globals.UserId.Value.ToString()
                   + " in company : " + Dynamics.Globals.IntercompanyId.Value.ToString());
                    if (vendorLookupForm.IsOpen)
                    {
                        vendorLookupForm.Close();
                    }
                    vendorLookupForm.Open();
                    // Assigning the customer number to lookup form
                    vendorLookupForm.VendorLookup.VendorSortBy.Value = 1;
                    // select the record
                    vendorLookupForm.VendorLookup.VendorId.Value = apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromVendorId.Value;
                    vendorLookupForm.VendorLookup.RedisplayButton.RunValidate();
                    // event which calls before the lookup window got closed.
                    vendorLookupForm.VendorLookup.LocalCalledBy.Value = 2;

                    lookupWindowName = Resources.STR_APIDocumentsWithManualPayments;
                    if (!RegisterApiVendorLookupSelect)
                    {
                        vendorLookupForm.VendorLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.vendorLookupFormVendorLookupSelectButtonClickBeforeOriginal);
                        RegisterApiVendorLookupSelect = true;
                    }
                }
                logMessage.AppendLine(DateTime.Now + " Completed returning the from search field.");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// To assign the scroll window values to object
        /// </summary>
        /// <returns></returns>
        private PayableManagementRequest AssignApiGridValuetoRequestObject(int invoiceType)
        {
            PayableManagementRequest reqObj = null;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                reqObj = new PayableManagementRequest();
                reqObj.PayableDetailsEntity = new PayableDetailsEntity();
                reqObj.userId = Dynamics.Globals.UserId.Value;
                if (invoiceType == 1)
                {
                    apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Close();
                    apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Release();

                    TableError tableError = apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.GetFirst();
                    while (tableError == TableError.NoError
                                && tableError != TableError.EndOfTable)
                    {
                        if (apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.SelectCheckBox.Value)
                        {
                            reqObj.PayableDetailsEntity.AddInvoiceLine(new PayableLineEntity()
                            {
                                DocumentRowId = apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.DocumentId.Value,
                                DocumentNumber = apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.ManualPaymentNumber.Value,
                                DocumentDate = apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.DocumentDate.Value,
                                VendorId = apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.VendorId.Value,
                                DocumentAmount = apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.DocumentAmount.Value
                            });
                        }
                        tableError = apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.GetNext();
                    }
                    apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Release();
                    apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Close();
                }
                else if (invoiceType == 2)
                {
                    apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Close();
                    apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Release();

                    TableError tableError = apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.GetFirst();
                    while (tableError == TableError.NoError
                                && tableError != TableError.EndOfTable)
                    {
                        if (apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.SelectCheckBox.Value)
                        {
                            reqObj.PayableDetailsEntity.AddInvoiceLine(new PayableLineEntity()
                            {
                                DocumentRowId = apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.DocumentId.Value,
                                DocumentNumber = apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.ManualPaymentNumber.Value,
                                DocumentDate = apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.ReceiptDate.Value,
                                VendorId = apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.VendorId.Value,
                                DocumentAmount = apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.DocumentAmount.Value
                            });
                        }
                        tableError = apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.GetNext();
                    }
                    apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Release();
                    apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
            return reqObj;
        }
        #endregion

        #region PrivateApiReuploadfailedDocuments

        private void ReloadFailedApiDocumentscrollContents(PayableManagementResponse responseObj)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                //load the details into window
                if (responseObj.Status == Response.Success)
                {
                    this.ClearScrollingWindows(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_AllScroll);

                    //if the line details exists then fill scroll
                    if (responseObj.PayableDetailsEntity != null && responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Count > 0)
                    {
                        if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalInvoiceType.Value == 1)
                        {
                            apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();
                            apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                            for (int i = 0; i < responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Count; i++)
                            {
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.SelectCheckBox.Value = false;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.OriginalApiInvoiceId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].OriginalApiInvoiceId;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.DocumentId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentId;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.VoucherNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].VoucherNumber;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.DocumentDate.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentDate;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.VendorId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].VendorId;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.DocumentAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentAmount;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.ApprovedAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ApprovedAmount;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.FreightAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].FreightAmount;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.TaxAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].TaxAmount;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.PurchasesAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].PurchaseAmount;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.CurrencyId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].CurrencyCode;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.DocumentTypeName.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentTypeName;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.GlAccountNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GLAccount;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.GlAccountDescription.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GLAccountDescription;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.TradeDiscountAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].TradeDiscounts;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.MiscAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].MiscellaneousAmount;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.TaxScheduleId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].TaxScheduleId;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.LocationName.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].LocationName;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.AccountIndex.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GLIndex;
                                apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Save();

                            }

                            apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Release();
                            apiReUploadFailureDocumentsForm.Tables.ApiNonPoReUpload.Close();

                            //Fill details to scrolling window.Call dex procedure to fill the scrolling window.
                            apiReUploadFailureDocumentsForm.Procedures.ApiNonPoReUploadDocumentsSelectScrollFill.Invoke();
                        }
                        else
                        {
                            apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();
                            apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                            for (int i = 0; i < responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Count; i++)
                            {

                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.SelectCheckBox.Value = false;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.OriginalApiInvoiceId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].OriginalApiInvoiceId;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.DocumentId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentId;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.DocumentNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentNumber;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ReceiptDate.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ReceiptDate;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.VendorId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].VendorId;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.DocumentAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentAmount;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ApprovedAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ApprovedAmount;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.FreightAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].FreightAmount;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.TaxAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].TaxAmount;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.MiscAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].MiscellaneousAmount;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.PurchasesAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].PurchaseAmount;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.PoNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].PurchaseOrderNumber;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.CurrencyId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].CurrencyCode;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.DocumentTypeName.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentTypeName;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.PoLineNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].POLineNumber;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ItemNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ItemNumber;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.PopReceiptNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ReceiptNumber;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ReceiptLineNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ReceiptLineNumber;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.QtyShipped.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].QuantityShipped;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.QtyToInvoice.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].AdjustedItemUnitQuantity;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.UnitCost.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].UnitCost;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.ExtendedCost.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ExtendedCost;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.AdjustedUnitCost.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].AdjustedItemUnitPrice;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.GlAccountNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GLAccount;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.TaxScheduleId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].TaxScheduleId;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.LocationName.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].LocationName;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.AccountIndex.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GLIndex;
                                apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Save();
                            }

                            apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Release();
                            apiReUploadFailureDocumentsForm.Tables.ApipoReUploadTemp.Close();

                            //Fill details to scrolling window.Call dex procedure to fill the scrolling window.
                            apiReUploadFailureDocumentsForm.Procedures.ApipoReUploadDocumentsSelectScrollFill.Invoke();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="responseObj"></param>
        private void LoadReuploadValidApiScrollContents(PayableManagementResponse responseObj)
        {
            try
            {
                if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalInvoiceType.Value == 1)
                {
                    //clear scroll
                    ClearScrollingWindows(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APINonPOValidDocumentsScroll);
                    apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Close();
                    apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Release();
                    for (int i = 0; i < responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Count; i++)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.DocumentIdStatus.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentIdStatus.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.OriginalApiInvoiceId.Value = Convert.ToInt32(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].OriginalApiInvoiceId.ToString().Trim());
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.DocumentId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentId.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.VoucherNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].VoucherNumber.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.DocumentDate.Value = Convert.ToDateTime(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentDate);
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.VendorId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].VendorId.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.DocumentAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentAmount);
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.ApprovedAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ApprovedAmount);
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.FreightAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].FreightAmount);
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.TaxAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].TaxAmount);
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.PurchasesAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].PurchaseAmount);
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.TaxScheduleId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].TaxScheduleId.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.CurrencyId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].CurrencyCode.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.DocumentTypeName.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentTypeName.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.GlAccountNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GLAccount.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.GlAccountDescription.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GLAccountDescription.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.TradeDiscountAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].TradeDiscounts);
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.MiscAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].MiscellaneousAmount);
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.LocationName.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].LocationName;
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Status.Value = Convert.ToInt16(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].StatusId);
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.ErrorDescription.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ErrorDescription.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.AccountIndex.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GLIndex;

                        apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Save();
                    }


                    apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Release();
                    apiReUploadFailureDocumentsForm.Tables.ApiNonPoValidReUploadTemp.Close();

                    apiReUploadFailureDocumentsForm.Procedures.ApiNonPoValidDocumentsScrollFill.Invoke();

                    if ((responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Where(o => o.DocumentIdStatus.ToString().ToUpper() == Resources.STR_APIStatusFail)).Count() == 0)
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.PushToGp.Enable();
                    else
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.PushToGp.Disable();
                }
                else
                {
                    //clear scroll
                    ClearScrollingWindows(Resources.STR_ReUploadFailureApiDocuments, Resources.STR_APIPOValidDocumentsScroll);
                    apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Close();
                    apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Release();
                    for (int i = 0; i < responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Count; i++)
                    {
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.DocumentIdStatus.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentIdStatus.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.OriginalApiInvoiceId.Value = Convert.ToInt32(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].OriginalApiInvoiceId.ToString().Trim());
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.DocumentId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentId.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.DocumentNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentNumber.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ReceiptDate.Value = Convert.ToDateTime(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ReceiptDate);
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.VendorId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].VendorId.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.DocumentAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentAmount);
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ApprovedAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ApprovedAmount);
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.FreightAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].FreightAmount);
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.TaxAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].TaxAmount);
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.MiscAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].MiscellaneousAmount);
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.PurchasesAmount.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].PurchaseAmount);
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.CurrencyId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].CurrencyCode.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.DocumentTypeName.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentTypeName.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.PoNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].PurchaseOrderNumber.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.PoLineNumber.Value = Convert.ToInt32(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].POLineNumber);
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ItemNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ItemNumber.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.PopReceiptNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ReceiptNumber.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ReceiptLineNumber.Value = Convert.ToInt32(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ReceiptLineNumber);
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.QtyShipped.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].QuantityShipped);
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.QtyToInvoice.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].AdjustedItemUnitQuantity);
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.UnitCost.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].UnitCost);
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ExtendedCost.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ExtendedCost);
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.AdjustedUnitCost.Value = Convert.ToDecimal(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].AdjustedItemUnitPrice);
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.GlAccountNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GLAccount.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.TaxScheduleId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].TaxScheduleId.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.LocationName.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].LocationName;
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Status.Value = Convert.ToInt16(responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].StatusId);
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.ErrorDescription.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ErrorDescription.ToString().Trim();
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.AccountIndex.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GLIndex;
                        apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Save();
                    }


                    apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Release();
                    apiReUploadFailureDocumentsForm.Tables.ApipoValidReUploadTemp.Close();

                    apiReUploadFailureDocumentsForm.Procedures.ApipoValidDocumentsScrollFill.Invoke();

                    if ((responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Where(o => o.DocumentIdStatus.ToString().ToUpper() == Resources.STR_APIStatusFail)).Count() == 0)
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.PushToGp.Enable();
                    else
                        apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.PushToGp.Disable();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void ShowReUploadApiFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_ReUploadFailureApiDocuments)
                {
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromApiId.Show();
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToApiId.Show();
                }
                else if (formName == Resources.STR_APIDocumentsWithManualPayments)
                {
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromDocId.Show();
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToDocId.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// Method to hide expense fields of the windows based on the passed parameter form name.
        /// </summary>
        /// <param name="formName"></param>
        private void HideReUploadApiFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_ReUploadFailureApiDocuments)
                {
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromApiId.Hide();
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToApiId.Hide();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// Method to clear expense fields of the windows based on the passed parameter form name.
        /// </summary>
        /// <param name="formName"></param>
        private void ClearApiIdFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_ReUploadFailureApiDocuments)
                {
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromApiId.Clear();
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToApiId.Clear();
                }
                else if (formName == Resources.STR_APIDocumentsWithManualPayments)
                {
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromDocId.Clear();
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToDocId.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="formName"></param>
        private void RestrictUpdatingApiFields(string formName, string scrollingWindowName)
        {
            StringBuilder logMessage = new StringBuilder();

            try
            {
                if (formName == Resources.STR_ReUploadFailureApiDocuments)
                {
                    if (scrollingWindowName == Resources.STR_APINonPOReUploadDocumentsSelectScroll)
                    {
                        if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.SelectCheckBox.Value == false)
                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApiNonPoReUploadDocumentsSelectScroll.SelectCheckBox.Focus();
                    }
                    else
                    {
                        if (apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.SelectCheckBox.Value == false)
                            apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.ApipoReUploadDocumentsSelectScroll.SelectCheckBox.Focus();
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
        }

        #endregion

        #region PrivateMethodsAPIManualPayments

        private void ClearAPIScrollingWindows(string formName, int invoiceType)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_APIDocumentsWithManualPayments)
                {
                    if (invoiceType == 1)
                    {
                        apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Close();
                        apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Release();
                        TableError errorFailureTemp = apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.ChangeFirst();
                        while (errorFailureTemp != TableError.EndOfTable && errorFailureTemp == TableError.NoError)
                        {
                            apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Remove();
                            errorFailureTemp = apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.ChangeNext();
                        }
                        apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Close();
                        apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Release();
                        apiDocumentsWithManualPaymentsForm.Procedures.ApiDocumentsWithManualPaymentsPoFillScroll.Invoke();
                    }
                    else if (invoiceType == 2)
                    {
                        apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Close();
                        apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Release();
                        TableError errorFailureTemp = apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.ChangeFirst();
                        while (errorFailureTemp != TableError.EndOfTable && errorFailureTemp == TableError.NoError)
                        {
                            apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Remove();
                            errorFailureTemp = apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.ChangeNext();
                        }
                        apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Close();
                        apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Release();
                        apiDocumentsWithManualPaymentsForm.Procedures.ApiDocumentsWithManualPaymentsNonPoFillScroll.Invoke();
                    }
                    else if (invoiceType == 0)
                    {
                        apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Close();
                        apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Release();
                        TableError errorFailureTemp = apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.ChangeFirst();
                        while (errorFailureTemp != TableError.EndOfTable && errorFailureTemp == TableError.NoError)
                        {
                            apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Remove();
                            errorFailureTemp = apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.ChangeNext();
                        }
                        apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Close();
                        apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Release();
                        apiDocumentsWithManualPaymentsForm.Procedures.ApiDocumentsWithManualPaymentsPoFillScroll.Invoke();

                        apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Close();
                        apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Release();
                        TableError errorNonFailureTemp = apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.ChangeFirst();
                        while (errorNonFailureTemp != TableError.EndOfTable && errorNonFailureTemp == TableError.NoError)
                        {
                            apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Remove();
                            errorNonFailureTemp = apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.ChangeNext();
                        }
                        apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Close();
                        apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Release();
                        apiDocumentsWithManualPaymentsForm.Procedures.ApiDocumentsWithManualPaymentsNonPoFillScroll.Invoke();
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }
        /// <summary>
        /// Method to hide all the fields of the windows based on the passed parameter form name.
        /// </summary>
        /// <param name="formName"></param>
        private void HideAllSortByReUploadApiFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_ReUploadFailureApiDocuments)
                {
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromApiId.Hide();
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToApiId.Hide();
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromDocumentDate.Hide();
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToDocumentDate.Hide();
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalFromVendorId.Hide();
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LocalToVendorId.Hide();
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LookupButton1.Hide();
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.LookupButton2.Hide();
                    apiReUploadFailureDocumentsForm.ApiReUploadFailureDocuments.PushToGp.Lock();
                }
                else if (formName == Resources.STR_APIDocumentsWithManualPayments)
                {
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromDocId.Hide();
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToDocId.Hide();
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalFromVendorId.Hide();
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LocalToVendorId.Hide();
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LookupButton1.Hide();
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.LookupButton2.Hide();
                    apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.UpdateAsManuallyCreated.Lock();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }
        }

        /// <summary>
        /// Fill the scroll values
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="responseObj"></param>
        private void LinkFailedApiRecordsScrollContents(PayableManagementResponse responseObj, string scrollingwindow)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                //load the details into window
                if (responseObj.Status == Response.Success)
                {
                    this.ClearScrollingWindows(Resources.STR_APIDocumentsWithManualPayments, scrollingwindow);

                    if (scrollingwindow == "POInvoice")
                    {
                        //if the line details exists then fill scroll
                        if (responseObj.PayableDetailsEntity != null && responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Count > 0)
                        {
                            apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Close();
                            apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Release();

                            for (int i = 0; i < responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Count; i++)
                            {

                                apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.SelectCheckBox.Value = false;
                                apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.DocumentId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentRowId;
                                apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.ReceiptDate.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].ReceiptDate;
                                apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.VendorId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].VendorId;
                                apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.DocumentAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentAmount;
                                apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.ManualPaymentNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GPVoucherNumber;
                                apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.DocumentNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentNumber;
                                apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Save();
                                //TableError err = apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Save();
                                //if (TableError.NoError ==err)
                                //{
                                //    MessageBox.Show("Error");
                                //}
                            }

                            apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Release();
                            apiDocumentsWithManualPaymentsForm.Tables.ApipoManualPaymentsTemp.Close();

                            //Fill details to scrolling window.Call dex procedure to fill the scrolling window.
                            apiDocumentsWithManualPaymentsForm.Procedures.ApiDocumentsWithManualPaymentsPoFillScroll.Invoke();
                            apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.UpdateAsManuallyCreated.Unlock();
                            logMessage.AppendLine(DateTime.Now + " Successfully completed loading the scroll contents to link API to manual payments scrolling window..");

                        }
                    }
                    else if (scrollingwindow == "NonPOInvoice")
                    {
                        //if the line details exists then fill scroll
                        if (responseObj.PayableDetailsEntity != null && responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Count > 0)
                        {
                            apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Close();
                            apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Release();

                            for (int i = 0; i < responseObj.PayableDetailsEntity.GetInvoiceLineDetails.Count; i++)
                            {

                                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.SelectCheckBox.Value = false;
                                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.DocumentId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentRowId;
                                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.DocumentDate.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentDate;
                                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.VendorId.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].VendorId;
                                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.DocumentAmount.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentAmount;
                                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.ManualPaymentNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].GPVoucherNumber;
                                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.DocumentNumber.Value = responseObj.PayableDetailsEntity.GetInvoiceLineDetails[i].DocumentNumber;
                                apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Save();

                            }

                            apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Release();
                            apiDocumentsWithManualPaymentsForm.Tables.ApiNonPoManualPaymentsTemp.Close();

                            //Fill details to scrolling window.Call dex procedure to fill the scrolling window.
                            apiDocumentsWithManualPaymentsForm.Procedures.ApiDocumentsWithManualPaymentsNonPoFillScroll.Invoke();
                            apiDocumentsWithManualPaymentsForm.ApiDocumentsWithManualPayments.UpdateAsManuallyCreated.Unlock();
                            logMessage.AppendLine(DateTime.Now + " Successfully completed loading the scroll contents to link API to manual payments scrolling window..");

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PayablesMgmt");
                logMessage = null;
            }

        }
        #endregion

        #endregion

        /// <summary>

        ///Method to move the create  the log file and for todays date and write to log file.

        /// </summary>

        /// <param name="message">log message</param>

        /// <param name="logFileName">Name of the log file</param>

        /// <param name="logFilePath">Path for the log file</param>

        private void LogPostProcessDetailsToFile(string message, string fileType)
        {

            switch (fileType)
            {

                case "PayablesMgmt":

                    if (isPmLoggingEnabled && message != "")

                        new TextLogger().LogInformationIntoFile(message, pmLogFilePath, pmLogFileName);

                    break;

                default:

                    break;

            }

        }
    }
}