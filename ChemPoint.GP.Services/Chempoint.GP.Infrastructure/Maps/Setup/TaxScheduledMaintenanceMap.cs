using Chempoint.GP.Infrastructure.Maps.Base;
using System;
using System.Data;
using ChemPoint.GP.Entities.Business_Entities.Setup;
using ChemPoint.GP.Entities.BaseEntities;

namespace Chempoint.GP.Infrastructure.Maps.Setup
{
    public class TaxScheduledMaintenanceMap : BaseMap<SetupEntity>
    {
        public override SetupEntity Map(IDataRecord dr)
        {
            var taxScheduleDetailEntity = new SetupEntity();
            TaxSetupInformation setupInformation = new TaxSetupInformation();
            setupInformation.TaxScheduleId = dr["TaxScheduleId"].ToString();
            setupInformation.TaxScheduleChempointVatNumber = dr["ChempointVat"].ToString();
            setupInformation.TaxScheduleIsActive = Convert.ToBoolean(dr["InActive"].ToString());
            taxScheduleDetailEntity.SetupDetails = setupInformation;
            return taxScheduleDetailEntity;
        }
    }
}
