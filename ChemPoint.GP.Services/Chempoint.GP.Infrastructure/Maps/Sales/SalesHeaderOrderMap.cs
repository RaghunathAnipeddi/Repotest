using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Sales
{
    public class SalesHeaderOrderMap : BaseDataTableMap<OrderHeader>
    {
        public override OrderHeader Map(DataRow dr)
        {
            int value;
            decimal decimalValue;
            OrderHeader orderHeader = new OrderHeader();
            
            orderHeader.SopNumber = dr["SopNumber"].ToString();
            int.TryParse(dr["SopType"].ToString(), out value);
            orderHeader.SopType = value;
            orderHeader.OrderGuid = Convert.ToString(dr["OrderGUID"]).Trim();
            orderHeader.CustomerGuid = Convert.ToString(dr["CustomerGUID"]).Trim();
            orderHeader.CarrierInstructionId = dr["CustomerPickupAddressId"].ToString();
            orderHeader.CarrierAccountNumber = dr["CarrierAccountNumber"].ToString();
            orderHeader.CarrierName = dr["CarrierName"].ToString();
            orderHeader.ServiceType = dr["ServiceType"].ToString();
            orderHeader.ImporterofRecord = dr["ImporterofRecord"].ToString(); 
            orderHeader.CustomBroker = dr["CustomBroker"].ToString();
            orderHeader.CarrierPhone = dr["CarrierPhone"].ToString();
            orderHeader.IncoTerm = dr["IncoTerm"].ToString();
            orderHeader.FreightTerm = dr["FreightTerm"].ToString();
            orderHeader.ToBeCancelled = Convert.ToInt16(dr["IsOrderToCancel"]);
            orderHeader.CustomizeServiceSkus = Convert.ToInt16(dr["CustomizeServiceSKU"]);
            decimal.TryParse(dr["OrderTotalAmount"].ToString(), out decimalValue);
            orderHeader.OrderTotalAmount = decimalValue;
            decimal.TryParse(dr["MiscAmount"].ToString(), out decimalValue);
            orderHeader.MiscAmount = decimalValue;
            decimal.TryParse(dr["FreightAmount"].ToString(), out decimalValue);
            orderHeader.FreightAmount = decimalValue;
            decimal.TryParse(dr["TaxAmount"].ToString(), out decimalValue);
            orderHeader.TaxAmount = decimalValue;
            orderHeader.PaymentTerm = dr["PaymentTermId"].ToString();
            return orderHeader;
        }
    }
}
