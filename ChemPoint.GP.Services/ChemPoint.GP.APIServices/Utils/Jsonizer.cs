using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
//using ChemPoint.GP.Entities.Business_Entities;

namespace ChemPoint.GP.APIServices.Utils
{
    public class Jsonizer : IHttpActionResult
    {
        object _value;
        HttpRequestMessage _request;

        public Jsonizer(object value, HttpRequestMessage request)
        {
            _value = value;
            _request = request;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage()
            {
                Content = new StringContent(JsonConvert.SerializeObject(_value), Encoding.UTF8, "application/json"),
                RequestMessage = _request,
                StatusCode = HttpStatusCode.OK
            };

            return Task.FromResult(response);
        }
    }
}