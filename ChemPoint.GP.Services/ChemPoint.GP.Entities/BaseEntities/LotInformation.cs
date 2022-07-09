using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class LotInformation : IModelBase
    {
        public string LotNumber { get; set; }

        public decimal LotQuantity { get; set; }

        public bool IsCoaSent { get; set; }
    }
}
