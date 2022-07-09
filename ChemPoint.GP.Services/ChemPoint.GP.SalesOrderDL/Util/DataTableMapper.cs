using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.Entities.BaseEntities;

namespace ChemPoint.GP.SalesOrderDL.Utils
{
    public class DataTableMapper
    {
        public static DataTable GetSalesOrderTypeDataTable(SalesOrderRequest salesOrderRequest, DataColumnMap[] columns)
        {
            //var dt = GetDataTable(columns);
            // FillData<T>(t, dt, columns);
            DataTable dt = GetDataTable(columns);
            var dataRow = dt.NewRow();
            dataRow["SopType"] = salesOrderRequest.SalesOrderEntity.SopType.ToString();
            dataRow["SopNumber"] = salesOrderRequest.SalesOrderEntity.SopNumber.ToString();
            dataRow["IsInternational"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.SalesOrderType.IsInternational;
            dataRow["IsNAFTA"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.SalesOrderType.IsNaFta;
            dataRow["IsDropship"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.SalesOrderType.IsDropship;
            dataRow["IsBulk"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.SalesOrderType.IsBulk;
            dataRow["IsI3Order"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.SalesOrderType.IsI3Order;
            dataRow["IsCreditCard"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.SalesOrderType.IsCreditCard;
            dataRow["IsCorrective"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.SalesOrderType.IsCorrective;
            dataRow["IsFullTruckLoad"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.SalesOrderType.IsFullTruckLoad;
            dataRow["IsConsignment"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.SalesOrderType.IsConsignment;
            dataRow["IsCorrectiveBoo"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.SalesOrderType.IsCorrectiveBoo;
            dataRow["IsSampleOrder"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.SalesOrderType.IsSampleOrder;
            dataRow["IsTempControl"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.SalesOrderType.IsTempControl;
            dataRow["IsHazmat"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.SalesOrderType.IsHazmat;
            dataRow["IsFreezeProtect"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.SalesOrderType.IsFreezeProtect;
            dataRow["IsAutoPTEligible"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.SalesOrderType.IsAutoPTEligible;
            dataRow["IsCreditEnginePassed"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.SalesOrderType.IsCreditEnginePassed;
            dataRow["IsTaxEnginePassed"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.SalesOrderType.IsTaxEnginePassed;
            dataRow["ModifiedBy"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.AuditInformation.ModifiedBy;
            dataRow["CompanyId"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.AuditInformation.CompanyId.ToString();
            
            dt.Rows.Add(dataRow);
            return dt;
        }

        /// <summary>
        /// save header Comment details..
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable GetHeaderCommentInstruction(SalesOrderRequest salesOrderRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            DataRow dataRow = null;
            foreach (var sopHeaderInstruction in salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.SalesHeaderInstructionEntity)
            {
                dataRow = dt.NewRow();
                dataRow["SopType"] = salesOrderRequest.SalesOrderEntity.SopType.ToString();
                dataRow["SopNumber"] = salesOrderRequest.SalesOrderEntity.SopNumber.ToString();
                dataRow["InstructionTypeID"] = sopHeaderInstruction.InstructionTypeID;
                dataRow["CommentID"] = sopHeaderInstruction.InstructionCode;
                dataRow["CommentText"] = sopHeaderInstruction.CommentText;
                dataRow["ModifiedBy"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.AuditInformation.ModifiedBy;
                dataRow["CompanyId"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.AuditInformation.CompanyId.ToString();
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        /// <summary>
        /// save header Comment details..
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable GetAllocatedQty(SalesOrderRequest salesOrderRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            DataRow dataRow = null;
            foreach (var soLineAllocatedQty in salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.LineItems)
            {
                dataRow = dt.NewRow();
                dataRow["SopType"] = soLineAllocatedQty.SopType.ToString();
                dataRow["SopNumber"] = soLineAllocatedQty.SopNumber.ToString();
                dataRow["LineItemSequence"] = soLineAllocatedQty.OrderLineId;
                dataRow["ComponentLineSeqNumber"] = soLineAllocatedQty.ComponentSequenceNumber;
                dataRow["ItemNumber"] = soLineAllocatedQty.ItemNumber;
                dataRow["LocationCode"] = soLineAllocatedQty.LocationCode;
                dataRow["AllocatedQty"] = soLineAllocatedQty.AllocatedQuantity;
                dataRow["Quantity"] = soLineAllocatedQty.Quantity;
                dataRow["CancelledQuantity"] = soLineAllocatedQty.CancelledQuantity;
                dataRow["QuantityRemaining"] = soLineAllocatedQty.QuantityRemaining;
                dataRow["ModifiedBy"] = salesOrderRequest.AuditInformation.ModifiedBy;
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        /// <summary>
        /// Save Line Item comment detail
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable GetLineCommentInstruction(SalesOrderRequest salesOrderRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            DataRow dataRow = null;
            var sopLineInstructionlist = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.LineItems[0];
            foreach (var sopLineInstruction in sopLineInstructionlist.SalesLineInstruction)
            {
                dataRow = dt.NewRow();
                dataRow["SopType"] = salesOrderRequest.SalesOrderEntity.SopType.ToString();
                dataRow["SopNumber"] = salesOrderRequest.SalesOrderEntity.SopNumber.ToString();
                dataRow["LineItemSequence"] = sopLineInstruction.LineItemSequence.ToString();
                dataRow["ComponentSeqNumber"] = sopLineInstruction.ComponentSequenceNumber.ToString();
                dataRow["InstructionTypeID"] = sopLineInstruction.InstructionTypeID;
                dataRow["CommentID"] = sopLineInstruction.InstructionCode;
                dataRow["CommentText"] = sopLineInstruction.CommentText;
                dataRow["ModifiedBy"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.AuditInformation.ModifiedBy;
                dataRow["CompanyId"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.AuditInformation.CompanyId.ToString();
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        /// <summary>
        /// Third party DT Convert
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable GetTransactionAddressCode(SalesOrderRequest salesOrderRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            DataRow dataRow = null;
            foreach (var sopTransactionAddressCode in salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.AddressDetails)
            {
                dataRow = dt.NewRow();
                dataRow["SopType"] = salesOrderRequest.SalesOrderEntity.SopType.ToString();
                dataRow["SopNumber"] = salesOrderRequest.SalesOrderEntity.SopNumber.ToString();
                dataRow["CustomerID"] = sopTransactionAddressCode.CustomerID;
                dataRow["CustomerName"] = sopTransactionAddressCode.CustomerName;
                dataRow["AddressTypeID"] = sopTransactionAddressCode.AddressId;
                dataRow["AddressCode"] = sopTransactionAddressCode.AddressCode;
                dataRow["ShipToName"] = sopTransactionAddressCode.ShipToName;
                dataRow["ContactPerson"] = sopTransactionAddressCode.ContactName;
                dataRow["Address1"] = sopTransactionAddressCode.AddressLine1;
                dataRow["Address2"] = sopTransactionAddressCode.AddressLine2;
                dataRow["Address3"] = sopTransactionAddressCode.AddressLine3;
                dataRow["Country"] = sopTransactionAddressCode.Country;
                dataRow["City"] = sopTransactionAddressCode.City;
                dataRow["State"] = sopTransactionAddressCode.State;
                dataRow["Zip"] = sopTransactionAddressCode.ZipCode;
                dataRow["CCode"] = sopTransactionAddressCode.CountryCode;
                dataRow["Phone1"] = sopTransactionAddressCode.Phone1;
                dataRow["Phone2"] = sopTransactionAddressCode.Phone2;
                dataRow["Phone3"] = sopTransactionAddressCode.Phone3;
                dataRow["Fax"] = sopTransactionAddressCode.Fax;
                dataRow["ModifiedBy"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.AuditInformation.ModifiedBy;
                dataRow["CompanyId"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.AuditInformation.CompanyId;

                dt.Rows.Add(dataRow);
            }
            return dt;
        }
            
        /// <summary>
        /// SOP Customer Detail transaction details
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable GetSalesLineItemDataTable(SalesOrderRequest salesOrderRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            var dataRow = dt.NewRow();
             
            List<SalesLineItem> lineItemlist = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.LineItems;
            foreach (var lineItem in lineItemlist)
            {
                dataRow["SopType"] = lineItem.SopType;
                dataRow["SopNumber"] = lineItem.SopNumber;
                dataRow["LineItemSequence"] = lineItem.OrderLineId;
                dataRow["ComponentLineSeqNumber"] = lineItem.ComponentSequenceNumber;
                dataRow["ItemNumber"] = lineItem.ItemNumber;
                dataRow["FreightPrice"] = lineItem.OrginatingFreightPrice;
                dataRow["QuoteNumber"] = lineItem.QuoteNumber;
                dataRow["ActualShipDate"] = lineItem.ActualShipDate;
                dataRow["ShipWeight"] = lineItem.ShipWeight;
                dataRow["NetWeight"] = lineItem.NetWeight;
                dataRow["UNNumber"] = lineItem.UnNumber;
                dataRow["CountryofOrigin"] = lineItem.CountryOfOrigin;
                dataRow["ECCNNumber"] = lineItem.EccnNumber;
                dataRow["HTSNumber"] = lineItem.HtsNumber;
                //dataRow["OriginatingFreightPrice"] = lineItem.OrginatingFreightPrice;
            }
            dt.Rows.Add(dataRow);
            return dt;
        }

        /// <summary>
        /// SOP Customer Detail transaction details
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable GetSalesOrderHeaderDataTable(SalesOrderRequest salesOrderRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            var dataRow = dt.NewRow();
            dataRow["SopType"] = salesOrderRequest.SalesOrderEntity.SopType.ToString();
            dataRow["SopNumber"] = salesOrderRequest.SalesOrderEntity.SopNumber.ToString();
            dataRow["FreightTerm"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.FreightTerm;
            dataRow["IncoTerm"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.IncoTerm;
            dataRow["CarrierName"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.CarrierName;
            dataRow["CarrierAccountNumber"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.CarrierAccountNumber;
            dataRow["CarrierPhone"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.CarrierPhone;
            dataRow["ThirdPartyAddressId"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.ThirdPartyAddressId;
            dataRow["ServiceType"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.ServiceType;
            dataRow["ImporterofRecord"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.ImporterofRecord;
            dataRow["CustomBroker"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.CustomBroker;
              
            dt.Rows.Add(dataRow);
            return dt;
        }

        /// <summary>
        /// SOP Customer Detail Schedule details
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable GetSalesOrderScheduleDataTable(SalesOrderRequest salesOrderRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            var dataRow = dt.NewRow();
            dataRow["SopType"] = salesOrderRequest.SalesOrderEntity.SopType.ToString();
            dataRow["SopNumber"] = salesOrderRequest.SalesOrderEntity.SopNumber.ToString();
            dataRow["CurCustReqShipDate"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderSchedule.CustomerRequestedShipDate;
            dataRow["CurCustReqDelDate"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderSchedule.CustomerRequestedDeliveryDate;
            dataRow["CurSchedDelDate"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderSchedule.RequestedDeliveryDate;
            dataRow["CurSchedShipDate"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderSchedule.RequestedShipDate;
            dataRow["OrigCustReqDelDate"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderSchedule.OriginalCustomerRequestedDeliveryDate;
            dataRow["OrigSchedDelDate"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderSchedule.OriginalRequestedDeliveryDate;
            dataRow["OrigSchedShipDate"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderSchedule.OriginalRequestedShipDate;
              
            dt.Rows.Add(dataRow);
            return dt;
        }

        /// <summary>
        /// Third party DT Convert
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable Get3rdPartyDataTable(SalesOrderRequest salesOrderRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            var dataRow = dt.NewRow();
            dataRow["SalesThirdPartyAddressID"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.Carrier3rdPartyAddress.AddressId;
            dataRow["SopType"] = salesOrderRequest.SalesOrderEntity.SopType.ToString();
            dataRow["SopNumber"] = salesOrderRequest.SalesOrderEntity.SopNumber.ToString();
            dataRow["AddressId"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.Carrier3rdPartyAddress.AddressId;
            dataRow["ContactName"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.Carrier3rdPartyAddress.ContactName;
            dataRow["AddressLine1"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.Carrier3rdPartyAddress.AddressLine1;
            dataRow["AddressLine2"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.Carrier3rdPartyAddress.AddressLine2;
            dataRow["AddressLine3"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.Carrier3rdPartyAddress.AddressLine3;
            dataRow["City"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.Carrier3rdPartyAddress.City;
            dataRow["State"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.Carrier3rdPartyAddress.State;
            dataRow["ZipCode"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.Carrier3rdPartyAddress.ZipCode;
            dataRow["Country"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.Carrier3rdPartyAddress.Country;
            dataRow["ModifiedBy"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.AuditInformation.ModifiedBy;
            dataRow["CompanyId"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.AuditInformation.CompanyId;
            
            dt.Rows.Add(dataRow);
            return dt;
        }

        /// <summary>
        ///Customer Pick up  DT Convert
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable GetCustomerPickupDataTable(SalesOrderRequest salesOrderRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            var dataRow = dt.NewRow();
            dataRow["SalesThirdPartyAddressID"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.CustomerPickupAddress.AddressId;
            dataRow["SopType"] = salesOrderRequest.SalesOrderEntity.SopType.ToString();
            dataRow["SopNumber"] = salesOrderRequest.SalesOrderEntity.SopNumber.ToString();
            dataRow["AddressId"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.CustomerPickupAddress.AddressId;
            dataRow["ContactName"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.CustomerPickupAddress.ContactName;
            dataRow["AddressLine1"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.CustomerPickupAddress.AddressLine1;
            dataRow["AddressLine2"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.CustomerPickupAddress.AddressLine2;
            dataRow["AddressLine3"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.CustomerPickupAddress.AddressLine3;
            dataRow["City"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.CustomerPickupAddress.City;
            dataRow["State"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.CustomerPickupAddress.State;
            dataRow["ZipCode"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.CustomerPickupAddress.ZipCode;
            dataRow["Country"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.CustomerPickupAddress.Country;
            dataRow["ModifiedBy"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.AuditInformation.ModifiedBy;
            dataRow["CompanyId"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.AuditInformation.CompanyId;

            dt.Rows.Add(dataRow);
            return dt;
        }

        /// <summary>
        /// Service SKU Validation DT convert.
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable GetServiceSKUValidationDataTable(SalesOrderRequest salesOrderRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            List<SalesLineItem> lineItemlist = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.LineItems;
            foreach (var lineItem in lineItemlist)
            {
                var dataRow = dt.NewRow();
                dataRow["SopType"] = salesOrderRequest.SalesOrderEntity.SopType.ToString();
                dataRow["SopNumber"] = salesOrderRequest.SalesOrderEntity.SopNumber.ToString();
                dataRow["FreightTerm"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.FreightTerm;
                dataRow["CurrencyId"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.OrderCurrency;
                dataRow["CustomerNumber"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerId;
                dataRow["MiceAmount"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.MiscAmount;
                dataRow["ItemNumber"] = lineItem.ItemNumber;
                dataRow["Quantity"] = lineItem.OrderedQuantity;
                dataRow["WarehouseID"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Warehouse.WarehouseId;
                dataRow["CreatedOn"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.OrderSubmittedDate;
                dataRow["RequestedShipDate"] = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderSchedule.RequestedShipDate;
                dt.Rows.Add(dataRow);
            }
            return dt;
        }


        /// <summary>
        /// Service SKU Validation DT convert.
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable SaveApplyToOpenOrder(ReceivablesRequest cashApplicationRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            foreach (var cashApplication in cashApplicationRequest.ReceivableEntity)
            {
                var dataRow = dt.NewRow();
                dataRow["DocumentNumber"] = cashApplication.ApplyFromDocumentNumber.ToString();
                dataRow["DocumentType"] = cashApplication.ApplyFromDocumentTypeId;
                dataRow["CustomerId"] = cashApplicationRequest.CustomerInformation.CustomerId.ToString();
                dataRow["CurrencyId"] = cashApplication.Amount.Currency.ToString();
                dataRow["ApplyToDocumentNumber"] = cashApplication.ApplyToDocumentNumber.ToString();
                dataRow["ApplyToDocumentType"] = cashApplication.ApplyToDocumentType.ToString();
                dataRow["ApplyToCustomerId"] = cashApplication.ApplyToCustomerId.ToString();
                dataRow["OriginatingAmountRemaining"] = cashApplication.Amount.AmountRemaining;
                dataRow["OriginatingDocumentAmount"] =  cashApplication.Amount.DocumentAmount;
                dataRow["OriginatingApplyAmount"] = cashApplication.Amount.ApplyAmount;
                dataRow["FunctionalAmountRemaining"] = cashApplication.Amount.AmountRemaining;
                dataRow["FunctionalDocumentAmount"] = cashApplication.Amount.DocumentAmount;
                dataRow["FunctionalApplyAmount"] = cashApplication.Amount.ApplyAmount;
                dataRow["DocumentCurrencyId"] = cashApplication.DocumentCurrencyId.ToString();
                //dataRow["DocumentAmountinOrignCurrency"] = cashApplication.Amount.OriginatingCurrencyDocumentAmount;
                //dataRow["ApplyAmountInOrignCurrency"] = cashApplication.Amount.ApplyAmountInOrignCurrency;
                dataRow["ExchangeRate"] = cashApplication.Amount.ExchangeRate;
                dataRow["ApplyDate"] = cashApplication.ApplyDate;
                dataRow["DocumentStatus"] = cashApplication.DocumentStatus.ToString();
                dataRow["JobStatusId"] = cashApplication.StatusId;
                dataRow["IsSelected"] = cashApplication.IsSelected;
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        /// <summary>
        /// SOP Customer Detail transaction details
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable SaveReceivablesDetailDataTable(ReceivablesRequest receivablesRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            foreach (var lineItem in receivablesRequest.ReceivableEntity)
            {
                var dataRow = dt.NewRow();
                dataRow["InvoiceNumber"] = lineItem.ApplyToDocumentNumber;
                dataRow["SupportId"] = lineItem.SupportId;
                dataRow["IncidentId"] = lineItem.IncidentId;
                dataRow["ItemNumber"] = lineItem.ItemNumber;
                dataRow["ItemDescription"] = lineItem.ItemDescription;
                dataRow["LineItemSequence"] = lineItem.OrderLineId;
                dataRow["IsSelected"] = lineItem.IsSelected;
                dataRow["Volume"] = lineItem.ShipWeight;
                dataRow["Quantity"] = lineItem.OrderedQuantity;
                dataRow["NetWeight"] = lineItem.NetWeight;
                dataRow["ExtendedPrice"] = lineItem.LineTotalAmount;
                dataRow["AppliedAmount"] = lineItem.UnitPriceAmount;
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        /// <summary>
        /// Save EFT Email Remittance Details ...
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable SaveEFTEmailRemittances(ReceivablesRequest receivablesRequest, DataColumnMap[] columns)
        {
            int rowId = 1;
            DataTable dt = GetDataTable(columns);
            foreach (var eftEFTCustomerRemittanceList in receivablesRequest.EFTPaymentList)
            {
                var dataRow = dt.NewRow();
                dataRow["RowId"] = eftEFTCustomerRemittanceList.EFTRowId;
                dataRow["EftId"] = eftEFTCustomerRemittanceList.EftId;
                dataRow["ReferenceNumber"] = eftEFTCustomerRemittanceList.ReferenceNumber;
                dataRow["DateReceived"] = eftEFTCustomerRemittanceList.DateReceived;
                dataRow["PaymentAmount"] = eftEFTCustomerRemittanceList.PaymentAmount;
                dataRow["CustomerID"] = eftEFTCustomerRemittanceList.CustomerID;
                dataRow["CurrencyId"] = eftEFTCustomerRemittanceList.CurrencyID;
                dataRow["ItemReferenceNumber"] = eftEFTCustomerRemittanceList.ItemReference;
                dataRow["ItemAmount"] = eftEFTCustomerRemittanceList.ItemAmount;
                dataRow["CreatedBy"] = receivablesRequest.AuditInformation.CreatedBy;
                dataRow["IsUpdated"] = eftEFTCustomerRemittanceList.IsUpdated;
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        /// <summary>
        /// Save EFT Customer Details
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable SaveEFTCustomerDetails(ReceivablesRequest receivablesRequest, DataColumnMap[] columns)
        {
            DataTable dt = GetDataTable(columns);
            foreach (var eftTubleList in receivablesRequest.CustomerInformation.CustomerMappingDetails)
            {
                var dataRow = dt.NewRow();
                dataRow["NAEFTCustomerMappingId"] = eftTubleList.EftCustomerMappingId;
                dataRow["EftCustomerDetail"] = eftTubleList.EftCTXCustomerReference.ToString();
                dataRow["CustomerId"] = receivablesRequest.CustomerInformation.CustomerId.ToString().Trim();
                dataRow["Type"] = eftTubleList.Type;
                dataRow["CreatedBy"] = receivablesRequest.AuditInformation.CreatedBy.ToString().Trim();
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        /// <summary>
        /// Save EFT Customer Remittance Details ...
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTable SaveEFTCustomerRemittances(ReceivablesRequest receivablesRequest, DataColumnMap[] columns)
        {
            int rowId = 1;
            DataTable dt = GetDataTable(columns);
            foreach (var eftEFTCustomerRemittanceList in receivablesRequest.EFTPaymentList)
            {
                var dataRow = dt.NewRow();
                if (eftEFTCustomerRemittanceList.EftId == 0)
                {
                    dataRow["RowId"] = rowId;
                    rowId++;
                }
                else
                    dataRow["RowId"] = 0;
                dataRow["EftId"] = eftEFTCustomerRemittanceList.EftId;
                dataRow["EftAppId"] = eftEFTCustomerRemittanceList.EftAppId;
                dataRow["PaymentNumber"] = eftEFTCustomerRemittanceList.PaymentNumber;
                dataRow["ReferenceNumber"] = eftEFTCustomerRemittanceList.ReferenceNumber;
                dataRow["DateReceived"] = eftEFTCustomerRemittanceList.DateReceived;
                dataRow["PaymentAmount"] = eftEFTCustomerRemittanceList.PaymentAmount;
                dataRow["CustomerID"] = eftEFTCustomerRemittanceList.CustomerID;
                dataRow["CurrencyId"] = eftEFTCustomerRemittanceList.CurrencyID;
                dataRow["IsFullyApplied"] = eftEFTCustomerRemittanceList.IsFullyApplied;
                dataRow["Source"] = eftEFTCustomerRemittanceList.Source;
                dataRow["BankOriginating"] = eftEFTCustomerRemittanceList.BankOriginatingID;
                dataRow["ItemReferenceNumber"] = eftEFTCustomerRemittanceList.ItemReference;
                dataRow["ItemAmount"] = eftEFTCustomerRemittanceList.ItemAmount;
                dataRow["CreatedBy"] = receivablesRequest.AuditInformation.CreatedBy;
                dataRow["IsUpdated"] = eftEFTCustomerRemittanceList.IsUpdated;
                dataRow["EftStatusId"] = eftEFTCustomerRemittanceList.EftStatusId;
                dataRow["EftNotes"] = eftEFTCustomerRemittanceList.Notes;
                dataRow["FileId"] = eftEFTCustomerRemittanceList.EftFileId;
                dt.Rows.Add(dataRow);
            }
            return dt;
        }
        public static DataTable GetDataTable<T>(List<T> lst, DataColumnMap[] columns)
        {
            var dt = GetDataTable(columns);
            FillData<T>(lst, dt, columns);
            return dt;
        }

        public static DataTable GetDataTable<T>(T t, DataColumnMap[] columns)
        {
            var dt = GetDataTable(columns);
            FillData<T>(t, dt, columns);
            return dt;
        }

        private static DataTable GetDataTable(DataColumnMap[] columns)
        {
            var dataTable = new DataTable();

            foreach (var column in columns)
            {
                var dataColumn = new DataColumn();
                dataColumn.ColumnName = column.ColumnName;
                dataColumn.DataType = column.Type;

                if (column.IsDefaultValue)
                    dataColumn.DefaultValue = column.DefaultValue;

                dataTable.Columns.Add(dataColumn);
            }
            return dataTable;
        }

        private static void FillData<T>(List<T> lstData, DataTable dataTable, DataColumnMap[] columns)
        {
            foreach (var data in lstData)
            {
                FillData<T>(data, dataTable, columns);
            }
        }

        private static void FillData<T>(T data, DataTable dataTable, DataColumnMap[] columns)
        {
            var dataRow = dataTable.NewRow();

            foreach (var column in columns)
                dataRow[column.ColumnName] = GetPropValue(data, column.ColumnName);

            dataTable.Rows.Add(dataRow);
        }

        private static object GetPropValue(object src, string propName)
        {
            object value = src.GetType().GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(src, null);

            if (value.GetType() == typeof(string) && string.IsNullOrEmpty(value.ToString()))
                return "";

            else if (value == null || string.IsNullOrEmpty(value.ToString()))
                return DBNull.Value;

            else if (value.GetType() == typeof(DateTime) && ((DateTime)value) == DateTime.MinValue)
                return DBNull.Value;

            //else if (value.GetType().BaseType == typeof(TriggerSource) && propName.ToUpper() == "TRIGGERSOURCE")
            //    return ((TriggerSource)value).Name.ToString();

            //else if (value.GetType().BaseType == typeof(TriggerSource))
            //    return ((TriggerSource)value).EmployeeStatus.ToString();

            else
                return value;
        }
    }
}

