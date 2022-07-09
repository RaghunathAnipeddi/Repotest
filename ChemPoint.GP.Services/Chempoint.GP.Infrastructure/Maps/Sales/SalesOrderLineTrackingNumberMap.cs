using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.BaseEntities;
using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Sales
{
    public class SalesOrderLineTrackingNumberMap : BaseDataTableMap<TrackingInformation>
    {
        public override TrackingInformation Map(DataRow dr)
        {
            var sopOrderTrackingDetail = new TrackingInformation();
            int value;
            int.TryParse(dr["SopType"].ToString(), out value);
            sopOrderTrackingDetail.SopType = value;
            sopOrderTrackingDetail.SopNumber = dr["SopNumber"].ToString();
            int.TryParse(dr["LineItemSequence"].ToString(), out value);
            sopOrderTrackingDetail.LineItemSequence = value;
            int.TryParse(dr["ComponentLineSeqNumber"].ToString(), out value);
            sopOrderTrackingDetail.ComponentSequenceNumber = value;
            sopOrderTrackingDetail.TrackingNumber = dr["TrackingNumber"].ToString().Trim();
            return sopOrderTrackingDetail;
        }
    }
}
