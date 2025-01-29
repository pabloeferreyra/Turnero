using Moq;
using System.Runtime.InteropServices;
using Turnero.SL.Services.Interfaces;
using Turnero.SL.Services;
using Xunit;

namespace Turnero.Test;

public class LoggerServicesTests
{
    private readonly LoggerServices _loggerServices;

    public LoggerServicesTests()
    {
        _loggerServices = new LoggerServices();
    }

    [Fact]
    public void Info_ShouldWriteInfoLog()
    {
        // Arrange
        var infoMessage = "This is an info message";
        var expectedMessage = DateTime.Now + ": " + infoMessage;

        // Act
        _loggerServices.Info(infoMessage);

        // Assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var logContent = File.ReadAllText(@"/root/TurneroLogs/infoLog.txt");
            Assert.Contains(expectedMessage, logContent);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var logContent = File.ReadAllText(@"D:\infoLog.txt");
            Assert.Contains(expectedMessage, logContent);
        }
    }

    [Fact]
    public void Debug_ShouldWriteDebugLog()
    {
        // Arrange
        var debugMessage = "This is a debug message";
        var expectedMessage = DateTime.Now + ": " + debugMessage;

        // Act
        _loggerServices.Debug(debugMessage);

        // Assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var logContent = File.ReadAllText(@"/root/TurneroLogs/debugLog.txt");
            Assert.Contains(expectedMessage, logContent);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var logContent = File.ReadAllText(@"D:\debugLog.txt");
            Assert.Contains(expectedMessage, logContent);
        }
    }

    [Fact]
    public void Error_ShouldWriteErrorLog()
    {
        // Arrange
        var errorMessage = "This is an error message";
        var exception = new Exception("Test exception");
        var expectedMessage = DateTime.Now + ": " + errorMessage + " - " + exception;

        // Act
        _loggerServices.Error(errorMessage, exception);

        // Assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var logContent = File.ReadAllText(@"/root/TurneroLogs/errorLog.txt");
            Assert.Contains(expectedMessage, logContent);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var logContent = File.ReadAllText(@"D:\errorLog.txt");
            Assert.Contains(expectedMessage, logContent);
        }
    }
}
