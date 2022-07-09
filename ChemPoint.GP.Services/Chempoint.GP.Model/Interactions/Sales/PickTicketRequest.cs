using System;

namespace Chempoint.GP.Model.Interactions.Sales
{
    public class PickTicketRequest
    {
        public int CompanyID { get; set; }

        public string CompanyName { get; set; }

        public string UserID { get; set; }

        public string ConnectionString { get; set; }

        public int SopType { get; set; }

        public string SopNumber { get; set; }

        public string OrigNumber { get; set; }

        public string XrmServiceUrl { get; set; }

        public string WarehouseEdiServiceUrl { get; set; }

        public string RequestType { get; set; }

        public int OperationStatus { get; set; }

        public bool IsXmlWarehouseEnabled { get; set; }

        public bool IsEdiWarehouseEnabled { get; set; }

        public string WarehouseID { get; set; }

        public string StyleSheetPath { get; set; }

        public string LoggingPath { get; set; }

        public string LoggingFileName { get; set; }
    }
}
