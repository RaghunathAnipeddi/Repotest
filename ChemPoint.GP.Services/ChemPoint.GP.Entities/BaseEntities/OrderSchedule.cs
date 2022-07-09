using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class OrderSchedule : IModelBase
    {
        public DateTime OrderCreatedDate { get; set; }

        public DateTime RequestedShipDate { get; set; }

        public DateTime RequestedDeliveryDate { get; set; }
        
        public DateTime CustomerRequestedDeliveryDate { get; set; }

        public DateTime CustomerRequestedShipDate { get; set; }

        public DateTime OriginalRequestedDeliveryDate { get; set; }

        public DateTime OriginalRequestedShipDate { get; set; }

        public DateTime OriginalCustomerRequestedDeliveryDate { get; set; }
    }
}
