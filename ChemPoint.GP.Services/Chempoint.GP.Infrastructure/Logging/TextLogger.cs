using System;
using System.IO;

namespace Chempoint.GP.Infrastructure.Logging
{
    public class TextLogger
    {
        public void LogInformationIntoFile(string logMessage, string logFilePath, string logFileName)
        {
            string strLogFileName;
            try
            {
                // Get the file name from the configuration xml.
                strLogFileName = logFileName + DateTime.Now.ToString("MM_dd_yy") + ".log";
                if (!Directory.Exists(logFilePath))
                    Directory.CreateDirectory(logFilePath);
                logFilePath = logFilePath + strLogFileName;

                if (!File.Exists(logFilePath))
                    File.Create(logFilePath).Close();

                // Log the details into the file.
                using (StreamWriter w = File.AppendText(logFilePath))
                {
                    w.WriteLine(logMessage);
                    w.Flush();
                    w.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
