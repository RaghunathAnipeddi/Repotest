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
        public class SalesOrderServiceSKUMap : BaseDataTableMap<SalesLineItem>
        {
            public override SalesLineItem Map(DataRow dr)
            {
                SalesLineItem serviceSKU = new SalesLineItem();
                decimal decimalValue;
                #region serviceSKU
                serviceSKU.ItemNumber = dr["ItemNumber"].ToString();
                serviceSKU.ItemDescription = dr["ItemDescription"].ToString();
                serviceSKU.ItemUofM = dr["UofM"].ToString();
                decimal.TryParse(dr["ServiceSKUPrice"].ToString(), out decimalValue);
                serviceSKU.UnitCostAmount = decimalValue;
                decimal.TryParse(dr["ServiceSKUPrice"].ToString(), out decimalValue);
                serviceSKU.UnitPriceAmount = decimalValue;
                return serviceSKU;
                #endregion serviceSKU
            }
        }
}
