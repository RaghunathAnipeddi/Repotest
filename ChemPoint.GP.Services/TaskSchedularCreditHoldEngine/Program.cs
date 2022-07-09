using Chempoint.GP.Model.Interactions.HoldEngine;
using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.HoldEngine;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;

namespace TaskSchedularCreditHoldEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            HoldEngineRequest holdEngineRequest = null;
            HoldEngineEntity holdEngineEntity = null;
            bool isStatus = false;
                          
            holdEngineRequest = new HoldEngineRequest();
            holdEngineEntity = new HoldEngineEntity();
               
            holdEngineEntity.BatchUserID = "batch";
            holdEngineRequest.HoldEngineEntity = holdEngineEntity;

            #region PrcessHoldEngine
            
            using (HttpClient client = new HttpClient())
            {
                string service = ConfigurationManager.AppSettings["serviceURL"].ToString();
                client.BaseAddress = new Uri(service);
                client.Timeout = TimeSpan.FromMinutes(15);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                
                var response = client.PostAsJsonAsync("api/HoldEngine/ProcessCreditHoldEngine", holdEngineRequest); // we need to refer the web.api service url here.
                if (response.Result.IsSuccessStatusCode)
                {
                    var responseStatus = response.Result.Content.ReadAsAsync<HoldEngineResponse>().Result;
                    if (responseStatus.Status == HoldEngineResponseStatus.Success)
                    {
                        isStatus = true;
                    }
                    else
                    {
                        isStatus = false;
                    }
                }
                else
                {
                    isStatus = false;
                }
            }
            if (!isStatus)
            {
                // MessageBox.Show("Error: Updating ProcessHold Engine");//, Resources.STR_MESSAGE_TITLE);
            }
        
            #endregion PrcessHoldEngine
        }
    }
}
