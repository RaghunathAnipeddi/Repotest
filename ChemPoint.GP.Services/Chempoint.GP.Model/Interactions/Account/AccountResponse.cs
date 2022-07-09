using System;
using System.Linq;

namespace Chempoint.GP.Model.Interactions.Account
{
    public enum ResponseStatus
    {
        Success = 0,
        Error = 1,
        Unknown = 2,
        Custom = 3
    }

    public class AccountResponse
    {
        public ResponseStatus Status { get; set; }

        public string ErrorMessage { get; set; }
    }
}
