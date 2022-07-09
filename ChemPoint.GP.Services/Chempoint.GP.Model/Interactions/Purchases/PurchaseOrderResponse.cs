using ChemPoint.GP.Entities.BaseEntities;
using ChemPoint.GP.Entities.Business_Entities.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Model.Interactions.Purchases
{
    public class PurchaseOrderResponse
    {
        public ResponseStatus Status { get; set; }

        public string ErrorMessage { get; set; }

        public PurchaseOrderEntity PurchaseOrderEntity { get; set; }

        public List<PurchaseCostManagement> PurchaseCostMgt { get; set; }

        public List<Integration> XRMIntegrationList { get; set; }


        public List<PurchaseShipmentEstimateDetails> PurchaseShipmentEstimateList { get; set; }
        public PurchaseShipmentEstimateDetails PurchaseShipmentEstimateDetails { get; set; }

        #region LandedCost

        public List<PurchaseShipmentEstimateDetails> PurchaseShipmentEstimateLineList { get; set; }
        public List<PurchaseShipmentEstimateDetails> PurchaseShipmentEstimateHeaderList { get; set; }

        public List<PurchaseSelectPoLines> SelectPoHeaderList { get; set; }
        public List<PurchaseSelectPoLines> SelectPoLineList { get; set; }

        public List<PoEstimateId> PurchaseEstimatedId { get; set; }

        public List<PoList> PurchaseNumberLists { get; set; }

        public List<CurrencyDetails> CurrencyDetailLists { get; set; }
        #endregion

        public int IsAvailable { get; set; }
        public string Report { get; set; }
    }
}
