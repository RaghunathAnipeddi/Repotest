using ChemPoint.GP.Entities.Business_Entities.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Model.Interactions.Purchases
{
    public class PurchaseElemicaRequest
    {
        public int companyId { get; set; }

        public string userId { get; set; }

        public string connnectionString { get; set; }

        public PurchaseOrderEntity purchaseEntityDetails { get; set; }  
    }
}
