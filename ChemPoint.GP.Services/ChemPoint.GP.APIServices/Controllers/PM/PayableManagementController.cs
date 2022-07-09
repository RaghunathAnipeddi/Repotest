using Chempoint.GP.Infrastructure.Exceptions;
using Chempoint.GP.Infrastructure.Extensions;
using Chempoint.GP.Model.Interactions.Email;
using Chempoint.GP.Model.Interactions.PayableManagement;
using ChemPoint.GP.ApiServices.Controllers.Sales;
using ChemPoint.GP.APIServices.Utils;
using ChemPoint.GP.BusinessContracts.PM;
using ChemPoint.GP.Entities.BaseEntities;
using ChemPoint.GP.PMBL;
using System;
using System.Configuration;
using System.Web.Http;


namespace ChemPoint.GP.APIServices.Controllers.PM
{
    /// <PayableManagement>
    /// Project Name        :   payable mangement migration
    /// Affected Module     :   Purchase
    /// Affected Windows    :   Controller For API to GP, GP to API,CTSI To GP and GP to CTSI.
    /// Developed on        :   April2017
    /// Developed by        :   Muthu
    ///-------------------------------------------------------------------------------------------
    ///     Date		Author          Description
    ///-------------------------------------------------------------------------------------------
    /// **Apr2017       Mthangaraj      Initial Creation.
    /// </PayableManagement>
    public class PayableManagementController : BaseApiController
    {
        private IPayableManagementBusiness payableService;

        public PayableManagementController()
        {
            payableService = new PMBusiness();
        }

        #region PayableManagement
        [HttpGet]
        [HttpPost]
        /// <summary>
        /// API to GP job
        /// process the flat file data
        /// </summary>
        /// <param name="requestObj">Request contains file name withpath etc.</param>
        /// <returns>Reponse object contains logging information</returns>
        public IHttpActionResult ProcessApOutFile(PayableManagementRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("PayableManagementRequest details is null"));
            SendEmailRequest sendEmail = new SendEmailRequest();

            EMailInformation emailInfo = new EMailInformation();

            emailInfo.EmailFrom = ConfigurationManager.AppSettings["PMToGPFrom"].ToString();

            emailInfo.EmailTo = ConfigurationManager.AppSettings["PMToGPTo"].ToString();

            emailInfo.EmailCc = ConfigurationManager.AppSettings["PMToGPCC"].ToString();

            emailInfo.EmailBcc = ConfigurationManager.AppSettings["PMToGPBCC"].ToString();

            emailInfo.SmtpAddress = ConfigurationManager.AppSettings["PMToGPSMTP"].ToString();

            emailInfo.Subject = ConfigurationManager.AppSettings["ApiToGPMailSubject"].ToString();

            emailInfo.Body = ConfigurationManager.AppSettings["APIToGPFailureMailContent"].ToString();

            sendEmail.Source = ConfigurationManager.AppSettings["SourceApi"].ToString();

            sendEmail.EmailInformation = emailInfo;

            request.EmailRequest = sendEmail;

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            if (request.companyId == 1)
                request.EconnectConString = ConfigurationManager.ConnectionStrings["CHMPTEconnectConString"].ConnectionString;
            else
                request.EconnectConString = ConfigurationManager.ConnectionStrings["CPEUREconnectConString"].ConnectionString;

            request.PMInputHeader = ConfigurationManager.AppSettings["PMInputHeader"].ToString();

            request.POInputHeader = ConfigurationManager.AppSettings["POInputHeader"].ToString();

            request.CPEURFileNameForPO = ConfigurationManager.AppSettings["CPEURFileNameForPO"].ToString();

            request.CHMPTFileNameForPO = ConfigurationManager.AppSettings["CHMPTFileNameForPO"].ToString();

            request.CHMPTFileNameForNonPO = ConfigurationManager.AppSettings["CHMPTFileNameForNonPO"].ToString();

            request.CPEURFileNameForNonPO = ConfigurationManager.AppSettings["CPEURFileNameForNonPO"].ToString();

            request.ExtractedFilesArchiveFolder = ConfigurationManager.AppSettings["ExtractedFilesArchiveFolder"].ToString();

            request.CPEURFileName = ConfigurationManager.AppSettings["CPEURFileName"].ToString();

            request.GBPFileName = ConfigurationManager.AppSettings["GBPFileName"].ToString();

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.ProcessApOutFile(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        /// <summary>
        /// ProcessCtsiFile the data
        /// </summary>
        /// <param name="requestObj"></param>
        /// <returns></returns>
        public IHttpActionResult ProcessCtsiFile(PayableManagementRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ProcessCtsiFile Request is null"));

            SendEmailRequest sendEmail = new SendEmailRequest();

            EMailInformation emailInfo = new EMailInformation();

            emailInfo.EmailFrom = ConfigurationManager.AppSettings["PMToGPFrom"].ToString();

            emailInfo.EmailTo = ConfigurationManager.AppSettings["PMToGPTo"].ToString();

            emailInfo.EmailCc = ConfigurationManager.AppSettings["PMToGPCC"].ToString();

            emailInfo.EmailBcc = ConfigurationManager.AppSettings["PMToGPBCC"].ToString();

            emailInfo.SmtpAddress = ConfigurationManager.AppSettings["PMToGPSMTP"].ToString();

            emailInfo.Subject = ConfigurationManager.AppSettings["CtsiToGPMailSubject"].ToString();

            emailInfo.Body = ConfigurationManager.AppSettings["CTSIToGPFailureMailContent"].ToString();

            sendEmail.Source = ConfigurationManager.AppSettings["SourceCtsi"].ToString();

            sendEmail.EmailInformation = emailInfo;

            request.EmailRequest = sendEmail;

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            if (request.companyId == 1)
                request.EconnectConString = ConfigurationManager.ConnectionStrings["CHMPTEconnectConString"].ConnectionString;
            else
                request.EconnectConString = ConfigurationManager.ConnectionStrings["CPEUREconnectConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.ProcessCtsiFile(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }


        #endregion PayableManagement

        #region PayableService

        #region PayableMgtService

        [HttpGet]
        [HttpPost]
        public IHttpActionResult UploadPayableDetailsIntoGpForCtsi(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            if (payableRequest.companyId == 1)
                payableRequest.EconnectConString = ConfigurationManager.ConnectionStrings["CHMPTEconnectConString"].ConnectionString;
            else
                payableRequest.EconnectConString = ConfigurationManager.ConnectionStrings["CPEUREconnectConString"].ConnectionString;

            SendEmailRequest sendEmail = new SendEmailRequest();

            EMailInformation emailInfo = new EMailInformation();

            emailInfo.EmailFrom = ConfigurationManager.AppSettings["PMToGPFrom"].ToString();

            emailInfo.EmailTo = ConfigurationManager.AppSettings["PMToGPTo"].ToString();

            emailInfo.EmailCc = ConfigurationManager.AppSettings["PMToGPCC"].ToString();

            emailInfo.EmailBcc = ConfigurationManager.AppSettings["PMToGPBCC"].ToString();

            emailInfo.SmtpAddress = ConfigurationManager.AppSettings["PMToGPSMTP"].ToString();

            emailInfo.Subject = ConfigurationManager.AppSettings["CtsiToGPMailSubject"].ToString();

            emailInfo.Body = ConfigurationManager.AppSettings["CTSIToGPFailureMailContent"].ToString();

            sendEmail.Source = ConfigurationManager.AppSettings["SourceCtsi"].ToString();

            sendEmail.EmailInformation = emailInfo;

            payableRequest.EmailRequest = sendEmail;


            return DoExecute<SalesException>(() =>
            {
                var result = payableService.UploadPayableDetailsIntoGpForCtsi(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult UploadPayableDetailsIntoGpForApi(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            if (payableRequest.companyId == 1)
                payableRequest.EconnectConString = ConfigurationManager.ConnectionStrings["CHMPTEconnectConString"].ConnectionString;
            else
                payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["CPEUREconnectConString"].ConnectionString;

            SendEmailRequest sendEmail = new SendEmailRequest();

            EMailInformation emailInfo = new EMailInformation();

            emailInfo.EmailFrom = ConfigurationManager.AppSettings["PMToGPFrom"].ToString();

            emailInfo.EmailTo = ConfigurationManager.AppSettings["PMToGPTo"].ToString();

            emailInfo.EmailCc = ConfigurationManager.AppSettings["PMToGPCC"].ToString();

            emailInfo.EmailBcc = ConfigurationManager.AppSettings["PMToGPBCC"].ToString();

            emailInfo.SmtpAddress = ConfigurationManager.AppSettings["PMToGPSMTP"].ToString();

            emailInfo.Subject = ConfigurationManager.AppSettings["ApiToGPMailSubject"].ToString();

            emailInfo.Body = ConfigurationManager.AppSettings["APIToGPFailureMailContent"].ToString();

            sendEmail.Source = ConfigurationManager.AppSettings["SourceApi"].ToString();

            sendEmail.EmailInformation = emailInfo;

            payableRequest.EmailRequest = sendEmail;

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.UploadPayableDetailsIntoGpForApi(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion



        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetFailedCTSIIdsList(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.GetFailedCTSIIdsList(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }



        [HttpGet]
        [HttpPost]
        public IHttpActionResult ValidateVoucherExistsForVendor(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.ValidateVoucherExistsForVendor(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }


        [HttpGet]
        [HttpPost]
        public IHttpActionResult VerifyLookupValueExists(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.VerifyLookupValueExists(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        //CTSI
        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetFailedCtsiDocuments(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.GetFailedCtsiDocuments(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ValidateAndGetCtsiTransactions(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.ValidateAndGetCtsiTransactions(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        //API
        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetFailedApiDocuments(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.GetFailedApiDocuments(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult VerifyApiLookupValueExists(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.VerifyApiLookupValueExists(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ValidateAndGetApiTransactions(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.ValidateAndGetApiTransactions(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetFailedAPIIdsList(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.GetFailedAPIIdsList(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult UpdateManualPaymentNumberForAPIDocuments(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.UpdateManualPaymentNumberForAPIDocuments(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetFailedApiIdsToLinkManualPayments(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.GetFailedApiIdsToLinkManualPayments(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ValidatePODocumentExistsForVendor(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.ValidatePODocumentExistsForVendor(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ValidateNonPODocumentExistsForVendor(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.ValidateNonPODocumentExistsForVendor(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult UpdateManualPaymentNumberForCTSIDocuments(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.UpdateManualPaymentNumberForCTSIDocuments(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetFailedCtsiIdsToLinkManualPayments(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.GetFailedCtsiIdsToLinkManualPayments(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult VerifyCtsiLookupValueExists(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.VerifyCtsiLookupValueExists(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetApiDuplicateDocumentRowDetails(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("PayableMgt details Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = payableService.GetApiDuplicateDocumentRowDetails(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion

        #region APIFIleGenerator

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetAPIFiles(PayableManagementRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("PayableManagementRequest details is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            return DoExecute<SalesException>(() =>
            {
                var result = payableService.GetAPIFiles(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetVenGLDetails(PayableManagementRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("PayableManagementRequest details is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            return DoExecute<SalesException>(() =>
            {
                var result = payableService.GetVenGLDetails(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion

        #region CTSI File Generator
        [HttpGet]
        [HttpPost]
        public IHttpActionResult GenerateCtsiFilesToBeUploaded(PayableManagementRequest payableRequest)
        {
            if (payableRequest.IsInValid())
                return InternalServerError(new Exception("CTSI File Uploader Request is null"));

            payableRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            payableRequest.TempFolderPath = ConfigurationManager.AppSettings["TempFolderPath"].ToString().Trim();

            payableRequest.InboundHeader = ConfigurationManager.AppSettings["InboundHeader"].ToString().Trim();

            payableRequest.OutboundHeader = ConfigurationManager.AppSettings["OutboundHeader"].ToString().Trim();

            payableRequest.TransferHeader = ConfigurationManager.AppSettings["TransferHeader"].ToString().Trim();

            payableRequest.SupplierReturnHeader = ConfigurationManager.AppSettings["SupplierReturnHeader"].ToString().Trim();

            payableRequest.CustomerReturnHeader = ConfigurationManager.AppSettings["CustomerReturnHeader"].ToString().Trim();

            payableRequest.NaInboundFileName = ConfigurationManager.AppSettings["NaInboundFileName"].ToString().Trim();

            payableRequest.NaOutboundFileName = ConfigurationManager.AppSettings["NaOutboundFileName"].ToString().Trim();

            payableRequest.NaTransfersFileName = ConfigurationManager.AppSettings["NaTransfersFileName"].ToString().Trim();

            payableRequest.NaSupplierReturnFileName = ConfigurationManager.AppSettings["NaSupplierReturnFileName"].ToString().Trim();

            payableRequest.NaCustomerReturnFileName = ConfigurationManager.AppSettings["NaCustomerReturnFileName"].ToString().Trim();

            payableRequest.EuInboundFileName = ConfigurationManager.AppSettings["EuInboundFileName"].ToString().Trim();

            payableRequest.EuOutboundFileName = ConfigurationManager.AppSettings["EuOutboundFileName"].ToString().Trim();

            payableRequest.EuCustomerReturnFileName = ConfigurationManager.AppSettings["EuCustomerReturnFileName"].ToString().Trim();

            payableRequest.OutputNaFileName = ConfigurationManager.AppSettings["OutputNaFileName"].ToString().Trim();

            payableRequest.OutputEuFileName = ConfigurationManager.AppSettings["OutputEuFileName"].ToString().Trim();

            payableRequest.DownloadForNAandEU = ConfigurationManager.AppSettings["DownloadForNAandEU"].ToString().Trim();

            payableRequest.FileNameStarts = ConfigurationManager.AppSettings["FileNameStarts"].ToString().Trim();

            payableRequest.FTPOutFolderPath = ConfigurationManager.AppSettings["FTPOutFolderPath"].ToString().Trim();

            payableRequest.FTPInFolderPath = ConfigurationManager.AppSettings["FTPInFolderPath"].ToString().Trim();

            payableRequest.ZipExtractedFilePath = ConfigurationManager.AppSettings["ZipExtractedFilePath"].ToString().Trim();

            payableRequest.FTPUserName = ConfigurationManager.AppSettings["FTPUserName"].ToString().Trim();

            payableRequest.FTPPath = ConfigurationManager.AppSettings["FTPPath"].ToString().Trim();

            payableRequest.FTPPassword = ConfigurationManager.AppSettings["FTPPassword"].ToString().Trim();
            // Mail
            payableRequest.GPToCtsiMailFrom = ConfigurationManager.AppSettings["GPToCtsiMailFrom"].ToString().Trim();

            payableRequest.GPToCtsiMailCC = ConfigurationManager.AppSettings["GPToCtsiMailCC"].ToString().Trim();

            payableRequest.GPToCtsiMailTo = ConfigurationManager.AppSettings["GPToCtsiMailTo"].ToString().Trim();

            payableRequest.GPToCtsiMailBcc = ConfigurationManager.AppSettings["GPToCtsiMailBcc"].ToString().Trim();

            payableRequest.GPToCtsiMailSubject = ConfigurationManager.AppSettings["GPToCtsiMailSubject"].ToString().Trim();

            payableRequest.CTSISignature = ConfigurationManager.AppSettings["CTSISignature"].ToString().Trim();

            payableRequest.OutputFilePath = ConfigurationManager.AppSettings["OutputFilePath"].ToString().Trim();

            payableRequest.CTSIUserId = ConfigurationManager.AppSettings["CTSIUserId"].ToString().Trim();
            return DoExecute<SalesException>(() =>
            {
                var result = payableService.GenerateCtsiFilesToBeUploaded(payableRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion CTSI File Generator
    }
}