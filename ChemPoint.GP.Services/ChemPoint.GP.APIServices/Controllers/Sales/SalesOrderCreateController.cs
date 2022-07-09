using System;
using System.Web.Http;
using Chempoint.GP.Infrastructure.Exceptions;
using Chempoint.GP.Infrastructure.Extensions;
using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.APIServices.Utils;
using ChemPoint.GP.BusinessContracts.Sales;
using ChemPoint.GP.SalesOrderCreateBL;
using System.Configuration;

namespace ChemPoint.GP.ApiServices.Controllers.Sales
{
    public class SalesOrderCreateController : BaseApiController
    {
        private ISalesOrderCreateBI salesService = null;

        public SalesOrderCreateController()
        {
            salesService = new SalesOrderCreateBusiness();
        }

        [HttpPost]
        public IHttpActionResult CreateSalesOrder(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Sales order create request is null"));

            //fill config detals
            request.XrmServiceUrl = ConfigurationManager.AppSettings["XrmServiceURL"].ToString();
            request.LoggingPath = ConfigurationManager.AppSettings["OrderPushloggingPath"].ToString();
            request.LoggingFileName = ConfigurationManager.AppSettings["OrderPushloggingFileName"].ToString();
            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            request.NAConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            request.NAEconnectConnectionString = ConfigurationManager.ConnectionStrings["CHMPTEconnectConString"].ConnectionString;
            request.EUConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;
            request.EUEconnectConnectionString = ConfigurationManager.ConnectionStrings["CPEUREconnectConString"].ConnectionString;
            request.StyleSheetPath = ConfigurationManager.AppSettings["OrderCreateXSLTPath"].ToString();
            request.MailStyleSheetPath = ConfigurationManager.AppSettings["MailStyleSheet"].ToString();

            request.SalesOrderFailureEmail = new ChemPoint.GP.Entities.BaseEntities.EMailInformation();
            request.SalesOrderFailureEmail.EmailFrom = ConfigurationManager.AppSettings["OrderPushfailureEmailFrom"].ToString();
            request.SalesOrderFailureEmail.EmailTo = ConfigurationManager.AppSettings["OrderPushfailureEmailTo"].ToString();
            request.SalesOrderFailureEmail.EmailCc = ConfigurationManager.AppSettings["OrderPushfailureEmailCC"].ToString();
            request.SalesOrderFailureEmail.Subject = ConfigurationManager.AppSettings["OrderPushfailureEmailSubject"].ToString();
            request.SalesOrderFailureEmail.SmtpAddress = ConfigurationManager.AppSettings["SmtpAddress"].ToString();
            request.SalesOrderFailureEmail.UserId = ConfigurationManager.AppSettings["EmailUserId"].ToString();
            request.SalesOrderFailureEmail.Password = ConfigurationManager.AppSettings["EmailPassword"].ToString();

            request.SalesPriorityOrdersEmail = new ChemPoint.GP.Entities.BaseEntities.EMailInformation();
            request.SalesPriorityOrdersEmail.EmailFrom = ConfigurationManager.AppSettings["SalesPriorityOrdersEmailFrom"].ToString();
            request.SalesPriorityOrdersEmail.EmailTo = ConfigurationManager.AppSettings["SalesPriorityOrdersEmailTo"].ToString();
            request.SalesPriorityOrdersEmail.EmailCc = ConfigurationManager.AppSettings["SalesPriorityOrdersEmailCC"].ToString();
            request.SalesPriorityOrdersEmail.Subject = ConfigurationManager.AppSettings["SalesPriorityOrdersEmailSubject"].ToString();
            request.SalesPriorityOrdersEmail.SmtpAddress = ConfigurationManager.AppSettings["SmtpAddress"].ToString();
            request.SalesPriorityOrdersEmail.UserId = ConfigurationManager.AppSettings["EmailUserId"].ToString();
            request.SalesPriorityOrdersEmail.Password = ConfigurationManager.AppSettings["EmailPassword"].ToString();

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.CreateSalesOrder(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
    }
}