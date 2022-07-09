using ChemPoint.GP.Entities.Business_Entities.Setup;

namespace Chempoint.GP.Model.Interactions.Setup
{
    public enum ResponseStatus
    {
        Success = 0,
        Error = 1,
        Unknown = 2,
        Custom = 3
    }

    public class SetupResponse
    {
        public ResponseStatus Status { get; set; }

        public string ErrorMessage { get; set; }

        public SetupEntity SetupDetailsEntity { get; set; }
    }
}
