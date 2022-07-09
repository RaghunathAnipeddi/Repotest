using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.BusinessContracts.Sales;
using ChemPoint.GP.SalesOrderBL;
using System;
using System.Web.Http;
using Chempoint.GP.Infrastructure.Extensions;
using System.Configuration;
using Chempoint.GP.Infrastructure.Exceptions;
using ChemPoint.GP.APIServices.Utils;

namespace ChemPoint.GP.ApiServices.Controllers.Sales
{
    public class SalesOrderVoidController : BaseApiController
    {
        private ISalesOrderBusiness salesService;

        public SalesOrderVoidController()
        {
            salesService = new SalesOrderBusiness();
        }

        /// <summary>
        /// Get SalesOrderVoid service method...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SalesOrderVoid(SalesOrderRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("SalesOrderVoid  is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;
            
            request.XrmServiceUrl = ConfigurationManager.AppSettings["XrmServiceURL"].ToString();

            return DoExecute<SalesException>(() =>
            {
                var result = salesService.SalesOrderVoid(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
    }
}
