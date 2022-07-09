using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.BaseEntities;
using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Sales
{
    public class QuoteMap : BaseDataTableMap<SalesLineItem>
    {
        public override SalesLineItem Map(DataRow dr)
        {
            SalesLineItem lineItem = new SalesLineItem();
            int value;
            int.TryParse(dr["Version"].ToString(), out value);
            lineItem.QuoteNumber = dr["QuoteNumber"].ToString();
            lineItem.QuoteNumberVersion = value;
            lineItem.QuoteGuid = dr["QuoteGuid"].ToString();
            return lineItem;
        }
    }
}
