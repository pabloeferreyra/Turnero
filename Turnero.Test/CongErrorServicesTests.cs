using Moq;
using Turnero.DAL.Models;
using Turnero.SL.Services;
using Turnero.SL.Services.CongErrorServices;
using Turnero.SL.Services.Repositories;
using Xunit;

namespace Turnero.Test;

public class CongErrorServicesTests
{
    private readonly Mock<LoggerService> _loggerMock;
    private readonly Mock<ICongErrorsRepository> _repositoryMock;

    public CongErrorServicesTests()
    {
        _loggerMock = new Mock<LoggerService>();
        _repositoryMock = new Mock<ICongErrorsRepository>();
    }

    #region GetCongErrorService

    [Fact]
    public async Task GetCongError_ShouldReturnEntity()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new CongErrors { Id = id };
        _repositoryMock.Setup(repo => repo.Get(id)).ReturnsAsync(entity);
        var service = new GetCongErrorService(_loggerMock.Object, _repositoryMock.Object);

        // Act
        var result = await service.GetCongError(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task GetCongError_ShouldThrowWhenNotFound()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync((CongErrors?)null);
        var service = new GetCongErrorService(_loggerMock.Object, _repositoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetCongError(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetCongError_ShouldThrowAndLogOnException()
    {
        _repositoryMock.Setup(repo => repo.Get(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new GetCongErrorService(_loggerMock.Object, _repositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.GetCongError(Guid.NewGuid()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region UpdateCongErrorService

    [Fact]
    public async Task UpdateCongError_ShouldCallRepository()
    {
        // Arrange
        var entity = new CongErrors { Id = Guid.NewGuid() };
        var service = new UpdateCongErrorService(_loggerMock.Object, _repositoryMock.Object);

        // Act
        await service.UpdateCongError(entity);

        // Assert
        _repositoryMock.Verify(repo => repo.Update(entity), Times.Once);
    }

    [Fact]
    public async Task UpdateCongError_ShouldThrowAndLogOnException()
    {
        _repositoryMock.Setup(repo => repo.Update(It.IsAny<CongErrors>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new UpdateCongErrorService(_loggerMock.Object, _repositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.UpdateCongError(new CongErrors()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    #endregion
}
