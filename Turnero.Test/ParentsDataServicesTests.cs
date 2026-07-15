using Moq;
using Turnero.DAL.Models;
using Turnero.SL.Services;
using Turnero.SL.Services.ParentsDataServices;
using Turnero.SL.Services.Repositories;
using Xunit;

namespace Turnero.Test;

public class ParentsDataServicesTests
{
    private readonly Mock<LoggerService> _loggerMock;
    private readonly Mock<IParentsDataRepository> _repositoryMock;

    public ParentsDataServicesTests()
    {
        _loggerMock = new Mock<LoggerService>();
        _repositoryMock = new Mock<IParentsDataRepository>();
    }

    #region GetParentsDataService

    [Fact]
    public async Task GetParentsData_ShouldReturnEntity()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new ParentsData { Id = id };
        _repositoryMock.Setup(repo => repo.Get(id)).ReturnsAsync(entity);
        var service = new GetParentsDataService(_repositoryMock.Object);

        // Act
        var result = await service.GetParentsData(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
    }

    [Fact]
    public async Task GetParentsData_ShouldReturnNullWhenNotFound()
    {
        _repositoryMock.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync((ParentsData?)null);
        var service = new GetParentsDataService(_repositoryMock.Object);

        var result = await service.GetParentsData(Guid.NewGuid());

        Assert.Null(result);
    }

    #endregion

    #region UpdateParentsDataService

    [Fact]
    public async Task UpdateParentsData_ShouldCallRepository()
    {
        // Arrange
        var entity = new ParentsData { Id = Guid.NewGuid() };
        var service = new UpdateParentsDataService(_loggerMock.Object, _repositoryMock.Object);

        // Act
        await service.UpdateParentsData(entity);

        // Assert
        _repositoryMock.Verify(repo => repo.Update(entity), Times.Once);
    }

    [Fact]
    public async Task UpdateParentsData_ShouldThrowAndLogOnException()
    {
        _repositoryMock.Setup(repo => repo.Update(It.IsAny<ParentsData>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new UpdateParentsDataService(_loggerMock.Object, _repositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.UpdateParentsData(new ParentsData()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region DeleteParentsDataService

    [Fact]
    public void DeleteParentsData_ShouldCallRepository()
    {
        // Arrange
        var entity = new ParentsData { Id = Guid.NewGuid() };
        var service = new DeleteParentsDataService(_repositoryMock.Object);

        // Act
        service.DeleteParentsData(entity);

        // Assert
        _repositoryMock.Verify(repo => repo.Delete(entity), Times.Once);
    }

    [Fact]
    public void DeleteParentsData_ShouldThrowWhenNull()
    {
        var service = new DeleteParentsDataService(_repositoryMock.Object);

        Assert.Throws<ArgumentNullException>(() => service.DeleteParentsData(null!));
    }

    #endregion
}
