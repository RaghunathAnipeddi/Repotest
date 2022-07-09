using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.BaseEntities;
using ChemPoint.GP.Entities.Business_Entities.Setup;
using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Setup
{
    public class PaymentTermsSetupMap : BaseMap<SetupEntity>
    {
        public override SetupEntity Map(IDataRecord dr)
        {
            var paymentTermEntity = new SetupEntity();
            PaymentTermsInformation ptInformation = new PaymentTermsInformation();
            ptInformation.PaymentTermsID = dr["PaymentTermsID"].ToString();
            ptInformation.DueOfMonths = dr["DueOfMonths"].ToString();
            int value;
            bool boolValue;
            int.TryParse(dr["EOMEnabled"].ToString(), out value);
            ptInformation.EomEnabled = value;
            ptInformation.OrderPrePaymentPct = dr["OrderPrePaymentPct"].ToString();
            
            int.TryParse(dr["TermsGracePeriod"].ToString(), out value);
            ptInformation.TermsGracePeriod = value;
            
            bool.TryParse(dr["Nested"].ToString(), out boolValue);
            ptInformation.Nested = boolValue;
            
            int.TryParse(dr["DateWithin"].ToString(), out value);
            ptInformation.DateWithin = value;
            
            ptInformation.TermsIfYes = dr["TermsIfYes"].ToString();
            ptInformation.TermsIfNo = dr["TermsIfNo"].ToString();
            
            paymentTermEntity.PaymentTermsDetails = ptInformation;
            
            return paymentTermEntity;
        }
    }
}
