using ChemPoint.GP.Entities.BaseEntities;
using System;

namespace ChemPoint.GP.Entities.Business_Entities.Inventory
{
    public class InventoryResourceEntity : IModelBase
    {
        public string ItemNumber { get; set; }

        public string WarehouseId { get; set; }

        public int ItemDemandIndicatorId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public int ModifiedBy { get; set; }

        public DateTime ModifiedOn { get; set; }
    }
}
