using Chempoint.GP.Infrastructure.Maps.Base;
using System.Data;
using ChemPoint.GP.Entities.Business_Entities.Setup;
using ChemPoint.GP.Entities.BaseEntities;

namespace Chempoint.GP.Infrastructure.Maps.Setup
{
    public class TaxDetailMaintenanceMap : BaseMap<SetupEntity>
    {
        public override SetupEntity Map(IDataRecord dr)
        {
            var setupDetailEntity = new SetupEntity();
            TaxSetupInformation setupInformation = new TaxSetupInformation();
            setupInformation.TaxDetailId = dr["TaxDetailId"].ToString();
            setupInformation.TaxDetailReference = dr["TaxDetailReference"].ToString();
            setupInformation.TaxDetailUnivarNvTaxCode = dr["UnivarNvTaxCode"].ToString();
            setupInformation.TaxDetailUnivarNvTaxCodeDescription = dr["UnivarNvTaxCodeDescription"].ToString();
            setupDetailEntity.SetupDetails = setupInformation;
            return setupDetailEntity;
        }
    }
}
