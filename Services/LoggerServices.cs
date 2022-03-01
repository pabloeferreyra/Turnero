using System;
using System.IO;
using System.Runtime.InteropServices;
using Turnero.Services.Interfaces;

namespace Turnero.Services
{
    public class LoggerServices : ILoggerServices
    {
        public LoggerServices()
        {

        }

        public void Info(string info)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                File.WriteAllText(@"/tmp/TurneroLogs/infoLog.txt", info);
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                File.WriteAllText(@"C:\infoLog.txt", info);
            }
        }

        public void Debug(string debug)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                File.WriteAllText(@"/tmp/TurneroLogs/debugLog.txt", debug);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                File.WriteAllText(@"C:\debugLog.txt", debug);
            }
        }

        public void Error(string error, Exception ex)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                File.WriteAllText(@"/tmp/TurneroLogs/errorLog.txt", error + " " + ex);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                File.WriteAllText(@"C:\errorLog.txt", error + " " + ex);
            }
        }
    }
}
