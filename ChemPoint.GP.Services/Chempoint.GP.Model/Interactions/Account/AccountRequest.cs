using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Linq;

namespace Chempoint.GP.Model.Interactions.Account
{
    public class AccountRequest
    {
        public int CompanyID { get; set; }

        public string CompanyName { get; set; }

        public string AccountXml { get; set; }

        public string QuoteXml { get; set; }
       
        public CustomerInformation GPCustomerInformation { get; set; }

        public WarehouseInformation GPWarehouseInformation { get; set; }

        public Chempoint.BusinessObjects.CustomerInformation XrmCustomerInformation { get; set; }

        public Chempoint.BusinessObjects.QuoteInformation XrmQuoteInformation { get; set; }

        // Config details
        public string XrmServiceUrl { get; set; }

        public string LoggingPath { get; set; }

        public string LoggingFileName { get; set; }

        public EMailInformation AccountFailureEmail { get; set; }

        public EMailInformation AvalaraEmail { get; set; }

        public string NAConnectionString { get; set; }

        public string EUConnectionString { get; set; }

        public string EUEconnectConnectionString { get; set; }

        public string NAEconnectConnectionString { get; set; }

        public string AccountStyleSheetPath { get; set; }

        public string QuoteStyleSheetPath { get; set; }

        public string RemoveNamespaceStyleSheetPath { get; set; }

        public string AvalaraWebServiceUrl { get; set; }

        public string CurrencyId { get; set; }
    }
}
