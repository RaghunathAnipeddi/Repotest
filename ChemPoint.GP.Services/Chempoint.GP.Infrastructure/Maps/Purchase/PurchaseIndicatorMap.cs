using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.Business_Entities.Purchase;
using System;
using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Purchase
{
    public class PurchaseIndicatorMap : BaseMap<PurchaseIndicatorEntity>
    {
        public override PurchaseIndicatorEntity Map(IDataRecord dr)
        {
            var poIndicatorDetail = new PurchaseIndicatorEntity();

            poIndicatorDetail.PoNumber = dr["PoNumber"].ToString();
            int result;
            short shortValue;
            int.TryParse(dr["POLineNumber"].ToString(), out result);
            poIndicatorDetail.POLineNumber = result;
            poIndicatorDetail.ItemNumber = dr["ItemNumber"].ToString();
            short.TryParse(dr["POIndicatorStatusId"].ToString(), out shortValue);
            poIndicatorDetail.POIndicatorStatusId = shortValue;
            short.TryParse(dr["BackOrderReason"].ToString(), out shortValue);
            poIndicatorDetail.BackOrderReason = shortValue;
            DateTime dateValue;
            DateTime.TryParse(dr["InitialBackOrderDate"].ToString(), out dateValue);
            poIndicatorDetail.InitialBackOrderDate = dateValue;
            short.TryParse(dr["CancelledReason"].ToString(), out shortValue);
            poIndicatorDetail.CancelledReason = shortValue;
            DateTime.TryParse(dr["InitialCancelledDate"].ToString(), out dateValue);
            poIndicatorDetail.InitialCancelledDate = dateValue;
            bool boolValue;
            bool.TryParse(dr["IsCostVariance"].ToString(), out boolValue);
            poIndicatorDetail.IsCostVariance = boolValue;
            DateTime.TryParse(dr["AcknowledgementDate"].ToString(), out dateValue);
            poIndicatorDetail.AcknowledgementDate = dateValue;
            DateTime.TryParse(dr["ConfirmedDate"].ToString(), out dateValue);
            poIndicatorDetail.ConfirmedDate = dateValue;
            DateTime.TryParse(dr["ActualShipDate"].ToString(), out dateValue);
            poIndicatorDetail.ActualShipDate = dateValue;
            return poIndicatorDetail;
        }
    }
}
