using Chempoint.GP.Model.Interactions.Account;
using Chempoint.GP.Model.Interactions.Sales;

namespace ChemPoint.GP.DataContracts.Account
{
    public interface IAccountRepository
    {
        object FetchAccountDetails(string customerId, int companyId);

        string GetCustomerOpenTransactionStatus(string customerId, int companyId);

        int GetWarehouseDeactivationStatus(AccountRequest aRequest);

        void DeactivateCustomerinGP(string customerId, int companyId);

        bool IsCustomerExistsInGP(string customerId, int companyId);

        object FetchAvalaraRequestDetails(string customerId, int companyId);

        object FetchCustomerDetailsToPushToAvalara(string source, string customerId, int companyId);

        void DeactivateQuoteinGP(string quoteId, string statusId, string statusReason, int companyId);

        bool GetCustomerIsServiceSKUEligible(SalesOrderRequest aRequest);

        
    }
}
