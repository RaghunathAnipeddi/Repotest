using System.Collections.Generic;
using Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider.Context;
using ChemPoint.GP.DataContracts.Inventory;
using Chempoint.GP.Infrastructure.Utils;
using ChemPoint.GP.Entities.Business_Entities.Inventory;
using Chempoint.GP.Infrastructure.Config;
using System.Data;
using Chempoint.GP.Infrastructure.Maps.Inventory;
using ChemPoint.GP.Entities.BaseEntities;
using Chempoint.GP.Model.Interactions.Inventory;

namespace Chempoint.GP.InventoryDL
{
    public class InventoryDL : RepositoryBase, IInventoryRepository
    {
        public InventoryDL(string connectionString) : base(new GpAddInDbContext(connectionString))
        {
        }

        public IEnumerable<InventoryResourceEntity> GetItemResourceDetail(InventoryInformation inventoryBase, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetItemDetailResourceDetailNA : Configuration.SPGetItemDetailResourceDetailEU);
            cmd.Parameters.AddInputParams(Configuration.SPGetItemDetailResourceDetailParm1, SqlDbType.VarChar, inventoryBase.ItemNumber, 31);
            cmd.Parameters.AddInputParams(Configuration.SPGetItemDetailResourceDetailParm2, SqlDbType.VarChar, inventoryBase.WarehouseId, 21);
            return base.FindAll<InventoryResourceEntity, ItemResourceMap>(cmd);
        }

        public IEnumerable<InventoryItemDemandEntity> GetItemDemandDetail(int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPFetchDemandIndicatorStatusNA : Configuration.SPFetchDemandIndicatorStatusEU);
            var ds = base.GetDataSet(cmd);
            return base.GetAllEntities<InventoryItemDemandEntity, DemandIndicatorStatusMap>(ds.Tables[0]);
        }

        public object SaveItemResourceDetail(InventoryResourceRequest inventoryResourceRequest, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPSaveItemDetailResourceDetailNA : Configuration.SPSaveItemDetailResourceDetailEU);
            cmd.Parameters.AddInputParams(Configuration.SPSaveItemDetailResourceDetailParm1, SqlDbType.VarChar, inventoryResourceRequest.UserId, 50);
            cmd.Parameters.AddInputParams(Configuration.SPSaveItemDetailResourceDetailParm2, SqlDbType.VarChar, inventoryResourceRequest.CompanyID, 21);
            cmd.Parameters.AddInputParams(Configuration.SPSaveItemDetailResourceDetailParm3, SqlDbType.VarChar, inventoryResourceRequest.InventoryResourceEntity.ItemNumber, 31);
            cmd.Parameters.AddInputParams(Configuration.SPSaveItemDetailResourceDetailParm4, SqlDbType.VarChar, inventoryResourceRequest.InventoryResourceEntity.WarehouseId, 21);
            cmd.Parameters.AddInputParams(Configuration.SPSaveItemDetailResourceDetailParm5, SqlDbType.VarChar, inventoryResourceRequest.InventoryResourceEntity.ItemDemandIndicatorId, 21);
            return base.Insert(cmd);
        }
    }
}
