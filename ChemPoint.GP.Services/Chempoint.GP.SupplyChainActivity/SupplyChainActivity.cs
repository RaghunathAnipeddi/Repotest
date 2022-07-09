using Chempoint.GP.Model.Interactions.Purchases;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.SupplyChainActivity
{
    public class SupplyChainActivity
    {
        static void Main(string[] args)
        {

            try
            {
                bool isStatus = false;

                string gpServiceConfigurationUrl = ConfigurationManager.AppSettings["XRMActivityURL"];

                PurchaseOrderRequest poActivityRequest = new PurchaseOrderRequest();
                poActivityRequest.CompanyID = Convert.ToInt32(ConfigurationManager.AppSettings["CompanyId"]);

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(10);
                    client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.PostAsJsonAsync("api/PurchaseOrder/CreateActivity", poActivityRequest); // we need to refer the web.api service url here.
                    if (response.Result.IsSuccessStatusCode)
                    {
                        PurchaseOrderResponse objPurchaseOrderResponse = response.Result.Content.ReadAsAsync<PurchaseOrderResponse>().Result;
                        if (objPurchaseOrderResponse.Status == ResponseStatus.Success)
                        {
                            isStatus = true;
                        }
                    }
                    else
                    {
                        System.IO.File.WriteAllText("supplychain.txt", "Service call failed");
                        isStatus = false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("supplychain.txt", ex.ToString());
            }
        }
    }
}

