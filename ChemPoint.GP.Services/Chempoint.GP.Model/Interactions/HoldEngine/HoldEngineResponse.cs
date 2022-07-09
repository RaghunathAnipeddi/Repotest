using System;

namespace Chempoint.GP.Model.Interactions.HoldEngine
{
    public enum HoldEngineResponseStatus
    {
        Success = 0,
        Error = 1,
        Unknown = 2,
        Custom = 3
    }

    public class HoldEngineResponse
    {
        public HoldEngineResponseStatus Status { get; set; }

        public string ErrorMessage { get; set; }

        public int Result { get; set; }
    }
}
