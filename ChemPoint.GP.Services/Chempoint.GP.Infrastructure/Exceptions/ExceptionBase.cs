using System;

namespace Chempoint.GP.Infrastructure.Exceptions
{
    public class ExceptionBase : Exception
    {
        public string CustomMessage { get; set; }

        private ExceptionBase() : base()
        {
        }

        public ExceptionBase(string message) : base(message)
        {
        }

        public ExceptionBase(string fmt, params object[] vars) : base(string.Format(fmt, vars))
        {
        }

        protected ExceptionBase(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
