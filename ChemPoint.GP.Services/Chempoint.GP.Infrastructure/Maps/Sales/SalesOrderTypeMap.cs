using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.BaseEntities;
using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Sales
{
    public class SalesOrderTypeMap : BaseDataTableMap<SalesOrderType>
    {
        public override SalesOrderType Map(DataRow dr)
        {
            SalesOrderType salesOrderType = new SalesOrderType();
            int value;

            int.TryParse(dr["IsInternational"].ToString(), out value);
            salesOrderType.IsInternational = value;
            int.TryParse(dr["IsNAFTA"].ToString(), out value);
            salesOrderType.IsNaFta = value;
            int.TryParse(dr["IsDropship"].ToString(), out value);
            salesOrderType.IsDropship = value;
            int.TryParse(dr["IsBulk"].ToString(), out value);
            salesOrderType.IsBulk = value;
            int.TryParse(dr["IsI3Order"].ToString(), out value);
            salesOrderType.IsI3Order = value;
            int.TryParse(dr["IsCreditCard"].ToString(), out value);
            salesOrderType.IsCreditCard = value;
            int.TryParse(dr["IsCorrective"].ToString(), out value);
            salesOrderType.IsCorrective = value;
            int.TryParse(dr["IsFullTruckLoad"].ToString(), out value);
            salesOrderType.IsFullTruckLoad = value;
            int.TryParse(dr["IsConsignment"].ToString(), out value);
            salesOrderType.IsConsignment = value;
            int.TryParse(dr["IsCorrectiveBoo"].ToString(), out value);
            salesOrderType.IsCorrectiveBoo = value;
            int.TryParse(dr["IsSampleOrder"].ToString(), out value);
            salesOrderType.IsSampleOrder = value;
            int.TryParse(dr["IsTempControl"].ToString(), out value);
            salesOrderType.IsTempControl = value;
            int.TryParse(dr["IsHazmat"].ToString(), out value);
            salesOrderType.IsHazmat = value;
            int.TryParse(dr["IsFreezeProtect"].ToString(), out value);
            salesOrderType.IsFreezeProtect = value;
            int.TryParse(dr["IsAutoPTEligible"].ToString(), out value);
            salesOrderType.IsAutoPTEligible = value;
            int.TryParse(dr["IsCreditEnginePassed"].ToString(), out value);
            salesOrderType.IsCreditEnginePassed = value;
            int.TryParse(dr["IsTaxEnginePassed"].ToString(), out value);
            salesOrderType.IsTaxEnginePassed = value;

            return salesOrderType;
        }
    }
}