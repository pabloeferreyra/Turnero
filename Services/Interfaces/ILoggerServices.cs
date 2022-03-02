using System;

namespace Turnero.Services.Interfaces
{
    public interface ILoggerServices
    {
        void Info(string info);
        void Debug(string debug);
        void Error(string error, Exception ex);

    }
}
