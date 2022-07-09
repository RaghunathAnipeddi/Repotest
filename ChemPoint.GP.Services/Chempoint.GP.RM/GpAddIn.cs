using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Dexterity.Bridge;
using Microsoft.Dexterity.Applications;
using Dic4745 = Microsoft.Dexterity.Applications.ChemPointSalesExtDictionary;
using System.Windows.Forms;
using Chempoint.GP.RM.Properties;
using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.Entities.Business_Entities;
using ChemPoint.GP.Entities.BaseEntities;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Runtime.InteropServices;
using Chempoint.GP.Infrastructure.Logging;
using Microsoft.Dexterity.Applications.DynamicsDictionary;
using System.Text.RegularExpressions;

namespace Chempoint.GP.RM
{
    /// <CashApplication>
    /// Project Name        :   Cash Applciation 
    /// Affected Module     :   Finance
    /// Affected Windows    :   Cash Receipt Entry & inquiry
    ///                         Receiavable Transaction Entry and Inquiry
    /// Developed on        :   2016Aug22  
    /// Developed by        :   Muthu and Amit.
    ///-------------------------------------------------------------------------------------------
    ///     Date		Author          Description
    ///-------------------------------------------------------------------------------------------
    /// **26Jan2017     Mthangaraj      EFT Enahncement(Delete and Map functionality included in Bank Entry window),Email Remittance Changes, and Cash App changes(Included Functional and Originating Field in custom Table)
    /// **8Mar2017      Mthangaraj      Included Sorting Functionality on Bank and Payment Remittance Entry.
    /// **Apr2017       Mthangaraj      Dll Upgrade Regarding GP 2016
    /// </CashApplication>
    public class GPAddIn : IDexterityAddIn
    {
        // IDexterityAddIn interface

        private static Dic4745.RmSalesEntryForm rmSalesEntryForm;
        private static Dic4745.RmSalesInquiryForm rmSalesInquiryForm;
        private static Dic4745.CpReceivablesTransactionDetailsForm cpReceivablesTransactionDetailsForm;
        private static Dic4745.CpReceivablesTransactionInquiryForm cpReceivablesTransactionInquiryForm;
        private static Dic4745.CashApplicationProcessEntryForm cashApplicationProcessForm;
        private static Dic4745.CashApplicationProcessInquiryForm cashApplicationProcessInquiryForm;
        private static RmCashReceiptsForm rmCashReceiptsForm;
        private static RmCashInquiryForm rmCashReceiptInquiryForm;
        private static RmCashApplyForm rmCashApplyForm;
        private static Dic4745.PalbTransactionsForm lockBoxTransactionsForm;
        private static Dic4745.EftCustomerMappingForm eftCustomerMappingForm;
        static Dic4745.BankCtxRemittanceImportForm bankCtxRemittanceImportForm;
        static Dic4745.BankCtxSummaryImportForm bankCtxSummaryImportForm;
        private static CustomerLookupForm customerLookupForm;

        #region EFT
        private static Dic4745.EftCustomerBankEntryForm eftCustomerRemittancesEntryForm;
        private static Dic4745.EftBatchIdLookupForm eftBatchIdLookupForm;
        private static Dic4745.EftCustomerIdLookupForm eftCustomerIdLookupForm;
        private static Dic4745.EftDocumentNumberLookupForm eftDocumentNumberLookupForm;
        private static Dic4745.EftReferenceNumberLookupForm eftReferenceNumberLookupForm;
        private static Dic4745.EftCustomerBankInquiryForm eftCustomerRemittancesInquiryForm;
        private static Dic4745.RemittancePaymentEntryForm eftRemittancePaymentEntryForm;
        private static Dic4745.EmailRemittanceInquiryForm eftEmailRemittanceInquiryForm;
        //New EFT EMAIl Remittance Window 
        private static Dic4745.EftEmailRemittanceEntryForm eftEmailRemittanceForm;
        private static Dic4745.EftEmailReferenceLookupForm eftEmailReferenceLookupForm;

        string lookupWindowName = string.Empty;
        string referenceLookupName = string.Empty;
        string documentLookupName = string.Empty;
        Boolean RegisterCidLookupSelect = false;
        Boolean RegisterRefidLookupSelect = false;
        Boolean RegisterDocidLookupSelect = false;
        Boolean RegisterBatchidLookupSelect = false;
        bool isValidatedSucess = true;
        int countValue = 0;
        int eftRowId = 1;
        int eftNextCustomerMappingId = 0;
        int customerBankEntryNextRowId = 0;
        bool isEftReferenceNumAvailble = false;
        int itemRefCount = 1;
        int rowIdForClear = 0;
        int scrollTempEFTAppid = 0;

        #endregion EFT

        Boolean RegisterRmDetailsMgmt = false;
        Boolean RegisterRmDetailsInquiryMgmt = false;
        Boolean RegisterCashApplication = false;
        Boolean RegisterCashApplicationInquiry = false;
        string expansionWindowType = string.Empty;
        string expansionWindowInquiryType = string.Empty;
        string originalCurrencyID = string.Empty;
        string remittanceCustomerLookup = string.Empty;
        string remittanceCustomerLookupValue = string.Empty;

        bool isRejectCheckBox = false;
        bool isCsvFileCheck = false;
        bool RegisterBankCtxSummaryImport = false;
        bool RegisterBankCtxRemittanceImport = false;

        int originalTypeIdEntry = 0;
        int originalTypeIdInquiry = 0;
        string customer = null;

        [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileStringW",
            SetLastError = true,
            CharSet = CharSet.Unicode, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, string lpReturnString, int nSize, string lpFilename);

        static string gpServiceConfigurationURL = null;
        static string EFTSummaryBackupPath = null;
        static string EFTRemittanceBackupPath = null;
        bool isRmLoggingEnabled = false;
        bool isCashAppEnabled = false;
        string rmLogFileName = null;
        string rmLogFilePath = null;
        string cashLogFileName = null;
        string cashLogFilePath = null;

        public void Initialize()
        {

            rmSalesEntryForm = ChemPointSalesExt.Forms.RmSalesEntry;
            rmSalesInquiryForm = ChemPointSalesExt.Forms.RmSalesInquiry;
            rmCashReceiptsForm = Dynamics.Forms.RmCashReceipts;
            rmCashReceiptInquiryForm = Dynamics.Forms.RmCashInquiry;
            lockBoxTransactionsForm = ChemPointSalesExt.Forms.PalbTransactions;
            eftCustomerMappingForm = ChemPointSalesExt.Forms.EftCustomerMapping;
            customerLookupForm = Dynamics.Forms.CustomerLookup;
            eftCustomerIdLookupForm = ChemPointSalesExt.Forms.EftCustomerIdLookup;
            eftDocumentNumberLookupForm = ChemPointSalesExt.Forms.EftDocumentNumberLookup;
            eftReferenceNumberLookupForm = ChemPointSalesExt.Forms.EftReferenceNumberLookup;
            eftCustomerRemittancesInquiryForm = ChemPointSalesExt.Forms.EftCustomerBankInquiry;
            eftCustomerRemittancesEntryForm = ChemPointSalesExt.Forms.EftCustomerBankEntry;
            eftBatchIdLookupForm = ChemPointSalesExt.Forms.EftBatchIdLookup;
            bankCtxRemittanceImportForm = ChemPointSalesExt.Forms.BankCtxRemittanceImport;
            bankCtxSummaryImportForm = ChemPointSalesExt.Forms.BankCtxSummaryImport;
            eftRemittancePaymentEntryForm = ChemPointSalesExt.Forms.RemittancePaymentEntry;
            eftEmailRemittanceInquiryForm = ChemPointSalesExt.Forms.EmailRemittanceInquiry;
            eftEmailRemittanceForm = ChemPointSalesExt.Forms.EftEmailRemittanceEntry;
            eftEmailReferenceLookupForm = ChemPointSalesExt.Forms.EftEmailReferenceLookup;

            rmCashApplyForm = Dynamics.Forms.RmCashApply;
            rmCashApplyForm.RmApplyDocument.RmAppliedDocumentScroll.LocalApplySelect.Change += new EventHandler(RmAppliedDocumentScrollLocalApplySelectChange);
            rmCashApplyForm.RmApplyDocument.UnapplyButton.ClickAfterOriginal += new EventHandler(rmCashApplyFormApplyAndUnapplyButtonClickAfterOriginal);
            rmCashApplyForm.RmApplyDocument.AutoApplyButton.ClickAfterOriginal += new EventHandler(rmCashApplyFormApplyAndUnapplyButtonClickAfterOriginal);
            rmCashApplyForm.RmApplyDocument.CloseAfterOriginal += new EventHandler(rmCashApplyFormRmApplyDocument_CloseAfterOriginal);
            rmCashApplyForm.Functions.ChangeApplyAmt.InvokeBeforeOriginal += ChangeApplyAmt_InvokeBeforeOriginal;

            cpReceivablesTransactionDetailsForm = ChemPointSalesExt.Forms.CpReceivablesTransactionDetails;
            rmSalesEntryForm.AddMenuHandler(this.ReceivablesTransactionDetails, "Receivables Transaction Details", "D");
            cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.OpenBeforeOriginal += new System.ComponentModel.CancelEventHandler(CpReceivablesTransactionDetails_OpenBeforeOriginal);
            cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.OpenAfterOriginal += new EventHandler(CpReceivablesTransactionDetails_OpenAfterOriginal);

            cpReceivablesTransactionInquiryForm = ChemPointSalesExt.Forms.CpReceivablesTransactionInquiry;
            rmSalesInquiryForm.AddMenuHandler(this.ReceivablesInquiryDetails, "Receivables Inquiry Details", "D");
            cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.OpenBeforeOriginal += new System.ComponentModel.CancelEventHandler(CpReceivablesTransactionInquiry_OpenBeforeOriginal);
            cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.OpenAfterOriginal += new EventHandler(CpReceivablesTransactionInquiry_OpenAfterOriginal);

            rmSalesEntryForm.AddMenuHandler(this.ReceivableTransactionProcessingWindow, "Apply to Open Orders Window", "I");
            rmSalesInquiryForm.AddMenuHandler(this.ReceivableTransactionProcessingInquiryWindow, "Apply to Open Orders Window", "I");


            //CPservice Configuration 

            #region CashApplication
            cashApplicationProcessForm = ChemPointSalesExt.Forms.CashApplicationProcessEntry;
            rmCashReceiptsForm.AddMenuHandler(this.CashReceiptProcessingWindow, "Apply to Open Orders Window", "I");
            cashApplicationProcessForm.ApplyToOpenOrders.OpenBeforeOriginal += new System.ComponentModel.CancelEventHandler(ApplyToOpenOrders_OpenBeforeOriginal);
            cashApplicationProcessForm.ApplyToOpenOrders.OpenAfterOriginal += new EventHandler(ApplyToOpenOrders_OpenAfterOriginal);
            rmCashReceiptsForm.Procedures.DeleteCashReceipt.InvokeAfterOriginal += DeleteCashReceipt_InvokeAfterOriginal;

            rmCashReceiptsForm.RmCashReceipts.CurrencyId.EnterAfterOriginal += new EventHandler(RmCashReceiptsCurrencyId_EnterAfterOriginal);
            rmCashReceiptsForm.RmCashReceipts.CurrencyId.LeaveAfterOriginal += new EventHandler(RmCashReceiptsCurrencyId_LeaveAfterOriginal);
            rmCashReceiptsForm.Procedures.McCurrencyVerification.InvokeBeforeOriginal += RmCashReceiptsMcCurrencyVerification_InvokeBeforeOriginal;

            rmSalesEntryForm.RmSalesEntry.CurrencyId.EnterAfterOriginal += new EventHandler(RmSalesEntryCurrencyId_EnterAfterOriginal);
            rmSalesEntryForm.RmSalesEntry.CurrencyId.LeaveAfterOriginal += new EventHandler(RmSalesEntryCurrencyId_LeaveAfterOriginal);
            rmSalesEntryForm.Procedures.McCurrVerification.InvokeBeforeOriginal += RmSalesEntryMcCurrVerification_InvokeBeforeOriginal;
            
            cashApplicationProcessInquiryForm = ChemPointSalesExt.Forms.CashApplicationProcessInquiry;
            rmCashReceiptInquiryForm.AddMenuHandler(this.CashReceiptProcessingInquiryWindow, "Apply to Open Orders Window", "I");
            cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.OpenBeforeOriginal += new System.ComponentModel.CancelEventHandler(ApplyToOpenOrdersInquiry_OpenBeforeOriginal);
            cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.OpenAfterOriginal += new EventHandler(ApplyToOpenOrdersInquiry_OpenAfterOriginal);

            lockBoxTransactionsForm.AddMenuHandler(this.LockBoxProcessingWindow, "Apply to Open Orders Window", "I");
            lockBoxTransactionsForm.PalbTransactions.ExpansionButton1.ClickAfterOriginal += new EventHandler(this.LockBoxProcessingWindow);

            #endregion CashApplication


            #region EFT AUtomation


            eftDocumentNumberLookupForm.EftDocumentNumberLookup.CancelButton.ClickAfterOriginal += eftDocumentNumberLookupFormCancelButton_ClickAfterOriginal;
            eftReferenceNumberLookupForm.EftReferenceNumberLookup.CancelButton.ClickAfterOriginal += eftReferenceNumberLookupFormCancelButton_ClickAfterOriginal;
            eftReferenceNumberLookupForm.EftReferenceNumberLookup.CloseAfterOriginal += EftReferenceNumberLookup_CloseAfterOriginal;

            eftDocumentNumberLookupForm.EftDocumentNumberLookup.CloseAfterOriginal += EftDocumentNumberLookup_CloseAfterOriginal;

            eftDocumentNumberLookupForm.EftDocumentNumberLookup.RedisplayButton.ClickAfterOriginal += EftDocumentNumberLookupRedisplayButton_ClickAfterOriginal;
            eftReferenceNumberLookupForm.EftReferenceNumberLookup.RedisplayButton.ClickAfterOriginal += eftReferenceNumberLookupFormRedisplayButton_ClickAfterOriginal;
            eftBatchIdLookupForm.EftBatchIdLookup.RedisplayButton.ClickAfterOriginal += EftBAtchIdLookupRedisplayButton_ClickAfterOriginal;

            #region EFT_Customer_Mapping_Window
            eftCustomerMappingForm.EftCustomerMapping.OpenAfterOriginal += new EventHandler(EftCustomerMapping_OpenAfterOriginal);
            eftCustomerMappingForm.EftCustomerMapping.LookupButton1.ClickAfterOriginal += new EventHandler(EftCustomerMappingLookup1_ClickAfterOriginal);
            eftCustomerMappingForm.EftCustomerMapping.DeleteButton.ClickAfterOriginal += new EventHandler(EftCustomerMappingDelete_ClickAfterOriginal);
            eftCustomerMappingForm.EftCustomerMapping.SaveButton.ClickAfterOriginal += new EventHandler(EftCustomerMappingSave_ClickAfterOriginal);
            eftCustomerMappingForm.EftCustomerMapping.EftctxCustomerSourceScroll.LineEnterAfterOriginal += new EventHandler(EftCustomerMappingScrollEnter_LineEnterAfterOriginal);
            eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Change += new EventHandler(EftCustomerMappingCustomerNumber_Change);
            eftCustomerMappingForm.EftCustomerMapping.EftctxCustomerSourceScroll.EftCtxCustomerSource.LeaveBeforeOriginal += EftCustomerMappingCtxCustomerSource_LeaveBeforeOriginal;
            eftCustomerMappingForm.EftCustomerMapping.CloseBeforeOriginal += EftCustomerMapping_CloseBeforeOriginal;
            eftCustomerMappingForm.EftCustomerMapping.ClearButton.ClickAfterOriginal += EftCustomerMappingClear_ClickAfterOriginal;

            #endregion EFT_Customer_Mapping_Window

            customerLookupForm.CustomerLookup.SelectButton.ClickAfterOriginal += new EventHandler(CustomerLookupSelect_OpenAfterOriginal);

            #region CTX_Import_Window

            bankCtxRemittanceImportForm.BankCtxRemittanceImport.OpenBeforeOriginal += new System.ComponentModel.CancelEventHandler(BankCtxRemittanceImport_OpenBeforeOriginal);
            bankCtxRemittanceImportForm.BankCtxRemittanceImport.ImportButton.ClickAfterOriginal += new EventHandler(BankCtxRemittanceImportButton_ClickAfterOriginal);
            bankCtxRemittanceImportForm.BankCtxRemittanceImport.ClearButton.ClickAfterOriginal += new EventHandler(BankCtxRemittanceImportClearButton_ClickAfterOriginal);
            bankCtxRemittanceImportForm.BankCtxRemittanceImport.FilePath.Change += new EventHandler(BankCtxRemittanceFilePath_Change);

            #endregion CTX_Import_Window

            #region BankSummary_Import_Window

            bankCtxSummaryImportForm.BankCtxSummaryImport.ImportButton.ClickAfterOriginal += new EventHandler(BankCtxSummaryImportButton_ClickAfterOriginal);
            bankCtxSummaryImportForm.BankCtxSummaryImport.ClearButton.ClickAfterOriginal += new EventHandler(BankCtxSummaryClearButton_ClickAfterOriginal);
            bankCtxSummaryImportForm.BankCtxSummaryImport.OpenBeforeOriginal += new System.ComponentModel.CancelEventHandler(BankCtxSummaryImport_OpenBeforeOriginal);
            bankCtxSummaryImportForm.BankCtxSummaryImport.FilePath.Change += new EventHandler(BankCtxSummaryFilePath_Change);
            bankCtxSummaryImportForm.BankCtxSummaryImport.EftBatchId.LeaveAfterOriginal += BankCtxSummaryImportEftBatchId_LeaveAfterOriginal;
            bankCtxSummaryImportForm.BankCtxSummaryImport.EftBatchId.EnterBeforeOriginal += EftBatchId_EnterBeforeOriginal;

            #endregion BankSummary_Import_Window

            #region BankEntry_Window


            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.OpenAfterOriginal += new EventHandler(EFTCustomerRemittanceEntry_OpenAfterOriginal);
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.RedisplayButton.ClickAfterOriginal += new EventHandler(EFTCustomerRemittance_ClickAfterOriginal);
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.SaveButton.ClickAfterOriginal += new EventHandler(EFTCustomerRemittanceSave_ClickAfterOriginal);
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.ClearButton.ClickAfterOriginal += new EventHandler(EFTCustomerRemittanceClear_ClickAfterOriginal);
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.BatchNumber.Change += CustomerBankEntryEftBatchId_Change;

            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromCustomerNumber.LeaveAfterOriginal += new EventHandler(EFTRemittanceFromCustomerNumber_LeaveAfterOriginal);
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromItemReference.LeaveAfterOriginal += new EventHandler(EFTRemittanceFromItemReference_LeaveAfterOriginal);
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromReferenceNumber.LeaveAfterOriginal += new EventHandler(EFTRemittanceFromReference_LeaveAfterOriginal);
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToCustomerNumber.LeaveAfterOriginal += new EventHandler(EFTRemittanceToCustomerNumber_LeaveAfterOriginal);
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToItemReference.LeaveAfterOriginal += new EventHandler(EFTRemittanceToItemReference_LeaveAfterOriginal);
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToReferenceNumber.LeaveAfterOriginal += new EventHandler(EFTRemittanceToReference_LeaveAfterOriginal);
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromDateReceived.LeaveAfterOriginal += new EventHandler(EFTRemittanceDateFrom_LeaveAfterOriginal);
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.AllOrRange.LeaveAfterOriginal += new EventHandler(AllOrRange_Change);
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalSearchBy.Change += LocalSearchBy_Change;
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LookupButton1.ClickAfterOriginal += CustomerRemitEntryLookupButton1_ClickAfterOriginal;
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LookupButton2.ClickAfterOriginal += CustomerRemitEntryLookupButton2_ClickAfterOriginal;
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LookupButton9.ClickAfterOriginal += CustomerBankEntryLookupButton9_ClickAfterOriginal;
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.DeleteRowButton.ClickAfterOriginal += CustomerBankEntryDeleteRowButton_ClickAfterOriginal;

            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftReferenceNumber.EnterBeforeOriginal += EftBankReferenceNumber_EnterBeforeOriginal;
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftReferenceNumber.LeaveAfterOriginal += EftReferenceNumber_LeaveAfterOriginal;
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.LineLeaveBeforeOriginal += EftCustomerRemitScroll_LineLeaveAfterOriginal;
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.DeleteEftPayment.ClickAfterOriginal += eftCustomerRemittancesDeleteButton_ClickAfterOriginal;
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.MapEmailRemittances.ClickAfterOriginal += eftCustomerRemittancesMapEmailRemittancesButton_ClickAfterOriginal;
            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalNameSortBy.Change += eftCustomerRemittancesNameSortBy_Change;

            eftBatchIdLookupForm.EftBatchIdLookup.CancelButton.ClickAfterOriginal += EftBatchIdLookupCancelButton_ClickAfterOriginal;

            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.OpenBeforeOriginal += CustomerRemittanceInquiry_OpenBeforeOriginal;
            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.RedisplayButton.ClickAfterOriginal += new EventHandler(EFTCustomerRemittanceInquiry_ClickAfterOriginal);
            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.OkButton.ClickAfterOriginal += new EventHandler(EFTCustomerRemittanceOK_ClickAfterOriginal);
            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalSearchBy.Change += EFTCustomerInquiryLocalSearchBy_Change;
            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LookupButton1.ClickAfterOriginal += CustomerRemitInquiryLookupButton1_ClickAfterOriginal;
            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LookupButton2.ClickAfterOriginal += CustomerRemitInquiryLookupButton2_ClickAfterOriginal;
            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.AllOrRange.LeaveAfterOriginal += new EventHandler(EFTCustomerInquiryAllOrRange_Change);
            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LookupButton9.ClickAfterOriginal += CustomerBankInquiryLookupButton9_ClickAfterOriginal;
            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.EftBatchId.Change += CustomerBankInquiryEftBatchId_Change;
            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.ClearButton.ClickAfterOriginal += EftBankInquiryClearButton_ClickAfterOriginal;

            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromCustomerNumber.LeaveAfterOriginal += new EventHandler(EFTRemittanceFromCustomerNumberInquiry_LeaveAfterOriginal);
            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromItemReference.LeaveAfterOriginal += new EventHandler(EFTRemittanceFromItemReferenceInquiry_LeaveAfterOriginal);
            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromReferenceNumber.LeaveAfterOriginal += new EventHandler(EFTRemittanceFromReferenceInquiry_LeaveAfterOriginal);
            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToCustomerNumber.LeaveAfterOriginal += new EventHandler(EFTRemittanceToCustomerNumberInquiry_LeaveAfterOriginal);
            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToItemReference.LeaveAfterOriginal += new EventHandler(EFTRemittanceToItemReferenceInquiry_LeaveAfterOriginal);
            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToReferenceNumber.LeaveAfterOriginal += new EventHandler(EFTRemittanceToReferenceInquiry_LeaveAfterOriginal);
            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromDateReceived.LeaveAfterOriginal += new EventHandler(EFTRemittanceDateFromInquiry_LeaveAfterOriginal);

            #endregion BankEntry_Window

            #region Payment_Entry_Window

            eftRemittancePaymentEntryForm.RemittancePaymentEntry.OpenBeforeOriginal += PaymentRemittanceEntry_OpenBeforeOriginal;
            eftRemittancePaymentEntryForm.RemittancePaymentEntry.RedisplayButton.ClickAfterOriginal += new EventHandler(EFTPaymentRemittanceRedisplayButton_ClickAfterOriginal);

            eftRemittancePaymentEntryForm.RemittancePaymentEntry.ClearButton.ClickAfterOriginal += new EventHandler(EFTPaymentRemittanceClear_ClickAfterOriginal);

            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalSearchBy.Change += PaymentRemittanceLocalSearchBy_Change;
            eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftApplyType.Change += PaymentRemittanceEftApplyType_Change;
            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LookupButton1.ClickAfterOriginal += CustomerPaymentRemitEntryLookupButton1_ClickAfterOriginal;
            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LookupButton2.ClickAfterOriginal += CustomerPaymentRemitEntryLookupButton2_ClickAfterOriginal;


            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromCustomerNumber.LeaveAfterOriginal += new EventHandler(EFTPaymentRemittanceFromCustomerNumber_LeaveAfterOriginal);
            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromItemRefernce.LeaveAfterOriginal += new EventHandler(EFTPaymentRemittanceFromItemReference_LeaveAfterOriginal);
            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromReferenceNumber.LeaveAfterOriginal += new EventHandler(EFTPaymentRemittanceFromReference_LeaveAfterOriginal);
            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToCustomerNumber.LeaveAfterOriginal += new EventHandler(EFTPaymentRemittanceToCustomerNumber_LeaveAfterOriginal);
            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToItemRefernce.LeaveAfterOriginal += new EventHandler(EFTPaymentRemittanceToItemReference_LeaveAfterOriginal);
            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToReferenceNumber.LeaveAfterOriginal += new EventHandler(EFTPaymentRemittanceToReference_LeaveAfterOriginal);
            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromDateReceived.LeaveAfterOriginal += new EventHandler(EFTPaymentRemittanceDateFrom_LeaveAfterOriginal);
            eftRemittancePaymentEntryForm.RemittancePaymentEntry.AllOrRange.LeaveAfterOriginal += new EventHandler(EFTPaymentRemitAllOrRange_Change);


            eftRemittancePaymentEntryForm.RemittancePaymentEntry.PushToGp.ClickAfterOriginal += new EventHandler(EFTPaymentRemitPushToGp_ClickAfterOriginal);
            eftRemittancePaymentEntryForm.RemittancePaymentEntry.Validate.ClickAfterOriginal += new EventHandler(EFTPaymentRemitValidate_ClickAfterOriginal);
            eftRemittancePaymentEntryForm.RemittancePaymentEntry.IsSelected.Change += IsSelected_Change;
            eftRemittancePaymentEntryForm.RemittancePaymentEntry.RemittancePaymentScroll.IsSelected.Change += new EventHandler(EFTPaymentRemitIsSelected_Change);
            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LookupButton9.ClickAfterOriginal += RemittancePaymentEntryLookupButton9_ClickAfterOriginal;
            eftRemittancePaymentEntryForm.RemittancePaymentEntry.BatchNumber.Change += new EventHandler(RemittancePaymentEntryEftBatchId_Change);
            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalNameSortBy.Change += eftRemittancePaymentEntryNameSortBy_Change;
            #endregion Payment_Entry_Window

            #region EFTEmailRemtitance

            eftEmailRemittanceForm.EftEmailRemittanceEntry.LookupButton1.ClickAfterOriginal += EmailRemittanceEntryLookupButton1_ClickAfterOriginal;
            eftEmailRemittanceForm.EftEmailRemittanceEntry.DeleteButton.ClickAfterOriginal += EmailRemittanceEntryDelete_ClickAfterOriginal;
            eftEmailRemittanceForm.EftEmailRemittanceEntry.SaveButton.ClickAfterOriginal += EmailRemittanceEntrySave_ClickAfterOriginal;
            eftEmailRemittanceForm.EftEmailRemittanceEntry.EftEmailRemittanceScroll.LineLeaveAfterOriginal += EmailRemittanceEntryScroll_LineLeaveAfterOriginal;
            eftEmailRemittanceForm.EftEmailRemittanceEntry.ClearButton.ClickAfterOriginal += EmailRemittanceEntryClear_ClickAfterOriginal;
            eftEmailRemittanceForm.EftEmailRemittanceEntry.CloseBeforeOriginal += EftEmailRemittanceEntry_CloseBeforeOriginal;
            eftEmailRemittanceForm.EftEmailRemittanceEntry.CurrencyId.LeaveAfterOriginal += new EventHandler(EFTEmailRemittanceCurrencyID_LeaveAfterOriginal);
            eftEmailRemittanceForm.EftEmailRemittanceEntry.EftReferenceNumber.LeaveAfterOriginal += EftEmailReferenceNumber_LeaveAfterOriginal;
            eftEmailRemittanceForm.EftEmailRemittanceEntry.EftEmailRemittanceScroll.EftItemReference.LeaveAfterOriginal += EftEmailRemittanceItemReference_LeaveAfterOriginal;
            
            eftEmailRemittanceForm.EftEmailRemittanceEntry.OpenAfterOriginal += EftEmailRemittanceEntry_OpenAfterOriginal; 
            eftEmailReferenceLookupForm.EftEmailReferenceLookup.CancelButton.ClickAfterOriginal += EftEmailReferenceLookupCancelButton_ClickAfterOriginal;
            eftEmailReferenceLookupForm.EftEmailReferenceLookup.CloseAfterOriginal += EftEmailReferenceLookup_CloseAfterOriginal;
            eftEmailReferenceLookupForm.EftEmailReferenceLookup.RedisplayButton.ClickAfterOriginal += EftEmailReferenceLookupRedisplayButton_ClickAfterOriginal;
            eftEmailReferenceLookupForm.EftEmailReferenceLookup.SelectButton.ClickAfterOriginal += EftEmailReferenceLookupSelectButton_ClickAfterOriginal;

            #endregion EFTEmailRemittance

            #region EmailRemittance



            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.OpenBeforeOriginal += EmailRemittanceInquiry_OpenBeforeOriginal;
            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.RedisplayButton.ClickAfterOriginal += new EventHandler(EFTEmailRemittanceInquiryRedisplayButton_ClickAfterOriginal);
            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.ClearButton.ClickAfterOriginal += new EventHandler(EFTEmailRemittanceInquiryClear_ClickAfterOriginal);

            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalSearchBy.Change += EmailRemittanceInquiryLocalSearchBy_Change;
            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LookupButton1.ClickAfterOriginal += EmailRemitEntryInquiryLookupButton1_ClickAfterOriginal;
            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LookupButton2.ClickAfterOriginal += EmailRemitEntryInquiryLookupButton2_ClickAfterOriginal;

            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromCustomerNumber.LeaveAfterOriginal += new EventHandler(EFTEmailRemittanceInquiryFromCustomerNumber_LeaveAfterOriginal);
            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromItemReference.LeaveAfterOriginal += new EventHandler(EFTEmailRemittanceInquiryFromItemReference_LeaveAfterOriginal);
            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromReferenceNumber.LeaveAfterOriginal += new EventHandler(EFTEmailRemittanceInquiryFromReference_LeaveAfterOriginal);
            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToCustomerNumber.LeaveAfterOriginal += new EventHandler(EFTEmailRemittanceInquiryToCustomerNumber_LeaveAfterOriginal);
            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToItemReference.LeaveAfterOriginal += new EventHandler(EFTEmailRemittanceInquiryToItemReference_LeaveAfterOriginal);
            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToReferenceNumber.LeaveAfterOriginal += new EventHandler(EFTEmailRemittanceInquiryToReference_LeaveAfterOriginal);
            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromDateReceived.LeaveAfterOriginal += new EventHandler(EFTEmailRemittanceInquiryDateFrom_LeaveAfterOriginal);
            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.AllOrRange.LeaveAfterOriginal += new EventHandler(EFTEmailRemitInquiryAllOrRange_Change);

            #endregion EmailRemittance

            #endregion EFT Automation

            string getCurrentDirectoryPath = @Directory.GetCurrentDirectory();
            string iniFilePath = getCurrentDirectoryPath + "\\Data\\Dex.ini";

            List<string> categories = GetCategories(iniFilePath);
            foreach (string category in categories)
            {
                //Get the key values
                gpServiceConfigurationURL = GetIniFileString(iniFilePath, category, "GPSERVICE", "");
                EFTSummaryBackupPath = GetIniFileString(iniFilePath, category, "EFTSummaryBackupPath", "");
                EFTRemittanceBackupPath = GetIniFileString(iniFilePath, category, "EFTRemittanceBackupPath", "");
                isRmLoggingEnabled = Convert.ToBoolean(GetIniFileString(iniFilePath, category, "ISRMLOGENABLED", ""));
                rmLogFileName = GetIniFileString(iniFilePath, category, "RMLOGFILENAME", "");
                rmLogFilePath = GetIniFileString(iniFilePath, category, "RMLOGFILEPATH", "");


            }
        }

        private void eftRemittancePaymentEntryNameSortBy_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                eftRemittancePaymentEntryForm.Procedures.EftPaymentRemittancesEntryFormScrollFill.Invoke();
            }
            catch (Exception exception)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + "Error: eftCustomerRemittancesMapEmailRemittancesButton_ClickAfterOriginal Method ");
                MessageBox.Show(exception.Message);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }



        /// <summary>
        /// EftEmailRemittanceItemReference_LeaveAfterOriginal 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EftEmailRemittanceItemReference_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            int totalEFTLine = 0;
            try
            {
                if (!string.IsNullOrEmpty(eftEmailRemittanceForm.EftEmailRemittanceEntry.EftEmailRemittanceScroll.EftItemReference.Value))
                {
                    eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();
                    eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();
                    TableError errorValidTemp = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.GetFirst();
                    while (errorValidTemp == TableError.NoError && errorValidTemp != TableError.EndOfTable)
                    {
                        totalEFTLine++;
                        errorValidTemp = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.GetNext();
                    }
                    eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();
                    eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();
                    if (totalEFTLine == 0)
                    {
                        eftEmailRemittanceForm.EftEmailRemittanceEntry.EftEmailRemittanceScroll.EftItemAmount.Value = eftEmailRemittanceForm.EftEmailRemittanceEntry.PaymentAmount.Value;
                    }
                }

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftEmailRemittanceItemReference_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }


        #region EFTEmailRemittance

        /// <summary>
        /// Before open the window clear the temp table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EftEmailRemittanceEntry_OpenAfterOriginal(object sender, EventArgs e)
        {
           StringBuilder logMessage = new StringBuilder();
           try
           {
               ClearEFTEmailScrollTemp();
           }
           catch (Exception ex)
           {
               logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftEmailRemittanceEntry_OpenAfterOriginal Method: " + ex.Message.ToString());
               MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
           }
           finally
           {
               LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
               logMessage = null;
           }
        }

        /// <summary>
        /// Email Remittance Lookup Cancel Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EftEmailReferenceLookupCancelButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                eftEmailReferenceLookupForm.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftEmailReferenceLookupCancelButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Email Reference lookup Close 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EftEmailReferenceLookup_CloseAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                eftDocumentNumberLookupForm.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In eftDocumentNumberLookupFormCancelButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Email Remittance Entry before close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EftEmailRemittanceEntry_CloseBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (eftEmailRemittanceForm.EftEmailRemittanceEntry.IsUpdated.Value == 1)
                {
                    DialogResult dialogresult;
                    dialogresult = MessageBox.Show("Do you want to save the changes" + " ?", Resources.STR_MESSAGE_TITLE,
                        MessageBoxButtons.OKCancel);
                    if (dialogresult == DialogResult.OK)
                        SaveEFTEmailReference();
                    else
                        eftCustomerMappingForm.EftCustomerMapping.Close();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftEmailRemittanceEntry_CloseBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }

        /// <summary>
        /// EFT Email Reference Scroll Line leave 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EmailRemittanceEntryScroll_LineLeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {

                if (string.IsNullOrEmpty(eftEmailRemittanceForm.EftEmailRemittanceEntry.EftEmailRemittanceScroll.EftItemReference.Value) &&
                        eftEmailRemittanceForm.EftEmailRemittanceEntry.EftEmailRemittanceScroll.EftItemAmount.Value == 0)
                {
                    eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();
                    eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();
                    eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftLineSeqNumber.Value = eftEmailRemittanceForm.EftEmailRemittanceEntry.EftEmailRemittanceScroll.EftLineSeqNumber.Value;
                    TableError errorValidTemp = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Change();
                    if (errorValidTemp == TableError.NoError)
                    {
                        TableError errorValid = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Remove();
                    }
                    eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();
                    eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();
                    eftEmailRemittanceForm.Procedures.EftEmailRemittancesEntryScrollFill.Invoke();
                    ReFillEftEmailRemittanceTemp();


                }
                else
                {
                    int eftLineSeqId = validateEmailScrollTotalLineItem(eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftLineSeqNumber.Value);

                    if (eftLineSeqId != -1)
                    {
                        eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();
                        eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();
                        eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftLineSeqNumber.Value = eftEmailRemittanceForm.EftEmailRemittanceEntry.EftEmailRemittanceScroll.EftLineSeqNumber.Value;

                        eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Key = 1;
                        TableError errorValidTemp = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Change();
                        if (errorValidTemp != TableError.EndOfTable && errorValidTemp == TableError.NoError)
                        {
                            if (eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftItemReference.Value != eftEmailRemittanceForm.EftEmailRemittanceEntry.EftEmailRemittanceScroll.EftItemReference.Value ||
                                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftItemAmount.Value != eftEmailRemittanceForm.EftEmailRemittanceEntry.EftEmailRemittanceScroll.EftItemAmount.Value
                             )
                            {
                                eftEmailRemittanceForm.EftEmailRemittanceEntry.IsUpdated.Value = 1;
                            }

                            if (eftEmailRemittanceForm.EftEmailRemittanceEntry.IsUpdated.Value == 1)
                            {
                                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftItemReference.Value = eftEmailRemittanceForm.EftEmailRemittanceEntry.EftEmailRemittanceScroll.EftItemReference.Value;
                                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftItemAmount.Value = eftEmailRemittanceForm.EftEmailRemittanceEntry.EftEmailRemittanceScroll.EftItemAmount.Value;
                                TableError isSave = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Save();
                            }

                        }
                        else if (errorValidTemp == TableError.NotFound)
                        {
                            eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftItemReference.Value = eftEmailRemittanceForm.EftEmailRemittanceEntry.EftEmailRemittanceScroll.EftItemReference.Value;
                            eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftItemAmount.Value = eftEmailRemittanceForm.EftEmailRemittanceEntry.EftEmailRemittanceScroll.EftItemAmount.Value;
                            eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftlIneApplyId.Value = --scrollTempEFTAppid;
                            eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftLineSeqNumber.Value = eftLineSeqId;
                            TableError isSave = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Save();
                            eftEmailRemittanceForm.EftEmailRemittanceEntry.IsUpdated.Value = 1;
                        }
                        eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();
                        eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();
                        eftEmailRemittanceForm.Procedures.EftEmailRemittancesEntryScrollFill.Invoke();
                    }
                    else
                    {
                        MessageBox.Show("Sum of Total Item amount should be less then or equal Payment amount" + Resources.STR_MESSAGE_TITLE);
                        eftEmailRemittanceForm.EftEmailRemittanceEntry.EftEmailRemittanceScroll.EftItemAmount.Focus();
                    }
                }
                RemoveEmptyEFTEmailLine();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EmailRemittanceEntryScroll_LineLeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }

        /// <summary>
        /// ReFillEftEmailRemittanceTemp
        /// </summary>
        private void ReFillEftEmailRemittanceTemp()
        {
            int eftLineSeq = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();
                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();
                TableError errorValidTemp = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.ChangeFirst();
                while (errorValidTemp == TableError.NoError && errorValidTemp != TableError.EndOfTable)
                {
                    eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftLineSeqNumber.Value = ++eftLineSeq;
                    eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Save();
                    errorValidTemp = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.ChangeNext();
                }

                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();
                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ReFillEftEmailRemittanceTemp Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Remove the record from Temp table noth Item Number and Item Amount empty.
        /// </summary>
        private void RemoveEmptyEFTEmailLine()
        {
            StringBuilder logMessage = new StringBuilder();
            bool isRemoved = false;
            try
            {
                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();
                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();
                TableError errorValidTemp = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.ChangeFirst();
                while (errorValidTemp == TableError.NoError && errorValidTemp != TableError.EndOfTable)
                {
                    if (string.IsNullOrEmpty(eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftItemReference.Value) &&
                        eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftItemAmount.Value == 0)
                    {
                        isRemoved = true;
                        eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Remove();
                    }
                    errorValidTemp = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.ChangeNext();
                }
                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();
                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();
                if (isRemoved)
                    ReFillEftEmailRemittanceTemp();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In validateEmailScrollTotalLineItem Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// validate sum of Item Amount.
        /// </summary>
        /// <returns></returns>
        private int validateEmailScrollTotalLineItem(int lineSeqNumber)
        {
            decimal sumofItemAmount = 0.00M;
            int lineNumber = 0;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();
                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();
                TableError errorValidTemp = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.GetFirst();
                while (errorValidTemp == TableError.NoError && errorValidTemp != TableError.EndOfTable)
                {
                    ++lineNumber;
                    if (lineSeqNumber != eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftLineSeqNumber.Value)
                        sumofItemAmount += eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftItemAmount.Value;
                    errorValidTemp = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.GetNext();
                }
                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();
                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();
                if (sumofItemAmount + eftEmailRemittanceForm.EftEmailRemittanceEntry.EftEmailRemittanceScroll.EftItemAmount.Value <= eftEmailRemittanceForm.EftEmailRemittanceEntry.PaymentAmount.Value)
                    return ++lineNumber;
                else
                    return -1;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In validateEmailScrollTotalLineItem Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                return -1;
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Eft Email Reference Lookup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EmailRemittanceEntryLookupButton1_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!eftEmailReferenceLookupForm.IsOpen)
                {
                    eftEmailReferenceLookupForm.Open();
                    eftEmailReferenceLookupForm.EftEmailReferenceLookup.RedisplayButton.RunValidate();
                    scrollTempEFTAppid = 0;
                }
                else
                {
                    MessageBox.Show("EFT Email Reference Lookup Window already opened. Please close the window" + Resources.STR_MESSAGE_TITLE);
                }

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTEmailRemittanceClear_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }

        /// <summary>
        /// Delete EFT Email Remittance
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EmailRemittanceEntryDelete_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (eftEmailRemittanceForm.EftEmailRemittanceEntry.EftRowId.Value == 0)
                {
                    ClearEFTEmailReference();
                }
                else
                {
                    DialogResult dialogresult;
                    dialogresult = MessageBox.Show("Do you want to delete this email entry?", Resources.STR_MESSAGE_TITLE,
                        MessageBoxButtons.OKCancel);
                    if (dialogresult == DialogResult.OK)
                    {
                        ReceivablesRequest receivablesRequest = new ReceivablesRequest();
                        ReceivablesResponse receivablesResponse = new ReceivablesResponse();
                        AuditInformation auditInformation = new AuditInformation();
                        EFTPayment eftPayment = new EFTPayment();

                        auditInformation.CompanyId = Dynamics.Globals.CompanyId.Value;

                        eftPayment.EFTRowId = eftEmailRemittanceForm.EftEmailRemittanceEntry.EftRowId.Value;
                        receivablesRequest.AuditInformation = auditInformation;
                        receivablesRequest.EFTPayment = eftPayment;

                        //Service Call
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationURL);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/ReceivablesManagement/DeleteEFTEmailRemittance", receivablesRequest); // we need to refer the web.api service url here.
                            if (response.Result.IsSuccessStatusCode)
                            {
                                receivablesResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                                if (receivablesResponse.Status == ResponseStatus.Success)
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + "Deleted EFT Row id" + eftEmailRemittanceForm.EftEmailRemittanceEntry.EftRowId.Value);
                                    ClearEFTEmailReference();
                                }
                                else
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EmailRemittanceEntryDelete_ClickAfterOriginal id Method (FetchCustomerIdForReference): " + receivablesResponse.ErrorMessage.ToString());
                                    MessageBox.Show("Error: " + receivablesResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Error:  DeleteEFTEmailRemittance", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTEmailRemittanceClear_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }

        /// <summary>
        /// Clear EFT Email scroll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EmailRemittanceEntryClear_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ClearEFTEmailReference();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTEmailRemittanceClear_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Clear EFTEmail Reference
        /// </summary>
        private void ClearEFTEmailReference()
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                scrollTempEFTAppid = 0;
                eftEmailRemittanceForm.EftEmailRemittanceEntry.EftRowId.Value = 0;
                eftEmailRemittanceForm.EftEmailRemittanceEntry.EftReferenceNumber.Value = string.Empty;
                eftEmailRemittanceForm.EftEmailRemittanceEntry.PaymentAmount.Value = 0.00M;
                eftEmailRemittanceForm.EftEmailRemittanceEntry.DateReceived.Value = Convert.ToDateTime("1/1/1990");
                eftEmailRemittanceForm.EftEmailRemittanceEntry.CustomerNumber.Value = string.Empty;
                eftEmailRemittanceForm.EftEmailRemittanceEntry.CurrencyId.Value = string.Empty;
                ClearEFTEmailScrollTemp();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearEFTEmailReference Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Clear Eft Email scroll Temp..
        /// </summary>
        private void ClearEFTEmailScrollTemp()
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();
                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();

                TableError tableRemove = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.ChangeFirst();
                while (tableRemove == TableError.NoError && tableRemove != TableError.EndOfTable)
                {
                    eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Remove();
                    tableRemove = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.ChangeNext();
                }
                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();
                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();
                eftEmailRemittanceForm.Procedures.EftEmailRemittancesEntryScrollFill.Invoke();
                eftEmailRemittanceForm.EftEmailRemittanceEntry.IsUpdated.Value = 0;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearEFTEmailScrollTemp Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        /// <summary>
        /// Line Leave after originals...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EftEmailReferenceNumber_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (string.IsNullOrEmpty(eftEmailRemittanceForm.EftEmailRemittanceEntry.CustomerNumber.Value.ToString().Trim()) &&
                    !string.IsNullOrEmpty(eftEmailRemittanceForm.EftEmailRemittanceEntry.EftReferenceNumber.Value.ToString().Trim()))
                {
                    ReceivablesRequest receivablesRequest = new ReceivablesRequest();
                    ReceivablesResponse receivablesResponse = new ReceivablesResponse();
                    AuditInformation auditInformation = new AuditInformation();
                    EFTPayment eftPayment = new EFTPayment();

                    auditInformation.CompanyId = Dynamics.Globals.CompanyId.Value;

                    eftPayment.ReferenceNumber = eftEmailRemittanceForm.EftEmailRemittanceEntry.EftReferenceNumber.Value;
                    receivablesRequest.AuditInformation = auditInformation;
                    receivablesRequest.EFTPayment = eftPayment;

                    //Service Call
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SalesOrderUpdate/FetchCustomerIdForReference", receivablesRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            receivablesResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                            if (receivablesResponse.Status == ResponseStatus.Success)
                            {
                                eftEmailRemittanceForm.EftEmailRemittanceEntry.CustomerNumber.Value = receivablesResponse.CustomerInformation.CustomerId;
                                eftEmailRemittanceForm.EftEmailRemittanceEntry.EftRowId.Value = 0;
                                eftEmailRemittanceForm.EftEmailRemittanceEntry.DateReceived.Value = DateTime.Today.Date;
                                eftEmailRemittanceForm.EftEmailRemittanceEntry.CurrencyId.Value = "USD";

                            }
                            else
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In FetchCustomerIdForReference id Method (FetchCustomerIdForReference): " + receivablesResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + receivablesResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Error: FetchCustomerIdForReference ", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftEmailReferenceNumber_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }

        /// <summary>
        /// Select lookup value to Email Remittance Entry Window 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EftEmailReferenceLookupSelectButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            ReceivablesResponse receivablesResponse = new ReceivablesResponse();
            try
            {
                //Assign Email Reference Hdr Value..
                eftEmailRemittanceForm.EftEmailRemittanceEntry.EftRowId.Value = eftEmailReferenceLookupForm.EftEmailReferenceLookup.EftEmailReferenceLookupScroll.EftRowId.Value;
                eftEmailReferenceLookupForm.Close();
                if (eftEmailRemittanceForm.EftEmailRemittanceEntry.EftRowId.Value != 0)
                {
                    ClearEFTEmailScrollTemp();
                    // Fetch eftEmailReference Scroll details ...
                    ReceivablesRequest receivablesRequest = new ReceivablesRequest();
                    EFTPayment eftPayment = new EFTPayment();
                    AuditInformation auditInformation = new AuditInformation();

                    eftPayment.EFTRowId = eftEmailRemittanceForm.EftEmailRemittanceEntry.EftRowId.Value;
                    auditInformation.CompanyId = Dynamics.Globals.CompanyId.Value;
                    receivablesRequest.AuditInformation = auditInformation;
                    receivablesRequest.EFTPayment = eftPayment;
                    //Service Call
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/ReceivablesManagement/FetchEmailReferenceScroll", receivablesRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            receivablesResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                            if (receivablesResponse.Status == ResponseStatus.Success)
                            {
                                if (receivablesResponse != null && receivablesResponse.EFTPayment != null)
                                {
                                    eftEmailRemittanceForm.EftEmailRemittanceEntry.EftReferenceNumber.Value = receivablesResponse.EFTPayment.ReferenceNumber.ToString().Trim();
                                    eftEmailRemittanceForm.EftEmailRemittanceEntry.PaymentAmount.Value = receivablesResponse.EFTPayment.PaymentAmount;
                                    eftEmailRemittanceForm.EftEmailRemittanceEntry.DateReceived.Value = receivablesResponse.EFTPayment.DateReceived;
                                    eftEmailRemittanceForm.EftEmailRemittanceEntry.CustomerNumber.Value = receivablesResponse.EFTPayment.CustomerID.ToString().Trim();
                                    eftEmailRemittanceForm.EftEmailRemittanceEntry.CurrencyId.Value = receivablesResponse.EFTPayment.CurrencyID.ToString().Trim();


                                    if (receivablesResponse != null && receivablesResponse.EFTCustomerRemittancesList.Count > 0)
                                    {
                                        int EFTEmaillineId = 0;



                                        eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();
                                        eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();

                                        foreach (var eftEmailScroll in receivablesResponse.EFTCustomerRemittancesList)
                                        {
                                            eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();
                                            eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftLineSeqNumber.Value = ++EFTEmaillineId;
                                            eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftlIneApplyId.Value = eftEmailScroll.EftAppId;
                                            eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftItemReference.Value = eftEmailScroll.ItemReference;
                                            eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftItemAmount.Value = eftEmailScroll.ItemAmount;

                                            eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Save();
                                        }
                                        eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();
                                        eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();

                                        eftEmailRemittanceForm.Procedures.EftEmailRemittancesEntryScrollFill.Invoke();
                                    }
                                    else
                                    {
                                        MessageBox.Show("No Line Items available", Resources.STR_MESSAGE_TITLE);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("No email reference available", Resources.STR_MESSAGE_TITLE);
                                }
                            }
                            else
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In customer id Method (FetchEFTDocnumber): " + receivablesResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + receivablesResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Error: Data does notFetchEFTDocnumber idLookup Table", Resources.STR_MESSAGE_TITLE);
                        }

                    }
                }
                eftEmailRemittanceForm.Procedures.EftEmailRemittancesEntryScrollFill.Invoke();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftEmailReferenceLookupSelectButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Email Reference Lookup Fetch...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EftEmailReferenceLookupRedisplayButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (eftEmailReferenceLookupForm.IsOpen)
                {
                    ReceivablesRequest receivablesRequest = new ReceivablesRequest();
                    ReceivablesResponse receivablesResponse = new ReceivablesResponse();
                    AuditInformation auditInformation = new AuditInformation();

                    bool recordsExists = false;

                    auditInformation.CompanyId = Dynamics.Globals.CompanyId.Value;
                    receivablesRequest.AuditInformation = auditInformation;

                    //Service Call
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/ReceivablesManagement/FetchEmailReferenceLookup", receivablesRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            receivablesResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                            if (receivablesResponse.Status == ResponseStatus.Success)
                            {
                                recordsExists = true;
                            }
                            else
                            {
                                recordsExists = false;
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In customer id Method (FetchEFTDocnumber): " + receivablesResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + receivablesResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                        }
                        else
                        {
                            recordsExists = false;
                            MessageBox.Show("Error: Data does notFetchEFTDocnumber idLookup Table", Resources.STR_MESSAGE_TITLE);
                        }
                    }

                    if (receivablesResponse != null && recordsExists && receivablesResponse.EFTCustomerRemittancesList.Count != 0)
                    {

                        eftEmailReferenceLookupForm.Tables.EftEmailReferenceTemp.Close();
                        eftEmailReferenceLookupForm.Tables.EftEmailReferenceTemp.Release();

                        foreach (var eftEmailLookup in receivablesResponse.EFTCustomerRemittancesList)
                        {
                            eftEmailReferenceLookupForm.Tables.EftEmailReferenceTemp.Release();
                            eftEmailReferenceLookupForm.Tables.EftEmailReferenceTemp.EftRowId.Value = eftEmailLookup.EFTRowId;
                            eftEmailReferenceLookupForm.Tables.EftEmailReferenceTemp.EftReferenceNumber.Value = eftEmailLookup.ReferenceNumber;
                            eftEmailReferenceLookupForm.Tables.EftEmailReferenceTemp.DateReceived.Value = eftEmailLookup.DateReceived;
                            eftEmailReferenceLookupForm.Tables.EftEmailReferenceTemp.PaymentAmount.Value = eftEmailLookup.PaymentAmount;
                            eftEmailReferenceLookupForm.Tables.EftEmailReferenceTemp.CustomerNumber.Value = eftEmailLookup.CustomerID;
                            eftEmailReferenceLookupForm.Tables.EftEmailReferenceTemp.CurrencyId.Value = eftEmailLookup.CurrencyID;


                            eftEmailReferenceLookupForm.Tables.EftEmailReferenceTemp.Save();
                        }
                        eftEmailReferenceLookupForm.Tables.EftEmailReferenceTemp.Release();
                        eftEmailReferenceLookupForm.Tables.EftEmailReferenceTemp.Close();

                        eftEmailReferenceLookupForm.Procedures.EmailReferenceLookupScrollFill.Invoke();
                    }
                    else
                    {
                        MessageBox.Show("No records available", Resources.STR_MESSAGE_TITLE);
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftEmailReferenceLookupRedisplayButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Email remittance Entry Save
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EmailRemittanceEntrySave_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                SaveEFTEmailReference();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftEmailReferenceLookupRedisplayButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                scrollTempEFTAppid = 0;
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        private void SaveEFTEmailReference()
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!string.IsNullOrEmpty(eftEmailRemittanceForm.EftEmailRemittanceEntry.EftReferenceNumber.Value) &&
                   eftEmailRemittanceForm.EftEmailRemittanceEntry.DateReceived.Value != Convert.ToDateTime("1/1/1990") &&
                    eftEmailRemittanceForm.EftEmailRemittanceEntry.PaymentAmount.Value != 0.00M &&
                   !string.IsNullOrEmpty(eftEmailRemittanceForm.EftEmailRemittanceEntry.CustomerNumber.Value) &&
                    !string.IsNullOrEmpty(eftEmailRemittanceForm.EftEmailRemittanceEntry.CurrencyId.Value))
                {
                    if (ValidateSumofItemAmount())
                    {
                        // Fetch eftEmailReference Scroll details ...
                        ReceivablesRequest receivablesRequest = new ReceivablesRequest();
                        ReceivablesResponse receivablesResponse = new ReceivablesResponse();
                        EFTPayment eftPayment = new EFTPayment();
                        List<EFTPayment> eftEmailLineList = new List<EFTPayment>();

                        AuditInformation auditInformation = new AuditInformation();

                        //EFT Email Remittance Header Details...
                        eftPayment.EFTRowId = eftEmailRemittanceForm.EftEmailRemittanceEntry.EftRowId.Value;
                        eftPayment.CustomerID = eftEmailRemittanceForm.EftEmailRemittanceEntry.CustomerNumber.Value;
                        eftPayment.CurrencyID = eftEmailRemittanceForm.EftEmailRemittanceEntry.CurrencyId.Value;
                        eftPayment.PaymentAmount = eftEmailRemittanceForm.EftEmailRemittanceEntry.PaymentAmount.Value;
                        eftPayment.ReferenceNumber = eftEmailRemittanceForm.EftEmailRemittanceEntry.EftReferenceNumber.Value;
                        eftPayment.DateReceived = eftEmailRemittanceForm.EftEmailRemittanceEntry.DateReceived.Value;

                        //EFT Email Remittance Line Details...
                        eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();
                        eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();
                        TableError errorValidTemp = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.GetFirst();
                        while (errorValidTemp == TableError.NoError && errorValidTemp != TableError.EndOfTable)
                        {
                            EFTPayment eftPaymentLine = new EFTPayment();
                            eftPaymentLine.ItemReferenceNumber = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftItemReference.Value;
                            eftPaymentLine.ItemAmount = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftItemAmount.Value;
                            eftPaymentLine.EftAppId = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftlIneApplyId.Value;
                            eftPaymentLine.EftId = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftLineSeqNumber.Value;
                            eftPaymentLine.EFTRowId = eftEmailRemittanceForm.EftEmailRemittanceEntry.EftRowId.Value;
                            eftEmailLineList.Add(eftPaymentLine);

                            errorValidTemp = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.GetNext();
                        }
                        eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();
                        eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();

                        auditInformation.CompanyId = Dynamics.Globals.CompanyId.Value;
                        auditInformation.CreatedBy = Dynamics.Globals.UserId.Value.ToString().Trim();

                        receivablesRequest.AuditInformation = auditInformation;
                        receivablesRequest.EFTPayment = eftPayment;
                        receivablesRequest.EFTPaymentList = eftEmailLineList;

                        //Service Call
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationURL);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/ReceivablesManagement/SaveEmailReferences", receivablesRequest); // we need to refer the web.api service url here.
                            if (response.Result.IsSuccessStatusCode)
                            {
                                receivablesResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                                if (receivablesResponse.Status == ResponseStatus.Success)
                                {
                                    ClearEFTEmailReference();
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + "Record Saved Successfully");
                                }
                                else
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + "Error While saving the record");
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Sum of line item amount should be equal to payment amount", Resources.STR_MESSAGE_TITLE);
                    }
                }
                else
                {
                    MessageBox.Show("Please fill all mandatory fields...", Resources.STR_MESSAGE_TITLE);
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SaveEFTEmailReference Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        private bool ValidateSumofItemAmount()
        {
            StringBuilder logMessage = new StringBuilder();
            decimal sumOfItemAmount = 0.00M;
            try
            {
                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();
                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();
                TableError errorValidTemp = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.GetFirst();
                while (errorValidTemp == TableError.NoError && errorValidTemp != TableError.EndOfTable)
                {
                    sumOfItemAmount += eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.EftItemAmount.Value;
                    errorValidTemp = eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.GetNext();
                }
                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Release();
                eftEmailRemittanceForm.Tables.EftEmailRemittanceTemp.Close();
                if (sumOfItemAmount == eftEmailRemittanceForm.EftEmailRemittanceEntry.PaymentAmount.Value)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ValidateSumofItemAmount Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
                return false;
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        #endregion EFTEmailRemittance

        #region Cash Application

        /// <summary>
        /// New window open from Receivable Transaction window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReceivableTransactionProcessingWindow(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (cashApplicationProcessForm.ApplyToOpenOrders.IsOpen || rmCashApplyForm.RmApplyDocument.IsOpen || cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.IsOpen)
                {
                    MessageBox.Show("Please close Apply sales document window first.", Resources.STR_MESSAGE_TITLE);
                }
                else
                {
                    if (!string.IsNullOrEmpty(rmSalesEntryForm.RmSalesEntry.RmDocumentNumberWork.Value.ToString().Trim()) &&
                    !string.IsNullOrEmpty(rmSalesEntryForm.RmSalesEntry.CustomerNumber.Value.ToString().Trim()) &&
                    !string.IsNullOrEmpty(rmSalesEntryForm.RmSalesEntry.CurrencyId.Value.ToString().Trim()))
                    {
                        if ((rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value != 8)
                                && (rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value != 7))
                        {
                            MessageBox.Show(Resources.STR_ERROR_VALIDCashDocType, Resources.STR_MESSAGE_TITLE);
                        }
                        else
                        {
                            if (rmSalesEntryForm.RmSalesEntry.OriginatingDocumentAmount.Value > 0)
                            {
                                rmSalesEntryForm.RmSalesEntry.CurrencyId.Lock();

                                rmSalesEntryForm.RmSalesEntry.SalesAmount.Lock();
                                rmSalesEntryForm.RmSalesEntry.TradeDiscountAmount.Lock();
                                rmSalesEntryForm.RmSalesEntry.FreightAmount.Lock();
                                rmSalesEntryForm.RmSalesEntry.MiscAmount.Lock();
                                rmSalesEntryForm.RmSalesEntry.TaxAmount.Lock();

                                rmSalesEntryForm.RmSalesEntry.OriginatingSalesAmount.Lock();
                                rmSalesEntryForm.RmSalesEntry.OriginatingTradeDiscountAmount.Lock();
                                rmSalesEntryForm.RmSalesEntry.OriginatingFreightAmount.Lock();
                                rmSalesEntryForm.RmSalesEntry.OriginatingMiscAmount.Lock();
                                rmSalesEntryForm.RmSalesEntry.OriginatingTaxAmount.Lock();

                                rmSalesEntryForm.RmSalesEntry.ApplyButton.Lock();

                                expansionWindowType = Resources.STR_ReceivingEntryForm;
                                cashApplicationProcessForm.ApplyToOpenOrders.Open();
                            }
                            else
                            {
                                MessageBox.Show(Resources.STR_ERROR_TRX_AMOUNT, Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                    else
                        MessageBox.Show("Please fill all mandatory fields.", Resources.STR_MESSAGE_TITLE);

                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ReceivableTransactionProcessingWindow Method: " + ex.Message.ToString());
                MessageBox.Show("Could not open Apply to Open Orders Window : " + ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        /// <summary>
        /// New window open from Receivable Transaction Inquiry window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReceivableTransactionProcessingInquiryWindow(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (cashApplicationProcessForm.ApplyToOpenOrders.IsOpen || rmCashApplyForm.RmApplyDocument.IsOpen || cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.IsOpen)
                {
                    MessageBox.Show("Please close Apply sales document window first.", Resources.STR_MESSAGE_TITLE);
                }
                else
                {
                    if (!string.IsNullOrEmpty(rmSalesInquiryForm.RmSalesInquiry.DocumentNumber.Value.ToString().Trim()) &&
                        !string.IsNullOrEmpty(rmSalesInquiryForm.RmSalesInquiry.CustomerNumber.Value.ToString().Trim()) &&
                        !string.IsNullOrEmpty(rmSalesInquiryForm.RmSalesInquiry.CurrencyId.Value.ToString().Trim()))
                    {
                        if ((rmSalesInquiryForm.RmSalesInquiry.RmDocumentTypeAll.Value != 8)
                                && (rmSalesInquiryForm.RmSalesInquiry.RmDocumentTypeAll.Value != 7))
                        {
                            MessageBox.Show(Resources.STR_ERROR_VALIDCashDocType, Resources.STR_MESSAGE_TITLE);
                        }
                        else
                        {
                            if (rmSalesInquiryForm.RmSalesInquiry.OriginatingDocumentAmount.Value > 0)
                            {
                                expansionWindowInquiryType = Resources.STR_ReceivingInquiryForm;
                                cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.Open();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ReceivableTransactionProcessingInquiryWindow Method: " + ex.Message.ToString());
                MessageBox.Show("Could not open Apply to Open Orders Window : " + ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        /// <summary>
        /// New window open from cash Receipt window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CashReceiptProcessingWindow(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (cashApplicationProcessForm.ApplyToOpenOrders.IsOpen || rmCashApplyForm.RmApplyDocument.IsOpen || cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.IsOpen)
                {
                    MessageBox.Show("Please close Apply sales document window first.", Resources.STR_MESSAGE_TITLE);
                }
                else
                {
                    if (!String.IsNullOrEmpty(rmCashReceiptsForm.RmCashReceipts.LocalStR17DocNumber.Value) &&
                        !String.IsNullOrEmpty(rmCashReceiptsForm.RmCashReceipts.CustomerNumber.Value) &&
                        !String.IsNullOrEmpty(rmCashReceiptsForm.RmCashReceipts.CurrencyId.Value))
                    {

                        if (rmCashReceiptsForm.RmCashReceipts.OriginatingOriginalTrxAmount.Value > 0)
                        {
                            rmCashReceiptsForm.RmCashReceipts.CurrencyId.Lock();
                            rmCashReceiptsForm.RmCashReceipts.OriginalTrxAmount.Lock();
                            rmCashReceiptsForm.RmCashReceipts.OriginatingOriginalTrxAmount.Lock();
                            rmCashReceiptsForm.RmCashReceipts.ApplyButton.Lock();

                            expansionWindowType = Resources.STR_RMCashReceiptForm;
                            cashApplicationProcessForm.ApplyToOpenOrders.Open();
                        }
                        else
                        {
                            MessageBox.Show(Resources.STR_ERROR_TRX_AMOUNT, Resources.STR_MESSAGE_TITLE);
                        }

                    }
                    else
                        MessageBox.Show("Please fill all mandatory fields.", Resources.STR_MESSAGE_TITLE);
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CashReceiptProcessingWindow Method: " + ex.Message.ToString());
                MessageBox.Show("Could not open Apply to Open Orders Window : " + ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        /// <summary>
        /// New inquiry window open from cash Receipt Window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CashReceiptProcessingInquiryWindow(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (cashApplicationProcessForm.ApplyToOpenOrders.IsOpen || rmCashApplyForm.RmApplyDocument.IsOpen || cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.IsOpen)
                {
                    MessageBox.Show("Please close Apply sales document window first.", Resources.STR_MESSAGE_TITLE);
                }
                else
                {

                    if (!String.IsNullOrEmpty(rmCashReceiptInquiryForm.RmCashReceiptsInquiry.DocumentNumber.Value) &&
                        !String.IsNullOrEmpty(rmCashReceiptInquiryForm.RmCashReceiptsInquiry.CustomerNumber.Value) &&
                        !String.IsNullOrEmpty(rmCashReceiptInquiryForm.RmCashReceiptsInquiry.CurrencyId.Value))
                    {
                        if (rmCashReceiptInquiryForm.RmCashReceiptsInquiry.OriginatingOriginalTrxAmount.Value > 0)
                        {
                            expansionWindowInquiryType = Resources.STR_RMCashReceiptinquiryForm;
                            cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.Open();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CashReceiptProcessingInquiryWindow Method: " + ex.Message.ToString());
                MessageBox.Show("Could not open Apply to Open Orders inquiry Window : " + ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        /// <summary>
        /// New window open from cash Receipt window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LockBoxProcessingWindow(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (cashApplicationProcessForm.ApplyToOpenOrders.IsOpen || rmCashApplyForm.RmApplyDocument.IsOpen || cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.IsOpen)
                {
                    MessageBox.Show("Please close Apply sales document window first.", Resources.STR_MESSAGE_TITLE);
                }
                else
                {
                    if (!String.IsNullOrEmpty(lockBoxTransactionsForm.PalbTransactions.CashScroll.DocumentNumber.Value))
                    {

                        if (!String.IsNullOrEmpty(lockBoxTransactionsForm.PalbTransactions.CashScroll.CustomerNumber.Value) &&
                            !String.IsNullOrEmpty(lockBoxTransactionsForm.PalbTransactions.CashScroll.CurrencyId.Value))
                        {
                            expansionWindowType = Resources.STR_LockBoxTransactionsForm;
                            cashApplicationProcessForm.ApplyToOpenOrders.Open();
                        }
                        else
                        {
                            MessageBox.Show("Could not fetch details with customer number/currency is blank.", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select document number.", Resources.STR_MESSAGE_TITLE);
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CashReceiptProcessingWindow Method: " + ex.Message.ToString());
                MessageBox.Show("Could not open Apply to Open Orders Window : " + ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        /// <summary>
        /// Before Open window Register events..
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ApplyToOpenOrders_OpenBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (RegisterCashApplication == false)
                {
                    cashApplicationProcessForm.ApplyToOpenOrders.SaveButton.ClickAfterOriginal += new EventHandler(CashApplicationSaveButton_ClickAfterOriginal);
                    cashApplicationProcessForm.ApplyToOpenOrders.CancelButton.ClickAfterOriginal += new EventHandler(CashApplicationCancelButton_ClickAfterOriginal);
                    cashApplicationProcessForm.ApplyToOpenOrders.CloseAfterOriginal += new EventHandler(ApplyToOpenOrders_CloseAfterOriginal);

                    cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyToDocCheckBox.Change += new EventHandler(CashApplicationCashApplyToDocCheckBox_Change);
                    cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyAmount.Change += new EventHandler(CashApplicationCashApplyAmount_Change);
                    cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(CashApplicationCashApplyAmount_EnterBeforeOriginal);

                    RegisterCashApplication = true;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ApplyToOpenOrders_OpenBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("ApplyToOpenOrders window Registration Failed : " + ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        /// <summary>
        /// Event fired when closing the cash apply window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ApplyToOpenOrders_CloseAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (expansionWindowType == Resources.STR_ReceivingEntryForm
                        && rmSalesEntryForm.RmSalesEntry.IsOpen
                        && !String.IsNullOrEmpty(rmSalesEntryForm.RmSalesEntry.RmDocumentNumberWork.Value.ToString().Trim())
                        && !String.IsNullOrEmpty(rmSalesEntryForm.RmSalesEntry.CustomerNumber.Value.ToString().Trim())
                        && (rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value == 7
                            || rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value == 8))
                {
                    rmSalesEntryForm.RmSalesEntry.CurrencyId.Unlock();

                    rmSalesEntryForm.RmSalesEntry.SalesAmount.Unlock();
                    rmSalesEntryForm.RmSalesEntry.TradeDiscountAmount.Unlock();
                    rmSalesEntryForm.RmSalesEntry.FreightAmount.Unlock();
                    rmSalesEntryForm.RmSalesEntry.MiscAmount.Unlock();
                    rmSalesEntryForm.RmSalesEntry.TaxAmount.Unlock();

                    rmSalesEntryForm.RmSalesEntry.OriginatingSalesAmount.Unlock();
                    rmSalesEntryForm.RmSalesEntry.OriginatingTradeDiscountAmount.Unlock();
                    rmSalesEntryForm.RmSalesEntry.OriginatingFreightAmount.Unlock();
                    rmSalesEntryForm.RmSalesEntry.OriginatingMiscAmount.Unlock();
                    rmSalesEntryForm.RmSalesEntry.OriginatingTaxAmount.Unlock();

                    rmSalesEntryForm.RmSalesEntry.ApplyButton.Unlock();

                }
                else if (expansionWindowType == Resources.STR_RMCashReceiptForm
                    && rmCashReceiptsForm.RmCashReceipts.IsOpen
                    && !String.IsNullOrEmpty(rmCashReceiptsForm.RmCashReceipts.LocalStR17DocNumber.Value)
                    && !String.IsNullOrEmpty(rmCashReceiptsForm.RmCashReceipts.CustomerNumber.Value))
                {
                    rmCashReceiptsForm.RmCashReceipts.CurrencyId.Unlock();
                    rmCashReceiptsForm.RmCashReceipts.OriginalTrxAmount.Unlock();
                    rmCashReceiptsForm.RmCashReceipts.OriginatingOriginalTrxAmount.Unlock();
                    rmCashReceiptsForm.RmCashReceipts.ApplyButton.Unlock();
                }
                expansionWindowType = string.Empty;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CashApplicationCancelButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }

        }
        /// <summary>
        /// After Open window events..
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ApplyToOpenOrders_OpenAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                InsertCashApplicationDetails(expansionWindowType);

                cashApplicationProcessForm.ApplyToOpenOrders.ScrollShrinkSwitch.Value = 1;
                cashApplicationProcessForm.ApplyToOpenOrders.LocalShrinkButton.RunValidate();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ApplyToOpenOrders_OpenAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("ApplyToOpenOrders_OpenAfterOriginal: " + ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        /// <summary>
        ///  Fetching cash application related data and assigning to related Temp Table...
        /// </summary>
        /// <param name="formName"></param>
        private void InsertCashApplicationDetails(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            bool isResult = false;
            try
            {
                if (formName == Resources.STR_LockBoxTransactionsForm)
                {
                    ClearApplyOpenOrderTemp(formName);

                    cashApplicationProcessForm.ApplyToOpenOrders.CurrencyIndex.Value = lockBoxTransactionsForm.PalbTransactions.CashScroll.OriginatingCurrencyIndex.Value;

                    cashApplicationProcessForm.ApplyToOpenOrders.DocumentNumber.Value = lockBoxTransactionsForm.PalbTransactions.CashScroll.DocumentNumber.Value.ToString().Trim();
                    cashApplicationProcessForm.ApplyToOpenOrders.CustomerNumber.Value = lockBoxTransactionsForm.PalbTransactions.CashScroll.CustomerNumber.Value.ToString().Trim();
                    cashApplicationProcessForm.ApplyToOpenOrders.CurrencyId.Value = lockBoxTransactionsForm.PalbTransactions.CashScroll.CurrencyId.Value.ToString().Trim();
                    cashApplicationProcessForm.ApplyToOpenOrders.CashApplyTotalAmt.Value = lockBoxTransactionsForm.PalbTransactions.CashScroll.OriginalTrxAmount.Value;
                    cashApplicationProcessForm.ApplyToOpenOrders.CashApplyUnappliedAmt.Value = 0.00M;

                    ReceivablesRequest cashRequest = new ReceivablesRequest();
                    ReceivablesResponse cashResponse = new ReceivablesResponse();
                    CustomerInformation customerInfo = new CustomerInformation();
                    Amount amountInfo = new Amount();
                    AuditInformation auditInfo = new AuditInformation();
                    ReceivablesHeader receivablesHeader = new ReceivablesHeader();

                    auditInfo.CompanyId = Dynamics.Globals.CompanyId.Value;
                    cashRequest.Source = "Entry";
                    customerInfo.CustomerId = lockBoxTransactionsForm.PalbTransactions.CashScroll.CustomerNumber.Value.ToString().Trim();
                    amountInfo.Currency = lockBoxTransactionsForm.PalbTransactions.CashScroll.CurrencyId.Value.ToString().Trim();
                    receivablesHeader.DocumentNumber = lockBoxTransactionsForm.PalbTransactions.CashScroll.DocumentNumber.Value.ToString().Trim();
                    receivablesHeader.DocumentType = 9;
                    receivablesHeader.Amount = amountInfo;
                    cashRequest.ReceivablesHeader = receivablesHeader;
                    cashRequest.AuditInformation = auditInfo;
                    cashRequest.CustomerInformation = customerInfo;

                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SalesOrderUpdate/FetchApplyToOpenOrder", cashRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            cashResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                            if (cashResponse.Status == ResponseStatus.Success)
                            {
                                isResult = true;
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(cashResponse.ErrorMessage))
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Unknown error occurred to fetch details due to " + cashResponse.ErrorMessage + ". Please contact IT.");
                                    MessageBox.Show("Unknown error occurred to fetch details due to " + cashResponse.ErrorMessage + ". Please contact IT.", Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Unknown error occurred to fetch details. Please contact IT.");
                                    MessageBox.Show("Unknown error occurred to fetch details. Please contact IT.", Resources.STR_MESSAGE_TITLE);
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error occurred while fetching cash application details. Please contact IT.");
                            MessageBox.Show("Error occurred while fetching eligiblity status. Please contact IT.", Resources.STR_MESSAGE_TITLE);
                        }
                    }

                    if (isResult && cashResponse != null)
                    {
                        if (cashResponse.ReceivableEntity != null)
                        {
                            cashApplicationProcessForm.Tables.CashApplyProcessTemp.Close();
                            cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();

                            foreach (var cashResponseInfo in cashResponse.ReceivableEntity)
                            {
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocumentType.Value = 9;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocumentNumber.Value = lockBoxTransactionsForm.PalbTransactions.CashScroll.DocumentNumber.Value.ToString().Trim();
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocNumber.Value = cashResponseInfo.ApplyToDocumentNumber.ToString().Trim();
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocType.Value = Convert.ToInt16(cashResponseInfo.ApplyToDocumentType);
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CustomerNumber.Value = cashResponseInfo.ApplyToCustomerId.ToString().Trim();
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmountRemaining.Value = cashResponseInfo.Amount.AmountRemaining;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.OriginatingAmountRemaining.Value = cashResponseInfo.Amount.AmountRemaining + cashResponseInfo.Amount.ApplyAmount;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocAmount.Value = cashResponseInfo.Amount.DocumentAmount;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value = cashResponseInfo.Amount.ApplyAmount;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocCurrencyId.Value = cashResponseInfo.Amount.Currency;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CurrencyIndex.Value = lockBoxTransactionsForm.PalbTransactions.CashScroll.OriginatingCurrencyIndex.Value;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocAmtInOrigCurr.Value = cashResponseInfo.Amount.OriginatingCurrencyDocumentAmount;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmtInOrigCurr.Value = cashResponseInfo.Amount.ApplyAmountInOrignCurrency;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.ExchangeRate.Value = cashResponseInfo.Amount.ExchangeRate;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocCheckBox.Value = cashResponseInfo.IsSelected;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDate.Value = Convert.ToDateTime(cashResponseInfo.ApplyDate);
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.Status.Value = Convert.ToInt16(cashResponseInfo.StatusId);
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.SopTypeDatabase.Value = Convert.ToInt16(cashResponseInfo.SopTypeDatabase);

                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.UserId.Value = Dynamics.Globals.UserId.Value.ToString();
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.IsUpdated.Value = 0;

                                TableError tempErrorDate1 = cashApplicationProcessForm.Tables.CashApplyProcessTemp.Save();
                            }

                            cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();
                            cashApplicationProcessForm.Tables.CashApplyProcessTemp.Close();
                        }
                    }

                    cashApplicationProcessForm.Procedures.CashApplicationProcessEntryFillWindow.Invoke();

                    CalculateLineTotal(formName);
                }
                else if (formName == Resources.STR_RMCashReceiptForm)
                {
                    ClearApplyOpenOrderTemp(formName);

                    cashApplicationProcessForm.ApplyToOpenOrders.CurrencyIndex.Value = rmCashReceiptsForm.RmCashReceipts.CurrencyIndex.Value;

                    cashApplicationProcessForm.ApplyToOpenOrders.DocumentNumber.Value = rmCashReceiptsForm.RmCashReceipts.LocalStR17DocNumber.Value.ToString();
                    cashApplicationProcessForm.ApplyToOpenOrders.CustomerNumber.Value = rmCashReceiptsForm.RmCashReceipts.CustomerNumber.Value.ToString();
                    cashApplicationProcessForm.ApplyToOpenOrders.CurrencyId.Value = rmCashReceiptsForm.RmCashReceipts.CurrencyId.Value.ToString();
                    cashApplicationProcessForm.ApplyToOpenOrders.CashApplyTotalAmt.Value = rmCashReceiptsForm.RmCashReceipts.OriginatingOriginalTrxAmount.Value;
                    cashApplicationProcessForm.ApplyToOpenOrders.CashApplyUnappliedAmt.Value = 0.00M;

                    ReceivablesRequest cashRequest = new ReceivablesRequest();
                    ReceivablesResponse cashResponse = new ReceivablesResponse();
                    CustomerInformation customerInfo = new CustomerInformation();
                    Amount amountInfo = new Amount();
                    AuditInformation auditInfo = new AuditInformation();
                    ReceivablesHeader receivablesHeader = new ReceivablesHeader();

                    auditInfo.CompanyId = Dynamics.Globals.CompanyId.Value;
                    cashRequest.Source = "Entry";
                    customerInfo.CustomerId = rmCashReceiptsForm.RmCashReceipts.CustomerNumber.Value.ToString().Trim();
                    amountInfo.Currency = rmCashReceiptsForm.RmCashReceipts.CurrencyId.Value.ToString().Trim();
                    receivablesHeader.DocumentNumber = rmCashReceiptsForm.RmCashReceipts.LocalStR17DocNumber.Value.ToString().Trim();
                    receivablesHeader.DocumentType = 9;
                    receivablesHeader.Amount = amountInfo;
                    cashRequest.ReceivablesHeader = receivablesHeader;
                    cashRequest.AuditInformation = auditInfo;
                    cashRequest.CustomerInformation = customerInfo;

                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SalesOrderUpdate/FetchApplyToOpenOrder", cashRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            cashResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                            if (cashResponse.Status == ResponseStatus.Success)
                            {
                                isResult = true;
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(cashResponse.ErrorMessage))
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Unknown error occurred to fetch details due to " + cashResponse.ErrorMessage + ". Please contact IT.");
                                    MessageBox.Show("Unknown error occurred to fetch details due to " + cashResponse.ErrorMessage + ". Please contact IT.", Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Unknown error occurred to fetch details. Please contact IT.");
                                    MessageBox.Show("Unknown error occurred to fetch details. Please contact IT.", Resources.STR_MESSAGE_TITLE);
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error occurred while fetching cash application details. Please contact IT.");
                            MessageBox.Show("Error occurred while fetching eligiblity status. Please contact IT.", Resources.STR_MESSAGE_TITLE);
                        }
                    }

                    if (isResult && cashResponse != null)
                    {
                        if (cashResponse.ReceivableEntity != null)
                        {
                            cashApplicationProcessForm.Tables.CashApplyProcessTemp.Close();
                            cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();

                            foreach (var cashResponseInfo in cashResponse.ReceivableEntity)
                            {
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocumentType.Value = 9;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocumentNumber.Value = rmCashReceiptsForm.RmCashReceipts.LocalStR17DocNumber.Value.ToString().Trim();
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocNumber.Value = cashResponseInfo.ApplyToDocumentNumber.ToString().Trim();
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocType.Value = Convert.ToInt16(cashResponseInfo.ApplyToDocumentType);
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CustomerNumber.Value = cashResponseInfo.ApplyToCustomerId.ToString().Trim();
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmountRemaining.Value = cashResponseInfo.Amount.AmountRemaining;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.OriginatingAmountRemaining.Value = cashResponseInfo.Amount.AmountRemaining + cashResponseInfo.Amount.ApplyAmount;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocAmount.Value = cashResponseInfo.Amount.DocumentAmount;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value = cashResponseInfo.Amount.ApplyAmount;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocCurrencyId.Value = cashResponseInfo.Amount.Currency;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CurrencyIndex.Value = rmCashReceiptsForm.RmCashReceipts.CurrencyIndex.Value;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocAmtInOrigCurr.Value = cashResponseInfo.Amount.OriginatingCurrencyDocumentAmount;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmtInOrigCurr.Value = cashResponseInfo.Amount.ApplyAmountInOrignCurrency;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.ExchangeRate.Value = cashResponseInfo.Amount.ExchangeRate;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocCheckBox.Value = cashResponseInfo.IsSelected;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDate.Value = Convert.ToDateTime(cashResponseInfo.ApplyDate);
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.Status.Value = Convert.ToInt16(cashResponseInfo.StatusId);
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.SopTypeDatabase.Value = Convert.ToInt16(cashResponseInfo.SopTypeDatabase);

                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.UserId.Value = Dynamics.Globals.UserId.Value.ToString();
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.IsUpdated.Value = 0;

                                TableError tempErrorDate1 = cashApplicationProcessForm.Tables.CashApplyProcessTemp.Save();
                            }

                            cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();
                            cashApplicationProcessForm.Tables.CashApplyProcessTemp.Close();
                        }
                    }

                    cashApplicationProcessForm.Procedures.CashApplicationProcessEntryFillWindow.Invoke();

                    CalculateLineTotal(formName);
                }
                else if (formName == Resources.STR_RMCashReceiptinquiryForm)
                {
                    ClearApplyOpenOrderTemp(formName);

                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CurrencyIndex.Value = rmCashReceiptInquiryForm.RmCashReceiptsInquiry.CurrencyIndex.Value;

                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.DocumentNumber.Value = rmCashReceiptInquiryForm.RmCashReceiptsInquiry.DocumentNumber.Value.ToString();
                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CustomerNumber.Value = rmCashReceiptInquiryForm.RmCashReceiptsInquiry.CustomerNumber.Value.ToString();
                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CurrencyId.Value = rmCashReceiptInquiryForm.RmCashReceiptsInquiry.CurrencyId.Value.ToString();
                    if (rmCashReceiptInquiryForm.RmCashReceiptsInquiry.CurrencyView.Value == 1)
                    {
                        cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyTotalAmt.Value = rmCashReceiptInquiryForm.RmCashReceiptsInquiry.OriginalTrxAmount.Value;
                    }
                    else
                    {
                        cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyTotalAmt.Value = rmCashReceiptInquiryForm.RmCashReceiptsInquiry.OriginatingOriginalTrxAmount.Value;
                    }                     
                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyUnappliedAmt.Value = 0.00M;

                    ReceivablesRequest cashRequest = new ReceivablesRequest();
                    ReceivablesResponse cashResponse = new ReceivablesResponse();
                    CustomerInformation customerInfo = new CustomerInformation();
                    Amount amountInfo = new Amount();
                    AuditInformation auditInfo = new AuditInformation();
                    ReceivablesHeader receivableInfo = new ReceivablesHeader();

                    auditInfo.CompanyId = Dynamics.Globals.CompanyId.Value;
                    cashRequest.Source = "Inquiry";
                    customerInfo.CustomerId = rmCashReceiptInquiryForm.RmCashReceiptsInquiry.CustomerNumber.Value.ToString().Trim();
                    if (rmCashReceiptInquiryForm.RmCashReceiptsInquiry.CurrencyId.Value.ToString() != "EUR" || rmCashReceiptInquiryForm.RmCashReceiptsInquiry.CurrencyId.Value.ToString() != "Z-US$")
                    {
                        if (rmCashReceiptInquiryForm.RmCashReceiptsInquiry.CurrencyView.Value == 1)
                        {
                            if (Dynamics.Globals.CompanyId.Value == 1)
                            {
                                amountInfo.Currency = "Z-US$";
                            }
                            else if (Dynamics.Globals.CompanyId.Value == 2)
                            {
                                amountInfo.Currency = "EUR";
                            }                             
                        }
                        else
                        {
                            amountInfo.Currency = rmCashReceiptInquiryForm.RmCashReceiptsInquiry.CurrencyId.Value.ToString().Trim();
                        }
                    }
                    else
                    {
                        amountInfo.Currency = rmCashReceiptInquiryForm.RmCashReceiptsInquiry.CurrencyId.Value.ToString().Trim();
                    }                     
                    receivableInfo.DocumentNumber = rmCashReceiptInquiryForm.RmCashReceiptsInquiry.DocumentNumber.Value.ToString().Trim();
                    receivableInfo.DocumentType = 9;
                    receivableInfo.Amount = amountInfo;

                    cashRequest.ReceivablesHeader = receivableInfo;
                    cashRequest.AuditInformation = auditInfo;
                    cashRequest.CustomerInformation = customerInfo;

                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SalesOrderUpdate/FetchApplyToOpenOrder", cashRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            cashResponse = response.Result.Content.ReadAsAsync<Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse>().Result;
                            if (cashResponse.Status == Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success)
                            {
                                isResult = true;
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(cashResponse.ErrorMessage))
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Unknown error occurred to fetch details due to " + cashResponse.ErrorMessage + ". Please contact IT.");
                                    MessageBox.Show("Unknown error occurred to fetch details due to " + cashResponse.ErrorMessage + ". Please contact IT.", Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Unknown error occurred to fetch details. Please contact IT.");
                                    MessageBox.Show("Unknown error occurred to fetch details. Please contact IT.", Resources.STR_MESSAGE_TITLE);
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error occurred while fetching cash application details. Please contact IT.");
                            MessageBox.Show("Error occurred while fetching eligiblity status. Please contact IT.", Resources.STR_MESSAGE_TITLE);
                        }
                    }

                    if (isResult && cashResponse != null)
                    {
                        if (cashResponse.ReceivableEntity != null)
                        {
                            cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Close();
                            cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Release();

                            foreach (var cashResponseInfo in cashResponse.ReceivableEntity)
                            {
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Release();
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocumentType.Value = 9;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocumentNumber.Value = rmCashReceiptInquiryForm.RmCashReceiptsInquiry.DocumentNumber.Value.ToString().Trim();
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyToDocNumber.Value = cashResponseInfo.ApplyToDocumentNumber.ToString().Trim();
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyToDocType.Value = Convert.ToInt16(cashResponseInfo.ApplyToDocumentType);
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CustomerNumber.Value = cashResponseInfo.ApplyToCustomerId.ToString().Trim();
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyAmountRemaining.Value = cashResponseInfo.Amount.AmountRemaining;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.OriginatingAmountRemaining.Value = cashResponseInfo.Amount.AmountRemaining + cashResponseInfo.Amount.ApplyAmount;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocAmount.Value = cashResponseInfo.Amount.DocumentAmount;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value = cashResponseInfo.Amount.ApplyAmount;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocCurrencyId.Value = cashResponseInfo.Amount.Currency;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CurrencyIndex.Value = rmCashReceiptInquiryForm.RmCashReceiptsInquiry.CurrencyIndex.Value;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocAmtInOrigCurr.Value = cashResponseInfo.Amount.OriginatingCurrencyDocumentAmount;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyAmtInOrigCurr.Value = cashResponseInfo.Amount.ApplyAmountInOrignCurrency;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.ExchangeRate.Value = cashResponseInfo.Amount.ExchangeRate;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDate.Value = Convert.ToDateTime(cashResponseInfo.ApplyDate);
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyToDocCheckBox.Value = cashResponseInfo.IsSelected;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Status.Value = Convert.ToInt16(cashResponseInfo.StatusId);
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.SopTypeDatabase.Value = Convert.ToInt16(cashResponseInfo.SopTypeDatabase);

                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.UserId.Value = Dynamics.Globals.UserId.Value.ToString();
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.IsUpdated.Value = 0;

                                TableError tempErrorDate1 = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Save();
                            }

                            cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Release();
                            cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Close();
                        }
                    }

                    cashApplicationProcessInquiryForm.Procedures.CashApplicationProcessInquiryFillWindow.Invoke();

                    CalculateLineTotal(formName);
                }

                else if (formName == Resources.STR_ReceivingEntryForm)
                {
                    ClearApplyOpenOrderTemp(formName);

                    cashApplicationProcessForm.ApplyToOpenOrders.CurrencyIndex.Value = rmSalesEntryForm.RmSalesEntry.CurrencyIndex.Value;

                    cashApplicationProcessForm.ApplyToOpenOrders.DocumentNumber.Value = rmSalesEntryForm.RmSalesEntry.RmDocumentNumberWork.Value.ToString().Trim();
                    cashApplicationProcessForm.ApplyToOpenOrders.CustomerNumber.Value = rmSalesEntryForm.RmSalesEntry.CustomerNumber.Value.ToString().Trim();
                    cashApplicationProcessForm.ApplyToOpenOrders.CurrencyId.Value = rmSalesEntryForm.RmSalesEntry.CurrencyId.Value.ToString().Trim();
                    cashApplicationProcessForm.ApplyToOpenOrders.CashApplyTotalAmt.Value = rmSalesEntryForm.RmSalesEntry.OriginatingDocumentAmount.Value;
                    cashApplicationProcessForm.ApplyToOpenOrders.CashApplyUnappliedAmt.Value = 0.00M;

                    Chempoint.GP.Model.Interactions.Sales.ReceivablesRequest cashRequest = new Chempoint.GP.Model.Interactions.Sales.ReceivablesRequest();
                    Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse cashResponse = new Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse();
                    CustomerInformation customerInfo = new CustomerInformation();
                    Amount amountInfo = new Amount();
                    AuditInformation auditInfo = new AuditInformation();
                    ReceivablesHeader receivableInfo = new ReceivablesHeader();

                    auditInfo.CompanyId = Dynamics.Globals.CompanyId.Value;
                    cashRequest.Source = "Entry";
                    customerInfo.CustomerId = rmSalesEntryForm.RmSalesEntry.CustomerNumber.Value.ToString().Trim();
                    amountInfo.Currency = rmSalesEntryForm.RmSalesEntry.CurrencyId.Value.ToString().Trim();
                    receivableInfo.DocumentNumber = rmSalesEntryForm.RmSalesEntry.RmDocumentNumberWork.Value.ToString().Trim();
                    receivableInfo.DocumentType = rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value;
                    receivableInfo.Amount = amountInfo;
                    cashRequest.ReceivablesHeader = receivableInfo;
                    cashRequest.AuditInformation = auditInfo;
                    cashRequest.CustomerInformation = customerInfo;

                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SalesOrderUpdate/FetchApplyToOpenOrder", cashRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            cashResponse = response.Result.Content.ReadAsAsync<Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse>().Result;
                            if (cashResponse.Status == Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success)
                            {
                                isResult = true;
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(cashResponse.ErrorMessage))
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Unknown error occurred to fetch details due to " + cashResponse.ErrorMessage + ". Please contact IT.");
                                    MessageBox.Show("Unknown error occurred to fetch details due to " + cashResponse.ErrorMessage + ". Please contact IT.", Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Unknown error occurred to fetch details. Please contact IT.");
                                    MessageBox.Show("Unknown error occurred to fetch details. Please contact IT.", Resources.STR_MESSAGE_TITLE);
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error occurred while fetching cash application details. Please contact IT.");
                            MessageBox.Show("Error occurred while fetching eligiblity status. Please contact IT.", Resources.STR_MESSAGE_TITLE);
                        }
                    }

                    if (isResult && cashResponse != null)
                    {
                        if (cashResponse.ReceivableEntity != null)
                        {
                            cashApplicationProcessForm.Tables.CashApplyProcessTemp.Close();
                            cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();

                            foreach (var cashResponseInfo in cashResponse.ReceivableEntity)
                            {
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocumentType.Value = rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocumentNumber.Value = rmSalesEntryForm.RmSalesEntry.RmDocumentNumberWork.Value.ToString().Trim();
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocNumber.Value = cashResponseInfo.ApplyToDocumentNumber.ToString().Trim();
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocType.Value = Convert.ToInt16(cashResponseInfo.ApplyToDocumentType);
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CustomerNumber.Value = cashResponseInfo.ApplyToCustomerId.ToString().Trim();
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmountRemaining.Value = cashResponseInfo.Amount.AmountRemaining;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.OriginatingAmountRemaining.Value = cashResponseInfo.Amount.AmountRemaining + cashResponseInfo.Amount.ApplyAmount;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocAmount.Value = cashResponseInfo.Amount.DocumentAmount;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value = cashResponseInfo.Amount.ApplyAmount;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocCurrencyId.Value = cashResponseInfo.Amount.Currency;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CurrencyIndex.Value = rmSalesEntryForm.RmSalesEntry.CurrencyIndex.Value;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocAmtInOrigCurr.Value = cashResponseInfo.Amount.OriginatingCurrencyDocumentAmount;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmtInOrigCurr.Value = cashResponseInfo.Amount.ApplyAmountInOrignCurrency;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.ExchangeRate.Value = cashResponseInfo.Amount.ExchangeRate;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDate.Value = Convert.ToDateTime(cashResponseInfo.ApplyDate);
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocCheckBox.Value = cashResponseInfo.IsSelected;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.Status.Value = Convert.ToInt16(cashResponseInfo.StatusId);
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.SopTypeDatabase.Value = Convert.ToInt16(cashResponseInfo.SopTypeDatabase);

                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.UserId.Value = Dynamics.Globals.UserId.Value.ToString();
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.IsUpdated.Value = 0;

                                TableError tempErrorDate1 = cashApplicationProcessForm.Tables.CashApplyProcessTemp.Save();
                            }

                            cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();
                            cashApplicationProcessForm.Tables.CashApplyProcessTemp.Close();
                        }
                    }

                    cashApplicationProcessForm.Procedures.CashApplicationProcessEntryFillWindow.Invoke();

                    CalculateLineTotal(formName);
                }

                else if (formName == Resources.STR_ReceivingInquiryForm)
                {
                    ClearApplyOpenOrderTemp(formName);

                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CurrencyIndex.Value = rmSalesInquiryForm.RmSalesInquiry.CurrencyIndex.Value;

                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.DocumentNumber.Value = rmSalesInquiryForm.RmSalesInquiry.DocumentNumber.Value.ToString();
                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CustomerNumber.Value = rmSalesInquiryForm.RmSalesInquiry.CustomerNumber.Value.ToString();
                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CurrencyId.Value = rmSalesInquiryForm.RmSalesInquiry.CurrencyId.Value.ToString();
                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyTotalAmt.Value = rmSalesInquiryForm.RmSalesInquiry.OriginatingOriginalTrxAmount.Value;
                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyUnappliedAmt.Value = 0.00M;

                    Chempoint.GP.Model.Interactions.Sales.ReceivablesRequest cashRequest = new Chempoint.GP.Model.Interactions.Sales.ReceivablesRequest();
                    Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse cashResponse = new Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse();
                    CustomerInformation customerInfo = new CustomerInformation();
                    Amount amountInfo = new Amount();
                    AuditInformation auditInfo = new AuditInformation();
                    ReceivablesHeader receivableInfo = new ReceivablesHeader();

                    auditInfo.CompanyId = Dynamics.Globals.CompanyId.Value;
                    cashRequest.Source = "Inquiry";
                    customerInfo.CustomerId = rmSalesInquiryForm.RmSalesInquiry.CustomerNumber.Value.ToString().Trim();
                    amountInfo.Currency = rmSalesInquiryForm.RmSalesInquiry.CurrencyId.Value.ToString().Trim();
                    receivableInfo.DocumentNumber = rmSalesInquiryForm.RmSalesInquiry.DocumentNumber.Value.ToString().Trim();
                    receivableInfo.DocumentType = rmSalesInquiryForm.RmSalesInquiry.RmDocumentTypeAll.Value;
                    receivableInfo.Amount = amountInfo;
                    cashRequest.AuditInformation = auditInfo;
                    cashRequest.CustomerInformation = customerInfo;
                    cashRequest.ReceivablesHeader = receivableInfo;

                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SalesOrderUpdate/FetchApplyToOpenOrder", cashRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            cashResponse = response.Result.Content.ReadAsAsync<Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse>().Result;
                            if (cashResponse.Status == Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success)
                            {
                                isResult = true;
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(cashResponse.ErrorMessage))
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Unknown error occurred to fetch details due to " + cashResponse.ErrorMessage + ". Please contact IT.");
                                    MessageBox.Show("Unknown error occurred to fetch details due to " + cashResponse.ErrorMessage + ". Please contact IT.", Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Unknown error occurred to fetch details. Please contact IT.");
                                    MessageBox.Show("Unknown error occurred to fetch details. Please contact IT.", Resources.STR_MESSAGE_TITLE);
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error occurred while fetching cash application details. Please contact IT.");
                            MessageBox.Show("Error occurred while fetching eligiblity status. Please contact IT.", Resources.STR_MESSAGE_TITLE);
                        }
                    }

                    if (isResult && cashResponse != null)
                    {
                        if (cashResponse.ReceivableEntity != null)
                        {
                            cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Close();
                            cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Release();

                            foreach (var cashResponseInfo in cashResponse.ReceivableEntity)
                            {
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Release();
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocumentType.Value = rmSalesInquiryForm.RmSalesInquiry.RmDocumentTypeAll.Value;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocumentNumber.Value = rmSalesInquiryForm.RmSalesInquiry.DocumentNumber.Value.ToString().Trim();
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyToDocNumber.Value = cashResponseInfo.ApplyToDocumentNumber.ToString().Trim();
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyToDocType.Value = Convert.ToInt16(cashResponseInfo.ApplyToDocumentType);
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CustomerNumber.Value = cashResponseInfo.ApplyToCustomerId.ToString().Trim();
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyAmountRemaining.Value = cashResponseInfo.Amount.AmountRemaining;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.OriginatingAmountRemaining.Value = cashResponseInfo.Amount.AmountRemaining + cashResponseInfo.Amount.ApplyAmount;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocAmount.Value = cashResponseInfo.Amount.DocumentAmount;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value = cashResponseInfo.Amount.ApplyAmount;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocCurrencyId.Value = cashResponseInfo.Amount.Currency;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CurrencyIndex.Value = rmSalesInquiryForm.RmSalesInquiry.CurrencyIndex.Value;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocAmtInOrigCurr.Value = cashResponseInfo.Amount.OriginatingCurrencyDocumentAmount;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyAmtInOrigCurr.Value = cashResponseInfo.Amount.ApplyAmountInOrignCurrency;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.ExchangeRate.Value = cashResponseInfo.Amount.ExchangeRate;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDate.Value = Convert.ToDateTime(cashResponseInfo.ApplyDate);
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyToDocCheckBox.Value = cashResponseInfo.IsSelected;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Status.Value = Convert.ToInt16(cashResponseInfo.StatusId);
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.SopTypeDatabase.Value = Convert.ToInt16(cashResponseInfo.SopTypeDatabase);

                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.UserId.Value = Dynamics.Globals.UserId.Value.ToString();
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.IsUpdated.Value = 0;

                                TableError tempErrorDate1 = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Save();
                            }

                            cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Release();
                            cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Close();
                        }
                    }

                    cashApplicationProcessInquiryForm.Procedures.CashApplicationProcessInquiryFillWindow.Invoke();

                    CalculateLineTotal(formName);
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertCashApplicationDetails Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        /// <summary>
        /// Cancel Button 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CashApplicationCancelButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                cashApplicationProcessForm.ApplyToOpenOrders.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CashApplicationCancelButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }

        }



        /// <summary>
        /// Cash Apply window change event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CashApplicationCashApplyToDocCheckBox_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();

            try
            {
                if (!isRejectCheckBox)
                {
                    if (cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.Status.Value == 0
                        && cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyToDocCheckBox.Value == false)
                    {
                        MessageBox.Show(Resources.STR_ERROR_APPLIED_TRX, Resources.STR_MESSAGE_TITLE);
                        isRejectCheckBox = true;
                        cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyToDocCheckBox.Value = true;
                        isRejectCheckBox = false;
                        return;
                    }

                    if (cashApplicationProcessForm.ApplyToOpenOrders.CashApplyUnappliedAmt.Value <= 0
                        && cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyToDocCheckBox.Value == true)
                    {
                        MessageBox.Show(Resources.STR_ERROR_SELECTAMOUNT, Resources.STR_MESSAGE_TITLE);
                        isRejectCheckBox = true;
                        cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyToDocCheckBox.Value = false;
                        isRejectCheckBox = false;
                        return;
                    }

                    string cashApplyDocumentNumber = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyDocumentNumber.Value.ToString().Trim();
                    int cashApplyDocumentType = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyDocumentType.Value;
                    string CashApplyToDocNumber = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyToDocNumber.Value.ToString().Trim();
                    int cashApplyToDocType = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyToDocType.Value;
                    decimal docAmount = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyAmount.Value;

                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Close();
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();

                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocumentNumber.Value = cashApplyDocumentNumber;
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocumentType.Value = Convert.ToInt16(cashApplyDocumentType);
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocNumber.Value = CashApplyToDocNumber;
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocType.Value = Convert.ToInt16(cashApplyToDocType);

                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Key = 1;
                    TableError tableError = cashApplicationProcessForm.Tables.CashApplyProcessTemp.Change();
                    if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        if (cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyToDocCheckBox.Value == true)
                        {
                            if (cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.OriginatingAmountRemaining.Value >= cashApplicationProcessForm.ApplyToOpenOrders.CashApplyUnappliedAmt.Value)
                            {
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmountRemaining.Value = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.OriginatingAmountRemaining.Value - cashApplicationProcessForm.ApplyToOpenOrders.CashApplyUnappliedAmt.Value;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyUnappliedAmt.Value;
                            }
                            else
                            {
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmountRemaining.Value = 0;
                                cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.OriginatingAmountRemaining.Value;
                            }
                        }
                        else
                        {
                            cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmountRemaining.Value = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.OriginatingAmountRemaining.Value;
                            cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value = 0;
                        }
                        cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocCheckBox.Value = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyToDocCheckBox.Value;

                        cashApplicationProcessForm.Tables.CashApplyProcessTemp.Save();
                    }
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Close();

                    decimal newDocAmount = 0;
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Close();
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocumentNumber.Value = cashApplyDocumentNumber;
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocumentType.Value = Convert.ToInt16(cashApplyDocumentType);
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocNumber.Value = CashApplyToDocNumber;
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocType.Value = Convert.ToInt16(cashApplyToDocType);
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Key = 1;
                    TableError tableError1 = cashApplicationProcessForm.Tables.CashApplyProcessTemp.Get();
                    if (tableError1 == TableError.NoError && tableError1 != TableError.EndOfTable)
                    {
                        newDocAmount = cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value;
                    }
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Close();
                    if (docAmount != newDocAmount)
                    {
                        cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyAmount.Value = newDocAmount;
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
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        void CashApplicationCashApplyAmount_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                decimal oldCashApplyAmount = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.OriginatingAmountRemaining.Value - cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyAmountRemaining.Value;
                decimal oldCashRemainingAmount = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.OriginatingAmountRemaining.Value - cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyAmount.Value;
                decimal oldUnappliedAmount = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyUnappliedAmt.Value;

                UpdateLineDetails(Resources.STR_RMCashReceiptForm);
                CalculateLineTotal(Resources.STR_RMCashReceiptForm);

                if (oldCashApplyAmount >= 0 && oldUnappliedAmount != cashApplicationProcessForm.ApplyToOpenOrders.CashApplyUnappliedAmt.Value && cashApplicationProcessForm.ApplyToOpenOrders.CashApplyUnappliedAmt.Value < 0)
                {
                    cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyAmount.Value = oldCashApplyAmount;
                }
                else if (oldCashApplyAmount > 0 && oldCashRemainingAmount < 0)
                {
                    cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyAmount.Value = oldCashApplyAmount;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CashApplicationCashApplyAmount_EnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyToDocCheckBox.Value == false)
                {
                    cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyToDocCheckBox.Focus();
                }
                else if (cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.Status.Value == 0
                    && cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyToDocCheckBox.Value == true)
                {
                    cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyToDocCheckBox.Focus();
                }
                else if (cashApplicationProcessForm.ApplyToOpenOrders.CashApplyUnappliedAmt.Value < 0)
                {
                    cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyToDocCheckBox.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }


        /// <summary>
        /// cash Application window save button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CashApplicationSaveButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (cashApplicationProcessForm.ApplyToOpenOrders.CashApplyUnappliedAmt.Value < 0)
                {
                    MessageBox.Show(Resources.STR_ERROR_UNAPPLIED_AMOUNT, Resources.STR_MESSAGE_TITLE);
                    return;
                }

                bool isLineSelected = false;
                bool isLineEmpty = false;

                ReceivablesRequest cashRequest = new ReceivablesRequest();
                ReceivablesResponse cashResponse = new ReceivablesResponse();
                CustomerInformation customerInfo = new CustomerInformation();
                AuditInformation auditInfo = new AuditInformation();

                List<ReceivableDetails> cashRequestList = new List<ReceivableDetails>();
                ReceivableDetails receivableInfo = null;
                Amount amountInfo = null;

                auditInfo.CompanyId = Dynamics.Globals.CompanyId.Value;
                auditInfo.ModifiedBy = Dynamics.Globals.UserId.Value;
                customerInfo.CustomerId = cashApplicationProcessForm.ApplyToOpenOrders.CustomerNumber.Value;

                cashApplicationProcessForm.Tables.CashApplyProcessTemp.Close();
                cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();
                TableError tableError = cashApplicationProcessForm.Tables.CashApplyProcessTemp.GetFirst();
                while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    amountInfo = new Amount();
                    receivableInfo = new ReceivableDetails();

                    amountInfo.AmountRemaining = cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmountRemaining.Value;
                    amountInfo.ApplyAmount = cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value;
                    amountInfo.DocumentAmount = cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocAmount.Value;
                    amountInfo.ExchangeRate = cashApplicationProcessForm.Tables.CashApplyProcessTemp.ExchangeRate.Value;
                    amountInfo.Currency = cashApplicationProcessForm.ApplyToOpenOrders.CurrencyId.Value;
                    amountInfo.OriginatingCurrencyDocumentAmount = cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocAmtInOrigCurr.Value;
                    amountInfo.ApplyAmountInOrignCurrency = cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmtInOrigCurr.Value;

                    receivableInfo.ApplyFromDocumentNumber = cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocumentNumber.Value;
                    receivableInfo.ApplyFromDocumentTypeId = cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocumentType.Value;
                    receivableInfo.ApplyToDocumentNumber = cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocNumber.Value;
                    receivableInfo.ApplyToDocumentType = cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocType.Value;
                    receivableInfo.ApplyToCustomerId = cashApplicationProcessForm.Tables.CashApplyProcessTemp.CustomerNumber.Value;

                    receivableInfo.DocumentCurrencyId = cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocCurrencyId.Value;
                    receivableInfo.ApplyDate = cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDate.Value;
                    receivableInfo.IsSelected = cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocCheckBox.Value;
                    receivableInfo.StatusId = cashApplicationProcessForm.Tables.CashApplyProcessTemp.Status.Value;

                    if (cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocCheckBox.Value == true)
                    {
                        receivableInfo.DocumentStatus = "Applied";
                        isLineSelected = true;
                        if (cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value == 0)
                        {
                            isLineEmpty = true;
                        }
                    }
                    else
                    {
                        receivableInfo.DocumentStatus = "UnApplied";
                    }

                    receivableInfo.Amount = amountInfo;
                    cashRequestList.Add(receivableInfo);

                    tableError = cashApplicationProcessForm.Tables.CashApplyProcessTemp.GetNext();
                }
                cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();
                cashApplicationProcessForm.Tables.CashApplyProcessTemp.Close();

                cashRequest.AuditInformation = auditInfo;
                cashRequest.CustomerInformation = customerInfo;
                cashRequest.ReceivableEntity = cashRequestList;


                if (isLineEmpty)
                {
                    MessageBox.Show(Resources.STR_ERROR_AMOUNT, Resources.STR_MESSAGE_TITLE);
                }
                else
                {
                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SalesOrderUpdate/SaveApplyToOpenOrder", cashRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            cashResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                            if (cashResponse.Status == ResponseStatus.Error)
                            {
                                MessageBox.Show("Error: " + cashResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CashApplicationSaveButton_ClickAfterOriginal Method (SaveApplyToOpenOrder): " + cashResponse.ErrorMessage.ToString());
                            }
                            else
                            {
                                if (cashResponse.Status == ResponseStatus.Success && !cashResponse.ValidationStatus)
                                {
                                    MessageBox.Show(Resources.STR_ERROR_ALREADYSAVED, Resources.STR_MESSAGE_TITLE);
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CashApplicationSaveButton_ClickAfterOriginal Method (SaveApplyToOpenOrder): " + Resources.STR_ERROR_ALREADYSAVED);

                                    if (rmSalesEntryForm.RmSalesEntry.IsOpen
                                           && !String.IsNullOrEmpty(rmSalesEntryForm.RmSalesEntry.RmDocumentNumberWork.Value.ToString().Trim())
                                           && !String.IsNullOrEmpty(rmSalesEntryForm.RmSalesEntry.CustomerNumber.Value.ToString().Trim())
                                           && (rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value == 7
                                               || rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value == 8))
                                    {
                                        InsertCashApplicationDetails(Resources.STR_ReceivingEntryForm);
                                    }
                                    else if ((rmCashReceiptsForm.RmCashReceipts.IsOpen)
                                        && !String.IsNullOrEmpty(rmCashReceiptsForm.RmCashReceipts.LocalStR17DocNumber.Value)
                                        && !String.IsNullOrEmpty(rmCashReceiptsForm.RmCashReceipts.CustomerNumber.Value))
                                    {
                                        InsertCashApplicationDetails(Resources.STR_RMCashReceiptForm);
                                    }
                                    else if ((lockBoxTransactionsForm.PalbTransactions.IsOpen)
                                        && !String.IsNullOrEmpty(lockBoxTransactionsForm.PalbTransactions.CashScroll.DocumentNumber.Value)
                                        && !String.IsNullOrEmpty(lockBoxTransactionsForm.PalbTransactions.CashScroll.CustomerNumber.Value))
                                    {
                                        InsertCashApplicationDetails(Resources.STR_LockBoxTransactionsForm);
                                    }
                                }
                                else
                                {
                                    cashApplicationProcessForm.ApplyToOpenOrders.Close();
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Error: Data does not save into custom details for cash application", Resources.STR_MESSAGE_TITLE);
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not save into custom details for cash application");
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
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        /// <summary>
        /// Open Apply to open order inquiry window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ApplyToOpenOrdersInquiry_OpenBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (RegisterCashApplicationInquiry == false)
                {
                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.OkButton.ClickAfterOriginal += new EventHandler(ApplyToOpenOrdersInquiryOkButton_ClickAfterOriginal);
                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CloseAfterOriginal += new EventHandler(ApplyToOpenOrdersInquiry_CloseAfterOriginal);

                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyToDocCheckBox.Change += new EventHandler(CashApplicationInquiryCashApplyToDocCheckBox_Change);
                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyAmount.Change += new EventHandler(CashApplicationInquiryCashApplyAmount_Change);
                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(CashApplicationInquiryCashApplyAmount_EnterBeforeOriginal);

                    RegisterCashApplicationInquiry = true;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ApplyToOpenOrdersInquiry_OpenBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("ApplyToOpenOrdersInquiry_OpenBeforeOriginal window Registration Failed : " + ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        /// <summary>
        /// Event fired when closing the cash apply window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ApplyToOpenOrdersInquiry_CloseAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                expansionWindowInquiryType = string.Empty;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ApplyToOpenOrdersInquiry_CloseAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }

        }

        /// <summary>
        /// After Open window events..
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ApplyToOpenOrdersInquiry_OpenAfterOriginal(object sender, EventArgs e)
        {
            InsertCashApplicationDetails(expansionWindowInquiryType);

            cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.ScrollShrinkSwitch.Value = 1;
            cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.LocalShrinkButton.RunValidate();
        }

        /// <summary>
        /// Clear Cash Application window Temp table field...
        /// </summary>
        /// <param name="formName"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        private void ClearApplyOpenOrderTemp(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_RMCashReceiptForm || formName == Resources.STR_ReceivingEntryForm || formName == Resources.STR_LockBoxTransactionsForm)
                {
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Close();
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();
                    TableError tempError = cashApplicationProcessForm.Tables.CashApplyProcessTemp.ChangeFirst();
                    while (tempError == TableError.NoError && tempError != TableError.EndOfTable)
                    {
                        cashApplicationProcessForm.Tables.CashApplyProcessTemp.Remove();
                        tempError = cashApplicationProcessForm.Tables.CashApplyProcessTemp.ChangeNext();
                    }
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Close();
                }
                else if (formName == Resources.STR_RMCashReceiptinquiryForm || formName == Resources.STR_ReceivingInquiryForm)
                {
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Close();
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Release();
                    TableError tempError = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.ChangeFirst();
                    while (tempError == TableError.NoError && tempError != TableError.EndOfTable)
                    {
                        cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Remove();
                        tempError = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.ChangeNext();
                    }
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Release();
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Close();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearApplyOpenOrderTemp Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        /// <summary>
        /// Cash Apply window change event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CashApplicationInquiryCashApplyToDocCheckBox_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();

            try
            {
                if (!isRejectCheckBox)
                {
                    if (cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.Status.Value == 0
                        && cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyToDocCheckBox.Value == false)
                    {
                        MessageBox.Show(Resources.STR_ERROR_APPLIED_TRX, Resources.STR_MESSAGE_TITLE);
                        isRejectCheckBox = true;
                        cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyToDocCheckBox.Value = true;
                        isRejectCheckBox = false;
                        return;
                    }

                    if (cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyUnappliedAmt.Value <= 0
                        && cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyToDocCheckBox.Value == true)
                    {
                        MessageBox.Show(Resources.STR_ERROR_SELECTAMOUNT, Resources.STR_MESSAGE_TITLE);
                        isRejectCheckBox = true;
                        cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyToDocCheckBox.Value = false;
                        isRejectCheckBox = false;
                        return;
                    }

                    string cashApplyDocumentNumber = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyDocumentNumber.Value.ToString().Trim();
                    int cashApplyDocumentType = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyDocumentType.Value;
                    string CashApplyToDocNumber = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyToDocNumber.Value.ToString().Trim();
                    int cashApplyToDocType = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyToDocType.Value;
                    decimal docAmount = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyAmount.Value;

                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Close();
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Release();

                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocumentNumber.Value = cashApplyDocumentNumber;
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocumentType.Value = Convert.ToInt16(cashApplyDocumentType);
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyToDocNumber.Value = CashApplyToDocNumber;
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyToDocType.Value = Convert.ToInt16(cashApplyToDocType);

                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Key = 1;
                    TableError tableError = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Change();
                    if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        if (cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyToDocCheckBox.Value == true)
                        {
                            if (cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.OriginatingAmountRemaining.Value >= cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyUnappliedAmt.Value)
                            {
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyAmountRemaining.Value = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.OriginatingAmountRemaining.Value - cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyUnappliedAmt.Value;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyUnappliedAmt.Value;
                            }
                            else
                            {
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyAmountRemaining.Value = 0;
                                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.OriginatingAmountRemaining.Value;
                            }
                        }
                        else
                        {
                            cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyAmountRemaining.Value = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.OriginatingAmountRemaining.Value;
                            cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value = 0;
                        }
                        cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyToDocCheckBox.Value = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyToDocCheckBox.Value;

                        cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Save();
                    }
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Release();
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Close();

                    decimal newDocAmount = 0;
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Close();
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Release();
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocumentNumber.Value = cashApplyDocumentNumber;
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocumentType.Value = Convert.ToInt16(cashApplyDocumentType);
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyToDocNumber.Value = CashApplyToDocNumber;
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyToDocType.Value = Convert.ToInt16(cashApplyToDocType);
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Key = 1;
                    TableError tableError1 = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Get();
                    if (tableError1 == TableError.NoError && tableError1 != TableError.EndOfTable)
                    {
                        newDocAmount = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value;
                    }
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Release();
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Close();
                    if (docAmount != newDocAmount)
                    {
                        cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyAmount.Value = newDocAmount;
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
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        void CashApplicationInquiryCashApplyAmount_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                decimal oldCashApplyAmount = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.OriginatingAmountRemaining.Value - cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyAmountRemaining.Value;
                decimal oldCashRemainingAmount = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.OriginatingAmountRemaining.Value - cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyAmount.Value;
                decimal oldUnappliedAmount = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyUnappliedAmt.Value;

                UpdateLineDetails(Resources.STR_RMCashReceiptinquiryForm);
                CalculateLineTotal(Resources.STR_RMCashReceiptinquiryForm);

                if (oldCashApplyAmount >= 0 && oldUnappliedAmount != cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyUnappliedAmt.Value && cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyUnappliedAmt.Value < 0)
                {
                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyAmount.Value = oldCashApplyAmount;
                }
                else if (oldCashApplyAmount > 0 && oldCashRemainingAmount < 0)
                {
                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyAmount.Value = oldCashApplyAmount;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CashApplicationInquiryCashApplyAmount_EnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyToDocCheckBox.Value == false)
                {
                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyToDocCheckBox.Focus();
                }
                else if (cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.Status.Value == 0
                    && cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyToDocCheckBox.Value == true)
                {
                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyToDocCheckBox.Focus();
                }
                else if (cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyUnappliedAmt.Value < 0)
                {
                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyToDocCheckBox.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                logMessage.AppendLine(DateTime.Now + " Error : " + ex.ToString());
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        /// <summary>
        /// Ok Button 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ApplyToOpenOrdersInquiryOkButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyUnappliedAmt.Value < 0)
                {
                    MessageBox.Show(Resources.STR_ERROR_UNAPPLIED_AMOUNT, Resources.STR_MESSAGE_TITLE);
                    return;
                }

                bool isLineSelected = false;
                bool isLineEmpty = false;

                ReceivablesRequest cashRequest = new ReceivablesRequest();
                ReceivablesResponse cashResponse = new ReceivablesResponse();
                CustomerInformation customerInfo = new CustomerInformation();
                AuditInformation auditInfo = new AuditInformation();

                List<ReceivableDetails> cashRequestList = new List<ReceivableDetails>();
                ReceivableDetails receivableInfo = null;
                Amount amountInfo = null;

                auditInfo.CompanyId = Dynamics.Globals.CompanyId.Value;
                auditInfo.ModifiedBy = Dynamics.Globals.UserId.Value;
                customerInfo.CustomerId = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CustomerNumber.Value;

                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Close();
                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Release();
                TableError tableError = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.GetFirst();
                while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    amountInfo = new Amount();
                    receivableInfo = new ReceivableDetails();

                    amountInfo.AmountRemaining = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyAmountRemaining.Value;
                    amountInfo.ApplyAmount = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value;
                    amountInfo.DocumentAmount = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocAmount.Value;
                    amountInfo.ExchangeRate = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.ExchangeRate.Value;
                    if (rmCashReceiptInquiryForm.RmCashReceiptsInquiry.CurrencyView.Value == 1)
                    {   
                        if(Dynamics.Globals.CompanyId.Value==1)
                        {
                            amountInfo.Currency = "Z-US$";
                        }
                        else if (Dynamics.Globals.CompanyId.Value == 2)
                        {
                            amountInfo.Currency = "EUR";
                        }
                        
                    }
                    else
                    {
                        amountInfo.Currency = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CurrencyId.Value;
                    }                     

                    receivableInfo.ApplyFromDocumentNumber = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocumentNumber.Value;
                    receivableInfo.ApplyFromDocumentTypeId = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocumentType.Value;
                    receivableInfo.ApplyToDocumentNumber = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyToDocNumber.Value;
                    receivableInfo.ApplyToDocumentType = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyToDocType.Value;
                    receivableInfo.ApplyToCustomerId = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CustomerNumber.Value;

                    receivableInfo.DocumentCurrencyId = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CurrencyId.Value;
                    //receivableInfo.DocumentCurrencyId = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocCurrencyId.Value;
                    receivableInfo.ApplyDate = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDate.Value;
                    receivableInfo.IsSelected = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyToDocCheckBox.Value;
                    receivableInfo.StatusId = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Status.Value;

                    if (cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyToDocCheckBox.Value == true)
                    {
                        receivableInfo.DocumentStatus = "Applied";
                        isLineSelected = true;
                        if (cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value == 0)
                        {
                            isLineEmpty = true;
                        }
                    }
                    else
                    {
                        receivableInfo.DocumentStatus = "UnApplied";
                    }

                    receivableInfo.Amount = amountInfo;
                    cashRequestList.Add(receivableInfo);

                    tableError = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.GetNext();
                }
                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Release();
                cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Close();

                cashRequest.AuditInformation = auditInfo;
                cashRequest.CustomerInformation = customerInfo;
                cashRequest.ReceivableEntity = cashRequestList;


                if (isLineEmpty)
                {
                    MessageBox.Show(Resources.STR_ERROR_AMOUNT, Resources.STR_MESSAGE_TITLE);
                }
                else
                {
                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SalesOrderUpdate/SaveApplyToOpenOrder", cashRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            cashResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                            if (cashResponse.Status == ResponseStatus.Error)
                            {
                                MessageBox.Show("Error: " + cashResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ApplyToOpenOrdersInquiryOkButton_ClickAfterOriginal Method (SaveApplyToOpenOrder): " + cashResponse.ErrorMessage.ToString());
                            }
                            else
                            {
                                if (cashResponse.Status == ResponseStatus.Success && !cashResponse.ValidationStatus)
                                {
                                    MessageBox.Show(Resources.STR_ERROR_ALREADYSAVED, Resources.STR_MESSAGE_TITLE);
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ApplyToOpenOrdersInquiryOkButton_ClickAfterOriginal Method (SaveApplyToOpenOrder): " + Resources.STR_ERROR_ALREADYSAVED);

                                    if (rmSalesInquiryForm.RmSalesInquiry.IsOpen
                                           && !String.IsNullOrEmpty(rmSalesInquiryForm.RmSalesInquiry.DocumentNumber.Value.ToString().Trim())
                                           && !String.IsNullOrEmpty(rmSalesInquiryForm.RmSalesInquiry.CustomerNumber.Value.ToString().Trim())
                                           && (rmSalesInquiryForm.RmSalesInquiry.RmDocumentTypeAll.Value == 7
                                               || rmSalesInquiryForm.RmSalesInquiry.RmDocumentTypeAll.Value == 8))
                                    {
                                        InsertCashApplicationDetails(Resources.STR_ReceivingInquiryForm);
                                    }
                                    else if ((rmCashReceiptInquiryForm.RmCashReceiptsInquiry.IsOpen)
                                        && !String.IsNullOrEmpty(rmCashReceiptInquiryForm.RmCashReceiptsInquiry.DocumentNumber.Value)
                                        && !String.IsNullOrEmpty(rmCashReceiptInquiryForm.RmCashReceiptsInquiry.CustomerNumber.Value))
                                    {
                                        InsertCashApplicationDetails(Resources.STR_RMCashReceiptinquiryForm);
                                    }
                                }
                                else
                                {
                                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.Close();
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Error: Data does not save into custom details for cash application", Resources.STR_MESSAGE_TITLE);
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not save into custom details for cash application");
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
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        /// <summary>
        /// Delete Payment
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DeleteCashReceipt_InvokeAfterOriginal(object sender, RmCashReceiptsForm.DeleteCashReceiptProcedure.InvokeEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!String.IsNullOrEmpty(e.inParam1.ToString().Trim()))
                {
                    ReceivablesRequest cashRequest = new ReceivablesRequest();
                    ReceivablesResponse cashResponse = new ReceivablesResponse();
                    ReceivablesHeader receivablesHeader = new ReceivablesHeader();
                    AuditInformation auditInfo = new AuditInformation();

                    auditInfo.CompanyId = Dynamics.Globals.CompanyId.Value;
                    auditInfo.ModifiedBy = Dynamics.Globals.UserId.Value;
                    receivablesHeader.DocumentNumber = e.inParam1.ToString().Trim(); // Document Number
                    receivablesHeader.DocumentType = 9;
                    cashRequest.ReceivablesHeader = receivablesHeader;
                    cashRequest.AuditInformation = auditInfo;

                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SalesOrderUpdate/DeleteApplyToOpenOrder", cashRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            cashResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                            if (cashResponse.Status == ResponseStatus.Error)
                            {
                                MessageBox.Show("Error: " + cashResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In DeleteCashReceipt_InvokeAfterOriginal Method (DeleteApplyToOpenOrder): " + cashResponse.ErrorMessage.ToString());
                            }
                        }
                        else
                        {
                            MessageBox.Show("Error: Data does not delete from custom details for cash application", Resources.STR_MESSAGE_TITLE);
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not delete from custom details for cash application");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In DeleteCashReceipt_InvokeAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        /// <summary>
        /// CurrencyId enter field event...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RmCashReceiptsCurrencyId_EnterAfterOriginal(object sender, EventArgs e)
        {
            originalCurrencyID = rmCashReceiptsForm.RmCashReceipts.CurrencyId.Value.ToString().Trim();
        }

        /// <summary>
        /// CurrencyId enter field event...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RmSalesEntryCurrencyId_EnterAfterOriginal(object sender, EventArgs e)
        {
            originalCurrencyID = rmSalesEntryForm.RmSalesEntry.CurrencyId.Value.ToString().Trim();
        }

        /// <summary>
        ///  CurrencyId leave after original event...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RmCashReceiptsCurrencyId_LeaveAfterOriginal(object sender, EventArgs e)
        {
            originalCurrencyID = string.Empty;
        }

        /// <summary>
        ///  CurrencyId leave after original event...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RmSalesEntryCurrencyId_LeaveAfterOriginal(object sender, EventArgs e)
        {
            originalCurrencyID = string.Empty;
        }


        /// <summary>
        /// Implemented currency change event for cash receipt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RmCashReceiptsMcCurrencyVerification_InvokeBeforeOriginal(object sender, RmCashReceiptsForm.McCurrencyVerificationProcedure.InvokeEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();

            try
            {
                if (rmCashReceiptsForm.RmCashReceipts.IsOpen
                                      && !String.IsNullOrEmpty(rmCashReceiptsForm.RmCashReceipts.LocalStR17DocNumber.Value)
                                      && !String.IsNullOrEmpty(rmCashReceiptsForm.RmCashReceipts.CustomerNumber.Value))
                {
                    if (!string.IsNullOrEmpty(originalCurrencyID)
                        && !string.IsNullOrEmpty(rmCashReceiptsForm.RmCashReceipts.CurrencyId.Value.ToString().Trim())
                        && originalCurrencyID != rmCashReceiptsForm.RmCashReceipts.CurrencyId.Value.ToString().Trim())
                    {
                        int SprocStatus = 0;
                        short IsValid = 3;

                        ChemPointSalesExt.Procedures.ValidateAppliedOpenSalesDocumentAmount.Invoke
                            (ref SprocStatus,
                                rmCashReceiptsForm.RmCashReceipts.LocalStR17DocNumber.Value.ToString().Trim(),
                                9,
                                rmCashReceiptsForm.RmCashReceipts.OriginatingOriginalTrxAmount.Value,
                                ref IsValid);

                        if (IsValid != 3)
                        {
                            MessageBox.Show(Resources.STR_ERROR_APPLIED_CURRENCY.Trim() + " " + originalCurrencyID + ".", Resources.STR_MESSAGE_TITLE);
                            rmCashReceiptsForm.RmCashReceipts.LocalPrevCurrency.Value = originalCurrencyID;
                            rmCashReceiptsForm.RmCashReceipts.CurrencyId.Value = originalCurrencyID;
                            ChemPointSalesExt.Procedures.EdiRejectScript.Invoke();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In RmCashReceiptsMcCurrencyVerification_InvokeBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        /// <summary>
        /// Implemented currency change event for rm sales entry receipt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RmSalesEntryMcCurrVerification_InvokeBeforeOriginal(object sender, RmSalesEntryForm.McCurrVerificationProcedure.InvokeEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();

            try
            {
                if (rmSalesEntryForm.RmSalesEntry.IsOpen
                       && !String.IsNullOrEmpty(rmSalesEntryForm.RmSalesEntry.RmDocumentNumberWork.Value.ToString().Trim())
                       && !String.IsNullOrEmpty(rmSalesEntryForm.RmSalesEntry.CustomerNumber.Value.ToString().Trim())
                       && (rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value == 7
                           || rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value == 8))
                {
                    if (!string.IsNullOrEmpty(originalCurrencyID)
                        && !string.IsNullOrEmpty(rmSalesEntryForm.RmSalesEntry.CurrencyId.Value.ToString().Trim())
                        && originalCurrencyID != rmSalesEntryForm.RmSalesEntry.CurrencyId.Value.ToString().Trim())
                    {
                        int SprocStatus = 0;
                        short IsValid = 3;

                        ChemPointSalesExt.Procedures.ValidateAppliedOpenSalesDocumentAmount.Invoke
                            (ref SprocStatus,
                                rmSalesEntryForm.RmSalesEntry.RmDocumentNumberWork.Value.ToString().Trim(),
                                rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value,
                                rmSalesEntryForm.RmSalesEntry.OriginatingDocumentAmount.Value,
                                ref IsValid);

                        if (IsValid != 3)
                        {
                            MessageBox.Show(Resources.STR_ERROR_APPLIED_CURRENCY.Trim() + " " + originalCurrencyID + ".", Resources.STR_MESSAGE_TITLE);
                            rmSalesEntryForm.RmSalesEntry.LocalPrevCurrency.Value = originalCurrencyID;
                            rmSalesEntryForm.RmSalesEntry.CurrencyId.Value = originalCurrencyID;
                            ChemPointSalesExt.Procedures.EdiRejectScript.Invoke();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In RmSalesEntryMcCurrVerification_InvokeBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        void ChangeApplyAmt_InvokeBeforeOriginal(object sender, RmCashApplyForm.ChangeApplyAmtFunction.InvokeEventArgs e)
        {
            if (!String.IsNullOrEmpty(rmCashApplyForm.RmApplyDocument.DocumentNumber.Value.ToString().Trim())
                        && !String.IsNullOrEmpty(rmCashApplyForm.RmApplyDocument.CustomerNumber.Value.ToString().Trim())
                        && rmCashApplyForm.RmApplyDocument.RmAppliedDocumentScroll.DebitApplyAmt.Value > 0
                        && e.inParam1 > 0) // difference should be greater than zero
            {
                if (rmCashApplyForm.RmApplyDocument.DocumentType.Value == 1 || rmCashApplyForm.RmApplyDocument.DocumentType.Value == 2 || rmCashApplyForm.RmApplyDocument.DocumentType.Value == 3)
                {
                    short DocumentType = 0;
                    switch (rmCashApplyForm.RmApplyDocument.DocumentType.Value)
                    {
                        case 1:
                            DocumentType = 7;
                            break;
                        case 2:
                            DocumentType = 8;
                            break;
                        case 3:
                            DocumentType = 9;
                            break;
                        default:
                            break;
                    }

                    int SprocStatus = 0;
                    short IsValid = 1;

                    ChemPointSalesExt.Procedures.ValidateAlreadyAppliedAmount.Invoke
                     (ref SprocStatus,
                         rmCashApplyForm.RmApplyDocument.DocumentNumber.Value.ToString().Trim(),
                         DocumentType,
                         rmCashApplyForm.RmApplyDocument.RmAppliedDocumentScroll.DocumentNumber.Value.ToString().Trim(),
                         rmCashApplyForm.RmApplyDocument.RmAppliedDocumentScroll.RmDocumentTypeAll.Value,
                          rmCashApplyForm.RmApplyDocument.CreditOriginalTrxAmt.Value,
                         rmCashApplyForm.RmApplyDocument.RmAppliedDocumentScroll.DebitApplyAmt.Value,
                         ref IsValid);

                    if (IsValid == 0)
                    {
                        rmCashApplyForm.RmApplyDocument.RmAppliedDocumentScroll.LocalApplySelect.Value = false;
                        rmCashApplyForm.RmApplyDocument.RmAppliedDocumentScroll.LocalApplySelect.RunValidate();

                        MessageBox.Show(Resources.STR_ERROR_ALREADY_APPLIED_AMOUNT.Trim(), Resources.STR_MESSAGE_TITLE);
                        rmCashApplyForm.RmApplyDocument.RmAppliedDocumentScroll.DebitApplyAmt.Focus();
                    }
                    else if (IsValid == 2)
                    {
                        rmCashApplyForm.RmApplyDocument.RmAppliedDocumentScroll.LocalApplySelect.Value = false;
                        rmCashApplyForm.RmApplyDocument.RmAppliedDocumentScroll.LocalApplySelect.RunValidate();

                        MessageBox.Show(Resources.STR_ERROR_ALREADY_APPLIED_TRX_AMOUNT.Trim(), Resources.STR_MESSAGE_TITLE);
                        rmCashApplyForm.RmApplyDocument.RmAppliedDocumentScroll.DebitApplyAmt.Focus();
                    }
                }
            }
        }

        /// <summary>
        /// Triggers the ApplySelect Change event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RmAppliedDocumentScrollLocalApplySelectChange(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (rmSalesEntryForm.RmSalesEntry.IsOpen
                        && !String.IsNullOrEmpty(rmSalesEntryForm.RmSalesEntry.RmDocumentNumberWork.Value.ToString().Trim())
                        && !String.IsNullOrEmpty(rmSalesEntryForm.RmSalesEntry.CustomerNumber.Value.ToString().Trim())
                        && (rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value == 3
                            || rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value == 7
                            || rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value == 8))
                {
                    if ((rmCashApplyForm.RmApplyDocument.RmAppliedDocumentScroll.IsChanged == true) || (rmCashApplyForm.RmApplyDocument.IsChanged == true))
                        rmSalesEntryForm.RmSalesEntry.IsChanged = true;
                }
                else if (rmCashReceiptsForm.RmCashReceipts.IsOpen
                    && !String.IsNullOrEmpty(rmCashReceiptsForm.RmCashReceipts.LocalStR17DocNumber.Value)
                    && !String.IsNullOrEmpty(rmCashReceiptsForm.RmCashReceipts.CustomerNumber.Value))
                {
                    if ((rmCashApplyForm.RmApplyDocument.RmAppliedDocumentScroll.IsChanged == true) || (rmCashApplyForm.RmApplyDocument.IsChanged == true))
                        rmCashReceiptsForm.RmCashReceipts.IsChanged = true;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In RmAppliedDocumentScrollLocalApplySelectChange Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Method to set the Is changes flag on RM Sales Entry window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void rmCashApplyFormApplyAndUnapplyButtonClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (rmSalesEntryForm.RmSalesEntry.IsOpen
                        && !String.IsNullOrEmpty(rmSalesEntryForm.RmSalesEntry.RmDocumentNumberWork.Value.ToString().Trim())
                        && !String.IsNullOrEmpty(rmSalesEntryForm.RmSalesEntry.CustomerNumber.Value.ToString().Trim())
                        && (rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value == 3
                            || rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value == 7
                            || rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value == 8))
                {
                    rmSalesEntryForm.RmSalesEntry.IsChanged = true;
                }
                else if ((rmCashReceiptsForm.RmCashReceipts.IsOpen)
                    && !String.IsNullOrEmpty(rmCashReceiptsForm.RmCashReceipts.LocalStR17DocNumber.Value)
                    && !String.IsNullOrEmpty(rmCashReceiptsForm.RmCashReceipts.CustomerNumber.Value))
                {
                    rmCashReceiptsForm.RmCashReceipts.IsChanged = true;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In rmCashApplyFormApplyAndUnapplyButtonClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Event fired when closing the cash apply window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void rmCashApplyFormRmApplyDocument_CloseAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                bool isChanged = false;
                string documentNumber = "";
                short documentType = 0;
                string customerNumber = "";

                if (!String.IsNullOrEmpty(rmCashApplyForm.RmApplyDocument.DocumentNumber.Value.ToString().Trim())
                         && !String.IsNullOrEmpty(rmCashApplyForm.RmApplyDocument.CustomerNumber.Value.ToString().Trim()))
                {
                    if (rmCashApplyForm.RmApplyDocument.DocumentType.Value == 1 || rmCashApplyForm.RmApplyDocument.DocumentType.Value == 2 || rmCashApplyForm.RmApplyDocument.DocumentType.Value == 3)
                    {
                        short DocumentType = 0;
                        switch (rmCashApplyForm.RmApplyDocument.DocumentType.Value)
                        {
                            case 1:
                                DocumentType = 7;
                                break;
                            case 2:
                                DocumentType = 8;
                                break;
                            case 3:
                                DocumentType = 9;
                                break;
                            default:
                                break;
                        }

                        if (DocumentType != 0)
                        {
                            isChanged = true;
                            documentNumber = rmCashApplyForm.RmApplyDocument.DocumentNumber.Value.ToString().Trim();
                            documentType = DocumentType;
                            customerNumber = rmCashApplyForm.RmApplyDocument.CustomerNumber.Value.ToString().Trim();
                        }
                    }
                }


                if (isChanged)
                {
                    Chempoint.GP.Model.Interactions.Sales.ReceivablesRequest cashRequest = new Chempoint.GP.Model.Interactions.Sales.ReceivablesRequest();
                    Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse cashResponse = new Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse();
                    CustomerInformation customerInfo = new CustomerInformation();
                    AuditInformation auditInfo = new AuditInformation();
                    ReceivablesHeader receivableInfo = new ReceivablesHeader();

                    auditInfo.CompanyId = Dynamics.Globals.CompanyId.Value;
                    auditInfo.ModifiedBy = Dynamics.Globals.UserId.Value;
                    customerInfo.CustomerId = customerNumber;
                    receivableInfo.DocumentNumber = documentNumber;
                    receivableInfo.DocumentType = documentType;
                    cashRequest.ReceivablesHeader = receivableInfo;
                    cashRequest.AuditInformation = auditInfo;
                    cashRequest.CustomerInformation = customerInfo;

                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SalesOrderUpdate/UpdateApplyToOpenOrder", cashRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            cashResponse = response.Result.Content.ReadAsAsync<Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse>().Result;
                            if (cashResponse.Status == Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error)
                            {
                                if (!string.IsNullOrWhiteSpace(cashResponse.ErrorMessage))
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Unknown error occurred to update details due to " + cashResponse.ErrorMessage + ". Please contact IT.");
                                    MessageBox.Show("Unknown error occurred to update details due to " + cashResponse.ErrorMessage + ". Please contact IT.", Resources.STR_MESSAGE_TITLE);
                                }
                                else
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Unknown error occurred to update details. Please contact IT.");
                                    MessageBox.Show("Unknown error occurred to fetch details. Please contact IT.", Resources.STR_MESSAGE_TITLE);
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error occurred while updating apply to open orders details. Please contact IT.");
                            MessageBox.Show("Error occurred while updating apply to open orders details. Please contact IT.", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In rmCashApplyFormRmApplyDocument_CloseAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        #endregion Cash Application

        #region Receivables Trx Details Entry

        /// <summary>
        /// Additional Menu for ReceivablesTransactionDetails Window
        /// </summar
        /// <param name="sender">Sender param</param>
        /// <param name="e">Event Arguments param</param>
        private void ReceivablesTransactionDetails(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (string.IsNullOrEmpty(rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value.ToString())
                            || string.IsNullOrEmpty(rmSalesEntryForm.RmSalesEntry.CustomerNumber.Value.ToString()))
                {
                    MessageBox.Show(Resources.STR_ERROR_REQUIREDFIELDS, Resources.STR_MESSAGE_TITLE);
                }
                else if ((rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value != 3)
                            && (rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value != 7))
                {
                    MessageBox.Show(Resources.STR_ERROR_VALIDRMDocType, Resources.STR_MESSAGE_TITLE);
                }
                else
                {
                    if (cpReceivablesTransactionDetailsForm == null)
                    {
                        cpReceivablesTransactionDetailsForm = ChemPointSalesExt.Forms.CpReceivablesTransactionDetails;
                    }
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.Open();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ReceivablesTransactionDetails Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        ///  ReceivablesInquiryDetails Window
        /// </summary>
        private void ReceivablesInquiryDetails(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (string.IsNullOrEmpty(rmSalesInquiryForm.RmSalesInquiry.RmDocumentTypeAll.Value.ToString())
                            || string.IsNullOrEmpty(rmSalesInquiryForm.RmSalesInquiry.CustomerNumber.Value.ToString()))
                {
                    MessageBox.Show(Resources.STR_ERROR_REQUIREDFIELDS, Resources.STR_MESSAGE_TITLE);
                }
                else if ((rmSalesInquiryForm.RmSalesInquiry.RmDocumentTypeAll.Value != 3)
                            && (rmSalesInquiryForm.RmSalesInquiry.RmDocumentTypeAll.Value != 7))
                {
                    MessageBox.Show(Resources.STR_ERROR_VALIDRMDocType, Resources.STR_MESSAGE_TITLE);
                }
                else
                {
                    if (cpReceivablesTransactionInquiryForm == null)
                    {
                        cpReceivablesTransactionInquiryForm = ChemPointSalesExt.Forms.CpReceivablesTransactionInquiry;
                    }
                    cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.Open();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ReceivablesInquiryDetails Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Event triggerd before opening the inquiry window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CpReceivablesTransactionInquiry_OpenBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (RegisterRmDetailsInquiryMgmt == false)
                {
                    cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.OkButton.ClickAfterOriginal += new EventHandler(CpReceivablesTransactionInquiryOkButton_ClickAfterOriginal);
                    RegisterRmDetailsInquiryMgmt = true;
                }

                originalTypeIdInquiry = 0;

                //Load the TypeId Combo
                cpReceivablesTransactionInquiryForm.Procedures.CpReceivablesTransactionInquiryFormFillType.Invoke();

                if (rmSalesInquiryForm.RmSalesInquiry.RmDocumentTypeAll.Value == 3)
                {
                    cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.LocalCpDebitCreditValue.Value = Resources.STR_DEBIT_VALUE;
                    cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.LocalCpTotalDebitCreditAmount.Value = Resources.STR_TOTAL_DEBIT_AMOUNT;
                }
                else if (rmSalesInquiryForm.RmSalesInquiry.RmDocumentTypeAll.Value == 7)
                {
                    cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.LocalCpDebitCreditValue.Value = Resources.STR_CREDIT_VALUE;
                    cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.LocalCpTotalDebitCreditAmount.Value = Resources.STR_TOTAL_CREDIT_AMOUNT;
                }

                //Display the volume column in Lb for NA and Kg for EU
                if (Dynamics.Globals.CompanyId.Value == 1)
                {
                    cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.LocalVolume.Value = Resources.STR_VOLUME_LB;
                }
                else if (Dynamics.Globals.CompanyId.Value == 2)
                {
                    cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.LocalVolume.Value = Resources.STR_VOLUME_KG;
                }

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CpReceivablesTransactionInquiry_OpenBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }

        /// <summary>
        /// Method fired after opening the Receivables Transaction window.
        /// </summary>
        /// <param name="sender">Sender param</param>
        /// <param name="e">Event Arguments param</param>
        void CpReceivablesTransactionInquiry_OpenAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();

            try
            {
                InsertReceivablesTransactionDetails(Resources.STR_ReceivablesInquiryForm);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CpReceivablesTransactionInquiry_OpenAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;

                cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.ScrollShrinkSwitch.Value = 1;
                cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.LocalShrinkButton.RunValidate();
            }
        }

        /// <summary>
        /// Event triggered before opening the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CpReceivablesTransactionDetails_OpenBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (RegisterRmDetailsMgmt == false)
                {
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.SaveButton.ClickAfterOriginal += new EventHandler(SaveButton_ClickAfterOriginal);
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.CpDistribute.ClickAfterOriginal += new EventHandler(CpDistribute_ClickAfterOriginal);
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.CancelButton.ClickAfterOriginal += new EventHandler(CancelButton_ClickAfterOriginal);

                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalType.Change += new EventHandler(LocalType_Change);
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.IsSelected.Change += new EventHandler(CpReceivablesTransactionDetailsIsSelected_Change);
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.DocumentAmount.Change += new EventHandler(CpReceivablesTransactionDetailsDocumentAmount_Change);
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.SupportId.Change += new EventHandler(CpReceivablesTransactionDetailsSupportIdAndIncidentId_Change);
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.IncidentId.Change += new EventHandler(CpReceivablesTransactionDetailsSupportIdAndIncidentId_Change);

                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.DocumentAmount.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(CpReceivablesTransactionDetailsDocumentAmount_EnterBeforeOriginal);
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.SupportId.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(CpReceivablesTransactionDetailsDocumentAmount_EnterBeforeOriginal);
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.IncidentId.EnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(CpReceivablesTransactionDetailsDocumentAmount_EnterBeforeOriginal);

                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.LineEnterBeforeOriginal += new System.ComponentModel.CancelEventHandler(ReceivablesTransactionDetailScroll_LineEnterBeforeOriginal);
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.CheckAll.ValidateAfterOriginal += CheckAll_Change;
                    RegisterRmDetailsMgmt = true;
                }

                originalTypeIdEntry = 0;

                //Load the TypeId Combo
                cpReceivablesTransactionDetailsForm.Procedures.CpReceivablesTransactionDetailsFormFillType.Invoke();

                if (rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value == 3)
                {
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalCpDebitCreditValue.Value = Resources.STR_DEBIT_VALUE;
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalCpTotalDebitCreditAmount.Value = Resources.STR_TOTAL_DEBIT_AMOUNT;
                }
                else if (rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value == 7)
                {
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalCpDebitCreditValue.Value = Resources.STR_CREDIT_VALUE;
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalCpTotalDebitCreditAmount.Value = Resources.STR_TOTAL_CREDIT_AMOUNT;
                }

                //Display the volume column in Lb for NA and Kg for EU
                if (Dynamics.Globals.CompanyId.Value == 1)
                {
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalVolume.Value = Resources.STR_VOLUME_LB;
                }
                else if (Dynamics.Globals.CompanyId.Value == 2)
                {
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalVolume.Value = Resources.STR_VOLUME_KG;
                }

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CpReceivablesTransactionDetails_OpenBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Method fired after opening the Receivables Transaction window.
        /// </summary>
        /// <param name="sender">Sender param</param>
        /// <param name="e">Event Arguments param</param>
        void CpReceivablesTransactionDetails_OpenAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                InsertReceivablesTransactionDetails(Resources.STR_ReceivablesEntryForm);
                CheckOrUncheckAllLinesInTemp();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CpReceivablesTransactionDetails_OpenAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;

                cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ScrollShrinkSwitch.Value = 1;
                cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalShrinkButton.RunValidate();
            }
        }


        private void InsertReceivablesTransactionDetails(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_ReceivablesEntryForm)
                {
                    ClearReceivablesTransactionScroll(formName);

                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalType.Value = 0;
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalDetails.Clear();

                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.CurrencyIndex.Value = rmSalesEntryForm.RmSalesEntry.CurrencyIndex.Value;
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.CurrencyIndex.Value = rmSalesEntryForm.RmSalesEntry.CurrencyIndex.Value;

                    bool isResult = true;

                    Chempoint.GP.Model.Interactions.Sales.ReceivablesRequest cashRequest = new Chempoint.GP.Model.Interactions.Sales.ReceivablesRequest();
                    Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse cashResponse = new Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse();
                    ReceivablesHeader receivablesHeader = new ReceivablesHeader();
                    CustomerInformation customerInfo = new CustomerInformation();
                    AuditInformation auditInfo = new AuditInformation();

                    try
                    {
                        auditInfo.CompanyId = Dynamics.Globals.CompanyId.Value;
                        cashRequest.Source = "Entry";
                        customerInfo.CustomerId = rmSalesEntryForm.RmSalesEntry.CustomerNumber.Value.Trim();
                        receivablesHeader.DocumentNumber = rmSalesEntryForm.RmSalesEntry.RmDocumentNumberWork.Value.Trim();
                        receivablesHeader.DocumentType = rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value;
                        receivablesHeader.CustomerInformation = customerInfo;
                        cashRequest.AuditInformation = auditInfo;
                        cashRequest.ReceivablesHeader = receivablesHeader;

                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationURL);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/SalesOrderUpdate/GetReceivablesDetail/", cashRequest); // we need to refer the web.api service url here.

                            if (response.Result.IsSuccessStatusCode)
                            {
                                cashResponse = response.Result.Content.ReadAsAsync<Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse>().Result;
                                if (cashResponse.Status == Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success)
                                {
                                    isResult = true;
                                }
                                else
                                {
                                    isResult = false;
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertReceivablesTransactionDetails Method (GetReceivablesDetail): " + cashResponse.ErrorMessage.ToString());
                                    MessageBox.Show("Error: " + cashResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                            }
                            else
                            {
                                isResult = false;
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not fetch from custom receivables details Table");
                                MessageBox.Show("Error: Data does not fetch from custom receivables details Table", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                    catch (AggregateException aggredateException)
                    {
                        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertReceivablesTransactionDetails Method (AggregateException): " + aggredateException.Message.ToString());
                        MessageBox.Show(aggredateException.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                    }
                    catch (Exception ex)
                    {
                        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertReceivablesTransactionDetails Method (Exception): " + ex.Message.ToString());
                        MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                    }

                    #region Line detail

                    string receivedDetails = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalDetails.Value.ToString().Trim();

                    if (cashResponse != null && isResult)
                    {
                        if (cashResponse.ReceivableEntity != null)
                        {
                            int i = 1;

                            cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();
                            cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();

                            foreach (var itemDetail in cashResponse.ReceivableEntity)
                            {
                                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.ItemNumber.Value = itemDetail.ItemNumber;
                                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.ItemDescription.Value = itemDetail.ItemDescription;
                                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Qty.Value = itemDetail.OrderedQuantity;
                                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.CpVolume.Value = itemDetail.ShipWeight;
                                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.CpNetWeight.Value = itemDetail.NetWeight;
                                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.IsSelected.Value = itemDetail.IsSelected;
                                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.DocumentAmount.Value = itemDetail.UnitPriceAmount;
                                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.LineNumber.Value = Convert.ToInt16(i);
                                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.LineItemSequence.Value = itemDetail.OrderLineId;
                                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.CurrencyIndex.Value = rmSalesEntryForm.RmSalesEntry.CurrencyIndex.Value;
                                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.ExtendedPrice.Value = itemDetail.LineTotalAmount;
                                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.InvoiceNumber.Value = itemDetail.ApplyToDocumentNumber;
                                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.SupportId.Value = itemDetail.SupportId;
                                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.IncidentId.Value = itemDetail.IncidentId;

                                if (originalTypeIdEntry == 0 && itemDetail.TypeId != 0)
                                {
                                    originalTypeIdEntry = Convert.ToInt16(itemDetail.TypeId); // stop firing type id change event many times.
                                }
                                if (String.IsNullOrEmpty(receivedDetails) &&
                                    !String.IsNullOrEmpty(itemDetail.CommentText.ToString().Trim()))
                                {
                                    receivedDetails = receivedDetails + " " + itemDetail.CommentText.ToString().Trim();
                                }

                                if ((itemDetail.IsSelected && itemDetail.SupportId != "0") || (itemDetail.IsSelected == false))
                                {
                                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Save();
                                    i = i + 1;
                                }
                            }
                            cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                            cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();
                        }
                    }

                    #endregion Line detail
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalType.Value = Convert.ToInt16(originalTypeIdEntry);
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalDetails.Value = receivedDetails.ToString().Trim();

                    //Fill details to scrolling window.Call dex procedure to fill the scrolling window.
                    cpReceivablesTransactionDetailsForm.Procedures.CpReceivablesTransactionDetailsFormFillScroll.Invoke();

                    CalculateLineTotal(formName);

                }
                else if (formName == Resources.STR_ReceivablesInquiryForm)
                {
                    ClearReceivablesTransactionScroll(formName);

                    cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.LocalType.Value = 0;
                    cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.LocalDetails.Clear();

                    cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.CurrencyIndex.Value = rmSalesInquiryForm.RmSalesInquiry.CurrencyIndex.Value;
                    cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.CprmScrollInquiry.CurrencyIndex.Value = rmSalesInquiryForm.RmSalesInquiry.CurrencyIndex.Value;

                    bool isResult = true;
                    Chempoint.GP.Model.Interactions.Sales.ReceivablesRequest cashRequest = new Chempoint.GP.Model.Interactions.Sales.ReceivablesRequest();
                    Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse cashResponse = new Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse();
                    ReceivablesHeader receivablesHeader = new ReceivablesHeader();
                    CustomerInformation customerInfo = new CustomerInformation();
                    AuditInformation auditInfo = new AuditInformation();

                    try
                    {
                        auditInfo.CompanyId = Dynamics.Globals.CompanyId.Value;
                        cashRequest.Source = "Inquiry";

                        customerInfo.CustomerId = rmSalesInquiryForm.RmSalesInquiry.CustomerNumber.Value.Trim();
                        receivablesHeader.DocumentNumber = rmSalesInquiryForm.RmSalesInquiry.DocumentNumber.Value.Trim();
                        receivablesHeader.DocumentType = rmSalesInquiryForm.RmSalesInquiry.RmDocumentTypeAll.Value;
                        receivablesHeader.CustomerInformation = customerInfo;
                        cashRequest.AuditInformation = auditInfo;
                        cashRequest.ReceivablesHeader = receivablesHeader;

                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationURL);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/SalesOrderUpdate/GetReceivablesDetail/", cashRequest); // we need to refer the web.api service url here.

                            if (response.Result.IsSuccessStatusCode)
                            {
                                cashResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                                if (cashResponse.Status == ResponseStatus.Success)
                                {
                                    isResult = true;
                                }
                                else
                                {
                                    isResult = false;
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertReceivablesTransactionDetails Method (GetReceivablesDetail): " + cashResponse.ErrorMessage.ToString());
                                    MessageBox.Show("Error: " + cashResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                            }
                            else
                            {
                                isResult = false;
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not fetch from custom receivables details Table");
                                MessageBox.Show("Error: Data does not fetch from custom receivables details Table", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                    catch (AggregateException aggredateException)
                    {
                        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertReceivablesTransactionDetails Method (AggregateException): " + aggredateException.Message.ToString());
                        MessageBox.Show(aggredateException.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                    }
                    catch (Exception ex)
                    {
                        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertReceivablesTransactionDetails Method (Exception): " + ex.Message.ToString());
                        MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                    }

                    #region Line detail

                    string receivedDetails = cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.LocalDetails.Value.ToString().Trim();

                    if (cashResponse != null && isResult)
                    {
                        if (cashResponse.ReceivableEntity != null)
                        {
                            int i = 1;

                            cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.Close();
                            cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.Release();

                            foreach (var itemDetail in cashResponse.ReceivableEntity)
                            {
                                cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.Release();
                                cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.ItemNumber.Value = itemDetail.ItemNumber;
                                cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.ItemDescription.Value = itemDetail.ItemDescription;
                                cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.Qty.Value = itemDetail.OrderedQuantity;
                                cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.CpVolume.Value = itemDetail.ShipWeight;
                                cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.CpNetWeight.Value = itemDetail.NetWeight;
                                cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.IsSelected.Value = itemDetail.IsSelected;
                                cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.DocumentAmount.Value = itemDetail.UnitPriceAmount;
                                cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.LineNumber.Value = Convert.ToInt16(i);
                                cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.LineItemSequence.Value = itemDetail.OrderLineId;
                                cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.CurrencyIndex.Value = rmSalesInquiryForm.RmSalesInquiry.CurrencyIndex.Value;
                                cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.ExtendedPrice.Value = itemDetail.LineTotalAmount;
                                cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.InvoiceNumber.Value = itemDetail.ApplyToDocumentNumber;
                                cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.SupportId.Value = itemDetail.SupportId;
                                cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.IncidentId.Value = itemDetail.IncidentId;

                                if (originalTypeIdInquiry == 0 && itemDetail.TypeId != 0)
                                {
                                    originalTypeIdInquiry = Convert.ToInt16(itemDetail.TypeId); // stop firing type id change event many times.
                                }
                                if (String.IsNullOrEmpty(receivedDetails) &&
                                    !String.IsNullOrEmpty(itemDetail.CommentText.ToString().Trim()))
                                {
                                    receivedDetails = receivedDetails + " " + itemDetail.CommentText.ToString().Trim();
                                }

                                if ((itemDetail.IsSelected && itemDetail.SupportId != "0") || (itemDetail.IsSelected == false))
                                {
                                    cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.Save();
                                    i = i + 1;
                                }
                            }
                            cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.Release();
                            cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.Close();
                        }
                    }

                    #endregion Line detail
                    cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.LocalType.Value = Convert.ToInt16(originalTypeIdInquiry);
                    cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.LocalDetails.Value = receivedDetails.ToString().Trim();

                    //Fill details to scrolling window.Call dex procedure to fill the scrolling window.
                    cpReceivablesTransactionInquiryForm.Procedures.CpReceivablesTransactionInquiryFormFillScroll.Invoke();

                    CalculateLineTotal(formName);
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertReceivablesTransactionDetails Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "CashApplication");
                logMessage = null;
            }
        }

        /// <summary>
        /// To close the ReceivablesTranscationInquiry window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CpReceivablesTransactionInquiryOkButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (cpReceivablesTransactionInquiryForm != null && cpReceivablesTransactionInquiryForm.IsOpen == true)
                    cpReceivablesTransactionInquiryForm.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CpReceivablesTransactionInquiryOkButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                if (cpReceivablesTransactionDetailsForm != null)
                    cpReceivablesTransactionInquiryForm = null;

                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        ///  Event called on cancel change in window
        /// </summary>
        /// <param name="sender">Sender param</param>
        /// <param name="e">Event Arguments param</param>
        void CancelButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (cpReceivablesTransactionDetailsForm != null && cpReceivablesTransactionDetailsForm.IsOpen == true)
                    cpReceivablesTransactionDetailsForm.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CancelButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                if (cpReceivablesTransactionDetailsForm != null)
                    cpReceivablesTransactionDetailsForm = null;

                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }



        /// <summary>
        /// Checking all the lines in the scroll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CheckAll_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalType.Value != 0)
                {
                    if (cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.CheckAll.Value == true)
                    {
                        cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.CheckAll.Disable();
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                        TableError tableError = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.ChangeFirst();
                        while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                        {
                            cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.IsSelected.Value = true;
                            cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Save();

                            tableError = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.ChangeNext();
                        }
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();

                        DistributeReceivables(1);
                        cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.CheckAll.Enable();
                    }
                    else
                    {
                        cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.CheckAll.Disable();
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                        TableError tableError = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.ChangeFirst();
                        while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                        {
                            cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.IsSelected.Value = false;
                            cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.DocumentAmount.Value = 0;
                            cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.SupportId.Value = "0";
                            cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.IncidentId.Value = "0";
                            cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Save();

                            tableError = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.ChangeNext();
                        }
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();

                        DistributeReceivables(1);
                        cpReceivablesTransactionDetailsForm.Procedures.CpReceivablesTransactionDetailsFormFillScroll.Invoke();
                        cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.CheckAll.Enable();
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CheckAll_Change Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "Receivables Details");
                logMessage = null;
            }
        }

        private void CheckOrUncheckAllLinesInTemp()
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                int totalCount = 0, lineCount = 0;
                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();
                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.InvoiceNumber.Value = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.InvoiceNumber.Value;
                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.LineItemSequence.Value = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.LineItemSequence.Value;
                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Key = 1;
                TableError tableError = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.GetFirst();
                while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    totalCount++;
                    tableError = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.GetNext();
                }
                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();

                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();
                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.InvoiceNumber.Value = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.InvoiceNumber.Value;
                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.LineItemSequence.Value = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.LineItemSequence.Value;
                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Key = 1;
                TableError tableError1 = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.GetFirst();
                while (tableError1 == TableError.NoError && tableError1 != TableError.EndOfTable)
                {
                    if (cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.IsSelected.Value == true)
                    {
                        lineCount++;
                    }
                    tableError1 = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.GetNext();
                }
                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();

                if (totalCount == lineCount)
                {
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.CheckAll.Value = true;
                }
                else
                {
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.CheckAll.Value = false;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CheckOrUncheckAllLinesInTemp Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "Receivables Details");
                logMessage = null;
            }
        }
        
        
        
        
        /// <summary>
        /// Event called on selecting and deselecting the check box in scrolling window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CpReceivablesTransactionDetailsIsSelected_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (rmSalesEntryForm.RmSalesEntry.OriginatingDocumentAmount.Value == 0
                    && cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.IsSelected.Value == true)
                {
                    MessageBox.Show(Resources.STR_ERROR_REC_AMOUNT, Resources.STR_MESSAGE_TITLE);
                    isRejectCheckBox = true;
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.IsSelected.Value = false;
                    isRejectCheckBox = false;
                    return;
                }

                if (!isRejectCheckBox)
                {
                    string invoiceNumber = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.InvoiceNumber.Value;
                    int lineNumber = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.LineItemSequence.Value;
                    decimal docAmount = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.DocumentAmount.Value;

                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.InvoiceNumber.Value = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.InvoiceNumber.Value;
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.LineItemSequence.Value = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.LineItemSequence.Value;
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Key = 1;
                    TableError tableError = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Change();
                    if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        if (cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.IsSelected.Value == false)
                        {
                            cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.DocumentAmount.Value = 0;
                            cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.SupportId.Value = "0";
                            cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.IncidentId.Value = "0";
                        }
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.IsSelected.Value = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.IsSelected.Value;

                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Save();
                    }
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();

                    DistributeReceivables(1);

                    decimal newDocAmount = 0;
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.InvoiceNumber.Value = invoiceNumber;
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.LineItemSequence.Value = lineNumber;
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Key = 1;
                    TableError tableError1 = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Get();
                    if (tableError1 == TableError.NoError && tableError1 != TableError.EndOfTable)
                    {
                        newDocAmount = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.DocumentAmount.Value;
                    }
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();
                    if (docAmount != newDocAmount)
                    {
                        cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.DocumentAmount.Value = newDocAmount;
                    }
                    CheckOrUncheckAllLinesInTemp();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CpReceivablesTransactionDetailsIsSelected_Change Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Method to update the document total on entering the credit value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CpReceivablesTransactionDetailsDocumentAmount_Change(object sender, EventArgs e)
        {
            UpdateLineDetails(Resources.STR_ReceivablesEntryForm);
            CalculateLineTotal(Resources.STR_ReceivablesEntryForm);
        }

        /// <summary>
        /// Method to update the document total on entering the credit value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CpReceivablesTransactionDetailsSupportIdAndIncidentId_Change(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.SupportId.Value))
            {
                cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.SupportId.Value = "0";
            }
            if (String.IsNullOrEmpty(cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.IncidentId.Value))
            {
                cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.IncidentId.Value = "0";
            }
            UpdateLineDetails(Resources.STR_ReceivablesEntryForm);
        }

        void CpReceivablesTransactionDetailsDocumentAmount_EnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.IsSelected.Value == false)
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.IsSelected.Focus();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CpReceivablesTransactionDetailsDocumentAmount_EnterBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }
        /// <summary>
        /// Triggers the typeid change event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LocalType_Change(object sender, EventArgs e)
        {
            if (originalTypeIdEntry != cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalType.Value)
            {
                SelectAllReceivableLines();
                originalTypeIdEntry = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalType.Value; // stop repeated calling of LocalType_Change 
            }
        }

        /// <summary>
        /// Select all lines and distribute based on the type id
        /// </summary>
        private void SelectAllReceivableLines()
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalType.Value == 3
                    || cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalType.Value == 7)
                {
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                    TableError tableError = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.ChangeFirst();
                    while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.DocumentAmount.Value = 0;
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.SupportId.Value = "0";
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.IncidentId.Value = "0";
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.IsSelected.Value = true;
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Save();

                        tableError = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.ChangeNext();
                    }
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();
                }

                if (cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalType.Value > 0)
                    DistributeReceivables(1);
                CheckOrUncheckAllLinesInTemp();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SelectAllReceivableLines Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Triggers the scroll line pre event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ReceivablesTransactionDetailScroll_LineEnterBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalType.Value == 0)
                {
                    MessageBox.Show(Resources.STR_ERROR_SELECTLTYPEID, Resources.STR_MESSAGE_TITLE);
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalType.Focus();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ReceivablesTransactionDetailScroll_LineEnterBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Calculate the sum of linetotal
        /// </summary>
        /// <returns></returns>
        private void UpdateLineDetails(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_ReceivablesEntryForm)
                {
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.InvoiceNumber.Value = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.InvoiceNumber.Value;
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.LineItemSequence.Value = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.LineItemSequence.Value;
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Key = 1;
                    TableError tableError = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Change();
                    if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.DocumentAmount.Value = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.DocumentAmount.Value;
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.SupportId.Value = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.SupportId.Value;
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.IncidentId.Value = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.IncidentId.Value;
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.IsSelected.Value = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.ReceivablesTransactionDetailScroll.IsSelected.Value;
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Save();
                    }
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();
                }
                else if (formName == Resources.STR_RMCashReceiptForm)
                {
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Close();
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();

                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocumentNumber.Value = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyDocumentNumber.Value.ToString().Trim();
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyDocumentType.Value = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyDocumentType.Value;
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocNumber.Value = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyToDocNumber.Value.ToString().Trim();
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocType.Value = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyToDocType.Value;

                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Key = 1;
                    TableError tableError = cashApplicationProcessForm.Tables.CashApplyProcessTemp.Change();
                    if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyAmountRemaining.Value = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.OriginatingAmountRemaining.Value - cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyAmount.Value;

                        cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyAmount.Value;
                        cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmountRemaining.Value = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.OriginatingAmountRemaining.Value - cashApplicationProcessForm.ApplyToOpenOrders.CashApplyScroll.CashApplyAmount.Value;
                        cashApplicationProcessForm.Tables.CashApplyProcessTemp.Save();
                    }
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Close();
                }
                else if (formName == Resources.STR_RMCashReceiptinquiryForm)
                {
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Close();
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Release();

                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocumentNumber.Value = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyDocumentNumber.Value.ToString().Trim();
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyDocumentType.Value = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyDocumentType.Value;
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyToDocNumber.Value = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyToDocNumber.Value.ToString().Trim();
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyToDocType.Value = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyToDocType.Value;

                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Key = 1;
                    TableError tableError = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Change();
                    if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyAmountRemaining.Value = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.OriginatingAmountRemaining.Value - cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyAmount.Value;

                        cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyAmount.Value;
                        cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyAmountRemaining.Value = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.OriginatingAmountRemaining.Value - cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyScroll.CashApplyAmount.Value;
                        cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Save();
                    }
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Release();
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Close();
                }

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In UpdateLineDetails Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Calculate the sum of linetotal
        /// </summary>
        /// <returns></returns>
        private void CalculateLineTotal(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_ReceivablesEntryForm)
                {
                    decimal totalLineAmount = 0;
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                    TableError tableError = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.GetFirst();
                    while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        //get the sum of line credit amount
                        totalLineAmount += cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.DocumentAmount.Value;
                        tableError = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.GetNext();
                    }

                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();

                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.DocumentAmount.Value = totalLineAmount;
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.RemainingBalance.Value = rmSalesEntryForm.RmSalesEntry.OriginatingDocumentAmount.Value - totalLineAmount;
                }
                else if (formName == Resources.STR_ReceivablesInquiryForm)
                {
                    decimal totalLineAmount = 0;
                    cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.Close();
                    cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.Release();
                    TableError tableError = cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.GetFirst();
                    while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        //get the sum of line credit amount
                        totalLineAmount += cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.DocumentAmount.Value;
                        tableError = cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.GetNext();
                    }

                    cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.Release();
                    cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.Close();

                    cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.DocumentAmount.Value = totalLineAmount;
                    cpReceivablesTransactionInquiryForm.CpReceivablesTransactionInquiry.RemainingBalance.Value = rmSalesInquiryForm.RmSalesInquiry.OriginatingDocumentAmount.Value - totalLineAmount;
                }
                else if (formName == Resources.STR_RMCashReceiptForm || formName == Resources.STR_ReceivingEntryForm || formName == Resources.STR_LockBoxTransactionsForm)
                {
                    decimal totalLineAmount = 0;
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Close();
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();
                    TableError tableError = cashApplicationProcessForm.Tables.CashApplyProcessTemp.GetFirst();
                    while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        if (cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyToDocCheckBox.Value == true)
                        {
                            //get the sum of line credit amount/debit amount
                            totalLineAmount += cashApplicationProcessForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value;
                        }
                        tableError = cashApplicationProcessForm.Tables.CashApplyProcessTemp.GetNext();
                    }

                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Release();
                    cashApplicationProcessForm.Tables.CashApplyProcessTemp.Close();

                    cashApplicationProcessForm.ApplyToOpenOrders.CashApplyUnappliedAmt.Value = cashApplicationProcessForm.ApplyToOpenOrders.CashApplyTotalAmt.Value - totalLineAmount;
                }
                else if (formName == Resources.STR_RMCashReceiptinquiryForm || formName == Resources.STR_ReceivingInquiryForm)
                {
                    decimal totalLineAmount = 0;
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Close();
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Release();
                    TableError tableError = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.GetFirst();
                    while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        if (cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyToDocCheckBox.Value == true)
                        {
                            //get the sum of line credit amount/debit amount
                            totalLineAmount += cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.CashApplyAmount.Value;
                        }
                        tableError = cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.GetNext();
                    }

                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Release();
                    cashApplicationProcessInquiryForm.Tables.CashApplyProcessTemp.Close();

                    cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyUnappliedAmt.Value = cashApplicationProcessInquiryForm.ApplyToOpenOrdersInquiry.CashApplyTotalAmt.Value - totalLineAmount;
                }

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In UpdateLineDetails Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Cleare scroll temp table
        /// </summary>
        private void ClearReceivablesTransactionScroll(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_ReceivablesEntryForm)
                {
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                    TableError tableRemove = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.ChangeFirst();
                    while (tableRemove == TableError.NoError && tableRemove != TableError.EndOfTable)
                    {
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Remove();
                        tableRemove = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.ChangeNext();
                    }
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();
                }
                else if (formName == Resources.STR_ReceivablesInquiryForm)
                {
                    cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.Close();
                    cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.Release();
                    TableError tableRemove = cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.ChangeFirst();
                    while (tableRemove == TableError.NoError && tableRemove != TableError.EndOfTable)
                    {
                        cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.Remove();
                        tableRemove = cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.ChangeNext();
                    }
                    cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.Release();
                    cpReceivablesTransactionInquiryForm.Tables.RmMemoLineDetailTemp.Close();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearReceivablesTransactionScroll Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Distribute the amount to selected lines
        /// </summary>
        /// <param name="source"></param>
        private void DistributeReceivables(int source)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalType.Value == 0)
                {
                    MessageBox.Show(Resources.STR_ERROR_SELECTLTYPEID, Resources.STR_MESSAGE_TITLE);
                }
                else
                {
                    try
                    {
                        bool isLineSelected = false;

                        Chempoint.GP.Model.Interactions.Sales.ReceivablesRequest cashRequest = new Chempoint.GP.Model.Interactions.Sales.ReceivablesRequest();
                        Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse cashResponse = new Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse();
                        ReceivablesHeader receivablesHeader = new ReceivablesHeader();
                        Amount amountInfo = new Amount();
                        AuditInformation auditInfo = new AuditInformation();

                        List<ReceivableDetails> receivableEntityList = new List<ReceivableDetails>();
                        ReceivableDetails receivableInfo = null;

                        auditInfo.CompanyId = Dynamics.Globals.CompanyId.Value;
                        auditInfo.ModifiedBy = Dynamics.Globals.UserId.Value;

                        amountInfo.TotalAmount = rmSalesEntryForm.RmSalesEntry.OriginatingDocumentAmount.Value;
                        receivablesHeader.TypeId = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalType.Value;
                        receivablesHeader.CommentText = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalDetails.Value.ToString().Trim();
                        receivablesHeader.Amount = amountInfo;

                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                        TableError tableError = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.GetFirst();
                        while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                        {
                            receivableInfo = new ReceivableDetails();

                            receivableInfo.ApplyToDocumentNumber = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.InvoiceNumber.Value;
                            receivableInfo.SupportId = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.SupportId.Value;
                            receivableInfo.IncidentId = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.IncidentId.Value;
                            receivableInfo.ItemNumber = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.ItemNumber.Value;
                            receivableInfo.ItemDescription = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.ItemDescription.Value.ToString();
                            receivableInfo.OrderLineId = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.LineItemSequence.Value;
                            receivableInfo.IsSelected = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.IsSelected.Value;
                            receivableInfo.ShipWeight = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.CpVolume.Value;
                            receivableInfo.OrderedQuantity = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Qty.Value;
                            receivableInfo.NetWeight = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.CpNetWeight.Value;
                            receivableInfo.LineTotalAmount = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.ExtendedPrice.Value;
                            receivableInfo.UnitPriceAmount = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.DocumentAmount.Value;

                            if (cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.IsSelected.Value == true)
                                isLineSelected = true;

                            receivableEntityList.Add(receivableInfo);

                            tableError = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.GetNext();
                        }
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();

                        cashRequest.ReceivablesHeader = receivablesHeader;
                        cashRequest.AuditInformation = auditInfo;
                        cashRequest.ReceivableEntity = receivableEntityList;

                        if (source == 2 && !isLineSelected)
                        {
                            MessageBox.Show(Resources.STR_ERROR_SELECTLINE, Resources.STR_MESSAGE_TITLE);
                        }
                        else
                        {
                            if (isLineSelected)
                            {
                                // Service call ...
                                using (HttpClient client = new HttpClient())
                                {
                                    client.BaseAddress = new Uri(gpServiceConfigurationURL);
                                    client.DefaultRequestHeaders.Accept.Clear();
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                    var response = client.PostAsJsonAsync("api/SalesOrderUpdate/GetDistributeAmountDetail", cashRequest); // we need to refer the web.api service url here.
                                    if (response.Result.IsSuccessStatusCode)
                                    {
                                        cashResponse = response.Result.Content.ReadAsAsync<Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse>().Result;
                                        if (cashResponse.Status == Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error)
                                        {
                                            MessageBox.Show("Error: " + cashResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In DistributeReceivables Method (GetDistributeAmountDetail): " + cashResponse.ErrorMessage.ToString());
                                        }
                                        else
                                        {
                                            if (cashResponse.ReceivableEntity != null)
                                            {
                                                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();
                                                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();

                                                foreach (var itemDetail in cashResponse.ReceivableEntity)
                                                {
                                                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                                                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.InvoiceNumber.Value = itemDetail.ApplyToDocumentNumber;
                                                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.LineItemSequence.Value = itemDetail.OrderLineId;
                                                    cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Key = 1;
                                                    TableError tableError1 = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Change();
                                                    if (tableError1 == TableError.NoError && tableError1 != TableError.EndOfTable)
                                                    {
                                                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.DocumentAmount.Value = itemDetail.UnitPriceAmount;
                                                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Save();
                                                    }
                                                }
                                                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                                                cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();

                                                //Fill details to scrolling window.Call dex procedure to fill the scrolling window.
                                                cpReceivablesTransactionDetailsForm.Procedures.CpReceivablesTransactionDetailsFormFillScroll.Invoke();

                                                CalculateLineTotal(Resources.STR_ReceivablesEntryForm);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Error: Data does not fetch for distribute amount details for selected lines", Resources.STR_MESSAGE_TITLE);
                                        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not fetch for distribute amount details for selected lines");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: " + exception.Message);
                        MessageBox.Show("Error: " + exception.Message, Resources.STR_MESSAGE_TITLE);
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In DistributeReceivables Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        ///  Event called on Save change in window
        /// </summary>
        /// <param name="sender">Sender param</param>
        /// <param name="e">Event Arguments param</param>
        void SaveButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (String.IsNullOrEmpty(cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalDetails.Value))
                {
                    MessageBox.Show(Resources.STR_ERROR_DETAILS, Resources.STR_MESSAGE_TITLE);
                    cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalDetails.Focus();
                }

                else
                {
                    try
                    {
                        bool isLineEmpty = false;

                        Chempoint.GP.Model.Interactions.Sales.ReceivablesRequest cashRequest = new Chempoint.GP.Model.Interactions.Sales.ReceivablesRequest();
                        Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse cashResponse = new Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse();
                        ReceivablesHeader receivablesHeader = new ReceivablesHeader();
                        CustomerInformation customerInfo = new CustomerInformation();
                        AuditInformation auditInfo = new AuditInformation();

                        List<ReceivableDetails> receivableEntityList = new List<ReceivableDetails>();
                        ReceivableDetails receivableInfo = null;

                        auditInfo.CompanyId = Dynamics.Globals.CompanyId.Value;
                        auditInfo.ModifiedBy = Dynamics.Globals.UserId.Value;

                        customerInfo.CustomerId = rmSalesEntryForm.RmSalesEntry.CustomerNumber.Value.Trim();
                        receivablesHeader.DocumentNumber = rmSalesEntryForm.RmSalesEntry.RmDocumentNumberWork.Value.Trim();
                        receivablesHeader.DocumentType = rmSalesEntryForm.RmSalesEntry.RmDocumentTypeAll.Value;

                        receivablesHeader.TypeId = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalType.Value;
                        receivablesHeader.CommentText = cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.LocalDetails.Value.ToString().Trim();

                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                        TableError tableError = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.GetFirst();
                        while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                        {
                            receivableInfo = new ReceivableDetails();

                            receivableInfo.ApplyToDocumentNumber = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.InvoiceNumber.Value;
                            receivableInfo.ItemNumber = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.ItemNumber.Value;
                            receivableInfo.ItemDescription = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.ItemDescription.Value.ToString();
                            receivableInfo.OrderLineId = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.LineItemSequence.Value;
                            receivableInfo.IsSelected = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.IsSelected.Value;
                            receivableInfo.ShipWeight = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.CpVolume.Value;
                            receivableInfo.OrderedQuantity = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Qty.Value;
                            receivableInfo.NetWeight = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.CpNetWeight.Value;
                            receivableInfo.LineTotalAmount = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.ExtendedPrice.Value;
                            receivableInfo.UnitPriceAmount = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.DocumentAmount.Value;
                            receivableInfo.SupportId = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.SupportId.Value;
                            receivableInfo.IncidentId = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.IncidentId.Value;

                            if (cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.IsSelected.Value == true)
                            {
                                if (cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.SupportId.Value == "0")
                                {
                                    isLineEmpty = true;
                                }
                            }

                            receivableEntityList.Add(receivableInfo);

                            tableError = cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.GetNext();
                        }
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Release();
                        cpReceivablesTransactionDetailsForm.Tables.RmMemoLineDetailTemp.Close();

                        receivablesHeader.CustomerInformation = customerInfo;
                        cashRequest.ReceivablesHeader = receivablesHeader;
                        cashRequest.AuditInformation = auditInfo;
                        cashRequest.ReceivableEntity = receivableEntityList;

                        if (isLineEmpty)
                        {
                            MessageBox.Show(Resources.STR_ERROR_SUPPORTID, Resources.STR_MESSAGE_TITLE);
                        }
                        else
                        {
                            // Service call ...
                            using (HttpClient client = new HttpClient())
                            {
                                client.BaseAddress = new Uri(gpServiceConfigurationURL);
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                var response = client.PostAsJsonAsync("api/SalesOrderUpdate/SaveReceivablesDetail", cashRequest); // we need to refer the web.api service url here.
                                if (response.Result.IsSuccessStatusCode)
                                {
                                    cashResponse = response.Result.Content.ReadAsAsync<Chempoint.GP.Model.Interactions.Sales.ReceivablesResponse>().Result;
                                    if (cashResponse.Status == Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error)
                                    {
                                        MessageBox.Show("Error: " + cashResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SaveButton_ClickAfterOriginal Method (SaveReceivablesDetail): " + cashResponse.ErrorMessage.ToString());
                                    }
                                    else
                                    {
                                        cpReceivablesTransactionDetailsForm.CpReceivablesTransactionDetails.Close();
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Error: Data does not save into custom details for receivables details", Resources.STR_MESSAGE_TITLE);
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: Data does not save into custom details for receivables details");
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error: " + exception.Message);
                        MessageBox.Show("Error: " + exception.Message, Resources.STR_MESSAGE_TITLE);
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In SaveButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Event called on DistributetoSelected change in window
        /// </summary>
        /// <param name="sender">Sender param</param>
        /// <param name="e">Event Arguments param</param>
        void CpDistribute_ClickAfterOriginal(object sender, EventArgs e)
        {
            DistributeReceivables(2);
        }



        #endregion Receivables Trx Details Entry

        #region CPServiceConfiguration

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

        #endregion CPServiceConfiguration

        #region EFT

        #region EFT_Customer_Mapping_Window

        /// <summary>
        /// EFT Customer Mapping Window Open After ...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EftCustomerMapping_OpenAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                eftCustomerMappingForm.EftCustomerMapping.DeleteButton.Disable();
                eftCustomerMappingForm.EftCustomerMapping.SaveButton.Disable();
                eftCustomerMappingForm.EftCustomerMapping.EftctxCustomerSourceScroll.EftCtxCustomerSource.Enable();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        /// <summary>
        /// EFT Customer Mapping window Clear button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EftCustomerMappingClear_ClickAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Clear();
                eftCustomerMappingForm.EftCustomerMapping.CorporateCustomerNumber.Clear();
                eftCustomerMappingForm.EftCustomerMapping.EftXrmParentCustomer.Clear();

                ClearEFTCustomerMappingScrollWindowDetails();
                eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Enable();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void EftCustomerMappingLookup1_ClickAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                if (!customerLookupForm.IsOpen)
                {
                    customerLookupForm.Open();
                    remittanceCustomerLookup = Resources.STR_CustomerMappingWindow;
                    remittanceCustomerLookupValue = string.Empty;
                }
                else
                    MessageBox.Show("Customer Lookup Window already opened. Please close the window " + Resources.STR_MESSAGE_TITLE);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        /// <summary>
        /// Delete ...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EftCustomerMappingDelete_ClickAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Close();
                eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Release();
                eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.EftCustRowId.Value = eftCustomerMappingForm.EftCustomerMapping.EftctxCustomerSourceScroll.EftCustRowId.Value;
                eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Key = 1;
                TableError tableError = eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Change();
                if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    DialogResult dialogresult;
                    dialogresult = MessageBox.Show("Are you sure you want to remove reference " + customer + " from " +
                        eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Value + " ?", Resources.STR_MESSAGE_TITLE,
                        MessageBoxButtons.OKCancel);
                    if (dialogresult == DialogResult.OK)
                    {
                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.EftRowType.Value = "Delete";
                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Save();
                    }
                    eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Release();
                    eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Close();
                }

                eftCustomerMappingForm.Procedures.EftCustomerMappingListScrollFill.Invoke();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        /// <summary>
        /// EFT Window Clsoe...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EftCustomerMappingSave_ClickAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                SaveEFTCustomerDetail();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        /// <summary>
        /// EFT window Scroll field ...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EftCustomerMappingScrollEnter_LineEnterAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                customer = eftCustomerMappingForm.EftCustomerMapping.EftctxCustomerSourceScroll.EftCtxCustomerSource.Value.ToString().Trim();
                if (string.IsNullOrEmpty(eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Value.Trim()))
                {
                    MessageBox.Show("Please select the customer.", Resources.STR_MESSAGE_TITLE);
                    eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Focus();
                }
                else
                {
                    if (customer != string.Empty)
                    {
                        eftCustomerMappingForm.EftCustomerMapping.DeleteButton.Enable();
                    }
                    else
                    {
                        eftCustomerMappingForm.EftCustomerMapping.DeleteButton.Disable();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        /// <summary>
        /// Customer Number leave after original
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EftCustomerMappingCustomerNumber_Change(object sender, EventArgs e)
        {
            try
            {
                EFTCustomernumberChange();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        void EftCustomerMappingCtxCustomerSource_LeaveBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool isRecordExists = false;
            if (eftCustomerMappingForm.EftCustomerMapping.EftctxCustomerSourceScroll.EftCustRowId.Value != 0)
            {
                if (eftCustomerMappingForm.EftCustomerMapping.EftctxCustomerSourceScroll.EftCtxCustomerSource.Value != "")
                {
                    eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Close();
                    eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Release();
                    eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.EftCtxCustomerSource.Value = eftCustomerMappingForm.EftCustomerMapping.EftctxCustomerSourceScroll.EftCtxCustomerSource.Value;
                    eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.CustomerNumber.Value = eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Value;
                    eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Key = 2;
                    TableError tableError = eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Get();
                    if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {
                        if (eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.EftCustRowId.Value != eftCustomerMappingForm.EftCustomerMapping.EftctxCustomerSourceScroll.EftCustRowId.Value)
                        {
                            isRecordExists = true;
                            MessageBox.Show("The record is already exists");
                        }
                    }
                    eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Release();
                    eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Close();

                    if (!isRecordExists)
                    {
                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Close();
                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Release();
                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.EftCustRowId.Value = eftCustomerMappingForm.EftCustomerMapping.EftctxCustomerSourceScroll.EftCustRowId.Value;
                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Key = 1;
                        TableError tableError1 = eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Change();
                        if (tableError1 == TableError.NoError)
                        {

                            eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.CustomerNumber.Value = eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Value.ToString().Trim();
                            eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.EftCtxCustomerSource.Value = eftCustomerMappingForm.EftCustomerMapping.EftctxCustomerSourceScroll.EftCtxCustomerSource.Value.ToString().Trim();

                            if (eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.EftRowType.Value != "Insert")
                                eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.EftRowType.Value = "Update";

                            eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Save();
                        }
                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Release();
                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Close();
                    }

                }
            }
            else
            {
                if (eftCustomerMappingForm.EftCustomerMapping.EftctxCustomerSourceScroll.EftCtxCustomerSource.Value != "")
                {
                    //We have to check wheather the same row id is exists or not
                    eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Close();
                    eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Release();
                    eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.EftCtxCustomerSource.Value = eftCustomerMappingForm.EftCustomerMapping.EftctxCustomerSourceScroll.EftCtxCustomerSource.Value;
                    eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.CustomerNumber.Value = eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Value;
                    eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Key = 2;
                    TableError tableError = eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Get();
                    if (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                    {

                        if (eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.EftCustRowId.Value != eftCustomerMappingForm.EftCustomerMapping.EftctxCustomerSourceScroll.EftCustRowId.Value)
                        {
                            isRecordExists = true;
                            MessageBox.Show("The record is already exists");
                        }
                    }
                    eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Release();
                    eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Close();

                    if (!isRecordExists)
                    {
                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Close();
                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Release();
                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Key = 1;
                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.EftCustRowId.Value = eftNextCustomerMappingId++;
                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.CustomerNumber.Value = eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Value.ToString().Trim();
                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.EftCtxCustomerSource.Value = eftCustomerMappingForm.EftCustomerMapping.EftctxCustomerSourceScroll.EftCtxCustomerSource.Value.ToString().Trim();
                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.EftRowType.Value = "Insert";

                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Save();

                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Release();
                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Close();
                    }


                }
            }
            eftCustomerMappingForm.Procedures.EftCustomerMappingListScrollFill.Invoke();


        }

        /// <summary>
        /// Save While window Close...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EftCustomerMapping_CloseBeforeOriginal(object sender, EventArgs e)
        {
            try
            {
                if (eftCustomerMappingForm.EftCustomerMapping.IsUpdated.Value == 1)
                {
                    DialogResult dialogresult;
                    dialogresult = MessageBox.Show("Do you want to save the changes" + " ?", Resources.STR_MESSAGE_TITLE,
                        MessageBoxButtons.OKCancel);
                    if (dialogresult == DialogResult.OK)
                        SaveEFTCustomerDetail();
                    else
                        eftCustomerMappingForm.EftCustomerMapping.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        /***************Private events*************/

        /// <summary>
        /// Clear EFT Customer Details Temp...
        /// </summary>
        /// <param name="eftResponse"></param>
        private void ClearEFTCustomerMappingScrollWindowDetails()
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                // Clearing scroll table
                eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Close();
                eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Release();
                TableError tableRemove = eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.ChangeFirst();
                while (tableRemove == TableError.NoError && tableRemove != TableError.EndOfTable)
                {
                    eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Remove();
                    tableRemove = eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.ChangeNext();
                }
                eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Release();
                eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Close();

                eftCustomerMappingForm.EftCustomerMapping.DeleteButton.Disable();
                eftCustomerMappingForm.EftCustomerMapping.SaveButton.Disable();

                //Filling scroll
                eftCustomerMappingForm.Procedures.EftCustomerMappingListScrollFill.Invoke();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearEFTCustomerDetailScroll Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// /Calling service method to save EFT Customer Details 
        /// </summary>
        private void SaveEFTCustomerDetail()
        {
            eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Close();
            eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Release();
            ReceivablesRequest eftRequest = new ReceivablesRequest();
            CustomerInformation customerInfo = new CustomerInformation();
            AuditInformation auditInfo = new AuditInformation();
            List<CustomerMappingDetails> eftCustomerSourceList = new List<CustomerMappingDetails>();
            TableError eftCustomerDetail = eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.GetFirst();
            while (eftCustomerDetail == TableError.NoError && eftCustomerDetail != TableError.EndOfTable)
            {
                CustomerMappingDetails customerMappingDetails = new CustomerMappingDetails();
                customerMappingDetails.CustomerId = eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Value.ToString();
                customerMappingDetails.Type = eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.EftRowType.Value;
                customerMappingDetails.EftCTXCustomerReference = eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.EftCtxCustomerSource.Value.ToString();
                customerMappingDetails.EftCustomerMappingId = eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.EftCustRowId.Value;

                eftCustomerSourceList.Add(customerMappingDetails);
                eftCustomerDetail = eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.GetNext();
            }
            customerInfo.CustomerMappingDetails = eftCustomerSourceList;

            eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Release();
            eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Close();

            customerInfo.CustomerId = eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Value.ToString().Trim();
            auditInfo.CreatedBy = Dynamics.Globals.UserId.Value.ToString().Trim();
            auditInfo.CompanyId = Dynamics.Globals.CompanyId.Value;
            eftRequest.CustomerInformation = customerInfo;
            eftRequest.AuditInformation = auditInfo;

            // Service call ...
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(gpServiceConfigurationURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = client.PostAsJsonAsync("api/SalesOrderUpdate/SaveEFTCustomerMappingDetails/", eftRequest); // we need to refer the web.api service url here.

                if (response.Result.IsSuccessStatusCode)
                {
                    ReceivablesResponse eftResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                    if (eftResponse.Status == ResponseStatus.Error)
                    {
                        MessageBox.Show("Error: " + eftResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                    }
                    else if (eftResponse.Status == ResponseStatus.Success)
                    {
                        eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Clear();
                        eftCustomerMappingForm.EftCustomerMapping.CorporateCustomerNumber.Clear();
                        eftCustomerMappingForm.EftCustomerMapping.EftXrmParentCustomer.Clear();
                        ClearEFTCustomerMappingScrollWindowDetails();
                        eftCustomerMappingForm.Procedures.EftCustomerMappingListScrollFill.Invoke();
                        eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Enable();
                    }
                }
                else
                {
                    MessageBox.Show("Error: Data does not saved into SaveEFTCustomerDetail Table", Resources.STR_MESSAGE_TITLE);
                }
            }
        }

        /// <summary>
        /// Invoke customer leave Button
        /// </summary>
        private void EFTCustomernumberChange()
        {

            eftCustomerMappingForm.EftCustomerMapping.CorporateCustomerNumber.Clear();
            eftCustomerMappingForm.EftCustomerMapping.EftXrmParentCustomer.Clear();

            if (eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Value != "")
            {
                eftCustomerMappingForm.Tables.RmCustomerMstr.Close();
                eftCustomerMappingForm.Tables.RmCustomerMstr.Release();
                eftCustomerMappingForm.Tables.RmCustomerMstr.CustomerNumber.Value = eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Value.ToString().Trim();
                TableError tableError = eftCustomerMappingForm.Tables.RmCustomerMstr.Get();
                if (tableError == TableError.NotFound)
                {
                    MessageBox.Show("Please enter an existing Customer Number.", Resources.STR_MESSAGE_TITLE);
                    eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Clear();
                    eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Focus();
                    eftCustomerMappingForm.Procedures.EftCustomerMappingListScrollFill.Invoke();
                }
                else if (tableError == TableError.NoError)
                {

                    eftCustomerMappingForm.Tables.RmCustomerMstr.Release();
                    eftCustomerMappingForm.Tables.RmCustomerMstr.Close();
                    ReceivablesRequest eftRequest = new ReceivablesRequest();
                    CustomerInformation eftCustomerInfo = new CustomerInformation();
                    AuditInformation eftAudit = new AuditInformation();
                    //Custoemr ID value get it from EFT Window..
                    eftAudit.CompanyId = Dynamics.Globals.CompanyId.Value;
                    eftCustomerInfo.CustomerId = eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Value.ToString().Trim();
                    eftRequest.CustomerInformation = eftCustomerInfo;
                    eftRequest.AuditInformation = eftAudit;
                    int maxRowid = 0;

                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SalesOrderUpdate/GetEFTCustomerMappingDetails/", eftRequest); // we need to refer the web.api service url here.

                        if (response.Result.IsSuccessStatusCode)
                        {
                            ReceivablesResponse eftResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                            if (eftResponse.Status == ResponseStatus.Success && eftResponse != null)
                            {
                                eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Disable();
                                if (eftResponse.CustomerInformation.ParentCustomerId != null && eftResponse.CustomerInformation.ParentCustomerId != "") // Display Parent customer numbber
                                    eftCustomerMappingForm.EftCustomerMapping.CorporateCustomerNumber.Value = eftResponse.CustomerInformation.ParentCustomerId;
                                else
                                    eftCustomerMappingForm.EftCustomerMapping.CorporateCustomerNumber.Value = "";
                                if (eftResponse.CustomerInformation.XrmParentCustomerId != null && eftResponse.CustomerInformation.XrmParentCustomerId != "") // Display Parent customer numbber
                                    eftCustomerMappingForm.EftCustomerMapping.EftXrmParentCustomer.Value = eftResponse.CustomerInformation.XrmParentCustomerId.ToString();
                                else
                                    eftCustomerMappingForm.EftCustomerMapping.EftXrmParentCustomer.Value = "";

                                ClearEFTCustomerMappingScrollWindowDetails();
                                eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Close();
                                eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Release();
                                eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.CustomerNumber.Value = eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Value.ToString().Trim();
                                eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Key = 1;
                                TableError tableError1 = eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Change();
                                if (tableError1 == TableError.NotFound)
                                {
                                    foreach (var eftCustomerSource in eftResponse.CustomerInformation.CustomerMappingDetails)
                                    {
                                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.CustomerNumber.Value = eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Value.ToString().Trim();
                                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.EftCtxCustomerSource.Value = eftCustomerSource.EftCTXCustomerReference.ToString().Trim();
                                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.EftCustRowId.Value = eftCustomerSource.EftCustomerMappingId;
                                        eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Save();
                                        if (maxRowid < eftCustomerSource.EftCustomerMappingId)
                                            maxRowid = eftCustomerSource.EftCustomerMappingId;
                                    }

                                    eftNextCustomerMappingId = eftResponse.CustomerInformation.MaxCustomerReferenceId + 1;
                                }
                                eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Release();
                                eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Close();
                                eftCustomerMappingForm.Procedures.EftCustomerMappingListScrollFill.Invoke();

                                eftCustomerMappingForm.EftCustomerMapping.SaveButton.Enable();

                            }
                            else if (eftResponse.Status == ResponseStatus.Error)
                            {
                                MessageBox.Show("Error: " + eftResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                eftCustomerMappingForm.EftCustomerMapping.DeleteButton.Disable();
                                eftCustomerMappingForm.EftCustomerMapping.SaveButton.Disable();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Error: Data does not saved into UpdateTaxScheduleIdToLine Table", Resources.STR_MESSAGE_TITLE);
                        }
                    }

                }

            }
        }

        #endregion EFT_Customer_Mapping_Window

        #region BankSummary_Import_Window


        void BankCtxSummaryClearButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                bankCtxSummaryImportForm.BankCtxSummaryImport.EftBatchId.Value = string.Empty;
                bankCtxSummaryImportForm.BankCtxSummaryImport.FilePath.Value = string.Empty;
                bankCtxSummaryImportForm.BankCtxSummaryImport.ImportButton.Disable();
                isCsvFileCheck = false;
                bankCtxSummaryImportForm.BankCtxSummaryImport.EftBatchId.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Resources.STR_MESSAGE_TITLE);
            }
        }

        void BankCtxSummaryFilePath_Change(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(bankCtxSummaryImportForm.BankCtxSummaryImport.EftBatchId.Value)
                    && !string.IsNullOrEmpty(bankCtxSummaryImportForm.BankCtxSummaryImport.FilePath.Value))
                {
                    bankCtxSummaryImportForm.BankCtxSummaryImport.ImportButton.Disable();
                    MessageBox.Show("Please Enter Batch Id", Resources.STR_MESSAGE_TITLE);
                    bankCtxSummaryImportForm.BankCtxSummaryImport.FilePath.Value = string.Empty;
                    bankCtxSummaryImportForm.BankCtxSummaryImport.EftBatchId.Focus();
                }
                else if (!string.IsNullOrEmpty(bankCtxSummaryImportForm.BankCtxSummaryImport.EftBatchId.Value)
                    && !string.IsNullOrEmpty(bankCtxSummaryImportForm.BankCtxSummaryImport.FilePath.Value))
                {

                    bankCtxSummaryImportForm.BankCtxSummaryImport.ImportButton.Enable();
                }
                else
                {
                    bankCtxSummaryImportForm.BankCtxSummaryImport.ImportButton.Disable();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Resources.STR_MESSAGE_TITLE);
            }
        }

        void EftBatchId_EnterBeforeOriginal(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(bankCtxSummaryImportForm.BankCtxSummaryImport.FilePath.Value))
                bankCtxSummaryImportForm.BankCtxSummaryImport.ImportButton.Enable();
        }



        /// <summary>
        /// EFT Batch ID Value verified
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BankCtxSummaryImportEftBatchId_LeaveAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(bankCtxSummaryImportForm.BankCtxSummaryImport.EftBatchId.Value)
                   && !string.IsNullOrEmpty(bankCtxSummaryImportForm.BankCtxSummaryImport.FilePath.Value))
                    bankCtxSummaryImportForm.BankCtxSummaryImport.ImportButton.Enable();
                else
                    bankCtxSummaryImportForm.BankCtxSummaryImport.ImportButton.Disable();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Resources.STR_MESSAGE_TITLE);
            }
        }


        void BankCtxSummaryImportButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            ReceivablesRequest receivablesRequest = null;
            string FilePath = string.Empty, csvCheck = string.Empty, strEftBatchId = string.Empty;

            try
            {
                logMessage.Append(DateTime.Now + " - BankCtxSummaryImportButton_ClickAfterOriginal event is started");

                if (!string.IsNullOrEmpty(bankCtxSummaryImportForm.BankCtxSummaryImport.EftBatchId.Value))
                    strEftBatchId = bankCtxSummaryImportForm.BankCtxSummaryImport.EftBatchId.Value;


                if (!string.IsNullOrEmpty(bankCtxSummaryImportForm.BankCtxSummaryImport.FilePath.Value))
                    FilePath = bankCtxSummaryImportForm.BankCtxSummaryImport.FilePath.Value;


                if (string.IsNullOrEmpty(strEftBatchId) && string.IsNullOrEmpty(FilePath))
                {
                    logMessage.Append(DateTime.Now + " - Batch Id is empty and File has not selected to import");
                    MessageBox.Show("Please enter Batch Id and summary file to import", Resources.STR_MESSAGE_TITLE);
                    bankCtxSummaryImportForm.BankCtxSummaryImport.FilePath.Value = string.Empty;
                }
                else if (string.IsNullOrEmpty(strEftBatchId))
                {
                    logMessage.Append(DateTime.Now + " - Batch Id is empty");
                    MessageBox.Show("Please enter Batch Id", Resources.STR_MESSAGE_TITLE);
                }
                else if (string.IsNullOrEmpty(FilePath))
                {
                    logMessage.Append(DateTime.Now + " - File has not selected to import");
                    MessageBox.Show("Please select bank summary file to import", Resources.STR_MESSAGE_TITLE);
                    bankCtxSummaryImportForm.BankCtxSummaryImport.FilePath.Value = string.Empty;
                }
                else if (Path.GetFileNameWithoutExtension(FilePath).Length > 32)
                {
                    logMessage.Append(DateTime.Now + " - File name cannot be greater than 32 characters");
                    MessageBox.Show("File name cannot be greater than 32 characters. Please update the file name and retry", Resources.STR_MESSAGE_TITLE);
                    bankCtxSummaryImportForm.BankCtxSummaryImport.FilePath.Value = string.Empty;
                }
                else
                {
                    if (!FilePath.Contains(":"))
                    {
                        FilePath = FilePath.Replace("/", @"\");
                        FilePath = FilePath.Insert(0, @"\");
                    }
                    else if (FilePath.Contains(":"))
                        FilePath = FilePath.Insert(2, "/");

                    if (File.Exists(FilePath))
                    {
                        logMessage.Append(DateTime.Now + " - File is not  duplicate");
                        csvCheck = Path.GetExtension(FilePath);

                        if (csvCheck.ToLower() == ".csv")
                        {
                            Regex regexItem = new Regex("^[a-zA-Z0-9 ]*$");

                            if (regexItem.IsMatch(Path.GetFileNameWithoutExtension(FilePath)))
                            {
                                string strOutputFile = EFTSummaryBackupPath + Path.GetFileNameWithoutExtension(FilePath) + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";

                                // copy the file to backup folder
                                File.Copy(FilePath, strOutputFile, true);
                                logMessage.Append(DateTime.Now + " - File copied to " + strOutputFile);

                                // Creating reqest
                                receivablesRequest = new ReceivablesRequest();
                                receivablesRequest.EFTPaymentFilePath = strOutputFile;
                                receivablesRequest.CompanyId = Dynamics.Globals.CompanyId.Value;
                                receivablesRequest.UserName = Dynamics.Globals.UserId.Value.ToString();
                                receivablesRequest.BatchId = strEftBatchId;

                                //calling service
                                using (HttpClient client = new HttpClient())
                                {
                                    client.BaseAddress = new Uri(gpServiceConfigurationURL);
                                    client.DefaultRequestHeaders.Accept.Clear();
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                    client.Timeout = new TimeSpan(1, 0, 0);
                                    logMessage.Append(DateTime.Now + " - " + gpServiceConfigurationURL + "api/ReceivablesManagement/ImportEFTBankSummaryReport is invoked from GpAddIn.cs");
                                    receivablesRequest.LogMessage = logMessage.ToString();

                                    var response = client.PostAsJsonAsync("api/ReceivablesManagement/ImportEFTBankSummaryReport", receivablesRequest); // we need to refer the web.api service url here.
                                    ReceivablesResponse recvgResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                                    logMessage.Append(recvgResponse.LogMessage);

                                    if (response.Result.IsSuccessStatusCode)
                                    {
                                        if (recvgResponse.Status == ResponseStatus.Success)
                                        {
                                            logMessage.Append(DateTime.Now + " - Status from" + gpServiceConfigurationURL + "api/ReceivablesManagement/ImportEFTBankSummaryReport is invoked from GpAddIn.cs is SUCCESS");
                                            MessageBox.Show("Bank Summary Imported Successfully", Resources.STR_MESSAGE_TITLE);
                                        }
                                        else if (recvgResponse.Status == ResponseStatus.Error)
                                        {
                                            logMessage.Append(DateTime.Now + " - Status from" + gpServiceConfigurationURL + "api/ReceivablesManagement/ImportEFTBankSummaryReport is invoked from GpAddIn.cs is ERROR");
                                            logMessage.AppendLine(DateTime.Now + " - " + recvgResponse.ErrorMessage);
                                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In BankCtxSummaryImportButton_ClickAfterOriginal Method (ProcessRequest): ");// + recvgResponse.ErrorMessage.ToString());
                                            MessageBox.Show(recvgResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                        }
                                    }
                                }
                                bankCtxSummaryImportForm.BankCtxSummaryImport.FilePath.Value = string.Empty;
                                bankCtxSummaryImportForm.BankCtxSummaryImport.EftBatchId.Value = string.Empty;
                            }
                            else
                            {
                                logMessage.Append(DateTime.Now + " - File name contained with special characters is not allowed");
                                bankCtxSummaryImportForm.BankCtxSummaryImport.FilePath.Value = string.Empty;
                                MessageBox.Show("Please remove special characters from file name and retry", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                        else
                        {
                            logMessage.Append(DateTime.Now + " - Wrong extension file is seleected");
                            bankCtxSummaryImportForm.BankCtxSummaryImport.FilePath.Value = string.Empty;
                            MessageBox.Show("Please select summary file in .csv format", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                    else
                    {
                        logMessage.Append(DateTime.Now + " - Invalid File Path");
                        MessageBox.Show("File does not exist in the selected path", Resources.STR_MESSAGE_TITLE);
                        bankCtxSummaryImportForm.BankCtxSummaryImport.FilePath.Value = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In BankCtxSummaryImportButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error while importing csv file" + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                logMessage.Append(DateTime.Now + " - BankCtxSummaryImportButton_ClickAfterOriginal event is completed");
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        #endregion BankSummary_Import_Window

        #region CTX_Import_Window

        //Remittance Window
        void BankCtxRemittanceImport_OpenBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bankCtxRemittanceImportForm.BankCtxRemittanceImport.ImportButton.Disable();
        }

        void BankCtxRemittanceFilePath_Change(object sender, EventArgs e)
        {
            try
            {
                if (bankCtxRemittanceImportForm.BankCtxRemittanceImport.FilePath.Value != string.Empty)
                    bankCtxRemittanceImportForm.BankCtxRemittanceImport.ImportButton.Enable();
                else
                    bankCtxRemittanceImportForm.BankCtxRemittanceImport.ImportButton.Disable();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Resources.STR_MESSAGE_TITLE);
            }
        }

        void BankCtxRemittanceImportClearButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                bankCtxRemittanceImportForm.BankCtxRemittanceImport.FilePath.Value = string.Empty;
                bankCtxRemittanceImportForm.BankCtxRemittanceImport.FilePath.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Resources.STR_MESSAGE_TITLE);
            }

        }

        void BankCtxRemittanceImportButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            ReceivablesRequest receivablesRequest = null;
            string FilePath = string.Empty, FileType = string.Empty;
            try
            {
                logMessage.Append(DateTime.Now + " - BankCtxRemittanceImportButton_ClickAfterOriginal event is started");
                FilePath = bankCtxRemittanceImportForm.BankCtxRemittanceImport.FilePath.Value.ToString();

                if (bankCtxRemittanceImportForm.BankCtxRemittanceImport.FilePath.Value != string.Empty)
                {

                    FilePath = bankCtxRemittanceImportForm.BankCtxRemittanceImport.FilePath.Value;

                    if (!FilePath.Contains(":"))
                    {
                        FilePath = FilePath.Replace("/", @"\");
                        FilePath = FilePath.Insert(0, @"\");
                    }
                    else if (FilePath.Contains(":"))
                        FilePath = FilePath.Insert(2, "/");

                    logMessage.Append(DateTime.Now + " - BankRemittanceImport  uploaded file path :" + FilePath);

                    if (File.Exists(FilePath))
                    {
                        logMessage.Append(DateTime.Now + " - File is not duplicate");
                        FileType = Path.GetExtension(FilePath);
                        if (FileType == ".txt")
                        {

                            string strOutputFile = EFTRemittanceBackupPath + Path.GetFileName(FilePath);
                            File.Copy(FilePath, strOutputFile, true);
                            logMessage.Append(DateTime.Now + " - File is copied to " + strOutputFile);

                            receivablesRequest = new ReceivablesRequest();
                            receivablesRequest.EFTPaymentFilePath = strOutputFile;
                            receivablesRequest.CompanyId = Dynamics.Globals.CompanyId.Value;
                            receivablesRequest.UserName = Dynamics.Globals.UserId.Value.ToString();
                            receivablesRequest.BatchId = string.Empty;

                            using (HttpClient client = new HttpClient())
                            {
                                client.BaseAddress = new Uri(gpServiceConfigurationURL);
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                client.Timeout = new TimeSpan(1, 0, 0);
                                logMessage.Append(DateTime.Now + " - " + gpServiceConfigurationURL + "api/ReceivablesManagement/ImportEFTRemittanceReport is invoked from GpAddIn.cs");
                                receivablesRequest.LogMessage = logMessage.ToString();

                                var response = client.PostAsJsonAsync("api/ReceivablesManagement/ImportEFTRemittanceReport", receivablesRequest); // we need to refer the web.api service url here.
                                ReceivablesResponse recvgResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                                logMessage.Append(recvgResponse.LogMessage);
                                if (response.Result.IsSuccessStatusCode)
                                {
                                    logMessage.Append(DateTime.Now + " - Status from" + gpServiceConfigurationURL + "api/ReceivablesManagement/ImportEFTRemittanceReport is invoked from GpAddIn.cs is SUCCESS");

                                    if (recvgResponse.Status == ResponseStatus.Success)
                                    {
                                        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Inserted csv file records");
                                        MessageBox.Show("Bank CTX Remittance Imported Successfully", Resources.STR_MESSAGE_TITLE);
                                    }
                                    else if (recvgResponse.Status == ResponseStatus.Error)
                                    {
                                        logMessage.Append(DateTime.Now + " - Status from" + gpServiceConfigurationURL + "api/ReceivablesManagement/ImportEFTRemittanceReport is invoked from GpAddIn.cs is ERROR");
                                        logMessage.AppendLine(DateTime.Now + " - " + recvgResponse.ErrorMessage);
                                        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In BankCtxSummaryImportButton_ClickAfterOriginal Method (ProcessRequest): ");// + recvgResponse.ErrorMessage.ToString());
                                        MessageBox.Show(recvgResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                    }
                                }
                            }
                            bankCtxRemittanceImportForm.BankCtxRemittanceImport.FilePath.Value = string.Empty;
                        }
                        else
                        {
                            logMessage.Append(DateTime.Now + " - Wrong extension file is seleected");
                            bankCtxRemittanceImportForm.BankCtxRemittanceImport.FilePath.Value = string.Empty;
                            MessageBox.Show("Please select a valid bank remittance file", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                    else
                    {
                        logMessage.Append(DateTime.Now + " - Invalid File Path");
                        MessageBox.Show("Invalid File Path", Resources.STR_MESSAGE_TITLE);
                        bankCtxRemittanceImportForm.BankCtxRemittanceImport.FilePath.Value = string.Empty;
                    }
                }
                else
                {
                    logMessage.Append(DateTime.Now + " - File has not selected to import");
                    MessageBox.Show("Please select bank remittance file to import", Resources.STR_MESSAGE_TITLE);
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In bankCtxRemittanceImportForm Method: " + ex.Message.ToString());
                MessageBox.Show("Error while importing bank remittance file" + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                logMessage.Append(DateTime.Now + " - BankCtxRemittanceImportButton_ClickAfterOriginal event is completed");
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }

        #endregion CTX_Import_Window

        #region BankEntry_Window

        void EftCustomerBankEntry_OpenAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalSearchBy.Value = 1;
                eftCustomerRemittancesEntryForm.Procedures.EftCustomerRemittancesEntryFormScrollFill.Invoke();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// EFT Customer Remittance Redisplay Button Click ...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTCustomerRemittance_ClickAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                FetchEftWindowDetails(Resources.STR_CustomerRemittanceEntry);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }




        /// <summary>
        /// ValidateEFTItemReference from leave ItemReference Number...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTRemittanceToItemReference_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!string.IsNullOrEmpty(eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToItemReference.Value.ToString().Trim())
                    && (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromItemReference.Value.ToString().Trim() != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToItemReference.Value.ToString().Trim()))
                {

                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.AllOrRange.Value = 1;
                    if (string.IsNullOrEmpty(eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromItemReference.Value.ToString()))
                        eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromItemReference.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToItemReference.Value;

                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceToItemReference_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }

        /// <summary>
        /// Delete EFTTransaction based on the EFT ID.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void eftCustomerRemittancesDeleteButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " eftCustomerRemittancesDeleteButton_ClickAfterOriginal Method ");

                if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value != 0)
                {
                    ReceivablesRequest eftrequest = new ReceivablesRequest();
                    ReceivablesResponse eftResponse = new ReceivablesResponse();
                    AuditInformation information = new AuditInformation
                    {
                        CompanyId = Dynamics.Globals.CompanyId.Value
                    };
                    EFTPayment payment = new EFTPayment();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Key = 1;
                    TableError error = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Change();
                    if ((error == TableError.NoError) && (error != TableError.EndOfTable))
                    {
                        if (MessageBox.Show("Are you sure you want to remove all Item reference number permanently?", Resources.STR_MESSAGE_TITLE, MessageBoxButtons.OKCancel) == DialogResult.OK)
                        {
                            using (HttpClient client = new HttpClient())
                            {
                                payment.EftId = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftId.Value;
                                eftrequest.EFTPayment = payment;
                                eftrequest.AuditInformation = information;
                                client.BaseAddress = new Uri(gpServiceConfigurationURL);
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " DeleteBankEntryEFTTransaction Service Call Started ");
                                var response = client.PostAsJsonAsync("api/ReceivablesManagement/DeleteBankEntryEFTTransaction/", eftrequest); // we need to refer the web.api service url here.

                                if (response.Result.IsSuccessStatusCode)
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " DeleteBankEntryEFTTransaction Service Called SuccessFully ");
                                    eftResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                                    if (eftResponse.Status == ResponseStatus.Error)
                                    {
                                        logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + "Error: DeleteBankEntryEFTTransaction Service ");
                                        MessageBox.Show("Error: Deleting DeleteBankEntryEFTTransaction", Resources.STR_MESSAGE_TITLE);
                                    }
                                }
                                else
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + "Error: DeleteBankEntryEFTTransaction Service Call");
                                    MessageBox.Show("Error: Deleting DeleteBankEntryEFTTransaction", Resources.STR_MESSAGE_TITLE);
                                }
                            }
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " DeleteBankEntryEFTTransaction Service Call Ended ");
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.RedisplayButton.RunValidate();
                        }
                    }
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                    eftCustomerRemittancesEntryForm.Procedures.EftCustomerRemittancesEntryFormScrollFill.Invoke();
                }
                else
                {
                    MessageBox.Show("Please select any one of the line from the bellow list.", Resources.STR_MESSAGE_TITLE);
                }
            }
            catch (Exception exception)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + "Error: eftCustomerRemittancesDeleteButton_ClickAfterOriginal Method ");
                MessageBox.Show(exception.Message);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        /// <summary>
        /// Bank Entry Map to Email Remittance
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void eftCustomerRemittancesMapEmailRemittancesButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " eftCustomerRemittancesMapEmailRemittancesButton_ClickAfterOriginal Method ");
                if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftFileId.Value != 0)
                {
                    ReceivablesRequest eftrequest = new ReceivablesRequest();
                    ReceivablesResponse eftResponse = new ReceivablesResponse();
                    AuditInformation information = new AuditInformation
                    {
                        CompanyId = Dynamics.Globals.CompanyId.Value
                    };
                    EFTPayment payment = new EFTPayment();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Key = 1;
                    TableError error = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Get();
                    if ((error == TableError.NoError) && (error != TableError.EndOfTable))
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            payment.EftFileId = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftFileId.Value;
                            eftrequest.EFTPayment = payment;
                            eftrequest.AuditInformation = information;
                            client.BaseAddress = new Uri(gpServiceConfigurationURL);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " MapBankEntryToEmailRemittance Service Call Started ");
                            var response = client.PostAsJsonAsync("api/ReceivablesManagement/MapBankEntryToEmailRemittance/", eftrequest); // we need to refer the web.api service url here.

                            if (response.Result.IsSuccessStatusCode)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " MapBankEntryToEmailRemittance Service Called SuccessFully ");
                                eftResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                                if (eftResponse.Status == ResponseStatus.Success)
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " MapBankEntryToEmailRemittance Service Success ");
                                    MessageBox.Show("Map Email Remittance Successfully :" + Resources.STR_MESSAGE_TITLE);
                                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.RedisplayButton.RunValidate(); 
                                }
                                else
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + "Error: MapBankEntryToEmailRemittance Service ");
                                    MessageBox.Show("Error: MapBankEntryToEmailRemittance :" + Resources.STR_MESSAGE_TITLE);
                                }

                            }
                            else
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + "Error: MapBankEntryToEmailRemittance Service Call");
                                MessageBox.Show("Error: MapBankEntryToEmailRemittance Service Call", Resources.STR_MESSAGE_TITLE);
                            }
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " MapBankEntryToEmailRemittance Service Call Ended ");

                        }
                    }
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                }
                else
                {
                    MessageBox.Show("Please select any one of the line from the bellow list.", Resources.STR_MESSAGE_TITLE);
                }
            }
            catch (Exception exception)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + "Error: eftCustomerRemittancesMapEmailRemittancesButton_ClickAfterOriginal Method ");
                MessageBox.Show(exception.Message);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }




        private void eftCustomerRemittancesNameSortBy_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                eftCustomerRemittancesEntryForm.Procedures.EftCustomerRemittancesEntryFormScrollFill.Invoke();
            }
            catch (Exception exception)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + "Error: eftCustomerRemittancesMapEmailRemittancesButton_ClickAfterOriginal Method ");
                MessageBox.Show(exception.Message);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// When the focus leaves the scroll window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EftCustomerRemitScroll_LineLeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            short shortValues = 1;
            int setRowidValue = 0, rowId = 0;
            string eventLeaveAfter = "LeaveAfter";
            try
            {
                if ((eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftReferenceNumber.Value != "") &&
                (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.PaymentAmount.Value != 0) &&
                !(DateTime.Equals(eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.DateReceived.Value, "1/1/1900 12:00:00 AM")))
                {
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Key = 1;
                    TableError errorValidTemp = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Change();
                    if (errorValidTemp == TableError.NoError && errorValidTemp != TableError.EndOfTable)
                    {
                        if (
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftReferenceNumber.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftReferenceNumber.Value ||
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemReference.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftItemReference.Value ||
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftBankOriginating.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftBankOriginating.Value ||
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemAmount.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftItemAmount.Value ||
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.DateReceived.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.DateReceived.Value ||
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.PaymentAmount.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.PaymentAmount.Value ||
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.CustomerNumber.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.CustomerNumber.Value ||
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemReference.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftItemReference.Value ||
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemAmount.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftItemAmount.Value ||
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.CurrencyId.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.CurrencyId.Value
                         )
                        {

                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.IsUpdated.Value = 1;
                        }
                        else
                        {

                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.IsUpdated.Value = 0;
                        }

                        rowId = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value;
                    }
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                    

                    if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.IsUpdated.Value == 1)
                    {
                        EFTCustomerPayment eftRemittance = new EFTCustomerPayment();
                        eftRemittance.EftId = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftId.Value;
                        eftRemittance.EftAppId = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftAppId.Value;
                        eftRemittance.PaymentNumber = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.PaymentNumber.Value.ToString().Trim();
                        eftRemittance.ReferenceNumber = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftReferenceNumber.Value.ToString().Trim();
                        eftRemittance.DateReceived = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.DateReceived.Value;
                        eftRemittance.PaymentAmount = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.PaymentAmount.Value;
                        eftRemittance.CustomerID = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.CustomerNumber.Value.ToString().Trim();
                        eftRemittance.CurrencyId = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.CurrencyId.Value.ToString().Trim();
                        if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.IsfullyAppliedStr.Value == "Yes")
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.IsFullyApplied.Value = true;
                        else
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.IsFullyApplied.Value = false;
                        eftRemittance.IsFullyApplied = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.IsFullyApplied.Value;
                        eftRemittance.BankOriginating = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftBankOriginating.Value;
                        eftRemittance.ItemReferenceNumber = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftItemReference.Value;
                        eftRemittance.ItemAmount = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftItemAmount.Value;
                        eftRemittance.CreatedBy = Dynamics.Globals.UserId.Value.ToString().Trim();
                        eftRemittance.IsUpdated = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.IsUpdated.Value;
                        eftRemittance.Status = Convert.ToInt16(eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftStatus.Value);
                        eftRemittance.StatusReason = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftStatusDescription.Value;
                        eftRemittance.AccountName = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftAccountName.Value;

                        ValidateEFTCustomerRemittance(eventLeaveAfter, rowId, eftRemittance);
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftReferenceNumber.Value))
                    {
                        rowIdForClear = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value;
                        ClearEFTCustomerLine(rowIdForClear);
                        eftCustomerRemittancesEntryForm.Procedures.EftCustomerRemittancesEntryFormScrollFill.Invoke();
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftCustomerRemitScroll_LineLeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }



        /// <summary>
        /// Clear Customer Remittance window 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTCustomerRemittanceClear_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ClearRemitCustomerIdFields(Resources.STR_CustomerRemittanceEntry);
                ClearRemitDateReceivedFields(Resources.STR_CustomerRemittanceEntry);
                ClearRemitReferenceNumberFields(Resources.STR_CustomerRemittanceEntry);
                ClearRemitDocumentNumberFields(Resources.STR_CustomerRemittanceEntry);
                ClearEFTCustomerRemittance(Resources.STR_CustomerRemittanceEntry);
                eftCustomerRemittancesEntryForm.Procedures.EftCustomerRemittancesEntryFormScrollFill.Invoke();
                eftCustomerRemittancesEntryForm.EftCustomerBankEntry.BatchNumber.Clear();
                eftCustomerRemittancesEntryForm.EftCustomerBankEntry.DocumentAmount.Clear();
                eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LookupButton9.Enable();
                eftCustomerRemittancesEntryForm.EftCustomerBankEntry.MapEmailRemittances.Disable();
                eftCustomerRemittancesEntryForm.EftCustomerBankEntry.DeleteEftPayment.Disable();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTCustomerRemittanceClear_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        private void EftBankInquiryClearButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ClearRemitCustomerIdFields(Resources.STR_CustomerRemittanceInquiry);
                ClearRemitDateReceivedFields(Resources.STR_CustomerRemittanceInquiry);
                ClearRemitReferenceNumberFields(Resources.STR_CustomerRemittanceInquiry);
                ClearRemitDocumentNumberFields(Resources.STR_CustomerRemittanceInquiry);
                ClearEFTCustomerRemittance(Resources.STR_CustomerRemittanceInquiry);
                eftCustomerRemittancesInquiryForm.Procedures.EftCustomerBankInquiryFormScrollFill.Invoke();
                eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.EftBatchId.Clear();
                eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.DocumentAmount.Clear();
                eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LookupButton9.Enable();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTCustomerRemittanceClear_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        /// <summary>
        /// From customer Number Leave After Original validate the customer number..
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTRemittanceFromCustomerNumber_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!string.IsNullOrEmpty(eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromCustomerNumber.Value.ToString().Trim()))
                {
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.AllOrRange.Value = 1;
                    if (string.IsNullOrEmpty(eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToCustomerNumber.Value.ToString()))
                        eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToCustomerNumber.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromCustomerNumber.Value;


                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceFromCustomerNumber_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }






        /// <summary>
        /// EFT Remittance Window Date From Field Leave 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTRemittanceDateFrom_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                eftCustomerRemittancesEntryForm.EftCustomerBankEntry.AllOrRange.Value = 1;
                eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToDateReceived.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromDateReceived.Value;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceDateFrom_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// eftCustomerRemittancesEntry Window select all value...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AllOrRange_Change(object sender, EventArgs e)
        {
            if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.AllOrRange.Value == 0)
            {
                ClearRemitCustomerIdFields(Resources.STR_CustomerRemittanceEntry);
                ClearRemitDateReceivedFields(Resources.STR_CustomerRemittanceEntry);
                ClearRemitReferenceNumberFields(Resources.STR_CustomerRemittanceEntry);
                ClearRemitDocumentNumberFields(Resources.STR_CustomerRemittanceEntry);
                ClearEFTCustomerRemittance(Resources.STR_CustomerRemittanceEntry);
                eftCustomerRemittancesEntryForm.Procedures.EftCustomerRemittancesEntryFormScrollFill.Invoke();
            }
        }

        /// <summary>
        /// When the selection in the drop down is made
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LocalSearchBy_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                InitilizeSortBySelection(Resources.STR_CustomerRemittanceEntry);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In LocalSearchBy_Change Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        void EFTCustomerInquiryLocalSearchBy_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                InitilizeSortBySelection(Resources.STR_CustomerRemittanceInquiry);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In LocalSearchBy_Change Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        /// <summary>
        /// When the lookup button 1 is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CustomerRemitEntryLookupButton1_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalSearchBy.Value == 1)
                {
                    if (!customerLookupForm.IsOpen)
                    {
                        customerLookupForm.Open();
                        remittanceCustomerLookup = Resources.STR_CustomerRemittanceEntry;
                        remittanceCustomerLookupValue = Resources.STR_From;
                    }
                    else
                        MessageBox.Show("Customer Lookup Window already opened. Please close the window" + Resources.STR_MESSAGE_TITLE);
                }
                else if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalSearchBy.Value == 2)
                {
                    if (eftReferenceNumberLookupForm.IsOpen)
                    {
                        eftReferenceNumberLookupForm.Close();
                    }
                    eftReferenceNumberLookupForm.Open();
                    referenceLookupName = Resources.STR_CustomerRemittanceEntry;
                    eftReferenceNumberLookupForm.EftReferenceNumberLookup.RedisplayButton.RunValidate();
                    eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value = 1;

                    lookupWindowName = Resources.STR_CustomerRemittanceEntry;
                    if (!RegisterRefidLookupSelect)
                    {
                        eftReferenceNumberLookupForm.EftReferenceNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftReferenceNumberLookupSelectButton_ClickBeforeOriginal);
                        RegisterRefidLookupSelect = true;
                    }
                }
                else if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalSearchBy.Value == 4)
                {
                    if (eftDocumentNumberLookupForm.IsOpen)
                    {
                        eftDocumentNumberLookupForm.Close();
                    }
                    eftDocumentNumberLookupForm.Open();
                    documentLookupName = Resources.STR_CustomerRemittanceEntry;
                    eftDocumentNumberLookupForm.EftDocumentNumberLookup.RedisplayButton.RunValidate();
                    eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value = 1;

                    lookupWindowName = Resources.STR_CustomerRemittanceEntry;
                    if (!RegisterDocidLookupSelect)
                    {
                        eftDocumentNumberLookupForm.EftDocumentNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftDocumentNumberLookupSelectButton_ClickBeforeOriginal);
                        RegisterDocidLookupSelect = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CustomerRemitEntryLookupButton1_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// When the lookup button 2 is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CustomerRemitEntryLookupButton2_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalSearchBy.Value == 1)
                {
                    if (!customerLookupForm.IsOpen)
                    {
                        customerLookupForm.Open();
                        remittanceCustomerLookup = Resources.STR_CustomerRemittanceEntry;
                        remittanceCustomerLookupValue = Resources.STR_To;
                    }
                    else
                        MessageBox.Show("Customer Lookup Window already opened. Please close the window" + Resources.STR_MESSAGE_TITLE);
                }
                else if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalSearchBy.Value == 2)
                {
                    if (eftReferenceNumberLookupForm.IsOpen)
                    {
                        eftReferenceNumberLookupForm.Close();
                    }
                    eftReferenceNumberLookupForm.Open();
                    referenceLookupName = Resources.STR_CustomerRemittanceEntry;
                    eftReferenceNumberLookupForm.EftReferenceNumberLookup.RedisplayButton.RunValidate();
                    eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value = 2;

                    lookupWindowName = Resources.STR_CustomerRemittanceEntry;
                    if (!RegisterRefidLookupSelect)
                    {
                        eftReferenceNumberLookupForm.EftReferenceNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftReferenceNumberLookupSelectButton_ClickBeforeOriginal);
                        RegisterRefidLookupSelect = true;
                    }
                }
                else if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalSearchBy.Value == 4)
                {
                    if (eftDocumentNumberLookupForm.IsOpen)
                    {
                        eftDocumentNumberLookupForm.Close();
                    }
                    eftDocumentNumberLookupForm.Open();
                    documentLookupName = Resources.STR_CustomerRemittanceEntry;
                    eftDocumentNumberLookupForm.EftDocumentNumberLookup.RedisplayButton.RunValidate();
                    eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value = 2;

                    lookupWindowName = Resources.STR_CustomerRemittanceEntry;
                    if (!RegisterDocidLookupSelect)
                    {
                        eftDocumentNumberLookupForm.EftDocumentNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftDocumentNumberLookupSelectButton_ClickBeforeOriginal);
                        RegisterDocidLookupSelect = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CustomerRemitEntryLookupButton2_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        /// <summary>
        /// Currency ID validation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EFTEmailRemittanceCurrencyID_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (eftEmailRemittanceForm.EftEmailRemittanceEntry.CurrencyId.Value != "")
                {
                    if ((eftEmailRemittanceForm.EftEmailRemittanceEntry.CurrencyId.Value.ToString().Trim().ToUpper() != "Z-US$") &&
                        (eftEmailRemittanceForm.EftEmailRemittanceEntry.CurrencyId.Value.ToString().Trim().ToUpper() != "USD") &&
                        (eftEmailRemittanceForm.EftEmailRemittanceEntry.CurrencyId.Value.ToString().Trim().ToUpper() != "CAD") &&
                    (eftEmailRemittanceForm.EftEmailRemittanceEntry.CurrencyId.Value.ToString().Trim().ToUpper() != "EUR") &&
                    (eftEmailRemittanceForm.EftEmailRemittanceEntry.CurrencyId.Value.ToString().Trim().ToUpper() != "GBP"))
                    {
                        MessageBox.Show("Please enter valid Currency Id ", Resources.STR_MESSAGE_TITLE);
                        eftEmailRemittanceForm.EftEmailRemittanceEntry.CurrencyId.Value = "";
                        eftEmailRemittanceForm.EftEmailRemittanceEntry.CurrencyId.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ValidateRemittanceDate Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /*************************Private Methods ******************/
        /// <summary>
        /// Insert EFT Customer Remittance From service call...
        /// </summary>
        /// <param name="p"></param>
        private void FetchEftWindowDetails(string formName)
        {
            try
            {
                if (formName == Resources.STR_CustomerRemittanceEntry)
                {
                    if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.BatchNumber.Value != "")
                    {
                        if ((eftCustomerRemittancesEntryForm.EftCustomerBankEntry.AllOrRange.Value == 1 &&
                            //Date From and Date To Field should not empty
                         ((eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromDateReceived.Value != Convert.ToDateTime("1/1/1900 12:00:00 AM")) &&
                         (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToDateReceived.Value != Convert.ToDateTime("1/1/1900 12:00:00 AM"))) ||
                            //Customer Number Start and End should not empty...
                         ((eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromCustomerNumber.Value.ToString().Trim() != "") &&
                         (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToCustomerNumber.Value.ToString().Trim() != "")) ||
                            //Customer Number Start and End should not empty...
                         ((eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromReferenceNumber.Value.ToString().Trim() != "") &&
                         (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToReferenceNumber.Value.ToString().Trim() != ""))))
                        {
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.AllOrRange.Value = 0;
                        }

                        ReceivablesRequest eftCustomerRemittanceRequest = new ReceivablesRequest();
                        ReceivablesResponse eftResponse = new ReceivablesResponse();
                        AuditInformation eftAudit = new AuditInformation();
                        eftAudit.CompanyId = Dynamics.Globals.CompanyId.Value;
                        eftCustomerRemittanceRequest.BatchId = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.BatchNumber.Value.ToString().Trim();
                        eftCustomerRemittanceRequest.CustomerIdStart = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromCustomerNumber.Value.ToString().Trim() != "" ? eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromCustomerNumber.Value.ToString().Trim() : string.Empty;
                        eftCustomerRemittanceRequest.CustomerIdEnd = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToCustomerNumber.Value.ToString().Trim() != "" ? eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToCustomerNumber.Value.ToString().Trim() : string.Empty;
                        eftCustomerRemittanceRequest.DateStart = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromDateReceived.Value != null ? eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromDateReceived.Value : Convert.ToDateTime("1900/01/01");
                        eftCustomerRemittanceRequest.DateEnd = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToDateReceived.Value != null ? eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToDateReceived.Value : Convert.ToDateTime("1900/01/01");
                        eftCustomerRemittanceRequest.ReferenceNoStart = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromReferenceNumber.Value.ToString().Trim() != "" ? eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromReferenceNumber.Value.ToString().Trim() : string.Empty;
                        eftCustomerRemittanceRequest.ReferenceNoEnd = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToReferenceNumber.Value.ToString().Trim() != "" ? eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToReferenceNumber.Value.ToString().Trim() : string.Empty;
                        eftCustomerRemittanceRequest.DocNumbrStart = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromItemReference.Value.ToString().Trim() != "" ? eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromItemReference.Value.ToString().Trim() : string.Empty;
                        eftCustomerRemittanceRequest.DocNumbrend = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToItemReference.Value.ToString().Trim() != "" ? eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToItemReference.Value.ToString().Trim() : string.Empty;
                        eftCustomerRemittanceRequest.AuditInformation = eftAudit;

                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationURL);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/SalesOrderUpdate/GetEFTCustomerRemittances/", eftCustomerRemittanceRequest); // we need to refer the web.api service url here.

                            if (response.Result.IsSuccessStatusCode)
                            {
                                eftResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                                if (eftResponse.Status == ResponseStatus.Error)
                                {
                                    MessageBox.Show("Error: " + eftResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Error: Data does not saved into UpdateTaxScheduleIdToLine Table", Resources.STR_MESSAGE_TITLE);
                            }
                        }

                        if (eftResponse != null && eftResponse.EFTCustomerRemittancesList.Count > 0)
                        {
                            FillEftDetailsToFormTable(eftResponse, Resources.STR_CustomerRemittanceEntry);
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.DocumentAmount.Value = eftResponse.BatchAmount;
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.MapEmailRemittances.Enable();
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.DeleteEftPayment.Enable();
                        }
                        else
                        {
                            MessageBox.Show("No records found for mentioned search type");
                            ClearEFTCustomerRemittance(Resources.STR_CustomerRemittanceEntry);
                            eftCustomerRemittancesEntryForm.Procedures.EftCustomerRemittancesEntryFormScrollFill.Invoke();
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.MapEmailRemittances.Disable();
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.DeleteEftPayment.Disable();
                        }
                    }
                    else
                        MessageBox.Show("Please select the BatchId");
                }
                else if (formName == Resources.STR_CustomerRemittanceInquiry)
                {
                    if (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.EftBatchId.Value != "")
                    {
                        if (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.AllOrRange.Value == 0 ||
                            ///Option Button select From...
                        (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.AllOrRange.Value == 1 &&
                            //Date From and Date To Field should not empty
                        ((eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromDateReceived.Value == Convert.ToDateTime("1/1/1900 12:00:00 AM")) &&
                        (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToDateReceived.Value == Convert.ToDateTime("1/1/1900 12:00:00 AM"))) ||
                            //Customer Number Start and End should not empty...
                        ((eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromCustomerNumber.Value.ToString().Trim() == "") &&
                        (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToCustomerNumber.Value.ToString().Trim() == "")) ||
                            //Customer Number Start and End should not empty...
                        ((eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromReferenceNumber.Value.ToString().Trim() == "") &&
                        (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToReferenceNumber.Value.ToString().Trim() == ""))))
                        {
                            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.AllOrRange.Value = 0;
                        }
                        ReceivablesRequest eftCustomerRemittanceRequest = new ReceivablesRequest();
                        ReceivablesResponse eftResponse = new ReceivablesResponse();
                        AuditInformation eftAudit = new AuditInformation();
                        eftAudit.CompanyId = Dynamics.Globals.CompanyId.Value;
                        eftCustomerRemittanceRequest.BatchId = eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.EftBatchId.Value.ToString().Trim();
                        eftCustomerRemittanceRequest.CustomerIdStart = eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromCustomerNumber.Value.ToString().Trim() != "" ? eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromCustomerNumber.Value.ToString().Trim() : string.Empty;
                        eftCustomerRemittanceRequest.CustomerIdEnd = eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToCustomerNumber.Value.ToString().Trim() != "" ? eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToCustomerNumber.Value.ToString().Trim() : string.Empty;
                        eftCustomerRemittanceRequest.DateStart = eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromDateReceived.Value != null ? eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromDateReceived.Value : Convert.ToDateTime("1900/01/01");
                        eftCustomerRemittanceRequest.DateEnd = eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToDateReceived.Value != null ? eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToDateReceived.Value : Convert.ToDateTime("1900/01/01");
                        eftCustomerRemittanceRequest.ReferenceNoStart = eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromReferenceNumber.Value.ToString().Trim() != "" ? eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromReferenceNumber.Value.ToString().Trim() : string.Empty;
                        eftCustomerRemittanceRequest.ReferenceNoEnd = eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToReferenceNumber.Value.ToString().Trim() != "" ? eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToReferenceNumber.Value.ToString().Trim() : string.Empty;
                        eftCustomerRemittanceRequest.DocNumbrStart = eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromItemReference.Value.ToString().Trim() != "" ? eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromItemReference.Value.ToString().Trim() : string.Empty;
                        eftCustomerRemittanceRequest.DocNumbrend = eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToItemReference.Value.ToString().Trim() != "" ? eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToItemReference.Value.ToString().Trim() : string.Empty;
                        eftCustomerRemittanceRequest.AuditInformation = eftAudit;

                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationURL);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/SalesOrderUpdate/GetEFTCustomerRemittances/", eftCustomerRemittanceRequest); // we need to refer the web.api service url here.

                            if (response.Result.IsSuccessStatusCode)
                            {
                                eftResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                                if (eftResponse.Status == ResponseStatus.Error)
                                {
                                    MessageBox.Show("Error: " + eftResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Error: Data does not saved into UpdateTaxScheduleIdToLine Table", Resources.STR_MESSAGE_TITLE);
                            }
                        }

                        if (eftResponse != null && eftResponse.EFTCustomerRemittancesList.Count > 0)
                        {
                            FillEftDetailsToFormTable(eftResponse, Resources.STR_CustomerRemittanceInquiry);
                            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.DocumentAmount.Value = eftResponse.BatchAmount;
                        }
                        else
                        {
                            MessageBox.Show("No records found for mentioned search type");
                            ClearEFTCustomerRemittance(Resources.STR_CustomerRemittanceInquiry);
                            eftCustomerRemittancesInquiryForm.Procedures.EftCustomerBankInquiryFormScrollFill.Invoke();
                        }
                    }
                    else
                        MessageBox.Show("Please select the BatchId");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }


        /// <summary>
        /// Insert EFT Customer Remittance details to Temp Table...
        /// </summary>
        /// <param name="eftResponse"></param>
        private void FillEftDetailsToFormTable(ReceivablesResponse eftResponse, string formName)
        {
            try
            {
                short shorValue, shortStatusId;
                eftRowId = 1;
                if (formName == Resources.STR_CustomerRemittanceEntry)
                {
                    ClearEFTCustomerRemittance(formName);
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    foreach (var eftCustomerRemittance in eftResponse.EFTCustomerRemittancesList)
                    {
                        short.TryParse(eftRowId.ToString().Trim(), out shorValue);
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value = shorValue++;
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.PaymentNumber.Value = eftCustomerRemittance.PaymentNumber;
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftReferenceNumber.Value = eftCustomerRemittance.ReferenceNumber.ToString().Trim();
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.DateReceived.Value = eftCustomerRemittance.DateReceived;
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.PaymentAmount.Value = eftCustomerRemittance.PaymentAmount;
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.CustomerNumber.Value = eftCustomerRemittance.CustomerID.ToString().Trim();
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.CurrencyId.Value = eftCustomerRemittance.CurrencyID.ToString().Trim();
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftId.Value = eftCustomerRemittance.EftId;
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftAppId.Value = eftCustomerRemittance.EftAppId;
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsFullyApplied.Value = Convert.ToBoolean(eftCustomerRemittance.IsFullyApplied);
                        if (eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsFullyApplied.Value == true)
                            eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsfullyAppliedStr.Value = "Yes";
                        else
                            eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsfullyAppliedStr.Value = "No";
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftBankOriginating.Value = eftCustomerRemittance.BankOriginatingID.ToString().Trim();
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemReference.Value = eftCustomerRemittance.ItemReference;
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemAmount.Value = eftCustomerRemittance.ItemAmount;
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.TotalPaymentAmount.Value = eftCustomerRemittance.TotalPaymentAmount;
                        short.TryParse(eftCustomerRemittance.EftStatusId.ToString().Trim(), out shortStatusId);
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftStatus.Value = shortStatusId;
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftStatusDescription.Value = eftCustomerRemittance.Description.ToString();
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsUpdated.Value = 0;
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftNotes.Value = eftCustomerRemittance.Notes.ToString().Trim();
                        short.TryParse(eftCustomerRemittance.EftFileId.ToString().Trim(), out shortStatusId);
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftFileId.Value = shortStatusId;
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftAccountName.Value = eftCustomerRemittance.AccountName.ToString().Trim();
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Save();
                        eftRowId++;
                    }
                    customerBankEntryNextRowId = eftRowId;

                    eftRowId = 1;

                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                    eftCustomerRemittancesEntryForm.Procedures.EftCustomerRemittancesEntryFormScrollFill.Invoke();
                }
                else if (formName == Resources.STR_CustomerRemittanceInquiry)
                {
                    ClearEFTCustomerRemittance(formName);
                    eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.Close();
                    eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.Release();
                    foreach (var eftCustomerRemittance in eftResponse.EFTCustomerRemittancesList)
                    {
                        short.TryParse(eftRowId.ToString().Trim(), out shorValue);
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.EftRowId.Value = shorValue++;
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.PaymentNumber.Value = eftCustomerRemittance.PaymentNumber.ToString().Trim();
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.EftReferenceNumber.Value = eftCustomerRemittance.ReferenceNumber.ToString().Trim();
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.DateReceived.Value = eftCustomerRemittance.DateReceived;
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.PaymentAmount.Value = eftCustomerRemittance.PaymentAmount;
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.CustomerNumber.Value = eftCustomerRemittance.CustomerID.ToString().Trim();
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.CurrencyId.Value = eftCustomerRemittance.CurrencyID.ToString().Trim();
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.EftId.Value = eftCustomerRemittance.EftId;
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.EftAppId.Value = eftCustomerRemittance.EftAppId;
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.IsFullyApplied.Value = Convert.ToBoolean(eftCustomerRemittance.IsFullyApplied);
                        if (eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.IsFullyApplied.Value == true)
                            eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.IsfullyAppliedStr.Value = "Yes";
                        else
                            eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.IsfullyAppliedStr.Value = "No";
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.EftBankOriginating.Value = eftCustomerRemittance.BankOriginatingID.ToString().Trim();
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.EftItemReference.Value = eftCustomerRemittance.ItemReference;
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.EftItemAmount.Value = eftCustomerRemittance.ItemAmount;
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.TotalPaymentAmount.Value = eftCustomerRemittance.TotalPaymentAmount;
                        short.TryParse(eftCustomerRemittance.EftStatusId.ToString().Trim(), out shortStatusId);
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.EftStatus.Value = shortStatusId;
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.EftStatusDescription.Value = eftCustomerRemittance.Description.ToString();
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.EftNotes.Value = eftCustomerRemittance.Notes.ToString().Trim();
                        short.TryParse(eftCustomerRemittance.EftFileId.ToString().Trim(), out shortStatusId);
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.EftFileId.Value = shortStatusId;
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.EftAccountName.Value = eftCustomerRemittance.AccountName.ToString().Trim();
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.Save();
                        eftRowId++;
                    }
                    eftRowId = 1;
                    eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.Release();
                    eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.Close();
                    eftCustomerRemittancesInquiryForm.Procedures.EftCustomerBankInquiryFormScrollFill.Invoke();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void SetEFTRowIdCount()
        {
            short shortValues = 1;
            try
            {
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value;
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Key = 1;
                TableError errorValidTemp = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Change();
                if (errorValidTemp == TableError.NotFound)
                {
                    if (eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value == 0)
                    {
                        eftRowId = GetRowIdFromTempTable();
                        short.TryParse(eftRowId.ToString().Trim(), out shortValues);
                        shortValues++;
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value = shortValues;
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftId.Value = 0;
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftAppId.Value = 0;
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftStatus.Value = 0;
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftStatusDescription.Value = "";
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsUpdated.Value = 0;
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Save();
                    }

                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value = shortValues;
                }
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
            }
            catch (Exception ex)
            {

            }

        }

        private void SetMatchedEFTReferenceNumber()
        {
            short shortIdValue;
            isEftReferenceNumAvailble = false;
            int rowId = 0;
            try
            {
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                rowId = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value;
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value;
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Key = 1;
                TableError errorValidTemp = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.GetFirst();
                while (errorValidTemp == TableError.NoError && errorValidTemp != TableError.EndOfTable)
                {
                    if (eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftReferenceNumber.Value.ToString().Trim() == eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftReferenceNumber.Value)
                    {
                        eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.DateReceived.Value = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.DateReceived.Value;
                        eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.PaymentAmount.Value = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.PaymentAmount.Value;
                        eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.CurrencyId.Value = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.CurrencyId.Value;
                        eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftBankOriginating.Value = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftBankOriginating.Value;
                        eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftNotes.Value = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftNotes.Value;
                        eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.CustomerNumber.Value = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.CustomerNumber.Value;
                        if (rowId != eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value)
                        {
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftId.Value = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftId.Value;
                        }

                        isEftReferenceNumAvailble = true;
                    }

                    errorValidTemp = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.GetNext();
                }
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();

                if (!isEftReferenceNumAvailble)
                {
                    MessageBox.Show("Please enter valid bank originating number");
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftReferenceNumber.Focus();
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion BankEntry_Window


        /// <summary>
        /// Customer Number selected from look up...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomerLookupSelect_OpenAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                if (remittanceCustomerLookup == Resources.STR_CustomerMappingWindow && remittanceCustomerLookupValue == string.Empty)
                {
                    eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Value = customerLookupForm.CustomerLookup.CustomerLookupScroll.CustomerNumber.Value.ToString().Trim();
                    customerLookupForm.Close();
                    EFTCustomernumberChange();
                    eftCustomerMappingForm.Procedures.EftCustomerMappingListScrollFill.Invoke();
                }
                else if (remittanceCustomerLookup == Resources.STR_EFTPaymentRemittanceWindow && remittanceCustomerLookupValue == Resources.STR_From)
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromCustomerNumber.Value = customerLookupForm.CustomerLookup.CustomerLookupScroll.CustomerNumber.Value.ToString().Trim();
                    customerLookupForm.Close();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.AllOrRange.Value = 1;
                }
                else if (remittanceCustomerLookup == Resources.STR_EFTPaymentRemittanceWindow && remittanceCustomerLookupValue == Resources.STR_To)
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToCustomerNumber.Value = customerLookupForm.CustomerLookup.CustomerLookupScroll.CustomerNumber.Value.ToString().Trim();
                    customerLookupForm.Close();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.AllOrRange.Value = 1;
                }
                else if (remittanceCustomerLookup == Resources.STR_CustomerRemittanceEntry && remittanceCustomerLookupValue == Resources.STR_From)
                {
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromCustomerNumber.Value = customerLookupForm.CustomerLookup.CustomerLookupScroll.CustomerNumber.Value.ToString().Trim();
                    customerLookupForm.Close();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.AllOrRange.Value = 1;
                }
                else if (remittanceCustomerLookup == Resources.STR_CustomerRemittanceEntry && remittanceCustomerLookupValue == Resources.STR_To)
                {
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToCustomerNumber.Value = customerLookupForm.CustomerLookup.CustomerLookupScroll.CustomerNumber.Value.ToString().Trim();
                    customerLookupForm.Close();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.AllOrRange.Value = 1;
                }
                else if (remittanceCustomerLookup == Resources.STR_CustomerRemittanceInquiry && remittanceCustomerLookupValue == Resources.STR_From)
                {
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromCustomerNumber.Value = customerLookupForm.CustomerLookup.CustomerLookupScroll.CustomerNumber.Value.ToString().Trim();
                    customerLookupForm.Close();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.AllOrRange.Value = 1;
                }
                else if (remittanceCustomerLookup == Resources.STR_CustomerRemittanceInquiry && remittanceCustomerLookupValue == Resources.STR_To)
                {
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToCustomerNumber.Value = customerLookupForm.CustomerLookup.CustomerLookupScroll.CustomerNumber.Value.ToString().Trim();
                    customerLookupForm.Close();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.AllOrRange.Value = 1;
                }
                //else if (remittanceCustomerLookup == Resources.STR_EmailRemittanceEntry && remittanceCustomerLookupValue == Resources.STR_From)
                //{
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromCustomerNumber.Value = customerLookupForm.CustomerLookup.CustomerLookupScroll.CustomerNumber.Value.ToString().Trim();
                //    if (string.IsNullOrEmpty(eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToCustomerNumber.Value))
                //        eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToCustomerNumber.Value = customerLookupForm.CustomerLookup.CustomerLookupScroll.CustomerNumber.Value.ToString().Trim();
                //    customerLookupForm.Close();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.AllOrRange.Value = 1;
                //}
                //else if (remittanceCustomerLookup == Resources.STR_EmailRemittanceEntry && remittanceCustomerLookupValue == Resources.STR_To)
                //{
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToCustomerNumber.Value = customerLookupForm.CustomerLookup.CustomerLookupScroll.CustomerNumber.Value.ToString().Trim();
                //    customerLookupForm.Close();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.AllOrRange.Value = 1;
                //}
                else if (remittanceCustomerLookup == Resources.STR_EmailRemittanceInquiry && remittanceCustomerLookupValue == Resources.STR_From)
                {
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromCustomerNumber.Value = customerLookupForm.CustomerLookup.CustomerLookupScroll.CustomerNumber.Value.ToString().Trim();
                    customerLookupForm.Close();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.AllOrRange.Value = 1;
                }
                else if (remittanceCustomerLookup == Resources.STR_EmailRemittanceInquiry && remittanceCustomerLookupValue == Resources.STR_To)
                {
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToCustomerNumber.Value = customerLookupForm.CustomerLookup.CustomerLookupScroll.CustomerNumber.Value.ToString().Trim();
                    customerLookupForm.Close();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.AllOrRange.Value = 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        /// <summary>
        /// GP Customer lookup window Value assign...
        /// </summary>
        /// <param name="formName"></param>
        private void CustomerLookup(string formName, string value)
        {
            if (formName == Resources.STR_CustomerMappingWindow && value == string.Empty)
            {
                eftCustomerMappingForm.EftCustomerMapping.CustomerNumber.Value = customerLookupForm.CustomerLookup.CustomerLookupScroll.CustomerNumber.Value.ToString().Trim();
                customerLookupForm.Close();
                EFTCustomernumberChange();
                eftCustomerMappingForm.Procedures.EftCustomerMappingListScrollFill.Invoke();
            }
            else if (formName == Resources.STR_EFTPaymentRemittanceWindow && value == Resources.STR_From)
            {
                eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromCustomerNumber.Value = customerLookupForm.CustomerLookup.CustomerLookupScroll.CustomerNumber.Value.ToString().Trim();
                customerLookupForm.Close();
            }
            else if (formName == Resources.STR_EFTPaymentRemittanceWindow && value == Resources.STR_To)
            {
                eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToCustomerNumber.Value = customerLookupForm.CustomerLookup.CustomerLookupScroll.CustomerNumber.Value.ToString().Trim();
                customerLookupForm.Close();
            }
        }






        /// <summary>
        /// EFT Customer Remittance Redisplay Button click ...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTCustomerRemittanceInquiry_ClickAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                FetchEftWindowDetails(Resources.STR_CustomerRemittanceInquiry);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }




        private void ClearEFTCustomerLine(int rowId)
        {
            try
            {
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value;
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Key = 1;
                TableError tableRemove = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Change();
                if (tableRemove == TableError.NoError && tableRemove != TableError.EndOfTable)
                {
                    if (eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value == rowId)
                    {
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Remove();
                    }
                }
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// Clear EFT Customer Remittance Temp table...
        /// </summary>
        /// <param name="eftResponse"></param>
        private void ClearEFTCustomerRemittance(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_CustomerRemittanceEntry)
                {
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    TableError tableRemove = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.ChangeFirst();
                    while (tableRemove == TableError.NoError && tableRemove != TableError.EndOfTable)
                    {
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Remove();
                        tableRemove = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.ChangeNext();
                    }
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                }
                else if (formName == Resources.STR_CustomerRemittanceInquiry)
                {
                    eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.Close();
                    eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.Release();
                    TableError tableRemove = eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.ChangeFirst();
                    while (tableRemove == TableError.NoError && tableRemove != TableError.EndOfTable)
                    {
                        eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.Remove();
                        tableRemove = eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.ChangeNext();
                    }
                    eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.Release();
                    eftCustomerRemittancesInquiryForm.Tables.EftCustomerRemitTemp.Close();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearEFTCustomerRemittance Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// EFT Customer Remittance Save Button...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTCustomerRemittanceSave_ClickAfterOriginal(object sender, EventArgs e)
        {
            string eventSave = "SaveEvent";
            try
            {
                bool result = ValidateEFTCustomerRemittance(eventSave, 0, null);
                SaveEFTCustomerRemittanceDetail();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        /// <summary>
        /// Validate EFT Customer Remittance before save
        /// </summary>
        private bool ValidateEFTCustomerRemittance(string eventName, int rowId, EFTCustomerPayment eftBankEntry)
        {
            bool result = false;
            try
            {
                ReceivablesRequest eftRequest = new ReceivablesRequest();
                AuditInformation auditInfo = new AuditInformation();
                List<EFTCustomerPayment> eftCustomerRemittanceList = new List<EFTCustomerPayment>();

                if (eventName == "LeaveAfter")
                {
                    eftCustomerRemittanceList.Add(eftBankEntry);
                }
                else
                {
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    TableError tableErrorDate = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.GetFirst();
                    while (tableErrorDate == TableError.NoError && tableErrorDate != TableError.EndOfTable)
                    {
                        EFTCustomerPayment eftRemittance = new EFTCustomerPayment();
                        eftRemittance.EftId = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftId.Value;
                        eftRemittance.EftAppId = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftAppId.Value;
                        eftRemittance.PaymentNumber = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.PaymentNumber.Value.ToString().Trim();
                        eftRemittance.ReferenceNumber = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftReferenceNumber.Value.ToString().Trim();
                        eftRemittance.DateReceived = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.DateReceived.Value;
                        eftRemittance.PaymentAmount = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.PaymentAmount.Value;
                        eftRemittance.CustomerID = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.CustomerNumber.Value.ToString().Trim();
                        eftRemittance.CurrencyId = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.CurrencyId.Value.ToString().Trim();
                        eftRemittance.IsFullyApplied = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsFullyApplied.Value;
                        eftRemittance.BankOriginating = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftBankOriginating.Value;
                        eftRemittance.ItemReferenceNumber = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemReference.Value;
                        eftRemittance.ItemAmount = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemAmount.Value;
                        eftRemittance.CreatedBy = Dynamics.Globals.UserId.Value.ToString().Trim();
                        eftRemittance.IsUpdated = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsUpdated.Value;
                        eftRemittance.Status = Convert.ToInt16(eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftStatus.Value);
                        eftRemittance.StatusReason = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftStatusDescription.Value;
                        eftRemittance.AccountName = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftAccountName.Value.ToString().Trim();
                        eftCustomerRemittanceList.Add(eftRemittance);
                        tableErrorDate = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.GetNext();
                    }
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                }

                eftRequest.CompanyId = Dynamics.Globals.CompanyId.Value;
                eftRequest.EFTCustomerPaymentList = eftCustomerRemittanceList;

                if (eftRequest.EFTCustomerPaymentList != null && eftRequest.EFTCustomerPaymentList.Count > 0)
                {
                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SalesOrderUpdate/ValidateEFTCustomerRemittanceSummary/", eftRequest); // we need to refer the web.api service url here.

                        if (response.Result.IsSuccessStatusCode)
                        {
                            ReceivablesResponse eftResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                            if (eftResponse.Status == ResponseStatus.Success)
                            {
                                if (eventName == "SaveEvent")
                                {
                                    result = FillCustomerRemittanceScrollResponse(eftResponse);
                                    if (!result)
                                        MessageBox.Show("One or more data having issue. Please check the data and resave");
                                }
                                else
                                {
                                    FillCustomerScrollResponseForLeaveAfter(eftResponse, rowId);
                                }
                            }
                            else if (eftResponse.Status == ResponseStatus.Error)
                            {
                                MessageBox.Show("Error: " + eftResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Error: Data does not saved into SaveEFTCustomerDetail Table", Resources.STR_MESSAGE_TITLE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            finally
            {

            }
            return result;

        }

        private void FillCustomerScrollResponseForLeaveAfter(ReceivablesResponse eftResponse, int rowId)
        {
            short shortValue = 1;

            eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
            eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();

            foreach (var eftCustomerRemittance in eftResponse.EFTCustomerPaymentList)
            {
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value;
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Key = 1;
                TableError errorValidTemp = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Change();
                if (errorValidTemp == TableError.NoError)
                {
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.PaymentNumber.Value = eftCustomerRemittance.PaymentNumber;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftReferenceNumber.Value = eftCustomerRemittance.ReferenceNumber.ToString().Trim();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.DateReceived.Value = eftCustomerRemittance.DateReceived;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.PaymentAmount.Value = eftCustomerRemittance.PaymentAmount;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.CustomerNumber.Value = eftCustomerRemittance.CustomerID.ToString().Trim();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftId.Value = eftCustomerRemittance.EftId;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftAppId.Value = eftCustomerRemittance.EftAppId;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsFullyApplied.Value = Convert.ToBoolean(eftCustomerRemittance.IsFullyApplied);
                    if (eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsFullyApplied.Value == true)
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsfullyAppliedStr.Value = "Yes";
                    else
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsfullyAppliedStr.Value = "No";
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftBankOriginating.Value = eftCustomerRemittance.BankOriginating.ToString().Trim();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemReference.Value = eftCustomerRemittance.ItemReferenceNumber;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemAmount.Value = eftCustomerRemittance.ItemAmount;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.CurrencyId.Value = eftCustomerRemittance.CurrencyId;
                    short.TryParse(eftCustomerRemittance.IsUpdated.ToString().Trim(), out shortValue);
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsUpdated.Value = shortValue;
                    short.TryParse(eftCustomerRemittance.Status.ToString().Trim(), out shortValue);
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftStatus.Value = shortValue;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftStatusDescription.Value = eftCustomerRemittance.StatusReason.ToString().Trim();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftAccountName.Value = eftCustomerRemittance.AccountName.ToString().Trim();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Save();
                }
            }
            eftRowId = 1;
            eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
            eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
            eftCustomerRemittancesEntryForm.Procedures.EftCustomerRemittancesEntryFormScrollFill.Invoke();
        }

        private bool FillCustomerRemittanceScrollResponse(ReceivablesResponse eftResponse)
        {
            try
            {
                short shortValue, shortRowId = 1;
                eftRowId = 1;
                ClearEFTCustomerRemittance(Resources.STR_CustomerRemittanceEntry);
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                foreach (var eftCustomerRemittance in eftResponse.EFTCustomerPaymentList)
                {
                    short.TryParse(eftRowId.ToString().Trim(), out shortRowId);
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value = shortRowId;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.PaymentNumber.Value = eftCustomerRemittance.PaymentNumber;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftReferenceNumber.Value = eftCustomerRemittance.ReferenceNumber.ToString().Trim();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.DateReceived.Value = eftCustomerRemittance.DateReceived;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.PaymentAmount.Value = eftCustomerRemittance.PaymentAmount;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.CustomerNumber.Value = eftCustomerRemittance.CustomerID.ToString().Trim();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftId.Value = eftCustomerRemittance.EftId;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftAppId.Value = eftCustomerRemittance.EftAppId;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsFullyApplied.Value = Convert.ToBoolean(eftCustomerRemittance.IsFullyApplied);
                    if (eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsFullyApplied.Value == true)
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsfullyAppliedStr.Value = "Yes";
                    else
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsfullyAppliedStr.Value = "No";
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftBankOriginating.Value = eftCustomerRemittance.BankOriginating.ToString().Trim();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemReference.Value = eftCustomerRemittance.ItemReferenceNumber;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemAmount.Value = eftCustomerRemittance.ItemAmount;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.CurrencyId.Value = eftCustomerRemittance.CurrencyId;
                    short.TryParse(eftCustomerRemittance.IsUpdated.ToString().Trim(), out shortValue);
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsUpdated.Value = shortValue;
                    short.TryParse(eftCustomerRemittance.Status.ToString().Trim(), out shortValue);
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftStatus.Value = shortValue;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftStatusDescription.Value = eftCustomerRemittance.StatusReason.ToString().Trim();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftAccountName.Value = eftCustomerRemittance.AccountName.ToString().Trim();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Save();

                    if (eftCustomerRemittance.Status != 0)
                        isValidatedSucess = false;
                    else
                        isValidatedSucess = true;
                    countValue++;
                    eftRowId++;
                }
                eftRowId = 1;
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                eftCustomerRemittancesEntryForm.Procedures.EftCustomerRemittancesEntryFormScrollFill.Invoke();

                if (eftResponse.EFTCustomerPaymentList != null &&
                    eftResponse.EFTCustomerPaymentList.Count == countValue && isValidatedSucess == true)
                {
                    isValidatedSucess = true;
                }
                countValue = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

            return isValidatedSucess;
        }

        /// <summary>
        /// Save EFT Customer Remittance save details
        /// </summary>
        private void SaveEFTCustomerRemittanceDetail()
        {

            eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
            eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();

            ReceivablesRequest eftRequest = new ReceivablesRequest();
            AuditInformation auditInfo = new AuditInformation();
            List<EFTPayment> eftCustomerRemittanceList = new List<EFTPayment>();

            TableError eftCustomerRemittance = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.GetFirst();

            while (eftCustomerRemittance == TableError.NoError && eftCustomerRemittance != TableError.EndOfTable)
            {
                EFTPayment eftRemittance = new EFTPayment();
                eftRemittance.EftId = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftId.Value;
                eftRemittance.EftAppId = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftAppId.Value;
                eftRemittance.PaymentNumber = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.PaymentNumber.Value.ToString().Trim();
                eftRemittance.ReferenceNumber = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftReferenceNumber.Value.ToString().Trim();
                eftRemittance.DateReceived = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.DateReceived.Value;
                eftRemittance.PaymentAmount = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.PaymentAmount.Value;
                eftRemittance.CustomerID = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.CustomerNumber.Value.ToString().Trim();
                eftRemittance.CurrencyID = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.CurrencyId.Value.ToString().Trim();
                eftRemittance.IsFullyApplied = Convert.ToInt16(eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsFullyApplied.Value);
                eftRemittance.BankOriginatingID = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftBankOriginating.Value;
                eftRemittance.ItemReference = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemReference.Value;
                eftRemittance.ItemAmount = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemAmount.Value;
                eftRemittance.CreatedBy = Dynamics.Globals.UserId.Value.ToString().Trim();
                eftRemittance.IsUpdated = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsUpdated.Value;
                eftRemittance.EftStatusId = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftStatus.Value;
                eftRemittance.Notes = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftNotes.Value.ToString().Trim();
                eftRemittance.EftFileId = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftFileId.Value;
                eftCustomerRemittanceList.Add(eftRemittance);
                eftRemittance = null;
                eftCustomerRemittance = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.GetNext();
            }
            eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
            eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();

            auditInfo.CreatedBy = Dynamics.Globals.UserId.Value.ToString().Trim();
            auditInfo.CompanyId = Dynamics.Globals.CompanyId.Value;
            eftRequest.EFTPaymentList = eftCustomerRemittanceList;
            eftRequest.AuditInformation = auditInfo;

            if (eftRequest.EFTPaymentList != null && eftRequest.EFTPaymentList.Count > 0)
            {
                // Service call ...
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(gpServiceConfigurationURL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.PostAsJsonAsync("api/SalesOrderUpdate/SaveEFTCustomerRemittances/", eftRequest); // we need to refer the web.api service url here.

                    if (response.Result.IsSuccessStatusCode)
                    {
                        ReceivablesResponse eftResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                        if (eftResponse.Status == ResponseStatus.Success)
                        {
                            //eftCustomerRemittancesEntryForm.EftCustomerBankEntry.BatchNumber.Clear();
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.DocumentAmount.Clear();
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LookupButton9.Enable();
                            ClearEFTCustomerRemittance(Resources.STR_CustomerRemittanceEntry);
                            eftCustomerRemittancesEntryForm.Procedures.EftCustomerRemittancesEntryFormScrollFill.Invoke();
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.MapEmailRemittances.Disable();
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.DeleteEftPayment.Disable();
                        }
                        else if (eftResponse.Status == ResponseStatus.Error)
                        {
                            MessageBox.Show("Error: " + eftResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error: Data does not saved into SaveEFTCustomerDetail Table", Resources.STR_MESSAGE_TITLE);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please check the data and resave");
            }
        }

        void EFTCustomerRemittanceEntry_OpenAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalSearchBy.Value = 1;
                eftCustomerRemittancesEntryForm.EftCustomerBankEntry.MapEmailRemittances.Disable();
                eftCustomerRemittancesEntryForm.EftCustomerBankEntry.DeleteEftPayment.Disable();
                eftCustomerRemittancesEntryForm.Procedures.EftCustomerRemittancesEntryFormScrollFill.Invoke();
            }
            catch (Exception)
            {
                throw;
            }
        }



        private void EFTRemittanceFromCustomerNumberInquiry_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!string.IsNullOrEmpty(eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromCustomerNumber.Value.ToString().Trim()))
                {
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.AllOrRange.Value = 1;
                    if (string.IsNullOrEmpty(eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToCustomerNumber.Value.ToString()))
                        eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToCustomerNumber.Value = eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromCustomerNumber.Value;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceFromCustomerNumberInquiry_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }

        /// <summary>
        /// validate Referencer number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTRemittanceFromReference_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!string.IsNullOrEmpty(eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromReferenceNumber.Value.ToString().Trim()))
                {
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.AllOrRange.Value = 1;
                    if (string.IsNullOrEmpty(eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToReferenceNumber.Value.ToString()))
                        eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToReferenceNumber.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromReferenceNumber.Value;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceFromReference_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        private void EFTRemittanceFromReferenceInquiry_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!string.IsNullOrEmpty(eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromReferenceNumber.Value.ToString().Trim()))
                {
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.AllOrRange.Value = 1;
                    if (string.IsNullOrEmpty(eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToReferenceNumber.Value.ToString()))
                        eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToReferenceNumber.Value = eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromReferenceNumber.Value;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceFromReferenceInquiry_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// validate Item Reference Number...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTRemittanceFromItemReference_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!string.IsNullOrEmpty(eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromItemReference.Value.ToString().Trim()))
                {
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.AllOrRange.Value = 1;
                    if (string.IsNullOrEmpty(eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToItemReference.Value.ToString()))
                        eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToItemReference.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromItemReference.Value;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceFromItemReference_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        private void EFTRemittanceFromItemReferenceInquiry_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!string.IsNullOrEmpty(eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromItemReference.Value.ToString().Trim()))
                {
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.AllOrRange.Value = 1;
                    if (string.IsNullOrEmpty(eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToItemReference.Value.ToString()))
                        eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToItemReference.Value = eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromItemReference.Value;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceFromItemReferenceInquiry_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        /// <summary>
        /// When the focus leaves the ref. no.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EftReferenceNumber_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();

            try
            {
                if (!string.IsNullOrEmpty(eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftReferenceNumber.Value.ToString().Trim()))
                {
                    SetEFTRowIdCount();
                    SetMatchedEFTReferenceNumber();
                }
                
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In FetchCustomerIdForReference Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }

        void EftBankReferenceNumber_EnterBeforeOriginal(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftReferenceNumber.Value.ToString().Trim()))
            {
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value;
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Key = 1;
                TableError errorValidTemp = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Get();
                if (errorValidTemp == TableError.NoError)
                {
                    if (eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftId.Value != 0)
                    {
                        eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftItemReference.Focus();
                    }
                }
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
            }
        }

        /// <summary>
        /// When the reference number lookup is seleceted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void eftReferenceNumberLookupFormRedisplayButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                FetchReferenceRemittance(referenceLookupName);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In eftDocNumbrLookup Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        private void FetchReferenceRemittance(string referenceLookupName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (referenceLookupName == Resources.STR_EmailRemittanceEntry || referenceLookupName == Resources.STR_EmailRemittanceInquiry)
                {
                    ReceivablesRequest receivablesRequest = new ReceivablesRequest();
                    ReceivablesResponse receivablesResponse = new ReceivablesResponse();
                    AuditInformation auditInformation = new AuditInformation();
                    bool recordsExists = false;

                    auditInformation.CompanyId = Dynamics.Globals.CompanyId.Value;
                    receivablesRequest.AuditInformation = auditInformation;
                    receivablesRequest.Searchtype = 1;
                    //Service Call
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SalesOrderUpdate/FetchReferenceId", receivablesRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            receivablesResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                            if (receivablesResponse.Status == ResponseStatus.Success)
                            {
                                recordsExists = true;
                            }
                            else
                            {
                                recordsExists = false;
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In FetchReferenceId id Method (FetchFetchReferenceId): " + receivablesResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + receivablesResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                        }
                        else
                        {
                            recordsExists = false;
                            MessageBox.Show("Error: Data does not FetchReferenceId idLookup Table", Resources.STR_MESSAGE_TITLE);
                        }
                    }

                    if (receivablesResponse != null && recordsExists && receivablesResponse.EFTPayment.EFTReferenceNumber.Count != 0)
                    {
                        eftReferenceNumberLookupForm.Tables.EftReferenceNumberTemp.Close();
                        eftReferenceNumberLookupForm.Tables.EftReferenceNumberTemp.Release();

                        foreach (var eftRefNumbrLookup in receivablesResponse.EFTPayment.EFTReferenceNumber)
                        {
                            eftReferenceNumberLookupForm.Tables.EftReferenceNumberTemp.Release();
                            eftReferenceNumberLookupForm.Tables.EftReferenceNumberTemp.EftReferenceNumber.Value = eftRefNumbrLookup.ToString().Trim();

                            eftReferenceNumberLookupForm.Tables.EftReferenceNumberTemp.Save();
                        }
                        eftReferenceNumberLookupForm.Tables.EftReferenceNumberTemp.Release();
                        eftReferenceNumberLookupForm.Tables.EftReferenceNumberTemp.Close();
                        eftReferenceNumberLookupForm.Procedures.EftReferenceNumberLookupFormScrollFill.Invoke();

                    }
                }
                else
                {
                    ReceivablesRequest receivablesRequest = new ReceivablesRequest();
                    ReceivablesResponse receivablesResponse = new ReceivablesResponse();
                    AuditInformation auditInformation = new AuditInformation();
                    bool recordsExists = false;

                    auditInformation.CompanyId = Dynamics.Globals.CompanyId.Value;
                    receivablesRequest.AuditInformation = auditInformation;
                    receivablesRequest.Searchtype = 0;
                    //Service Call
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SalesOrderUpdate/FetchReferenceId", receivablesRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            receivablesResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                            if (receivablesResponse.Status == ResponseStatus.Success)
                            {
                                recordsExists = true;
                            }
                            else
                            {
                                recordsExists = false;
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In FetchReferenceId id Method (FetchFetchReferenceId): " + receivablesResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + receivablesResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                        }
                        else
                        {
                            recordsExists = false;
                            MessageBox.Show("Error: Data does not FetchReferenceId idLookup Table", Resources.STR_MESSAGE_TITLE);
                        }
                    }

                    if (receivablesResponse != null && recordsExists && receivablesResponse.EFTPayment.EFTReferenceNumber.Count != 0)
                    {
                        eftReferenceNumberLookupForm.Tables.EftReferenceNumberTemp.Close();
                        eftReferenceNumberLookupForm.Tables.EftReferenceNumberTemp.Release();

                        foreach (var eftRefNumbrLookup in receivablesResponse.EFTPayment.EFTReferenceNumber)
                        {
                            eftReferenceNumberLookupForm.Tables.EftReferenceNumberTemp.Release();
                            eftReferenceNumberLookupForm.Tables.EftReferenceNumberTemp.EftReferenceNumber.Value = eftRefNumbrLookup.ToString().Trim();

                            eftReferenceNumberLookupForm.Tables.EftReferenceNumberTemp.Save();
                        }
                        eftReferenceNumberLookupForm.Tables.EftReferenceNumberTemp.Release();
                        eftReferenceNumberLookupForm.Tables.EftReferenceNumberTemp.Close();
                        eftReferenceNumberLookupForm.Procedures.EftReferenceNumberLookupFormScrollFill.Invoke();

                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In eftDocNumbrLookup Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// When teh document number lookup is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EftDocumentNumberLookupRedisplayButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                FetchDocumentNumber(documentLookupName);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In eftDocNumbrLookup Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }


        /// <summary>
        /// Fetch Document Number based on the lookup
        /// </summary>
        /// <param name="documentLookupName"></param>
        private void FetchDocumentNumber(string documentLookupName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (documentLookupName == Resources.STR_EmailRemittanceEntry || documentLookupName == Resources.STR_EmailRemittanceInquiry)
                {

                    ReceivablesRequest receivablesRequest = new ReceivablesRequest();
                    ReceivablesResponse receivablesResponse = new ReceivablesResponse();
                    AuditInformation auditInformation = new AuditInformation();


                    bool recordsExists = false;

                    auditInformation.CompanyId = Dynamics.Globals.CompanyId.Value;
                    receivablesRequest.AuditInformation = auditInformation;
                    receivablesRequest.Actiontype = 1;
                    //Service Call
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SalesOrderUpdate/FetchDocumentNumber", receivablesRequest); // we need to refer the web.api service url here.
                        if (response.Result.IsSuccessStatusCode)
                        {
                            receivablesResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                            if (receivablesResponse.Status == ResponseStatus.Success)
                            {
                                recordsExists = true;
                            }
                            else
                            {
                                recordsExists = false;
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In customer id Method (FetchEFTDocnumber): " + receivablesResponse.ErrorMessage.ToString());
                                MessageBox.Show("Error: " + receivablesResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                        }
                        else
                        {
                            recordsExists = false;
                            MessageBox.Show("Error: Data does notFetchEFTDocnumber idLookup Table", Resources.STR_MESSAGE_TITLE);
                        }
                    }

                    if (receivablesResponse != null && recordsExists && receivablesResponse.EFTPayment.EFTItemReference.Count != 0)
                    {
                        eftDocumentNumberLookupForm.Tables.EftDocumentNumberTemp.Close();
                        eftDocumentNumberLookupForm.Tables.EftDocumentNumberTemp.Release();

                        foreach (var eftDocNumbrLookup in receivablesResponse.EFTPayment.EFTItemReference)
                        {
                            eftDocumentNumberLookupForm.Tables.EftDocumentNumberTemp.Release();
                            eftDocumentNumberLookupForm.Tables.EftDocumentNumberTemp.EftItemReference.Value = eftDocNumbrLookup.ToString().Trim();

                            eftDocumentNumberLookupForm.Tables.EftDocumentNumberTemp.Save();
                        }
                        eftDocumentNumberLookupForm.Tables.EftDocumentNumberTemp.Release();
                        eftDocumentNumberLookupForm.Tables.EftDocumentNumberTemp.Close();
                        eftDocumentNumberLookupForm.Procedures.EftDocumentNumberLookupFormScrollFill.Invoke();

                    }
                }
                else
                {
                    if (documentLookupName == Resources.STR_CustomerRemittanceEntry || documentLookupName == Resources.STR_CustomerRemittanceInquiry)
                    {
                        ReceivablesRequest receivablesRequest = new ReceivablesRequest();
                        ReceivablesResponse receivablesResponse = new ReceivablesResponse();
                        AuditInformation auditInformation = new AuditInformation();
                        bool recordsExists = false;

                        auditInformation.CompanyId = Dynamics.Globals.CompanyId.Value;
                        receivablesRequest.AuditInformation = auditInformation;
                        receivablesRequest.Actiontype = 0;
                        //Service Call
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationURL);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/SalesOrderUpdate/FetchDocumentNumber", receivablesRequest); // we need to refer the web.api service url here.
                            if (response.Result.IsSuccessStatusCode)
                            {
                                receivablesResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                                if (receivablesResponse.Status == ResponseStatus.Success)
                                {
                                    recordsExists = true;
                                }
                                else
                                {
                                    recordsExists = false;
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In customer id Method (FetchEFTDocnumber): " + receivablesResponse.ErrorMessage.ToString());
                                    MessageBox.Show("Error: " + receivablesResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                            }
                            else
                            {
                                recordsExists = false;
                                MessageBox.Show("Error: Data does notFetchEFTDocnumber idLookup Table", Resources.STR_MESSAGE_TITLE);
                            }
                        }

                        if (receivablesResponse != null && recordsExists && receivablesResponse.EFTPayment.EFTItemReference.Count != 0)
                        {
                            eftDocumentNumberLookupForm.Tables.EftDocumentNumberTemp.Close();
                            eftDocumentNumberLookupForm.Tables.EftDocumentNumberTemp.Release();

                            foreach (var eftDocNumbrLookup in receivablesResponse.EFTPayment.EFTItemReference)
                            {
                                eftDocumentNumberLookupForm.Tables.EftDocumentNumberTemp.Release();
                                eftDocumentNumberLookupForm.Tables.EftDocumentNumberTemp.EftItemReference.Value = eftDocNumbrLookup.ToString().Trim();

                                eftDocumentNumberLookupForm.Tables.EftDocumentNumberTemp.Save();
                            }
                            eftDocumentNumberLookupForm.Tables.EftDocumentNumberTemp.Release();
                            eftDocumentNumberLookupForm.Tables.EftDocumentNumberTemp.Close();
                            eftDocumentNumberLookupForm.Procedures.EftDocumentNumberLookupFormScrollFill.Invoke();

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In eftDocNumbrLookup Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        /// <summary>
        /// when the redisplay button is invoked of customer lookup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void eftCustomerIdLookupFormRedisplayButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            ReceivablesRequest receivablesRequest = new ReceivablesRequest();
            ReceivablesResponse receivablesResponse = new ReceivablesResponse();
            AuditInformation auditInformation = new AuditInformation();
            try
            {

                bool recordsExists = false;

                auditInformation.CompanyId = Dynamics.Globals.CompanyId.Value;
                receivablesRequest.AuditInformation = auditInformation;
                //Service Call
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(gpServiceConfigurationURL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.PostAsJsonAsync("api/SalesOrderUpdate/FetchCustomerId", receivablesRequest); // we need to refer the web.api service url here.
                    if (response.Result.IsSuccessStatusCode)
                    {
                        receivablesResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                        if (receivablesResponse.Status == ResponseStatus.Success)
                        {
                            recordsExists = true;
                        }
                        else
                        {
                            recordsExists = false;
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In customer id Method (FetchEFTcustomerid): " + receivablesResponse.ErrorMessage.ToString());
                            MessageBox.Show("Error: " + receivablesResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                        }
                    }
                    else
                    {
                        recordsExists = false;
                        MessageBox.Show("Error: Data does not Fetch customer idLookup Table", Resources.STR_MESSAGE_TITLE);
                    }
                }

                if (receivablesResponse != null && recordsExists && receivablesResponse.EFTPayment.EFTCustomerId.Count != 0)
                {
                    eftCustomerIdLookupForm.Tables.CustomerIdTemp.Close();
                    eftCustomerIdLookupForm.Tables.CustomerIdTemp.Release();

                    foreach (var eftCustomerIdLookup in receivablesResponse.EFTPayment.EFTCustomerId)
                    {
                        eftCustomerIdLookupForm.Tables.CustomerIdTemp.Release();
                        eftCustomerIdLookupForm.Tables.CustomerIdTemp.CustomerNumber.Value = eftCustomerIdLookup.ToString().Trim();

                        eftCustomerIdLookupForm.Tables.CustomerIdTemp.Save();
                    }
                    eftCustomerIdLookupForm.Tables.CustomerIdTemp.Release();
                    eftCustomerIdLookupForm.Tables.CustomerIdTemp.Close();

                    eftCustomerIdLookupForm.Procedures.CustomerIdLookupFormScrollFill.Invoke();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In eftCustomerIdLookupFormRedisplayButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }

        /// <summary>
        /// InsertEFT Customer Details to Temp Table.
        /// </summary>
        /// <param name="eftResponse"></param>
        private void InsertEFTCustomerIdDetailsForLookup(ReceivablesResponse eftResponse)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ClearEFTCustomerIdLookupScroll(eftResponse);
                eftCustomerIdLookupForm.Tables.CustomerIdTemp.Close();
                eftCustomerIdLookupForm.Tables.CustomerIdTemp.Release();

                foreach (var eftCustomerSource in eftResponse.CustomerInformation.EftCTXCustomerSourceList)
                {
                    eftCustomerIdLookupForm.Tables.CustomerIdTemp.CustomerNumber.Value = eftCustomerSource.ToString().Trim();

                    eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Save();
                }
                eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Release();
                eftCustomerMappingForm.Tables.EftCtxCustomerSourceTemp.Close();
                eftCustomerMappingForm.Procedures.EftCustomerMappingListScrollFill.Invoke();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertEFTCustomerIdDetailsForLookup Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Clear EFT Customer id Temp. for lookup..
        /// </summary>
        /// <param name="eftResponse"></param>
        private void ClearEFTCustomerIdLookupScroll(ReceivablesResponse eftResponse)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                eftCustomerIdLookupForm.Tables.CustomerIdTemp.Close();
                eftCustomerIdLookupForm.Tables.CustomerIdTemp.Release();
                TableError tableRemove = eftCustomerIdLookupForm.Tables.CustomerIdTemp.ChangeFirst();
                while (tableRemove == TableError.NoError && tableRemove != TableError.EndOfTable)
                {
                    eftCustomerIdLookupForm.Tables.CustomerIdTemp.Remove();
                    tableRemove = eftCustomerIdLookupForm.Tables.CustomerIdTemp.ChangeNext();
                }
                eftCustomerIdLookupForm.Tables.CustomerIdTemp.Release();
                eftCustomerIdLookupForm.Tables.CustomerIdTemp.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearEFTCustomerIdLookupScroll Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the ok button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void eftCustomerRemittancesInquiryFormOkButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                eftCustomerRemittancesInquiryForm.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In eftCustomerRemittancesInquiryFormOkButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the customer lookup is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EftCustomerIdLookup_CloseAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                lookupWindowName = string.Empty;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftCustomerIdLookup_CloseAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the document lookup is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EftDocumentNumberLookup_CloseAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                lookupWindowName = string.Empty;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftCustomerIdLookup_CloseAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the refrence lookup is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EftReferenceNumberLookup_CloseAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                lookupWindowName = string.Empty;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftReferenceNumberLookup_CloseAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// When the cancel button of customer lookup is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void eftCustomerIdLookupFormCancelButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                eftCustomerIdLookupForm.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In eftCustomerIdLookupFormCancelButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the cancel button of document number is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void eftDocumentNumberLookupFormCancelButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                eftDocumentNumberLookupForm.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In eftDocumentNumberLookupFormCancelButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// when cancel button of reference of lookup is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void eftReferenceNumberLookupFormCancelButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                eftReferenceNumberLookupForm.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In eftReferenceNumberLookupFormCancelButton_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        /// <summary>
        /// When the inquiry window is opened
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CustomerRemittanceInquiry_OpenBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalSearchBy.Value = 1;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CustomerRemittanceInquiry_OpenBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }



        private int GetRowIdKey()
        {
            int idValue = 0;
            eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
            eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
            TableError errorValidTemp = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Change();
            if (errorValidTemp != TableError.EndOfTable && errorValidTemp == TableError.NoError)
            {
                if (eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value == 0)
                    idValue = 0;
            }
            eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
            eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();

            return idValue;
        }


        /// <summary>
        /// When the focus leaves the scroll window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EftCustomerRemitScroll_LineLeaveBeforeOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            //short shortValues = 1;
            int setRowidValue = 0, rowId = 0;
            string eventLeaveAfter = "LeaveAfter";
            try
            {
                if ((eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftReferenceNumber.Value != "") &&
                (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.PaymentAmount.Value != 0) &&
                !(DateTime.Equals(eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.DateReceived.Value, "1/1/1900 12:00:00 AM")))
                {
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Key = 1;
                    TableError errorValidTemp = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Change();
                    if (errorValidTemp == TableError.NoError && errorValidTemp != TableError.EndOfTable)
                    {
                        if (
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftReferenceNumber.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftReferenceNumber.Value ||
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemReference.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftItemReference.Value ||
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftBankOriginating.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftBankOriginating.Value ||
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemAmount.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftItemAmount.Value ||
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.DateReceived.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.DateReceived.Value ||
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.PaymentAmount.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.PaymentAmount.Value ||
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.CustomerNumber.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.CustomerNumber.Value ||
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemReference.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftItemReference.Value ||
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemAmount.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftItemAmount.Value ||
                         eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.CurrencyId.Value != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.CurrencyId.Value
                         )
                        {

                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.IsUpdated.Value = 1;
                        }
                        else
                        {

                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.IsUpdated.Value = 0;
                        }

                        rowId = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value;
                    }

                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();

                    if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.IsUpdated.Value == 1)
                    {
                        EFTCustomerPayment eftRemittance = new EFTCustomerPayment();
                        eftRemittance.EftId = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftId.Value;
                        eftRemittance.EftAppId = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftAppId.Value;
                        eftRemittance.PaymentNumber = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.PaymentNumber.Value.ToString().Trim();
                        eftRemittance.ReferenceNumber = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftReferenceNumber.Value.ToString().Trim();
                        eftRemittance.DateReceived = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.DateReceived.Value;
                        eftRemittance.PaymentAmount = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.PaymentAmount.Value;
                        eftRemittance.CustomerID = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.CustomerNumber.Value.ToString().Trim();
                        eftRemittance.CurrencyId = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.CurrencyId.Value.ToString().Trim();
                        eftRemittance.IsFullyApplied = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.IsFullyApplied.Value;
                        eftRemittance.BankOriginating = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftBankOriginating.Value;
                        eftRemittance.ItemReferenceNumber = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftItemReference.Value;
                        eftRemittance.ItemAmount = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftItemAmount.Value;
                        eftRemittance.CreatedBy = Dynamics.Globals.UserId.Value.ToString().Trim();
                        eftRemittance.IsUpdated = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.IsUpdated.Value;
                        eftRemittance.Status = Convert.ToInt16(eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftStatus.Value);
                        eftRemittance.StatusReason = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftStatusDescription.Value;


                        ValidateEFTCustomerRemittance(eventLeaveAfter, rowId, eftRemittance);
                    }
                }
                else
                {
                    if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftReferenceNumber.Value == "")
                    {
                        rowIdForClear = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value;
                        ClearEFTCustomerLine(rowIdForClear);
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftCustomerRemitScroll_LineLeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        private int GetRowIdFromTempTable()
        {
            int rowCount = 0;
            try
            {
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();

                TableError errorValidTemp = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.GetFirst();
                while (errorValidTemp == TableError.NoError && errorValidTemp != TableError.EndOfTable)
                {
                    rowCount = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value;
                    errorValidTemp = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.GetNext();
                }

                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
            }
            catch (Exception ex)
            {

            }
            return rowCount;
        }


        /// <summary>
        /// When the lookup button 1 is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CustomerRemitInquiryLookupButton1_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalSearchBy.Value == 1)
                {
                    if (!customerLookupForm.IsOpen)
                    {
                        customerLookupForm.Open();
                        remittanceCustomerLookup = Resources.STR_CustomerRemittanceInquiry;
                        remittanceCustomerLookupValue = Resources.STR_From;
                    }
                    else
                        MessageBox.Show("Customer Lookup Window already opened. Please close the window" + Resources.STR_MESSAGE_TITLE);


                }
                else if (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalSearchBy.Value == 2)
                {
                    if (eftReferenceNumberLookupForm.IsOpen)
                    {
                        eftReferenceNumberLookupForm.Close();
                    }
                    eftReferenceNumberLookupForm.Open();
                    referenceLookupName = Resources.STR_CustomerRemittanceInquiry;
                    eftReferenceNumberLookupForm.EftReferenceNumberLookup.RedisplayButton.RunValidate();
                    eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value = 1;

                    lookupWindowName = Resources.STR_CustomerRemittanceInquiry;
                    if (!RegisterRefidLookupSelect)
                    {
                        eftReferenceNumberLookupForm.EftReferenceNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftReferenceNumberLookupSelectButton_ClickBeforeOriginal);
                        RegisterRefidLookupSelect = true;
                    }
                }
                else if (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalSearchBy.Value == 4)
                {
                    if (eftDocumentNumberLookupForm.IsOpen)
                    {
                        eftDocumentNumberLookupForm.Close();
                    }
                    eftDocumentNumberLookupForm.Open();
                    documentLookupName = Resources.STR_CustomerRemittanceInquiry;
                    eftDocumentNumberLookupForm.EftDocumentNumberLookup.RedisplayButton.RunValidate();
                    eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value = 1;

                    lookupWindowName = Resources.STR_CustomerRemittanceInquiry;
                    if (!RegisterDocidLookupSelect)
                    {
                        eftDocumentNumberLookupForm.EftDocumentNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftDocumentNumberLookupSelectButton_ClickBeforeOriginal);
                        RegisterDocidLookupSelect = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CustomerRemitInquiryLookupButton1_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the lookup button 2 is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CustomerRemitInquiryLookupButton2_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalSearchBy.Value == 1)
                {
                    if (!customerLookupForm.IsOpen)
                    {
                        customerLookupForm.Open();
                        remittanceCustomerLookup = Resources.STR_CustomerRemittanceInquiry;
                        remittanceCustomerLookupValue = Resources.STR_To;
                    }
                    else
                        MessageBox.Show("Customer Lookup Window already opened. Please close the window" + Resources.STR_MESSAGE_TITLE);

                }
                else if (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalSearchBy.Value == 2)
                {
                    if (eftReferenceNumberLookupForm.IsOpen)
                    {
                        eftReferenceNumberLookupForm.Close();
                    }
                    eftReferenceNumberLookupForm.Open();
                    referenceLookupName = Resources.STR_CustomerRemittanceInquiry;
                    eftReferenceNumberLookupForm.EftReferenceNumberLookup.RedisplayButton.RunValidate();
                    eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value = 2;

                    lookupWindowName = Resources.STR_CustomerRemittanceInquiry;
                    if (!RegisterRefidLookupSelect)
                    {
                        eftReferenceNumberLookupForm.EftReferenceNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftReferenceNumberLookupSelectButton_ClickBeforeOriginal);
                        RegisterRefidLookupSelect = true;
                    }
                }
                else if (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalSearchBy.Value == 4)
                {
                    if (eftDocumentNumberLookupForm.IsOpen)
                    {
                        eftDocumentNumberLookupForm.Close();
                    }
                    eftDocumentNumberLookupForm.Open();
                    documentLookupName = Resources.STR_CustomerRemittanceInquiry;
                    eftDocumentNumberLookupForm.EftDocumentNumberLookup.RedisplayButton.RunValidate();
                    eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value = 2;

                    lookupWindowName = Resources.STR_CustomerRemittanceInquiry;
                    if (!RegisterDocidLookupSelect)
                    {
                        eftDocumentNumberLookupForm.EftDocumentNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftDocumentNumberLookupSelectButton_ClickBeforeOriginal);
                        RegisterDocidLookupSelect = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CustomerRemitInquiryLookupButton2_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// When teh document number lookup is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EftBAtchIdLookupRedisplayButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            ReceivablesRequest receivablesRequest = new ReceivablesRequest();
            ReceivablesResponse receivablesResponse = new ReceivablesResponse();
            AuditInformation auditInformation = new AuditInformation();
            try
            {

                bool recordsExists = false;

                auditInformation.CompanyId = Dynamics.Globals.CompanyId.Value;
                receivablesRequest.AuditInformation = auditInformation;
                //Service Call
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(gpServiceConfigurationURL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.PostAsJsonAsync("api/SalesOrderUpdate/FetchBatchId", receivablesRequest); // we need to refer the web.api service url here.
                    if (response.Result.IsSuccessStatusCode)
                    {
                        receivablesResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                        if (receivablesResponse.Status == ResponseStatus.Success)
                        {
                            recordsExists = true;
                        }
                        else
                        {
                            recordsExists = false;
                            logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftBAtchIdLookupRedisplayButton_ClickAfterOriginal: " + receivablesResponse.ErrorMessage.ToString());
                            MessageBox.Show("Error: " + receivablesResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                        }
                    }
                    else
                    {
                        recordsExists = false;
                        MessageBox.Show("Error: Data does not FetchEFTBatchIdLookup Table", Resources.STR_MESSAGE_TITLE);
                    }
                }

                if (receivablesResponse != null && recordsExists && receivablesResponse.EFTPayment.EFTBatchId.Count != 0)
                {
                    eftBatchIdLookupForm.Tables.EftBatchIdTemp.Close();
                    eftBatchIdLookupForm.Tables.EftBatchIdTemp.Release();

                    foreach (var eftBatchIdLookup in receivablesResponse.EFTPayment.EFTBatchId)
                    {
                        eftBatchIdLookupForm.Tables.EftBatchIdTemp.Release();
                        eftBatchIdLookupForm.Tables.EftBatchIdTemp.BatchNumber.Value = eftBatchIdLookup.ToString().Trim();

                        eftBatchIdLookupForm.Tables.EftBatchIdTemp.Save();
                    }
                    eftBatchIdLookupForm.Tables.EftBatchIdTemp.Release();
                    eftBatchIdLookupForm.Tables.EftBatchIdTemp.Close();
                    eftBatchIdLookupForm.Procedures.EftBatchIdLookupFormScrollFill.Invoke();

                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In eftDocNumbrLookup Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EftBatchIdLookupSelectButton_ClickBeforeOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!String.IsNullOrEmpty(lookupWindowName))
                {
                    if (lookupWindowName == Resources.STR_CustomerRemittanceEntry && eftBatchIdLookupForm.IsOpen)
                    {
                        eftCustomerRemittancesEntryForm.EftCustomerBankEntry.BatchNumber.Focus();
                        eftCustomerRemittancesEntryForm.EftCustomerBankEntry.BatchNumber.Value = eftBatchIdLookupForm.EftBatchIdLookup.EftBatchIdScroll.BatchNumber.Value;
                        eftBatchIdLookupForm.Close();
                    }

                    else if (lookupWindowName == Resources.STR_CustomerRemittanceInquiry && eftBatchIdLookupForm.IsOpen)
                    {
                        eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.EftBatchId.Focus();
                        eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.EftBatchId.Value = eftBatchIdLookupForm.EftBatchIdLookup.EftBatchIdScroll.BatchNumber.Value;
                        eftBatchIdLookupForm.Close();
                    }
                    if (lookupWindowName == Resources.STR_EFTPaymentRemittanceWindow && eftBatchIdLookupForm.IsOpen)
                    {
                        eftRemittancePaymentEntryForm.RemittancePaymentEntry.BatchNumber.Focus();
                        eftRemittancePaymentEntryForm.RemittancePaymentEntry.BatchNumber.Value = eftBatchIdLookupForm.EftBatchIdLookup.EftBatchIdScroll.BatchNumber.Value;
                        eftBatchIdLookupForm.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftBatchIdLookupSelectButton_ClickBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CustomerBankEntryLookupButton9_ClickAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                if (eftBatchIdLookupForm.IsOpen)
                {
                    eftBatchIdLookupForm.Close();
                }
                eftBatchIdLookupForm.Open();
                eftBatchIdLookupForm.EftBatchIdLookup.RedisplayButton.RunValidate();

                lookupWindowName = Resources.STR_CustomerRemittanceEntry;
                if (!RegisterBatchidLookupSelect)
                {
                    eftBatchIdLookupForm.EftBatchIdLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftBatchIdLookupSelectButton_ClickBeforeOriginal);
                    RegisterBatchidLookupSelect = true;
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// EFT Customer Remittance CustomerBankEntryDeleteRowButton_ClickAfterOriginal ...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CustomerBankEntryDeleteRowButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                ReceivablesRequest eftrequest = new ReceivablesRequest();
                ReceivablesResponse eftResponse = new ReceivablesResponse();
                AuditInformation information = new AuditInformation
                {
                    CompanyId = Dynamics.Globals.CompanyId.Value
                };
                EFTPayment payment = new EFTPayment();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value;
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Key = 1;
                TableError error = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Change();
                if ((error == TableError.NoError) && (error != TableError.EndOfTable))
                {
                    if (MessageBox.Show("Are you sure you want to remove the Item reference number permanently?", Resources.STR_MESSAGE_TITLE, MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            payment.EftAppId = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftAppId.Value;
                            eftrequest.EFTPayment = payment;
                            eftrequest.AuditInformation = information;
                            client.BaseAddress = new Uri(gpServiceConfigurationURL);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            var response = client.PostAsJsonAsync("api/SalesOrderUpdate/DeleteBankEntryItemReference/", eftrequest); // we need to refer the web.api service url here.

                            if (response.Result.IsSuccessStatusCode)
                            {
                                eftResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                                if (eftResponse.Status == ResponseStatus.Error)
                                {
                                    MessageBox.Show("Error: Error: Deleting Item Reference", Resources.STR_MESSAGE_TITLE);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Error: Deleting Item Reference", Resources.STR_MESSAGE_TITLE);
                            }


                        }
                        eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Remove();
                    }
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                }
                eftCustomerRemittancesEntryForm.Procedures.EftCustomerRemittancesEntryFormScrollFill.Invoke();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        void EftBatchIdLookupCancelButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                eftBatchIdLookupForm.EftBatchIdLookup.Close();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        void CustomerBankEntryEftBatchId_Change(object sender, EventArgs e)
        {
            try
            {
                if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.BatchNumber.Value != string.Empty)
                {
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.BatchNumber.Disable();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LookupButton9.Disable();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.RedisplayButton.RunValidate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void CustomerBankInquiryEftBatchId_Change(object sender, EventArgs e)
        {
            try
            {
                if (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.EftBatchId.Value != string.Empty)
                {
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.EftBatchId.Disable();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.RedisplayButton.RunValidate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CustomerBankInquiryLookupButton9_ClickAfterOriginal(object sender, EventArgs e)
        {
            if (eftBatchIdLookupForm.IsOpen)
            {
                eftBatchIdLookupForm.Close();
            }
            eftBatchIdLookupForm.Open();
            eftBatchIdLookupForm.EftBatchIdLookup.RedisplayButton.RunValidate();

            lookupWindowName = Resources.STR_CustomerRemittanceInquiry;
            if (!RegisterBatchidLookupSelect)
            {
                eftBatchIdLookupForm.EftBatchIdLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftBatchIdLookupSelectButton_ClickBeforeOriginal);
                RegisterBatchidLookupSelect = true;
            }
        }

        /// <summary>
        /// When the document lookup  select button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void EftDocumentNumberLookupSelectButton_ClickBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!String.IsNullOrEmpty(lookupWindowName))
                {
                    if (lookupWindowName == Resources.STR_CustomerRemittanceEntry && eftDocumentNumberLookupForm.IsOpen)
                    {
                        if (eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value == 1)
                        {
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromItemReference.Focus();
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromItemReference.Value = eftDocumentNumberLookupForm.EftDocumentNumberLookup.EftDocumentNumberScroll.EftItemReference.Value;
                            eftDocumentNumberLookupForm.Close();
                        }
                        else if (eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value == 2)
                        {
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToItemReference.Focus();
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToItemReference.Value = eftDocumentNumberLookupForm.EftDocumentNumberLookup.EftDocumentNumberScroll.EftItemReference.Value;
                            eftDocumentNumberLookupForm.Close();
                        }
                    }
                    else if (lookupWindowName == Resources.STR_CustomerRemittanceInquiry && eftDocumentNumberLookupForm.IsOpen)
                    {
                        if (eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value == 1)
                        {
                            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromItemReference.Focus();
                            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromItemReference.Value = eftDocumentNumberLookupForm.EftDocumentNumberLookup.EftDocumentNumberScroll.EftItemReference.Value;
                            eftDocumentNumberLookupForm.Close();
                        }
                        else if (eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value == 2)
                        {
                            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToItemReference.Focus();
                            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToItemReference.Value = eftDocumentNumberLookupForm.EftDocumentNumberLookup.EftDocumentNumberScroll.EftItemReference.Value;
                            eftDocumentNumberLookupForm.Close();
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftDocumentNumberLookupSelectButton_ClickBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// When the reference lookup  select button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void EftReferenceNumberLookupSelectButton_ClickBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!String.IsNullOrEmpty(lookupWindowName))
                {
                    if (lookupWindowName == Resources.STR_CustomerRemittanceEntry && eftReferenceNumberLookupForm.IsOpen)
                    {
                        if (eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value == 1)
                        {
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromReferenceNumber.Focus();
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromReferenceNumber.Value = eftReferenceNumberLookupForm.EftReferenceNumberLookup.EftReferenceNumberScroll.EftReferenceNumber.Value;
                            eftReferenceNumberLookupForm.Close();
                        }
                        else if (eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value == 2)
                        {
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToReferenceNumber.Focus();
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToReferenceNumber.Value = eftReferenceNumberLookupForm.EftReferenceNumberLookup.EftReferenceNumberScroll.EftReferenceNumber.Value;
                            eftReferenceNumberLookupForm.Close();
                        }
                    }
                    else if (lookupWindowName == Resources.STR_CustomerRemittanceInquiry && eftReferenceNumberLookupForm.IsOpen)
                    {
                        if (eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value == 1)
                        {
                            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromReferenceNumber.Focus();
                            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromReferenceNumber.Value = eftReferenceNumberLookupForm.EftReferenceNumberLookup.EftReferenceNumberScroll.EftReferenceNumber.Value;
                            eftReferenceNumberLookupForm.Close();
                        }
                        else if (eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value == 2)
                        {
                            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToReferenceNumber.Focus();
                            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToReferenceNumber.Value = eftReferenceNumberLookupForm.EftReferenceNumberLookup.EftReferenceNumberScroll.EftReferenceNumber.Value;
                            eftReferenceNumberLookupForm.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftReferenceNumberLookupSelectButton_ClickBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// When the customer lookup  select button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void EftCustomerIdLookupSelectButton_ClickBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!String.IsNullOrEmpty(lookupWindowName))
                {
                    if (lookupWindowName == Resources.STR_CustomerRemittanceEntry && eftCustomerIdLookupForm.IsOpen)
                    {
                        if (eftCustomerIdLookupForm.EftCustomerIdLookup.LocalCalledBy.Value == 1)
                        {
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromCustomerNumber.Focus();
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromCustomerNumber.Value = eftCustomerIdLookupForm.EftCustomerIdLookup.CustomerIdScroll.CustomerNumber.Value;
                            eftCustomerIdLookupForm.Close();
                        }
                        else if (eftCustomerIdLookupForm.EftCustomerIdLookup.LocalCalledBy.Value == 2)
                        {
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToCustomerNumber.Focus();
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToCustomerNumber.Value = eftCustomerIdLookupForm.EftCustomerIdLookup.CustomerIdScroll.CustomerNumber.Value;
                            eftCustomerIdLookupForm.Close();
                        }
                    }
                    else if (lookupWindowName == Resources.STR_CustomerRemittanceInquiry && eftCustomerIdLookupForm.IsOpen)
                    {
                        if (eftCustomerIdLookupForm.EftCustomerIdLookup.LocalCalledBy.Value == 1)
                        {
                            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromCustomerNumber.Focus();
                            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromCustomerNumber.Value = eftCustomerIdLookupForm.EftCustomerIdLookup.CustomerIdScroll.CustomerNumber.Value;
                            eftCustomerIdLookupForm.Close();
                        }
                        else if (eftCustomerIdLookupForm.EftCustomerIdLookup.LocalCalledBy.Value == 2)
                        {
                            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToCustomerNumber.Focus();
                            eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToCustomerNumber.Value = eftCustomerIdLookupForm.EftCustomerIdLookup.CustomerIdScroll.CustomerNumber.Value;
                            eftCustomerIdLookupForm.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftReferenceNumberLookupSelectButton_ClickBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        ///to clear, show the fileds and lookup
        /// </summary>
        /// <param name="formName"></param>

        private void InitilizeSortBySelection(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_CustomerRemittanceEntry)
                {

                    HideAllSortByRemitEntryFields(Resources.STR_CustomerRemittanceEntry);
                    ClearRemitCustomerIdFields(Resources.STR_CustomerRemittanceEntry);
                    ClearRemitDateReceivedFields(Resources.STR_CustomerRemittanceEntry);
                    ClearRemitReferenceNumberFields(Resources.STR_CustomerRemittanceEntry);
                    ClearRemitDocumentNumberFields(Resources.STR_CustomerRemittanceEntry);
                    ClearEFTCustomerRemittance(Resources.STR_CustomerRemittanceEntry);

                    if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalSearchBy.Value == 1)
                    {
                        ShowRemitCustomerIdFields(Resources.STR_CustomerRemittanceEntry);
                        ShowHeaderLookupButtons(Resources.STR_CustomerRemittanceEntry);
                    }
                    else if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalSearchBy.Value == 3)
                    {
                        ShowRemitDateReceivedFields(Resources.STR_CustomerRemittanceEntry);
                    }
                    else if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalSearchBy.Value == 2)
                    {
                        ShowRemitReferenceNumberFields(Resources.STR_CustomerRemittanceEntry);
                        ShowHeaderLookupButtons(Resources.STR_CustomerRemittanceEntry);
                    }
                    else if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalSearchBy.Value == 4)
                    {
                        ShowRemitDocumentNumberFields(Resources.STR_CustomerRemittanceEntry);
                        ShowHeaderLookupButtons(Resources.STR_CustomerRemittanceEntry);
                    }

                    ClearEFTCustomerRemittance(Resources.STR_CustomerRemittanceEntry);
                    eftCustomerRemittancesEntryForm.Procedures.EftCustomerRemittancesEntryFormScrollFill.Invoke();
                }
                else if (formName == Resources.STR_CustomerRemittanceInquiry)
                {

                    HideAllSortByRemitEntryFields(Resources.STR_CustomerRemittanceInquiry);
                    ClearRemitCustomerIdFields(Resources.STR_CustomerRemittanceInquiry);
                    ClearRemitDateReceivedFields(Resources.STR_CustomerRemittanceInquiry);
                    ClearRemitReferenceNumberFields(Resources.STR_CustomerRemittanceInquiry);
                    ClearRemitDocumentNumberFields(Resources.STR_CustomerRemittanceInquiry);
                    ClearEFTCustomerRemittance(Resources.STR_CustomerRemittanceInquiry);

                    if (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalSearchBy.Value == 1)
                    {
                        ShowRemitCustomerIdFields(Resources.STR_CustomerRemittanceInquiry);
                        ShowHeaderLookupButtons(Resources.STR_CustomerRemittanceInquiry);
                    }
                    else if (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalSearchBy.Value == 3)
                    {
                        ShowRemitDateReceivedFields(Resources.STR_CustomerRemittanceInquiry);
                    }
                    else if (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalSearchBy.Value == 2)
                    {
                        ShowRemitReferenceNumberFields(Resources.STR_CustomerRemittanceInquiry);
                        ShowHeaderLookupButtons(Resources.STR_CustomerRemittanceInquiry);
                    }
                    else if (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalSearchBy.Value == 4)
                    {
                        ShowRemitDocumentNumberFields(Resources.STR_CustomerRemittanceInquiry);
                        ShowHeaderLookupButtons(Resources.STR_CustomerRemittanceInquiry);
                    }

                    ClearEFTCustomerRemittance(Resources.STR_CustomerRemittanceInquiry);
                    eftCustomerRemittancesInquiryForm.Procedures.EftCustomerBankInquiryFormScrollFill.Invoke();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InitilizeSortBySelection Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// ValidateEFTItemReference from leave Reference Number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTRemittanceToReference_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!string.IsNullOrEmpty(eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToReferenceNumber.Value.ToString().Trim())
                    && (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromReferenceNumber.Value.ToString().Trim() != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToReferenceNumber.Value.ToString().Trim()))
                {
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.AllOrRange.Value = 1;
                    if (string.IsNullOrEmpty(eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromReferenceNumber.Value.ToString()))
                        eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromReferenceNumber.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToReferenceNumber.Value;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceToReference_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        private void EFTRemittanceToReferenceInquiry_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!string.IsNullOrEmpty(eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToReferenceNumber.Value.ToString().Trim())
                    && (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromReferenceNumber.Value.ToString().Trim() != eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToReferenceNumber.Value.ToString().Trim()))
                {
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.AllOrRange.Value = 1;
                    if (string.IsNullOrEmpty(eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromReferenceNumber.Value.ToString()))
                        eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromReferenceNumber.Value = eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToReferenceNumber.Value;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceToReferenceInquiry_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        private void EFTRemittanceToItemReferenceInquiry_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!string.IsNullOrEmpty(eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToItemReference.Value.ToString().Trim())
                    && (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromItemReference.Value.ToString().Trim() != eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToItemReference.Value.ToString().Trim()))
                {

                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.AllOrRange.Value = 1;
                    if (string.IsNullOrEmpty(eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromItemReference.Value.ToString()))
                        eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromItemReference.Value = eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToItemReference.Value;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceToItemReferenceInquiry_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }

        /// <summary>
        /// ValidateEftCustomer From ToCustomer Number leave
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTRemittanceToCustomerNumber_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!string.IsNullOrEmpty(eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToCustomerNumber.Value.ToString().Trim())
                    && (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromCustomerNumber.Value.ToString().Trim() != eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToCustomerNumber.Value.ToString().Trim()))
                {
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.AllOrRange.Value = 1;
                    if (string.IsNullOrEmpty(eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromCustomerNumber.Value.ToString()))
                        eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromCustomerNumber.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToCustomerNumber.Value;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceToCustomerNumber_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        private void EFTRemittanceToCustomerNumberInquiry_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!string.IsNullOrEmpty(eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToCustomerNumber.Value.ToString().Trim())
                    && (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromCustomerNumber.Value.ToString().Trim() != eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToCustomerNumber.Value.ToString().Trim()))
                {
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.AllOrRange.Value = 1;
                    if (string.IsNullOrEmpty(eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromCustomerNumber.Value.ToString()))
                        eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromCustomerNumber.Value = eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToCustomerNumber.Value;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceToCustomerNumberInquiry_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        private void EFTRemittanceDateFromInquiry_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.AllOrRange.Value = 1;
                eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToDateReceived.Value = eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromDateReceived.Value;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceDateFromInquiry_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        void EFTCustomerInquiryAllOrRange_Change(object sender, EventArgs e)
        {
            if (eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.AllOrRange.Value == 0)
            {
                ClearRemitCustomerIdFields(Resources.STR_CustomerRemittanceInquiry);
                ClearRemitDateReceivedFields(Resources.STR_CustomerRemittanceInquiry);
                ClearRemitReferenceNumberFields(Resources.STR_CustomerRemittanceInquiry);
                ClearRemitDocumentNumberFields(Resources.STR_CustomerRemittanceInquiry);
                ClearEFTCustomerRemittance(Resources.STR_CustomerRemittanceInquiry);
                eftCustomerRemittancesInquiryForm.Procedures.EftCustomerBankInquiryFormScrollFill.Invoke();
            }
        }

        /// <summary>
        /// to show the lookup id fields
        /// </summary>
        /// <param name="formName"></param>
        private void ShowHeaderLookupButtons(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_CustomerRemittanceEntry)
                {
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LookupButton1.Show();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LookupButton2.Show();
                }
                else if (formName == Resources.STR_CustomerRemittanceInquiry)
                {
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LookupButton1.Show();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LookupButton2.Show();

                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ShowHeaderLookupButtons Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// to show the customer id fields
        /// </summary>
        /// <param name="formName"></param>
        private void ShowRemitCustomerIdFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_CustomerRemittanceEntry)
                {
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToCustomerNumber.Show();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromCustomerNumber.Show();
                }
                else if (formName == Resources.STR_CustomerRemittanceInquiry)
                {
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToCustomerNumber.Show();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromCustomerNumber.Show();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ShowHeaderLookupButtons Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// to show the date fields
        /// </summary>
        /// <param name="formName"></param>
        private void ShowRemitDateReceivedFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_CustomerRemittanceEntry)
                {
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromDateReceived.Show();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToDateReceived.Show();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LookupButton1.Hide();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LookupButton2.Hide();
                }
                else if (formName == Resources.STR_CustomerRemittanceInquiry)
                {
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromDateReceived.Show();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToDateReceived.Show();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LookupButton1.Hide();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LookupButton2.Hide();

                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ShowRemitDateReceivedFields Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// to show the ref no.fields
        /// </summary>
        /// <param name="formName"></param>
        private void ShowRemitReferenceNumberFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_CustomerRemittanceEntry)
                {
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToReferenceNumber.Show();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromReferenceNumber.Show();
                }
                else if (formName == Resources.STR_CustomerRemittanceInquiry)
                {
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToReferenceNumber.Show();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromReferenceNumber.Show();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ShowRemitDateReceivedFields Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// to show the doc numb id fields
        /// </summary>
        /// <param name="formName"></param>
        private void ShowRemitDocumentNumberFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_CustomerRemittanceEntry)
                {
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromItemReference.Show();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToItemReference.Show();
                }
                else if (formName == Resources.STR_CustomerRemittanceInquiry)
                {
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromItemReference.Show();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToItemReference.Show();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ShowRemitDocumentNumberFields Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }
        /// <summary>
        /// to clear the customer id fields
        /// </summary>
        /// <param name="formName"></param>
        private void ClearRemitCustomerIdFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_CustomerRemittanceEntry)
                {
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromCustomerNumber.Clear();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToCustomerNumber.Clear();
                }
                else if (formName == Resources.STR_CustomerRemittanceInquiry)
                {
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromCustomerNumber.Clear();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToCustomerNumber.Clear();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearRemitCustomerIdFields Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// to clear the date fields
        /// </summary>
        /// <param name="formName"></param>
        private void ClearRemitDateReceivedFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_CustomerRemittanceEntry)
                {
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromDateReceived.Clear();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToDateReceived.Clear();
                }
                else if (formName == Resources.STR_CustomerRemittanceInquiry)
                {
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromDateReceived.Clear();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToDateReceived.Clear();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearRemitDateReceivedFields Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// to clear the ref no. fields
        /// </summary>
        /// <param name="formName"></param>
        private void ClearRemitReferenceNumberFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_CustomerRemittanceEntry)
                {
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromReferenceNumber.Clear();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToReferenceNumber.Clear();
                }
                else if (formName == Resources.STR_CustomerRemittanceInquiry)
                {
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromReferenceNumber.Clear();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToReferenceNumber.Clear();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearRemitReferenceNumberFields Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }
        /// <summary>
        /// to clear the ref no. fields
        /// </summary>
        /// <param name="formName"></param>
        private void ClearRemitDocumentNumberFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_CustomerRemittanceEntry)
                {
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromItemReference.Clear();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToItemReference.Clear();
                }
                else if (formName == Resources.STR_CustomerRemittanceInquiry)
                {
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromItemReference.Clear();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToItemReference.Clear();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearRemitDocumentNumberFields Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        /// <summary>
        /// to hide all the fields when all is selected
        /// </summary>
        /// <param name="formName"></param>
        private void HideAllSortByRemitEntryFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_CustomerRemittanceEntry)
                {
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromCustomerNumber.Hide();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToCustomerNumber.Hide();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromDateReceived.Hide();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToDateReceived.Hide();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromItemReference.Hide();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToItemReference.Hide();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalFromReferenceNumber.Hide();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.LocalToReferenceNumber.Hide();
                }
                else if (formName == Resources.STR_CustomerRemittanceInquiry)
                {
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromCustomerNumber.Hide();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToCustomerNumber.Hide();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromDateReceived.Hide();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToDateReceived.Hide();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromItemReference.Hide();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToItemReference.Hide();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalFromReferenceNumber.Hide();
                    eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.LocalToReferenceNumber.Hide();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In HideAllSortByRemitEntryFields Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// EFT Item Amount Leave after 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTItemAmount_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftReferenceNumber.Value != "")
                    CheckFullyApplyAmount();
                else
                {
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftItemAmount.Clear();
                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftReferenceNumber.Focus();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTItemAmount_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        private void CheckFullyApplyAmount()
        {
            decimal totalPaymentAmount = 0, itemAmount = 0;
            string refNumber = string.Empty;
            int rowId = 0;
            try
            {
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                rowId = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value;
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value;
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Key = 1;
                TableError errorValidTemp = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.GetFirst();
                while (errorValidTemp == TableError.NoError && errorValidTemp != TableError.EndOfTable)
                {
                    if (eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftReferenceNumber.Value.ToString().Trim() == eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftReferenceNumber.Value)
                    {
                        totalPaymentAmount = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.PaymentAmount.Value;
                        refNumber = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftReferenceNumber.Value.ToString().Trim();
                        if (rowId == eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value)
                            itemAmount += eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftItemAmount.Value;
                        else
                            itemAmount += eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemAmount.Value;
                    }
                    errorValidTemp = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.GetNext();
                }
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();


                if (totalPaymentAmount == itemAmount)
                {
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Key = 1;
                    TableError errorTemp = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.GetFirst();
                    while (errorTemp == TableError.NoError && errorTemp != TableError.EndOfTable)
                    {
                        if (refNumber == eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftReferenceNumber.Value)
                        {
                            TableError errorChangeTemp = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Change();
                            if (errorChangeTemp == TableError.NoError)
                            {
                                if (rowId == eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value)
                                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.IsfullyAppliedStr.Value = "Yes";
                                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsfullyAppliedStr.Value = "Yes";
                                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsFullyApplied.Value = true;
                                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Save();
                            }
                        }
                        errorTemp = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.GetNext();
                    }
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                    
                }
                else if (totalPaymentAmount > itemAmount)
                {
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Key = 1;
                    TableError errorTempValue = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.GetFirst();
                    while (errorTempValue == TableError.NoError && errorTempValue != TableError.EndOfTable)
                    {
                        if (refNumber == eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftReferenceNumber.Value)
                        {
                            TableError errorChange = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Change();
                            if (errorChange == TableError.NoError)
                            {
                                if (rowId == eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value)
                                    eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.IsfullyAppliedStr.Value = "No";
                                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsfullyAppliedStr.Value = "No";
                                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.IsFullyApplied.Value = false;
                                eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Save();
                            }
                        }
                        errorTempValue = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.GetNext();
                    }
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                    
                }
                else if (totalPaymentAmount < itemAmount)
                {
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftRowId.Value = eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftRowId.Value;
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Key = 1;
                    TableError errorTempValue = eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Get();
                    if (errorTempValue == TableError.NoError && errorTempValue != TableError.EndOfTable)
                    {
                        if (eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.EftItemAmount.Value != eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.EftItemAmount.Value)
                        {
                            MessageBox.Show("Item Amount exceeding the Total Payment");
                            eftCustomerRemittancesEntryForm.EftCustomerBankEntry.EftCustomerRemitScroll.CustomerNumber.Focus();
                        }
                    }
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Release();
                    eftCustomerRemittancesEntryForm.Tables.EftCustomerRemitTemp.Close();
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }



        /// <summary>
        /// Customer Remittance inquiry window ok button 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTCustomerRemittanceOK_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                eftCustomerRemittancesInquiryForm.EftCustomerBankInquiry.Close();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In RmAppliedDocumentScrollLocalApplySelectChange Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        #endregion EFT

        #region EftCsvImport

        void BankCtxSummaryImport_OpenBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (RegisterBankCtxSummaryImport == false)
            {
                bankCtxSummaryImportForm.BankCtxSummaryImport.ImportButton.Disable();
                RegisterBankCtxSummaryImport = true;
            }
        }

        void BankCtxSummaryImport_CloseAfterOriginal(object sender, EventArgs e)
        {
            RegisterBankCtxSummaryImport = false;
        }


        /// <summary>
        /// EFT Batch ID Value verified
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EftBatchId_Change(object sender, EventArgs e)
        {
            try
            {
                if (bankCtxSummaryImportForm.BankCtxSummaryImport.FilePath.Value != string.Empty && bankCtxSummaryImportForm.BankCtxSummaryImport.EftBatchId.Value != string.Empty)
                    bankCtxSummaryImportForm.BankCtxSummaryImport.ImportButton.Enable();
                else
                    bankCtxSummaryImportForm.BankCtxSummaryImport.ImportButton.Disable();

            }
            catch
            {

            }
        }


        #endregion EftCsvImport

        #region EmailRemittance




        /// <summary>
        /// EFT Customer Remittance Redisplay Button Click ...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTEmailRemittanceRedisplayButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                InsertEFTEmailRemittance(Resources.STR_EmailRemittanceEntry);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        /// <summary>
        /// Insert EFT Customer Remittance From service call...
        /// </summary>
        /// <param name="p"></param>
        private void InsertEFTEmailRemittance(string formName)
        {
            try
            {
                if (formName == Resources.STR_EmailRemittanceInquiry)
                {

                    if (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.AllOrRange.Value == 0 ||
                        ///Option Button select From...
                     (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.AllOrRange.Value == 1 &&
                        //Date From and Date To Field should not empty
                     ((eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromDateReceived.Value.ToString().Trim() != "") &&
                     (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToDateReceived.Value.ToString().Trim() != "")) ||
                        //Customer Number Start and End should not empty...
                     ((eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromCustomerNumber.Value.ToString().Trim() != "") &&
                     (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToCustomerNumber.Value.ToString().Trim() != "")) ||
                        //Customer Number Start and End should not empty...
                     ((eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromReferenceNumber.Value.ToString().Trim() != "") &&
                     (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToReferenceNumber.Value.ToString().Trim() != "")) ||
                        //Document Number start and End Should not empty...
                     ((eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromDateReceived.Value.ToString().Trim() != "") &&
                     (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToDateReceived.Value.ToString().Trim() != ""))))
                    {
                        ReceivablesRequest eftEmailRemittanceRequest = new ReceivablesRequest();
                        ReceivablesResponse eftResponse = new ReceivablesResponse();
                        AuditInformation eftAudit = new AuditInformation();
                        eftAudit.CompanyId = Dynamics.Globals.CompanyId.Value;
                        eftEmailRemittanceRequest.CustomerIdStart = eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromCustomerNumber.Value.ToString().Trim() != "" ? eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromCustomerNumber.Value.ToString().Trim() : string.Empty;
                        eftEmailRemittanceRequest.CustomerIdEnd = eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToCustomerNumber.Value.ToString().Trim() != "" ? eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToCustomerNumber.Value.ToString().Trim() : string.Empty;
                        eftEmailRemittanceRequest.DateStart = eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromDateReceived.Value != null ? eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromDateReceived.Value : Convert.ToDateTime("1900/01/01");
                        eftEmailRemittanceRequest.DateEnd = eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToDateReceived.Value != null ? eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToDateReceived.Value : Convert.ToDateTime("1900/01/01");
                        eftEmailRemittanceRequest.ReferenceNoStart = eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromReferenceNumber.Value.ToString().Trim() != "" ? eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromReferenceNumber.Value.ToString().Trim() : string.Empty;
                        eftEmailRemittanceRequest.ReferenceNoEnd = eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToReferenceNumber.Value.ToString().Trim() != "" ? eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToReferenceNumber.Value.ToString().Trim() : string.Empty;
                        eftEmailRemittanceRequest.Searchtype = eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalSearchBy.Value != null ? Convert.ToInt16(eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalSearchBy.Value) : Convert.ToInt16(0);
                        eftEmailRemittanceRequest.DocNumbrStart = eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromItemReference.Value.ToString().Trim() != "" ? eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromItemReference.Value.ToString().Trim() : string.Empty;
                        eftEmailRemittanceRequest.DocNumbrend = eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToItemReference.Value.ToString().Trim() != "" ? eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToReferenceNumber.Value.ToString().Trim() : string.Empty;
                        eftEmailRemittanceRequest.Source = "Inquiry";
                        eftEmailRemittanceRequest.AuditInformation = eftAudit;

                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationURL);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/SalesOrderUpdate/GetEFTEmailRemittances/", eftEmailRemittanceRequest); // we need to refer the web.api service url here.

                            if (response.Result.IsSuccessStatusCode)
                            {
                                eftResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                                if (eftResponse.Status == ResponseStatus.Error)
                                {
                                    MessageBox.Show("Error: " + eftResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Error: Data does not saved into InsertEFTEmailRemittance Table", Resources.STR_MESSAGE_TITLE);
                            }
                        }

                        if (eftResponse.EFTCustomerRemittancesList.Count == 0)
                        {
                            MessageBox.Show("No records available for process", Resources.STR_MESSAGE_TITLE);
                            ClearEFTRemittanceTemp(Resources.STR_EmailRemittanceInquiry);
                            eftEmailRemittanceInquiryForm.Procedures.EftEmailRemittancesInquiryFormScrollFill.Invoke();
                        }
                        else if (eftResponse != null && eftResponse.EFTCustomerRemittancesList.Count > 0)
                        {
                            InsertEmailRemittance(eftResponse, Resources.STR_EmailRemittanceInquiry);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        /// <summary>
        /// Insert EFT Customer Remittance details to Temp Table...
        /// </summary>
        /// <param name="eftResponse"></param>
        private void InsertEmailRemittance(ReceivablesResponse eftResponse, string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            int rowId = 1;
            try
            {
                if (formName == Resources.STR_EmailRemittanceInquiry)
                {
                    ClearEFTRemittanceTemp(formName);
                    eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.Close();
                    eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.Release();
                    foreach (var eftEmailRemittance in eftResponse.EFTCustomerRemittancesList)
                    {
                        eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.EftRowId.Value = rowId++;
                        eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.EftReferenceNumber.Value = eftEmailRemittance.ReferenceNumber.ToString().Trim();
                        eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.DateReceived.Value = eftEmailRemittance.DateReceived;
                        eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.PaymentAmount.Value = eftEmailRemittance.PaymentAmount;
                        eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.TotalPaymentAmount.Value = eftEmailRemittance.TotalPaymentAmount;
                        eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.CustomerNumber.Value = eftEmailRemittance.CustomerID.ToString().Trim();
                        eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.EftAppId.Value = eftEmailRemittance.EftAppId;
                        eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.EftId.Value = eftEmailRemittance.EftId;
                        eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.EftItemReference.Value = eftEmailRemittance.ItemReference;
                        eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.EftItemAmount.Value = eftEmailRemittance.ItemAmount;
                        eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.CurrencyId.Value = eftEmailRemittance.CurrencyID.Trim();
                        eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.Save();
                    }
                    eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.Release();
                    eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.Close();
                    eftEmailRemittanceInquiryForm.Procedures.EftEmailRemittancesInquiryFormScrollFill.Invoke();
                }

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertEmailRemittance Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        /// <summary>
        /// When the EmailRemittanceInquiry window is opened
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EmailRemittanceInquiry_OpenBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalSearchBy.Value = 1;
                eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.AllOrRange.Value = 0;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EmailRemittanceEntry_OpenBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        /// <summary>
        /// EFT Customer Remittance Redisplay Button Click ...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTEmailRemittanceInquiryRedisplayButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                InsertEFTEmailRemittance(Resources.STR_EmailRemittanceInquiry);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        /// <summary>
        /// Clear Customer Remittance window 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTEmailRemittanceInquiryClear_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ClearEFTCustomerIdFields(Resources.STR_EmailRemittanceInquiry);
                ClearEFTDateReceivedFields(Resources.STR_EmailRemittanceInquiry);
                ClearEFTReferenceNumberFields(Resources.STR_EmailRemittanceInquiry);
                ClearEFTDocumentNumberFields(Resources.STR_EmailRemittanceInquiry);
                ClearEFTRemittanceTemp(Resources.STR_EmailRemittanceInquiry);
                eftEmailRemittanceInquiryForm.Procedures.EftEmailRemittancesInquiryFormScrollFill.Invoke();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTEmailRemittanceClear_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        /// <summary>
        /// When the selection in the drop down is made
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EmailRemittanceInquiryLocalSearchBy_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                InitilizeSortBySelectionForEFTRemittance(Resources.STR_EmailRemittanceInquiry);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EmailRemittanceInquiryLocalSearchBy_Change Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// When the lookup button 1 is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EmailRemitEntryInquiryLookupButton1_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                SelectLookupButton1(Resources.STR_EmailRemittanceInquiry);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EmailRemitEntryInquiryLookupButton1_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        /// <summary>
        /// When the lookup button 2 is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EmailRemitEntryInquiryLookupButton2_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                SelectLookupButton2(Resources.STR_EmailRemittanceInquiry);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EmailRemitEntryLookupButton2_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }




        /// <summary>
        /// From customer Number Leave After Original validate the customer number..
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTEmailRemittanceInquiryFromCustomerNumber_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ValidateEFTCustomer(Resources.STR_EmailRemittanceInquiry);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTEmailRemittanceInquiryFromCustomerNumber_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }

        /// <summary>
        /// validate Item Reference Number...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTEmailRemittanceInquiryFromItemReference_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ValidateEFTItemReference(Resources.STR_EmailRemittanceInquiry);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceFromItemReference_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        // <summary>
        /// validate Referencer number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTEmailRemittanceInquiryFromReference_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ValidateEftReference(Resources.STR_EmailRemittanceInquiry);

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTEmailRemittanceInquiryFromReference_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// ValidateEftCustomer From ToCustomer Number leave
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTEmailRemittanceInquiryToCustomerNumber_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ValidateEftToCustomer(Resources.STR_EmailRemittanceInquiry);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTEmailRemittanceInquiryToCustomerNumber_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// ValidateEFTItemReference from leave ItemReference Number...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTEmailRemittanceInquiryToItemReference_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ValidateEFTToItemReference(Resources.STR_EmailRemittanceInquiry);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTEmailRemittanceInquiryToItemReference_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        /// <summary>
        /// ValidateEFTItemReference from leave Reference Number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTEmailRemittanceInquiryToReference_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ValidateEftToReference(Resources.STR_EmailRemittanceInquiry);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTEmailRemittanceInquiryToReference_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// EFT Remittance Window Date From Field Leave 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTEmailRemittanceInquiryDateFrom_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ValidateRemittanceDate(Resources.STR_EmailRemittanceInquiry);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTEmailRemittanceInquiryDateFrom_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// eftCustomerRemittancesEntry Window select all value...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EFTEmailRemitInquiryAllOrRange_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RemitttanceAllorRange(Resources.STR_EmailRemittanceInquiry);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTEmailRemitinquiryAllOrRange_Change Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }



        #endregion EmailRemittance

        #region PaymentRemittance

        /// <summary>
        /// When the entry window is opened
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PaymentRemittanceEntry_OpenBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                eftRemittancePaymentEntryForm.RemittancePaymentEntry.PushToGp.Disable();
                eftRemittancePaymentEntryForm.RemittancePaymentEntry.Validate.Disable();
                eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalSearchBy.Value = 1;
                eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftApplyType.Value = 2;    // Set default Action Type 
                eftRemittancePaymentEntryForm.RemittancePaymentEntry.IsSelected.Disable();
                eftRemittancePaymentEntryForm.RemittancePaymentEntry.IsSelected.Value = false;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CustomerRemittanceEntry_OpenBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// When the selection in the drop down is made
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PaymentRemittanceLocalSearchBy_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                InitilizeSortBySelectionForEFTRemittance(Resources.STR_EFTPaymentRemittanceWindow);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In LocalSearchBy_Change Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        /// <summary>
        /// When the Action selection in the drop down is made
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PaymentRemittanceEftApplyType_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ClearEFTCustomerIdFields(Resources.STR_EFTPaymentRemittanceWindow);
                ClearEFTDateReceivedFields(Resources.STR_EFTPaymentRemittanceWindow);
                ClearEFTReferenceNumberFields(Resources.STR_EFTPaymentRemittanceWindow);
                ClearEFTDocumentNumberFields(Resources.STR_EFTPaymentRemittanceWindow);
                ClearEFTRemittanceTemp(Resources.STR_EFTPaymentRemittanceWindow);

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PaymentRemittanceEftApplyType_Change Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        ///to clear, show the fileds and lookup
        /// </summary>
        /// <param name="formName"></param>

        private void InitilizeSortBySelectionForEFTRemittance(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.AllOrRange.Value = 0;
                    HideAllSortByPaymentRemitEntryFields(formName);
                    ClearEFTCustomerIdFields(formName);
                    ClearEFTDateReceivedFields(formName);
                    ClearEFTReferenceNumberFields(formName);
                    ClearEFTDocumentNumberFields(formName);
                    ClearEFTRemittanceTemp(formName);

                    if (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalSearchBy.Value == 1)
                    {
                        ShowEFTCustomerIdFields(formName);
                        ShowEFTHeaderLookupButtons(formName);
                    }
                    else if (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalSearchBy.Value == 3)
                    {
                        ShowEFTDateReceivedFields(formName);
                    }
                    else if (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalSearchBy.Value == 2)
                    {
                        ShowEFTReferenceNumberFields(formName);
                        ShowEFTHeaderLookupButtons(formName);
                    }
                    else if (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalSearchBy.Value == 4)
                    {
                        ShowEFTDocumentNumberFields(formName);
                        ShowEFTHeaderLookupButtons(formName);
                    }
                }
                //else if (formName == Resources.STR_EmailRemittanceEntry)
                //{
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.AllOrRange.Value = 0;
                //    HideAllSortByPaymentRemitEntryFields(formName);
                //    ClearEFTCustomerIdFields(formName);
                //    ClearEFTDateReceivedFields(formName);
                //    ClearEFTReferenceNumberFields(formName);
                //    ClearEFTDocumentNumberFields(formName);
                //    ClearEFTRemittanceTemp(formName);


                //    if (eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalSearchBy.Value == 1)
                //    {
                //        ShowEFTCustomerIdFields(formName);
                //        ShowEFTHeaderLookupButtons(formName);
                //    }
                //    else if (eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalSearchBy.Value == 3)
                //    {
                //        ShowEFTDateReceivedFields(formName);
                //    }
                //    else if (eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalSearchBy.Value == 2)
                //    {
                //        ShowEFTReferenceNumberFields(formName);
                //        ShowEFTHeaderLookupButtons(formName);
                //    }
                //    else if (eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalSearchBy.Value == 4)
                //    {
                //        ShowEFTDocumentNumberFields(formName);
                //        ShowEFTHeaderLookupButtons(formName);
                //    }
                //}
                else if (formName == Resources.STR_EmailRemittanceInquiry)
                {
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.AllOrRange.Value = 0;
                    HideAllSortByPaymentRemitEntryFields(formName);
                    ClearEFTCustomerIdFields(formName);
                    ClearEFTDateReceivedFields(formName);
                    ClearEFTReferenceNumberFields(formName);
                    ClearEFTDocumentNumberFields(formName);
                    ClearEFTRemittanceTemp(formName);

                    if (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalSearchBy.Value == 1)
                    {
                        ShowEFTCustomerIdFields(formName);
                        ShowEFTHeaderLookupButtons(formName);
                    }
                    else if (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalSearchBy.Value == 3)
                    {
                        ShowEFTDateReceivedFields(formName);
                    }
                    else if (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalSearchBy.Value == 2)
                    {
                        ShowEFTReferenceNumberFields(formName);
                        ShowEFTHeaderLookupButtons(formName);
                    }
                    else if (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalSearchBy.Value == 4)
                    {
                        ShowEFTDocumentNumberFields(formName);
                        ShowEFTHeaderLookupButtons(formName);
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InitilizeSortBySelection Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// to show the customer id fields
        /// </summary>
        /// <param name="formName"></param>
        private void ShowEFTCustomerIdFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToCustomerNumber.Show();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromCustomerNumber.Show();
                }
                //else if (formName == Resources.STR_EmailRemittanceEntry)
                //{
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToCustomerNumber.Show();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromCustomerNumber.Show();
                //}
                else if (formName == Resources.STR_EmailRemittanceInquiry)
                {
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToCustomerNumber.Show();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromCustomerNumber.Show();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ShowHeaderLookupButtons Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// to show the lookup id fields
        /// </summary>
        /// <param name="formName"></param>
        private void ShowEFTHeaderLookupButtons(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LookupButton1.Show();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LookupButton2.Show();
                }
                //else if (formName == Resources.STR_EmailRemittanceEntry)
                //{
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LookupButton1.Show();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LookupButton2.Show();
                //}
                else if (formName == Resources.STR_EmailRemittanceInquiry)
                {
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LookupButton1.Show();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LookupButton2.Show();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ShowHeaderLookupButtons Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// to show the date fields
        /// </summary>
        /// <param name="formName"></param>
        private void ShowEFTDateReceivedFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromDateReceived.Show();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToDateReceived.Show();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LookupButton1.Hide();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LookupButton2.Hide();
                }
                if (formName == Resources.STR_EmailRemittanceEntry)
                //{
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromDateReceived.Show();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToDateReceived.Show();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LookupButton1.Hide();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LookupButton2.Hide();
                //}
                if (formName == Resources.STR_EmailRemittanceInquiry)
                {
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromDateReceived.Show();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToDateReceived.Show();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LookupButton1.Hide();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LookupButton2.Hide();
                }

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ShowRemitDateReceivedFields Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// to show the ref no.fields
        /// </summary>
        /// <param name="formName"></param>
        private void ShowEFTReferenceNumberFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToReferenceNumber.Show();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromReferenceNumber.Show();
                }
                //else if (formName == Resources.STR_EmailRemittanceEntry)
                //{
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToReferenceNumber.Show();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromReferenceNumber.Show();
                //}
                else if (formName == Resources.STR_EmailRemittanceInquiry)
                {
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToReferenceNumber.Show();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromReferenceNumber.Show();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ShowPaymentRemitReferenceNumberFields Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// to show the doc numb id fields
        /// </summary>
        /// <param name="formName"></param>
        private void ShowEFTDocumentNumberFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromItemRefernce.Show();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToItemRefernce.Show();
                }
                //else if (formName == Resources.STR_EmailRemittanceEntry)
                //{
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromItemReference.Show();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToItemReference.Show();
                //}
                else if (formName == Resources.STR_EmailRemittanceInquiry)
                {
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToItemReference.Show();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromItemReference.Show();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ShowRemitDocumentNumberFields Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }

        /// <summary>
        /// to hide all the fields when all is selected
        /// </summary>
        /// <param name="formName"></param>
        private void HideAllSortByPaymentRemitEntryFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromCustomerNumber.Hide();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToCustomerNumber.Hide();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromDateReceived.Hide();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToDateReceived.Hide();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromItemRefernce.Hide();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToItemRefernce.Hide();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromReferenceNumber.Hide();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToReferenceNumber.Hide();
                }
                //else if (formName == Resources.STR_EmailRemittanceEntry)
                //{
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromCustomerNumber.Hide();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToCustomerNumber.Hide();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromDateReceived.Hide();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToDateReceived.Hide();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromItemReference.Hide();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToItemReference.Hide();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromReferenceNumber.Hide();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToReferenceNumber.Hide();
                //}
                else if (formName == Resources.STR_EmailRemittanceInquiry)
                {
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromCustomerNumber.Hide();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToCustomerNumber.Hide();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromDateReceived.Hide();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToDateReceived.Hide();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromItemReference.Hide();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToItemReference.Hide();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromReferenceNumber.Hide();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToReferenceNumber.Hide();
                }

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In HideAllSortByRemitEntryFields Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// to clear the customer id fields
        /// </summary>
        /// <param name="formName"></param>
        private void ClearEFTCustomerIdFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromCustomerNumber.Clear();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToCustomerNumber.Clear();
                }
                //else if (formName == Resources.STR_EmailRemittanceEntry)
                //{
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromCustomerNumber.Clear();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToCustomerNumber.Clear();
                //}
                else if (formName == Resources.STR_EmailRemittanceInquiry)
                {
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromCustomerNumber.Clear();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToCustomerNumber.Clear();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearRemitCustomerIdFields Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// to clear the date fields
        /// </summary>
        /// <param name="formName"></param>
        private void ClearEFTDateReceivedFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromDateReceived.Clear();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToDateReceived.Clear();
                }
                //else if (formName == Resources.STR_EmailRemittanceEntry)
                //{
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromDateReceived.Clear();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToDateReceived.Clear();
                //}
                else if (formName == Resources.STR_EmailRemittanceInquiry)
                {
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToDateReceived.Clear();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromDateReceived.Clear();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearRemitDateReceivedFields Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// to clear the ref no. fields
        /// </summary>
        /// <param name="formName"></param>
        private void ClearEFTReferenceNumberFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromReferenceNumber.Clear();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToReferenceNumber.Clear();
                }
                //else if (formName == Resources.STR_EmailRemittanceEntry)
                //{
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromReferenceNumber.Clear();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToReferenceNumber.Clear();
                //}
                else if (formName == Resources.STR_EmailRemittanceInquiry)
                {
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToReferenceNumber.Clear();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromReferenceNumber.Clear();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearRemitReferenceNumberFields Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// to clear the ref no. fields
        /// </summary>
        /// <param name="formName"></param>
        private void ClearEFTDocumentNumberFields(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromItemRefernce.Clear();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToItemRefernce.Clear();
                }
                //else if (formName == Resources.STR_EmailRemittanceEntry)
                //{
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromItemReference.Clear();
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToItemReference.Clear();
                //}
                else if (formName == Resources.STR_EmailRemittanceInquiry)
                {
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToItemReference.Clear();
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromItemReference.Clear();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearRemitDocumentNumberFields Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }



        /// <summary>
        /// When the lookup button 1 is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CustomerPaymentRemitEntryLookupButton1_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                SelectLookupButton1(Resources.STR_EFTPaymentRemittanceWindow);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CustomerRemitEntryLookupButton1_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// lookup selection from payment,email,emailinquiry Remittance entry Window
        /// </summary>
        /// <param name="formName"></param>
        private void SelectLookupButton1(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                // Payment Remittance window
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    if (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalSearchBy.Value == 1)
                    {
                        if (!customerLookupForm.IsOpen)
                        {
                            customerLookupForm.Open();
                            remittanceCustomerLookup = Resources.STR_EFTPaymentRemittanceWindow;
                            remittanceCustomerLookupValue = Resources.STR_From;
                        }
                        else
                            MessageBox.Show("Customer Lookup Window already opened. Please close the window" + Resources.STR_MESSAGE_TITLE);
                    }
                    else if (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalSearchBy.Value == 2)
                    {
                        if (eftReferenceNumberLookupForm.IsOpen)
                        {
                            eftReferenceNumberLookupForm.Close();
                        }
                        eftReferenceNumberLookupForm.Open();
                        referenceLookupName = Resources.STR_EFTPaymentRemittanceWindow;
                        eftReferenceNumberLookupForm.EftReferenceNumberLookup.RedisplayButton.RunValidate();
                        eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value = 1;

                        lookupWindowName = Resources.STR_EFTPaymentRemittanceWindow;
                        if (!RegisterRefidLookupSelect)
                        {
                            eftReferenceNumberLookupForm.EftReferenceNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftPaymentRemitReferenceNumberLookupSelectButton_ClickBeforeOriginal);
                            RegisterRefidLookupSelect = true;
                        }
                    }
                    else if (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalSearchBy.Value == 4)
                    {
                        if (eftDocumentNumberLookupForm.IsOpen)
                        {
                            eftDocumentNumberLookupForm.Close();
                        }
                        eftDocumentNumberLookupForm.Open();
                        documentLookupName = Resources.STR_EFTPaymentRemittanceWindow;
                        eftDocumentNumberLookupForm.EftDocumentNumberLookup.RedisplayButton.RunValidate();
                        eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value = 1;

                        lookupWindowName = Resources.STR_EFTPaymentRemittanceWindow;
                        if (!RegisterDocidLookupSelect)
                        {
                            eftDocumentNumberLookupForm.EftDocumentNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftPaymentRemitDocumentNumberLookupSelectButton_ClickBeforeOriginal);
                            RegisterDocidLookupSelect = true;
                        }
                    }
                }
                //Email Remittance entry window
                //else if (formName == Resources.STR_EmailRemittanceEntry)
                //{

                //    if (eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalSearchBy.Value == 1)
                //    {
                //        if (!customerLookupForm.IsOpen)
                //        {
                //            customerLookupForm.Open();
                //            remittanceCustomerLookup = Resources.STR_EmailRemittanceEntry;
                //            remittanceCustomerLookupValue = Resources.STR_From;
                //        }
                //        else
                //            MessageBox.Show("Customer Lookup Window already opened. Please close the window" + Resources.STR_MESSAGE_TITLE);
                //    }
                    //else if (eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalSearchBy.Value == 2)
                    //{
                    //    if (eftReferenceNumberLookupForm.IsOpen)
                    //    {
                    //        eftReferenceNumberLookupForm.Close();
                    //    }
                    //    eftReferenceNumberLookupForm.Open();
                    //    referenceLookupName = Resources.STR_EmailRemittanceEntry;
                    //    eftReferenceNumberLookupForm.EftReferenceNumberLookup.RedisplayButton.RunValidate();
                    //    eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value = 1;

                    //    lookupWindowName = Resources.STR_EmailRemittanceEntry;
                    //    if (!RegisterRefidLookupSelect)
                    //    {
                    //        eftReferenceNumberLookupForm.EftReferenceNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftPaymentRemitReferenceNumberLookupSelectButton_ClickBeforeOriginal);
                    //        RegisterRefidLookupSelect = true;
                    //    }
                    //}
                    //else if (eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalSearchBy.Value == 4)
                    //{
                    //    if (eftDocumentNumberLookupForm.IsOpen)
                    //    {
                    //        eftDocumentNumberLookupForm.Close();
                    //    }
                    //    eftDocumentNumberLookupForm.Open();
                    //    documentLookupName = Resources.STR_EmailRemittanceEntry;
                    //    eftDocumentNumberLookupForm.EftDocumentNumberLookup.RedisplayButton.RunValidate();
                    //    eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value = 1;

                    //    lookupWindowName = Resources.STR_EmailRemittanceEntry;
                    //    if (!RegisterDocidLookupSelect)
                    //    {
                    //        eftDocumentNumberLookupForm.EftDocumentNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftPaymentRemitDocumentNumberLookupSelectButton_ClickBeforeOriginal);
                    //        RegisterDocidLookupSelect = true;
                    //    }
                    //}
                //}
                //Email Remittance inquiry window 
                else if (formName == Resources.STR_EmailRemittanceInquiry)
                {
                    if (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalSearchBy.Value == 1)
                    {
                        if (!customerLookupForm.IsOpen)
                        {
                            customerLookupForm.Open();
                            remittanceCustomerLookup = Resources.STR_EmailRemittanceInquiry;
                            remittanceCustomerLookupValue = Resources.STR_From;
                        }
                        else
                            MessageBox.Show("Customer Lookup Window already opened. Please close the window" + Resources.STR_MESSAGE_TITLE);
                    }
                    else if (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalSearchBy.Value == 2)
                    {
                        if (eftReferenceNumberLookupForm.IsOpen)
                        {
                            eftReferenceNumberLookupForm.Close();
                        }
                        eftReferenceNumberLookupForm.Open();
                        referenceLookupName = Resources.STR_EmailRemittanceInquiry;
                        eftReferenceNumberLookupForm.EftReferenceNumberLookup.RedisplayButton.RunValidate();
                        eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value = 1;

                        lookupWindowName = Resources.STR_EmailRemittanceInquiry;
                        if (!RegisterRefidLookupSelect)
                        {
                            eftReferenceNumberLookupForm.EftReferenceNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftPaymentRemitReferenceNumberLookupSelectButton_ClickBeforeOriginal);
                            RegisterRefidLookupSelect = true;
                        }
                    }
                    else if (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalSearchBy.Value == 4)
                    {
                        if (eftDocumentNumberLookupForm.IsOpen)
                        {
                            eftDocumentNumberLookupForm.Close();
                        }
                        eftDocumentNumberLookupForm.Open();
                        documentLookupName = Resources.STR_EmailRemittanceInquiry;
                        eftDocumentNumberLookupForm.EftDocumentNumberLookup.RedisplayButton.RunValidate();
                        eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value = 1;

                        lookupWindowName = Resources.STR_EmailRemittanceInquiry;
                        if (!RegisterDocidLookupSelect)
                        {
                            eftDocumentNumberLookupForm.EftDocumentNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftPaymentRemitDocumentNumberLookupSelectButton_ClickBeforeOriginal);
                            RegisterDocidLookupSelect = true;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftReferenceNumberLookupSelectButton_ClickBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        private void CustomerLookupSelectFromPaymentRemittance_OpenAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                CustomerLookup(Resources.STR_EFTPaymentRemittanceWindow, Resources.STR_From);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CustomerLookupSelectFromPaymentRemittance_OpenAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// When the customer lookup  select button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void EftPaymentRemitCustomerIdLookupSelectButton_ClickBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (eftCustomerIdLookupForm.EftCustomerIdLookup.LocalCalledBy.Value == 1)
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromCustomerNumber.Focus();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromCustomerNumber.Value = eftCustomerIdLookupForm.EftCustomerIdLookup.CustomerIdScroll.CustomerNumber.Value;
                    eftCustomerIdLookupForm.Close();
                }
                else if (eftCustomerIdLookupForm.EftCustomerIdLookup.LocalCalledBy.Value == 2)
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToCustomerNumber.Focus();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToCustomerNumber.Value = eftCustomerIdLookupForm.EftCustomerIdLookup.CustomerIdScroll.CustomerNumber.Value;
                    eftCustomerIdLookupForm.Close();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftReferenceNumberLookupSelectButton_ClickBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// When the reference lookup  select button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void EftPaymentRemitReferenceNumberLookupSelectButton_ClickBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!String.IsNullOrEmpty(lookupWindowName))
                {
                    if (lookupWindowName == Resources.STR_EFTPaymentRemittanceWindow && eftReferenceNumberLookupForm.IsOpen)
                    {
                        if (eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value == 1)
                        {
                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromReferenceNumber.Focus();
                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromReferenceNumber.Value = eftReferenceNumberLookupForm.EftReferenceNumberLookup.EftReferenceNumberScroll.EftReferenceNumber.Value;
                            if (string.IsNullOrEmpty(eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToReferenceNumber.Value))
                                eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToReferenceNumber.Value = eftReferenceNumberLookupForm.EftReferenceNumberLookup.EftReferenceNumberScroll.EftReferenceNumber.Value;

                            eftReferenceNumberLookupForm.Close();
                        }
                        else if (eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value == 2)
                        {
                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToReferenceNumber.Focus();
                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToReferenceNumber.Value = eftReferenceNumberLookupForm.EftReferenceNumberLookup.EftReferenceNumberScroll.EftReferenceNumber.Value;
                            eftReferenceNumberLookupForm.Close();
                        }
                    }



                    else if (lookupWindowName == Resources.STR_EFTPaymentRemittanceWindow && eftReferenceNumberLookupForm.IsOpen)
                    {
                        if (eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value == 1)
                        {
                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromReferenceNumber.Focus();
                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromReferenceNumber.Value = eftReferenceNumberLookupForm.EftReferenceNumberLookup.EftReferenceNumberScroll.EftReferenceNumber.Value;
                            eftReferenceNumberLookupForm.Close();
                        }
                        else if (eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value == 2)
                        {
                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToReferenceNumber.Focus();
                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToReferenceNumber.Value = eftReferenceNumberLookupForm.EftReferenceNumberLookup.EftReferenceNumberScroll.EftReferenceNumber.Value;
                            eftReferenceNumberLookupForm.Close();
                        }
                    }

                    //else if (lookupWindowName == Resources.STR_EmailRemittanceEntry && eftReferenceNumberLookupForm.IsOpen)
                    //{
                    //    if (eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value == 1)
                    //    {
                    //        eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromReferenceNumber.Focus();
                    //        eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromReferenceNumber.Value = eftReferenceNumberLookupForm.EftReferenceNumberLookup.EftReferenceNumberScroll.EftReferenceNumber.Value;
                    //        if (string.IsNullOrEmpty(eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToReferenceNumber.Value))
                    //            eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToReferenceNumber.Value = eftReferenceNumberLookupForm.EftReferenceNumberLookup.EftReferenceNumberScroll.EftReferenceNumber.Value;

                    //        eftReferenceNumberLookupForm.Close();
                    //    }
                    //    else if (eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value == 2)
                    //    {
                    //        eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToReferenceNumber.Focus();
                    //        eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToReferenceNumber.Value = eftReferenceNumberLookupForm.EftReferenceNumberLookup.EftReferenceNumberScroll.EftReferenceNumber.Value;
                    //        eftReferenceNumberLookupForm.Close();
                    //    }
                    //}
                    else if (lookupWindowName == Resources.STR_EmailRemittanceInquiry && eftReferenceNumberLookupForm.IsOpen)
                    {
                        if (eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value == 1)
                        {

                            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromReferenceNumber.Focus();
                            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromReferenceNumber.Value = eftReferenceNumberLookupForm.EftReferenceNumberLookup.EftReferenceNumberScroll.EftReferenceNumber.Value;
                            if (string.IsNullOrEmpty(eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToReferenceNumber.Value))
                                eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToReferenceNumber.Value = eftReferenceNumberLookupForm.EftReferenceNumberLookup.EftReferenceNumberScroll.EftReferenceNumber.Value;

                            eftReferenceNumberLookupForm.Close();
                        }
                        else if (eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value == 2)
                        {
                            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToReferenceNumber.Focus();
                            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToReferenceNumber.Value = eftReferenceNumberLookupForm.EftReferenceNumberLookup.EftReferenceNumberScroll.EftReferenceNumber.Value;
                            eftReferenceNumberLookupForm.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftReferenceNumberLookupSelectButton_ClickBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// When the document lookup  select button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void EftPaymentRemitDocumentNumberLookupSelectButton_ClickBeforeOriginal(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!String.IsNullOrEmpty(lookupWindowName))
                {
                    if (lookupWindowName == Resources.STR_EFTPaymentRemittanceWindow && eftDocumentNumberLookupForm.IsOpen)
                    {
                        if (eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value == 1)
                        {
                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromItemRefernce.Focus();
                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromItemRefernce.Value = eftDocumentNumberLookupForm.EftDocumentNumberLookup.EftDocumentNumberScroll.EftItemReference.Value;
                            eftDocumentNumberLookupForm.Close();
                        }
                        else if (eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value == 2)
                        {
                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToItemRefernce.Focus();
                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToItemRefernce.Value = eftDocumentNumberLookupForm.EftDocumentNumberLookup.EftDocumentNumberScroll.EftItemReference.Value;
                            eftDocumentNumberLookupForm.Close();
                        }
                    }
                    //else if (lookupWindowName == Resources.STR_EmailRemittanceEntry && eftDocumentNumberLookupForm.IsOpen)
                    //{
                    //    if (eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value == 1)
                    //    {
                    //        eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromItemReference.Focus();
                    //        eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromItemReference.Value = eftDocumentNumberLookupForm.EftDocumentNumberLookup.EftDocumentNumberScroll.EftItemReference.Value;
                    //        eftDocumentNumberLookupForm.Close();
                    //    }
                    //    else if (eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value == 2)
                    //    {
                    //        eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToItemReference.Focus();
                    //        eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToItemReference.Value = eftDocumentNumberLookupForm.EftDocumentNumberLookup.EftDocumentNumberScroll.EftItemReference.Value;
                    //        eftDocumentNumberLookupForm.Close();
                    //    }
                    //}
                    else if (lookupWindowName == Resources.STR_EmailRemittanceInquiry && eftDocumentNumberLookupForm.IsOpen)
                    {
                        if (eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value == 1)
                        {
                            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromItemReference.Focus();
                            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromItemReference.Value = eftDocumentNumberLookupForm.EftDocumentNumberLookup.EftDocumentNumberScroll.EftItemReference.Value;
                            eftDocumentNumberLookupForm.Close();
                        }
                        else if (eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value == 2)
                        {
                            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToItemReference.Focus();
                            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToItemReference.Value = eftDocumentNumberLookupForm.EftDocumentNumberLookup.EftDocumentNumberScroll.EftItemReference.Value;
                            eftDocumentNumberLookupForm.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftDocumentNumberLookupSelectButton_ClickBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// When the lookup button 2 is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CustomerPaymentRemitEntryLookupButton2_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                SelectLookupButton2(Resources.STR_EFTPaymentRemittanceWindow);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In CustomerRemitEntryLookupButton2_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// lookup2 selection from payment,email,emailinquiry Remittance entry Window
        /// </summary>
        /// <param name="formName"></param>
        private void SelectLookupButton2(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                // Payment Remittance window
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    if (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalSearchBy.Value == 1)
                    {
                        if (!customerLookupForm.IsOpen)
                        {
                            customerLookupForm.Open();
                            remittanceCustomerLookup = Resources.STR_EFTPaymentRemittanceWindow;
                            remittanceCustomerLookupValue = Resources.STR_To;

                        }
                        else
                            MessageBox.Show("Customer Lookup Window already opened. Please close the window" + Resources.STR_MESSAGE_TITLE);

                    }
                    else if (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalSearchBy.Value == 2)
                    {
                        if (eftReferenceNumberLookupForm.IsOpen)
                        {
                            eftReferenceNumberLookupForm.Close();
                        }
                        eftReferenceNumberLookupForm.Open();
                        referenceLookupName = Resources.STR_EFTPaymentRemittanceWindow;
                        eftReferenceNumberLookupForm.EftReferenceNumberLookup.RedisplayButton.RunValidate();
                        eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value = 2;

                        lookupWindowName = Resources.STR_EFTPaymentRemittanceWindow;
                        if (!RegisterRefidLookupSelect)
                        {
                            eftReferenceNumberLookupForm.EftReferenceNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftPaymentRemitReferenceNumberLookupSelectButton_ClickBeforeOriginal);
                            RegisterRefidLookupSelect = true;
                        }
                    }
                    else if (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalSearchBy.Value == 4)
                    {
                        if (eftDocumentNumberLookupForm.IsOpen)
                        {
                            eftDocumentNumberLookupForm.Close();
                        }
                        eftDocumentNumberLookupForm.Open();
                        documentLookupName = Resources.STR_EFTPaymentRemittanceWindow;
                        eftDocumentNumberLookupForm.EftDocumentNumberLookup.RedisplayButton.RunValidate();
                        eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value = 2;

                        lookupWindowName = Resources.STR_EFTPaymentRemittanceWindow;
                        if (!RegisterDocidLookupSelect)
                        {
                            eftDocumentNumberLookupForm.EftDocumentNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftPaymentRemitDocumentNumberLookupSelectButton_ClickBeforeOriginal);
                            RegisterDocidLookupSelect = true;
                        }
                    }
                }
                ////Email Remittance entry window
                //if (formName == Resources.STR_EmailRemittanceEntry)
                //{

                //    if (eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalSearchBy.Value == 1)
                //    {
                //        if (!customerLookupForm.IsOpen)
                //        {
                //            customerLookupForm.Open();
                //            remittanceCustomerLookup = Resources.STR_EmailRemittanceEntry;
                //            remittanceCustomerLookupValue = Resources.STR_To;
                //        }
                //        else
                //            MessageBox.Show("Customer Lookup Window already opened. Please close the window" + Resources.STR_MESSAGE_TITLE);
                //    }
                //    else if (eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalSearchBy.Value == 2)
                //    {
                //        if (eftReferenceNumberLookupForm.IsOpen)
                //        {
                //            eftReferenceNumberLookupForm.Close();
                //        }
                //        eftReferenceNumberLookupForm.Open();
                //        referenceLookupName = Resources.STR_EmailRemittanceEntry;
                //        eftReferenceNumberLookupForm.EftReferenceNumberLookup.RedisplayButton.RunValidate();
                //        eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value = 2;

                //        lookupWindowName = Resources.STR_EmailRemittanceEntry;
                //        if (!RegisterRefidLookupSelect)
                //        {
                //            eftReferenceNumberLookupForm.EftReferenceNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftPaymentRemitReferenceNumberLookupSelectButton_ClickBeforeOriginal);
                //            RegisterRefidLookupSelect = true;
                //        }
                //    }
                //    else if (eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalSearchBy.Value == 4)
                //    {
                //        if (eftDocumentNumberLookupForm.IsOpen)
                //        {
                //            eftDocumentNumberLookupForm.Close();
                //        }
                //        eftDocumentNumberLookupForm.Open();
                //        documentLookupName = Resources.STR_EmailRemittanceEntry;
                //        eftDocumentNumberLookupForm.EftDocumentNumberLookup.RedisplayButton.RunValidate();
                //        eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value = 2;

                //        lookupWindowName = Resources.STR_EmailRemittanceEntry;
                //        if (!RegisterDocidLookupSelect)
                //        {
                //            eftDocumentNumberLookupForm.EftDocumentNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftPaymentRemitDocumentNumberLookupSelectButton_ClickBeforeOriginal);
                //            RegisterDocidLookupSelect = true;
                //        }
                //    }
                //}
                //Email Remittance inquiry window 
                if (formName == Resources.STR_EmailRemittanceInquiry)
                {
                    if (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalSearchBy.Value == 1)
                    {
                        if (!customerLookupForm.IsOpen)
                        {
                            customerLookupForm.Open();
                            remittanceCustomerLookup = Resources.STR_EmailRemittanceInquiry;
                            remittanceCustomerLookupValue = Resources.STR_To;
                        }
                        else
                            MessageBox.Show("Customer Lookup Window already opened. Please close the window" + Resources.STR_MESSAGE_TITLE);
                    }
                    else if (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalSearchBy.Value == 2)
                    {
                        if (eftReferenceNumberLookupForm.IsOpen)
                        {
                            eftReferenceNumberLookupForm.Close();
                        }
                        eftReferenceNumberLookupForm.Open();
                        referenceLookupName = Resources.STR_EmailRemittanceInquiry;
                        eftReferenceNumberLookupForm.EftReferenceNumberLookup.RedisplayButton.RunValidate();
                        eftReferenceNumberLookupForm.EftReferenceNumberLookup.LocalCalledBy.Value = 2;

                        lookupWindowName = Resources.STR_EmailRemittanceInquiry;
                        if (!RegisterRefidLookupSelect)
                        {
                            eftReferenceNumberLookupForm.EftReferenceNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftPaymentRemitReferenceNumberLookupSelectButton_ClickBeforeOriginal);
                            RegisterRefidLookupSelect = true;
                        }
                    }
                    else if (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalSearchBy.Value == 4)
                    {
                        if (eftDocumentNumberLookupForm.IsOpen)
                        {
                            eftDocumentNumberLookupForm.Close();
                        }
                        eftDocumentNumberLookupForm.Open();
                        documentLookupName = Resources.STR_EmailRemittanceInquiry;
                        eftDocumentNumberLookupForm.EftDocumentNumberLookup.RedisplayButton.RunValidate();
                        eftDocumentNumberLookupForm.EftDocumentNumberLookup.LocalCalledBy.Value = 2;

                        lookupWindowName = Resources.STR_EmailRemittanceInquiry;
                        if (!RegisterDocidLookupSelect)
                        {
                            eftDocumentNumberLookupForm.EftDocumentNumberLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftPaymentRemitDocumentNumberLookupSelectButton_ClickBeforeOriginal);
                            RegisterDocidLookupSelect = true;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EftReferenceNumberLookupSelectButton_ClickBeforeOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        private void CustomerLookupSelectToPaymentRemittance_OpenAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                CustomerLookup(Resources.STR_EFTPaymentRemittanceWindow, Resources.STR_To);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }




        /// <summary>
        /// Fetch Customer id form leave after reference  number
        /// </summary>
        /// <param name="p"></param>
        private void FetchCustomerIdForReference(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    if (!string.IsNullOrEmpty(eftRemittancePaymentEntryForm.RemittancePaymentEntry.RemittancePaymentScroll.ReferenceNumber.Value.ToString().Trim()))
                    {
                        ReceivablesRequest receivablesRequest = new ReceivablesRequest();
                        ReceivablesResponse receivablesResponse = new ReceivablesResponse();
                        AuditInformation auditInformation = new AuditInformation();
                        EFTPayment eftPayment = new EFTPayment();

                        auditInformation.CompanyId = Dynamics.Globals.CompanyId.Value;
                        eftPayment.ReferenceNumber = eftRemittancePaymentEntryForm.RemittancePaymentEntry.RemittancePaymentScroll.ReferenceNumber.Value;
                        receivablesRequest.AuditInformation = auditInformation;
                        receivablesRequest.EFTPayment = eftPayment;

                        //Service Call
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationURL);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/SalesOrderUpdate/FetchCustomerIdForReference", receivablesRequest); // we need to refer the web.api service url here.
                            if (response.Result.IsSuccessStatusCode)
                            {
                                receivablesResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                                if (receivablesResponse.Status == ResponseStatus.Success)
                                {
                                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.RemittancePaymentScroll.CustomerNumber.Value = receivablesResponse.CustomerInformation.CustomerId;
                                }
                                else
                                {
                                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In FetchCustomerIdForReference id Method (FetchCustomerIdForReference): " + receivablesResponse.ErrorMessage.ToString());
                                    MessageBox.Show("Error: " + receivablesResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Error: FetchCustomerIdForReference ", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Reference number is mandatory field");
                        eftRemittancePaymentEntryForm.RemittancePaymentEntry.RemittancePaymentScroll.ReferenceNumber.Focus();
                    }
                }

                //// email remittance entry window
                //else if (formName == Resources.STR_EmailRemittanceEntry)
                //{

                //    if (!string.IsNullOrEmpty(eftEmailRemittanceEntryForm.EmailRemittanceEntry.EftEmailRemitScroll.EftReferenceNumber.Value.ToString().Trim()))
                //    {
                //        ReceivablesRequest receivablesRequest = new ReceivablesRequest();
                //        ReceivablesResponse receivablesResponse = new ReceivablesResponse();
                //        AuditInformation auditInformation = new AuditInformation();
                //        EFTPayment eftPayment = new EFTPayment();

                //        auditInformation.CompanyId = Dynamics.Globals.CompanyId.Value;
                //        eftPayment.ReferenceNumber = eftEmailRemittanceEntryForm.EmailRemittanceEntry.EftEmailRemitScroll.EftReferenceNumber.Value;
                //        receivablesRequest.AuditInformation = auditInformation;
                //        receivablesRequest.EFTPayment = eftPayment;

                //        //Service Call
                //        using (HttpClient client = new HttpClient())
                //        {
                //            client.BaseAddress = new Uri(gpServiceConfigurationURL);
                //            client.DefaultRequestHeaders.Accept.Clear();
                //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //            var response = client.PostAsJsonAsync("api/SalesOrderUpdate/FetchCustomerIdForReference", receivablesRequest); // we need to refer the web.api service url here.
                //            if (response.Result.IsSuccessStatusCode)
                //            {
                //                receivablesResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                //                if (receivablesResponse.Status == ResponseStatus.Success)
                //                {
                //                    eftEmailRemittanceEntryForm.EmailRemittanceEntry.EftEmailRemitScroll.CustomerNumber.Value = receivablesResponse.CustomerInformation.CustomerId;
                //                }
                //                else
                //                {
                //                    logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In FetchCustomerIdForReference id Method (FetchCustomerIdForReference): " + receivablesResponse.ErrorMessage.ToString());
                //                    MessageBox.Show("Error: " + receivablesResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                //                }
                //            }
                //            else
                //            {
                //                MessageBox.Show("Error: FetchCustomerIdForReference ", Resources.STR_MESSAGE_TITLE);
                //            }
                //        }
                //    }
                //    else
                //    {
                //        MessageBox.Show("Reference number is mandatory field");
                //    }
                //}
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In FetchCustomerIdForReference Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// EFT Customer Remittance Redisplay Button Click ...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTPaymentRemittanceRedisplayButton_ClickAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                eftRemittancePaymentEntryForm.RemittancePaymentEntry.PushToGp.Disable();
                eftRemittancePaymentEntryForm.RemittancePaymentEntry.Validate.Disable();
                InsertEFTPaymentRemittance();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }



        /// <summary>
        /// Insert EFT Customer Remittance From service call...
        /// </summary>
        /// <param name="p"></param>
        private void InsertEFTPaymentRemittance()
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (!string.IsNullOrEmpty(eftRemittancePaymentEntryForm.RemittancePaymentEntry.BatchNumber.Value))
                {
                    if (eftRemittancePaymentEntryForm.RemittancePaymentEntry.AllOrRange.Value == 0 ||
                        ///Option Button select From...
                     (eftRemittancePaymentEntryForm.RemittancePaymentEntry.AllOrRange.Value == 1 &&
                        //Date From and Date To Field should not empty
                     ((eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromDateReceived.Value.ToString().Trim() != "") &&
                     (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToDateReceived.Value.ToString().Trim() != "")) ||
                        //Customer Number Start and End should not empty...
                     ((eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromCustomerNumber.Value.ToString().Trim() != "") &&
                     (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToCustomerNumber.Value.ToString().Trim() != "")) ||
                        //Customer Number Start and End should not empty...
                     ((eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromReferenceNumber.Value.ToString().Trim() != "") &&
                     (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToReferenceNumber.Value.ToString().Trim() != "")) ||
                        //Document Number start and End Should not empty...
                     ((eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromDateReceived.Value.ToString().Trim() != "") &&
                     (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToDateReceived.Value.ToString().Trim() != ""))))
                    {
                        ReceivablesRequest eftCustomerRemittanceRequest = new ReceivablesRequest();
                        ReceivablesResponse eftResponse = new ReceivablesResponse();
                        AuditInformation eftAudit = new AuditInformation();
                        eftAudit.CompanyId = Dynamics.Globals.CompanyId.Value;
                        eftCustomerRemittanceRequest.CustomerIdStart = eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromCustomerNumber.Value.ToString().Trim() != "" ? eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromCustomerNumber.Value.ToString().Trim() : string.Empty;
                        eftCustomerRemittanceRequest.CustomerIdEnd = eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToCustomerNumber.Value.ToString().Trim() != "" ? eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToCustomerNumber.Value.ToString().Trim() : string.Empty;
                        eftCustomerRemittanceRequest.DateStart = eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromDateReceived.Value != null ? eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromDateReceived.Value : Convert.ToDateTime("1900/01/01");
                        eftCustomerRemittanceRequest.DateEnd = eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToDateReceived.Value != null ? eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToDateReceived.Value : Convert.ToDateTime("1900/01/01");
                        eftCustomerRemittanceRequest.ReferenceNoStart = eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromReferenceNumber.Value.ToString().Trim() != "" ? eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromReferenceNumber.Value.ToString().Trim() : string.Empty;
                        eftCustomerRemittanceRequest.ReferenceNoEnd = eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToReferenceNumber.Value.ToString().Trim() != "" ? eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToReferenceNumber.Value.ToString().Trim() : string.Empty;
                        eftCustomerRemittanceRequest.Searchtype = eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalSearchBy.Value != null ? Convert.ToInt16(eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalSearchBy.Value) : Convert.ToInt16(0);
                        eftCustomerRemittanceRequest.DocNumbrStart = eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromItemRefernce.Value.ToString().Trim() != "" ? eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromItemRefernce.Value.ToString().Trim() : string.Empty;
                        eftCustomerRemittanceRequest.DocNumbrend = eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToItemRefernce.Value.ToString().Trim() != "" ? eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToItemRefernce.Value.ToString().Trim() : string.Empty;
                        eftCustomerRemittanceRequest.Actiontype = eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftApplyType.Value;
                        eftCustomerRemittanceRequest.BatchId = eftRemittancePaymentEntryForm.RemittancePaymentEntry.BatchNumber.Value;
                        eftCustomerRemittanceRequest.AuditInformation = eftAudit;

                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationURL);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/SalesOrderUpdate/GetEFTPaymentRemittances/", eftCustomerRemittanceRequest); // we need to refer the web.api service url here.

                            if (response.Result.IsSuccessStatusCode)
                            {
                                eftResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                                if (eftResponse.Status == ResponseStatus.Error)
                                {
                                    MessageBox.Show("Error: " + eftResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Error: Data does not saved into UpdateTaxScheduleIdToLine Table", Resources.STR_MESSAGE_TITLE);
                            }
                        }

                        if (eftResponse.EFTCustomerRemittancesList.Count == 0)
                        {
                            MessageBox.Show("No records found for the search criteria.", Resources.STR_MESSAGE_TITLE);
                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.Validate.Disable();
                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.PushToGp.Disable();
                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.IsSelected.Disable();
                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.IsSelected.Value = false;
                            ClearEFTRemittanceTemp(Resources.STR_EFTPaymentRemittanceWindow);

                        }
                        else if (eftResponse != null)
                        {
                            InsertPaymentRemittanceBatchDetails(eftResponse);
                            InsertPaymentRemittance(eftResponse);
                        }
                    }
                }
                else
                    MessageBox.Show("Please select Batch ID ", Resources.STR_MESSAGE_TITLE);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertEFTPaymentRemittance Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        /// <summary>
        /// Batch Payment Remittance Details 
        /// </summary>
        /// <param name="eftResponse"></param>
        private void InsertPaymentRemittanceBatchDetails(ReceivablesResponse eftResponse)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (eftResponse.EFTPayment != null)
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftCreatedPaymentTotal.Value = eftResponse.EFTPayment.PaymentAmount;
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftReminingTotal.Value = eftResponse.EFTPayment.RemainingAmount;
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftReminPymntYetToCreate.Value = eftResponse.EFTPayment.RemainingCount;
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftTotalPaymentsCreated.Value = eftResponse.EFTPayment.PaymentCount;
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.DocumentAmount.Value = eftResponse.EFTPayment.ControlAmount;
                }
                else
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftCreatedPaymentTotal.Value = 0;
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftReminingTotal.Value = 0;
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftReminPymntYetToCreate.Value = 0;
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftTotalPaymentsCreated.Value = 0;
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.DocumentAmount.Value = 0;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In InsertPaymentRemittanceBatchDetails Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Insert EFT Customer Remittance details to Temp Table...
        /// </summary>
        /// <param name="eftResponse"></param>
        private void InsertPaymentRemittance(ReceivablesResponse eftResponse)
        {
            try
            {
                if (eftResponse.EFTCustomerRemittancesList.Count > 0)
                {
                    ClearEFTRemittanceTemp(Resources.STR_EFTPaymentRemittanceWindow);
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Close();
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Release();
                    foreach (var eftCustomerRemittance in eftResponse.EFTCustomerRemittancesList)
                    {
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.IsSelected.Value = false;
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.PaymentNumber.Value = eftCustomerRemittance.PaymentNumber;
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftAccountName.Value = eftCustomerRemittance.AccountName;
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.ReferenceNumber.Value = eftCustomerRemittance.ReferenceNumber.ToString().Trim();
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.DateReceived.Value = eftCustomerRemittance.DateReceived;
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.PaymentAmount.Value = eftCustomerRemittance.PaymentAmount;
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.CustomerNumber.Value = eftCustomerRemittance.CustomerID.ToString().Trim();
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftAppId.Value = eftCustomerRemittance.EftAppId;
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftId.Value = eftCustomerRemittance.EftId;
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.IsFullyApplied.Value = Convert.ToBoolean(eftCustomerRemittance.IsFullyApplied);
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.SourceDescription.Value = eftCustomerRemittance.Source;
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftBankOriginating.Value = eftCustomerRemittance.BankOriginatingID.ToString().Trim();
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftItemReference.Value = eftCustomerRemittance.ItemReference;
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftItemAmount.Value = eftCustomerRemittance.ItemAmount;
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.CurrencyId.Value = eftCustomerRemittance.CurrencyID.Trim();
                        short shortVal;
                        short.TryParse(eftCustomerRemittance.EftStatusId.ToString().Trim(), out shortVal);
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftStatus.Value = shortVal;
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftStatusDescription.Value = string.Empty;
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Save();
                    }
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Release();
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Close();
                    eftRemittancePaymentEntryForm.Procedures.EftPaymentRemittancesEntryFormScrollFill.Invoke();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.Validate.Enable();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.IsSelected.Enable();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        /// <summary>
        /// Clear EFT Customer Remittance Temp table...
        /// </summary>
        /// <param name="eftResponse"></param>
        private void ClearEFTRemittanceTemp(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Close();
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Release();
                    TableError tableRemove = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.ChangeFirst();
                    while (tableRemove == TableError.NoError && tableRemove != TableError.EndOfTable)
                    {
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Remove();
                        tableRemove = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.ChangeNext();
                    }
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Release();
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Close();
                    eftRemittancePaymentEntryForm.Procedures.EftPaymentRemittancesEntryFormScrollFill.Invoke();
                }



                else if (formName == Resources.STR_EmailRemittanceInquiry)
                {
                    eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.Close();
                    eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.Release();
                    TableError tableRemove = eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.ChangeFirst();
                    while (tableRemove == TableError.NoError && tableRemove != TableError.EndOfTable)
                    {
                        eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.Remove();
                        tableRemove = eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.ChangeNext();
                    }
                    eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.Release();
                    eftEmailRemittanceInquiryForm.Tables.EftEmailRemitTemp.Close();
                    eftEmailRemittanceInquiryForm.Procedures.EftEmailRemittancesInquiryFormScrollFill.Invoke();
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearEFTCustomerRemittance Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }



        /// <summary>
        /// Clear Customer Remittance window 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTPaymentRemittanceClear_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ClearEFTBatchIDField(Resources.STR_EFTPaymentRemittanceWindow);
                ClearEFTCustomerIdFields(Resources.STR_EFTPaymentRemittanceWindow);
                ClearEFTDateReceivedFields(Resources.STR_EFTPaymentRemittanceWindow);
                ClearEFTReferenceNumberFields(Resources.STR_EFTPaymentRemittanceWindow);
                ClearEFTDocumentNumberFields(Resources.STR_EFTPaymentRemittanceWindow);
                ClearEFTRemittanceTemp(Resources.STR_EFTPaymentRemittanceWindow);

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTCustomerRemittanceClear_ClickAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        /// <summary>
        /// ClearEFTBatchIDField
        /// </summary>
        /// <param name="p"></param>
        private void ClearEFTBatchIDField(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.BatchNumber.Clear();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftCreatedPaymentTotal.Clear();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftReminingTotal.Clear();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftReminPymntYetToCreate.Clear();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftTotalPaymentsCreated.Clear();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.DocumentAmount.Clear();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.BatchNumber.Enable();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LookupButton9.Enable();

                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ClearEFTBatchIDField Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// From customer Number Leave After Original validate the customer number..
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTPaymentRemittanceFromCustomerNumber_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ValidateEFTCustomer(Resources.STR_EFTPaymentRemittanceWindow);

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceFromCustomerNumber_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }

        private void ValidateEFTCustomer(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    if (!string.IsNullOrEmpty(eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromCustomerNumber.Value.ToString().Trim()))
                    {
                        eftRemittancePaymentEntryForm.RemittancePaymentEntry.AllOrRange.Value = 1;
                        if (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToCustomerNumber.Value.ToString().Trim() == string.Empty)
                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToCustomerNumber.Value = eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromCustomerNumber.Value;

                    }
                }
                ////Customer number validate from Email Remittance
                //else if (formName == Resources.STR_EmailRemittanceEntry)
                //{
                //    if (!string.IsNullOrEmpty(eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromCustomerNumber.Value.ToString().Trim()))
                //    {
                //        eftEmailRemittanceEntryForm.EmailRemittanceEntry.AllOrRange.Value = 1;
                //        if (eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToCustomerNumber.Value.ToString().Trim() == string.Empty)
                //            eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToCustomerNumber.Value = eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromCustomerNumber.Value;

                //    }
                //}


                //Customer number validate from Email Remittance
                else if (formName == Resources.STR_EmailRemittanceInquiry)
                {
                    if (!string.IsNullOrEmpty(eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromCustomerNumber.Value.ToString().Trim()))
                    {
                        eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.AllOrRange.Value = 1;
                        if (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToCustomerNumber.Value.ToString().Trim() == string.Empty)
                            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToCustomerNumber.Value = eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromCustomerNumber.Value;

                    }
                }

                     //Customer number validate from Email Remittance scroll
                //else if (formName == Resources.STR_EmailRemittanceEntryScroll)
                //{
                //    if (!string.IsNullOrEmpty(eftEmailRemittanceEntryForm.EmailRemittanceEntry.EftEmailRemitScroll.CustomerNumber.Value.ToString().Trim()))
                //    {
                //        ReceivablesRequest eftCustomerNumberValidateRequest = new ReceivablesRequest();
                //        AuditInformation audit = new AuditInformation();
                //        CustomerInformation custInfo = new CustomerInformation();
                //        custInfo.CustomerId = eftEmailRemittanceEntryForm.EmailRemittanceEntry.EftEmailRemitScroll.CustomerNumber.Value.ToString();
                //        audit.CompanyId = Dynamics.Globals.CompanyId.Value;
                //        eftCustomerNumberValidateRequest.CustomerInformation = custInfo;
                //        eftCustomerNumberValidateRequest.AuditInformation = audit;
                //        // Service call ...
                //        using (HttpClient client = new HttpClient())
                //        {
                //            client.BaseAddress = new Uri(gpServiceConfigurationURL);
                //            client.DefaultRequestHeaders.Accept.Clear();
                //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //            var response = client.PostAsJsonAsync("api/SalesOrderUpdate/ValidateEftCustomer/", eftCustomerNumberValidateRequest); // we need to refer the web.api service url here.

                //            if (response.Result.IsSuccessStatusCode)
                //            {
                //                ReceivablesResponse eftResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                //                if (eftResponse.Status == ResponseStatus.Error)
                //                {
                //                    MessageBox.Show("Error: Customer number not available", Resources.STR_MESSAGE_TITLE);
                //                    eftEmailRemittanceEntryForm.EmailRemittanceEntry.EftEmailRemitScroll.CustomerNumber.Focus();
                //                }
                //            }
                //            else
                //            {
                //                MessageBox.Show("Error: Data does not validate into ValidateEftCustomer", Resources.STR_MESSAGE_TITLE);
                //            }
                //        }
                //    }
                //}

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ValidateEFTCustomer Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// validate Item Reference Number...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTPaymentRemittanceFromItemReference_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ValidateEFTItemReference(Resources.STR_EFTPaymentRemittanceWindow);

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceFromItemReference_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        /// <summary>
        /// ValidateEFTItemReference
        /// </summary>
        /// <param name="formName"></param>
        private void ValidateEFTItemReference(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    if (!string.IsNullOrEmpty(eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromItemRefernce.Value.ToString().Trim()))
                    {
                        eftRemittancePaymentEntryForm.RemittancePaymentEntry.AllOrRange.Value = 1;
                        if (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToItemRefernce.Value.ToString().Trim() == string.Empty)
                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToItemRefernce.Value = eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromItemRefernce.Value;

                    }
                }
                //else if (formName == Resources.STR_EmailRemittanceEntry)
                //{
                //    if (!string.IsNullOrEmpty(eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromItemReference.Value.ToString().Trim()))
                //    {
                //        eftEmailRemittanceEntryForm.EmailRemittanceEntry.AllOrRange.Value = 1;
                //        if (eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToItemReference.Value.ToString().Trim() == string.Empty)
                //            eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToItemReference.Value = eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromItemReference.Value;


                //    }
                //}
                else if (formName == Resources.STR_EmailRemittanceInquiry)
                {
                    if (!string.IsNullOrEmpty(eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromItemReference.Value.ToString().Trim()))
                    {
                        eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.AllOrRange.Value = 1;
                        if (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToItemReference.Value.ToString().Trim() == string.Empty)
                            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToItemReference.Value = eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromItemReference.Value;


                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ValidateEFTItemReference Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// validate Referencer number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTPaymentRemittanceFromReference_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ValidateEftReference(Resources.STR_EFTPaymentRemittanceWindow);

            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceFromReference_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        private void ValidateEftReference(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    if (!string.IsNullOrEmpty(eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromReferenceNumber.Value.ToString().Trim()))
                    {
                        eftRemittancePaymentEntryForm.RemittancePaymentEntry.AllOrRange.Value = 1;
                        if (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToItemRefernce.Value.ToString().Trim() == string.Empty)
                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToItemRefernce.Value = eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromItemRefernce.Value;


                    }
                }
                //else if (formName == Resources.STR_EmailRemittanceEntry)
                //{

                //    if (!string.IsNullOrEmpty(eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromReferenceNumber.Value.ToString().Trim()))
                //    {
                //        eftEmailRemittanceEntryForm.EmailRemittanceEntry.AllOrRange.Value = 1;
                //        if (eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToItemReference.Value.ToString().Trim() == string.Empty)
                //            eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToItemReference.Value = eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromItemReference.Value;


                //    }
                //}
                else if (formName == Resources.STR_EmailRemittanceInquiry)
                {

                    if (!string.IsNullOrEmpty(eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromReferenceNumber.Value.ToString().Trim()))
                    {
                        eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.AllOrRange.Value = 1;
                        if (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToItemReference.Value.ToString().Trim() == string.Empty)
                            eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToItemReference.Value = eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromItemReference.Value;


                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceFromReference_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }

        /// <summary>
        /// ValidateEftCustomer From ToCustomer Number leave
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTPaymentRemittanceToCustomerNumber_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ValidateEftToCustomer(Resources.STR_EFTPaymentRemittanceWindow);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTPaymentRemittanceToCustomerNumber_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// ValidateEftToCustomer
        /// </summary>
        /// <param name="formName"></param>
        private void ValidateEftToCustomer(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    if (!string.IsNullOrEmpty(eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToCustomerNumber.Value.ToString().Trim())
                        && (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromCustomerNumber.Value.ToString().Trim() != eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToCustomerNumber.Value.ToString().Trim()))
                    {
                        eftRemittancePaymentEntryForm.RemittancePaymentEntry.AllOrRange.Value = 1;

                    }
                }

                //else if (formName == Resources.STR_EmailRemittanceEntry)
                //{
                //    if (!string.IsNullOrEmpty(eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToCustomerNumber.Value.ToString().Trim())
                //    && (eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromCustomerNumber.Value.ToString().Trim() != eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToCustomerNumber.Value.ToString().Trim()))
                //    {
                //        eftEmailRemittanceEntryForm.EmailRemittanceEntry.AllOrRange.Value = 1;

                //    }
                //}

                else if (formName == Resources.STR_EmailRemittanceInquiry)
                {

                    if (!string.IsNullOrEmpty(eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToCustomerNumber.Value.ToString().Trim())
                    && (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromCustomerNumber.Value.ToString().Trim() != eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToCustomerNumber.Value.ToString().Trim()))
                    {
                        eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.AllOrRange.Value = 1;

                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ValidateEftToCustomer Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// ValidateEFTItemReference from leave ItemReference Number...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTPaymentRemittanceToItemReference_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ValidateEFTToItemReference(Resources.STR_EFTPaymentRemittanceWindow);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTPaymentRemittanceToItemReference_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        private void ValidateEFTToItemReference(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    if (!string.IsNullOrEmpty(eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToItemRefernce.Value.ToString().Trim())
                        && (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromItemRefernce.Value.ToString().Trim() != eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToItemRefernce.Value.ToString().Trim()))
                    {

                        eftRemittancePaymentEntryForm.RemittancePaymentEntry.AllOrRange.Value = 1;

                    }
                }

                //else if (formName == Resources.STR_EmailRemittanceEntry)
                //{

                //    if (!string.IsNullOrEmpty(eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToItemReference.Value.ToString().Trim())
                //        && (eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromItemReference.Value.ToString().Trim() != eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToItemReference.Value.ToString().Trim()))
                //    {

                //        eftEmailRemittanceEntryForm.EmailRemittanceEntry.AllOrRange.Value = 1;

                //    }
                //}

                if (formName == Resources.STR_EmailRemittanceInquiry)
                {

                    if (!string.IsNullOrEmpty(eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToItemReference.Value.ToString().Trim())
                        && (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromItemReference.Value.ToString().Trim() != eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToItemReference.Value.ToString().Trim()))
                    {
                        eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.AllOrRange.Value = 1;

                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ValidateEFTToItemReference Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }

        }

        /// <summary>
        /// ValidateEFTItemReference from leave Reference Number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTPaymentRemittanceToReference_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ValidateEftToReference(Resources.STR_EFTPaymentRemittanceWindow);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceToReference_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// ValidateEftToReference
        /// </summary>
        /// <param name="formName"></param>
        private void ValidateEftToReference(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    if (!string.IsNullOrEmpty(eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToReferenceNumber.Value.ToString().Trim())
                        && (eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromReferenceNumber.Value.ToString().Trim() != eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToReferenceNumber.Value.ToString().Trim()))
                    {
                        eftRemittancePaymentEntryForm.RemittancePaymentEntry.AllOrRange.Value = 1;
                        ReceivablesRequest eftCustomerNumberValidateRequest = new ReceivablesRequest();
                        AuditInformation audit = new AuditInformation();
                        EFTPayment eftPayment = new EFTPayment();
                        eftPayment.ReferenceNumber = eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToReferenceNumber.Value.ToString().Trim();
                        audit.CompanyId = Dynamics.Globals.CompanyId.Value;
                        eftCustomerNumberValidateRequest.EFTPayment = eftPayment;
                        eftCustomerNumberValidateRequest.AuditInformation = audit;
                        eftCustomerNumberValidateRequest.Actiontype = 0;
                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationURL);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/SalesOrderUpdate/ValidateEftReference/", eftCustomerNumberValidateRequest); // we need to refer the web.api service url here.

                            if (response.Result.IsSuccessStatusCode)
                            {
                                ReceivablesResponse eftResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                                if (eftResponse.Status == ResponseStatus.Error)
                                {
                                    MessageBox.Show("Error: Reference Number available in EFT", Resources.STR_MESSAGE_TITLE);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Error: Data does not saved into ValidateEftReference", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                }

                //else if (formName == Resources.STR_EmailRemittanceEntry)
                //{

                //    if (!string.IsNullOrEmpty(eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToReferenceNumber.Value.ToString().Trim())
                //    && (eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromReferenceNumber.Value.ToString().Trim() != eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToReferenceNumber.Value.ToString().Trim()))
                //    {
                //        eftEmailRemittanceEntryForm.EmailRemittanceEntry.AllOrRange.Value = 1;
                //        ReceivablesRequest eftCustomerNumberValidateRequest = new ReceivablesRequest();
                //        AuditInformation audit = new AuditInformation();
                //        EFTPayment eftPayment = new EFTPayment();
                //        eftPayment.ReferenceNumber = eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToReferenceNumber.Value.ToString().Trim();
                //        audit.CompanyId = Dynamics.Globals.CompanyId.Value;
                //        eftCustomerNumberValidateRequest.EFTPayment = eftPayment;
                //        eftCustomerNumberValidateRequest.AuditInformation = audit;
                //        eftCustomerNumberValidateRequest.Actiontype = 1;
                //        // Service call ...
                //        using (HttpClient client = new HttpClient())
                //        {
                //            client.BaseAddress = new Uri(gpServiceConfigurationURL);
                //            client.DefaultRequestHeaders.Accept.Clear();
                //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //            var response = client.PostAsJsonAsync("api/SalesOrderUpdate/ValidateEftReference/", eftCustomerNumberValidateRequest); // we need to refer the web.api service url here.

                //            if (response.Result.IsSuccessStatusCode)
                //            {
                //                ReceivablesResponse eftResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                //                if (eftResponse.Status == ResponseStatus.Error)
                //                {
                //                    MessageBox.Show("Error: Reference Number available", Resources.STR_MESSAGE_TITLE);
                //                }
                //            }
                //            else
                //            {
                //                MessageBox.Show("Error: Data does not saved into ValidateEftReference", Resources.STR_MESSAGE_TITLE);
                //            }
                //        }
                //    }
                //}
                if (formName == Resources.STR_EmailRemittanceInquiry)
                {

                    if (!string.IsNullOrEmpty(eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToReferenceNumber.Value.ToString().Trim())
                    && (eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromReferenceNumber.Value.ToString().Trim() != eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToReferenceNumber.Value.ToString().Trim()))
                    {
                        eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.AllOrRange.Value = 1;
                        ReceivablesRequest eftCustomerNumberValidateRequest = new ReceivablesRequest();
                        AuditInformation audit = new AuditInformation();
                        EFTPayment eftPayment = new EFTPayment();
                        eftPayment.ReferenceNumber = eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToReferenceNumber.Value.ToString().Trim();
                        audit.CompanyId = Dynamics.Globals.CompanyId.Value;
                        eftCustomerNumberValidateRequest.EFTPayment = eftPayment;
                        eftCustomerNumberValidateRequest.AuditInformation = audit;
                        eftCustomerNumberValidateRequest.Actiontype = 1;
                        // Service call ...
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(gpServiceConfigurationURL);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = client.PostAsJsonAsync("api/SalesOrderUpdate/ValidateEftReference/", eftCustomerNumberValidateRequest); // we need to refer the web.api service url here.

                            if (response.Result.IsSuccessStatusCode)
                            {
                                ReceivablesResponse eftResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                                if (eftResponse.Status == ResponseStatus.Error)
                                {
                                    MessageBox.Show("Error: Reference Number available in EFT", Resources.STR_MESSAGE_TITLE);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Error: Data does not saved into ValidateEftReference", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceToReference_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// EFT Remittance Window Date From Field Leave 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTPaymentRemittanceDateFrom_LeaveAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ValidateRemittanceDate(Resources.STR_EFTPaymentRemittanceWindow);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In EFTRemittanceDateFrom_LeaveAfterOriginal Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Validate Date
        /// </summary>
        /// <param name="formName"></param>
        private void ValidateRemittanceDate(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.AllOrRange.Value = 1;
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalToDateReceived.Value = eftRemittancePaymentEntryForm.RemittancePaymentEntry.LocalFromDateReceived.Value;
                }
                //else if (formName == Resources.STR_EmailRemittanceEntry)
                //{
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.AllOrRange.Value = 1;
                //    eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalToDateReceived.Value = eftEmailRemittanceEntryForm.EmailRemittanceEntry.LocalFromDateReceived.Value;
                //}
                else if (formName == Resources.STR_EmailRemittanceInquiry)
                {
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.AllOrRange.Value = 1;
                    eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalToDateReceived.Value = eftEmailRemittanceInquiryForm.EmailRemittanceInquiry.LocalFromDateReceived.Value;
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ValidateRemittanceDate Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// eftCustomerRemittancesEntry Window select all value...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EFTPaymentRemitAllOrRange_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                RemitttanceAllorRange(Resources.STR_EFTPaymentRemittanceWindow);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ValidateRemittanceDate Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        private void RemitttanceAllorRange(string formName)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                if (formName == Resources.STR_EFTPaymentRemittanceWindow)
                {
                    ClearEFTCustomerIdFields(formName);
                    ClearEFTDateReceivedFields(formName);
                    ClearEFTReferenceNumberFields(formName);
                    ClearEFTDocumentNumberFields(formName);
                    ClearEFTRemittanceTemp(formName);

                }
                else if (formName == Resources.STR_EmailRemittanceEntry)
                {
                    ClearEFTCustomerIdFields(formName);
                    ClearEFTDateReceivedFields(formName);
                    ClearEFTReferenceNumberFields(formName);
                    ClearEFTDocumentNumberFields(formName);
                    ClearEFTRemittanceTemp(formName);

                }
                else if (formName == Resources.STR_EmailRemittanceInquiry)
                {
                    ClearEFTCustomerIdFields(formName);
                    ClearEFTDateReceivedFields(formName);
                    ClearEFTReferenceNumberFields(formName);
                    ClearEFTDocumentNumberFields(formName);
                    ClearEFTRemittanceTemp(formName);

                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In ValidateRemittanceDate Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        /// <summary>
        /// Push To GP
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTPaymentRemitPushToGp_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;
            eftRemittancePaymentEntryForm.RemittancePaymentEntry.PushToGp.Disable();
            try
            {
                ReceivablesRequest eftRequest = new ReceivablesRequest();

                List<EFTCustomerPayment> EFTOriginalList = new List<EFTCustomerPayment>();
                AuditInformation auditInformation = new AuditInformation();
                eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Close();
                eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Release();
                TableError eftPaymentRemittance = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.GetFirst();
                //push Record one by  one...
                while (eftPaymentRemittance == TableError.NoError && eftPaymentRemittance != TableError.EndOfTable)
                {
                    if (eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.IsSelected.Value == true)
                    {
                        EFTCustomerPayment eftPayment = new EFTCustomerPayment();
                        eftPayment.EftId = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftId.Value;
                        eftPayment.PaymentNumber = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.PaymentNumber.Value;
                        eftPayment.ReferenceNumber = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.ReferenceNumber.Value;
                        eftPayment.DateReceived = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.DateReceived.Value;
                        eftPayment.PaymentAmount = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.PaymentAmount.Value;
                        eftPayment.CustomerID = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.CustomerNumber.Value;
                        eftPayment.EftAppId = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftAppId.Value;
                        eftPayment.IsFullyApplied = Convert.ToBoolean(eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.IsFullyApplied.Value);
                        eftPayment.Source = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.SourceDescription.Value;
                        eftPayment.BankOriginating = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftBankOriginating.Value;
                        eftPayment.ItemReferenceNumber = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftItemReference.Value;
                        eftPayment.ItemAmount = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftItemAmount.Value;
                        eftPayment.CurrencyId = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.CurrencyId.Value;
                        eftPayment.IsUpdated = 1;
                        eftPayment.Status = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftStatus.Value;
                        eftPayment.StatusReason = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftStatusDescription.Value;
                        eftPayment.AccountName = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftAccountName.Value;
                        EFTOriginalList.Add(eftPayment);
                        eftPayment = null;
                    }
                    eftPaymentRemittance = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.GetNext();
                }
                if (EFTOriginalList.Count == 0)
                {
                    MessageBox.Show("Please select the record to process.", Resources.STR_MESSAGE_TITLE); MessageBox.Show("Select atleast one record ", Resources.STR_MESSAGE_TITLE);
                }
                else
                {
                    auditInformation.CreatedBy = Dynamics.Globals.UserName.Value;
                    auditInformation.CompanyId = Dynamics.Globals.CompanyId.Value;
                    eftRequest.AuditInformation = auditInformation;
                    eftRequest.EFTCustomerPaymentList = EFTOriginalList;
                    eftRequest.Actiontype = eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftApplyType.Value;
                    eftRequest.BatchId = eftRemittancePaymentEntryForm.RemittancePaymentEntry.BatchNumber.Value;
                    eftRequest.PaymentCount = eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftReminPymntYetToCreate.Value + eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftTotalPaymentsCreated.Value;
                    eftRequest.ControlAmount = eftRemittancePaymentEntryForm.RemittancePaymentEntry.DocumentAmount.Value;

                    using (HttpClient client = new HttpClient())
                    {

                        client.BaseAddress = new Uri(gpServiceConfigurationURL);

                        client.DefaultRequestHeaders.Accept.Clear();

                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/ReceivablesManagement/PushEftTransactionsToGP", eftRequest); // we need to refer the web.api service url here.

                        if (response.Result.IsSuccessStatusCode)
                        {
                            ReceivablesResponse recvgResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                            if (recvgResponse.Status == ResponseStatus.NoRecord)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " No Record Available for process");
                                eftRemittancePaymentEntryForm.RemittancePaymentEntry.RedisplayButton.RunValidate();
                                ClearEFTRemittanceTemp(Resources.STR_EFTPaymentRemittanceWindow);
                                eftRemittancePaymentEntryForm.RemittancePaymentEntry.Validate.Disable();
                                eftRemittancePaymentEntryForm.RemittancePaymentEntry.IsSelected.Disable();
                                eftRemittancePaymentEntryForm.RemittancePaymentEntry.IsSelected.Value = false;
                                MessageBox.Show("No Record Available for process", Resources.STR_MESSAGE_TITLE);
                            }
                            else if (recvgResponse.Status == ResponseStatus.Success)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " PushToGp Work Successfully");
                                eftRemittancePaymentEntryForm.RemittancePaymentEntry.RedisplayButton.RunValidate();
                                ClearEFTRemittanceTemp(Resources.STR_EFTPaymentRemittanceWindow);
                                eftRemittancePaymentEntryForm.RemittancePaymentEntry.Validate.Disable();
                                eftRemittancePaymentEntryForm.RemittancePaymentEntry.IsSelected.Disable();
                                eftRemittancePaymentEntryForm.RemittancePaymentEntry.IsSelected.Value = false;
                                MessageBox.Show("Pushed Successfully", Resources.STR_MESSAGE_TITLE);
                            }
                            else if (recvgResponse.Status == ResponseStatus.Partial)
                            {
                                ClearEFTRemittanceTemp(Resources.STR_EFTPaymentRemittanceWindow);
                                eftRemittancePaymentEntryForm.RemittancePaymentEntry.Validate.Disable();
                                eftRemittancePaymentEntryForm.RemittancePaymentEntry.IsSelected.Disable();
                                eftRemittancePaymentEntryForm.RemittancePaymentEntry.IsSelected.Value = false;
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " one or more record failed to Push");
                                MessageBox.Show("One or more record failed to push", Resources.STR_MESSAGE_TITLE);
                            }
                            else if (recvgResponse.Status == ResponseStatus.Error)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + "Error in PushToGP ");// + recvgResponse.ErrorMessage.ToString());
                                ClearEFTRemittanceTemp(Resources.STR_EFTPaymentRemittanceWindow);
                                MessageBox.Show("Error in PushToGP", Resources.STR_MESSAGE_TITLE);
                            }
                            else if (recvgResponse.Status == ResponseStatus.Unknown)
                            {
                                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Unkown error occured): ");// + recvgResponse.ErrorMessage.ToString());
                                ClearEFTRemittanceTemp(Resources.STR_EFTPaymentRemittanceWindow);
                                MessageBox.Show("Unkown error", Resources.STR_MESSAGE_TITLE);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PushToGP Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
                ClearEFTRemittanceTemp(Resources.STR_EFTPaymentRemittanceWindow);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
                System.Windows.Forms.Cursor.Current = Cursors.Arrow;
            }

        }

        /// <summary>
        /// Validate Payment Remittance Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EFTPaymentRemitValidate_ClickAfterOriginal(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                ReceivablesRequest eftRequest = new ReceivablesRequest();
                List<EFTCustomerPayment> eftPaymentRemittanceList = new List<EFTCustomerPayment>();
                AuditInformation auditInformation = new AuditInformation();
                eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Close();
                eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Release();

                TableError eftPaymentRemittance = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.GetFirst();

                while (eftPaymentRemittance == TableError.NoError && eftPaymentRemittance != TableError.EndOfTable)
                {
                    if (eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.IsSelected.Value == true)
                    {
                        EFTCustomerPayment eftCashReceipt = new EFTCustomerPayment();
                        eftCashReceipt.EftId = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftId.Value;
                        eftCashReceipt.EftAppId = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftAppId.Value;
                        eftCashReceipt.PaymentNumber = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.PaymentNumber.Value.ToString().Trim();
                        eftCashReceipt.ReferenceNumber = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.ReferenceNumber.Value.ToString().Trim();
                        eftCashReceipt.DateReceived = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.DateReceived.Value;
                        eftCashReceipt.PaymentAmount = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.PaymentAmount.Value;
                        eftCashReceipt.CustomerID = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.CustomerNumber.Value.ToString().Trim();
                        eftCashReceipt.CurrencyId = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.CurrencyId.Value;
                        eftCashReceipt.IsFullyApplied = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.IsFullyApplied.Value;
                        eftCashReceipt.Source = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.SourceDescription.Value.ToString().Trim();
                        eftCashReceipt.BankOriginating = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftBankOriginating.Value.ToString().Trim();
                        eftCashReceipt.ItemReferenceNumber = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftItemReference.Value.ToString().Trim();
                        eftCashReceipt.ItemAmount = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftItemAmount.Value;
                        eftCashReceipt.CreatedBy = Dynamics.Globals.UserId.Value.ToString();
                        eftCashReceipt.IsUpdated = 1;
                        eftCashReceipt.Status = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftStatus.Value;
                        eftCashReceipt.StatusReason = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftStatusDescription.Value;
                        eftCashReceipt.AccountName = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftAccountName.Value;

                        eftPaymentRemittanceList.Add(eftCashReceipt);
                    }
                    eftPaymentRemittance = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.GetNext();
                }
                eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Release();
                eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Close();

                if (eftPaymentRemittanceList.Count == 0)
                {
                    MessageBox.Show("Select atleast one record ", Resources.STR_MESSAGE_TITLE);
                }
                else
                {
                    auditInformation.CreatedBy = Dynamics.Globals.UserId.Value.ToString();
                    auditInformation.CompanyId = Dynamics.Globals.CompanyId.Value;

                    eftRequest.AuditInformation = auditInformation;
                    eftRequest.EFTCustomerPaymentList = eftPaymentRemittanceList;
                    eftRequest.Actiontype = eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftApplyType.Value;

                    int intIsSelectedCount = eftRequest.EFTCustomerPaymentList.Count;

                    if (eftRequest.EFTCustomerPaymentList != null && eftRequest.EFTCustomerPaymentList.Count > 0)
                    {
                        if (eftRequest != null)
                        {
                            // Service call ...
                            using (HttpClient client = new HttpClient())
                            {
                                client.BaseAddress = new Uri(gpServiceConfigurationURL);
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                var response = client.PostAsJsonAsync("api/ReceivablesManagement/ValidatePaymentRemittance/", eftRequest); // we need to refer the web.api service url here.

                                if (response.Result.IsSuccessStatusCode)
                                {
                                    ReceivablesResponse eftResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                                    if (eftResponse.Status == ResponseStatus.Error)
                                    {
                                        MessageBox.Show("Error: Data does not saved into ValidateEftPaymentRemittance", Resources.STR_MESSAGE_TITLE);

                                    }
                                    else
                                    {
                                        if (eftResponse.EFTCustomerPaymentList.Count > 0)
                                        {
                                            eftRemittancePaymentEntryForm.RemittancePaymentEntry.IsSelected.Value = true;
                                            FillPaymentRemittanceScrollResponse(eftResponse);
                                            EnablePushToGPButton(eftResponse, intIsSelectedCount);
                                        }
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Error: Data does not saved into ValidateEftPaymentRemittance", Resources.STR_MESSAGE_TITLE);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In PushToGP Method: " + ex.Message.ToString());
                MessageBox.Show(ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }

        private void FillPaymentRemittanceScrollResponse(ReceivablesResponse eftResponse)
        {
            try
            {
                ClearEFTRemittanceTemp(Resources.STR_EFTPaymentRemittanceWindow);
                eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Close();
                eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Release();
                foreach (var eftPaymentRemittance in eftResponse.EFTCustomerPaymentList)
                {
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.PaymentNumber.Value = eftPaymentRemittance.PaymentNumber.ToString().Trim();
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.ReferenceNumber.Value = eftPaymentRemittance.ReferenceNumber.ToString().Trim();
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.DateReceived.Value = eftPaymentRemittance.DateReceived;
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.PaymentAmount.Value = eftPaymentRemittance.PaymentAmount;
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.CustomerNumber.Value = eftPaymentRemittance.CustomerID.ToString().Trim();
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftId.Value = eftPaymentRemittance.EftId;
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftAppId.Value = eftPaymentRemittance.EftAppId;
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.IsFullyApplied.Value = Convert.ToBoolean(eftPaymentRemittance.IsFullyApplied);
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftStatus.Value = Convert.ToInt16(eftPaymentRemittance.Status);
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftStatusDescription.Value = eftPaymentRemittance.StatusReason;
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.SourceDescription.Value = eftPaymentRemittance.Source;
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftBankOriginating.Value = eftPaymentRemittance.BankOriginating.ToString().Trim();
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftItemReference.Value = eftPaymentRemittance.ItemReferenceNumber;
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftItemAmount.Value = eftPaymentRemittance.ItemAmount;
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.CurrencyId.Value = eftPaymentRemittance.CurrencyId;
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.IsSelected.Value = true;
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftAccountName.Value = eftPaymentRemittance.AccountName;
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Save();
                }
                eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Release();
                eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Close();
                eftRemittancePaymentEntryForm.Procedures.EftPaymentRemittancesEntryFormScrollFill.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

        }

        private void EnablePushToGPButton(ReceivablesResponse eftResponse, int isSelectedCount)
        {
            int statusDescriptionCount = 0;
            eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Close();
            eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Release();

            TableError eftPaymentRemittance = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.GetFirst();
            while (eftPaymentRemittance == TableError.NoError && eftPaymentRemittance != TableError.EndOfTable)
            {

                if (eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.IsSelected.Value == true &&
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.EftStatus.Value == 0)
                {
                    statusDescriptionCount++;
                }

                eftPaymentRemittance = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.GetNext();
            }
            eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Release();
            eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Close();

            if (statusDescriptionCount == isSelectedCount)
            {
                eftRemittancePaymentEntryForm.RemittancePaymentEntry.PushToGp.Enable();
                eftRemittancePaymentEntryForm.RemittancePaymentEntry.Validate.Disable();
            }
            else
            {
                MessageBox.Show("One or More line having issues", Resources.STR_MESSAGE_TITLE);
                eftRemittancePaymentEntryForm.RemittancePaymentEntry.Validate.Enable();
            }
        }

        void RemittancePaymentEntryLookupButton9_ClickAfterOriginal(object sender, EventArgs e)
        {
            try
            {
                if (eftBatchIdLookupForm.IsOpen)
                {
                    eftBatchIdLookupForm.Close();
                }
                eftBatchIdLookupForm.Open();
                eftBatchIdLookupForm.EftBatchIdLookup.RedisplayButton.RunValidate();

                lookupWindowName = Resources.STR_EFTPaymentRemittanceWindow;

                eftBatchIdLookupForm.EftBatchIdLookup.SelectButton.ClickBeforeOriginal += new System.ComponentModel.CancelEventHandler(this.EftBatchIdLookupSelectButton_ClickBeforeOriginal);

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        void RemittancePaymentEntryEftBatchId_Change(object sender, EventArgs e)
        {
            try
            {
                if (eftRemittancePaymentEntryForm.RemittancePaymentEntry.BatchNumber.Value != string.Empty)
                {
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.BatchNumber.Disable();
                    eftRemittancePaymentEntryForm.RemittancePaymentEntry.LookupButton9.Disable();


                    ReceivablesRequest eftCustomerRemittanceRequest = new ReceivablesRequest();
                    ReceivablesResponse eftResponse = new ReceivablesResponse();
                    AuditInformation eftAudit = new AuditInformation();
                    eftAudit.CompanyId = Dynamics.Globals.CompanyId.Value;
                    eftCustomerRemittanceRequest.BatchId = eftRemittancePaymentEntryForm.RemittancePaymentEntry.BatchNumber.Value;
                    eftCustomerRemittanceRequest.AuditInformation = eftAudit;


                    // Service call ...
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(gpServiceConfigurationURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = client.PostAsJsonAsync("api/SalesOrderUpdate/GetEFTPaymentRemittanceAmountDetails/", eftCustomerRemittanceRequest); // we need to refer the web.api service url here.

                        if (response.Result.IsSuccessStatusCode)
                        {
                            eftResponse = response.Result.Content.ReadAsAsync<ReceivablesResponse>().Result;
                            if (eftResponse.Status == ResponseStatus.Error)
                            {
                                MessageBox.Show("Error: " + eftResponse.ErrorMessage, Resources.STR_MESSAGE_TITLE);
                            }
                            else
                            {
                                eftRemittancePaymentEntryForm.RemittancePaymentEntry.DocumentAmount.Value = eftResponse.EFTPayment.ControlAmount;
                                eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftCreatedPaymentTotal.Value = eftResponse.EFTPayment.PaymentAmount;
                                eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftReminPymntYetToCreate.Value = eftResponse.EFTPayment.RemainingCount;
                                eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftTotalPaymentsCreated.Value = eftResponse.EFTPayment.PaymentCount;
                                eftRemittancePaymentEntryForm.RemittancePaymentEntry.EftReminingTotal.Value = eftResponse.EFTPayment.RemainingAmount;

                            }
                        }
                        else
                        {
                            MessageBox.Show("Error: Data does not saved into UpdateTaxScheduleIdToLine Table", Resources.STR_MESSAGE_TITLE);
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void EFTPaymentRemitIsSelected_Change(object sender, EventArgs e)
        {
            eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Close();
            eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Release();

            eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Key = 1;
            TableError errorValidTemp = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Change();
            if (errorValidTemp != TableError.EndOfTable && errorValidTemp == TableError.NoError)
            {
                if (eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.IsSelected.Value == false)
                {
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.IsSelected.Value = false;
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Save();
                }
                else
                {
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.IsSelected.Value = true;
                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Save();
                }
            }

        }

        /// <summary>
        /// Multiple select and deselect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void IsSelected_Change(object sender, EventArgs e)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Close();
                eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Release();
                TableError tableError = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.ChangeFirst();
                while (tableError == TableError.NoError && tableError != TableError.EndOfTable)
                {
                    if (eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.IsSelected.Value == false)
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.IsSelected.Value = true;
                    else
                        eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.IsSelected.Value = false;

                    eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Save();
                    tableError = eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.ChangeNext();
                }
                eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Release();
                eftRemittancePaymentEntryForm.Tables.RemittancePaymentTemp.Close();
                eftRemittancePaymentEntryForm.Procedures.EftPaymentRemittancesEntryFormScrollFill.Invoke();
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now + " - " + Dynamics.Globals.UserId.Value.ToString() + " Error In IsSelected_Change Method: " + ex.Message.ToString());
                MessageBox.Show("Could not be select multiple selection : " + ex.Message.ToString(), Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
                LogPostProcessDetailsToFile(logMessage.ToString(), "ReceivablesDetails");
                logMessage = null;
            }
        }


        #endregion PaymentRemittance



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
                case "ReceivablesDetails":
                    if (isRmLoggingEnabled && message != "")
                        new TextLogger().LogInformationIntoFile(message, rmLogFilePath, rmLogFileName);
                    break;
                case "CashApplication":
                    if (isCashAppEnabled && message != "")
                        new TextLogger().LogInformationIntoFile(message, cashLogFilePath, cashLogFileName);
                    break;
                default:
                    break;
            }
        }
    }
}
