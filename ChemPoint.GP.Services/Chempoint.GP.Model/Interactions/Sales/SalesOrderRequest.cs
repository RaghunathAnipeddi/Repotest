using ChemPoint.GP.Entities.Business_Entities;
using System.Data.SqlClient;
using ChemPoint.GP.Entities.BaseEntities;
using System.Collections.Generic;

namespace Chempoint.GP.Model.Interactions.Sales
{
    public class SalesOrderRequest
    {
        public int CompanyID { get; set; }

        public string UserID { get; set; }

        public string ConnectionString { get; set; }

        public SqlConnection Connection { get; set; }

        public string FormName { get; set; }

        public string eComXML { get; set; }

        public string eConnectXML { get; set; }

        public AuditInformation AuditInformation { get; set; }

        public SalesOrderEntity SalesOrderEntity { get; set; }

        public bool IsHoldEngineRun { get; set; }

        public bool IsSopStatusExecute { get; set; }

        public string Source { get; set; }

        public string XrmServiceUrl { get; set; }

        public string LoggingPath { get; set; }

        public string LoggingFileName { get; set; }

        public EMailInformation SalesOrderFailureEmail { get; set; }

        public EMailInformation SalesPriorityOrdersEmail { get; set; }

        public string NAConnectionString { get; set; }

        public string EUConnectionString { get; set; }

        public string EUEconnectConnectionString { get; set; }

        public string NAEconnectConnectionString { get; set; }

        public string StyleSheetPath { get; set; }

        public string MailStyleSheetPath { get; set; }

        public string PickticketWarehouseEdiServiceUrl { get; set; }

        public decimal TermHoldRemainingBalance { get; set; }

        public bool IsCustomizeServiceSkUs { get; set; }

        

    }
}
