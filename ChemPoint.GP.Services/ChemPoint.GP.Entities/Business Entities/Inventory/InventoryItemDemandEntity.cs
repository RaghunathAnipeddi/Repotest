using ChemPoint.GP.Entities.BaseEntities;
using System;

namespace ChemPoint.GP.Entities.Business_Entities.Inventory
{
    public class InventoryItemDemandEntity : IModelBase
    {
        public int ItemDemandIndicatorId { get; set; }

        public string DemandIndicator { get; set; }
    }
}
