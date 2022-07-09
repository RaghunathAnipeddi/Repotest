using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Infrastructure.Maps.Sales
{
    public class SalesOrderAllocatedQtyMap : BaseDataTableMap<SalesLineItem>
    {
        public override SalesLineItem Map(DataRow dr)
        {
            var sopOrderAllocatedQtyDetail = new SalesLineItem();
            int value; decimal decimalValue;
            int.TryParse(dr["SopType"].ToString(), out value);
            sopOrderAllocatedQtyDetail.SopType = value;
            sopOrderAllocatedQtyDetail.SopNumber = dr["SopNumber"].ToString().Trim();
            int.TryParse(dr["LineItemSequence"].ToString(), out value);
            sopOrderAllocatedQtyDetail.OrderLineId = value;
            int.TryParse(dr["ComponentLineSeqNumber"].ToString(), out value);
            sopOrderAllocatedQtyDetail.ComponentSequenceNumber = value;
            sopOrderAllocatedQtyDetail.ItemNumber = dr["ItemNumber"].ToString().Trim();
            sopOrderAllocatedQtyDetail.LocationCode = dr["LocationCode"].ToString().Trim();
            decimal.TryParse(dr["AllocatedQty"].ToString(), out decimalValue);
            sopOrderAllocatedQtyDetail.AllocatedQuantity = decimalValue;
            decimal.TryParse(dr["Quantity"].ToString(), out decimalValue);
            sopOrderAllocatedQtyDetail.Quantity = decimalValue;
            decimal.TryParse(dr["QuantityCancel"].ToString(), out decimalValue);
            sopOrderAllocatedQtyDetail.CancelledQuantity = decimalValue;
            decimal.TryParse(dr["QuantityRemaining"].ToString(), out decimalValue);
            sopOrderAllocatedQtyDetail.QuantityRemaining = decimalValue;
            return sopOrderAllocatedQtyDetail;
        }
    }
}
