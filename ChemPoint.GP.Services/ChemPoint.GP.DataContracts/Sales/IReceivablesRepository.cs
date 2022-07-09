using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChemPoint.GP.DataContracts.Base;
using Chempoint.GP.Model.Interactions.Sales;
namespace ChemPoint.GP.DataContracts.Sales
{
    public interface IReceivablesRepository : IRepository
    {
        #region EFTPayment

        object GetEftOpenTransactions(int companyId);

        object GetEftTransactionDetails(int companyId);

        object SaveImportedEFTBankSummaryDetail(ReceivablesRequest ReceivablesRequest);

        object UpdateEFTTransactionsApplyDetails(DataTable dtUpdateEFTTransactions, DataTable dtInsertEFTTransactions, DataTable dtBankRemittanceTrans, DataTable dtBankRemittanceTransMap, int companyId, string userId);


        object SaveRemittanceFileDetails(DataTable dtBankRemittanceOriginalEFTTransaction, DataTable dtBankRemittanceOriginalEFTTransactionMap, string strUserName, string strFileName, int companyId);

        string IsEftFileAlreadyProcessed(string strFileName, string BatchNumber, int companyId);
        #endregion

        #region Remittance

        DataSet ValidatePaymentRemittance(ReceivablesRequest eftRequest);

        ReceivablesResponse ValidateEFTLine(DataTable dtSummaryLineRef, DataTable dtSummaryLineItemRef, int companyId);

        ReceivablesResponse GetPushToGP(ReceivablesRequest eftRequest, int actionType);

        ReceivablesResponse UpdatePaymentStatus(string createdBy, string nextAvailablePaymentNumber, int companyId, string referenceNumber);

        ReceivablesResponse UpdateApplyStatus(int EftId, DataTable applyType, int companyId);


        object SaveDocumentForApplyToOpenOrdersForEngine(ReceivablesResponse cashEntity, int companyId);

        string GetNextPaymentNumber(int orderType, int companyId);

        ReceivablesResponse FetchEmailReferenceLookup(ReceivablesRequest ReceivableRequest);

        ReceivablesResponse FetchEmailReferenceScroll(ReceivablesRequest ReceivableRequest);

        ReceivablesResponse DeleteEFTEmailRemittance(ReceivablesRequest ReceivableRequest);

        ReceivablesResponse SaveEmailReferences(ReceivablesRequest ReceivableRequest);

        int DeleteBankEntryEFTTransaction(ReceivablesRequest ReceivableRequest);

        ReceivablesResponse MapBankEntryToEmailRemittance(ReceivablesRequest receivableRequest);

        #endregion
    }
}
