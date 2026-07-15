using Moq;
using Turnero.DAL.Models;
using Turnero.SL.Services.PermMedServices;
using Turnero.SL.Services.Repositories;
using Xunit;

namespace Turnero.Test;

public class PermMedServicesTests
{
    private readonly Mock<IPermMedRepository> _permMedRepositoryMock;

    public PermMedServicesTests()
    {
        _permMedRepositoryMock = new Mock<IPermMedRepository>();
    }

    #region GetPermMedService

    [Fact]
    public async Task Get_ShouldReturnList()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var permMeds = new List<PermMed> { new() { Id = Guid.NewGuid(), PatientId = patientId } };
        _permMedRepositoryMock.Setup(repo => repo.GetByPatientId(patientId)).ReturnsAsync(permMeds);
        var service = new GetPermMedService(_permMedRepositoryMock.Object);

        // Act
        var result = await service.Get(patientId);

        // Assert
        Assert.Single(result);
        Assert.Equal(patientId, result[0].PatientId);
    }

    [Fact]
    public async Task Get_ShouldReturnEmptyList()
    {
        _permMedRepositoryMock.Setup(repo => repo.GetByPatientId(It.IsAny<Guid>()))
            .ReturnsAsync(new List<PermMed>());
        var service = new GetPermMedService(_permMedRepositoryMock.Object);

        var result = await service.Get(Guid.NewGuid());

        Assert.Empty(result);
    }

    #endregion

    #region InsertPermMedService

    [Fact]
    public async Task Create_ShouldCallRepository()
    {
        // Arrange
        var permMed = new PermMed { Id = Guid.NewGuid(), PatientId = Guid.NewGuid() };
        var service = new InsertPermMedService(_permMedRepositoryMock.Object);

        // Act
        await service.Create(permMed);

        // Assert
        _permMedRepositoryMock.Verify(repo => repo.Insert(permMed), Times.Once);
    }

    #endregion

    #region DeletePermMedService

    [Fact]
    public async Task Delete_ShouldCallRepository()
    {
        // Arrange
        var id = Guid.NewGuid();
        var service = new DeletePermMedService(_permMedRepositoryMock.Object);

        // Act
        await service.Delete(id);

        // Assert
        _permMedRepositoryMock.Verify(repo => repo.Remove(id), Times.Once);
    }

    #endregion
}
