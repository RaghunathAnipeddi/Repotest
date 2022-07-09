using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.BaseEntities;
using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Sales
{
    public class SalesLotDetailsMap : BaseDataTableMap<LotInformation>
    {
        public override LotInformation Map(DataRow dr)
        {
            LotInformation lotInformation = new LotInformation();
           
            bool coaSent;
            decimal qty;
            lotInformation.LotNumber = dr["LotNumber"].ToString();
            decimal.TryParse(dr["LotQuantity"].ToString(), out qty);
            lotInformation.LotQuantity = qty;
            bool.TryParse(dr["IsCoaSent"].ToString(), out coaSent);
            lotInformation.IsCoaSent = coaSent;
            return lotInformation;
        }
    }
}
