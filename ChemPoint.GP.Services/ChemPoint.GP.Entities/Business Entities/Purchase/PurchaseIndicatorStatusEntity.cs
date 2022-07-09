using ChemPoint.GP.Entities.BaseEntities;
using System;

namespace ChemPoint.GP.Entities.Business_Entities.Purchase
{
    public class PurchaseIndicatorStatusEntity : IModelBase
    {
        public int POIndicatorStatusId { get; set; }

        public string Status { get; set; }
    }
}
