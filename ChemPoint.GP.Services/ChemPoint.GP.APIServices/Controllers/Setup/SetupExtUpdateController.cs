using System;
using System.Web.Http;
using Chempoint.GP.Infrastructure.Exceptions;
using Chempoint.GP.Infrastructure.Extensions;
using ChemPoint.GP.APIServices.Utils;
using System.Configuration;
using ChemPoint.GP.ApiServices.Controllers.Sales;
using ChemPoint.GP.BusinessContracts.Setup;
using Chempoint.GP.Model.Interactions.Setup;
using ChemPoint.GP.SetupBL;

namespace ChemPoint.GP.ApiServices.Controllers.Setup
{
    public class SetupExtUpdateController : BaseApiController
    {
        private ISetupDetailUpdate iSetupDetailUpdate = null;

        public SetupExtUpdateController()
        {
            iSetupDetailUpdate = new SetupDetailsBusiness();
        }

        #region TaxDetailRef

        [HttpGet]
        [HttpPost]
        public IHttpActionResult SaveTaxDetailCustomRecord(SetupRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Save Tax Detail Ref Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else if (request.CompanyID == 2)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SetupExtException>(() =>
            {
                var result = iSetupDetailUpdate.SaveTaxDetailsToDB(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult DeleteTaxDetailCustomRecord(SetupRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Delete Tax Detail Ref Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else if (request.CompanyID == 2)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SetupExtException>(() =>
            {
                var result = iSetupDetailUpdate.DeleteTaxDetailsFromDB(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult FetchTaxDetailCustomRecord(SetupRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Fetch Tax Detail Ref Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else if (request.CompanyID == 2)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SetupExtException>(() =>
            {
                var result = iSetupDetailUpdate.FetchTaxDetailCustomRecord(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion TaxDetailRef

        #region TaxScheduleMaint

        [HttpGet]
        [HttpPost]
        public IHttpActionResult SaveTaxScheduleCustomRecord(SetupRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Save Tax Detail Ref Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SetupExtException>(() =>
            {
                var result = iSetupDetailUpdate.SaveTaxScheduleToDB(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult DeleteTaxScheduleCustomRecord(SetupRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Delete Tax Detail Ref Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SetupExtException>(() =>
            {
                var result = iSetupDetailUpdate.DeleteTaxScheduleFromDB(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult FetchTaxScheduleCustomRecord(SetupRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Fetch Tax Detail Ref Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SetupExtException>(() =>
            {
                var result = iSetupDetailUpdate.FetchTaxScheduleCustomRecord(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion TaxScheduleMaint

        #region PaymentTermSetup

        [HttpGet]
        [HttpPost]
        public IHttpActionResult SavePaymentTermCustomRecord(SetupRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Save Payment Terms Request is null"));
           
            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SetupExtException>(() =>
            {
                var result = iSetupDetailUpdate.SavePaymentTermDetail(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult DeletePaymentTermCustomRecord(SetupRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Delete Tax Detail Ref Request is null"));
            
            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            return DoExecute<SetupExtException>(() =>
            {
                var result = iSetupDetailUpdate.DeletePaymentTermDetail(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetPaymentTermCustomRecord(SetupRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Fetch Tax Detail Ref Request is null"));
            
            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SetupExtException>(() =>
            {
                var result = iSetupDetailUpdate.FetchPaymentTermDetail(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetCalulatedDueDateByPaymentTerm(SetupRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Sales Entry Save Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;            

            return DoExecute<SalesException>(() =>
            {
                var result = iSetupDetailUpdate.GetCalulatedDueDateByPaymentTerm(request, request.CompanyID);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
        
        #endregion  PaymentTermsSetup
    }
}