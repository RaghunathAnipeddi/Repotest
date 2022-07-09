using System;
using System.Web.Http;
using Chempoint.GP.Infrastructure.Exceptions;
using Chempoint.GP.Infrastructure.Extensions;
using ChemPoint.GP.APIServices.Utils;
using ChemPoint.GP.BusinessContracts.Sales;
using System.Configuration;
using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.PickTicketBL;

namespace ChemPoint.GP.ApiServices.Controllers.Sales
{
    public class PickTicketController : BaseApiController
    {
        private IPickTicketBI pickTicketService;

        public PickTicketController()
        {
            pickTicketService = new PickTicketBusiness();
        }

        [HttpPost]
        public IHttpActionResult PrintPickTicket(PickTicketRequest pickTicketRequest)
        {
            if (pickTicketRequest.IsInValid())
                return InternalServerError(new Exception("Prick Ticket Request is null"));

            if (pickTicketRequest.CompanyID == 1)
            {
                pickTicketRequest.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
                pickTicketRequest.CompanyName = "Chmpt";
            }
            else
            {
                pickTicketRequest.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;
                pickTicketRequest.CompanyName = "Cpeur";
            }

            pickTicketRequest.XrmServiceUrl = ConfigurationManager.AppSettings["XrmServiceURL"].ToString();
            pickTicketRequest.WarehouseEdiServiceUrl = ConfigurationManager.AppSettings["PickTicketWarehouseEdiServiceURL"].ToString();
            pickTicketRequest.StyleSheetPath = ConfigurationManager.AppSettings["PickTicketStyleSheetPath"].ToString();
            pickTicketRequest.LoggingPath = ConfigurationManager.AppSettings["PickTicketLoggingPath"].ToString();
            pickTicketRequest.LoggingFileName = ConfigurationManager.AppSettings["PickTicketLoggingFileName"].ToString();

            return DoExecute<SalesException>(() =>
            {
                var result = pickTicketService.PrintPickTicket(pickTicketRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetOrderPickTicketEligibleDetails(PickTicketRequest pickTicketRequest)
        {
            if (pickTicketRequest.IsInValid())
                return InternalServerError(new Exception("Prick Ticket Request is null"));

            if (pickTicketRequest.CompanyID == 1)
            {
                pickTicketRequest.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
                pickTicketRequest.CompanyName = "Chmpt";
            }
            else
            {
                pickTicketRequest.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;
                pickTicketRequest.CompanyName = "Cpeur";
            }

            pickTicketRequest.XrmServiceUrl = ConfigurationManager.AppSettings["XrmServiceURL"].ToString();
            pickTicketRequest.WarehouseEdiServiceUrl = ConfigurationManager.AppSettings["PickTicketWarehouseEdiServiceURL"].ToString();
            pickTicketRequest.StyleSheetPath = ConfigurationManager.AppSettings["PickTicketStyleSheetPath"].ToString();
            pickTicketRequest.LoggingPath = ConfigurationManager.AppSettings["PickTicketLoggingPath"].ToString();
            pickTicketRequest.LoggingFileName = ConfigurationManager.AppSettings["PickTicketLoggingFileName"].ToString();

            return DoExecute<SalesException>(() =>
            {
                var result = pickTicketService.GetOrderPickTicketEligibleDetails(pickTicketRequest);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
    }
}

