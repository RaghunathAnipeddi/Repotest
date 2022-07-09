using Chempoint.GP.Model.Interactions.Inventory;

namespace ChemPoint.GP.BusinessContracts.Inventory
{
    public interface IInventoryBusiness
    {
        InventoryResourceResponse SaveItemResourceDetail(InventoryResourceRequest inventoryResourceRequest);

        InventoryResourceResponse GetItemResourceDetail(InventoryResourceRequest inventoryResourceRequest);
    }
}
