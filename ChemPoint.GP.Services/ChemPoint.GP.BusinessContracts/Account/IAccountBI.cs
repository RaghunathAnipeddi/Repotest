using Chempoint.GP.Model.Interactions.Account;
using Chempoint.GP.Model.Interactions.Sales;

namespace ChemPoint.GP.BusinessContracts.Account
{
    public interface IAccountBI
    {
        AccountResponse SaveCustomerDetails(AccountRequest aRequest);

        AccountResponse SaveQuoteDetailsIntoGP(AccountRequest aRequest);

        AccountResponse IsOpenTransactionExistsForCustomer(AccountRequest aRequest);

        AccountResponse GetWarehouseDeactivationStatus(AccountRequest aRequest);

        SalesOrderResponse GetCustomerIsServiceSKUEligible(SalesOrderRequest aRequest);

       

    }
}
