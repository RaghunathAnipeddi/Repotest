using Chempoint.GP.Infrastructure.Config;
using Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider.Context;
using Chempoint.GP.Infrastructure.Utils;
using Chempoint.GP.Model.Interactions.PayableManagement;
using ChemPoint.GP.DataContracts.PM;
using System;
using System.Data;
using System.Data.SqlClient;

namespace ChemPoint.GP.PMDL
{
    /// <PayableManagement>
    /// Project Name        :   payable mangement migration
    /// Affected Module     :   Purchase
    /// Affected Windows    :   DAL
    /// Developed on        :   April2017
    /// Developed by        :   Muthu
    ///-------------------------------------------------------------------------------------------
    ///     Date		Author          Description
    ///-------------------------------------------------------------------------------------------
    /// **Apr2017       Mthangaraj      Initial Creation.
    /// </PayableManagement>
    public class PMRepositoryDL : RepositoryBase, IPayableManagementRepository
    {
        public PMRepositoryDL(string connectionString) : base(new GpAddInDbContext(connectionString))
        {
        }

        #region PayableManagement
        /// <summary>
        /// Save and validate API file details
        /// </summary>
        /// <param name="requestObj"></param>
        /// <param name="poInv">po transaction details</param>
        /// <param name="nonPoInv">non-po transaction details</param>
        /// <returns>return validated recsult set</returns>
        public DataSet SaveAPIDetailsToDB(PayableManagementRequest requestObj, DataSet poInv, DataSet nonPoInv)
        {
            DataSet uploadedAPIDetails = null;
            uploadedAPIDetails = new DataSet();
            //Fetch the records from the view 
            var cmd = CreateStoredProcCommand(requestObj.companyId == 1 ? Configuration.SPUploadAllAPITransactionsNA : Configuration.SPUploadAllAPITransactionsEU);

            cmd.Parameters.AddInputParams(Configuration.SPUploadAllAPITransactions_Param1, SqlDbType.Structured, poInv.Tables[0]);
            cmd.Parameters.AddInputParams(Configuration.SPUploadAllAPITransactions_Param2, SqlDbType.Structured, nonPoInv.Tables[0]);

            uploadedAPIDetails = base.GetDataSet(cmd);

            if (uploadedAPIDetails.Tables[1].Rows[0]["ErrorDetails"].ToString().Length > 0)
            {
                throw new Exception(uploadedAPIDetails.Tables[1].Rows[0]["ErrorDetails"].ToString());
            }
            return uploadedAPIDetails;
        }

        /// <summary>
        /// Validate API file details
        /// </summary>
        /// <param name="savedApiDetails">poAll saved details</param>
        /// <returns>return validated recsult set</returns>
        public DataSet ValidateAPIDetailsToDB(DataSet savedApiDetails, int companyId)
        {
            DataSet uploadedAPIDetails = null;
            uploadedAPIDetails = new DataSet();
            //Validating the records from the view 
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPValidateAPITransactionsNA : Configuration.SPValidateAPITransactionsEU);
            cmd.Parameters.AddInputParams(Configuration.SPValidateAPITransactions_Param1, SqlDbType.Structured, savedApiDetails.Tables[0]);
            cmd.Parameters.AddOutputParams(Configuration.SPValidateAPITransactions_Param2, SqlDbType.VarChar, 50);

            uploadedAPIDetails = base.GetDataSet(cmd);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            string error = Convert.ToString(commandResult.Parameters["@" + Configuration.SPValidateAPITransactions_Param2].Value);

            if (error.Length > 0)
            {
                throw new Exception(error);
            }
            return uploadedAPIDetails;
        }

        #endregion PayableManagement

        #region CTSI Implementation

        public DataSet GetFailedCTSIIdsList(int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetFailedCTSIIdsListNA : Configuration.SPGetFailedCTSIIdsListEU);
            return base.GetDataSet(cmd);
        }
        public DataSet GetFailedCtsiDocuments(string searchType, string fromCtsiId, string toCtsiId, DateTime fromDocumentDate, DateTime toDocumentDate, string fromVendorId, string toVendorId, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetFailedCtsiDocumentsNA : Configuration.SPGetFailedCtsiDocumentsEU);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedCtsiDocumentsParam1, SqlDbType.VarChar, searchType);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedCtsiDocumentsParam2, SqlDbType.VarChar, fromCtsiId);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedCtsiDocumentsParam3, SqlDbType.VarChar, toCtsiId);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedCtsiDocumentsParam4, SqlDbType.DateTime, fromDocumentDate);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedCtsiDocumentsParam5, SqlDbType.DateTime, toDocumentDate);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedCtsiDocumentsParam6, SqlDbType.VarChar, fromVendorId);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedCtsiDocumentsParam7, SqlDbType.VarChar, toVendorId);
            return base.GetDataSet(cmd);
        }
        public DataSet ValidateAndGetCtsiTransactions(string userId, int companyId, DataTable transactionDetails)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPValidateAndGetCtsiTransactionsNA : Configuration.SPValidateAndGetCtsiTransactionsEU);
            cmd.Parameters.AddInputParams(Configuration.SPValidateAndGetCtsiTransactionsParam1, SqlDbType.VarChar, userId);
            cmd.Parameters.AddInputParams(Configuration.SPValidateAndGetCtsiTransactionsParam2, SqlDbType.Int, companyId);
            cmd.Parameters.AddInputParams(Configuration.SPValidateAndGetCtsiTransactionsParam3, SqlDbType.Structured, transactionDetails);
            return base.GetDataSet(cmd);
        }
        public bool ValidateVoucherExistsForVendor(string paymentNumber, string vendorId, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPValidateVoucherExistsForVendorNA : Configuration.SPValidateVoucherExistsForVendorEU);
            cmd.Parameters.AddInputParams(Configuration.SPValidateVoucherExistsForVendorParam1, SqlDbType.VarChar, paymentNumber);
            cmd.Parameters.AddInputParams(Configuration.SPValidateVoucherExistsForVendorParam2, SqlDbType.VarChar, vendorId);
            cmd.Parameters.AddOutputParams(Configuration.SPValidateVoucherExistsForVendorParam3, SqlDbType.Bit);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            return Convert.ToBoolean(commandResult.Parameters["@" + Configuration.SPValidateVoucherExistsForVendorParam3].Value);
        }
        public Int16 UpdateManualPaymentNumberForCTSIDocuments(PayableManagementRequest requestObj)
        {
            var cmd = CreateStoredProcCommand(requestObj.companyId == 1 ? Configuration.SPUpdateManualPaymentNumberForCTSIDocumentsNA : Configuration.SPUpdateManualPaymentNumberForCTSIDocumentsEU);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateManualPaymentNumberForCTSIDocumentsParam1, SqlDbType.VarChar, requestObj.userId);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateManualPaymentNumberForCTSIDocumentsParam2, SqlDbType.Int, requestObj.companyId);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateManualPaymentNumberForCTSIDocumentsParam3, SqlDbType.Structured, this.ConvertCtsiManualPaymentListtoTable(requestObj));
            cmd.Parameters.AddOutputParams(Configuration.SPUpdateManualPaymentNumberForCTSIDocumentsParam4, SqlDbType.Int);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            return Convert.ToInt16(commandResult.Parameters["@" + Configuration.SPUpdateManualPaymentNumberForCTSIDocumentsParam4].Value);
        }
        public DataSet GetFailedCtsiIdsToLinkManualPayments(string searchType, string fromCtsiId, string toCtsiId, DateTime fromDocumentDate,
           DateTime toDocumentDate, string fromVendorId, string toVendorId, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetFailedCtsiIdsToLinkManualPaymentsNA : Configuration.SPGetFailedCtsiIdsToLinkManualPaymentsEU);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedCtsiIdsToLinkManualPaymentsParam1, SqlDbType.VarChar, searchType);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedCtsiIdsToLinkManualPaymentsParam2, SqlDbType.VarChar, fromCtsiId);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedCtsiIdsToLinkManualPaymentsParam3, SqlDbType.VarChar, toCtsiId);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedCtsiIdsToLinkManualPaymentsParam4, SqlDbType.DateTime, fromDocumentDate);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedCtsiIdsToLinkManualPaymentsParam5, SqlDbType.DateTime, toDocumentDate);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedCtsiIdsToLinkManualPaymentsParam6, SqlDbType.VarChar, fromVendorId);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedCtsiIdsToLinkManualPaymentsParam7, SqlDbType.VarChar, toVendorId);
            return base.GetDataSet(cmd);
        }
        public bool VerifyCtsiLookupValueExists(string sourceLookup, string lookupValue, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPVerifyCtsiLookupValueExistsNA : Configuration.SPVerifyCtsiLookupValueExistsEU);
            cmd.Parameters.AddInputParams(Configuration.SPVerifyCtsiLookupValueExistsParam1, SqlDbType.VarChar, sourceLookup);
            cmd.Parameters.AddInputParams(Configuration.SPVerifyCtsiLookupValueExistsParam2, SqlDbType.VarChar, lookupValue);
            cmd.Parameters.AddOutputParams(Configuration.SPVerifyCtsiLookupValueExistsParam3, SqlDbType.Bit);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            return Convert.ToBoolean(commandResult.Parameters["@" + Configuration.SPVerifyCtsiLookupValueExistsParam3].Value);
        }

        #endregion

        #region PayableService

        public DataSet FetchAccountDetails(int actIndex, string vendorID, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPFetchAccountNumberDetailsNA : Configuration.SPFetchAccountNumberDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPFetchAccountNumberDetailsParam1, SqlDbType.Int, actIndex);
            cmd.Parameters.AddInputParams(Configuration.SPFetchAccountNumberDetailsParam2, SqlDbType.VarChar, vendorID);
            return base.GetDataSet(cmd);
        }

        public DataTable GetNonPODistributionValues(DataTable paymentTable, string payAcntNumb, string payAcntDesc, int companyId, out double ten99Amnt)
        {
            ten99Amnt = 0.0;
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetNonPODistributionValuesNA : Configuration.SPGetNonPODistributionValuesEU);
            cmd.Parameters.AddInputParams(Configuration.SPGetNonPODistributionValuesParam1, SqlDbType.Structured, paymentTable);
            cmd.Parameters.AddInputParams(Configuration.SPGetNonPODistributionValuesParam2, SqlDbType.VarChar, payAcntNumb);
            cmd.Parameters.AddInputParams(Configuration.SPGetNonPODistributionValuesParam3, SqlDbType.VarChar, payAcntDesc);
            cmd.Parameters.AddOutputParams(Configuration.SPGetNonPODistributionValuesParam4, SqlDbType.Float);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            double.TryParse(commandResult.Parameters["@" + Configuration.SPGetNonPODistributionValuesParam4].Value.ToString(), out ten99Amnt);
            DataSet nonPODT = base.GetDataSet(cmd);
            return nonPODT.Tables[0];
        }
        public string GetNextInvoiceNumberForApi(int companyId, int invType)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetNextInvoiceNumberForApiNA : Configuration.SPGetNextInvoiceNumberForApiEU);
            cmd.Parameters.AddInputParams(Configuration.SPGetNextInvoiceNumberForApiParam1, SqlDbType.Int, invType);
            cmd.Parameters.AddOutputParams(Configuration.SPGetNextInvoiceNumberForApiParam2, SqlDbType.VarChar, 51);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            return Convert.ToString(commandResult.Parameters["@" + Configuration.SPGetNextInvoiceNumberForApiParam2].Value);
        }

        public string GetNextInvoiceNumber(int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetNextInvoiceNumberNA : Configuration.SPGetNextInvoiceNumberEU);
            cmd.Parameters.AddInputParams(Configuration.SPGetNextInvoiceNumberParam1, SqlDbType.Int, 2);
            cmd.Parameters.AddOutputParams(Configuration.SPGetNextInvoiceNumberParam2, SqlDbType.VarChar, 51);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            return Convert.ToString(commandResult.Parameters["@" + Configuration.SPGetNextInvoiceNumberParam2].Value);
        }

        public void UpdatePaymentStatus(DataTable transactionDetails, string userId, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPUpdatePaymentStatusNA : Configuration.SPUpdatePaymentStatusEU);
            cmd.Parameters.AddInputParams(Configuration.SPUpdatePaymentStatusParam1, SqlDbType.VarChar, userId);
            cmd.Parameters.AddInputParams(Configuration.SPUpdatePaymentStatusParam2, SqlDbType.Int, companyId);
            cmd.Parameters.AddInputParams(Configuration.SPUpdatePaymentStatusParam3, SqlDbType.Structured, transactionDetails);
            base.Update(cmd);
        }

        public void UpdateCtsiPaymentStatus(DataTable transactionDetails, string userId, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPUpdateCtsiPaymentStatusNA : Configuration.SPUpdateCtsiPaymentStatusEU);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateCtsiPaymentStatusParam1, SqlDbType.VarChar, userId);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateCtsiPaymentStatusParam2, SqlDbType.Int, companyId);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateCtsiPaymentStatusParam3, SqlDbType.Structured, transactionDetails);
            base.Update(cmd);
        }

        public void UpdateApiPaymentStatus(DataTable transactionDetails, string userId, int companyId, int invType)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPUpdateApiPaymentStatusNA : Configuration.SPUpdateApiPaymentStatusEU);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateApiPaymentStatusParam1, SqlDbType.VarChar, userId);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateApiPaymentStatusParam2, SqlDbType.Int, companyId);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateApiPaymentStatusParam3, SqlDbType.Structured, transactionDetails);
            base.Update(cmd);
        }
        public bool SaveCTSITransactions(DataTable transactionDetails, string userId, int companyId, string fileName, string currencyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPSaveCTSITransactionsNA : Configuration.SPSaveCTSITransactionsEU);
            cmd.Parameters.AddInputParams(Configuration.SPSaveCTSITransactionsParam1, SqlDbType.VarChar, userId);
            cmd.Parameters.AddInputParams(Configuration.SPSaveCTSITransactionsParam2, SqlDbType.Int, companyId);
            cmd.Parameters.AddInputParams(Configuration.SPSaveCTSITransactionsParam3, SqlDbType.VarChar, currencyId);
            cmd.Parameters.AddInputParams(Configuration.SPSaveCTSITransactionsParam4, SqlDbType.VarChar, fileName);
            cmd.Parameters.AddInputParams(Configuration.SPSaveCTSITransactionsParam5, SqlDbType.Structured, transactionDetails);
            cmd.Parameters.AddOutputParams(Configuration.SPSaveCTSITransactionsParam6, SqlDbType.Bit);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            return Convert.ToBoolean(commandResult.Parameters["@" + Configuration.SPSaveCTSITransactionsParam6].Value);
        }

        public void SaveCTSIFileDetailsForReupload(DataTable ctsiDetails, string userId, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPSaveCTSIFileDetailsForReuploadNA : Configuration.SPSaveCTSIFileDetailsForReuploadEU);
            cmd.Parameters.AddInputParams(Configuration.SPSaveCTSIFileDetailsForReuploadParam1, SqlDbType.VarChar, userId);
            cmd.Parameters.AddInputParams(Configuration.SPSaveCTSIFileDetailsForReuploadParam2, SqlDbType.Int, companyId);
            cmd.Parameters.AddInputParams(Configuration.SPSaveCTSIFileDetailsForReuploadParam3, SqlDbType.Structured, ctsiDetails);
            base.Update(cmd);
        }

        public void SaveAPIFileDetailsForReupload(DataTable apiDetails, string userId, int companyId, int invType, DataTable distributionDetails)
        {
            if (invType == 1)
            {
                var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPSaveAPIFileDetailsForReuploadNA : Configuration.SPSaveAPIFileDetailsForReuploadEU);
                cmd.Parameters.AddInputParams(Configuration.SPSaveAPIFileDetailsForReuploadParam1, SqlDbType.VarChar, userId);
                cmd.Parameters.AddInputParams(Configuration.SPSaveAPIFileDetailsForReuploadParam2, SqlDbType.Int, companyId);
                cmd.Parameters.AddInputParams(Configuration.SPSaveAPIFileDetailsForReuploadParam3, SqlDbType.Structured, apiDetails);
                base.Update(cmd);
            }
            else
            {
                var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPSaveAPIFileDetailsForReuploadForPONA : Configuration.SPSaveAPIFileDetailsForReuploadForPOEU);
                cmd.Parameters.AddInputParams(Configuration.SPSaveAPIFileDetailsForReuploadForPOParam1, SqlDbType.VarChar, userId);
                cmd.Parameters.AddInputParams(Configuration.SPSaveAPIFileDetailsForReuploadForPOParam2, SqlDbType.Int, companyId);
                cmd.Parameters.AddInputParams(Configuration.SPSaveAPIFileDetailsForReuploadForPOParam3, SqlDbType.Structured, apiDetails);
                cmd.Parameters.AddInputParams(Configuration.SPSaveAPIFileDetailsForReuploadForPOParam4, SqlDbType.Structured, distributionDetails);
                base.Update(cmd);
            }

        }

        public DataTable GetAPITaxScheduleDetails(DataTable TaxScheduleDetails, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetAPITaxScheduleDetails : Configuration.SPGetAPITaxScheduleDetails);
            cmd.Parameters.AddInputParams(Configuration.SPGetAPITaxScheduleDetailsParam1, SqlDbType.Structured, TaxScheduleDetails);
            DataSet TaxScheduletDT = base.GetDataSet(cmd);
            return TaxScheduletDT.Tables[0];
        }

        public DataSet ValidateAndGetTransactions(DataTable transactionDetails, string userId, int naCompanyId, string formName)
        {
            var cmd = CreateStoredProcCommand(naCompanyId == 1 ? Configuration.SPValidateAndGetTransactions : Configuration.SPValidateAndGetTransactions);
            cmd.Parameters.AddInputParams(Configuration.SPValidateAndGetTransactionsParam1, SqlDbType.VarChar, userId);
            cmd.Parameters.AddInputParams(Configuration.SPValidateAndGetTransactionsParam2, SqlDbType.Int, naCompanyId);
            cmd.Parameters.AddInputParams(Configuration.SPValidateAndGetTransactionsParam3, SqlDbType.VarChar, formName);
            cmd.Parameters.AddInputParams(Configuration.SPValidateAndGetTransactionsParam4, SqlDbType.Structured, transactionDetails);
            return base.GetDataSet(cmd);
        }

        public bool VerifyLookupValueExists(string sourceLookup, string lookupValue, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPVerifyLookupValueExistsNA : Configuration.SPVerifyLookupValueExistsEU);
            cmd.Parameters.AddInputParams(Configuration.SPVerifyLookupValueExistsParam1, SqlDbType.VarChar, sourceLookup);
            cmd.Parameters.AddInputParams(Configuration.SPVerifyLookupValueExistsParam2, SqlDbType.VarChar, lookupValue);
            cmd.Parameters.AddOutputParams(Configuration.SPVerifyLookupValueExistsParam3, SqlDbType.Bit);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            return Convert.ToBoolean(commandResult.Parameters["@" + Configuration.SPVerifyLookupValueExistsParam3].Value);
        }



        //API
        public DataSet GetFailedApiDocuments(int invoiceType, string searchType, string fromApiInvoiceId, string toApiInvoiceId, DateTime fromDocumentDate, DateTime toDocumentDate, string fromVendorId, string toVendorId, int companyId)
        {
            DataSet failedDS = new DataSet();
            if (invoiceType == 1)
            {
                var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetFailedApiDocumentsNA : Configuration.SPGetFailedApiDocumentsEU);
                cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiDocumentsParam1, SqlDbType.VarChar, searchType);
                cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiDocumentsParam2, SqlDbType.VarChar, fromApiInvoiceId);
                cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiDocumentsParam3, SqlDbType.VarChar, toApiInvoiceId);
                cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiDocumentsParam4, SqlDbType.DateTime, fromDocumentDate);
                cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiDocumentsParam5, SqlDbType.DateTime, toDocumentDate);
                cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiDocumentsParam6, SqlDbType.VarChar, fromVendorId);
                cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiDocumentsParam7, SqlDbType.VarChar, toVendorId);
                failedDS = base.GetDataSet(cmd);
            }
            else
            {
                var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetFailedApiDocumentsForPoNA : Configuration.SPGetFailedApiDocumentsForPoEU);
                cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiDocumentsParam1, SqlDbType.VarChar, searchType);
                cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiDocumentsParam2, SqlDbType.VarChar, fromApiInvoiceId);
                cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiDocumentsParam3, SqlDbType.VarChar, toApiInvoiceId);
                cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiDocumentsParam4, SqlDbType.DateTime, fromDocumentDate);
                cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiDocumentsParam5, SqlDbType.DateTime, toDocumentDate);
                cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiDocumentsParam6, SqlDbType.VarChar, fromVendorId);
                cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiDocumentsParam7, SqlDbType.VarChar, toVendorId);
                failedDS = base.GetDataSet(cmd);
            }
            return failedDS;
        }

        public bool VerifyApiLookupValueExists(string sourceLookup, string lookupValue, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPVerifyApiLookupValueExistsNA : Configuration.SPVerifyApiLookupValueExistsEU);
            cmd.Parameters.AddInputParams(Configuration.SPVerifyApiLookupValueExistsParam1, SqlDbType.VarChar, sourceLookup);
            cmd.Parameters.AddInputParams(Configuration.SPVerifyApiLookupValueExistsParam2, SqlDbType.VarChar, lookupValue);
            cmd.Parameters.AddOutputParams(Configuration.SPVerifyApiLookupValueExistsParam3, SqlDbType.Bit);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            return Convert.ToBoolean(commandResult.Parameters["@" + Configuration.SPVerifyApiLookupValueExistsParam3].Value);
        }
        public DataSet ValidateAndGetApiTransactions(int invoiceType, string userId, int companyId, DataTable transactionDetails, string source)
        {
            DataSet validateDS = new DataSet();
            if (invoiceType == 1)
            {
                var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPValidateAndGetApiTransactionsNA : Configuration.SPValidateAndGetApiTransactionsEU);
                cmd.Parameters.AddInputParams(Configuration.SPValidateAndGetApiTransactionsParam1, SqlDbType.VarChar, userId);
                cmd.Parameters.AddInputParams(Configuration.SPValidateAndGetApiTransactionsParam2, SqlDbType.Int, companyId);
                cmd.Parameters.AddInputParams(Configuration.SPValidateAndGetApiTransactionsParam3, SqlDbType.Structured, transactionDetails);
                cmd.Parameters.AddInputParams(Configuration.SPValidateAndGetApiTransactionsParam4, SqlDbType.VarChar, string.IsNullOrEmpty(source.ToString()) ? string.Empty : source.ToString().Trim());
                validateDS = base.GetDataSet(cmd);
            }
            else
            {
                var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPValidatePOAndGetAPITransactionsNA : Configuration.SPValidatePOAndGetAPITransactionsEU);
                cmd.Parameters.AddInputParams(Configuration.SPValidatePOAndGetAPITransactionsParam1, SqlDbType.VarChar, userId);
                cmd.Parameters.AddInputParams(Configuration.SPValidatePOAndGetAPITransactionsParam2, SqlDbType.Int, companyId);
                cmd.Parameters.AddInputParams(Configuration.SPValidatePOAndGetAPITransactionsParam3, SqlDbType.Structured, transactionDetails);
                cmd.Parameters.AddInputParams(Configuration.SPValidatePOAndGetAPITransactionsParam4, SqlDbType.VarChar, string.IsNullOrEmpty(source.ToString()) ? string.Empty : source.ToString().Trim());
                validateDS = base.GetDataSet(cmd);
            }
            return validateDS;
        }
        public DataSet GetApiDuplicateDocumentRowDetails(string userId, int companyId, DataTable transactionDetails)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetApiDuplicateDocumentRowDetailsNA : Configuration.SPGetApiDuplicateDocumentRowDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPGetApiDuplicateDocumentRowDetailsParam1, SqlDbType.VarChar, userId);
            cmd.Parameters.AddInputParams(Configuration.SPGetApiDuplicateDocumentRowDetailsParam2, SqlDbType.Int, companyId);
            cmd.Parameters.AddInputParams(Configuration.SPGetApiDuplicateDocumentRowDetailsParam3, SqlDbType.Structured, transactionDetails);
            return base.GetDataSet(cmd);
        }

        public DataSet GetFailedAPIIdsList(int invoiceType, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetFailedAPIIdsListNA : Configuration.SPGetFailedAPIIdsListEU);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedAPIIdsListParam1, SqlDbType.Int, invoiceType);
            return base.GetDataSet(cmd);
        }
        public Int16 UpdateManualPaymentNumberForAPIDocuments(PayableManagementRequest requestObj)
        {
            var cmd = CreateStoredProcCommand(requestObj.companyId == 1 ? Configuration.SPUpdateManualPaymentNumberForAPIDocumentsNA : Configuration.SPUpdateManualPaymentNumberForAPIDocumentsEU);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateManualPaymentNumberForAPIDocumentsParam1, SqlDbType.VarChar, requestObj.userId);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateManualPaymentNumberForAPIDocumentsParam2, SqlDbType.Int, requestObj.companyId);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateManualPaymentNumberForAPIDocumentsParam3, SqlDbType.Structured, this.ConvertApiManualPaymentListtoTable(requestObj));
            cmd.Parameters.AddOutputParams(Configuration.SPUpdateManualPaymentNumberForAPIDocumentsParam4, SqlDbType.Int);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            return Convert.ToInt16(commandResult.Parameters["@" + Configuration.SPUpdateManualPaymentNumberForAPIDocumentsParam4].Value);

        }
        private DataTable ConvertApiManualPaymentListtoTable(PayableManagementRequest requestObj)
        {
            DataTable lineDetailsTable = null;
            // Create a DataTable with the modified rows.
            lineDetailsTable = new DataTable("@LineDetails");
            lineDetailsTable.Columns.Add("DocumentRowID", typeof(string));
            lineDetailsTable.Columns.Add("DocumentDate", typeof(DateTime));
            lineDetailsTable.Columns.Add("VendorId", typeof(string));
            lineDetailsTable.Columns.Add("DocumentAmount", typeof(decimal));
            lineDetailsTable.Columns.Add("GPVoucherNumber", typeof(string));

            foreach (var data in requestObj.PayableDetailsEntity.GetInvoiceLineDetails)
            {
                var row = lineDetailsTable.NewRow();
                row["DocumentRowID"] = data.DocumentRowId;
                row["DocumentDate"] = data.DocumentDate;
                row["VendorId"] = data.VendorId;
                row["DocumentAmount"] = data.DocumentAmount;
                row["GPVoucherNumber"] = data.DocumentNumber;
                lineDetailsTable.Rows.Add(row);
            }
            return lineDetailsTable;
        }

        public DataSet GetFailedApiIdsToLinkManualPayments(int invoiceType, string searchType, string fromApiId, string toApiId, string fromVendorId, string toVendorId, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetFailedApiIdsToLinkManualPaymentsNA : Configuration.SPGetFailedApiIdsToLinkManualPaymentsEU);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiIdsToLinkManualPaymentsParam1, SqlDbType.Int, invoiceType);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiIdsToLinkManualPaymentsParam2, SqlDbType.VarChar, searchType);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiIdsToLinkManualPaymentsParam3, SqlDbType.VarChar, fromApiId);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiIdsToLinkManualPaymentsParam4, SqlDbType.VarChar, toApiId);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiIdsToLinkManualPaymentsParam5, SqlDbType.VarChar, fromVendorId);
            cmd.Parameters.AddInputParams(Configuration.SPGetFailedApiIdsToLinkManualPaymentsParam6, SqlDbType.VarChar, toVendorId);
            return base.GetDataSet(cmd);
        }
        public bool ValidatePODocumentExistsForVendor(string invoiceNumber, string vendorId, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPValidatePODocumentExistsForVendorNA : Configuration.SPValidatePODocumentExistsForVendorEU);
            cmd.Parameters.AddInputParams(Configuration.SPValidatePODocumentExistsForVendorParam1, SqlDbType.VarChar, invoiceNumber);
            cmd.Parameters.AddInputParams(Configuration.SPValidatePODocumentExistsForVendorParam2, SqlDbType.VarChar, vendorId);
            cmd.Parameters.AddOutputParams(Configuration.SPValidatePODocumentExistsForVendorParam3, SqlDbType.Bit);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            return Convert.ToBoolean(commandResult.Parameters["@" + Configuration.SPValidatePODocumentExistsForVendorParam3].Value);
        }
        public bool ValidateNonPODocumentExistsForVendor(string paymentNumber, string vendorId, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPValidateNonPODocumentExistsForVendorNA : Configuration.SPValidateNonPODocumentExistsForVendorEU);
            cmd.Parameters.AddInputParams(Configuration.SPValidateNonPODocumentExistsForVendorParam1, SqlDbType.VarChar, paymentNumber);
            cmd.Parameters.AddInputParams(Configuration.SPValidateNonPODocumentExistsForVendorParam2, SqlDbType.VarChar, vendorId);
            cmd.Parameters.AddOutputParams(Configuration.SPValidateNonPODocumentExistsForVendorParam3, SqlDbType.Bit);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            return Convert.ToBoolean(commandResult.Parameters["@" + Configuration.SPValidateNonPODocumentExistsForVendorParam3].Value);
        }

        private DataTable ConvertCtsiManualPaymentListtoTable(PayableManagementRequest requsetObj)
        {
            DataTable lineDetailsTable = null;
            // Create a DataTable with the modified rows.
            lineDetailsTable = new DataTable("@LineDetails");
            lineDetailsTable.Columns.Add("CtsiId", typeof(string));
            lineDetailsTable.Columns.Add("DocumentDate", typeof(DateTime));
            lineDetailsTable.Columns.Add("VendorId", typeof(string));
            lineDetailsTable.Columns.Add("TotalApprovedDocumentAmount", typeof(decimal));
            lineDetailsTable.Columns.Add("GPVocuherNumber", typeof(string));

            foreach (var data in requsetObj.PayableDetailsEntity.GetInvoiceLineDetails)
            {
                var row = lineDetailsTable.NewRow();
                row["CtsiId"] = data.CtsiId;
                row["DocumentDate"] = data.DocumentDate;
                row["VendorId"] = data.VendorId;
                row["TotalApprovedDocumentAmount"] = data.TotalApprovedDocumentAmount;
                row["GPVocuherNumber"] = data.VoucherNumber;
                lineDetailsTable.Rows.Add(row);
            }
            return lineDetailsTable;
        }


        #endregion

        #region APIFileGenerator

        public DataSet GetAPIFiles(string CutofMonth, int companyId)
        {
            DataSet apiFilesDS = new DataSet();

            if (CutofMonth == Configuration.ApiFileDaily)
            {
                var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetAPIFilesDailyNA : Configuration.SPGetAPIFilesDailyEU);
                apiFilesDS = base.GetDataSet(cmd);
            }
            else if (CutofMonth == Configuration.ApiFileQuarterly)
            {
                var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetAPIFilesQuaterlyNA : Configuration.SPGetAPIFilesQuaterlyEU);
                apiFilesDS = base.GetDataSet(cmd);
            }
            return apiFilesDS;
        }

        public DataSet GetVenGLDetails(int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetVenGLDetailsNA : Configuration.SPGetVenGLDetailsEU);
            return base.GetDataSet(cmd);
        }

        #endregion

        #region CTSIFileUploader

        public DataSet FetchAllCtsiTransactionDetails(string companyName, DateTime processingDate, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPFetchAllCtsiTransactionDetailsNA : Configuration.SPFetchAllCtsiTransactionDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPFetchAllCtsiTransactionDetailsParam1, SqlDbType.DateTime, processingDate.ToShortDateString());
            return base.GetDataSet(cmd);
        }

        public void SaveAllUploadedDetails(DataSet transactionDetails, string userId, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPSaveAllUploadedDetailsNA : Configuration.SPSaveAllUploadedDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPSaveAllUploadedDetailsParam1, SqlDbType.Structured, transactionDetails.Tables[0]);
            cmd.Parameters.AddInputParams(Configuration.SPSaveAllUploadedDetailsParam2, SqlDbType.Structured, transactionDetails.Tables[1]);
            cmd.Parameters.AddInputParams(Configuration.SPSaveAllUploadedDetailsParam3, SqlDbType.Structured, transactionDetails.Tables[2]);
            if (companyId == 1)
            {
                cmd.Parameters.AddInputParams(Configuration.SPSaveAllUploadedDetailsParam4, SqlDbType.Structured, transactionDetails.Tables[3]);
                cmd.Parameters.AddInputParams(Configuration.SPSaveAllUploadedDetailsParam5, SqlDbType.Structured, transactionDetails.Tables[4]);
            }
            cmd.Parameters.AddInputParams(Configuration.SPSaveAllUploadedDetailsParam6, SqlDbType.VarChar, userId);
            base.Update(cmd);
        }

        #endregion
    }
}
