using Moq;
using Turnero.DAL.Models;
using Turnero.SL.Services.Repositories;
using Turnero.SL.Services;
using Xunit;
using Turnero.SL.Services.TurnsServices;

namespace Turnero.Test;

public class InsertTimeTurnServicesTests
{
    private readonly Mock<LoggerService> _loggerMock;
    private readonly Mock<ITimeTurnRepository> _timeTurnRepositoryMock;
    private readonly InsertTimeTurnServices _insertTimeTurnServices;

    public InsertTimeTurnServicesTests()
    {
        _loggerMock = new Mock<LoggerService>();
        _timeTurnRepositoryMock = new Mock<ITimeTurnRepository>();
        _insertTimeTurnServices = new InsertTimeTurnServices(_loggerMock.Object, _timeTurnRepositoryMock.Object);
    }

    [Fact]
    public async Task Create_ShouldCallCreateTTOnRepository()
    {
        // Arrange
        var timeTurn = new TimeTurn { Id = Guid.NewGuid(), Time = "10:00" };

        // Act
        await _insertTimeTurnServices.Create(timeTurn);

        // Assert
        _timeTurnRepositoryMock.Verify(repo => repo.CreateTT(timeTurn), Times.Once);
    }

    [Fact]
    public async Task Create_ShouldLogInfoMessage()
    {
        // Arrange
        var timeTurn = new TimeTurn { Id = Guid.NewGuid(), Time = "10:00" };

        // Act
        await _insertTimeTurnServices.Create(timeTurn);

        // Assert
        _loggerMock.Verify(logger => logger.Log(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Create_ShouldHandleException()
    {
        // Arrange
        var timeTurn = new TimeTurn { Id = Guid.NewGuid(), Time = "10:00" };
        _timeTurnRepositoryMock.Setup(repo => repo.CreateTT(It.IsAny<TimeTurn>())).Throws(new Exception("Test exception"));

        // Act
        await _insertTimeTurnServices.Create(timeTurn);

        // Assert
        _loggerMock.Verify(logger => logger.Log(It.IsAny<string>()), Times.Never);
    }
}