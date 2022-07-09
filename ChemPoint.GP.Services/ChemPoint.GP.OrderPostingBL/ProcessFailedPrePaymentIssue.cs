using Chempoint.GP.Model.Interactions.Email;
using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.BusinessContracts.Sales;
using ChemPoint.GP.DataContracts.Sales;
using ChemPoint.GP.Email;
using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.OrderPostingBL
{
    public class ProcessFailedPrePaymentIssue : ISalesOrderPostBL
    {
        public OrderBatchPostResponse Process(OrderBatchPostRequest batchRequest)
        {
            OrderBatchPostResponse batchResponse = null;
            ISalesOrderPostRepository salesDataAcess = null;
            batchResponse = new OrderBatchPostResponse();
            EmailBusiness emailBusiness = new EmailBusiness();
            SendEmailRequest emailRequest = new SendEmailRequest();
            try
            {
                salesDataAcess = new ChemPoint.GP.OrderPostingDL.SalesPostingDL(batchRequest.ConnectionString);
                batchResponse = salesDataAcess.GetAllFailedPrePaymentTransactions(batchRequest, batchRequest.CompanyID);
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


        
