using Chempoint.GP.Infrastructure.Config;
using Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider.Context;
using Chempoint.GP.Infrastructure.Maps.Purchase;
using Chempoint.GP.Infrastructure.Utils;
using Chempoint.GP.Model.Interactions.PayableManagement;
using Chempoint.GP.Model.Interactions.Purchases;
using ChemPoint.GP.DataContracts.Purchase;
using ChemPoint.GP.Entities.BaseEntities;
using ChemPoint.GP.Entities.Business_Entities.Purchase;
using ChemPoint.GP.Podl.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ChemPoint.GP.Podl
{
    public class PORepositoryDL : RepositoryBase, IPurchaseOrderRepository
    {
        public PORepositoryDL(string connectionString) : base(new GpAddInDbContext(connectionString))
        {
        }

        #region PO Indicator

        public IEnumerable<PurchaseIndicatorEntity> GetPoIndicatorDetail(PurchaseOrderInformation poIndicator, int companId)
        {
            var cmd = CreateStoredProcCommand(companId == 1 ? Configuration.SPFetchPOIndicatorDetailsNA : Configuration.SPFetchPOIndicatorDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPFetchPOIndicatorDetailsParam1, SqlDbType.VarChar, poIndicator.PoNumber, 31);
            cmd.Parameters.AddInputParams(Configuration.SPFetchPOIndicatorDetailsParam2, SqlDbType.Int, poIndicator.PoLineNumber, 4);
            return base.FindAll<PurchaseIndicatorEntity, PurchaseIndicatorMap>(cmd);
        }

        public object SavePoIndicatorDetail(PurchaseIndicatorRequest poIndicator, int companId)
        {
            DataTable poIndicatorDT = DataTableMapper.GetDataTable(new List<PurchaseIndicatorEntity>() { poIndicator.PurchaseIndicatorEntity },
                DataColumnMappings.SavePurchaseIndicator);
            var cmd = CreateStoredProcCommand(companId == 1 ? Configuration.SPSavePOIndicatorDetailsNA : Configuration.SPSavePOIndicatorDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPSavePOIndicatorDetailsParam1, SqlDbType.VarChar, poIndicator.UserId, 50);
            cmd.Parameters.AddInputParams(Configuration.SPSavePOIndicatorDetailsParam2, SqlDbType.Int, poIndicator.CompanyID, 2);
            cmd.Parameters.AddInputParams(Configuration.SPSavePOIndicatorDetailsParam3, SqlDbType.Structured, poIndicatorDT);
            return base.Insert(cmd);
        }

        public object DeletePoIndicatorDetail(PurchaseOrderInformation poIndicator, int companId)
        {
            var cmd = CreateStoredProcCommand(companId == 1 ? Configuration.SPDeletePOIndicatorDetailsNA : Configuration.SPDeletePOIndicatorDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPDeletePOIndicatorDetailsParam1, SqlDbType.VarChar, poIndicator.PoNumber, 50);
            cmd.Parameters.AddInputParams(Configuration.SPDeletePOIndicatorDetailsParam2, SqlDbType.Int, poIndicator.PoLineNumber, 4);
            return base.Delete(cmd);
        }

        #endregion PO Indicator

        #region POCostMgt

        public void SavePoCostManagementChangestoAudit(DataTable savePOUnitCostDetails, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPInsertPoCostManagementChangestoAuditNA : Configuration.SPInsertPoCostManagementChangestoAuditEU);
            cmd.Parameters.AddInputParams(Configuration.SPInsertPoCostManagementChangestoAuditParam1, SqlDbType.VarChar, companyId == 1 ? "Chmpt" : "Cpeur", 6);
            cmd.Parameters.AddInputParams(Configuration.SPInsertPoCostManagementChangestoAuditParam2, SqlDbType.Structured, savePOUnitCostDetails);
            base.Update(cmd);
        }

        public DataSet ValidatePoCostChanges(DataTable costMgntDetails, int curcnyView, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPValidatePoCostManagementDetailsNA : Configuration.SPValidatePoCostManagementDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPValidatePoCostManagementDetailsParam1, SqlDbType.Structured, costMgntDetails);
            cmd.Parameters.AddInputParams(Configuration.SPValidatePoCostManagementDetailsParam2, SqlDbType.Int, curcnyView);
            return base.GetDataSet(cmd);
        }

        public void UpdatePoCostNotes(decimal noteIndex, string costNotes, int companyId)//decimal noteIndex, string costNotes, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPUpdatePoCostManagementDetailsNoteNA : Configuration.SPUpdatePoCostManagementDetailsNoteEU);
            cmd.Parameters.AddInputParams(Configuration.SPUpdatePoCostManagementDetailsNoteParam1, SqlDbType.Decimal, noteIndex, 50);
            cmd.Parameters.AddInputParams(Configuration.SPUpdatePoCostManagementDetailsNoteParam2, SqlDbType.Text, costNotes);
            base.Update(cmd);
        }

        public DataSet FetchCostBookModifiedDetails(int intCompanyId)
        {
            var cmd = CreateStoredProcCommand(intCompanyId == 1 ? Configuration.SPFetchCostBookCostModifiedDetailsNA : Configuration.SPFetchCostBookCostModifiedDetailsEU);
            return base.GetDataSet(cmd);
        }

        public void UpdateHasCostVariance(DataTable hasCostVarianceDT, int intCompanyId)
        {
            var cmd = CreateStoredProcCommand(intCompanyId == 1 ? Configuration.SPUpdateHasCostVarianceNA : Configuration.SPUpdateHasCostVarianceEU);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateHasCostVarianceParam1, SqlDbType.Structured, hasCostVarianceDT);
            base.Update(cmd);
        }

        public void SavePoUnitCostDetails(DataTable savePoUnitCostDT, string UserId, int intCompanyId)
        {
            var cmd = CreateStoredProcCommand(intCompanyId == 1 ? Configuration.SPSavePoUnitCostDetailsNA : Configuration.SPSavePoUnitCostDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPSavePoUnitCostDetailsParam1, SqlDbType.VarChar, UserId);
            cmd.Parameters.AddInputParams(Configuration.SPSavePoUnitCostDetailsParam2, SqlDbType.Structured, savePoUnitCostDT);
            base.Update(cmd);
        }

        public void UpdatePoCostProactiveMailStatus(DataTable dtUpdatePoCost, int intCompanyId)
        {
            var cmd = CreateStoredProcCommand(intCompanyId == 1 ? Configuration.SPUpdatePoCostProactiveMailStatusNA : Configuration.SPUpdatePoCostProactiveMailStatusEU);
            cmd.Parameters.AddInputParams(Configuration.SPProcessIdsTypeParam, SqlDbType.Structured, dtUpdatePoCost);
            base.Update(cmd);
        }

        #endregion

        #region MaterialMgt

        //public PurchaseOrderResponse ValidatePoForMailApproval(string poNumber, string vendorId, string userId, int companyId)
        //{
        //    string strFirstPO = string.Empty;
        //    DataSet firstPODT = new DataSet();
        //    DataTable firstPOWithoutNotes = new DataTable();
        //    PurchaseOrderResponse response = null;
        //    try
        //    {
        //        response = new PurchaseOrderResponse();
        //        PurchaseOrderEntity purchaseOrderEntity = new PurchaseOrderEntity();
        //        PurchaseCostManagement purchaseCostManagement = new PurchaseCostManagement();
        //        var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPValidatePoForMailApprovalNA : Configuration.SPValidatePoForMailApprovalEU);
        //        cmd.Parameters.AddInputParams(Configuration.SPValidatePoForMailApprovalParam1, SqlDbType.VarChar, poNumber);
        //        cmd.Parameters.AddInputParams(Configuration.SPValidatePoForMailApprovalParam2, SqlDbType.VarChar, vendorId);
        //        cmd.Parameters.AddInputParams(Configuration.SPValidatePoForMailApprovalParam3, SqlDbType.VarChar, userId);
        //        cmd.Parameters.AddOutputParams(Configuration.SPValidatePoForMailApprovalParam4, SqlDbType.Bit);
        //        cmd.Parameters.AddOutputParams(Configuration.SPValidatePoForMailApprovalParam5, SqlDbType.Bit);
        //        firstPODT = base.GetDataSet(cmd);

        //        IEnumerable<string> query = from dt in firstPODT.Tables[0].AsEnumerable()
        //                                    select dt.Field<string>("Notes");
        //        List<string> str = new List<string>();

        //        foreach (string values in query)
        //        {
        //            str.Add(values);
        //        }

        //        //firstPOWithoutNotes = firstPODT.Tables[0];
        //        //firstPOWithoutNotes.Columns.RemoveAt(firstPOWithoutNotes.Columns.Count - 1);


        //        firstPODT.Tables[0].Columns.RemoveAt(firstPODT.Tables[0].Columns.Count - 1);

        //        SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
        //        purchaseCostManagement.IsFirstPoForMaterialMgt = Convert.ToBoolean(commandResult.Parameters["@" + Configuration.SPValidatePoForMailApprovalParam4].Value);
        //        purchaseCostManagement.IsValidForMaterialMgt = Convert.ToBoolean(commandResult.Parameters["@" + Configuration.SPValidatePoForMailApprovalParam5].Value);
        //        purchaseCostManagement.strPoCostNotesHistory = str;
        //        purchaseOrderEntity.PurchaseCostMgtInformation = purchaseCostManagement;
        //        response.PurchaseOrderEntity = purchaseOrderEntity;

        //        if (purchaseCostManagement.IsFirstPoForMaterialMgt == true ||
        //            purchaseCostManagement.IsValidForMaterialMgt == true)
        //        {
        //            if (firstPODT.Tables[0] != null)
        //                response.Report = ConvertDataTableToString(firstPODT.Tables[0]);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //    return response;
        //}

        public PurchaseOrderResponse ValidatePoForMailApproval(string poNumber, string vendorId, string userId, int companyId)
        {
            string strFirstPO = string.Empty;
            DataSet firstPODT = new DataSet();
            DataTable materialMgtDT = new DataTable();
            PurchaseOrderResponse response = null;
            try
            {
                response = new PurchaseOrderResponse();
                PurchaseOrderEntity purchaseOrderEntity = new PurchaseOrderEntity();
                PurchaseCostManagement purchaseCostManagement = new PurchaseCostManagement();
                var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPValidatePoForMailApprovalNA : Configuration.SPValidatePoForMailApprovalEU);
                cmd.Parameters.AddInputParams(Configuration.SPValidatePoForMailApprovalParam1, SqlDbType.VarChar, poNumber);
                cmd.Parameters.AddInputParams(Configuration.SPValidatePoForMailApprovalParam2, SqlDbType.VarChar, vendorId);
                cmd.Parameters.AddInputParams(Configuration.SPValidatePoForMailApprovalParam3, SqlDbType.VarChar, userId);
                cmd.Parameters.AddOutputParams(Configuration.SPValidatePoForMailApprovalParam4, SqlDbType.Bit);
                cmd.Parameters.AddOutputParams(Configuration.SPValidatePoForMailApprovalParam5, SqlDbType.Bit);
                firstPODT = base.GetDataSet(cmd);
                if (firstPODT.Tables[0] != null && firstPODT.Tables[0].Rows.Count>0)
                {
                    IEnumerable<string> query = from dt in firstPODT.Tables[0].AsEnumerable()
                                                select dt.Field<string>("Notes");
                    List<string> str = new List<string>();

                    foreach (string values in query)
                    {
                        str.Add(values);
                    }

                    //firstPOWithoutNotes = firstPODT.Tables[0];
                    //firstPOWithoutNotes.Columns.RemoveAt(firstPOWithoutNotes.Columns.Count - 1);


                    firstPODT.Tables[0].Columns.RemoveAt(firstPODT.Tables[0].Columns.Count - 1);

                    SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
                    purchaseCostManagement.IsFirstPoForMaterialMgt = Convert.ToBoolean(commandResult.Parameters["@" + Configuration.SPValidatePoForMailApprovalParam4].Value);
                    purchaseCostManagement.IsValidForMaterialMgt = Convert.ToBoolean(commandResult.Parameters["@" + Configuration.SPValidatePoForMailApprovalParam5].Value);
                    purchaseCostManagement.strPoCostNotesHistory = str;
                    purchaseOrderEntity.PurchaseCostMgtInformation = purchaseCostManagement;
                    response.PurchaseOrderEntity = purchaseOrderEntity;

                    if (purchaseCostManagement.IsFirstPoForMaterialMgt == true)
                    {
                        firstPODT.Tables[0].Columns.Remove("POLineItemNumber");
                        firstPODT.Tables[0].Columns.Remove("ItemDescription");
                        firstPODT.Tables[0].Columns.Remove("ProposedUnitCost");
                        firstPODT.Tables[0].Columns.Remove("CostVariance");
                        firstPODT.Tables[0].Columns.Remove("ApprovalRequired");
                    }
                    else if (purchaseCostManagement.IsValidForMaterialMgt == true)
                    {
                        firstPODT.Tables[0].Columns.Remove("ItemClassId");
                        firstPODT.Tables[0].Columns.Remove("QtyOrdered");
                        firstPODT.Tables[0].Columns.Remove("LbsPurchased");
                        firstPODT.Tables[0].Columns.Remove("ExtendedPrice");

                        firstPODT.Tables[0].Columns["UnitCost"].ColumnName = "CostEntered";
                        firstPODT.Tables[0].Columns["ProposedUnitCost"].ColumnName = "CurrentCost";

                        for (int i = 0; i < firstPODT.Tables[0].Rows.Count;)// i++)
                        {
                            DataRow dr = firstPODT.Tables[0].Rows[i];
                            if ((Convert.ToDecimal(firstPODT.Tables[0].Rows[i]["PoCostVariance"]) < 20 &&
                                Convert.ToDecimal(firstPODT.Tables[0].Rows[i]["PoCostVariance"]) > -20) &&
                                 Convert.ToDecimal(firstPODT.Tables[0].Rows[i]["PoCostVariance"])==0)
                            {
                                firstPODT.Tables[0].Rows.Remove(dr);
                                i--;
                            }
                            i++;
                        }
                    }

                    if (firstPODT.Tables[0] != null)
                        response.Report = ConvertDataTableToString(firstPODT.Tables[0]);

                }
                else
                {
                    purchaseCostManagement.IsFirstPoForMaterialMgt = false;
                    purchaseCostManagement.IsValidForMaterialMgt = false;
                    purchaseCostManagement.strPoCostNotesHistory = null;
                    purchaseOrderEntity.PurchaseCostMgtInformation = purchaseCostManagement;
                    response.PurchaseOrderEntity = purchaseOrderEntity;
                    response.Report = string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return response;
        }
        private static string ConvertDataTableToString(DataTable dataTable)
        {
            string data = string.Empty;
            int rowsCount = dataTable.Rows.Count;
            for (int rowValue = 0; rowValue < rowsCount; rowValue++)
            {
                DataRow row = dataTable.Rows[rowValue];
                int columnsCount = dataTable.Columns.Count;
                for (int colValue = 0; colValue < columnsCount; colValue++)
                {
                    data += dataTable.Columns[colValue].ColumnName + "~" + row[colValue];
                    if (colValue == columnsCount - 1)
                    {
                        if (rowValue != (rowsCount - 1))
                            data += "#";
                    }
                    else
                        data += "|";
                }
            }
            return data;
        }

        #endregion

        /// <summary>
        /// Fetch Create Activity Records.
        /// </summary>
        /// <param name="poActivityRequest"></param>
        /// <returns></returns>
        public PurchaseOrderResponse CreateActivity(PurchaseOrderRequest poActivityRequest)
        {
            PurchaseOrderResponse poActivityResponse = new PurchaseOrderResponse();
            List<Integration> integrationList = new List<Integration>();
            var cmd = CreateStoredProcCommand(poActivityRequest.CompanyID == 1 ? Configuration.SPNAGetPOActivity : Configuration.SPEUGetPOActivity);

            DataSet ds = base.GetDataSet(cmd);
            integrationList = base.GetAllDifferentEntities<Integration, XRMCreateActivityMap>(ds.Tables[0]);
            poActivityResponse.XRMIntegrationList = integrationList;
            poActivityResponse.Status = ResponseStatus.Success;
            return poActivityResponse;
        }

        /// <summary>
        /// Log Created Activity record to Table.
        /// </summary>
        /// <param name="poActivityRequest"></param>
        /// <returns></returns>
        public PurchaseOrderResponse LogActivity(PurchaseOrderRequest poActivityRequest)
        {
            PurchaseOrderResponse poActivityResponse = new PurchaseOrderResponse();

            DataTable poActivityDT = DataTableMapper.SaveXRMActivityLOG(poActivityRequest,
                DataColumnMappings.SaveXRMActivityLOG);

            var cmd = CreateStoredProcCommand(poActivityRequest.CompanyID == 1 ? Configuration.SPUpdatePoActivityLogNA : Configuration.SPUpdatePoActivityLogEU);

            cmd.Parameters.AddInputParams(Configuration.UpdatePOActivityLogTypeParam, SqlDbType.Structured, poActivityDT);
            base.Update(cmd);
            poActivityResponse.Status = ResponseStatus.Success;
            return poActivityResponse;
        }

        #region LandedCost

        public void SaveEstimatedShipmentCostEntry(PurchaseOrderRequest purchaseOrderRequest, int companyId)
        {
            DataTable EstimatedShipmentDT = DataTableMapper.SaveEstimatedShipmentCostEntry(purchaseOrderRequest,
               DataColumnMappings.SaveEstimatedShipmentCostEntry);

            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPSaveEstimatedShipmentCostEntryNA : Configuration.SPSaveEstimatedShipmentCostEntryEU);
            cmd.Parameters.AddInputParams(Configuration.SPSaveEstimatedShipmentCostEntryParam1, SqlDbType.Int, purchaseOrderRequest.PurchaseShipmentEstimateDetails.EstimateID);
            cmd.Parameters.AddInputParams(Configuration.SPSaveEstimatedShipmentCostEntryParam2, SqlDbType.VarChar, purchaseOrderRequest.PurchaseShipmentEstimateDetails.Warehouse);
            cmd.Parameters.AddInputParams(Configuration.SPSaveEstimatedShipmentCostEntryParam3, SqlDbType.VarChar, purchaseOrderRequest.PurchaseShipmentEstimateDetails.Vendor);
            cmd.Parameters.AddInputParams(Configuration.SPSaveEstimatedShipmentCostEntryParam4, SqlDbType.VarChar, purchaseOrderRequest.PurchaseShipmentEstimateDetails.CarrierReference);
            cmd.Parameters.AddInputParams(Configuration.SPSaveEstimatedShipmentCostEntryParam5, SqlDbType.Decimal, purchaseOrderRequest.PurchaseShipmentEstimateDetails.EstimatedCost);
            cmd.Parameters.AddInputParams(Configuration.SPSaveEstimatedShipmentCostEntryParam6, SqlDbType.DateTime, purchaseOrderRequest.PurchaseShipmentEstimateDetails.EstimatedShipDate);
            cmd.Parameters.AddInputParams(Configuration.SPSaveEstimatedShipmentCostEntryParam7, SqlDbType.VarChar, purchaseOrderRequest.PurchaseShipmentEstimateDetails.CurrencyId);
            cmd.Parameters.AddInputParams(Configuration.SPSaveEstimatedShipmentCostEntryParam10, SqlDbType.Decimal, purchaseOrderRequest.PurchaseShipmentEstimateDetails.TotalNetWeight);
            cmd.Parameters.AddInputParams(Configuration.SPSaveEstimatedShipmentCostEntryParam13, SqlDbType.VarChar, purchaseOrderRequest.PurchaseShipmentEstimateDetails.EstimatedShipmentNotes);
            cmd.Parameters.AddInputParams(Configuration.SPSaveEstimatedShipmentCostEntryParam14, SqlDbType.VarChar, purchaseOrderRequest.PurchaseShipmentEstimateDetails.UserId);
            cmd.Parameters.AddInputParams(Configuration.SPSaveEstimatedShipmentCostEntryParam15, SqlDbType.Structured, EstimatedShipmentDT);
            cmd.Parameters.AddInputParams(Configuration.SPSaveEstimatedShipmentCostEntryParam16, SqlDbType.Decimal, purchaseOrderRequest.PurchaseShipmentEstimateDetails.ExchangeRate);
            cmd.Parameters.AddInputParams(Configuration.SPSaveEstimatedShipmentCostEntryParam17, SqlDbType.DateTime, purchaseOrderRequest.PurchaseShipmentEstimateDetails.ExchangeExpirationDate);
            base.Update(cmd);
        }

        public DataSet FetchEstimatedShipmentDetails(PurchaseOrderRequest purchaseOrderRequest, int companyId)
        {
            DataTable EstimatedShipmentDT = DataTableMapper.SaveEstimatedShipmentCostEntry(purchaseOrderRequest,
              DataColumnMappings.SaveEstimatedShipmentCostEntry);
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPFetchEstimatedShipmentDetails : Configuration.SPFetchEstimatedShipmentDetails);
            cmd.Parameters.AddInputParams(Configuration.SPFetchEstimatedShipmentDetailsParam1, SqlDbType.Structured, EstimatedShipmentDT);
            return base.GetDataSet(cmd);
        }

        public DataSet GetShipmentEstimateDetails(int intCompanyId, int intEstimateId)
        {
            var cmd = CreateStoredProcCommand(intCompanyId == 1 ? Configuration.SPNAGetShipmentEstimateDetails : Configuration.SPEUGetShipmentEstimateDetails);
            cmd.Parameters.AddInputParams(Configuration.SPGetShipmentEstimateDetailsParam1, SqlDbType.Int, intEstimateId);
            return base.GetDataSet(cmd);
        }

        public DataSet GetShipmentEstimateInquiryDetails(int intCompanyId, int intEstimateId)
        {
            var cmd = CreateStoredProcCommand(intCompanyId == 1 ? Configuration.SPGetShipmentEstimateInquiryDetails : Configuration.SPGetShipmentEstimateInquiryDetails);
            cmd.Parameters.AddInputParams(Configuration.SPGetShipmentEstimateInquiryDetailsParam1, SqlDbType.Int, intEstimateId);
            return base.GetDataSet(cmd);
        }

        public DataSet GetPOShipmentEstimateDetails(int intCompanyId, string strPONumber)
        {
            var cmd = CreateStoredProcCommand(intCompanyId == 1 ? Configuration.SPNAGetPOShipmentEstimateDetails : Configuration.SPEUGetPOShipmentEstimateDetails);
            cmd.Parameters.AddInputParams(Configuration.SPGetPOShipmentEstimateDetailsParam1, SqlDbType.VarChar, strPONumber);
            return base.GetDataSet(cmd);
        }

        public object DeleteEstimateLineDetails(int intCompanyId, string strPONumber, int intPOLineNumber, string userName)
        {
            var cmd = CreateStoredProcCommand(intCompanyId == 1 ? Configuration.SPNADeleteestimatelinedetails : Configuration.SPEUDeleteestimatelinedetails);
            cmd.Parameters.AddInputParams(Configuration.SPDeleteestimatelinedetailsParam1, SqlDbType.VarChar, strPONumber);
            cmd.Parameters.AddInputParams(Configuration.SPDeleteestimatelinedetailsParam2, SqlDbType.Int, intPOLineNumber);
            cmd.Parameters.AddInputParams(Configuration.SPDeleteestimatelinedetailsParam3, SqlDbType.VarChar, userName);
            return base.Delete(cmd);
        }

        public DataSet GetEstimateId(int intCompanyId, int windowValue)
        {
            var cmd = CreateStoredProcCommand(intCompanyId == 1 ? Configuration.SPGetEstimateIdNA : Configuration.SPGetEstimateIdEU);
            cmd.Parameters.AddInputParams(Configuration.SPGetEstimateIdParam1, SqlDbType.Int, windowValue);
            return base.GetDataSet(cmd);
        }
        public object DeleteEstimatedId(int intCompanyId, int intEstimateId, string userName)
        {
            var cmd = CreateStoredProcCommand(intCompanyId == 1 ? Configuration.SPDeleteEstimatedIdNA : Configuration.SPDeleteEstimatedIdNA);
            cmd.Parameters.AddInputParams(Configuration.SPDeleteEstimatedIdParam1, SqlDbType.Int, intEstimateId);
            cmd.Parameters.AddInputParams(Configuration.SPDeleteEstimatedIdParam2, SqlDbType.VarChar, userName);
            return base.Delete(cmd);
        }

        public int GetNextEstimateId(int intCompanyId)
        {
            int nextNumber;
            var cmd = CreateStoredProcCommand(intCompanyId == 1 ? Configuration.SPNAGetNextEstimateId : Configuration.SPEUGetNextEstimateId);
            cmd.Parameters.AddInputParams(Configuration.SPNAGetNextEstimateIdParam1, SqlDbType.Int, intCompanyId);
            cmd.Parameters.AddOutputParams(Configuration.SPNAGetNextEstimateIdParam2, SqlDbType.Int);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            nextNumber = Convert.ToInt16(commandResult.Parameters["@" + Configuration.SPNAGetNextEstimateIdParam2].Value);
            return nextNumber;
        }


        public DataSet GetCurrencyIndex(PurchaseOrderRequest purchaseOrderRequest, int companyId)
        {
            PurchaseOrderResponse purchaseOrderResponse = new PurchaseOrderResponse();
            PurchaseShipmentEstimateDetails purchaseShipmentEstimateDetails = new PurchaseShipmentEstimateDetails();
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetCurrencyIndex : Configuration.SPGetCurrencyIndex);
            cmd.Parameters.AddInputParams(Configuration.SPGetCurrencyIndexParam1, SqlDbType.VarChar, purchaseOrderRequest.PurchaseShipmentEstimateDetails.CurrencyId.ToString().Trim());
            return base.GetDataSet(cmd);
        }

        /// <summary>
        /// To get Total net weight 
        /// </summary>
        /// <param name="intCompanyId"></param>
        /// <param name="itemNumber"></param>
        /// <param name="ShippedQty"></param>
        /// <returns></returns>
        public decimal GetShipmentQtyTotal(int intCompanyId, string itemNumber, decimal ShippedQty)
        {
            decimal totalValue = 0.0m;
            var cmd = CreateStoredProcCommand(intCompanyId == 1 ? Configuration.SPGetShipmentQtyTotal : Configuration.SPGetShipmentQtyTotal);
            cmd.Parameters.AddInputParams(Configuration.SPGetShipmentQtyTotalParam1, SqlDbType.VarChar, itemNumber);
            cmd.Parameters.AddInputParams(Configuration.SPGetShipmentQtyTotalParam2, SqlDbType.Decimal, ShippedQty);
            totalValue = Convert.ToDecimal(base.GetSingle(cmd));
            return totalValue;
        }


        public DataSet GetPoNumber(int intCompanyId, string locationCode, string vendorId)
        {
            var cmd = CreateStoredProcCommand(intCompanyId == 1 ? Configuration.SPGetPoNumber : Configuration.SPGetPoNumber);
            cmd.Parameters.AddInputParams(Configuration.SPGetPoNumberParam1, SqlDbType.VarChar, locationCode);
            cmd.Parameters.AddInputParams(Configuration.SPGetPoNumberParam2, SqlDbType.VarChar, vendorId);
            return base.GetDataSet(cmd);
        }

        public int ValidateCarrierReference(int intCompanyId, int estimateId, string CarrierReference)
        {
            int isAvailable;
            var cmd = CreateStoredProcCommand(intCompanyId == 1 ? Configuration.SPValidateCarrierReference : Configuration.SPValidateCarrierReference);
            cmd.Parameters.AddInputParams(Configuration.SPValidateCarrierReferenceParam1, SqlDbType.Int, estimateId);
            cmd.Parameters.AddInputParams(Configuration.SPValidateCarrierReferenceParam2, SqlDbType.VarChar, CarrierReference);
            cmd.Parameters.AddOutputParams(Configuration.SPValidateCarrierReferenceParam3, SqlDbType.Decimal);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            isAvailable = Convert.ToInt16(commandResult.Parameters["@" + Configuration.SPValidateCarrierReferenceParam3].Value);
            return isAvailable;
        }

        #endregion

        #region Purchase Order | default BuyerID to improve user experience

        public int ValidatePurchaseBuyerId(string poNumber, int companyId)
        {
            int nextNumber;
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPValidatePurchaseBuyerIdNA : Configuration.SPValidatePurchaseBuyerIdEU);
            cmd.Parameters.AddInputParams(Configuration.SPValidatePurchaseBuyerIdParam1, SqlDbType.VarChar, poNumber);
            cmd.Parameters.AddInputParams(Configuration.SPValidatePurchaseBuyerIdParam2, SqlDbType.Int, companyId);
            cmd.Parameters.AddOutputParams(Configuration.SPValidatePurchaseBuyerIdParam3, SqlDbType.Int);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            nextNumber = Convert.ToInt16(commandResult.Parameters["@" + Configuration.SPValidatePurchaseBuyerIdParam3].Value);
            return nextNumber;
        }
        #endregion

        #region Elemica

        public DataSet RetrieveElemicaDetail(PurchaseElemicaRequest poElemicaRequest, int companyId)
        {
            PurchaseElemicaResponse poElemicaResponse = new PurchaseElemicaResponse();
            PurchaseElemicaEntity purchaseElemicaEntity = new PurchaseElemicaEntity();

            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPRetrieveElemicaDetailNA : Configuration.SPRetrieveElemicaDetailEU);
            cmd.Parameters.AddInputParams(Configuration.SPRetrieveElemicaDetailParam1, SqlDbType.VarChar, poElemicaRequest.purchaseEntityDetails.PurchaseOrderDetails.PoNumber.ToString().Trim());
            cmd.Parameters.AddInputParams(Configuration.SPRetrieveElemicaDetailParam2, SqlDbType.VarChar, poElemicaRequest.purchaseEntityDetails.PurchaseOrderDetails.VendorId.ToString().Trim());

            return base.GetDataSet(cmd);
        }

        public object SendElemicaDetail(PurchaseElemicaRequest poElemicaRequest, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPSendElemicaDetailNA : Configuration.SPSendElemicaDetailEU);
            cmd.Parameters.AddInputParams(Configuration.SPSendElemicaDetailParam1, SqlDbType.VarChar, poElemicaRequest.purchaseEntityDetails.PurchaseOrderDetails.PoNumber.ToString().Trim());
            return base.Update(cmd);

        }

        public object UpdatePOStatusForElemica(PurchaseElemicaRequest poElemicaRequest, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPUpdatePOStatusForElemicaNA : Configuration.SPUpdatePOStatusForElemicaEU);
            cmd.Parameters.AddInputParams(Configuration.SPUpdatePOStatusForElemicaParam1, SqlDbType.VarChar, poElemicaRequest.purchaseEntityDetails.PurchaseOrderDetails.PoNumber.ToString().Trim());
            return base.Update(cmd);
        }

        #endregion

    }
}
