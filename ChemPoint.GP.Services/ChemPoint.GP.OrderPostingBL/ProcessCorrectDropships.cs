using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.BusinessContracts.Sales;
using ChemPoint.GP.DataContracts.Sales;
using System;

namespace ChemPoint.GP.OrderPostingBL
{
    public class ProcessCorrectDropships : ISalesOrderPostBL
    {
        
        public OrderBatchPostResponse Process(OrderBatchPostRequest batchRequest)
        {
            OrderBatchPostResponse batchResponse = null;
            ISalesOrderPostRepository salesDataAcess = null;
            batchResponse = new OrderBatchPostResponse();
            try
            {
                salesDataAcess = new ChemPoint.GP.OrderPostingDL.SalesPostingDL(batchRequest.ConnectionString);
                batchResponse = salesDataAcess.GetAllDropshipTransactions(batchRequest, batchRequest.CompanyID);                
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
