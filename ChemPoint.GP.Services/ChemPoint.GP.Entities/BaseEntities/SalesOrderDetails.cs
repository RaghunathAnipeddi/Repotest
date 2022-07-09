using System;
using System.Collections.Generic;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class SalesOrderDetails
    {
        public string CustomerPONumber { get; set; }

        public OrderHeader OrderHeader { get; set; }

        public SalesOrderType SalesOrderType { get; set; }

        public OrderSchedule OrderSchedule { get; set; }

        public List<SalesLineItem> LineItems { get; set; }

        public SalesLineItem LineItemDetails { get; set; }

        public Status Status { get; set; }

        public AuditInformation AuditInformation { get; set; }

        public string Instructions { get; set; }
    }
}
