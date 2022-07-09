using ChemPoint.GP.Entities.Business_Entities.Setup;

namespace Chempoint.GP.Model.Interactions.Setup
{
    public class SetupRequest
    {
        public string UserId { get; set; }

        public int CompanyID { get; set; }

        public string ConnectionString { get; set; }

        public SetupEntity SetupEntity { get; set; }
    }
}

