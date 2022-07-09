using System.Collections.Generic;
using System.Data;

namespace ChemPoint.GP.Entities.Business_Entities.Sales
{
    public class BulkOrderTransferEntity
    {
        public DataSet SalesOrderDetails { get; set; }

        public List<string> FipOrders { get; set; }

        public List<string> LockedOrders { get; set; }

        public List<string> UnlockedOrders { get; set; }
    }
}
