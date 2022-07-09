using System;
using System.Web.Http;
using Chempoint.GP.Infrastructure.Exceptions;
using Chempoint.GP.Infrastructure.Constants;
using Chempoint.GP.Infrastructure.Logging;

namespace ChemPoint.GP.ApiServices.Controllers.Sales
{
    public abstract class BaseApiController : ApiController
    {
        protected IHttpActionResult DoExecute<TException>(Func<IHttpActionResult> action)
            where TException : ExceptionBase
        {
            try
            {
                return action.Invoke();
            }
            catch (Exception exception)
            {
                LogException(exception);
                return Ok("failure");
            }
        }

        protected void LogException(Exception exception)
        {
            LogException(exception, ApplicationConstants.ExceptionPolicy);
        }

        protected void LogException(Exception exception, string policy)
        {
            ExceptionLogger.LogException(exception, policy);
        }
    }
}
