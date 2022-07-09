using ChemPoint.GP.Entities.Business_Entities.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Model.Interactions.Purchases
{
    public class PurchaseElemicaResponse
    {
        public ResponseStatus Status { get; set; }

        public string ErrorMessage { get; set; }

        public PurchaseOrderEntity purchaseOrderDetails { get; set; } 
    }
}
