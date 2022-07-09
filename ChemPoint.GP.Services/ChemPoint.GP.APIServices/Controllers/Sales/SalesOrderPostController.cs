using Chempoint.GP.Model.Interactions.Sales;
using System;
using Chempoint.GP.Infrastructure.Exceptions;
using Chempoint.GP.Infrastructure.Extensions;
using ChemPoint.GP.APIServices.Utils;
using ChemPoint.GP.BusinessContracts.Sales;
using System.Web.Http;
using System.Configuration;
using ChemPoint.GP.OrderPostingBL;
using ChemPoint.GP.Entities.BaseEntities;
using Chempoint.GP.Model.Interactions.Email;


namespace ChemPoint.GP.ApiServices.Controllers.Sales
{
    public class SalesOrderPostController : BaseApiController
    {
        private ISalesOrderPostBL salesService;

        public SalesOrderPostController()
        {
            salesService = new PostProcessFactory();
        }

        [HttpPost]
        public IHttpActionResult ProcessRequest(OrderBatchPostRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Sales order create request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
            request.OpenOrdersSsrsReportUrl = ConfigurationManager.AppSettings["OpenOrdersSsrsReportURL"].ToString();

            // If Mail is needed, Following code will execute only ...
            if ((int)request.OperationType == 18 || (int)request.OperationType == 19)
            {
                SendEmailRequest EmailRequest = new SendEmailRequest();
                EMailInformation EmailInformation = new EMailInformation();
                EmailRequest.ServiceFileName = ConfigurationManager.AppSettings["ServiceFileName"].ToString();
                EmailRequest.LoggingPath = ConfigurationManager.AppSettings["LoggingPath"].ToString();
                EmailRequest.LogFileName = ConfigurationManager.AppSettings["LogFileName"].ToString();
                EmailRequest.FilePath = ConfigurationManager.AppSettings["FilePath"].ToString();
                EmailRequest.FileType = ConfigurationManager.AppSettings["FileType"].ToString();
                EmailInformation.SmtpAddress = ConfigurationManager.AppSettings["SMTPServcer"].ToString();
                EmailRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
                switch ((int)request.OperationType)
                {
                    case 18:
                        EmailInformation.EmailFrom = ConfigurationManager.AppSettings["LinkedPaymentBatch-EMailFrom"].ToString();
                        EmailRequest.EmailConfigID = Convert.ToInt32(ConfigurationManager.AppSettings["Rev-LinkedPmtsBatchEmailConfigId"].ToString());
                        EmailInformation.Signature = ConfigurationManager.AppSettings["FinancialEmailSignatiure"].ToString();
                        EmailInformation.IsDataTableBodyRequired = true;
                        break;
                    case 19:
                        EmailInformation.EmailFrom = ConfigurationManager.AppSettings["FailedPrepayment-EMailFrom"].ToString();
                        EmailRequest.EmailConfigID = Convert.ToInt32(ConfigurationManager.AppSettings["Rev-FailPPDBatchEmailConfigId"].ToString());
                        EmailInformation.Signature = ConfigurationManager.AppSettings["FinancialEmailSignatiure"].ToString();
                        EmailInformation.IsDataTableBodyRequired = true;
                        break;
                    default:
                        break;
                }

                EmailRequest.EmailInformation = EmailInformation;
                request.EmailRequest = EmailRequest;
            }
            
            return DoExecute<SalesException>(() =>
            {
                var result = salesService.Process(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
    }
}