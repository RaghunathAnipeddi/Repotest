using ChemPoint.GP.Entities.BaseEntities;
using ChemPoint.GP.Entities.Business_Entities.Purchase;

namespace Chempoint.GP.Model.Interactions.Purchases
{
    public class PurchaseIndicatorRequest
    {
        public int CompanyID { get; set; }

        public string UserId { get; set; }

        public string ConnectionString { get; set; }

        public PurchaseOrderInformation PoIndicatorBase { get; set; }

        public PurchaseIndicatorEntity PurchaseIndicatorEntity { get; set; }
    }
}
