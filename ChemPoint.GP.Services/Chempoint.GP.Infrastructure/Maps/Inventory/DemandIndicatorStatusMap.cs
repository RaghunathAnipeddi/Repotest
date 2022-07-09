using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.Business_Entities.Inventory;
using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Inventory
{
    public class DemandIndicatorStatusMap : BaseDataTableMap<InventoryItemDemandEntity>
    {
        public override InventoryItemDemandEntity Map(DataRow dr)
        {
            var poIndicatorDetail = new InventoryItemDemandEntity();

            int result;
            int.TryParse(dr["ItemDemandIndicatorId"].ToString(), out result);
            poIndicatorDetail.ItemDemandIndicatorId = result;
            poIndicatorDetail.DemandIndicator = dr["DemandIndicator"].ToString();

            return poIndicatorDetail;
        }
    }
}
