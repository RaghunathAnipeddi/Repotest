using Chempoint.GP.Model.Interactions.Sales;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;

namespace ChemPoint.TransferToFO.Library
{
    class Program
    {
        static void Main(string[] args)
        {
            bool isStatus = false;
            string gpServiceConfigurationUrl = ConfigurationManager.AppSettings["serviceURL"].ToString();

            BulkOrderTransferRequest bulkOrderTransferRequest = new BulkOrderTransferRequest();
            bulkOrderTransferRequest.CompanyID = 1;

            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(10);
                client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = client.PostAsJsonAsync("api/SalesOrderUpdate/TransferOrderToFO", bulkOrderTransferRequest); // we need to refer the web.api service url here.
                if (response.Result.IsSuccessStatusCode)
                {
                    bool bulkOrderTransferResponse = response.Result.Content.ReadAsAsync<bool>().Result;
                    if (bulkOrderTransferResponse)
                    {
                        isStatus = true;
                    }
                }
                else
                {
                    isStatus = false;
                }
            }
            if (!isStatus)
            {
                // MessageBox.Show("Error: TransferToFO Engine");//, Resources.STR_MESSAGE_TITLE);
            }
        }
    }
}
