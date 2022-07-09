using Chempoint.GP.Model.Interactions.Sales;

namespace ChemPoint.GP.BusinessContracts.Sales
{
    public interface ISalesOrderPostBL
    {
        OrderBatchPostResponse Process(OrderBatchPostRequest salesOrderRequest);
    }
}
