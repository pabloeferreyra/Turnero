using Moq;
using Turnero.DAL.Models;
using Turnero.SL.Services.Repositories;
using Turnero.SL.Services;
using Xunit;
using Turnero.SL.Services.TurnsServices;

namespace Turnero.Test;

public class InsertTurnsServicesTests
{
    private readonly Mock<LoggerService> _loggerMock;
    private readonly Mock<ITurnRepository> _turnRepositoryMock;
    private readonly InsertTurnsServices _insertTurnsServices;
    private readonly Turn turn = new()
    {
        Id = Guid.NewGuid(),
        Name = "Patient1",
        DateTurn = DateTime.Today,
        MedicId = new Guid(),
        Dni = "99999999",
        TimeId = new Guid(),
        SocialWork = "O.S. Test",
        Accessed = false,
        Reason = "Test Reason"
    };
    public InsertTurnsServicesTests()
    {
        _loggerMock = new Mock<LoggerService>();
        _turnRepositoryMock = new Mock<ITurnRepository>();
        _insertTurnsServices = new InsertTurnsServices(_loggerMock.Object, _turnRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateTurnAsync_ShouldCallCreateTurnOnRepository()
    {
        // Act
        var result = await _insertTurnsServices.CreateTurnAsync(turn);

        // Assert
        _turnRepositoryMock.Verify(repo => repo.CreateTurn(turn), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task CreateTurnAsync_ShouldLogInfoMessage()
    {
        // Act
        var result = await _insertTurnsServices.CreateTurnAsync(turn);

        // Assert
        _loggerMock.Verify(logger => logger.Log(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateTurnAsync_ShouldHandleException()
    {
        // Arrange
        _turnRepositoryMock.Setup(repo => repo.CreateTurn(It.IsAny<Turn>())).Throws(new Exception("Test exception"));

        // Act
        var result = await _insertTurnsServices.CreateTurnAsync(turn);

        // Assert
        _loggerMock.Verify(logger => logger.Log(It.IsAny<string>()), Times.Never);
        Assert.False(result);
    }
}
