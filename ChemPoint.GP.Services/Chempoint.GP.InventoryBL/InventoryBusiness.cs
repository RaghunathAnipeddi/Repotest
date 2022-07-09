using Chempoint.GP.Model.Interactions.Inventory;
using ChemPoint.GP.BusinessContracts.Inventory;
using ChemPoint.GP.DataContracts.Inventory;
using Chempoint.GP.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using ChemPoint.GP.Entities.Business_Entities.Inventory;
using System.Data;
using System.Reflection;

namespace Chempoint.GP.InventoryBL
{
    public class InventoryBusiness : IInventoryBusiness
    {
        public InventoryBusiness()
        {
        }

        public InventoryResourceResponse GetItemResourceDetail(InventoryResourceRequest inventoryResourceRequest)
        {
            InventoryResourceResponse inventoryResourceResponse = null;
            IInventoryRepository inventoryDataAccess = null;

            inventoryResourceRequest.ThrowIfNull("Purchase Indicator");
            inventoryResourceRequest.InventoryBase.ThrowIfNull("inventoryResourceRequest.InventoryResourceEntity");
            try
            {
                inventoryResourceResponse = new InventoryResourceResponse();
                IEnumerable<InventoryItemDemandEntity> demandIndicatorStatusList;
                inventoryDataAccess = new Chempoint.GP.InventoryDL.InventoryDL(inventoryResourceRequest.ConnectionString);
                var inventoryResourceDetailList = inventoryDataAccess.GetItemResourceDetail(inventoryResourceRequest.InventoryBase, inventoryResourceRequest.CompanyID);
                demandIndicatorStatusList = inventoryDataAccess.GetItemDemandDetail(inventoryResourceRequest.CompanyID);
                inventoryResourceResponse.InventoryResourceEntity = inventoryResourceDetailList.ToList().FirstOrDefault();
                inventoryResourceResponse.ItemDemandIndicatorList = demandIndicatorStatusList.ToList();
                inventoryResourceResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                inventoryResourceResponse.Status = ResponseStatus.Error;
                inventoryResourceResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return inventoryResourceResponse;
        }

        /// <summary>
        /// Item resource Planning window (demand indicator) business layer calling DAL...
        /// </summary>
        /// <param name="inventoryResourceRequest"></param>
        /// <returns></returns>
        public InventoryResourceResponse SaveItemResourceDetail(InventoryResourceRequest inventoryResourceRequest)
        {
            InventoryResourceResponse inventoryResourceResponse = null;
            IInventoryRepository inventoryDataAccess = null;

            inventoryResourceRequest.ThrowIfNull("SalesItemDetailEntry");
            inventoryResourceRequest.InventoryResourceEntity.ThrowIfNull("inventoryResourceRequest.InventoryResourceEntity");
            try
            {
                inventoryResourceResponse = new InventoryResourceResponse();
                inventoryDataAccess = new Chempoint.GP.InventoryDL.InventoryDL(inventoryResourceRequest.ConnectionString);
                inventoryDataAccess.SaveItemResourceDetail(inventoryResourceRequest, inventoryResourceRequest.CompanyID);
                inventoryResourceResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                inventoryResourceResponse.Status = ResponseStatus.Error;
                inventoryResourceResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return inventoryResourceResponse;
        }
    }
}
