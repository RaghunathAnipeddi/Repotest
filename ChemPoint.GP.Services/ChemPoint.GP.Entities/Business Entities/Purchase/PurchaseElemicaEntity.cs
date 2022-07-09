using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.Entities.Business_Entities.Purchase
{
    public class PurchaseElemicaEntity
    {
        public string ItemList { get; set; }

        public int LastSentTimeCount { get; set; }

        public DateTime LastSentDate { get; set; }

        public int IsValidToSendElemica { get; set; }
    }
}
