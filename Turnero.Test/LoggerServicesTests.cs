using Turnero.SL.Services;
using Xunit;

namespace Turnero.Test;

public class LoggerServicesTests : IDisposable
{
    private readonly LoggerService _loggerServices;
    private readonly string _logFilePath;

    public LoggerServicesTests()
    {
        _loggerServices = new LoggerService();
        _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppLogs.log");
        // Clean up before tests
        CleanupLogFile();
    }

    public void Dispose()
    {
        CleanupLogFile();
    }

    private void CleanupLogFile()
    {
        try
        {
            if (File.Exists(_logFilePath))
                File.Delete(_logFilePath);
        }
        catch
        {
            // File may be locked - ignore cleanup failures
        }
    }

    [Fact]
    public void Info_ShouldWriteInfoLog()
    {
        // Arrange
        var infoMessage = "This is an info message";

        // Act
        _loggerServices.Log(infoMessage);

        // Assert
        var logContent = File.ReadAllText(_logFilePath);
        Assert.Contains(infoMessage, logContent);
    }

    [Fact]
    public void Debug_ShouldWriteDebugLog()
    {
        // Arrange
        var debugMessage = "This is a debug message";

        // Act
        _loggerServices.Log(debugMessage);

        // Assert
        var logContent = File.ReadAllText(_logFilePath);
        Assert.Contains(debugMessage, logContent);
    }

    [Fact]
    public void Error_ShouldWriteErrorLog()
    {
        // Arrange
        var errorMessage = "This is an error message";

        // Act
        _loggerServices.Log(errorMessage);

        // Assert
        var logContent = File.ReadAllText(_logFilePath);
        Assert.Contains(errorMessage, logContent);
    }

    [Fact]
    public async Task LogAsync_ShouldWriteToFile()
    {
        // Arrange
        var message = "Async log message";

        // Act
        await _loggerServices.LogAsync(message);

        // Assert
        var logContent = await File.ReadAllTextAsync(_logFilePath);
        Assert.Contains(message, logContent);
    }
}
