using ChemPoint.GP.Entities.BaseEntities;

namespace ChemPoint.GP.Entities.Business_Entities
{
    public class SalesOrderEntity : IModelBase
    {
        public int SopType { get; set; }

        public string SopNumber { get; set; }

        public int OrigType { get; set; }

        public string OrigNumber { get; set; }

        public string BilltoAddressId { get; set; }

        public string ShiptoAddressId { get; set; }

        public string CompanyName { get; set; }

        public SalesOrderInformation SalesOrderDetails { get; set; }
    }
}
