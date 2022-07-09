using ChemPoint.GP.Entities.BaseEntities;

namespace ChemPoint.GP.Entities.Business_Entities.Setup
{
    public class SetupEntity : IModelBase
    {
        //Tax Schedule & detail Setup
        public TaxSetupInformation SetupDetails { get; set; }

        //Payment Terms Setup
        public PaymentTermsInformation PaymentTermsDetails { get; set; }

        public AuditInformation AuditInformation { get; set; }
    }
}
