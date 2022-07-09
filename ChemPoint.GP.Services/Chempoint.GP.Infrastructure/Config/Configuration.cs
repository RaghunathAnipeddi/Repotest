using System;
using System.Linq;

namespace Chempoint.GP.Infrastructure.Config
{
    public class Configuration
    {
        #region ServiceSKUExpeditedCharges
        public const string ExcludeForServiceSKU = "8000002,8000007,9000002,9000007";
        #endregion ServiceSKUExpeditedCharges

        #region SalesModule

        #region SalesItemDetailEntry
        //Fetch
        public const string SPFetchItemEntryDetailsNA = "GPCustomizations.chmpt.FetchSalesLineItemDetails";
        public const string SPFetchItemEntryDetailsEU = "GPCustomizations.cpeur.FetchSalesLineItemDetails";
        public const string SPFetchItemEntryDetailsParam1 = "SopType";
        public const string SPFetchItemEntryDetailsParam2 = "SopNumber";
        public const string SPFetchItemEntryDetailsParam3 = "LineItemSequence";
        public const string SPFetchItemEntryDetailsParam4 = "ComponentLineSeqNumber";
        //Save
        public const string SPSaveItemEntryDetailsNA = "GPCustomizations.chmpt.SaveSalesLineItemDetails";
        public const string SPSaveItemEntryDetailsEU = "GPCustomizations.cpeur.SaveSalesLineItemDetails";
        public const string SPSaveItemEntryDetailsParam1 = "SaveSalesLineItemDetailsType";
        public const string SPSaveItemEntryDetailsParam2 = "TotalFrightAmount";
        //Delete
        public const string SPDeleteItemEntryDetailsNA = "GPCustomizations.chmpt.DeleteSalesLineItemDetails";
        public const string SPDeleteItemEntryDetailsEU = "GPCustomizations.cpeur.DeleteSalesLineItemDetails";
        public const string SPDeleteItemEntryDetailsParam1 = "SopType";
        public const string SPDeleteItemEntryDetailsParam2 = "SopNumber";
        public const string SPDeleteItemEntryDetailsParam3 = "LineItemSequence";
        public const string SPDeleteItemEntryDetailsParam4 = "ComponentLineSeqNumber";
        #endregion SalesItemDetailEntry

        #region SOPEntry
        //Fetch
        public const string SPFetchSalesOrderNA = "GPCustomizations.chmpt.FetchSalesOrderDetails";
        public const string SPFetchSalesOrderEU = "GPCustomizations.cpeur.FetchSalesOrderDetails";
        public const string SPFetchSopEntryDetailsParam1 = "SopType";
        public const string SPFetchSopEntryDetailsParam2 = "SopNumber";
        public const string SPFetchSopEntryDetailsParam3 = "CustomerNumber";
        //Save
        public const string SPSaveSopEntryDetailsNA = "GPCustomizations.chmpt.SaveSalesHeaderDetails";
        public const string SPSaveSopEntryDetailsEU = "GPCustomizations.cpeur.SaveSalesHeaderDetails";
        public const string SPSaveSopEntryDetailsParam1 = "SopType";
        public const string SPSaveSopEntryDetailsParam2 = "SopNumber";
        public const string SPSaveSopEntryDetailsParam3 = "ToBeCancelled";


        public const string SPSaveSopEntryDetailsParam4 = "IsExcludeServiceSkus";
        public const string SPSaveSopEntryDetailsParam5 = "ScheduledDatesType";
        public const string SPSaveSopEntryDetailsParam6 = "TransportationType";

        //Save ShipTo/BillTo Address ID
        public const string SPSaveTransactionAddressCodesNA = "GPCustomizations.chmpt.SaveSopTransactionAddressCodes";
        public const string SPSaveTransactionAddressCodesEU = "GPCustomizations.cpeur.SaveSopTransactionAddressCodes";
        public const string SPSaveTransactionAddressCodesParam1 = "SopTransactionAddressCodesType";
        //Save HeaderComments..
        public const string SPSaveSopHeaderInstructionDetailsNA = "GPCustomizations.Chmpt.SaveSopHeaderInstructionDetails";
        public const string SPSaveSopHeaderInstructionDetailsEU = "GPCustomizations.Cpeur.SaveSopHeaderInstructionDetails";
        public const string SPSaveSopHeaderInstructionDetailsParam1 = "Sopheaderinstructiondetailstype";
        //Save LineComments..
        public const string SPSaveSopLineInstructionDetailsNA = "GPCustomizations.Chmpt.SaveSopLineInstructionDetails";
        public const string SPSaveSopLineInstructionDetailsEU = "GPCustomizations.Cpeur.SaveSopLineInstructionDetails";
        public const string SPSaveSopLineInstructionDetailsParam1 = "SopLineInstructionDetailsType";
        //AddressId Validation..   
        public const string SPValidateSopTransactionAddressCodesNA = "GPCustomizations.Chmpt.ValidateSopTransactionAddressCodes";
        public const string SPValidateSopTransactionAddressCodesEU = "GPCustomizations.Cpeur.ValidateSopTransactionAddressCodes";
        public const string SPValidateSopTransactionAddressCodesParam1 = "CUSTNMBR";
        //Service SKU Items Validation..   
        public const string SPValidateSopTransactionServiceSkuItemsNA = "GPCustomizations.Chmpt.ValidateServiceSKUItems";
        public const string SPValidateSopTransactionServiceSkuItemsEU = "GPCustomizations.Cpeur.ValidateServiceSKUItems";
        public const string SPValidateSopTransactionServiceSkuItemsParam1 = "SopNumber";
        public const string SPValidateSopTransactionServiceSkuItemsParam2 = "SopType";

        public const string SPValidateSopTransactionServiceSkuItemsParam3 = "IsExcludeServiceSkus";
        public const string SPValidateSopTransactionServiceSkuItemsParam4 = "RequestedShipDate";
        public const string SPValidateSopTransactionServiceSkuItemsParam5 = "SopOrderTypeDetails";
        public const string SPValidateSopTransactionServiceSkuItemsParam6 = "TransportationType";

        // 
        //SOP Line Quote Lookup
        public const string SPFetchSopLineQuoteDetailsNA = "GPCustomizations.Chmpt.FetchSopLineQuoteDetails";
        public const string SPFetchSopLineQuoteDetailsEU = "GPCustomizations.Cpeur.FetchSopLineQuoteDetails";
        public const string SPFetchSopLineQuoteDetailsParam1 = "CustomerNumber";
        public const string SPFetchSopLineQuoteDetailsParam2 = "ItemNumber";
        public const string SPFetchSopLineQuoteDetailsParam3 = "LocationCode";

        //SOP Line Quote Validate
        public const string SPValidateSopLineQuoteDetailsNA = "GPCustomizations.Chmpt.ValidateSopLineQuoteDetails";
        public const string SPValidateSopLineQuoteDetailsEU = "GPCustomizations.Cpeur.ValidateSopLineQuoteDetails";
        public const string SPValidateSopLineQuoteDetailsParam1 = "SopType";
        public const string SPValidateSopLineQuoteDetailsParam2 = "CustomerNumber";
        public const string SPValidateSopLineQuoteDetailsParam3 = "QuoteNumber";
        public const string SPValidateSopLineQuoteDetailsParam4 = "ItemNumber";

        public const string SPFetchIncoTerm = "gpcustomizations.Common.GetIncoTermDetails";
        public const string SPFetchIncoTermParam1 = "Company";

        public const string SPFetchCountryDetails = "Cpeur.dbo.CPGetCountryDetails";
        public const string SPFetchCountryDetailsParam1 = "SopNumbe";
        public const string SPFetchCountryDetailsParam2 = "SopType";
        public const string SPFetchCountryDetailsParam3 = "CCode";
        public const string SPFetchCountryDetailsParam4 = "County";

        public const string SPGetSopCurSchedDelDateNA = "GPCustomizations.Chmpt.CPGetSOPCurSchedDelDate";
        public const string SPGetSopCurSchedDelDateEU = "GPCustomizations.Cpeur.CPGetSOPCurSchedDelDate";
        public const string SPGetSopCurSchedDelDateParam1 = "ReqShipDate";
        public const string SPGetSopCurSchedDelDateParam2 = "CustReqShip";
        public const string SPGetSopCurSchedDelDateParam3 = "CustReqDel";
        public const string SPGetSopCurSchedDelDateParam4 = "CalcDate";

        public const string SPGetSopCurSchedShipDateNA = "GPCustomizations.chmpt.CPGetSOPCurSchedShipDate";
        public const string SPGetSopCurSchedShipDateEU = "GPCustomizations.cpeur.CPGetSOPCurSchedShipDate";
        public const string SPGetSopCurSchedShipDateParam1 = "SopType";
        public const string SPGetSopCurSchedShipDateParam2 = "SopNumber";

        public const string SPUpdateTaxScheduleIdToLine = "GPCustomizations.cpeur.UpdateTaxScheduleIdToLine";
        public const string SPUpdateTaxScheduleIdToLineParam1 = "SopType";
        public const string SPUpdateTaxScheduleIdToLineParam2 = "SopNumber";
        public const string SPUpdateTaxScheduleIdToLineParam3 = "TaxScheduleID";

        //public const string SPUpdatePrintPickTicketStatusNA = "GPCustomizations.Chmpt.UpdateSopPickTicketStatus";
        //public const string SPUpdatePrintPickTicketStatusEU = "GPCustomizations.Cpeur.UpdateSopPickTicketStatus";
        //public const string SopNumberParam = "@SopNumber";
        //public const string SopTypeParam = "@SopType";


        public const string SPMoveVoidRecordToHistoryNA = "GPCustomizations.Chmpt.MoveVoidRecordToHistory";
        public const string SPMoveVoidRecordToHistoryEU = "GPCustomizations.Cpeur.MoveVoidRecordToHistory";
        public const string SPMoveVoidRecordToHistoryParam1 = "SopNumber";

        public const string SPGetServiceSKULineItemNA = "GpCustomizations.Chmpt.Validateserviceskuitemsorderpush";
        public const string SPGetServiceSKULineItemEU = "GpCustomizations.Cpeur.Validateserviceskuitemsorderpush";
        public const string SPGetServiceSKULineItemParam1 = "SopNumber";
        public const string SPGetServiceSKULineItemParam2 = "SopType";
        public const string SPGetServiceSKULineItemParam3 = "SopOrderTypeDetails";
        public const string SPGetServiceSKULineItemParam4 = "ServiceSKUVAlidateInOrderPush";


        public const string SPSaveAllocatedQtyDetailsNA = "GPCustomizations.chmpt.SaveAllocatedQty";
        public const string SPSaveAllocatedQtyDetailsEU = "GPCustomizations.cpeur.SaveAllocatedQty";
        public const string SPSaveAllocatedQtyDetailsTableType = "SopOrderQtyAllocatedTypeDetails";

        public const string SPFetchSopChangedQtyNA = "GPCustomizations.chmpt.FetchSalesOrderLineForTermHold";
        public const string SPFetchSopChangedQtyEU = "GPCustomizations.cpeur.FetchSalesOrderLineForTermHold";
        public const string SPFetchSopChangedQtyParam1 = "SalesLinesTypeDetails";
        public const string SPFetchSopChangedQtyParam2 = "Remainingbalance";

        //Fetch
        public const string SPAuditCustomizeServiceSkUNA = "GPCustomizations.chmpt.SaveCustomizedServiceSKUAudit";
        public const string SPAuditCustomizeServiceSkUEU = "GPCustomizations.cpeur.SaveCustomizedServiceSKUAudit";
        public const string SPAuditCustomizeServiceSkUParam1 = "Sopnumber";
        public const string SPAuditCustomizeServiceSkUParam2 = "Soptype";
        public const string SPAuditCustomizeServiceSkUParam3 = "IsCustomizedServiceSKU";
        public const string SPAuditCustomizeServiceSkUParam4 = "UserName";
        #endregion SOPEntry

        #region OrderType

        public const string SPSaveOrderTypeDetailsNA = "GPCustomizations.chmpt.SaveOrderTypeDetails";
        public const string SPSaveOrderTypeDetailsEU = "GPCustomizations.cpeur.SaveOrderTypeDetails";
        public const string SPSaveOrderTypeDetailsOrderType = "SopOrderTypeDetails";

        //public const string SPGetOrderTypeDetailsNA = "GPCustomizations.chmpt.FetchOrderTypeDetails";
        //public const string SPGetOrderTypeDetailsNA_Param1 = "SopType";
        //public const string SPGetOrderTypeDetailsNA_Param2 = "SopNumbe";
        #endregion OrderType

        #region ThirdPartyAddress
        //Save ThirdParty Address 
        public const string SPSaveThirdPartyAddressDetailsNA = "GPCustomizations.chmpt.SaveThirdPartyAddressDetails";
        public const string SPSaveThirdPartyAddressDetailsEU = "GPCustomizations.cpeur.SaveThirdPartyAddressDetails";
        public const string SPSaveThirdPartyAddressDetailsParam1 = "SaveSopThirdPartyAddressDetailsType";
        //Save Customer Detail Address...
        public const string SPSaveCustomerPickupAddressDetailsNA = "GPCustomizations.chmpt.SaveCustomerPickupAddressDetails";
        public const string SPSaveCustomerPickupAddressDetailsEU = "GPCustomizations.cpeur.SaveCustomerPickupAddressDetails";
        public const string SPSaveCustomerPickupAddressDetailsParam1 = "SaveCustomerPickupAddressDetailsType";


        //public const string SPSaveThirdPartyAddressDetailsParam1 = "Cpeur.SopThirdPartyAddressDetailsType";
        #endregion ThirdPartyAddress

        #region CommentInstructionDetail
        public const string SPFetchSopHeaderInstructionDetailsNA = "Chmpt.xrm.FetchSopHeaderInstructionDetails";
        public const string SPFetchSopHeaderInstructionDetailsEU = "cpeur.xrm.FetchSopHeaderInstructionDetails";
        public const string SPFetchSopHeaderInstructionDetailsParam1 = "SOPTYPE";
        public const string SPFetchSopHeaderInstructionDetailsParam2 = "SOPNUMBE";

        public const string SPFetchSopLineInstructionDetailsNA = "Chmpt.xrm.FetchSopLineInstructionDetails";
        public const string SPFetchSopLineInstructionDetailsEU = "cpeur.xrm.FetchSopLineInstructionDetails";
        public const string SPFetchSopLineInstructionDetailsParam1 = "SOPTYPE";
        public const string SPFetchSopLineInstructionDetailsParam2 = "SOPNUMBE";
        public const string SPFetchSopLineInstructionDetailsParam3 = "ITEMNMBR";
        public const string SPFetchSopLineInstructionDetailsParam4 = "LNITMSEQ";

        #endregion CommentInstructionDetail

        #region WarehouseClosure

        public const string SPValidateWarehouseClosureDateNA = "Chmpt.ValidateSalesWarehouseClosureDate";
        public const string SPValidateWarehouseClosureDateEU = "Cpeur.ValidateSalesWarehouseClosureDate";
        public const string SPValidateWarehouseClosureDateWarehouseId = "WarehouseId";
        public const string SPValidateWarehouseClosureDateCurSchedShip = "CurSchedShip";
        public const string SPValidateWarehouseClosureDateCurSchedDel = "CurSchedDel";
        public const string SPValidateWarehouseClosureDateValidateStatus = "ValidateStatus";

        #endregion

        #endregion SalesModule

        #region InventoryModule

        #region ItemResourcePlanning
        //Save
        public const string SPSaveItemDetailResourceDetailNA = "Chmpt.SaveItemIndicatorDetail";
        public const string SPSaveItemDetailResourceDetailEU = "Cpeur.SaveItemIndicatorDetail";
        public const string SPSaveItemDetailResourceDetailParm1 = "UserName";
        public const string SPSaveItemDetailResourceDetailParm2 = "CompanyId";
        public const string SPSaveItemDetailResourceDetailParm3 = "Itemnumber";
        public const string SPSaveItemDetailResourceDetailParm4 = "WarehouseId";
        public const string SPSaveItemDetailResourceDetailParm5 = "ItemDemandIndicatorId";

        //Fetch
        public const string SPGetItemDetailResourceDetailNA = "Chmpt.GetItemIndicatorDetail";
        public const string SPGetItemDetailResourceDetailEU = "Cpeur.GetItemIndicatorDetail";
        public const string SPGetItemDetailResourceDetailParm1 = "Itemnumber";
        public const string SPGetItemDetailResourceDetailParm2 = "WarehouseId";
        public const string SPFetchDemandIndicatorStatusNA = "Chmpt.GetDemandIndicatorDropList";
        public const string SPFetchDemandIndicatorStatusEU = "Cpeur.GetDemandIndicatorDropList";
        #endregion ItemResourcePlanning

        #endregion Inventorymodule

        #region PurchaseModule

        #region POIndicator
        //Fetch
        public const string SPFetchPOIndicatorDetailsNA = "Chmpt.GetPOIndicatorDetail";
        public const string SPFetchPOIndicatorDetailsEU = "Cpeur.GetPOIndicatorDetail";
        public const string SPFetchPOIndicatorDetailsParam1 = "PONumber";
        public const string SPFetchPOIndicatorDetailsParam2 = "POLineNumber";
        //public const string SPFetchPOIndicatorStatusNA = "Chmpt.GetPOIndicatorDropList";
        //public const string SPFetchPOIndicatorStatusEU = "Cpeur.GetPOIndicatorDropList";

        //Save
        public const string SPSavePOIndicatorDetailsNA = "Chmpt.SavePOIndicatorDetail";
        public const string SPSavePOIndicatorDetailsEU = "Cpeur.SavePOIndicatorDetail";
        public const string SPSavePOIndicatorDetailsParam1 = "UserName";
        public const string SPSavePOIndicatorDetailsParam2 = "CompanyId";
        public const string SPSavePOIndicatorDetailsParam3 = "POndicatorDetails";

        //Delete
        public const string SPDeletePOIndicatorDetailsNA = "Chmpt.Deletepoindicatordetail";
        public const string SPDeletePOIndicatorDetailsEU = "Cpeur.Deletepoindicatordetail";
        public const string SPDeletePOIndicatorDetailsParam1 = "PONumber";
        public const string SPDeletePOIndicatorDetailsParam2 = "POLinenumber";
        #endregion POIndicator

        #region POCostMgt

        public const string SPInsertPoCostManagementChangestoAuditNA = "chmpt.InsertPoCostManagementChangestoAudit";
        public const string SPInsertPoCostManagementChangestoAuditEU = "cpeur.InsertPoCostManagementChangestoAudit";
        public const string SPInsertPoCostManagementChangestoAuditParam1 = "Company";
        public const string SPInsertPoCostManagementChangestoAuditParam2 = "AuditUnitCostChanges";

        public const string SPValidatePoCostManagementDetailsNA = "chmpt.ValidatePoCostManagementDetails";
        public const string SPValidatePoCostManagementDetailsEU = "cpeur.ValidatePoCostManagementDetails";
        public const string SPValidatePoCostManagementDetailsParam1 = "PoCostManagementDetails";
        public const string SPValidatePoCostManagementDetailsParam2 = "CurcnyView";

        public const string SPUpdatePoCostManagementDetailsNoteNA = "chmpt.UpdatePoCostManagementDetailsNote";
        public const string SPUpdatePoCostManagementDetailsNoteEU = "cpeur.UpdatePoCostManagementDetailsNote";
        public const string SPUpdatePoCostManagementDetailsNoteParam1 = "NoteIndx";
        public const string SPUpdatePoCostManagementDetailsNoteParam2 = "TxtField";

        public const string SPFetchCostBookCostModifiedDetailsNA = "chmpt.FetchSkuUnitCostModifiedDetails";
        public const string SPFetchCostBookCostModifiedDetailsEU = "Cpeur.FetchSkuUnitCostModifiedDetails";

        public const string SPUpdateHasCostVarianceNA = "Chmpt.Updatehascostvariance";
        public const string SPUpdateHasCostVarianceEU = "Cpeur.Updatehascostvariance";
        public const string SPUpdateHasCostVarianceParam1 = "HasCostVarianceType";

        public const string SPSavePoUnitCostDetailsNA = "Chmpt.Savepounitcostdetails";
        public const string SPSavePoUnitCostDetailsEU = "Cpeur.Savepounitcostdetails";
        public const string SPSavePoUnitCostDetailsParam1 = "UserName";
        public const string SPSavePoUnitCostDetailsParam2 = "POCostManagementDetailsType";

        public const string SPUpdatePoCostProactiveMailStatusNA = "Chmpt.UpdatePOCostProactiveMailProcessStatus";
        public const string SPUpdatePoCostProactiveMailStatusEU = "Cpeur.UpdatePOCostProactiveMailProcessStatus";
        public const string SPGetUpdatedPOCostDetailsNA = "GPCustomizations.[Chmpt].[GetUpdatedPOCostDetails]";
        public const string SPGetUpdatedPOCostDetailsEU = "GPCustomizations.[Cpeur].[GetUpdatedPOCostDetails]";
        public const string SPProcessIdsTypeParam = "ProcessIdsType";

        #region PO Activity

        public const string SPNAGetPOActivity = "GPCustomizations.chmpt.GetPOSupplyChainActivityDetails";
        public const string SPEUGetPOActivity = "GPCustomizations.cpeur.GetPOSupplyChainActivityDetails";


        public const string SPUpdatePoActivityLogNA = "Chmpt.UpdatePOSupplyChainActivityLog";
        public const string SPUpdatePoActivityLogEU = "Cpeur.UpdatePOSupplyChainActivityLog";
        public const string UpdatePOActivityLogTypeParam = "UpdatePOActivityLogType";
        #endregion PO Activity


        #endregion

        #region MaterialMgt

        public const string SPValidatePoForMailApprovalNA = "Chmpt.ValidatePoForMailApproval";
        public const string SPValidatePoForMailApprovalEU = "Cpeur.ValidatePoForMailApproval";
        public const string SPValidatePoForMailApprovalParam1 = "PoNumber";               
        public const string SPValidatePoForMailApprovalParam2 = "VendorId";
        public const string SPValidatePoForMailApprovalParam3 = "UserId";
        public const string SPValidatePoForMailApprovalParam4 = "IsFirstPO";
        public const string SPValidatePoForMailApprovalParam5 = "IsValid";


        #endregion

        #region LandedCost

        public const string SPSaveEstimatedShipmentCostEntryNA = "Chmpt.SaveShipmentEstimateDetails";
        public const string SPSaveEstimatedShipmentCostEntryEU = "Chmpt.SaveShipmentEstimateDetails";
        public const string SPSaveEstimatedShipmentCostEntryParam1 = "EstimateID";
        public const string SPSaveEstimatedShipmentCostEntryParam2 = "Warehouse";
        public const string SPSaveEstimatedShipmentCostEntryParam3 = "Vendor";
        public const string SPSaveEstimatedShipmentCostEntryParam4 = "CarrierReference";
        public const string SPSaveEstimatedShipmentCostEntryParam5 = "EstimatedCost";
        public const string SPSaveEstimatedShipmentCostEntryParam6 = "EstimatedShipDate";
        public const string SPSaveEstimatedShipmentCostEntryParam7 = "CurrencyId";
        public const string SPSaveEstimatedShipmentCostEntryParam8 = "ExchangeRate";
        public const string SPSaveEstimatedShipmentCostEntryParam9 = "ExchangeExpirationDate";
        public const string SPSaveEstimatedShipmentCostEntryParam10 = "TotalNetWeight";
        public const string SPSaveEstimatedShipmentCostEntryParam11 = "IsMatchedToReceipt";
        public const string SPSaveEstimatedShipmentCostEntryParam12 = "POReceipt";
        public const string SPSaveEstimatedShipmentCostEntryParam13 = "EstimatedShipmentNotes";
        public const string SPSaveEstimatedShipmentCostEntryParam14 = "UserName";
        public const string SPSaveEstimatedShipmentCostEntryParam15 = "ShipmentEstimateLineType";
        public const string SPSaveEstimatedShipmentCostEntryParam16 = "ExchangeRate";
        public const string SPSaveEstimatedShipmentCostEntryParam17 = "ExchangeExpirationDate";

        public const string SPNADeleteestimatelinedetails = "GPCustomizations.Chmpt.Deleteestimatelinedetails";
        public const string SPNAGetPOShipmentEstimateDetails = "GPCustomizations.Chmpt.GetPOShipmentEstimateDetails";
        public const string SPNAGetShipmentEstimateDetails = "GPCustomizations.Chmpt.GetShipmentEstimateDetails";

        public const string SPEUDeleteestimatelinedetails = "GPCustomizations.Chmpt.Deleteestimatelinedetails";
        public const string SPEUGetPOShipmentEstimateDetails = "GPCustomizations.Chmpt.GetPOShipmentEstimateDetails";
        public const string SPEUGetShipmentEstimateDetails = "GPCustomizations.Chmpt.GetShipmentEstimateDetails";

        public const string SPDeleteestimatelinedetailsParam1 = "PoNumber";
        public const string SPGetPOShipmentEstimateDetailsParam1 = "PoNumber";
        public const string SPDeleteestimatelinedetailsParam2 = "PoLineNumber";
        public const string SPDeleteestimatelinedetailsParam3 = "UserName";
        public const string SPGetShipmentEstimateDetailsParam1 = "EstimateId";

        public const string SPGetEstimateIdNA = "Chmpt.GetEstimateId";
        public const string SPGetEstimateIdEU = "Chmpt.GetEstimateId";
        public const string SPGetEstimateIdParam1 = "WindowValue";

        public const string SPDeleteEstimatedIdNA = "Chmpt.DeleteEstimateDetails";
        public const string SPDeleteEstimatedIdParam1 = "EstimateId";
        public const string SPDeleteEstimatedIdParam2 = "UserName";

        public const string SPNAGetNextEstimateId = "GPCustomizations.common.GetApplicationNextNumber";
        public const string SPEUGetNextEstimateId = "GPCustomizations.common.GetApplicationNextNumber";
        public const string SPNAGetNextEstimateIdParam1 = "CompanyId";
        public const string SPNAGetNextEstimateIdParam2 = "NextEstimateId";

        public const string SPGetShipmentQtyTotal = "Chmpt.GetEstimatedShipWeight";
        public const string SPGetShipmentQtyTotalParam1 = "ItemNumber";
        public const string SPGetShipmentQtyTotalParam2 = "ShippedQty";
        public const string SPGetShipmentQtyTotalParam3 = "TotalNetWeight";

        public const string SPGetPoNumber = "Chmpt.GetPoNumber";
        public const string SPGetPoNumberParam1 = "Location";
        public const string SPGetPoNumberParam2 = "VendorId";

        public const string SPValidateCarrierReference = "Chmpt.ValidateCarrierReference";
        public const string SPValidateCarrierReferenceParam1 = "EstimateId";
        public const string SPValidateCarrierReferenceParam2 = "CarrierReference";
        public const string SPValidateCarrierReferenceParam3 = "IsAvailableAlready";

        public const string SPFetchEstimatedShipmentDetails = "Chmpt.FetchEstimatedShipmentDetails";
        public const string SPFetchEstimatedShipmentDetailsParam1 = "ShipmentEstimateLineType";

        public const string SPGetCurrencyIndex = "Chmpt.GetCurrencyIndex";
        public const string SPGetCurrencyIndexParam1 = "CurrencyId";
        public const string SPGetCurrencyIndexParam2 = "CurrencyIndex";
        public const string SPGetCurrencyIndexParam3 = "DecimalPlaces";
        public const string SPGetCurrencyIndexParam4 = "ExchangeRate";
        public const string SPGetCurrencyIndexParam5 = "ExchangeExpirationDate";

        public const string SPGetShipmentEstimateInquiryDetails = "Chmpt.GetShipmentEstimateInquiryDetails";
        public const string SPGetShipmentEstimateInquiryDetailsParam1 = "EstimateId";

        #region Purchase Order | default BuyerID to improve user experience
        public const string SPValidatePurchaseBuyerIdNA = "Chmpt.ValidatePurchaseBuyerId";
        public const string SPValidatePurchaseBuyerIdEU = "Cpeur.ValidatePurchaseBuyerId";
        public const string SPValidatePurchaseBuyerIdParam1 = "PoNumber";
        public const string SPValidatePurchaseBuyerIdParam2 = "CompanyId";
        public const string SPValidatePurchaseBuyerIdParam3 = "Isvalid";
        #endregion

        #endregion

        #region Elemica

        public const string SPRetrieveElemicaDetailNA = "Chmpt.Fetchelemicadetail";
        public const string SPRetrieveElemicaDetailEU = "Cpeur.Fetchelemicadetail";
        public const string SPRetrieveElemicaDetailParam1 = "PoNumber";
        public const string SPRetrieveElemicaDetailParam2 = "DowVendor";

        public const string SPSendElemicaDetailNA = "Chmpt.Sendelemicadetail";
        public const string SPSendElemicaDetailEU = "Cpeur.Sendelemicadetail";
        public const string SPSendElemicaDetailParam1 = "PONumber";

        public const string SPUpdatePOStatusForElemicaNA = "Chmpt.UpdatePoStatusForElemica";
        public const string SPUpdatePOStatusForElemicaEU = "Cpeur.UpdatePoStatusForElemica";
        public const string SPUpdatePOStatusForElemicaParam1 = "PoNumber";

        #endregion

        #region PayableManagement
        public const string STR_XLS_EXTENSION = "xls";
        public const string STR_XLSX_EXTENSION = ".xlsx";
        public const string STR_CTSISOURCE = "CTSI";
        public const string STR_APISOURCE = "API";
        public const string ColumnsCount = "13";
        public const string CtsiEuColumnsCount = "21";
        public const string CtsiNaColumnsCount = "18";
        public const string RetryCount = "0";
        public const string DocType = "INV";
        public const string STR_PROCESSEDSUCCESSFULLY = "All the payments having the status PASS has been created successfully.";
        public const string STR_APISTATUSMESSAGE = "Some of the records are failed to create payment in GP.Please check your mail for the failure and reprocess its from api reupload window.";
        public const string STR_ALREADYPROCESSED = "The same file is processed already.";
        public const string STR_STATUSMESSAGE = "Some of the records are failed to create payment in GP.Please check your mail for the failure and reprocess its from ctsi reupload window.";
        public const string SPUploadAllAPITransactionsNA = "Chmpt.UploadAllAPITransactions";
        public const string SPUploadAllAPITransactionsEU = "Cpeur.UploadAllAPITransactions";
        public const string SPUploadAllAPITransactions_Param1 = "PoDetails";
        public const string SPUploadAllAPITransactions_Param2 = "NonPoDetails";
        public const string SPValidateAPITransactionsNA = "Chmpt.ValidateAPIFileDetails";
        public const string SPValidateAPITransactionsEU = "Cpeur.ValidateAPIFileDetails";
        public const string SPValidateAPITransactions_Param1 = "SavedApiDetails";
        public const string SPValidateAPITransactions_Param2 = "ErrorDetails";
        //public const string SPValidateAndGetTransactions = "ValidateAndGetCTSITransactions";
        //public const string SPValidateAndGetTransactionsParameter1 = "UserId";
        //public const string SPValidateAndGetTransactionsParameter2 = "CompanyId";
        //public const string SPValidateAndGetTransactionsParameter3 = "CTSITransactionsReceived";

        public const string CTSIToGPUserId = "SystemJob";
        //public const string ExtractedFilesArchiveFolder = "D:'\'chempoint'\'logging'\'CTSI'\'ProcessedFilesArchive'\'";
        public const string STR_EURCompanyId = "2";

        #endregion PayableManagement

        #region PayableService

        #region PayableManagement

        public const string ExpenseReportUploadForm = "pmExpenseReportUploadForm";
        public const string ExpenseStatus = "PASS";
        public const string ExpenseReportReUploadFailureForm = "ReUploadFailureExpenses";
        public const string NACompanyId = "1";
        public const string EUCompanyId = "2";
        public const string LocalCharge = "LocalCharge";
        public const string ZeroCharge = "ZeroCharge";
        public const string ReverseCharge = "ReverseCharge";

        public const string XsltParameterInvoiceNumber = "InvoiceNumber";
        public const string XsltParameterBatchNumber = "BatchNumber";
        public const string XsltParameterCtsiBatchNumber = "CtsiBatchNumber";
        public const string CtsiBatchNumber = "CTSIFINREVIEW";
        public const string BatchNumber = "EV";
        public const string XsltParameterNotes = "Notes";
        public const string PaymentCreationStyleSheet = @"D:\ChemPoint\Bin\XSL stylesheets\APOut\PaymentCreation.xsl";
        public const string POPaymentCreationStyleSheet = @"D:\ChemPoint\Bin\XSL stylesheets\APOut\POPaymentCreation.xsl";
        public const string InvoiceCreationStyleSheet = @"D:\Chempoint\bin\XSL StyleSheets\ExpenseVisor\InvoiceCreation.xsl";
        public const string CtsiInvoiceCreationStyleSheet = @"D:\ChemPoint\Bin\XSL stylesheets\CtsiToGP\InvoiceCreation.xsl";
        public const string XsltParameterCurncyid = "CurncyId";
        public const string XsltParameterLocncode = "LocnCode";
        public const string XsltParameterTen99amnt = "Ten99Amnt";
        public const string XsltParameterCompanyID = "CompanyID";
        public const string XsltParameterIsRequiredDistribution = "IsRequiredDistribution";

        public const string PartialNone = "Some of the records are failed to create payment in GP.Please check your mail for the failure and reprocess its from expense reupload window.";
        public const string AllPass = "All the payments having the status 'Pass' has been created successfully.";
        public const string AlreadyProcessed = "The same file is processed already.";


        public const string CTSIReportReUploadFailureForm = "ReUploadFailureCTSIDocuments";
        public const string ExtractedFilesArchiveFolder = @"D:\chempoint\logging\CTSI\ProcessedFilesArchive\";

        public const string APIReportReUploadFailureForm = "ReUploadFailureAPIDocuments";

        //public const string MailFrom = "stagedbAlert@Chempoint.com";
        //public const string MailCC = "nagarajan.seetharaman@Chempoint.com";
        //public const string MailTo = "raghunath.anipeddi@Chempoint.com";
        //public const string MailBcc = "raghunath.anipeddi@Chempoint.com";
        //public const string MailSubject = "Dev Expense Visor Payment Creation Failure Notification";
        //public const string SMTP = "mail10.chempoint.com";
        //public const string SourceApi = "API";
        //public const string MailApiToGPSubject = "Stage APOut Payment Creation Failure Notification - ";
        //public const string MailCtsiToGPSubject = "Dev CTSI Payment Creation Failure Notification - ";
        //public const string CTSIFailureMailContent = "&lt;html&gt;&lt;h3&gt;Following are the documents failed&lt;/h3&gt;&#xA;      &lt;table border = 1&gt; &#xA;        &lt;tr&gt;&#xA;          &lt;th&gt;CtsiID&lt;/th&gt;&#xA;          &lt;th&gt;DocumentNumber&lt;/th&gt;&#xA;          &lt;th&gt;VendorId&lt;/th&gt;&#xA;          &lt;th&gt;ErrorDescription&lt;/th&gt;&#xA;        &lt;/tr&gt;&#xA;      &lt;/table&gt;&#xA;      &lt;/html&gt;";
        //public const string APIFailureMailContent = "<html><h3>Following are the documents failed</h3>&#xA;      <table border = 1> &#xA;        <tr>&#xA;          <th>ApiID</th>&#xA;          <th>DocumentNumber</th>&#xA;          <th>VendorId</th>&#xA;          <th>ErrorDescription</th>&#xA;        </tr>&#xA;      </table>&#xA;      </html>";
        //public const string FailureMailContent = "&lt;html&gt;&lt;h3&gt;Following are the documents failed&lt;/h3&gt;&#xA;      &lt;table border = 1&gt; &#xA;        &lt;tr&gt;&#xA;          &lt;th&gt;ExpenseRecordID&lt;/th&gt;&#xA;          &lt;th&gt;VendorId&lt;/th&gt;&#xA;          &lt;th&gt;ErrorDescription&lt;/th&gt;&#xA;        &lt;/tr&gt;&#xA;      &lt;/table&gt;&#xA;      &lt;/html&gt;";


        public const string SPSaveOriginalExpenseFileDetail = "chmpt.SaveOriginalExpenseFileDetail";
        public const string SPSaveOriginalExpenseFileDetailParam1 = "UserId";
        public const string SPSaveOriginalExpenseFileDetailParam2 = "NACompanyId";
        public const string SPSaveOriginalExpenseFileDetailParam3 = "SavedExpenseFileDetails";
        public const string SPSaveOriginalExpenseFileDetailParam4 = "IsAlreadyExist";

        public const string SPSaveExpenseFileDetailsForReupload = "SaveExpenseFileDetails";
        public const string SPSaveExpenseFileDetailsForReuploadParam1 = "UserId";
        public const string SPSaveExpenseFileDetailsForReuploadParam2 = "NACompanyId";
        public const string SPSaveExpenseFileDetailsForReuploadParam3 = "SavedExpenseFileDetails";

        public const string SPFetchAccountNumberDetailsNA = "Chmpt.CPGETACCOUNTNUMBER";
        public const string SPFetchAccountNumberDetailsEU = "Cpeur.CPGETACCOUNTNUMBER";
        public const string SPFetchAccountNumberDetailsParam1 = "ACTINDEX";
        public const string SPFetchAccountNumberDetailsParam2 = "VENDORID";

        public const string SPGetNonPODistributionValuesNA = "Chmpt.CPCALCULATEPAYMENTDISTRIBUTION";
        public const string SPGetNonPODistributionValuesEU = "Cpeur.CPCALCULATEPAYMENTDISTRIBUTION";
        public const string SPGetNonPODistributionValuesParam1 = "APOUTDATA";
        public const string SPGetNonPODistributionValuesParam2 = "CREDITACCOUNT";
        public const string SPGetNonPODistributionValuesParam3 = "CREDITACCOUNTNAME";
        public const string SPGetNonPODistributionValuesParam4 = "TEN99AMNT";

        public const string SPGetNextInvoiceNumberForApiNA = "Chmpt.CPGetNextInvNumber";
        public const string SPGetNextInvoiceNumberForApiEU = "Cpeur.CPGetNextInvNumber";
        public const string SPGetNextInvoiceNumberForApiParam1 = "INVTYPE";
        public const string SPGetNextInvoiceNumberForApiParam2 = "INVNUMBER";

        public const string SPGetNextInvoiceNumberNA = "Chmpt.CPGetNextInvNumber";
        public const string SPGetNextInvoiceNumberEU = "Cpeur.CPGetNextInvNumber";
        public const string SPGetNextInvoiceNumberParam1 = "INVTYPE";
        public const string SPGetNextInvoiceNumberParam2 = "INVNUMBER";

        public const string SPUpdatePaymentStatusNA = "Chmpt.UpdatePaymentsCreated";
        public const string SPUpdatePaymentStatusEU = "Cpeur.UpdatePaymentsCreated";
        public const string SPUpdatePaymentStatusParam1 = "UserId";
        public const string SPUpdatePaymentStatusParam2 = "NACompanyId";
        public const string SPUpdatePaymentStatusParam3 = "Payments";

        public const string SPUpdateCtsiPaymentStatusNA = "Chmpt.UpdatePaymentsCreatedForCTSI";
        public const string SPUpdateCtsiPaymentStatusEU = "Cpeur.UpdatePaymentsCreatedForCTSI";
        public const string SPUpdateCtsiPaymentStatusParam1 = "UserId";
        public const string SPUpdateCtsiPaymentStatusParam2 = "CompanyId";
        public const string SPUpdateCtsiPaymentStatusParam3 = "Payments";

        public const string SPUpdateApiPaymentStatusNA = "Chmpt.UpdatePaymentsCreatedForAPI";
        public const string SPUpdateApiPaymentStatusEU = "Cpeur.UpdatePaymentsCreatedForAPI";
        public const string SPUpdateApiPaymentStatusParam1 = "UserId";
        public const string SPUpdateApiPaymentStatusParam2 = "CompanyId";
        public const string SPUpdateApiPaymentStatusParam3 = "Payments";

        public const string SPSaveCTSITransactionsNA = "Chmpt.SaveOriginalCtsiFileDetail";
        public const string SPSaveCTSITransactionsEU = "Cpeur.SaveOriginalCtsiFileDetail";
        public const string SPSaveCTSITransactionsParam1 = "UserId";
        public const string SPSaveCTSITransactionsParam2 = "CompanyId";
        public const string SPSaveCTSITransactionsParam3 = "CurrencyId";
        public const string SPSaveCTSITransactionsParam4 = "FileName";
        public const string SPSaveCTSITransactionsParam5 = "SavedCTSIFileDetails";
        public const string SPSaveCTSITransactionsParam6 = "IsAlreadyExist";

        public const string SPSaveCTSIFileDetailsForReuploadNA = "Chmpt.SaveCtsiFileDetails";
        public const string SPSaveCTSIFileDetailsForReuploadEU = "Cpeur.SaveCtsiFileDetails";
        public const string SPSaveCTSIFileDetailsForReuploadParam1 = "UserId";
        public const string SPSaveCTSIFileDetailsForReuploadParam2 = "CompanyId";
        public const string SPSaveCTSIFileDetailsForReuploadParam3 = "SavedCtsiFileDetails";

        public const string SPSaveAPIFileDetailsForReuploadNA = "Chmpt.SaveNonPOApiFileDetails";
        public const string SPSaveAPIFileDetailsForReuploadEU = "Cpeur.SaveNonPOApiFileDetails";
        public const string SPSaveAPIFileDetailsForReuploadParam1 = "UserId";
        public const string SPSaveAPIFileDetailsForReuploadParam2 = "CompanyId";
        public const string SPSaveAPIFileDetailsForReuploadParam3 = "SavedNonPOApiFileDetails";
        public const string SPSaveAPIFileDetailsForReuploadForPONA = "Chmpt.SavePOApiFileDetails";
        public const string SPSaveAPIFileDetailsForReuploadForPOEU = "Cpeur.SavePOApiFileDetails";
        public const string SPSaveAPIFileDetailsForReuploadForPOParam1 = "UserId";
        public const string SPSaveAPIFileDetailsForReuploadForPOParam2 = "CompanyId";
        public const string SPSaveAPIFileDetailsForReuploadForPOParam3 = "SavedPoApiFileDetails";
        public const string SPSaveAPIFileDetailsForReuploadForPOParam4 = "SavedGloFileDetails";

        //public const string STR_CTSISOURCE = "CTSI";
        //public const string ColumnsCount = "13";
        public const string CHMPTFileName = "EOW3439";
        public const string CPEURFileName = "EOW5151";
        public const string GBPFileName = "EOW5152";
        //public const string CtsiNaColumnsCount = "18";
        //public const string CtsiEuColumnsCount = "21";
        //public const string DocType = "INV";
        public const string STR_EURCompany = "CPEUR";




        public const string SPGetAPITaxScheduleDetails = "Cpeur.GetAPITaxScheduleDetails";
        public const string SPGetAPITaxScheduleDetailsParam1 = "ApiTaxScheduleDetails";

        public const string SPGetFailedExpenseIdsList = "Chmpt.dbo.GetExpenseReportIdsForLookup";
        public const string SPGetFailedCTSIIdsListNA = "chmpt.GetCTSIIdsForLookup";
        public const string SPGetFailedCTSIIdsListEU = "Cpeur.GetCTSIIdsForLookup";

        public const string SPValidateAndGetTransactions = "ValidateExpenseFileDetail";
        public const string SPValidateAndGetTransactionsParam1 = "UserId";
        public const string SPValidateAndGetTransactionsParam2 = "NACompanyId";
        public const string SPValidateAndGetTransactionsParam3 = "FormName";
        public const string SPValidateAndGetTransactionsParam4 = "OriginalExpenseFileDetails";

        public const string SPValidateVoucherExistsForVendorNA = "Chmpt.ValidateEnteredManualPayment";
        public const string SPValidateVoucherExistsForVendorEU = "Cpeur.ValidateEnteredManualPayment";
        public const string SPValidateVoucherExistsForVendorParam1 = "InvoiceNumber";
        public const string SPValidateVoucherExistsForVendorParam2 = "VendorId";
        public const string SPValidateVoucherExistsForVendorParam3 = "IsValid";

        public const string SPGetFailedExpenseIdsToLinkManualPayments = "Chmpt.dbo.GetFailedExpensesToLinkManually";
        public const string SPGetFailedExpenseIdsToLinkManualPaymentsParam1 = "SearchType";
        public const string SPGetFailedExpenseIdsToLinkManualPaymentsParam2 = "ExpenseFieldIdStart";
        public const string SPGetFailedExpenseIdsToLinkManualPaymentsParam3 = "ExpenseFieldIdEnd";
        public const string SPGetFailedExpenseIdsToLinkManualPaymentsParam4 = "DateStart";
        public const string SPGetFailedExpenseIdsToLinkManualPaymentsParam5 = "DateEnd";
        public const string SPGetFailedExpenseIdsToLinkManualPaymentsParam6 = "VendorIdStart";
        public const string SPGetFailedExpenseIdsToLinkManualPaymentsParam7 = "VendorIdEnd";

        public const string SPUpdateManualPaymentNumberForExpenseRecord = "Chmpt.dbo.UpdateLinkedManualPayments";
        public const string SPUpdateManualPaymentNumberForExpenseRecordParam1 = "UserId";
        public const string SPUpdateManualPaymentNumberForExpenseRecordParam2 = "NACompanyId";
        public const string SPUpdateManualPaymentNumberForExpenseRecordParam3 = "LinkedManualPayments";
        public const string SPUpdateManualPaymentNumberForExpenseRecordParam4 = "IsValidStatus";

        public const string SPVerifyLookupValueExistsNA = "Chmpt.VerifyLookupValueExists";
        public const string SPVerifyLookupValueExistsEU = "Cpeur.VerifyLookupValueExists";
        public const string SPVerifyLookupValueExistsParam1 = "SouceLookup";
        public const string SPVerifyLookupValueExistsParam2 = "UpdatedLookupValue";
        public const string SPVerifyLookupValueExistsParam3 = "IsUpdatedLookupExists";

        public const string SPGetBusinessGroupDetailsForLookup = "Chmpt.dbo.GetBusinessGroupDetails";
        public const string SPGetBusinessUnitDetailsForLookup = "Chmpt.dbo.GetBusinessUnitDetails";
        public const string SPGetExpenseTypeNameDetailsForLookup = "Chmpt.dbo.GetExpenseTypeNameDetails";

        public const string SPGetFailedExpenseDocuments = "Chmpt.dbo.GetReUploadFailureExpenses";
        public const string SPGetFailedExpenseDocumentsParam1 = "SearchType";
        public const string SPGetFailedExpenseDocumentsParam2 = "ExpenseFieldIdStart";
        public const string SPGetFailedExpenseDocumentsParam3 = "ExpenseFieldIdEnd";
        public const string SPGetFailedExpenseDocumentsParam4 = "DateStart";
        public const string SPGetFailedExpenseDocumentsParam5 = "DateEnd";
        public const string SPGetFailedExpenseDocumentsParam6 = "VendorIdStart";
        public const string SPGetFailedExpenseDocumentsParam7 = "VendorIdEnd";

        public const string SPGetFailedCtsiDocumentsNA = "Chmpt.GetReUploadFailureCtsi";
        public const string SPGetFailedCtsiDocumentsEU = "Cpeur.GetReUploadFailureCtsi";
        public const string SPGetFailedCtsiDocumentsParam1 = "SearchType";
        public const string SPGetFailedCtsiDocumentsParam2 = "CTSIInvoiceIDStart";
        public const string SPGetFailedCtsiDocumentsParam3 = "CTSIInvoiceIDEnd";
        public const string SPGetFailedCtsiDocumentsParam4 = "DateStart";
        public const string SPGetFailedCtsiDocumentsParam5 = "DateEnd";
        public const string SPGetFailedCtsiDocumentsParam6 = "VendorIdStart";
        public const string SPGetFailedCtsiDocumentsParam7 = "VendorIdEnd";

        public const string SPValidateAndGetCtsiTransactionsNA = "chmpt.ValidateAndGetCTSITransactions";
        public const string SPValidateAndGetCtsiTransactionsEU = "Cpeur.ValidateAndGetCTSITransactions";
        public const string SPValidateAndGetCtsiTransactionsParam1 = "UserId";
        public const string SPValidateAndGetCtsiTransactionsParam2 = "CompanyId";
        public const string SPValidateAndGetCtsiTransactionsParam3 = "CTSITransactionsReceived";

        public const string SPGetFailedApiDocumentsNA = "Chmpt.GetReUploadFailureNonPOApi";
        public const string SPGetFailedApiDocumentsEU = "Cpeur.GetReUploadFailureNonPOApi";
        public const string SPGetFailedApiDocumentsForPoNA = "Chmpt.GetReUploadFailurePOApi";
        public const string SPGetFailedApiDocumentsForPoEU = "Cpeur.GetReUploadFailurePOApi";
        public const string SPGetFailedApiDocumentsParam1 = "SearchType";
        public const string SPGetFailedApiDocumentsParam2 = "ApiInvoiceIDStart";
        public const string SPGetFailedApiDocumentsParam3 = "ApiInvoiceIDEnd";
        public const string SPGetFailedApiDocumentsParam4 = "DateStart";
        public const string SPGetFailedApiDocumentsParam5 = "DateEnd";
        public const string SPGetFailedApiDocumentsParam6 = "VendorIdStart";
        public const string SPGetFailedApiDocumentsParam7 = "VendorIdEnd";

        public const string SPVerifyApiLookupValueExistsNA = "Chmpt.VerifyLookupValueExists";
        public const string SPVerifyApiLookupValueExistsEU = "Cpeur.VerifyLookupValueExists";
        public const string SPVerifyApiLookupValueExistsParam1 = "SouceLookup";
        public const string SPVerifyApiLookupValueExistsParam2 = "UpdatedLookupValue";
        public const string SPVerifyApiLookupValueExistsParam3 = "IsUpdatedLookupExists";

        public const string SPValidateAndGetApiTransactionsNA = "Chmpt.ValidateNonPOAndGetAPITransactions";
        public const string SPValidateAndGetApiTransactionsEU = "Cpeur.ValidateNonPOAndGetAPITransactions";
        public const string SPValidateAndGetApiTransactionsParam1 = "UserId";
        public const string SPValidateAndGetApiTransactionsParam2 = "CompanyId";
        public const string SPValidateAndGetApiTransactionsParam3 = "APINonPOTransactionsReceived";
        public const string SPValidateAndGetApiTransactionsParam4 = "SourceName";
        public const string SPValidatePOAndGetAPITransactionsNA = "Chmpt.ValidatePOAndGetAPITransactions";
        public const string SPValidatePOAndGetAPITransactionsEU = "Cpeur.ValidatePOAndGetAPITransactions";
        public const string SPValidatePOAndGetAPITransactionsParam1 = "UserId";
        public const string SPValidatePOAndGetAPITransactionsParam2 = "CompanyId";
        public const string SPValidatePOAndGetAPITransactionsParam3 = "APITransactionsReceived";
        public const string SPValidatePOAndGetAPITransactionsParam4 = "SourceName";

        public const string SPGetApiDuplicateDocumentRowDetailsNA = "Chmpt.GetDuplicateDocumentRowDetails";
        public const string SPGetApiDuplicateDocumentRowDetailsEU = "Cpeur.GetDuplicateDocumentRowDetails";
        public const string SPGetApiDuplicateDocumentRowDetailsParam1 = "UserId";
        public const string SPGetApiDuplicateDocumentRowDetailsParam2 = "CompanyId";
        public const string SPGetApiDuplicateDocumentRowDetailsParam3 = "ApiDuplicateRowDetails";

        public const string SPGetFailedAPIIdsListNA = "Chmpt.GetAPIIdsForLookup";
        public const string SPGetFailedAPIIdsListEU = "Cpeur.GetAPIIdsForLookup";
        public const string SPGetFailedAPIIdsListParam1 = "docType";

        public const string SPUpdateManualPaymentNumberForAPIDocumentsNA = "Chmpt.ApiUpdateLinkedManualPayments";
        public const string SPUpdateManualPaymentNumberForAPIDocumentsEU = "Cpeur.ApiUpdateLinkedManualPayments";
        public const string SPUpdateManualPaymentNumberForAPIDocumentsParam1 = "UserId";
        public const string SPUpdateManualPaymentNumberForAPIDocumentsParam2 = "CompanyId";
        public const string SPUpdateManualPaymentNumberForAPIDocumentsParam3 = "ApiLinkedManualPayments";
        public const string SPUpdateManualPaymentNumberForAPIDocumentsParam4 = "IsValidStatus";

        public const string SPGetFailedApiIdsToLinkManualPaymentsNA = "Chmpt.GetFailedApiIdToLinkManually";
        public const string SPGetFailedApiIdsToLinkManualPaymentsEU = "Cpeur.GetFailedApiIdToLinkManually";
        public const string SPGetFailedApiIdsToLinkManualPaymentsParam1 = "InvoiceType";
        public const string SPGetFailedApiIdsToLinkManualPaymentsParam2 = "SearchType";
        public const string SPGetFailedApiIdsToLinkManualPaymentsParam3 = "FromApiId";
        public const string SPGetFailedApiIdsToLinkManualPaymentsParam4 = "ToApiId";
        public const string SPGetFailedApiIdsToLinkManualPaymentsParam5 = "VendorIdStart";
        public const string SPGetFailedApiIdsToLinkManualPaymentsParam6 = "VendorIdEnd";

        public const string SPValidatePODocumentExistsForVendorNA = "Chmpt.ValidateEnteredPOManualPayment";
        public const string SPValidatePODocumentExistsForVendorEU = "Cpeur.ValidateEnteredPOManualPayment";
        public const string SPValidatePODocumentExistsForVendorParam1 = "InvoiceNumber";
        public const string SPValidatePODocumentExistsForVendorParam2 = "VendorId";
        public const string SPValidatePODocumentExistsForVendorParam3 = "IsValid";

        public const string SPValidateNonPODocumentExistsForVendorNA = "Chmpt.ValidateEnteredNonPOManualPayment";
        public const string SPValidateNonPODocumentExistsForVendorEU = "Cpeur.ValidateEnteredNonPOManualPayment";
        public const string SPValidateNonPODocumentExistsForVendorParam1 = "VoucherNumber";
        public const string SPValidateNonPODocumentExistsForVendorParam2 = "VendorId";
        public const string SPValidateNonPODocumentExistsForVendorParam3 = "IsValid";

        public const string SPUpdateManualPaymentNumberForCTSIDocumentsNA = "Chmpt.CtsiUpdateLinkedManualPayments";
        public const string SPUpdateManualPaymentNumberForCTSIDocumentsEU = "Cpeur.CtsiUpdateLinkedManualPayments";
        public const string SPUpdateManualPaymentNumberForCTSIDocumentsParam1 = "UserId";
        public const string SPUpdateManualPaymentNumberForCTSIDocumentsParam2 = "CompanyId";
        public const string SPUpdateManualPaymentNumberForCTSIDocumentsParam3 = "CtsiLinkedManualPayments";
        public const string SPUpdateManualPaymentNumberForCTSIDocumentsParam4 = "IsValidStatus";

        public const string SPGetFailedCtsiIdsToLinkManualPaymentsNA = "chmpt.GetFailedCtsiIdToLinkManually";
        public const string SPGetFailedCtsiIdsToLinkManualPaymentsEU = "chmpt.GetFailedCtsiIdToLinkManually";
        public const string SPGetFailedCtsiIdsToLinkManualPaymentsParam1 = "SearchType";
        public const string SPGetFailedCtsiIdsToLinkManualPaymentsParam2 = "FromCtsiId";
        public const string SPGetFailedCtsiIdsToLinkManualPaymentsParam3 = "ToCtsiId";
        public const string SPGetFailedCtsiIdsToLinkManualPaymentsParam4 = "DateStart";
        public const string SPGetFailedCtsiIdsToLinkManualPaymentsParam5 = "DateEnd";
        public const string SPGetFailedCtsiIdsToLinkManualPaymentsParam6 = "VendorIdStart";
        public const string SPGetFailedCtsiIdsToLinkManualPaymentsParam7 = "VendorIdEnd";

        public const string SPVerifyCtsiLookupValueExistsNA = "Chmpt.VerifyLookupValueExists";
        public const string SPVerifyCtsiLookupValueExistsEU = "Cpeur.VerifyLookupValueExists";
        public const string SPVerifyCtsiLookupValueExistsParam1 = "SouceLookup";
        public const string SPVerifyCtsiLookupValueExistsParam2 = "UpdatedLookupValue";
        public const string SPVerifyCtsiLookupValueExistsParam3 = "IsUpdatedLookupExists";

        #region APIFIleGenerator

        public const string NAFileNamePrefix = "CHPAPO";
        public const string EUFileNamePrefix = "CH2APO";
        public const string OutputNAFileName = "CHPAPOAPS_";
        public const string OutputEUFileName = "CH2APOAPS_";
        public const string OutputNAGLVFileName = "CHPAPOSGLV_";
        public const string OutputEUGLVFileName = "CH2PAPOSGLV_";
        public const string ExecuRetryCount = "1";

        public const string ApiFileDaily = "Daily";
        public const string ApiFileQuarterly = "Quarterly";

        public const string SPGetAPIFilesDailyNA = "Chmpt.CPGetAPIDetailsDaily";
        public const string SPGetAPIFilesDailyEU = "Cpeur.CPGetAPIDetailsDaily";
        public const string SPGetAPIFilesQuaterlyNA = "Chmpt.CPGetAPIDetailsQuarterly";
        public const string SPGetAPIFilesQuaterlyEU = "Cpeur.CPGetAPIDetailsQuarterly";

        public const string SPGetVenGLDetailsNA = "Chmpt.CPGetAPIVendorGLDetails";
        public const string SPGetVenGLDetailsEU = "Cpeur.CPGetAPIVendorGLDetails";


        #endregion

        #region CTSIFileUploader

        public const string SPFetchAllCtsiTransactionDetailsNA = "Chmpt.GetAllCtsiTransactionDetails";
        public const string SPFetchAllCtsiTransactionDetailsEU = "Cpeur.GetAllCtsiTransactionDetails";
        public const string SPFetchAllCtsiTransactionDetailsParam1 = "ProcessingDate";

        public const string SPSaveAllUploadedDetailsNA = "Chmpt.SaveAllUploadedDetailsToCtsi";
        public const string SPSaveAllUploadedDetailsEU = "Cpeur.SaveAllUploadedDetailsToCtsi";
        public const string SPSaveAllUploadedDetailsParam1 = "CtsiInBoundDetails";
        public const string SPSaveAllUploadedDetailsParam2 = "CtsiOutBoundDetails";
        public const string SPSaveAllUploadedDetailsParam3 = "CtsiCustomerReturnDetails";
        public const string SPSaveAllUploadedDetailsParam4 = "CtsiSupplierReturnDetails";
        public const string SPSaveAllUploadedDetailsParam5 = "CtsiTransferReturnDetails";
        public const string SPSaveAllUploadedDetailsParam6 = "UserId";
        public const string SPSaveAllUploadedDetailsParam7 = "";

        #endregion


        #endregion

        #endregion

        #endregion PurchaseModule

        #region XRMServiceAuditLog

        public const string SPUpdateXrmIntegrationsAuditLog = "Dynamics.xrm.UpdateXRMIntegrationsAuditLog";
        public const string SPUpdateXrmIntegrationsAuditLogParam1 = "XRMIntegrationsAudit";
        public const string SPUpdateXrmIntegrationsAuditLogParam2 = "CompanyId";
        public const string SPUpdateXrmIntegrationsAuditLogParam3 = "UserId";

        #endregion XRMServiceAuditLog

        #region TaskSchedulerModule

        #region HoldEngine

        public string BatchUserId = "batch";

        public const string UpdateHoldDetails = "GPCustomizations.Chmpt.UpdateHoldEngine";
        public const string ProcessHoldParamSopNumber = "SopNumber";
        public const string ProcessHoldParamSopType = "SopType";
        public const string ProcessHoldParamBatchUserId = "BatchUserId";
        public const string ProcessHoldParamCustomerList = "CustomerList";
        //Process Holds
        public const string FetchHoldDetailsNA = "GPCustomizations.Chmpt.CreditHoldEngine";
        public const string SPProcessCustCreditStandCacheNA = "GPCustomizations.Chmpt.CpDocumentHoldCustomerCreditStandingCache";
        public const string SPProcessCreditHoldNA = "GPCustomizations.Chmpt.CpDocumentHoldsDocumentCreditLimitHolds";
        public const string SPProcessDocumentTermsHoldNA = "GPCustomizations.Chmpt.CpDocumentHoldTermsHolds";
        public const string SPProcessDocumentSalesAlertHoldNA = "GPCustomizations.Chmpt.CpDocumentHoldSalesAlertsProcessHolds";
        public const string SPProcessDocumentFirstOrderHoldNA = "GPCustomizations.Chmpt.CpDocumentHoldFirstOrderProcessHolds";
        public const string SPProcessDocumentCustomerHoldNA = "GPCustomizations.Chmpt.CpDocumentHoldCustomerHold";
        public const string SPProcessDocumentManualHoldNA = "GPCustomizations.Chmpt.CpDocumentHoldsManualHolds";
        public const string SPProcessCustomerHoldNA = "GPCustomizations.Chmpt.Customerhold";
        public const string SPProcessOpenOrdersPaymentTermsNA = "GPCustomizations.Chmpt.UpdateOpenOrdersPaymentTerms";
        public const string SPProcessTaxHoldNA = "GPCustomizations.Chmpt.CPDocumentHoldTaxHolds";
        public const string SPProcessVatHoldNA = "GPCustomizations.Cpeur.CPDocumentHoldVatHolds";
        public const string SPProcessCreditHoldEngineNA = "GPCustomizations.Chmpt.CreditHoldEngine";
        public const string SPProcessCreditHoldEngineEU = "GPCustomizations.Cpeur.CreditHoldEngine";
        public const string SPProcessHoldEngine = "GPCustomizations.Common.Processholdengine";

        public const string SPProcessCreditHoldEngineForOrderNA = "GPCustomizations.Chmpt.NaCreditHoldEngineForOrder";
        public const string SPProcessCreditHoldEngineForOrderEU = "GPCustomizations.Cpeur.EuCreditHoldEngineForOrder";

        public const string FetchHoldDetailsEU = "GPCustomizations.Cpeur.CreditHoldEngine";
        public const string SPProcessCustCreditStandCacheEU = "GPCustomizations.Cpeur.CpDocumentHoldCustomerCreditStandingCache";
        public const string SPProcessCreditHoldEU = "GPCustomizations.Cpeur.CpDocumentHoldsDocumentCreditLimitHolds";
        public const string SPProcessDocumentTermsHoldEU = "GPCustomizations.Cpeur.CpDocumentHoldTermsHolds";
        public const string SPProcessDocumentSalesAlertHoldEU = "GPCustomizations.Cpeur.CpDocumentHoldSalesAlertsProcessHolds";
        public const string SPProcessDocumentFirstOrderHoldNeu = "GPCustomizations.Cpeur.CpDocumentHoldFirstOrderProcessHolds";
        public const string SPProcessDocumentCustomerHoldEU = "GPCustomizations.Cpeur.CpDocumentHoldCustomerHold";
        public const string SPProcessDocumentManualHoldEU = "GPCustomizations.Cpeur.CpDocumentHoldsManualHolds";
        public const string SPProcessCustomerHoldEU = "GPCustomizations.Cpeur.Customerhold";
        public const string SPProcessOpenOrdersPaymentTermsEU = "GPCustomizations.Cpeur.UpdateOpenOrdersPaymentTerms";
        public const string SPProcessTaxHoldEU = "GPCustomizations.Cpeur.CPDocumentHoldTaxHolds";
        public const string SPProcessVatHoldEU = "GPCustomizations.Cpeur.CPDocumentHoldVatHolds";
        public const string SPProcessHoldEngineEU = "GPCustomizations.Cpeur.ProcessCreditHoldEngineEU";

        #endregion HoldEngine

        #endregion TaskSchedulerModule

        #region TaxExtDetails
        //Save
        public const string SPSaveTaxExtDetails = "GPCustomizations.Cpeur.SaveTaxExtDetails";
        public const string SPSaveTaxExtDetailsParam1 = "TaxDetailId";
        public const string SPSaveTaxExtDetailsParam2 = "TaxDetailReference";
        public const string SPSaveTaxExtDetailsParam3 = "UnivarNvTaxCode";
        public const string SPSaveTaxExtDetailsParam4 = "UnivarNvTaxCodeDescription";
        public const string SPSaveTaxExtDetailsParam5 = "UserName";
        public const string SPSaveTaxExtDetailsParam6 = "CompanyID";

        //Delete
        public const string SPDeleteTaxExtDetails = "GPCustomizations.Cpeur.DeleteTaxExtDetails";
        public const string SPDeleteTaxExtDetailsParam1 = "TaxDetailId";

        //Display
        public const string SPGetTaxExtDetails = "GPCustomizations.Cpeur.GetTaxExtDetails";
        public const string SPGetTaxExtDetailsParam1 = "TaxDetailId";

        //Save
        public const string SPSaveTaxScheduleVatDetails = "GPCustomizations.Cpeur.SaveTaxScheduleVATDetails";
        public const string SPSaveTaxScheduleVatDetailsParam1 = "TaxScheduleId";
        public const string SPSaveTaxScheduleVatDetailsParam2 = "ChempointVat";
        public const string SPSaveTaxScheduleVatDetailsParam3 = "IsActive";
        public const string SPSaveTaxScheduleVatDetailsParam4 = "UserName";
        public const string SPSaveTaxScheduleVatDetailsParam5 = "CompanyId";

        //Delete
        public const string SPDeleteTaxSchduleDetails = "GPCustomizations.Cpeur.DeleteTaxSchduleDetails";
        public const string SPDeleteTaxSchduleDetailsParam1 = "TaxScheduleId ";

        //Display
        public const string SPGetTaxScheduleDetails = "GPCustomizations.Cpeur.GetTaxScheduleDetails";
        public const string SPGetTaxScheduleDetailsParam1 = "TaxScheduleId";

        #endregion TaxExtDetails

        #region PaymentTermsSetup
        //Fetch
        public const string SPFetchPaymentTermSetupDetailsNA = "chmpt.NaGetPaymentTermSetupDetails";
        public const string SPFetchPaymentTermSetupDetailsEU = "cpeur.EuGetPaymentTermSetupDetails";
        public const string SPFetchPaymentTermSetupDetailsParam1 = "PaymentTermsID";

        //Save
        public const string SPSavePaymentTermsSetupNA = "Chmpt.NASavePaymentTermDetails";
        public const string SPSavePaymentTermsSetupEU = "cpeur.EUSavePaymentTermDetails";
        public const string SPSavePaymentTermsSetupParam1 = "UserName";
        public const string SPSavePaymentTermsSetupParam2 = "CompanyId";
        public const string SPSavePaymentTermsSetupParam3 = "SavePaymentTermsSetupType";


        public const string SpnaGetPaymentTermsDueDate = "Chmpt.NaCalculateDueDate";
        public const string SpeuGetPaymentTermsDueDate = "Cpeur.EuCalculateDueDate";
        public const string SPGetPaymentTermsDueDateParam1 = "DocDate";
        public const string SPGetPaymentTermsDueDateParam2 = "PaymentTerm";
        public const string SPGetPaymentTermsDueDateParam3 = "DueDate";

        public const string SPDeletePaymentTermsDetailsNA = "Chmpt.DeletePaymentTermDetail";
        public const string SPDeletePaymentTermsDetailsEU = "Cpeur.DeletePaymentTermDetail";
        public const string SPDeletePaymentTermsDetailsParam1 = "PaymentTermsID";

        #endregion PaymentTermsSetup

        #region OrderPush
        public const string SPFetchOrderDetailsNA = "GPCustomizations.chmpt.GetOrderDetailsForPushToGP";
        public const string SPFetchOrderDetailsEU = "GPCustomizations.cpeur.GetOrderDetailsForPushToGP";
        public const string SPFetchOrderDetailsParam1 = "ItemNumbers";
        public const string SPFetchOrderDetailsParam2 = "CurrencyID";
        public const string SPFetchOrderDetailsParam3 = "CustomerID";
        public const string SPFetchOrderDetailsParam4 = "ShipFromLocationCode";
        public const string SPFetchOrderDetailsParam5 = "ShipVia";
        public const string SPFetchOrderDetailsParam6 = "FreightTerm";
        #endregion OrderPush

        #region PTE
        public const string SPUpdatePteRequestNA = "GPCustomizations.chmpt.UpdatePTEDetails";
        public const string SPUpdatePteRequestParam1 = "PteData";
        #endregion PTE

        #region GpLotDetails
        public const string SpcpUpdateLotsInfo = "Chmpt.UpdateLotsInfo";
        public const string SpcpGetSalesItemLotInfo = "Chmpt.CPGetSalesItemLotInfo";

        public const string SPSalesLotsInfoSopNumber = "SopNumber";
        public const string SPSalesLotsInfoSopType = "SopType";
        public const string SPSalesLotsInfoItemNumber = "ItemNumber";
        public const string SPSalesLotsInfoLineItemSequence = "LineItemSequence";
        public const string SPSalesLotsInfoComponentSequence = "ComponentSequence";
        public const string SPSalesLotsInfoCompanyId = "CompanyId";
        public const string SPSalesLotsInfoUserName = "UserName";
        public const string SPSalesLotsInfoInputXml = "LotXml";

        #endregion GpLotDetails

        #region PTStatusUpdate

        //Chmpt.Cpupdatesoppickticketstatus
        public const string SPCpupdatesoppickticketstatusNA = "GpCustomizations.Chmpt.CpUpdateSopPickTicketStatus";
        public const string SPCpupdatesoppickticketstatusEU = "GpCustomizations.Cpeur.CpUpdateSopPickTicketStatus";
        public const string SPCpupdatesoppickticketstatusSopNumber = "InvoiceNumber";
        public const string SPCpupdatesoppickticketstatusSopType = "SopType";

        public const string SPCpUpdateAutoSendPtLog = "GPCustomizations.Chmpt.CpUpdateAutoSendPtLog";
        public const string SPCpUpdateAutoSendPtLogParam1 = "I_iInvoiceType";
        public const string SPCpUpdateAutoSendPtLogParam2 = "I_vInvoiceNo";
        public const string SPCpUpdateAutoSendPtLogParam3 = "I_iStatus";
        public const string SPCpUpdateAutoSendPtLogParam4 = "I_vMessage";

        #endregion PTStatusUpdate

        //VAT Number
        public const string SPFetchVatReferenceNumber = "Cpeur.FetchVATReferenceNumber";
        public const string SPFetchVatReferenceNumberCustomerId = "CustomerId";


        #region OrderToFoTransfer
        public const string SPGetOrderToAutoTransferNA = "GPCustomizations.Chmpt.NaGetOrdersToAutoFOTransfer";
        public const string SPGetNextAvailableFONumberNA = "GPCustomizations.Chmpt.NAGetNextAvailableSopNumber";
        public const string SPGetNextAvailableFONumberEU = "GPCustomizations.Cpeur.EUGetNextAvailableSopNumber";
        public const string SPGetNextAvailableFONumberParam1 = "SopType";
        public const string SPGetNextAvailableFONumberParam2 = "OrderDocumentNumber";
        public const string SPGetNextAvailableFONumberParam3 = "SopNumber";
        #endregion OrderToFoTransfer

        #region PT
        public const string SPGetOrderPTDetails = "GPCustomizations.Chmpt.GetOrderEligibilityDetailsToSendPT";
        public const string SPGetOrderPTDetailsParam1 = "InvoiceNumber";
        public const string SPGetOrderPTDetailsParam2 = "SOPType";
        //public const string SPGetOrderPTDetails_Param3 = "RequestType";

        public const string SPSubmitPTtoWHandChr = "Chmpt.dbo.CPSubmitPTtoWHandCHR";
        public const string SPSubmitPTtoWHandChrParam1 = "I_vInvoiceNo";

        public const string SPUpdatePTStatusToGpna = "GPCustomizations.Chmpt.UpdatePTStatusToGP";
        public const string SPUpdatePTStatusToGpeu = "GPCustomizations.Cpeur.UpdatePTStatusToGP";
        public const string SPUpdatePTStatusToGPParam1 = "SopNumber";



        #endregion PT

        #region CustomerIntegrations

        public const string SPNAIsServiceSKUEligible = "GpCustomizations.Chmpt.IsServiceSKUEligible";
        public const string SPEUIsServiceSKUEligible = "GpCustomizations.Cpeur.IsServiceSKUEligible";
        public const string SPIsServiceSKUEligibleParam1 = "CustomerNumber";
        public const string SPIsServiceSKUEligibleParam2 = "IsEligibleServiceSKU";

        public const string SpnaFetchAccountDetails = "GPCustomizations.Chmpt.[NaGetQuoteAccountAddressReferences]";
        public const string SpeuFetchAccountDetails = "GPCustomizations.Cpeur.[EuGetQuoteAccountAddressReferences]";
        public const string SPFetchAccountDetailsParam1 = "CustomerId";

        public const string SpnaGetAccountHasOpenTransDetails = "GPCustomizations.Chmpt.[NaGetAccountHasOpenTransactions]";
        public const string SpeuGetAccountHasOpenTransDetails = "GPCustomizations.Cpeur.[EuGetAccountHasOpenTransactions]";
        public const string SPGetAccountHasOpenTransDetailsParam1 = "CustomerId";
        public const string SPGetAccountHasOpenTransDetailsParam2 = "Message";

        public const string SPGetWarehouseDeactivationDetails = "GPCustomizations.Common.Getwarehousedeactivationstatus";
        public const string SPGetWarehouseDeactivationDetailsParam1 = "WarehouseId";
        public const string SPGetWarehouseDeactivationDetailsParam2 = "CurrencyId";
        public const string SPGetWarehouseDeactivationDetailsParam3 = "isValid";


        public const string SPNaDeActivateCustomerInGP = "GPCustomizations.Chmpt.[NaDeActivateCustomerInGP]";
        public const string SPEuDeActivateCustomerInGP = "GPCustomizations.Cpeur.[EuDeActivateCustomerInGP]";
        public const string SPDeActivateCustomerInGPParam1 = "CustomerId";


        public const string SPNaIsCustomerExistsInGP = "GPCustomizations.Chmpt.[NaIsAccountExistsInGP]";
        public const string SPEuIsCustomerExistsInGP = "GPCustomizations.Cpeur.[EuIsAccountExistsInGP]";
        public const string SPIsCustomerExistsInGPParam1 = "CustomerId";
        public const string SPIsCustomerExistsInGPParam2 = "Isexists";


        public const string SPNaGetAvalaraRequestDetails = "GPCustomizations.Chmpt.[NaGetAvalaraRequestDetails]";
        public const string SPNaGetAvalaraRequestDetailsParam1 = "CustomerNumber";

        public const string SPNaGetCustomerDetailsToPushToAvalara = "GPCustomizations.Chmpt.FetchAvalaraDetails";
        public const string SPNaGetCustomerDetailsToPushToAvalaraParam1 = "Source";
        public const string SPNaGetCustomerDetailsToPushToAvalaraParam2 = "SourceValue";


        public const string SPNaDeActivateQuoteInGP = "GPCustomizations.Chmpt.[NaDeActivateQuoteInGP]";
        public const string SPEuDeActivateQuoteInGP = "GPCustomizations.Cpeur.[EuDeActivateQuoteInGP]";
        public const string SPDeActivateQuoteInGPParam1 = "QuoteId";
        public const string SPDeActivateQuoteInGPParam2 = "StatusCode";
        public const string SPDeActivateQuoteInGPParam3 = "StatusReason";




        #endregion CustomerIntegrations

        #region PostingUtility

        #region RemoveBatchLockedUsers
        public const string SPRemoveBatchLockedUsers = "Common.ClearBatchLock";
        public const string SPRemoveBatchLockedUsersBatchNumber = "BatchId";
        public const string SPRemoveBatchLockedUsersCompanyId = "CompanyId";
        public const string SPRemoveBatchLockedUsersIsBatchLockCleared = "IsBatchLockCleared";
        public const string SPRemoveBatchLockedUsersRemoveUsers = "LockingUsers";
        public const string SPRemoveBatchLockedUsersUserId = "UserId";
        #endregion RemoveBatchLockedUsers

        #region LockBatchforPosting
        public const string SPLockBatchForPostingNA = "Chmpt.LockBatchPosting";
        public const string SPLockBatchForPostingEU = "Cpeur.LockBatchPosting";
        public const string SPLockBatchForPostingBatchNumber = "Batchid";
        public const string SPLockBatchForPostingUserId = "Userid";
        #endregion LockBatchforPosting

        #region ValidateCadExemptionRules
        public const string SPValidateCadExemptionRules = "Chmpt.ProcessSOCanadianTaxExemptions";
        public const string SPValidateCadExemptionRulesBatchNumber = "BatchNumber";
        #endregion ValidateCadExemptionRules

        #region UnLockBatchforPosting
        public const string SPUnLockBatchforPostingNA = "Chmpt.UnLockBatchPosting";
        public const string SPUnLockBatchforPostingEU = "Cpeur.UnLockBatchPosting";
        public const string SPUnLockBatchforPostingBatchNumber = "BatchId";
        public const string SPUnLockBatchforPostingUserId = "UserId";
        #endregion UnLockBatchforPosting

        #region FetchNumTrxandBatchTotal
        public const string SPFetchNumTrxandBatchTotalNA = "Chmpt.GetTrxCountAndBatchTotal";
        public const string SPFetchNumTrxandBatchTotalEU = "Cpeur.GetTrxCountAndBatchTotal";
        public const string SPFetchNumTrxandBatchTotalBatchNumber = "BatchId";
        public const string SPFetchNumTrxandBatchTotalBatchTotal = "BatchTotal";
        public const string SPFetchNumTrxandBatchTotalCompanyId = "CompanyId";
        public const string SPFetchNumTrxandBatchTotalTransTotal = "TrxCount";
        public const string SPFetchNumTrxandBatchTotalUserId = "UserId";
        #endregion FetchNumTrxandBatchTotal

        #region ValidateBatchAndFetchData
        public const string SPValidateBatchAndFetchData = "Common.GetBatchTransandLockedUsersList";
        public const string SPValidateBatchAndFetchDataBatchNumber = "BatchId";
        public const string SPValidateBatchAndFetchDataCompanyId = "CompanyId";
        public const string SPValidateBatchAndFetchDataUserId = "UserId";
        #endregion ValidateBatchAndFetchData

        #region UpdateSalesBatchTotals
        public const string SPUpdateSalesBatchTotalsNA = "Chmpt.UpdateBatchTotals";
        public const string SPUpdateSalesBatchTotalsEU = "Cpeur.UpdateBatchTotals";
        public const string SPUpdateSalesBatchTotalsCompanyId = "CompanyId";
        public const string SPUpdateSalesBatchTotalsCompanyName = "CompanyName";
        public const string SPUpdateSalesBatchTotalsNewBatch = "NewBatchId";
        public const string SPUpdateSalesBatchTotalsOldBatch = "OldBatchId";
        public const string SPUpdateSalesBatchTotalsSopNumber = "OrderNumber";
        public const string SPUpdateSalesBatchTotalsStatus = "Status";
        public const string SPUpdateSalesBatchTotalsUserId = "UserId";
        #endregion UpdateSalesBatchTotals

        #region GetAllCanadianErrorTransactions

        public const string SPGetAllCanadianErrorTransactions = "Chmpt.GetAllCanadianErrorTransactions";
        public const string SPGetAllCanadianErrorTransactionsBatchNumber = "BatchId";
        public const string SPGetAllCanadianErrorTransactionssUserId = "UserId";

        #endregion GetAllCanadianErrorTransactions

        #region GetAllDropshipTransactions

        public const string SPGetAllDropshipTransactionsNA = "Chmpt.GetAllDropshipTransactions";
        public const string SPGetAllDropshipTransactionsEU = "Cpeur.GetAllDropshipTransactions";
        public const string SPGetAllDropshipTransactionsBatchNumber = "BatchId";
        public const string SPGetAllDropshipTransactionsUserId = "UserId";
        #endregion GetAllDropshipTransactions

        #region UpdateShipviatoDropshipTrans

        public const string SPUpdateShipviatoDropshipTransNA = "Chmpt.UpdateShipViaForDropshipTransactions";
        public const string SPUpdateShipviatoDropshipTransEU = "Cpeur.UpdateShipViaForDropshipTransactions";
        public const string SPUpdateShipviatoDropshipTransLineNumber = "LineSeqNumber";
        public const string SPUpdateShipviatoDropshipTransSopNumber = "InvoiceNumber";
        public const string SPUpdateShipviatoDropshipTransUserId = "UserId";

        #endregion UpdateShipviatoDropshipTrans

        #region GetAllDropshipErrorTransactions

        public const string SPGetAllDropshipErrorTransactionsNA = "Chmpt.GetAllDropshipErrorTransactions";
        public const string SPGetAllDropshipErrorTransactionsEU = "Cpeur.GetAllDropshipErrorTransactions";
        public const string SPGetAllDropshipErrorTransactionsBatchNumber = "BatchId";
        public const string SPGetAllDropshipErrorTransactionsUserId = "UserId";
        #endregion GetAllDropshipErrorTransactions

        #region GetAllCreditCardTransactions

        public const string SPGetAllCreditCardTransactions = "Chmpt.GetAllCreditCardTransactions";
        public const string SPGetAllCreditCardTransactionsBathNumber = "BatchId";
        public const string SPGetAllCreditCardTransactionsUserId = "UserId";
        #endregion GetAllCreditCardTransactions

        #region GetAllInvalidExchangeRateTransactions

        public const string SPGetAllInvalidExchangeRateTransactions = "Chmpt.GetAllInvalidExchangeRateTransactions";
        public const string SPGetAllInvalidExchangeRateTransactionsBathNumber = "BatchId";
        public const string SPGetAllInvalidExchangeRateTransactionsUserId = "UserId";

        #endregion GetAllInvalidExchangeRateTransactions

        #region GetAllDistributionErrorTransactions

        public const string SPGetAllDistributionErrorTransactionsNA = "Chmpt.GetAllDistributionErrorTransactions";
        public const string SPGetAllDistributionErrorTransactionsEU = "Cpeur.GetAllDistributionErrorTransactions";
        public const string SPGetAllDistributionErrorTransactionsBathNumber = "BatchId";
        public const string SPGetAllDistributionErrorTransactionsUserId = "UserId";

        #endregion GetAllDistributionErrorTransactions

        #region GetAllServiceSkuErrorTransactions

        public const string SPGetAllServiceSkuErrorTransactionsNA = "Chmpt.GetAllServiceSkuErrorTransactions";
        public const string SPGetAllServiceSkuErrorTransactionsEU = "Cpeur.GetAllServiceSkuErrorTransactions";
        public const string SPGetAllServiceSkuErrorTransactionsBathNumber = "BatchId";
        public const string SPGetAllServiceSkuErrorTransactionsUserId = "UserId";

        #endregion GetAllServiceSkuErrorTransactions

        #region GetAllInterCompanyTransactions

        public const string SPGetAllInterCompanyTransactionsNA = "Chmpt.GetAllInterCompanyTransactions";
        public const string SPGetAllInterCompanyTransactionsEU = "Cpeur.GetAllInterCompanyTransactions";
        public const string SPGetAllInterCompanyTransactionsBathNumber = "BatchId";
        public const string SPGetAllInterCompanyTransactionsUserId = "UserId";

        #endregion GetAllInterCompanyTransactions

        #region GetAllMissingShipViaTransactions

        public const string SpeuGetAllMissingShipViaTransactions = "Cpeur.GetAllShipViaMissingTransactions";
        public const string SpnaGetAllMissingShipViaTransactions = "Chmpt.GetAllShipViaMissingTransactions";
        public const string SPGetAllMissingShipViaTransactionsBathNumber = "BatchId";
        public const string SPGetAllMissingShipViaTransactionsUserId = "UserId";

        #endregion GetAllMissingShipViaTransactions

        #region GetAllIncorrectTaxScheduleIdTranscations

        public const string SPGetAllIncorrectTaxScheduleIdTranscations = "Cpeur.GetAllIncorrectTaxScheduleIdTransactions";
        public const string SPGetAllIncorrectTaxScheduleIdTranscationsBathNumber = "BatchId";
        public const string SPGetAllIncorrectTaxScheduleIdTranscationsUserId = "UserId";

        #endregion GetAllIncorrectTaxScheduleIdTranscations

        #region GetFailedPrePayment

        public const string SPGetNAFailedPrePaymentTransactions = "Chmpt.GetAllFailedPPDDetails";
        public const string SPGetEUFailedPrePaymentTransactions = "Cpeur.GetAllFailedPPDDetails";
        public const string GetFailedPrePaymentBatchId = "BatchId";
        public const string GetFailedPrePaymentUserId = "UserID";

        #endregion GetFailedPrePayment

        #region LinkedPaymentIssue

        public const string SPGetNALinkedPaymentIssuesTransactions = "Chmpt.GetAllInvoiceLessDocAmt";
        public const string SPGetEULinkedPaymentIssuesTransactions = "Cpeur.GetAllInvoiceLessDocAmt";
        public const string GetLinkedPaymentIssuesTransactionsBatchId = "BatchId";
        public const string GetLinkedPaymentIssuesTransactionsUserId = "UserID";

        #endregion LinkedPaymentIssue

        #region Currency

        public const string CanadianTaxIssueBatch = "Rev-CadTax";
        public const string CreditCardBatch = "Rev-CADCCards";
        public const string CurrencyBatch = "Rev-Currency";
        public const string DistributionIssueBatch = "Rev-Dist Issues";
        public const string InterCompanyBatch = "Rev-IntrCoSales";
        public const string MissingShipViaBatch = "Rev-MsngShipVia";
        public const string EuvatBatch = "Rev-VAT Issues";
        public const string ServiceSkuIssueBatch = "Rev-ServiceSku";
        public const string DropshipIssueBatch = "Rev-Dropship";
        public const string DocumentAmountIssueBatch = "Rev-AmtMisMatch";
        public const string LinkedPaymentBatch = "Rev-LinkedPmts";
        public const string FailedPrePaymentBatch = "Rev-FailPPD";

        #endregion Currency

        #endregion PostingUtility

        public const string NaspCommitSalesOrderReview = "GPCustomizations.chmpt.CPSalesCommitmentEngineOrderReview";
        public const string EuspCommitSalesOrderReview = "GPCustomizations.cpeur.CPSalesCommitmentEngineOrderReview";
        public const string SPCommitSalesOrderReviewParam1 = "SopNumbe";
        public const string SPCommitSalesOrderReviewParam2 = "SopType";
        public const string SPCommitSalesOrderReviewParam3 = "CustomerId";
        public const string SPCommitSalesOrderReviewParam4 = "UserName";
        public const string SPCommitSalesOrderReviewParam5 = "Source";

        public const string EuspOrderStatusToWhiBoard = "GPCustomizations.Cpeur.EUSaveSalesOrderstoWHiboard";
        public const string NaspOrderStatusToWhiBoard = "GPCustomizations.Chmpt.NASaveSalesOrderstoWHiboard";
        public const string SopNumberParam = "SopNumber";
        public const string SopTypeParam = "SopType";


        public const string NaspUpdateSofoToInvoice = "GPCustomizations.chmpt.UpdateSalesOrderInvoiceStatus";
        public const string EuspUpdateSofoToInvoice = "GPCustomizations.Cpeur.UpdateSalesOrderInvoiceStatus";
        public const string SourceParam = "Source";
        //public const string Source = "GP";

        #region VATHold
        public const string VatHoldUpdateSP = "Cpeur.UpdateEUVAtHold";
        public const string VatLocationCode = "LocationCode";
        public const string VatSubTotal = "SubTotal";
        public const string VatTaxScheduleId = "TaxScheduleId";
        public const string VatTaxRegNumebr = "TaxRegNumebr";
        public const string VatShiptoAddress = "ShiptoAddress";
        public const string VatUserId = "UserId";
        #endregion VATHold

        #region MovePostedInv
        public const string NaspMovepostedinvoice = "GPCustomizations.Chmpt.Movepostedinvoice";
        public const string EuspMovepostedinvoice = "GPCustomizations.Cpeur.Movepostedinvoice";
        #endregion

        #region Cash Application Process

        public const string SPDistributeAmountToCashApplyInvoicesNA = "GPCustomizations.Chmpt.DistributeAmountToCashApplyInvoices";
        public const string SPDistributeAmountToCashApplyInvoicesEU = "GPCustomizations.Cpeur.DistributeAmountToCashApplyInvoices";
        public const string SPDistributeAmountToCashApplyInvoicesParam1 = "SopType";
        public const string SPDistributeAmountToCashApplyInvoicesParam2 = "SopNumber";
        public const string SPDistributeAmountToCashApplyInvoicesParam3 = "UserId";

        public const string SPGetDocumentsForCashEngineNA = "GPCustomizations.Chmpt.GetDocumentsForCashEngine";
        public const string SPGetDocumentsForCashEngineEU = "GPCustomizations.Cpeur.GetDocumentsForCashEngine";
        public const string SPSaveDocumentsForCashEngineNA = "GPCustomizations.Chmpt.SaveDocumentsForCashEngine";
        public const string SPSaveDocumentsForCashEngineEU = "GPCustomizations.Cpeur.SaveDocumentsForCashEngine";
        public const string SPSaveDocumentsForCashEngineParam1 = "DocumentsForCashEngineType";
        //Fetch
        public const string SPGetInvoiceLineDetailsNA = "GPCustomizations.Chmpt.GetInvoiceLineDetails";
        public const string SPGetInvoiceLineDetailsEU = "GPCustomizations.Cpeur.GetInvoiceLineDetails";
        public const string SPGetInvoiceLineDetailsParam1 = "RMDocumentType";
        public const string SPGetInvoiceLineDetailsParam2 = "RMDocumentNumber";
        public const string SPGetInvoiceLineDetailsParam3 = "CustomerNumber";
        public const string SPGetInvoiceLineDetailsParam4 = "Source";
        //Save
        public const string SPSaveAppliedInvoiceDetailsNA = "GPCustomizations.chmpt.SaveAppliedInvoiceDetails";
        public const string SPSaveAppliedInvoiceDetailsEU = "GPCustomizations.cpeur.SaveAppliedInvoiceDetails";
        public const string SPSaveAppliedInvoiceDetailsParam1 = "RMDocumentType";
        public const string SPSaveAppliedInvoiceDetailsParam2 = "RMDocumentNumber";
        public const string SPSaveAppliedInvoiceDetailsParam3 = "CustomerNumber";
        public const string SPSaveAppliedInvoiceDetailsParam4 = "TypeId";
        public const string SPSaveAppliedInvoiceDetailsParam5 = "Details";
        public const string SPSaveAppliedInvoiceDetailsParam6 = "UserId";
        public const string SPSaveAppliedInvoiceDetailsParam7 = "LineDetails";
        //Distribute
        public const string SPDistributeAmountToSelectedLinesNA = "GPCustomizations.chmpt.DistributeAmountToSelectedLines";
        public const string SPDistributeAmountToSelectedLinesEU = "GPCustomizations.cpeur.DistributeAmountToSelectedLines";
        public const string SPDistributeAmountToSelectedLinesParam1 = "LineDetails";
        public const string SPDistributeAmountToSelectedLinesParam2 = "TotalAmount";
        public const string SPDistributeAmountToSelectedLinesParam3 = "DistributionType";

        public const string SPNAFetchCashApplyInvoices = "GpCustomizations.Chmpt.FetchCashApplyInvoices";
        public const string SPEUFetchCashApplyInvoices = "GpCustomizations.Cpeur.FetchCashApplyInvoices";
        public const string FetchCashApplyInvoicesParam1 = "CustomerNumber";
        public const string FetchCashApplyInvoicesParam2 = "DocumentNumber";
        public const string FetchCashApplyInvoicesParam3 = "DocumentType";
        public const string FetchCashApplyInvoicesParam4 = "CurrencyID";
        public const string FetchCashApplyInvoicesParam5 = "Source";

        public const string SPNASaveApplyToOpenOrder = "GpCustomizations.Chmpt.Savecashapplieddetails";
        public const string SPEUSaveApplyToOpenOrder = "GpCustomizations.Cpeur.Savecashapplieddetails";
        public const string SaveApplyToOpenOrderParam1 = "AppliedOpenSalesDocumentsType";
        public const string SaveApplyToOpenOrderParam2 = "UserId";
        public const string SaveApplyToOpenOrderParam3 = "Status";

        public const string SPNAUpdateCashApplyInvoices = "GpCustomizations.Chmpt.UpdateCashApplyInvoices";
        public const string SPEUUpdateCashApplyInvoices = "GpCustomizations.Cpeur.UpdateCashApplyInvoices";
        public const string UpdateCashApplyInvoicesParam1 = "DocumentNumber";
        public const string UpdateCashApplyInvoicesParam2 = "DocumentType";
        public const string UpdateCashApplyInvoicesParam3 = "CustomerNumber";
        public const string UpdateCashApplyInvoicesParam4 = "UserId";

        public const string SPNADeleteCashApplyInvoices = "GpCustomizations.Chmpt.DeleteCashApplyInvoices";
        public const string SPEUDeleteCashApplyInvoices = "GpCustomizations.Cpeur.DeleteCashApplyInvoices";
        public const string DeleteCashApplyInvoicesParam1 = "DocumentNumber";
        public const string DeleteCashApplyInvoicesParam2 = "DocumentType";

        #region EFT
        /*******Start : EFT Customer Mapping Window********/
        public const string SPNAGetEFTCustomerMappingDetails = "Gpcustomizations.Chmpt.GetEftCustomerReferences";
        public const string SPEUGetEFTCustomerMappingDetails = "Gpcustomizations.Cpeur.GetEftCustomerReferences";
        public const string GetEFTCustomerDetailsParam1 = "CustomerId";

        public const string SPNASaveEftCustomerReferences = "Gpcustomizations.Chmpt.SaveEftCustomerReferences";
        public const string SPEUSaveEftCustomerReferences = "Gpcustomizations.Cpeur.SaveEftCustomerReferences";
        public const string SaveEFTCustomerDetailsParam1 = "EftCustomerReferences";
        /*******End: EFT Customer Mapping Window********/

        /*******Start : EFT Import Bank Summary Window********/
        public const string SPSaveimportedeftdetailsNA = "GPCustomizations.Chmpt.SaveImportedEftBankDetails";
        public const string SPSaveimportedeftdetailsEU = "GPCustomizations.Cpeur.SaveImportedEftBankDetails";
        public const string SPSaveimportedeftdetailsOriginal = "EftBankSummary";
        public const string SPSaveimportedeftdetailsActual = "EFTTransaction";
        public const string SaveimportedeftdetailsFileParam = "FileName";
        public const string SaveimportedeftdetailsUserNameParam = "UserName";
        /*******END : EFT Import Bank Summary Window********/


        /*******Start : EFT Import CTX Window********/
        public const string SPSaveOriginalRemittanceFileDetailsNA = "Gpcustomizations.Chmpt.SaveImportedRemittanceFileDetails";
        public const string SPSaveOriginalRemittanceFileDetailsEU = "Gpcustomizations.Cpeur.SaveImportedRemittanceFileDetails";

        public const string BankRemittanceOrigType = "EFTRemittance";
        public const string BankRemittanceOrigMapType = "EFTRemittanceApply";

        public const string SPFilevalidations = "GPCustomizations.Chmpt.GetEftFileProcessedStatus";

        public const string SPInsertEFTTransactionDetailsNA = "Gpcustomizations.Chmpt.InsertEFTTransactionDetails";
        //public const string SPInsertEFTTransactionMapDetailsNA = "Gpcustomizations.Chmpt.InsertEFTTransactionMapDetails";

        public const string SPGetEftOpenTransactionDetailNA = "GPCustomizations.Chmpt.GetEftOpenTransactions";
        public const string SPGetEftOpenTransactionDetailEU = null;

        /*******END : EFT Import CTX Window********/


        /*******Start : EFT Bank Entry Window********/
        public const string SPNAGetEFTCustomerRemittanceTransactions = "Gpcustomizations.chmpt.GetEFTBankEntryDetails";
        public const string SPEUGetEFTCustomerRemittanceTransactions = "Gpcustomizations.cpeur.GetEFTBankEntryDetails";
        public const string GetEFTCustomerRemittanceTransactionsBatchId = "BatchId";
        public const string GetEFTCustomerRemittanceTransactionsParam1 = "CustomerIdStart";
        public const string GetEFTCustomerRemittanceTransactionsParam2 = "CustomerIdEnd";
        public const string GetEFTCustomerRemittanceTransactionsParam3 = "DateStart";
        public const string GetEFTCustomerRemittanceTransactionsParam4 = "DateEnd";
        public const string GetEFTCustomerRemittanceTransactionsParam5 = "ReferenceNoStart";
        public const string GetEFTCustomerRemittanceTransactionsParam6 = "ReferenceNoEnd";
        public const string GetEFTCustomerRemittanceTransactionsParam7 = "DocNumbrStart";
        public const string GetEFTCustomerRemittanceTransactionsParam8 = "DocNumbrend";
        public const string GetEFTCustomerRemittanceTransactionsParam9 = "ActionType";

        public const string SPNAGetEFTPaymentRemittanceTransactions = "Gpcustomizations.chmpt.GetEFTRemittancePaymentTransactions";
        public const string SPEUGetEFTPaymentRemittanceTransactions = "Gpcustomizations.cpeur.GetEFTRemittancePaymentTransactions";
        public const string GetEFTPaymentRemittanceTransactionsParam1 = "CustomerIdStart";
        public const string GetEFTPaymentRemittanceTransactionsParam2 = "CustomerIdEnd";
        public const string GetEFTPaymentRemittanceTransactionsParam3 = "DateStart";
        public const string GetEFTPaymentRemittanceTransactionsParam4 = "DateEnd";
        public const string GetEFTPaymentRemittanceTransactionsParam5 = "ReferenceNoStart";
        public const string GetEFTPaymentRemittanceTransactionsParam6 = "ReferenceNoEnd";
        public const string GetEFTPaymentRemittanceTransactionsParam7 = "DocNumbrStart";
        public const string GetEFTPaymentRemittanceTransactionsParam8 = "DocNumbrend";
        public const string GetEFTPaymentRemittanceTransactionsParam9 = "ActionType";
        public const string GetEFTPaymentRemittanceTransactionsParam10 = "BatchId";

        public const string SPNASaveEFTCustomerTransactions = "Gpcustomizations.chmpt.SaveEftCustomerPayment";
        public const string SPEUSaveEFTCustomerTransactions = "Gpcustomizations.Cpeur.SaveEftCustomerPayment";
        public const string SaveEFTCustomerTransactionsParam1 = "EftCustomerPaymentType";

        public const string SPNAEFTPaymentApplyRemittanceDetails = "Gpcustomizations.chmpt.GetEFTPushToGPPaymentTransactions";
        public const string SPEUEFTPaymentApplyRemittanceDetails = "Gpcustomizations.Cpeur.GetEFTPushToGPPaymentTransactions";
        public const string SPEFTPaymentApplyRemittanceDetailsParam1 = "EFTCustomerPaymentReference";
        public const string SPEFTPaymentApplyRemittanceDetailsParam2 = "ActionType";

        public const string SPNAUpdateEFTPaymentTransactionsDetails = "Gpcustomizations.chmpt.UpdateEFTPaymentTransactions";
        public const string SPEUUpdateEFTPaymentTransactionsDetails = "Gpcustomizations.Cpeur.UpdateEFTPaymentTransactions";
        public const string SPUpdateEFTPaymentTransactionsDetailsParam1 = "PaymentNumber";
        public const string SPUpdateEFTPaymentTransactionsDetailsParam2 = "ReferenceNumber";
        public const string SPUpdateEFTPaymentTransactionsDetailsParam3 = "UserName";

        public const string SPNAUpdateEFTApplyTransactionsDetails = "Gpcustomizations.chmpt.UpdateEFTApplyTransactions";
        public const string SPEUUpdateEFTApplyTransactionsDetails = "Gpcustomizations.Cpeur.UpdateEFTApplyTransactions";
        public const string SPUpdateEFTApplyTransactionsDetailsParam1 = "EFTId";
        public const string SPUpdateEFTApplyTransactionsDetailsParam2 = "EFTApplyType";

        public const string SPGetNextAvailablePaymentNumberNA = "GPCustomizations.Chmpt.NaGetNextAvailablePaymentNumber";
        public const string SPGetNextAvailablePaymentNumberEU = "GPCustomizations.Cpeur.NaGetNextAvailablePaymentNumber";
        public const string SPGetNextAvailablePaymentNumberParam1 = "PaymentType";
        public const string SPGetNextAvailablePaymentNumberParam2 = "PaymentNumber";

        public const string SPNAGetEFTCustomerDetailsForLookup = "Gpcustomizations.Chmpt.GetEFTCustomerDetails";
        public const string SPEuGetEFTCustomerDetailsForLookup = "Gpcustomizations.Cpeur.GetEFTCustomerDetails";

        public const string SPNAGetEFTBatchIdForLookup = "Gpcustomizations.Chmpt.GetBatchIdDetails";
        public const string SPEuGetEFTBatchIdForLookup = "Gpcustomizations.Cpeur.GetBatchIdDetails";

        public const string SPNAGetEFTDocumentNumberForLookup = "Gpcustomizations.chmpt.GetEFTItemReferenceDetails";
        public const string SPEUGetEFTDocumentNumberForLookup = "Gpcustomizations.Cpeur.GetEFTItemReferenceDetails";
        public const string GetEFTDocumentNumberForLookupParam1 = "ActionType";

        public const string SPNAGetEFTReferenceNumberForLookup = "Gpcustomizations.chmpt.GetEFTReferenceDetails";
        public const string SPEUGetEFTReferenceNumberForLookup = "Gpcustomizations.Cpeur.GetEFTReferenceDetails";
        public const string EFTReferenceNumberForLookupParam1 = "ReferenceId";

        public const string SPNAFetchCustomerIdForReference = "Gpcustomizations.chmpt.FetchCustomerId";
        public const string SPEUFetchCustomerIdForReference = "Gpcustomizations.cpeur.FetchCustomerId";
        public const string SPNAFetchCustomerIdForReferenceParam1 = "ReferenceNumber";

        public const string SPValidatePaymentRemittanceNA = "Chmpt.ValidateEFTRemittancePaymentTransactions";
        public const string SPValidatePaymentRemittanceEU = "Cpeur.ValidateEFTRemittancePaymentTransactions";
        public const string SPValidatePaymentRemittanceParam1 = "EFTCustomerPaymentReference";
        public const string SPValidatePaymentRemittanceParam2 = "ActionType";

        public const string SPNAGetEFTEmailRemittanceTransactions = "Gpcustomizations.chmpt.GeteftMailReferenceTransactions";
        public const string SPEUGetEFTEmailRemittanceTransactions = "Gpcustomizations.cpeur.GeteftMailReferenceTransactions";
        public const string GetEFTEmailRemittanceTransactionsParam1 = "CustomerIdStart";
        public const string GetEFTEmailRemittanceTransactionsParam2 = "CustomerIdEnd";
        public const string GetEFTEmailRemittanceTransactionsParam3 = "DateStart";
        public const string GetEFTEmailRemittanceTransactionsParam4 = "DateEnd";
        public const string GetEFTEmailRemittanceTransactionsParam5 = "ReferenceNoStart";
        public const string GetEFTEmailRemittanceTransactionsParam6 = "ReferenceNoEnd";
        public const string GetEFTEmailRemittanceTransactionsParam7 = "DocNumbrStart";
        public const string GetEFTEmailRemittanceTransactionsParam8 = "DocNumbrend";
        public const string GetEFTEmailRemittanceTransactionsParam9 = "Source";

        public const string SPNAFetchEftPaymentAmountDetails = "Gpcustomizations.Chmpt.FetchEftPaymentAmountDetails";
        public const string SPEUFetchEftPaymentAmountDetails = "Gpcustomizations.cpeur.FetchEftPaymentAmountDetails";
        public const string GetEFTFetchEftPaymentAmountDetailsParam1 = "EftBatchNumber";

        public const string SPNAValidateEFTCustomer = "Gpcustomizations.Chmpt.ValidateCustomerId";
        public const string SPEUValidateEFTCustomer = "Gpcustomizations.Cpeur.ValidateCustomerId";
        public const string ValidateEFTCustomerParam1 = "CustomerId";
        public const string ValidateEFTCustomerParam2 = "isValid";

        public const string SPNAValidateEFTReference = "Gpcustomizations.Chmpt.ValidateEFTReferenceDetails";
        public const string SPEUValidateEFTReference = "Gpcustomizations.Cpeur.ValidateEFTReferenceDetails";
        public const string ValidateEFTReferenceParam1 = "ReferenceNumber";
        public const string ValidateEFTReferenceParam2 = "ActionType";
        public const string ValidateEFTReferenceParam3 = "isValid";

        public const string SPNAValidateEFTItemReference = "Gpcustomizations.Chmpt.ValidateItemReferenceDetails";
        public const string SPEUValidateEFTItemReference = "Gpcustomizations.Cpeur.ValidateItemReferenceDetails";
        public const string ValidateEFTItemReferenceParam1 = "ItemReferenceNumber";
        public const string ValidateEFTItemReferenceParam2 = "isValid";


        public const string UpdateEFTTransactionDetailsType = "UpdateEFTTransactionDetailsType";
        public const string InsertEFTTransactionDetailsType = "InsertEFTTransactionDetailsType";

        public const string EFTUpdateTransactionApplyDetailsType = "EFTUpdateTransactionApplyDetailsType";
        public const string EFTInsertTransactionApplyDetailsType = "EFTInsertTransactionApplyDetailsType";

        public const string UserNameParam = "UserName";
        public const string SPGetEftTransactionDetailNA = "Gpcustomizations.chmpt.GetEftTransactionDetail";

        public const string SPValidateEFTLineNA = "Gpcustomizations.chmpt.ValidateEFTLine";
        public const string EFTSummaryLineReferenceType = "EFTSummaryLineReference";
        public const string EFTSummaryLineItemReferenceType = "EFTSummaryLineItemReference";


        public const string SPNASaveEFTMailTransactions = "Gpcustomizations.chmpt.SaveEftMailReferenceTransactions";
        public const string SPEUSaveEFTMailTransactions = "Gpcustomizations.Cpeur.SaveEftMailReferenceTransactions";
        public const string SaveEFTMailTransactionsParam1 = "EFTMailType";

        public const string SPNADeleteItemReferenceNumber = "Gpcustomizations.chmpt.DeleteItemReferenceNumber";
        public const string SPEUDeleteItemReferenceNumber = "Gpcustomizations.cpeur.DeleteItemReferenceNumber";
        public const string SPNADeleteItemReferenceNumberParam1 = "EftAppId";

        public const string SPNADeleteEFTTransactionCustomerRemittance = "Gpcustomizations.chmpt.DeleteEFTTransactionFromCustomerBankEntry";
        public const string SPEUDeleteEFTTransactionCustomerRemittance = "Gpcustomizations.cpeur.DeleteEFTTransactionFromCustomerBankEntry";
        public const string SPNADeleteEFTTransactionCustomerRemittanceParam1 = "EftId";

        public const string SPNAUpdateEftEmailRemittancesToPayments = "Gpcustomizations.chmpt.NAUpdateEftEmailRemittancesToPayments";
        public const string SPEUUpdateEftEmailRemittancesToPayments = "Gpcustomizations.cpeur.NAUpdateEftEmailRemittancesToPayments";
        public const string SPNAUpdateEftEmailRemittancesToPaymentsParam1 = "NaEftFileId";
        public const string SPNAValidateEFTEmailReference = "Gpcustomizations.Chmpt.ValidateEmailRemittanceDetails";
        public const string SPEUValidateEFTEmailReference = "Gpcustomizations.Cpeur.ValidateEmailRemittanceDetails";
        public const string ValidateEFTEmailReferenceParam1 = "EftEmailReference";
        public const string ValidateEFTEmailReferenceParam2 = "EftItemReference";
        public const string ValidateEFTEmailReferenceParam3 = "IsAvailable";

        #region EFTEmail
        public const string SPNADeleteEFTEmailRemittance = "Gpcustomizations.Chmpt.DeleteEFTEmailRemittance";
        public const string SPEUDeleteEFTEmailRemittance = "Gpcustomizations.Cpeur.DeleteEFTEmailRemittance";
        public const string DeleteEFTEmailRemittanceParam1 = "EftRowId";

        public const string SPNAFetchEmailReferenceScroll = "Gpcustomizations.Chmpt.FetchEmailReferences";
        public const string SPEUFetchEmailReferenceScroll = "Gpcustomizations.Cpeur.FetchEmailReferences";
        public const string FetchEmailReferenceScrollParam1 = "EftRowId";

        public const string SPNAFetchEmailReference = "Gpcustomizations.Chmpt.FetchEmailReferenceLookup";
        public const string SPEUFetchEmailReference = "Gpcustomizations.Cpeur.FetchEmailReferenceLookup";

        public const string SPNASaveEmailReferences = "Gpcustomizations.Chmpt.SaveEmailReferences";
        public const string SPEUSaveEmailReferences = "Gpcustomizations.Cpeur.SaveEmailReferences";
        public const string SaveEmailReferencesParam1 = "EftRowId";
        public const string SaveEmailReferencesParam2 = "ReferenceNumber";
        public const string SaveEmailReferencesParam3 = "PaymentAmount";
        public const string SaveEmailReferencesParam4 = "CustomerID";
        public const string SaveEmailReferencesParam5 = "CurrencyID";
        public const string SaveEmailReferencesParam6 = "DateReceived";
        public const string SaveEmailReferencesParam7 = "EFTEmailLineType";

        #endregion EFTEmail

        #endregion EFT
        #endregion Cash Application Process

        #region EmailService
        public const string GREETINGS_MESSAGE = "Hello,<br>";
        public const string TABLE_BORDER = "<html><body><table border='1'>";
        public const string TABLEROW_START = "<tr align='center'>";
        public const string TABLEROW_END = "</tr>";
        public const string TABLECOLH_START = "<th align='center'  style='font-weight:bold;' >";
        public const string TABLECOLH_END = "</th>";
        public const string TABLECOL_START = "<td align='center'>";
        public const string TABLECOL_END = "</td>";
        public const string TABLE_END = "</table><br>";
        public const string HTMLEND = "</body></html>";
        public const string BREAK_LINE = "<br>";

        public const string SPGetEmailConfiguration = "GPCustomizations.common.GetEmailConfiguration";
        public const string SPGetEmailConfiguration_param1 = "ConfigID";
        #endregion EmailService

        #region POCostMgt


        public const string SPValidateEFTCustomerRemittanceSummaryNA = "chmpt.Validateeftsummary";
        public const string SPValidateEFTCustomerRemittanceSummaryEU = "cpeur.Validateeftsummary";
        public const string SPValidateEFTCustomerRemittanceSummaryParam1 = "EFTCustomerPaymentReference";

        #endregion

        #region Cryptogrpahy
        public const string CryptoSuccess = "Success";
        public const string CryptoFailure = "Failure";
        #endregion

        #region FTP
        public const string FtpSuccess = "Success";
        public const string FtpFailure = "Failure";
        public const int FtpAdminId = 615;
        public const int ChmptId = 1;
        public const int CpeurId = 2;
        #endregion

        #region BlackLine

        public const string SPSaveFtpLog = "GpTrace.Common.SaveFTPLog";
        public const string SPFetchftpconfigdetails = "GpCustomizations.Common.Fetchftpconfigdetails";
        public const string SPGetUploadReportConfigDetails = "GpCustomizations.Common.GetUploadReportConfigDetails";

        #endregion

        #region Tax

        public const string SPGetCanadianTaxEligibleDetails = "GpCustomizations.chmpt.GetCanadianTaxEligibleDetails";
        public const string OrderDtlsType = "OrderDtlsType";
        public const string CanadianTaxUserCode = "CAN";

        #endregion

        public const string SPProcessDocumentFreightHoldNA = "GPCustomizations.Chmpt.CpDocumentFreightHold";

        public const string SPProcessDocumentExportHoldNA = "GPCustomizations.Chmpt.CpApplyAndValidateExportHold";
        public const string SPProcessDocumentExportHoldShipFromCountry = "ShipFromCountry";
        public const string SPProcessDocumentExportHoldShipFromCC = "ShipFromCountryCode";
        public const string SPProcessDocumentExportHoldShipToCountry = "ShipToCountry";
        public const string SPProcessDocumentExportHoldShipToCC = "ShipToCountryCode";
        public const string SPProcessDocumentExportHoldFinalCountry = "FinalDestCountry";
        public const string SPProcessDocumentExportHoldFinalCC = "FinalDestCountryCode";
        public const string SPProcessDocumentExportHoldNewShipFromCountry = "NewShipFromCountry";
        public const string SPProcessDocumentExportHoldNewShipFromCC = "NewShipFromCountryCode";
        public const string SPProcessDocumentExportHoldNewShipToCountry = "NewShipToCountry";
        public const string SPProcessDocumentExportHoldNewShipToCC = "NewShipToCountryCode";
        public const string SPProcessDocumentExportHoldNewFinalCountry = "NewFinalDestCountry";
        public const string SPProcessDocumentExportHoldNewFinalCC = "NewFinalDestCountryCode";
        public const string SPProcessDocumentExportHoldResult = "Result";
        public const string SPProcessDocumentExportHoldIsLineAdd = "IsLineAdded";


    }
}

