namespace Turnero.SL.Services;

public class LoggerService(string logFilePath = "AppLogs.log")
{
    private readonly string _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFilePath);

    public async Task LogAsync(string message)
    {
        var logEntry = $"{DateTime.UtcNow:dd-MM-yyyy HH:mm:ss} | {message}{Environment.NewLine}";
        await File.AppendAllTextAsync(_logFilePath, logEntry);
    }

    public void Log(string message)
    {
        var logEntry = $"{DateTime.UtcNow:dd-MM-yyyy HH:mm:ss} | {message}{Environment.NewLine}";
        File.AppendAllText(_logFilePath, logEntry);
    }
}
