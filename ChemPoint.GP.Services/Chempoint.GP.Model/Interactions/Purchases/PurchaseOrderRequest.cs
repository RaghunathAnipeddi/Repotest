using ChemPoint.GP.Entities.BaseEntities;
using ChemPoint.GP.Entities.Business_Entities.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Model.Interactions.Purchases
{
    public class PurchaseOrderRequest
    {
        public int CurrencyView { get; set; }

        public int CompanyID { get; set; }

        public string UserId { get; set; }

        public string ConnectionString { get; set; }

        public List<PurchaseCostManagement> PurchaseCostMgt { get; set; }

        public PurchaseOrderEntity PurchaseOrderEntity { get; set; }

        public List<Integration> XRMIntegrationList { get; set; }

        public Integration XRMIntegration { get; set; }       

        public string UpdatePoCostDataTableString { get; set; }

        public string CrmActivityConfigurationURL { get; set; }

        public string LoggingPath { get; set; }

        public string LoggingFileName { get; set; }

        public List<POActivityLog> POActivityLogList { get; set; }

        public List<PurchaseShipmentEstimateDetails> PurchaseShipmentEstimateList { get; set; }

        public PurchaseShipmentEstimateDetails PurchaseShipmentEstimateDetails { get; set; }

        public AuditInformation AuditInformation { get; set; }

        public PurchaseOrderInformation PurchaseOrderInformation { get; set; }
        public EMailInformation emailInfomation { get; set; }
        public string Report { get; set; }
        public int AppConfigID { get; set; }
    }
}
