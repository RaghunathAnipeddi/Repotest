using System;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace Chempoint.GP.Infrastructure.Logging
{
    public static class ExceptionLogger
    {
        public static void LogException(Exception exception, string policyName)
        {
            //ExceptionPolicy.HandleException(exception, policyName);
        }

        public static void RegisterExceptionLogger()
        {
            IConfigurationSource config = ConfigurationSourceFactory.Create();
            Logger.SetLogWriter(new LogWriterFactory(config).Create(), false);

            ExceptionPolicyFactory factory = new ExceptionPolicyFactory(config);
            ExceptionManager exManager = factory.CreateManager();
            ExceptionPolicy.SetExceptionManager(exManager, false);
        }
    }
}
