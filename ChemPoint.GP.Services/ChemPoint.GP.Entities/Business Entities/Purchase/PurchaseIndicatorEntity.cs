using ChemPoint.GP.Entities.BaseEntities;
using System;

namespace ChemPoint.GP.Entities.Business_Entities.Purchase
{
    public class PurchaseIndicatorEntity : IModelBase
    {
        public string PoNumber { get; set; }

        public int POLineNumber { get; set; }

        public string ItemNumber { get; set; }

        public short POIndicatorStatusId { get; set; }

        public short BackOrderReason { get; set; }

        public DateTime InitialBackOrderDate { get; set; }

        public short CancelledReason { get; set; }

        public DateTime InitialCancelledDate { get; set; }

        public bool IsCostVariance { get; set; }

        public DateTime AcknowledgementDate { get; set; }

        public DateTime ConfirmedDate { get; set; }

        public DateTime ActualShipDate { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public int ModifiedBy { get; set; }

        public DateTime ModifiedOn { get; set; }
    }
}
