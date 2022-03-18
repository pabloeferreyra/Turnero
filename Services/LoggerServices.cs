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
                File.WriteAllText(@"/root/TurneroLogs/infoLog.txt", DateTime.Now +": "+ info);
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                File.WriteAllText(@"D:\infoLog.txt", DateTime.Now + ": " + info);
            }
        }

        public void Debug(string debug)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                File.WriteAllText(@"/root/TurneroLogs/debugLog.txt", DateTime.Now + ": " + debug);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                File.WriteAllText(@"D:\debugLog.txt", DateTime.Now + ": " + debug);
            }
        }

        public void Error(string error, Exception ex)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                File.WriteAllText(@"/root/TurneroLogs/errorLog.txt", DateTime.Now + ": " + error + " - " + ex);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                File.WriteAllText(@"D:\errorLog.txt", DateTime.Now + ": " + error + " - " + ex);
            }
        }
    }
}
