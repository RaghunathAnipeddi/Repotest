using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Sales
{
    public class SalesOrderLineMap : BaseDataTableMap<SalesLineItem>
    {
        public override SalesLineItem Map(DataRow dr)
        {
            var sopOrderDetail = new SalesLineItem();
            int value;
            DateTime dateValue;
            decimal decimalValue;
            int.TryParse(dr["SopType"].ToString(), out value);
            sopOrderDetail.SopType = value;
            sopOrderDetail.SopNumber = dr["SopNumber"].ToString();
            int.TryParse(dr["LineItemSequence"].ToString(), out value);
            sopOrderDetail.OrderLineId = value;
            int.TryParse(dr["ComponentLineSeqNumber"].ToString(), out value);
            sopOrderDetail.ComponentSequenceNumber = value;
            sopOrderDetail.ItemNumber = dr["ItemNumber"].ToString();
            decimal.TryParse(dr["FreightPrice"].ToString(), out decimalValue);
            sopOrderDetail.FreightIncludedPrice = decimalValue;
            sopOrderDetail.QuoteNumber = dr["QuoteNumber"].ToString();
            DateTime.TryParse(dr["ActualShipDate"].ToString(), out dateValue);
            sopOrderDetail.ActualShipDate = dateValue;
            sopOrderDetail.WarehouseInstructionId = dr["WarehouseInstructionID"].ToString();
            sopOrderDetail.CarrierInstructionId = dr["CarrierInstructionID"].ToString();
            int.TryParse(dr["ShipWeight"].ToString(), out value);
            sopOrderDetail.ShipWeight = value;
            int.TryParse(dr["NetWeight"].ToString(), out value);
            sopOrderDetail.NetWeight = value;  
            int.TryParse(dr["UNNumber"].ToString(), out value);
            sopOrderDetail.NetWeight = value;             
            sopOrderDetail.CountryOfOrigin = dr["CountryofOrigin"].ToString();
            sopOrderDetail.EccnNumber = dr["ECCNNumber"].ToString();
            sopOrderDetail.HtsNumber = dr["HTSNumber"].ToString();
            sopOrderDetail.LineStatus = dr["LineShipStatus"].ToString();
            int.TryParse(dr["NaSop10200RowId"].ToString(), out value);

            return sopOrderDetail;
        }
    }
}
