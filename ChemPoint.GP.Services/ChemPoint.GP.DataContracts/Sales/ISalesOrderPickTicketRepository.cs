using System;

namespace ChemPoint.GP.DataContracts.Sales
{
    public interface ISalesOrderPickTicketRepository
    {
        #region PickTicket
        
        object GetSalesOrderDetailsForPT(string invoiceNumber, int invoiceType);
        
        object SendPTToWHandChr(string invoiceNumber);
        
        object UpdateSalesDocumentStatus(string invoiceNumber, int companyID);
    
        #endregion PickTicket
    }
}
