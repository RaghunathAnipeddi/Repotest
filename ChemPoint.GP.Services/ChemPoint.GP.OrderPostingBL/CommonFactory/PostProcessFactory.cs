using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.BusinessContracts.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChemPoint.GP.OrderPostingBL
{
    /// <summary>
    /// Factory class
    /// </summary>
    public class PostProcessFactory : ISalesOrderPostBL
    {
        /// <summary>
        /// Factory method to fetch the corresponding class method based on the operation type passed.
        /// </summary>
        /// <param name="operation">Type of operation performed</param>
        /// <returns></returns>
        //public static ISalesOrderPostBI PostOperationInstanceFactory(OperationType operation)
        public OrderBatchPostResponse Process(OrderBatchPostRequest batchRequest)
        {
            OrderBatchPostResponse batchResponse = new OrderBatchPostResponse();
            try
            {
                switch (batchRequest.OperationType)
                {
                    case OrderPostOperationType.BatchValidate:
                        return new BatchValidateBL().Process(batchRequest);
                    case OrderPostOperationType.RemoveLockedUsers:
                        return new ProcessRemoveLockedUsers().Process(batchRequest);                         
                    case OrderPostOperationType.IntercompanySalesTransactions:
                        return new ProcessInterCompanyOrders().Process(batchRequest);                         
                    case OrderPostOperationType.CurrencyIssues:
                        return new ProcessCurrencyIssues().Process(batchRequest);                         
                    case OrderPostOperationType.VatIssues:
                        return new ProcessVatIssues().Process(batchRequest);                         
                    case OrderPostOperationType.CorrectDropships:
                        return new ProcessCorrectDropships().Process(batchRequest);                         
                    case OrderPostOperationType.ProcessCreditCards:
                        return new ProcessCreditCards().Process(batchRequest);                         
                    case OrderPostOperationType.DistributionIssues:
                        return new ProcessDistributionIssues().Process(batchRequest);                         
                    case OrderPostOperationType.PostTransactions:
                        return new ProcessPostTransactions().Process(batchRequest);                         
                    case OrderPostOperationType.UnlockPostingBatch:
                         return new UnlockPostingBatch().Process(batchRequest);                         
                    case OrderPostOperationType.LockPostingBatch:
                        return new LockPostingBatch().Process(batchRequest);                         
                    case OrderPostOperationType.SsrsReport:
                        return new ProcessSsrsReport().Process(batchRequest);                         
                    case OrderPostOperationType.CadTaxExemptions:
                        return new ProcessCadTaxExemptions().Process(batchRequest);                         
                    case OrderPostOperationType.MissingShipVia:
                        return new ProcessMissingShipViaIssues().Process(batchRequest);                         
                    case OrderPostOperationType.CanadianTaxIssue:
                        return new ProcessCanadianTaxIssuesFix().Process(batchRequest);
                    case OrderPostOperationType.ServiceSkuIssue:
                        return new ProcessServiceSkuIssues().Process(batchRequest);
                    case OrderPostOperationType.DropShipIssues:
                        return new ProcessDropshipIssues().Process(batchRequest);
                    case OrderPostOperationType.LinkedPayment:
                        return new ProcessLinkPaymentIssues().Process(batchRequest);
                    case OrderPostOperationType.FailedPrepayment:
                        return new ProcessFailedPrePaymentIssue().Process(batchRequest); 
                    default:
                         batchResponse.Status = ResponseStatus.Error;
                        return batchResponse;                        
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
