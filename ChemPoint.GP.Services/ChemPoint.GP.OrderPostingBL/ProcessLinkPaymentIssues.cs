using Chempoint.GP.Model.Interactions.Email;
using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.BusinessContracts.Sales;
using ChemPoint.GP.DataContracts.Sales;
using ChemPoint.GP.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.OrderPostingBL
{
    public class ProcessLinkPaymentIssues : ISalesOrderPostBL
    {
        public OrderBatchPostResponse Process(OrderBatchPostRequest batchRequest)
        {
            OrderBatchPostResponse batchResponse = null;
            ISalesOrderPostRepository salesDataAcess = null;
            EmailBusiness emailBusiness = new EmailBusiness();
            SendEmailRequest emailRequest = new SendEmailRequest();
            batchResponse = new OrderBatchPostResponse();
            try
            {
                salesDataAcess = new ChemPoint.GP.OrderPostingDL.SalesPostingDL(batchRequest.ConnectionString);
                batchResponse = salesDataAcess.GetAllLinkedPaymentIssuesTransactions(batchRequest, batchRequest.CompanyID);
                if (!string.IsNullOrWhiteSpace(batchResponse.EmailContent))
                {
                    emailRequest = batchRequest.EmailRequest;
                    emailRequest.Report = batchResponse.EmailContent;
                    emailBusiness.SendEmail(emailRequest);
                }
                batchResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                batchResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                batchResponse.ErrorMessage = ex.Message.ToString();
            }
            return batchResponse;
        }
    }
}
