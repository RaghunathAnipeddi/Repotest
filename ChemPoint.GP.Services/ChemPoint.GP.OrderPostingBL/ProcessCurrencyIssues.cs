using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.BusinessContracts.Sales;
using ChemPoint.GP.DataContracts.Sales;
using System;

namespace ChemPoint.GP.OrderPostingBL
{
    public class ProcessCurrencyIssues : ISalesOrderPostBL
    {
       
        public OrderBatchPostResponse Process(OrderBatchPostRequest batchRequest)
        {
            OrderBatchPostResponse batchResponse = null;
            ISalesOrderPostRepository salesDataAcess = null;
            batchResponse = new OrderBatchPostResponse();
            try
            {
                salesDataAcess = new ChemPoint.GP.OrderPostingDL.SalesPostingDL(batchRequest.ConnectionString);
                batchResponse = salesDataAcess.GetAllInvalidExchangeRateTransactions(batchRequest, batchRequest.CompanyID);
                batchResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                batchResponse.Status = ResponseStatus.Error;
                batchResponse.ErrorMessage = ex.Message.ToString();
               
            }

            return batchResponse;
        }
    }
}
