using System;
using System.Web.Http;
using Chempoint.GP.Infrastructure.Exceptions;
using Chempoint.GP.Infrastructure.Extensions;
using ChemPoint.GP.APIServices.Utils;
using ChemPoint.GP.BusinessContracts.Sales;
using System.Configuration;
using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.SalesOrderBL;

namespace ChemPoint.GP.ApiServices.Controllers.Sales
{
    public class SalesOrderUpdateController : BaseApiController
    {
        private ISalesOrderBusiness salesService;

        public SalesOrderUpdateController()
        {
            salesService = new SalesOrderBusiness();
        }

        /// <summary>
        /// Get Inco Term service method...
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult FetchIncoTerm(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Inco Term Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.FetchIncoTerm(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        /// <summary>
        /// Get country name method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetCountryDetails(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("country name Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.GetCountryDetails(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        /// <summary>
        /// Get Schedule Delivery Date service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetSalesCurSchedDelDate(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Sales CurSchedDel Date Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.GetSalesCurSchedDelDate(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        /// <summary>
        /// Get Schedule Ship Date service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetSalesCurSchedShipDate(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Sales CurSchedShip Date Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.GetSalesCurSchedShipDate(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        /// <summary>
        /// Get Schedule Ship Date service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult UpdateTaxScheduleIdToLine(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Sales Update TaxScheduleId To Line Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.UpdateTaxScheduleIdToLine(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        /// <summary>
        /// Get Schedule Ship Date service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult FetchSalesOrderLineForTermHold(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Sales Update TaxScheduleId To Line Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            request.TermHoldRemainingBalance = Convert.ToDecimal(ConfigurationManager.AppSettings["TermHoldRemainingBalance"].ToString());

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.FetchSalesOrderLineForTermHold(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #region SOPOrderEntry

        /// <summary>
        /// Get SalesOrder Entity service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetSalesOrder(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Sales Item detail Fetch Request is null"));
            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.GetSalesOrder(request);

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
        [HttpGet]
        [HttpPost]
        public IHttpActionResult SaveAllocatedQty(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Sales Item detail Fetch Request is null"));
            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.SaveAllocatedQty(request);

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
        public IHttpActionResult SaveSalesOrder(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SaveSalesEntryDetail Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.SaveSalesOrder(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpPost]
        public IHttpActionResult UpdatePrintPickTicketStatus(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("UpdatePrintPickTicketStatus Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.UpdatePrintPickTicketStatus(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion SOPOrderEntry

        #region SalesLineItem

        /// <summary>
        /// Get SalesOrder Entity service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SaveSalesItemDetail(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Sales Item detail Fetch Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.SaveSalesItemDetail(request);

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
        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetSalesItemDetail(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Sales Item detail Fetch Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.GetSalesItemDetail(request);

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
        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetQuoteNumber(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Sales Item detail GetQuoteNumber Fetch Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.GetQuoteNumber(request);

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
        [HttpGet]
        [HttpPost]
        public IHttpActionResult ValidateQuoteNumber(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Sales Item detail QuoteNumber Validate Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.ValidateQuoteNumber(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion SalesLineItem

        #region SalesOrderType

        /// <summary>
        /// Get SalesOrder Entity service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SaveOrderTypeDetail(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SaveOrderTypeDetail Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.SaveOrderTypeDetail(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion SalesOrderType

        #region 3rdParty/CustomerPickup
        
        /// <summary>
        /// Save Third party Address service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SaveThirdPartyAddress(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SaveThirdPartyAddress Request is null"));
            
            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;
            
            return DoExecute<SalesException>(() =>
            {
                var result = salesService.SaveThirdPartyAddress(request);
                
                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
        
        /// <summary>
        /// save customer pickup address service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SaveCustomerPickupAddress(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SaveCustomerPickupAddress Request is null"));
            
            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;
            
            return DoExecute<SalesException>(() =>
            {
                var result = salesService.SaveCustomerPickupAddress(request);
                
                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
        
        #endregion 3rdParty/CustomerPickup
        
        #region SaveSopTransactionAddressCodes
        
        /// <summary>
        /// Save SaveSopTransactionAddressCodes service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SaveSopTransactionAddressCodes(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SaveSopTransactionAddressCodes Request is null"));
            
            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;
            
            return DoExecute<SalesException>(() =>
            {
                var result = salesService.SaveSopTransactionAddressCodes(request);
                
                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
        
        /// <summary>
        /// Save Header SaveSopTransactionAddressCodes service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SaveHeaderCommentInstruction(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SaveHeaderCommentInstruction Request is null"));
            
            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;
            
            return DoExecute<SalesException>(() =>
            {
                var result = salesService.SaveHeaderCommentInstruction(request);
                
                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
        
        /// <summary>
        /// Save Line SaveSopTransactionAddressCodes service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SaveLineCommentInstruction(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SaveHeaderCommentInstruction Request is null"));
            
            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;
            
            return DoExecute<SalesException>(() =>
            {
                var result = salesService.SaveLineCommentInstruction(request);
                
                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
        
        /// <summary>
        /// Validate Line SaveSopTransactionAddressCodes service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult ValidateSopTransactionAddressCodes(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ValidateSopTransactionAddressCodes Request is null"));
            
            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;
            
            return DoExecute<SalesException>(() =>
            {
                var result = salesService.ValidateSopTransactionAddressCodes(request);
                
                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        /// <summary>
        /// Validate Line SaveSopTransactionAddressCodes service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult ValidateSopTransactionServiceSkuItems(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ValidateSopTransactionServiceSkuItems Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.ValidateSopTransactionServiceSkuItems(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
        
        #endregion SaveSopTransactionAddressCodes  
        
        #region PTE
        
        /// <summary>
        /// Get SalesOrder Entity service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult UpdatePteRequest(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("PTE Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            
            return DoExecute<SalesException>(() =>
            {
                var result = salesService.UpdatePteLog(request);
                    
                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion PTE
        
        #region LotDetails
        
        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetLotDetail(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Lot detail Fetch Request is null"));
            
            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            return DoExecute<SalesException>(() =>
            {
                var result = salesService.GetLotDetails(request);
                
                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
        
        [HttpPost]
        public IHttpActionResult SaveSalesLotDetails(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Lot detail Fetch Request is null"));
            
            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
                
            return DoExecute<SalesException>(() =>
            {
                var result = salesService.SaveSalesLotDetails(request);
                
                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
        
        #endregion LotDetails
        
        #region VATDetails
        
        /// <summary>
        /// Get SalesOrder Entity service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetVatLookupDetails(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("VAT Request is null"));
            
            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.GetVatLookupDetails(request);
                    
                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
        
        #endregion VATDetails
        
        #region UpdateOrderType
        
        /// <summary>
        /// Get SalesOrder Entity service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult UpdateOrderDetailsToInvoiceType(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Invalid satus update request"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;
                
            return DoExecute<SalesException>(() =>
            {
                var result = salesService.UpdateOrderDetailsToInvoiceType(request);
            
                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
        
        #endregion UpdateOrderType
        
        #region OrderToFO
        
        /// <summary>
        /// Get SalesOrder Entity service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult TransferOrderToFO(BulkOrderTransferRequest soRequest)
        {
            if (soRequest.IsInValid())
                return InternalServerError(new Exception("Tranfer To FO Request is null"));
            
            soRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            soRequest.NAEconnectConnectionString = ConfigurationManager.ConnectionStrings["CHMPTEconnectConString"].ConnectionString;
            soRequest.StyleSheetPath = ConfigurationManager.AppSettings["AutoTransferXSLTPath"].ToString();
            soRequest.XrmServiceUrl = ConfigurationManager.AppSettings["XrmServiceURL"].ToString();
            soRequest.PickTicketServiceUrl = ConfigurationManager.AppSettings["PickTicketServiceUrl"].ToString();
            soRequest.WarehouseEdiServiceUrl = ConfigurationManager.AppSettings["PickTicketWarehouseEdiServiceURL"].ToString();
            soRequest.PickTicketStyleSheetPath = ConfigurationManager.AppSettings["PickTicketStyleSheetPath"].ToString();
            soRequest.LoggingPath = ConfigurationManager.AppSettings["AutoTransferloggingPath"].ToString();
            soRequest.LoggingFileName = ConfigurationManager.AppSettings["AutoTransferloggingFileName"].ToString();
            
            soRequest.WarehouseSameDayEmail = new ChemPoint.GP.Entities.BaseEntities.EMailInformation();
            soRequest.WarehouseSameDayEmail.EmailFrom = ConfigurationManager.AppSettings["AutoTransferWarehouseEmailFrom"].ToString();
            soRequest.WarehouseSameDayEmail.EmailTo = ConfigurationManager.AppSettings["AutoTransferWarehouseEmailTo"].ToString();
            soRequest.WarehouseSameDayEmail.EmailCc = ConfigurationManager.AppSettings["AutoTransferWarehouseEmailCC"].ToString();
            soRequest.WarehouseSameDayEmail.EmailBcc = ConfigurationManager.AppSettings["AutoTransferWarehouseEmailBcc"].ToString();
            soRequest.WarehouseSameDayEmail.Subject = ConfigurationManager.AppSettings["AutoTransferWarehouseEmailSubject"].ToString();
            soRequest.WarehouseSameDayEmail.Body = ConfigurationManager.AppSettings["AutoTransferWarehouseEmailBody"].ToString();
            soRequest.WarehouseSameDayEmail.SmtpAddress = ConfigurationManager.AppSettings["SmtpAddress"].ToString();
            soRequest.WarehouseSameDayEmail.UserId = ConfigurationManager.AppSettings["EmailUserId"].ToString();
            soRequest.WarehouseSameDayEmail.Password = ConfigurationManager.AppSettings["EmailPassword"].ToString();
            
            soRequest.TransferFailureEmail = new ChemPoint.GP.Entities.BaseEntities.EMailInformation();
            soRequest.TransferFailureEmail.EmailFrom = ConfigurationManager.AppSettings["AutoTransferFailureEmailFrom"].ToString();
            soRequest.TransferFailureEmail.EmailTo = ConfigurationManager.AppSettings["AutoTransferFailureEmailTo"].ToString();
            soRequest.TransferFailureEmail.EmailCc = ConfigurationManager.AppSettings["AutoTransferFailureEmailCC"].ToString();
            soRequest.TransferFailureEmail.EmailBcc = ConfigurationManager.AppSettings["AutoTransferFailureEmailBcc"].ToString();
            soRequest.TransferFailureEmail.Subject = ConfigurationManager.AppSettings["AutoTransferFailureEmailSubject"].ToString();
            soRequest.TransferFailureEmail.Body = "";
            soRequest.TransferFailureEmail.SmtpAddress = ConfigurationManager.AppSettings["SmtpAddress"].ToString();
            soRequest.TransferFailureEmail.UserId = ConfigurationManager.AppSettings["EmailUserId"].ToString();
            soRequest.TransferFailureEmail.Password = ConfigurationManager.AppSettings["EmailPassword"].ToString();
                    
            return DoExecute<SalesException>(() =>
            {
                var result = salesService.TransferOrderToFO(soRequest);
        
                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
        
        #endregion OrderToFO
                
        #region ShipmentManager
            
        [HttpPost]
        public IHttpActionResult UpdateOrderDetailsToWarehouseIBoard(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Invalid satus update request"));
            
            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;
                
            return DoExecute<SalesException>(() =>
            {
                var result = salesService.UpdateOrderDetailsToWarehouseIBoard(request);
        
                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
        
        #endregion ShipmentManager

        #region Cash Application Process

        /// <summary>
        /// Get SalesOrder Entity service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult DistributeAmountToCashApplyInvoices(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("DistributeAmountToCashApplyInvoices Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.DistributeAmountToCashApplyInvoices(request);

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
        [HttpGet]
        [HttpPost]
        public IHttpActionResult ApplyToOpenOrders(SalesOrderRequest soRequest)
        {
            if (soRequest.IsInValid())
                return InternalServerError(new Exception("Apply To Open Orders Request is null"));

            soRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            soRequest.NAEconnectConnectionString = ConfigurationManager.ConnectionStrings["CHMPTEconnectConString"].ConnectionString;
            soRequest.EUEconnectConnectionString = ConfigurationManager.ConnectionStrings["CPEUREconnectConString"].ConnectionString;
            soRequest.StyleSheetPath = ConfigurationManager.AppSettings["CashProcessXSLTPath"].ToString();

            soRequest.LoggingPath = ConfigurationManager.AppSettings["CashProcessloggingPath"].ToString();
            soRequest.LoggingFileName = ConfigurationManager.AppSettings["CashProcessloggingFileName"].ToString();

            soRequest.SalesOrderFailureEmail = new ChemPoint.GP.Entities.BaseEntities.EMailInformation();
            soRequest.SalesOrderFailureEmail.EmailFrom = ConfigurationManager.AppSettings["CashProcessFailureEmailFrom"].ToString();
            soRequest.SalesOrderFailureEmail.EmailTo = ConfigurationManager.AppSettings["CashProcessFailureEmailTo"].ToString();
            soRequest.SalesOrderFailureEmail.EmailCc = ConfigurationManager.AppSettings["CashProcessFailureEmailCC"].ToString();
            soRequest.SalesOrderFailureEmail.EmailBcc = ConfigurationManager.AppSettings["CashProcessFailureEmailBcc"].ToString();
            soRequest.SalesOrderFailureEmail.Subject = ConfigurationManager.AppSettings["CashProcessFailureEmailSubject"].ToString();
            soRequest.SalesOrderFailureEmail.Body = "";
            soRequest.SalesOrderFailureEmail.SmtpAddress = ConfigurationManager.AppSettings["SmtpAddress"].ToString();
            soRequest.SalesOrderFailureEmail.UserId = ConfigurationManager.AppSettings["EmailUserId"].ToString();
            soRequest.SalesOrderFailureEmail.Password = ConfigurationManager.AppSettings["EmailPassword"].ToString();

            soRequest.SalesPriorityOrdersEmail = new ChemPoint.GP.Entities.BaseEntities.EMailInformation();
            soRequest.SalesPriorityOrdersEmail.EmailFrom = ConfigurationManager.AppSettings["CashProcessCurrencyEmailFrom"].ToString();
            soRequest.SalesPriorityOrdersEmail.EmailTo = ConfigurationManager.AppSettings["CashProcessCurrencyEmailTo"].ToString();
            soRequest.SalesPriorityOrdersEmail.EmailCc = ConfigurationManager.AppSettings["CashProcessCurrencyEmailCC"].ToString();
            soRequest.SalesPriorityOrdersEmail.EmailBcc = ConfigurationManager.AppSettings["CashProcessCurrencyEmailBcc"].ToString();
            soRequest.SalesPriorityOrdersEmail.Subject = ConfigurationManager.AppSettings["CashProcessCurrencyEmailSubject"].ToString().Trim();
            soRequest.SalesPriorityOrdersEmail.Body = "";
            soRequest.SalesPriorityOrdersEmail.SmtpAddress = ConfigurationManager.AppSettings["SmtpAddress"].ToString();
            soRequest.SalesPriorityOrdersEmail.UserId = ConfigurationManager.AppSettings["EmailUserId"].ToString();
            soRequest.SalesPriorityOrdersEmail.Password = ConfigurationManager.AppSettings["EmailPassword"].ToString();

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.ApplyToOpenOrders(soRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #region Cash Application
        /// <summary>
        /// Fetch value and assign to cash apply window
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult FetchApplyToOpenOrder(ReceivablesRequest request)
        {
            string errorMessage = string.Empty;

            if (request.IsInValid())
                return InternalServerError(new Exception("FetchApplyToOpenOrder Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.FetchApplyToOpenOrder(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }


        /// <summary>
        /// Save value and assign to cash apply window
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult SaveApplyToOpenOrder(ReceivablesRequest request)
        {
            string errorMessage = string.Empty;

            if (request.IsInValid())
                return InternalServerError(new Exception("SaveApplyToOpenOrder Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.SaveApplyToOpenOrder(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        /// <summary>
        /// Update value and assign to cash apply window
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult UpdateApplyToOpenOrder(ReceivablesRequest request)
        {
            string errorMessage = string.Empty;

            if (request.IsInValid())
                return InternalServerError(new Exception("SaveApplyToOpenOrder Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.UpdateApplyToOpenOrder(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }


        /// <summary>
        /// Delete and assign to cash apply window
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult DeleteApplyToOpenOrder(ReceivablesRequest request)
        {
            string errorMessage = string.Empty;

            if (request.IsInValid())
                return InternalServerError(new Exception("DeleteApplyToOpenOrder Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.DeleteApplyToOpenOrder(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetReceivablesDetail(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Receivables detail Fetch Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.GetReceivablesDetail(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetDistributeAmountDetail(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Distribute Amount detail Fetch Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            return DoExecute<SalesException>(() =>
            {
                var result = salesService.GetDistributeAmountDetail(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult SaveReceivablesDetail(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Receivables detail Save Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            return DoExecute<SalesException>(() =>
            {
                var result = salesService.SaveReceivablesDetail(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion Cash Application

        #endregion Cash Application Process

        #region WarehouseClosure

        /// <summary>
        /// Validate Warehouse closure date
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult ValidateWarehouseClosureDate(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Sales Update TaxScheduleId To Line Request is null"));
         
                request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.ValidateWarehouseClosureDate(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }


        #endregion WarehouseClosure

        #region EFTAutomation

        #region EFT_Customer_Mapping_Window

        /// <summary>
        /// Get EFT Customer Details ...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetEFTCustomerMappingDetails(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SalesOrderRequest is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.GetEFTCustomerMappingDetails(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        /// <summary>
        /// Get EFT Customer Details ...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult SaveEFTCustomerMappingDetails(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SalesOrderRequest is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.SaveEFTCustomerMappingDetails(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion EFT_Customer_Mapping_Window

        /// <summary>
        /// GetEFTCustomerRemittances ...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetEFTCustomerRemittances(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SalesOrderRequest is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.GetEFTCustomerRemittances(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        /// <summary>
        /// GetEFTPaymentRemittances ...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetEFTPaymentRemittances(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SalesOrderRequest is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.GetEFTPaymentRemittances(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        /// <summary>
        /// DeleteBankEntryItemReference ...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult DeleteBankEntryItemReference(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SalesOrderRequest is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.DeleteBankEntryItemReference(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }


        /// <summary>
        /// Get EFT Customer Details ...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult SaveEFTCustomerRemittances(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SalesOrderRequest is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.SaveEFTCustomerRemittances(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        /// <summary>
        /// Validate EFT Customer Details ...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult ValidateEftCustomer(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SalesOrderRequest is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.ValidateEftCustomer(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        /// <summary>
        /// ValidateEFTItemReference
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IHttpActionResult ValidateEFTItemReference(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SalesOrderRequest is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.ValidateEFTItemReference(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        /// <summary>
        /// ValidateEftReference
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IHttpActionResult ValidateEftReference(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SalesOrderRequest is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.ValidateEftReference(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }


        /// <summary>
        /// ValidateEftReference
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        
        [HttpGet]
        [HttpPost]
        public IHttpActionResult ValidateEftEmailReference(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SalesOrderRequest is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.ValidateEftEmailReference(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult ValidateEFTCustomerRemittanceSummary(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ValidateEFTCustomerRemittanceSummary Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.ValidateEFTCustomerRemittanceSummary(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        /// <summary>
        /// Get Inco Term service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult FetchCustomerId(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Customer Id Request is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.FetchCustomerId(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult FetchDocumentNumber(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Document Id Request is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.FetchDocumentNumber(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult FetchReferenceId(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Reference Id Request is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.FetchReferenceId(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }


        [HttpGet]
        [HttpPost]
        public IHttpActionResult FetchCustomerIdForReference(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Reference Id Request is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.FetchCustomerIdForReference(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        /// <summary>
        /// GetEFTEmailRemittances ...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetEFTEmailRemittances(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ReceivablesRequest is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.GetEFTEmailRemittances(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }


        /// <summary>
        /// SaveEFTEmailRemittances ...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult SaveEFTEmailRemittances(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("ReceivablesRequest is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.SaveEFTEmailRemittances(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }



        [HttpGet]
        [HttpPost]
        public IHttpActionResult FetchBatchId(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("FetchBatchId Request is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.FetchBatchId(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }


        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetEFTPaymentRemittanceAmountDetails(ReceivablesRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("FetchBatchId Request is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.GetEFTPaymentRemittanceAmountDetails(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        #endregion EFTAutomation

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult AuditCustomizeServiceSkU(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("FetchBatchId Request is null"));

            if (request.AuditInformation.CompanyId == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.AuditCustomizeServiceSkU(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
    }
}

