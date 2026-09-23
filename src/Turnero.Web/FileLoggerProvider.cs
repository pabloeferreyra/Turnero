namespace Turnero.Web;

public sealed class FileLoggerProvider(string logFilePath = "AppLogs.log") : ILoggerProvider
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    private const int MaxBackupFiles = 5;

    private readonly string _logFilePath = Path.Combine(AppContext.BaseDirectory, logFilePath);
    private readonly object _sync = new();

    public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, _logFilePath, _sync);

    public void Dispose()
    {
    }

    private sealed class FileLogger(string categoryName, string logFilePath, object syncLock) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var directory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var message = formatter(state, exception);
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff 'UTC'");
            var line = $"[{timestamp}] [{logLevel}] [{categoryName}] {message}";

            if (exception is not null)
            {
                line += $"{Environment.NewLine}{exception}";
            }

            lock (syncLock)
            {
                RotateIfNeeded(logFilePath);
                File.AppendAllText(logFilePath, line + Environment.NewLine);
            }
        }

        private static void RotateIfNeeded(string logFilePath)
        {
            if (!File.Exists(logFilePath))
            {
                return;
            }

            var fileInfo = new FileInfo(logFilePath);
            if (fileInfo.Length < MaxFileSizeBytes)
            {
                return;
            }

            for (var i = MaxBackupFiles - 1; i >= 1; i--)
            {
                var oldPath = $"{logFilePath}.{i}";
                var newPath = $"{logFilePath}.{i + 1}";

                if (File.Exists(newPath))
                {
                    File.Delete(newPath);
                }

                if (File.Exists(oldPath))
                {
                    File.Move(oldPath, newPath);
                }
            }

            var latestBackup = $"{logFilePath}.1";
            if (File.Exists(latestBackup))
            {
                File.Delete(latestBackup);
            }

            File.Move(logFilePath, latestBackup);
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}
