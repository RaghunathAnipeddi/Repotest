using Chempoint.GP.Model.Interactions.Account;
using ChemPoint.GP.AccountBL;
using ChemPoint.GP.ApiServices.Controllers.Sales;
using ChemPoint.GP.BusinessContracts.Account;
using System.Web.Http;
using System.Configuration;
using Chempoint.GP.Infrastructure.Extensions;
using Chempoint.GP.Model.Interactions.Sales;
using System;
using Chempoint.GP.Infrastructure.Exceptions;
using ChemPoint.GP.APIServices.Utils;

namespace ChemPoint.GP.ApiServices.Controllers.Account
{
    public class CustomerController : BaseApiController
    {
        private IAccountBI accountService = null;

        public CustomerController()
        {
            accountService = new AccountBusiness();
        }

        /// <summary>
        /// Service SKU eligible For the customer 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public IHttpActionResult GetCustomerIsServiceSKUEligible(SalesOrderRequest request)
        {
            string errorMessage = string.Empty;

            if (request.IsInValid())
                return InternalServerError(new Exception("GetCustomerIsServiceSKUEligible Request is null"));

            if (request.CompanyID == 1)
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            else
                request.ConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;

            return DoExecute<SalesException>(() =>
            {
                var result = accountService.GetCustomerIsServiceSKUEligible(request);

                if (result.IsValid())
                    return new Jsonizer(result, Request);
                else
                    return NotFound();
            });
        }

        [HttpGet]
        [HttpPost]
        public string SaveAccountInGP(Chempoint.BusinessObjects.CustomerInformation xrmCustomerInformation)
        {
            string errorMessage = string.Empty;

            if (xrmCustomerInformation.IsInValid())
            {
                errorMessage = "Account request is null";
                return errorMessage;
            }

            AccountRequest request = new AccountRequest();
            request.XrmCustomerInformation = xrmCustomerInformation;

            //fill config detals
            request.XrmServiceUrl = ConfigurationManager.AppSettings["XrmServiceURL"].ToString();
            request.LoggingPath = ConfigurationManager.AppSettings["CustomerPushloggingPath"].ToString();
            request.LoggingFileName = ConfigurationManager.AppSettings["CustomerPushloggingFileName"].ToString();
            request.NAConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            request.NAEconnectConnectionString = ConfigurationManager.ConnectionStrings["CHMPTEconnectConString"].ConnectionString;
            request.EUConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;
            request.EUEconnectConnectionString = ConfigurationManager.ConnectionStrings["CPEUREconnectConString"].ConnectionString;
            request.AccountStyleSheetPath = ConfigurationManager.AppSettings["CustomerPushXSLTPath"].ToString();
            request.QuoteStyleSheetPath = ConfigurationManager.AppSettings["QuoteXSLTPath"].ToString();
            request.RemoveNamespaceStyleSheetPath = ConfigurationManager.AppSettings["RemoveNamespaceXSLTPath"].ToString();
            request.AvalaraEmail = new ChemPoint.GP.Entities.BaseEntities.EMailInformation();
            request.AvalaraEmail.EmailFrom = ConfigurationManager.AppSettings["CustomerPushAvalaraEmailFrom"].ToString();
            request.AvalaraEmail.EmailTo = ConfigurationManager.AppSettings["CustomerPushAvalarafailureEmailTo"].ToString();
            request.AvalaraEmail.EmailCc = ConfigurationManager.AppSettings["CustomerPushAvalarafailureEmailCC"].ToString();
            request.AvalaraEmail.Subject = ConfigurationManager.AppSettings["CustomerPushAvalarafailureEmailSubject"].ToString();
            request.AvalaraEmail.SmtpAddress = ConfigurationManager.AppSettings["SmtpAddress"].ToString();
            request.AvalaraEmail.UserId = ConfigurationManager.AppSettings["EmailUserId"].ToString();
            request.AvalaraEmail.Password = ConfigurationManager.AppSettings["EmailPassword"].ToString();

            request.AccountFailureEmail = new ChemPoint.GP.Entities.BaseEntities.EMailInformation();
            request.AccountFailureEmail.EmailFrom = ConfigurationManager.AppSettings["CustomerPushfailureEmailFrom"].ToString();
            request.AccountFailureEmail.EmailTo = ConfigurationManager.AppSettings["CustomerPushfailureEmailTo"].ToString();
            request.AccountFailureEmail.EmailCc = ConfigurationManager.AppSettings["CustomerPushfailureEmailCC"].ToString();
            request.AccountFailureEmail.Subject = ConfigurationManager.AppSettings["CustomerPushfailureEmailSubject"].ToString();
            request.AccountFailureEmail.SmtpAddress = ConfigurationManager.AppSettings["SmtpAddress"].ToString();
            request.AccountFailureEmail.UserId = ConfigurationManager.AppSettings["EmailUserId"].ToString();
            request.AccountFailureEmail.Password = ConfigurationManager.AppSettings["EmailPassword"].ToString();
            request.AvalaraWebServiceUrl = ConfigurationManager.AppSettings["AvalaraServiceURL"].ToString();

            AccountResponse response = accountService.SaveCustomerDetails(request);
            errorMessage = response.ErrorMessage;

            return errorMessage;
        }

        [HttpGet]
        [HttpPost]
        public string SaveQuoteInGP(Chempoint.BusinessObjects.QuoteInformation xrmQuoteInformation)
        {
            string errorMessage = string.Empty;

            if (xrmQuoteInformation.IsInValid())
            {
                errorMessage = "Account request is null";
                return errorMessage;
            }

            AccountRequest request = new AccountRequest();
            request.XrmQuoteInformation = xrmQuoteInformation;

            //fill config detals
            request.XrmServiceUrl = ConfigurationManager.AppSettings["XrmServiceURL"].ToString();
            request.LoggingPath = ConfigurationManager.AppSettings["CustomerPushloggingPath"].ToString();
            request.LoggingFileName = ConfigurationManager.AppSettings["CustomerPushloggingFileName"].ToString();
            request.NAConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            request.NAEconnectConnectionString = ConfigurationManager.ConnectionStrings["CHMPTEconnectConString"].ConnectionString;
            request.EUConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;
            request.EUEconnectConnectionString = ConfigurationManager.ConnectionStrings["CPEUREconnectConString"].ConnectionString;
            request.AccountStyleSheetPath = ConfigurationManager.AppSettings["CustomerPushXSLTPath"].ToString();
            request.QuoteStyleSheetPath = ConfigurationManager.AppSettings["QuoteXSLTPath"].ToString();
            request.RemoveNamespaceStyleSheetPath = ConfigurationManager.AppSettings["RemoveNamespaceXSLTPath"].ToString();

            request.AvalaraEmail = new ChemPoint.GP.Entities.BaseEntities.EMailInformation();
            request.AvalaraEmail.EmailFrom = ConfigurationManager.AppSettings["CustomerPushAvalaraEmailFrom"].ToString();
            request.AvalaraEmail.EmailTo = ConfigurationManager.AppSettings["CustomerPushAvalarafailureEmailTo"].ToString();
            request.AvalaraEmail.EmailCc = ConfigurationManager.AppSettings["CustomerPushAvalarafailureEmailCC"].ToString();
            request.AvalaraEmail.Subject = ConfigurationManager.AppSettings["CustomerPushAvalarafailureEmailSubject"].ToString();
            request.AvalaraEmail.SmtpAddress = ConfigurationManager.AppSettings["SmtpAddress"].ToString();
            request.AvalaraEmail.UserId = ConfigurationManager.AppSettings["EmailUserId"].ToString();
            request.AvalaraEmail.Password = ConfigurationManager.AppSettings["EmailPassword"].ToString();

            request.AccountFailureEmail = new ChemPoint.GP.Entities.BaseEntities.EMailInformation();
            request.AccountFailureEmail.EmailFrom = ConfigurationManager.AppSettings["CustomerPushfailureEmailFrom"].ToString();
            request.AccountFailureEmail.EmailTo = ConfigurationManager.AppSettings["CustomerPushfailureEmailTo"].ToString();
            request.AccountFailureEmail.EmailCc = ConfigurationManager.AppSettings["CustomerPushfailureEmailCC"].ToString();
            request.AccountFailureEmail.Subject = ConfigurationManager.AppSettings["CustomerPushfailureEmailSubject"].ToString();
            request.AccountFailureEmail.SmtpAddress = ConfigurationManager.AppSettings["SmtpAddress"].ToString();
            request.AccountFailureEmail.UserId = ConfigurationManager.AppSettings["EmailUserId"].ToString();
            request.AccountFailureEmail.Password = ConfigurationManager.AppSettings["EmailPassword"].ToString();
            request.AvalaraWebServiceUrl = ConfigurationManager.AppSettings["AvalaraServiceURL"].ToString();

            AccountResponse response = accountService.SaveQuoteDetailsIntoGP(request);
            errorMessage = response.ErrorMessage;

            return errorMessage;
        }

        [HttpGet]
        [HttpPost]
        public bool IsCustomerHasOpenTransactionInGP(string customerId, string region)
        {

            AccountRequest request = new AccountRequest();
            ChemPoint.GP.Entities.BaseEntities.CustomerInformation customerInformation = new ChemPoint.GP.Entities.BaseEntities.CustomerInformation();
            customerInformation.CustomerId = customerId;
            request.GPCustomerInformation = customerInformation;

            if (region.ToLower() == "north america")
                request.CompanyID = 1;
            else
                request.CompanyID = 2;

            //fill config detals
            request.XrmServiceUrl = ConfigurationManager.AppSettings["XrmServiceURL"].ToString();
            request.LoggingPath = ConfigurationManager.AppSettings["CustomerPushloggingPath"].ToString();
            request.LoggingFileName = ConfigurationManager.AppSettings["CustomerPushloggingFileName"].ToString();
            request.NAConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            request.NAEconnectConnectionString = ConfigurationManager.ConnectionStrings["CHMPTEconnectConString"].ConnectionString;
            request.EUConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;
            request.EUEconnectConnectionString = ConfigurationManager.ConnectionStrings["CPEUREconnectConString"].ConnectionString;

            AccountResponse response = accountService.IsOpenTransactionExistsForCustomer(request);
            if (Chempoint.GP.Model.Interactions.Account.ResponseStatus.Success == response.Status)
                return true;
            else
                return false;

        }

        [HttpGet]
        [HttpPost]
        /// <summary>
        /// Validate the wareshouse is having any open transaction or not
        /// </summary>
        public bool GetWarehouseDeactivationStatus(string warehouseId, string currencyId, string region)
        {
            AccountRequest request = new AccountRequest();
            ChemPoint.GP.Entities.BaseEntities.WarehouseInformation warehouseInformation = new ChemPoint.GP.Entities.BaseEntities.WarehouseInformation();
            warehouseInformation.WarehouseId = warehouseId;

            request.GPWarehouseInformation = warehouseInformation;
            request.CurrencyId = currencyId;

            if (region.ToLower() == "north america")
                request.CompanyID = 1;
            else
                request.CompanyID = 2;

            request.XrmServiceUrl = ConfigurationManager.AppSettings["XrmServiceURL"].ToString();
            request.LoggingPath = ConfigurationManager.AppSettings["CustomerPushloggingPath"].ToString();
            request.LoggingFileName = ConfigurationManager.AppSettings["CustomerPushloggingFileName"].ToString();
            request.NAConnectionString = ConfigurationManager.ConnectionStrings["CHMPTConString"].ConnectionString;
            request.NAEconnectConnectionString = ConfigurationManager.ConnectionStrings["CHMPTEconnectConString"].ConnectionString;
            request.EUConnectionString = ConfigurationManager.ConnectionStrings["CPEURConString"].ConnectionString;
            request.EUEconnectConnectionString = ConfigurationManager.ConnectionStrings["CPEUREconnectConString"].ConnectionString;

            AccountResponse response = accountService.GetWarehouseDeactivationStatus(request);
            if (Chempoint.GP.Model.Interactions.Account.ResponseStatus.Success == response.Status)
                return true;
            else
                return false;
        }
    }
}
