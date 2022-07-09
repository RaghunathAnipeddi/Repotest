using System;
using System.Web.Http;
using Chempoint.GP.Infrastructure.Exceptions;
using Chempoint.GP.Infrastructure.Extensions;
using ChemPoint.GP.APIServices.Utils;
using ChemPoint.GP.HoldEngineBL;
using System.Configuration;
using Chempoint.GP.Model.Interactions.HoldEngine;
using ChemPoint.GP.BusinessContracts.TaskScheduler.HoldEngine;
using ChemPoint.GP.ApiServices.Controllers.Sales;
using Chempoint.GP.Model.Interactions.Email;
using ChemPoint.GP.Entities.BaseEntities;

namespace ChemPoint.GP.ApiServices.Controllers.TaskScheduler.HoldEngine
{
    public class HoldEngineController : BaseApiController
    {
        private IHoldEngineBusiness holdEngineService;

        public HoldEngineController()
        {
            holdEngineService = new HoldEngineBusiness();
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ProcessCreditHold(HoldEngineRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ProcessCreditHold Request is null"));

            if (request.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = holdEngineService.ProcessCreditHold(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ProcessCustomerHold(HoldEngineRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ProcessCustomerHold Request is null"));

            if (request.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = holdEngineService.ProcessCustomerHold(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ProcessCustomerDocumentHold(HoldEngineRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ProcessCustomerDocumentHold Request is null"));

            if (request.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = holdEngineService.ProcessCustomerDocumentHold(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ProcessFirstOrderHold(HoldEngineRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ProcessFirstOrderHold Request is null"));

            if (request.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = holdEngineService.ProcessFirstOrderHold(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ProcessSalesAlertHold(HoldEngineRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ProcessSalesAlertHold Request is null"));

            if (request.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = holdEngineService.ProcessSalesAlertHold(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ProcessManualHold(HoldEngineRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ProcessManualHold Request is null"));

            if (request.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = holdEngineService.ProcessManualHold(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ProcessTermHold(HoldEngineRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ProcessTermHold Request is null"));

            if (request.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = holdEngineService.ProcessTermHold(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ProcessUpdateOpenOrdersPaymentTerms(HoldEngineRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ProcessUpdateOpenOrdersPaymentTerms Request is null"));

            if (request.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = holdEngineService.ProcessUpdateOpenOrdersPaymentTerms(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ProcessVatHold(HoldEngineRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ProcessVATHold Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            return DoExecute<SalesException>(() =>
            {
                var result = holdEngineService.ProcessVatHold(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ProcessTaxHold(HoldEngineRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ProcessTaxHold Request is null"));

            if (request.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = holdEngineService.ProcessTaxHold(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ProcessCustCreditCache(HoldEngineRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ProcessCustCreditCache Request is null"));

            if (request.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = holdEngineService.ProcessCustCreditCache(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ProcessHoldEngine(HoldEngineRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ProcessHoldEngine Request is null"));
              
            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = holdEngineService.ProcessHoldEngine(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ProcessCreditHoldEngine(HoldEngineRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ProcessCreditHoldEngine Request is null"));
            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            return DoExecute<SalesException>(() =>
            {
                var result = holdEngineService.ProcessCreditHoldEngine(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #region  freight holds

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ProcessFreightHold(HoldEngineRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ProcessFreightHold Request is null"));

            if (request.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = holdEngineService.ProcessFreightHold(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
        #endregion  freight holds

        #region  Export holds

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ProcessExportHold(HoldEngineRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ProcessFreightHold Request is null"));

            if (request.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = holdEngineService.ProcessExportHold(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ExportHoldMailingDetails(HoldEngineRequest request)
        {
            SendEmailRequest EmailRequest = new SendEmailRequest();
            EMailInformation EmailInformation = new EMailInformation();
            EmailInformation.SmtpAddress = ConfigurationManager.AppSettings["SMTPServcer"].ToString();
            EmailInformation.EmailFrom = ConfigurationManager.AppSettings["ExportHoldEmailFrom"].ToString();
            EmailInformation.Signature = ConfigurationManager.AppSettings["ExportHoldSignature"].ToString();
            EmailInformation.Subject = request.EmailRequest.EmailInformation.Subject;
            EmailInformation.Body = request.EmailRequest.EmailInformation.Body;
            EmailRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            EmailRequest.EmailInformation = EmailInformation;
            request.EmailRequest = EmailRequest;
            request.AppConfigID = Convert.ToInt16(ConfigurationManager.AppSettings["ExportConfigId"]);
            if (request.IsInValid())
                return InternalServerError(new Exception("ProcessFreightHold Request is null"));

            if (request.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = holdEngineService.ExportHoldMailingDetails(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion  freight holds

    }
}