using Chempoint.GP.Model.Interactions.PayableManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.BusinessContracts.PM
{
    public interface IPayableManagementBusiness
    {
        #region PayableManagement
        PayableManagementResponse ProcessApOutFile(PayableManagementRequest poActivityRequest);
        PayableManagementResponse ProcessCtsiFile(PayableManagementRequest poActivityRequest);
        #endregion PayableManamgement

        #region PaybleService

        PayableManagementResponse UploadPayableDetailsIntoGpForCtsi(PayableManagementRequest request);
        PayableManagementResponse UploadPayableDetailsIntoGpForApi(PayableManagementRequest request);
        PayableManagementResponse GetFailedCTSIIdsList(PayableManagementRequest request);
        PayableManagementResponse ValidateVoucherExistsForVendor(PayableManagementRequest request);
        PayableManagementResponse VerifyLookupValueExists(PayableManagementRequest request);

        //CTSI
        PayableManagementResponse GetFailedCtsiDocuments(PayableManagementRequest requestObj);
        PayableManagementResponse ValidateAndGetCtsiTransactions(PayableManagementRequest requestObj);

        //API
        PayableManagementResponse GetFailedApiDocuments(PayableManagementRequest requestObj);
        PayableManagementResponse VerifyApiLookupValueExists(PayableManagementRequest requestObj);
        PayableManagementResponse ValidateAndGetApiTransactions(PayableManagementRequest requestObj);
        PayableManagementResponse GetApiDuplicateDocumentRowDetails(PayableManagementRequest requestObj);

        PayableManagementResponse GetFailedAPIIdsList(PayableManagementRequest requestObj);
        PayableManagementResponse UpdateManualPaymentNumberForAPIDocuments(PayableManagementRequest requestObj);
        PayableManagementResponse GetFailedApiIdsToLinkManualPayments(PayableManagementRequest requestObj);
        PayableManagementResponse ValidatePODocumentExistsForVendor(PayableManagementRequest requestObj);
        PayableManagementResponse ValidateNonPODocumentExistsForVendor(PayableManagementRequest requestObj);
        PayableManagementResponse UpdateManualPaymentNumberForCTSIDocuments(PayableManagementRequest requestObj);
        PayableManagementResponse GetFailedCtsiIdsToLinkManualPayments(PayableManagementRequest requestObj);
        PayableManagementResponse VerifyCtsiLookupValueExists(PayableManagementRequest requestObj);

        #endregion

        #region APIFileGenerator

        PayableManagementResponse GetAPIFiles(PayableManagementRequest requestObj);
        PayableManagementResponse GetVenGLDetails(PayableManagementRequest requestObj);

        #endregion

        #region CTSIFileUploader
        PayableManagementResponse GenerateCtsiFilesToBeUploaded(PayableManagementRequest ctsiFileUploaderRequest);

        #endregion
    }
}
