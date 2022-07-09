using Chempoint.GP.Infrastructure.Exceptions;
using Chempoint.GP.Infrastructure.Extensions;
using Chempoint.GP.InventoryBL;
using Chempoint.GP.Model.Interactions.Inventory;
using ChemPoint.GP.ApiServices.Controllers.Sales;
using ChemPoint.GP.APIServices.Utils;
using ChemPoint.GP.BusinessContracts.Inventory;
using System;
using System.Configuration;
using System.Web.Http;

namespace ChemPoint.GP.ApiServices.Controllers.Inventory
{
    public class InventoryController : BaseApiController
    {
        private IInventoryBusiness itemResourceService;

        public InventoryController()
        {
            itemResourceService = new InventoryBusiness();
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetItemresourceDetail(InventoryResourceRequest request)
        {
            if (request.IsInValid())
                return InternalServerError(new Exception("Demand Indicator Fetch Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;            

            return DoExecute<SalesException>(() =>
            {
                var result = itemResourceService.GetItemResourceDetail(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult SaveItemResourceDetail(InventoryResourceRequest request)
        { 
            if (request.IsInValid())
                return InternalServerError(new Exception("Demand Indicator Save Request is null"));

            request.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;            

            return DoExecute<SalesException>(() =>
            {
                var result = itemResourceService.SaveItemResourceDetail(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }
    }
}