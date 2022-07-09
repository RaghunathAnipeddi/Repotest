using Chempoint.GP.Infrastructure.Exceptions;
using Chempoint.GP.Infrastructure.Extensions;
using Chempoint.GP.Model.Interactions.PayableManagement;
using Chempoint.GP.Model.Interactions.Purchases;
using ChemPoint.GP.ApiServices.Controllers.Sales;
using ChemPoint.GP.APIServices.Utils;
using ChemPoint.GP.BusinessContracts.Purchase;
using ChemPoint.GP.Entities.BaseEntities;
using ChemPoint.GP.Pobl;
using System;
using System.Configuration;
using System.Web.Http;

namespace ChemPoint.GP.ApiServices.Controllers.Purchase
{
    public class PurchaseOrderController : BaseApiController
    {
        private IPurchaseOrderBusiness purchaseService;

        public PurchaseOrderController()
        {
            purchaseService = new POBusiness();
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetPoIndicatorDetail(PurchaseIndicatorRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Po Indicator Fetch Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.GetPoIndicatorDetail(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult SavePoIndicatorDetail(PurchaseIndicatorRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Purchase Indicator details Save Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.SavePoIndicatorDetail(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult DeletePoIndicatorDetail(PurchaseIndicatorRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Purchase Indicator details Delete Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.DeletePoIndicatorDetail(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #region POCostMgt

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ValidatePoCostChanges(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Purchase Indicator details Delete Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.ValidatePoCostChanges(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult SavePoCostManagementChangestoAudit(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Purchase Indicator details Delete Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.SavePoCostManagementChangestoAudit(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult UpdatePoCostNotes(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Purchase Indicator details Delete Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.UpdatePoCostNotes(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult FetchCostBookCostModifiedDetails(PurchaseOrderRequest objPurchaseOrderRequest)
        {

            objPurchaseOrderRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.FetchCostBookModifiedDetails(objPurchaseOrderRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult UpdateHasCostVariance(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Purchase Indicator details Delete Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.UpdateHasCostVariance(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult SavePoUnitCostDetails(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Purchase Indicator details Delete Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.SavePoUnitCostDetails(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult UpdatePoCostProactiveMailStatus(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Purchase Indicator details Delete Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.UpdatePoCostProactiveMailStatus(request);

                if (result.IsValid() && result.Status == 0)
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion

        #region MaterialMgt

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ValidatePoForMailApproval(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Purchase Order details Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.ValidatePoForMailApproval(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult SendMailForMaterialManagement(PurchaseOrderRequest request)
        {
            EMailInformation EmailInformation = new EMailInformation();
            EmailInformation.SmtpAddress = ConfigurationManager.AppSettings["SMTPServcer"].ToString();
            EmailInformation.EmailFrom = ConfigurationManager.AppSettings["MaterialMgtEmailFrom"].ToString();
            EmailInformation.Signature = request.emailInfomation.Signature+ ConfigurationManager.AppSettings["MaterialMgtSignature"].ToString();
            EmailInformation.Subject = request.emailInfomation.Subject;
            EmailInformation.Body = request.emailInfomation.Body;            
            request.emailInfomation = EmailInformation;
            request.AppConfigID = Convert.ToInt16(ConfigurationManager.AppSettings["MaterialMgtConfigId"].ToString()); 

            if (request.IsInValid())
                return InternalServerError(new Exception("Purchase Order details Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.SendMailForMaterialManagement(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion


        #region POActivityCreatedtoXRM

        [HttpGet]
        [HttpPost]
        public IHttpActionResult CreateActivity(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Po Indicator Fetch Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            request.CrmActivityConfigurationURL = ConfigurationManager.AppSettings["CrmActivityConfigurationURL"].ToString().Trim();
            request.LoggingPath = ConfigurationManager.AppSettings["CrmActivityCreateloggingPath"].ToString().Trim();
            request.LoggingFileName = ConfigurationManager.AppSettings["CrmActivityCreateloggingFileName"].ToString().Trim();

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.CreateActivity(request);
                Console.WriteLine(result.Status);
                Console.ReadLine();
                if (result.IsValid() && result.Status == ResponseStatus.Success)
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion POActivityCreatedtoXRM

        #region LandedCost

        [HttpGet]
        [HttpPost]
        public IHttpActionResult SaveEstimatedShipmentCostEntry(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Purchase landedcost details Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.SaveEstimatedShipmentCostEntry(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }


        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetShipmentEstimateDetails(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Po Indicator Fetch Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.GetShipmentEstimateDetails(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetShipmentEstimateInquiryDetails(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Po Indicator Fetch Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.GetShipmentEstimateInquiryDetails(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetPOShipmentEstimateDetails(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Po Indicator Fetch Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.GetPOShipmentEstimateDetails(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult DeleteEstimateLineDetails(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Po Indicator Fetch Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.DeleteEstimateLineDetails(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetEstimateId(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Po Indicator Fetch Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.GetEstimateId(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult DeleteEstimatedId(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Po Indicator Fetch Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.DeleteEstimatedId(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetNextEstimateId(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Estimated next number Fetch Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.GetNextEstimateId(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetShipmentQtyTotal(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Estimated next number Fetch Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.GetShipmentQtyTotal(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetPoNumber(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Estimated next number Fetch Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.GetPoNumber(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ValidateCarrierReference(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Estimated next number Fetch Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.ValidateCarrierReference(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult FetchEstimatedShipmentDetails(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Estimated next number Fetch Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.FetchEstimatedShipmentDetails(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetCurrencyIndex(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Estimated next number Fetch Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.GetCurrencyIndex(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion

        #region Purchase Order | default BuyerID to improve user experience

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ValidatePurchaseBuyerId(PurchaseOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Purchase Indicator details Delete Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.ValidatePurchaseBuyerId(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion

        #region Elemica

        [HttpGet]
        [HttpPost]
        public IHttpActionResult RetrieveElemicaDetail(PurchaseElemicaRequest poElemicaRequest)
        {
            if (poElemicaRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            poElemicaRequest.connnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.RetrieveElemicaDetail(poElemicaRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult SendElemicaDetail(PurchaseElemicaRequest poElemicaRequest)
        {
            if (poElemicaRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            poElemicaRequest.connnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.SendElemicaDetail(poElemicaRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult UpdatePOStatusForElemica(PurchaseElemicaRequest poElemicaRequest)
        {
            if (poElemicaRequest.IsInValid())
                return InternalServerError(new Exception("Purchase elemica details Request is null"));

            poElemicaRequest.connnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = purchaseService.UpdatePOStatusForElemica(poElemicaRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion

    }
}
