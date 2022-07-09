using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.Business_Entities.Inventory;
using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Inventory
{
    public class ItemResourceMap : BaseMap<InventoryResourceEntity>
    {
        public override InventoryResourceEntity Map(IDataRecord dr)
        {
            var inventoryResourceDetail = new InventoryResourceEntity();
            inventoryResourceDetail.ItemNumber = dr["ItemNumber"].ToString();
            inventoryResourceDetail.WarehouseId = dr["WarehouseId"].ToString();
            int result;
            int.TryParse(dr["ItemDemandIndicatorId"].ToString(), out result);
            inventoryResourceDetail.ItemDemandIndicatorId = result;
            return inventoryResourceDetail;
        }
    }
}

