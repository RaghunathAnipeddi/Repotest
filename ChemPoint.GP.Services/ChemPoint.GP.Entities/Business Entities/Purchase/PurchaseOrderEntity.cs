using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.Entities.Business_Entities.Purchase
{
    public class PurchaseOrderEntity
    {
        public string PoNumber { get; set; }

        public int POLineNumber { get; set; }

        public string VendorId { get; set; }

        public int PoType { get; set; }

        public string PoLineStatus { get; set; }

        public PurchaseCostManagement PurchaseCostMgtInformation { get; set; }

        //public System.Data.DataTable CostBookPriceModifiedList { get; set; }
        public PurchaseShipmentEstimateDetails PurchaseShipmentEstimateDetails { get; set; }

        public PurchaseOrderInformation PurchaseOrderDetails { get; set; }
    }
}
