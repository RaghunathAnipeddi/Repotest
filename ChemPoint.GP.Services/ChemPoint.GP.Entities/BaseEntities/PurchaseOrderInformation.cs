using ChemPoint.GP.Entities.Business_Entities.Purchase;
using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class PurchaseOrderInformation
    {
        public string PoNumber { get; set; }

        public int PoLineNumber { get; set; }

        public string VendorId { get; set; }

        public PurchaseElemicaEntity PoElemicaDetails { get; set; }
    }
}
