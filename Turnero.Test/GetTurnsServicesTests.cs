using Moq;
using Turnero.DAL.Models;
using Turnero.SL.Services.Interfaces;
using Turnero.SL.Services.Repositories;
using Turnero.SL.Services;
using Xunit;

namespace Turnero.Test;

public class GetTurnsServicesTests
{
    private readonly Mock<ILoggerServices> _loggerMock;
    private readonly Mock<ITurnRepository> _turnRepositoryMock;
    private readonly GetTurnsServices _getTurnsServices;

    public GetTurnsServicesTests()
    {
        _loggerMock = new Mock<ILoggerServices>();
        _turnRepositoryMock = new Mock<ITurnRepository>();
        _getTurnsServices = new GetTurnsServices(_loggerMock.Object, _turnRepositoryMock.Object);
    }

    [Fact]
    public void GetTurns_ShouldReturnTurnsList()
    {
        // Arrange
        var dateTurn = DateTime.Now;
        var medicId = Guid.NewGuid();
        var turns = new List<Turn> { new Turn { Id = Guid.NewGuid(), Name = "Turn1" } };
        _turnRepositoryMock.Setup(repo => repo.GetList(dateTurn, medicId)).Returns(turns);

        // Act
        var result = _getTurnsServices.GetTurns(dateTurn, medicId);

        // Assert
        Assert.Single(result);
        Assert.Equal("Turn1", result[0].Name);
    }

    [Fact]
    public async Task GetTurn_ShouldReturnTurn()
    {
        // Arrange
        var turnId = Guid.NewGuid();
        var turn = new Turn { Id = turnId, Name = "Turn1" };
        _turnRepositoryMock.Setup(repo => repo.GetById(turnId)).ReturnsAsync(turn);

        // Act
        var result = await _getTurnsServices.GetTurn(turnId);

        // Assert
        Assert.Equal("Turn1", result.Name);
    }

    [Fact]
    public async Task GetTurnDTO_ShouldReturnTurnDTO()
    {
        // Arrange
        var turnId = Guid.NewGuid();
        var turnDTO = new TurnDTO { Id = turnId, Name = "Turn1" };
        _turnRepositoryMock.Setup(repo => repo.GetDTOById(turnId)).ReturnsAsync(turnDTO);

        // Act
        var result = await _getTurnsServices.GetTurnDTO(turnId);

        // Assert
        Assert.Equal("Turn1", result.Name);
    }

    [Fact]
    public void Exists_ShouldReturnTrueIfExists()
    {
        // Arrange
        var turnId = Guid.NewGuid();
        _turnRepositoryMock.Setup(repo => repo.TurnExists(turnId)).Returns(true);

        // Act
        var result = _getTurnsServices.Exists(turnId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CheckTurn_ShouldReturnTrueIfTurnExists()
    {
        // Arrange
        var medicId = Guid.NewGuid();
        var date = DateTime.Now;
        var timeTurn = Guid.NewGuid();
        _turnRepositoryMock.Setup(repo => repo.CheckTurn(medicId, date, timeTurn)).Returns(true);

        // Act
        var result = _getTurnsServices.CheckTurn(medicId, date, timeTurn);

        // Assert
        Assert.True(result);
    }
}