using Moq;
using Turnero.DAL.Models;
using Turnero.SL.Services;
using Turnero.SL.Services.PersonalBackgroundServices;
using Turnero.SL.Services.Repositories;
using Xunit;

namespace Turnero.Test;

public class PersonalBackgroundServicesTests
{
    private readonly Mock<LoggerService> _loggerMock;
    private readonly Mock<IPersonalBackgroundRepository> _repositoryMock;

    public PersonalBackgroundServicesTests()
    {
        _loggerMock = new Mock<LoggerService>();
        _repositoryMock = new Mock<IPersonalBackgroundRepository>();
    }

    #region GetPersonalBackgroundService

    [Fact]
    public async Task GetPersonalBackground_ShouldReturnEntity()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new PersonalBackground { Id = id };
        _repositoryMock.Setup(repo => repo.Get(id)).ReturnsAsync(entity);
        var service = new GetPersonalBackgroundService(_loggerMock.Object, _repositoryMock.Object);

        // Act
        var result = await service.GetPersonalBackground(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task GetPersonalBackground_ShouldThrowWhenNotFound()
    {
        _repositoryMock.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync((PersonalBackground?)null);
        var service = new GetPersonalBackgroundService(_loggerMock.Object, _repositoryMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetPersonalBackground(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetPersonalBackground_ShouldThrowAndLogOnException()
    {
        _repositoryMock.Setup(repo => repo.Get(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new GetPersonalBackgroundService(_loggerMock.Object, _repositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.GetPersonalBackground(Guid.NewGuid()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region UpdatePersonalBackgroundService

    [Fact]
    public async Task UpdatePersonalBackground_ShouldCallRepository()
    {
        // Arrange
        var entity = new PersonalBackground { Id = Guid.NewGuid() };
        var service = new UpdatePersonalBackgroundService(_loggerMock.Object, _repositoryMock.Object);

        // Act
        await service.UpdatePersonalBackground(entity);

        // Assert
        _repositoryMock.Verify(repo => repo.Update(entity), Times.Once);
    }

    [Fact]
    public async Task UpdatePersonalBackground_ShouldThrowAndLogOnException()
    {
        _repositoryMock.Setup(repo => repo.Update(It.IsAny<PersonalBackground>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new UpdatePersonalBackgroundService(_loggerMock.Object, _repositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.UpdatePersonalBackground(new PersonalBackground()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    #endregion
}
