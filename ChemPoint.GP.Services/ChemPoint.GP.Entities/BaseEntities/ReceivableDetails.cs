using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class ReceivableDetails : IModelBase
    {
        public string ApplyFromDocumentNumber { get; set; }

        public int ApplyFromDocumentTypeId { get; set; }

        public string ApplyToDocumentNumber { get; set; }

        public int ApplyToDocumentType { get; set; }

        public string ApplyToCustomerId { get; set; }

        public string DocumentCurrencyId { get; set; }

        public DateTime ApplyDate { get; set; }

        public Amount Amount { get; set; }

        public int OrderLineId { get; set; }

        public string ItemNumber { get; set; }

        public string ItemDescription { get; set; }

        public decimal NetWeight { get; set; }

        public decimal ShipWeight { get; set; }

        public decimal OrderedQuantity { get; set; }

        public decimal UnitPriceAmount { get; set; }

        public decimal LineTotalAmount { get; set; }

        public bool IsSelected { get; set; }

        public string SupportId { get; set; }

        public string IncidentId { get; set; }

        public int TypeId { get; set; }

        public string CommentText { get; set; }

        public string DocumentStatus { get; set; }

        public int StatusId { get; set; }

        public int SopTypeDatabase { get; set; }
        
    }
}
