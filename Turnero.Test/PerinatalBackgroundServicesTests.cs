using Moq;
using Turnero.DAL.Models;
using Turnero.SL.Services;
using Turnero.SL.Services.PerinatalBackgroundServices;
using Turnero.SL.Services.Repositories;
using Xunit;

namespace Turnero.Test;

public class PerinatalBackgroundServicesTests
{
    private readonly Mock<LoggerService> _loggerMock;
    private readonly Mock<IPerinatalBackgroundRepository> _repositoryMock;

    public PerinatalBackgroundServicesTests()
    {
        _loggerMock = new Mock<LoggerService>();
        _repositoryMock = new Mock<IPerinatalBackgroundRepository>();
    }

    #region GetPerinatalBackgroundService

    [Fact]
    public async Task Get_ShouldReturnEntity()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new PerinatalBackground { Id = id };
        _repositoryMock.Setup(repo => repo.Get(id)).ReturnsAsync(entity);
        var service = new GetPerinatalBackgroundService(_loggerMock.Object, _repositoryMock.Object);

        // Act
        var result = await service.Get(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task Get_ShouldThrowWhenNotFound()
    {
        _repositoryMock.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync((PerinatalBackground?)null);
        var service = new GetPerinatalBackgroundService(_loggerMock.Object, _repositoryMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.Get(Guid.NewGuid()));
    }

    [Fact]
    public async Task Get_ShouldThrowAndLogOnException()
    {
        _repositoryMock.Setup(repo => repo.Get(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new GetPerinatalBackgroundService(_loggerMock.Object, _repositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.Get(Guid.NewGuid()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region UpdatePerinatalBackgroundService

    [Fact]
    public async Task Update_ShouldCallRepository()
    {
        // Arrange
        var entity = new PerinatalBackground { Id = Guid.NewGuid() };
        var service = new UpdatePerinatalBackgroundService(_loggerMock.Object, _repositoryMock.Object);

        // Act
        await service.Update(entity);

        // Assert
        _repositoryMock.Verify(repo => repo.Update(entity), Times.Once);
    }

    [Fact]
    public async Task Update_ShouldThrowOnInvalidOperation()
    {
        _repositoryMock.Setup(repo => repo.Update(It.IsAny<PerinatalBackground>()))
            .ThrowsAsync(new InvalidOperationException("Not found"));
        var service = new UpdatePerinatalBackgroundService(_loggerMock.Object, _repositoryMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.Update(new PerinatalBackground()));
    }

    [Fact]
    public async Task Update_ShouldThrowAndLogOnException()
    {
        _repositoryMock.Setup(repo => repo.Update(It.IsAny<PerinatalBackground>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new UpdatePerinatalBackgroundService(_loggerMock.Object, _repositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.Update(new PerinatalBackground()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    #endregion
}
