using System;

namespace Chempoint.GP.Infrastructure.Exceptions
{
    public class SetupExtException : ExceptionBase
    {
        public SetupExtException(string message) : base(message)
        { 
        }

        public SetupExtException(string fmt, params object[] vars) : base(string.Format(fmt, vars))
        {
        }
    }
}
