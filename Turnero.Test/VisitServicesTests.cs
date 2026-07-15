using Moq;
using Turnero.DAL.Models;
using Turnero.SL.Services;
using Turnero.SL.Services.Repositories;
using Turnero.SL.Services.VisitServices;
using Xunit;

namespace Turnero.Test;

public class VisitServicesTests
{
    private readonly Mock<LoggerService> _loggerMock;
    private readonly Mock<IVisitRepository> _visitRepositoryMock;

    public VisitServicesTests()
    {
        _loggerMock = new Mock<LoggerService>();
        _visitRepositoryMock = new Mock<IVisitRepository>();
    }

    #region GetVisitService

    [Fact]
    public async Task SearchVisits_ShouldReturnQueryable()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var visits = new List<VisitDTO> { new() { Id = Guid.NewGuid() } }.AsQueryable();
        _visitRepositoryMock.Setup(repo => repo.SearchVisits(patientId)).ReturnsAsync(visits);
        var service = new GetVisitService(_loggerMock.Object, _visitRepositoryMock.Object);

        // Act
        var result = await service.SearchVisits(patientId);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task SearchVisits_ShouldThrowOnException()
    {
        _visitRepositoryMock.Setup(repo => repo.SearchVisits(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new GetVisitService(_loggerMock.Object, _visitRepositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.SearchVisits(Guid.NewGuid()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Get_ShouldReturnVisit()
    {
        // Arrange
        var id = Guid.NewGuid();
        var visit = new Visit { Id = id, PatientId = Guid.NewGuid() };
        _visitRepositoryMock.Setup(repo => repo.Get(id)).ReturnsAsync(visit);
        var service = new GetVisitService(_loggerMock.Object, _visitRepositoryMock.Object);

        // Act
        var result = await service.Get(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
    }

    [Fact]
    public async Task Get_ShouldReturnNullOnException()
    {
        _visitRepositoryMock.Setup(repo => repo.Get(It.IsAny<Guid?>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new GetVisitService(_loggerMock.Object, _visitRepositoryMock.Object);

        var result = await service.Get(Guid.NewGuid());

        Assert.Null(result);
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region InsertVisitService

    [Fact]
    public async Task Create_ShouldCallRepository()
    {
        // Arrange
        var visit = new Visit { Id = Guid.NewGuid(), PatientId = Guid.NewGuid() };
        var service = new InsertVisitService(_loggerMock.Object, _visitRepositoryMock.Object);

        // Act
        await service.Create(visit);

        // Assert
        _visitRepositoryMock.Verify(repo => repo.CreateVisit(visit), Times.Once);
    }

    [Fact]
    public async Task Create_ShouldLogOnException()
    {
        _visitRepositoryMock.Setup(repo => repo.CreateVisit(It.IsAny<Visit>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new InsertVisitService(_loggerMock.Object, _visitRepositoryMock.Object);

        // Should not throw (service swallows exceptions)
        await service.Create(new Visit());

        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    #endregion
}
