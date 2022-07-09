using Chempoint.GP.Model.Interactions.FTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using System.Configuration;
using System.Runtime.Serialization;
namespace Chempoint.FTPutility
{
    [Serializable]
    [DataContract]
    public class Error
    {
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string ErrorReferenceCode { get; set; }
        [DataMember]
        public string ExceptionMessage { get; set; }
        [DataMember]
        public string StackTrace { get; set; }
    }

    class Program
    {

        static void Main(string[] args)
        {

            try
            {
                #region Upload files
                FTPRequest objFTPRequest = new FTPRequest();
                objFTPRequest.objFTPEntity = new ChemPoint.GP.Entities.Business_Entities.TaskScheduler.FTP.FTPEntity();
                objFTPRequest.objFTPEntity.AppName = ConfigurationManager.AppSettings["AppName"];
                objFTPRequest.objFTPEntity.BatchFileName = ConfigurationManager.AppSettings["BatchFileName"];
                objFTPRequest.objFTPEntity.LoggingFileName = ConfigurationManager.AppSettings["FtpLogFileName"];
                objFTPRequest.objFTPEntity.LoggingFilePath = ConfigurationManager.AppSettings["FtpLogPath"];
                var json = new JavaScriptSerializer().Serialize(objFTPRequest);

                HttpClient objHttpClient = new HttpClient();
                string baseUrl = ConfigurationManager.AppSettings["ServiceUrl"];
                objHttpClient.BaseAddress = new Uri(baseUrl);

                // Add an Accept header for JSON format.
                objHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string strServiceUrl = "api/FTP/FilesUploadToFtp";

                HttpResponseMessage objHttpResponseMessage = objHttpClient.PostAsJsonAsync(strServiceUrl, objFTPRequest).Result;
                if (objHttpResponseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine("Files are uploaded successfully");
                    Console.ReadKey();
                }
                else
                {
                    Error error = objHttpResponseMessage.Content.ReadAsAsync<Error>().Result;
                }

                #endregion


            }
            catch (Exception ex)
            {


            }
        }


    }
}
