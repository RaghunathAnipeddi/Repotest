using ChemPoint.GP.Entities.Business_Entities.Inventory;
using System.Collections.Generic;
using System.Data;

namespace Chempoint.GP.Model.Interactions.Inventory
{
    public enum ResponseStatus
    {
        Success = 0,
        Error = 1,
        Unknown = 2,
        Custom = 3
    }

    public class InventoryResourceResponse
    {
        public ResponseStatus Status { get; set; }

        public string ErrorMessage { get; set; }

        public InventoryResourceEntity InventoryResourceEntity { get; set; }

        public List<InventoryItemDemandEntity> ItemDemandIndicatorList { get; set; }
    }
}
