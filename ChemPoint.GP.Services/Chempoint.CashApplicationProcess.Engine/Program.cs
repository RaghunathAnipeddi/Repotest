using Chempoint.GP.Model.Interactions.Sales;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.CashApplicationProcess.Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            SalesOrderRequest soRequest = new SalesOrderRequest();

            string isNAEnabled = ConfigurationManager.AppSettings["IsNAEnabled"].Trim();
            string isEUEnabled = ConfigurationManager.AppSettings["IsEUEnabled"].Trim();

            if ((Convert.ToInt16(args[0]) == 1) && (Convert.ToInt16(isNAEnabled) == 1))
            {
                soRequest.Source = ConfigurationManager.AppSettings["CHMPTCompanyName"].Trim();
                soRequest.CompanyID = 1;
            }
            if ((Convert.ToInt16(args[0]) == 2) && (Convert.ToInt16(isEUEnabled) == 1))
            {
                soRequest.Source = ConfigurationManager.AppSettings["CPEURCompanyName"].Trim();
                soRequest.CompanyID = 2;
            }

            if (!String.IsNullOrEmpty(soRequest.Source.Trim()))
            {
                bool isStatus = false;
                string gpServiceConfigurationUrl = ConfigurationManager.AppSettings["serviceURL"].ToString();

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(10);
                    client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.PostAsJsonAsync("api/SalesOrderUpdate/ApplyToOpenOrders", soRequest); // we need to refer the web.api service url here.
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
}
