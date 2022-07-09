using Chempoint.GP.Model.Interactions.PayableManagement;
using Chempoint.GP.Model.Interactions.Purchases;
using ChemPoint.GP.DataContracts.Base;
using ChemPoint.GP.Entities.BaseEntities;
using ChemPoint.GP.Entities.Business_Entities.Purchase;
using System;
using System.Collections.Generic;
using System.Data;

namespace ChemPoint.GP.DataContracts.Purchase
{
    public interface IPurchaseOrderRepository : IRepository
    {
        IEnumerable<PurchaseIndicatorEntity> GetPoIndicatorDetail(PurchaseOrderInformation poIndicator, int companyId);

        object SavePoIndicatorDetail(PurchaseIndicatorRequest poIndicator, int companyId);

        object DeletePoIndicatorDetail(PurchaseOrderInformation poIndicator, int companyId);

        #region PCostMgt

        void UpdatePoCostNotes(decimal noteIndex, string costNotes, int companyId);

        void SavePoCostManagementChangestoAudit(DataTable savePOUnitCostDetails, int companyId);

        DataSet ValidatePoCostChanges(DataTable costMgntDetails, int curcnyView, int companyId);

        DataSet FetchCostBookModifiedDetails(int intCompanyId);

        void UpdateHasCostVariance(DataTable hasCostVarianceDT, int intCompanyId);

        void SavePoUnitCostDetails(DataTable savePoUnitCostDT, string UserId, int intCompanyId);

        void UpdatePoCostProactiveMailStatus(DataTable dtUpdatePoCost,int intCompanyId);

        #endregion POSupport

        #region MaterialMgt

        PurchaseOrderResponse ValidatePoForMailApproval(string poNumber,string vendorId, string userId, int companyId);


        #endregion

        PurchaseOrderResponse CreateActivity(PurchaseOrderRequest poActivityRequest);

        PurchaseOrderResponse LogActivity(PurchaseOrderRequest poActivityRequest);

        #region LandedCost

        void SaveEstimatedShipmentCostEntry(PurchaseOrderRequest purchaseOrderRequest, int companyId);

        DataSet GetShipmentEstimateDetails(int intCompanyId, int intEstimateId);

        DataSet GetShipmentEstimateInquiryDetails(int intCompanyId, int intEstimateId);

        DataSet GetPOShipmentEstimateDetails(int intCompanyId, string strPONumber);

        object DeleteEstimateLineDetails(int intCompanyId, string strPONumber, int intPOLineNumber, string userName);

        DataSet GetEstimateId(int intCompanyId, int windowValue);

        object DeleteEstimatedId(int intCompanyId, int intEstimateId, string userName);

        int GetNextEstimateId(int intCompanyId);

        decimal GetShipmentQtyTotal(int intCompanyId, string itemNumber, decimal ShippedQty);

        DataSet GetPoNumber(int intCompanyId, string locationCode, string vendorId);

        int ValidateCarrierReference(int intCompanyId, int estimateId, string CarrierReference);

        DataSet FetchEstimatedShipmentDetails(PurchaseOrderRequest purchaseOrderRequest, int companyId);

        DataSet GetCurrencyIndex(PurchaseOrderRequest purchaseOrderRequest, int companyId);

        #endregion

        #region Purchase Order | default BuyerID to improve user experience

        int ValidatePurchaseBuyerId(string poNumber, int companyId);

        #endregion

        #region Elemica

        DataSet RetrieveElemicaDetail(PurchaseElemicaRequest poElemicaRequest, int companyId);

        object SendElemicaDetail(PurchaseElemicaRequest poElemicaRequest, int companyId);

        object UpdatePOStatusForElemica(PurchaseElemicaRequest poElemicaRequest, int companyId);

        #endregion


    }
}
