using Chempoint.GP.Model.Interactions.Sales;

namespace ChemPoint.GP.BusinessContracts.Sales
{
    public interface ISalesOrderCreateBI
    {
        SalesOrderResponse CreateSalesOrder(SalesOrderRequest salesOrderRequest);
    }
}

