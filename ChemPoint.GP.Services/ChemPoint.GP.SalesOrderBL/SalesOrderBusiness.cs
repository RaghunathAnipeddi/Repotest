using System;
using System.Collections.Generic;
using System.Text;
using ChemPoint.GP.BusinessContracts.Sales;
using Chempoint.GP.Infrastructure.Utils;
using Chempoint.GP.Infrastructure.Logging;
using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.HoldEngine;
using ChemPoint.GP.DataContracts.Sales;
using Chempoint.GP.Model.Interactions.Sales;
using System.Data;
using ChemPoint.GP.Entities.BaseEntities;
using ChemPoint.GP.Entities.Business_Entities.Sales;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using ChemPoint.GP.XrmServices;
using Microsoft.Dynamics.GP.eConnect;
using Chempoint.GP.Infrastructure.Email;
using ChemPoint.GP.PickTicketBL;
using Chempoint.GP.Model.Interactions.HoldEngine;
using ChemPoint.GP.BusinessContracts.TaskScheduler.HoldEngine;
using ChemPoint.GP.HoldEngineBL;
using System.ServiceModel;
using ChemPoint.GP.SalesOrderBL.PTService;
using ChemPoint.GP.Entities.Business_Entities;
using System.Linq;
using System.Reflection;
using System.Configuration;
using Chempoint.GP.Model.Interactions.Email;
using ChemPoint.GP.Email;

namespace ChemPoint.GP.SalesOrderBL
{
    public class SalesOrderBusiness : ISalesOrderBusiness
    {
        public SalesOrderBusiness()
        {
        }

        #region SOPEntry

        /// <summary>
        /// SOP Entry Fetch
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse GetSalesOrder(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse salesOrderResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesOrderRequest.ThrowIfNull("SalesOrderRequest");
            salesOrderRequest.SalesOrderEntity.ThrowIfNull("SalesOrderRequest.SalesOrderEntity");
            try
            {
                salesOrderResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderRequest.ConnectionString);
                salesOrderResponse = salesDataAccess.GetSalesOrder(salesOrderRequest);
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesOrderResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return salesOrderResponse;
        }

        /// <summary>
        /// Sop Entry Save
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse SaveSalesOrder(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse salesOrderResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesOrderRequest.ThrowIfNull("SalesOrderResponse");
            salesOrderRequest.SalesOrderEntity.ThrowIfNull("SalesOrderRequest.SalesOrderEntity");
            try
            {
                salesOrderResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderRequest.ConnectionString);
                salesDataAccess.SaveSalesOrder(salesOrderRequest, salesOrderRequest.CompanyID);
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                if (salesOrderRequest.IsHoldEngineRun == true)
                {
                    //Call Hold Engine
                    HoldEngineRequest holdEngineRequest = new HoldEngineRequest();
                    HoldEngineResponse holdEngineResponse = new HoldEngineResponse();
                    HoldEngineEntity holdEngineEntity = new HoldEngineEntity();
                    OrderHeader orderHeader = new OrderHeader();
                    orderHeader.SopNumber = salesOrderRequest.SalesOrderEntity.SopNumber.ToString().Trim();
                    orderHeader.SopType = salesOrderRequest.SalesOrderEntity.SopType;
                    orderHeader.MainCustomerNumber = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerId.ToString().Trim();
                    holdEngineEntity.BatchUserID = "batch";
                    holdEngineEntity.OrderHeader = orderHeader;
                    holdEngineRequest.HoldEngineEntity = holdEngineEntity;
                    holdEngineRequest.CompanyId = Convert.ToInt16(salesOrderRequest.CompanyID);
                    holdEngineRequest.ConnectionString = salesOrderRequest.ConnectionString;

                    IHoldEngineBusiness creditHold = new HoldEngineBusiness();
                    holdEngineResponse = creditHold.ProcessHoldForOrder(holdEngineRequest);
                    if (holdEngineResponse.Status == Chempoint.GP.Model.Interactions.HoldEngine.HoldEngineResponseStatus.Success)
                    {
                        salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                    }
                }

                if (salesOrderRequest.IsSopStatusExecute == true)
                {
                    ISalesOrderUpdateRepository salesStatusUdpate = null;
                    salesStatusUdpate = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderRequest.ConnectionString);
                    salesStatusUdpate.SaveSalesOrder(salesOrderRequest, salesOrderRequest.CompanyID);
                    salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                }
            }
            catch (Exception ex)
            {
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesOrderResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return salesOrderResponse;
        }

        /// <summary>
        /// Fetch inco Term 
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse FetchIncoTerm(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse salesOrderResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesOrderRequest.ThrowIfNull("SalesOrderRequest");
            try
            {
                salesOrderResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderRequest.ConnectionString);
                salesOrderResponse = salesDataAccess.FetchIncoTerm(salesOrderRequest);
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesOrderResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return salesOrderResponse;
        }

        /// <summary>
        /// SOP Entry Fetch
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse GetCountryDetails(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse salesOrderResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesOrderRequest.ThrowIfNull("SalesOrderRequest");
            salesOrderRequest.SalesOrderEntity.ThrowIfNull("SalesOrderRequest.SalesOrderEntity");
            try
            {
                salesOrderResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderRequest.ConnectionString);
                salesOrderResponse = salesDataAccess.GetCountryDetails(salesOrderRequest);
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesOrderResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return salesOrderResponse;
        }

        /// <summary>
        /// Get Schedule Delivery Date
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse GetSalesCurSchedDelDate(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse salesOrderResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesOrderRequest.ThrowIfNull("SalesOrderRequest");
            try
            {
                salesOrderResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderRequest.ConnectionString);
                salesOrderResponse = salesDataAccess.GetSalesCurSchedDelDate(salesOrderRequest);
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesOrderResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return salesOrderResponse;
        }

        /// <summary>
        /// Get Schedule Ship Date
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse GetSalesCurSchedShipDate(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse salesOrderResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesOrderRequest.ThrowIfNull("SalesOrderRequest");
            try
            {
                salesOrderResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderRequest.ConnectionString);
                salesOrderResponse = salesDataAccess.GetSalesCurSchedShipDate(salesOrderRequest);
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesOrderResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return salesOrderResponse;
        }

        /// <summary>
        /// Get Schedule Ship Date
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse UpdateTaxScheduleIdToLine(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse salesOrderResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesOrderRequest.ThrowIfNull("SalesOrderRequest");
            try
            {
                salesOrderResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderRequest.ConnectionString);
                salesDataAccess.UpdateTaxScheduleIdToLine(salesOrderRequest);
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesOrderResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return salesOrderResponse;
        }

        /// <summary>
        /// Save Allocated Qty...
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse SaveAllocatedQty(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse salesOrderResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesOrderRequest.ThrowIfNull("SalesOrderRequest");
            try
            {
                salesOrderResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderRequest.ConnectionString);
                salesOrderResponse = salesDataAccess.SaveAllocatedQty(salesOrderRequest);
            }
            catch (Exception ex)
            {
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesOrderResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return salesOrderResponse;
        }

        /// <summary>
        /// FetchSopChangedQty...
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse FetchSalesOrderLineForTermHold(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse salesOrderResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesOrderRequest.ThrowIfNull("SalesOrderRequest");
            try
            {
                salesOrderResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderRequest.ConnectionString);
                salesOrderResponse = salesDataAccess.FetchSalesOrderLineForTermHold(salesOrderRequest);
                if (salesOrderResponse.Status == Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success)
                {
                    // Service SKU Qty add and changes
                    if (salesOrderRequest.IsSopStatusExecute)
                    {

                        if (SendReport(null, salesOrderRequest))
                            salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                        else
                            salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                    }
                    else
                    {
                        bool isServiceSkuExist = salesOrderResponse.SalesOrderDetails.SalesOrderDetails.SalesOrderDetails.LineItems.Any(record => record.Quantity > 0 && record.IsServiceSKU == true);
                        if (isServiceSkuExist)
                        {
                            if (SendReport(salesOrderResponse, null))
                                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                            else
                                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                        }
                        else
                            salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.NoRecord;
                    }

                    // Inventory SKU Qty add and changes
                    bool isInventoryExist = salesOrderResponse.SalesOrderDetails.SalesOrderDetails.SalesOrderDetails.LineItems.Any(record => record.Quantity > 0 && record.IsServiceSKU == false);

                    if (isInventoryExist)
                        salesOrderResponse.TermHoldInventoryStatus = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                    else
                        salesOrderResponse.TermHoldInventoryStatus = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.NoRecord;
                }
                else
                {
                    salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.NoRecord;
                    salesOrderResponse.TermHoldInventoryStatus = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.NoRecord;
                }

            }
            catch (Exception ex)
            {
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesOrderResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return salesOrderResponse;
        }
        #endregion SOPEntry

        #region SalesItemDetail

        public SalesOrderResponse SaveSalesItemDetail(SalesOrderRequest salesItemDetailRequest)
        {
            SalesOrderResponse salesItemDetailResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesItemDetailRequest.ThrowIfNull("SalesOrderResponse");
            salesItemDetailRequest.SalesOrderEntity.ThrowIfNull("salesItemDetailRequest.SalesOrderEntity");
            try
            {
                salesItemDetailResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesItemDetailRequest.ConnectionString);
                salesDataAccess.SaveSalesItemDetail(salesItemDetailRequest, salesItemDetailRequest.CompanyID);
                salesItemDetailResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesItemDetailResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesItemDetailResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return salesItemDetailResponse;
        }

        #endregion SalesItemDetail

        public SalesOrderResponse UpdatePrintPickTicketStatus(SalesOrderRequest printPickTicketStatusRequest)
        {
            SalesOrderResponse printPickTicketStatusResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            printPickTicketStatusRequest.ThrowIfNull("SalesOrderResponse");
            printPickTicketStatusRequest.SalesOrderEntity.ThrowIfNull("SalesOrderRequest.SalesOrderEntity");
            try
            {
                printPickTicketStatusResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(printPickTicketStatusRequest.ConnectionString);
                salesDataAccess.UpdatePrintPickTicketStatus(printPickTicketStatusRequest.SalesOrderEntity.SopNumber, printPickTicketStatusRequest.SalesOrderEntity.SopType, printPickTicketStatusRequest.CompanyID);
                printPickTicketStatusResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                printPickTicketStatusResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                printPickTicketStatusResponse.ErrorMessage = ex.Message.ToString().Trim();

            }

            return printPickTicketStatusResponse;
        }

        /// <summary>
        /// Sales Item detail Entry Fetch...
        /// </summary>
        /// <param name="salesItemDetailRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse GetSalesItemDetail(SalesOrderRequest salesItemDetailRequest)
        {
            SalesOrderResponse salesItemDetailResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesItemDetailRequest.ThrowIfNull("SalesItemDetailEntry");
            salesItemDetailRequest.SalesOrderEntity.ThrowIfNull("salesItemDetailRequest.SalesOrderEntity");
            try
            {
                salesItemDetailResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesItemDetailRequest.ConnectionString);
                salesItemDetailResponse = salesDataAccess.GetSalesItemDetail(salesItemDetailRequest);
                salesItemDetailResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesItemDetailResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesItemDetailResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return salesItemDetailResponse;
        }

        /// <summary>
        /// Save order Type Business Logic
        /// </summary>
        /// <param name="salesOrderTypeRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse SaveOrderTypeDetail(SalesOrderRequest salesOrderTypeRequest)
        {
            SalesOrderResponse salesItemDetailResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesOrderTypeRequest.ThrowIfNull("SalesOrderResponse");
            salesOrderTypeRequest.SalesOrderEntity.ThrowIfNull("salesItemDetailRequest.SalesOrderEntity");
            try
            {
                salesItemDetailResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderTypeRequest.ConnectionString);
                salesDataAccess.SaveOrderTypeDetail(salesOrderTypeRequest);
                salesItemDetailResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesItemDetailResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesItemDetailResponse.ErrorMessage = ex.Message.ToString().Trim();

            }

            return salesItemDetailResponse;
        }

        /// <summary>
        /// Save Third Party Address....
        /// </summary>
        /// <param name="thirdPartyRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse SaveThirdPartyAddress(SalesOrderRequest thirdPartyRequest)
        {
            SalesOrderResponse thirdPartyResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            thirdPartyRequest.ThrowIfNull("SalesOrderResponse");
            thirdPartyRequest.SalesOrderEntity.ThrowIfNull("thirdPartyRequest.SalesOrderEntity");
            try
            {
                thirdPartyResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(thirdPartyRequest.ConnectionString);
                salesDataAccess.SaveThirdPartyAddress(thirdPartyRequest);
                thirdPartyResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                thirdPartyResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                thirdPartyResponse.ErrorMessage = ex.Message.ToString().Trim();

            }

            return thirdPartyResponse;
        }

        /// <summary>
        /// Save Third Party Address....
        /// </summary>
        /// <param name="thirdPartyRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse SaveCustomerPickupAddress(SalesOrderRequest customerPickupRequest)
        {
            SalesOrderResponse customerPickupResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            customerPickupRequest.ThrowIfNull("SalesOrderResponse");
            customerPickupRequest.SalesOrderEntity.ThrowIfNull("customerPickupRequest.SalesOrderEntity");
            try
            {
                customerPickupResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(customerPickupRequest.ConnectionString);
                salesDataAccess.SaveCustomerPickupAddress(customerPickupRequest);
                customerPickupResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                customerPickupResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                customerPickupResponse.ErrorMessage = ex.Message.ToString().Trim();

            }

            return customerPickupResponse;
        }

        #region SaveSopTransactionAddressCodes

        /// <summary>
        /// Sop Transaction Address Code 
        /// Save Sop Customer Detail Entry aDDRESS Id...
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse SaveSopTransactionAddressCodes(SalesOrderRequest salesOrderAddressRequest)
        {
            SalesOrderResponse salesOrderAddressResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesOrderAddressRequest.ThrowIfNull("salesOrderAddressRequest");
            salesOrderAddressRequest.SalesOrderEntity.ThrowIfNull("salesOrderAddressRequest.SalesOrderEntity Object is null");
            try
            {
                salesOrderAddressResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderAddressRequest.ConnectionString);
                salesDataAccess.SaveSopTransactionAddressCodes(salesOrderAddressRequest);
                salesOrderAddressResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesOrderAddressResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesOrderAddressResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return salesOrderAddressResponse;
        }

        #endregion SaveSopTransactionAddressCodes

        /// <summary>
        /// Save Header Comment instruction details 
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse SaveHeaderCommentInstruction(SalesOrderRequest salesOrderHeaderRequest)
        {
            SalesOrderResponse salesOrderHeaderResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesOrderHeaderRequest.ThrowIfNull("salesOrderAddressRequest");
            salesOrderHeaderRequest.SalesOrderEntity.ThrowIfNull("salesOrderAddressRequest.SalesOrderEntity Object is null");
            try
            {
                salesOrderHeaderResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderHeaderRequest.ConnectionString);
                salesDataAccess.SaveHeaderCommentInstruction(salesOrderHeaderRequest);
                salesOrderHeaderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesOrderHeaderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesOrderHeaderResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return salesOrderHeaderResponse;
        }

        /// <summary>
        /// Void SalesOrder From SopEntry 
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse SalesOrderVoid(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse salesOrderVoidResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;
            try
            {
                string companyName = string.Empty;
                if (salesOrderRequest.CompanyID == 1)
                    companyName = "chmpt";
                else
                    companyName = "cpeur";

                salesOrderVoidResponse = new SalesOrderResponse();
                salesOrderRequest.ThrowIfNull("salesOrderRequest");
                salesOrderRequest.SalesOrderEntity.ThrowIfNull("salesOrderRequest.SalesOrderEntity Object is null");
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderRequest.ConnectionString);
                salesDataAccess.SalesOrderVoid(salesOrderRequest);
                salesOrderVoidResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                if (!String.IsNullOrEmpty(salesOrderRequest.SalesOrderEntity.OrigNumber))
                {
                    string xrmNotesResult = new XrmService().PublishSalesOrderNotes(salesOrderRequest.SalesOrderEntity.OrigNumber, salesOrderRequest.XrmServiceUrl, "GP", "SalesOrderVoid ", "GP", companyName);
                    string xrmStatusResult = new XrmService().PublishSalesOrderStatus(salesOrderRequest.SalesOrderEntity.OrigNumber, companyName, salesOrderRequest.XrmServiceUrl, "GP", "GP");
                }
            }
            catch (Exception ex)
            {
                salesOrderVoidResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesOrderVoidResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return salesOrderVoidResponse;
        }

        /// <summary>
        /// Save Header Comment instruction details 
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse SaveLineCommentInstruction(SalesOrderRequest salesOrderLineRequest)
        {
            SalesOrderResponse salesOrderLineResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesOrderLineRequest.ThrowIfNull("SalesOrderLineRequest");
            salesOrderLineRequest.SalesOrderEntity.ThrowIfNull("SalesOrderLineRequest.SalesOrderEntity Object is null");
            try
            {
                salesOrderLineResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderLineRequest.ConnectionString);
                salesDataAccess.SaveLineCommentInstruction(salesOrderLineRequest);
                salesOrderLineResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesOrderLineResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesOrderLineResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return salesOrderLineResponse;
        }

        /// <summary>
        /// ValidateSopTransactionAddressCodes details 
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse ValidateSopTransactionAddressCodes(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse validateSopTransactionAddressCodesResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesOrderRequest.ThrowIfNull("SalesOrderLineRequest");
            salesOrderRequest.SalesOrderEntity.ThrowIfNull("SalesOrderLineRequest.SalesOrderEntity Object is null");
            try
            {
                validateSopTransactionAddressCodesResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderRequest.ConnectionString);
                validateSopTransactionAddressCodesResponse = salesDataAccess.ValidateSopTransactionAddressCodes(salesOrderRequest);
                validateSopTransactionAddressCodesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                validateSopTransactionAddressCodesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                validateSopTransactionAddressCodesResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return validateSopTransactionAddressCodesResponse;
        }

        /// <summary>
        /// ValidateSopTransactionServiceSkuItems details 
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse ValidateSopTransactionServiceSkuItems(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse validateSopTransactionServiceSkuItemsResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesOrderRequest.ThrowIfNull("salesOrderRequest");
            salesOrderRequest.SalesOrderEntity.ThrowIfNull("salesOrderRequest.SalesOrderEntity Object is null");
            try
            {
                validateSopTransactionServiceSkuItemsResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderRequest.ConnectionString);
                validateSopTransactionServiceSkuItemsResponse = salesDataAccess.ValidateSopTransactionServiceSkuItems(salesOrderRequest);
                validateSopTransactionServiceSkuItemsResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                validateSopTransactionServiceSkuItemsResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                validateSopTransactionServiceSkuItemsResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return validateSopTransactionServiceSkuItemsResponse;
        }

        /// <summary>
        /// Get Quote lookup ....
        /// </summary>
        /// <param name="thirdPartyRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse GetQuoteNumber(SalesOrderRequest quoteLookupRequest)
        {
            SalesOrderResponse quoteLookupResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            quoteLookupRequest.ThrowIfNull("SalesOrderResponse");
            quoteLookupRequest.SalesOrderEntity.ThrowIfNull("quoteLookupRequest.SalesOrderEntity");
            try
            {
                quoteLookupResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(quoteLookupRequest.ConnectionString);
                quoteLookupResponse = salesDataAccess.GetQuoteNumber(quoteLookupRequest);
                quoteLookupResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                quoteLookupResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                quoteLookupResponse.ErrorMessage = ex.Message.ToString().Trim();

            }

            return quoteLookupResponse;
        }

        /// <summary>
        /// Get Quote lookup ....
        /// </summary>
        /// <param name="thirdPartyRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse ValidateQuoteNumber(SalesOrderRequest quoteLookupRequest)
        {
            SalesOrderResponse quoteLookupResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            quoteLookupRequest.ThrowIfNull("SalesOrderResponse");
            quoteLookupRequest.SalesOrderEntity.ThrowIfNull("quoteLookupRequest.SalesOrderEntity");
            try
            {
                quoteLookupResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(quoteLookupRequest.ConnectionString);
                quoteLookupResponse = salesDataAccess.ValidateQuoteNumber(quoteLookupRequest);
                quoteLookupResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                quoteLookupResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                quoteLookupResponse.ErrorMessage = ex.Message.ToString().Trim();

            }

            return quoteLookupResponse;
        }

        #region Pte_Request

        public SalesOrderResponse UpdatePteLog(SalesOrderRequest soRequest)
        {
            SalesOrderResponse soResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            if (soRequest.CompanyID == 1)
            {
                soRequest.ThrowIfNull("PTE request object is null");
                soRequest.SalesOrderEntity.ThrowIfNull("PTE inner request object is null");

                try
                {
                    soResponse = new SalesOrderResponse();
                    salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(soRequest.ConnectionString);
                    salesDataAccess.UpdatePteRequestToGP(soRequest);
                    soResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                }
                catch (Exception ex)
                {
                    soResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                    soResponse.ErrorMessage = ex.Message.ToString().Trim();

                }
            }
            else
                soResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;

            return soResponse;
        }

        #endregion Pte_Request

        #region LotDetails

        public SalesOrderResponse SaveSalesLotDetails(SalesOrderRequest salesGpLotsRequest)
        {
            SalesOrderResponse salesOrderResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;
            string sopNumber = salesGpLotsRequest.SalesOrderEntity.SopNumber.ToString();
            int sopType = salesGpLotsRequest.SalesOrderEntity.SopType;
            int lineItemSequence = salesGpLotsRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.LineItemDetails.OrderLineId;
            string itemNumber = salesGpLotsRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.LineItemDetails.ItemNumber.ToString();
            string inPutXml = salesGpLotsRequest.eComXML.ToString();
            int companyId = salesGpLotsRequest.CompanyID;
            string userId = salesGpLotsRequest.UserID;
            salesGpLotsRequest.ThrowIfNull("Lot Details");
            salesGpLotsRequest.SalesOrderEntity.ThrowIfNull("Save Lot Detail Entity");
            try
            {
                salesOrderResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesGpLotsRequest.ConnectionString);
                salesDataAccess.SaveSalesLotDetails(sopNumber, sopType, lineItemSequence, itemNumber, inPutXml, companyId, userId);
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesOrderResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return salesOrderResponse;
        }

        public SalesOrderResponse GetLotDetails(SalesOrderRequest salesGpLotsRequest)
        {
            SalesOrderResponse salesOrderResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;
            SalesLineItem salesLineItem = null;
            SalesOrderDetails salesOrderDetails = null;
            SalesOrderInformation salesOrderInformation = null;

            SalesOrderEntity salesOrderEntity = null;
            string sopNumber = salesGpLotsRequest.SalesOrderEntity.SopNumber.ToString();
            int sopType = salesGpLotsRequest.SalesOrderEntity.SopType;
            string itemNumber = salesGpLotsRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.LineItemDetails.ItemNumber.ToString();
            salesGpLotsRequest.ThrowIfNull("Lot Details");
            salesGpLotsRequest.SalesOrderEntity.ThrowIfNull("Get Lot Detail Entity");
            try
            {
                salesOrderResponse = new SalesOrderResponse();
                salesLineItem = new SalesLineItem();
                salesOrderDetails = new SalesOrderDetails();
                salesOrderInformation = new SalesOrderInformation();

                salesOrderEntity = new SalesOrderEntity();

                // List<LotInformation> lotInformationList;

                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesGpLotsRequest.ConnectionString);
                salesLineItem = salesDataAccess.GetLotDetails(sopNumber, sopType, itemNumber);

                salesOrderDetails.LineItemDetails = salesLineItem;
                salesOrderInformation.SalesOrderDetails = salesOrderDetails;
                salesOrderEntity.SalesOrderDetails = salesOrderInformation;
                salesOrderResponse.SalesOrderDetails = salesOrderEntity;

                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesOrderResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return salesOrderResponse;
        }

        #endregion LotDetails

        public SalesOrderResponse GetVatLookupDetails(SalesOrderRequest salesVatRequest)
        {
            SalesOrderResponse salesOrderResponse = null;
            SalesOrderEntity salesOrderEntity = null;
            SalesOrderInformation salesOrderInformation = null;
            ISalesOrderUpdateRepository salesDataAccess = null;
            CustomerInformation customerInformation = new CustomerInformation();
            salesVatRequest.ThrowIfNull("Sales VAT Request is null");
            try
            {
                salesOrderResponse = new SalesOrderResponse();
                salesOrderInformation = new SalesOrderInformation();
                salesOrderEntity = new SalesOrderEntity();
                string customerNumber = salesVatRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerId.ToString();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesVatRequest.ConnectionString);
                customerInformation = salesDataAccess.GetVatNumberDetails(customerNumber);
                salesOrderInformation.Customer = customerInformation;
                salesOrderEntity.SalesOrderDetails = salesOrderInformation;
                salesOrderResponse.SalesOrderDetails = salesOrderEntity;
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesOrderResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return salesOrderResponse;
        }

        #region OrderTransferTOFO

        /// <summary>
        /// Transers the order to FO
        /// Called from Engine
        /// </summary>
        /// <param name="soRequest"></param>
        /// <returns></returns>
        public bool TransferOrderToFO(BulkOrderTransferRequest soRequest)
        {
            ISalesOrderUpdateRepository salesDataAccess = null;
            StringBuilder logMessage = new StringBuilder();
            StringBuilder failureMailMessage = new StringBuilder();
            int i = 0;
            failureMailMessage.Append("<html><h3>Following are the error(s) occured while transferring Order to FO through auto transfer engine </h3><br>");
            failureMailMessage.Append("<table border=1><tr><th>Order Number</th><th>Reason for failure</th></tr>");

            if (soRequest.CompanyID == 1)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - ****************************************************************");
                logMessage.AppendLine(DateTime.Now.ToString() + " - Auto Transfer job started.");
                logMessage.AppendLine(DateTime.Now.ToString() + " - Fetching all the orders from the view.");

                soRequest.ThrowIfNull("Order to FO transfer request object is null");

                try
                {

                    salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(soRequest.ConnectionString);
                    BulkOrderTransferEntity transferEntity = salesDataAccess.GetOrdersToAutoFOTransfer();
                    transferEntity.UnlockedOrders = new List<string>();
                    transferEntity.LockedOrders = new List<string>();
                    transferEntity.FipOrders = new List<string>();

                    if (transferEntity.SalesOrderDetails == null || transferEntity.SalesOrderDetails.Tables.Count == 0)
                    {
                        return true;
                    }

                    foreach (DataRow theRow in transferEntity.SalesOrderDetails.Tables[0].Rows)
                    {
                        if (theRow["IsFreightIncludedPrice"].ToString().Trim() == "1" && !transferEntity.FipOrders.Contains(theRow["SopNumbe"].ToString().Trim()))
                            transferEntity.FipOrders.Add(" Order # : " + theRow["SopNumbe"].ToString().Trim() +
                                                         " Line# : " + theRow["LnItmSeq"].ToString().Trim() +
                                                         " LineTimeStamp : " + theRow["LatestTimeStamp"].ToString().Trim() +
                                                         " AuditLogId : " + theRow["AuditLogId"].ToString().Trim());

                        if (theRow["LockStatus"].ToString().Trim() == "0" && !transferEntity.UnlockedOrders.Contains(theRow["SopNumbe"].ToString().Trim()))
                            transferEntity.UnlockedOrders.Add(theRow["SopNumbe"].ToString().Trim());

                        if (theRow["LockStatus"].ToString().Trim() == "1" && !transferEntity.LockedOrders.Contains(theRow["SopNumbe"].ToString().Trim()))
                            transferEntity.LockedOrders.Add(theRow["SopNumbe"].ToString().Trim() + ',');
                    }

                    // Log the details
                    if (transferEntity.LockedOrders.Count > 0)
                    {
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Locked order : " + String.Join(",", (string[])transferEntity.LockedOrders.ToArray()));
                    }
                    if (transferEntity.UnlockedOrders.Count > 0)
                    {
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Unlocked Orders  : " + String.Join(",", (string[])transferEntity.UnlockedOrders.ToArray()));
                    }
                    if (transferEntity.FipOrders.Count > 0)
                    {
                        logMessage.AppendLine(DateTime.Now.ToString() + " - FIP Orders  : " + String.Join(",", (string[])transferEntity.FipOrders.ToArray()));
                    }

                    //for every order, transfer to FO
                    if (transferEntity.UnlockedOrders.Count > 0)
                    {
                        foreach (string orderNumber in transferEntity.UnlockedOrders)
                        {
                            if (!transferEntity.FipOrders.Contains(orderNumber))
                            {
                                logMessage.AppendLine(DateTime.Now.ToString() + " - --------------------------------------------------");
                                logMessage.AppendLine(DateTime.Now.ToString() + " - Started transferring order : " + orderNumber);

                                // Get specific order details to view
                                DataView specificOrderDV = new DataView(transferEntity.SalesOrderDetails.Tables[0]);
                                specificOrderDV.RowFilter = "SopNumbe='" + orderNumber + "'";
                                DataTable inputDataTable = specificOrderDV.ToTable();



                                // Get the next avialable FO number
                                string nextAvailableFONumber = salesDataAccess.GetNextAvailableFONumber(3, "INVOICE", soRequest.CompanyID);

                                //prepare Econnect XML
                                string inputXml = SerializeToString(inputDataTable);
                                inputXml = inputXml.Replace("<DocumentElement>", "<Order>");
                                inputXml = inputXml.Replace("</DocumentElement>", "</Order>");
                                inputXml = inputXml.Replace("<Table>", "<Details>");
                                inputXml = inputXml.Replace("</Table>", "</Details>");
                                logMessage.AppendLine(DateTime.Now.ToString() + " - Input XML: " + inputXml);
                                logMessage.AppendLine(DateTime.Now.ToString() + " - Creating eConnect XML");
                                string eConnectXml = TransformForFO(inputXml, orderNumber, nextAvailableFONumber, soRequest.StyleSheetPath);
                                logMessage.AppendLine(DateTime.Now.ToString() + " - Econnect XML : " + eConnectXml);


                                // Create FO in GP

                                if (eConnectXml != string.Empty)
                                {
                                    eConnectMethods eConObj = null;
                                    bool result = false;
                                    try
                                    {
                                        eConObj = new eConnectMethods();
                                        logMessage.AppendLine(DateTime.Now.ToString() + " - Initializing Econnect Object");
                                        result = eConObj.eConnect_EntryPoint(soRequest.NAEconnectConnectionString, Microsoft.Dynamics.GP.eConnect.EnumTypes.ConnectionStringType.SqlClient, eConnectXml, Microsoft.Dynamics.GP.eConnect.EnumTypes.SchemaValidationType.XSD, "");

                                        if (result == true)
                                        {
                                            logMessage.AppendLine(DateTime.Now.ToString() + " - Order has been successfully transfered to FO : " + orderNumber + " | " + nextAvailableFONumber);

                                            //PUBLISH order Notes to XRM
                                            string xRMNotesResult = new XrmService().PublishSalesOrderNotes(orderNumber, soRequest.XrmServiceUrl, "AutoTransferEngine", "Transferred to Fulfillment : " + nextAvailableFONumber, "AutoTransfer", "chmpt");
                                            logMessage.AppendLine(DateTime.Now.ToString() + " - XRM order notes publish result - " + xRMNotesResult);

                                            //PUBLISH order status to XRM
                                            string xrmStatusResult = new XrmService().PublishSalesOrderStatus(orderNumber,
                                                                                          (soRequest.CompanyID == 1 ? "Chmpt" : "Cpeur"), soRequest.XrmServiceUrl, "AutoTransfer", "AutoTransferToFO");
                                            logMessage.AppendLine(DateTime.Now.ToString() + " - XRM order status publish result - " + xrmStatusResult);

                                            #region PT

                                            logMessage.AppendLine(DateTime.Now.ToString() + " - Checking and sending PT to WH and CHR");

                                            logMessage.AppendLine(DateTime.Now.ToString() + " - Sending pick ticket to the Warehouse with new EDI flow");
                                            // if the validation message shows as failed then it will log the details and won't send the PT
                                            if (inputDataTable.Rows[0]["ValidationMessage"].ToString() != string.Empty)
                                            {

                                                logMessage.AppendLine(DateTime.Now.ToString() + " - The PT will not be send as vaildation fails : " + inputDataTable.Rows[0]["ValidationMessage"].ToString());
                                            }

                                            else
                                            {
                                                SalesOrderRequest salesWhiboardRequest = new SalesOrderRequest();
                                                SalesOrderResponse salesWhiboardResponse = null;
                                                salesWhiboardRequest.CompanyID = 1;
                                                salesWhiboardRequest.ConnectionString = soRequest.ConnectionString;
                                                SalesOrderEntity salesOrder = new SalesOrderEntity();
                                                salesOrder.SopType = 6;
                                                salesOrder.SopNumber = nextAvailableFONumber;
                                                salesWhiboardRequest.SalesOrderEntity = salesOrder;

                                                salesWhiboardResponse = UpdateOrderDetailsToWarehouseIBoard(salesWhiboardRequest);
                                                if (salesWhiboardResponse != null && salesWhiboardResponse.Status == Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error)
                                                {
                                                    logMessage.AppendLine(DateTime.Now.ToString() + " - Unknown error occurred and unable to process pick ticket. Please contact IT.");
                                                }

                                                PickTicketRequest pickTicketRequest = new PickTicketRequest();
                                                pickTicketRequest.CompanyID = 1;
                                                pickTicketRequest.CompanyName = "Chmpt";
                                                pickTicketRequest.UserID = "Auto PT";
                                                pickTicketRequest.ConnectionString = soRequest.ConnectionString;

                                                pickTicketRequest.SopType = 6;
                                                pickTicketRequest.SopNumber = nextAvailableFONumber;
                                                pickTicketRequest.RequestType = "Insert";
                                                pickTicketRequest.OperationStatus = 1;
                                                pickTicketRequest.OrigNumber = orderNumber;
                                                pickTicketRequest.XrmServiceUrl = soRequest.XrmServiceUrl;
                                                pickTicketRequest.WarehouseEdiServiceUrl = soRequest.WarehouseEdiServiceUrl;
                                                pickTicketRequest.StyleSheetPath = soRequest.PickTicketStyleSheetPath;
                                                pickTicketRequest.LoggingPath = soRequest.LoggingPath;
                                                pickTicketRequest.LoggingFileName = soRequest.LoggingFileName;

                                                IPickTicketBI pickTicketObj = new PickTicketBusiness();
                                                PickTicketResponse pickTicketResponse = pickTicketObj.PrintPickTicket(pickTicketRequest);

                                                if (pickTicketResponse != null)
                                                {
                                                    if (pickTicketResponse.Status == Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success)
                                                    {
                                                        //Generate Pdf and attach to XRM
                                                        logMessage.AppendLine(DateTime.Now.ToString() + " - Call PickTicket Service to Attach PT to XRM");
                                                        AttachPTToXrm(nextAvailableFONumber, orderNumber, soRequest.PickTicketServiceUrl, ref logMessage);

                                                        salesDataAccess.UpdateAutoSendPTLog(pickTicketRequest.SopType, pickTicketRequest.SopNumber, 0, "PT has been submitted to WH - " + inputDataTable.Rows[0]["HLocnCode"].ToString().Trim() + pickTicketResponse.Message);
                                                    }
                                                    else
                                                    {
                                                        if (!string.IsNullOrWhiteSpace(pickTicketResponse.Message))
                                                        {
                                                            logMessage.AppendLine(DateTime.Now.ToString() + " - Failed to process send pickTicket due to " + pickTicketResponse.Message);
                                                            salesDataAccess.UpdateAutoSendPTLog(pickTicketRequest.SopType, pickTicketRequest.SopNumber, 0, "Error sending pick ticket: " + pickTicketResponse.Message);
                                                        }
                                                        else
                                                        {
                                                            logMessage.AppendLine(DateTime.Now.ToString() + " - Unknown error occurred and unable to process pick ticket. Please contact IT.");
                                                            salesDataAccess.UpdateAutoSendPTLog(pickTicketRequest.SopType, pickTicketRequest.SopNumber, 0, "-- Unknown error occurred and unable to process pick ticket");
                                                        }
                                                    }
                                                }
                                            }

                                            #endregion PT

                                            // Check WH cutoff time and send email
                                            if (inputDataTable.Rows[0]["WarehouseId"].ToString().Trim() != "" && (
                                                                                                                  Convert.ToDateTime(inputDataTable.Rows[0]["LineReqShipDate"]).Date == DateTime.Now.Date ||
                                                                                                                  Convert.ToDateTime(inputDataTable.Rows[0]["HdrReqShipDate"]).Date == DateTime.Now.Date))
                                            {
                                                logMessage.AppendLine(DateTime.Now.ToString() + " - Sending email for same day shipping notification.");
                                                EMailInformation sameDayEmail = new EMailInformation();
                                                sameDayEmail.Body = soRequest.WarehouseSameDayEmail.Body;
                                                sameDayEmail.EmailFrom = soRequest.WarehouseSameDayEmail.EmailFrom;
                                                sameDayEmail.EmailTo = soRequest.WarehouseSameDayEmail.EmailTo;
                                                sameDayEmail.EmailCc = "";
                                                sameDayEmail.EmailBcc = "";
                                                sameDayEmail.Subject = soRequest.WarehouseSameDayEmail.Subject;
                                                sameDayEmail.SmtpAddress = soRequest.WarehouseSameDayEmail.SmtpAddress;
                                                sameDayEmail.UserId = soRequest.WarehouseSameDayEmail.UserId;
                                                sameDayEmail.Password = soRequest.WarehouseSameDayEmail.Password;
                                                sameDayEmail.Body = sameDayEmail.Body.Replace("1234567", orderNumber);
                                                sameDayEmail.Body = sameDayEmail.Body.Replace("NA00###### ", nextAvailableFONumber);
                                                sameDayEmail.Body = sameDayEmail.Body.Replace("[DATE] ", DateTime.Now.Date.ToShortDateString());
                                                sameDayEmail.Body = sameDayEmail.Body.Replace("COMPANY NAME", inputDataTable.Rows[0]["CustNmbr"].ToString().Trim() + "-" +
                                                                                                                                                    inputDataTable.Rows[0]["CustName"].ToString().Trim());
                                                if (!string.IsNullOrEmpty(inputDataTable.Rows[0]["PrimaryEmail"].ToString()))
                                                {
                                                    sameDayEmail.EmailTo = inputDataTable.Rows[0]["PrimaryEmail"].ToString().Trim();
                                                    if (!string.IsNullOrEmpty(inputDataTable.Rows[0]["EmailBackup1"].ToString()))
                                                    {
                                                        sameDayEmail.EmailCc = inputDataTable.Rows[0]["EmailBackup1"].ToString().Trim();
                                                    }
                                                    if (!string.IsNullOrEmpty(inputDataTable.Rows[0]["EmailBackup2"].ToString()))
                                                    {
                                                        sameDayEmail.EmailBcc = inputDataTable.Rows[0]["EmailBackup2"].ToString().Trim();
                                                    }
                                                }


                                                // send email
                                                if (new EmailHelper().SendMail(sameDayEmail))
                                                    logMessage.AppendLine(DateTime.Now.ToString() + " - Mail successfully sent to the warehouse");
                                                else
                                                    logMessage.AppendLine(DateTime.Now.ToString() + " - Mail is not sent to the warehouse");
                                            }
                                            logMessage.AppendLine(DateTime.Now.ToString() + " --------------------------------------.");
                                        }
                                        else
                                        {
                                            logMessage.AppendLine(DateTime.Now.ToString() + " - Error : Order is not converted to FO .");
                                            logMessage.AppendLine(DateTime.Now.ToString() + " --------------------------------------.");
                                        }
                                    }
                                    catch (eConnectException econEx)
                                    {
                                        string errorCode = string.Empty;
                                        if (econEx.Message.Contains("Error Number = "))
                                        {
                                            logMessage.AppendLine(DateTime.Now.ToString() + " - Status : Error while transferring Order to FO into GP");

                                            errorCode = econEx.Message.Substring(econEx.Message.IndexOf("Error Number = ") + 15,
                                                (econEx.Message.IndexOf(" ", econEx.Message.IndexOf("Error Number = ") + 15) - (econEx.Message.IndexOf("Error Number = ") + 15)));

                                            logMessage.AppendLine(DateTime.Now.ToString() + " - Error Code: " + errorCode);
                                        }

                                        failureMailMessage.AppendLine("<tr><td>" + orderNumber +
                                                                      "</td><td>" + econEx.Message.Trim() + "</td></tr>");

                                        logMessage.AppendLine(DateTime.Now.ToString() + " - Econnect Error: " + econEx.Message);
                                        logMessage.AppendLine(DateTime.Now.ToString() + " - -------------------------------------.");
                                    }
                                    catch (System.Data.SqlClient.SqlException sqlEx)
                                    {
                                        failureMailMessage.AppendLine("<tr><td>" + orderNumber +
                                                                      "</td><td>" + sqlEx.Message.Trim() + "</td></tr>");
                                        logMessage.AppendLine(sqlEx.Message);
                                        logMessage.AppendLine(sqlEx.StackTrace);
                                        logMessage.AppendLine(DateTime.Now.ToString() + " - -------------------------------------.");
                                    }
                                    catch (Exception ex)
                                    {
                                        failureMailMessage.AppendLine("<tr><td>" + orderNumber +
                                                                      "</td><td>" + ex.Message.Trim() + "</td></tr>");
                                        logMessage.AppendLine(ex.Message);
                                        logMessage.AppendLine(ex.StackTrace);
                                        logMessage.AppendLine(DateTime.Now.ToString() + " - -------------------------------------.");
                                    }
                                    finally
                                    {
                                        if (eConObj != null)
                                        {
                                            eConObj.Dispose();
                                        }
                                    }
                                }
                                else
                                {
                                    logMessage.AppendLine(DateTime.Now.ToString() + " - Econnect XML is not generated.");
                                }
                            }
                            i = i + 1;
                        }
                    }
                    else
                    {
                        logMessage.AppendLine(DateTime.Now.ToString() + " - There is no order to transfer into FO. ");
                        logMessage.AppendLine(DateTime.Now.ToString() + " ----------------------------------------------.");
                    }
                }
                catch (Exception ex)
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message);
                    logMessage.AppendLine(DateTime.Now.ToString() + " - " + ex.StackTrace);
                    logMessage.AppendLine(DateTime.Now.ToString() + " - " + "-------------------------------------.");
                }
                finally
                {
                    //Send out email to tech team for failures
                    if (failureMailMessage.ToString().Contains("<tr><td>") || failureMailMessage.ToString().Contains("<tr><td colspan=3>"))
                    {
                        failureMailMessage.Append("</table>");
                        soRequest.TransferFailureEmail.Body = failureMailMessage.ToString();

                        if (new EmailHelper().SendMail(soRequest.TransferFailureEmail))
                            logMessage.AppendLine(DateTime.Now.ToString() + " - Failure mail has been successfully sent");
                        else
                            logMessage.AppendLine(DateTime.Now.ToString() + " - Failure mail failed to send.");
                    }

                    //log the message
                    new TextLogger().LogInformationIntoFile(logMessage.ToString(), soRequest.LoggingPath, soRequest.LoggingFileName);
                }
            }
            return true;
        }


        /// <summary>
        /// Calling PickTicket Service to Attach PT to XRM
        /// </summary>
        private void AttachPTToXrm(string nextFoNumber, string sopNumber, string pickTicketServiceUrl, ref StringBuilder logMessage)
        {
            try
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Pick Ticket service Method calling: " + pickTicketServiceUrl);
                BasicHttpBinding binding = new BasicHttpBinding();
                binding.MaxReceivedMessageSize = Int32.MaxValue;
                binding.MaxBufferSize = Int32.MaxValue;
                binding.MaxBufferPoolSize = Int32.MaxValue;
                binding.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
                binding.ReaderQuotas.MaxBytesPerRead = Int32.MaxValue;
                PickTicketServiceClient objPickTicketServiceClient = new PickTicketServiceClient(binding, new EndpointAddress(pickTicketServiceUrl));
                string pTStatus = objPickTicketServiceClient.GeneratePickTicket(nextFoNumber, sopNumber);
                logMessage.AppendLine(DateTime.Now.ToString() + " -  " + pTStatus);
                logMessage.AppendLine(DateTime.Now.ToString() + " - Pick Ticket service Method called Successfully...");
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message);
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.StackTrace);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Method to transform object into xml string
        /// </summary>
        /// <param name="inputTable"> input data </param>
        /// <returns> input xml </returns>
        private string SerializeToString(DataTable inputTable)
        {
            //// creates the serializer object
            using (StringWriter writer = new StringWriter())
            {
                inputTable.WriteXml(writer);
                return writer.ToString();
            }

        }

        /// <summary>
        /// Method to transform table information into xml file
        /// </summary>
        /// <param name="tableXml"> input xml </param>
        /// <param name="sopNumbe"> sop number </param>
        /// <param name="nextFoNumber"> fo number </param>
        /// <returns> econnect xml </returns>
        private static string TransformForFO(string tableXml, string sopNumber, string nextFoNumber, string styleSheetPath)
        {
            // Local variables.
            string transformedXml = string.Empty;
            //Create object for XmlDocument
            XmlDocument xmlDoc = new XmlDocument();
            //Create object for XslTransform 
            XslCompiledTransform xslTrans = new XslCompiledTransform();
            //Creating Argument List Object
            XsltArgumentList xsltArgsFO = new XsltArgumentList();

            //Loading ChemMsgEcomOrder XML
            xmlDoc.LoadXml(tableXml);
            xsltArgsFO.AddParam("Sopnumbe", string.Empty, nextFoNumber);
            xsltArgsFO.AddParam("Orignumb", string.Empty, sopNumber);
            xslTrans.Load(styleSheetPath);

            //Creating StringWriter Object
            StringWriter strWriter = new StringWriter(System.Globalization.CultureInfo.CurrentCulture);
            //Peroforming transformation
            xslTrans.Transform(xmlDoc, xsltArgsFO, strWriter);
            // Set the transformed xml.
            transformedXml = strWriter.ToString().Trim();
            // Dispose the objects.                                                
            strWriter.Dispose();

            // Return the transformed xml to the caller
            return transformedXml;
        }

        #endregion OrderTransferTOFO

        #region CommitmentMentManager

        public SalesOrderResponse RunCommitmentEngine(SalesOrderRequest commitRequest)
        {
            SalesOrderResponse salesOrderResponse = new SalesOrderResponse();

            ISalesOrderUpdateRepository salesDataAccess = null;

            commitRequest.ThrowIfNull("commitRequest");
            commitRequest.SalesOrderEntity.ThrowIfNull("commitRequest.SalesOrderEntity");
            try
            {
                salesOrderResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(commitRequest.ConnectionString);
                salesDataAccess.ExecuteCommitmentEngine(commitRequest);
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesOrderResponse.ErrorMessage = ex.Message.ToString().Trim();

            }
            return salesOrderResponse;
        }

        public SalesOrderResponse UpdateOrderDetailsToInvoiceType(SalesOrderRequest salesSofOtoInvoiceRequest)
        {
            SalesOrderResponse salesSofOtoInvoiceResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesSofOtoInvoiceRequest.ThrowIfNull("salesSOFOtoInvoiceRequest");
            salesSofOtoInvoiceRequest.SalesOrderEntity.ThrowIfNull("salesSOFOtoInvoiceRequest.SalesOrderEntity");
            try
            {
                salesSofOtoInvoiceResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesSofOtoInvoiceRequest.ConnectionString);
                salesDataAccess.UpdateOrderDetailsToInvoiceType(salesSofOtoInvoiceRequest);
                salesSofOtoInvoiceResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesSofOtoInvoiceResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesSofOtoInvoiceResponse.ErrorMessage = ex.Message.ToString().Trim();

            }

            return salesSofOtoInvoiceResponse;
        }

        #endregion CommitmentMentManager

        public SalesOrderResponse UpdateOrderDetailsToWarehouseIBoard(SalesOrderRequest salesWhiboardRequest)
        {
            SalesOrderResponse salesWhiboardResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesWhiboardRequest.ThrowIfNull("salesWhiboardRequest");
            salesWhiboardRequest.SalesOrderEntity.ThrowIfNull("salesWhiboardRequest");
            try
            {
                salesWhiboardResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesWhiboardRequest.ConnectionString);
                salesDataAccess.UpdateOrderDetailsToWarehouseIBoard(salesWhiboardRequest);
                salesWhiboardResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesWhiboardResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesWhiboardResponse.ErrorMessage = ex.Message.ToString().Trim();

            }

            return salesWhiboardResponse;
        }


        #region Cash Application Process

        /// <summary>
        /// Save order Type Business Logic
        /// </summary>
        /// <param name="salesOrderTypeRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse DistributeAmountToCashApplyInvoices(SalesOrderRequest salesApplyRequest)
        {
            SalesOrderResponse salesApplyResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;

            salesApplyRequest.ThrowIfNull("SalesApplyResponse");
            salesApplyRequest.SalesOrderEntity.ThrowIfNull("salesApplyRequest.SalesOrderEntity");
            try
            {
                salesApplyResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesApplyRequest.ConnectionString);
                salesDataAccess.DistributeAmountToCashApplyInvoices(salesApplyRequest, salesApplyRequest.CompanyID);
                salesApplyResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesApplyResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesApplyResponse.ErrorMessage = ex.Message.ToString().Trim();

            }

            return salesApplyResponse;
        }

        /// <summary>
        /// Apply To Open Orders
        /// Called from Engine
        /// </summary>
        /// <param name="soRequest"></param>
        /// <returns></returns>
        public bool ApplyToOpenOrders(SalesOrderRequest soRequest)
        {
            ISalesOrderUpdateRepository salesDataAccess = null;
            StringBuilder logMessage = new StringBuilder();
            StringBuilder failureMailMessage = new StringBuilder();
            failureMailMessage.Append("<html><h3>Following are the error(s) occured while applying cash to open orders through cash application process engine</h3><br>");
            failureMailMessage.Append("<table border=1><tr><th>CID Number</th><th>Customer Name</th><th>Invoice Number</th><th>Applied Amount</th><th>Document Number</th><th>Error Description</th></tr>");

            StringBuilder multiCurrencyMailMessage = new StringBuilder();
            multiCurrencyMailMessage.Append("<html><h3>Please apply the following payments to corresponding posted invoices manually</h3><br>");
            multiCurrencyMailMessage.Append("<table border=1><tr><th>CID Number</th><th>Customer Name</th><th>Invoice Number</th><th>Applied Amount</th><th>Document Number</th></tr>");

            logMessage.AppendLine(DateTime.Now.ToString() + " - ****************************************************************");
            logMessage.AppendLine(DateTime.Now.ToString() + " - " + soRequest.Source + " - Cash Application Process job started");
            logMessage.AppendLine(DateTime.Now.ToString() + " - Fetching all the details from the view.");

            soRequest.ThrowIfNull("Apply To Open Orders Request object is null");

            try
            {
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(soRequest.ConnectionString);
                BulkOrderTransferEntity cashEntity = salesDataAccess.GetDocumentForApplyToOpenOrdersForEngine(soRequest.CompanyID);
                cashEntity.UnlockedOrders = new List<string>();

                if (cashEntity.SalesOrderDetails == null || cashEntity.SalesOrderDetails.Tables.Count == 0)
                {
                    return true;
                }

                foreach (DataRow theRow in cashEntity.SalesOrderDetails.Tables[0].Rows)
                {
                    if (!cashEntity.UnlockedOrders.Contains(theRow["DocumentNumber"].ToString().Trim()))
                        cashEntity.UnlockedOrders.Add(theRow["DocumentNumber"].ToString().Trim());
                }

                foreach (DataRow theRow in cashEntity.SalesOrderDetails.Tables[1].Rows)
                {
                    multiCurrencyMailMessage.AppendLine("<tr><td>" + theRow["CustomerNumber"].ToString().Trim() +
                                                          "</td><td>" + theRow["CustomerName"].ToString().Trim() +
                                                          "</td><td>" + theRow["ApplyToDocumentNumber"].ToString().Trim() +
                                                          "</td><td>" + theRow["CurrencySymbol"].ToString().Trim() + theRow["ApplyAmount"].ToString().Trim() +
                                                          "</td><td>" + theRow["DocumentNumber"].ToString().Trim() +
                                                          "</td></tr>");
                }

                if (cashEntity.UnlockedOrders.Count > 0)
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Document Numbers : " + String.Join(",", (string[])cashEntity.UnlockedOrders.ToArray()));
                }

                //for every order, transfer to FO
                if (cashEntity.UnlockedOrders.Count > 0)
                {
                    foreach (string documentNumber in cashEntity.UnlockedOrders)
                    {
                        logMessage.AppendLine(DateTime.Now.ToString() + " - --------------------------------------------------");
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Started applying cash to open orders: " + documentNumber);

                        // Get specific order details to view
                        DataView specificOrderDV = new DataView(cashEntity.SalesOrderDetails.Tables[0]);
                        specificOrderDV.RowFilter = "DocumentNumber='" + documentNumber + "'";
                        DataTable inputDataTable = specificOrderDV.ToTable();

                        //prepare Econnect XML
                        string inputXml = SerializeToString(inputDataTable);
                        inputXml = inputXml.Replace("<DocumentElement>", "<Order>");
                        inputXml = inputXml.Replace("</DocumentElement>", "</Order>");
                        inputXml = inputXml.Replace("<Table>", "<Details>");
                        inputXml = inputXml.Replace("</Table>", "</Details>");
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Input XML: " + inputXml);
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Creating eConnect XML");
                        string eConnectXml = TransformForCash(inputXml, soRequest.Source, soRequest.StyleSheetPath);
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Econnect XML : " + eConnectXml);

                        // Create FO in GP
                        if (eConnectXml != string.Empty)
                        {
                            eConnectMethods eConObj = null;
                            bool result = false;
                            try
                            {
                                eConObj = new eConnectMethods();
                                logMessage.AppendLine(DateTime.Now.ToString() + " - Initializing Econnect Object");


                                result = eConObj.eConnect_EntryPoint(soRequest.CompanyID == 1 ? soRequest.NAEconnectConnectionString : soRequest.EUEconnectConnectionString, Microsoft.Dynamics.GP.eConnect.EnumTypes.ConnectionStringType.SqlClient, eConnectXml, Microsoft.Dynamics.GP.eConnect.EnumTypes.SchemaValidationType.XSD, "");

                                if (result == true)
                                {
                                    UpdateStatusForCash(cashEntity.SalesOrderDetails.Tables[0], "cash has been successfully applied to document number", "0", documentNumber, ref failureMailMessage);

                                    logMessage.AppendLine(DateTime.Now.ToString() + " - cash has been successfully applied to document number : " + documentNumber);
                                    logMessage.AppendLine(DateTime.Now.ToString() + " --------------------------------------.");
                                }
                                else
                                {
                                    UpdateStatusForCash(cashEntity.SalesOrderDetails.Tables[0], "Error : Cash is not linked to document number.", "3", documentNumber, ref failureMailMessage);

                                    logMessage.AppendLine(DateTime.Now.ToString() + " - Error : Cash is not linked to document number.");
                                    logMessage.AppendLine(DateTime.Now.ToString() + " --------------------------------------.");
                                }
                            }
                            catch (eConnectException econEx)
                            {
                                string errorCode = string.Empty;
                                if (econEx.Message.Contains("Error Number = "))
                                {
                                    logMessage.AppendLine(DateTime.Now.ToString() + " - Status : Error while applying cash to open orders into GP");

                                    errorCode = econEx.Message.Substring(econEx.Message.IndexOf("Error Number = ") + 15,
                                        (econEx.Message.IndexOf(" ", econEx.Message.IndexOf("Error Number = ") + 15) - (econEx.Message.IndexOf("Error Number = ") + 15)));

                                    logMessage.AppendLine(DateTime.Now.ToString() + " - Error Code: " + errorCode);
                                }

                                UpdateStatusForCash(cashEntity.SalesOrderDetails.Tables[0], econEx.Message.Trim(), errorCode, documentNumber, ref failureMailMessage);

                                logMessage.AppendLine(DateTime.Now.ToString() + " - Econnect Error: " + econEx.Message);
                                logMessage.AppendLine(DateTime.Now.ToString() + " - -------------------------------------.");
                            }
                            catch (System.Data.SqlClient.SqlException sqlEx)
                            {
                                UpdateStatusForCash(cashEntity.SalesOrderDetails.Tables[0], sqlEx.Message.Trim(), "2", documentNumber, ref failureMailMessage);

                                logMessage.AppendLine(sqlEx.Message);
                                logMessage.AppendLine(sqlEx.StackTrace);
                                logMessage.AppendLine(DateTime.Now.ToString() + " - -------------------------------------.");
                            }
                            catch (Exception ex)
                            {
                                UpdateStatusForCash(cashEntity.SalesOrderDetails.Tables[0], ex.Message.Trim(), "1", documentNumber, ref failureMailMessage);

                                logMessage.AppendLine(ex.Message);
                                logMessage.AppendLine(ex.StackTrace);
                                logMessage.AppendLine(DateTime.Now.ToString() + " - -------------------------------------.");
                            }
                            finally
                            {
                                if (eConObj != null)
                                {
                                    eConObj.Dispose();
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(DateTime.Now.ToString() + " - Econnect XML is not generated.");
                        }
                    }

                    //update data into back end
                    salesDataAccess.SaveDocumentForApplyToOpenOrdersForEngine(cashEntity, soRequest.CompanyID);
                }
                else
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " - There is no order to process for cash application.");
                    logMessage.AppendLine(DateTime.Now.ToString() + " ----------------------------------------------.");
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message);
                logMessage.AppendLine(DateTime.Now.ToString() + " - " + ex.StackTrace);
                logMessage.AppendLine(DateTime.Now.ToString() + " - " + "-------------------------------------.");
            }
            finally
            {
                //Send out email to tech team for failures
                if (failureMailMessage.ToString().Contains("<tr><td>") || failureMailMessage.ToString().Contains("<tr><td colspan=3>"))
                {
                    failureMailMessage.Append("</table>");
                    soRequest.SalesOrderFailureEmail.Body = failureMailMessage.ToString();

                    if (new EmailHelper().SendMail(soRequest.SalesOrderFailureEmail))
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Failure mail has been successfully sent");
                    else
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Failure mail failed to send.");
                }

                //Send out email to tech team for failures
                if (multiCurrencyMailMessage.ToString().Contains("<tr><td>") || multiCurrencyMailMessage.ToString().Contains("<tr><td colspan=3>"))
                {
                    multiCurrencyMailMessage.Append("</table>");
                    soRequest.SalesPriorityOrdersEmail.Body = multiCurrencyMailMessage.ToString();
                    soRequest.SalesPriorityOrdersEmail.Subject = soRequest.SalesPriorityOrdersEmail.Subject + " - " + soRequest.Source.ToString().Trim();

                    if (new EmailHelper().SendMail(soRequest.SalesPriorityOrdersEmail))
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Multi-Currency mail has been successfully sent");
                    else
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Multi-Currency mail failed to send.");
                }

                //log the message
                new TextLogger().LogInformationIntoFile(logMessage.ToString(), soRequest.LoggingPath, soRequest.LoggingFileName);
            }

            return true;
        }

        private static string TransformForCash(string tableXml, string companyName, string styleSheetPath)
        {
            // Local variables.
            string transformedXml = string.Empty;
            //Create object for XmlDocument
            XmlDocument xmlDoc = new XmlDocument();
            //Create object for XslTransform 
            XslCompiledTransform xslTrans = new XslCompiledTransform();
            //Creating Argument List Object
            XsltArgumentList xsltArgsCash = new XsltArgumentList();

            //Loading ChemMsgEcomOrder XML
            xmlDoc.LoadXml(tableXml);
            xsltArgsCash.AddParam("CompanyID", string.Empty, companyName.ToUpper());
            xslTrans.Load(styleSheetPath);

            //Creating StringWriter Object
            StringWriter strWriter = new StringWriter(System.Globalization.CultureInfo.CurrentCulture);
            //Peroforming transformation
            xslTrans.Transform(xmlDoc, xsltArgsCash, strWriter);
            // Set the transformed xml.
            transformedXml = strWriter.ToString().Trim();
            // Dispose the objects.                                                
            strWriter.Dispose();

            // Return the transformed xml to the caller
            return transformedXml;
        }

        /// <summary>
        /// Log the mail content
        /// </summary>
        /// <param name="documentRowId"></param>
        /// <param name="documentNumber"></param>
        /// <param name="vendorId"></param>
        /// <param name="errorMessage"></param>
        private void UpdateStatusForCash(DataTable paymentDv, string errorMessage, string errorId, string documentNumber, ref StringBuilder failureMailMessage)
        {
            try
            {
                IEnumerable<DataRow> rows =
                            from DataRow row in paymentDv.Rows
                            where row.Field<string>("DocumentNumber").ToString().Trim() == documentNumber.ToString()
                            select row;

                if (rows != null && rows.Count() > 0)
                {
                    foreach (var rowitem in rows)
                    {
                        rowitem["ErrorId"] = errorId;
                        rowitem["ErrorDescription"] = string.IsNullOrEmpty(errorMessage) ? string.Empty : errorMessage.Trim();

                        if (errorId != "0")
                        {
                            failureMailMessage.AppendLine("<tr><td>" + rowitem["CustomerNumber"].ToString().Trim() +
                                                         "</td><td>" + rowitem["CustomerName"].ToString().Trim() +
                                                         "</td><td>" + rowitem["ApplyToDocumentNumber"].ToString().Trim() +
                                                         "</td><td>" + rowitem["CurrencySymbol"].ToString().Trim() + rowitem["ApplyAmount"].ToString().Trim() +
                                                         "</td><td>" + documentNumber +
                                                         "</td><td>" + errorMessage.Trim() + "</td></tr>");
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Fetch Cash Application
        /// </summary>
        /// <param name="aRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse FetchApplyToOpenOrder(ReceivablesRequest aRequest)
        {
            ReceivablesResponse cashApplicationResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;
            aRequest.ThrowIfNull("aRequest");

            try
            {
                cashApplicationResponse = new ReceivablesResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(aRequest.ConnectionString);
                cashApplicationResponse = salesDataAccess.FetchApplyToOpenOrder(aRequest);
                if (cashApplicationResponse != null)
                    cashApplicationResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                else
                    cashApplicationResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;

            }
            catch (Exception ex)
            {
                cashApplicationResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                cashApplicationResponse.ErrorMessage = ex.Message.ToString().Trim();
            }

            return cashApplicationResponse;
        }

        /// <summary>
        /// Save Cash Application
        /// </summary>
        /// <param name="aRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse SaveApplyToOpenOrder(ReceivablesRequest aRequest)
        {
            ReceivablesResponse cashApplicationResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;
            aRequest.ThrowIfNull("aRequest");

            try
            {
                cashApplicationResponse = new ReceivablesResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(aRequest.ConnectionString);
                cashApplicationResponse.ValidationStatus = salesDataAccess.SaveApplyToOpenOrder(aRequest);
                cashApplicationResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                cashApplicationResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                cashApplicationResponse.ErrorMessage = ex.Message.ToString().Trim();
            }

            return cashApplicationResponse;
        }

        /// <summary>
        /// Save Cash Application
        /// </summary>
        /// <param name="aRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse UpdateApplyToOpenOrder(ReceivablesRequest aRequest)
        {
            ReceivablesResponse cashApplicationResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;
            aRequest.ThrowIfNull("aRequest");

            try
            {
                cashApplicationResponse = new ReceivablesResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(aRequest.ConnectionString);
                if (salesDataAccess.UpdateApplyToOpenOrder(aRequest))
                    cashApplicationResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                else
                    cashApplicationResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;

            }
            catch (Exception ex)
            {
                cashApplicationResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                cashApplicationResponse.ErrorMessage = ex.Message.ToString().Trim();
            }

            return cashApplicationResponse;
        }

        /// <summary>
        /// Delete Cash Application
        /// </summary>
        /// <param name="aRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse DeleteApplyToOpenOrder(ReceivablesRequest aRequest)
        {
            ReceivablesResponse cashApplicationResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;
            aRequest.ThrowIfNull("aRequest");

            try
            {
                cashApplicationResponse = new ReceivablesResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(aRequest.ConnectionString);
                if (salesDataAccess.DeleteApplyToOpenOrder(aRequest))
                    cashApplicationResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                else
                    cashApplicationResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;

            }
            catch (Exception ex)
            {
                cashApplicationResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                cashApplicationResponse.ErrorMessage = ex.Message.ToString().Trim();
            }

            return cashApplicationResponse;
        }

        public ReceivablesResponse GetReceivablesDetail(ReceivablesRequest receivablesRequest)
        {
            ReceivablesResponse receivablesResponse = null;
            ISalesOrderUpdateRepository receivablesDataAccess = null;

            receivablesRequest.ThrowIfNull("receivablesRequest");

            try
            {
                receivablesResponse = new ReceivablesResponse();
                receivablesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(receivablesRequest.ConnectionString);
                receivablesResponse = receivablesDataAccess.GetReceivablesDetail(receivablesRequest);
                receivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                receivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                receivablesResponse.ErrorMessage = ex.Message.ToString().Trim();

            }

            return receivablesResponse;
        }

        public ReceivablesResponse GetDistributeAmountDetail(ReceivablesRequest receivablesRequest)
        {
            ReceivablesResponse receivablesResponse = null;
            ISalesOrderUpdateRepository receivablesDataAccess = null;

            receivablesRequest.ThrowIfNull("receivablesRequest");
            try
            {
                receivablesResponse = new ReceivablesResponse();
                receivablesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(receivablesRequest.ConnectionString);
                receivablesResponse = receivablesDataAccess.GetDistributeAmountDetail(receivablesRequest);
                receivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                receivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                receivablesResponse.ErrorMessage = ex.Message.ToString().Trim();

            }

            return receivablesResponse;
        }

        public ReceivablesResponse SaveReceivablesDetail(ReceivablesRequest receivablesRequest)
        {
            ReceivablesResponse receivablesResponse = null;
            ISalesOrderUpdateRepository receivablesDataAccess = null;

            receivablesRequest.ThrowIfNull("receivablesRequest");

            try
            {
                receivablesResponse = new ReceivablesResponse();
                receivablesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(receivablesRequest.ConnectionString);
                receivablesDataAccess.SaveReceivablesDetail(receivablesRequest);
                receivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                receivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                receivablesResponse.ErrorMessage = ex.Message.ToString().Trim();

            }

            return receivablesResponse;
        }

        #endregion Cash Application Process

        #region WarehouseClosure

        public SalesOrderResponse ValidateWarehouseClosureDate(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse salesResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;
            try
            {
                salesResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderRequest.ConnectionString);
                salesResponse = salesDataAccess.ValidateWarehouseClosureDate(salesOrderRequest);
                salesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                salesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesResponse.ErrorMessage = ex.Message.ToString().Trim();
            }

            return salesResponse;
        }

        #endregion

        #region EFTAutomation


        #region EFT_Customer_Mapping_Window

        /// <summary>
        /// Get EFT Customer Details...
        /// </summary>
        /// <param name="eftRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse GetEFTCustomerMappingDetails(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = null;
            ISalesOrderUpdateRepository eftDataAccess = null;

            eftRequest.ThrowIfNull("eftRequest");
            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(eftRequest.ConnectionString);
                eftResponse = eftDataAccess.GetEFTCustomerMappingDetails(eftRequest);
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return eftResponse;
        }

        /// <summary>
        /// Save EFT Customer Details...
        /// </summary>
        /// <param name="eftRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse SaveEFTCustomerMappingDetails(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = null;
            ISalesOrderUpdateRepository eftDataAccess = null;

            eftRequest.ThrowIfNull("eftRequest");
            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(eftRequest.ConnectionString);
                eftResponse = eftDataAccess.SaveEFTCustomerMappingDetails(eftRequest);
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return eftResponse;
        }

        #endregion EFT_Customer_Mapping_Window

        /// <summary>
        /// Get EFT Customer Details...
        /// </summary>
        /// <param name="eftRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse GetEFTCustomerRemittances(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = null;
            ISalesOrderUpdateRepository eftDataAccess = null;

            eftRequest.ThrowIfNull("eftRequest");
            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(eftRequest.ConnectionString);
                eftResponse = eftDataAccess.GetEFTCustomerRemittances(eftRequest);
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return eftResponse;
        }


        /// <summary>
        /// SaveEFTCustomerRemittances...
        /// </summary>
        /// <param name="eftRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse DeleteBankEntryItemReference(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = null;
            ISalesOrderUpdateRepository eftDataAccess = null;

            eftRequest.ThrowIfNull("eftRequest");
            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(eftRequest.ConnectionString);

                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;

                int returnval = eftDataAccess.DeleteBankEntryItemReference(eftRequest);
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;

            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return eftResponse;
        }

        /// <summary>
        /// Get EFT Customer Details...
        /// </summary>
        /// <param name="eftRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse GetEFTPaymentRemittances(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = null;
            ISalesOrderUpdateRepository eftDataAccess = null;

            eftRequest.ThrowIfNull("eftRequest");
            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(eftRequest.ConnectionString);
                eftResponse = eftDataAccess.GetEFTPaymentRemittances(eftRequest);
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return eftResponse;
        }

        /// <summary>
        /// SaveEFTCustomerRemittances...
        /// </summary>
        /// <param name="eftRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse SaveEFTCustomerRemittances(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = null;
            ISalesOrderUpdateRepository eftDataAccess = null;

            eftRequest.ThrowIfNull("eftRequest");
            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(eftRequest.ConnectionString);
                eftResponse = eftDataAccess.SaveEFTCustomerRemittances(eftRequest);
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return eftResponse;
        }

        /// <summary>
        /// Fetch eft customer id
        /// </summary>
        /// <param name="aRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse FetchCustomerId(ReceivablesRequest aRequest)
        {
            ReceivablesResponse eftResponse = null;
            ISalesOrderUpdateRepository eftDataAccess = null;
            aRequest.ThrowIfNull("aRequest");

            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(aRequest.ConnectionString);
                eftResponse = eftDataAccess.FetchCustomerId(aRequest);
                if (eftResponse != null)
                    eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                else
                    eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;

            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }

            return eftResponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse FetchBatchId(ReceivablesRequest aRequest)
        {
            ReceivablesResponse eftResponse = null;
            ISalesOrderUpdateRepository eftDataAccess = null;
            aRequest.ThrowIfNull("aRequest");

            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(aRequest.ConnectionString);
                eftResponse = eftDataAccess.FetchBatchId(aRequest);
                if (eftResponse != null)
                    eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                else
                    eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;

            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }

            return eftResponse;
        }

        /// <summary>
        /// Fetch eft document number (item reference)
        /// </summary>
        /// <param name="aRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse FetchDocumentNumber(ReceivablesRequest aRequest)
        {
            ReceivablesResponse eftResponse = null;
            ISalesOrderUpdateRepository eftDataAccess = null;
            aRequest.ThrowIfNull("aRequest");

            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(aRequest.ConnectionString);
                eftResponse = eftDataAccess.FetchDocumentNumber(aRequest);
                if (eftResponse != null)
                    eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                else
                    eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;

            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }

            return eftResponse;
        }

        /// <summary>
        /// Fetch eft document number (item reference)
        /// </summary>
        /// <param name="aRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse FetchReferenceId(ReceivablesRequest aRequest)
        {
            ReceivablesResponse eftResponse = null;
            ISalesOrderUpdateRepository eftDataAccess = null;
            aRequest.ThrowIfNull("aRequest");

            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(aRequest.ConnectionString);
                eftResponse = eftDataAccess.FetchReferenceId(aRequest);
                if (eftResponse != null)
                    eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                else
                    eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;

            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }

            return eftResponse;
        }
        /// <summary>
        /// Fetch eft document number (item reference)
        /// </summary>
        /// <param name="aRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse FetchCustomerIdForReference(ReceivablesRequest aRequest)
        {
            ReceivablesResponse eftResponse = null;
            ISalesOrderUpdateRepository eftDataAccess = null;
            aRequest.ThrowIfNull("aRequest");

            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(aRequest.ConnectionString);
                eftResponse = eftDataAccess.FetchCustomerIdForReference(aRequest);
                if (eftResponse != null)
                    eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                else
                    eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;

            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }

            return eftResponse;
        }


        /// <summary>
        /// ValidateEftCustomer...
        /// </summary>
        /// <param name="eftRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse ValidateEftCustomer(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = null;
            ISalesOrderUpdateRepository eftDataAccess = null;

            eftRequest.ThrowIfNull("eftRequest");
            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(eftRequest.ConnectionString);
                if (eftDataAccess.ValidateEftCustomer(eftRequest))
                    eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                else
                    eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return eftResponse;
        }

        /// <summary>
        /// ValidateEFTItemReference...
        /// </summary>
        /// <param name="eftRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse ValidateEFTItemReference(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = null;
            ISalesOrderUpdateRepository eftDataAccess = null;

            eftRequest.ThrowIfNull("eftRequest");
            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(eftRequest.ConnectionString);
                if (eftDataAccess.ValidateEFTItemReference(eftRequest))
                    eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                else
                    eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return eftResponse;
        }


        /// <summary>
        /// ValidateEftReference...
        /// </summary>
        /// <param name="eftRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse ValidateEftReference(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = null;
            ISalesOrderUpdateRepository eftDataAccess = null;

            eftRequest.ThrowIfNull("eftRequest");
            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(eftRequest.ConnectionString);
                if (eftDataAccess.ValidateEftReference(eftRequest))
                    eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                else
                    eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return eftResponse;
        }


        /// <summary>
        /// ValidateEftEmailReference...
        /// </summary>
        /// <param name="eftRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse ValidateEftEmailReference(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = null;
            ISalesOrderUpdateRepository eftDataAccess = null;

            eftRequest.ThrowIfNull("eftRequest");
            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(eftRequest.ConnectionString);
                if (eftDataAccess.ValidateEftEmailReference(eftRequest))
                    eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                else
                    eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return eftResponse;
        }


        public ReceivablesResponse ValidateEFTCustomerRemittanceSummary(ReceivablesRequest receivablesRequest)
        {
            ReceivablesResponse objReceivablesResponse = null;
            ISalesOrderUpdateRepository eftDataAccess = null;
            DataTable eftCustomerDT = new DataTable();

            try
            {
                objReceivablesResponse = new ReceivablesResponse();
                eftCustomerDT = ToDataTable<EFTCustomerPayment>(receivablesRequest.EFTCustomerPaymentList);
                eftDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(receivablesRequest.ConnectionString);
                objReceivablesResponse = eftDataAccess.ValidateEFTCustomerRemittanceSummary(eftCustomerDT, receivablesRequest.CompanyId);
                objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;

            }
            catch (Exception ex)
            {
                objReceivablesResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                objReceivablesResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return objReceivablesResponse;
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

        /// <summary>
        /// Get EFT Email Remittance Entry 
        /// </summary>
        /// <param name="eftEmailRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse GetEFTEmailRemittances(ReceivablesRequest eftEmailRequest)
        {
            ReceivablesResponse eftResponse = null;
            ISalesOrderUpdateRepository eftDataAccess = null;

            eftEmailRequest.ThrowIfNull("eftEmailRequest");
            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(eftEmailRequest.ConnectionString);
                eftResponse = eftDataAccess.GetEFTEmailRemittances(eftEmailRequest);
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return eftResponse;
        }

        /// <summary>
        /// Save EFT Email Remittance Entry
        /// </summary>
        /// <param name="eftEmailRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse SaveEFTEmailRemittances(ReceivablesRequest eftEmailRequest)
        {
            ReceivablesResponse eftResponse = null;
            ISalesOrderUpdateRepository eftDataAccess = null;

            eftEmailRequest.ThrowIfNull("eftRequest");
            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(eftEmailRequest.ConnectionString);
                eftResponse = eftDataAccess.SaveEFTEmailRemittances(eftEmailRequest);
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return eftResponse;
        }

        /// <summary>
        /// Get EFT Customer Details...
        /// </summary>
        /// <param name="eftRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse GetEFTPaymentRemittanceAmountDetails(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = null;
            ISalesOrderUpdateRepository eftDataAccess = null;

            eftRequest.ThrowIfNull("eftRequest");
            try
            {
                eftResponse = new ReceivablesResponse();
                eftDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(eftRequest.ConnectionString);
                eftResponse = eftDataAccess.GetEFTPaymentRemittanceAmountDetails(eftRequest);
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                eftResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                eftResponse.ErrorMessage = ex.Message.ToString().Trim();
            }
            return eftResponse;
        }

        #endregion EFTAutomation

        #region Termshold

        public bool SendReport(SalesOrderResponse salesOrderResponse, SalesOrderRequest salesOrderRequest)
        {
            StringBuilder logMessage = new StringBuilder();
            string MailSubject = string.Empty;
            string strCustomerId = string.Empty, strCustomerName = string.Empty, strInvoiceNumber = string.Empty, strCurrentShippDate = string.Empty;
            try
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - SendReport method in ChemPoint.GP.SalesOrderBL is started");

                SendEmailRequest objSendEmailRequest = new SendEmailRequest();
                objSendEmailRequest.EmailInformation = new EMailInformation();

                if (salesOrderResponse != null)
                {
                    strInvoiceNumber = salesOrderResponse.SalesOrderDetails.SalesOrderDetails.SalesOrderDetails.OrderHeader.SopNumber;
                    strCustomerId = salesOrderResponse.SalesOrderDetails.SalesOrderDetails.Customer.CustomerId;
                    strCustomerName = salesOrderResponse.SalesOrderDetails.SalesOrderDetails.Customer.CustomerName;
                    //Req ship Date
                    strCurrentShippDate = salesOrderResponse.SalesOrderDetails.SalesOrderDetails.SalesOrderDetails.OrderHeader.OrderDate.ToShortDateString();
                }
                else
                {
                    strInvoiceNumber = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.SopNumber;
                    strCustomerId = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerId;
                    strCustomerName = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerName;
                    //Req ship Date
                    strCurrentShippDate = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.OrderDate.ToShortDateString();
                }

                objSendEmailRequest.ConnectionString = ConfigurationManager.ConnectionStrings["GPCUSTOMIZATIONCONSTRING"].ConnectionString;
                objSendEmailRequest.EmailInformation.SmtpAddress = ConfigurationManager.AppSettings["SMTPServcer"];
                objSendEmailRequest.EmailConfigID = Convert.ToInt32(ConfigurationManager.AppSettings["TermHoldEmailConfigId"]);
                objSendEmailRequest.EmailInformation.EmailFrom = ConfigurationManager.AppSettings["TermHoldEmailFrom"];
                objSendEmailRequest.EmailInformation.Signature = ConfigurationManager.AppSettings["TermHoldSignature"];
                objSendEmailRequest.EmailInformation.IsDataTableBodyRequired = false;

                MailSubject = "Prepayment Customer w/Remaining Balance | " + strInvoiceNumber + " |" + " Cur Sched Ship Date: " + strCurrentShippDate + " | " + strCustomerName + " | " + " Main Account: " + strCustomerId;

                objSendEmailRequest.EmailInformation.Subject = MailSubject;

                var response = new EmailBusiness().SendEmail(objSendEmailRequest);
                return true;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.Message.Trim());
                logMessage.AppendLine(DateTime.Now.ToString() + " - Stack Trace: " + ex.StackTrace.Trim());
                return false;
            }
            finally
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - SendReport method in ChemPoint.GP.SalesOrderBL ended.");

            }


        }
        #endregion Termshold



        public SalesOrderResponse AuditCustomizeServiceSkU(SalesOrderRequest salesOrderRequest)
        {
            StringBuilder AuditCustomizeServiceSkULogginSB = new StringBuilder();
            string AuditCustomizeServiceSkULoggin;
            AuditCustomizeServiceSkULogginSB.AppendLine(DateTime.Now.ToString() + "AuditCustomizeServiceSkULogginSB Business layer Calling");
            SalesOrderResponse salesOrderResponse = null;
            ISalesOrderUpdateRepository salesDataAccess = null;
            

            salesOrderRequest.ThrowIfNull("salesOrderRequest");
            salesOrderRequest.SalesOrderEntity.ThrowIfNull("salesOrderRequest");
            try
            {
                salesOrderResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderRequest.ConnectionString);
                AuditCustomizeServiceSkULogginSB.AppendLine(DateTime.Now.ToString() + "AuditCustomizeServiceSkULoggin DL Calling from BL");
                AuditCustomizeServiceSkULoggin = AuditCustomizeServiceSkULogginSB.ToString();
                salesOrderResponse = salesDataAccess.AuditCustomizeServiceSkU(salesOrderRequest, ref AuditCustomizeServiceSkULoggin);
                AuditCustomizeServiceSkULogginSB.AppendLine(DateTime.Now.ToString() + AuditCustomizeServiceSkULoggin);
                salesOrderResponse.LogginMessage = AuditCustomizeServiceSkULogginSB.ToString();
                
            }
            catch (Exception ex)
            {
                AuditCustomizeServiceSkULogginSB.AppendLine(DateTime.Now.ToString() + "Error in AuditCustomizeServiceSkULoggin BL");
                salesOrderResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                salesOrderResponse.ErrorMessage = ex.Message.ToString().Trim();
                salesOrderResponse.LogginMessage = AuditCustomizeServiceSkULogginSB.ToString();
            }

            return salesOrderResponse;
        }

       
    }
}
