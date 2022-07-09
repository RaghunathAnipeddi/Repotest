using System;

namespace Chempoint.GP.Infrastructure.Constants
{
    public static class ApplicationConstants
    {
        public const string ExceptionPolicy = "GeneralExceptionPolicy";

        public const string DateTimeFormat = "MM/dd/yyyy HH:mm:ss.fff";

        public const string DateFormat = "MM/dd/yyyy";

        public static class ConnectionString
        {
            public const string MasterConnectionString = "MasterConnectionString";

            public const string ApplicationConnectionString = "ApplicationConnectionString";

            public const string IntegrationConnectionString = "IntegrationConnectionString";

            public const string TransactionConnectionString = "TransactionConnectionString";
        }
    }
}
