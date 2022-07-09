using System;
using System.Web.Http;
using Chempoint.GP.Infrastructure.Exceptions;
using Chempoint.GP.Infrastructure.Extensions;
using ChemPoint.GP.APIServices.Utils;
using ChemPoint.GP.BusinessContracts.Sales;
using System.Configuration;
using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.SalesOrderBL;
using Chempoint.GP.ReceivablesBusiness;
using System.IO;

namespace ChemPoint.GP.ApiServices.Controllers.Sales
{
    public class ReceivablesManagementController : BaseApiController
    {
        private IReceivablesBusiness receivablesService = null;

        public ReceivablesManagementController()
        {
            receivablesService = new ReceivablesBusiness();
        }
        
        #region EFT Payment

        /// <summary>
        /// Get SalesOrder Entity service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult ImportEFTBankSummaryReport(ReceivablesRequest request)
        {
            request.LogMessage += DateTime.Now.ToString() + " - ImportEFTBankSummaryReport Service method in ChemPoint.GP.ApiServices.Controllers.Sales.ReceivablesManagementController is stared";

            if (request.IsInValid())
            {
                request.LogMessage += DateTime.Now.ToString() + " - Request is valid";
                return InternalServerError(new Exception("ReceivablesRequest Request is null"));
            }

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            request.LogMessage += DateTime.Now.ToString() + " - ConnectionString: " + request.ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = receivablesService.ImportEFTBankSummaryReport(request);

                if (result.IsValid())
                {
                    result.LogMessage += DateTime.Now.ToString() + " - ImportEFTBankSummaryReport Service method in ChemPoint.GP.ApiServices.Controllers.Sales.ReceivablesManagementController is completed";
                    return new Jsonizer(result, Request);
                }
                else
                    return NotFound();
            });
        }

        [HttpPost]
        public IHttpActionResult ImportEFTRemittanceReport(ReceivablesRequest request)
        {

            request.LogMessage += DateTime.Now.ToString() + " - ImportEFTRemittanceReport Service method in ChemPoint.GP.ApiServices.Controllers.Sales.ReceivablesManagementController is stared";

            if (request.IsInValid())
            {
                request.LogMessage += DateTime.Now.ToString() + " - Request is valid";
                return InternalServerError(new Exception("ReceivablesRequest Request is null"));
            };

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            request.LogMessage += DateTime.Now.ToString() + " - ConnectionString: " + request.ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = receivablesService.ImportEFTRemittanceReport(request);

                if (result.IsValid())
                {
                    result.LogMessage += DateTime.Now.ToString() + " - ImportEFTRemittanceReport Service method in ChemPoint.GP.ApiServices.Controllers.Sales.ReceivablesManagementController is completed";
                    return new Jsonizer(result, Request);
                }
                else
                    return NotFound();
            });
        }


        [HttpPost]
        public IHttpActionResult ValidateEFTLine(ReceivablesRequest request)
        {

            if (request.IsInValid())
                return InternalServerError(new Exception("ReceivablesRequest Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = receivablesService.ValidateEFTLine(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion EFT Payment

        #region Remittance

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ValidatePaymentRemittance(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Customer Id Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = receivablesService.ValidatePaymentRemittance(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        /// <summary>
        /// Get SalesOrder Entity service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult PushEftTransactionsToGP(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ReceivablesRequest Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            request.NAEconnectConnectionString = ConfigurationManager.ConnectionStrings["CHMPTEconnectConString"].ConnectionString;

            request.EUEconnectConnectionString = ConfigurationManager.ConnectionStrings["CPEUREconnectConString"].ConnectionString;

            request.EFTPaymentStyleSheetPath = ConfigurationManager.AppSettings["EFTPaymentStyleSheetPath"].ToString();

            request.EFTApplyStyleSheetPath = ConfigurationManager.AppSettings["EFTApplyStyleSheetPath"].ToString();

            request.EFTPaymentAndApplyStyleSheetPath = ConfigurationManager.AppSettings["EFTPaymentAndApplyStyleSheetPath"].ToString();

            request.LoggingPath = ConfigurationManager.AppSettings["EFTAutomationloggingPath"].ToString();
            request.LoggingFileName = ConfigurationManager.AppSettings["EFTAutomationloggingFileName"].ToString();

            request.SalesOrderFailureEmail = new ChemPoint.GP.Entities.BaseEntities.EMailInformation();
            request.SalesOrderFailureEmail.EmailFrom = ConfigurationManager.AppSettings["CashProcessFailureEmailFrom"].ToString();
            request.SalesOrderFailureEmail.EmailTo = ConfigurationManager.AppSettings["CashProcessFailureEmailTo"].ToString();
            request.SalesOrderFailureEmail.EmailCc = ConfigurationManager.AppSettings["CashProcessFailureEmailCC"].ToString();
            request.SalesOrderFailureEmail.EmailBcc = ConfigurationManager.AppSettings["CashProcessFailureEmailBcc"].ToString();
            request.SalesOrderFailureEmail.Subject = ConfigurationManager.AppSettings["CashProcessFailureEmailSubject"].ToString();
            request.SalesOrderFailureEmail.Body = "";
            request.SalesOrderFailureEmail.SmtpAddress = ConfigurationManager.AppSettings["SmtpAddress"].ToString();
            request.SalesOrderFailureEmail.UserId = ConfigurationManager.AppSettings["EmailUserId"].ToString();
            request.SalesOrderFailureEmail.Password = ConfigurationManager.AppSettings["EmailPassword"].ToString();

            request.SalesPriorityOrdersEmail = new ChemPoint.GP.Entities.BaseEntities.EMailInformation();
            request.SalesPriorityOrdersEmail.EmailFrom = ConfigurationManager.AppSettings["CashProcessCurrencyEmailFrom"].ToString();
            request.SalesPriorityOrdersEmail.EmailTo = ConfigurationManager.AppSettings["CashProcessCurrencyEmailTo"].ToString();
            request.SalesPriorityOrdersEmail.EmailCc = ConfigurationManager.AppSettings["CashProcessCurrencyEmailCC"].ToString();
            request.SalesPriorityOrdersEmail.EmailBcc = ConfigurationManager.AppSettings["CashProcessCurrencyEmailBcc"].ToString();
            request.SalesPriorityOrdersEmail.Subject = ConfigurationManager.AppSettings["CashProcessCurrencyEmailSubject"].ToString().Trim();
            request.SalesPriorityOrdersEmail.Body = "";
            request.SalesPriorityOrdersEmail.SmtpAddress = ConfigurationManager.AppSettings["SmtpAddress"].ToString();
            request.SalesPriorityOrdersEmail.UserId = ConfigurationManager.AppSettings["EmailUserId"].ToString();
            request.SalesPriorityOrdersEmail.Password = ConfigurationManager.AppSettings["EmailPassword"].ToString();

            return DoExecute<SalesException>(() =>
            {
                var result = receivablesService.PushEftTransactionsToGP(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }


        //[HttpGet]
        [HttpPost]
        public IHttpActionResult FetchEmailReferenceLookup(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Customer Id Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = receivablesService.FetchEmailReferenceLookup(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }


        [HttpGet]
        [HttpPost]
        public IHttpActionResult FetchEmailReferenceScroll(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = receivablesService.FetchEmailReferenceScroll(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult DeleteEFTEmailRemittance(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = receivablesService.DeleteEFTEmailRemittance(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult SaveEmailReferences(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = receivablesService.SaveEmailReferences(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
/// <summary>
        /// DeleteBankEntryEFT Transaction ...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult DeleteBankEntryEFTTransaction(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SalesOrderRequest is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = receivablesService.DeleteBankEntryEFTTransaction(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }


        /// <summary>
        /// MapBankEntryToEmailRemittance Transaction ...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult MapBankEntryToEmailRemittance(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SalesOrderRequest is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = receivablesService.MapBankEntryToEmailRemittance(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
     
        #endregion
    }
}

