using Chempoint.GP.Model.Interactions.Inventory;
using ChemPoint.GP.DataContracts.Base;
using ChemPoint.GP.Entities.BaseEntities;
using ChemPoint.GP.Entities.Business_Entities.Inventory;
using System.Collections.Generic;

namespace ChemPoint.GP.DataContracts.Inventory
{
    public interface IInventoryRepository : IRepository
    {
        IEnumerable<InventoryResourceEntity> GetItemResourceDetail(InventoryInformation inventoryBase, int companyId);

        object SaveItemResourceDetail(InventoryResourceRequest inventoryResourceRequest, int companyId);

        IEnumerable<InventoryItemDemandEntity> GetItemDemandDetail(int companyId);
    }
}
