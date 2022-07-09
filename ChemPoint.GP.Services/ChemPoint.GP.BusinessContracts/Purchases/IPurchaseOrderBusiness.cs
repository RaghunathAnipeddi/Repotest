using Chempoint.GP.Model.Interactions.PayableManagement;
using Chempoint.GP.Model.Interactions.Purchases;


namespace ChemPoint.GP.BusinessContracts.Purchase
{
    public interface IPurchaseOrderBusiness
    {
        PurchaseIndicatorResponse GetPoIndicatorDetail(PurchaseIndicatorRequest poIndicatorRequest);

        PurchaseIndicatorResponse SavePoIndicatorDetail(PurchaseIndicatorRequest poIndicatorRequest);

        PurchaseIndicatorResponse DeletePoIndicatorDetail(PurchaseIndicatorRequest poIndicatorRequest);

        #region POCostMgt

        PurchaseOrderResponse FetchCostBookModifiedDetails(PurchaseOrderRequest objPurchaseOrderRequest);

        PurchaseOrderResponse ValidatePoCostChanges(PurchaseOrderRequest purchaseOrderRequest);

        PurchaseOrderResponse SavePoCostManagementChangestoAudit(PurchaseOrderRequest purchaseOrderRequest);

        PurchaseOrderResponse UpdatePoCostNotes(PurchaseOrderRequest purchaseOrderRequest);

        PurchaseOrderResponse UpdateHasCostVariance(PurchaseOrderRequest purchaseOrderRequest);

        PurchaseOrderResponse SavePoUnitCostDetails(PurchaseOrderRequest purchaseOrderRequest);

        PurchaseOrderResponse UpdatePoCostProactiveMailStatus(PurchaseOrderRequest purchaseOrderRequest);

        #endregion

        #region MaterialMgt

        PurchaseOrderResponse ValidatePoForMailApproval(PurchaseOrderRequest PurchaseOrderRequest);
        PurchaseOrderResponse SendMailForMaterialManagement(PurchaseOrderRequest PurchaseOrderRequest);

        #endregion

        PurchaseOrderResponse CreateActivity(PurchaseOrderRequest poActivityRequest);

        #region LandedCost

        PurchaseOrderResponse SaveEstimatedShipmentCostEntry(PurchaseOrderRequest purchaseOrderRequest);

        PurchaseOrderResponse GetShipmentEstimateDetails(PurchaseOrderRequest purchaseOrderRequest);

        PurchaseOrderResponse GetShipmentEstimateInquiryDetails(PurchaseOrderRequest purchaseOrderRequest);

        PurchaseOrderResponse GetPOShipmentEstimateDetails(PurchaseOrderRequest purchaseOrderRequest);

        PurchaseOrderResponse DeleteEstimateLineDetails(PurchaseOrderRequest purchaseOrderRequest);

        PurchaseOrderResponse GetEstimateId(PurchaseOrderRequest purchaseOrderRequest);

        PurchaseOrderResponse DeleteEstimatedId(PurchaseOrderRequest purchaseOrderRequest);

        PurchaseOrderResponse GetNextEstimateId(PurchaseOrderRequest purchaseOrderRequest);

        PurchaseOrderResponse GetShipmentQtyTotal(PurchaseOrderRequest purchaseOrderRequest);

        PurchaseOrderResponse GetPoNumber(PurchaseOrderRequest purchaseOrderRequest);

        PurchaseOrderResponse ValidateCarrierReference(PurchaseOrderRequest purchaseOrderRequest);

        PurchaseOrderResponse FetchEstimatedShipmentDetails(PurchaseOrderRequest purchaseOrderRequest);

        PurchaseOrderResponse GetCurrencyIndex(PurchaseOrderRequest purchaseOrderRequest);
        #endregion

        #region Purchase Order | default BuyerID to improve user experience

        PurchaseOrderResponse ValidatePurchaseBuyerId(PurchaseOrderRequest purchaseOrderRequest);

        #endregion

        #region Elemica

        PurchaseElemicaResponse RetrieveElemicaDetail(PurchaseElemicaRequest poElemicaRequest);

        PurchaseElemicaResponse SendElemicaDetail(PurchaseElemicaRequest poElemicaRequest);

        PurchaseElemicaResponse UpdatePOStatusForElemica(PurchaseElemicaRequest poElemicaRequest);

        #endregion

        

    }
}
