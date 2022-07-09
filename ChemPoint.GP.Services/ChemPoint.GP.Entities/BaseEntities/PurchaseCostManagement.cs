using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class PurchaseCostManagement
    {
        public string PoNumber { get; set; }
        public string VendorId { get; set; }
        public int PoType { get; set; }
        public string PoLineStatus { get; set; }


        public int Ord { get; set; }

        public int LineNumber { get; set; }
        public string ItemNumber { get; set; }
        public string UOfM { get; set; }
        public decimal UnitCost { get; set; }
        public decimal ProposedUnitCost { get; set; }
        public int CostStatus { get; set; }
        public string CostSupportId { get; set; }
        public string CostBookCost { get; set; }
        public decimal QtyOrder { get; set; }
        public decimal QtyCancel { get; set; }
        public int HasCostVariance { get; set; }
        public string CostNotes { get; set; }
        public string LineCostNotes { get; set; }
        public decimal NoteIndex { get; set; }
        public int CurrencyIndex { get; set; }
        public string UserId { get; set; }
        public int IsUpdated { get; set; }
        public bool IsDeleted { get; set; }

        public DateTime ModifiedOn { get; set; }
        public string Reason { get; set; }

        public string ItemDescription { get; set; }
        public decimal CostSupportCost { get; set; }
        public decimal PoCostVariance { get; set; }
        //public string PoReasonCode { get; set; }
        public string PoLineCostSource { get; set; }

        public bool IsFirstPoForMaterialMgt { get; set; }
        public bool IsValidForMaterialMgt { get; set; }
        public IEnumerable<string>PoCostNotesHistory { get; set; }
        public List<string> strPoCostNotesHistory { get; set; }
    }
}
