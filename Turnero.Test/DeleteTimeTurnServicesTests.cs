using Moq;
using Turnero.SL.Services.Interfaces;
using Turnero.SL.Services.Repositories;
using Turnero.SL.Services;
using Turnero.DAL.Models;
using Xunit;

namespace Turnero.Test;

public class DeleteTimeTurnServicesTests
{
    private readonly Mock<ILoggerServices> _loggerMock;
    private readonly Mock<ITimeTurnRepository> _timeTurnRepositoryMock;
    private readonly DeleteTimeTurnServices _deleteTimeTurnServices;

    public DeleteTimeTurnServicesTests()
    {
        _loggerMock = new Mock<ILoggerServices>();
        _timeTurnRepositoryMock = new Mock<ITimeTurnRepository>();
        _deleteTimeTurnServices = new DeleteTimeTurnServices(_timeTurnRepositoryMock.Object);
    }

    [Fact]
    public void Delete_ShouldCallDeleteTTOnRepository()
    {
        // Arrange
        var timeTurn = new TimeTurn
        {
            Id = Guid.NewGuid(),
            Time = "10:00"
        };

        // Act
        _deleteTimeTurnServices.Delete(timeTurn);

        // Assert
        _timeTurnRepositoryMock.Verify(repo => repo.DeleteTT(timeTurn), Times.Once);
    }

    [Fact]
    public void Delete_ShouldLogInfoMessage()
    {
        // Arrange
        var timeTurn = new TimeTurn { Id = Guid.NewGuid(), Time = "10:00" };

        // Act
        _deleteTimeTurnServices.Delete(timeTurn);

        // Assert
        _loggerMock.Verify(logger => logger.Info(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Delete_ShouldHandleException()
    {
        // Arrange
        var timeTurn = new TimeTurn { Id = Guid.NewGuid(), Time = "10:00" };
        _timeTurnRepositoryMock.Setup(repo => repo.DeleteTT(It.IsAny<TimeTurn>())).Throws(new Exception("Test exception"));

        // Act
        _deleteTimeTurnServices.Delete(timeTurn);

        // Assert
        _loggerMock.Verify(logger => logger.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
    }
}
