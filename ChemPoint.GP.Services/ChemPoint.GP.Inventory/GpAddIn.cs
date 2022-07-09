using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Dexterity.Bridge;
using Microsoft.Dexterity.Applications;
using Dic1311 = Microsoft.Dexterity.Applications.ChempointCustomizationsDictionary;
using System.Net.Http.Headers;
using System.Net.Http;
using Chempoint.GP.Model.Interactions.Inventory;
using System.Runtime.InteropServices;
using System.IO;
using ChemPoint.GP.Entities.Business_Entities.Inventory;
using ChemPoint.GP.Inventory.Properties;
using ChemPoint.GP.Entities.BaseEntities;
using System.Data;
using Chempoint.GP.Infrastructure.Logging;
using System.Text;
using System.Reflection;
using System.Linq;

namespace ChemPoint.GP.Inventory
{
    /// <PO>
    /// Project Name        :   GP Service 
    /// Affected Module     :   Inventory
    /// Affected Windows    :   
    /// Developed on        :   2016Aug22  
    /// Developed by        :   Nagaraj ,Muthu and Amit.
    ///-------------------------------------------------------------------------------------------
    ///     Date		Author          Description
    ///-------------------------------------------------------------------------------------------
    /// **Apr2017       Mthangaraj      Dll Upgrade Regarding GP 2016
    /// </PO>
    public class GPAddIn : IDexterityAddIn
    {
        static Dic1311.IvRpItemSiteMntForm ivRpItemSiteMntForm;
        static Dic1311.IvRpItemSiteInquiryForm ivRpItemSiteMntInquiryForm;
        Boolean registerIvrpMgmt = false;
        Boolean registerIvrpMgmtInquiry = false;

        static string gpServiceConfigurationUrl = null;
        bool isInventoryLoggingEnabled = false;
        string inventoryLogFileName = null;
        string inventoryLogFilePath = null;

        [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileStringW",
            SetLastError = true,
            CharSet = CharSet.Unicode, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, string lpReturnString, int nSize, string lpFilename);

        public void Initialize()
        {
            //Item Resource planning form initialization 
            ivRpItemSiteMntForm = ChempointCustomizations.Forms.IvRpItemSiteMnt;
            ivRpItemSiteMntInquiryForm = ChempointCustomizations.Forms.IvRpItemSiteInquiry;

            ivRpItemSiteMntForm.IvRpItemSiteMnt.OpenBeforeOriginal += new System.ComponentModel.CancelEventHandler(IvRpItemSiteMnt_OpenBeforeOriginal);
            ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.OpenBeforeOriginal += new System.ComponentModel.CancelEventHandler(IvRpItemSiteInquiry_OpenBeforeOriginal);

            string getCurrentDirectoryPath = @Directory.GetCurrentDirectory();
            string iniFilePath = getCurrentDirectoryPath + "\\Data\\Dex.ini";

            List<string> categories = GetCategories(iniFilePath);
            foreach (string category in categories)
            {
                //Get the key values
                gpServiceConfigurationUrl = GetIniFileString(iniFilePath, category, "GPSERVICE", "");
                isInventoryLoggingEnabled = Convert.ToBoolean(GetIniFileString(iniFilePath, category, "ISINVENTORYLOGENABLED", ""));
                inventoryLogFileName = GetIniFileString(iniFilePath, category, "INVENTORYLOGFILENAME", "");
                inventoryLogFilePath = GetIniFileString(iniFilePath, category, "INVENTORYLOGFILEPATH", "");
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

        #region ItemResourcePlanning

        private void IvRpItemSiteMnt_OpenBeforeOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (registerIvrpMgmt == false)
                {
                    //Unlock the commandIndicator field...
                    ivRpItemSiteMntForm.IvRpItemSiteMnt.LocationCode.Change += new EventHandler(IvRpItemSiteMntLocationCode_Change);
                    //Demand Indicator Change Event...
                    ivRpItemSiteMntForm.IvRpItemSiteMnt.DiRetrieveFlag.Change += new EventHandler(IvRpItemSiteMntDiRetrieveFlag_Change);
                    ivRpItemSiteMntForm.IvRpItemSiteMnt.DemandIndicator.Change += new EventHandler(IvRpItemSiteMntDemandIndicator_Change);
                    //Dem
                    ivRpItemSiteMntForm.IvRpItemSiteMnt.DiSaveFlag.Change += new EventHandler(IvRpItemSiteMntDiSaveFlag_Change);

                    registerIvrpMgmt = true;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In IvRpItemSiteMnt_OpenBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "InventoryDetails");
                logMessage = null;
            }
        }

        void IvRpItemSiteInquiry_OpenBeforeOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (registerIvrpMgmtInquiry == false)
                {
                    ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.LocationCode.Change += new EventHandler(IvRpItemSiteInquiryLocationCode_Change);
                    ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.DiInquiryRetrieveFlag.Change += new EventHandler(IvRpItemSiteInquiryDiInquiryRetrieveFlag_Change);
                    registerIvrpMgmtInquiry = true;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In IvRpItemSiteInquiry_OpenBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "InventoryDetails");
                logMessage = null;
            }

        }
        void IvRpItemSiteMntDiRetrieveFlag_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if ((String.IsNullOrEmpty(ivRpItemSiteMntForm.IvRpItemSiteMnt.ItemNumber.Value)) || (String.IsNullOrEmpty(ivRpItemSiteMntForm.IvRpItemSiteMnt.LocationCode.Value)))
                {
                    ivRpItemSiteMntForm.IvRpItemSiteMnt.DemandIndicator.Lock();
                }
                else
                {
                    ivRpItemSiteMntForm.IvRpItemSiteMnt.DemandIndicator.Unlock();
                    DisplayDemandIndicator(Resources.STR_RPEntryForm);
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In DiRetrieveFlag_Change Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "InventoryDetails");
                logMessage = null;
            }
        }

        void IvRpItemSiteInquiryDiInquiryRetrieveFlag_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if ((String.IsNullOrEmpty(ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.ItemNumber.Value)) || (String.IsNullOrEmpty(ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.LocationCode.Value)))
                {
                    ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.DemandIndicator.Lock();
                }
                else
                {
                    ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.DemandIndicator.Unlock();
                    DisplayDemandIndicator(Resources.STR_RPInquiryForm);
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In DiInquiryRetrieveFlag_Change Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "InventoryDetails");
                logMessage = null;
            }
        }

        void IvRpItemSiteMntLocationCode_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (String.IsNullOrEmpty(ivRpItemSiteMntForm.IvRpItemSiteMnt.LocationCode.Value))
                {
                    ivRpItemSiteMntForm.IvRpItemSiteMnt.DemandIndicator.Lock();
                    ivRpItemSiteMntForm.IvRpItemSiteMnt.DemandIndicator.Clear();
                }
                else
                {
                    ivRpItemSiteMntForm.IvRpItemSiteMnt.DemandIndicator.Unlock();
                    DisplayDemandIndicator(Resources.STR_RPEntryForm);
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In LocationCode_Change Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "InventoryDetails");
                logMessage = null;
            }
        }

        void IvRpItemSiteInquiryLocationCode_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (String.IsNullOrEmpty(ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.LocationCode.Value))
                {
                    ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.DemandIndicator.Lock();
                    ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.DemandIndicator.Clear();
                }
                else
                {
                    ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.DemandIndicator.Unlock();
                    DisplayDemandIndicator(Resources.STR_RPInquiryForm);
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In LocationCodeInquiry_Change Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "InventoryDetails");
                logMessage = null;
            }
        }

        private void DisplayDemandIndicator(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                InventoryResourceRequest ivRequest = null;
                InventoryResourceEntity inventoryEntity = null;
                List<InventoryItemDemandEntity> demandIndicatorEntityStatus = null;

                ivRequest = new InventoryResourceRequest();
                ivRequest.CompanyID = Dynamics.Globals.CompanyId.Value;
                InventoryInformation inventoryBase = new InventoryInformation();

                if (formName == Resources.STR_RPEntryForm)
                {
                    inventoryBase.ItemNumber = ivRpItemSiteMntForm.IvRpItemSiteMnt.ItemNumber.Value.ToString().Trim();
                    inventoryBase.WarehouseId = ivRpItemSiteMntForm.IvRpItemSiteMnt.LocationCode.Value.ToString().Trim();
                    ivRequest.InventoryBase = inventoryBase;
                }
                if (formName == Resources.STR_RPInquiryForm)
                {
                    inventoryBase.ItemNumber = ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.ItemNumber.Value.ToString().Trim();
                    inventoryBase.WarehouseId = ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.LocationCode.Value.ToString().Trim();
                    ivRequest.InventoryBase = inventoryBase;
                }

                // Service call ...
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.PostAsJsonAsync("api/Inventory/GetItemresourceDetail", ivRequest); // we need to refer the web.api service url here.
                    if (response.Result.IsSuccessStatusCode)
                    {
                        InventoryResourceResponse inventoryResponse = response.Result.Content.ReadAsAsync<InventoryResourceResponse>().Result;
                        if (inventoryResponse.Status == ResponseStatus.Error)
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In DisplayDemandIndicator Method (GetItemresourceDetail): " + inventoryResponse.ErrorMessage.ToString());
                            MessageBox.Show("Error: " + inventoryResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                        }
                        else
                        {
                            inventoryEntity = new InventoryResourceEntity();
                            demandIndicatorEntityStatus = new List<InventoryItemDemandEntity>();
                            inventoryEntity = inventoryResponse.InventoryResourceEntity;
                            demandIndicatorEntityStatus = inventoryResponse.ItemDemandIndicatorList;
                        }
                    }
                    else
                    {
                        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not fetch from custom inventory details Table");
                        MessageBox.Show("Error: Data does not fetch from custom inventory details Table", Resources.STR_MESSAGE_TITLE);
                    }
                }

                if (formName == Resources.STR_RPEntryForm)
                {
                    ClearDemandIndicatorWindow(formName,
                                            ivRpItemSiteMntForm.IvRpItemSiteMnt.ItemNumber.Value.ToString().Trim(),
                                            ivRpItemSiteMntForm.IvRpItemSiteMnt.LocationCode.Value.ToString().Trim());

                    if (demandIndicatorEntityStatus != null)
                    {
                        short dataShort;
                        foreach (var item in demandIndicatorEntityStatus)
                        {
                            //To save records in holds 
                            ivRpItemSiteMntForm.Tables.DemandIndicatorStatusTemp.Release();
                            short.TryParse(item.ItemDemandIndicatorId.ToString(), out dataShort);
                            ivRpItemSiteMntForm.Tables.DemandIndicatorStatusTemp.DemandIndicatorId.Value = dataShort;
                            ivRpItemSiteMntForm.Tables.DemandIndicatorStatusTemp.DemandIndicatorStatus.Value = item.DemandIndicator.ToString();
                            ivRpItemSiteMntForm.Tables.DemandIndicatorStatusTemp.Save();
                        }
                        ivRpItemSiteMntForm.Tables.DemandIndicatorStatusTemp.Release();
                        ivRpItemSiteMntForm.Tables.DemandIndicatorStatusTemp.Close();
                        ChempointCustomizations.Procedures.DisplayDemandIndicator.Invoke();
                    }

                    if (inventoryEntity != null)
                    {
                        ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Close();
                        ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Release();
                        ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.LocationCode.Value = ivRpItemSiteMntForm.IvRpItemSiteMnt.LocationCode.Value;
                        ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.ItemNumber.Value = ivRpItemSiteMntForm.IvRpItemSiteMnt.ItemNumber.Value;
                        ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Key = 1;
                        TableError tableError = ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Change();
                        if (tableError == TableError.NoError)
                        {
                            ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.LocationCode.Value = ivRpItemSiteMntForm.IvRpItemSiteMnt.LocationCode.Value;
                            ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.ItemNumber.Value = ivRpItemSiteMntForm.IvRpItemSiteMnt.ItemNumber.Value;
                            ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.DemandIndicatorId.Value = Convert.ToInt16(inventoryEntity.ItemDemandIndicatorId);
                            ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Save();
                        }
                        else if (tableError == TableError.NotFound)
                        {
                            ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.LocationCode.Value = ivRpItemSiteMntForm.IvRpItemSiteMnt.LocationCode.Value;
                            ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.ItemNumber.Value = ivRpItemSiteMntForm.IvRpItemSiteMnt.ItemNumber.Value;
                            ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Save();
                        }
                        ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Release();
                        ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Close();
                    }

                    DisplayDemandIndicatorWindow(Resources.STR_RPEntryForm);
                }
                if (formName == Resources.STR_RPInquiryForm)
                {
                    ClearDemandIndicatorWindow(formName,
                                            ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.ItemNumber.Value.ToString().Trim(),
                                            ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.LocationCode.Value.ToString().Trim());

                    if (demandIndicatorEntityStatus != null)
                    {
                        short dataShort;
                        foreach (var item in demandIndicatorEntityStatus)
                        {
                            //To save records in holds 
                            ivRpItemSiteMntInquiryForm.Tables.DemandIndicatorStatusTemp.Release();
                            short.TryParse(item.ItemDemandIndicatorId.ToString(), out dataShort);
                            ivRpItemSiteMntInquiryForm.Tables.DemandIndicatorStatusTemp.DemandIndicatorId.Value = dataShort;
                            ivRpItemSiteMntInquiryForm.Tables.DemandIndicatorStatusTemp.DemandIndicatorStatus.Value = item.DemandIndicator.ToString();
                            ivRpItemSiteMntInquiryForm.Tables.DemandIndicatorStatusTemp.Save();
                        }
                        ivRpItemSiteMntInquiryForm.Tables.DemandIndicatorStatusTemp.Release();
                        ivRpItemSiteMntInquiryForm.Tables.DemandIndicatorStatusTemp.Close();
                        ChempointCustomizations.Procedures.DisplayDemandIndicatorInquiry.Invoke();
                    }

                    if (inventoryEntity != null)
                    {
                        ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.Close();
                        ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.Release();
                        ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.LocationCode.Value = ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.LocationCode.Value;
                        ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.ItemNumber.Value = ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.ItemNumber.Value;
                        ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.Key = 1;
                        TableError tableError = ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.Change();
                        if (tableError == TableError.NoError || tableError == TableError.NotFound)
                        {
                            ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.LocationCode.Value = ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.LocationCode.Value;
                            ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.ItemNumber.Value = ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.ItemNumber.Value;
                            ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.DemandIndicatorId.Value = Convert.ToInt16(inventoryEntity.ItemDemandIndicatorId);
                            ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.Save();
                        }
                        else
                        {
                            ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.LocationCode.Value = ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.LocationCode.Value;
                            ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.ItemNumber.Value = ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.ItemNumber.Value;
                            ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.Save();
                        }
                        ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.Release();
                        ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.Close();
                    }
                    DisplayDemandIndicatorWindow(Resources.STR_RPInquiryForm);
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In DisplayDemandIndicator Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "InventoryDetails");
                logMessage = null;
            }
        }

        private void ClearDemandIndicatorWindow(string formName, string itemNumber, string locationCode)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_RPEntryForm)
                {
                    ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Close();
                    ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Release();
                    TableError tempError = ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.ChangeFirst();
                    while (tempError == TableError.NoError && tempError != TableError.EndOfTable)
                    {
                        //check order type and order number to clear temp data
                        if ((itemNumber == "" || itemNumber != ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.ItemNumber.Value.ToString().Trim() ||
                             locationCode == "" || locationCode != ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.LocationCode.Value.ToString().Trim()))
                        {
                            ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Remove();
                        }

                        tempError = ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.ChangeNext();
                    }
                    ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Release();
                    ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Close();
                }
                else if (formName == Resources.STR_RPInquiryForm)
                {
                    ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.Close();
                    ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.Release();
                    TableError tempError = ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.ChangeFirst();
                    while (tempError == TableError.NoError && tempError != TableError.EndOfTable)
                    {
                        //check order type and order number to clear temp data
                        if ((itemNumber == "" || itemNumber != ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.ItemNumber.Value.ToString().Trim() ||
                             locationCode == "" || locationCode != ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.LocationCode.Value.ToString().Trim()))
                        {
                            ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.Remove();
                        }

                        tempError = ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.ChangeNext();
                    }
                    ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.Release();
                    ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.Close();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearDemandIndicatorWindow Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "SalesDetails");
                logMessage = null;
            }
        }

        private void DisplayDemandIndicatorWindow(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_RPEntryForm)
                {
                    ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Close();
                    ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Release();
                    ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.LocationCode.Value = ivRpItemSiteMntForm.IvRpItemSiteMnt.LocationCode.Value;
                    ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.ItemNumber.Value = ivRpItemSiteMntForm.IvRpItemSiteMnt.ItemNumber.Value;
                    ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Key = 1;
                    TableError tableError = ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Get();
                    if (tableError == TableError.NoError)
                    {
                        ivRpItemSiteMntForm.IvRpItemSiteMnt.DemandIndicator.Value = ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.DemandIndicatorId.Value;
                    }
                    else if (tableError == TableError.NotFound)
                    {
                        //ivRpItemSiteMntForm.IvRpItemSiteMnt.DemandIndicator.Value = ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.DemandIndicatorId.Value;
                        ivRpItemSiteMntForm.IvRpItemSiteMnt.DemandIndicator.Clear();
                    }
                    ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Release();
                    ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Close();
                }
                if (formName == Resources.STR_RPInquiryForm)
                {
                    ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.Close();
                    ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.Release();
                    ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.LocationCode.Value = ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.LocationCode.Value;
                    ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.ItemNumber.Value = ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.ItemNumber.Value;
                    ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.Key = 1;
                    TableError tableError = ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.Get();
                    if (tableError == TableError.NoError)
                    {
                        ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.DemandIndicator.Value = ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.DemandIndicatorId.Value;
                    }
                    else if (tableError == TableError.NotFound)
                    {
                        //ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.DemandIndicator.Value = ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.DemandIndicatorId.Value;
                        ivRpItemSiteMntInquiryForm.IvRpItemSiteInquiry.DemandIndicator.Clear();
                    }
                    ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.Release();
                    ivRpItemSiteMntInquiryForm.Tables.IvrpDemandIndicator.Close();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In DisplayDemandIndicatorWindow Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "InventoryDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Demand Indicator Chagne
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void IvRpItemSiteMntDemandIndicator_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Close();
                ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Release();
                ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.LocationCode.Value = ivRpItemSiteMntForm.IvRpItemSiteMnt.LocationCode.Value;
                ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.ItemNumber.Value = ivRpItemSiteMntForm.IvRpItemSiteMnt.ItemNumber.Value;
                ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Key = 1;
                TableError tableError = ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Change();
                if (tableError == TableError.NoError)
                {
                    ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.DemandIndicatorId.Value = ivRpItemSiteMntForm.IvRpItemSiteMnt.DemandIndicator.Value;
                    ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Save();
                }
                else if (tableError == TableError.NotFound)
                {
                    ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.LocationCode.Value = ivRpItemSiteMntForm.IvRpItemSiteMnt.LocationCode.Value;
                    ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.ItemNumber.Value = ivRpItemSiteMntForm.IvRpItemSiteMnt.ItemNumber.Value;
                    ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.DemandIndicatorId.Value = ivRpItemSiteMntForm.IvRpItemSiteMnt.DemandIndicator.Value;
                    ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Save();
                }
                ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Release();
                ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Close();

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In DemandIndicator_Change Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "InventoryDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Save Demand Indicator value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void IvRpItemSiteMntDiSaveFlag_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();            
            try
            {
                InventoryResourceRequest inventoryRequest = null;
                InventoryResourceEntity inventoryResourceEntity = null;

                ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Close();
                ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Release();
                ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.LocationCode.Value = ivRpItemSiteMntForm.IvRpItemSiteMnt.LocationCode.Value;
                ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.ItemNumber.Value = ivRpItemSiteMntForm.IvRpItemSiteMnt.ItemNumber.Value;
                ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Key = 1;
                TableError tableError = ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Get();
                if (tableError == TableError.NoError)
                {
                    inventoryRequest = new InventoryResourceRequest();
                    inventoryResourceEntity = new InventoryResourceEntity();

                    inventoryResourceEntity.ItemNumber = ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.ItemNumber.Value;
                    inventoryResourceEntity.WarehouseId = ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.LocationCode.Value;
                    inventoryResourceEntity.ItemDemandIndicatorId = ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.DemandIndicatorId.Value;

                    int status;
                    int.TryParse(Dynamics.Globals.UserId.Value.ToString(), out status);
                    inventoryResourceEntity.CreatedBy = status;
                    inventoryResourceEntity.CreatedOn = Dynamics.Globals.UserDate.Value;
                    int.TryParse(Dynamics.Globals.UserId.Value.ToString(), out status);
                    inventoryResourceEntity.ModifiedBy = status;
                    inventoryResourceEntity.ModifiedOn = Dynamics.Globals.UserDate.Value;
                    inventoryRequest.InventoryResourceEntity = inventoryResourceEntity;
                    inventoryRequest.UserId = Dynamics.Globals.UserId.Value.ToString();
                    inventoryRequest.CompanyID = Dynamics.Globals.CompanyId.Value;
                }
                ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Release();
                ivRpItemSiteMntForm.Tables.IvrpDemandIndicator.Close();



                #region ServiceCall
                if (inventoryRequest != null && inventoryResourceEntity != null)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/Inventory/SaveItemResourceDetail", inventoryRequest);
                        if (response.Result.IsSuccessStatusCode)
                        {
                            InventoryResourceResponse inventoryResponse = response.Result.Content.ReadAsAsync<InventoryResourceResponse>().Result;
                            if (inventoryResponse.Status == ResponseStatus.Error)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SaveButton_ClickAfterOriginal Method (SaveItemResourceDetail): " + inventoryResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + inventoryResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }                            
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not save into custom inventory details Table");
                            MessageBox.Show("Error: Data does not save into custom inventory details Table", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }

                #endregion ServiceCall
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SaveButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "InventoryDetails");
                logMessage = null;
            }
        }

        #endregion ItemresourcePlanning

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
                case "InventoryDetails":
                    if (isInventoryLoggingEnabled && message != "")
                        new TextLogger().LogInformationIntoFile(message, inventoryLogFilePath, inventoryLogFileName);
                    break;
                default:
                    break;
            }
        }
    }
}
