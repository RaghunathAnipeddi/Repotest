using Chempoint.GP.Model.Interactions.Sales;

namespace ChemPoint.GP.BusinessContracts.Sales
{
    public interface ISalesOrderBusiness
    {
        //SOP Entry Fetch 
        SalesOrderResponse GetSalesOrder(SalesOrderRequest salesOrderRequest);

        //Fetch Inco Term
        SalesOrderResponse FetchIncoTerm(SalesOrderRequest salesOrderRequest);

        //Fetch Country Details
        SalesOrderResponse GetCountryDetails(SalesOrderRequest salesOrderRequest);

        //Fetch Current Delivery Date
        SalesOrderResponse GetSalesCurSchedDelDate(SalesOrderRequest salesOrderRequest);

        //SaveAllocatedQty 
        SalesOrderResponse SaveAllocatedQty(SalesOrderRequest salesOrderRequest);

        //FetchSopChangedQty
        SalesOrderResponse FetchSalesOrderLineForTermHold(SalesOrderRequest salesOrderRequest);

        //Fetch Current Ship Date
        SalesOrderResponse GetSalesCurSchedShipDate(SalesOrderRequest salesOrderRequest);

        //Update Tax Schedule ID For Tax calculation
        SalesOrderResponse UpdateTaxScheduleIdToLine(SalesOrderRequest salesOrderRequest);

        //SOP Entry Save
        SalesOrderResponse SaveSalesOrder(SalesOrderRequest salesOrderRequest);

        //Item detail save
        SalesOrderResponse GetSalesItemDetail(SalesOrderRequest salesOrderRequest);

        //Item detail save
        SalesOrderResponse SaveSalesItemDetail(SalesOrderRequest salesOrderRequest);

        //Save Order Type...
        SalesOrderResponse SaveOrderTypeDetail(SalesOrderRequest salesOrderRequest);

        //Save ThirdParty
        SalesOrderResponse SaveThirdPartyAddress(SalesOrderRequest salesOrderRequest);

        //Save Customer Pick up Business Contract..
        SalesOrderResponse SaveCustomerPickupAddress(SalesOrderRequest salesOrderRequest);

        //SalesOrder Void..
        SalesOrderResponse SalesOrderVoid(SalesOrderRequest salesOrderRequest);

        SalesOrderResponse UpdatePrintPickTicketStatus(SalesOrderRequest salesOrderRequest);

        //PTE
        SalesOrderResponse UpdatePteLog(SalesOrderRequest soRequest);

        //Save AddressId from Customer Detail Entry
        SalesOrderResponse SaveSopTransactionAddressCodes(SalesOrderRequest salesOrderRequest);

        //Save Header Comment instruction data..
        SalesOrderResponse SaveHeaderCommentInstruction(SalesOrderRequest salesOrderRequest);

        //Save Line Comment instruction data..
        SalesOrderResponse SaveLineCommentInstruction(SalesOrderRequest salesOrderRequest);

        //validate Sop Address code...
        SalesOrderResponse ValidateSopTransactionAddressCodes(SalesOrderRequest salesOrderRequest);

        //validate ServiceSkuItems
        SalesOrderResponse ValidateSopTransactionServiceSkuItems(SalesOrderRequest salesOrderRequest);

        //Get Quote Number...
        SalesOrderResponse GetQuoteNumber(SalesOrderRequest salesOrderRequest);

        //Validate Quote Number...
        SalesOrderResponse ValidateQuoteNumber(SalesOrderRequest salesOrderRequest);

        //Lot Details
        SalesOrderResponse SaveSalesLotDetails(SalesOrderRequest salesGpLotsRequest);

        SalesOrderResponse GetLotDetails(SalesOrderRequest salesGpLotsRequest);

        //Tax Enhancement
        SalesOrderResponse GetVatLookupDetails(SalesOrderRequest salesVatRequest);

        //Order to FO Transfer
        bool TransferOrderToFO(BulkOrderTransferRequest soRequest);

        SalesOrderResponse RunCommitmentEngine(SalesOrderRequest salesOrderRequest);

        SalesOrderResponse UpdateOrderDetailsToWarehouseIBoard(SalesOrderRequest salesOrderRequest);

        SalesOrderResponse UpdateOrderDetailsToInvoiceType(SalesOrderRequest salesOrderRequest);

        //Cash Application Process - Apply To Open Orders 

        SalesOrderResponse DistributeAmountToCashApplyInvoices(SalesOrderRequest salesApplyRequest);

        bool ApplyToOpenOrders(SalesOrderRequest soRequest);

        ReceivablesResponse FetchApplyToOpenOrder(ReceivablesRequest aRequest);

        ReceivablesResponse SaveApplyToOpenOrder(ReceivablesRequest aRequest);

        ReceivablesResponse UpdateApplyToOpenOrder(ReceivablesRequest aRequest);

        ReceivablesResponse DeleteApplyToOpenOrder(ReceivablesRequest aRequest);

        ReceivablesResponse GetReceivablesDetail(ReceivablesRequest receivablesRequest);

        ReceivablesResponse GetDistributeAmountDetail(ReceivablesRequest receivablesRequest);

        ReceivablesResponse SaveReceivablesDetail(ReceivablesRequest receivablesRequest);

        #region WarehouseClosure

        SalesOrderResponse ValidateWarehouseClosureDate(SalesOrderRequest salesOrderRequest);


        #endregion 

        #region EFTAutomation

        // Save the EFT Customre Mapping details
        ReceivablesResponse SaveEFTCustomerMappingDetails(ReceivablesRequest eftRequest);

        // Get the EFT Customre Mapping details
        ReceivablesResponse GetEFTCustomerMappingDetails(ReceivablesRequest eftRequest);

        ReceivablesResponse GetEFTPaymentRemittances(ReceivablesRequest eftRequest);

        //Get Eft Customer Details 
        ReceivablesResponse FetchCustomerId(ReceivablesRequest receivablesRequest);

        ReceivablesResponse FetchDocumentNumber(ReceivablesRequest receivablesRequest);

        ReceivablesResponse FetchReferenceId(ReceivablesRequest receivablesRequest);

        ReceivablesResponse FetchCustomerIdForReference(ReceivablesRequest receivablesRequest);

        ReceivablesResponse FetchBatchId(ReceivablesRequest receivablesRequest);

        //Get Eft Customer Details 
        ReceivablesResponse GetEFTCustomerRemittances(ReceivablesRequest salesRequest);

        //Get EFT Email Remittance Window
        ReceivablesResponse GetEFTEmailRemittances(ReceivablesRequest eftEmailRequest);

        //Save EFT Email Remittance Window
        ReceivablesResponse SaveEFTEmailRemittances(ReceivablesRequest eftEmailRequest);

        //Get Eft Customer Details 
        ReceivablesResponse SaveEFTCustomerRemittances(ReceivablesRequest salesRequest);

        //Get Eft Customer Details 
        ReceivablesResponse ValidateEftCustomer(ReceivablesRequest salesRequest);

        ReceivablesResponse ValidateEFTItemReference(ReceivablesRequest salesRequest);

        ReceivablesResponse ValidateEftReference(ReceivablesRequest salesRequest);

        ReceivablesResponse ValidateEFTCustomerRemittanceSummary(ReceivablesRequest salesRequest);

        ReceivablesResponse GetEFTPaymentRemittanceAmountDetails(ReceivablesRequest eftRequest);

        ReceivablesResponse DeleteBankEntryItemReference(ReceivablesRequest eftRequest);

        //Validate eft Email Reference
        ReceivablesResponse ValidateEftEmailReference(ReceivablesRequest eftRequest);

        #endregion EFTAutomation

        SalesOrderResponse AuditCustomizeServiceSkU(SalesOrderRequest salesOrderRequest);

        

    }
}
