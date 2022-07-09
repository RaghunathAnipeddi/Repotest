using ChemPoint.GP.DataContracts.Base;
using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.Entities.BaseEntities;
using ChemPoint.GP.Entities.Business_Entities.Sales;
using System.Data;

namespace ChemPoint.GP.DataContracts.Sales
{
    public interface ISalesOrderUpdateRepository : IRepository
    {
        SalesOrderResponse GetSalesOrder(SalesOrderRequest salesOrderRequest);

        SalesOrderResponse FetchIncoTerm(SalesOrderRequest salesOrderRequest);

        SalesOrderResponse GetCountryDetails(SalesOrderRequest salesOrderRequest);

        SalesOrderResponse GetSalesCurSchedDelDate(SalesOrderRequest salesOrderRequest);

        SalesOrderResponse GetSalesCurSchedShipDate(SalesOrderRequest salesOrderRequest);

        SalesOrderResponse SaveAllocatedQty(SalesOrderRequest salesOrderRequest);

        SalesOrderResponse FetchSalesOrderLineForTermHold(SalesOrderRequest salesOrderRequest);

        //Get Service SKU
        SalesOrderResponse GetServiceSKULineItem(SalesOrderRequest salesOrderRequest);

        object UpdateTaxScheduleIdToLine(SalesOrderRequest salesOrderRequest);

        object SalesOrderVoid(SalesOrderRequest salesOrderRequest);

        object SaveSalesItemDetail(SalesOrderRequest salesOrderRequest, int companyId);

        //Item detail save
        SalesOrderResponse GetSalesItemDetail(SalesOrderRequest salesOrderRequest);

        object SaveSalesOrder(SalesOrderRequest salesOrderRequest, int companyId);
        
        //Save Order Type...   
        object SaveOrderTypeDetail(SalesOrderRequest salesOrderRequest);  

        //Save Third party Data contract
        object SaveThirdPartyAddress(SalesOrderRequest salesOrderRequest);

        //Save Customer Pick up Data Contract..
        object SaveCustomerPickupAddress(SalesOrderRequest salesOrderRequest);

        SalesOrderResponse GetOrderDetailsForPushOrderToGP(SalesOrderRequest salesOrderRequest, int companyId);

        bool UpdatePteRequestToGP(SalesOrderRequest salesOrderRequest);

        #region LotDetails
        
        void SaveSalesLotDetails(string sopNumber, int sopType, int lineItemSequence, string itemNumber, string inPutXml, int companyId, string userName);
        
        SalesLineItem GetLotDetails(string sopNumber, int sopType, string itemNumber);
        
        #endregion LotDetails
        
        //SaveAddressID From Customer detail Entry...    
        object SaveSopTransactionAddressCodes(SalesOrderRequest salesOrderRequest);
        
        //Save Header Comments instruction list...
        object SaveHeaderCommentInstruction(SalesOrderRequest salesOrderRequest);
        
        //Save Line Comment instruction data..
        object SaveLineCommentInstruction(SalesOrderRequest salesOrderRequest);
        
        //validate Sop Address code...
        SalesOrderResponse ValidateSopTransactionAddressCodes(SalesOrderRequest salesOrderRequest);

        //validate Validate ServiceSkuItems
        SalesOrderResponse ValidateSopTransactionServiceSkuItems(SalesOrderRequest salesOrderRequest);
        
        //Get Quote Number...
        SalesOrderResponse GetQuoteNumber(SalesOrderRequest salesOrderRequest);
        
        //Validate Quote Number...
        SalesOrderResponse ValidateQuoteNumber(SalesOrderRequest salesOrderRequest);
        
        object UpdatePrintPickTicketStatus(string invoiceNumber, int sopType, int companyId);
        
        CustomerInformation GetVatNumberDetails(string customerNumber);
        
        #region TransferToFO

        BulkOrderTransferEntity GetOrdersToAutoFOTransfer();

        string GetNextAvailableFONumber(int orderType, string documentId, int companyId);

        #endregion TransferToFO
       
        object ExecuteCommitmentEngine(SalesOrderRequest salesOrderRequest);

        object UpdateOrderDetailsToWarehouseIBoard(SalesOrderRequest salesOrderRequest);

        object UpdateOrderDetailsToInvoiceType(SalesOrderRequest salesOrderRequest);
    
        object UpdateAutoSendPTLog(int invoiceType, string invoiceNumber, int status, string message);

        #region Cash Application Process

        object DistributeAmountToCashApplyInvoices(SalesOrderRequest salesApplyRequest, int companyId);

        BulkOrderTransferEntity GetDocumentForApplyToOpenOrdersForEngine(int companyId);

        object SaveDocumentForApplyToOpenOrdersForEngine(BulkOrderTransferEntity cashEntity, int companyId);

        ReceivablesResponse FetchApplyToOpenOrder(ReceivablesRequest aRequest);

        bool SaveApplyToOpenOrder(ReceivablesRequest aRequest);

        bool UpdateApplyToOpenOrder(ReceivablesRequest aRequest);

        bool DeleteApplyToOpenOrder(ReceivablesRequest aRequest);

        ReceivablesResponse GetReceivablesDetail(ReceivablesRequest receivablesRequest);

        ReceivablesResponse GetDistributeAmountDetail(ReceivablesRequest receivablesRequest);

        object SaveReceivablesDetail(ReceivablesRequest receivablesRequest);

        #endregion Cash Application Process

        #region WarehouseClosure

        SalesOrderResponse ValidateWarehouseClosureDate(SalesOrderRequest salesOrderRequest);

        #endregion

        #region EFT Automation

        #region eft
        //Get Eft Customer Details 
        ReceivablesResponse FetchCustomerId(ReceivablesRequest receivablesRequest);
        ReceivablesResponse FetchDocumentNumber(ReceivablesRequest receivablesRequest);
        ReceivablesResponse FetchReferenceId(ReceivablesRequest receivablesRequest);
        ReceivablesResponse FetchCustomerIdForReference(ReceivablesRequest receivablesRequest);
        //Get Eft Customer Details 
        bool ValidateEftCustomer(ReceivablesRequest receivablesRequest);
        bool ValidateEFTItemReference(ReceivablesRequest receivablesRequest);
        bool ValidateEftReference(ReceivablesRequest receivablesRequest);

        ReceivablesResponse ValidateEFTCustomerRemittanceSummary(DataTable dt, int CompanyId);

        ReceivablesResponse FetchBatchId(ReceivablesRequest receivablesRequest);

        ReceivablesResponse GetEFTPaymentRemittanceAmountDetails(ReceivablesRequest eftRequest);

        //Get Eft Customer Details 
        ReceivablesResponse GetEFTCustomerMappingDetails(ReceivablesRequest salesRequest);

        ReceivablesResponse GetEFTPaymentRemittances(ReceivablesRequest eftRequest);

        //Get EFT Email Remittance Window
        ReceivablesResponse GetEFTEmailRemittances(ReceivablesRequest eftEmailRequest);

        //Save EFT Email Remittance Window
        ReceivablesResponse SaveEFTEmailRemittances(ReceivablesRequest eftEmailRequest);

        //Save Eft Customer Details 
        ReceivablesResponse SaveEFTCustomerMappingDetails(ReceivablesRequest salesRequest);

        //Get Eft Customer Details 
        ReceivablesResponse GetEFTCustomerRemittances(ReceivablesRequest salesRequest);

        //Get Eft Customer Details 
        ReceivablesResponse SaveEFTCustomerRemittances(ReceivablesRequest salesRequest);

        int DeleteBankEntryItemReference(ReceivablesRequest eftRequest);

        //Validate Email Reference
        bool ValidateEftEmailReference(ReceivablesRequest eftRequest);
        #endregion eft

        #endregion EFT Automation

        string GetCanadianTaxEligibleDetails(DataTable dtOrderDtlsType, int companyId);

        SalesOrderResponse AuditCustomizeServiceSkU(SalesOrderRequest salesOrderRequest,ref string logginMessage);
        
    }
}
