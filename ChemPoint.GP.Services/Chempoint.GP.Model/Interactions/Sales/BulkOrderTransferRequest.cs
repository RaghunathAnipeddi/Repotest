using ChemPoint.GP.Entities.BaseEntities;

namespace Chempoint.GP.Model.Interactions.Sales
{
    public class BulkOrderTransferRequest
    {
        public int CompanyID { get; set; }

        public string ConnectionString { get; set; }
        
        public string StyleSheetPath { get; set; }

        public string XrmServiceUrl { get; set; }

        public string PickTicketServiceUrl { get; set; }

        public string WarehouseEdiServiceUrl { get; set; }

        public string PickTicketStyleSheetPath { get; set; }

        public string NAEconnectConnectionString { get; set; }

        public string LoggingPath { get; set; }

        public string LoggingFileName { get; set; }

        public EMailInformation WarehouseSameDayEmail { get; set; }

        public EMailInformation TransferFailureEmail { get; set; }
    }
}
