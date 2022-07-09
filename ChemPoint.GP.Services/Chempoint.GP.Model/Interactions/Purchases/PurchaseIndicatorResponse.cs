using ChemPoint.GP.Entities.Business_Entities.Purchase;

namespace Chempoint.GP.Model.Interactions.Purchases
{
    public enum ResponseStatus
    {
        Success = 0,
        Error = 1,
        Unknown = 2,
        Custom = 3
    }

    public class PurchaseIndicatorResponse
    {
        public ResponseStatus Status { get; set; }

        public string ErrorMessage { get; set; }

        public PurchaseIndicatorEntity PurchaseIndicatorList { get; set; }
    }
}
