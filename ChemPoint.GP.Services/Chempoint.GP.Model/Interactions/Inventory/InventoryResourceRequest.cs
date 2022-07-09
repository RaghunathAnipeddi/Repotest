using ChemPoint.GP.Entities.BaseEntities;
using ChemPoint.GP.Entities.Business_Entities.Inventory;

namespace Chempoint.GP.Model.Interactions.Inventory
{
    public class InventoryResourceRequest
    {
        public int CompanyID { get; set; }

        public string UserId { get; set; }

        public string ConnectionString { get; set; }

        public InventoryResourceEntity InventoryResourceEntity { get; set; }

        public InventoryInformation InventoryBase { get; set; }
    }
}
