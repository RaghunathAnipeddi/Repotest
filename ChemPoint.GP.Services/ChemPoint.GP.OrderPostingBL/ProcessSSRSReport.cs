using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.BusinessContracts.Sales;
using System;
using System.ServiceModel;

namespace ChemPoint.GP.OrderPostingBL
{
    public class ProcessSsrsReport : ISalesOrderPostBL
    {
        public OrderBatchPostResponse Process(OrderBatchPostRequest batchRequest)
        {
            OrderBatchPostResponse batchResponse = null;
            batchResponse = new OrderBatchPostResponse();
            try
            {
                BasicHttpBinding binding = new BasicHttpBinding();
                binding.MaxReceivedMessageSize = Int32.MaxValue;
                binding.MaxBufferSize = Int32.MaxValue;
                binding.CloseTimeout = TimeSpan.FromMinutes(30);
                binding.OpenTimeout = TimeSpan.FromMinutes(30);
                binding.ReceiveTimeout = TimeSpan.FromMinutes(30);
                ReportServiceReference.ReportServiceClient reportServiceClient = new ReportServiceReference.ReportServiceClient(binding, new EndpointAddress(batchRequest.OpenOrdersSsrsReportUrl));
                reportServiceClient.InnerChannel.OperationTimeout = TimeSpan.FromMinutes(30);

                if (batchRequest.CompanyID == 1)
                {
                    batchResponse.SsrsReportLink = reportServiceClient.GetAllUnpostedOrder("NA", "");
                }
                else
                {
                    batchResponse.SsrsReportLink = reportServiceClient.GetAllUnpostedOrder("EU", "");
                }
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
