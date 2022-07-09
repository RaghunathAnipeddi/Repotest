using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Chempoint.GP.Infrastructure.Exceptions;
using Chempoint.GP.Infrastructure.Extensions;
using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.APIServices.Utils;
using ChemPoint.GP.BusinessContracts.Sales;
using Chempoint.GP.Model;
using ChemPoint.GP.SalesOrderBL;
using Chempoint.GP.Model.SearchQueryModel.Sales;
using System.Configuration;
using ChemPoint.GP.Entities.Business_Entities;
using ChemPoint.GP.APIServices.Controllers.Sales;
using ChemPoint.GP.BusinessContracts.Setup;
using Chempoint.GP.Model.Interactions.Setup;
using ChemPoint.GP.SetupWindowBL;


namespace ChemPoint.GP.APIServices.Controllers.Setup
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
        public IHttpActionResult SaveTaxRefDetail(TaxDetailSetupRequest request)
        {
            //request = new TaxDetailSetupRequest();
            //SetupDetailEntity setupDetailEntity = new SetupDetailEntity();
            //request.CompanyID = 3;
            //setupDetailEntity.TaxDetailId = "Test1";
            //setupDetailEntity.TaxDetailReference = "testRef";
            //setupDetailEntity.UnivarNvTaxCode = "NV1";
            //setupDetailEntity.UnivarNvTaxCodeDescription = "NV2";
            //setupDetailEntity.UserId = "ranipeddi";
            //request.setupDetailEntity = setupDetailEntity;

            if (request.IsInValid())
                return InternalServerError(new Exception("Save Tax Detail Ref Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else if (request.CompanyID == 2)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GpCustomizationConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = iSetupDetailUpdate.SaveTaxExtDetails(request);

                if (result.IsValid())
                    return new Jsonizer(result.Status, Request);
                else
                    return NotFound();
            }
            );
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult DeleteTaxRefDetail(TaxDetailSetupRequest request)
        {
            //request = new TaxDetailSetupRequest();
            //SetupDetailEntity setupDetailEntity = new SetupDetailEntity();
            //request.CompanyID = 3;
            //setupDetailEntity.TaxDetailId = "Test1";
            //request.setupDetailEntity = setupDetailEntity;

            if (request.IsInValid())
                return InternalServerError(new Exception("Delete Tax Detail Ref Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GpCustomizationConString"].ConnectionString;
            else if (request.CompanyID == 2)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GpCustomizationConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GpCustomizationConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = iSetupDetailUpdate.DeleteTaxExtDetails(request);

                if (result.IsValid())
                    return new Jsonizer(result.Status, Request);
                else
                    return NotFound();
            }
            );
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult FetchTaxRefDetail(TaxDetailSetupRequest request)
        
        {
            //request = new TaxDetailSetupRequest();
            //SetupDetailEntity setupDetailEntity = new SetupDetailEntity();
            //request.CompanyID = 3;
            //setupDetailEntity.TaxDetailId = "TEST";
            //request.setupDetailEntity = setupDetailEntity;

            if (request.IsInValid())
                return InternalServerError(new Exception("Fetch Tax Detail Ref Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GpCustomizationConString"].ConnectionString;
            else if (request.CompanyID == 2)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GpCustomizationConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GpCustomizationConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = iSetupDetailUpdate.GetTaxExtDetails(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            }
            );
        }
        #endregion TaxDetailRef

        #region TaxScheduleMaint
        [HttpGet]
        [HttpPost]
        public IHttpActionResult SaveTaxScheduleDetail(TaxScheduledMaintenanceRequest request)
        {
            request = new TaxScheduledMaintenanceRequest();
            TaxScheduledMaintenanceEntity setupDetailEntity = new TaxScheduledMaintenanceEntity();
            request.CompanyID = 3;
            setupDetailEntity.TaxScheduleId = "Test1";
            setupDetailEntity.ChempointVatNumber = "testRef";
            request.UserId = "ranipeddi";
            request.taxScheduledMaintenanceEntity = setupDetailEntity;

            if (request.IsInValid())
                return InternalServerError(new Exception("Save Tax Detail Ref Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GpCustomizationConString"].ConnectionString;
            else if (request.CompanyID == 2)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GpCustomizationConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GpCustomizationConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = iSetupDetailUpdate.SaveTaxScheduleDetails(request);

                if (result.IsValid())
                    return new Jsonizer(result.Status, Request);
                else
                    return NotFound();
            }
            );
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult DeleteTaxScheduleDetail(TaxScheduledMaintenanceRequest request)
        {
            request = new TaxScheduledMaintenanceRequest();
            TaxScheduledMaintenanceEntity setupDetailEntity = new TaxScheduledMaintenanceEntity();
            request.CompanyID = 3;
            setupDetailEntity.TaxScheduleId = "Test1";
            request.taxScheduledMaintenanceEntity = setupDetailEntity;

            if (request.IsInValid())
                return InternalServerError(new Exception("Delete Tax Detail Ref Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GpCustomizationConString"].ConnectionString;
            else if (request.CompanyID == 2)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GpCustomizationConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GpCustomizationConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = iSetupDetailUpdate.DeleteTaxScheduleDetails(request);

                if (result.IsValid())
                    return new Jsonizer(result.Status, Request);
                else
                    return NotFound();
            }
            );
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult FetchTaxScheduleDetail(TaxScheduledMaintenanceRequest request)
        {
            request = new TaxScheduledMaintenanceRequest();
            TaxScheduledMaintenanceEntity setupDetailEntity = new TaxScheduledMaintenanceEntity();
            request.CompanyID = 3;
            setupDetailEntity.TaxScheduleId = "Test1";
            request.taxScheduledMaintenanceEntity = setupDetailEntity;

            if (request.IsInValid())
                return InternalServerError(new Exception("Fetch Tax Detail Ref Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GpCustomizationConString"].ConnectionString;
            else if (request.CompanyID == 2)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GpCustomizationConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GpCustomizationConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = iSetupDetailUpdate.GetTaxScheduleDetails(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            }
            );
        }
        #endregion TaxScheduleMaint


    }
}