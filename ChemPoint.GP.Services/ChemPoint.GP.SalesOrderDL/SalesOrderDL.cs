using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ChemPoint.GP.DataContracts.Sales;
using ChemPoint.GP.Entities.Business_Entities;
using Chempoint.GP.Infrastructure.Utils;
using Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider.Context;
using ChemPoint.GP.SalesOrderDL.Utils;
using Chempoint.GP.Infrastructure.Config;
using System.Data.SqlClient;
using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.Entities.BaseEntities;
using Chempoint.GP.Infrastructure.Maps.Sales;
using ChemPoint.GP.Entities.Business_Entities.Sales;

namespace ChemPoint.GP.SalesOrderDL
{
    /// <summary>
    ///-------------------------------------------------------------------------------------------
    ///     Date		Author          Description
    ///-------------------------------------------------------------------------------------------
    /// **26Jan2017     RReddy      Exclude Service Sku Changes included
    /// </summary>
    public class SalesOrderDL : RepositoryBase, ISalesOrderUpdateRepository
    {
        public SalesOrderDL(string connectionString)
            : base(new GpAddInDbContext(connectionString))
        {
        }

        public SalesOrderDL(SqlConnection connectionString)
            : base(new GpAddInDbContext(connectionString))
        {
        }

        #region SOPEntry

        /// <summary>
        /// Fetch sales order Related details...
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="CompanyId"></param>Th
        public SalesOrderResponse GetSalesOrder(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderEntity salesOrderEntity = new SalesOrderEntity();
            SalesOrderInformation salesOrderInformation = new SalesOrderInformation();
            SalesOrderDetails salesOrderDetails = new SalesOrderDetails();
            OrderHeader orderHeader = new OrderHeader();
            List<SalesInstruction> salesHeaderInstructionList = new List<SalesInstruction>();
            SalesOrderType salesOrderType = new SalesOrderType();
            OrderSchedule orderSchedule = new OrderSchedule();
            List<AddressInformation> addressInformationList = new List<AddressInformation>();
            AddressInformation addressInformation = new AddressInformation();
            SalesOrderResponse salesOrderResponse = new SalesOrderResponse();
            List<SalesLineItem> salesLineItemList = new List<SalesLineItem>();
            var cmd = CreateStoredProcCommand(salesOrderRequest.CompanyID == 1 ? Configuration.SPFetchSalesOrderNA : Configuration.SPFetchSalesOrderEU);
            cmd.Parameters.AddInputParams(Configuration.SPFetchSopEntryDetailsParam1, SqlDbType.Int, salesOrderRequest.SalesOrderEntity.SopType);
            cmd.Parameters.AddInputParams(Configuration.SPFetchSopEntryDetailsParam2, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.SPFetchSopEntryDetailsParam3, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerId, 21);
            bool isSSKUExcempted;
            var ds = base.GetDataSet(cmd);

            salesOrderType = base.GetEntity<SalesOrderType, SalesOrderTypeMap>(ds.Tables[0]); // Sales order Type 
            if (salesOrderType != null)
            {
                salesOrderDetails.SalesOrderType = salesOrderType;
                int value;
                int.TryParse(ds.Tables[0].Rows[0]["SopType"].ToString(), out value);
                salesOrderEntity.SopType = value;
                salesOrderEntity.SopNumber = ds.Tables[0].Rows[0]["SopNumber"].ToString();
            }

            orderSchedule = base.GetEntity<OrderSchedule, SalesOrderScheduleMap>(ds.Tables[1]);  // Saels Date Entry
            if (orderSchedule != null)
                salesOrderDetails.OrderSchedule = orderSchedule;

            orderHeader = base.GetEntity<OrderHeader, SalesHeaderOrderMap>(ds.Tables[2]);  //Sales transportation Entry
            if (orderHeader != null)
            {
                salesOrderEntity.ShiptoAddressId = ds.Tables[2].Rows[0]["ShipToAddressID"].ToString();
                salesOrderEntity.BilltoAddressId = ds.Tables[2].Rows[0]["BillToAddressID"].ToString();
                bool.TryParse(ds.Tables[2].Rows[0]["IsCustomerExcempted"].ToString(), out isSSKUExcempted);
                orderHeader.IsCustomerExcempted = isSSKUExcempted;
                orderHeader.ShipToCountry = ds.Tables[2].Rows[0]["ShipToCountry"].ToString();
                orderHeader.ShipToCountryCode = ds.Tables[2].Rows[0]["ShipToCountryCode"].ToString();
                orderHeader.ShipFromCountry = ds.Tables[2].Rows[0]["ShipFromCountry"].ToString();
                orderHeader.ShipFromCountryCode = ds.Tables[2].Rows[0]["ShipFromCountryCode"].ToString();
                orderHeader.FinalDestinationCountry = ds.Tables[2].Rows[0]["FinalDestinationCountry"].ToString();
                orderHeader.FinalDestinationCountryCode = ds.Tables[2].Rows[0]["FinalDestinationCountryCode"].ToString();
            }
            else if (orderHeader == null)
            {
                orderHeader = new OrderHeader();
                orderHeader.OrderGuid = "";
                orderHeader.CustomerGuid = "";
                orderHeader.FreightTerm = "";
                orderHeader.IncoTerm = "";
                orderHeader.CarrierName = "";
                orderHeader.CarrierAccountNumber = "";
                orderHeader.CarrierPhone = "";
                orderHeader.ServiceType = "";
                orderHeader.ImporterofRecord = "";
                orderHeader.CustomBroker = "";
                salesOrderEntity.ShiptoAddressId = "";
                salesOrderEntity.BilltoAddressId = "";
                orderHeader.PaymentTerm = "";
            }

            salesHeaderInstructionList = base.GetAllEntities<SalesInstruction, SalesOrderHeaderInstructionMap>(ds.Tables[3]).ToList();  // Sales header instruction
            if (salesHeaderInstructionList.Count != 0)
                orderHeader.SalesHeaderInstructionEntity = salesHeaderInstructionList;



            addressInformation = base.GetEntity<AddressInformation, ThirdPartyAddressMap>(ds.Tables[4]);  // Sales 3rd party
            if (addressInformation != null)
                orderHeader.Carrier3rdPartyAddress = addressInformation;

            addressInformationList = base.GetAllEntities<AddressInformation, CustomerDetailAddressIDMap>(ds.Tables[5]).ToList();  // Customre detail Bill Id
            if (addressInformationList != null && orderHeader != null)
                orderHeader.AddressDetails = addressInformationList;

            addressInformation = base.GetEntity<AddressInformation, ThirdPartyAddressMap>(ds.Tables[6]);  //Customer pickup address
            orderHeader.CustomerPickupAddress = addressInformation;

            salesLineItemList = base.GetAllEntities<SalesLineItem, SalesOrderAllocatedQtyMap>(ds.Tables[7]).ToList();  // Customre detail Bill Id
            if (salesLineItemList.Count != 0)
                salesOrderDetails.LineItems = salesLineItemList;

            salesOrderDetails.OrderHeader = orderHeader;
            salesOrderInformation.SalesOrderDetails = salesOrderDetails;
            salesOrderEntity.SalesOrderDetails = salesOrderInformation;
            salesOrderResponse.SalesOrderDetails = salesOrderEntity;

            return salesOrderResponse;
        }

        /// <summary>
        /// Save SalesOrder (sopEntry)
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        public object SaveSalesOrder(SalesOrderRequest salesOrderRequest, int companyId)
        {
            DataTable orderHeaderDT = DataTableMapper.GetSalesOrderHeaderDataTable(salesOrderRequest,
                DataColumnMappings.SaveSalesOrder);

            DataTable orderScheduleDT = DataTableMapper.GetSalesOrderScheduleDataTable(salesOrderRequest,
                DataColumnMappings.SaveScheduleDate);

            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPSaveSopEntryDetailsNA : Configuration.SPSaveSopEntryDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPSaveSopEntryDetailsParam1, SqlDbType.Int, salesOrderRequest.SalesOrderEntity.SopType);
            cmd.Parameters.AddInputParams(Configuration.SPSaveSopEntryDetailsParam2, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.SPSaveSopEntryDetailsParam3, SqlDbType.Bit, salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.ToBeCancelled);
            cmd.Parameters.AddInputParams(Configuration.SPSaveSopEntryDetailsParam4, SqlDbType.Bit, salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.CustomizeServiceSkus);
            cmd.Parameters.AddInputParams(Configuration.SPSaveSopEntryDetailsParam5, SqlDbType.Structured, orderScheduleDT);
            cmd.Parameters.AddInputParams(Configuration.SPSaveSopEntryDetailsParam6, SqlDbType.Structured, orderHeaderDT);
            return base.Insert(cmd);
        }

        public SalesOrderResponse FetchIncoTerm(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse salesOrderResponse = new SalesOrderResponse();
            SalesOrderEntity salesOrderEntity = new SalesOrderEntity();
            SalesOrderDetails salesOrderDetails = new SalesOrderDetails();
            SalesOrderInformation salesOrderInformation = new SalesOrderInformation();
            OrderHeader orderHeader = new OrderHeader();
            List<IncoTerm> incoTermList = new List<ChemPoint.GP.Entities.BaseEntities.IncoTerm>();

            var cmd = CreateStoredProcCommand(Configuration.SPFetchIncoTerm);
            cmd.Parameters.AddInputParams(Configuration.SPFetchIncoTermParam1, SqlDbType.Int, salesOrderRequest.CompanyID);
            var ds = base.GetDataSet(cmd);

            incoTermList = base.GetAllEntities<IncoTerm, IncoTermMap>(ds.Tables[0]).ToList();

            orderHeader.Inco = incoTermList;
            salesOrderDetails.OrderHeader = orderHeader;
            salesOrderInformation.SalesOrderDetails = salesOrderDetails;
            salesOrderEntity.SalesOrderDetails = salesOrderInformation;
            salesOrderResponse.SalesOrderDetails = salesOrderEntity;
            return salesOrderResponse;
        }

        public SalesOrderResponse GetCountryDetails(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse salesOrderResponse = new SalesOrderResponse();
            SalesOrderEntity salesOrderEntity = new SalesOrderEntity();
            SalesOrderDetails salesOrderDetails = new SalesOrderDetails();
            SalesOrderInformation salesOrderInformation = new SalesOrderInformation();
            OrderHeader orderHeader = new OrderHeader();
            List<AddressInformation> addressInformationList = new List<AddressInformation>();
            AddressInformation addressInformation = new AddressInformation();

            var cmd = CreateStoredProcCommand(Configuration.SPFetchCountryDetails);
            cmd.Parameters.AddInputParams(Configuration.SPFetchCountryDetailsParam1, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.SPFetchCountryDetailsParam2, SqlDbType.Int, salesOrderRequest.SalesOrderEntity.SopType);
            cmd.Parameters.AddOutputParams(Configuration.SPFetchCountryDetailsParam3, SqlDbType.VarChar, 7);
            cmd.Parameters.AddOutputParams(Configuration.SPFetchCountryDetailsParam4, SqlDbType.VarChar, 61);

            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            addressInformation.CountryCode = Convert.ToString(commandResult.Parameters["@" + Configuration.SPFetchCountryDetailsParam3].Value);
            addressInformation.Country = Convert.ToString(commandResult.Parameters["@" + Configuration.SPFetchCountryDetailsParam4].Value);

            addressInformationList.Add(addressInformation);
            orderHeader.AddressDetails = addressInformationList;
            salesOrderDetails.OrderHeader = orderHeader;
            salesOrderInformation.SalesOrderDetails = salesOrderDetails;
            salesOrderEntity.SalesOrderDetails = salesOrderInformation;
            salesOrderResponse.SalesOrderDetails = salesOrderEntity;
            return salesOrderResponse;
        }

        public SalesOrderResponse GetSalesCurSchedDelDate(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse salesOrderResponse = new SalesOrderResponse();
            SalesOrderEntity salesOrderEntity = new SalesOrderEntity();
            SalesOrderDetails salesOrderDetails = new SalesOrderDetails();
            SalesOrderInformation salesOrderInformation = new SalesOrderInformation();
            OrderSchedule orderSchedule = new OrderSchedule();

            DateTime reqShipDate = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderSchedule.RequestedShipDate;
            DateTime custReqShip = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderSchedule.CustomerRequestedShipDate;
            DateTime custReqDel = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderSchedule.CustomerRequestedDeliveryDate;

            string spName = string.Empty;
            if (salesOrderRequest.CompanyID == 1)
                spName = Configuration.SPGetSopCurSchedDelDateNA;
            else
                spName = Configuration.SPGetSopCurSchedDelDateEU;

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.SPGetSopCurSchedDelDateParam1, SqlDbType.DateTime, reqShipDate);
            cmd.Parameters.AddInputParams(Configuration.SPGetSopCurSchedDelDateParam2, SqlDbType.DateTime, custReqShip);
            cmd.Parameters.AddInputParams(Configuration.SPGetSopCurSchedDelDateParam3, SqlDbType.DateTime, custReqDel);
            cmd.Parameters.AddOutputParams(Configuration.SPGetSopCurSchedDelDateParam4, SqlDbType.DateTime);

            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            orderSchedule.RequestedDeliveryDate = Convert.ToDateTime(commandResult.Parameters["@" + Configuration.SPGetSopCurSchedDelDateParam4].Value);

            salesOrderDetails.OrderSchedule = orderSchedule;
            salesOrderInformation.SalesOrderDetails = salesOrderDetails;
            salesOrderEntity.SalesOrderDetails = salesOrderInformation;
            salesOrderResponse.SalesOrderDetails = salesOrderEntity;
            return salesOrderResponse;
        }

        public SalesOrderResponse GetSalesCurSchedShipDate(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderEntity salesOrderEntity = new SalesOrderEntity();
            SalesOrderInformation salesOrderInformation = new SalesOrderInformation();
            SalesOrderDetails salesOrderDetails = new SalesOrderDetails();
            OrderSchedule orderSchedule = new OrderSchedule();

            SalesOrderResponse salesOrderResponse = new SalesOrderResponse();

            var cmd = CreateStoredProcCommand(salesOrderRequest.CompanyID == 1 ? Configuration.SPGetSopCurSchedShipDateNA : Configuration.SPGetSopCurSchedShipDateEU);
            cmd.Parameters.AddInputParams(Configuration.SPGetSopCurSchedShipDateParam1, SqlDbType.Int, salesOrderRequest.SalesOrderEntity.SopType);
            cmd.Parameters.AddInputParams(Configuration.SPGetSopCurSchedShipDateParam2, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SopNumber, 21);

            var ds = base.GetDataSet(cmd);

            orderSchedule = base.GetEntity<OrderSchedule, SalesOrderScheduleMap>(ds.Tables[0]);  // Saels Date Entry
            if (orderSchedule != null)
            {
                int value;
                int.TryParse(ds.Tables[0].Rows[0]["SopType"].ToString(), out value);
                salesOrderEntity.SopType = value;
                salesOrderEntity.SopNumber = ds.Tables[0].Rows[0]["SopNumber"].ToString();
                salesOrderDetails.OrderSchedule = orderSchedule;
            }

            salesOrderInformation.SalesOrderDetails = salesOrderDetails;
            salesOrderEntity.SalesOrderDetails = salesOrderInformation;
            salesOrderResponse.SalesOrderDetails = salesOrderEntity;

            return salesOrderResponse;
        }

        public object UpdateTaxScheduleIdToLine(SalesOrderRequest salesOrderRequest)
        {
            var cmd = CreateStoredProcCommand(Configuration.SPUpdateTaxScheduleIdToLine);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateTaxScheduleIdToLineParam1, SqlDbType.Int, salesOrderRequest.SalesOrderEntity.SopType);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateTaxScheduleIdToLineParam2, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.SPUpdateTaxScheduleIdToLineParam3, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.TaxScheduleId, 15);
            return base.Insert(cmd);
        }

        /// <summary>
        /// Save Allocated Qty Details
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse SaveAllocatedQty(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse salesOrderResponse = new SalesOrderResponse();

            DataTable allocatedQtyDT = DataTableMapper.GetAllocatedQty(salesOrderRequest,
                DataColumnMappings.SaveAllocatedQty);

            var cmd = CreateStoredProcCommand(salesOrderRequest.AuditInformation.CompanyId == 1 ? Configuration.SPSaveAllocatedQtyDetailsNA : Configuration.SPSaveAllocatedQtyDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPSaveAllocatedQtyDetailsTableType, SqlDbType.Structured, allocatedQtyDT);
            base.Insert(cmd);
            salesOrderResponse.Status = ResponseStatus.Success;
            return salesOrderResponse;
        }


        /// <summary>
        /// Fetch Changed Qty Details
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse FetchSalesOrderLineForTermHold(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse salesOrderResponse = new SalesOrderResponse();
            OrderHeader orderHdr = new OrderHeader();

            List<SalesLineItem> salesLineItemList = new List<SalesLineItem>();
            SalesOrderEntity salesOrderEntity = new SalesOrderEntity();
            SalesOrderInformation salesOrderInformation = new SalesOrderInformation();
            SalesOrderDetails salesOrderDetails = new SalesOrderDetails();
            CustomerInformation customerInfo = new CustomerInformation();

            DataTable changedQtyDT = DataTableMapper.GetAllocatedQty(salesOrderRequest,
                DataColumnMappings.SaveAllocatedQty);

            var cmd = CreateStoredProcCommand(salesOrderRequest.AuditInformation.CompanyId == 1 ? Configuration.SPFetchSopChangedQtyNA : Configuration.SPFetchSopChangedQtyEU);
            cmd.Parameters.AddInputParams(Configuration.SPFetchSopChangedQtyParam1, SqlDbType.Structured, changedQtyDT);
            cmd.Parameters.AddOutputParams(Configuration.SPFetchSopChangedQtyParam2, SqlDbType.Decimal, 21);

            var ds = base.GetDataSet(cmd);
            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;

            if ((ds.Tables[0].Rows.Count > 0) && Convert.ToDecimal(commandResult.Parameters["@" + Configuration.SPFetchSopChangedQtyParam2].Value) > salesOrderRequest.TermHoldRemainingBalance)
            {
                //order Header 
                int value = 0;
                int.TryParse(ds.Tables[0].Rows[0]["SopType"].ToString(), out value);
                orderHdr.SopType = value;
                orderHdr.SopNumber = ds.Tables[0].Rows[0]["SopNumber"].ToString();
                orderHdr.OrderDate = Convert.ToDateTime(ds.Tables[0].Rows[0]["ReqShipDate"].ToString().Trim());

                //Customer Details
                customerInfo.CustomerId = ds.Tables[0].Rows[0]["CustomerID"].ToString().Trim();
                customerInfo.CustomerName = ds.Tables[0].Rows[0]["CustomerName"].ToString().Trim();

                //Line Details
                foreach (DataRow FetchSopChangedQtyDR in ds.Tables[0].Rows)
                {

                    SalesLineItem lineItem = new SalesLineItem();
                    lineItem.OrderLineId = Convert.ToInt32(FetchSopChangedQtyDR["LineItemSequence"]);
                    lineItem.ItemNumber = FetchSopChangedQtyDR["ItemNumber"].ToString().Trim(); ;
                    lineItem.Quantity = Convert.ToDecimal(FetchSopChangedQtyDR["Quantity"]);
                    lineItem.IsServiceSKU = Convert.ToBoolean(FetchSopChangedQtyDR["IsServcieSKU"]);
                    salesLineItemList.Add(lineItem);
                    lineItem = null;
                }


                if (salesLineItemList.Count != 0)
                    salesOrderDetails.LineItems = salesLineItemList;
                salesOrderDetails.OrderHeader = orderHdr;

                salesOrderInformation.SalesOrderDetails = salesOrderDetails;
                salesOrderInformation.Customer = customerInfo;
                salesOrderEntity.SalesOrderDetails = salesOrderInformation;
                salesOrderResponse.SalesOrderDetails = salesOrderEntity;
                salesOrderResponse.Status = ResponseStatus.Success;
            }
            else
                salesOrderResponse.Status = ResponseStatus.NoRecord;
            return salesOrderResponse;
        }
        #endregion SOPEntry

        #region SaleslineItem

        /// <summary>
        /// save sales item detail Entry details...
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="CompanyId"></param>Th
        public object SaveSalesItemDetail(SalesOrderRequest salesOrderRequest, int companyId)
        {
            DataTable salesItemDetailDT = DataTableMapper.GetSalesLineItemDataTable(salesOrderRequest,
                DataColumnMappings.SaveItemDetail);

            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPSaveItemEntryDetailsNA : Configuration.SPSaveItemEntryDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPSaveItemEntryDetailsParam1, SqlDbType.Structured, salesItemDetailDT);
            return base.Insert(cmd);
        }

        /// <summary>
        /// Get salesItemdetail Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse GetSalesItemDetail(SalesOrderRequest salesOrderRequest)
        {
            string sopNumber = string.Empty;
            int sopType = 0, lineItemsequence = 0, componentSeqNumber = 0;
            SalesLineItem salesLineItem = new SalesLineItem();
            List<SalesLineItem> salesLineItemList = new List<SalesLineItem>();
            List<SalesInstruction> salesLineInstructionList = new List<SalesInstruction>();
            List<TrackingInformation> salesLineTrackingNumberList = new List<TrackingInformation>();
            SalesOrderEntity salesOrderEntity = new SalesOrderEntity();
            SalesOrderInformation salesOrderInformation = new SalesOrderInformation();
            SalesOrderDetails salesOrderDetails = new SalesOrderDetails();
            SalesOrderResponse salesOrderResponse = new SalesOrderResponse();

            if (salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.LineItems.Any())
            {
                var itemDetail = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.LineItems[0];
                sopNumber = itemDetail.SopNumber;
                sopType = itemDetail.SopType;
                lineItemsequence = itemDetail.OrderLineId;
                componentSeqNumber = itemDetail.ComponentSequenceNumber;
            }

            if (!string.IsNullOrEmpty(sopNumber) && sopType != 0 && lineItemsequence != 0)
            {
                var cmd = CreateStoredProcCommand(salesOrderRequest.CompanyID == 1 ? Configuration.SPFetchItemEntryDetailsNA : Configuration.SPFetchItemEntryDetailsEU);
                cmd.Parameters.AddInputParams(Configuration.SPFetchItemEntryDetailsParam1, SqlDbType.Int, sopType);
                cmd.Parameters.AddInputParams(Configuration.SPFetchItemEntryDetailsParam2, SqlDbType.VarChar, sopNumber, 21);
                cmd.Parameters.AddInputParams(Configuration.SPFetchItemEntryDetailsParam3, SqlDbType.Int, lineItemsequence);
                cmd.Parameters.AddInputParams(Configuration.SPFetchItemEntryDetailsParam4, SqlDbType.Int, componentSeqNumber);
                var ds = base.GetDataSet(cmd);
                //Assign Line item detail...
                salesLineItem = base.GetEntity<SalesLineItem, SalesItemDetailMap>(ds.Tables[0]); // Sales order Type 
                if (salesLineItem != null)
                {
                    //Assign Line instruction id...
                    salesLineInstructionList = base.GetAllEntities<SalesInstruction, SalesOrderLineInstructionMap>(ds.Tables[1]).ToList();
                    if (salesLineInstructionList.Count != 0)
                        salesLineItem.SalesLineInstruction = salesLineInstructionList;

                    if (ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
                    {
                        //Assign Line instruction id...
                        salesLineTrackingNumberList = base.GetAllEntities<TrackingInformation, SalesOrderLineTrackingNumberMap>(ds.Tables[2]).ToList();
                        if (salesLineTrackingNumberList.Count != 0)
                            salesLineItem.SalesLineTrackingNumber = salesLineTrackingNumberList;
                    }

                    salesLineItemList.Add(salesLineItem);
                    salesOrderDetails.LineItems = salesLineItemList;
                    salesOrderInformation.SalesOrderDetails = salesOrderDetails;
                    salesOrderEntity.SalesOrderDetails = salesOrderInformation;
                    salesOrderResponse.SalesOrderDetails = salesOrderEntity;
                }
            }
            else
                throw new Exception("Invalid SopNumber or SopType or LineSeqnumber");
            return salesOrderResponse;
        }

        #endregion SaleslineItem

        /// <summary>
        /// Save Order Type from Sop entry window 
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        public object SaveOrderTypeDetail(SalesOrderRequest salesOrderRequest)
        {
            DataTable salesOrderTypeDT = DataTableMapper.GetSalesOrderTypeDataTable(salesOrderRequest,
                DataColumnMappings.SaveSalesOrderType);

            var cmd = CreateStoredProcCommand(salesOrderRequest.CompanyID == 1 ? Configuration.SPSaveOrderTypeDetailsNA : Configuration.SPSaveOrderTypeDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPSaveOrderTypeDetailsOrderType, SqlDbType.Structured, salesOrderTypeDT);
            return base.Insert(cmd);
        }

        /// <summary>
        /// Save Order Type from Sop entry window 
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        public object UpdatePrintPickTicketStatus(string invoiceNumber, int sopType, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPCpupdatesoppickticketstatusNA : Configuration.SPCpupdatesoppickticketstatusEU);
            cmd.Parameters.AddInputParams(Configuration.SPCpupdatesoppickticketstatusSopType, SqlDbType.Int, sopType);
            cmd.Parameters.AddInputParams(Configuration.SPCpupdatesoppickticketstatusSopNumber, SqlDbType.VarChar, invoiceNumber, 21);
            return base.Insert(cmd);
        }

        /// <summary>
        /// Save Third party address SPSaveThirdPartyAddressDetailsNA 
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        public object SaveThirdPartyAddress(SalesOrderRequest salesOrderRequest)
        {
            DataTable save3rdPartyDT = DataTableMapper.Get3rdPartyDataTable(salesOrderRequest,
                DataColumnMappings.SaveThirdPartyAddress);
            var cmd = CreateStoredProcCommand(salesOrderRequest.CompanyID == 1 ? Configuration.SPSaveThirdPartyAddressDetailsNA : Configuration.SPSaveThirdPartyAddressDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPSaveThirdPartyAddressDetailsParam1, SqlDbType.Structured, save3rdPartyDT);
            return base.Update(cmd);
        }

        /// <summary>
        /// Save customer pickup DL
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        public object SaveCustomerPickupAddress(SalesOrderRequest salesOrderRequest)
        {
            DataTable saveCustomerPickupDT = DataTableMapper.GetCustomerPickupDataTable(salesOrderRequest,
                DataColumnMappings.SaveThirdPartyAddress);

            var cmd = CreateStoredProcCommand(salesOrderRequest.CompanyID == 1 ? Configuration.SPSaveCustomerPickupAddressDetailsNA : Configuration.SPSaveCustomerPickupAddressDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPSaveCustomerPickupAddressDetailsParam1, SqlDbType.Structured, saveCustomerPickupDT);
            return base.Update(cmd);
        }

        /// <summary>
        /// Saving AddressID From Custoemr Detail Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public object SaveSopTransactionAddressCodes(SalesOrderRequest salesOrderRequest)
        {
            DataTable salesOrderAddressCodeDT = DataTableMapper.GetTransactionAddressCode(salesOrderRequest,
                DataColumnMappings.SaveTransactionAddressCodes);
            var cmd = CreateStoredProcCommand(salesOrderRequest.CompanyID == 1 ? Configuration.SPSaveTransactionAddressCodesNA : Configuration.SPSaveTransactionAddressCodesEU);
            cmd.Parameters.AddInputParams(Configuration.SPSaveTransactionAddressCodesParam1, SqlDbType.Structured, salesOrderAddressCodeDT);
            return base.Update(cmd);
        }

        /// <summary>
        /// Validate SopTransactionAddressCodes  ...
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse ValidateSopTransactionAddressCodes(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse salesOrderResponse = new SalesOrderResponse();

            SalesOrderDetails salesOrderDetails = new SalesOrderDetails();
            SalesOrderInformation salesOrderInformation = new SalesOrderInformation();
            SalesOrderEntity salesOrderEntity1 = new SalesOrderEntity();
            AddressInformation addressInformation = new AddressInformation();
            OrderHeader orderHeader = new OrderHeader();
            List<AddressInformation> addressInformationList = new List<AddressInformation>();
            int outaddcount;

            List<int> addressCount = new List<int>();
            var cmd = CreateStoredProcCommand(salesOrderRequest.CompanyID == 1 ? Configuration.SPValidateSopTransactionAddressCodesNA : Configuration.SPValidateSopTransactionAddressCodesEU);
            cmd.Parameters.AddInputParams(Configuration.SPValidateSopTransactionAddressCodesParam1, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerId);
            DataSet validateSopTransactionAddressCodesDS = base.GetDataSet(cmd);   //DB Hit

            if (validateSopTransactionAddressCodesDS.Tables.Count > 0 && validateSopTransactionAddressCodesDS.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow validateSopTransactionAddressCodesDR in validateSopTransactionAddressCodesDS.Tables[0].Rows)
                {
                    outaddcount = -1;
                    int.TryParse(validateSopTransactionAddressCodesDR["AddressCount"].ToString(), out outaddcount);
                    addressInformation.CustomerID = validateSopTransactionAddressCodesDR["CustomerID"].ToString().Trim();
                    addressInformation.AddressId = Convert.ToInt16(validateSopTransactionAddressCodesDR["AddressTypeID"].ToString());
                    addressCount.Add(outaddcount);  //AddressCount List
                    addressInformationList.Add(addressInformation);  // CustomerID and AddressTypeID list...
                }
            }

            orderHeader.AddressDetails = addressInformationList;
            salesOrderDetails.OrderHeader = orderHeader;
            salesOrderInformation.SalesOrderDetails = salesOrderDetails;
            salesOrderEntity1.SalesOrderDetails = salesOrderInformation;
            salesOrderResponse.AddressCount = addressCount;
            salesOrderResponse.SalesOrderDetails = salesOrderEntity1;

            return salesOrderResponse;
        }

        /// <summary>
        /// Validate ServiceSkuItems
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse ValidateSopTransactionServiceSkuItems(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse salesOrderResponse = new SalesOrderResponse();
            SalesOrderDetails salesOrderDetails = new SalesOrderDetails();
            SalesOrderInformation salesOrderInformation = new SalesOrderInformation();
            SalesOrderEntity salesOrderEntity = new SalesOrderEntity();
            List<SalesLineItem> salesLineItemList = new List<SalesLineItem>();
            SalesLineItem salesLineItem = null;

            DataTable salesOrderTypeDT = DataTableMapper.GetSalesOrderTypeDataTable(salesOrderRequest,
                DataColumnMappings.SaveSalesOrderType);

            DataTable orderHeaderDT = DataTableMapper.GetSalesOrderHeaderDataTable(salesOrderRequest,
                DataColumnMappings.SaveSalesOrder);

            var cmd = CreateStoredProcCommand(salesOrderRequest.CompanyID == 1 ? Configuration.SPValidateSopTransactionServiceSkuItemsNA : Configuration.SPValidateSopTransactionServiceSkuItemsEU);
            cmd.Parameters.AddInputParams(Configuration.SPValidateSopTransactionServiceSkuItemsParam1, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.SPValidateSopTransactionServiceSkuItemsParam2, SqlDbType.Int, salesOrderRequest.SalesOrderEntity.SopType);
            cmd.Parameters.AddInputParams(Configuration.SPValidateSopTransactionServiceSkuItemsParam3, SqlDbType.Bit, salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.CustomizeServiceSkus);
            cmd.Parameters.AddInputParams(Configuration.SPValidateSopTransactionServiceSkuItemsParam4, SqlDbType.DateTime, salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderSchedule.RequestedShipDate);
            cmd.Parameters.AddInputParams(Configuration.SPValidateSopTransactionServiceSkuItemsParam5, SqlDbType.Structured, salesOrderTypeDT);
            cmd.Parameters.AddInputParams(Configuration.SPValidateSopTransactionServiceSkuItemsParam6, SqlDbType.Structured, orderHeaderDT);


            DataSet validateSopTransactionServiceSkuItemsDS = base.GetDataSet(cmd);   //DB Hit

            if (validateSopTransactionServiceSkuItemsDS.Tables.Count > 0 && validateSopTransactionServiceSkuItemsDS.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow validateSopTransactionServiceSkuItemsDR in validateSopTransactionServiceSkuItemsDS.Tables[0].Rows)
                {
                    salesLineItem = new SalesLineItem();

                    salesLineItem.SopType = Convert.ToInt16(validateSopTransactionServiceSkuItemsDR["SopType"].ToString());
                    salesLineItem.SopNumber = validateSopTransactionServiceSkuItemsDR["SopNumber"].ToString().Trim();
                    salesLineItem.ItemNumber = validateSopTransactionServiceSkuItemsDR["ItemNumber"].ToString().Trim();
                    salesLineItem.ItemDescription = validateSopTransactionServiceSkuItemsDR["ItemDescription"].ToString().Trim();
                    salesLineItem.ItemUofM = validateSopTransactionServiceSkuItemsDR["UofM"].ToString().Trim();
                    salesLineItem.UnitPriceAmount = Convert.ToDecimal(validateSopTransactionServiceSkuItemsDR["ServiceSKUPrice"]);
                    salesLineItem.LineStatus = validateSopTransactionServiceSkuItemsDR["ErrorMessage"].ToString().Trim();

                    salesLineItemList.Add(salesLineItem);  // CustomerID and AddressTypeID list...
                }
            }
            salesOrderDetails.LineItems = salesLineItemList;
            salesOrderInformation.SalesOrderDetails = salesOrderDetails;
            salesOrderEntity.SalesOrderDetails = salesOrderInformation;
            salesOrderResponse.SalesOrderDetails = salesOrderEntity;
            return salesOrderResponse;
        }

        /// <summary>
        /// Save Header Instruction Details ...
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public object SaveHeaderCommentInstruction(SalesOrderRequest salesOrderRequest)
        {
            DataTable salesOrderAddressCodeDT = DataTableMapper.GetHeaderCommentInstruction(salesOrderRequest,
                DataColumnMappings.SaveHeaderComment);
            var cmd = CreateStoredProcCommand(salesOrderRequest.CompanyID == 1 ? Configuration.SPSaveSopHeaderInstructionDetailsNA : Configuration.SPSaveSopHeaderInstructionDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPSaveSopHeaderInstructionDetailsParam1, SqlDbType.Structured, salesOrderAddressCodeDT);
            return base.Update(cmd);
        }

        #region PushtoGP

        /// <summary>
        /// Get salesItemdetail Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse GetOrderDetailsForPushOrderToGP(SalesOrderRequest salesOrderRequest, int companyID)
        {
            SalesOrderResponse salesOrderResponse = new SalesOrderResponse();

            StringBuilder itemNumbers = new StringBuilder();
            foreach (var itemDetail in salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.LineItems)
            {
                itemNumbers.Append(itemDetail.ItemNumber.Trim() + "~");
            }
            if (!string.IsNullOrEmpty(salesOrderRequest.SalesOrderEntity.SopNumber))
            {
                var cmd = CreateStoredProcCommand(companyID == 1 ? Configuration.SPFetchOrderDetailsNA : Configuration.SPFetchOrderDetailsEU);
                cmd.Parameters.AddInputParams(Configuration.SPFetchOrderDetailsParam1, SqlDbType.VarChar, itemNumbers.ToString(), 1000);
                cmd.Parameters.AddInputParams(Configuration.SPFetchOrderDetailsParam2, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.OrderCurrency, 15);
                cmd.Parameters.AddInputParams(Configuration.SPFetchOrderDetailsParam3, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerId, 15);
                cmd.Parameters.AddInputParams(Configuration.SPFetchOrderDetailsParam4, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Warehouse.WarehouseId, 15);
                cmd.Parameters.AddInputParams(Configuration.SPFetchOrderDetailsParam5, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.ShipVia, 50);
                cmd.Parameters.AddInputParams(Configuration.SPFetchOrderDetailsParam6, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.FreightTerm, 50);

                DataSet orderDS = base.GetDataSet(cmd);

                foreach (DataRow row in orderDS.Tables[0].Rows)
                {
                    foreach (var itemDetail in salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.LineItems)
                    {
                        if (itemDetail.ItemNumber == row["ItemNmbr"].ToString().Trim())
                        {
                            itemDetail.ItemUofM = row["Uofm"].ToString().Trim();
                            int decimalPlaces;
                            Int32.TryParse(row["DecPlCur"].ToString().Trim(), out decimalPlaces);
                            decimalPlaces = decimalPlaces - 1;
                            if (decimalPlaces <= 0)
                                itemDetail.CurrencyDecimalPlaces = 0;
                            else
                                itemDetail.CurrencyDecimalPlaces = (decimalPlaces <= 2 ? 100 : Convert.ToInt32(Math.Pow(10, decimalPlaces)));
                        }
                    }
                }

                if (orderDS.Tables[1] != null && orderDS.Tables[1].Rows.Count > 0)
                {
                    int decimalPlaces;
                    Int32.TryParse(orderDS.Tables[1].Rows[0]["FreightDecimal"].ToString(), out decimalPlaces);
                    salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.FreightDecimalPlaces = (decimalPlaces <= 2 ? 100 : Convert.ToInt32(Math.Pow(10, decimalPlaces)));

                    Int32.TryParse(orderDS.Tables[1].Rows[0]["ExtendedDecimal"].ToString(), out decimalPlaces);
                    salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.ExtendedDecimalPlaces = (decimalPlaces <= 2 ? 100 : Convert.ToInt32(Math.Pow(10, decimalPlaces)));
                }
                if (companyID == 1)
                {
                    if (orderDS.Tables[2] != null && orderDS.Tables[2].Rows.Count > 0)
                    {
                        salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Warehouse.Address1 = orderDS.Tables[2].Rows[0]["Address1"].ToString().Trim();
                        salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Warehouse.Address2 = orderDS.Tables[2].Rows[0]["Address2"].ToString().Trim();
                        salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Warehouse.Address3 = orderDS.Tables[2].Rows[0]["Address3"].ToString().Trim();
                        salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Warehouse.City = orderDS.Tables[2].Rows[0]["City"].ToString().Trim();
                        salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Warehouse.State = orderDS.Tables[2].Rows[0]["State"].ToString().Trim();
                        salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Warehouse.Country = orderDS.Tables[2].Rows[0]["Country"].ToString().Trim();
                        salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Warehouse.ZipCode = orderDS.Tables[2].Rows[0]["ZipCode"].ToString().Trim();
                    }

                    if (orderDS.Tables[3] != null && orderDS.Tables[3].Rows.Count > 0)
                    {
                        salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.ShipViaType = orderDS.Tables[3].Rows[0]["ShipType"].ToString().Trim();
                    }

                    if (orderDS.Tables[4] != null && orderDS.Tables[4].Rows.Count > 0)
                    {
                        salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerTaxExemptCode = orderDS.Tables[4].Rows[0]["TaxExempt"].ToString().Trim();
                        salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerTaxUseCode = orderDS.Tables[4].Rows[0]["UseCode"].ToString().Trim();
                    }
                }
            }

            salesOrderResponse.SalesOrderDetails = salesOrderRequest.SalesOrderEntity;

            return salesOrderResponse;
        }

        #endregion PushtoGP

        #region PTERequest

        /// <summary>
        /// Get salesItemdetail Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public bool UpdatePteRequestToGP(SalesOrderRequest salesOrderRequest)
        {
            bool status = false;

            if (salesOrderRequest.CompanyID == 1)
            {
                DataTable pteRequest = new DataTable();
                pteRequest.Columns.Add("SopNumber", typeof(string));
                pteRequest.Columns.Add("SopType", typeof(int));
                pteRequest.Columns.Add("ItemNumber", typeof(string));
                pteRequest.Columns.Add("LocationCode", typeof(string));
                pteRequest.Columns.Add("QuantityOrdered", typeof(decimal));

                foreach (var itemDetail in salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.LineItems)
                {
                    DataRow row = pteRequest.NewRow();
                    row["SopNumber"] = salesOrderRequest.SalesOrderEntity.SopNumber;
                    row["SopType"] = salesOrderRequest.SalesOrderEntity.SopType;
                    row["ItemNumber"] = itemDetail.ItemNumber;
                    row["LocationCode"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Warehouse.WarehouseId;
                    row["QuantityOrdered"] = itemDetail.OrderedQuantity;
                    pteRequest.Rows.Add(row);
                }

                if (!string.IsNullOrEmpty(salesOrderRequest.SalesOrderEntity.SopNumber))
                {
                    var cmd = CreateStoredProcCommand(Configuration.SPUpdatePteRequestNA);
                    cmd.Parameters.AddInputParams(Configuration.SPUpdatePteRequestParam1, SqlDbType.Structured, pteRequest);

                    DataSet orderDS = base.GetDataSet(cmd);
                }
                status = true;
            }
            else
                status = true;

            return status;
        }

        #endregion PTERequest

        #region LotDetails

        public void SaveSalesLotDetails(string sopNumber, int sopType, int lineItemSequence, string itemNumber, string inPutXml, int companyId, string userName)
        {
            var cmd = CreateStoredProcCommand(Configuration.SpcpUpdateLotsInfo);
            cmd.Parameters.AddInputParams(Configuration.SPSalesLotsInfoSopNumber, SqlDbType.VarChar, sopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.SPSalesLotsInfoSopType, SqlDbType.Int, sopType);
            cmd.Parameters.AddInputParams(Configuration.SPSalesLotsInfoLineItemSequence, SqlDbType.Int, lineItemSequence);
            cmd.Parameters.AddInputParams(Configuration.SPSalesLotsInfoItemNumber, SqlDbType.VarChar, itemNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.SPSalesLotsInfoInputXml, SqlDbType.VarChar, inPutXml, 2000);
            cmd.Parameters.AddInputParams(Configuration.SPSalesLotsInfoCompanyId, SqlDbType.Int, companyId);
            cmd.Parameters.AddInputParams(Configuration.SPSalesLotsInfoUserName, SqlDbType.VarChar, userName, 20);
            base.Insert(cmd);
        }

        public SalesLineItem GetLotDetails(string sopNumber, int sopType, string itemNumber)
        {
            SalesLineItem salesLineItem = new SalesLineItem();
            List<LotInformation> lotInformationlIst = new List<LotInformation>();
            decimal lotQty;
            bool coaSent;
            var cmd = CreateStoredProcCommand(Configuration.SpcpGetSalesItemLotInfo);
            cmd.Parameters.AddInputParams(Configuration.SPSalesLotsInfoSopNumber, SqlDbType.VarChar, sopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.SPSalesLotsInfoSopType, SqlDbType.Int, sopType);
            cmd.Parameters.AddInputParams(Configuration.SPSalesLotsInfoItemNumber, SqlDbType.VarChar, itemNumber, 21);
            var ds = base.GetDataSet(cmd);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                LotInformation lotInformation = new LotInformation();
                lotInformation.LotNumber = row["LotNumber"].ToString();
                decimal.TryParse(row["LotQuantity"].ToString(), out lotQty);
                lotInformation.LotQuantity = lotQty;
                bool.TryParse(row["IsCoaSent"].ToString(), out coaSent);
                lotInformation.IsCoaSent = coaSent;
                lotInformationlIst.Add(lotInformation);
            }
            salesLineItem.LotsList = lotInformationlIst;
            return salesLineItem;
        }

        #endregion LotDetails

        #region OrderToFOTransfer

        /// <summary>
        /// Get Order details to auto transfer window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public BulkOrderTransferEntity GetOrdersToAutoFOTransfer()
        {
            BulkOrderTransferEntity transferEntity = new BulkOrderTransferEntity();
            var cmd = CreateStoredProcCommand(Configuration.SPGetOrderToAutoTransferNA);
            transferEntity.SalesOrderDetails = base.GetDataSet(cmd);
            return transferEntity;
        }

        public string GetNextAvailableFONumber(int orderType, string documentId, int companyId)
        {
            string spName = string.Empty;
            if (companyId == 1)
                spName = Configuration.SPGetNextAvailableFONumberNA;
            else
                spName = Configuration.SPGetNextAvailableFONumberEU;

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.SPGetNextAvailableFONumberParam1, SqlDbType.TinyInt, orderType);
            cmd.Parameters.AddInputParams(Configuration.SPGetNextAvailableFONumberParam2, SqlDbType.VarChar, documentId, 15);
            cmd.Parameters.AddOutputParams(Configuration.SPGetNextAvailableFONumberParam3, SqlDbType.VarChar, 21);

            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            return commandResult.Parameters["@" + Configuration.SPGetNextAvailableFONumberParam3].Value.ToString().Trim();
        }

        #endregion OrderToFOTransfer

        /// <summary>
        /// Save Header Instruction Details ...
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public object SaveLineCommentInstruction(SalesOrderRequest salesOrderRequest)
        {
            DataTable salesOrderAddressCodeDT = DataTableMapper.GetLineCommentInstruction(salesOrderRequest,
                DataColumnMappings.SaveLineComment);
            var cmd = CreateStoredProcCommand(salesOrderRequest.CompanyID == 1 ? Configuration.SPSaveSopLineInstructionDetailsNA : Configuration.SPSaveSopLineInstructionDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPSaveSopLineInstructionDetailsParam1, SqlDbType.Structured, salesOrderAddressCodeDT);
            return base.Update(cmd);
        }

        /// <summary>
        /// Get Quote Number from Quote Lookup
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse GetQuoteNumber(SalesOrderRequest salesOrderRequest)
        {
            List<SalesLineItem> salesLineItemList = new List<SalesLineItem>();
            SalesOrderDetails salesOrderDetails = new SalesOrderDetails();
            SalesOrderInformation salesOrderInformation = new SalesOrderInformation();
            SalesOrderEntity salesOrderEntity = new SalesOrderEntity();
            SalesOrderResponse salesOrderResponse = new SalesOrderResponse();
            SalesLineItem salesLineItem = new SalesLineItem();

            salesLineItem = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.LineItems[0];
            var cmd = CreateStoredProcCommand(salesOrderRequest.CompanyID == 1 ? Configuration.SPFetchSopLineQuoteDetailsNA : Configuration.SPFetchSopLineQuoteDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPFetchSopLineQuoteDetailsParam1, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerId);
            cmd.Parameters.AddInputParams(Configuration.SPFetchSopLineQuoteDetailsParam2, SqlDbType.VarChar, salesLineItem.ItemNumber);
            cmd.Parameters.AddInputParams(Configuration.SPFetchSopLineQuoteDetailsParam3, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Warehouse.WarehouseId);
            DataSet quoteNumberResult = base.GetDataSet(cmd);

            salesLineItemList = base.GetAllEntities<SalesLineItem, QuoteMap>(quoteNumberResult.Tables[0]).ToList();

            salesOrderDetails.LineItems = salesLineItemList;
            salesOrderInformation.SalesOrderDetails = salesOrderDetails;
            salesOrderEntity.SalesOrderDetails = salesOrderInformation;
            salesOrderResponse.SalesOrderDetails = salesOrderEntity;

            return salesOrderResponse;
        }

        /// <summary>
        /// Get Quote Number from Quote Lookup
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse ValidateQuoteNumber(SalesOrderRequest salesOrderRequest)
        {
            List<SalesLineItem> salesLineItemList = new List<SalesLineItem>();
            SalesOrderDetails salesOrderDetails = new SalesOrderDetails();
            SalesOrderInformation salesOrderInformation = new SalesOrderInformation();
            SalesOrderEntity salesOrderEntity = new SalesOrderEntity();
            SalesOrderResponse salesOrderResponse = new SalesOrderResponse();
            SalesLineItem salesLineItem = new SalesLineItem();

            salesLineItem = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.LineItems[0];
            var cmd = CreateStoredProcCommand(salesOrderRequest.CompanyID == 1 ? Configuration.SPValidateSopLineQuoteDetailsNA : Configuration.SPValidateSopLineQuoteDetailsEU);
            cmd.Parameters.AddInputParams(Configuration.SPValidateSopLineQuoteDetailsParam1, SqlDbType.Int, salesOrderRequest.SalesOrderEntity.SopType);
            cmd.Parameters.AddInputParams(Configuration.SPValidateSopLineQuoteDetailsParam2, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerId);
            cmd.Parameters.AddInputParams(Configuration.SPValidateSopLineQuoteDetailsParam3, SqlDbType.VarChar, salesLineItem.QuoteNumber, 50);
            cmd.Parameters.AddInputParams(Configuration.SPValidateSopLineQuoteDetailsParam4, SqlDbType.VarChar, salesLineItem.ItemNumber, 31);
            DataSet quoteNumberResult = base.GetDataSet(cmd);

            salesLineItemList = base.GetAllEntities<SalesLineItem, QuoteMap>(quoteNumberResult.Tables[0]).ToList();

            salesOrderDetails.LineItems = salesLineItemList;
            salesOrderInformation.SalesOrderDetails = salesOrderDetails;
            salesOrderEntity.SalesOrderDetails = salesOrderInformation;
            salesOrderResponse.SalesOrderDetails = salesOrderEntity;

            return salesOrderResponse;
        }

        public CustomerInformation GetVatNumberDetails(string customerNumber)
        {
            CustomerInformation cust = new CustomerInformation();
            cust.VatNumber = new List<string>();
            var cmd = CreateStoredProcCommand(Configuration.SPFetchVatReferenceNumber);
            cmd.Parameters.AddInputParams(Configuration.SPFetchVatReferenceNumberCustomerId, SqlDbType.VarChar, customerNumber, 21);
            var ds = base.GetDataSet(cmd);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                cust.VatNumber.Add(row["VATNumber"].ToString());
            }
            return cust;
        }

        public object ExecuteCommitmentEngine(SalesOrderRequest salesOrderRequest)
        {
            var cmd = CreateStoredProcCommand(salesOrderRequest.CompanyID == 1 ? Configuration.NaspCommitSalesOrderReview : Configuration.EuspCommitSalesOrderReview);
            cmd.Parameters.AddInputParams(Configuration.SPCommitSalesOrderReviewParam1, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.SPCommitSalesOrderReviewParam2, SqlDbType.Int, salesOrderRequest.SalesOrderEntity.SopType);
            cmd.Parameters.AddInputParams(Configuration.SPCommitSalesOrderReviewParam3, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerId, 15);
            cmd.Parameters.AddInputParams(Configuration.SPCommitSalesOrderReviewParam4, SqlDbType.VarChar, salesOrderRequest.UserID, 15);
            cmd.Parameters.AddInputParams(Configuration.SPCommitSalesOrderReviewParam5, SqlDbType.VarChar, salesOrderRequest.Source, 25);
            return base.Insert(cmd);
        }

        public object UpdateOrderDetailsToWarehouseIBoard(SalesOrderRequest salesOrderRequest)
        {
            var cmd = CreateStoredProcCommand(salesOrderRequest.CompanyID == 1 ? Configuration.NaspOrderStatusToWhiBoard : Configuration.EuspOrderStatusToWhiBoard);
            cmd.Parameters.AddInputParams(Configuration.SopNumberParam, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.SopTypeParam, SqlDbType.Int, salesOrderRequest.SalesOrderEntity.SopType);
            return base.Insert(cmd);
        }

        public object UpdateOrderDetailsToInvoiceType(SalesOrderRequest salesOrderRequest)
        {
            var cmd = CreateStoredProcCommand(salesOrderRequest.CompanyID == 1 ? Configuration.NaspUpdateSofoToInvoice : Configuration.EuspUpdateSofoToInvoice);
            cmd.Parameters.AddInputParams(Configuration.SopNumberParam, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.SourceParam, SqlDbType.VarChar, salesOrderRequest.Source);
            return base.Insert(cmd);
        }

        /// <summary>
        /// Update Sop Status
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        public object UpdateAutoSendPTLog(int invoiceType, string invoiceNumber, int status, string message)
        {
            var cmd = CreateStoredProcCommand(Configuration.SPCpUpdateAutoSendPtLog);
            cmd.Parameters.AddInputParams(Configuration.SPCpUpdateAutoSendPtLogParam1, SqlDbType.TinyInt, invoiceType);
            cmd.Parameters.AddInputParams(Configuration.SPCpUpdateAutoSendPtLogParam2, SqlDbType.VarChar, invoiceNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.SPCpUpdateAutoSendPtLogParam3, SqlDbType.TinyInt, status);
            cmd.Parameters.AddInputParams(Configuration.SPCpUpdateAutoSendPtLogParam4, SqlDbType.VarChar, message, 255);

            return base.Update(cmd);
        }

        /// <summary>
        /// Sales Order Void...
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <returns></returns>
        public object SalesOrderVoid(SalesOrderRequest salesOrderRequest)
        {
            var cmd = CreateStoredProcCommand(salesOrderRequest.CompanyID == 1 ? Configuration.SPMoveVoidRecordToHistoryNA : Configuration.SPMoveVoidRecordToHistoryEU);
            cmd.Parameters.AddInputParams(Configuration.SPMoveVoidRecordToHistoryParam1, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SopNumber);
            return base.Delete(cmd);
        }

        /// <summary>
        /// Fetch Service SKU
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse GetServiceSKULineItem(SalesOrderRequest salesOrderRequest)
        {
            List<SalesLineItem> serviceSKULineItemList = new List<SalesLineItem>();
            SalesOrderDetails salesOrderDetails = new SalesOrderDetails();
            SalesOrderInformation salesOrderInformation = new SalesOrderInformation();
            SalesOrderEntity salesOrderEntity = new SalesOrderEntity();
            SalesOrderResponse salesOrderResponse = new SalesOrderResponse();

            DataTable salesOrderTypeDT = DataTableMapper.GetSalesOrderTypeDataTable(salesOrderRequest,
                 DataColumnMappings.SaveSalesOrderType);

            DataTable serviceSKUValidationDT = DataTableMapper.GetServiceSKUValidationDataTable(salesOrderRequest,
                 DataColumnMappings.ValidateServiceSKU);

            var cmd = CreateStoredProcCommand(salesOrderRequest.CompanyID == 1 ? Configuration.SPGetServiceSKULineItemNA : Configuration.SPGetServiceSKULineItemEU);
            cmd.Parameters.AddInputParams(Configuration.SPGetServiceSKULineItemParam1, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SopNumber);
            cmd.Parameters.AddInputParams(Configuration.SPGetServiceSKULineItemParam2, SqlDbType.SmallInt, 2);
            cmd.Parameters.AddInputParams(Configuration.SPGetServiceSKULineItemParam3, SqlDbType.Structured, salesOrderTypeDT);
            cmd.Parameters.AddInputParams(Configuration.SPGetServiceSKULineItemParam4, SqlDbType.Structured, serviceSKUValidationDT);
            var ds = base.GetDataSet(cmd);
            serviceSKULineItemList = base.GetAllEntities<SalesLineItem, SalesOrderServiceSKUMap>(ds.Tables[0]).ToList();  // Customre detail Bill Id

            salesOrderDetails.LineItems = serviceSKULineItemList;
            salesOrderInformation.SalesOrderDetails = salesOrderDetails;
            salesOrderEntity.SalesOrderDetails = salesOrderInformation;
            salesOrderResponse.SalesOrderDetails = salesOrderEntity;
            return salesOrderResponse;
        }

        #region Cash Application Process

        /// <summary>
        /// Distribute Amount To CashApplyInvoices (sopEntry)
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        public object DistributeAmountToCashApplyInvoices(SalesOrderRequest salesApplyRequest, int companyId)
        {
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPDistributeAmountToCashApplyInvoicesNA : Configuration.SPDistributeAmountToCashApplyInvoicesEU);
            cmd.Parameters.AddInputParams(Configuration.SPDistributeAmountToCashApplyInvoicesParam1, SqlDbType.Int, salesApplyRequest.SalesOrderEntity.SopType);
            cmd.Parameters.AddInputParams(Configuration.SPDistributeAmountToCashApplyInvoicesParam2, SqlDbType.VarChar, salesApplyRequest.SalesOrderEntity.SopNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.SPDistributeAmountToCashApplyInvoicesParam3, SqlDbType.VarChar, salesApplyRequest.UserID, 15);
            return base.Insert(cmd);
        }

        /// <summary>
        /// Get documents details for apply to open orders window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public BulkOrderTransferEntity GetDocumentForApplyToOpenOrdersForEngine(int companyId)
        {
            string spName = string.Empty;
            if (companyId == 1)
                spName = Configuration.SPGetDocumentsForCashEngineNA;
            else
                spName = Configuration.SPGetDocumentsForCashEngineEU;

            BulkOrderTransferEntity cashEntity = new BulkOrderTransferEntity();
            var cmd = CreateStoredProcCommand(spName);
            cashEntity.SalesOrderDetails = base.GetDataSet(cmd);
            return cashEntity;
        }

        /// <summary>
        /// Get documents details for apply to open orders window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public object SaveDocumentForApplyToOpenOrdersForEngine(BulkOrderTransferEntity cashEntity, int companyId)
        {
            string spName = string.Empty;
            if (companyId == 1)
                spName = Configuration.SPSaveDocumentsForCashEngineNA;
            else
                spName = Configuration.SPSaveDocumentsForCashEngineEU;

            DataTable documentCashDT = cashEntity.SalesOrderDetails.Tables[0].DefaultView.ToTable(false, "ApplyOpenOrdersId", "ErrorId");

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.SPSaveDocumentsForCashEngineParam1, SqlDbType.Structured, documentCashDT);
            return base.Update(cmd);
        }

        /// <summary>
        /// fetch Cash application window value...
        /// </summary>
        /// <param name="aRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse FetchApplyToOpenOrder(ReceivablesRequest aRequest)
        {
            List<ReceivableDetails> cashApplicationList = new List<ReceivableDetails>();
            ReceivablesResponse caseApplicationResponse = new ReceivablesResponse();
            string spName = string.Empty;

            if (aRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAFetchCashApplyInvoices;
            else
                spName = Configuration.SPEUFetchCashApplyInvoices;

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.FetchCashApplyInvoicesParam1, SqlDbType.VarChar, aRequest.CustomerInformation.CustomerId, 15);
            cmd.Parameters.AddInputParams(Configuration.FetchCashApplyInvoicesParam2, SqlDbType.VarChar, aRequest.ReceivablesHeader.DocumentNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.FetchCashApplyInvoicesParam3, SqlDbType.SmallInt, aRequest.ReceivablesHeader.DocumentType);
            cmd.Parameters.AddInputParams(Configuration.FetchCashApplyInvoicesParam4, SqlDbType.VarChar, aRequest.ReceivablesHeader.Amount.Currency, 15);
            cmd.Parameters.AddInputParams(Configuration.FetchCashApplyInvoicesParam5, SqlDbType.VarChar, aRequest.Source, 31);

            var ds = base.GetDataSet(cmd);

            cashApplicationList = base.GetAllEntities<ReceivableDetails, CashApplicationMap>(ds.Tables[0]).ToList();

            if (cashApplicationList != null)
                caseApplicationResponse.ReceivableEntity = cashApplicationList;

            return caseApplicationResponse;
        }

        /// <summary>
        /// Save cash application 
        /// </summary>
        /// <param name="quoteId"></param>
        /// <param name="statusId"></param>
        /// <param name="statusReason"></param>
        /// <param name="companyId"></param>

        public bool SaveApplyToOpenOrder(ReceivablesRequest aRequest)
        {
            ReceivablesResponse receivableResponse = new ReceivablesResponse();
            string spName = string.Empty;

            DataTable saveCashApplicationDT = DataTableMapper.SaveApplyToOpenOrder(aRequest,
               DataColumnMappings.SaveApplyToOpenOrder);

            if (aRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNASaveApplyToOpenOrder;
            else
                spName = Configuration.SPEUSaveApplyToOpenOrder;

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.SaveApplyToOpenOrderParam1, SqlDbType.Structured, saveCashApplicationDT);
            cmd.Parameters.AddInputParams(Configuration.SaveApplyToOpenOrderParam2, SqlDbType.VarChar, aRequest.AuditInformation.ModifiedBy, 25);
            cmd.Parameters.AddOutputParams(Configuration.SaveApplyToOpenOrderParam3, SqlDbType.Bit);

            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            return Convert.ToBoolean(commandResult.Parameters["@" + Configuration.SaveApplyToOpenOrderParam3].Value);
        }

        /// <summary>
        /// Update Cash application window value from apply to open order(GP) ...
        /// </summary>
        /// <param name="aRequest"></param>
        /// <returns></returns>
        public bool UpdateApplyToOpenOrder(ReceivablesRequest aRequest)
        {
            string spName = string.Empty;

            if (aRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAUpdateCashApplyInvoices;
            else
                spName = Configuration.SPEUUpdateCashApplyInvoices;

            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.UpdateCashApplyInvoicesParam1, SqlDbType.VarChar, aRequest.ReceivablesHeader.DocumentNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.UpdateCashApplyInvoicesParam2, SqlDbType.SmallInt, aRequest.ReceivablesHeader.DocumentType);
            cmd.Parameters.AddInputParams(Configuration.UpdateCashApplyInvoicesParam3, SqlDbType.VarChar, aRequest.CustomerInformation.CustomerId, 15);
            cmd.Parameters.AddInputParams(Configuration.UpdateCashApplyInvoicesParam4, SqlDbType.VarChar, aRequest.AuditInformation.ModifiedBy, 25);

            base.Update(cmd);
            return true;
            //mmm
        }

        /// <summary>
        /// Delete cash application Payments
        /// </summary>
        /// <param name="quoteId"></param>
        /// <param name="statusId"></param>
        /// <param name="statusReason"></param>
        /// <param name="companyId"></param>

        public bool DeleteApplyToOpenOrder(ReceivablesRequest aRequest)
        {
            string spName = string.Empty;

            if (aRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNADeleteCashApplyInvoices;
            else
                spName = Configuration.SPEUDeleteCashApplyInvoices;

            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.DeleteCashApplyInvoicesParam1, SqlDbType.VarChar, aRequest.ReceivablesHeader.DocumentNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.DeleteCashApplyInvoicesParam2, SqlDbType.SmallInt, aRequest.ReceivablesHeader.DocumentType);

            base.Update(cmd);
            return true;
        }




        /// <summary>
        /// save receivables detail Entry details...
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="CompanyId"></param>Th
        public object SaveReceivablesDetail(ReceivablesRequest receivablesRequest)
        {
            DataTable receivablesItemDetailDT = DataTableMapper.SaveReceivablesDetailDataTable(receivablesRequest,
                DataColumnMappings.ReceivablesItemDetail);

            var cmd = CreateStoredProcCommand(receivablesRequest.AuditInformation.CompanyId == 1 ? Configuration.SPSaveAppliedInvoiceDetailsNA : Configuration.SPSaveAppliedInvoiceDetailsEU);

            cmd.Parameters.AddInputParams(Configuration.SPSaveAppliedInvoiceDetailsParam1, SqlDbType.Int, receivablesRequest.ReceivablesHeader.DocumentType);
            cmd.Parameters.AddInputParams(Configuration.SPSaveAppliedInvoiceDetailsParam2, SqlDbType.VarChar, receivablesRequest.ReceivablesHeader.DocumentNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.SPSaveAppliedInvoiceDetailsParam3, SqlDbType.VarChar, receivablesRequest.ReceivablesHeader.CustomerInformation.CustomerId, 15);
            cmd.Parameters.AddInputParams(Configuration.SPSaveAppliedInvoiceDetailsParam4, SqlDbType.Int, receivablesRequest.ReceivablesHeader.TypeId);
            cmd.Parameters.AddInputParams(Configuration.SPSaveAppliedInvoiceDetailsParam5, SqlDbType.VarChar, receivablesRequest.ReceivablesHeader.CommentText, 250);
            cmd.Parameters.AddInputParams(Configuration.SPSaveAppliedInvoiceDetailsParam6, SqlDbType.VarChar, receivablesRequest.AuditInformation.ModifiedBy, 15);
            cmd.Parameters.AddInputParams(Configuration.SPSaveAppliedInvoiceDetailsParam7, SqlDbType.Structured, receivablesItemDetailDT);

            return base.Update(cmd);
        }

        /// <summary>
        /// Get Receivables Detail Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse GetReceivablesDetail(ReceivablesRequest receivablesRequest)
        {
            List<ReceivableDetails> cashApplicationList = new List<ReceivableDetails>();
            ReceivablesResponse caseApplicationResponse = new ReceivablesResponse();
            string spName = string.Empty;

            if (receivablesRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPGetInvoiceLineDetailsNA;
            else
                spName = Configuration.SPGetInvoiceLineDetailsEU;

            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.SPGetInvoiceLineDetailsParam1, SqlDbType.Int, receivablesRequest.ReceivablesHeader.DocumentType);
            cmd.Parameters.AddInputParams(Configuration.SPGetInvoiceLineDetailsParam2, SqlDbType.VarChar, receivablesRequest.ReceivablesHeader.DocumentNumber, 21);
            cmd.Parameters.AddInputParams(Configuration.SPGetInvoiceLineDetailsParam3, SqlDbType.VarChar, receivablesRequest.ReceivablesHeader.CustomerInformation.CustomerId, 15);
            cmd.Parameters.AddInputParams(Configuration.SPGetInvoiceLineDetailsParam4, SqlDbType.VarChar, receivablesRequest.Source, 31);

            var ds = base.GetDataSet(cmd);

            cashApplicationList = base.GetAllEntities<ReceivableDetails, ReceivablesItemDetailMap>(ds.Tables[0]).ToList();

            if (cashApplicationList != null)
                caseApplicationResponse.ReceivableEntity = cashApplicationList;

            return caseApplicationResponse;
        }

        /// <summary>
        /// Get Distribute Amount Detail Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse GetDistributeAmountDetail(ReceivablesRequest receivablesRequest)
        {
            List<ReceivableDetails> cashApplicationList = new List<ReceivableDetails>();
            ReceivablesResponse caseApplicationResponse = new ReceivablesResponse();
            string spName = string.Empty;

            DataTable receivablesItemDetailDT = DataTableMapper.SaveReceivablesDetailDataTable(receivablesRequest,
               DataColumnMappings.ReceivablesItemDetail);

            if (receivablesRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPDistributeAmountToSelectedLinesNA;
            else
                spName = Configuration.SPDistributeAmountToSelectedLinesEU;

            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.SPDistributeAmountToSelectedLinesParam1, SqlDbType.Structured, receivablesItemDetailDT);
            cmd.Parameters.AddInputParams(Configuration.SPDistributeAmountToSelectedLinesParam2, SqlDbType.Decimal, receivablesRequest.ReceivablesHeader.Amount.TotalAmount);
            cmd.Parameters.AddInputParams(Configuration.SPDistributeAmountToSelectedLinesParam3, SqlDbType.Int, receivablesRequest.ReceivablesHeader.TypeId);

            var ds = base.GetDataSet(cmd);

            cashApplicationList = base.GetAllEntities<ReceivableDetails, ReceivablesItemDetailMap>(ds.Tables[0]).ToList();

            if (cashApplicationList != null)
                caseApplicationResponse.ReceivableEntity = cashApplicationList;

            return caseApplicationResponse;
        }

        #endregion Cash Application Process


        #region WareshouseClosure

        public SalesOrderResponse ValidateWarehouseClosureDate(SalesOrderRequest salesOrderRequest)
        {
            SalesOrderResponse salesResponse = new SalesOrderResponse();
            var cmd = CreateStoredProcCommand(salesOrderRequest.CompanyID == 1 ? Configuration.SPValidateWarehouseClosureDateNA : Configuration.SPValidateWarehouseClosureDateEU);
            cmd.Parameters.AddInputParams(Configuration.SPValidateWarehouseClosureDateWarehouseId, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Warehouse.WarehouseId.ToString().Trim());
            cmd.Parameters.AddInputParams(Configuration.SPValidateWarehouseClosureDateCurSchedShip, SqlDbType.DateTime, salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderSchedule.RequestedShipDate);
            cmd.Parameters.AddInputParams(Configuration.SPValidateWarehouseClosureDateCurSchedDel, SqlDbType.DateTime, salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderSchedule.RequestedDeliveryDate);
            cmd.Parameters.AddOutputParams(Configuration.SPValidateWarehouseClosureDateValidateStatus, SqlDbType.Int);

            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            salesResponse.ValidateStatus = Convert.ToInt16(commandResult.Parameters["@" + Configuration.SPValidateWarehouseClosureDateValidateStatus].Value);
            return salesResponse;
        }


        #endregion

        #region EFTAutomation

        #region EFT

        /// <summary>
        /// Get EFT Customer Id Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse FetchCustomerId(ReceivablesRequest eftRequest)
        {
            List<string> eftCustomerIdList = new List<string>();
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            EFTPayment eftPayment = new EFTPayment();
            string spName = string.Empty;

            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAGetEFTCustomerDetailsForLookup;
            else
                spName = Configuration.SPEuGetEFTCustomerDetailsForLookup;

            var cmd = CreateStoredProcCommand(spName);

            var ds = base.GetDataSet(cmd);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                eftCustomerIdList.Add(row["CustomerId"].ToString().Trim()); //EFT Customer source
            }
            eftPayment.EFTCustomerId = eftCustomerIdList;
            eftResponse.EFTPayment = eftPayment;

            return eftResponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eftRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse FetchBatchId(ReceivablesRequest eftRequest)
        {
            List<string> eftBatchIdList = new List<string>();
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            EFTPayment eftPayment = new EFTPayment();
            string spName = string.Empty;

            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAGetEFTBatchIdForLookup;
            else
                spName = Configuration.SPEuGetEFTBatchIdForLookup;

            var cmd = CreateStoredProcCommand(spName);

            var ds = base.GetDataSet(cmd);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                eftBatchIdList.Add(row["BatchNumber"].ToString().Trim());
            }
            eftPayment.EFTBatchId = eftBatchIdList;
            eftResponse.EFTPayment = eftPayment;

            return eftResponse;
        }


        /// <summary>
        /// Get EF Documetn number
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse FetchDocumentNumber(ReceivablesRequest eftRequest)
        {
            List<string> eftDocIdList = new List<string>();
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            EFTPayment eftPayment = new EFTPayment();
            string spName = string.Empty;

            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAGetEFTDocumentNumberForLookup;
            else
                spName = Configuration.SPEUGetEFTDocumentNumberForLookup;

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.GetEFTDocumentNumberForLookupParam1, SqlDbType.Int, eftRequest.Actiontype);

            var ds = base.GetDataSet(cmd);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                eftDocIdList.Add(row["ItemReference"].ToString().Trim());
            }
            eftPayment.EFTItemReference = eftDocIdList;
            eftResponse.EFTPayment = eftPayment;

            return eftResponse;
        }

        /// <summary>
        /// Get EFT Customer Id Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse FetchReferenceId(ReceivablesRequest eftRequest)
        {
            List<string> eftRefIdList = new List<string>();
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            EFTPayment eftPayment = new EFTPayment();
            string spName = string.Empty;

            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAGetEFTReferenceNumberForLookup;
            else
                spName = Configuration.SPEUGetEFTReferenceNumberForLookup;

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.EFTReferenceNumberForLookupParam1, SqlDbType.Int, eftRequest.Searchtype);

            var ds = base.GetDataSet(cmd);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                eftRefIdList.Add(row["ReferenceNumber"].ToString().Trim()); //EFT Customer source
            }
            eftPayment.EFTReferenceNumber = eftRefIdList;
            eftResponse.EFTPayment = eftPayment;

            return eftResponse;
        }
        /// <summary>
        /// Get EFT Customer Id Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse FetchCustomerIdForReference(ReceivablesRequest eftRequest)
        {

            ReceivablesResponse eftResponse = new ReceivablesResponse();
            EFTPayment eftPayment = new EFTPayment();
            CustomerInformation customerInformation = new CustomerInformation();
            string spName = string.Empty;

            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAFetchCustomerIdForReference;
            else
                spName = Configuration.SPEUFetchCustomerIdForReference;

            var cmd = CreateStoredProcCommand(spName);
            cmd.Parameters.AddInputParams(Configuration.SPNAFetchCustomerIdForReferenceParam1, SqlDbType.VarChar, eftRequest.EFTPayment.ReferenceNumber);
            var ds = base.GetDataSet(cmd);
            customerInformation.CustomerId = ds.Tables[0].Rows.Count != 0 ? ds.Tables[0].Rows[0]["CustomerId"].ToString().Trim() : string.Empty;
            eftResponse.CustomerInformation = customerInformation;

            return eftResponse;
        }


        /// <summary>
        /// Validate EFT Cutomer
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public bool ValidateEftCustomer(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            string spName = string.Empty;
            //Convert object to DataTable.

            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAValidateEFTCustomer;
            else
                spName = Configuration.SPEUValidateEFTCustomer;
            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.ValidateEFTCustomerParam1, SqlDbType.VarChar, eftRequest.CustomerInformation.CustomerId);
            cmd.Parameters.AddOutputParams(Configuration.ValidateEFTCustomerParam2, SqlDbType.Bit);
            var ds = base.GetDataSet(cmd);

            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            return Convert.ToBoolean(commandResult.Parameters["@" + Configuration.ValidateEFTCustomerParam2].Value);


        }

        /// <summary>
        /// Validate EFT Reference
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public bool ValidateEftReference(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            string spName = string.Empty;
            //Convert object to DataTable.

            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAValidateEFTReference;
            else
                spName = Configuration.SPEUValidateEFTReference;
            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.ValidateEFTReferenceParam1, SqlDbType.VarChar, eftRequest.EFTPayment.ReferenceNumber);
            cmd.Parameters.AddInputParams(Configuration.ValidateEFTReferenceParam2, SqlDbType.Int, eftRequest.Actiontype);
            cmd.Parameters.AddOutputParams(Configuration.ValidateEFTReferenceParam3, SqlDbType.Bit);
            var ds = base.GetDataSet(cmd);

            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            return Convert.ToBoolean(commandResult.Parameters["@" + Configuration.ValidateEFTReferenceParam3].Value);
        }

        /// <summary>
        /// Validate EFT Reference
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public bool ValidateEftEmailReference(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            string spName = string.Empty;
            //Convert object to DataTable.

            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAValidateEFTEmailReference;
            else
                spName = Configuration.SPNAValidateEFTEmailReference;
            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.ValidateEFTEmailReferenceParam1, SqlDbType.VarChar, eftRequest.EFTPayment.ReferenceNumber);
            cmd.Parameters.AddInputParams(Configuration.ValidateEFTEmailReferenceParam2, SqlDbType.VarChar, eftRequest.EFTPayment.ItemReference);
            cmd.Parameters.AddOutputParams(Configuration.ValidateEFTEmailReferenceParam3, SqlDbType.Bit);
            var ds = base.GetDataSet(cmd);

            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            return Convert.ToBoolean(commandResult.Parameters["@" + Configuration.ValidateEFTEmailReferenceParam3].Value);
        }

        /// <summary>
        /// Get EFT Customer Detail Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public bool ValidateEFTItemReference(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            string spName = string.Empty;
            //Convert object to DataTable.

            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAValidateEFTItemReference;
            else
                spName = Configuration.SPEUValidateEFTItemReference;
            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.ValidateEFTItemReferenceParam1, SqlDbType.VarChar, eftRequest.EFTPayment.ItemReference);
            cmd.Parameters.AddInputParams(Configuration.ValidateEFTItemReferenceParam2, SqlDbType.Int, eftRequest.Actiontype);
            var ds = base.GetDataSet(cmd);

            SqlCommand commandResult = base.InsertReturn(cmd) as SqlCommand;
            return Convert.ToBoolean(commandResult.Parameters["@" + Configuration.ValidateEFTItemReferenceParam2].Value);
        }

        /// <summary>
        /// GetEFTEmailRemittances
        /// </summary>
        /// <param name="eftEmailRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse GetEFTEmailRemittances(ReceivablesRequest eftEmailRequest)
        {
            List<EFTPayment> eftCustomerRemittancesList = new List<EFTPayment>();
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            CustomerInformation eftCustomerInfo = new CustomerInformation();
            string spName = string.Empty;

            if (eftEmailRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAGetEFTEmailRemittanceTransactions;
            else
                spName = Configuration.SPEUGetEFTEmailRemittanceTransactions;

            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.GetEFTEmailRemittanceTransactionsParam1, SqlDbType.VarChar, eftEmailRequest.CustomerIdStart);
            cmd.Parameters.AddInputParams(Configuration.GetEFTEmailRemittanceTransactionsParam2, SqlDbType.VarChar, eftEmailRequest.CustomerIdEnd);
            cmd.Parameters.AddInputParams(Configuration.GetEFTEmailRemittanceTransactionsParam3, SqlDbType.DateTime, eftEmailRequest.DateStart);
            cmd.Parameters.AddInputParams(Configuration.GetEFTEmailRemittanceTransactionsParam4, SqlDbType.DateTime, eftEmailRequest.DateEnd);
            cmd.Parameters.AddInputParams(Configuration.GetEFTEmailRemittanceTransactionsParam5, SqlDbType.VarChar, eftEmailRequest.ReferenceNoStart);
            cmd.Parameters.AddInputParams(Configuration.GetEFTEmailRemittanceTransactionsParam6, SqlDbType.VarChar, eftEmailRequest.ReferenceNoEnd);
            cmd.Parameters.AddInputParams(Configuration.GetEFTEmailRemittanceTransactionsParam7, SqlDbType.VarChar, eftEmailRequest.DocNumbrStart);
            cmd.Parameters.AddInputParams(Configuration.GetEFTEmailRemittanceTransactionsParam8, SqlDbType.VarChar, eftEmailRequest.DocNumbrend);
            cmd.Parameters.AddInputParams(Configuration.GetEFTEmailRemittanceTransactionsParam9, SqlDbType.VarChar, eftEmailRequest.Source);

            var ds = base.GetDataSet(cmd);

            eftCustomerRemittancesList = base.GetAllEntities<EFTPayment, EFTEmailRemittanceMap>(ds.Tables[0]).ToList();
            eftResponse.EFTCustomerRemittancesList = eftCustomerRemittancesList;

            return eftResponse;
        }

        /// <summary>
        /// SaveEFTEmailRemittances
        /// </summary>
        /// <param name="eftEmailRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse SaveEFTEmailRemittances(ReceivablesRequest eftEmailRequest)
        {
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            string spName = string.Empty;
            //Convert object to DataTable.
            DataTable eftCustomerRemittancesDT = DataTableMapper.SaveEFTEmailRemittances(eftEmailRequest,
                DataColumnMappings.SaveEFTEmailRemittance);

            if (eftEmailRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNASaveEFTMailTransactions;
            else
                spName = Configuration.SPEUSaveEFTMailTransactions;
            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.SaveEFTMailTransactionsParam1, SqlDbType.Structured, eftCustomerRemittancesDT);
            var ds = base.Insert(cmd);
            eftResponse.Status = ResponseStatus.Success;
            return eftResponse;
        }

        public ReceivablesResponse ValidateEFTCustomerRemittanceSummary(DataTable dt, int CompanyId)
        {
            List<EFTCustomerPayment> eftCustomerRemittancesList = new List<EFTCustomerPayment>();
            ReceivablesResponse eftResponse = new ReceivablesResponse();

            var cmd = CreateStoredProcCommand(CompanyId == 1 ? Configuration.SPValidateEFTCustomerRemittanceSummaryNA : Configuration.SPValidateEFTCustomerRemittanceSummaryEU);
            cmd.Parameters.AddInputParams(Configuration.SPValidateEFTCustomerRemittanceSummaryParam1, SqlDbType.Structured, dt);
            var ds = base.GetDataSet(cmd);
            eftCustomerRemittancesList = base.GetAllEntities<EFTCustomerPayment, EFTCustomerSummaryRemittanceMap>(ds.Tables[0]).ToList();
            eftResponse.EFTCustomerPaymentList = eftCustomerRemittancesList;

            return eftResponse;
        }


        /// <summary>
        /// Get EFT Customer Detail Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse GetEFTPaymentRemittanceAmountDetails(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            EFTPayment eFTPayment = new EFTPayment();

            string spName = string.Empty;

            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAFetchEftPaymentAmountDetails;
            else
                spName = Configuration.SPEUFetchEftPaymentAmountDetails;

            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.GetEFTFetchEftPaymentAmountDetailsParam1, SqlDbType.VarChar, eftRequest.BatchId);

            var ds = base.GetDataSet(cmd);
            if (ds.Tables[0].Rows.Count > 0)
            {
                eFTPayment.ControlAmount = Convert.ToDecimal(ds.Tables[0].Rows[0]["ControlAmount"].ToString());
                eFTPayment.PaymentAmount = Convert.ToDecimal(ds.Tables[0].Rows[0]["PaymentAmount"].ToString());
                eFTPayment.PaymentCount = Convert.ToInt32(ds.Tables[0].Rows[0]["PaymentCount"].ToString());
                eFTPayment.RemainingAmount = Convert.ToDecimal(ds.Tables[0].Rows[0]["RemainingAmount"].ToString());
                eFTPayment.RemainingCount = Convert.ToInt32(ds.Tables[0].Rows[0]["RemainingCount"].ToString());
            }
            else
            {
                eFTPayment.ControlAmount = 0.00M;
                eFTPayment.PaymentAmount = 0.00M;
                eFTPayment.PaymentCount = 0;
                eFTPayment.RemainingAmount = 0.00M;
                eFTPayment.RemainingCount = 0;
            }
            eftResponse.EFTPayment = eFTPayment;
            return eftResponse;
        }



        /// <summary>
        /// Get EFT Customer Detail Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse GetEFTCustomerDetails(ReceivablesRequest eftRequest)
        {

            List<CustomerMappingDetails> eftCustomerSourceList = new List<CustomerMappingDetails>();
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            CustomerInformation eftCustomerInfo = new CustomerInformation();
            string spName = string.Empty;

            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAGetEFTCustomerMappingDetails;
            else
                spName = Configuration.SPEUGetEFTCustomerMappingDetails;

            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.GetEFTCustomerDetailsParam1, SqlDbType.VarChar, eftRequest.CustomerInformation.CustomerId);

            var ds = base.GetDataSet(cmd);
            eftCustomerInfo.ParentCustomerId = ds.Tables[0].Rows[0]["ParentCustomerId"].ToString().Trim(); // parent Customer ID
            eftCustomerInfo.XrmParentCustomerId = ds.Tables[0].Rows[0]["XrmParentAccountNumber"].ToString().Trim(); // parent Customer ID

            foreach (DataRow row in ds.Tables[1].Rows)
            {
                CustomerMappingDetails customerMappingDetails = new CustomerMappingDetails();
                customerMappingDetails.EftCustomerMappingId = int.Parse(row["NAEFTCustomerMappingId"].ToString().Trim());
                customerMappingDetails.EftCTXCustomerReference = row["EftCustomerSource"].ToString().Trim();
                customerMappingDetails.CustomerId = row["CustomerId"].ToString().Trim();
                eftCustomerSourceList.Add(customerMappingDetails);
            }
            eftCustomerInfo.CustomerMappingDetails = eftCustomerSourceList;
            eftResponse.CustomerInformation = eftCustomerInfo;

            return eftResponse;
        }

        /// <summary>
        /// Get EFT Customer Detail Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse GetEFTCustomerRemittances(ReceivablesRequest eftRequest)
        {
            List<EFTPayment> eftCustomerRemittancesList = new List<EFTPayment>();
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            CustomerInformation eftCustomerInfo = new CustomerInformation();
            string spName = string.Empty;

            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAGetEFTCustomerRemittanceTransactions;
            else
                spName = Configuration.SPEUGetEFTCustomerRemittanceTransactions;

            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.GetEFTCustomerRemittanceTransactionsBatchId, SqlDbType.VarChar, eftRequest.BatchId);
            cmd.Parameters.AddInputParams(Configuration.GetEFTCustomerRemittanceTransactionsParam1, SqlDbType.VarChar, eftRequest.CustomerIdStart);
            cmd.Parameters.AddInputParams(Configuration.GetEFTCustomerRemittanceTransactionsParam2, SqlDbType.VarChar, eftRequest.CustomerIdEnd);
            cmd.Parameters.AddInputParams(Configuration.GetEFTCustomerRemittanceTransactionsParam3, SqlDbType.DateTime, eftRequest.DateStart);
            cmd.Parameters.AddInputParams(Configuration.GetEFTCustomerRemittanceTransactionsParam4, SqlDbType.DateTime, eftRequest.DateEnd);
            cmd.Parameters.AddInputParams(Configuration.GetEFTCustomerRemittanceTransactionsParam5, SqlDbType.VarChar, eftRequest.ReferenceNoStart);
            cmd.Parameters.AddInputParams(Configuration.GetEFTCustomerRemittanceTransactionsParam6, SqlDbType.VarChar, eftRequest.ReferenceNoEnd);
            cmd.Parameters.AddInputParams(Configuration.GetEFTCustomerRemittanceTransactionsParam7, SqlDbType.VarChar, eftRequest.DocNumbrStart);
            cmd.Parameters.AddInputParams(Configuration.GetEFTCustomerRemittanceTransactionsParam8, SqlDbType.VarChar, eftRequest.DocNumbrend);

            var ds = base.GetDataSet(cmd);

            if (ds.Tables[0].Rows.Count != 0)
            {
                eftResponse.BatchAmount = Convert.ToDecimal(ds.Tables[0].Rows[0]["ControlAmount"]);
            }
            eftCustomerRemittancesList = base.GetAllEntities<EFTPayment, EFTCustomerRemittanceMap>(ds.Tables[1]).ToList();
            eftResponse.EFTCustomerRemittancesList = eftCustomerRemittancesList;

            return eftResponse;
        }

        /// <summary>
        /// Get EFT Customer Detail Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse GetEFTPaymentRemittances(ReceivablesRequest eftRequest)
        {
            List<EFTPayment> eftCustomerRemittancesList = new List<EFTPayment>();
            EFTPayment eftBatchDetails = new EFTPayment();
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            CustomerInformation eftCustomerInfo = new CustomerInformation();
            string spName = string.Empty;

            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAGetEFTPaymentRemittanceTransactions;
            else
                spName = Configuration.SPEUGetEFTPaymentRemittanceTransactions;

            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.GetEFTPaymentRemittanceTransactionsParam1, SqlDbType.VarChar, eftRequest.CustomerIdStart);
            cmd.Parameters.AddInputParams(Configuration.GetEFTPaymentRemittanceTransactionsParam2, SqlDbType.VarChar, eftRequest.CustomerIdEnd);
            cmd.Parameters.AddInputParams(Configuration.GetEFTPaymentRemittanceTransactionsParam3, SqlDbType.DateTime, eftRequest.DateStart);
            cmd.Parameters.AddInputParams(Configuration.GetEFTPaymentRemittanceTransactionsParam4, SqlDbType.DateTime, eftRequest.DateEnd);
            cmd.Parameters.AddInputParams(Configuration.GetEFTPaymentRemittanceTransactionsParam5, SqlDbType.VarChar, eftRequest.ReferenceNoStart);
            cmd.Parameters.AddInputParams(Configuration.GetEFTPaymentRemittanceTransactionsParam6, SqlDbType.VarChar, eftRequest.ReferenceNoEnd);
            cmd.Parameters.AddInputParams(Configuration.GetEFTPaymentRemittanceTransactionsParam7, SqlDbType.VarChar, eftRequest.DocNumbrStart);
            cmd.Parameters.AddInputParams(Configuration.GetEFTPaymentRemittanceTransactionsParam8, SqlDbType.VarChar, eftRequest.DocNumbrend);
            cmd.Parameters.AddInputParams(Configuration.GetEFTPaymentRemittanceTransactionsParam9, SqlDbType.Int, eftRequest.Actiontype);
            cmd.Parameters.AddInputParams(Configuration.GetEFTPaymentRemittanceTransactionsParam10, SqlDbType.VarChar, eftRequest.BatchId);

            var ds = base.GetDataSet(cmd);

            eftCustomerRemittancesList = base.GetAllEntities<EFTPayment, EFTPaymentRemittanceMap>(ds.Tables[0]).ToList();
            // Batch Details 
            if (ds.Tables[1].Rows.Count > 0)
            {
                DataRow eftBatchDetailRow = ds.Tables[1].Rows[0];
                {
                    eftBatchDetails.ControlAmount = Convert.ToDecimal(eftBatchDetailRow["ControlAmount"]);
                    eftBatchDetails.PaymentAmount = Convert.ToDecimal(eftBatchDetailRow["PaymentAmount"]);
                    eftBatchDetails.PaymentCount = Convert.ToInt32(eftBatchDetailRow["PaymentCount"]);
                    eftBatchDetails.RemainingAmount = Convert.ToDecimal(eftBatchDetailRow["RemainingAmount"]);
                    eftBatchDetails.RemainingCount = Convert.ToInt32(eftBatchDetailRow["RemainingCount"]);
                }

            }
            else
            {
                eftBatchDetails.ControlAmount = 0.00M;
                eftBatchDetails.PaymentAmount = 0.00M;
                eftBatchDetails.PaymentCount = 0;
                eftBatchDetails.RemainingAmount = 0.00M;
                eftBatchDetails.RemainingCount = 0;
            }

            eftResponse.EFTPayment = eftBatchDetails;
            eftResponse.EFTCustomerRemittancesList = eftCustomerRemittancesList;

            return eftResponse;
        }

        #region EFT_Customer_Mapping_Window

        /// <summary>
        /// Get EFT Customer Detail Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse SaveEFTCustomerMappingDetails(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            string spName = string.Empty;
            //Convert object to DataTable.

            DataTable eftDT = DataTableMapper.SaveEFTCustomerDetails(eftRequest,
                DataColumnMappings.SaveEFTCustomerSource);

            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNASaveEftCustomerReferences;
            else
                spName = Configuration.SPEUSaveEftCustomerReferences;
            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.SaveEFTCustomerDetailsParam1, SqlDbType.Structured, eftDT);
            var ds = base.GetDataSet(cmd);
            eftResponse.Status = ResponseStatus.Success;
            return eftResponse;
        }

        /// <summary>
        /// Get EFT Customer Detail Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse GetEFTCustomerMappingDetails(ReceivablesRequest eftRequest)
        {

            List<CustomerMappingDetails> eftCustomerSourceList = new List<CustomerMappingDetails>();
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            CustomerInformation eftCustomerInfo = new CustomerInformation();
            string spName = string.Empty;

            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNAGetEFTCustomerMappingDetails;
            else
                spName = Configuration.SPEUGetEFTCustomerMappingDetails; ;

            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.GetEFTCustomerDetailsParam1, SqlDbType.VarChar, eftRequest.CustomerInformation.CustomerId);

            var ds = base.GetDataSet(cmd);
            eftCustomerInfo.ParentCustomerId = ds.Tables[0].Rows[0]["ParentCustomerId"].ToString().Trim(); // parent Customer ID
            eftCustomerInfo.XrmParentCustomerId = ds.Tables[0].Rows[0]["XrmParentAccountNumber"].ToString().Trim(); // parent Customer ID

            foreach (DataRow row in ds.Tables[1].Rows)
            {
                CustomerMappingDetails customerMappingDetails = new CustomerMappingDetails();
                customerMappingDetails.EftCustomerMappingId = int.Parse(row["NAEFTCustomerMappingId"].ToString().Trim());
                customerMappingDetails.EftCTXCustomerReference = row["EftCustomerSource"].ToString().Trim();
                customerMappingDetails.CustomerId = row["CustomerId"].ToString().Trim();
                eftCustomerSourceList.Add(customerMappingDetails);
            }
            eftCustomerInfo.MaxCustomerReferenceId = int.Parse(ds.Tables[2].Rows[0]["MaxCustomerReferenceId"].ToString().Trim());

            eftCustomerInfo.CustomerMappingDetails = eftCustomerSourceList;
            eftResponse.CustomerInformation = eftCustomerInfo;

            return eftResponse;
        }

        #endregion EFT_Customer_Mapping_Window

        /// <summary>
        /// Get EFT Customer Detail Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public ReceivablesResponse SaveEFTCustomerRemittances(ReceivablesRequest eftRequest)
        {
            ReceivablesResponse eftResponse = new ReceivablesResponse();
            string spName = string.Empty;
            //Convert object to DataTable.
            DataTable eftCustomerRemittancesDT = DataTableMapper.SaveEFTCustomerRemittances(eftRequest,
                DataColumnMappings.SaveEFTCustomerRemittance);

            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNASaveEFTCustomerTransactions;
            else
                spName = Configuration.SPEUSaveEFTCustomerTransactions;
            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.SaveEFTCustomerTransactionsParam1, SqlDbType.Structured, eftCustomerRemittancesDT);
            var ds = base.Insert(cmd);
            eftResponse.Status = ResponseStatus.Success;
            return eftResponse;
        }

        /// <summary>
        /// Get EFT Customer Detail Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public int DeleteBankEntryItemReference(ReceivablesRequest eftRequest)
        {
            string spName = string.Empty;

            if (eftRequest.AuditInformation.CompanyId == 1)
                spName = Configuration.SPNADeleteItemReferenceNumber;
            else
                spName = Configuration.SPEUDeleteItemReferenceNumber;

            var cmd = CreateStoredProcCommand(spName);

            cmd.Parameters.AddInputParams(Configuration.SPNADeleteItemReferenceNumberParam1, SqlDbType.Int, eftRequest.EFTPayment.EftAppId);

            return base.Delete(cmd);

        }
        #endregion EFT


        #endregion EFTAutomation

        /// <summary>
        /// To get tax user code
        /// </summary>
        /// <param name="dtOrderDtlsType"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public string GetCanadianTaxEligibleDetails(DataTable dtOrderDtlsType, int companyId)
        {
            string strTaxUserCode = string.Empty;
            var cmd = CreateStoredProcCommand(companyId == 1 ? Configuration.SPGetCanadianTaxEligibleDetails : null);
            cmd.Parameters.AddInputParams(Configuration.OrderDtlsType, SqlDbType.Structured, dtOrderDtlsType);
            DataSet ds = base.GetDataSet(cmd);

            if (ds != null)
            {
                DataTable dt = ds.Tables[0];

                foreach (DataRow row in dt.Rows)
                {
                    strTaxUserCode = row[0].ToString();
                }
            }
            return strTaxUserCode;
        }

        /// <summary>
        /// Get EFT Customer Detail Entry window
        /// </summary>
        /// <param name="SalesOrderRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse AuditCustomizeServiceSkU(SalesOrderRequest salesOrderRequest,ref string logMessag)
        {
            SalesOrderResponse salesOrderResponse = new SalesOrderResponse();
            StringBuilder AuditCustomizeServiceSkULoggin = new StringBuilder();
            AuditCustomizeServiceSkULoggin.AppendLine(DateTime.Now.ToString() + Configuration.SPAuditCustomizeServiceSkUParam1.ToString() + salesOrderRequest.SalesOrderEntity.SopNumber);
            AuditCustomizeServiceSkULoggin.AppendLine(DateTime.Now.ToString() + Configuration.SPAuditCustomizeServiceSkUParam2.ToString() + salesOrderRequest.SalesOrderEntity.SopType);
            AuditCustomizeServiceSkULoggin.AppendLine(DateTime.Now.ToString() + Configuration.SPAuditCustomizeServiceSkUParam3.ToString() + salesOrderRequest.IsCustomizeServiceSkUs);
            AuditCustomizeServiceSkULoggin.AppendLine(DateTime.Now.ToString() + Configuration.SPAuditCustomizeServiceSkUParam4.ToString() + salesOrderRequest.AuditInformation.CreatedBy.ToString());

            var cmd = CreateStoredProcCommand(salesOrderRequest.AuditInformation.CompanyId == 1 ? Configuration.SPAuditCustomizeServiceSkUNA : Configuration.SPAuditCustomizeServiceSkUEU);
            cmd.Parameters.AddInputParams(Configuration.SPAuditCustomizeServiceSkUParam1, SqlDbType.VarChar, salesOrderRequest.SalesOrderEntity.SopNumber);
            cmd.Parameters.AddInputParams(Configuration.SPAuditCustomizeServiceSkUParam2, SqlDbType.Int, salesOrderRequest.SalesOrderEntity.SopType);
            cmd.Parameters.AddInputParams(Configuration.SPAuditCustomizeServiceSkUParam3, SqlDbType.TinyInt, salesOrderRequest.IsCustomizeServiceSkUs == true ? 1 :0);
            cmd.Parameters.AddInputParams(Configuration.SPAuditCustomizeServiceSkUParam4, SqlDbType.VarChar, salesOrderRequest.AuditInformation.CreatedBy.ToString());
            AuditCustomizeServiceSkULoggin.AppendLine(DateTime.Now.ToString() + "Before SP Called Successflly");
            var ds = base.Insert(cmd);
            AuditCustomizeServiceSkULoggin.AppendLine(DateTime.Now.ToString() + "SP Called Successflly");
            logMessag = AuditCustomizeServiceSkULoggin.ToString();
            salesOrderResponse.Status = ResponseStatus.Success;
            return salesOrderResponse;
        }
    }
}