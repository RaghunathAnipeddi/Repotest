using Chempoint.GP.Model.Interactions.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.BusinessContracts.Sales
{
    public interface IReceivablesBusiness
    {
        #region EFTPayment

        ReceivablesResponse ImportEFTBankSummaryReport(ReceivablesRequest receivablesRequest);

        ReceivablesResponse ImportEFTRemittanceReport(ReceivablesRequest request);

        ReceivablesResponse ValidateEFTLine(ReceivablesRequest receivablesRequest);

        #endregion

        #region Remittance



        ReceivablesResponse PushEftTransactionsToGP(ReceivablesRequest salesRequest);

        ReceivablesResponse ValidatePaymentRemittance(ReceivablesRequest salesRequest);

        ReceivablesResponse FetchEmailReferenceLookup(ReceivablesRequest receivableRequest);

        ReceivablesResponse FetchEmailReferenceScroll(ReceivablesRequest receivableRequest);

        ReceivablesResponse DeleteEFTEmailRemittance(ReceivablesRequest ReceivableRequest);

        ReceivablesResponse SaveEmailReferences(ReceivablesRequest ReceivableRequest);

        ReceivablesResponse DeleteBankEntryEFTTransaction(ReceivablesRequest receivableRequest);

        ReceivablesResponse MapBankEntryToEmailRemittance(ReceivablesRequest receivableRequest);
        #endregion
    }
}
