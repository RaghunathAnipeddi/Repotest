using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class SalesOrderType : IModelBase
    {
        public int IsInternational { get; set; }

        public int IsNaFta { get; set; }

        public int IsDropship { get; set; }

        public int IsBulk { get; set; }

        public int IsI3Order { get; set; }

        public int IsCreditCard { get; set; }

        public int IsCorrective { get; set; }

        public int IsFullTruckLoad { get; set; }

        public int IsConsignment { get; set; }

        public int IsCorrectiveBoo { get; set; }

        public int IsSampleOrder { get; set; }

        public int IsTempControl { get; set; }

        public int IsHazmat { get; set; }

        public int IsFreezeProtect { get; set; }

        public int IsAutoPTEligible { get; set; }

        public int IsCreditEnginePassed { get; set; }

        public int IsTaxEnginePassed { get; set; }
        
        public AuditInformation AuditInformation { get; set; }

        public string Instructions { get; set; }
    }
}
