using ChemPoint.GP.Entities.Business_Entities.Purchase;
using Chempoint.GP.Infrastructure.Maps.Base;
using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Purchase
{
    public class PurchaseIndicatorStatusMap : BaseDataTableMap<PurchaseIndicatorStatusEntity>
    {
        public override PurchaseIndicatorStatusEntity Map(DataRow dr)
        {
            var poIndicatorDetail = new PurchaseIndicatorStatusEntity();

            int result;
            int.TryParse(dr["POIndicatorStatusId"].ToString(), out result);
            poIndicatorDetail.POIndicatorStatusId = result;
            poIndicatorDetail.Status = dr["Status"].ToString();

            return poIndicatorDetail;
        }
    }
}
