using Moq;
using Turnero.DAL.Models;
using Turnero.SL.Services;
using Turnero.SL.Services.Repositories;
using Turnero.SL.Services.TurnsServices;
using Xunit;

namespace Turnero.Test;

public class GetTimeTurnsServicesTests
{
    private readonly Mock<LoggerService> _loggerMock;
    private readonly Mock<ITimeTurnRepository> _timeTurnRepositoryMock;
    private readonly GetTimeTurnsServices _getTimeTurnsServices;

    public GetTimeTurnsServicesTests()
    {
        _loggerMock = new Mock<LoggerService>();
        _timeTurnRepositoryMock = new Mock<ITimeTurnRepository>();
        _getTimeTurnsServices = new GetTimeTurnsServices(_loggerMock.Object, _timeTurnRepositoryMock.Object);
    }

    [Fact]
    public async Task GetTimeTurns_ShouldReturnTimeTurnsList()
    {
        // Arrange
        var timeTurns = new List<TimeTurn> { new TimeTurn { Id = Guid.NewGuid(), Time = "10:00" } };
        _timeTurnRepositoryMock.Setup(repo => repo.GetList()).ReturnsAsync(timeTurns);

        // Act
        var result = await _getTimeTurnsServices.GetTimeTurns();

        // Assert
        Assert.Single(result);
        Assert.Equal("10:00", result[0].Time);
    }

    [Fact]
    public void GetTimeTurnsQ_ShouldReturnTimeTurnsQueryable()
    {
        // Arrange
        var timeTurns = new List<TimeTurn> { new TimeTurn { Id = Guid.NewGuid(), Time = "10:00" } }.AsQueryable();
        _timeTurnRepositoryMock.Setup(repo => repo.GetQueryable()).Returns(timeTurns);

        // Act
        var result = _getTimeTurnsServices.GetTimeTurnsQ();

        // Assert
        Assert.Single(result);
        Assert.Equal("10:00", result.First().Time);
    }

    [Fact]
    public async Task GetTimeTurn_ShouldReturnTimeTurn()
    {
        // Arrange
        var timeTurnId = Guid.NewGuid();
        var timeTurn = new TimeTurn { Id = timeTurnId, Time = "10:00" };
        _timeTurnRepositoryMock.Setup(repo => repo.GetbyId(timeTurnId)).ReturnsAsync(timeTurn);

        // Act
        var result = await _getTimeTurnsServices.GetTimeTurn(timeTurnId);

        // Assert
        Assert.Equal("10:00", result.Time);
    }

    [Fact]
    public void TimeTurnViewModelExists_ShouldReturnTrueIfExists()
    {
        // Arrange
        var timeTurnId = Guid.NewGuid();
        _timeTurnRepositoryMock.Setup(repo => repo.Exists(timeTurnId)).Returns(true);

        // Act
        var result = _getTimeTurnsServices.TimeTurnViewModelExists(timeTurnId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetCachedTimes_ShouldReturnCachedTimeTurnsList()
    {
        // Arrange
        var cachedTimeTurns = new List<TimeTurn> { new TimeTurn { Id = Guid.NewGuid(), Time = "10:00" } };
        _timeTurnRepositoryMock.Setup(repo => repo.GetCachedTimes()).ReturnsAsync(cachedTimeTurns);

        // Act
        var result = await _getTimeTurnsServices.GetCachedTimes();

        // Assert
        Assert.Single(result);
        Assert.Equal("10:00", result[0].Time);
    }
}