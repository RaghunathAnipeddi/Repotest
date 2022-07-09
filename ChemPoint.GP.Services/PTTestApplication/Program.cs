using Chempoint.GP.Model.Interactions.Sales;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PTTestApplication
{
    public class Program
    {
        static void Main(string[] args)
        {


            string gpServiceConfigurationUrl = "http://localhost:63466/";
            string invoice = "NA00474959";
            PickTicketRequest pickTicketRequest = new PickTicketRequest();
            pickTicketRequest.CompanyID = 1;
            pickTicketRequest.CompanyName = "Chmpt";
            pickTicketRequest.UserID = "sa";

            pickTicketRequest.SopType = 6;
            pickTicketRequest.SopNumber = invoice;
            pickTicketRequest.WarehouseEdiServiceUrl = "http://cs01services01/Shipmentintegration/";
            pickTicketRequest.RequestType = "Insert";


            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(gpServiceConfigurationUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = client.PostAsJsonAsync("api/PickTicket/PrintPickTicket", pickTicketRequest); // we need to refer the web.api service url here.
                if (response.Result.IsSuccessStatusCode)
                {
                    PickTicketResponse pickTicketResponse = response.Result.Content.ReadAsAsync<PickTicketResponse>().Result;
                    if (pickTicketResponse.Status == ResponseStatus.Success)
                    {
                    }
                    else
                    {
                    }
                }
                else
                {

                }
            }
        }
    }
}
