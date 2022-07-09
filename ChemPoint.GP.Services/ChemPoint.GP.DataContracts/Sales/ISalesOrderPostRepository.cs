using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.DataContracts.Base;

namespace ChemPoint.GP.DataContracts.Sales
{
    public interface ISalesOrderPostRepository : IRepository
    {
        OrderBatchPostResponse RemoveBatchLockedUsers(OrderBatchPostRequest lockUserRequest, int companyId);

        object LockBatchforPosting(OrderBatchPostRequest lockBatchRequest, int companyId);

        object ValidateCadExemptionRules(OrderBatchPostRequest lockBatchRequest, int companyId);

        object UnLockBatchforPosting(OrderBatchPostRequest lockBatchRequest, int companyId);

        OrderBatchPostResponse FetchNumTrxandBatchTotal(OrderBatchPostRequest lockBatchRequest, int companyId);

        OrderBatchPostResponse ValidateBatchAndFetchData(OrderBatchPostRequest batchRequest, int companyId);

        OrderBatchPostResponse GetAllCanadianErrorTransactions(OrderBatchPostRequest batchRequest, int companyId);

        void UpdateSalesBatchTotals(OrderBatchPostRequest batchRequest, string orderNumber, string newBatch, int companyId);

        OrderBatchPostResponse GetAllDropshipTransactions(OrderBatchPostRequest batchRequest, int companyId);

        OrderBatchPostResponse GetAllDropShipErrorTransactions(OrderBatchPostRequest batchRequest, int companyId);

        OrderBatchPostResponse GetAllCreditCardTransactions(OrderBatchPostRequest batchRequest, int companyId);

        OrderBatchPostResponse GetAllInvalidExchangeRateTransactions(OrderBatchPostRequest batchRequest, int companyId);

        OrderBatchPostResponse GetAllDistributionErrorTransactions(OrderBatchPostRequest batchRequest, int companyId);

        OrderBatchPostResponse GetAllServiceSkuErrorTransactions(OrderBatchPostRequest batchRequest, int companyId);

        OrderBatchPostResponse GetAllInterCompanyTransactions(OrderBatchPostRequest batchRequest, int companyId);

        OrderBatchPostResponse GetAllMissingShipViaTransactions(OrderBatchPostRequest batchRequest, int companyId);

        OrderBatchPostResponse GetAllIncorrectTaxScheduleIdTranscations(OrderBatchPostRequest batchRequest, int companyId);

        object MovePostedInvoiceTiHistory(int companyId);

        OrderBatchPostResponse GetAllFailedPrePaymentTransactions(OrderBatchPostRequest batchRequest, int companyId);

        OrderBatchPostResponse GetAllLinkedPaymentIssuesTransactions(OrderBatchPostRequest batchRequest, int companyId);

    }
}
