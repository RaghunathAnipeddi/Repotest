using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class TrackingInformation : IModelBase
    {
        public int SopType { get; set; }

        public string SopNumber { get; set; }

        public int LineItemSequence { get; set; }

        public int ComponentSequenceNumber { get; set; }

        public string TrackingNumber { get; set; }
    }
}
