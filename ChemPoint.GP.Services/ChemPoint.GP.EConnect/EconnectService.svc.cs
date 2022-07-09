using Chempoint.GP.Model.Interactions.Sales;
using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace ChemPoint.GP.EConnect
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class EConnectService : IEconnectService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eComOrderXml"></param>
        /// <returns></returns>
        public bool ProcessOrderMessage(string eComOrderXml, int iterationNumber)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                SalesOrderRequest request = new SalesOrderRequest();
                logMessage.AppendLine("EcomOrderXML :" + eComOrderXml);
                request.eComXML = eComOrderXml;

                using (HttpClient client = new HttpClient())
                {
                    logMessage.AppendLine("Service URL :" + ConfigurationManager.AppSettings["APIService"].ToString().Trim());
                    client.BaseAddress = new Uri(ConfigurationManager.AppSettings["APIService"].ToString().Trim());
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    logMessage.AppendLine(DateTime.Now.ToString() + " - API Service Call Start ");
                    var response = client.PostAsJsonAsync("api/SalesOrderCreate/CreateSalesOrder", request); // we need to refer the web.api service url here.
                    if (response.Result.IsSuccessStatusCode)
                    {
                        SalesOrderResponse salesOrderResponse = response.Result.Content.ReadAsAsync<SalesOrderResponse>().Result;
                        if (salesOrderResponse.Status == ResponseStatus.Success)
                        {
                            logMessage.AppendLine(DateTime.Now.ToString() + " - API Service Called Successfully ");
                        }
                    }
                    else
                    {
                        logMessage.AppendLine(DateTime.Now.ToString() + " - " + response.Result.ReasonPhrase.ToString());
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Some Problem in the API Service");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception eConex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + eConex.StackTrace);
                throw eConex;
            }
            finally
            {
                logMessage.AppendLine(DateTime.Now.ToString() + "API Service Call End ");
                LogInformationIntoFile(logMessage.ToString());
            }
        }

        internal void LogInformationIntoFile(string message)
        {
            string strPath;
            string strLogFileName;
            try
            {
                // Get the file name from the configuration xml.
                strLogFileName = "SalesOrder" + DateTime.Now.ToString("MM_dd_yy") + ".log";
                strPath = "D:/ChemPoint/logging/BizTalk/EcconnectService/";
                if (!Directory.Exists(strPath))
                    Directory.CreateDirectory(strPath);
                strPath = strPath + strLogFileName;

                if (!File.Exists(strPath))
                    File.Create(strPath).Close();

                // Log the details into the file.
                using (StreamWriter w = File.AppendText(strPath))
                {
                    w.WriteLine(message);
                    w.Flush();
                    w.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
