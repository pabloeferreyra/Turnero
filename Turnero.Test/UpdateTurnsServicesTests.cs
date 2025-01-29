using Microsoft.EntityFrameworkCore;
using Moq;
using Turnero.DAL.Models;
using Turnero.SL.Services.Interfaces;
using Turnero.SL.Services.Repositories;
using Turnero.SL.Services;
using Xunit;

namespace Turnero.Test;

public class UpdateTurnsServicesTests
{
    private readonly Mock<ILoggerServices> _loggerMock;
    private readonly Mock<ITurnRepository> _turnRepositoryMock;
    private readonly UpdateTurnsServices _updateTurnsServices;
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
    public UpdateTurnsServicesTests()
    {
        _loggerMock = new Mock<ILoggerServices>();
        _turnRepositoryMock = new Mock<ITurnRepository>();
        _updateTurnsServices = new UpdateTurnsServices(_loggerMock.Object, _turnRepositoryMock.Object);
    }

    [Fact]
    public void Accessed_ShouldCallAccessOnRepository_WhenDateTurnIsTodayOrEarlier()
    {
        // Act
        _updateTurnsServices.Accessed(turn);

        // Assert
        _turnRepositoryMock.Verify(repo => repo.Access(turn), Times.Once);
    }

    [Fact]
    public void Accessed_ShouldNotCallAccessOnRepository_WhenDateTurnIsInFuture()
    {
        // Arrange
        turn.DateTurn = DateTime.Today.AddDays(1);

        // Act
        _updateTurnsServices.Accessed(turn);

        // Assert
        _turnRepositoryMock.Verify(repo => repo.Access(It.IsAny<Turn>()), Times.Never);
    }

    [Fact]
    public void Accessed_ShouldHandleException()
    {
        // Arrange
        _turnRepositoryMock.Setup(repo => repo.Access(It.IsAny<Turn>())).Throws(new Exception("Test exception"));

        // Act
        _updateTurnsServices.Accessed(turn);

        // Assert
        _loggerMock.Verify(logger => logger.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
    }

    [Fact]
    public void Update_ShouldCallUpdateTurnOnRepository()
    {
        // Act
        _updateTurnsServices.Update(turn);

        // Assert
        _turnRepositoryMock.Verify(repo => repo.UpdateTurn(turn), Times.Once);
    }

    [Fact]
    public void Update_ShouldHandleDbUpdateConcurrencyException()
    {
        // Arrange
        _turnRepositoryMock.Setup(repo => repo.UpdateTurn(It.IsAny<Turn>())).Throws(new DbUpdateConcurrencyException("Test exception"));

        // Act
        _updateTurnsServices.Update(turn);

        // Assert
        _loggerMock.Verify(logger => logger.Error(It.IsAny<string>(), It.IsAny<DbUpdateConcurrencyException>()), Times.Once);
    }

    [Fact]
    public void Delete_ShouldCallDeleteTurnOnRepository()
    {
        // Act
        _updateTurnsServices.Delete(turn);

        // Assert
        _turnRepositoryMock.Verify(repo => repo.DeleteTurn(turn), Times.Once);
    }

    [Fact]
    public void Delete_ShouldHandleDbUpdateConcurrencyException()
    {
        // Arrange
        _turnRepositoryMock.Setup(repo => repo.DeleteTurn(It.IsAny<Turn>())).Throws(new DbUpdateConcurrencyException("Test exception"));

        // Act
        _updateTurnsServices.Delete(turn);

        // Assert
        _loggerMock.Verify(logger => logger.Error(It.IsAny<string>(), It.IsAny<DbUpdateConcurrencyException>()), Times.Once);
    }
}
