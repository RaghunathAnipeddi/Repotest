using System;

namespace Chempoint.GP.Infrastructure.Exceptions
{
    public class SalesException : ExceptionBase
    {
        public SalesException(string message) : base(message)
        {
        }

        public SalesException(string fmt, params object[] vars) : base(string.Format(fmt, vars))
        {
        }
    }
}
