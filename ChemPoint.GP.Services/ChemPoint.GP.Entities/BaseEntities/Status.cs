using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class Status
    {
        public int Statuscode { get; set; }

        public string Description { get; set; }

        public string EffectiveDateRange { get; set; }

        public string StatusReason { get; set; }

        public string Instructions { get; set; }
    }
}
