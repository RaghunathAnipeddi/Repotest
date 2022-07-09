using Chempoint.GP.Model.Interactions.Email;
using ChemPoint.GP.ApiServices.Controllers.Sales;
using ChemPoint.GP.BusinessContracts.Email;
using ChemPoint.GP.Email;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Chempoint.GP.Infrastructure.Extensions;
using Chempoint.GP.Infrastructure.Exceptions;
using ChemPoint.GP.APIServices.Utils;
using ChemPoint.GP.Entities.BaseEntities;

namespace ChemPoint.GP.APIServices.Controllers.Email
{
    public class SendEmailController : BaseApiController
    {
        private ISendEmail sentEmailService;

        public SendEmailController()
        {
            sentEmailService = new EmailBusiness();
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult SendEmail(SendEmailRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Send Email Request is Null"));

            request.ServiceFileName = ConfigurationManager.AppSettings["ServiceFileName"].ToString();
            request.LoggingPath = ConfigurationManager.AppSettings["LoggingPath"].ToString();
            request.LogFileName = ConfigurationManager.AppSettings["LogFileName"].ToString();
            request.FilePath = ConfigurationManager.AppSettings["FilePath"].ToString();


            return DoExecute<SalesException>(() =>
            {
                var result = sentEmailService.SendEmail(request);
                if (result.IsValid() && result.IsMailSent)
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
    }
}
