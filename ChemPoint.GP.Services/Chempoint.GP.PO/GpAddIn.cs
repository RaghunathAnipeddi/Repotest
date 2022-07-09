using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Chempoint.GP.Model.Interactions.Purchases;
using Microsoft.Dexterity.Bridge;
using Microsoft.Dexterity.Applications;
using Microsoft.Dexterity.Applications.DynamicsDictionary;
using Dic1311 = Microsoft.Dexterity.Applications.ChempointCustomizationsDictionary;
using System.Net.Http;
using System.Net.Http.Headers;
using ChemPoint.GP.Entities.Business_Entities.Purchase;
using System.Runtime.InteropServices;
using ChemPoint.GP.Entities.BaseEntities;
using System.IO;
using Chempoint.GP.PO.Properties;
using Chempoint.GP.Infrastructure.Logging;
using System.Text;
using System.Linq;
using System.Globalization;
using Elemica;
using System.Data;
using Chempoint.GP.Model.Interactions.PayableManagement;
using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.APOut;

namespace Chempoint.GP.PO
{
    /// <PO>
    /// Project Name        :   GP Service 
    /// Affected Module     :   Purchase
    /// Affected Windows    :   PO Entry,Indicator,ItemDetail
    /// Developed on        :   2016Aug22  
    /// Developed by        :   Nagaraj ,Muthu and Amit.
    ///-------------------------------------------------------------------------------------------
    ///     Date		Author          Description
    ///-------------------------------------------------------------------------------------------
    /// **Apr2017       Mthangaraj      Dll Upgrade Regarding GP 2016
    /// </PO>
    public class GPAddIn : IDexterityAddIn
    {
        // IDexterityAddIn interface
        //PO Indicator window..
        static Dic1311.PopPoEntryForm popPOEntryForm;
        static Dic1311.PopPoItemDetailEntryForm popPoItemDetailEntryForm;
        static Dic1311.PoIndicatorsForm poIndicatorsForm;
        static Dic1311.PoIndicatorsInquiryForm poIndicatorsInquiryForm;
        static Dic1311.PopInquiryPoItemDetailForm popInquiryPoItemDetailForm;
        static string gpServiceConfigurationUrl = null;
        bool isPoLoggingEnabled = false;
        string poLogFileName = null;
        string poLogFilePath = null;

        //Default BuyerID to improve user experience 
        public int POStatus;

        //Landed Cost
        static Dic1311.EstimatedShipmentCostEntryForm estimatedShipmentCostEntryForm;
        static CurrencyLookupForm currencyLookupForm;
        static IvLocationLookupForm ivLocationLookupForm;
        //static PopDocumentLookupForm popDocumentLookupForm;
        static VendorLookupForm vendorLookupForm;
        static Dic1311.SelectPoLinesForm selectPoLinesForm;
        static Dic1311.EstimatedShipmentCostInquiryForm estimatedShipmentCostInquiryForm;
        static Dic1311.EstimateIdLookupForm estimateIdLookupForm;
        static Dic1311.PoNumberLookupForm poNumberLookupForm;

        //PO Cost Management
        static Dic1311.PoCostManagementForm poCostManagementForm;
        bool registerPoCostManagement = false;

        //Material Management
        string firstPOForMail = string.Empty;
        List<string> PoCostNotesHistory;
        DataTable costVarianceDT = null;
        bool isMaterialAction = false;

        //LandedCost
        string lookupWindowType = string.Empty;
        Boolean RegisterCurrencyLookupSelect = false;
        Boolean RegisterLocationLookupSelect = false;
        Boolean RegisterVendorLookupSelect = false;
        Boolean RegisterPoLookupSelect = false;
        Boolean IsEstimateLineHasRow = false;

        //Elemica        
        //PopPoEntryForm popPoEntryForm;
        ElemicaStausForm poElemicaStausForm = null;
        string vendorId = string.Empty;
        string PoNumberForElemica = string.Empty;
               


        //Variables for PO unit cost changes
        bool lineEnterFlag = false;
        bool poEventsRegistered = false;
        [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileStringW",
            SetLastError = true,
            CharSet = CharSet.Unicode, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, string lpReturnString, int nSize, string lpFilename);
        public void Initialize()
        {            
            //PO Indicator form Initialization
            popPoItemDetailEntryForm = ChempointCustomizations.Forms.PopPoItemDetailEntry;
            poIndicatorsForm = ChempointCustomizations.Forms.PoIndicators;
            popInquiryPoItemDetailForm = ChempointCustomizations.Forms.PopInquiryPoItemDetail;
            poIndicatorsInquiryForm = ChempointCustomizations.Forms.PoIndicatorsInquiry;

            //Landed Cost Initilizations
            estimatedShipmentCostEntryForm = ChempointCustomizations.Forms.EstimatedShipmentCostEntry;
            selectPoLinesForm = ChempointCustomizations.Forms.SelectPoLines;
            estimatedShipmentCostInquiryForm = ChempointCustomizations.Forms.EstimatedShipmentCostInquiry;
            estimateIdLookupForm = ChempointCustomizations.Forms.EstimateIdLookup;
            poNumberLookupForm = ChempointCustomizations.Forms.PoNumberLookup;

            //Elemica
            //popPoEntryForm = Dynamics.Forms.PopPoEntry;


            //PO Cost Management
            popPOEntryForm = ChempointCustomizations.Forms.PopPoEntry;
            poCostManagementForm = ChempointCustomizations.Forms.PoCostManagement;
            popPoItemDetailEntryForm.AddMenuHandler(POIndicatorWindow, "PO Indicators", "O");
            popInquiryPoItemDetailForm.AddMenuHandler(POIndicatorInquiryWindow, "PO Indicator Inquiry", "O");
            poIndicatorsForm.PoIndicators.OpenAfterOriginal += new EventHandler(PoIndicators_OpenAfterOriginal);
            poIndicatorsInquiryForm.PoIndicatorsInquiry.OpenAfterOriginal += new EventHandler(PoIndicatorsInquiry_OpenAfterOriginal);
            popPoItemDetailEntryForm.PopPoItemDetailEntry.DeleteButton.ClickAfterOriginal += new EventHandler(PopPoItemDetailEntryDeleteButton_ClickAfterOriginal);
            poIndicatorsForm.PoIndicators.SaveButton.ClickAfterOriginal += new EventHandler(PoIndicatorsSaveButton_ClickAfterOriginal);
            poIndicatorsForm.PoIndicators.CancelButton.ClickAfterOriginal += new EventHandler(PoIndicatorsCancelButton_ClickAfterOriginal);
            poIndicatorsForm.PoIndicators.PoNewStatusDdl.Change += new EventHandler(PoIndicatorsPoNewStatusDdl_Change);
            poIndicatorsInquiryForm.PoIndicatorsInquiry.CancelButton.ClickAfterOriginal += new EventHandler(PoIndicatorsInquiryCancelButton_ClickAfterOriginal);
            //PO Cost Management Events
            popPOEntryForm.PopPoEntry.PoNumber.Change += new EventHandler(PoNumber_Change);
            popPOEntryForm.PopPoEntry.OpenBeforeOriginal += new System.ComponentModel.CancelEventHandler(PopPoEntry_OpenBeforeOriginal);
            poCostManagementForm.PoCostManagement.OpenBeforeOriginal += new System.ComponentModel.CancelEventHandler(PoCostManagementFormPoCostManagement_OpenBeforeOriginal);

            //PO default BuyerID to improve user experience
            popPOEntryForm.PopPoEntry.PoType.Change += new EventHandler(PoType_Change);             
            popPOEntryForm.PopPoEntry.LocationCode.LeaveAfterOriginal += LocationCode_LeaveAfterOriginal;
            popPOEntryForm.PopPoEntry.BuyerId.LeaveAfterOriginal += BuyerId_LeaveAfterOriginal;
            popPOEntryForm.PopPoEntry.PoNumber.LeaveAfterOriginal += PoNumber_LeaveAfterOriginal;

            /***Venture: Landed Cost -Start***/
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.SaveButton.ClickAfterOriginal += new EventHandler(EstimatedShipmentCostEntrySaveButton_ClickAfterOriginal);
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.ClearButton.ClickAfterOriginal += new EventHandler(EstimatedShipmentCostEntryClearButton_ClickAfterOriginal);
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.DeleteButton.ClickAfterOriginal += new EventHandler(EstimatedShipmentCostEntryDeleteButton_ClickAfterOriginal);
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.DeleteRowButton.ClickAfterOriginal += new EventHandler(EstimatedShipmentCostEntryDeleteRowButton_ClickAfterOriginal);
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.PrintButton.ClickAfterOriginal += new EventHandler(EstimatedShipmentCostEntryPrintButton_ClickAfterOriginal);
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.NewButton.ClickAfterOriginal += new EventHandler(EstimatedShipmentCostEntryNewButton_ClickAfterOriginal);
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.AddPoLines.ClickAfterOriginal += new EventHandler(EstimatedShipmentCostEntryAddPoLines_ClickAfterOriginal);
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CarrierReferenceNumber.LeaveAfterOriginal += new EventHandler(EstimatedShipmentCarrierReference_LeaveAfterOriginal);
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.EstimatedQtyShipped.LeaveBeforeOriginal += EstimatedQtyShipped_LeaveBeforeOriginal;
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.EstimatedQtyShipped.LeaveAfterOriginal += EstimatedQtyShipped_LeaveAfterOriginal;
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.RedisplayButton.ClickAfterOriginal += new EventHandler(EstimatedShipmentRedisplayButton_ClickAfterOriginal);
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CloseBeforeOriginal += EstimatedShipmentCostEntry_CloseBeforeOriginal;
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.EstimatedQtyShipped.EnterBeforeOriginal += EstimatedQtyShipped_EnterBeforeOriginal;

            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateId.Change += new EventHandler(EstimateId_Change);
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LookupButton1.ClickAfterOriginal += EstimatedShipmentCostEntryLookupButton1_ClickAfterOriginal;
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LookupButton2.ClickAfterOriginal += EstimatedShipmentCostEntryLookupButton2_ClickAfterOriginal;
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LookupButton3.ClickAfterOriginal += EstimatedShipmentCostEntryLookupButton3_ClickAfterOriginal;
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LookupButton4.ClickAfterOriginal += EstimatedShipmentCostEntryLookupButton4_ClickAfterOriginal;

            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationCode.LeaveBeforeOriginal += LocationCode_LeaveBeforeOriginal;
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorId.LeaveBeforeOriginal += VendorId_LeaveBeforeOriginal;
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIdKey.Change += CurrencyIdKey_Change;

            selectPoLinesForm.SelectPoLines.SelectButton.ClickAfterOriginal += new EventHandler(SelectButton_ClickAfterOriginal);
            selectPoLinesForm.SelectPoLines.CancelButton.ClickAfterOriginal += new EventHandler(CancelButton_ClickAfterOriginal);
            selectPoLinesForm.SelectPoLines.PoNumber.Change += new EventHandler(SelectPoLinesPoNumber_Change);
            selectPoLinesForm.SelectPoLines.SelectPoLinesScroll.SelectCheckBox.Change += new EventHandler(SelectCheckBox_Change);
            selectPoLinesForm.SelectPoLines.LookupButton1.ClickAfterOriginal += new EventHandler(selectPoLinesLookupButton1_ClickAfterOriginal);
            selectPoLinesForm.SelectPoLines.LocalSelectAllCheckBox.Change += LocalSelectAllCheckBox_Change;
            selectPoLinesForm.SelectPoLines.ClearButton.ClickAfterOriginal += SelectButtonClearButton_ClickAfterOriginal;

            estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.OkButton.ClickAfterOriginal += new EventHandler(EstimatedShipmentCostInquiryOkButton_ClickAfterOriginal);
            estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.RedisplayButton.ClickAfterOriginal += new EventHandler(EstimatedCostInquiryRedisplayButton_ClickAfterOriginal);
            estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.EstimateId.Change += new EventHandler(EstimatedCostInquiryEstimateId_Change);
            estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.LookupButton1.ClickAfterOriginal += new EventHandler(EstimatedShipmentCostInquiryLookupButton1_ClickAfterOriginal);

            currencyLookupForm = Dynamics.Forms.CurrencyLookup;
            ivLocationLookupForm = Dynamics.Forms.IvLocationLookup;
            vendorLookupForm = Dynamics.Forms.VendorLookup;
            estimateIdLookupForm.EstimateIdLookup.CancelButton.ClickAfterOriginal += new EventHandler(EstimateIdLookupCancelButton_ClickAfterOriginal);
            estimateIdLookupForm.EstimateIdLookup.SelectButtonMnemonic.ClickAfterOriginal += new EventHandler(EstimateIdLookupSelectButtonMnemonic_ClickAfterOriginal);
            poNumberLookupForm.PoNumberLookup.SelectButtonMnemonic.ClickAfterOriginal += new EventHandler(PoNumberLookupSelectButtonMnemonic_ClickAfterOriginal);
            poNumberLookupForm.PoNumberLookup.CancelButton.ClickAfterOriginal += new EventHandler(PoNumberLookupCancelButton_ClickAfterOriginal);


            /***Venture: Landed Cost -End***/

            //#region Elemica
            //popPoEntryForm.PopPoEntry.WindowPrint.ClickAfterOriginal += WindowPrint_ClickAfterOriginal;
            //popPoEntryForm.PopPoEntry.WindowPrint.ClickBeforeOriginal += WindowPrint_ClickBeforeOriginal;
            //#endregion


            string getCurrentDirectoryPath = @Directory.GetCurrentDirectory();
            string iniFilePath = getCurrentDirectoryPath + "\\Data\\Dex.ini";
            List<string> categories = GetCategories(iniFilePath);
            foreach (string category in categories)
            {
                //Get the key values
                gpServiceConfigurationUrl = GetIniFileString(iniFilePath, category, "GPSERVICE", "");
                isPoLoggingEnabled = Convert.ToBoolean(GetIniFileString(iniFilePath, category, "ISPOLOGENABLED", ""));
                poLogFileName = GetIniFileString(iniFilePath, category, "POLOGFILENAME", "");
                poLogFilePath = GetIniFileString(iniFilePath, category, "POLOGFILEPATH", "");

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



        #region POIndicatorWindow



        void POIndicatorWindow(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                if (String.IsNullOrEmpty(popPoItemDetailEntryForm.PopPoItemDetailEntry.ItemNumber.Value))
                {

                    MessageBox.Show("Please select valid Line Item");

                }

                else
                {

                    poIndicatorsForm.PoIndicators.Open();

                }

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In POIndicatorWindow Method: " + ex.Message.ToString());

                MessageBox.Show("Error: Could not open PO Indicators window- " + ex.Message, Resources.STR_MESSAGE_TITLE);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PurchaseDetails");

                logMessage = null;

            }

        }



        void POIndicatorInquiryWindow(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                if (String.IsNullOrEmpty(popInquiryPoItemDetailForm.PopInquiryPurchasingItemDetail.ItemNumber.Value))
                {

                    MessageBox.Show("selected Line Item is not valid");

                }

                else
                {

                    poIndicatorsInquiryForm.PoIndicatorsInquiry.Open();

                }

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In POIndicatorInquiryWindow Method: " + ex.Message.ToString());

                MessageBox.Show("Error: Could not open PO Indicators Inquiry window- " + ex.Message, Resources.STR_MESSAGE_TITLE);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PurchaseDetails");

                logMessage = null;

            }

        }



        void PoIndicatorsCancelButton_ClickAfterOriginal(object sender, EventArgs e)
        {

            poIndicatorsForm.PoIndicators.Close();

        }



        void PoIndicatorsInquiryCancelButton_ClickAfterOriginal(object sender, EventArgs e)
        {

            poIndicatorsInquiryForm.PoIndicatorsInquiry.Close();

        }



        void PoIndicatorsPoNewStatusDdl_Change(object sender, EventArgs e)
        {

            StatusChange();

        }



        void PoIndicatorsInquiry_OpenAfterOriginal(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                InsertPOIndicatorDetails(Resources.STR_POIndicatorInquiry);

                DisplayPOIndicatorDetails(Resources.STR_POIndicatorInquiry);

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoIndicatorsInquiryOpenAfterOriginal Method: " + ex.Message.ToString());

                MessageBox.Show("Error: Could not open PO Indicators Inquiry window- " + ex.Message, Resources.STR_MESSAGE_TITLE);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PurchaseDetails");

                logMessage = null;

            }

        }



        void PoIndicators_OpenAfterOriginal(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                InsertPOIndicatorDetails(Resources.STR_POIndicatorForm);

                DisplayPOIndicatorDetails(Resources.STR_POIndicatorForm);

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoIndicatorsOpenAfterOriginal Method: " + ex.Message.ToString());

                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PurchaseDetails");

                logMessage = null;

            }

        }



        //PO Indicator Detail Fetch

        private void InsertPOIndicatorDetails(string formName)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {







                PurchaseIndicatorEntity purchaseIndicatorEntity = null;



                PurchaseIndicatorRequest poIndicatorRequest = new PurchaseIndicatorRequest();

                poIndicatorRequest.CompanyID = Dynamics.Globals.CompanyId.Value;

                PurchaseOrderInformation poIndicatorBase = new PurchaseOrderInformation();

                if (formName == Resources.STR_POIndicatorForm)
                {

                    poIndicatorBase.PoNumber = popPoItemDetailEntryForm.PopPoItemDetailEntry.PoNumber.Value;

                    poIndicatorBase.PoLineNumber = popPoItemDetailEntryForm.PopPoItemDetailEntry.Ord.Value;

                    poIndicatorRequest.PoIndicatorBase = poIndicatorBase;

                }

                if (formName == Resources.STR_POIndicatorInquiry)
                {

                    poIndicatorBase.PoNumber = popInquiryPoItemDetailForm.PopInquiryPurchasingItemDetail.PoNumber.Value;

                    poIndicatorBase.PoLineNumber = popInquiryPoItemDetailForm.PopInquiryPurchasingItemDetail.Ord.Value;

                    poIndicatorRequest.PoIndicatorBase = poIndicatorBase;

                }

                if (poIndicatorRequest != null)
                {

                    // Service call ...

                    using (HttpClient client = new HttpClient())
                    {

                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);

                        client.DefaultRequestHeaders.Accept.Clear();

                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));



                        var response = client.PostAsJsonAsync("api/PurchaseOrder/GetPoIndicatorDetail", poIndicatorRequest); // we need to refer the web.api service url here.

                        if (response.Result.IsSuccessStatusCode)
                        {

                            PurchaseIndicatorResponse purchaseIndicatorResponse = response.Result.Content.ReadAsAsync<PurchaseIndicatorResponse>().Result;

                            if (purchaseIndicatorResponse.Status == ResponseStatus.Error)
                            {

                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertPOIndicatorDetails Method (GetPoIndicatorDetail): " + purchaseIndicatorResponse.ErrorMessage.ToString());

                                MessageBox.Show("Error: " + purchaseIndicatorResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);

                            }

                            else
                            {

                                purchaseIndicatorEntity = new PurchaseIndicatorEntity();

                                purchaseIndicatorEntity = purchaseIndicatorResponse.PurchaseIndicatorList;

                            }

                        }

                        else
                        {

                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not fetch from po indicator details Table");

                            MessageBox.Show("Error: Data does not fetch from po indicator details Table", Resources.STR_MESSAGE_TITLE);

                        }

                    }

                }



                if (formName == Resources.STR_POIndicatorForm)
                {

                    ClearPOindicatorDetails(formName,

                                            popPoItemDetailEntryForm.PopPoItemDetailEntry.PoNumber.Value,

                                            popPoItemDetailEntryForm.PopPoItemDetailEntry.Ord.Value);  // bypass or line sequence Number on double insertion



                    //Assign Object to Dex Table...

                    if (purchaseIndicatorEntity != null)
                    {

                        poIndicatorsForm.Tables.PoLineIndicatorTemp.Close();

                        poIndicatorsForm.Tables.PoLineIndicatorTemp.Release();

                        poIndicatorsForm.Tables.PoLineIndicatorTemp.PoNumber.Value = popPoItemDetailEntryForm.PopPoItemDetailEntry.PoNumber.Value;

                        poIndicatorsForm.Tables.PoLineIndicatorTemp.Ord.Value = popPoItemDetailEntryForm.PopPoItemDetailEntry.Ord.Value;

                        poIndicatorsForm.Tables.PoLineIndicatorTemp.Key = 1;

                        TableError tableError = poIndicatorsForm.Tables.PoLineIndicatorTemp.Change();

                        if (tableError == TableError.NoError)
                        {

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.PoNewStatusDdl.Value = purchaseIndicatorEntity.POIndicatorStatusId;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.PoBackOrderReasonDdl.Value = purchaseIndicatorEntity.BackOrderReason;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.BackOrderDate.Value = purchaseIndicatorEntity.InitialBackOrderDate;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.PoCancelledReasonDdl.Value = purchaseIndicatorEntity.CancelledReason;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.CancelledDate.Value = purchaseIndicatorEntity.InitialCancelledDate;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.CpHasCostVariance.Value = purchaseIndicatorEntity.IsCostVariance;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.AcknowledgementDate.Value = purchaseIndicatorEntity.AcknowledgementDate;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.ConfirmedDate.Value = purchaseIndicatorEntity.ConfirmedDate;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.ActualShipDate.Value = purchaseIndicatorEntity.ActualShipDate;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.Save();

                        }

                        else if (tableError == TableError.NotFound)
                        {

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.PoNumber.Value = popPoItemDetailEntryForm.PopPoItemDetailEntry.PoNumber.Value;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.Ord.Value = popPoItemDetailEntryForm.PopPoItemDetailEntry.Ord.Value;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.PoNewStatusDdl.Value = purchaseIndicatorEntity.POIndicatorStatusId;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.PoBackOrderReasonDdl.Value = purchaseIndicatorEntity.BackOrderReason;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.BackOrderDate.Value = purchaseIndicatorEntity.InitialBackOrderDate;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.PoCancelledReasonDdl.Value = purchaseIndicatorEntity.CancelledReason;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.CancelledDate.Value = purchaseIndicatorEntity.InitialCancelledDate;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.CpHasCostVariance.Value = purchaseIndicatorEntity.IsCostVariance;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.AcknowledgementDate.Value = purchaseIndicatorEntity.AcknowledgementDate;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.ConfirmedDate.Value = purchaseIndicatorEntity.ConfirmedDate;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.ActualShipDate.Value = purchaseIndicatorEntity.ActualShipDate;

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.Save();

                        }

                        poIndicatorsForm.Tables.PoLineIndicatorTemp.Release();

                        poIndicatorsForm.Tables.PoLineIndicatorTemp.Close();

                    }

                }



                if (formName == Resources.STR_POIndicatorInquiry)
                {

                    ClearPOindicatorDetails(formName,

                                            popInquiryPoItemDetailForm.PopInquiryPurchasingItemDetail.PoNumber.Value,

                                            popInquiryPoItemDetailForm.PopInquiryPurchasingItemDetail.Ord.Value);  // bypass or line sequence Number on double insertion



                    //Assign Object to Dex Table...

                    if (purchaseIndicatorEntity != null)
                    {

                        poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Close();

                        poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Release();

                        poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.PoNumber.Value = popInquiryPoItemDetailForm.PopInquiryPurchasingItemDetail.PoNumber.Value;

                        poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Ord.Value = popInquiryPoItemDetailForm.PopInquiryPurchasingItemDetail.Ord.Value;

                        poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Key = 1;

                        TableError tableError = poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Change();

                        if (tableError == TableError.NoError)
                        {

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.PoNewStatusDdl.Value = purchaseIndicatorEntity.POIndicatorStatusId;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.PoBackOrderReasonDdl.Value = purchaseIndicatorEntity.BackOrderReason;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.BackOrderDate.Value = purchaseIndicatorEntity.InitialBackOrderDate;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.PoCancelledReasonDdl.Value = purchaseIndicatorEntity.CancelledReason;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.CancelledDate.Value = purchaseIndicatorEntity.InitialCancelledDate;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.CpHasCostVariance.Value = purchaseIndicatorEntity.IsCostVariance;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.AcknowledgementDate.Value = purchaseIndicatorEntity.AcknowledgementDate;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.ConfirmedDate.Value = purchaseIndicatorEntity.ConfirmedDate;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.ActualShipDate.Value = purchaseIndicatorEntity.ActualShipDate;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Save();

                        }

                        else if (tableError == TableError.NotFound)
                        {

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.PoNumber.Value = popInquiryPoItemDetailForm.PopInquiryPurchasingItemDetail.PoNumber.Value;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Ord.Value = popInquiryPoItemDetailForm.PopInquiryPurchasingItemDetail.Ord.Value;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.PoNewStatusDdl.Value = purchaseIndicatorEntity.POIndicatorStatusId;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.PoBackOrderReasonDdl.Value = purchaseIndicatorEntity.BackOrderReason;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.BackOrderDate.Value = purchaseIndicatorEntity.InitialBackOrderDate;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.PoCancelledReasonDdl.Value = purchaseIndicatorEntity.CancelledReason;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.CancelledDate.Value = purchaseIndicatorEntity.InitialCancelledDate;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.CpHasCostVariance.Value = purchaseIndicatorEntity.IsCostVariance;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.AcknowledgementDate.Value = purchaseIndicatorEntity.AcknowledgementDate;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.ConfirmedDate.Value = purchaseIndicatorEntity.ConfirmedDate;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.ActualShipDate.Value = purchaseIndicatorEntity.ActualShipDate;

                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Save();

                        }

                        poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Release();

                        poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Close();

                    }

                }

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertPOIndicatorDetails Method: " + ex.Message.ToString());

                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PurchaseDetails");

                logMessage = null;

            }

        }



        private void DisplayPOIndicatorDetails(string formName)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                if (formName == Resources.STR_POIndicatorForm)
                {

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.Close();

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.Release();

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.PoNumber.Value = popPoItemDetailEntryForm.PopPoItemDetailEntry.PoNumber.Value;

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.Ord.Value = popPoItemDetailEntryForm.PopPoItemDetailEntry.Ord.Value;

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.Key = 1;

                    TableError tableError = poIndicatorsForm.Tables.PoLineIndicatorTemp.Get();

                    if (tableError == TableError.NoError)
                    {

                        poIndicatorsForm.PoIndicators.LocalPoNumber.Value = popPoItemDetailEntryForm.PopPoItemDetailEntry.PoNumber.Value;

                        poIndicatorsForm.PoIndicators.LocalLinenumber.Value = popPoItemDetailEntryForm.PopPoItemDetailEntry.LineNumber.Value;

                        poIndicatorsForm.PoIndicators.LocalItemNumber.Value = popPoItemDetailEntryForm.PopPoItemDetailEntry.ItemNumber.Value;

                        poIndicatorsForm.PoIndicators.PoNewStatusDdl.Value = poIndicatorsForm.Tables.PoLineIndicatorTemp.PoNewStatusDdl.Value;

                        poIndicatorsForm.PoIndicators.PoBackOrderReasonDdl.Value = poIndicatorsForm.Tables.PoLineIndicatorTemp.PoBackOrderReasonDdl.Value;

                        poIndicatorsForm.PoIndicators.LocalBackOrderDate.Value = poIndicatorsForm.Tables.PoLineIndicatorTemp.BackOrderDate.Value;

                        poIndicatorsForm.PoIndicators.PoCancelledReasonDdl.Value = poIndicatorsForm.Tables.PoLineIndicatorTemp.PoCancelledReasonDdl.Value;

                        poIndicatorsForm.PoIndicators.LocalCancelledDate.Value = poIndicatorsForm.Tables.PoLineIndicatorTemp.CancelledDate.Value;

                        poIndicatorsForm.PoIndicators.LocalHasCostVariance.Value = poIndicatorsForm.Tables.PoLineIndicatorTemp.CpHasCostVariance.Value;

                        poIndicatorsForm.PoIndicators.LocalAcknowledgementDate.Value = poIndicatorsForm.Tables.PoLineIndicatorTemp.AcknowledgementDate.Value;

                        poIndicatorsForm.PoIndicators.LocalConfirmedDate.Value = poIndicatorsForm.Tables.PoLineIndicatorTemp.ConfirmedDate.Value;

                        poIndicatorsForm.PoIndicators.LocalActualShipDate.Value = poIndicatorsForm.Tables.PoLineIndicatorTemp.ActualShipDate.Value;

                    }

                    else if (tableError == TableError.NotFound)
                    {

                        poIndicatorsForm.PoIndicators.LocalPoNumber.Value = popPoItemDetailEntryForm.PopPoItemDetailEntry.PoNumber.Value;

                        poIndicatorsForm.PoIndicators.LocalLinenumber.Value = popPoItemDetailEntryForm.PopPoItemDetailEntry.LineNumber.Value;

                        poIndicatorsForm.PoIndicators.LocalItemNumber.Value = popPoItemDetailEntryForm.PopPoItemDetailEntry.ItemNumber.Value;

                        poIndicatorsForm.PoIndicators.PoNewStatusDdl.Value = poIndicatorsForm.Tables.PoLineIndicatorTemp.PoNewStatusDdl.Value;

                    }

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.Release();

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.Close();

                }

                if (formName == Resources.STR_POIndicatorInquiry)
                {

                    poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Close();

                    poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Release();

                    poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.PoNumber.Value = popInquiryPoItemDetailForm.PopInquiryPurchasingItemDetail.PoNumber.Value;

                    poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Ord.Value = popInquiryPoItemDetailForm.PopInquiryPurchasingItemDetail.Ord.Value;

                    poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Key = 1;

                    TableError tableError = poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Get();

                    if (tableError == TableError.NoError)
                    {

                        poIndicatorsInquiryForm.PoIndicatorsInquiry.LocalPoNumber.Value = popInquiryPoItemDetailForm.PopInquiryPurchasingItemDetail.PoNumber.Value;

                        poIndicatorsInquiryForm.PoIndicatorsInquiry.LocalLinenumber.Value = popInquiryPoItemDetailForm.PopInquiryPurchasingItemDetail.LineNumber.Value;

                        poIndicatorsInquiryForm.PoIndicatorsInquiry.LocalItemNumber.Value = popInquiryPoItemDetailForm.PopInquiryPurchasingItemDetail.ItemNumber.Value;

                        poIndicatorsInquiryForm.PoIndicatorsInquiry.PoNewStatusDdl.Value = poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.PoNewStatusDdl.Value;

                        poIndicatorsInquiryForm.PoIndicatorsInquiry.PoBackOrderReasonDdl.Value = poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.PoBackOrderReasonDdl.Value;

                        poIndicatorsInquiryForm.PoIndicatorsInquiry.LocalBackOrderDate.Value = poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.BackOrderDate.Value;

                        poIndicatorsInquiryForm.PoIndicatorsInquiry.PoCancelledReasonDdl.Value = poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.PoCancelledReasonDdl.Value;

                        poIndicatorsInquiryForm.PoIndicatorsInquiry.LocalCancelledDate.Value = poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.CancelledDate.Value;

                        poIndicatorsInquiryForm.PoIndicatorsInquiry.LocalHasCostVariance.Value = poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.CpHasCostVariance.Value;

                        poIndicatorsInquiryForm.PoIndicatorsInquiry.LocalAcknowledgementDate.Value = poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.AcknowledgementDate.Value;

                        poIndicatorsInquiryForm.PoIndicatorsInquiry.LocalConfirmedDate.Value = poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.ConfirmedDate.Value;

                        poIndicatorsInquiryForm.PoIndicatorsInquiry.LocalActualShipDate.Value = poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.ActualShipDate.Value;

                    }

                    else if (tableError == TableError.NotFound)
                    {

                        poIndicatorsInquiryForm.PoIndicatorsInquiry.LocalPoNumber.Value = popInquiryPoItemDetailForm.PopInquiryPurchasingItemDetail.PoNumber.Value;

                        poIndicatorsInquiryForm.PoIndicatorsInquiry.LocalLinenumber.Value = popInquiryPoItemDetailForm.PopInquiryPurchasingItemDetail.LineNumber.Value;

                        poIndicatorsInquiryForm.PoIndicatorsInquiry.LocalItemNumber.Value = popInquiryPoItemDetailForm.PopInquiryPurchasingItemDetail.ItemNumber.Value;

                        poIndicatorsInquiryForm.PoIndicatorsInquiry.PoNewStatusDdl.Value = poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.PoNewStatusDdl.Value;

                    }

                    poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Release();

                    poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Close();

                }

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In DisplayPOIndicatorDetails Method: " + ex.Message.ToString());

                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PurchaseDetails");

                logMessage = null;

            }

        }



        void PoIndicatorsSaveButton_ClickAfterOriginal(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                if (poIndicatorsForm.IsOpen)
                {

                    if (poIndicatorsForm.PoIndicators.PoNewStatusDdl.Value != 0)
                    {

                        ValidateFieldValue();

                    }

                    else
                    {

                        poIndicatorsForm.PoIndicators.Close();

                    }

                }

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SaveButtonClickAfterOriginal Method: " + ex.Message.ToString());

                MessageBox.Show("Error: Could not save PO Indicators details- " + ex.Message, Resources.STR_MESSAGE_TITLE);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PurchaseDetails");

                logMessage = null;

            }

        }



        private void SavePoIndicatorWindow()
        {

            StringBuilder logMessage = new StringBuilder();

            bool isCloseForm = false;



            try
            {

                bool isExist = false;

                poIndicatorsForm.Tables.PoLineIndicatorTemp.Close();

                poIndicatorsForm.Tables.PoLineIndicatorTemp.Release();

                poIndicatorsForm.Tables.PoLineIndicatorTemp.PoNumber.Value = popPoItemDetailEntryForm.PopPoItemDetailEntry.PoNumber.Value;

                poIndicatorsForm.Tables.PoLineIndicatorTemp.Ord.Value = popPoItemDetailEntryForm.PopPoItemDetailEntry.Ord.Value;

                poIndicatorsForm.Tables.PoLineIndicatorTemp.Key = 1;

                TableError tableError = poIndicatorsForm.Tables.PoLineIndicatorTemp.Get();

                if (tableError == TableError.NoError || tableError == TableError.NotFound)
                {

                    isExist = true;

                }

                poIndicatorsForm.Tables.PoLineIndicatorTemp.Release();

                poIndicatorsForm.Tables.PoLineIndicatorTemp.Close();



                if (isExist)
                {

                    PurchaseIndicatorRequest poIndicatorRequest = new PurchaseIndicatorRequest();

                    poIndicatorRequest.CompanyID = Dynamics.Globals.CompanyId.Value;

                    //Assign SopEntry Value to Request Object.

                    AssignPoIndicatorWindowToDexTable();

                    //Assign Customer Detail Entry value to Object.

                    poIndicatorRequest = PoIndicatorDexTableToobject();



                    if (poIndicatorRequest != null)
                    {

                        // Service call ...

                        using (HttpClient client = new HttpClient())
                        {

                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);

                            client.DefaultRequestHeaders.Accept.Clear();

                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));



                            var response = client.PostAsJsonAsync("api/PurchaseOrder/SavePoIndicatorDetail", poIndicatorRequest); // we need to refer the web.api service url here.

                            if (response.Result.IsSuccessStatusCode)
                            {

                                PurchaseIndicatorResponse purchaseIndicatorResponse = response.Result.Content.ReadAsAsync<PurchaseIndicatorResponse>().Result;

                                if (purchaseIndicatorResponse.Status == ResponseStatus.Error)
                                {

                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SavePoIndicatorWindow Method (SavePoIndicatorDetail): " + purchaseIndicatorResponse.ErrorMessage.ToString());

                                    MessageBox.Show("Error: " + purchaseIndicatorResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);

                                }

                                else
                                {

                                    isCloseForm = true;

                                }

                            }

                            else
                            {

                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not saved into po indicator details Table");

                                MessageBox.Show("Error: Data does not saved into po indicator details Table", Resources.STR_MESSAGE_TITLE);

                            }

                        }

                    }

                }



            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SavePoIndicatorWindow Method: " + ex.Message.ToString());

                MessageBox.Show("Error: Could not save PO Indicators details- " + ex.Message, Resources.STR_MESSAGE_TITLE);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PurchaseDetails");

                logMessage = null;



                if (isCloseForm)
                {

                    poIndicatorsForm.PoIndicators.Close();

                }

            }

        }



        private void AssignPoIndicatorWindowToDexTable()
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                poIndicatorsForm.Tables.PoLineIndicatorTemp.Close();

                poIndicatorsForm.Tables.PoLineIndicatorTemp.Release();

                poIndicatorsForm.Tables.PoLineIndicatorTemp.PoNumber.Value = popPoItemDetailEntryForm.PopPoItemDetailEntry.PoNumber.Value;

                poIndicatorsForm.Tables.PoLineIndicatorTemp.Ord.Value = popPoItemDetailEntryForm.PopPoItemDetailEntry.Ord.Value;



                TableError tableError = poIndicatorsForm.Tables.PoLineIndicatorTemp.Get();

                if (tableError == TableError.NoError || tableError == TableError.NotFound)
                {

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.PoNumber.Value = poIndicatorsForm.PoIndicators.LocalPoNumber.Value;

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.Ord.Value = popPoItemDetailEntryForm.PopPoItemDetailEntry.Ord.Value;

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.ItemNumber.Value = poIndicatorsForm.PoIndicators.LocalItemNumber.Value;

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.PoNewStatusDdl.Value = poIndicatorsForm.PoIndicators.PoNewStatusDdl.Value;

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.PoBackOrderReasonDdl.Value = poIndicatorsForm.PoIndicators.PoBackOrderReasonDdl.Value;

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.BackOrderDate.Value = poIndicatorsForm.PoIndicators.LocalBackOrderDate.Value;

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.PoCancelledReasonDdl.Value = poIndicatorsForm.PoIndicators.PoCancelledReasonDdl.Value;

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.CancelledDate.Value = poIndicatorsForm.PoIndicators.LocalCancelledDate.Value;

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.CpHasCostVariance.Value = poIndicatorsForm.PoIndicators.LocalHasCostVariance.Value;

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.AcknowledgementDate.Value = poIndicatorsForm.PoIndicators.LocalAcknowledgementDate.Value;

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.ConfirmedDate.Value = poIndicatorsForm.PoIndicators.LocalConfirmedDate.Value;

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.ActualShipDate.Value = poIndicatorsForm.PoIndicators.LocalActualShipDate.Value;

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.Save();

                }

                poIndicatorsForm.Tables.PoLineIndicatorTemp.Release();

                poIndicatorsForm.Tables.PoLineIndicatorTemp.Close();

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In AssignPoIndicatorWindowToDexTable Method: " + ex.Message.ToString());

                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PurchaseDetails");

                logMessage = null;

            }

        }



        private PurchaseIndicatorRequest PoIndicatorDexTableToobject()
        {

            StringBuilder logMessage = new StringBuilder();

            PurchaseIndicatorRequest poIndicatorRequest = new PurchaseIndicatorRequest();

            try
            {

                PurchaseIndicatorEntity poIndicatorEntity = new PurchaseIndicatorEntity();

                poIndicatorEntity.PoNumber = poIndicatorsForm.Tables.PoLineIndicatorTemp.PoNumber.Value;

                poIndicatorEntity.POLineNumber = poIndicatorsForm.Tables.PoLineIndicatorTemp.Ord.Value;

                poIndicatorEntity.ItemNumber = poIndicatorsForm.Tables.PoLineIndicatorTemp.ItemNumber.Value;

                poIndicatorEntity.POIndicatorStatusId = poIndicatorsForm.Tables.PoLineIndicatorTemp.PoNewStatusDdl.Value;

                poIndicatorEntity.BackOrderReason = poIndicatorsForm.Tables.PoLineIndicatorTemp.PoBackOrderReasonDdl.Value;

                poIndicatorEntity.InitialBackOrderDate = poIndicatorsForm.Tables.PoLineIndicatorTemp.BackOrderDate.Value;

                poIndicatorEntity.CancelledReason = poIndicatorsForm.Tables.PoLineIndicatorTemp.PoCancelledReasonDdl.Value;

                poIndicatorEntity.InitialCancelledDate = poIndicatorsForm.Tables.PoLineIndicatorTemp.CancelledDate.Value;

                poIndicatorEntity.IsCostVariance = poIndicatorsForm.Tables.PoLineIndicatorTemp.CpHasCostVariance.Value;

                poIndicatorEntity.AcknowledgementDate = poIndicatorsForm.Tables.PoLineIndicatorTemp.AcknowledgementDate.Value;

                poIndicatorEntity.ConfirmedDate = poIndicatorsForm.Tables.PoLineIndicatorTemp.ConfirmedDate.Value;

                poIndicatorEntity.ActualShipDate = poIndicatorsForm.Tables.PoLineIndicatorTemp.ActualShipDate.Value;

                int status;

                int.TryParse(Dynamics.Globals.UserId.Value.ToString(), out status);

                poIndicatorEntity.CreatedBy = status;

                poIndicatorEntity.CreatedOn = Dynamics.Globals.UserDate.Value;

                int.TryParse(Dynamics.Globals.UserId.Value.ToString(), out status);

                poIndicatorEntity.ModifiedBy = status;

                poIndicatorEntity.ModifiedOn = Dynamics.Globals.UserDate.Value;

                poIndicatorRequest.PurchaseIndicatorEntity = poIndicatorEntity;



                poIndicatorRequest.CompanyID = Dynamics.Globals.CompanyId.Value;

                poIndicatorRequest.UserId = Dynamics.Globals.UserId.Value.ToString();

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoIndicatorDexTableToobject Method: " + ex.Message.ToString());

                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PurchaseDetails");

                logMessage = null;

            }

            return poIndicatorRequest;

        }



        void ValidateFieldValue()
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                if (poIndicatorsForm.PoIndicators.PoNewStatusDdl.Value == 1)
                {

                    if (poIndicatorsForm.PoIndicators.LocalAcknowledgementDate.Value.ToString() != "1/1/1900 12:00:00 AM")

                        SavePoIndicatorWindow();

                    else

                        MessageBox.Show("Please enter acknowledge date");

                }

                if (poIndicatorsForm.PoIndicators.PoNewStatusDdl.Value == 2)
                {

                    if (poIndicatorsForm.PoIndicators.LocalConfirmedDate.Value.ToString() == "1/1/1900 12:00:00 AM")

                        MessageBox.Show("Please enter confirm date");

                    else

                        SavePoIndicatorWindow();

                }

                if (poIndicatorsForm.PoIndicators.PoNewStatusDdl.Value == 3)
                {

                    if (poIndicatorsForm.PoIndicators.LocalBackOrderDate.Value.ToString() == "1/1/1900 12:00:00 AM" || poIndicatorsForm.PoIndicators.PoBackOrderReasonDdl.Value == 0)

                        MessageBox.Show("Please enter back order details");

                    else

                        SavePoIndicatorWindow();

                }

                if (poIndicatorsForm.PoIndicators.PoNewStatusDdl.Value == 4)
                {

                    if (poIndicatorsForm.PoIndicators.LocalCancelledDate.Value.ToString() == "1/1/1900 12:00:00 AM" || poIndicatorsForm.PoIndicators.PoCancelledReasonDdl.Value == 0)

                        MessageBox.Show("Please enter cancel order details");

                    else

                        SavePoIndicatorWindow();

                }

                if (poIndicatorsForm.PoIndicators.PoNewStatusDdl.Value == 5)
                {

                    SavePoIndicatorWindow();

                }

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ValidateFieldValue Method: " + ex.Message.ToString());

                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PurchaseDetails");

                logMessage = null;

            }

        }



        private void StatusChange()
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                if (poIndicatorsForm.PoIndicators.PoNewStatusDdl.Value == 1)
                {

                    poIndicatorsForm.PoIndicators.LocalAcknowledgementDate.Enable();

                    poIndicatorsForm.PoIndicators.LocalAcknowledgementDate.Value = DateTime.Today;

                    poIndicatorsForm.PoIndicators.LocalConfirmedDate.Clear();

                    poIndicatorsForm.PoIndicators.LocalConfirmedDate.Disable();

                    poIndicatorsForm.PoIndicators.LocalActualShipDate.Disable();

                    poIndicatorsForm.PoIndicators.PoBackOrderReasonDdl.Value = 0;

                    poIndicatorsForm.PoIndicators.LocalBackOrderDate.Clear();

                    poIndicatorsForm.PoIndicators.PoCancelledReasonDdl.Value = 0;

                    poIndicatorsForm.PoIndicators.LocalCancelledDate.Clear();

                    poIndicatorsForm.PoIndicators.PoBackOrderReasonDdl.Disable();

                    poIndicatorsForm.PoIndicators.LocalBackOrderDate.Disable();

                    poIndicatorsForm.PoIndicators.PoCancelledReasonDdl.Disable();

                    poIndicatorsForm.PoIndicators.LocalCancelledDate.Disable();

                    poIndicatorsForm.PoIndicators.LocalActualShipDate.Clear();

                }

                else if (poIndicatorsForm.PoIndicators.PoNewStatusDdl.Value == 2)
                {

                    poIndicatorsForm.PoIndicators.LocalAcknowledgementDate.Clear();

                    poIndicatorsForm.PoIndicators.LocalAcknowledgementDate.Disable();

                    poIndicatorsForm.PoIndicators.LocalConfirmedDate.Enable();

                    poIndicatorsForm.PoIndicators.LocalConfirmedDate.Value = DateTime.Today;

                    poIndicatorsForm.PoIndicators.LocalActualShipDate.Disable();

                    poIndicatorsForm.PoIndicators.PoBackOrderReasonDdl.Value = 0;

                    poIndicatorsForm.PoIndicators.LocalBackOrderDate.Clear();

                    poIndicatorsForm.PoIndicators.PoCancelledReasonDdl.Value = 0;

                    poIndicatorsForm.PoIndicators.LocalCancelledDate.Clear();

                    poIndicatorsForm.PoIndicators.PoBackOrderReasonDdl.Disable();

                    poIndicatorsForm.PoIndicators.LocalBackOrderDate.Disable();

                    poIndicatorsForm.PoIndicators.PoCancelledReasonDdl.Disable();

                    poIndicatorsForm.PoIndicators.LocalCancelledDate.Disable();

                    poIndicatorsForm.PoIndicators.LocalActualShipDate.Clear();

                }

                else if (poIndicatorsForm.PoIndicators.PoNewStatusDdl.Value == 3)
                {

                    poIndicatorsForm.PoIndicators.LocalAcknowledgementDate.Clear();

                    poIndicatorsForm.PoIndicators.LocalAcknowledgementDate.Disable();

                    poIndicatorsForm.PoIndicators.LocalConfirmedDate.Clear();

                    poIndicatorsForm.PoIndicators.LocalConfirmedDate.Disable();

                    poIndicatorsForm.PoIndicators.LocalActualShipDate.Disable();

                    poIndicatorsForm.PoIndicators.PoBackOrderReasonDdl.Enable();

                    poIndicatorsForm.PoIndicators.LocalBackOrderDate.Enable();

                    poIndicatorsForm.PoIndicators.LocalBackOrderDate.Value = DateTime.Today;

                    poIndicatorsForm.PoIndicators.PoCancelledReasonDdl.Value = 0;

                    poIndicatorsForm.PoIndicators.LocalCancelledDate.Clear();

                    poIndicatorsForm.PoIndicators.PoCancelledReasonDdl.Disable();

                    poIndicatorsForm.PoIndicators.LocalCancelledDate.Disable();

                    poIndicatorsForm.PoIndicators.LocalActualShipDate.Clear();

                }

                else if (poIndicatorsForm.PoIndicators.PoNewStatusDdl.Value == 4)
                {

                    poIndicatorsForm.PoIndicators.LocalAcknowledgementDate.Clear();

                    poIndicatorsForm.PoIndicators.LocalAcknowledgementDate.Disable();

                    poIndicatorsForm.PoIndicators.LocalConfirmedDate.Clear();

                    poIndicatorsForm.PoIndicators.LocalConfirmedDate.Disable();

                    poIndicatorsForm.PoIndicators.LocalActualShipDate.Disable();

                    poIndicatorsForm.PoIndicators.PoBackOrderReasonDdl.Value = 0;

                    poIndicatorsForm.PoIndicators.LocalBackOrderDate.Clear();

                    poIndicatorsForm.PoIndicators.PoBackOrderReasonDdl.Disable();

                    poIndicatorsForm.PoIndicators.LocalBackOrderDate.Disable();

                    poIndicatorsForm.PoIndicators.PoCancelledReasonDdl.Enable();

                    poIndicatorsForm.PoIndicators.LocalCancelledDate.Enable();

                    poIndicatorsForm.PoIndicators.LocalCancelledDate.Value = DateTime.Today;

                    poIndicatorsForm.PoIndicators.LocalActualShipDate.Clear();

                }

                else if (poIndicatorsForm.PoIndicators.PoNewStatusDdl.Value == 5)
                {

                    poIndicatorsForm.PoIndicators.LocalAcknowledgementDate.Clear();

                    poIndicatorsForm.PoIndicators.LocalAcknowledgementDate.Disable();

                    poIndicatorsForm.PoIndicators.LocalConfirmedDate.Clear();

                    poIndicatorsForm.PoIndicators.LocalConfirmedDate.Disable();

                    poIndicatorsForm.PoIndicators.LocalActualShipDate.Enable();

                    poIndicatorsForm.PoIndicators.LocalActualShipDate.Value = DateTime.Today;

                    poIndicatorsForm.PoIndicators.PoBackOrderReasonDdl.Value = 0;

                    poIndicatorsForm.PoIndicators.LocalBackOrderDate.Clear();

                    poIndicatorsForm.PoIndicators.PoCancelledReasonDdl.Value = 0;

                    poIndicatorsForm.PoIndicators.LocalCancelledDate.Clear();

                    poIndicatorsForm.PoIndicators.PoBackOrderReasonDdl.Disable();

                    poIndicatorsForm.PoIndicators.LocalBackOrderDate.Disable();

                    poIndicatorsForm.PoIndicators.PoCancelledReasonDdl.Disable();

                    poIndicatorsForm.PoIndicators.LocalCancelledDate.Disable();

                }

                else
                {

                    poIndicatorsForm.PoIndicators.LocalAcknowledgementDate.Clear();

                    poIndicatorsForm.PoIndicators.LocalAcknowledgementDate.Disable();

                    poIndicatorsForm.PoIndicators.LocalConfirmedDate.Clear();

                    poIndicatorsForm.PoIndicators.LocalConfirmedDate.Disable();

                    poIndicatorsForm.PoIndicators.LocalActualShipDate.Clear();

                    poIndicatorsForm.PoIndicators.LocalActualShipDate.Disable();

                    poIndicatorsForm.PoIndicators.PoBackOrderReasonDdl.Value = 0;

                    poIndicatorsForm.PoIndicators.LocalBackOrderDate.Clear();

                    poIndicatorsForm.PoIndicators.PoCancelledReasonDdl.Value = 0;

                    poIndicatorsForm.PoIndicators.LocalCancelledDate.Clear();

                    poIndicatorsForm.PoIndicators.PoBackOrderReasonDdl.Disable();

                    poIndicatorsForm.PoIndicators.LocalBackOrderDate.Disable();

                    poIndicatorsForm.PoIndicators.PoCancelledReasonDdl.Disable();

                    poIndicatorsForm.PoIndicators.LocalCancelledDate.Disable();

                }

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In StatusChange Method: " + ex.Message.ToString());

                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PurchaseDetails");

                logMessage = null;

            }

        }



        private void ClearPOindicatorDetails(string formName, string poNumber, int poLinenumber)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                if (formName == Resources.STR_POIndicatorForm)
                {

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.Close();

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.Release();



                    TableError tempError = poIndicatorsForm.Tables.PoLineIndicatorTemp.ChangeFirst();

                    while (tempError == TableError.NoError && tempError != TableError.EndOfTable)
                    {

                        if ((poNumber == "" || poNumber != poIndicatorsForm.Tables.PoLineIndicatorTemp.PoNumber.Value) ||

                            (poLinenumber == 0 || poLinenumber != poIndicatorsForm.Tables.PoLineIndicatorTemp.Ord.Value))
                        {

                            poIndicatorsForm.Tables.PoLineIndicatorTemp.Remove();

                        }

                        tempError = poIndicatorsForm.Tables.PoLineIndicatorTemp.ChangeNext();

                    }

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.Release();

                    poIndicatorsForm.Tables.PoLineIndicatorTemp.Close();

                }

                else if (formName == Resources.STR_POIndicatorInquiry)
                {

                    poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Close();

                    poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Release();



                    TableError tempError = poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.ChangeFirst();

                    while (tempError == TableError.NoError && tempError != TableError.EndOfTable)
                    {

                        if ((poNumber == "" || poNumber != poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.PoNumber.Value) ||

                            (poLinenumber == 0 || poLinenumber != poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Ord.Value))
                        {
                            poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Remove();

                        }

                        tempError = poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.ChangeNext();

                    }

                    poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Release();

                    poIndicatorsInquiryForm.Tables.PoLineIndicatorTemp.Close();

                }

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearPOindicatorDetails Method: " + ex.Message.ToString());

                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PurchaseDetails");

                logMessage = null;

            }

        }



        void PopPoItemDetailEntryDeleteButton_ClickAfterOriginal(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                PurchaseIndicatorRequest poIndicatorRequest = new PurchaseIndicatorRequest();

                poIndicatorRequest.CompanyID = Dynamics.Globals.CompanyId.Value;

                PurchaseOrderInformation poIndicatorBase = new PurchaseOrderInformation();

                poIndicatorBase.PoNumber = popPoItemDetailEntryForm.PopPoItemDetailEntry.PoNumber.Value;

                poIndicatorBase.PoLineNumber = popPoItemDetailEntryForm.PopPoItemDetailEntry.Ord.Value;

                poIndicatorRequest.PoIndicatorBase = poIndicatorBase;



                if (poIndicatorRequest != null)
                {

                    // Service call ...

                    using (HttpClient client = new HttpClient())
                    {

                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);

                        client.DefaultRequestHeaders.Accept.Clear();

                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));



                        var response = client.PostAsJsonAsync("api/PurchaseOrder/DeletePoIndicatorDetail", poIndicatorRequest); // we need to refer the web.api service url here.

                        if (response.Result.IsSuccessStatusCode)
                        {

                            PurchaseIndicatorResponse purchaseIndicatorResponse = response.Result.Content.ReadAsAsync<PurchaseIndicatorResponse>().Result;

                            if (purchaseIndicatorResponse.Status == ResponseStatus.Error)
                            {

                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertPOIndicatorDetails Method (GetPoIndicatorDetail): " + purchaseIndicatorResponse.ErrorMessage.ToString());

                                MessageBox.Show("Error: " + purchaseIndicatorResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);

                            }

                        }

                        else
                        {

                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Delete PO Line Item Table");

                            MessageBox.Show("Error: Delete PO Line Item Table", Resources.STR_MESSAGE_TITLE);

                        }

                    }

                }

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In DeleteButtonClickAfterOriginal Method: " + ex.Message.ToString());

                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PurchaseDetails");

                logMessage = null;

            }

        }



        #endregion POIndicatorWindow



        #region PO CostMgt



        void PoNumber_Change(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {
                //default BuyerID to improve user experience
               // PoNumber_ChangeAfterOriginal();
                //assign to local field

                popPOEntryForm.PopPoEntry.LocalCpShipMthd.Value = popPOEntryForm.Tables.PopPo.ShippingMethod.Value;



                InsertCostDetails();   // load data into temp table for po cost management window



            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoNumber_Change Method: " + ex.Message.ToString());

                MessageBox.Show("Error: 15 " + ex.Message);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }

        #region Default BuyerID to improve user experience

        void ValidatePurchaseBuyerId()
        {
            StringBuilder logMessage = new StringBuilder();
            PurchaseOrderRequest purchaseOrderRequest = null;
            PurchaseOrderInformation purchaseOrderInformation = null;

            purchaseOrderRequest = new PurchaseOrderRequest();
            purchaseOrderInformation = new PurchaseOrderInformation();
            purchaseOrderInformation.PoNumber = popPOEntryForm.PopPoEntry.PoNumber.Value.ToString().Trim();
            purchaseOrderRequest.PurchaseOrderInformation = purchaseOrderInformation;
            purchaseOrderRequest.CompanyID = Dynamics.Globals.CompanyId.Value;
            try
            {
                if ((popPOEntryForm.PopPoEntry.PoNumber.Value != null) && (popPOEntryForm.PopPoEntry.PoNumber.Value != string.Empty))
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = client.PostAsJsonAsync("api/PurchaseOrder/ValidatePurchaseBuyerId", purchaseOrderRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            PurchaseOrderResponse purchaseOrderResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                            if (purchaseOrderResponse.Status == ResponseStatus.Error)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ValidateCostDetails Method (ValidatePoCostChanges): " + purchaseOrderResponse.ErrorMessage.ToString());
                                MessageBox.Show(purchaseOrderResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            POStatus = purchaseOrderResponse.IsAvailable;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoType_Change Method: " + ex.Message.ToString());
                MessageBox.Show("Error: 15 " + ex.Message);
            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PO default BuyerID to improve user experience");
                logMessage = null;

            }

        }

        void PoType_Change(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();
            try
            {
                if ((popPOEntryForm.PopPoEntry.PoNumber.Value != null) && (popPOEntryForm.PopPoEntry.PoNumber.Value != string.Empty))
                {
                    ValidatePurchaseBuyerId();
                    if (POStatus == 0)
                    {
                        if (popPOEntryForm.PopPoEntry.PoType.Value == 2)
                        {
                            if (popPOEntryForm.PopPoEntry.BuyerId.Value.ToString().Trim() == Dynamics.Globals.UserId.Value.ToString().Trim())
                            {
                                popPOEntryForm.PopPoEntry.BuyerId.Value = null;
                            }
                        }
                        else
                        {
                            if ((popPOEntryForm.PopPoEntry.BuyerId.Value == null) || (popPOEntryForm.PopPoEntry.BuyerId.Value == string.Empty))
                            {
                                if (popPOEntryForm.PopPoEntry.LocationCode.Value.ToString().Trim() != "DROPSHIP")
                                {
                                    popPOEntryForm.PopPoEntry.BuyerId.Value = Dynamics.Globals.UserId.Value.ToString().Trim();
                                    popPOEntryForm.PopPoEntry.BuyerId.Focus();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoType_Change Method: " + ex.Message.ToString());
                MessageBox.Show("Error: 15 " + ex.Message);
            }

            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PO default BuyerID to improve user experience");
                logMessage = null;
            }

        }

        private void BuyerId_LeaveAfterOriginal(object sender, EventArgs e)
        {
            popPOEntryForm.PopPoEntry.BuyerId.RunValidate();
        }

        private void PoNumber_LeaveAfterOriginal(object sender, EventArgs e)
        {
            //default BuyerID to improve user experience
            PoNumber_ChangeAfterOriginal();
        }

        void LocationCode_LeaveAfterOriginal(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                if ((popPOEntryForm.PopPoEntry.PoNumber.Value != null) && (popPOEntryForm.PopPoEntry.PoNumber.Value != string.Empty))
                {
                    ValidatePurchaseBuyerId();

                    if (POStatus == 0)
                    {
                        if (popPOEntryForm.PopPoEntry.BuyerId.Value.ToString().Trim() == Dynamics.Globals.UserId.Value.ToString().Trim())
                        {
                            if ((popPOEntryForm.PopPoEntry.LocationCode.Value.ToString().Trim() == "DROPSHIP") || (popPOEntryForm.PopPoEntry.PoType.Value == 2))
                            {
                                popPOEntryForm.PopPoEntry.BuyerId.Value = null;
                            }
                            else
                            {
                                if ((popPOEntryForm.PopPoEntry.BuyerId.Value == null) || (popPOEntryForm.PopPoEntry.BuyerId.Value == string.Empty))
                                {
                                    popPOEntryForm.PopPoEntry.BuyerId.Value = Dynamics.Globals.UserId.Value.ToString().Trim();
                                    popPOEntryForm.PopPoEntry.BuyerId.Focus();
                                }
                            }
                        }
                        else
                        {
                            if ((popPOEntryForm.PopPoEntry.BuyerId.Value == null) || (popPOEntryForm.PopPoEntry.BuyerId.Value == string.Empty))
                            {
                                if ((popPOEntryForm.PopPoEntry.LocationCode.Value.ToString().Trim() == "DROPSHIP") || (popPOEntryForm.PopPoEntry.PoType.Value == 2))
                                {
                                    popPOEntryForm.PopPoEntry.BuyerId.Value = null;
                                }
                                else
                                {
                                    popPOEntryForm.PopPoEntry.BuyerId.Value = Dynamics.Globals.UserId.Value.ToString().Trim();
                                    popPOEntryForm.PopPoEntry.BuyerId.Focus();

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoNumber_LocationCode Method: " + ex.Message.ToString());
                MessageBox.Show("Error: 15 " + ex.Message);
            }

            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PO default BuyerID to improve user experience");
                logMessage = null;
            }

        }


        void PoNumber_ChangeAfterOriginal()
        {

            StringBuilder logMessage = new StringBuilder();
            try
            {
                ValidatePurchaseBuyerId();
                if (POStatus == 0)
                {
                    popPOEntryForm.PopPoEntry.CpPurchaseOrderType.Value = 1;
                    if (popPOEntryForm.PopPoEntry.BuyerId.Value.ToString().Trim() == Dynamics.Globals.UserId.Value.ToString().Trim())
                    {
                        if (popPOEntryForm.PopPoEntry.PoType.Value == 2)
                        {
                            popPOEntryForm.PopPoEntry.BuyerId.Value = null;
                        }
                        else
                        {
                            if ((popPOEntryForm.PopPoEntry.LocationCode.Value.ToString().Trim() != "DROPSHIP") && (popPOEntryForm.PopPoEntry.PoType.Value != 2))
                            {
                                popPOEntryForm.PopPoEntry.BuyerId.Value = Dynamics.Globals.UserId.Value.ToString().Trim();
                                popPOEntryForm.PopPoEntry.BuyerId.Focus();
                            }
                        }
                    }
                    else
                    {
                        if ((popPOEntryForm.PopPoEntry.LocationCode.Value.ToString().Trim() != "DROPSHIP") && (popPOEntryForm.PopPoEntry.PoType.Value != 2))
                        {
                            popPOEntryForm.PopPoEntry.BuyerId.Value = Dynamics.Globals.UserId.Value;
                            popPOEntryForm.PopPoEntry.BuyerId.Focus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoNumber_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: 15 " + ex.Message);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PO default BuyerID to improve user experience");
                logMessage = null;
            }
        }
        #endregion Default BuyerID to improve user experience






        /// <summary>

        ///  This event occurs when the pop po entry form open

        /// </summary>

        /// <param name="sender"></param>

        /// <param name="e"></param>

        void PopPoEntry_OpenBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                if (!poEventsRegistered)
                {

                    popPOEntryForm.PopPoEntry.LineScroll.LineEnterAfterOriginal += new EventHandler(PopPoEntryLineScroll_LineEnterAfterOriginal);

                    popPOEntryForm.PopPoEntry.LineScroll.LineLeaveAfterOriginal += new EventHandler(LineScroll_LineLeaveAfterOriginal);



                    popPOEntryForm.PopPoEntry.LineScroll.UnitCost.Change += new EventHandler(PoLineScrollUnitCost_Change);

                    popPOEntryForm.PopPoEntry.LineScroll.QtyOrdered.Change += new EventHandler(PoLineScrollQtyOrdered_Change);

                    popPOEntryForm.PopPoEntry.LineScroll.QtyCanceled.Change += new EventHandler(PoLineScrollQtyCanceled_Change);

                    popPOEntryForm.PopPoEntry.LineScroll.UOfM.Change += new EventHandler(PoLineScrollUOfM_Change);



                    poEventsRegistered = true;



                    // triggers when user click on save button

                    ChempointCustomizations.Procedures.PopSavePre.InvokeBeforeOriginal += new Dic1311.PopSavePreProcedure.InvokeEventHandler(PopSavePre_InvokeBeforeOriginal);
                    popPOEntryForm.Functions.CpPopPoEntryFormValidateLines.InvokeAfterOriginal += CpPopPoEntryFormValidateLines_InvokeAfterOriginal;


                    // triggers when user click on close button

                    ChempointCustomizations.Procedures.PopClosePre.InvokeBeforeOriginal += new Dic1311.PopClosePreProcedure.InvokeEventHandler(PopClosePre_InvokeBeforeOriginal);                    

                    // triggers when user click on navigate button

                    ChempointCustomizations.Procedures.PopNavigationPre.InvokeBeforeOriginal += new Dic1311.PopNavigationPreProcedure.InvokeEventHandler(PopNavigationPre_InvokeBeforeOriginal);
                    ChempointCustomizations.Procedures.PopNavigationPre.InvokeAfterOriginal += PopNavigationPre_InvokeAfterOriginal;


                    // triggers when user click on print button

                    ChempointCustomizations.Procedures.PopPrintPre.InvokeBeforeOriginal += new Dic1311.PopPrintPreProcedure.InvokeEventHandler(PopPrintPre_InvokeBeforeOriginal);
                    ChempointCustomizations.Procedures.PopPrintPre.InvokeAfterOriginal += PopPrintPre_InvokeAfterOriginal;


                    // triggers when user click on action button

                    ChempointCustomizations.Procedures.PopActionsPre.InvokeBeforeOriginal += new Dic1311.PopActionsPreProcedure.InvokeEventHandler(PopActionsPre_InvokeBeforeOriginal);
                    ChempointCustomizations.Procedures.PopActionsPre.InvokeAfterOriginal += PopActionsPre_InvokeAfterOriginal;


                    // triggers when user click on save button of pop item details window

                    //ChempointCustomizations.Procedures.PopPoItemDetailEntrySave.InvokeAfterOriginal += new Dic1311.PopPoItemDetailEntrySaveProcedure.InvokeEventHandler(PopPoItemDetailEntrySave_InvokeAfterOriginal);



                    popPoItemDetailEntryForm.PopPoItemDetailEntry.LocalPoCostManagementSave.Change += PopPoItemDetailEntrySave_InvokeAfterOriginal;



                    // triggers when user change qty ordered from pop item details window

                    ChempointCustomizations.Procedures.CpprPoItemDetailEntryCostBook.InvokeBeforeOriginal += new Dic1311.CpprPoItemDetailEntryCostBookProcedure.InvokeEventHandler(CpprPoItemDetailEntryCostBook_InvokeBeforeOriginal);



                }

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PopPoEntry_OpenBeforeOriginal Method: " + ex.Message.ToString());

                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }

        private void PopActionsPre_InvokeAfterOriginal(object sender, Dic1311.PopActionsPreProcedure.InvokeEventArgs e)
        {
            ValidateCostVarianceBeforeOpeartion();
        }

        private void PopPrintPre_InvokeAfterOriginal(object sender, Dic1311.PopPrintPreProcedure.InvokeEventArgs e)
        {
            ValidateCostVarianceBeforeOpeartion();
        }

        private void PopNavigationPre_InvokeAfterOriginal(object sender, Dic1311.PopNavigationPreProcedure.InvokeEventArgs e)
        {
            ValidateCostVarianceBeforeOpeartion();
        }
               

        private void ValidateCostVarianceBeforeOpeartion()
        {
            bool isCostVariance = false;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                //for (int i = 0; i < costVarianceDT.Rows.Count; i++)
                //{
                //    if (Convert.ToUInt32(costVarianceDT.Rows[i]["CostVariance"]) > 20 ||
                //        Convert.ToInt32(costVarianceDT.Rows[i]["CostVariance"]) < -20)
                //    {
                //        isCostVariance = true;
                //        break;
                //    }
                //}

                //if (isCostVariance ==true)
                //MessageBox.Show(string.Format(Resources.STR_MailBodyForCostVariance, popPOEntryForm.PopPoEntry.PoNumber.Value)
                //    , Resources.STR_MESSAGE_TITLE);
                //else
                //    MessageBox.Show(string.Format(Resources.STR_MailBodyForFirstPo, popPOEntryForm.PopPoEntry.PoNumber.Value)
                //       , Resources.STR_MESSAGE_TITLE);

                //if (isCostVariance == true)
                //    popPOEntryForm.Procedures.CpPopPoEntryFormRejectrecord.Invoke();  
                isMaterialAction = true;
                FetchingMaterialManagementDetails(isMaterialAction);
                popPOEntryForm.Procedures.CpPopPoEntryFormRejectrecord.Invoke();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CpPopPoEntryFormValidateLines_InvokeAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
                isCostVariance = false;
            }
        }

        private void CpPopPoEntryFormValidateLines_InvokeAfterOriginal(object sender, Dic1311.PopPoEntryForm.CpPopPoEntryFormValidateLinesFunction.InvokeEventArgs e)
        {
            bool isCostVariance = false;
            decimal costVariance = 0;
            decimal costVarianceNegative = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                //ChempointCustomizations.Tables.CpSetp.Close();
                //ChempointCustomizations.Tables.CpSetp.Release();
                //ChempointCustomizations.Tables.CpSetp.SetupKey.Value = 1;
                //ChempointCustomizations.Tables.CpSetp.Key = 1;
                //TableError tablerror = ChempointCustomizations.Tables.CpSetp.Get();
                //if (tablerror == TableError.NoError)
                //{
                //    costVariance = ChempointCustomizations.Tables.CpSetp.CpPoCostVariancePercentage.Value;
                //    costVarianceNegative = costVariance * -1;
                //}
                //ChempointCustomizations.Tables.CpSetp.Release();
                //ChempointCustomizations.Tables.CpSetp.Close();               

                //for (int i = 0; i < costVarianceDT.Rows.Count; i++)
                //{
                //    if(Convert.ToDecimal(costVarianceDT.Rows[i]["CostVariance"]) > costVariance || 
                //        Convert.ToDecimal(costVarianceDT.Rows[i]["CostVariance"]) < costVarianceNegative)
                //    {
                //        isCostVariance = true;
                //        break;
                //    }
                //}

                //if (isCostVariance==true)
                //{
                //    FetchingMaterialManagementDetails();
                //}
                isMaterialAction = false;
                FetchingMaterialManagementDetails(isMaterialAction);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CpPopPoEntryFormValidateLines_InvokeAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
                isCostVariance = false;
            }
        }



        /// <summary>

        ///  This event occurs when the line of po scroll window receives focus

        /// </summary>

        /// <param name="sender"></param>

        /// <param name="e"></param>

        void PopPoEntryLineScroll_LineEnterAfterOriginal(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                popPOEntryForm.PopPoEntry.LineScroll.LocalPoUnitCost.Value = popPOEntryForm.PopPoEntry.LineScroll.UnitCost.Value;

                popPOEntryForm.PopPoEntry.LineScroll.LocalQtyOrdered.Value = popPOEntryForm.PopPoEntry.LineScroll.QtyOrdered.Value;

                popPOEntryForm.PopPoEntry.LineScroll.LocalQtyCanceled.Value = popPOEntryForm.PopPoEntry.LineScroll.QtyCanceled.Value;

                popPOEntryForm.PopPoEntry.LineScroll.LocalUOfM.Value = popPOEntryForm.PopPoEntry.LineScroll.UOfM.Value;

                lineEnterFlag = true;



            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PopPoEntryLineScroll_LineEnterAfterOriginal Method: " + ex.Message.ToString());

                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }



        /// <summary>

        ///  This event occurs when the line of po scroll window leaves focus

        /// </summary>

        /// <param name="sender"></param>

        /// <param name="e"></param>

        void LineScroll_LineLeaveAfterOriginal(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                if (!String.IsNullOrEmpty(popPOEntryForm.PopPoEntry.LineScroll.ItemNumber.Value))   // Need to check if item number is blank or not. reason- it keeps focus on item number if value is blank
                {

                    lineEnterFlag = false;



                    popPOEntryForm.PopPoEntry.LineScroll.LocalPoUnitCost.Value = 0;

                    popPOEntryForm.PopPoEntry.LineScroll.LocalQtyOrdered.Value = 0;

                    popPOEntryForm.PopPoEntry.LineScroll.LocalQtyCanceled.Value = 0;

                    popPOEntryForm.PopPoEntry.LineScroll.LocalUOfM.Value = "";

                }

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In LineScroll_LineLeaveAfterOriginal Method: " + ex.Message.ToString());

                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }



        /// <summary>

        ///  This event occurs when the unit cost of po scroll window receives changes

        /// </summary>

        /// <param name="sender"></param>

        /// <param name="e"></param>

        void PoLineScrollUnitCost_Change(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                ValidateUnitCostChange(popPOEntryForm.PopPoEntry.PoNumber.Value, popPOEntryForm.PopPoEntry.LineScroll.LineNumber.Value,

                    popPOEntryForm.PopPoEntry.LineScroll.QtyOrdered.Value, popPOEntryForm.PopPoEntry.LineScroll.ItemNumber.Value,

                    popPOEntryForm.PopPoEntry.LineScroll.UnitCost.Value, popPOEntryForm.PopPoEntry.LineScroll.LocalPoUnitCost.Value, "LineScroll");

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoLineScrollUnitCost_Change Method: " + ex.Message.ToString());

                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }



        /// <summary>

        ///  This event occurs when the qty ordered po scroll window receives changes

        /// </summary>

        /// <param name="sender"></param>

        /// <param name="e"></param>

        void PoLineScrollQtyOrdered_Change(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                ValidateLineChange(popPOEntryForm.PopPoEntry.PoNumber.Value, popPOEntryForm.PopPoEntry.LineScroll.LineNumber.Value,

                    popPOEntryForm.PopPoEntry.LineScroll.ItemNumber.Value,

                    popPOEntryForm.PopPoEntry.LineScroll.QtyOrdered.Value, popPOEntryForm.PopPoEntry.LineScroll.LocalQtyOrdered.Value, "LineScroll", "QtyOrdered");

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoLineScrollQtyOrdered_Change Method: " + ex.Message.ToString());

                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }



        /// <summary>

        ///  This event occurs when the cancelled qty of po scroll window receives changes

        /// </summary>

        /// <param name="sender"></param>

        /// <param name="e"></param>

        void PoLineScrollQtyCanceled_Change(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                ValidateLineChange(popPOEntryForm.PopPoEntry.PoNumber.Value, popPOEntryForm.PopPoEntry.LineScroll.LineNumber.Value,

                    popPOEntryForm.PopPoEntry.LineScroll.ItemNumber.Value,

                    popPOEntryForm.PopPoEntry.LineScroll.QtyCanceled.Value, popPOEntryForm.PopPoEntry.LineScroll.LocalQtyCanceled.Value, "LineScroll", "QtyCanceled");

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoLineScrollQtyCanceled_Change Method: " + ex.Message.ToString());

                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }



        /// <summary>

        ///  This event occurs when the uofm po scroll window receives changes

        /// </summary>

        /// <param name="sender"></param>

        /// <param name="e"></param>

        void PoLineScrollUOfM_Change(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                ValidateUOfMChange(popPOEntryForm.PopPoEntry.PoNumber.Value, popPOEntryForm.PopPoEntry.LineScroll.LineNumber.Value,

                     popPOEntryForm.PopPoEntry.LineScroll.ItemNumber.Value,

                    popPOEntryForm.PopPoEntry.LineScroll.UOfM.Value, popPOEntryForm.PopPoEntry.LineScroll.LocalUOfM.Value, "LineScroll");

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoLineScrollUOfM_Change Method: " + ex.Message.ToString());

                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }



        /// <summary>

        /// This method will verify whether unit cost is changed in line scroll and track it in temp table

        /// </summary>

        /// <param name="poNumber">Po Number</param>

        /// <param name="lineNumber">Line Count</param>

        /// <param name="qtyOrdered">Order Quantity</param>

        /// <param name="itemNumber">Item Number</param>

        /// <param name="lineUnitCost">Current Unit Cost</param>

        /// <param name="previousLineScrollUnitCost">Previuos Unit Cost</param>

        /// <param name="windowType">Parameter: a.LineScroll b.ItemDetails</param>

        private void ValidateUnitCostChange(string poNumber, short lineNumber, decimal qtyOrdered, string itemNumber, decimal lineUnitCost, decimal previousLineScrollUnitCost, string windowType)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                if (lineEnterFlag == true)
                {

                    if (!String.IsNullOrEmpty(itemNumber) &&

                       lineUnitCost != previousLineScrollUnitCost)
                    {

                        if (windowType == "LineScroll")
                        {



                            popPOEntryForm.PopPoEntry.LineScroll.LocalPoUnitCost.Value = lineUnitCost;



                            ResetCostDetailsByLine(poNumber,

                                                   popPOEntryForm.PopPoEntry.Ord.Value,

                                                   popPOEntryForm.PopPoEntry.LineScroll.UnitCost.Value,

                                                   popPOEntryForm.PopPoEntry.LineScroll.QtyOrdered.Value,

                                                   popPOEntryForm.PopPoEntry.LineScroll.QtyCanceled.Value,

                                                   popPOEntryForm.PopPoEntry.LineScroll.UOfM.Value);

                        }

                    }

                }

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ValidateUnitCostChange Method: " + ex.Message.ToString());

                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }



        /// <summary>

        /// This method will verify whether order or cancel qty is changed in line scroll and track it in temp table

        /// </summary>

        /// <param name="poNumber">Po Number</param>

        /// <param name="lineNumber">Line Count</param>

        /// <param name="itemNumber">Item Number</param>

        /// <param name="currentValue">Current Quantity</param>

        /// <param name="previousValue">Previuos Quantity</param>

        /// <param name="windowType">Parameter: a.LineScroll b.ItemDetails</param>

        /// <param name="windowType">Parameter: a.QtyOrdered b.QtyCanceled</param>

        private void ValidateLineChange(string poNumber, short lineNumber, string itemNumber, decimal currentValue, decimal previousValue, string windowType, string changeType)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {



                if (lineEnterFlag == true)
                {

                    if (!String.IsNullOrEmpty(itemNumber) &&

                       currentValue != previousValue)
                    {

                        if (windowType == "LineScroll")
                        {

                            switch (changeType)
                            {

                                case "QtyOrdered":

                                    popPOEntryForm.PopPoEntry.LineScroll.LocalQtyOrdered.Value = currentValue;

                                    break;

                                case "QtyCanceled":

                                    popPOEntryForm.PopPoEntry.LineScroll.LocalQtyCanceled.Value = currentValue;

                                    break;



                                default:

                                    break;

                            }



                            ResetCostDetailsByLine(poNumber,

                                                   popPOEntryForm.PopPoEntry.Ord.Value,

                                                   popPOEntryForm.PopPoEntry.LineScroll.UnitCost.Value,

                                                   popPOEntryForm.PopPoEntry.LineScroll.QtyOrdered.Value,

                                                   popPOEntryForm.PopPoEntry.LineScroll.QtyCanceled.Value,

                                                   popPOEntryForm.PopPoEntry.LineScroll.UOfM.Value);



                            if (changeType == "QtyOrdered")
                            {

                                CalculateUnitCostByLine(poNumber,

                                                   popPOEntryForm.PopPoEntry.Ord.Value,

                                                   popPOEntryForm.PopPoEntry.LineScroll.QtyOrdered.Value,

                                                   previousValue);



                            }

                        }

                    }

                }



            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ValidateLineChange Method: " + ex.Message.ToString());

                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }



        /// <summary>

        /// This method will verify whether UofM is changed in line scroll and track it in temp table

        /// </summary>

        /// <param name="poNumber">Po Number</param>

        /// <param name="lineNumber">Line Count</param>

        /// <param name="itemNumber">Item Number</param>

        /// <param name="currentValue">Current UofM</param>

        /// <param name="previousValue">Previuos UofM</param>

        /// <param name="windowType">Parameter: a.LineScroll b.ItemDetails</param>

        private void ValidateUOfMChange(string poNumber, short lineNumber, string itemNumber, string currentValue, string previousValue, string windowType)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {



                if (lineEnterFlag == true)
                {

                    if (!String.IsNullOrEmpty(itemNumber) &&

                       currentValue.ToString().ToLower().Trim() != previousValue.ToString().ToLower().Trim())
                    {

                        if (windowType == "LineScroll")
                        {

                            popPOEntryForm.PopPoEntry.LineScroll.LocalUOfM.Value = currentValue;

                            ResetCostDetailsByLine(poNumber,

                                                   popPOEntryForm.PopPoEntry.Ord.Value,

                                                   popPOEntryForm.PopPoEntry.LineScroll.UnitCost.Value,

                                                   popPOEntryForm.PopPoEntry.LineScroll.QtyOrdered.Value,

                                                   popPOEntryForm.PopPoEntry.LineScroll.QtyCanceled.Value,

                                                   popPOEntryForm.PopPoEntry.LineScroll.UOfM.Value);



                        }

                    }

                }



            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ValidateUOfMChange Method: " + ex.Message.ToString());

                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }



        /// <summary>

        /// Calculate cost book cost once ordered quantity changed from qtyordered = 0 to qtyordered > 0

        /// </summary>

        /// <see cref=" Applicable for new line only"/>

        /// <param name="poNumber">Po Number</param>

        /// <param name="ord">Ord Number</param>

        /// <param name="qtyOrdered">Qty Ordered</param>

        /// <param name="previousValue">Old Qty Ordered</param>

        private void CalculateUnitCostByLine(string poNumber, int ord, decimal qtyOrdered, decimal previousValue)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                // To identify new line - check line exists into temp table or not

                popPOEntryForm.Tables.PoCostManagementDetails.Close();

                popPOEntryForm.Tables.PoCostManagementDetails.Release();



                popPOEntryForm.Tables.PoCostManagementDetails.PoNumber.Value = poNumber;

                popPOEntryForm.Tables.PoCostManagementDetails.Ord.Value = ord;

                popPOEntryForm.Tables.PoCostManagementDetails.UserId.Value = Dynamics.Globals.UserId.Value.ToString().Trim();

                popPOEntryForm.Tables.PoCostManagementDetails.Key = 1;



                TableError tempError = popPOEntryForm.Tables.PoCostManagementDetails.Change();

                if (tempError == TableError.NotFound && tempError != TableError.EndOfTable)
                {

                    if (previousValue == 0 && qtyOrdered > 0)
                    {

                        ChempointCustomizations.Procedures.CpprPoEntryLookupCostBook.Invoke();

                    }

                }

                else
                {

                    if (tempError == TableError.NoError && tempError != TableError.EndOfTable)
                    {

                        if (popPOEntryForm.Tables.PoCostManagementDetails.IsPoCostExist.Value == true)
                        {

                            if (previousValue == 0 && qtyOrdered > 0)
                            {

                                ChempointCustomizations.Procedures.CpprPoEntryLookupCostBook.Invoke();

                            }

                        }

                    }

                }



                popPOEntryForm.Tables.PoCostManagementDetails.Close();

                popPOEntryForm.Tables.PoCostManagementDetails.Release();

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CalculateUnitCostByLine Method: " + ex.Message.ToString());

                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }







        /// <summary>

        /// Initialize the controls before save pre events

        /// </summary>

        /// <param name="sender"></param>

        /// <param name="e"></param>

        void PopSavePre_InvokeBeforeOriginal(object sender, Dic1311.PopSavePreProcedure.InvokeEventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                if (!String.IsNullOrEmpty(popPOEntryForm.PopPoEntry.PoNumber.Value))
                {

                    bool recordsExists = ValidateCostDetails();  // call validate to check and load window

                    if (recordsExists == false)

                        SaveUnitCostChangestoAudit();
                    //ValidateMaterialManagmentDetails();
                }

            }

            catch (Exception exception)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PopSavePre_InvokeBeforeOriginal Method: " + exception.Message.ToString());

                MessageBox.Show(exception.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }



        /// <summary>

        /// Initialize the controls before print pre events

        /// </summary>

        /// <param name="sender"></param>

        /// <param name="e"></param>

        void PopClosePre_InvokeBeforeOriginal(object sender, Dic1311.PopClosePreProcedure.InvokeEventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                if (!String.IsNullOrEmpty(popPOEntryForm.PopPoEntry.PoNumber.Value))
                {

                    bool recordsExists = ValidateCostDetails();  // call validate to check and load window

                    if (recordsExists == false)

                        SaveUnitCostChangestoAudit();

                }

            }

            catch (Exception exception)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PopClosePre_InvokeBeforeOriginal Method: " + exception.Message.ToString());

                MessageBox.Show(exception.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }



        /// <summary>

        /// Initialize the controls before navigation pre events

        /// </summary>

        /// <param name="sender"></param>

        /// <param name="e"></param>

        void PopNavigationPre_InvokeBeforeOriginal(object sender, Dic1311.PopNavigationPreProcedure.InvokeEventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                if (!String.IsNullOrEmpty(popPOEntryForm.PopPoEntry.PoNumber.Value))
                {

                    bool recordsExists = ValidateCostDetails();  // call validate to check and load window

                    if (recordsExists == false)

                        SaveUnitCostChangestoAudit();

                }

            }

            catch (Exception exception)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PopNavigationPre_InvokeBeforeOriginal Method: " + exception.Message.ToString());

                MessageBox.Show(exception.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }



        /// <summary>

        /// Initialize the controls before print pre events

        /// </summary>

        /// <param name="sender"></param>

        /// <param name="e"></param>

        void PopPrintPre_InvokeBeforeOriginal(object sender, Dic1311.PopPrintPreProcedure.InvokeEventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                if (!String.IsNullOrEmpty(popPOEntryForm.PopPoEntry.PoNumber.Value))
                {

                    bool recordsExists = ValidateCostDetails();  // call validate to check and load window

                    if (recordsExists == false)

                        SaveUnitCostChangestoAudit();

                }

            }

            catch (Exception exception)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PopPrintPre_InvokeBeforeOriginal Method: " + exception.Message.ToString());

                MessageBox.Show(exception.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }



        /// <summary>

        /// Initialize the controls before action pre events

        /// </summary>

        /// <param name="sender"></param>

        /// <param name="e"></param>

        void PopActionsPre_InvokeBeforeOriginal(object sender, Dic1311.PopActionsPreProcedure.InvokeEventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                if (!String.IsNullOrEmpty(popPOEntryForm.PopPoEntry.PoNumber.Value))
                {

                    string actionbuttonName = poCostManagementForm.Functions.PoCostManagementActionButtonIndex.Invoke(popPOEntryForm.PopPoEntry.LocalActionsButton.Value);

                    if (!String.IsNullOrEmpty(actionbuttonName))
                    {

                        if (actionbuttonName.ToString().Trim() == Resources.STR_ACTION_RCVPOITEMS.Trim() ||

                            actionbuttonName == Resources.STR_ACTION_RCVIVCPOITEMS.Trim() ||

                            actionbuttonName == Resources.STR_ACTION_IVCPOITEMS.Trim() ||

                            actionbuttonName == Resources.STR_ACTION_EDITPOSTATUS.Trim() ||

                            actionbuttonName == Resources.STR_ACTION_COPYTOCURRENT.Trim() ||

                            actionbuttonName == Resources.STR_ACTION_COPYTONEW.Trim())
                        {

                            bool recordsExists = ValidateCostDetails();  // call validate to check and load window

                            if (recordsExists == false)

                                SaveUnitCostChangestoAudit();

                        }

                    }

                }

            }

            catch (Exception exception)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PopPrintPre_InvokeBeforeOriginal Method: " + exception.Message.ToString());

                MessageBox.Show(exception.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }



        /// <summary>

        /// This triggers updates the pop item details entry details and get controls after save events

        /// </summary>

        /// <param name="sender"></param>

        /// <param name="e"></param>

        void PopPoItemDetailEntrySave_InvokeAfterOriginal(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                ResetCostDetailsByLine(popPoItemDetailEntryForm.Tables.PopPoLine.PoNumber.Value.ToString().Trim(),

                                       popPoItemDetailEntryForm.Tables.PopPoLine.Ord.Value,

                                       popPoItemDetailEntryForm.Tables.PopPoLine.UnitCost.Value,

                                       popPoItemDetailEntryForm.Tables.PopPoLine.QtyOrdered.Value,

                                       popPoItemDetailEntryForm.Tables.PopPoLine.QtyCanceled.Value,

                                       popPoItemDetailEntryForm.Tables.PopPoLine.UOfM.Value);



            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PopPoItemDetailEntrySave_InvokeAfterOriginal Method: " + ex.Message.ToString());

                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }



        /// <summary>

        /// Initialize the controls before cost book calculated by CpprPoItemDetailEntryCostBookProcedure (dex) pre events

        /// </summary>

        /// <param name="sender"></param>

        /// <param name="e"></param>

        void CpprPoItemDetailEntryCostBook_InvokeBeforeOriginal(object sender, Dic1311.CpprPoItemDetailEntryCostBookProcedure.InvokeEventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                bool isValid = false;



                if (!String.IsNullOrEmpty(popPoItemDetailEntryForm.Tables.PopPoLine.ItemNumber.Value))
                {

                    decimal currentValue = popPoItemDetailEntryForm.PopPoItemDetailEntry.QtyOrdered.Value;

                    decimal previousValue = popPoItemDetailEntryForm.PopPoItemDetailEntry.LocalQtyOrdered.Value;



                    if (currentValue != previousValue)
                    {

                        //reset IsUpdated value to 0 if user want to change value again

                        popPOEntryForm.Tables.PoCostManagementDetails.Close();

                        popPOEntryForm.Tables.PoCostManagementDetails.Release();



                        popPOEntryForm.Tables.PoCostManagementDetails.PoNumber.Value = popPoItemDetailEntryForm.Tables.PopPoLine.PoNumber.Value.ToString().Trim();

                        popPOEntryForm.Tables.PoCostManagementDetails.Ord.Value = popPoItemDetailEntryForm.Tables.PopPoLine.Ord.Value;

                        popPOEntryForm.Tables.PoCostManagementDetails.UserId.Value = Dynamics.Globals.UserId.Value.ToString().Trim();

                        popPOEntryForm.Tables.PoCostManagementDetails.Key = 1;



                        TableError tempError = popPOEntryForm.Tables.PoCostManagementDetails.Change();

                        if (tempError == TableError.NotFound && tempError != TableError.EndOfTable)
                        {

                            popPoItemDetailEntryForm.PopPoItemDetailEntry.LocalQtyOrdered.Value = currentValue;



                            if (previousValue == 0 && currentValue > 0)
                            {

                                isValid = true;

                            }

                        }

                        else
                        {

                            if (tempError == TableError.NoError && tempError != TableError.EndOfTable)
                            {

                                if (popPOEntryForm.Tables.PoCostManagementDetails.IsPoCostExist.Value == true)
                                {

                                    popPoItemDetailEntryForm.PopPoItemDetailEntry.LocalQtyOrdered.Value = currentValue;



                                    if (previousValue == 0 && currentValue > 0)
                                    {

                                        isValid = true;

                                    }

                                }

                            }

                        }



                        popPOEntryForm.Tables.PoCostManagementDetails.Close();

                        popPOEntryForm.Tables.PoCostManagementDetails.Release();

                    }

                }



                popPoItemDetailEntryForm.PopPoItemDetailEntry.IsPoCostExist.Value = isValid;

                popPoItemDetailEntryForm.PopPoItemDetailEntry.IsPoCostExist.RunValidate();



            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CpprPoItemDetailEntryCostBook_InvokeBeforeOriginal Method: " + ex.Message.ToString());

                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }



        /// <summary>

        /// Initialize events before opening the screen

        /// </summary>

        /// <param name="sender"></param>

        /// <param name="e"></param>

        void PoCostManagementFormPoCostManagement_OpenBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (registerPoCostManagement == false)
                {
                    poCostManagementForm.PoCostManagement.LocalNotesDdl.Change += new EventHandler(PoCostManagementFormLocalNotesDdl_Change);
                    poCostManagementForm.PoCostManagement.ApplyButton.ClickAfterOriginal += new EventHandler(PoCostManagementFormApplyButton_ClickAfterOriginal);
                    poCostManagementForm.PoCostManagement.LocalAcceptButton.ClickAfterOriginal += new EventHandler(PoCostManagementFormLocalAcceptButton_ClickAfterOriginal);
                    poCostManagementForm.PoCostManagement.LocalDeclineButton.ClickAfterOriginal += new EventHandler(PoCostManagementFormLocalDeclineButton_ClickAfterOriginal);
                    poCostManagementForm.PoCostManagement.LocalRemindMeLaterWarning.ClickAfterOriginal += new EventHandler(PoCostManagementFormLocalRemindLaterButton_ClickAfterOriginal);
                    poCostManagementForm.PoCostManagement.UpdateButton.ClickAfterOriginal += new EventHandler(PoCostManagementFormUpdateButton_ClickAfterOriginal);
                    poCostManagementForm.PoCostManagement.CancelButton.ClickAfterOriginal += new EventHandler(PoCostManagementFormCancelButton_ClickAfterOriginal);
                    //poCostManagementForm.PoCostManagement.NotePresentButtonWindowArea.ClickAfterOriginal += new EventHandler(PoCostManagementFormNotePresentButtonWindowArea_ClickAfterOriginal);
                    poCostManagementForm.PoCostManagement.PoCostManagementScroll.LineChangeAfterOriginal += new EventHandler(PoCostManagemenScroll_LineChangeAfterOriginal);
                    poCostManagementForm.PoCostManagement.PoCostManagementScroll.LineFillAfterOriginal += new EventHandler(PoCostManagemenScroll_LineFillAfterOriginal);
                    poCostManagementForm.PoCostManagement.CloseAfterOriginal += new EventHandler(PoCostManagementFormPoCostManagement_CloseAfterOriginal);
                    poCostManagementForm.PoCostManagement.LocalCostApplyRejectAll.Change += new EventHandler(PoCostManagementFormPoCostManagement_LocalCostApplyRejectAll);
                    registerPoCostManagement = true;
                }
                poCostManagementForm.PoCostManagement.LocalMassApplyNotes.Disable();  // changed property as per note drop down box
                poCostManagementForm.PoCostManagement.ScrollExpandSwitch.Value = 1;
                poCostManagementForm.PoCostManagement.LocalExpandButton.RunValidate();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoCostManagementFormPoCostManagement_OpenBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// When po cost management window is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        void PoCostManagementFormPoCostManagement_CloseAfterOriginal(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                poCostManagementForm.PoCostManagement.Close();

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoCostManagementFormPoCostManagement_CloseAfterOriginal Method: " + ex.Message.ToString());

                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }



        /// <summary>

        /// Clear all values from cost details temp table

        /// </summary>

        /// <param name="poNumber">Specify po number</param>

        private void ClearCostDetails(string poNumber)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                popPOEntryForm.Tables.PoCostManagementDetails.Close();

                popPOEntryForm.Tables.PoCostManagementDetails.Release();



                TableError tempError = popPOEntryForm.Tables.PoCostManagementDetails.ChangeFirst();

                while (tempError == TableError.NoError && tempError != TableError.EndOfTable)
                {

                    if ((poNumber == "" || poNumber != popPOEntryForm.Tables.PoCostManagementDetails.PoNumber.Value.ToString().Trim()) &&

                        Dynamics.Globals.UserId.Value.ToString().ToUpper().Trim() == popPOEntryForm.Tables.PoCostManagementDetails.UserId.Value.ToString().ToUpper().Trim())
                    {

                        popPOEntryForm.Tables.PoCostManagementDetails.Remove();

                    }



                    tempError = popPOEntryForm.Tables.PoCostManagementDetails.ChangeNext();

                }



                popPOEntryForm.Tables.PoCostManagementDetails.Close();

                popPOEntryForm.Tables.PoCostManagementDetails.Release();

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearCostDetails Method: " + ex.Message.ToString());

                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }



        /// <summary>

        /// Insert pop- entry value into cost details temp table

        /// </summary>

        private void InsertCostDetails()
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                if (String.IsNullOrEmpty(popPOEntryForm.PopPoEntry.PoNumber.Value))
                {

                    ClearCostDetails("");   // pass blank PO Number to clear all data

                }

                else
                {

                    ClearCostDetails(popPOEntryForm.PopPoEntry.PoNumber.Value.ToString().Trim());  // bypass PO Number on double insertion
                    // set range to current po number to get line details from poLine table
                    popPOEntryForm.Tables.PopPoLine.Close();
                    popPOEntryForm.Tables.PopPoLine.Release();
                    popPOEntryForm.Tables.PopPoLine.RangeClear();
                    popPOEntryForm.Tables.PopPoLine.Ord.Value = 1;
                    popPOEntryForm.Tables.PopPoLine.BreakField1.Value = 0;
                    popPOEntryForm.Tables.PopPoLine.PoNumber.Value = popPOEntryForm.PopPoEntry.PoNumber.Value;
                    popPOEntryForm.Tables.PopPoLine.RangeStart();
                    popPOEntryForm.Tables.PopPoLine.Key = 1;
                    popPOEntryForm.Tables.PopPoLine.PoNumber.Value = popPOEntryForm.PopPoEntry.PoNumber.Value;
                    popPOEntryForm.Tables.PopPoLine.Ord.Fill();
                    popPOEntryForm.Tables.PopPoLine.BreakField1.Value = 0;
                    popPOEntryForm.Tables.PopPoLine.RangeEnd();
                    popPOEntryForm.Tables.PopPoLine.Key = 1;
                    TableError error = popPOEntryForm.Tables.PopPoLine.GetFirst();
                    while (error != TableError.EndOfTable && error == TableError.NoError)
                    {
                        string hdrPO = popPOEntryForm.PopPoEntry.PoNumber.Value.ToString().Trim();
                        string linePO = popPOEntryForm.Tables.PopPoLine.PoNumber.Value.ToString().Trim();
                        // check current po number is equal to line po number
                        if (hdrPO == linePO)
                        {
                            popPOEntryForm.Tables.PoCostManagementDetails.Close();
                            popPOEntryForm.Tables.PoCostManagementDetails.Release();
                            popPOEntryForm.Tables.PoCostManagementDetails.PoNumber.Value = popPOEntryForm.Tables.PopPoLine.PoNumber.Value;
                            popPOEntryForm.Tables.PoCostManagementDetails.Ord.Value = popPOEntryForm.Tables.PopPoLine.Ord.Value;
                            popPOEntryForm.Tables.PoCostManagementDetails.UserId.Value = Dynamics.Globals.UserId.Value.ToString();
                            popPOEntryForm.Tables.PoCostManagementDetails.Key = 1;
                            TableError tempError = popPOEntryForm.Tables.PoCostManagementDetails.Change();
                            if (tempError != TableError.NoError && tempError == TableError.NotFound)
                            {
                                popPOEntryForm.Tables.PoCostManagementDetails.PoNumber.Value = popPOEntryForm.Tables.PopPoLine.PoNumber.Value;
                                popPOEntryForm.Tables.PoCostManagementDetails.VendorId.Value = popPOEntryForm.Tables.PopPoLine.VendorId.Value;
                                popPOEntryForm.Tables.PoCostManagementDetails.Ord.Value = popPOEntryForm.Tables.PopPoLine.Ord.Value;
                                popPOEntryForm.Tables.PoCostManagementDetails.LineCount.Value = popPOEntryForm.Tables.PopPoLine.LineNumber.Value;
                                popPOEntryForm.Tables.PoCostManagementDetails.ItemNumber.Value = popPOEntryForm.Tables.PopPoLine.ItemNumber.Value;
                                popPOEntryForm.Tables.PoCostManagementDetails.UOfM.Value = popPOEntryForm.Tables.PopPoLine.UOfM.Value;
                                popPOEntryForm.Tables.PoCostManagementDetails.UnitCost.Value = popPOEntryForm.Tables.PopPoLine.UnitCost.Value;                                
                                popPOEntryForm.Tables.PoCostManagementDetails.QtyCanceled.Value = popPOEntryForm.Tables.PopPoLine.QtyCanceled.Value;
                                popPOEntryForm.Tables.PoCostManagementDetails.QtyOrdered.Value = popPOEntryForm.Tables.PopPoLine.QtyOrdered.Value;
                                popPOEntryForm.Tables.PoCostManagementDetails.NoteIndex.Value = popPOEntryForm.Tables.PopPoLine.PoLineNoteIdArray.Value[4];
                                popPOEntryForm.Tables.PoCostManagementDetails.CostStatus.Value = -1;
                                popPOEntryForm.Tables.PoCostManagementDetails.IsUpdated.Value = 1;
                                popPOEntryForm.Tables.PoCostManagementDetails.IsPoCostExist.Value = false;
                                popPOEntryForm.Tables.PoCostManagementDetails.UserId.Value = Dynamics.Globals.UserId.Value.ToString();                               
                                popPOEntryForm.Tables.PoCostManagementDetails.Save();
                            }

                            popPOEntryForm.Tables.PoCostManagementDetails.Close();
                            popPOEntryForm.Tables.PoCostManagementDetails.Release();
                        }
                        error = popPOEntryForm.Tables.PopPoLine.GetNext();
                    }
                    popPOEntryForm.Tables.PopPoLine.Close();
                    popPOEntryForm.Tables.PopPoLine.Release();
                }
            }
            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertCostDetails Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }



        /// <summary>

        /// Insert pop-entry value into cost details temp table- Newly added line

        /// </summary>

        private void InsertNewLineCostDetails()
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (String.IsNullOrEmpty(popPOEntryForm.PopPoEntry.PoNumber.Value))
                {
                    ClearCostDetails("");   // pass blank PO Number to clear all data
                }
                else
                {
                    // set range to current po number to get line details from poLine table
                    popPOEntryForm.Tables.PopPoLine.Close();
                    popPOEntryForm.Tables.PopPoLine.Release();
                    popPOEntryForm.Tables.PopPoLine.RangeClear();
                    popPOEntryForm.Tables.PopPoLine.Ord.Value = 1;
                    popPOEntryForm.Tables.PopPoLine.BreakField1.Value = 0;
                    popPOEntryForm.Tables.PopPoLine.PoNumber.Value = popPOEntryForm.PopPoEntry.PoNumber.Value;
                    popPOEntryForm.Tables.PopPoLine.RangeStart();
                    popPOEntryForm.Tables.PopPoLine.Key = 1;
                    popPOEntryForm.Tables.PopPoLine.PoNumber.Value = popPOEntryForm.PopPoEntry.PoNumber.Value;
                    popPOEntryForm.Tables.PopPoLine.Ord.Fill();
                    popPOEntryForm.Tables.PopPoLine.BreakField1.Value = 0;
                    popPOEntryForm.Tables.PopPoLine.RangeEnd();
                    popPOEntryForm.Tables.PopPoLine.Key = 1;
                    TableError error = popPOEntryForm.Tables.PopPoLine.GetFirst();
                    while (error != TableError.EndOfTable && error == TableError.NoError)
                    {
                        string hdrPO = popPOEntryForm.PopPoEntry.PoNumber.Value.ToString().Trim();
                        string linePO = popPOEntryForm.Tables.PopPoLine.PoNumber.Value.ToString().Trim();
                        // check current po number is equal to line po number
                        if (hdrPO == linePO)
                        {
                            popPOEntryForm.Tables.PoCostManagementDetails.Close();
                            popPOEntryForm.Tables.PoCostManagementDetails.Release();
                            popPOEntryForm.Tables.PoCostManagementDetails.PoNumber.Value = popPOEntryForm.Tables.PopPoLine.PoNumber.Value;
                            popPOEntryForm.Tables.PoCostManagementDetails.Ord.Value = popPOEntryForm.Tables.PopPoLine.Ord.Value;
                            popPOEntryForm.Tables.PoCostManagementDetails.UserId.Value = Dynamics.Globals.UserId.Value.ToString();
                            popPOEntryForm.Tables.PoCostManagementDetails.Key = 1;
                            TableError tempError = popPOEntryForm.Tables.PoCostManagementDetails.Change();
                            if (tempError != TableError.NoError && tempError == TableError.NotFound)
                            {
                                popPOEntryForm.Tables.PoCostManagementDetails.PoNumber.Value = popPOEntryForm.Tables.PopPoLine.PoNumber.Value;
                                popPOEntryForm.Tables.PoCostManagementDetails.VendorId.Value = popPOEntryForm.Tables.PopPoLine.VendorId.Value;
                                popPOEntryForm.Tables.PoCostManagementDetails.Ord.Value = popPOEntryForm.Tables.PopPoLine.Ord.Value;
                                popPOEntryForm.Tables.PoCostManagementDetails.LineCount.Value = popPOEntryForm.Tables.PopPoLine.LineNumber.Value;
                                popPOEntryForm.Tables.PoCostManagementDetails.ItemNumber.Value = popPOEntryForm.Tables.PopPoLine.ItemNumber.Value;
                                popPOEntryForm.Tables.PoCostManagementDetails.UOfM.Value = popPOEntryForm.Tables.PopPoLine.UOfM.Value;
                                popPOEntryForm.Tables.PoCostManagementDetails.UnitCost.Value = popPOEntryForm.Tables.PopPoLine.UnitCost.Value;
                                popPOEntryForm.Tables.PoCostManagementDetails.QtyCanceled.Value = popPOEntryForm.Tables.PopPoLine.QtyCanceled.Value;
                                popPOEntryForm.Tables.PoCostManagementDetails.QtyOrdered.Value = popPOEntryForm.Tables.PopPoLine.QtyOrdered.Value;
                                popPOEntryForm.Tables.PoCostManagementDetails.NoteIndex.Value = popPOEntryForm.Tables.PopPoLine.PoLineNoteIdArray.Value[4];
                                popPOEntryForm.Tables.PoCostManagementDetails.CostStatus.Value = -1;
                                popPOEntryForm.Tables.PoCostManagementDetails.IsUpdated.Value = 1;
                                popPOEntryForm.Tables.PoCostManagementDetails.IsPoCostExist.Value = true;
                                popPOEntryForm.Tables.PoCostManagementDetails.UserId.Value = Dynamics.Globals.UserId.Value.ToString();
                                popPOEntryForm.Tables.PoCostManagementDetails.IsCostUpdated.Value = 1;
                                popPOEntryForm.Tables.PoCostManagementDetails.Save();

                            }
                            popPOEntryForm.Tables.PoCostManagementDetails.Close();
                            popPOEntryForm.Tables.PoCostManagementDetails.Release();
                        }
                        error = popPOEntryForm.Tables.PopPoLine.GetNext();
                    }

                    popPOEntryForm.Tables.PopPoLine.Close();
                    popPOEntryForm.Tables.PopPoLine.Release();
                }
            }
            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertNewLineCostDetails Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }



        /// <summary>

        /// Reset all values to cost details temp table once cost window closed

        /// </summary>

        private void ResetCostDetails()
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                //reset IsUpdated value to 1 once cost window closed
                popPOEntryForm.Tables.PoCostManagementDetails.Close();
                popPOEntryForm.Tables.PoCostManagementDetails.Release();
                TableError tempError = popPOEntryForm.Tables.PoCostManagementDetails.ChangeFirst();
                while (tempError == TableError.NoError && tempError != TableError.EndOfTable)
                {
                    if (Dynamics.Globals.UserId.Value.ToString().ToUpper().Trim() == popPOEntryForm.Tables.PoCostManagementDetails.UserId.Value.ToString().ToUpper().Trim())
                    {
                        popPOEntryForm.Tables.PoCostManagementDetails.IsUpdated.Value = 0;
                        popPOEntryForm.Tables.PoCostManagementDetails.Save();
                    }
                    tempError = popPOEntryForm.Tables.PoCostManagementDetails.ChangeNext();
                }
                popPOEntryForm.Tables.PoCostManagementDetails.Close();

            }

            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ResetCostDetails Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }

            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }



        /// <summary>

        /// Reset values to cost details temp table once unit cost changed,  qty ordered, qty cancelled or uofm chnages

        /// </summary>

        /// <param name="poNumber">Po Number</param>

        /// <param name="lineordNumber">Ord Number</param>

        /// <param name="unitCost">Current Unit Cost</param>

        /// <param name="qtyOrdered">Ordered Qty Number</param>

        /// <param name="qtyCanceled">Cancelled Qty</param>

        /// <param name="uOfM">UofM</param>

        private void ResetCostDetailsByLine(string poNumber, int ord, decimal unitCost, decimal qtyOrdered, decimal qtyCanceled, string uOfM)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                //reset IsUpdated value to 0 if user want to change value again

                popPOEntryForm.Tables.PoCostManagementDetails.Close();

                popPOEntryForm.Tables.PoCostManagementDetails.Release();



                popPOEntryForm.Tables.PoCostManagementDetails.PoNumber.Value = poNumber;

                popPOEntryForm.Tables.PoCostManagementDetails.Ord.Value = ord;

                popPOEntryForm.Tables.PoCostManagementDetails.UserId.Value = Dynamics.Globals.UserId.Value.ToString().Trim();

                popPOEntryForm.Tables.PoCostManagementDetails.Key = 1;



                TableError tempError = popPOEntryForm.Tables.PoCostManagementDetails.Change();

                if (tempError == TableError.NoError && tempError != TableError.EndOfTable)
                {

                    // check and delete content if line is deleted from pop-entry screen

                    ChempointCustomizations.Tables.PopPoLine.Close();

                    ChempointCustomizations.Tables.PopPoLine.Release();

                    ChempointCustomizations.Tables.PopPoLine.PoNumber.Value = poNumber;

                    ChempointCustomizations.Tables.PopPoLine.Ord.Value = ord;

                    ChempointCustomizations.Tables.PopPoLine.BreakField1.Value = 0;

                    ChempointCustomizations.Tables.PopPoLine.Key = 1;



                    TableError error = ChempointCustomizations.Tables.PopPoLine.Change();

                    if (error == TableError.NotFound && error != TableError.EndOfTable)
                    {

                        popPOEntryForm.Tables.PoCostManagementDetails.Remove();

                    }

                    else
                    {

                        if (popPOEntryForm.Tables.PoCostManagementDetails.UnitCost.Value != unitCost ||

                            popPOEntryForm.Tables.PoCostManagementDetails.QtyOrdered.Value != qtyOrdered ||

                            popPOEntryForm.Tables.PoCostManagementDetails.QtyCanceled.Value != qtyCanceled ||

                            popPOEntryForm.Tables.PoCostManagementDetails.UOfM.Value.ToString().ToLower().Trim() != uOfM.ToString().ToLower().Trim())
                        {

                            popPOEntryForm.Tables.PoCostManagementDetails.IsUpdated.Value = 1;   // update if change found

                            popPOEntryForm.Tables.PoCostManagementDetails.Save();

                        }

                        else
                        {

                            popPOEntryForm.Tables.PoCostManagementDetails.IsUpdated.Value = 0;   // update if no chnage found

                            popPOEntryForm.Tables.PoCostManagementDetails.Save();

                        }

                    }



                    ChempointCustomizations.Tables.PopPoLine.Release();

                    ChempointCustomizations.Tables.PopPoLine.Close();

                }

                popPOEntryForm.Tables.PoCostManagementDetails.Release();

                popPOEntryForm.Tables.PoCostManagementDetails.Close();

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ResetCostDetailsByLine Method: " + ex.Message.ToString());

                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }



        /// <summary>
        /// Validate changes for cost variance
        /// </summary>

        private bool ValidateCostDetails()
        {
            PurchaseOrderRequest purchaseRequest = null;
            bool recordsExists = false;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                InsertNewLineCostDetails();  // insert missing line or new line into cost details table

                purchaseRequest = new PurchaseOrderRequest();
                PurchaseOrderEntity purchaseEntity = new PurchaseOrderEntity();
                purchaseRequest.PurchaseCostMgt = new List<PurchaseCostManagement>();

                popPOEntryForm.Tables.PoCostManagementDetails.Close();
                popPOEntryForm.Tables.PoCostManagementDetails.Release();

                TableError tableError = popPOEntryForm.Tables.PoCostManagementDetails.GetFirst();
                while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    if (popPOEntryForm.Tables.PoCostManagementDetails.PoNumber.Value.ToString().Trim() == popPOEntryForm.PopPoEntry.PoNumber.Value.ToString().Trim())
                    {
                        PurchaseCostManagement PurchaseCostMgtInformation = new PurchaseCostManagement();
                        PurchaseCostMgtInformation.PoNumber = popPOEntryForm.Tables.PoCostManagementDetails.PoNumber.Value;
                       // PurchaseCostMgtInformation.VendorId = popPOEntryForm.Tables.PoCostManagementDetails.VendorId.Value;
                        PurchaseCostMgtInformation.Ord = popPOEntryForm.Tables.PoCostManagementDetails.Ord.Value;
                        //PurchaseCostMgtInformation.LineNumber = popPOEntryForm.Tables.PoCostManagementDetails.LineCount.Value;
                        //PurchaseCostMgtInformation.ItemNumber = popPOEntryForm.Tables.PoCostManagementDetails.ItemNumber.Value;
                        //PurchaseCostMgtInformation.UOfM = popPOEntryForm.Tables.PoCostManagementDetails.UOfM.Value;
                        //PurchaseCostMgtInformation.UnitCost = popPOEntryForm.Tables.PoCostManagementDetails.UnitCost.Value;
                        //PurchaseCostMgtInformation.QtyCancel = popPOEntryForm.Tables.PoCostManagementDetails.QtyCanceled.Value;
                        //PurchaseCostMgtInformation.QtyOrder = popPOEntryForm.Tables.PoCostManagementDetails.QtyOrdered.Value;
                        //PurchaseCostMgtInformation.NoteIndex = popPOEntryForm.Tables.PoCostManagementDetails.NoteIndex.Value;
                        //PurchaseCostMgtInformation.CostStatus = popPOEntryForm.Tables.PoCostManagementDetails.CostStatus.Value;
                        PurchaseCostMgtInformation.UserId = Dynamics.Globals.UserId.Value.ToString();
                        purchaseRequest.PurchaseCostMgt.Add(PurchaseCostMgtInformation);
                    }
                    tableError = popPOEntryForm.Tables.PoCostManagementDetails.GetNext();
                }

                popPOEntryForm.Tables.PoCostManagementDetails.Close();
                popPOEntryForm.Tables.PoCostManagementDetails.Release();

                if (purchaseRequest.PurchaseCostMgt != null && purchaseRequest.PurchaseCostMgt.Count > 0)
                {
                    purchaseRequest.CurrencyView = popPOEntryForm.PopPoEntry.CurrencyViewButton.Value;
                    purchaseRequest.CompanyID = Dynamics.Globals.CompanyId.Value;
                    if (purchaseRequest != null)
                    {
                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            var response = client.PostAsJsonAsync("api/PurchaseOrder/ValidatePoCostChanges", purchaseRequest); // we need to refer the web.api service url here.

                            if (response.Result.IsSuccessStatusCode)
                            {
                                PurchaseOrderResponse purchaseOrderResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                                if (purchaseOrderResponse.Status == ResponseStatus.Error)
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ValidateCostDetails Method (ValidatePoCostChanges): " + purchaseOrderResponse.ErrorMessage.ToString());
                                    MessageBox.Show(purchaseOrderResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    if (purchaseOrderResponse.PurchaseCostMgt != null && purchaseOrderResponse.PurchaseCostMgt.Count > 0)
                                    {
                                        bool isUpdated = getIsUpdatedValue();
                                        if (isUpdated == true)
                                        {
                                            recordsExists = true;
                                            poCostManagementForm.PoCostManagement.Open();  // open window if data found

                                            FillScrollForPOCost(purchaseOrderResponse, purchaseRequest.CurrencyView);      // fill scrolling window
                                            poCostManagementForm.Procedures.PoCostManagementFormScrollFill.Invoke();
                                            poCostManagementForm.Procedures.PoCostManagementFormNotesFill.Invoke();   // fill note list
                                            poCostManagementForm.PoCostManagement.LocalNotesDdl.Value = 1;   // set default values to note list
                                        }
                                    }
                                    else
                                    {
                                        recordsExists = false;
                                    }
                                }
                            }
                            else
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not fetch from po cost mgt details Table");
                                MessageBox.Show("Error: Data does not fetch from po cost mgt details Table", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ValidateCostDetails Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
            return recordsExists;
        }



        private bool getIsUpdatedValue()
        {
            StringBuilder logMessage = new StringBuilder();
            bool result = false;
            List<int> isUpdatedValueList = new List<int>();
            try
            {
                //Checking IsUpdated value to either open cost window or not
                popPOEntryForm.Tables.PoCostManagementDetails.Close();
                popPOEntryForm.Tables.PoCostManagementDetails.Release();
                TableError tempError = popPOEntryForm.Tables.PoCostManagementDetails.Change();
                while (tempError == TableError.NoError && tempError != TableError.EndOfTable)
                {
                    if (Dynamics.Globals.UserId.Value.ToString().ToUpper().Trim() == popPOEntryForm.Tables.PoCostManagementDetails.UserId.Value.ToString().ToUpper().Trim())
                    {
                        isUpdatedValueList.Add(popPOEntryForm.Tables.PoCostManagementDetails.IsUpdated.Value);
                    }
                    tempError = popPOEntryForm.Tables.PoCostManagementDetails.ChangeNext();
                }
                popPOEntryForm.Tables.PoCostManagementDetails.Close();
                popPOEntryForm.Tables.PoCostManagementDetails.Release();

                if (isUpdatedValueList.Contains(1))
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In getIsUpdatedValue Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
            return result;
        }



        /// <summary>
        /// Fill PO cost management window with response
        /// </summary>
        /// <param name="responseObj">Data</param>
        /// <param name="curcnyView">cuurency view a. functional b. originating </param>
        private void FillScrollForPOCost(PurchaseOrderResponse responseObj, int curcnyView)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                var poCostMgtList = from d in responseObj.PurchaseCostMgt
                                    select d;

                if (responseObj.PurchaseCostMgt != null && responseObj.PurchaseCostMgt.Count > 0)
                {
                    foreach (var poCostMgt in poCostMgtList)
                    {
                        for (int costCnt = 0; costCnt < responseObj.PurchaseCostMgt.Count; costCnt++)
                        {
                            poCostManagementForm.Tables.PoLineCostDetails.Close();
                            poCostManagementForm.Tables.PoLineCostDetails.Release();
                            poCostManagementForm.Tables.PoLineCostDetails.PoNumber.Value = poCostMgt.PoNumber;
                            poCostManagementForm.Tables.PoLineCostDetails.VendorId.Value = poCostMgt.VendorId;
                            poCostManagementForm.Tables.PoLineCostDetails.Ord.Value = poCostMgt.Ord;
                            poCostManagementForm.Tables.PoLineCostDetails.LineCount.Value = Convert.ToInt16(poCostMgt.LineNumber);
                            poCostManagementForm.Tables.PoLineCostDetails.ItemNumber.Value = poCostMgt.ItemNumber;
                            poCostManagementForm.Tables.PoLineCostDetails.UOfM.Value = poCostMgt.UOfM;
                            poCostManagementForm.Tables.PoLineCostDetails.UnitCost.Value = Convert.ToDecimal(poCostMgt.UnitCost);
                            poCostManagementForm.Tables.PoLineCostDetails.ProposedUnitPrice.Value = Convert.ToDecimal(poCostMgt.ProposedUnitCost);
                            poCostManagementForm.Tables.PoLineCostDetails.CostStatus.Value = Convert.ToInt16(poCostMgt.CostStatus);
                            poCostManagementForm.Tables.PoLineCostDetails.CpPoCostSupportId.Value = poCostMgt.CostSupportId;
                            poCostManagementForm.Tables.PoLineCostDetails.CostBookCost.Value = Convert.ToDecimal(poCostMgt.CostBookCost);
                            // poCostManagementForm.Tables.PoLineCostDetails.QtyCanceled.Value = Convert.ToDecimal(poCostMgt.QtyCancel);
                            // poCostManagementForm.Tables.PoLineCostDetails.QtyOrdered.Value = Convert.ToDecimal(poCostMgt.QtyOrder);
                            poCostManagementForm.Tables.PoLineCostDetails.CostNotes.Value = poCostMgt.CostNotes;
                            poCostManagementForm.Tables.PoLineCostDetails.LineCostNotes.Value = poCostMgt.LineCostNotes;
                            poCostManagementForm.Tables.PoLineCostDetails.NoteIndex.Value = poCostMgt.NoteIndex;
                            poCostManagementForm.Tables.PoLineCostDetails.UserId.Value = poCostMgt.UserId;
                            poCostManagementForm.Tables.PoLineCostDetails.ItemDescription.Value = poCostMgt.ItemDescription;
                            if (poCostMgt.CostSupportCost != 0)
                                poCostManagementForm.Tables.PoLineCostDetails.CostSupportCostAmount.Value = poCostMgt.CostSupportCost;
                            else
                                poCostManagementForm.Tables.PoLineCostDetails.CostSupportCostAmount.Value = 0;
                            poCostManagementForm.Tables.PoLineCostDetails.PoCostVariance.Value = poCostMgt.PoCostVariance;
                            poCostManagementForm.Tables.PoLineCostDetails.PoReasonCode.Value = poCostMgt.Reason;
                            if (string.IsNullOrEmpty(poCostMgt.CostSupportId.Trim()))
                                poCostManagementForm.Tables.PoLineCostDetails.CpPoCostSupportId.Value = "-";
                            else
                                poCostManagementForm.Tables.PoLineCostDetails.CpPoCostSupportId.Value = poCostMgt.CostSupportId;
                            if (!string.IsNullOrEmpty(poCostMgt.PoLineCostSource))
                                poCostManagementForm.Tables.PoLineCostDetails.PoLineCostSource.Value = poCostMgt.PoLineCostSource;
                            else
                                poCostManagementForm.Tables.PoLineCostDetails.PoLineCostSource.Value = Dynamics.Globals.UserId.Value;

                            // calculate cuurency index- used to set currency symbol as per functional or originating view
                            poCostManagementForm.Tables.PoLineCostDetails.CurrencyIndex.Value =
                                                        poCostManagementForm.Functions.PoCostManagementCurrencyIndex.Invoke(
                                                        Convert.ToInt16(poCostMgt.CurrencyIndex), Convert.ToInt16(curcnyView));
                            poCostManagementForm.Tables.PoLineCostDetails.Save();
                        }
                        poCostManagementForm.Tables.PoLineCostDetails.Release();
                        poCostManagementForm.Tables.PoLineCostDetails.Close();
                        ChangeApplyToAllLinesRadioButton(poCostMgt.CostStatus);
                    }

                }

                //if (responseObj.PurchaseCostMgt != null && responseObj.PurchaseCostMgt.Count > 0)
                //{
                //    foreach (var poCostMgt in poCostMgtList)
                //    {
                //        for (int costCnt = 0; costCnt < responseObj.PurchaseCostMgt.Count; costCnt++)
                //        {
                //            poCostManagementForm.Tables.PoLineCostDetails.Close();
                //            poCostManagementForm.Tables.PoLineCostDetails.Release();
                //            TableError tempError = poCostManagementForm.Tables.PoLineCostDetails.GetFirst();
                //            while (tempError == TableError.NoError && tempError != TableError.EndOfTable)
                //            {
                //                if (popPOEntryForm.Tables.PoCostManagementDetails.IsUpdated.Value == 1)
                //                {
                //                    popPOEntryForm.Tables.PoCostManagementDetails.IsCostUpdated.Value = 1;
                //                    popPOEntryForm.Tables.PoCostManagementDetails.ProposedUnitPrice.Value = poCostMgt.ProposedUnitCost;
                //                    popPOEntryForm.Tables.PoCostManagementDetails.PoCostVariance.Value = poCostMgt.PoCostVariance;
                //                    popPOEntryForm.Tables.PoCostManagementDetails.Save();
                //                }
                //                tempError = poCostManagementForm.Tables.PoLineCostDetails.GetNext();
                //            }
                //        }
                //        popPOEntryForm.Tables.PoCostManagementDetails.Close();
                //        popPOEntryForm.Tables.PoCostManagementDetails.Release();
                //    }



                //    POCostManagementLoadDatatable();
                //}
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In FillScrollForPOCost Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }

        private void POCostManagementLoadDatatable(List<PurchaseCostManagement> poCostDetails)
        {
            if (poCostDetails != null && poCostDetails.Count > 0)
            {
                foreach (var poCostMgt in poCostDetails)
                {
                    //for (int costCnt = 0; costCnt < poCostDetails.Count; costCnt++)
                    //{
                    popPOEntryForm.Tables.PoCostManagementDetails.Close();
                    popPOEntryForm.Tables.PoCostManagementDetails.Release();
                    popPOEntryForm.Tables.PoCostManagementDetails.PoNumber.Value = poCostMgt.PoNumber;
                    popPOEntryForm.Tables.PoCostManagementDetails.Ord.Value = poCostMgt.Ord;
                    popPOEntryForm.Tables.PoCostManagementDetails.UserId.Value = Dynamics.Globals.UserId.Value.ToString();
                    popPOEntryForm.Tables.PoCostManagementDetails.Key = 1;
                    TableError tempError = popPOEntryForm.Tables.PoCostManagementDetails.Change();
                    if (tempError == TableError.NoError)
                    {
                        if (poCostMgt.CostStatus == 0)
                        {
                            popPOEntryForm.Tables.PoCostManagementDetails.UnitCost.Value = poCostMgt.ProposedUnitCost;
                            popPOEntryForm.Tables.PoCostManagementDetails.IsCostUpdated.Value = 0;
                        }
                        else
                        {
                            popPOEntryForm.Tables.PoCostManagementDetails.UnitCost.Value = poCostMgt.UnitCost;
                            popPOEntryForm.Tables.PoCostManagementDetails.IsCostUpdated.Value = 1;
                        }
                        popPOEntryForm.Tables.PoCostManagementDetails.ProposedUnitPrice.Value = poCostMgt.ProposedUnitCost;
                        popPOEntryForm.Tables.PoCostManagementDetails.PoCostVariance.Value = poCostMgt.PoCostVariance;
                        popPOEntryForm.Tables.PoCostManagementDetails.Save();
                    }
                    //}
                }
                popPOEntryForm.Tables.PoCostManagementDetails.Release();
                popPOEntryForm.Tables.PoCostManagementDetails.Close();
            }




            costVarianceDT = new DataTable();
            costVarianceDT.Columns.Add("PONumber", typeof(string));
            costVarianceDT.Columns.Add("POLineItemNumber", typeof(int));
            costVarianceDT.Columns.Add("ItemNumber", typeof(string));
            costVarianceDT.Columns.Add("CurrentCost", typeof(decimal));
            costVarianceDT.Columns.Add("CostEntered", typeof(decimal));
            costVarianceDT.Columns.Add("CostVariance", typeof(decimal));
            costVarianceDT.Columns.Add("ApprovalRequired", typeof(string));

            popPOEntryForm.Tables.PoCostManagementDetails.Close();
            popPOEntryForm.Tables.PoCostManagementDetails.Release();
            TableError tempCostVariance = popPOEntryForm.Tables.PoCostManagementDetails.GetFirst();
            while (tempCostVariance == TableError.NoError && tempCostVariance != TableError.EndOfTable)
            {
                if (popPOEntryForm.Tables.PoCostManagementDetails.IsCostUpdated.Value == 1)
                {
                    DataRow newRow = costVarianceDT.NewRow();
                    newRow["PONumber"] = popPOEntryForm.Tables.PoCostManagementDetails.PoNumber.Value.Trim();
                    newRow["POLineItemNumber"] = popPOEntryForm.Tables.PoCostManagementDetails.LineCount.Value;
                    newRow["ItemNumber"] = popPOEntryForm.Tables.PoCostManagementDetails.ItemNumber.Value.Trim();
                    newRow["CurrentCost"] = popPOEntryForm.Tables.PoCostManagementDetails.ProposedUnitPrice.Value;
                    newRow["CostEntered"] = popPOEntryForm.Tables.PoCostManagementDetails.UnitCost.Value;
                    newRow["CostVariance"] = decimal.Round(popPOEntryForm.Tables.PoCostManagementDetails.PoCostVariance.Value, 2, MidpointRounding.AwayFromZero);
                    newRow["ApprovalRequired"] = popPOEntryForm.Tables.PoCostManagementDetails.UnitCost.Value + " against the current line cost " +
                         popPOEntryForm.Tables.PoCostManagementDetails.ProposedUnitPrice.Value;
                    costVarianceDT.Rows.Add(newRow);
                }

                tempCostVariance = popPOEntryForm.Tables.PoCostManagementDetails.GetNext();
            }
            popPOEntryForm.Tables.PoCostManagementDetails.Release();
            popPOEntryForm.Tables.PoCostManagementDetails.Close();
        }



        /// <summary>
        /// When the po cost management window line changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        void PoCostManagemenScroll_LineChangeAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                // Store value in temp table
                poCostManagementForm.Tables.PoLineCostDetails.Close();
                poCostManagementForm.Tables.PoLineCostDetails.Release();

                poCostManagementForm.Tables.PoLineCostDetails.PoNumber.Value= poCostManagementForm.PoCostManagement.PoCostManagementScroll.PoNumber.Value.ToString().Trim();
                poCostManagementForm.Tables.PoLineCostDetails.Ord.Value = poCostManagementForm.PoCostManagement.PoCostManagementScroll.Ord.Value;
                poCostManagementForm.Tables.PoLineCostDetails.UserId.Value = Dynamics.Globals.UserId.Value.ToString().Trim();
                poCostManagementForm.Tables.PoLineCostDetails.Key = 1;
                TableError tableError = poCostManagementForm.Tables.PoLineCostDetails.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    // assign updated values to table if user changed note or status
                    poCostManagementForm.Tables.PoLineCostDetails.CostNotes.Value = poCostManagementForm.PoCostManagement.PoCostManagementScroll.CostNotes.Value;
                    poCostManagementForm.Tables.PoLineCostDetails.CostStatus.Value = poCostManagementForm.PoCostManagement.PoCostManagementScroll.LocalCostStatus.Value;
                    poCostManagementForm.Tables.PoLineCostDetails.Save();
                }
                poCostManagementForm.Tables.PoLineCostDetails.Close();
                poCostManagementForm.Tables.PoLineCostDetails.Release();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoCostManagemenScroll_LineChangeAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }



        /// <summary>
        /// When the po cost management window line fill
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        void PoCostManagemenScroll_LineFillAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                // Set currency symbol by resetting currency index
                poCostManagementForm.PoCostManagement.PoCostManagementScroll.CurrencyIndex.Value = poCostManagementForm.Tables.PoLineCostDetails.CurrencyIndex.Value;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoCostManagemenScroll_LineFillAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }



        /// <summary>
        /// When accept button of po cost management window is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PoCostManagementFormLocalAcceptButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                // set status as accepted for all lines
                poCostManagementForm.Tables.PoLineCostDetails.Close();
                poCostManagementForm.Tables.PoLineCostDetails.Release();

                TableError tableError = poCostManagementForm.Tables.PoLineCostDetails.ChangeFirst();
                while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    poCostManagementForm.Tables.PoLineCostDetails.CostStatus.Value = 0;
                    poCostManagementForm.Tables.PoLineCostDetails.Save();
                    tableError = poCostManagementForm.Tables.PoLineCostDetails.ChangeNext();
                }
                poCostManagementForm.Tables.PoLineCostDetails.Release();
                poCostManagementForm.Tables.PoLineCostDetails.Close();
                poCostManagementForm.Procedures.PoCostManagementFormScrollFill.Invoke();
                ChangeApplyToAllLinesRadioButton(0);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoCostManagementFormLocalAcceptButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }



        /// <summary>
        /// When declined button of po cost management window is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PoCostManagementFormLocalDeclineButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                // set status as declined for all lines
                poCostManagementForm.Tables.PoLineCostDetails.Close();
                poCostManagementForm.Tables.PoLineCostDetails.Release();

                TableError tableError = poCostManagementForm.Tables.PoLineCostDetails.ChangeFirst();
                while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    poCostManagementForm.Tables.PoLineCostDetails.CostStatus.Value = 1;
                    poCostManagementForm.Tables.PoLineCostDetails.Save();
                    tableError = poCostManagementForm.Tables.PoLineCostDetails.ChangeNext();
                }
                poCostManagementForm.Tables.PoLineCostDetails.Close();
                poCostManagementForm.Tables.PoLineCostDetails.Release();
                poCostManagementForm.Procedures.PoCostManagementFormScrollFill.Invoke();
                ChangeApplyToAllLinesRadioButton(1);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoCostManagementFormLocalDeclineButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }



        /// <summary>
        /// When RemindMeLater button of po cost management window is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PoCostManagementFormLocalRemindLaterButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                // set status as declined for all lines
                poCostManagementForm.Tables.PoLineCostDetails.Close();
                poCostManagementForm.Tables.PoLineCostDetails.Release();

                TableError tableError = poCostManagementForm.Tables.PoLineCostDetails.ChangeFirst();
                while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    poCostManagementForm.Tables.PoLineCostDetails.CostStatus.Value = 2;
                    poCostManagementForm.Tables.PoLineCostDetails.Save();
                    tableError = poCostManagementForm.Tables.PoLineCostDetails.ChangeNext();
                }
                poCostManagementForm.Tables.PoLineCostDetails.Close();
                poCostManagementForm.Tables.PoLineCostDetails.Release();

                poCostManagementForm.Procedures.PoCostManagementFormScrollFill.Invoke();
                ChangeApplyToAllLinesRadioButton(2);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoCostManagementFormLocalRemindLaterButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }



        /// <summary>
        /// When user changed note drop down value of po cost management window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>       

        void PoCostManagementFormLocalNotesDdl_Change(object sender, EventArgs e)
        {

            StringBuilder logMessage = new StringBuilder();

            try
            {

                if (!poCostManagementForm.PoCostManagement.LocalNotesDdl.IsEmpty)
                {

                    string actionbuttonName = poCostManagementForm.Functions.PoCostManagementNotesIndex.Invoke(poCostManagementForm.PoCostManagement.LocalNotesDdl.Value);

                    if (!String.IsNullOrEmpty(actionbuttonName))
                    {

                        if (actionbuttonName.ToString().Trim() == Resources.STR_NOTE_EDIT_ITEMS.Trim())
                        {

                            poCostManagementForm.PoCostManagement.LocalMassApplyNotes.Enable();

                            poCostManagementForm.PoCostManagement.LocalMassApplyNotes.Focus();

                        }

                        else
                        {

                            poCostManagementForm.PoCostManagement.LocalMassApplyNotes.Value = "";

                            poCostManagementForm.PoCostManagement.LocalMassApplyNotes.Disable();


                        }

                    }

                }

            }

            catch (Exception ex)
            {

                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoCostManagementFormLocalNotesDdl_Change Method: " + ex.Message.ToString());

                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);

            }

            finally
            {

                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

                logMessage = null;

            }

        }

        /// <summary>
        /// When accept button of po cost management window is clicked then append user defined note to all lines
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //void PoCostManagementFormApplyButton_ClickAfterOriginal(object sender, EventArgs e)
        //{

        //    StringBuilder logMessage = new StringBuilder();

        //    try
        //    {

        //        string textField = "";

        //        if (!poCostManagementForm.PoCostManagement.LocalNotesDdl.IsEmpty)
        //        {

        //            string actionbuttonName = poCostManagementForm.Functions.PoCostManagementNotesIndex.Invoke(poCostManagementForm.PoCostManagement.LocalNotesDdl.Value);

        //            if (!String.IsNullOrEmpty(actionbuttonName))
        //            {

        //                if (actionbuttonName.ToString().Trim() == Resources.STR_NOTE_EDIT_ITEMS.Trim())
        //                {

        //                    //{Check if there is a record being displayed.}

        //                    if (String.IsNullOrEmpty(poCostManagementForm.PoCostManagement.LocalMassApplyNotes.Value))
        //                    {

        //                        MessageBox.Show("Please, enter note", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        //                        return;

        //                    }

        //                    else
        //                    {

        //                        textField = poCostManagementForm.PoCostManagement.LocalMassApplyNotes.Value;

        //                    }

        //                }

        //                else
        //                {

        //                    textField = actionbuttonName;

        //                }

        //            }

        //            else
        //            {

        //                MessageBox.Show("Please, select value from note list", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        //                return;

        //            }



        //            // set status as accepted for all lines

        //            poCostManagementForm.Tables.PoLineCostDetails.Close();

        //            poCostManagementForm.Tables.PoLineCostDetails.Release();



        //            TableError tableError = poCostManagementForm.Tables.PoLineCostDetails.ChangeFirst();

        //            while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
        //            {

        //                // attach mass note to line note

        //                poCostManagementForm.Tables.PoLineCostDetails.CostNotes.Value = textField.ToString().Trim();



        //                poCostManagementForm.Tables.PoLineCostDetails.Save();



        //                tableError = poCostManagementForm.Tables.PoLineCostDetails.ChangeNext();

        //            }

        //            poCostManagementForm.Tables.PoLineCostDetails.Close();

        //            poCostManagementForm.Tables.PoLineCostDetails.Release();



        //            poCostManagementForm.Procedures.PoCostManagementFormScrollFill.Invoke();

        //        }

        //        else
        //        {

        //            MessageBox.Show("Operation denied: Note list empty", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        //            return;

        //        }

        //    }

        //    catch (Exception ex)
        //    {

        //        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoCostManagementFormApplyButton_ClickAfterOriginal Method: " + ex.Message.ToString());

        //        MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);

        //    }

        //    finally
        //    {

        //        LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

        //        logMessage = null;

        //    }

        //}
        void PoCostManagementFormApplyButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                string textField = "";
                if (!poCostManagementForm.PoCostManagement.LocalNotesDdl.IsEmpty)
                {
                    string actionbuttonName = poCostManagementForm.Functions.PoCostManagementNotesIndex.Invoke(poCostManagementForm.PoCostManagement.LocalNotesDdl.Value);
                    if (!String.IsNullOrEmpty(actionbuttonName))
                    {
                        if (actionbuttonName.ToString().Trim() == Resources.STR_NOTE_EDIT_ITEMS.Trim())
                        {
                            //{Check if there is a record being displayed.}
                            if (String.IsNullOrEmpty(poCostManagementForm.PoCostManagement.LocalMassApplyNotes.Value))
                            {
                                MessageBox.Show("Please, enter note", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            else
                            {
                                textField = poCostManagementForm.PoCostManagement.LocalMassApplyNotes.Value;
                            }
                        }
                        else
                        {
                            textField = actionbuttonName;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please, select value from note list", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // set status as accepted for all lines
                    poCostManagementForm.Tables.PoLineCostDetails.Close();
                    poCostManagementForm.Tables.PoLineCostDetails.Release();
                    TableError tableError = poCostManagementForm.Tables.PoLineCostDetails.ChangeFirst();
                    while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        // attach mass note to line note
                        poCostManagementForm.Tables.PoLineCostDetails.PoReasonCode.Value = textField.ToString().Trim();
                        poCostManagementForm.Tables.PoLineCostDetails.Save();
                        tableError = poCostManagementForm.Tables.PoLineCostDetails.ChangeNext();
                    }
                    poCostManagementForm.Tables.PoLineCostDetails.Close();
                    poCostManagementForm.Tables.PoLineCostDetails.Release();
                    poCostManagementForm.Procedures.PoCostManagementFormScrollFill.Invoke();
                }
                else
                {
                    MessageBox.Show("Operation denied: Note list empty", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoCostManagementFormApplyButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }


        /// <summary>
        /// When update button of po cost management window is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PoCostManagementFormUpdateButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            PurchaseOrderRequest purchaseRequest = null;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                purchaseRequest = new PurchaseOrderRequest();
                List<PurchaseCostManagement> poCostMgntTableMain = new List<PurchaseCostManagement>();
                poCostMgntTableMain = LoadPOCostData();   // load data into local data table
                if (poCostMgntTableMain != null && poCostMgntTableMain.Count > 0)
                {
                    ResetPOPEntryValues(poCostMgntTableMain);   // reset value to pop-entry screen
                    SavePoUnitCostDetails(poCostMgntTableMain);
                }
                poCostManagementForm.PoCostManagement.Close();   // close form
                ResetCostDetails();
                if (Dynamics.Globals.CompanyId.Value == 1)
                    UpdateHasCostVariance(poCostMgntTableMain);   //Update HasCostVariance If Cost status is "RemindMeLater"

            }

            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoCostManagementFormUpdateButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Save PO Unit Cost Details
        /// </summary>
        private void SavePoUnitCostDetails(List<PurchaseCostManagement> purchaseMgtInformation)
        {
            PurchaseOrderRequest purchaseRequest = null;
            StringBuilder logMessage = new StringBuilder();
            List<PurchaseCostManagement> PurchaseCostMgt = new List<PurchaseCostManagement>();
            try
            {
                purchaseRequest = new PurchaseOrderRequest();
                PurchaseCostMgt = purchaseMgtInformation;
                purchaseRequest.PurchaseCostMgt = PurchaseCostMgt;
                purchaseRequest.CompanyID = Dynamics.Globals.CompanyId.Value;
                purchaseRequest.UserId = Dynamics.Globals.UserId;
                if (purchaseRequest.PurchaseCostMgt != null && purchaseRequest.PurchaseCostMgt.Count > 0)
                {
                    if (purchaseRequest != null)
                    {
                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            var response = client.PostAsJsonAsync("api/PurchaseOrder/SavePoUnitCostDetails", purchaseRequest); // we need to refer the web.api service url here.
                            if (response.Result.IsSuccessStatusCode)
                            {
                                PurchaseOrderResponse purchaseResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                                if (purchaseResponse.Status == ResponseStatus.Error)
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoCostManagementFormUpdateButton_ClickAfterOriginal Method (UpdatePoCostNotes): " + purchaseResponse.ErrorMessage.ToString());
                                    MessageBox.Show(purchaseResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                            }
                            else
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not execute SavePoUnitCostDetails");
                                MessageBox.Show("Error: Data does not execute SavePoUnitCostDetails", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SavePoUnitCostDetails Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }



        /// <summary>
        /// HasCostVariance is set to 1 if CostStatus is "RemindMeLater"
        /// </summary>
        /// <param name="purchaseMgtInformation"></param>
        private void UpdateHasCostVariance(List<PurchaseCostManagement> purchaseMgtInformation)
        {
            PurchaseOrderRequest hasCostVarianceRequest = null;
            StringBuilder logMessage = new StringBuilder();
            List<PurchaseCostManagement> hasCostMgt = new List<PurchaseCostManagement>();
            try
            {
                hasCostVarianceRequest = new PurchaseOrderRequest();
                hasCostMgt = purchaseMgtInformation;
                hasCostVarianceRequest.PurchaseCostMgt = hasCostMgt;
                hasCostVarianceRequest.CompanyID = Dynamics.Globals.CompanyId.Value;
                if (hasCostVarianceRequest != null)
                {
                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/PurchaseOrder/UpdateHasCostVariance", hasCostVarianceRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            PurchaseOrderResponse purchaseResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                            if (purchaseResponse.Status == ResponseStatus.Error)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoCostManagementFormUpdateButton_ClickAfterOriginal Method (UpdatePoCostNotes): " + purchaseResponse.ErrorMessage.ToString());
                                MessageBox.Show(purchaseResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not execute po cost mgt details Table");
                            MessageBox.Show("Error: Data does not execute po cost mgt details Table", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In UpdateHasCostVariance Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }



        /// <summary>
        /// When cancel button of po cost management window is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PoCostManagementFormCancelButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                poCostManagementForm.PoCostManagement.Close(); // close form on close button
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoCostManagementFormCancelButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }



        /// <summary>
        /// Open note window
        /// </summary>

        //void PoCostManagementFormNotePresentButtonWindowArea_ClickAfterOriginal(object sender, EventArgs e)
        //{

        //    StringBuilder logMessage = new StringBuilder();

        //    try
        //    {

        //        string l_Note_ID;

        //        bool l_Found;

        //        short l_Form_Number;

        //        short l_Exists;



        //        decimal l_note_index;



        //        poCostManagementForm.PoCostManagement.PoCostManagementScroll.LocalCostStatus.Focus();



        //        // Set the note ID to the ID for the record

        //        //{set l_Note_ID to 'Item Number' of window Line_Scroll;} 

        //        l_Note_ID = poCostManagementForm.PoCostManagement.PoCostManagementScroll.ItemNumber.Value.ToString().Trim();



        //        //{Check if there is a record being displayed.}

        //        if (String.IsNullOrEmpty(l_Note_ID))
        //        {

        //            MessageBox.Show("Please, set focus on status (Accepted/Rejected) on current line", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        //            return;

        //        }



        //        // Check if the note is missing because someone deleted the record or just the note.

        //        l_note_index = Convert.ToDecimal(poCostManagementForm.PoCostManagement.PoCostManagementScroll.NoteIndex.Value);



        //        Dynamics.Procedures.CheckForRecordNote.Invoke(l_note_index, out l_Exists);



        //        // Check if a note window is already open for this record.  If a note window is already open, then it is brought to the front. 

        //        // If a note window is not open yet, then an available note window is used it there is one.



        //        Dynamics.Procedures.CheckNoteIndex.Invoke(l_note_index, out l_Found, out l_Form_Number);



        //        if (!l_Found)
        //        {

        //            Dynamics.Procedures.GetNextFormNoteToOpen.Invoke(out l_Form_Number);



        //            if (l_Form_Number == 0)
        //            {

        //                MessageBox.Show("Unable to open form window", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        //                return;

        //            }

        //        }



        //        // Open the appropriate note window

        //        if (l_Form_Number == 1)
        //        {

        //            Dynamics.Forms.FormNote1.FormNote1.Open();

        //            Dynamics.Forms.FormNote1.FormNote1.RecordNote.Value = true;

        //            Dynamics.Forms.FormNote1.FormNote1.NoteId.Value = l_Note_ID;



        //            Dynamics.Forms.FormNote1.FormNote1.NoteIndex.Value = l_note_index;



        //            if (!l_Found)
        //            {

        //                Dynamics.Forms.FormNote1.FormNote1.LocalDummyWinPre.RunValidate();

        //            }

        //        }



        //        if (l_Form_Number == 2)
        //        {

        //            Dynamics.Forms.FormNote2.FormNote2.Open();

        //            Dynamics.Forms.FormNote2.FormNote2.RecordNote.Value = true;

        //            Dynamics.Forms.FormNote2.FormNote2.NoteId.Value = l_Note_ID;

        //            Dynamics.Forms.FormNote2.FormNote2.NoteIndex.Value = l_note_index;



        //            if (!l_Found)
        //            {

        //                Dynamics.Forms.FormNote2.FormNote2.LocalDummyWinPre.RunValidate();

        //            }

        //        }



        //        if (l_Form_Number == 3)
        //        {

        //            Dynamics.Forms.FormNote3.FormNote3.Open();

        //            Dynamics.Forms.FormNote3.FormNote3.RecordNote.Value = true;

        //            Dynamics.Forms.FormNote3.FormNote3.NoteId.Value = l_Note_ID;



        //            Dynamics.Forms.FormNote3.FormNote3.NoteIndex.Value = l_note_index;



        //            if (!l_Found)
        //            {

        //                Dynamics.Forms.FormNote3.FormNote3.LocalDummyWinPre.RunValidate();

        //            }

        //        }



        //        if (l_Form_Number == 4)
        //        {

        //            Dynamics.Forms.FormNote4.FormNote4.Open();

        //            Dynamics.Forms.FormNote4.FormNote4.RecordNote.Value = true;

        //            Dynamics.Forms.FormNote4.FormNote4.NoteId.Value = l_Note_ID;



        //            Dynamics.Forms.FormNote4.FormNote4.NoteIndex.Value = l_note_index;



        //            if (!l_Found)
        //            {

        //                Dynamics.Forms.FormNote4.FormNote4.LocalDummyWinPre.RunValidate();

        //            }

        //        }



        //        if (l_Form_Number == 5)
        //        {

        //            Dynamics.Forms.FormNote5.FormNote5.Open();

        //            Dynamics.Forms.FormNote5.FormNote5.RecordNote.Value = true;

        //            Dynamics.Forms.FormNote5.FormNote5.NoteId.Value = l_Note_ID;



        //            Dynamics.Forms.FormNote5.FormNote5.NoteIndex.Value = l_note_index;



        //            if (!l_Found)
        //            {

        //                Dynamics.Forms.FormNote5.FormNote5.LocalDummyWinPre.RunValidate();

        //            }

        //        }



        //    }

        //    catch (Exception ex)
        //    {

        //        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoCostManagementFormNotePresentButtonWindowArea_ClickAfterOriginal Method: " + ex.Message.ToString());

        //        MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);

        //    }

        //    finally
        //    {

        //        LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");

        //        logMessage = null;

        //    }

        //}



        /// <summary>
        /// Load data into table before closing po cost management window
        /// </summary>
        /// <returns>return table with updated values</returns>
        private List<PurchaseCostManagement> LoadPOCostData()
        {
            PurchaseOrderRequest purchaseRequest = null;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                purchaseRequest = new PurchaseOrderRequest();
                PurchaseOrderEntity poCostEntity = new PurchaseOrderEntity();
                purchaseRequest.PurchaseCostMgt = new List<PurchaseCostManagement>();

                poCostManagementForm.Tables.PoLineCostDetails.Close();
                poCostManagementForm.Tables.PoLineCostDetails.Release();

                TableError tableError = poCostManagementForm.Tables.PoLineCostDetails.GetFirst();
                while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    PurchaseCostManagement poCostMgtInformation = new PurchaseCostManagement();
                    poCostMgtInformation.PoNumber = poCostManagementForm.Tables.PoLineCostDetails.PoNumber.Value.ToString().Trim();
                    poCostMgtInformation.Ord = poCostManagementForm.Tables.PoLineCostDetails.Ord.Value;
                    poCostMgtInformation.LineNumber = Convert.ToInt16(poCostManagementForm.Tables.PoLineCostDetails.LineCount.Value);
                    poCostMgtInformation.ItemNumber = poCostManagementForm.Tables.PoLineCostDetails.ItemNumber.Value.ToString().Trim();
                    poCostMgtInformation.UOfM = poCostManagementForm.Tables.PoLineCostDetails.UOfM.Value.ToString().Trim();
                    poCostMgtInformation.UnitCost = Convert.ToDecimal(poCostManagementForm.Tables.PoLineCostDetails.UnitCost.Value);
                    poCostMgtInformation.ProposedUnitCost = Convert.ToDecimal(poCostManagementForm.Tables.PoLineCostDetails.ProposedUnitPrice.Value);
                    poCostMgtInformation.CostStatus = Convert.ToInt16(poCostManagementForm.Tables.PoLineCostDetails.CostStatus.Value);
                    poCostMgtInformation.PoType = popPOEntryForm.PopPoEntry.PoType.Value;

                    if (poCostMgtInformation.CostStatus == 2)
                        poCostMgtInformation.HasCostVariance = 1;
                    else
                        poCostMgtInformation.HasCostVariance = 0;

                    poCostMgtInformation.IsDeleted = false;
                    poCostMgtInformation.VendorId = poCostManagementForm.Tables.PoLineCostDetails.VendorId;
                    string statusValue = "";
                    if (Convert.ToInt16(poCostManagementForm.Tables.PoLineCostDetails.CostStatus.Value) == 0)
                        statusValue = "Accepted";
                    else if (Convert.ToInt16(poCostManagementForm.Tables.PoLineCostDetails.CostStatus.Value) == 1)
                        statusValue = "Rejected";
                    else
                        statusValue = "RemaindMeLater";
                    //poCostMgtInformation.QtyCancel = Convert.ToDecimal(poCostManagementForm.Tables.PoLineCostDetails.QtyCanceled.Value);
                    //poCostMgtInformation.QtyOrder = Convert.ToDecimal(poCostManagementForm.Tables.PoLineCostDetails.QtyOrdered.Value);
                    poCostMgtInformation.CostBookCost = poCostManagementForm.Tables.PoLineCostDetails.CostBookCost.Value.ToString().Trim();
                    poCostMgtInformation.CostSupportId = poCostManagementForm.Tables.PoLineCostDetails.CpPoCostSupportId.Value.ToString().Trim();
                    poCostMgtInformation.LineCostNotes = poCostManagementForm.Tables.PoLineCostDetails.LineCostNotes.Value.ToString().Trim();

                    // customized note as per requirement
                    if (poCostManagementForm.Tables.PoLineCostDetails.CostNotes.Value.ToString().Trim() != "")
                    {
                        poCostMgtInformation.CostNotes = System.DateTime.Now.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) + " " + System.DateTime.Now.ToString("hh:mm:ss tt") +
                                            " " + poCostManagementForm.Tables.PoLineCostDetails.UserId.Value.ToString().Trim() +
                                            " " + statusValue +
                                            " " + poCostManagementForm.Tables.PoLineCostDetails.LineCostNotes.Value.ToString().Trim() +
                                            " " + poCostManagementForm.Tables.PoLineCostDetails.CostNotes.Value.ToString().Trim();
                    }
                    else
                    {
                        poCostMgtInformation.CostNotes = "";
                    }
                    poCostMgtInformation.NoteIndex = Convert.ToDecimal(poCostManagementForm.Tables.PoLineCostDetails.NoteIndex.Value);
                    poCostMgtInformation.UserId = poCostManagementForm.Tables.PoLineCostDetails.UserId.Value.ToString().Trim();
                    poCostMgtInformation.ItemDescription = poCostManagementForm.Tables.PoLineCostDetails.ItemDescription.Value.ToString().Trim();
                    poCostMgtInformation.CostSupportCost = poCostManagementForm.Tables.PoLineCostDetails.CostSupportCostAmount.Value;
                    poCostMgtInformation.PoCostVariance = poCostManagementForm.Tables.PoLineCostDetails.PoCostVariance.Value;
                    poCostMgtInformation.Reason = poCostManagementForm.Tables.PoLineCostDetails.PoReasonCode.Value.ToString().Trim();
                    poCostMgtInformation.PoLineCostSource = poCostManagementForm.Tables.PoLineCostDetails.PoLineCostSource.Value.ToString().Trim();
                    purchaseRequest.PurchaseCostMgt.Add(poCostMgtInformation);
                    tableError = poCostManagementForm.Tables.PoLineCostDetails.GetNext();
                }
                poCostManagementForm.Tables.PoLineCostDetails.Close();
                poCostManagementForm.Tables.PoLineCostDetails.Release();

                //POCostManagementLoadDatatable(purchaseRequest.PurchaseCostMgt);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In LoadPOCostData Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
            return purchaseRequest.PurchaseCostMgt;
        }



        /// <summary>

        /// Reset result to po entry screen

        /// </summary>

        /// <param name="poCostMgntTable">Specify table to reset values</param>

        private void ResetPOPEntryValues(List<PurchaseCostManagement> poCostMgntTableMain)
        {

            StringBuilder logMessage = new StringBuilder();
            try
            {
                var poCostMgtList = from d in poCostMgntTableMain
                                    select d;

                if (poCostMgntTableMain != null && poCostMgntTableMain.Count > 0)
                {
                    UpdatePoCostNotes(poCostMgntTableMain);
                    var acceptQuery = poCostMgtList.Where(x => x.CostStatus.Equals(0));
                    List<PurchaseCostManagement> poCostMgntTable = acceptQuery.ToList();
                    if (poCostMgntTable.Count > 0)
                    {
                        string poNumber = (from po in poCostMgntTable select po.PoNumber).FirstOrDefault();
                        var poCostMgmtArray = poCostMgntTable.ToArray<PurchaseCostManagement>();

                        // retrieve only accepted records
                        popPOEntryForm.PopPoEntry.ExpansionButton5.RunValidate();  // open pop item details entry window
                        if (popPoItemDetailEntryForm != null)
                        {
                            popPoItemDetailEntryForm = ChempointCustomizations.Forms.PopPoItemDetailEntry;
                        }
                        if (popPoItemDetailEntryForm.IsOpen)
                            popPoItemDetailEntryForm.PopPoItemDetailEntry.PullFocus();
                        int recordsCount = 0;
                        popPoItemDetailEntryForm.PopPoItemDetailEntry.TopOfFileButtonToolbar.RunValidate();    // scroll to first records
                        // get original data from PopPoLine table and process each line on next button toolbar
                        popPOEntryForm.Tables.PopPoLine.Close();
                        popPOEntryForm.Tables.PopPoLine.Release();
                        popPOEntryForm.Tables.PopPoLine.RangeClear();
                        popPOEntryForm.Tables.PopPoLine.Ord.Value = 1;
                        popPOEntryForm.Tables.PopPoLine.BreakField1.Value = 0;
                        popPOEntryForm.Tables.PopPoLine.PoNumber.Value = poNumber;
                        popPOEntryForm.Tables.PopPoLine.RangeStart();
                        popPOEntryForm.Tables.PopPoLine.Key = 1;
                        popPOEntryForm.Tables.PopPoLine.PoNumber.Value = poNumber;
                        popPOEntryForm.Tables.PopPoLine.Ord.Fill();
                        popPOEntryForm.Tables.PopPoLine.BreakField1.Value = 0;
                        popPOEntryForm.Tables.PopPoLine.RangeEnd();
                        popPOEntryForm.Tables.PopPoLine.Key = 1;
                        TableError error = popPOEntryForm.Tables.PopPoLine.GetFirst();

                        while (error != TableError.EndOfTable && error == TableError.NoError)
                        {

                            for (int poCnt = 0; poCnt < poCostMgmtArray.Length; poCnt++)
                            {

                                // check current poNumber and Ord matches with po item details entry poNumber and Ord fields
                                if (popPoItemDetailEntryForm.PopPoItemDetailEntry.PoNumber.Value.ToString().Trim() == poCostMgmtArray[poCnt].PoNumber
                                    && popPoItemDetailEntryForm.PopPoItemDetailEntry.Ord.Value == Convert.ToInt32(poCostMgmtArray[poCnt].Ord))
                                {
                                    //set freight term from CpPoFreightTerms table if value is blank - ( Error: window could not scroll if freight term is blank)
                                    if (popPoItemDetailEntryForm.PopPoItemDetailEntry.CpPoFreightTerms.Value.ToString().Trim() == "")
                                    {
                                        ChempointCustomizations.Tables.CpPoHdrFreightDetails.Close();
                                        ChempointCustomizations.Tables.CpPoHdrFreightDetails.Release();
                                        ChempointCustomizations.Tables.CpPoHdrFreightDetails.PoNumber.Value = poCostMgmtArray[poCnt].PoNumber;
                                        ChempointCustomizations.Tables.CpPoHdrFreightDetails.Key = 1;
                                        TableError tableError = ChempointCustomizations.Tables.CpPoHdrFreightDetails.Get();
                                        if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                                        {
                                            popPoItemDetailEntryForm.PopPoItemDetailEntry.CpPoFreightTerms.Value = ChempointCustomizations.Tables.CpPoHdrFreightDetails.CpPoFreightTerms.Value.ToString().Trim();
                                        }
                                        ChempointCustomizations.Tables.CpPoHdrFreightDetails.Release();
                                        ChempointCustomizations.Tables.CpPoHdrFreightDetails.Close();
                                    }
                                    // reset purpose cost to unit cost
                                    popPoItemDetailEntryForm.PopPoItemDetailEntry.UnitCost.Value = Convert.ToDecimal(poCostMgmtArray[poCnt].ProposedUnitCost);
                                    popPoItemDetailEntryForm.PopPoItemDetailEntry.UnitCost.RunValidate();
                                    popPoItemDetailEntryForm.PopPoItemDetailEntry.ExtendedCost.Focus();
                                    popPoItemDetailEntryForm.PopPoItemDetailEntry.SaveButton.RunValidate();
                                    recordsCount++;
                                }
                            }
                            if (recordsCount == poCostMgntTable.Count)
                                break;  // end of records
                            popPoItemDetailEntryForm.PopPoItemDetailEntry.NextButtonToolbar.RunValidate();
                            error = popPOEntryForm.Tables.PopPoLine.GetNext();
                        }
                        popPOEntryForm.Tables.PopPoLine.Close();
                        popPOEntryForm.Tables.PopPoLine.Release();
                        if (popPoItemDetailEntryForm != null && popPoItemDetailEntryForm.IsOpen)
                            popPoItemDetailEntryForm.Close();
                        foreach (var poCostMgt in poCostMgntTable)
                        {
                            //Check HasCostVaraince in Po Item entry window if cost status is "RemindMeLater"
                            if (poCostMgt.CostStatus == 2)
                            {
                                ChempointCustomizations.Tables.CpPoCostVariance.Close();
                                ChempointCustomizations.Tables.CpPoCostVariance.Release();
                                ChempointCustomizations.Tables.CpPoCostVariance.PoNumber.Value = poCostMgt.PoNumber.Trim();
                                ChempointCustomizations.Tables.CpPoCostVariance.Ord.Value = poCostMgt.Ord;
                                ChempointCustomizations.Tables.CpPoCostVariance.Key = 1;
                                TableError tableErrorValue = ChempointCustomizations.Tables.CpPoCostVariance.Change();
                                if (tableErrorValue == TableError.NoError)
                                {
                                    ChempointCustomizations.Tables.CpPoCostVariance.PoNumber.Value = poCostMgt.PoNumber.Trim();
                                    ChempointCustomizations.Tables.CpPoCostVariance.Ord.Value = poCostMgt.Ord;
                                    ChempointCustomizations.Tables.CpPoCostVariance.ItemNumber.Value = poCostMgt.ItemNumber.Trim();
                                    ChempointCustomizations.Tables.CpPoCostVariance.CpHasCostVariance.Value = Convert.ToBoolean(poCostMgt.HasCostVariance);
                                    ChempointCustomizations.Tables.CpPoCostVariance.Save();
                                }
                                ChempointCustomizations.Tables.CpPoCostVariance.Release();
                                ChempointCustomizations.Tables.CpPoCostVariance.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ResetPOPEntryValues Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }

            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// Update po cost notes to existing qty ordered notes
        /// </summary>
        /// <param name="noteIndex">Note index</param>
        /// <param name="costNotes">Required cost notes</param>
        private void UpdatePoCostNotes(List<PurchaseCostManagement> poCostMgtList)
        {
            PurchaseOrderRequest purchaseRequest = null;
            StringBuilder logMessage = new StringBuilder();
            List<PurchaseCostManagement> PurchaseCostMgt = new List<PurchaseCostManagement>();
            try
            {
                purchaseRequest = new PurchaseOrderRequest();
                PurchaseCostMgt = poCostMgtList;
                purchaseRequest.PurchaseCostMgt = PurchaseCostMgt;
                purchaseRequest.CompanyID = Dynamics.Globals.CompanyId.Value;
                purchaseRequest.UserId = Dynamics.Globals.UserId;
                purchaseRequest.CompanyID = Dynamics.Globals.CompanyId.Value;
                if (purchaseRequest != null)
                {
                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/PurchaseOrder/UpdatePoCostNotes", purchaseRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            PurchaseOrderResponse purchaseResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                            if (purchaseResponse.Status == ResponseStatus.Error)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In UpdatePoCostNotes Method (UpdatePoCostNotes): " + purchaseResponse.ErrorMessage.ToString());
                                MessageBox.Show(purchaseResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not execute UpdatePoCostNotes");
                            MessageBox.Show("Error: Data does not execute UpdatePoCostNotes", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In UpdatePoCostNotes Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }





        /// <summary>
        ///Set status as accepted, rejected and remindMeLater for all lines depends upon Cost status
        /// </summary>
        void PoCostManagementFormPoCostManagement_LocalCostApplyRejectAll(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                poCostManagementForm.Tables.PoLineCostDetails.Close();
                poCostManagementForm.Tables.PoLineCostDetails.Release();

                TableError tableError = poCostManagementForm.Tables.PoLineCostDetails.ChangeFirst();
                while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    poCostManagementForm.Tables.PoLineCostDetails.CostStatus.Value = poCostManagementForm.PoCostManagement.LocalCostApplyRejectAll.Value;
                    poCostManagementForm.Tables.PoLineCostDetails.Save();
                    tableError = poCostManagementForm.Tables.PoLineCostDetails.ChangeNext();
                }
                poCostManagementForm.Tables.PoLineCostDetails.Release();
                poCostManagementForm.Tables.PoLineCostDetails.Close();
                poCostManagementForm.Procedures.PoCostManagementFormScrollFill.Invoke();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoCostManagementFormPoCostManagement_LocalCostApplyRejectAll Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }



        private void ChangeApplyToAllLinesRadioButton(int costStatus)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                short costValue;
                short.TryParse(costStatus.ToString(), out costValue);
                poCostManagementForm.PoCostManagement.LocalCostApplyRejectAll.Value = costValue;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ChangeApplyToAllLinesRadioButton Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }



        /// <summary>
        /// Reset result to po entry screen
        /// </summary>
        /// <param name="poCostMgntTable">Specify table to reset values</param>

        private void SaveUnitCostChangestoAudit()
        {
            PurchaseOrderRequest purchaseRequest = null;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                // load data into request object
                purchaseRequest = new PurchaseOrderRequest();
                PurchaseOrderEntity purchaseEntity = new PurchaseOrderEntity();
                purchaseRequest.PurchaseCostMgt = new List<PurchaseCostManagement>();

                popPOEntryForm.Tables.PoCostManagementDetails.Close();
                popPOEntryForm.Tables.PoCostManagementDetails.Release();

                TableError tableError = popPOEntryForm.Tables.PoCostManagementDetails.GetFirst();
                while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    if (popPOEntryForm.Tables.PoCostManagementDetails.PoNumber.Value.ToString().Trim() == popPOEntryForm.PopPoEntry.PoNumber.Value.ToString().Trim())
                    {
                        PurchaseCostManagement PurchaseCostMgtInformation = new PurchaseCostManagement();
                        PurchaseCostMgtInformation.PoNumber = popPOEntryForm.Tables.PoCostManagementDetails.PoNumber.Value;
                        PurchaseCostMgtInformation.Ord = popPOEntryForm.Tables.PoCostManagementDetails.Ord.Value;
                        PurchaseCostMgtInformation.LineNumber = popPOEntryForm.Tables.PoCostManagementDetails.LineCount.Value;
                        PurchaseCostMgtInformation.ItemNumber = popPOEntryForm.Tables.PoCostManagementDetails.ItemNumber.Value;
                        PurchaseCostMgtInformation.QtyOrder = popPOEntryForm.Tables.PoCostManagementDetails.QtyOrdered.Value;
                        PurchaseCostMgtInformation.UnitCost = popPOEntryForm.Tables.PoCostManagementDetails.UnitCost.Value;
                        PurchaseCostMgtInformation.UserId = Dynamics.Globals.UserId.Value.ToString();
                        purchaseRequest.PurchaseCostMgt.Add(PurchaseCostMgtInformation);
                    }
                    tableError = popPOEntryForm.Tables.PoCostManagementDetails.GetNext();
                }
                popPOEntryForm.Tables.PoCostManagementDetails.Release();
                popPOEntryForm.Tables.PoCostManagementDetails.Close();

                if (purchaseRequest.PurchaseCostMgt != null && purchaseRequest.PurchaseCostMgt.Count > 0)
                {
                    purchaseRequest.CompanyID = Dynamics.Globals.CompanyId.Value;
                    if (purchaseRequest != null)
                    {
                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/PurchaseOrder/SavePoCostManagementChangestoAudit", purchaseRequest); // we need to refer the web.api service url here.
                            if (response.Result.IsSuccessStatusCode)
                            {
                                PurchaseOrderResponse purchaseResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                                if (purchaseResponse.Status == ResponseStatus.Error)
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SaveUnitCostChangestoAudit Method (SavePoCostManagementChangestoAudit): " + purchaseResponse.ErrorMessage.ToString());
                                    MessageBox.Show(purchaseResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                            }
                            else
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does save into po cost mgt details Table");
                                MessageBox.Show("Error: Data does not save into po cost mgt details Table", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SaveUnitCostChangestoAudit Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }

        public void FetchingMaterialManagementDetails(bool isMaterialAction)
        {
            StringBuilder logMessage = new StringBuilder();
            string poNumber= string.Empty,vendorName= string.Empty, itemNumber = string.Empty;
            int ordNumber = 0;
            bool isUpdated = false;            
            //DataTable costVarianceDT = null;
            String mailSubject = string.Empty;
            StringBuilder mailBody = new StringBuilder();
            StringBuilder mailSignature = new StringBuilder();
            EMailInformation emailInformation = null;
            PurchaseOrderRequest purchaseRequest = null;
            try
            {
                purchaseRequest = new PurchaseOrderRequest();
                emailInformation = new EMailInformation();
                popPOEntryForm.Tables.PoCostManagementDetails.Close();
                popPOEntryForm.Tables.PoCostManagementDetails.Release();
                TableError tempError = popPOEntryForm.Tables.PoCostManagementDetails.GetFirst();
                while (tempError == TableError.NoError && tempError != TableError.EndOfTable && isUpdated != true)
                {
                    //if (popPOEntryForm.Tables.PoCostManagementDetails.IsCostUpdated.Value == 1)
                    //{
                        poNumber = popPOEntryForm.Tables.PoCostManagementDetails.PoNumber.Value;
                        vendorName = popPOEntryForm.Tables.PoCostManagementDetails.VendorId.Value;
                        itemNumber = popPOEntryForm.Tables.PoCostManagementDetails.ItemNumber.Value;
                        ordNumber = popPOEntryForm.Tables.PoCostManagementDetails.Ord.Value;
                        isUpdated = true;
                    //}
                    tempError = popPOEntryForm.Tables.PoCostManagementDetails.GetNext();
                }
                popPOEntryForm.Tables.PoCostManagementDetails.Release();
                popPOEntryForm.Tables.PoCostManagementDetails.Close();


                int returnValue = ValidateMaterialManagmentDetails(poNumber, ordNumber, itemNumber, vendorName);
                if (returnValue != 0 && isMaterialAction==false && !string.IsNullOrEmpty(firstPOForMail))
                {
                    if (returnValue == 1)
                    {
                        mailBody.AppendFormat(Resources.STR_MailBodyForFirstPo, poNumber);
                        mailSubject = string.Format(Resources.STR_MailSubjectForFirstPo, poNumber);
                        MessageBox.Show(string.Format(Resources.STR_MailBodyForFirstPo, poNumber)
                            , Resources.STR_MESSAGE_TITLE);
                    }
                    if (returnValue == 2)
                    {
                        mailBody.AppendFormat(Resources.STR_MailBodyForCostVariance, poNumber);
                        mailSubject = string.Format(Resources.STR_MailSubjectForCostVariance, poNumber);
                        MessageBox.Show(string.Format(Resources.STR_MailBodyForCostVariance, poNumber)
                            , Resources.STR_MESSAGE_TITLE);
                    }
                    if (returnValue == 3)
                    {
                        mailBody.AppendFormat(Resources.STR_MailBodyForBothPOAndCostVariance, poNumber);
                        mailSubject = string.Format(Resources.STR_MailSubjectForBothPOAndCostVariance, poNumber);
                        MessageBox.Show(string.Format(Resources.STR_MailBodyForBothPOAndCostVariance, poNumber)
                           , Resources.STR_MESSAGE_TITLE);
                    }
                    mailBody.Append(Environment.NewLine);
                    mailBody.AppendLine("Vendor :" + popPOEntryForm.PopPoEntry.VendorId.Value + " - " + popPOEntryForm.PopPoEntry.VendorName.Value);
                    mailBody.Replace(Environment.NewLine, "<br />").ToString();
                    mailBody.AppendLine("Buyer   : " + popPOEntryForm.PopPoEntry.BuyerId.Value);
                    mailBody.Replace(Environment.NewLine, "<br />").ToString();
                    if (returnValue == 2)
                        mailBody.AppendLine("User:     " + Dynamics.Globals.UserId.Value);
                    mailBody.Replace(Environment.NewLine, "<br />").ToString();
                    mailSignature.AppendLine("Notes: ");

                    foreach (string notes in PoCostNotesHistory)
                    {
                        mailSignature.AppendLine(notes != null ? notes.Trim() : string.Empty);
                        mailSignature.Replace(Environment.NewLine, "<br />").ToString();
                    }

                    //if (returnValue == 2)
                    //{
                    //    costVarianceDT = new DataTable();
                    //    costVarianceDT.Columns.Add("PONumber", typeof(string));
                    //    costVarianceDT.Columns.Add("POLineItemNumber", typeof(int));
                    //    costVarianceDT.Columns.Add("ItemNumber", typeof(string));
                    //    costVarianceDT.Columns.Add("CurrentCost", typeof(decimal));
                    //    costVarianceDT.Columns.Add("CostEntered", typeof(decimal));
                    //    costVarianceDT.Columns.Add("CostVariance", typeof(decimal));
                    //    costVarianceDT.Columns.Add("ApprovalRequired", typeof(string));

                    //    popPOEntryForm.Tables.PoCostManagementDetails.Close();
                    //    popPOEntryForm.Tables.PoCostManagementDetails.Release();
                    //    TableError tempCostVariance = popPOEntryForm.Tables.PoCostManagementDetails.GetFirst();
                    //    while (tempCostVariance == TableError.NoError && tempCostVariance != TableError.EndOfTable)
                    //    {
                    //        if (popPOEntryForm.Tables.PoCostManagementDetails.IsCostUpdated.Value == 1)
                    //        {
                    //            DataRow newRow = costVarianceDT.NewRow();
                    //            newRow["PONumber"] = popPOEntryForm.Tables.PoCostManagementDetails.PoNumber.Value.Trim();
                    //            newRow["POLineItemNumber"] = popPOEntryForm.Tables.PoCostManagementDetails.LineCount.Value;
                    //            newRow["ItemNumber"] = popPOEntryForm.Tables.PoCostManagementDetails.ItemNumber.Value.Trim();
                    //            newRow["CurrentCost"] = popPOEntryForm.Tables.PoCostManagementDetails.ProposedUnitPrice.Value;
                    //            newRow["CostEntered"] = popPOEntryForm.Tables.PoCostManagementDetails.UnitCost.Value;
                    //            newRow["CostVariance"] = decimal.Round(popPOEntryForm.Tables.PoCostManagementDetails.PoCostVariance.Value, 2, MidpointRounding.AwayFromZero);
                    //            newRow["ApprovalRequired"] = popPOEntryForm.Tables.PoCostManagementDetails.UnitCost.Value + " against the current line cost " +
                    //                 popPOEntryForm.Tables.PoCostManagementDetails.ProposedUnitPrice.Value;
                    //            costVarianceDT.Rows.Add(newRow);
                    //        }
                    //        tempCostVariance = popPOEntryForm.Tables.PoCostManagementDetails.GetNext();
                    //    }
                    //    popPOEntryForm.Tables.PoCostManagementDetails.Release();
                    //    popPOEntryForm.Tables.PoCostManagementDetails.Close();
                    //}

                    emailInformation.Subject = mailSubject.ToString();
                    emailInformation.Body = mailBody.ToString();
                    emailInformation.Signature = mailSignature.ToString();
                    emailInformation.IsDataTableBodyRequired = true;
                    purchaseRequest.emailInfomation = emailInformation;
                    //purchaseRequest.AppConfigID = 1;
                    if (returnValue == 1 || returnValue == 3)
                        purchaseRequest.Report = firstPOForMail;
                    else if (returnValue == 2)
                        purchaseRequest.Report = firstPOForMail;
                    //purchaseRequest.Report = ConvertDataTableToString(costVarianceDT);
                    SendMail(purchaseRequest);
                }
                else if (returnValue == 1 && !string.IsNullOrEmpty(firstPOForMail))
                {
                    isMaterialAction = false;
                    MessageBox.Show(string.Format(Resources.STR_MailBodyForFirstPo, poNumber)
                        , Resources.STR_MESSAGE_TITLE);                    
                }
                else if(returnValue == 2 && !string.IsNullOrEmpty(firstPOForMail))
                {
                    isMaterialAction = false;
                    MessageBox.Show(string.Format(Resources.STR_MailBodyForCostVariance, poNumber)
                        , Resources.STR_MESSAGE_TITLE);
                }
                else if(returnValue == 3 && !string.IsNullOrEmpty(firstPOForMail))
                {
                    isMaterialAction = false;
                    MessageBox.Show(string.Format(Resources.STR_MailBodyForBothPOAndCostVariance, poNumber)
                       , Resources.STR_MESSAGE_TITLE);
                }

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In FetchingMaterialManagementDetails Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
                firstPOForMail = string.Empty;
                costVarianceDT = null;
                //ClearCostDetailsTemp();
                //ClearCostDetails("");
            }
        }

        private void ClearCostDetailsTemp()
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                popPOEntryForm.Tables.PoCostManagementDetails.Close();
                popPOEntryForm.Tables.PoCostManagementDetails.Release();
                TableError tempError = popPOEntryForm.Tables.PoCostManagementDetails.ChangeFirst();
                while (tempError == TableError.NoError && tempError != TableError.EndOfTable)
                {
                    popPOEntryForm.Tables.PoCostManagementDetails.Remove();
                    tempError = popPOEntryForm.Tables.PoCostManagementDetails.ChangeNext();
                }
                popPOEntryForm.Tables.PoCostManagementDetails.Release();
                popPOEntryForm.Tables.PoCostManagementDetails.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearCostDetailsTemp Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
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


        private int ValidateMaterialManagmentDetails(string PoNumber,int PoLineNumber,string ItemNumber,string vendorId)
        {
            StringBuilder logMessage = new StringBuilder();
            PurchaseOrderRequest purchaseRequest = null;
            PurchaseOrderEntity purchaseOrderEntity = null;
            PurchaseCostManagement purchaseCostManagement = null;

            string mailMessage=string.Empty;
            StringBuilder mailBody = new StringBuilder();
            int returnValue=0;
            try
            {
                if (!string.IsNullOrEmpty(PoNumber))
                {
                    purchaseRequest = new PurchaseOrderRequest();
                    purchaseOrderEntity = new PurchaseOrderEntity();
                    purchaseCostManagement = new PurchaseCostManagement();
                    purchaseCostManagement.PoNumber = PoNumber;
                    purchaseCostManagement.LineNumber = PoLineNumber;
                    purchaseCostManagement.ItemNumber = ItemNumber;
                    purchaseCostManagement.VendorId = vendorId;
                    purchaseOrderEntity.PurchaseCostMgtInformation = purchaseCostManagement;
                    purchaseRequest.PurchaseOrderEntity = purchaseOrderEntity;
                    purchaseRequest.CompanyID = Dynamics.Globals.CompanyId;
                    purchaseRequest.UserId = Dynamics.Globals.UserId;
                    if (purchaseRequest != null)
                    {
                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/PurchaseOrder/ValidatePoForMailApproval", purchaseRequest); // we need to refer the web.api service url here.
                            if (response.Result.IsSuccessStatusCode)
                            {
                                PurchaseOrderResponse purchaseResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                                if (purchaseResponse.Status == ResponseStatus.Error)
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SendMail Method: " + purchaseResponse.ErrorMessage.ToString());
                                    MessageBox.Show(purchaseResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    PoCostNotesHistory = new List<string>();
                                    PoCostNotesHistory = purchaseResponse.PurchaseOrderEntity.PurchaseCostMgtInformation.strPoCostNotesHistory;
                                    if (purchaseResponse.PurchaseOrderEntity.PurchaseCostMgtInformation.IsFirstPoForMaterialMgt == true &&
                                            purchaseResponse.PurchaseOrderEntity.PurchaseCostMgtInformation.IsValidForMaterialMgt == true)
                                    {
                                        returnValue = 3;
                                        firstPOForMail = purchaseResponse.Report;
                                    }
                                    else if (purchaseResponse.PurchaseOrderEntity.PurchaseCostMgtInformation.IsFirstPoForMaterialMgt == true)
                                    {
                                        returnValue = 1;
                                        firstPOForMail = purchaseResponse.Report;
                                    }
                                    else if (purchaseResponse.PurchaseOrderEntity.PurchaseCostMgtInformation.IsValidForMaterialMgt == true)
                                    {
                                        returnValue = 2;
                                        firstPOForMail = purchaseResponse.Report;
                                    }
                                }
                            }
                            else
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: ValidateMaterialManagmentDetails method");
                                MessageBox.Show("Error: ValidateMaterialManagmentDetails method", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ValidateMaterialManagmentDetails Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;                
            }
            return returnValue;

        }

        //public static int ValidateCostForMaterialManagement(string PoNumber, int PoLineNumber, decimal UnitCost, string ItemNumber,
        //    string vendorId, string uOfm, int qtyOrd, string itemDesc, bool isFirstPO, ref int hasVariance, ref int hasZCost, ref decimal currentCost, ref string mailMessage)
        //{
        //    Dynamics.Tables.IvItemMstr.Release();
        //    Dynamics.Tables.IvItemMstr.Close();
        //    Dynamics.Tables.IvItemMstr.ItemNumber.Value = ItemNumber.Trim().ToString();
        //    TableError tablerror = Dynamics.Tables.IvItemMstr.Get();            
        //    if (tablerror == TableError.NoError)
        //    {
        //        currentCost = Dynamics.Tables.IvItemMstr.CurrentCost.Value;
        //    }
        //    Dynamics.Tables.IvItemMstr.Release();
        //    Dynamics.Tables.IvItemMstr.Close();

        //    if(UnitCost==0 && currentCost > 0)
        //    {
        //        hasZCost = 1;
        //    }
        //    else if(UnitCost!= currentCost)
        //    {

        //    }

        //    if (isFirstPO == true ||(hasVariance == 1 || hasZCost == 1))
        //    {

        //    }

        //    if (isFirstPO == false && (hasVariance == 1 || hasZCost == 1))
        //    {

        //    }

        //    return 0;
        //}

        private void SendMail(PurchaseOrderRequest purchaseRequest)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (purchaseRequest != null)
                {
                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/PurchaseOrder/SendMailForMaterialManagement", purchaseRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            PurchaseOrderResponse purchaseResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                            if (purchaseResponse.Status == ResponseStatus.Error)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SendMail Method: " + purchaseResponse.ErrorMessage.ToString());
                                MessageBox.Show(purchaseResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Sending mail to business");
                            MessageBox.Show("Error: Sending mail to business", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SaveUnitCostChangestoAudit Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "PoCostMgtDetails");
                logMessage = null;
            }
        }



        #endregion

        #region LandedCost
        /***Venture: Landed Cost -Start***/

        void SelectButtonClearButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            selectPoLinesForm.SelectPoLines.CpCurSchedShip.Clear();
            selectPoLinesForm.SelectPoLines.CpCurSchedDel.Clear();
            selectPoLinesForm.SelectPoLines.PoAcknowledgementDate.Clear();
            selectPoLinesForm.SelectPoLines.PoConfirmedDate.Clear();
            selectPoLinesForm.SelectPoLines.ActualShipDate.Clear();
            selectPoLinesForm.SelectPoLines.PromisedDate.Clear();
            selectPoLinesForm.SelectPoLines.VendorId.Clear();
            selectPoLinesForm.SelectPoLines.LocationCode.Clear();
            selectPoLinesForm.SelectPoLines.CpHasCostVariance.Clear();
            ClearSelectPoLinesTemp();
            selectPoLinesForm.Procedures.PoIndicatorUpdateToolFormScrollFill.Invoke();
        }

        void EstimatedQtyShipped_EnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.PoNumber.Value = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.PoNumber.Value;
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Ord.Value = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.Ord.Value;
                TableError tableError = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Get();
                if (tableError == TableError.NoError)
                {
                    if (estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.IsLineMatched.Value == true)
                    {
                        MessageBox.Show("This Line is already matched.", Resources.STR_MESSAGE_TITLE);
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.AddPoLines.Focus();
                    }

                }
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }

        void EstimatedShipmentCostEntry_CloseBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsEstimateLineHasRow == true)
            {
                DialogResult result = MessageBox.Show("Do you want to save?", Resources.STR_MESSAGE_TITLE, MessageBoxButtons.YesNo);
                switch (result)
                {
                    case DialogResult.Yes:
                        {
                            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.SaveButton.RunValidate();
                            break;
                        }
                    case DialogResult.No:
                        {
                            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.Close();
                            break;
                        }
                }
                IsEstimateLineHasRow = false;
            }
        }

        void LocationCode_LeaveBeforeOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                Dynamics.Tables.IvLocationSetp.Release();
                Dynamics.Tables.IvLocationSetp.Close();
                Dynamics.Tables.IvLocationSetp.LocationCode.Value = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationCode.Value.Trim().ToString();
                TableError tablerror = Dynamics.Tables.IvLocationSetp.Get();
                if (tablerror == TableError.NotFound)
                {
                    MessageBox.Show("Location code does not exists", Resources.STR_MESSAGE_TITLE);
                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationCode.Clear();
                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationDescription.Clear();
                }
                if (tablerror == TableError.NoError)
                {
                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationDescription.Value = Dynamics.Tables.IvLocationSetp.LocationDescription.Value;
                }
                Dynamics.Tables.IvLocationSetp.Release();
                Dynamics.Tables.IvLocationSetp.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In LocationCode_LeaveBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        void VendorId_LeaveBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                Dynamics.Tables.PmVendorMstr.Release();
                Dynamics.Tables.PmVendorMstr.Close();
                Dynamics.Tables.PmVendorMstr.VendorId.Value = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorId.Value.Trim().ToString();
                TableError tablerror = Dynamics.Tables.PmVendorMstr.Get();
                if (tablerror == TableError.NotFound)
                {
                    if (estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorId.Value.ToString().Trim() != string.Empty)
                    {
                        MessageBox.Show("Vendor Id does not exists", Resources.STR_MESSAGE_TITLE);
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorId.Clear();
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorName.Clear();
                    }
                }
                if (tablerror == TableError.NoError)
                {
                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorName.Value = Dynamics.Tables.PmVendorMstr.VendorName.Value;
                }
                Dynamics.Tables.PmVendorMstr.Release();
                Dynamics.Tables.PmVendorMstr.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In VendorId_LeaveBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        void CurrencyIdKey_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            PurchaseOrderRequest purchaseOrderRequest = null;
            PurchaseShipmentEstimateDetails purchaseShipmentEstimateDetails = null;
            short shortValue;
            try
            {
                Dynamics.Tables.McCurrencySetp.Release();
                Dynamics.Tables.McCurrencySetp.Close();
                Dynamics.Tables.McCurrencySetp.CurrencyId.Value = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIdKey.Value.Trim().ToString();
                TableError tablerror = Dynamics.Tables.McCurrencySetp.Get();
                if (tablerror == TableError.NotFound)
                {
                    if (estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIdKey.Value.ToString().Trim() != string.Empty)
                    {
                        MessageBox.Show("Currency Id does not exists", Resources.STR_MESSAGE_TITLE);
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIdKey.Clear();
                    }
                }
                if (tablerror == TableError.NoError)
                {
                    purchaseOrderRequest = new PurchaseOrderRequest();
                    purchaseShipmentEstimateDetails = new PurchaseShipmentEstimateDetails();
                    purchaseShipmentEstimateDetails.CurrencyId = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIdKey.Value;
                    purchaseOrderRequest.PurchaseShipmentEstimateDetails = purchaseShipmentEstimateDetails;
                    purchaseOrderRequest.CompanyID = Dynamics.Globals.CompanyId.Value;

                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = client.PostAsJsonAsync("api/PurchaseOrder/GetCurrencyIndex/", purchaseOrderRequest); // we need to refer the web.api service url here.

                        if (response.Result.IsSuccessStatusCode)
                        {
                            PurchaseOrderResponse purchaseOrderResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                            if (purchaseOrderResponse.Status == ResponseStatus.Error)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In selectPoLinesLookupButton1_ClickAfterOriginal Method : " + purchaseOrderResponse.ErrorMessage.ToString());
                                MessageBox.Show(purchaseOrderResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                foreach (var currencyList in purchaseOrderResponse.CurrencyDetailLists)
                                {
                                    short.TryParse(currencyList.CurrencyIndex.ToString().Trim(), out shortValue);
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIndex.Value = shortValue;
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyDecimalPlaces.Value = currencyList.DecimalPlaces;
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.ExchangeRate.Value = currencyList.ExchangeRate;
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.ExpirationDate.Value = currencyList.ExchangeExpirationDate;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Error: Fetching Po Numbers", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
                Dynamics.Tables.McCurrencySetp.Release();
                Dynamics.Tables.McCurrencySetp.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CurrencyIdKey_LeaveBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        void LocalSelectAllCheckBox_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                selectPoLinesForm.Tables.SelectPoLinesTemp.Close();
                selectPoLinesForm.Tables.SelectPoLinesTemp.Release();

                TableError tableError = selectPoLinesForm.Tables.SelectPoLinesTemp.ChangeFirst();
                while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    selectPoLinesForm.Tables.SelectPoLinesTemp.SelectCheckBox.Value = selectPoLinesForm.SelectPoLines.LocalSelectAllCheckBox.Value;
                    selectPoLinesForm.Tables.SelectPoLinesTemp.Save();
                    tableError = selectPoLinesForm.Tables.SelectPoLinesTemp.ChangeNext();
                }
                selectPoLinesForm.Tables.SelectPoLinesTemp.Release();
                selectPoLinesForm.Tables.SelectPoLinesTemp.Close();
                selectPoLinesForm.Procedures.PoIndicatorUpdateToolFormScrollFill.Invoke();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In LocalSelectAllCheckBox_Change Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }


        void EstimatedShipmentCostInquiryOkButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentCostInquiryOkButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        void EstimateIdLookupSelectButtonMnemonic_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (lookupWindowType == Resources.STR_EstimatedCostEntry)
                {
                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateId.Value = estimateIdLookupForm.EstimateIdLookup.EstimateIdScroll.EstimateId.Value;
                    lookupWindowType = string.Empty;
                }
                if (lookupWindowType == Resources.STR_EstimaedCostInquiry)
                {
                    estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.EstimateId.Value = estimateIdLookupForm.EstimateIdLookup.EstimateIdScroll.EstimateId.Value;
                    lookupWindowType = string.Empty;
                }
                estimateIdLookupForm.EstimateIdLookup.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimateIdLookupSelectButtonMnemonic_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        void PoNumberLookupSelectButtonMnemonic_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                selectPoLinesForm.SelectPoLines.PoNumber.Value = poNumberLookupForm.PoNumberLookup.PoNumberScroll.PoNumber.Value;
                poNumberLookupForm.PoNumberLookup.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoNumberLookupSelectButtonMnemonic_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }



        void EstimateIdLookupCancelButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                estimateIdLookupForm.EstimateIdLookup.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimateIdLookupCancelButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        void PoNumberLookupCancelButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                poNumberLookupForm.PoNumberLookup.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PoNumberLookupCancelButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        void EstimatedCostInquiryEstimateId_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                //estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.RedisplayButton.RunValidate();
                FetchEstimatedCostDetails(Resources.STR_EstimaedCostInquiry);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentCostInquiryOkButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        void EstimatedCostInquiryRedisplayButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                FetchEstimatedCostDetails(Resources.STR_EstimaedCostInquiry);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedCostInquiryRedisplayButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        void selectPoLinesLookupButton1_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                PurchaseOrderRequest purchaseOrderRequest = new PurchaseOrderRequest();
                PurchaseShipmentEstimateDetails purchaseShipmentEstimateDetails = new PurchaseShipmentEstimateDetails();
                purchaseShipmentEstimateDetails.Warehouse = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationCode.Value.ToString().Trim();
                purchaseShipmentEstimateDetails.Vendor = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorId.Value.ToString().Trim();
                purchaseOrderRequest.PurchaseShipmentEstimateDetails = purchaseShipmentEstimateDetails;
                purchaseOrderRequest.CompanyID = Dynamics.Globals.CompanyId.Value;

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = client.PostAsJsonAsync("api/PurchaseOrder/GetPoNumber/", purchaseOrderRequest); // we need to refer the web.api service url here.

                    if (response.Result.IsSuccessStatusCode)
                    {
                        PurchaseOrderResponse purchaseOrderResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                        if (purchaseOrderResponse.Status == ResponseStatus.Error)
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In selectPoLinesLookupButton1_ClickAfterOriginal Method : " + purchaseOrderResponse.ErrorMessage.ToString());
                            MessageBox.Show(purchaseOrderResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                        }
                        else
                        {
                            if (poNumberLookupForm.PoNumberLookup.IsOpen)
                            {
                                poNumberLookupForm.PoNumberLookup.Close();
                            }
                            poNumberLookupForm.PoNumberLookup.Open();
                            poNumberLookupForm.Tables.PoNumberTemp.Close();
                            poNumberLookupForm.Tables.PoNumberTemp.Release();
                            foreach (var PoNumberLists in purchaseOrderResponse.PurchaseNumberLists)
                            {
                                poNumberLookupForm.Tables.PoNumberTemp.PoNumber.Value = PoNumberLists.PoNumber.ToString().Trim();
                                poNumberLookupForm.Tables.PoNumberTemp.LocationCode.Value = PoNumberLists.LocationCode.ToString().Trim();
                                poNumberLookupForm.Tables.PoNumberTemp.VendorId.Value = PoNumberLists.VendorId.ToString().Trim();
                                poNumberLookupForm.Tables.PoNumberTemp.Save();
                            }
                            poNumberLookupForm.Procedures.EstimateIdLookupFormScrollFill.Invoke();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error: Fetching Po Numbers", Resources.STR_MESSAGE_TITLE);
                    }

                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In selectPoLinesLookupButton1_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }


        }


        void SelectPoLinesPoNumber_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                Dynamics.Tables.PopPo.Release();
                Dynamics.Tables.PopPo.Close();
                Dynamics.Tables.PopPo.PoNumber.Value = selectPoLinesForm.SelectPoLines.PoNumber.Value.Trim().ToString();
                Dynamics.Tables.PopPo.Key = 1;
                TableError tablerror = Dynamics.Tables.PopPo.Get();
                if (tablerror == TableError.NotFound)
                {
                    if (selectPoLinesForm.SelectPoLines.PoNumber.Value.Trim().ToString() != string.Empty)
                    {
                        MessageBox.Show("Po number does not exists", Resources.STR_MESSAGE_TITLE);
                        selectPoLinesForm.SelectPoLines.LookupButton1.Focus();
                        selectPoLinesForm.SelectPoLines.ClearButton.RunValidate();
                    }
                }
                if (tablerror == TableError.NoError)
                {
                    PurchaseOrderRequest purchaseOrderRequest = new PurchaseOrderRequest();
                    PurchaseOrderInformation purchaseOrderInformation = new PurchaseOrderInformation();
                    purchaseOrderInformation.PoNumber = selectPoLinesForm.SelectPoLines.PoNumber.Value.ToString().Trim();

                    purchaseOrderRequest.PurchaseOrderInformation = purchaseOrderInformation;
                    purchaseOrderRequest.CompanyID = Dynamics.Globals.CompanyId.Value;

                    if (purchaseOrderInformation.PoNumber != null && purchaseOrderInformation.PoNumber != string.Empty)
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            var response = client.PostAsJsonAsync("api/PurchaseOrder/GetPOShipmentEstimateDetails/", purchaseOrderRequest); // we need to refer the web.api service url here.

                            if (response.Result.IsSuccessStatusCode)
                            {
                                PurchaseOrderResponse purchaseOrderResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                                if (purchaseOrderResponse.Status == ResponseStatus.Error)
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SelectPoLinesPoNumber_Change Method : " + purchaseOrderResponse.ErrorMessage.ToString());
                                    MessageBox.Show(purchaseOrderResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    InsertPoDetailsIntoTempTable(purchaseOrderResponse);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Error: Fetching Po Number", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                    else
                        MessageBox.Show("No data found for the PO Number", Resources.STR_MESSAGE_TITLE);
                }
                Dynamics.Tables.PopPo.Release();
                Dynamics.Tables.PopPo.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentCostEntryDeleteButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        private void InsertPoDetailsIntoTempTable(PurchaseOrderResponse purchaseOrderResponse)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ClearSelectPoLines();
                short shortInt;
                foreach (var selectedPoHdr in purchaseOrderResponse.SelectPoHeaderList)
                {
                    selectPoLinesForm.SelectPoLines.CpCurSchedShip.Value = selectedPoHdr.CurSchedShip;
                    selectPoLinesForm.SelectPoLines.CpCurSchedDel.Value = selectedPoHdr.CurSchedDel;
                    selectPoLinesForm.SelectPoLines.PromisedDate.Value = selectedPoHdr.CurAvailDate;
                    selectPoLinesForm.SelectPoLines.ActualShipDate.Value = selectedPoHdr.ActualShipDate;
                    selectPoLinesForm.SelectPoLines.PoAcknowledgementDate.Value = selectedPoHdr.AcknowledgementDate;
                    selectPoLinesForm.SelectPoLines.PoConfirmedDate.Value = selectedPoHdr.ConfirmedDate;
                    selectPoLinesForm.SelectPoLines.VendorId.Value = selectedPoHdr.VendorId.ToString().Trim();
                    selectPoLinesForm.SelectPoLines.LocationCode.Value = selectedPoHdr.Warehouse.ToString().Trim();
                    if (selectedPoHdr.HasCostVariance.ToString().Trim() == "Yes")
                        selectPoLinesForm.SelectPoLines.CpHasCostVariance.Value = true;
                    else
                        selectPoLinesForm.SelectPoLines.CpHasCostVariance.Value = false;
                }


                selectPoLinesForm.Tables.SelectPoLinesTemp.Close();
                selectPoLinesForm.Tables.SelectPoLinesTemp.Release();
                foreach (var selectedPoLines in purchaseOrderResponse.SelectPoLineList)
                {
                    selectPoLinesForm.Tables.SelectPoLinesTemp.PopNumber.Value = selectedPoLines.PoNumber.ToString().Trim();
                    selectPoLinesForm.Tables.SelectPoLinesTemp.Ord.Value = selectedPoLines.Ord;
                    short.TryParse(selectedPoLines.PoLineNumber.ToString().Trim(), out shortInt);
                    selectPoLinesForm.Tables.SelectPoLinesTemp.LineNumber.Value = shortInt;
                    selectPoLinesForm.Tables.SelectPoLinesTemp.PoLineStatusDescription.Value = selectedPoLines.POStatus.ToString().Trim();
                    selectPoLinesForm.Tables.SelectPoLinesTemp.PoNewStatus.Value = selectedPoLines.PoIndicatorStatus.ToString().Trim();
                    selectPoLinesForm.Tables.SelectPoLinesTemp.Uom.Value = selectedPoLines.UoM.ToString().Trim();
                    selectPoLinesForm.Tables.SelectPoLinesTemp.ItemNumber.Value = selectedPoLines.ItemNumber.ToString().Trim();
                    selectPoLinesForm.Tables.SelectPoLinesTemp.ItemDescription.Value = selectedPoLines.ItemDescription.ToString().Trim();
                    selectPoLinesForm.Tables.SelectPoLinesTemp.EstimatedNetWeight.Value = selectedPoLines.NetWeight;
                    selectPoLinesForm.Tables.SelectPoLinesTemp.Qty.Value = selectedPoLines.QtyOrdered;
                    selectPoLinesForm.Tables.SelectPoLinesTemp.QtyCanceled.Value = selectedPoLines.QtyCancelled;
                    selectPoLinesForm.Tables.SelectPoLinesTemp.QtyPrevShipped.Value = selectedPoLines.QtyPrevShipped;
                    selectPoLinesForm.Tables.SelectPoLinesTemp.QtyRemaining.Value = selectedPoLines.QtyRemaining;
                    selectPoLinesForm.Tables.SelectPoLinesTemp.Save();
                }
                selectPoLinesForm.Tables.SelectPoLinesTemp.Release();
                selectPoLinesForm.Tables.SelectPoLinesTemp.Close();

                selectPoLinesForm.Procedures.PoIndicatorUpdateToolFormScrollFill.Invoke();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertPoDetailsIntoTempTable Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        private void ClearSelectPoLinesTemp()
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                selectPoLinesForm.Tables.SelectPoLinesTemp.Close();
                selectPoLinesForm.Tables.SelectPoLinesTemp.Release();
                TableError tableRemove = selectPoLinesForm.Tables.SelectPoLinesTemp.ChangeFirst();
                while (tableRemove == TableError.NoError && tableRemove != TableError.EndOfTable)
                {
                    selectPoLinesForm.Tables.SelectPoLinesTemp.Remove();
                    tableRemove = selectPoLinesForm.Tables.SelectPoLinesTemp.ChangeNext();
                }
                selectPoLinesForm.Tables.SelectPoLinesTemp.Release();
                selectPoLinesForm.Tables.SelectPoLinesTemp.Close();

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearSelectPoLines Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        void EstimatedQtyShipped_LeaveAfterOriginal(object sender, EventArgs e) 
        {
            decimal totalWeight = 0;     
            TableError tableValue = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.GetFirst();
            while (tableValue == TableError.NoError && tableValue != TableError.EndOfTable)
            {
                totalWeight += estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.EstimatedNetWeight.Value;
                tableValue = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.GetNext();
            }

            estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
            estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();
            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.TotalNetWeight.Value = totalWeight;
        }

        void EstimatedQtyShipped_LeaveBeforeOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder(); 
            decimal totalWeight = 0;
            try
            {
                if (estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.QtyRemaining.Value
                    >= estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.EstimatedQtyShipped.Value)
                {
                    PurchaseOrderRequest purchaseOrderRequest = new PurchaseOrderRequest();
                    PurchaseShipmentEstimateDetails purchaseShipmentEstimateDetails = new PurchaseShipmentEstimateDetails();
                    purchaseShipmentEstimateDetails.ItemNumber = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.ItemNumber.Value.ToString().Trim();
                    purchaseShipmentEstimateDetails.EstimatedQtyShipped = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.EstimatedQtyShipped.Value;
                    purchaseOrderRequest.PurchaseShipmentEstimateDetails = purchaseShipmentEstimateDetails;
                    purchaseOrderRequest.CompanyID = Dynamics.Globals.CompanyId.Value;

                    if (purchaseShipmentEstimateDetails.EstimatedQtyShipped != 0 && purchaseShipmentEstimateDetails.EstimatedQtyShipped != null)
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            var response = client.PostAsJsonAsync("api/PurchaseOrder/GetShipmentQtyTotal/", purchaseOrderRequest); // we need to refer the web.api service url here.

                            if (response.Result.IsSuccessStatusCode)
                            {
                                PurchaseOrderResponse purchaseOrderResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                                if (purchaseOrderResponse.Status == ResponseStatus.Error)
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedQtyShipped_LeaveAfterOriginal Method : " + purchaseOrderResponse.ErrorMessage.ToString());
                                    MessageBox.Show(purchaseOrderResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    if (estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.EstimatedQtyShipped.Value != estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.EstimatedQtyShipped.Value)
                                    {
                                        IsEstimateLineHasRow = true;
                                    }
                                    estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();
                                    estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
                                    estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.PoNumber.Value = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.PoNumber.Value;
                                    estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Ord.Value = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.Ord.Value;
                                    TableError tableError = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Change();
                                    if (tableError == TableError.NoError)
                                    {
                                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.EstimatedQtyShipped.Value = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.EstimatedQtyShipped.Value;
                                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.EstimatedNetWeight.Value = purchaseOrderResponse.PurchaseShipmentEstimateDetails.EstimatedQtyNetWeight;
                                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.EstimatedNetWeight.Value = purchaseOrderResponse.PurchaseShipmentEstimateDetails.EstimatedQtyNetWeight;
                                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Save();
                                    }
                                    estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
                                    estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Error: Estimated shipemnt qty weight", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                    else
                    {
                        if (estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.EstimatedQtyShipped.Value != estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.EstimatedQtyShipped.Value)
                        {
                            IsEstimateLineHasRow = true;
                        }
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.PoNumber.Value = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.PoNumber.Value;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Ord.Value = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.Ord.Value;
                        TableError tableError = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Change();
                        if (tableError == TableError.NoError)
                        {
                            estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.EstimatedQtyShipped.Value = 0;
                            estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.EstimatedNetWeight.Value = 0;
                            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.EstimatedNetWeight.Value =0;
                            estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Save();
                        }
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Entered Qty exceeded the remaining qty", Resources.STR_MESSAGE_TITLE);
                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.EstimatedQtyShipped.Focus();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedQtyShipped_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }

        }

        void CancelButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                selectPoLinesForm.SelectPoLines.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CancelButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        void SelectCheckBox_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                selectPoLinesForm.Tables.SelectPoLinesTemp.Close();
                selectPoLinesForm.Tables.SelectPoLinesTemp.Release();
                TableError tableError = selectPoLinesForm.Tables.SelectPoLinesTemp.Change();
                if (tableError == TableError.NoError)
                {
                    if (selectPoLinesForm.Tables.SelectPoLinesTemp.SelectCheckBox.Value == false)
                        selectPoLinesForm.Tables.SelectPoLinesTemp.SelectCheckBox.Value = true;
                    else
                        selectPoLinesForm.Tables.SelectPoLinesTemp.SelectCheckBox.Value = false;
                    selectPoLinesForm.Tables.SelectPoLinesTemp.Save();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SelectCheckBox_Change Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        void SelectButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                List<PurchaseShipmentEstimateDetails> selectedPoLines = new List<PurchaseShipmentEstimateDetails>();
                selectPoLinesForm.Tables.SelectPoLinesTemp.Close();
                selectPoLinesForm.Tables.SelectPoLinesTemp.Release();
                TableError tableError = selectPoLinesForm.Tables.SelectPoLinesTemp.GetFirst();
                while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    if (selectPoLinesForm.Tables.SelectPoLinesTemp.SelectCheckBox.Value == true)
                    {
                        PurchaseShipmentEstimateDetails purchaseShipmentEstimateDetails = new PurchaseShipmentEstimateDetails();
                        purchaseShipmentEstimateDetails.PoNumber = selectPoLinesForm.Tables.SelectPoLinesTemp.PopNumber.Value.ToString().Trim();
                        purchaseShipmentEstimateDetails.PoOrdNumber = selectPoLinesForm.Tables.SelectPoLinesTemp.Ord.Value;
                        purchaseShipmentEstimateDetails.PoLineNumber = selectPoLinesForm.Tables.SelectPoLinesTemp.LineNumber.Value;
                        purchaseShipmentEstimateDetails.ItemNumber = selectPoLinesForm.Tables.SelectPoLinesTemp.ItemNumber.Value.ToString().Trim();
                        purchaseShipmentEstimateDetails.ItemDesc = selectPoLinesForm.Tables.SelectPoLinesTemp.ItemDescription.Value.ToString().Trim();
                        purchaseShipmentEstimateDetails.UoM = selectPoLinesForm.Tables.SelectPoLinesTemp.Uom.Value.ToString().Trim();
                        purchaseShipmentEstimateDetails.EstimatedQtyNetWeight = selectPoLinesForm.Tables.SelectPoLinesTemp.EstimatedNetWeight.Value;
                        purchaseShipmentEstimateDetails.QtyOrdered = selectPoLinesForm.Tables.SelectPoLinesTemp.Qty.Value;
                        purchaseShipmentEstimateDetails.QtyCancelled = selectPoLinesForm.Tables.SelectPoLinesTemp.QtyCanceled.Value;
                        purchaseShipmentEstimateDetails.QtyPrevShipped = selectPoLinesForm.Tables.SelectPoLinesTemp.QtyPrevShipped.Value;
                        purchaseShipmentEstimateDetails.QtyRemaining = selectPoLinesForm.Tables.SelectPoLinesTemp.QtyRemaining.Value;
                        selectedPoLines.Add(purchaseShipmentEstimateDetails);
                    }
                    tableError = selectPoLinesForm.Tables.SelectPoLinesTemp.GetNext();
                }
                selectPoLinesForm.Tables.SelectPoLinesTemp.Release();
                selectPoLinesForm.Tables.SelectPoLinesTemp.Close();

                if (selectedPoLines != null && selectedPoLines.Count > 0)
                {
                    AssignSelectedPoLines(selectedPoLines);
                    selectPoLinesForm.SelectPoLines.Close();
                    SumOfPoLinesTotalWeight();
                }
                else
                {
                    MessageBox.Show("No lines are selected. Please select", Resources.STR_MESSAGE_TITLE);
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SelectButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        private void SumOfPoLinesTotalWeight()
        {
            StringBuilder logMessage = new StringBuilder();
            decimal totalWeight = 0;
            try
            {
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
                TableError tableValue = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.GetFirst();
                while (tableValue == TableError.NoError && tableValue != TableError.EndOfTable)
                {
                    totalWeight += estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.EstimatedNetWeight.Value;
                    tableValue = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.GetNext();
                }

                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();
                estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.TotalNetWeight.Value = totalWeight;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SumOfPoLinesTotalWeight Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        private void AssignSelectedPoLines(List<PurchaseShipmentEstimateDetails> selectedPoLines)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
                foreach (var selectedPo in selectedPoLines)
                {
                    estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.PoNumber.Value = selectedPo.PoNumber.ToString().Trim();
                    estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Ord.Value = selectedPo.PoOrdNumber;
                    TableError tableError = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Change();

                    if (tableError == TableError.NotFound)
                    {
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.PoNumber.Value = selectedPo.PoNumber.ToString().Trim();
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Ord.Value = selectedPo.PoOrdNumber;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.PoLineNumber.Value = selectedPo.PoLineNumber;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.ItemNumber.Value = selectedPo.ItemNumber.ToString().Trim();
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.ItemDescription.Value = selectedPo.ItemDesc.ToString().Trim();
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Uom.Value = selectedPo.UoM.ToString().Trim();
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.EstimatedNetWeight.Value = 0;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Qty.Value = selectedPo.QtyOrdered;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyCanceled.Value = selectedPo.QtyCancelled;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyPrevShipped.Value = selectedPo.QtyPrevShipped;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.EstimatedQtyShipped.Value = 0;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyRemaining.Value = selectedPo.QtyRemaining;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Save();
                    }
                    if (tableError == TableError.NoError)
                    {
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.PoNumber.Value = selectedPo.PoNumber.ToString().Trim();
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Ord.Value = selectedPo.PoOrdNumber;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.PoLineNumber.Value = selectedPo.PoLineNumber;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.ItemNumber.Value = selectedPo.ItemNumber.ToString().Trim();
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.ItemDescription.Value = selectedPo.ItemDesc.ToString().Trim();
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Uom.Value = selectedPo.UoM.ToString().Trim();
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Qty.Value = selectedPo.QtyOrdered;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyCanceled.Value = selectedPo.QtyCancelled;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyPrevShipped.Value = selectedPo.QtyPrevShipped;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyRemaining.Value = selectedPo.QtyRemaining;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Save();
                    }
                }

                IsEstimateLineHasRow = true;

                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();
                estimatedShipmentCostEntryForm.Procedures.EstimatedShipmentCostEntryFill.Invoke();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In AssignSelectedPoLines Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }


        void EstimatedShipmentCostEntryAddPoLines_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                int? estimateId = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateId.Value;
                decimal? estimatedShipmentCost = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCost.Value;
                if (estimateId != 0)
                {
                    if (estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationCode.Value != string.Empty
                        && estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIdKey.Value != string.Empty
                        )
                    {
                        selectPoLinesForm.SelectPoLines.Open();
                    }
                    else
                        MessageBox.Show("Please fill all mandatory fields", Resources.STR_MESSAGE_TITLE);
                }
                else
                {
                    MessageBox.Show("Please select the Estimate Id", Resources.STR_MESSAGE_TITLE);
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentCostEntryAddPoLines_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        void EstimatedShipmentRedisplayButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            PurchaseOrderRequest purchaseRequest = null;
            bool recordsExists = false;
            StringBuilder logMessage = new StringBuilder();
            List<PurchaseShipmentEstimateDetails> ShipmentEstimateList = null;
            try
            {
                purchaseRequest = new PurchaseOrderRequest();
                PurchaseOrderEntity purchaseEntity = new PurchaseOrderEntity();
                AuditInformation auditInfo = new AuditInformation();
                ShipmentEstimateList = new List<PurchaseShipmentEstimateDetails>();

                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();

                TableError EstimatedShipmentCostEntry = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.GetFirst();
                while (EstimatedShipmentCostEntry == TableError.NoError && EstimatedShipmentCostEntry != TableError.EndOfTable)
                {
                    PurchaseShipmentEstimateDetails ShipmentEstimateDetails = new PurchaseShipmentEstimateDetails();
                    ShipmentEstimateDetails.PoNumber = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.PoNumber.Value.ToString().Trim();
                    ShipmentEstimateDetails.PoOrdNumber = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Ord.Value;
                    ShipmentEstimateDetails.PoLineNumber = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.PoLineNumber.Value;
                    ShipmentEstimateDetails.ItemNumber = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.ItemNumber.Value.ToString().Trim();
                    ShipmentEstimateDetails.UoM = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Uom.Value.ToString().Trim();
                    ShipmentEstimateDetails.QtyOrdered = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Qty.Value;
                    ShipmentEstimateDetails.QtyCancelled = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyCanceled.Value;
                    ShipmentEstimateDetails.QtyPrevShipped = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyPrevShipped.Value;
                    ShipmentEstimateDetails.QtyRemaining = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyRemaining.Value;
                    ShipmentEstimateDetails.EstimatedQtyShipped = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.EstimatedQtyShipped.Value;
                    ShipmentEstimateDetails.EstimatedQtyNetWeight = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.EstimatedNetWeight.Value;
                    ShipmentEstimateDetails.QtyVariance = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyVariance.Value;

                    ShipmentEstimateList.Add(ShipmentEstimateDetails);
                    ShipmentEstimateDetails = null;
                    EstimatedShipmentCostEntry = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.GetNext();
                }
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();


                purchaseRequest.PurchaseShipmentEstimateList = ShipmentEstimateList;
                auditInfo.CompanyId = Dynamics.Globals.CompanyId.Value;
                purchaseRequest.AuditInformation = auditInfo;

                if (estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateId.Value != 0
                    && estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationCode.Value != string.Empty
                    && estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCost.Value != 0
                    && estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateShipDate.Value != Convert.ToDateTime("1/1/1900 12:00:00 AM")
                    && estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIdKey.Value != string.Empty
                    )
                {

                    if (purchaseRequest.PurchaseShipmentEstimateList != null && purchaseRequest.PurchaseShipmentEstimateList.Count > 0)
                    {
                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/PurchaseOrder/FetchEstimatedShipmentDetails", purchaseRequest); // we need to refer the web.api service url here.
                            if (response.Result.IsSuccessStatusCode)
                            {
                                PurchaseOrderResponse purchaseOrderResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                                if (purchaseOrderResponse.Status == ResponseStatus.Error)
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentRedisplayButton_ClickAfterOriginal Method (EstimatedShipmentRedisplayButton_ClickAfterOriginal): " + purchaseOrderResponse.ErrorMessage.ToString());
                                    MessageBox.Show(purchaseOrderResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    ClearEstimatedShipmentCostEntry();
                                    foreach (var estimateCostLine in purchaseOrderResponse.PurchaseShipmentEstimateLineList)
                                    {
                                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.PoNumber.Value = estimateCostLine.PoNumber.ToString().Trim();
                                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Ord.Value = estimateCostLine.PoOrdNumber;
                                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.PoLineNumber.Value = estimateCostLine.PoLineNumber;
                                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.ItemNumber.Value = estimateCostLine.ItemNumber.ToString().Trim();
                                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.ItemDescription.Value = estimateCostLine.ItemDesc.ToString().Trim();
                                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Uom.Value = estimateCostLine.UoM.ToString().Trim();
                                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Qty.Value = estimateCostLine.QtyOrdered;
                                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyCanceled.Value = estimateCostLine.QtyCancelled;
                                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyPrevShipped.Value = estimateCostLine.QtyPrevShipped;
                                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyVariance.Value = estimateCostLine.QtyVariance;
                                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyRemaining.Value = estimateCostLine.QtyRemaining;
                                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.EstimatedQtyShipped.Value = estimateCostLine.EstimatedQtyShipped;
                                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.EstimatedNetWeight.Value = estimateCostLine.EstimatedQtyNetWeight;
                                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Save();
                                    }
                                    estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
                                    estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();

                                    estimatedShipmentCostEntryForm.Procedures.EstimatedShipmentCostEntryFill.Invoke();
                                }
                            }
                        }
                    }
                    else
                        MessageBox.Show("Unable to display. No record found in Line", Resources.STR_MESSAGE_TITLE);
                }
                else
                    MessageBox.Show("Please fill all mandatory fields", Resources.STR_MESSAGE_TITLE);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentRedisplayButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }


        void EstimatedShipmentCostEntrySaveButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            PurchaseOrderRequest purchaseRequest = null;
            bool recordsExists = false;
            StringBuilder logMessage = new StringBuilder();
            List<PurchaseShipmentEstimateDetails> ShipmentEstimateList = null;
            try
            {
                purchaseRequest = new PurchaseOrderRequest();
                PurchaseOrderEntity purchaseEntity = new PurchaseOrderEntity();
                AuditInformation auditInfo = new AuditInformation();
                ShipmentEstimateList = new List<PurchaseShipmentEstimateDetails>();

                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();

                TableError EstimatedShipmentCostEntry = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.GetFirst();
                while (EstimatedShipmentCostEntry == TableError.NoError && EstimatedShipmentCostEntry != TableError.EndOfTable)
                {
                    PurchaseShipmentEstimateDetails ShipmentEstimateDetails = new PurchaseShipmentEstimateDetails();
                    ShipmentEstimateDetails.PoNumber = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.PoNumber.Value.ToString().Trim();
                    ShipmentEstimateDetails.PoOrdNumber = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Ord.Value;
                    ShipmentEstimateDetails.PoLineNumber = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.PoLineNumber.Value;
                    ShipmentEstimateDetails.ItemNumber = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.ItemNumber.Value.ToString().Trim();
                    ShipmentEstimateDetails.UoM = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Uom.Value.ToString().Trim();
                    ShipmentEstimateDetails.QtyOrdered = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Qty.Value;
                    ShipmentEstimateDetails.QtyCancelled = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyCanceled.Value;
                    ShipmentEstimateDetails.QtyPrevShipped = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyPrevShipped.Value;
                    ShipmentEstimateDetails.QtyRemaining = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyRemaining.Value;
                    ShipmentEstimateDetails.EstimatedQtyShipped = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.EstimatedQtyShipped.Value;
                    ShipmentEstimateDetails.EstimatedQtyNetWeight = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.EstimatedNetWeight.Value;
                    ShipmentEstimateDetails.QtyVariance = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyVariance.Value;

                    ShipmentEstimateList.Add(ShipmentEstimateDetails);
                    ShipmentEstimateDetails = null;
                    EstimatedShipmentCostEntry = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.GetNext();
                }
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();

                PurchaseShipmentEstimateDetails ShipmentDetails = new PurchaseShipmentEstimateDetails();
                ShipmentDetails.EstimateID = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateId.Value;
                ShipmentDetails.Warehouse = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationCode.Value.ToString().Trim();
                ShipmentDetails.Vendor = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorId.Value.ToString().Trim();
                ShipmentDetails.CarrierReference = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CarrierReferenceNumber.Value.ToString().Trim();
                ShipmentDetails.EstimatedCost = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCost.Value;
                ShipmentDetails.EstimatedShipDate = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateShipDate.Value;
                ShipmentDetails.CurrencyId = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIdKey.Value;
                ShipmentDetails.TotalNetWeight = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.TotalNetWeight.Value;
                ShipmentDetails.EstimatedShipmentNotes = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentNotes.Value;
                ShipmentDetails.ExchangeRate = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.ExchangeRate.Value;
                ShipmentDetails.ExchangeExpirationDate = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.ExpirationDate.Value;
                ShipmentDetails.UserId = Dynamics.Globals.UserId.Value.ToString().Trim();

                purchaseRequest.PurchaseShipmentEstimateList = ShipmentEstimateList;
                purchaseRequest.PurchaseShipmentEstimateDetails = ShipmentDetails;
                auditInfo.CompanyId = Dynamics.Globals.CompanyId.Value;
                purchaseRequest.AuditInformation = auditInfo;

                if (purchaseRequest.PurchaseShipmentEstimateDetails.EstimateID != 0 && purchaseRequest.PurchaseShipmentEstimateDetails.Warehouse != string.Empty
                    && purchaseRequest.PurchaseShipmentEstimateDetails.EstimatedCost != 0
                    && purchaseRequest.PurchaseShipmentEstimateDetails.EstimatedShipDate != Convert.ToDateTime("1/1/1900 12:00:00 AM")
                    && purchaseRequest.PurchaseShipmentEstimateDetails.CurrencyId != string.Empty
                    )
                {

                    if (purchaseRequest.PurchaseShipmentEstimateList != null && purchaseRequest.PurchaseShipmentEstimateList.Count > 0)
                    {
                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/PurchaseOrder/SaveEstimatedShipmentCostEntry", purchaseRequest); // we need to refer the web.api service url here.
                            if (response.Result.IsSuccessStatusCode)
                            {
                                PurchaseOrderResponse purchaseOrderResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                                if (purchaseOrderResponse.Status == ResponseStatus.Error)
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SaveEstimatedShipmentCostEntry Method (EstimatedShipmentCostEntrySaveButton_ClickAfterOriginal): " + purchaseOrderResponse.ErrorMessage.ToString());
                                    MessageBox.Show(purchaseOrderResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateId.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationCode.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorId.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CarrierReferenceNumber.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIdKey.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCost.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateShipDate.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.TotalNetWeight.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentNotes.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationDescription.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorName.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.NewButton.Enable();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LookupButton1.Enable();
                                    ClearEstimatedShipmentCostEntry();
                                    estimatedShipmentCostEntryForm.Procedures.EstimatedShipmentCostEntryFill.Invoke();
                                    MessageBox.Show("Estimated details has been saved successfully", Resources.STR_MESSAGE_TITLE);
                                }
                            }
                        }
                    }
                    else
                        MessageBox.Show("Unable to save. No record found in Line", Resources.STR_MESSAGE_TITLE);
                }
                else
                    MessageBox.Show("Please fill all mandatory fields", Resources.STR_MESSAGE_TITLE);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentCostEntrySaveButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        private void ClearEstimatedShipmentCostEntry()
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
                TableError tableRemove = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.ChangeFirst();
                while (tableRemove == TableError.NoError && tableRemove != TableError.EndOfTable)
                {
                    estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Remove();
                    tableRemove = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.ChangeNext();
                }
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearEstimatedShipmentCostEntry Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        private void ClearSelectPoLines()
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                selectPoLinesForm.Tables.SelectPoLinesTemp.Close();
                selectPoLinesForm.Tables.SelectPoLinesTemp.Release();
                TableError tableRemove = selectPoLinesForm.Tables.SelectPoLinesTemp.ChangeFirst();
                while (tableRemove == TableError.NoError && tableRemove != TableError.EndOfTable)
                {
                    selectPoLinesForm.Tables.SelectPoLinesTemp.Remove();
                    tableRemove = selectPoLinesForm.Tables.SelectPoLinesTemp.ChangeNext();
                }
                selectPoLinesForm.Tables.SelectPoLinesTemp.Release();
                selectPoLinesForm.Tables.SelectPoLinesTemp.Close();

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearEstimatedShipmentCostEntry Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        private void ClearEstimatedShipmentCostInquiry()
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.Close();
                estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.Release();
                TableError tableRemove = estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.ChangeFirst();
                while (tableRemove == TableError.NoError && tableRemove != TableError.EndOfTable)
                {
                    estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.Remove();
                    tableRemove = estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.ChangeNext();
                }
                estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.Release();
                estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.Close();

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearEstimatedShipmentCostInquiry Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }
        void EstimatedShipmentCostEntryClearButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateId.Clear();
                estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationCode.Clear();
                estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorId.Clear();
                estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CarrierReferenceNumber.Clear();
                estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIdKey.Clear();
                estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCost.Clear();
                estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateShipDate.Clear();
                estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentNotes.Clear();
                estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.TotalNetWeight.Clear();
                estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationDescription.Clear();
                estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorName.Clear();
                estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LookupButton1.Enable();
                estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.NewButton.Enable();
                estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationCode.Enable();
                estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorId.Enable();
                estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIdKey.Enable();
                ClearEstimatedShipmentCostEntry();
                estimatedShipmentCostEntryForm.Procedures.EstimatedShipmentCostEntryFill.Invoke();
                IsEstimateLineHasRow = false;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentCostEntryClearButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }
        void EstimatedShipmentCostEntryDeleteButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                PurchaseOrderRequest purchaseOrderRequest = new PurchaseOrderRequest();
                PurchaseShipmentEstimateDetails purchaseShipmentEstimateDetails = new PurchaseShipmentEstimateDetails();
                purchaseShipmentEstimateDetails.EstimateID = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateId.Value;
                purchaseShipmentEstimateDetails.UserId = Dynamics.Globals.UserId.Value.ToString().Trim();
                purchaseOrderRequest.PurchaseShipmentEstimateDetails = purchaseShipmentEstimateDetails;
                purchaseOrderRequest.CompanyID = Dynamics.Globals.CompanyId.Value;
                int? estimateIdValue = purchaseOrderRequest.PurchaseShipmentEstimateDetails.EstimateID;

                if (estimateIdValue != 0)
                {
                    if (estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateId.Value != 0 && estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationCode.Value != string.Empty
                    && estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CarrierReferenceNumber.Value != string.Empty
                    && estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCost.Value != 0
                    && estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateShipDate.Value != Convert.ToDateTime("1/1/1900 12:00:00 AM")
                    && estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIdKey.Value != string.Empty
                    )
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            var response = client.PostAsJsonAsync("api/PurchaseOrder/DeleteEstimatedId/", purchaseOrderRequest); // we need to refer the web.api service url here.

                            if (response.Result.IsSuccessStatusCode)
                            {
                                PurchaseOrderResponse purchaseOrderResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                                if (purchaseOrderResponse.Status == ResponseStatus.Error)
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentCostEntryDeleteRowButton_ClickAfterOriginal Method : " + purchaseOrderResponse.ErrorMessage.ToString());
                                    MessageBox.Show(purchaseOrderResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Remove();

                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateId.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationCode.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorId.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CarrierReferenceNumber.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIdKey.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCost.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateShipDate.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentNotes.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.TotalNetWeight.Clear();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LookupButton1.Enable();
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.NewButton.Enable();
                                    ClearEstimatedShipmentCostEntry();
                                    estimatedShipmentCostEntryForm.Procedures.EstimatedShipmentCostEntryFill.Invoke();
                                    MessageBox.Show("Estimated details has been deleted successfully");
                                    IsEstimateLineHasRow = false;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Error: Deleting Estimate Id", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                    else
                        MessageBox.Show("There is no data to be delete", Resources.STR_MESSAGE_TITLE);
                }
                else
                    MessageBox.Show("Please select the Estimate Id", Resources.STR_MESSAGE_TITLE);

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentCostEntryDeleteButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }

        }
        void EstimatedShipmentCostEntryDeleteRowButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                PurchaseOrderRequest purchaseOrderRequest = new PurchaseOrderRequest();
                PurchaseOrderInformation purchaseOrderInformation = new PurchaseOrderInformation();
                PurchaseShipmentEstimateDetails purchaseShipmentEstimateDetails = new PurchaseShipmentEstimateDetails();

                purchaseOrderRequest.CompanyID = Dynamics.Globals.CompanyId.Value;
                EFTPayment payment = new EFTPayment();
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.PoNumber.Value = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.PoNumber.Value;
                estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Ord.Value = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCostScroll.Ord.Value;
                TableError error = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Change();
                if (error == TableError.NoError)
                {
                    if (MessageBox.Show("Are you sure you want to remove this line permanently?", Resources.STR_MESSAGE_TITLE, MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            purchaseOrderInformation.PoNumber = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.PoNumber.Value;
                            purchaseOrderInformation.PoLineNumber = estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.PoLineNumber.Value;
                            purchaseShipmentEstimateDetails.UserId = Dynamics.Globals.UserId.Value.ToString().Trim();
                            purchaseOrderRequest.PurchaseShipmentEstimateDetails = purchaseShipmentEstimateDetails;
                            purchaseOrderRequest.PurchaseOrderInformation = purchaseOrderInformation;
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            var response = client.PostAsJsonAsync("api/PurchaseOrder/DeleteEstimateLineDetails/", purchaseOrderRequest); // we need to refer the web.api service url here.

                            if (response.Result.IsSuccessStatusCode)
                            {
                                PurchaseOrderResponse purchaseOrderResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                                if (purchaseOrderResponse.Status == ResponseStatus.Error)
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentCostEntryDeleteRowButton_ClickAfterOriginal Method : " + purchaseOrderResponse.ErrorMessage.ToString());
                                    MessageBox.Show(purchaseOrderResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Remove();
                                    SumOfPoLinesTotalWeight();
                                    IsEstimateLineHasRow = true;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Error: Deleting PO Line", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                    estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
                    estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();
                }
                estimatedShipmentCostEntryForm.Procedures.EstimatedShipmentCostEntryFill.Invoke();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentCostEntryDeleteRowButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        void EstimatedShipmentCostEntryPrintButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
        void EstimatedShipmentCostEntryNewButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            PurchaseOrderRequest purchaseRequest = null;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                purchaseRequest = new PurchaseOrderRequest();
                AuditInformation auditInfo = new AuditInformation();
                PurchaseOrderEntity purchaseOrderEntity = new PurchaseOrderEntity();
                PurchaseShipmentEstimateDetails purchaseShipmentEstimateDetails = new PurchaseShipmentEstimateDetails();

                auditInfo.CompanyId = Dynamics.Globals.CompanyId.Value;
                purchaseRequest.AuditInformation = auditInfo;

                ClearEstimatedShipmentCostEntry();

                // Service call ...
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.PostAsJsonAsync("api/PurchaseOrder/GetNextEstimateId", purchaseRequest); // we need to refer the web.api service url here.
                    if (response.Result.IsSuccessStatusCode)
                    {
                        PurchaseOrderResponse purchaseOrderResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                        if (purchaseOrderResponse.Status == ResponseStatus.Error)
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentCostEntryNewButton_ClickAfterOriginal Method: " + purchaseOrderResponse.ErrorMessage.ToString());
                            MessageBox.Show(purchaseOrderResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                        }
                        else
                        {
                            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateId.Value = purchaseOrderResponse.PurchaseShipmentEstimateDetails.EstimateID;
                            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.NewButton.Disable();
                            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LookupButton1.Disable();
                            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationCode.Clear();
                            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorId.Clear();
                            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CarrierReferenceNumber.Clear();
                            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIdKey.Clear();
                            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCost.Clear();
                            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateShipDate.Clear();
                            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentNotes.Clear();
                            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.TotalNetWeight.Clear();
                            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationDescription.Clear();
                            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorName.Clear();
                            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationCode.Enable();
                            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorId.Enable();
                            estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIdKey.Enable();
                        }
                    }

                }
            }

            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In FetchNewEstimatedIdDetails Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        void EstimateId_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                FetchEstimatedCostDetails(Resources.STR_EstimatedCostEntry);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimateId_Change Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        private void FetchEstimatedCostDetails(string formName)
        {
            PurchaseOrderRequest purchaseRequest = null;
            StringBuilder logMessage = new StringBuilder();
            List<PurchaseShipmentEstimateDetails> ShipmentEstimateList = null;
            try
            {
                purchaseRequest = new PurchaseOrderRequest();
                AuditInformation auditInfo = new AuditInformation();
                PurchaseShipmentEstimateDetails purchaseShipmentEstimateDetails = new PurchaseShipmentEstimateDetails();
                if (formName == Resources.STR_EstimatedCostEntry)
                    purchaseShipmentEstimateDetails.EstimateID = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateId.Value;
                if (formName == Resources.STR_EstimaedCostInquiry)
                    purchaseShipmentEstimateDetails.EstimateID = estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.EstimateId.Value;
                purchaseRequest.PurchaseShipmentEstimateDetails = purchaseShipmentEstimateDetails;


                auditInfo.CompanyId = Dynamics.Globals.CompanyId.Value;
                purchaseRequest.AuditInformation = auditInfo;

                if (purchaseShipmentEstimateDetails.EstimateID != 0 && purchaseShipmentEstimateDetails.EstimateID != null)
                {
                    if (formName == Resources.STR_EstimatedCostEntry)
                    {
                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/PurchaseOrder/GetShipmentEstimateDetails", purchaseRequest); // we need to refer the web.api service url here.
                            if (response.Result.IsSuccessStatusCode)
                            {
                                PurchaseOrderResponse purchaseOrderResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                                if (purchaseOrderResponse.Status == ResponseStatus.Error)
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In FetchEstimatedCostDetails Method: " + purchaseOrderResponse.ErrorMessage.ToString());
                                    MessageBox.Show(purchaseOrderResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    InsertEstimateCostDetails(purchaseOrderResponse, formName);
                                }

                            }

                        }
                    }
                    else if (formName == Resources.STR_EstimaedCostInquiry)
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/PurchaseOrder/GetShipmentEstimateInquiryDetails", purchaseRequest); // we need to refer the web.api service url here.
                            if (response.Result.IsSuccessStatusCode)
                            {
                                PurchaseOrderResponse purchaseOrderResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                                if (purchaseOrderResponse.Status == ResponseStatus.Error)
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In GetShipmentEstimateInquiryDetails Method: " + purchaseOrderResponse.ErrorMessage.ToString());
                                    MessageBox.Show(purchaseOrderResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    InsertEstimateCostDetails(purchaseOrderResponse, formName);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In FetchEstimatedCostDetails Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        private void InsertEstimateCostDetails(PurchaseOrderResponse purchaseOrderResponse, string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EstimatedCostEntry)
                {
                    ClearEstimatedShipmentCostEntry();
                    foreach (var estimateCostHdr in purchaseOrderResponse.PurchaseShipmentEstimateHeaderList)
                    {
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationCode.Value = estimateCostHdr.Warehouse.ToString().Trim();
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationDescription.Value = estimateCostHdr.LocationDesc.ToString().Trim();
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorId.Value = estimateCostHdr.Vendor.ToString().Trim();
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorName.Value = estimateCostHdr.VendorName.ToString().Trim();
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CarrierReferenceNumber.Value = estimateCostHdr.CarrierReference.ToString().Trim();
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIdKey.Value = estimateCostHdr.CurrencyId.ToString().Trim();
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentCost.Value = estimateCostHdr.EstimatedShipmentCost;
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateShipDate.Value = estimateCostHdr.EstimatedShipDate;
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.TotalNetWeight.Value = estimateCostHdr.TotalNetWeight;
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimatedShipmentNotes.Value = estimateCostHdr.EstimatedShipmentNotes.ToString().Trim();
                    }

                    estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();
                    estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
                    foreach (var estimateCostLine in purchaseOrderResponse.PurchaseShipmentEstimateLineList)
                    {
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.PoNumber.Value = estimateCostLine.PoNumber.ToString().Trim();
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Ord.Value = estimateCostLine.PoOrdNumber;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.PoLineNumber.Value = estimateCostLine.PoLineNumber;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.ItemNumber.Value = estimateCostLine.ItemNumber.ToString().Trim();
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.ItemDescription.Value = estimateCostLine.ItemDesc.ToString().Trim();
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Uom.Value = estimateCostLine.UoM.ToString().Trim();
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Qty.Value = estimateCostLine.QtyOrdered;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyCanceled.Value = estimateCostLine.QtyCancelled;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyPrevShipped.Value = estimateCostLine.QtyPrevShipped;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.QtyRemaining.Value = estimateCostLine.QtyRemaining;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.EstimatedQtyShipped.Value = estimateCostLine.EstimatedQtyShipped;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.EstimatedNetWeight.Value = estimateCostLine.EstimatedQtyNetWeight;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.IsLineMatched.Value = estimateCostLine.IsLineMatched;
                        estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Save();
                    }
                    estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Release();
                    estimatedShipmentCostEntryForm.Tables.EstimateShipmentEntTemp.Close();

                    estimatedShipmentCostEntryForm.Procedures.EstimatedShipmentCostEntryFill.Invoke();
                    SumOfPoLinesTotalWeight();
                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationCode.Disable();
                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorId.Disable();
                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIdKey.Disable();
                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LookupButton1.Disable();
                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.NewButton.Disable();
                }
                else if (formName == Resources.STR_EstimaedCostInquiry)
                {
                    ClearEstimatedShipmentCostInquiry();
                    short shorValue;
                    foreach (var estimateCostHdr in purchaseOrderResponse.PurchaseShipmentEstimateHeaderList)
                    {
                        estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.LocationCode.Value = estimateCostHdr.Warehouse.ToString().Trim();
                        estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.LocationDescription.Value = estimateCostHdr.LocationDesc.ToString().Trim();
                        estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.VendorId.Value = estimateCostHdr.Vendor.ToString().Trim();
                        estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.VendorName.Value = estimateCostHdr.VendorName.ToString().Trim();
                        estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.IsMatchedToReceipt.Value = estimateCostHdr.IsMatchedToReceipt.ToString().Trim();
                        estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.CarrierReferenceNumber.Value = estimateCostHdr.CarrierReference.ToString().Trim();
                        estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.CurrencyIdKey.Value = estimateCostHdr.CurrencyId.ToString().Trim();
                        estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.EstimatedShipmentCost.Value = estimateCostHdr.EstimatedShipmentCost;
                        estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.EstimateShipDate.Value = estimateCostHdr.EstimatedShipDate;
                        estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.TotalNetWeight.Value = estimateCostHdr.TotalNetWeight;
                        estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.EstimatedShipmentNotes.Value = estimateCostHdr.EstimatedShipmentNotes.ToString().Trim();
                    }

                    estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.Close();
                    estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.Release();
                    foreach (var estimateCostLine in purchaseOrderResponse.PurchaseShipmentEstimateLineList)
                    {
                        estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.PoNumber.Value = estimateCostLine.PoNumber.ToString().Trim();
                        estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.Ord.Value = estimateCostLine.PoOrdNumber;
                        estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.PoLineNumber.Value = estimateCostLine.PoLineNumber;
                        estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.ItemNumber.Value = estimateCostLine.ItemNumber.ToString().Trim();
                        estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.Uom.Value = estimateCostLine.UoM.ToString().Trim();
                        estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.Qty.Value = estimateCostLine.QtyOrdered;
                        estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.QtyCanceled.Value = estimateCostLine.QtyCancelled;
                        estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.QtyPrevShipped.Value = estimateCostLine.QtyPrevShipped;
                        estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.QtyVariance.Value = estimateCostLine.QtyVariance;
                        estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.QtyRemaining.Value = estimateCostLine.QtyRemaining;
                        estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.EstimatedQtyShipped.Value = estimateCostLine.EstimatedQtyShipped;
                        estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.EstimatedNetWeight.Value = estimateCostLine.EstimatedQtyNetWeight;
                        estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.EstimatedShipmentCost.Value = estimateCostLine.EstimatedShipmentCost;
                        estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.ReceiptLineNumber.Value = estimateCostLine.ReceiptLineNumber;
                        estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.PoReceipt.Value = estimateCostLine.POReceipt.ToString().Trim();
                        short.TryParse(estimateCostLine.ShipmentEstimateMatchType.ToString().Trim(), out shorValue);
                        estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.ShipEstimateMatchType.Value = shorValue;
                        estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.IsLineMatched.Value = estimateCostLine.IsLineMatched;
                        if (estimateCostLine.IsLineMatched == true)
                            estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.IsLineMatchedStr.Value = "Yes";
                        else
                            estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.IsLineMatchedStr.Value = "No";
                        estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.Save();
                    }
                    estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.Release();
                    estimatedShipmentCostInquiryForm.Tables.EstimatedShipmentTemp.Close();

                    estimatedShipmentCostInquiryForm.Procedures.EstimatedShipmentCostInquiryFill.Invoke();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertEstimateCostDetails Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        void EstimatedShipmentCostInquiryLookupButton1_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                EstimateIdLookup(Resources.STR_EstimaedCostInquiry);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentCostInquiryLookupButton1_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        void EstimatedShipmentCostEntryLookupButton1_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                EstimateIdLookup(Resources.STR_EstimatedCostEntry);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentCostEntryLookupButton1_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }

        }

        private void EstimateIdLookup(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                PurchaseOrderRequest purchaseOrderRequest = new PurchaseOrderRequest();
                PurchaseShipmentEstimateDetails purchaseShipmentEstimateDetails = new PurchaseShipmentEstimateDetails();
                if (formName == Resources.STR_EstimatedCostEntry)
                {
                    purchaseShipmentEstimateDetails.EstimateID = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateId.Value;
                    lookupWindowType = Resources.STR_EstimatedCostEntry;
                    purchaseShipmentEstimateDetails.WindowValue = 1;
                }
                if (formName == Resources.STR_EstimaedCostInquiry)
                {
                    purchaseShipmentEstimateDetails.EstimateID = estimatedShipmentCostInquiryForm.EstimatedShipmentCostInquiry.EstimateId.Value;
                    lookupWindowType = Resources.STR_EstimaedCostInquiry;
                    purchaseShipmentEstimateDetails.WindowValue = 2;
                }
                purchaseOrderRequest.PurchaseShipmentEstimateDetails = purchaseShipmentEstimateDetails;
                purchaseOrderRequest.CompanyID = Dynamics.Globals.CompanyId.Value;

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = client.PostAsJsonAsync("api/PurchaseOrder/GetEstimateId/", purchaseOrderRequest); // we need to refer the web.api service url here.

                    if (response.Result.IsSuccessStatusCode)
                    {
                        PurchaseOrderResponse purchaseOrderResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                        if (purchaseOrderResponse.Status == ResponseStatus.Error)
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentCostEntryLookupButton1_ClickAfterOriginal Method : " + purchaseOrderResponse.ErrorMessage.ToString());
                            MessageBox.Show(purchaseOrderResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                        }
                        else
                        {
                            if (estimateIdLookupForm.EstimateIdLookup.IsOpen)
                            {
                                estimateIdLookupForm.EstimateIdLookup.Close();
                            }
                            estimateIdLookupForm.EstimateIdLookup.Open();
                            estimateIdLookupForm.Tables.EstimateIdTemp.Close();
                            estimateIdLookupForm.Tables.EstimateIdTemp.Release();
                            foreach (var EstimatedId in purchaseOrderResponse.PurchaseEstimatedId)
                            {
                                estimateIdLookupForm.Tables.EstimateIdTemp.EstimateId.Value = EstimatedId.EstimateID;
                                estimateIdLookupForm.Tables.EstimateIdTemp.Save();
                            }
                            estimateIdLookupForm.Procedures.EstimateIdLookupFormScrollFill.Invoke();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error: Leaving Estimate Id", Resources.STR_MESSAGE_TITLE);
                    }

                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimateIdLookup Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }
        void EstimatedShipmentCostEntryLookupButton2_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (ivLocationLookupForm.IvLocationLookup.IsOpen)
                {
                    ivLocationLookupForm.IvLocationLookup.Close();
                }

                ivLocationLookupForm.IvLocationLookup.Open();
                ivLocationLookupForm.IvLocationLookup.RedisplayButton.RunValidate();
                lookupWindowType = Resources.STR_LocationLookup;
                if (RegisterLocationLookupSelect == false)
                {
                    // event which calls before the lookup window got closed.
                    ivLocationLookupForm.IvLocationLookup.SelectButton.ClickAfterOriginal += new EventHandler(SelectButton_ClickBeforeOriginal);
                    RegisterLocationLookupSelect = true;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentCostEntryLookupButton2_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }
        void EstimatedShipmentCostEntryLookupButton3_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (vendorLookupForm.VendorLookup.IsOpen)
                {
                    vendorLookupForm.VendorLookup.Close();
                }

                vendorLookupForm.VendorLookup.Open();
                vendorLookupForm.VendorLookup.VendorSortBy.Value = 1;
                vendorLookupForm.VendorLookup.RedisplayButton.RunValidate();
                vendorLookupForm.VendorLookup.LocalCalledBy.Value = 1;
                lookupWindowType = Resources.STR_VendorLookup;
                if (RegisterVendorLookupSelect == false)
                {
                    // event which calls before the lookup window got closed.
                    vendorLookupForm.VendorLookup.SelectButton.ClickAfterOriginal += new EventHandler(SelectButton_ClickBeforeOriginal);
                    RegisterVendorLookupSelect = true;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentCostEntryLookupButton3_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }
        void EstimatedShipmentCostEntryLookupButton4_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (currencyLookupForm.CurrencyLookup.IsOpen)
                {
                    currencyLookupForm.CurrencyLookup.Close();
                }

                currencyLookupForm.CurrencyLookup.Open();
                currencyLookupForm.CurrencyLookup.RedisplayButton.RunValidate();
                lookupWindowType = Resources.STR_CurrencyLookup;
                if (RegisterCurrencyLookupSelect == false)
                {
                    // event which calls before the lookup window got closed.
                    currencyLookupForm.CurrencyLookup.SelectButton.ClickAfterOriginal += new EventHandler(SelectButton_ClickBeforeOriginal);
                    RegisterCurrencyLookupSelect = true;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentCostEntryLookupButton4_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }

        }

        void SelectButton_ClickBeforeOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.IsOpen && !String.IsNullOrEmpty(lookupWindowType))
                {
                    if (lookupWindowType == Resources.STR_CurrencyLookup)
                    {
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIdKey.Value = currencyLookupForm.CurrencyLookup.CurrencyLookupScroll.CurrencyId.Value;
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CurrencyIdKey.Focus();
                    }
                    else if (lookupWindowType == Resources.STR_LocationLookup)
                    {
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationCode.Value = ivLocationLookupForm.IvLocationLookup.LocationLookupScroll.LocationCode.Value;
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.LocationCode.Focus();
                    }
                    else if (lookupWindowType == Resources.STR_VendorLookup)
                    {
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorId.Value = vendorLookupForm.VendorLookup.VendorLookupScroll.VendorId.Value;
                        estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.VendorId.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SelectButton_ClickBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }

        void EstimatedShipmentCarrierReference_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                PurchaseOrderRequest purchaseOrderRequest = new PurchaseOrderRequest();
                PurchaseShipmentEstimateDetails purchaseShipmentEstimateDetails = new PurchaseShipmentEstimateDetails();
                purchaseShipmentEstimateDetails.EstimateID = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.EstimateId.Value;
                purchaseShipmentEstimateDetails.CarrierReference = estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CarrierReferenceNumber.Value.ToString().Trim();
                purchaseOrderRequest.PurchaseShipmentEstimateDetails = purchaseShipmentEstimateDetails;
                purchaseOrderRequest.CompanyID = Dynamics.Globals.CompanyId.Value;

                if (estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CarrierReferenceNumber.Value.ToString().Trim() != null
                    && estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CarrierReferenceNumber.Value.ToString().Trim() != string.Empty)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = client.PostAsJsonAsync("api/PurchaseOrder/ValidateCarrierReference/", purchaseOrderRequest); // we need to refer the web.api service url here.

                        if (response.Result.IsSuccessStatusCode)
                        {
                            PurchaseOrderResponse purchaseOrderResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                            if (purchaseOrderResponse.Status == ResponseStatus.Error)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentCarrierReference_LeaveAfterOriginal Method : " + purchaseOrderResponse.ErrorMessage.ToString());
                                MessageBox.Show(purchaseOrderResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                if (purchaseOrderResponse.PurchaseShipmentEstimateDetails.IsAvailable == 1)
                                {
                                    MessageBox.Show("Entered Carrier Reference is already exists", Resources.STR_MESSAGE_TITLE);
                                    estimatedShipmentCostEntryForm.EstimatedShipmentCostEntry.CarrierReferenceNumber.Clear();
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Error: Leaving Carrier Reference Number", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
                else
                    MessageBox.Show("Please fill carrier reference number", Resources.STR_MESSAGE_TITLE);

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EstimatedShipmentCarrierReferenceNumber_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "LandedCostDetails");
                logMessage = null;
            }
        }


        /***Venture: Landed Cost -Start***/
        #endregion LandedCost       

        #region Elemica

        void WindowPrint_ClickBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                vendorId = Dynamics.Forms.PopPoEntry.PopPoEntry.VendorId.Value.ToString().Trim();
                PoNumberForElemica = Dynamics.Forms.PopPoEntry.PopPoEntry.PoNumber.Value.ToString().Trim();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In WindowPrint_ClickBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ElemicaDetails");
                logMessage = null;
            }
        }

        void WindowPrint_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                FetchElemicaDetail();

                UpdatePopStatusForElemica();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In WindowPrint_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ElemicaDetails");
                logMessage = null;
            }
        }

        private void FetchElemicaDetail()
        {
            StringBuilder logMessage = new StringBuilder();
            PurchaseElemicaRequest poElemicaRequest = null;
            PurchaseOrderEntity purchaseOrderEntity = null;
            PurchaseOrderInformation purchaseOrderInformation = null;
            try
            {
                if (PoNumberForElemica != "")
                {
                    poElemicaRequest = new PurchaseElemicaRequest();
                    purchaseOrderEntity = new PurchaseOrderEntity();
                    purchaseOrderInformation = new PurchaseOrderInformation();
                    poElemicaStausForm = new ElemicaStausForm();

                    purchaseOrderInformation.PoNumber = PoNumberForElemica.ToString().Trim();
                    purchaseOrderInformation.VendorId = vendorId.ToString().Trim();
                    purchaseOrderEntity.PurchaseOrderDetails = purchaseOrderInformation;
                    poElemicaRequest.purchaseEntityDetails = purchaseOrderEntity;
                    poElemicaRequest.companyId = Dynamics.Globals.CompanyId.Value;

                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = client.PostAsJsonAsync("api/PurchaseOrder/RetrieveElemicaDetail/", poElemicaRequest); // we need to refer the web.api service url here.

                        if (response.Result.IsSuccessStatusCode)
                        {
                            PurchaseElemicaResponse poElemicaResponse = response.Result.Content.ReadAsAsync<PurchaseElemicaResponse>().Result;
                            if (poElemicaResponse.Status == ResponseStatus.Error)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In FetchElemicaDetail Method : " + poElemicaResponse.ErrorMessage.ToString());
                                MessageBox.Show(poElemicaResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                if (poElemicaResponse.purchaseOrderDetails.PurchaseOrderDetails.PoElemicaDetails.ItemList == string.Empty)
                                {
                                    if (poElemicaResponse.purchaseOrderDetails.PurchaseOrderDetails.PoElemicaDetails.IsValidToSendElemica == 1)
                                    {
                                        //dialog box --start

                                        poElemicaStausForm.SetDesktopLocation(400, 300);
                                        poElemicaStausForm.Show();
                                        poElemicaStausForm.Activate();
                                        poElemicaStausForm.SetProgessBarStatus(1);
                                        poElemicaStausForm.SetProgessBarStatus(2);
                                        poElemicaStausForm.SetProgessBarStatus(4);
                                        poElemicaStausForm.SetProgressWindowLabelText("Retrieving purchase order information from elemica.");
                                        poElemicaStausForm.SetProgessBarStatus(1);
                                        poElemicaStausForm.SetProgressWindowLabelText("Choose print options from the dialog box");
                                        poElemicaStausForm.Visible = false;
                                        DialogResult userInput = MessageBox.Show("Do you wish to send this purchase order to elemica? \n\n Times Sent\t:" + poElemicaResponse.purchaseOrderDetails.PurchaseOrderDetails.PoElemicaDetails.LastSentTimeCount.ToString()
                                                    + "\n Last Sent\t: " + poElemicaResponse.purchaseOrderDetails.PurchaseOrderDetails.PoElemicaDetails.LastSentDate.ToShortDateString() + "\n", "Microsoft Dynamics GP", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                                        poElemicaStausForm.Visible = true;
                                        poElemicaStausForm.SetProgessBarStatus(7);

                                        #region Elemica Opening_Dialog_box
                                        if (userInput == DialogResult.Yes)
                                        {
                                            poElemicaStausForm.SetProgressWindowLabelText("Sending to elemica");
                                            SendElemicaDetail();
                                            poElemicaStausForm.SetProgessBarStatus(8);
                                        }
                                        #endregion Elemica Opening_Dialog_box
                                    }
                                    else
                                    {
                                        poElemicaStausForm.SetProgressWindowLabelText("PO is not valid to sent to elemica");
                                    }
                                    poElemicaStausForm.SetProgessBarStatus(10);

                                    if (poElemicaStausForm != null)
                                    {
                                        // closing POElemica status form
                                        poElemicaStausForm.Close();
                                        poElemicaStausForm.Dispose();
                                    }
                                }
                                else
                                {
                                    DialogResult userInput1 = MessageBox.Show("This PO contains Dow Laminating products which are handled at both Dow and Rohm & Haas facilities. Please review and add these " + poElemicaResponse.purchaseOrderDetails.PurchaseOrderDetails.PoElemicaDetails.ItemList + " to list or split and reprint to send PO to supplier.", "Microsoft Dynamics GP", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In FetchElemicaDetail Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ElemicaDetails");
                logMessage = null;
            }
        }

        private void SendElemicaDetail()
        {
            StringBuilder logMessage = new StringBuilder();
            PurchaseElemicaRequest poElemicaRequest = null;
            PurchaseOrderEntity purchaseOrderEntity = null;
            PurchaseOrderInformation purchaseOrderInformation = null;
            try
            {
                if (PoNumberForElemica != "")
                {
                    poElemicaRequest = new PurchaseElemicaRequest();
                    purchaseOrderEntity = new PurchaseOrderEntity();
                    purchaseOrderInformation = new PurchaseOrderInformation();

                    purchaseOrderInformation.PoNumber = PoNumberForElemica.ToString().Trim();
                    purchaseOrderEntity.PurchaseOrderDetails = purchaseOrderInformation;
                    poElemicaRequest.purchaseEntityDetails = purchaseOrderEntity;
                    poElemicaRequest.companyId = Dynamics.Globals.CompanyId.Value;

                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = client.PostAsJsonAsync("api/PurchaseOrder/SendElemicaDetail/", poElemicaRequest); // we need to refer the web.api service url here.

                        if (response.Result.IsSuccessStatusCode)
                        {
                            PurchaseElemicaResponse poElemicaResponse = response.Result.Content.ReadAsAsync<PurchaseElemicaResponse>().Result;
                            if (poElemicaResponse.Status == ResponseStatus.Error)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SendElemicaDetail Method : " + poElemicaResponse.ErrorMessage.ToString());
                                MessageBox.Show(poElemicaResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Error: Send Elemica Detail", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SendElemicaDetail Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ElemicaDetails");
                logMessage = null;
            }
        }

        private void UpdatePopStatusForElemica()
        {
            StringBuilder logMessage = new StringBuilder();
            PurchaseElemicaRequest poElemicaRequest = null;
            PurchaseOrderEntity purchaseOrderEntity = null;
            PurchaseOrderInformation purchaseOrderInformation = null;
            try
            {
                if (PoNumberForElemica != "")
                {
                    poElemicaRequest = new PurchaseElemicaRequest();
                    purchaseOrderEntity = new PurchaseOrderEntity();
                    purchaseOrderInformation = new PurchaseOrderInformation();

                    purchaseOrderInformation.PoNumber = PoNumberForElemica.ToString().Trim();
                    purchaseOrderEntity.PurchaseOrderDetails = purchaseOrderInformation;
                    poElemicaRequest.purchaseEntityDetails = purchaseOrderEntity;
                    poElemicaRequest.companyId = Dynamics.Globals.CompanyId.Value;

                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = client.PostAsJsonAsync("api/PurchaseOrder/UpdatePOStatusForElemica/", poElemicaRequest); // we need to refer the web.api service url here.

                        if (response.Result.IsSuccessStatusCode)
                        {
                            PurchaseElemicaResponse poElemicaResponse = response.Result.Content.ReadAsAsync<PurchaseElemicaResponse>().Result;
                            if (poElemicaResponse.Status == ResponseStatus.Error)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In UpdatePopStatusForElemica Method : " + poElemicaResponse.ErrorMessage.ToString());
                                MessageBox.Show(poElemicaResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Error: Update Pop Status For Elemica", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In UpdatePopStatusForElemica Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message, PO.Properties.Resources.STR_DynamicsGP);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ElemicaDetails");
                logMessage = null;
            }
        }


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

                case "PurchaseDetails":

                    if (isPoLoggingEnabled && message != "")

                        new TextLogger().LogInformationIntoFile(message, poLogFilePath, poLogFileName);

                    break;

                default:

                    break;

            }

        }

    }

}



