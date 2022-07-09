using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Sales
{
    public class SalesOrderScheduleMap : BaseDataTableMap<OrderSchedule>
    {
        public override OrderSchedule Map(DataRow dr)
        {
            OrderSchedule orderSchedule = new OrderSchedule();
            DateTime dateValue;
            
            DateTime.TryParse(dr["OrigCustReqDelDate"].ToString(), out dateValue);
            orderSchedule.OriginalCustomerRequestedDeliveryDate = dateValue;
            DateTime.TryParse(dr["OrigSchedShipDate"].ToString(), out dateValue);
            orderSchedule.OriginalRequestedShipDate = dateValue;
            DateTime.TryParse(dr["OrigSchedDelDate"].ToString(), out dateValue);
            orderSchedule.OriginalRequestedDeliveryDate = dateValue;
            DateTime.TryParse(dr["CurSchedShipDate"].ToString(), out dateValue);
            orderSchedule.RequestedShipDate = dateValue;
            DateTime.TryParse(dr["CurSchedDelDate"].ToString(), out dateValue);
            orderSchedule.RequestedDeliveryDate = dateValue;
            DateTime.TryParse(dr["CurCustReqDelDate"].ToString(), out dateValue);
            orderSchedule.CustomerRequestedDeliveryDate = dateValue;
            DateTime.TryParse(dr["CurCustReqShipDate"].ToString(), out dateValue);
            orderSchedule.CustomerRequestedShipDate = dateValue;
            
            return orderSchedule;
        }
    }
}
