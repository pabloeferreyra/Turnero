using Moq;
using Turnero.DAL.Models;
using Turnero.SL.Services.Repositories;
using Turnero.SL.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Turnero.SL.Services.MedicServices;

namespace Turnero.Test;

public class UpdateMedicServicesTests
{
    private readonly Mock<LoggerService> _loggerMock;
    private readonly Mock<IMedicRepository> _medicRepositoryMock;
    private readonly UpdateMedicServices _updateMedicServices;
    private readonly Medic medic = new() { Id = Guid.NewGuid(), Name = "Medic1" };
    public UpdateMedicServicesTests()
    {
        _loggerMock = new Mock<LoggerService>();
        _medicRepositoryMock = new Mock<IMedicRepository>();
        _updateMedicServices = new UpdateMedicServices(_loggerMock.Object, _medicRepositoryMock.Object);
    }

    [Fact]
    public async Task Update_ShouldCallUpdateMedicOnRepository()
    {
        // Act
        var result = await _updateMedicServices.Update(medic);

        // Assert
        _medicRepositoryMock.Verify(repo => repo.UpdateMedic(medic), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task Update_ShouldHandleDbUpdateConcurrencyException()
    {
        // Arrange
        _medicRepositoryMock.Setup(repo => repo.UpdateMedic(It.IsAny<Medic>())).Throws(new DbUpdateConcurrencyException("Test exception"));

        // Act
        var result = await _updateMedicServices.Update(medic);

        // Assert
        _loggerMock.Verify(logger => logger.Log(It.IsAny<string>()), Times.Once);
        Assert.False(result);
    }

    [Fact]
    public void Delete_ShouldCallDeleteMedicOnRepository()
    {
        // Act
        _updateMedicServices.Delete(medic);

        // Assert
        _medicRepositoryMock.Verify(repo => repo.DeleteMedic(medic), Times.Once);
    }

    [Fact]
    public void Delete_ShouldHandleDbUpdateConcurrencyException()
    {
        // Arrange
        _medicRepositoryMock.Setup(repo => repo.DeleteMedic(It.IsAny<Medic>())).Throws(new DbUpdateConcurrencyException("Test exception"));

        // Act
        _updateMedicServices.Delete(medic);

        // Assert
        _loggerMock.Verify(logger => logger.Log(It.IsAny<string>()), Times.Once);
    }
}