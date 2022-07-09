using System;
using System.Collections.Generic;
using Microsoft.Dexterity.Bridge;
using Microsoft.Dexterity.Applications;
using Dic4745 = Microsoft.Dexterity.Applications.ChemPointSalesExtDictionary;
using System.Runtime.InteropServices;
using System.IO;
using Chempoint.GP.Model.Interactions.Setup;
using System.Windows.Forms;
using Chempoint.GP.Setup.Properties;
using System.Net.Http;
using System.Net.Http.Headers;
using ChemPoint.GP.Entities.Business_Entities.Setup;
using ChemPoint.GP.Entities.BaseEntities;
using System.Text;
using Chempoint.GP.Infrastructure.Logging;

namespace Chempoint.GP.Setup
{
    public class GPAddIn : IDexterityAddIn
    {
        static Dic4745.TaxDetailMaintenanceForm taxDetailMaintenanceForm;
        static Dic4745.TaxScheduleMaintenanceForm taxScheduleMaintenanceForm;
        static Dic4745.SyPaymentTermsForm syPaymentTermsForm;
        bool registerTaxDetalWindowEvents = false;
        bool registerTaxSchWindowEvents = false;
        bool registerPaymentTermWindowEvents = false;
        static string gpServiceConfigurationUrl = null;
        bool isSetupLoggingEnabled = false;
        string setupLogFileName = null;
        string setupLogFilePath = null;

        [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileStringW",
            SetLastError = true,
            CharSet = CharSet.Unicode, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, string lpReturnString, int nSize, string lpFilename);

        public void Initialize()
        {
            taxDetailMaintenanceForm = ChemPointSalesExt.Forms.TaxDetailMaintenance;
            taxScheduleMaintenanceForm = ChemPointSalesExt.Forms.TaxScheduleMaintenance;
            syPaymentTermsForm = ChemPointSalesExt.Forms.SyPaymentTerms;

            taxDetailMaintenanceForm.TaxDetailMaintenance.OpenBeforeOriginal += TaxDetailMaintenanceOpenBeforeOriginal;
            taxScheduleMaintenanceForm.TaxScheduleMaintenance.OpenBeforeOriginal += TaxScheduleMaintenanceOpenBeforeOriginal;
            syPaymentTermsForm.SyPaymentTerms.OpenBeforeOriginal += PaymentTermSetupOpenBeforeOriginal;

            string getCurrentDirectoryPath = @Directory.GetCurrentDirectory();
            string iniFilePath = getCurrentDirectoryPath + "\\Data\\Dex.ini";
            List<string> categories = GetCategories(iniFilePath);
            foreach (string category in categories)
            {
                //Get the key values
                gpServiceConfigurationUrl = GetIniFileString(iniFilePath, category, "GPSERVICE", "");
                isSetupLoggingEnabled = Convert.ToBoolean(GetIniFileString(iniFilePath, category, "ISSETUPLOGENABLED", ""));
                setupLogFileName = GetIniFileString(iniFilePath, category, "SETUPLOGFILENAME", "");
                setupLogFilePath = GetIniFileString(iniFilePath, category, "SETUPLOGFILEPATH", "");

            }
        }

        #region Common

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

        #endregion Common

        #region TaxDetailRefDetails

        void TaxDetailMaintenanceOpenBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (Dynamics.Globals.CompanyId.Value == 2)
                {
                    if (!registerTaxDetalWindowEvents)
                    {
                        taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailReference.LeaveAfterOriginal += TaxDetailMaintenance_TaxDetailReferenceChange;
                        taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCode.LeaveAfterOriginal += TaxDetailMaintenance_TaxDetailReferenceChange;
                        taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCodeDescription.LeaveAfterOriginal += TaxDetailMaintenance_TaxDetailReferenceChange;

                        taxDetailMaintenanceForm.TaxDetailMaintenance.SaveRecord.ValidateAfterOriginal += TaxDetailMaintenance_SaveRecordChange;
                        taxDetailMaintenanceForm.TaxDetailMaintenance.DisplayExistingRecord.ValidateAfterOriginal += TaxDetailMaintenance_DisplayExistingRecordChange;
                        taxDetailMaintenanceForm.TaxDetailMaintenance.DeleteButton.ClickBeforeOriginal += TaxDetailMaintenance_DeleteButtonClickAfterOriginal;
                        taxDetailMaintenanceForm.TaxDetailMaintenance.ClearButton.ClickAfterOriginal += TaxDetailMaintenance_ClearButtonClickAfterOriginal;

                        registerTaxDetalWindowEvents = true;
                    }

                    taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCode.Show();
                    taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCodeDescription.Show();
                    taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailReference.Show();
                    taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCode.Enable();
                    taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCodeDescription.Enable();
                    taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailReference.Enable();
                }
                else if (Dynamics.Globals.CompanyId.Value == 1)
                {
                    taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCode.Hide();
                    taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCodeDescription.Hide();
                    taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailReference.Hide();
                    taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCode.Disable();
                    taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCodeDescription.Disable();
                    taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailReference.Disable();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxDetailMaintenanceOpenBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Event called when the delete button on Tax detail window is pressed
        /// This event will delete the custom records from database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TaxDetailMaintenance_DeleteButtonClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (Dynamics.Globals.CompanyId.Value == 2)
                {
                    if (!string.IsNullOrEmpty(taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailId.Value))
                    {
                        SetupRequest taxDetailsSetupRequest = new SetupRequest();
                        SetupEntity setupDetailEntity = new SetupEntity();
                        TaxSetupInformation setupInformation = new TaxSetupInformation();
                        setupInformation.TaxDetailId = taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailId.Value.ToString();
                        setupDetailEntity.SetupDetails = setupInformation;
                        taxDetailsSetupRequest.SetupEntity = setupDetailEntity;

                        if (taxDetailsSetupRequest != null)
                        {
                            // Service call ...
                            using (HttpClient client = new HttpClient())
                            {
                                client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                var response = client.PostAsJsonAsync("api/SetupExtUpdate/DeleteTaxDetailCustomRecord", taxDetailsSetupRequest);
                                if (response.Result.IsSuccessStatusCode)
                                {
                                    SetupResponse setupResponse = response.Result.Content.ReadAsAsync<SetupResponse>().Result;
                                    if (setupResponse.Status == ResponseStatus.Error)
                                    {
                                        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxDetailMaintenance_DeleteButtonClickAfterOriginal Method (DeleteTaxDetailCustomRecord): " + setupResponse.ErrorMessage.ToString());
                                        MessageBox.Show("Error: " + setupResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                    }
                                    else
                                    {
                                        ClearTaxDetails();
                                    }
                                }
                                else
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not delete from tax details Table");
                                    MessageBox.Show("Error: Data does not delete from tax details Table", Resources.STR_MESSAGE_TITLE);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxDetailMaintenance_DeleteButtonClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Event called when clear button on the Tax detail widnow is clicked
        /// This event will clear the custom fields from form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TaxDetailMaintenance_ClearButtonClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (Dynamics.Globals.CompanyId.Value == 2)
                {
                    ClearTaxDetails();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxDetailMaintenance_ClearButtonClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Refresh the custom fields on tax detail change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TaxDetailMaintenance_DisplayExistingRecordChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (Dynamics.Globals.CompanyId.Value == 2)
                {
                    // clears the value in form.
                    ClearTaxDetails();

                    if (!string.IsNullOrEmpty(taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailId.Value))
                    {
                        // calls the service and get the values
                        InsertTaxDetails(taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailId.Value.ToString());

                        // loads the details to form
                        DisplayTaxDetails(taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailId.Value.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxDetailMaintenance_DisplayExistingRecordChange Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Event to save the custom field values to tables
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TaxDetailMaintenance_SaveRecordChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (Dynamics.Globals.CompanyId.Value == 2 && !string.IsNullOrEmpty(taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailId.Value))
                {
                    SetupRequest taxDetailsSetupRequest = new SetupRequest();
                    SetupEntity setupDetailEntity = new SetupEntity();
                    TaxSetupInformation setupInformation = new TaxSetupInformation();
                    taxDetailsSetupRequest.UserId = Dynamics.Globals.UserId.Value.ToString();

                    taxDetailMaintenanceForm.Tables.TaxDetailTemp.Close();
                    taxDetailMaintenanceForm.Tables.TaxDetailTemp.Release();
                    taxDetailMaintenanceForm.Tables.TaxDetailTemp.TaxDetailId.Value = taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailId.Value;
                    taxDetailMaintenanceForm.Tables.TaxDetailTemp.Key = 1;
                    TableError tableError = taxDetailMaintenanceForm.Tables.TaxDetailTemp.Get();
                    if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        setupInformation.TaxDetailId = taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailId.Value;
                        setupInformation.TaxDetailReference = taxDetailMaintenanceForm.Tables.TaxDetailTemp.TaxDetailReference.Value.ToString();
                        setupInformation.TaxDetailUnivarNvTaxCode = taxDetailMaintenanceForm.Tables.TaxDetailTemp.UnivarNvTaxCode.Value.ToString();
                        setupInformation.TaxDetailUnivarNvTaxCodeDescription = taxDetailMaintenanceForm.Tables.TaxDetailTemp.UnivarNvTaxCodeDescription.Value.ToString();
                    }
                    else
                    {
                        setupInformation.TaxDetailId = taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailId.Value;
                        setupInformation.TaxDetailReference = "";
                        setupInformation.TaxDetailUnivarNvTaxCode = "";
                        setupInformation.TaxDetailUnivarNvTaxCodeDescription = "";
                    }
                    taxDetailMaintenanceForm.Tables.TaxDetailTemp.Release();
                    taxDetailMaintenanceForm.Tables.TaxDetailTemp.Close();

                    setupDetailEntity.SetupDetails = setupInformation;
                    taxDetailsSetupRequest.SetupEntity = setupDetailEntity;
                    taxDetailsSetupRequest.CompanyID = Dynamics.Globals.CompanyId.Value;

                    if (taxDetailsSetupRequest != null)
                    {
                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/SetupExtUpdate/SaveTaxDetailCustomRecord", taxDetailsSetupRequest); // we need to refer the web.api service url here.
                            if (response.Result.IsSuccessStatusCode)
                            {
                                SetupResponse setupResponse = response.Result.Content.ReadAsAsync<SetupResponse>().Result;
                                if (setupResponse.Status == ResponseStatus.Error)
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxDetailMaintenance_SaveRecordChange Method (SaveTaxDetailCustomRecord): " + setupResponse.ErrorMessage.ToString());
                                    MessageBox.Show("Error: " + setupResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    //clears the field values
                                    ClearTaxDetails();
                                }
                            }
                            else
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not saved into custom Tax Details Table");
                                MessageBox.Show("Error: Data does not saved into custom Tax Details Table", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxDetailMaintenance_SaveRecordChange Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// To clear the temp table and the form fields of Tax Details maintenance window
        /// </summary>
        private void ClearTaxDetails()
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                // clears the temp table
                taxDetailMaintenanceForm.Tables.TaxDetailTemp.Close();
                taxDetailMaintenanceForm.Tables.TaxDetailTemp.Release();
                taxDetailMaintenanceForm.Tables.TaxDetailTemp.TaxDetailId.Value = taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailId.Value;
                taxDetailMaintenanceForm.Tables.TaxDetailTemp.Key = 1;
                TableError tempErrorQuote = taxDetailMaintenanceForm.Tables.TaxDetailTemp.ChangeFirst();
                while (tempErrorQuote == TableError.NoError && tempErrorQuote != TableError.EndOfTable)
                {
                    taxDetailMaintenanceForm.Tables.TaxDetailTemp.Remove();
                    tempErrorQuote = taxDetailMaintenanceForm.Tables.TaxDetailTemp.ChangeNext();
                }
                taxDetailMaintenanceForm.Tables.TaxDetailTemp.Release();
                taxDetailMaintenanceForm.Tables.TaxDetailTemp.Close();

                // clears the form fields
                taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailReference.Value = string.Empty;
                taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCode.Value = string.Empty;
                taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCodeDescription.Value = string.Empty;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearTaxDetails Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Fetches the values from DB by calling service
        /// Once it fetches it loads to Form temp table.
        /// </summary>
        /// <param name="taxDetailsSetupRequest"></param>
        private void InsertTaxDetails(string taxDetailId)
        {
            StringBuilder logMessage = new StringBuilder();

            SetupRequest taxDetailsSetupRequest = null;
            SetupResponse taxDetailsSetupResponse = null;
            try
            {
                taxDetailsSetupRequest = new SetupRequest();

                SetupEntity setupDetailEntity = new SetupEntity();
                TaxSetupInformation setupInformation = new TaxSetupInformation();
                setupInformation.TaxDetailId = taxDetailId;
                setupDetailEntity.SetupDetails = setupInformation;
                taxDetailsSetupRequest.SetupEntity = setupDetailEntity;

                if (taxDetailsSetupRequest != null)
                {
                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();

                        // Add an Accept header for JSON format.
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = client.PostAsJsonAsync("api/SetupExtUpdate/FetchTaxDetailCustomRecord", taxDetailsSetupRequest);
                        if (response.Result.IsSuccessStatusCode)
                        {
                            taxDetailsSetupResponse = response.Result.Content.ReadAsAsync<SetupResponse>().Result;
                            if (taxDetailsSetupResponse.Status == ResponseStatus.Error)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertTaxDetails Method (FetchTaxDetailCustomRecord): " + taxDetailsSetupResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + taxDetailsSetupResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                if (taxDetailsSetupResponse.SetupDetailsEntity != null)
                                {
                                    taxDetailMaintenanceForm.Tables.TaxDetailTemp.Close();
                                    taxDetailMaintenanceForm.Tables.TaxDetailTemp.Release();
                                    taxDetailMaintenanceForm.Tables.TaxDetailTemp.TaxDetailId.Value = taxDetailId;
                                    taxDetailMaintenanceForm.Tables.TaxDetailTemp.Key = 1;
                                    TableError tableError = taxDetailMaintenanceForm.Tables.TaxDetailTemp.Change();
                                    if (tableError == TableError.NotFound && tableError != TableError.EndOfTable)
                                    {
                                        taxDetailMaintenanceForm.Tables.TaxDetailTemp.TaxDetailId.Value = taxDetailsSetupResponse.SetupDetailsEntity.SetupDetails.TaxDetailId.ToString();
                                        taxDetailMaintenanceForm.Tables.TaxDetailTemp.TaxDetailReference.Value = taxDetailsSetupResponse.SetupDetailsEntity.SetupDetails.TaxDetailReference.ToString();
                                        taxDetailMaintenanceForm.Tables.TaxDetailTemp.UnivarNvTaxCode.Value = taxDetailsSetupResponse.SetupDetailsEntity.SetupDetails.TaxDetailUnivarNvTaxCode.ToString();
                                        taxDetailMaintenanceForm.Tables.TaxDetailTemp.UnivarNvTaxCodeDescription.Value = taxDetailsSetupResponse.SetupDetailsEntity.SetupDetails.TaxDetailUnivarNvTaxCodeDescription.ToString();
                                        taxDetailMaintenanceForm.Tables.TaxDetailTemp.Save();
                                    }
                                    taxDetailMaintenanceForm.Tables.TaxDetailTemp.Release();
                                    taxDetailMaintenanceForm.Tables.TaxDetailTemp.Close();
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not fetch for tax details");
                            MessageBox.Show("Error: Data does not fetch for tax details", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertTaxDetails Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Load the details to form
        /// </summary>
        /// <param name="TaxDetailId"></param>
        private void DisplayTaxDetails(string taxDetailId)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                taxDetailMaintenanceForm.Tables.TaxDetailTemp.Close();
                taxDetailMaintenanceForm.Tables.TaxDetailTemp.Release();
                taxDetailMaintenanceForm.Tables.TaxDetailTemp.TaxDetailId.Value = taxDetailId;
                taxDetailMaintenanceForm.Tables.TaxDetailTemp.Key = 1;
                TableError tableError = taxDetailMaintenanceForm.Tables.TaxDetailTemp.Get();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailReference.Value = taxDetailMaintenanceForm.Tables.TaxDetailTemp.TaxDetailReference.Value.ToString();
                    taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCode.Value = taxDetailMaintenanceForm.Tables.TaxDetailTemp.UnivarNvTaxCode.Value.ToString();
                    taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCodeDescription.Value = taxDetailMaintenanceForm.Tables.TaxDetailTemp.UnivarNvTaxCodeDescription.Value.ToString();
                }
                else
                {
                    taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailReference.Clear();
                    taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCode.Clear();
                    taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCodeDescription.Clear();
                }
                taxDetailMaintenanceForm.Tables.TaxDetailTemp.Release();
                taxDetailMaintenanceForm.Tables.TaxDetailTemp.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In DisplayTaxDetails Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Event to update temp tables with the changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaxDetailMaintenance_TaxDetailReferenceChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (Dynamics.Globals.CompanyId.Value == 2)
                {
                    taxDetailMaintenanceForm.Tables.TaxDetailTemp.Close();
                    taxDetailMaintenanceForm.Tables.TaxDetailTemp.Release();
                    taxDetailMaintenanceForm.Tables.TaxDetailTemp.TaxDetailId.Value = taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailId.Value;
                    taxDetailMaintenanceForm.Tables.TaxDetailTemp.Key = 1;
                    TableError tableError = taxDetailMaintenanceForm.Tables.TaxDetailTemp.Change();
                    if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        taxDetailMaintenanceForm.Tables.TaxDetailTemp.TaxDetailReference.Value = taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailReference.Value.ToString();
                        taxDetailMaintenanceForm.Tables.TaxDetailTemp.UnivarNvTaxCode.Value = taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCode.Value.ToString();
                        taxDetailMaintenanceForm.Tables.TaxDetailTemp.UnivarNvTaxCodeDescription.Value = taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCodeDescription.Value.ToString();
                        taxDetailMaintenanceForm.Tables.TaxDetailTemp.Save();
                    }
                    else
                    {
                        if (tableError == TableError.NotFound && tableError != TableError.EndOfTable)
                        {
                            taxDetailMaintenanceForm.Tables.TaxDetailTemp.TaxDetailId.Value = taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailId.Value;
                            taxDetailMaintenanceForm.Tables.TaxDetailTemp.TaxDetailReference.Value = taxDetailMaintenanceForm.TaxDetailMaintenance.TaxDetailReference.Value.ToString();
                            taxDetailMaintenanceForm.Tables.TaxDetailTemp.UnivarNvTaxCode.Value = taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCode.Value.ToString();
                            taxDetailMaintenanceForm.Tables.TaxDetailTemp.UnivarNvTaxCodeDescription.Value = taxDetailMaintenanceForm.TaxDetailMaintenance.UnivarNvTaxCodeDescription.Value.ToString();
                            taxDetailMaintenanceForm.Tables.TaxDetailTemp.Save();
                        }
                    }
                    taxDetailMaintenanceForm.Tables.TaxDetailTemp.Release();
                    taxDetailMaintenanceForm.Tables.TaxDetailTemp.Close();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxDetailMaintenance_TaxDetailReferenceChange Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        #endregion TaxDetailRefDetails

        #region  TaxScheduleMaintenance

        void TaxScheduleMaintenanceOpenBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (Dynamics.Globals.CompanyId.Value == 2)
                {
                    if (!registerTaxSchWindowEvents)
                    {
                        taxScheduleMaintenanceForm.TaxScheduleMaintenance.SaveRecord.ValidateAfterOriginal += TaxScheduleMaintenance_SaveRecordTaxScheduleDetails;
                        taxScheduleMaintenanceForm.TaxScheduleMaintenance.DisplayExistingRecord.ValidateAfterOriginal += TaxScheduleMaintenance_DisplayExistingTaxScheduleRecordChange;
                        taxScheduleMaintenanceForm.TaxScheduleMaintenance.ChempointVat.ValidateAfterOriginal += TaxScheduleMaintenance_CustomFieldValueChange;
                        taxScheduleMaintenanceForm.TaxScheduleMaintenance.Inactive.ValidateAfterOriginal += TaxScheduleMaintenance_CustomFieldValueChange;
                        taxScheduleMaintenanceForm.TaxScheduleMaintenance.DeleteButton.ClickBeforeOriginal += TaxScheduleMaintenance_DeleteButtonTaxScheduleClickBeforeOriginal;
                        registerTaxSchWindowEvents = true;
                    }
                    taxScheduleMaintenanceForm.TaxScheduleMaintenance.ChempointVat.Show();
                    taxScheduleMaintenanceForm.TaxScheduleMaintenance.Inactive.Show();
                    taxScheduleMaintenanceForm.TaxScheduleMaintenance.ChempointVat.Enable();
                    taxScheduleMaintenanceForm.TaxScheduleMaintenance.Inactive.Enable();
                }
                else if (Dynamics.Globals.CompanyId.Value == 1)
                {
                    taxScheduleMaintenanceForm.TaxScheduleMaintenance.ChempointVat.Hide();
                    taxScheduleMaintenanceForm.TaxScheduleMaintenance.Inactive.Hide();
                    taxScheduleMaintenanceForm.TaxScheduleMaintenance.ChempointVat.Disable();
                    taxScheduleMaintenanceForm.TaxScheduleMaintenance.Inactive.Disable();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxScheduleMaintenanceOpenBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Deletes the custom records from Tax Schedule window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaxScheduleMaintenance_DeleteButtonTaxScheduleClickBeforeOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (Dynamics.Globals.CompanyId.Value == 2)
                {
                    if (!string.IsNullOrEmpty(taxScheduleMaintenanceForm.TaxScheduleMaintenance.TaxScheduleId.Value))
                    {
                        SetupRequest taxScheduleSetupRequest = new SetupRequest();
                        SetupEntity setupDetailEntity = new SetupEntity();
                        TaxSetupInformation setupInformation = new TaxSetupInformation();
                        setupInformation.TaxScheduleId = taxScheduleMaintenanceForm.TaxScheduleMaintenance.TaxScheduleId.Value.ToString();
                        setupDetailEntity.SetupDetails = setupInformation;
                        taxScheduleSetupRequest.SetupEntity = setupDetailEntity;

                        if (taxScheduleSetupRequest != null)
                        {
                            // Service call ...
                            using (HttpClient client = new HttpClient())
                            {
                                client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                var response = client.PostAsJsonAsync("api/SetupExtUpdate/DeleteTaxScheduleCustomRecord", taxScheduleSetupRequest);
                                if (response.Result.IsSuccessStatusCode)
                                {
                                    SetupResponse setupResponse = response.Result.Content.ReadAsAsync<SetupResponse>().Result;
                                    if (setupResponse.Status == ResponseStatus.Error)
                                    {
                                        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxScheduleMaintenance_DeleteButtonTaxScheduleClickBeforeOriginal Method (DeleteTaxScheduleCustomRecord): " + setupResponse.ErrorMessage.ToString());
                                        MessageBox.Show("Error: " + setupResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                    }
                                    else
                                    {
                                        // clears the details
                                        ClearTaxScheduleFormDetails();
                                    }
                                }
                                else
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Delete custom Tax Details Table");
                                    MessageBox.Show("Error: Delete custom Tax Details Table", Resources.STR_MESSAGE_TITLE);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxScheduleMaintenance_DeleteButtonTaxScheduleClickBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Event to capture the changes everytime the custom properties were changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaxScheduleMaintenance_CustomFieldValueChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (Dynamics.Globals.CompanyId.Value == 2 && taxScheduleMaintenanceForm.TaxScheduleMaintenance.TaxScheduleId.Value != "")
                {
                    taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Close();
                    taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Release();
                    taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.TaxScheduleId.Value = taxScheduleMaintenanceForm.TaxScheduleMaintenance.TaxScheduleId.Value.ToString().Trim();
                    taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Key = 1;
                    TableError tableError = taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Change();
                    if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.ChempointVat.Value = taxScheduleMaintenanceForm.TaxScheduleMaintenance.ChempointVat.Value.ToString().Trim();
                        taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Inactive.Value = taxScheduleMaintenanceForm.TaxScheduleMaintenance.Inactive.Value;
                        taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Save();
                    }
                    else
                    {
                        if (tableError == TableError.NotFound && tableError != TableError.EndOfTable)
                        {
                            taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.TaxScheduleId.Value = taxScheduleMaintenanceForm.TaxScheduleMaintenance.TaxScheduleId.Value.ToString().Trim();
                            taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.ChempointVat.Value = taxScheduleMaintenanceForm.TaxScheduleMaintenance.ChempointVat.Value.ToString().Trim();
                            taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Inactive.Value = taxScheduleMaintenanceForm.TaxScheduleMaintenance.Inactive.Value;
                            taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Save();
                        }
                    }
                    taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Release();
                    taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Close();

                    taxScheduleMaintenanceForm.TaxScheduleMaintenance.IsChanged = true;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxScheduleMaintenance_CustomFieldValueChange Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Even to clear or load the custom fields upon tax schedule id change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TaxScheduleMaintenance_DisplayExistingTaxScheduleRecordChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (Dynamics.Globals.CompanyId.Value == 2)
                {
                    // clears the existing values
                    ClearTaxScheduleFormDetails();

                    if (!string.IsNullOrEmpty(taxScheduleMaintenanceForm.TaxScheduleMaintenance.TaxScheduleId.Value))
                    {
                        // Loads the custom fields for TaxScheduleId
                        SetupRequest taxScheduleSetupRequest = new SetupRequest();
                        SetupEntity taxScheduledMaintenanceEntity = new SetupEntity();
                        TaxSetupInformation setupInformation = new TaxSetupInformation();

                        setupInformation.TaxScheduleId = taxScheduleMaintenanceForm.TaxScheduleMaintenance.TaxScheduleId.Value.ToString();
                        taxScheduledMaintenanceEntity.SetupDetails = setupInformation;
                        taxScheduleSetupRequest.SetupEntity = taxScheduledMaintenanceEntity;

                        // service call to get the details from DB
                        InsertTaxScheduleDetails(taxScheduleSetupRequest);

                        DisplayTaxScheduleDetails(taxScheduleSetupRequest.SetupEntity.SetupDetails.TaxScheduleId);
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxScheduleMaintenance_DisplayExistingTaxScheduleRecordChange Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Event to saves the custom details to DB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TaxScheduleMaintenance_SaveRecordTaxScheduleDetails(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (Dynamics.Globals.CompanyId.Value == 2 && !string.IsNullOrEmpty(taxScheduleMaintenanceForm.TaxScheduleMaintenance.TaxScheduleId.Value))
                {
                    SetupRequest taxScheduleSetupRequest = new SetupRequest();
                    SetupEntity taxScheduledMaintenanceEntity = new SetupEntity();
                    TaxSetupInformation setupInformation = new TaxSetupInformation();

                    setupInformation.TaxScheduleId = taxScheduleMaintenanceForm.TaxScheduleMaintenance.TaxScheduleId.Value.ToString().Trim();
                    setupInformation.TaxScheduleChempointVatNumber = taxScheduleMaintenanceForm.TaxScheduleMaintenance.ChempointVat.Value.ToString().Trim();
                    setupInformation.TaxScheduleIsActive = taxScheduleMaintenanceForm.TaxScheduleMaintenance.Inactive.Value;
                    taxScheduledMaintenanceEntity.SetupDetails = setupInformation;
                    taxScheduleSetupRequest.CompanyID = Dynamics.Globals.CompanyId.Value;
                    taxScheduleSetupRequest.UserId = Dynamics.Globals.UserId.Value.ToString();

                    taxScheduleSetupRequest.SetupEntity = taxScheduledMaintenanceEntity;

                    if (taxScheduleSetupRequest != null)
                    {
                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();

                            // Add an Accept header for JSON format.
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/SetupExtUpdate/SaveTaxScheduleCustomRecord", taxScheduleSetupRequest);
                            if (response.Result.IsSuccessStatusCode)
                            {
                                SetupResponse setupResponse = response.Result.Content.ReadAsAsync<SetupResponse>().Result;
                                if (setupResponse.Status == ResponseStatus.Error)
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxScheduleMaintenance_SaveRecordTaxScheduleDetails Method (SaveTaxScheduleCustomRecord): " + setupResponse.ErrorMessage.ToString());
                                    MessageBox.Show("Error: " + setupResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    // clears the details
                                    ClearTaxScheduleFormDetails();
                                }
                            }
                            else
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not saved into custom Tax Schedule Table");
                                MessageBox.Show("Error: Data does not saved into custom Tax Schedule Table", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In TaxScheduleMaintenance_SaveRecordTaxScheduleDetails Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Fetches the data from dB
        /// </summary>
        /// <param name="taxDetailsSetupRequest"></param>
        private void InsertTaxScheduleDetails(SetupRequest taxDetailsSetupRequest)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (taxDetailsSetupRequest != null)
                {
                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();

                        // Add an Accept header for JSON format.
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SetupExtUpdate/FetchTaxScheduleCustomRecord", taxDetailsSetupRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            SetupResponse setupResponse = response.Result.Content.ReadAsAsync<SetupResponse>().Result;
                            if (setupResponse.Status == ResponseStatus.Error)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In FetchTaxScheduleCustomFeildValues Method (FetchTaxScheduleCustomRecord): " + setupResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + setupResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                if (setupResponse.SetupDetailsEntity != null)
                                {
                                    taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Close();
                                    taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Release();
                                    taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.TaxScheduleId.Value = taxScheduleMaintenanceForm.TaxScheduleMaintenance.TaxScheduleId.Value;
                                    taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Key = 1;
                                    TableError tableError = taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Change();
                                    if (tableError == TableError.NotFound && tableError != TableError.EndOfTable)
                                    {
                                        taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.TaxScheduleId.Value = taxScheduleMaintenanceForm.TaxScheduleMaintenance.TaxScheduleId.Value.ToString().Trim();
                                        taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.ChempointVat.Value = setupResponse.SetupDetailsEntity.SetupDetails.TaxScheduleChempointVatNumber.ToString().Trim();
                                        taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Inactive.Value = setupResponse.SetupDetailsEntity.SetupDetails.TaxScheduleIsActive;
                                        taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Save();
                                    }
                                    taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Release();
                                    taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Close();
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Unable to fetch Tax schedule customer details from DB.e");
                            MessageBox.Show("Error: Unable to fetch Tax schedule customer details from DB.", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In FetchTaxScheduleCustomFeildValues Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Loads the data from temp table to form
        /// </summary>
        /// <param name="formName"></param>
        /// <param name="TaxSchedulelId"></param>
        private void DisplayTaxScheduleDetails(string taxSchedulelId)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {

                taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Close();
                taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Release();
                taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.TaxScheduleId.Value = taxSchedulelId;
                taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Key = 1;
                TableError tableError = taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Get();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    taxScheduleMaintenanceForm.TaxScheduleMaintenance.ChempointVat.Value = taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.ChempointVat.Value.ToString().Trim();
                    taxScheduleMaintenanceForm.TaxScheduleMaintenance.Inactive.Value = taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Inactive.Value;
                }
                else
                {
                    taxScheduleMaintenanceForm.TaxScheduleMaintenance.ChempointVat.Clear();
                    taxScheduleMaintenanceForm.TaxScheduleMaintenance.Inactive.Clear();
                }
                taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Release();
                taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Close();

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In LoadTaxScheduleDetailsFromTempTable Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Clears the custom field on the Tax schedule form.
        /// </summary>
        private void ClearTaxScheduleFormDetails()
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                // clears the temp table
                taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Close();
                taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Release();
                taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.TaxScheduleId.Value = taxScheduleMaintenanceForm.TaxScheduleMaintenance.TaxScheduleId.Value.ToString().Trim();
                taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Key = 1;
                TableError tempErrorQuote = taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.ChangeFirst();
                while (tempErrorQuote == TableError.NoError && tempErrorQuote != TableError.EndOfTable)
                {
                    taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Remove();
                    tempErrorQuote = taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.ChangeNext();
                }
                taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Release();
                taxScheduleMaintenanceForm.Tables.TaxScheduleTemp.Close();

                // clears the form fields
                taxScheduleMaintenanceForm.TaxScheduleMaintenance.ChempointVat.Clear();
                taxScheduleMaintenanceForm.TaxScheduleMaintenance.Inactive.Clear();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearTaxScheduleFormDetails Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        #endregion  TaxScheduleMaintenance

        #region SyPaymentTerms

        void PaymentTermSetupOpenBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!registerPaymentTermWindowEvents)
                {
                    syPaymentTermsForm.SyPaymentTerms.ClearButton.ClickAfterOriginal += new EventHandler(SyPaymentTerms_ClearButton_ClickAfterOriginal);
                    syPaymentTermsForm.SyPaymentTerms.SaveRecord.ValidateAfterOriginal += SaveRecordPaymentTermDetails;
                    syPaymentTermsForm.SyPaymentTerms.DisplayExistingRecord.ValidateAfterOriginal += DisplayExistingPaymentTermRecordChange;
                    syPaymentTermsForm.SyPaymentTerms.OpenAfterOriginal += new EventHandler(SyPaymentTerms_OpenAfterOriginal);
                    syPaymentTermsForm.SyPaymentTerms.DueType.Change += new EventHandler(DueType_Change);
                    syPaymentTermsForm.SyPaymentTerms.CalculateButton.ClickAfterOriginal += new EventHandler(CalculateButton_ClickAfterOriginal);
                    syPaymentTermsForm.SyPaymentTermsExample.OkButton.ClickAfterOriginal += new EventHandler(SyPaymentTermsExample_OkButton_ClickAfterOriginal);
                    syPaymentTermsForm.SyPaymentTermsExample.RedisplayButton.ClickAfterOriginal += new EventHandler(SyPaymentTermsExample_RedisplayButton_ClickAfterOriginal);
                    syPaymentTermsForm.SyPaymentTerms.CpTermsIfYes.Change += new EventHandler(SyPaymentTermsCpTermsIfYesChange);
                    syPaymentTermsForm.SyPaymentTerms.CpTermsIfNo.Change += new EventHandler(SyPaymentTermsCpTermsIfNoChange);
                    syPaymentTermsForm.SyPaymentTerms.CpNested.Change += new EventHandler(SyPaymentTermsCpNestedChange);
                    syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Change += new EventHandler(SyPaymentTermsCpDateWithinChange);
                    syPaymentTermsForm.Procedures.LoadDroplistContents.Invoke();

                    registerPaymentTermWindowEvents = true;
                }
                syPaymentTermsForm.SyPaymentTerms.CpDueofMonths.Value = 1;
                syPaymentTermsForm.SyPaymentTerms.CpOrderPrePaymentPct.Value = 1;
                syPaymentTermsForm.SyPaymentTerms.CpTermsGracePeriod.Clear();
                syPaymentTermsForm.SyPaymentTerms.CpNested.Value = false;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PaymentTermSetupOpenBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        void SyPaymentTermsExample_OkButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            syPaymentTermsForm.SyPaymentTermsExample.Close();
        }

        /// <summary>
        /// Event to save the payment term details
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SaveRecordPaymentTermDetails(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Close();
                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Release();
                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.PaymentTermsId.Value = syPaymentTermsForm.SyPaymentTerms.PaymentTermsId.Value;
                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Key = 1;
                TableError tableErrors = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Change();
                if (tableErrors == TableError.NoError && tableErrors != TableError.NotFound)
                {
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpDueofMonths.Value = syPaymentTermsForm.SyPaymentTerms.CpDueofMonths.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpeomEnabled.Value = syPaymentTermsForm.SyPaymentTerms.CpeomEnabled.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpOrderPrePaymentPct.Value = syPaymentTermsForm.SyPaymentTerms.CpOrderPrePaymentPct.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsGracePeriod.Value = syPaymentTermsForm.SyPaymentTerms.CpTermsGracePeriod.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpNested.Value = syPaymentTermsForm.SyPaymentTerms.CpNested.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpDateWithin.Value = syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsIfYes.Value = syPaymentTermsForm.SyPaymentTerms.CpTermsIfYes.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsIfNo.Value = syPaymentTermsForm.SyPaymentTerms.CpTermsIfNo.Value;
                }
                else
                {
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpDueofMonths.Value = syPaymentTermsForm.SyPaymentTerms.CpDueofMonths.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpeomEnabled.Value = syPaymentTermsForm.SyPaymentTerms.CpeomEnabled.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpOrderPrePaymentPct.Value = syPaymentTermsForm.SyPaymentTerms.CpOrderPrePaymentPct.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsGracePeriod.Value = syPaymentTermsForm.SyPaymentTerms.CpTermsGracePeriod.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpNested.Value = syPaymentTermsForm.SyPaymentTerms.CpNested.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpDateWithin.Value = syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsIfYes.Value = syPaymentTermsForm.SyPaymentTerms.CpTermsIfYes.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsIfNo.Value = syPaymentTermsForm.SyPaymentTerms.CpTermsIfNo.Value;
                }
                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Save();
                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Release();
                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Close();

                SetupRequest paymentTermSetupRequest = new SetupRequest();
                SetupEntity setupDetailEntity = new SetupEntity();
                PaymentTermsInformation pterms = new PaymentTermsInformation();
                AuditInformation auditInformation = new AuditInformation();
                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Close();
                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Release();
                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.PaymentTermsId.Value = syPaymentTermsForm.SyPaymentTerms.PaymentTermsId.Value;
                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Key = 1;
                TableError tableError = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Get();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    pterms.PaymentTermsID = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.PaymentTermsId.Value.ToString();
                    pterms.DueOfMonths = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpDueofMonths.Value.ToString().Trim();
                    pterms.EomEnabled = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpeomEnabled ? 1 : 0;
                    pterms.OrderPrePaymentPct = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpOrderPrePaymentPct.Value.ToString().Trim();
                    pterms.TermsGracePeriod = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsGracePeriod.Value;
                    pterms.Nested = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpNested.Value;
                    pterms.DateWithin = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpDateWithin.Value;
                    pterms.TermsIfYes = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsIfYes.Value.ToString().Trim();
                    pterms.TermsIfNo = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsIfNo.Value.ToString().Trim();
                }
                setupDetailEntity.PaymentTermsDetails = pterms;

                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Release();
                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Close();

                paymentTermSetupRequest.CompanyID = Dynamics.Globals.CompanyId.Value;
                auditInformation.CreatedBy = Dynamics.Globals.UserId.Value.ToString().Trim();
                auditInformation.CreatedDate = Dynamics.Globals.UserDate;
                auditInformation.ModifiedBy = Dynamics.Globals.UserId.Value.ToString().Trim();
                auditInformation.ModifiedDate = Dynamics.Globals.UserDate;
                setupDetailEntity.AuditInformation = auditInformation;
                paymentTermSetupRequest.SetupEntity = setupDetailEntity;
                if (paymentTermSetupRequest != null)
                {
                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SetupExtUpdate/SavePaymentTermCustomRecord", paymentTermSetupRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            SetupResponse setupResponse = response.Result.Content.ReadAsAsync<SetupResponse>().Result;
                            if (setupResponse.Status == ResponseStatus.Error)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SaveRecordPaymentTermDetails Method (SavePaymentTermCustomRecord): " + setupResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + setupResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not saved into custom Tax Details Table");
                            MessageBox.Show("Error: Data does not saved into custom Tax Details Table", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SaveRecordPaymentTermDetails Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        private void CpDateWithin()
        {
            StringBuilder logMessage = new StringBuilder();

            try
            {
                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Close();
                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Release();
                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.PaymentTermsId.Value = syPaymentTermsForm.SyPaymentTerms.PaymentTermsId.Value;
                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Key = 1;
                TableError tableError = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.NotFound)
                {
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpDueofMonths.Value = syPaymentTermsForm.SyPaymentTerms.CpDueofMonths.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpeomEnabled.Value = syPaymentTermsForm.SyPaymentTerms.CpeomEnabled.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpOrderPrePaymentPct.Value = syPaymentTermsForm.SyPaymentTerms.CpOrderPrePaymentPct.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsGracePeriod.Value = syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpNested.Value = syPaymentTermsForm.SyPaymentTerms.CpNested.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpDateWithin.Value = syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsIfYes.Value = syPaymentTermsForm.SyPaymentTerms.CpTermsIfYes.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsIfNo.Value = syPaymentTermsForm.SyPaymentTerms.CpTermsIfNo.Value;
                }
                else
                {
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpDueofMonths.Value = syPaymentTermsForm.SyPaymentTerms.CpDueofMonths.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpeomEnabled.Value = syPaymentTermsForm.SyPaymentTerms.CpeomEnabled.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpOrderPrePaymentPct.Value = syPaymentTermsForm.SyPaymentTerms.CpOrderPrePaymentPct.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsGracePeriod.Value = syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpNested.Value = syPaymentTermsForm.SyPaymentTerms.CpNested.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpDateWithin.Value = syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsIfYes.Value = syPaymentTermsForm.SyPaymentTerms.CpTermsIfYes.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsIfNo.Value = syPaymentTermsForm.SyPaymentTerms.CpTermsIfNo.Value;
                }
                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Save();
                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Release();
                syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CpDateWithin Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        private void FillTempTablePaymentTermDetails(SetupResponse paymentTermSetupResponse)
        {
            StringBuilder logMessage = new StringBuilder();

            try
            {
                if (paymentTermSetupResponse.SetupDetailsEntity != null)
                {
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Close();
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Release();
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.PaymentTermsId.Value = syPaymentTermsForm.SyPaymentTerms.PaymentTermsId.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Key = 1;
                    TableError tableError = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Change();
                    if (tableError == TableError.NoError || tableError == TableError.NotFound)
                    {
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpDueofMonths.Value = Convert.ToInt16(paymentTermSetupResponse.SetupDetailsEntity.PaymentTermsDetails.DueOfMonths);
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpeomEnabled.Value = Convert.ToBoolean(paymentTermSetupResponse.SetupDetailsEntity.PaymentTermsDetails.EomEnabled);
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpOrderPrePaymentPct.Value = Convert.ToInt16(paymentTermSetupResponse.SetupDetailsEntity.PaymentTermsDetails.OrderPrePaymentPct);
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsGracePeriod.Value = Convert.ToInt16(paymentTermSetupResponse.SetupDetailsEntity.PaymentTermsDetails.TermsGracePeriod);
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpNested.Value = paymentTermSetupResponse.SetupDetailsEntity.PaymentTermsDetails.Nested;
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpDateWithin.Value = Convert.ToInt16(paymentTermSetupResponse.SetupDetailsEntity.PaymentTermsDetails.DateWithin);
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsIfYes.Value = paymentTermSetupResponse.SetupDetailsEntity.PaymentTermsDetails.TermsIfYes;
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsIfNo.Value = paymentTermSetupResponse.SetupDetailsEntity.PaymentTermsDetails.TermsIfNo;
                    }
                    else
                    {
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpDueofMonths.Value = syPaymentTermsForm.SyPaymentTerms.CpDueofMonths.Value;
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpeomEnabled.Value = syPaymentTermsForm.SyPaymentTerms.CpeomEnabled.Value;
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpOrderPrePaymentPct.Value = syPaymentTermsForm.SyPaymentTerms.CpOrderPrePaymentPct.Value;
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsGracePeriod.Value = syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Value;
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpNested.Value = syPaymentTermsForm.SyPaymentTerms.CpNested.Value;
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpDateWithin.Value = syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Value;
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsIfYes.Value = syPaymentTermsForm.SyPaymentTerms.CpTermsIfYes.Value;
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsIfNo.Value = syPaymentTermsForm.SyPaymentTerms.CpTermsIfNo.Value;
                    }
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Save();
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Release();
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Close();
                }
                else
                {
                    syPaymentTermsForm.SyPaymentTerms.CpDueofMonths.Value = 0;
                    syPaymentTermsForm.SyPaymentTerms.CpeomEnabled.Value = false;
                    syPaymentTermsForm.SyPaymentTerms.CpOrderPrePaymentPct.Value = 0;
                    syPaymentTermsForm.SyPaymentTerms.CpNested.Value = false;
                    syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Clear();
                    syPaymentTermsForm.SyPaymentTerms.CpTermsIfYes.Clear();
                    syPaymentTermsForm.SyPaymentTerms.CpTermsIfNo.Clear();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In FillTempTablePaymentTermDetails Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        private void InsertPaymentTermDetails(SetupRequest paymentTermSetupRequest)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                paymentTermSetupRequest.CompanyID = Dynamics.Globals.CompanyId.Value;
                if (paymentTermSetupRequest != null)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SetupExtUpdate/GetPaymentTermCustomRecord", paymentTermSetupRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            SetupResponse setupResponse = response.Result.Content.ReadAsAsync<SetupResponse>().Result;
                            if (setupResponse.Status == ResponseStatus.Error)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertPaymentTermDetails Method (GetPaymentTermCustomRecord): " + setupResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + setupResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                FillTempTablePaymentTermDetails(setupResponse);
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not fetch from custom sales details Table");
                            MessageBox.Show("Error: Data does not fetch from custom sales details Table", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertPaymentTermDetails Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        private void DisplayPaymentTermDetails(string formName)
        {
            StringBuilder logMessage = new StringBuilder();

            try
            {
                if (formName == Resources.STR_SyPaymentTermsSetupForm)
                {
                    if (String.IsNullOrEmpty(syPaymentTermsForm.SyPaymentTerms.PaymentTermsId.Value))
                    {
                        ClearPaymentTermDetails(formName);   // pass blank Sop Type, Sop Number, Item number and line seq number to clear all data
                    }
                    else
                    {
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Close();
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Release();
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.PaymentTermsId.Value = syPaymentTermsForm.SyPaymentTerms.PaymentTermsId.Value;
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Key = 1;
                        TableError tableError = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Get();
                        if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                        {
                            syPaymentTermsForm.SyPaymentTerms.CpDueofMonths.Value = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpDueofMonths.Value;
                            syPaymentTermsForm.SyPaymentTerms.CpeomEnabled.Value = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpeomEnabled.Value;
                            if (Convert.ToInt32(syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpeomEnabled.Value) == 1)
                            {
                                syPaymentTermsForm.SyPaymentTerms.CpeomEnabled.Value = true;
                            }
                            else
                            {
                                syPaymentTermsForm.SyPaymentTerms.CpeomEnabled.Value = false;
                            }
                            if (Convert.ToInt32(syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpNested.Value) == 1)
                            {
                                syPaymentTermsForm.SyPaymentTerms.CpNested.Value = true;
                            }
                            else
                            {
                                syPaymentTermsForm.SyPaymentTerms.CpNested.Value = false;
                            }
                            syPaymentTermsForm.SyPaymentTerms.CpOrderPrePaymentPct.Value = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpOrderPrePaymentPct.Value;
                            syPaymentTermsForm.SyPaymentTerms.CpTermsIfYes.Value = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsIfYes.Value.ToString();
                            syPaymentTermsForm.SyPaymentTerms.CpTermsGracePeriod.Value = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsGracePeriod.Value;
                            syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Value = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpDateWithin.Value;
                            syPaymentTermsForm.SyPaymentTerms.CpTermsIfNo.Value = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsIfNo.Value.ToString();
                        }
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Release();
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In DisplayPaymentTermDetails Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        void DisplayExistingPaymentTermRecordChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();

            try
            {
                string formName = Resources.STR_SyPaymentTermsSetupForm;
                ClearPaymentTermDetails(formName);
                SetupRequest paymentTermSetupRequest = new SetupRequest();
                SetupEntity paymentTermSetupEntity = new SetupEntity();

                PaymentTermsInformation pTerms = new PaymentTermsInformation();

                pTerms.PaymentTermsID = syPaymentTermsForm.SyPaymentTerms.PaymentTermsId.Value.ToString();
                paymentTermSetupEntity.PaymentTermsDetails = pTerms;

                paymentTermSetupRequest.SetupEntity = paymentTermSetupEntity;
                InsertPaymentTermDetails(paymentTermSetupRequest);
                DisplayPaymentTermDetails(formName);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In DisplayExistingPaymentTermRecordChange Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        private void ClearPaymentTermDetails(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_SyPaymentTermsSetupForm)
                {
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Close();
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Release();
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.PaymentTermsId.Value = syPaymentTermsForm.SyPaymentTerms.PaymentTermsId.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Key = 1;
                    TableError tempErrorQuote = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Change();
                    while (tempErrorQuote == TableError.NoError && tempErrorQuote != TableError.EndOfTable)
                    {
                        syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Remove();
                        tempErrorQuote = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.ChangeNext();
                    }
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Release();
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Close();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearPaymentTermDetails Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        void DueType_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();

            //Clear details
            try
            {
                if (syPaymentTermsForm.SyPaymentTerms.DueType.Value == 1 ||
                    syPaymentTermsForm.SyPaymentTerms.DueType.Value == 3 ||
                    syPaymentTermsForm.SyPaymentTerms.DueType.Value == 4 ||
                    syPaymentTermsForm.SyPaymentTerms.DueType.Value == 5)
                {
                    syPaymentTermsForm.SyPaymentTerms.CpeomEnabled.Disable();
                    syPaymentTermsForm.SyPaymentTerms.CpeomEnabled.Value = false;
                }
                else
                {
                    syPaymentTermsForm.SyPaymentTerms.CpeomEnabled.Enable();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In DueType_Change Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Calculate Due Date when calculate button hits
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CalculateButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (syPaymentTermsForm.SyPaymentTermsExample.IsOpen)  // check example form is open
                {
                    syPaymentTermsForm.SyPaymentTermsExample.RedisplayButton.RunValidate();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CalculateButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Calculate Due Date when re-display button hits..
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SyPaymentTermsExample_RedisplayButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                string paymentTerms;
                DateTime docDate;
                DateTime dueDate = new DateTime();
                if (syPaymentTermsForm.SyPaymentTermsExample.LocalPaymentTerms.Value != string.Empty &&
                    !String.IsNullOrEmpty(syPaymentTermsForm.SyPaymentTermsExample.LocalTransactionDate.Value.ToString()))
                {
                    paymentTerms = syPaymentTermsForm.SyPaymentTermsExample.LocalPaymentTerms.Value.ToString();
                    docDate = syPaymentTermsForm.SyPaymentTermsExample.LocalTransactionDate.Value;
                    dueDate = CalculateDueDateByPaymentTerm(paymentTerms, docDate);
                    syPaymentTermsForm.SyPaymentTermsExample.LocalDueDate.Value = dueDate;
                    syPaymentTermsForm.SyPaymentTermsExample.LocalDueDate.RunValidate();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SyPaymentTermsExample_RedisplayButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Method called on selecting and deselecting the nested check box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SyPaymentTermsCpNestedChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (syPaymentTermsForm.SyPaymentTerms.CpNested.Value == true)
                {
                    syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Enable();
                    syPaymentTermsForm.SyPaymentTerms.CpTermsIfYes.Enable();
                    syPaymentTermsForm.SyPaymentTerms.LookupButton2.Enable();
                    syPaymentTermsForm.SyPaymentTerms.CpTermsIfNo.Enable();
                    syPaymentTermsForm.SyPaymentTerms.LookupButton3.Enable();
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Close();
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Release();
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.PaymentTermsId.Value = syPaymentTermsForm.SyPaymentTerms.PaymentTermsId.Value;
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Key = 1;
                    TableError error = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Get();
                    if (error == TableError.NoError)
                    {
                        if (syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpDateWithin.Value == 0)
                        {
                            syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Value = 1;
                        }
                        else
                        {
                            syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Value = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpDateWithin.Value;
                        }
                        syPaymentTermsForm.SyPaymentTerms.CpTermsIfYes.Value = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsIfYes.Value;
                        syPaymentTermsForm.SyPaymentTerms.CpTermsIfNo.Value = syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.CpTermsIfNo.Value;
                    }
                    else if (error == TableError.NotFound)
                    {
                        syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Value = 1;
                    }
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Release();
                    syPaymentTermsForm.Tables.CpPaymentTermsSetupTemp.Close();
                }
                else
                {
                    syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Value = 0;
                    syPaymentTermsForm.SyPaymentTerms.CpTermsIfYes.Clear();
                    syPaymentTermsForm.SyPaymentTerms.CpTermsIfNo.Clear();
                    syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Disable();
                    syPaymentTermsForm.SyPaymentTerms.CpTermsIfYes.Disable();
                    syPaymentTermsForm.SyPaymentTerms.LookupButton2.Disable();
                    syPaymentTermsForm.SyPaymentTerms.CpTermsIfNo.Disable();
                    syPaymentTermsForm.SyPaymentTerms.LookupButton3.Disable();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SyPaymentTermsCpNestedChange Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Method to validate Terms if Yes field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SyPaymentTermsCpTermsIfYesChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (syPaymentTermsForm.SyPaymentTerms.CpNested.Value == true &&
                    syPaymentTermsForm.SyPaymentTerms.CpTermsIfYes.Value != string.Empty)
                {
                    if (syPaymentTermsForm.SyPaymentTerms.PaymentTermsId.Value != syPaymentTermsForm.SyPaymentTerms.CpTermsIfYes.Value)
                    {
                        ChemPointSalesExt.Tables.SyPaymentTermsMstr.Close();
                        ChemPointSalesExt.Tables.SyPaymentTermsMstr.Release();
                        ChemPointSalesExt.Tables.SyPaymentTermsMstr.PaymentTermsId.Value = syPaymentTermsForm.SyPaymentTerms.CpTermsIfYes.Value;
                        ChemPointSalesExt.Tables.SyPaymentTermsMstr.Key = 1;
                        TableError error = ChemPointSalesExt.Tables.SyPaymentTermsMstr.Get();
                        if (error == TableError.NotFound)
                        {
                            syPaymentTermsForm.SyPaymentTerms.CpTermsIfYes.Clear();
                            syPaymentTermsForm.SyPaymentTerms.CpTermsIfYes.Focus();
                        }
                        ChemPointSalesExt.Tables.SyPaymentTermsMstr.Release();
                        ChemPointSalesExt.Tables.SyPaymentTermsMstr.Close();
                    }
                    else
                    {
                        syPaymentTermsForm.SyPaymentTerms.CpTermsIfYes.Clear();
                        syPaymentTermsForm.SyPaymentTerms.CpTermsIfYes.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SyPaymentTermsCpTermsIfYesChange Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Method to validate Terms if No field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SyPaymentTermsCpTermsIfNoChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (syPaymentTermsForm.SyPaymentTerms.CpNested.Value == true &&
                    syPaymentTermsForm.SyPaymentTerms.CpTermsIfNo.Value != string.Empty)
                {
                    if (syPaymentTermsForm.SyPaymentTerms.PaymentTermsId.Value != syPaymentTermsForm.SyPaymentTerms.CpTermsIfNo.Value)
                    {
                        ChemPointSalesExt.Tables.SyPaymentTermsMstr.Close();
                        ChemPointSalesExt.Tables.SyPaymentTermsMstr.Release();
                        ChemPointSalesExt.Tables.SyPaymentTermsMstr.PaymentTermsId.Value = syPaymentTermsForm.SyPaymentTerms.CpTermsIfNo.Value;
                        ChemPointSalesExt.Tables.SyPaymentTermsMstr.Key = 1;
                        TableError error = ChemPointSalesExt.Tables.SyPaymentTermsMstr.Get();
                        if (error == TableError.NotFound)
                        {
                            syPaymentTermsForm.SyPaymentTerms.CpTermsIfNo.Clear();
                            syPaymentTermsForm.SyPaymentTerms.CpTermsIfNo.Focus();
                        }
                        ChemPointSalesExt.Tables.SyPaymentTermsMstr.Release();
                        ChemPointSalesExt.Tables.SyPaymentTermsMstr.Close();
                    }
                    else
                    {
                        syPaymentTermsForm.SyPaymentTerms.CpTermsIfNo.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SyPaymentTermsCpTermsIfNoChange Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Method to validate date with in value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SyPaymentTermsCpDateWithinChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if ((syPaymentTermsForm.SyPaymentTerms.CpNested.Value == true) &&
                    (syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Value > 31 || syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Value < 1))
                {
                    syPaymentTermsForm.SyPaymentTerms.CpDateWithin.Focus();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SyPaymentTermsCpDateWithinChange Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// To Clear the Payment Terms Setup Screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SyPaymentTerms_ClearButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                syPaymentTermsForm.SyPaymentTerms.CpDueofMonths.Value = 1;
                syPaymentTermsForm.SyPaymentTerms.CpOrderPrePaymentPct.Value = 1;
                syPaymentTermsForm.SyPaymentTerms.CpTermsGracePeriod.Clear();
                syPaymentTermsForm.SyPaymentTerms.CpNested.Value = false;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SyPaymentTerms_ClearButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// To fill the Drop down list contents in Payment Terms Setup window for the New fields.  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SyPaymentTerms_OpenAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                syPaymentTermsForm.Procedures.LoadDroplistContents.Invoke();
                syPaymentTermsForm.SyPaymentTerms.CpNested.Value = false;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SyPaymentTerms_OpenAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
        }

        private DateTime CalculateDueDateByPaymentTerm(string paymentTerms, DateTime docDate)
        {
            StringBuilder logMessage = new StringBuilder();
            DateTime dueDate = new DateTime();

            try
            {
                if (syPaymentTermsForm.SyPaymentTermsExample.LocalPaymentTerms.Value != string.Empty &&
                    !String.IsNullOrEmpty(syPaymentTermsForm.SyPaymentTermsExample.LocalTransactionDate.Value.ToString()))
                {
                    paymentTerms = syPaymentTermsForm.SyPaymentTermsExample.LocalPaymentTerms.Value.ToString();
                    docDate = syPaymentTermsForm.SyPaymentTermsExample.LocalTransactionDate.Value;

                    syPaymentTermsForm.SyPaymentTermsExample.LocalDueDate.RunValidate();
                }

                SetupRequest paymentTermRequest = new SetupRequest();
                SetupEntity paymentTermEntity = new SetupEntity();

                PaymentTermsInformation pTerms = new PaymentTermsInformation();
                pTerms.PaymentTermsID = syPaymentTermsForm.SyPaymentTerms.PaymentTermsId.Value.ToString();
                pTerms.DocDate = docDate;

                paymentTermEntity.PaymentTermsDetails = pTerms;
                paymentTermRequest.SetupEntity = paymentTermEntity;
                paymentTermRequest.CompanyID = Dynamics.Globals.CompanyId.Value;

                if (paymentTermRequest != null)
                {
                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SetupExtUpdate/GetCalulatedDueDateByPaymentTerm", paymentTermRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            SetupResponse setupResponse = response.Result.Content.ReadAsAsync<SetupResponse>().Result;
                            if (setupResponse.Status == ResponseStatus.Error)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CalculateDueDateByPaymentTerm Method (GetCalulatedDueDateByPaymentTerm): " + setupResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + setupResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                dueDate = setupResponse.SetupDetailsEntity.PaymentTermsDetails.Duedate;
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Due date does not calculate from custom values");
                            MessageBox.Show("Error: Due date does not calculate from custom values", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CalculateDueDateByPaymentTerm Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "TaxDetails");
                logMessage = null;
            }
            return dueDate;
        }

        #endregion SyPaymentTerms

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
                case "TaxDetails":
                    if (isSetupLoggingEnabled && message != "")
                        new TextLogger().LogInformationIntoFile(message, setupLogFilePath, setupLogFileName);
                    break;
                default:
                    break;
            }
        }
    }
}