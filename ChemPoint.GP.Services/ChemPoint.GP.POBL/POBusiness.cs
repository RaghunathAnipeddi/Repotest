using ChemPoint.GP.BusinessContracts.Purchase;
using ChemPoint.GP.DataContracts.Purchase;
using Chempoint.GP.Infrastructure.Utils;
using System;
using System.Linq;
using Chempoint.GP.Model.Interactions.Purchases;
using System.Data;
using System.Collections.Generic;
using System.Reflection;
using ChemPoint.GP.XrmServices;
using ChemPoint.GP.Entities.BaseEntities;
using System.Text;
using Chempoint.GP.Infrastructure.Logging;
using ChemPoint.GP.Entities.Business_Entities.Purchase;
using Chempoint.GP.Infrastructure.DataAccessEngine.Extensions;
using ChemPoint.GP.Podl.Utils;
using Chempoint.GP.Model.Interactions.Email;
using ChemPoint.GP.Email;

namespace ChemPoint.GP.Pobl
{
    public class POBusiness : IPurchaseOrderBusiness
    {
        public POBusiness()
        {
        }

        public PurchaseIndicatorResponse GetPoIndicatorDetail(PurchaseIndicatorRequest poIndicatorRequest)
        {
            PurchaseIndicatorResponse poIndicatorResponse = null;
            IPurchaseOrderRepository poIndicatorDataAccess = null;

            poIndicatorRequest.ThrowIfNull("Purchase Indicator");
            poIndicatorRequest.PoIndicatorBase.ThrowIfNull("poIndicatorRequest.PurchaseIndicatorBase.GetPurchaseIndicatorDetail");
            try
            {
                poIndicatorResponse = new PurchaseIndicatorResponse();
                poIndicatorDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(poIndicatorRequest.ConnectionString);
                var poIndicatorDetailList = poIndicatorDataAccess.GetPoIndicatorDetail(poIndicatorRequest.PoIndicatorBase, poIndicatorRequest.CompanyID);
                poIndicatorResponse.PurchaseIndicatorList = poIndicatorDetailList.ToList().FirstOrDefault();
                poIndicatorResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                poIndicatorResponse.Status = ResponseStatus.Error;
                poIndicatorResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return poIndicatorResponse;
        }

        public PurchaseIndicatorResponse SavePoIndicatorDetail(PurchaseIndicatorRequest poIndicatorRequest)
        {
            PurchaseIndicatorResponse poIndicatorResponse = null;
            IPurchaseOrderRepository poIndicatorDataAccess = null;

            poIndicatorRequest.ThrowIfNull("Sales Entry");
            poIndicatorRequest.PurchaseIndicatorEntity.ThrowIfNull("poIndicatorRequest.PurchaseIndicatorEntity.SavePurchaseIndicatorDetail");

            try
            {
                poIndicatorResponse = new PurchaseIndicatorResponse();
                poIndicatorDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(poIndicatorRequest.ConnectionString);
                poIndicatorDataAccess.SavePoIndicatorDetail(poIndicatorRequest, poIndicatorRequest.CompanyID);
                poIndicatorResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                poIndicatorResponse.Status = ResponseStatus.Error;
                poIndicatorResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return poIndicatorResponse;
        }

        public PurchaseIndicatorResponse DeletePoIndicatorDetail(PurchaseIndicatorRequest poIndicatorRequest)
        {
            PurchaseIndicatorResponse poIndicatorResponse = null;
            IPurchaseOrderRepository poIndicatorDataAccess = null;

            poIndicatorRequest.ThrowIfNull("Sales Entry");
            poIndicatorRequest.PoIndicatorBase.ThrowIfNull("poIndicatorRequest.PoIndicatorBase.DeletePoIndicatorDetail");

            try
            {
                poIndicatorResponse = new PurchaseIndicatorResponse();
                poIndicatorDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(poIndicatorRequest.ConnectionString);
                poIndicatorDataAccess.DeletePoIndicatorDetail(poIndicatorRequest.PoIndicatorBase, poIndicatorRequest.CompanyID);
                poIndicatorResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                poIndicatorResponse.Status = ResponseStatus.Error;
                poIndicatorResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return poIndicatorResponse;
        }

        #region POCostMgt

        public PurchaseOrderResponse ValidatePoCostChanges(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse purchaseOrderResponse = null;
            IPurchaseOrderRepository poIndicatorDataAccess = null;
            DataTable poCostMgntTable = null;
            try
            {
                purchaseOrderResponse = new PurchaseOrderResponse();
                poCostMgntTable = new DataTable();
                //poCostMgntTable = ToDataTable(purchaseOrderRequest.PurchaseCostMgt);
                poCostMgntTable = DataTableMapper.ValidatePOCostMgtDetails(purchaseOrderRequest,
                DataColumnMappings.ValidatePOCostMgt);

                //poCostMgntTable.Columns.Remove("PoType");
                //poCostMgntTable.Columns.Remove("PoLineStatus");
                //poCostMgntTable.Columns.Remove("ProposedUnitCost");
                //poCostMgntTable.Columns.Remove("CostSupportId");
                //poCostMgntTable.Columns.Remove("CostBookCost");
                //poCostMgntTable.Columns.Remove("CostNotes");
                //poCostMgntTable.Columns.Remove("LineCostNotes");
                //poCostMgntTable.Columns.Remove("CurrencyIndex");
                //poCostMgntTable.Columns.Remove("IsUpdated");
                //poCostMgntTable.Columns.Remove("ModifiedOn");
                //poCostMgntTable.Columns.Remove("Reason");
                //poCostMgntTable.Columns.Remove("HasCostVariance");
                //poCostMgntTable.Columns.Remove("IsDeleted");

                if (poCostMgntTable != null && poCostMgntTable.Rows.Count > 0)
                {
                    DataSet fetchTransactionDs = new DataSet();
                    poIndicatorDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);
                    fetchTransactionDs = poIndicatorDataAccess.ValidatePoCostChanges(poCostMgntTable, purchaseOrderRequest.CurrencyView, purchaseOrderRequest.CompanyID);
                    if (fetchTransactionDs.Tables.Count > 0 && fetchTransactionDs.Tables[0].Rows.Count > 0)
                    {
                        purchaseOrderResponse.PurchaseCostMgt = new List<Entities.BaseEntities.PurchaseCostManagement>();
                        foreach (DataRow row in fetchTransactionDs.Tables[0].Rows)
                        {
                            Entities.BaseEntities.PurchaseCostManagement PurchaseCostMgtInformation = new Entities.BaseEntities.PurchaseCostManagement();

                            PurchaseCostMgtInformation.PoNumber = string.IsNullOrEmpty(row["PoNumber"].ToString()) ? string.Empty : row["PoNumber"].ToString().Trim();
                            PurchaseCostMgtInformation.VendorId = string.IsNullOrEmpty(row["VendorId"].ToString()) ? string.Empty : row["VendorId"].ToString().Trim();
                            PurchaseCostMgtInformation.Ord = Convert.ToInt32(row["Ord"]);
                            PurchaseCostMgtInformation.LineNumber = Convert.ToInt16(row["LineNumber"]);
                            PurchaseCostMgtInformation.ItemNumber = string.IsNullOrEmpty(row["ItemNumber"].ToString()) ? string.Empty : row["ItemNumber"].ToString().Trim();
                            PurchaseCostMgtInformation.UOfM = string.IsNullOrEmpty(row["UOfM"].ToString()) ? string.Empty : row["UOfM"].ToString().Trim();
                            PurchaseCostMgtInformation.UnitCost = Convert.ToDecimal(row["UnitCost"]);
                            PurchaseCostMgtInformation.ProposedUnitCost = Convert.ToDecimal(row["ProposedUnitCost"]);
                            PurchaseCostMgtInformation.CostStatus = Convert.ToInt16(row["CostStatus"]);
                            PurchaseCostMgtInformation.CostSupportId = string.IsNullOrEmpty(row["CostSupportId"].ToString()) ? string.Empty : row["CostSupportId"].ToString().Trim();
                            PurchaseCostMgtInformation.CostBookCost = string.IsNullOrEmpty(row["CostBookCost"].ToString()) ? string.Empty : row["CostBookCost"].ToString().Trim();
                            //PurchaseCostMgtInformation.QtyCancel = Convert.ToDecimal(row["QtyCance"]);
                            //PurchaseCostMgtInformation.QtyOrder = Convert.ToDecimal(row["QtyOrder"]);
                            PurchaseCostMgtInformation.CostNotes = string.IsNullOrEmpty(row["Notes"].ToString()) ? string.Empty : row["Notes"].ToString().Trim();
                            PurchaseCostMgtInformation.NoteIndex = Convert.ToDecimal(row["NoteIndex"]);
                            PurchaseCostMgtInformation.CurrencyIndex = Convert.ToInt16(row["CURRNIDX"]);
                            PurchaseCostMgtInformation.UserId = string.IsNullOrEmpty(row["UserId"].ToString()) ? string.Empty : row["UserId"].ToString().Trim();
                            PurchaseCostMgtInformation.LineCostNotes = string.IsNullOrEmpty(row["LineCostNotes"].ToString()) ? string.Empty : row["LineCostNotes"].ToString().Trim();
                            PurchaseCostMgtInformation.ItemDescription = string.IsNullOrEmpty(row["ItemDescription"].ToString()) ? string.Empty : row["ItemDescription"].ToString().Trim();
                            PurchaseCostMgtInformation.CostSupportCost = Convert.ToDecimal(row["CostSupportCost"]);
                            PurchaseCostMgtInformation.PoCostVariance = Convert.ToDecimal(row["POVariance"]);
                            PurchaseCostMgtInformation.Reason = string.IsNullOrEmpty(row["PoReasonCode"].ToString()) ? string.Empty : row["PoReasonCode"].ToString().Trim();                            
                            PurchaseCostMgtInformation.PoLineCostSource = string.IsNullOrEmpty(row["PoLineCostSource"].ToString()) ? string.Empty : row["PoLineCostSource"].ToString().Trim();

                            purchaseOrderResponse.PurchaseCostMgt.Add(PurchaseCostMgtInformation);
                        }
                    }
                }

                purchaseOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                purchaseOrderResponse.Status = ResponseStatus.Error;
                purchaseOrderResponse.ErrorMessage = "ValidatePoCostChanges" + ex.Message.ToString().Trim();
            }
            return purchaseOrderResponse;
        }

        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            //put a breakpoint here and check datatable
            return dataTable;
        }

        public PurchaseOrderResponse SavePoCostManagementChangestoAudit(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse purchaseOrderResponse = null;
            IPurchaseOrderRepository poIndicatorDataAccess = null;
            DataTable savePOCostMgntTable = null;
            try
            {
                purchaseOrderResponse = new PurchaseOrderResponse();
                savePOCostMgntTable = new DataTable();
                savePOCostMgntTable = DataTableMapper.SavePoCostMgtChangestoAudit(purchaseOrderRequest,
               DataColumnMappings.SavePoCostMgtChangestoAudit);

                //savePOCostMgntTable = ToDataTable(purchaseOrderRequest.PurchaseCostMgt);
                //savePOCostMgntTable.Columns.Remove("VendorId");
                //savePOCostMgntTable.Columns.Remove("PoType");
                //savePOCostMgntTable.Columns.Remove("PoLineStatus");
                //savePOCostMgntTable.Columns.Remove("UOfM");
                //savePOCostMgntTable.Columns.Remove("ProposedUnitCost");
                //savePOCostMgntTable.Columns.Remove("CostSupportId");
                //savePOCostMgntTable.Columns.Remove("CostBookCost");
                //savePOCostMgntTable.Columns.Remove("QtyCancel");
                //savePOCostMgntTable.Columns.Remove("CostNotes");
                //savePOCostMgntTable.Columns.Remove("LineCostNotes");
                //savePOCostMgntTable.Columns.Remove("NoteIndex");
                //savePOCostMgntTable.Columns.Remove("CurrencyIndex");
                //savePOCostMgntTable.Columns.Remove("IsUpdated");
                //savePOCostMgntTable.Columns.Remove("ModifiedOn");
                //savePOCostMgntTable.Columns.Remove("Reason");
                //savePOCostMgntTable.Columns.Remove("HasCostVariance");
                //savePOCostMgntTable.Columns.Remove("CostStatus");
                //savePOCostMgntTable.Columns.Remove("IsDeleted");

                if (savePOCostMgntTable != null && savePOCostMgntTable.Rows.Count > 0)
                {
                    poIndicatorDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);
                    poIndicatorDataAccess.SavePoCostManagementChangestoAudit(savePOCostMgntTable, purchaseOrderRequest.CompanyID);
                }

                purchaseOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                purchaseOrderResponse.Status = ResponseStatus.Error;
                purchaseOrderResponse.ErrorMessage = "Error:SavePoCostManagementChangestoAudit" + ex.Message.ToString().Trim();
            }

            return purchaseOrderResponse;
        }

        public PurchaseOrderResponse UpdatePoCostNotes(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse purchaseOrderResponse = null;
            IPurchaseOrderRepository poIndicatorDataAccess = null;
            try
            {
                purchaseOrderResponse = new PurchaseOrderResponse();
                poIndicatorDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);

                var poCostMgtList = from d in purchaseOrderRequest.PurchaseCostMgt
                                    select d;

                if (purchaseOrderRequest.PurchaseCostMgt != null && purchaseOrderRequest.PurchaseCostMgt.Count > 0)
                {
                    foreach (var poCostMgt in poCostMgtList)
                    {
                        if (!String.IsNullOrEmpty(poCostMgt.CostNotes))
                            poIndicatorDataAccess.UpdatePoCostNotes(poCostMgt.NoteIndex, poCostMgt.CostNotes, purchaseOrderRequest.CompanyID);
                    }
                }
                purchaseOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                purchaseOrderResponse.Status = ResponseStatus.Error;
                purchaseOrderResponse.ErrorMessage = "UpdatePoCostNotes" + ex.Message.ToString().Trim();
            }
            return purchaseOrderResponse;
        }

        public PurchaseOrderResponse FetchCostBookModifiedDetails(PurchaseOrderRequest objPurchaseOrderRequest)
        {

            PurchaseOrderResponse poIndicatorResponse = null;
            IPurchaseOrderRepository poResponseDataAccess = null;
            try
            {
                poIndicatorResponse = new PurchaseOrderResponse();
                poResponseDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(objPurchaseOrderRequest.ConnectionString);
                DataSet ds = poResponseDataAccess.FetchCostBookModifiedDetails(objPurchaseOrderRequest.CompanyID);

                if (ds.Tables[0] != null)

                    poIndicatorResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                poIndicatorResponse.Status = ResponseStatus.Error;
                poIndicatorResponse.ErrorMessage = "FetchCBModified" + ex.Message.ToString().Trim();
            }
            return poIndicatorResponse;
        }

        public PurchaseOrderResponse UpdateHasCostVariance(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse purchaseOrderResponse = null;
            IPurchaseOrderRepository poIndicatorDataAccess = null;
            DataTable poCostMgntTable = null;
            try
            {
                purchaseOrderResponse = new PurchaseOrderResponse();
                poCostMgntTable = new DataTable();
                poCostMgntTable = DataTableMapper.UpdateHasCostVariance(purchaseOrderRequest,
               DataColumnMappings.UpdateHasCostVariance);

                //poCostMgntTable = ToDataTable(purchaseOrderRequest.PurchaseCostMgt);
                //poCostMgntTable.Columns.Remove("VendorId");
                //poCostMgntTable.Columns.Remove("PoType");
                //poCostMgntTable.Columns.Remove("PoLineStatus");
                //poCostMgntTable.Columns.Remove("LineNumber");
                //poCostMgntTable.Columns.Remove("UOfM");
                //poCostMgntTable.Columns.Remove("ProposedUnitCost");
                //poCostMgntTable.Columns.Remove("CostSupportId");
                //poCostMgntTable.Columns.Remove("CostBookCost");
                //poCostMgntTable.Columns.Remove("QtyOrder");
                //poCostMgntTable.Columns.Remove("QtyCancel");
                //poCostMgntTable.Columns.Remove("CostNotes");
                //poCostMgntTable.Columns.Remove("LineCostNotes");
                //poCostMgntTable.Columns.Remove("NoteIndex");
                //poCostMgntTable.Columns.Remove("CurrencyIndex");
                //poCostMgntTable.Columns.Remove("UserId");
                //poCostMgntTable.Columns.Remove("IsUpdated");
                //poCostMgntTable.Columns.Remove("ModifiedOn");
                //poCostMgntTable.Columns.Remove("Reason");
                //poCostMgntTable.Columns.Remove("UnitCost");
                //poCostMgntTable.Columns.Remove("CostStatus");
                //poCostMgntTable.Columns.Remove("IsDeleted");

                poIndicatorDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);
                poIndicatorDataAccess.UpdateHasCostVariance(poCostMgntTable, purchaseOrderRequest.CompanyID);
                purchaseOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                purchaseOrderResponse.Status = ResponseStatus.Error;
                purchaseOrderResponse.ErrorMessage = "Error:UpdateHasCost" + ex.Message.ToString().Trim();
            }
            return purchaseOrderResponse;
        }

        public PurchaseOrderResponse SavePoUnitCostDetails(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse purchaseOrderResponse = null;
            IPurchaseOrderRepository poIndicatorDataAccess = null;
            DataTable poCostMgntTable = null;
            try
            {
                purchaseOrderResponse = new PurchaseOrderResponse();
                poCostMgntTable = new DataTable();
                poCostMgntTable = DataTableMapper.SavePoUnitCostDetails(purchaseOrderRequest,
              DataColumnMappings.SavePoUnitCostDetails);
                //poCostMgntTable = ToDataTable(purchaseOrderRequest.PurchaseCostMgt);
                //poCostMgntTable.Columns.Remove("IsUpdated");
                //poCostMgntTable.Columns.Remove("ModifiedOn");
                //poCostMgntTable.Columns.Remove("Reason");
                //poCostMgntTable.Columns.Remove("IsDeleted");
                //poCostMgntTable.Columns.Remove("UserId");
                //poCostMgntTable.Columns.Remove("HasCostVariance");

                if (poCostMgntTable != null && poCostMgntTable.Rows.Count > 0)
                {
                    poIndicatorDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);
                    poIndicatorDataAccess.SavePoUnitCostDetails(poCostMgntTable, purchaseOrderRequest.UserId, purchaseOrderRequest.CompanyID);

                }

                purchaseOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                purchaseOrderResponse.Status = ResponseStatus.Error;
                purchaseOrderResponse.ErrorMessage = "Error:SavePoUnitcost" + ex.Message.ToString().Trim();
            }
            return purchaseOrderResponse;
        }

        public PurchaseOrderResponse UpdatePoCostProactiveMailStatus(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse purchaseOrderResponse = null;
            IPurchaseOrderRepository poIndicatorDataAccess = null;
            try
            {
                poIndicatorDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);

                DataTable dt = ConvertStringToDataTable(purchaseOrderRequest.UpdatePoCostDataTableString);
                poIndicatorDataAccess.UpdatePoCostProactiveMailStatus(dt, purchaseOrderRequest.CompanyID);
                purchaseOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                purchaseOrderResponse.Status = ResponseStatus.Error;
                purchaseOrderResponse.ErrorMessage = "Error:UpdatePoCostProactiveMailStatus" + ex.Message.ToString().Trim();
            }
            return purchaseOrderResponse;
        }

        public PurchaseOrderResponse ValidatePoForMailApproval(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse purchaseOrderResponse = null;
            IPurchaseOrderRepository poIndicatorDataAccess = null;
            try
            {
                purchaseOrderResponse = new PurchaseOrderResponse();
                poIndicatorDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);
                purchaseOrderResponse = poIndicatorDataAccess.ValidatePoForMailApproval(purchaseOrderRequest.PurchaseOrderEntity.PurchaseCostMgtInformation.PoNumber,                   
                    purchaseOrderRequest.PurchaseOrderEntity.PurchaseCostMgtInformation.VendorId,
                    purchaseOrderRequest.UserId,
                    purchaseOrderRequest.CompanyID);
                purchaseOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                purchaseOrderResponse.Status = ResponseStatus.Error;
                purchaseOrderResponse.ErrorMessage = "Error:SavePoUnitcost" + ex.Message.ToString().Trim();
            }
            return purchaseOrderResponse;
        }

        public PurchaseOrderResponse SendMailForMaterialManagement(PurchaseOrderRequest PurchaseOrderRequest)
        {
            PurchaseOrderResponse purchaseOrderResponse = null;            
            SendEmailRequest emailRequest = new SendEmailRequest();
            EMailInformation emailInformation = new EMailInformation();
            EmailBusiness emailBusiness = new EmailBusiness();
            try
            {
                purchaseOrderResponse = new PurchaseOrderResponse();
                emailRequest.EmailConfigID = PurchaseOrderRequest.AppConfigID;
                emailRequest.ConnectionString = PurchaseOrderRequest.ConnectionString;
                emailInformation.Subject = PurchaseOrderRequest.emailInfomation.Subject;
                emailInformation.EmailFrom = PurchaseOrderRequest.emailInfomation.EmailFrom;
                emailInformation.Body = PurchaseOrderRequest.emailInfomation.Body;
                emailInformation.SmtpAddress = PurchaseOrderRequest.emailInfomation.SmtpAddress;
                emailInformation.Signature = PurchaseOrderRequest.emailInfomation.Signature;
                emailInformation.IsDataTableBodyRequired = true;
                emailRequest.EmailInformation = emailInformation;
                emailRequest.Report = PurchaseOrderRequest.Report;
                emailBusiness.SendEmail(emailRequest);
                purchaseOrderResponse.Status = ResponseStatus.Success;
                
            }
            catch (Exception ex)
            {
                purchaseOrderResponse.Status = ResponseStatus.Error;
                purchaseOrderResponse.ErrorMessage = "Error:SavePoUnitcost" + ex.Message.ToString().Trim();
            }
            return purchaseOrderResponse;
        }
        private DataTable ConvertStringToDataTable(string data)
        {
            DataTable dataTable = new DataTable();
            bool columnsAdded = false;
            foreach (string row in data.Split('#'))
            {
                DataRow dataRow = dataTable.NewRow();
                foreach (string cell in row.Split('|'))
                {
                    string[] keyValue = cell.Split('~');
                    if (!columnsAdded)
                    {
                        DataColumn dataColumn = new DataColumn(keyValue[0]);
                        dataTable.Columns.Add(dataColumn);
                    }
                    dataRow[keyValue[0]] = keyValue[1];
                }
                columnsAdded = true;
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }

        ////private DataTable ConvertStringToDataTable(string data)
        ////{
        ////    DataTable dataTable = new DataTable();
        ////    bool columnsAdded = false;

        //    try
        //    {
        //        if (!string.IsNullOrEmpty(data))
        //        {

        //            foreach (string row in data.Split('#'))
        //            {
        //                DataRow dataRow = dataTable.NewRow();

        //                foreach (string cell in row.Split('|'))
        //                {

        //                    string[] keyValue = cell.Split('~');

        //                    if (!columnsAdded)
        //                    {
        //                        DataColumn dataColumn = new DataColumn(keyValue[0]);
        //                        dataTable.Columns.Add(dataColumn);
        //                    }

        //                    dataRow[keyValue[0]] = keyValue[1];

        //                }

        //                columnsAdded = true;
        //                dataTable.Rows.Add(dataRow);

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return dataTable;
        //}

        #endregion

        /// <summary>
        /// PO Activity Created to XRM.
        /// </summary>
        /// <param name="poActivityRequest"></param>
        /// <returns></returns>
        public PurchaseOrderResponse CreateActivity(PurchaseOrderRequest poActivityRequest)
        {
            StringBuilder logMessage = new StringBuilder();
            PurchaseOrderResponse poActivityResponse = null;
            IPurchaseOrderRepository poActivityDataAccess = null;
            List<POActivityLog> poActivityList = new List<POActivityLog>();
            POActivityLog PO = null;
            bool isSuccess = false;
            poActivityRequest.ThrowIfNull("poActivityRequest");
            try
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - ****************************************************************");
                logMessage.AppendLine(DateTime.Now.ToString() + " - Publish Activity to XRM started.");
                logMessage.AppendLine(DateTime.Now.ToString() + " - Fetching all the orders from the view.");

                poActivityResponse = new PurchaseOrderResponse();
                poActivityDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(poActivityRequest.ConnectionString);

                logMessage.AppendLine(DateTime.Now.ToString() + " - Fetching all Records for publish activity .");
                poActivityResponse = poActivityDataAccess.CreateActivity(poActivityRequest);

                logMessage.AppendLine(DateTime.Now.ToString() + " - Response :" + poActivityResponse.Status.ToString().Trim());

                logMessage.AppendLine(DateTime.Now.ToString() + " - No of Records :" + poActivityResponse.XRMIntegrationList.Count.ToString().Trim());

                if (poActivityResponse.Status == ResponseStatus.Success && poActivityResponse.XRMIntegrationList.Count > 0)
                {
                    foreach (Integration poActivity in poActivityResponse.XRMIntegrationList)
                    {
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Subject :" + poActivity.Subject.ToString().Trim()
                        + " | Description :" + poActivity.Description.ToString().Trim()
                        + " | DueDate :" + poActivity.DueDate.ToString().Trim()
                        + " | OwnerEntityId :" + poActivity.OwnerEntityId.ToString().Trim()
                        + " | OwnerEntityKeyName :" + poActivity.OwnerEntityKeyName.ToString().Trim()
                        + " | OwnerEntityName :" + poActivity.OwnerEntityName.ToString().Trim()
                        + " | POActivityTypeMasterID :" + poActivity.POActivityTypeMasterID.ToString().Trim()
                        + " | Priority :" + poActivity.Priority.ToString().Trim()
                        + " | RegardingEntityId :" + poActivity.RegardingEntityId.ToString().Trim()
                        + " | RegardingEntityKeyName :" + poActivity.RegardingEntityKeyName.ToString().Trim()
                        + " | RegardingEntityName :" + poActivity.RegardingEntityName.ToString().Trim() + "|");

                        poActivityRequest.XRMIntegration = poActivity;
                        XrmService xrmActivityService = new XrmService();
                        //Publish Activity to XRM.
                        logMessage.AppendLine(DateTime.Now.ToString() + " - " + "Calling XRM Publish Activity Method");
                        logMessage.AppendLine(DateTime.Now.ToString() + " - " + poActivityRequest.CrmActivityConfigurationURL.ToString());
                        string status = xrmActivityService.PublishPurchaseOrderActivity(poActivityRequest);

                        if (status.ToUpper().Trim() != "FAILURE")
                        {
                            PO = new POActivityLog();
                            logMessage.AppendLine(DateTime.Now.ToString() + " - " + "XRM Publish Activity Method called successfully");
                            PO.PONumber = poActivityRequest.XRMIntegration.RegardingEntityId.ToString().Trim();
                            PO.POActivityTypeMasterID = poActivityRequest.XRMIntegration.POActivityTypeMasterID;
                            poActivityList.Add(PO);
                            isSuccess = true;
                        }
                        else
                            logMessage.AppendLine(DateTime.Now.ToString() + " - " + "XRM Publish Activity Method Failled");
                        // }
                        //DT
                    }
                    if (isSuccess)
                    {
                        logMessage.AppendLine(DateTime.Now.ToString() + " - " + "Log XRM Published Activity to our table start");
                        logMessage.AppendLine(DateTime.Now.ToString() + " - " + "List of PO's");

                        foreach (var poActivity in poActivityList)
                        {
                            logMessage.AppendLine(DateTime.Now.ToString() + " - " + poActivity.PONumber.Trim() + "|" + poActivity.POActivityTypeMasterID.ToString() + "|");
                        }

                        poActivityRequest.POActivityLogList = poActivityList;
                        // log into Table.
                        poActivityResponse = poActivityDataAccess.LogActivity(poActivityRequest);

                        if (poActivityResponse.Status == ResponseStatus.Success)
                            logMessage.AppendLine(DateTime.Now.ToString() + " - " + "Log XRM Published Activity to our table Successfully");
                        else
                            logMessage.AppendLine(DateTime.Now.ToString() + " - " + "Log XRM Published Activity to our table Failled");

                        logMessage.AppendLine(DateTime.Now.ToString() + " - " + "Log XRM Published Activity to our table End");
                    }
                    else
                        logMessage.AppendLine(DateTime.Now.ToString() + " - " + "No Records available");
                }
                else
                    poActivityResponse.Status = ResponseStatus.Error;


            }
            catch (Exception ex)
            {
                poActivityResponse.Status = ResponseStatus.Error;
                poActivityResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            finally
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Publish Activity to XRM End.");
                logMessage.AppendLine(DateTime.Now.ToString() + " - ****************************************************************");

                new TextLogger().LogInformationIntoFile(logMessage.ToString(), poActivityRequest.LoggingPath, poActivityRequest.LoggingFileName);
            }
            return poActivityResponse;
        }

        #region LandedCost

        public PurchaseOrderResponse SaveEstimatedShipmentCostEntry(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse purchaseOrderResponse = null;
            IPurchaseOrderRepository poIndicatorDataAccess = null;

            try
            {
                purchaseOrderResponse = new PurchaseOrderResponse();
                poIndicatorDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);
                poIndicatorDataAccess.SaveEstimatedShipmentCostEntry(purchaseOrderRequest, purchaseOrderRequest.CompanyID);
                purchaseOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                purchaseOrderResponse.Status = ResponseStatus.Error;
                purchaseOrderResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return purchaseOrderResponse;
        }

        public PurchaseOrderResponse FetchEstimatedShipmentDetails(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse poOrderResponse = null;
            IPurchaseOrderRepository poResponseDataAccess = null;
            try
            {
                poOrderResponse = new PurchaseOrderResponse();
                poResponseDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);
                DataSet ds = poResponseDataAccess.FetchEstimatedShipmentDetails(purchaseOrderRequest, purchaseOrderRequest.CompanyID);

                if (ds.Tables[0] != null)
                {
                    List<PurchaseShipmentEstimateDetails> PurchaseShipmentEstimateDetailsList = POBusiness.ConvertDataTable<PurchaseShipmentEstimateDetails>(ds.Tables[0]);
                    poOrderResponse.PurchaseShipmentEstimateLineList = PurchaseShipmentEstimateDetailsList;
                }
                poOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                poOrderResponse.Status = ResponseStatus.Error;
                poOrderResponse.ErrorMessage = "FetchEstimatedShipmentDetails" + ex.Message.ToString().Trim();
            }
            return poOrderResponse;
        }


        /// <summary>
        /// To get shipment estimate details
        /// </summary>
        /// <param name="purchaseOrderRequest"></param>
        /// <returns></returns>
        public PurchaseOrderResponse GetShipmentEstimateDetails(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse poOrderResponse = null;
            IPurchaseOrderRepository poResponseDataAccess = null;
            try
            {
                poOrderResponse = new PurchaseOrderResponse();
                poResponseDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);
                DataSet ds = poResponseDataAccess.GetShipmentEstimateDetails(purchaseOrderRequest.CompanyID, purchaseOrderRequest.PurchaseShipmentEstimateDetails.EstimateID);

                if (ds.Tables[0] != null)
                {
                    List<PurchaseShipmentEstimateDetails> PurchaseShipmentEstimateDetailsList = POBusiness.ConvertDataTable<PurchaseShipmentEstimateDetails>(ds.Tables[0]);
                    poOrderResponse.PurchaseShipmentEstimateHeaderList = PurchaseShipmentEstimateDetailsList;
                }
                if (ds.Tables[1] != null)
                {
                    List<PurchaseShipmentEstimateDetails> PurchaseShipmentEstimateDetailsList = POBusiness.ConvertDataTable<PurchaseShipmentEstimateDetails>(ds.Tables[1]);
                    poOrderResponse.PurchaseShipmentEstimateLineList = PurchaseShipmentEstimateDetailsList;
                }
                poOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                poOrderResponse.Status = ResponseStatus.Error;
                poOrderResponse.ErrorMessage = "GetShipmentEstimateDetails" + ex.Message.ToString().Trim();
            }
            return poOrderResponse;
        }


        /// <summary>
        /// To get shipment estimate details
        /// </summary>
        /// <param name="purchaseOrderRequest"></param>
        /// <returns></returns>
        public PurchaseOrderResponse GetShipmentEstimateInquiryDetails(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse poOrderResponse = null;
            IPurchaseOrderRepository poResponseDataAccess = null;
            try
            {
                poOrderResponse = new PurchaseOrderResponse();
                poResponseDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);
                DataSet ds = poResponseDataAccess.GetShipmentEstimateInquiryDetails(purchaseOrderRequest.CompanyID, purchaseOrderRequest.PurchaseShipmentEstimateDetails.EstimateID);

                if (ds.Tables[0] != null)
                {
                    List<PurchaseShipmentEstimateDetails> PurchaseShipmentEstimateDetailsList = POBusiness.ConvertDataTable<PurchaseShipmentEstimateDetails>(ds.Tables[0]);
                    poOrderResponse.PurchaseShipmentEstimateHeaderList = PurchaseShipmentEstimateDetailsList;
                }
                if (ds.Tables[1] != null)
                {
                    List<PurchaseShipmentEstimateDetails> PurchaseShipmentEstimateDetailsList = POBusiness.ConvertDataTable<PurchaseShipmentEstimateDetails>(ds.Tables[1]);
                    poOrderResponse.PurchaseShipmentEstimateLineList = PurchaseShipmentEstimateDetailsList;
                }
                poOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                poOrderResponse.Status = ResponseStatus.Error;
                poOrderResponse.ErrorMessage = "GetShipmentEstimateDetails" + ex.Message.ToString().Trim();
            }
            return poOrderResponse;
        }

        /// <summary>
        /// To get PO shipment estimate details
        /// </summary>
        /// <param name="purchaseOrderRequest"></param>
        /// <returns></returns>
        public PurchaseOrderResponse GetPOShipmentEstimateDetails(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse PurchaseOrderResponse = null;
            IPurchaseOrderRepository poResponseDataAccess = null;
            try
            {
                PurchaseOrderResponse = new PurchaseOrderResponse();
                poResponseDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);
                DataSet ds = poResponseDataAccess.GetPOShipmentEstimateDetails(purchaseOrderRequest.CompanyID, purchaseOrderRequest.PurchaseOrderInformation.PoNumber);

                if (ds.Tables[0] != null)
                {
                    List<PurchaseSelectPoLines> purchaseSelectPoLines = POBusiness.ConvertDataTable<PurchaseSelectPoLines>(ds.Tables[0]);
                    PurchaseOrderResponse.SelectPoHeaderList = purchaseSelectPoLines;
                }
                if (ds.Tables[1] != null)
                {
                    List<PurchaseSelectPoLines> purchaseSelectPoLines = POBusiness.ConvertDataTable<PurchaseSelectPoLines>(ds.Tables[1]);
                    PurchaseOrderResponse.SelectPoLineList = purchaseSelectPoLines;
                }
                PurchaseOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                PurchaseOrderResponse.Status = ResponseStatus.Error;
                PurchaseOrderResponse.ErrorMessage = "GetPOShipmentEstimateDetails" + ex.Message.ToString().Trim();
            }
            return PurchaseOrderResponse;
        }

        /// <summary>
        /// To delete the Estimate  line details
        /// </summary>
        /// <param name="purchaseOrderRequest"></param>
        /// <returns></returns>
        public PurchaseOrderResponse DeleteEstimateLineDetails(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse PurchaseOrderResponse = null;
            IPurchaseOrderRepository poIndicatorDataAccess = null;

            try
            {
                PurchaseOrderResponse = new PurchaseOrderResponse();
                poIndicatorDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);
                poIndicatorDataAccess.DeleteEstimateLineDetails(purchaseOrderRequest.CompanyID,
                    purchaseOrderRequest.PurchaseOrderInformation.PoNumber,
                    purchaseOrderRequest.PurchaseOrderInformation.PoLineNumber,
                    purchaseOrderRequest.PurchaseShipmentEstimateDetails.UserId);
                PurchaseOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                PurchaseOrderResponse.Status = ResponseStatus.Error;
                PurchaseOrderResponse.ErrorMessage = "DeleteEstimateLineDetails" + ex.Message.ToString().Trim();
            }
            return PurchaseOrderResponse;
        }

        public PurchaseOrderResponse GetEstimateId(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse poOrderResponse = null;
            IPurchaseOrderRepository poResponseDataAccess = null;
            try
            {
                poOrderResponse = new PurchaseOrderResponse();
                poResponseDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);
                DataSet ds = poResponseDataAccess.GetEstimateId(purchaseOrderRequest.CompanyID, purchaseOrderRequest.PurchaseShipmentEstimateDetails.WindowValue);

                if (ds.Tables[0] != null)
                {
                    List<PoEstimateId> poEstimateId = POBusiness.ConvertDataTable<PoEstimateId>(ds.Tables[0]);
                    poOrderResponse.PurchaseEstimatedId = poEstimateId;
                }
                poOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                poOrderResponse.Status = ResponseStatus.Error;
                poOrderResponse.ErrorMessage = "GetEstimateId" + ex.Message.ToString().Trim();
            }
            return poOrderResponse;

        }

        public PurchaseOrderResponse DeleteEstimatedId(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse PurchaseOrderResponse = null;
            IPurchaseOrderRepository poIndicatorDataAccess = null;

            try
            {
                PurchaseOrderResponse = new PurchaseOrderResponse();
                poIndicatorDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);
                poIndicatorDataAccess.DeleteEstimatedId(purchaseOrderRequest.CompanyID,
                    purchaseOrderRequest.PurchaseShipmentEstimateDetails.EstimateID
                    , purchaseOrderRequest.PurchaseShipmentEstimateDetails.UserId);
                PurchaseOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                PurchaseOrderResponse.Status = ResponseStatus.Error;
                PurchaseOrderResponse.ErrorMessage = "DeleteEstimatedId" + ex.Message.ToString().Trim();
            }
            return PurchaseOrderResponse;
        }

        /// <summary>
        /// To Get new Estimate
        /// </summary>
        /// <param name="purchaseOrderRequest"></param>
        /// <returns></returns>
        public PurchaseOrderResponse GetNextEstimateId(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse purchaseOrderResponse = null;
            PurchaseShipmentEstimateDetails purchaseShipmentEstimateDetails = null;
            IPurchaseOrderRepository poDataAccess = null;

            try
            {

                purchaseOrderResponse = new PurchaseOrderResponse();
                purchaseShipmentEstimateDetails = new PurchaseShipmentEstimateDetails();
                poDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);
                purchaseShipmentEstimateDetails.EstimateID = poDataAccess.GetNextEstimateId(purchaseOrderRequest.AuditInformation.CompanyId);
                purchaseOrderResponse.PurchaseShipmentEstimateDetails = purchaseShipmentEstimateDetails;
                purchaseOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                purchaseOrderResponse.Status = ResponseStatus.Error;
                purchaseOrderResponse.ErrorMessage = "Generate New Estimate Id" + ex.Message.ToString().Trim();
            }
            return purchaseOrderResponse;
        }

        public PurchaseOrderResponse GetCurrencyIndex(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse purchaseOrderResponse = null;
            PurchaseShipmentEstimateDetails purchaseShipmentEstimateDetails = null;
            IPurchaseOrderRepository poDataAccess = null;

            try
            {
                purchaseOrderResponse = new PurchaseOrderResponse();
                purchaseShipmentEstimateDetails = new PurchaseShipmentEstimateDetails();
                poDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);
                DataSet ds = poDataAccess.GetCurrencyIndex(purchaseOrderRequest, purchaseOrderRequest.CompanyID);
                if (ds.Tables[0] != null)
                {
                    List<CurrencyDetails> poNumberLists = POBusiness.ConvertDataTable<CurrencyDetails>(ds.Tables[0]);
                    purchaseOrderResponse.CurrencyDetailLists = poNumberLists;
                }
                purchaseOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                purchaseOrderResponse.Status = ResponseStatus.Error;
                purchaseOrderResponse.ErrorMessage = "Generate New Estimate Id" + ex.Message.ToString().Trim();
            }
            return purchaseOrderResponse;
        }

        public PurchaseOrderResponse GetShipmentQtyTotal(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse purchaseOrderResponse = null;
            PurchaseShipmentEstimateDetails purchaseShipmentEstimateDetails = null;
            IPurchaseOrderRepository poDataAccess = null;

            try
            {

                purchaseOrderResponse = new PurchaseOrderResponse();
                purchaseShipmentEstimateDetails = new PurchaseShipmentEstimateDetails();
                poDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);
                purchaseShipmentEstimateDetails.EstimatedQtyNetWeight = poDataAccess.GetShipmentQtyTotal(purchaseOrderRequest.CompanyID
                    , purchaseOrderRequest.PurchaseShipmentEstimateDetails.ItemNumber.ToString().Trim()
                    , purchaseOrderRequest.PurchaseShipmentEstimateDetails.EstimatedQtyShipped);
                purchaseOrderResponse.PurchaseShipmentEstimateDetails = purchaseShipmentEstimateDetails;
                purchaseOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                purchaseOrderResponse.Status = ResponseStatus.Error;
                purchaseOrderResponse.ErrorMessage = "Generate New Estimate Id" + ex.Message.ToString().Trim();
            }
            return purchaseOrderResponse;
        }

        public PurchaseOrderResponse GetPoNumber(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse poOrderResponse = null;
            IPurchaseOrderRepository poResponseDataAccess = null;
            try
            {
                poOrderResponse = new PurchaseOrderResponse();
                poResponseDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);
                DataSet ds = poResponseDataAccess.GetPoNumber(purchaseOrderRequest.CompanyID
                    , purchaseOrderRequest.PurchaseShipmentEstimateDetails.Warehouse.ToString().Trim()
                    , purchaseOrderRequest.PurchaseShipmentEstimateDetails.Vendor.ToString().Trim());

                if (ds.Tables[0] != null)
                {
                    List<PoList> poNumberLists = POBusiness.ConvertDataTable<PoList>(ds.Tables[0]);
                    poOrderResponse.PurchaseNumberLists = poNumberLists;
                }
                poOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                poOrderResponse.Status = ResponseStatus.Error;
                poOrderResponse.ErrorMessage = "Get Po Number" + ex.Message.ToString().Trim();
            }
            return poOrderResponse;

        }

        public PurchaseOrderResponse ValidateCarrierReference(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse purchaseOrderResponse = null;
            PurchaseShipmentEstimateDetails purchaseShipmentEstimateDetails = null;
            IPurchaseOrderRepository poDataAccess = null;
            try
            {
                purchaseOrderResponse = new PurchaseOrderResponse();
                purchaseShipmentEstimateDetails = new PurchaseShipmentEstimateDetails();
                poDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);
                purchaseShipmentEstimateDetails.IsAvailable = poDataAccess.ValidateCarrierReference(purchaseOrderRequest.CompanyID
                    , purchaseOrderRequest.PurchaseShipmentEstimateDetails.EstimateID
                    , purchaseOrderRequest.PurchaseShipmentEstimateDetails.CarrierReference.ToString().Trim());
                purchaseOrderResponse.PurchaseShipmentEstimateDetails = purchaseShipmentEstimateDetails;
                purchaseOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                purchaseOrderResponse.Status = ResponseStatus.Error;
                purchaseOrderResponse.ErrorMessage = "Validate Carrier Reference" + ex.Message.ToString().Trim();
            }
            return purchaseOrderResponse;
        }

        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }
        #endregion

        #region Purchase Order | default BuyerID to improve user experience

        public PurchaseOrderResponse ValidatePurchaseBuyerId(PurchaseOrderRequest purchaseOrderRequest)
        {
            PurchaseOrderResponse purchaseOrderResponse = null;
            PurchaseShipmentEstimateDetails purchaseShipmentEstimateDetails = null;
            IPurchaseOrderRepository poDataAccess = null;

            try
            {

                purchaseOrderResponse = new PurchaseOrderResponse();
                purchaseShipmentEstimateDetails = new PurchaseShipmentEstimateDetails();
                poDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(purchaseOrderRequest.ConnectionString);
                purchaseOrderResponse.IsAvailable = poDataAccess.ValidatePurchaseBuyerId(purchaseOrderRequest.PurchaseOrderInformation.PoNumber.ToString().Trim(),
                    purchaseOrderRequest.CompanyID);
                purchaseOrderResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                purchaseOrderResponse.Status = ResponseStatus.Error;
                purchaseOrderResponse.ErrorMessage = "Validate Purchase BuyerId" + ex.Message.ToString().Trim();
            }
            return purchaseOrderResponse;
        }
        #endregion

        #region Elemica

        public PurchaseElemicaResponse RetrieveElemicaDetail(PurchaseElemicaRequest poElemicaRequest)
        {
            PurchaseElemicaResponse poElemicaResponse = null;
            IPurchaseOrderRepository poIndicatorDataAccess = null;
            PurchaseOrderEntity purchaseOrderEntity = null;
            PurchaseOrderInformation purchaseOrderInformation = null;
            PurchaseElemicaEntity purchaseElemicaEntity = null;

            try
            {
                poElemicaResponse = new PurchaseElemicaResponse();
                purchaseOrderEntity = new PurchaseOrderEntity();
                purchaseOrderInformation = new PurchaseOrderInformation();
                purchaseElemicaEntity = new PurchaseElemicaEntity();
                poIndicatorDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(poElemicaRequest.connnectionString);
                DataSet ds = poIndicatorDataAccess.RetrieveElemicaDetail(poElemicaRequest, poElemicaRequest.companyId);
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        purchaseElemicaEntity.ItemList = string.IsNullOrEmpty(row["ItemList"].ToString()) ? string.Empty : row["ItemList"].ToString().Trim();
                        purchaseElemicaEntity.LastSentTimeCount = Convert.ToInt16(row["LastSentTimeCount"]);
                        purchaseElemicaEntity.LastSentDate = Convert.ToDateTime(row["LastSentDate"]);
                        purchaseElemicaEntity.IsValidToSendElemica = Convert.ToInt16(row["IsValidToSendElemica"]);
                    }
                }
                purchaseOrderInformation.PoElemicaDetails = purchaseElemicaEntity;
                purchaseOrderEntity.PurchaseOrderDetails = purchaseOrderInformation;
                poElemicaResponse.purchaseOrderDetails = purchaseOrderEntity;

                poElemicaResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                poElemicaResponse.Status = ResponseStatus.Error;
                poElemicaResponse.ErrorMessage = "Retrieve Elemica Detail " + ex.Message.ToString().Trim();

            }
            return poElemicaResponse;

        }

        public PurchaseElemicaResponse SendElemicaDetail(PurchaseElemicaRequest poElemicaRequest)
        {
            PurchaseElemicaResponse poElemicaResponse = null;
            IPurchaseOrderRepository poIndicatorDataAccess = null;

            try
            {
                poElemicaResponse = new PurchaseElemicaResponse();
                poIndicatorDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(poElemicaRequest.connnectionString);
                poIndicatorDataAccess.SendElemicaDetail(poElemicaRequest, poElemicaRequest.companyId);
                poElemicaResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                poElemicaResponse.Status = ResponseStatus.Error;
                poElemicaResponse.ErrorMessage = "Send Elemica Detail " + ex.Message.ToString().Trim();

            }
            return poElemicaResponse;

        }

        public PurchaseElemicaResponse UpdatePOStatusForElemica(PurchaseElemicaRequest poElemicaRequest)
        {
            PurchaseElemicaResponse poElemicaResponse = null;
            IPurchaseOrderRepository poIndicatorDataAccess = null;

            try
            {
                poElemicaResponse = new PurchaseElemicaResponse();
                poIndicatorDataAccess = new ChemPoint.GP.Podl.PORepositoryDL(poElemicaRequest.connnectionString);
                poIndicatorDataAccess.UpdatePOStatusForElemica(poElemicaRequest, poElemicaRequest.companyId);
                poElemicaResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                poElemicaResponse.Status = ResponseStatus.Error;
                poElemicaResponse.ErrorMessage = "Update PO Status For Elemica " + ex.Message.ToString().Trim();

            }
            return poElemicaResponse;
        }

        #endregion

        

    }
    public static class StringExt
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
