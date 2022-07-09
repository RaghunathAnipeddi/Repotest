using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Sales
{
    public class SalesItemDetailMap : BaseDataTableMap<SalesLineItem>
    {
        public override SalesLineItem Map(DataRow dr)
        {
            var sopItemDetail = new SalesLineItem();
            sopItemDetail.SopType = Convert.ToInt16(dr["SopType"]);
            sopItemDetail.SopNumber = dr["SopNumber"].ToString().Trim();
            sopItemDetail.OrderLineId = Convert.ToInt32(dr["LineItemSequence"]);
            sopItemDetail.ComponentSequenceNumber = Convert.ToInt16(dr["ComponentLineSeqNumber"]);
            sopItemDetail.ItemNumber = dr["ItemNumber"].ToString().Trim();
            sopItemDetail.OrginatingFreightPrice = Convert.ToDecimal(dr["FreightPrice"]);            
            //Quote information
            sopItemDetail.QuoteNumber = dr["QuoteNumber"].ToString().Trim();
            sopItemDetail.QuoteGuid = dr["QuoteGUID"].ToString().Trim();
            sopItemDetail.QuoteNumberVersion = Convert.ToInt32(dr["QuoteVersion"]);
            sopItemDetail.IsFip = Convert.ToBoolean(dr["FreightIncludedPricing"]);
            sopItemDetail.LineFreightAmount = Convert.ToDecimal(dr["FreightIncludedPricingValue"]);

            //Product Information
            sopItemDetail.ShipWeight = Convert.ToDecimal(dr["ShipWeight"]);
            sopItemDetail.NetWeight = Convert.ToDecimal(dr["NetWeight"]);
            sopItemDetail.UnNumber = dr["UNNumber"].ToString().Trim();
            sopItemDetail.CountryOfOrigin = dr["CountryofOrigin"].ToString().Trim();
            sopItemDetail.EccnNumber = dr["ECCNNumber"].ToString().Trim();
            sopItemDetail.HtsNumber = dr["HTSNumber"].ToString().Trim();

            return sopItemDetail;
        }
    }
}
