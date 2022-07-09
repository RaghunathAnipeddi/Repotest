using Chempoint.GP.Model.Interactions.PayableManagement;
using ChemPoint.GP.DataContracts.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.DataContracts.PM
{
    public interface IPayableManagementRepository : IRepository
    {
        #region PayableManagement
        DataSet SaveAPIDetailsToDB(PayableManagementRequest requestObj, DataSet poInv, DataSet nonPoInv);
        DataSet ValidateAPIDetailsToDB(DataSet savedApiDetails, int companyId);
        //DataSet ValidateAndGetCtsiTransactions(string userId, int companyId, DataTable fileDetailsTable);
        #endregion PayableManagement

        #region PayableService

        //bool SaveExpenseTransactions(DataTable transactionDetails, string userId, int companyId);
        //void SaveExpenseFileDetailsForReupload(DataTable expenseDetails, string userId, int companyId);
        DataSet FetchAccountDetails(int actIndex, string vendorID, int companyId);
        DataTable GetNonPODistributionValues(DataTable paymentTable, string payAcntNumb, string payAcntDesc, int companyId, out double ten99Amnt);
        string GetNextInvoiceNumberForApi(int companyId, int invType);
        string GetNextInvoiceNumber(int companyId);
        void UpdatePaymentStatus(DataTable transactionDetails, string userId, int companyId);
        void UpdateCtsiPaymentStatus(DataTable transactionDetails, string userId, int companyId);
        void UpdateApiPaymentStatus(DataTable transactionDetails, string userId, int companyId, int invType);

        bool SaveCTSITransactions(DataTable transactionDetails, string userId, int companyId, string fileName, string currencyId);
        void SaveCTSIFileDetailsForReupload(DataTable ctsiDetails, string userId, int companyId);
        void SaveAPIFileDetailsForReupload(DataTable apiDetails, string userId, int companyId, int invType, DataTable distributionDetails);
        DataTable GetAPITaxScheduleDetails(DataTable TaxScheduleDetails, int companyId);



        DataSet GetFailedCTSIIdsList(int companyId);
        DataSet ValidateAndGetTransactions(DataTable transactionDetails, string userId, int naCompanyId, string formName);

        bool ValidateVoucherExistsForVendor(string paymentNumber, string vendorId, int companyId);

        bool VerifyLookupValueExists(string sourceLookup, string lookupValue, int companyId);

        //CTSI
        DataSet GetFailedCtsiDocuments(string searchType, string fromCtsiId, string toCtsiId, DateTime fromDocumentDate, DateTime toDocumentDate, string fromVendorId, string toVendorId, int companyId);
        DataSet ValidateAndGetCtsiTransactions(string userId, int companyId, DataTable transactionDetails);

        //API
        DataSet GetFailedApiDocuments(int invoiceType, string searchType, string fromApiInvoiceId, string toApiInvoiceId, DateTime fromDocumentDate, DateTime toDocumentDate, string fromVendorId, string toVendorId, int companyId);
        bool VerifyApiLookupValueExists(string sourceLookup, string lookupValue, int companyId);
        DataSet ValidateAndGetApiTransactions(int invoiceType, string userId, int companyId, DataTable transactionDetails, string source);
        DataSet GetApiDuplicateDocumentRowDetails(string userId, int companyId, DataTable transactionDetails);

        DataSet GetFailedAPIIdsList(int invoiceType, int companyId);
        Int16 UpdateManualPaymentNumberForAPIDocuments(PayableManagementRequest requestObj);
        DataSet GetFailedApiIdsToLinkManualPayments(int invoiceType, string searchType, string fromApiId, string toApiId, string fromVendorId, string toVendorId, int companyId);
        bool ValidatePODocumentExistsForVendor(string invoiceNumber, string vendorId, int companyId);
        bool ValidateNonPODocumentExistsForVendor(string paymentNumber, string vendorId, int companyId);
        Int16 UpdateManualPaymentNumberForCTSIDocuments(PayableManagementRequest requestObj);
        DataSet GetFailedCtsiIdsToLinkManualPayments(string searchType, string fromCtsiId, string toCtsiId, DateTime fromDocumentDate, DateTime toDocumentDate, string fromVendorId, string toVendorId, int companyId);
        bool VerifyCtsiLookupValueExists(string sourceLookup, string lookupValue, int companyId);

        #endregion

        #region APIFileGenerator

        DataSet GetAPIFiles(string CutofMonth, int companyId);
        DataSet GetVenGLDetails(int companyId);

        #endregion

        #region CTSIFileGenerator

        DataSet FetchAllCtsiTransactionDetails(string companyName, DateTime processingDate, int companyId);
        void SaveAllUploadedDetails(DataSet transactionDetails, string userId, int companyId);

        #endregion
    }
}
