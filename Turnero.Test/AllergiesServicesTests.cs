using Moq;
using Turnero.DAL.Models;
using Turnero.SL.Services;
using Turnero.SL.Services.AllergiesServices;
using Turnero.SL.Services.Repositories;
using Xunit;

namespace Turnero.Test;

public class AllergiesServicesTests
{
    private readonly Mock<LoggerService> _loggerMock;
    private readonly Mock<IAllergiesRepository> _allergiesRepositoryMock;

    public AllergiesServicesTests()
    {
        _loggerMock = new Mock<LoggerService>();
        _allergiesRepositoryMock = new Mock<IAllergiesRepository>();
    }

    #region GetAllergiesServices

    [Fact]
    public async Task GetAllergiesByPatient_ShouldReturnList()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var allergies = new List<Allergies> { new() { Id = Guid.NewGuid(), PatientId = patientId } };
        _allergiesRepositoryMock.Setup(repo => repo.GetAllergiesByPatient(patientId)).ReturnsAsync(allergies);
        var service = new GetAllergiesServices(_allergiesRepositoryMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetAllergiesByPatient(patientId);

        // Assert
        Assert.Single(result);
        Assert.Equal(patientId, result[0].PatientId);
    }

    [Fact]
    public async Task GetAllergiesByPatient_ShouldThrowOnException()
    {
        // Arrange
        _allergiesRepositoryMock.Setup(repo => repo.GetAllergiesByPatient(It.IsAny<Guid?>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new GetAllergiesServices(_allergiesRepositoryMock.Object, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.GetAllergiesByPatient(Guid.NewGuid()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetAllergies_ShouldReturnQueryable()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var allergies = new List<Allergies> { new() { Id = Guid.NewGuid(), PatientId = patientId } }.AsQueryable();
        _allergiesRepositoryMock.Setup(repo => repo.SearchAllergies(patientId)).ReturnsAsync(allergies);
        var service = new GetAllergiesServices(_allergiesRepositoryMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetAllergies(patientId);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetAllergies_ShouldThrowOnException()
    {
        // Arrange
        _allergiesRepositoryMock.Setup(repo => repo.SearchAllergies(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new GetAllergiesServices(_allergiesRepositoryMock.Object, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.GetAllergies(Guid.NewGuid()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Get_ShouldReturnAllergy()
    {
        // Arrange
        var id = Guid.NewGuid();
        var allergy = new Allergies { Id = id, PatientId = Guid.NewGuid() };
        _allergiesRepositoryMock.Setup(repo => repo.Get(id)).ReturnsAsync(allergy);
        var service = new GetAllergiesServices(_allergiesRepositoryMock.Object, _loggerMock.Object);

        // Act
        var result = await service.Get(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
    }

    [Fact]
    public async Task Get_ShouldReturnNullWhenNotFound()
    {
        // Arrange
        _allergiesRepositoryMock.Setup(repo => repo.Get(It.IsAny<Guid?>())).ReturnsAsync((Allergies?)null);
        var service = new GetAllergiesServices(_allergiesRepositoryMock.Object, _loggerMock.Object);

        // Act
        var result = await service.Get(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region InsertAllergiesServices

    [Fact]
    public async Task InsertAllergy_ShouldCallRepository()
    {
        // Arrange
        var allergy = new Allergies { Id = Guid.NewGuid(), PatientId = Guid.NewGuid() };
        var service = new InsertAllergiesServices(_allergiesRepositoryMock.Object, _loggerMock.Object);

        // Act
        await service.InsertAllergy(allergy);

        // Assert
        _allergiesRepositoryMock.Verify(repo => repo.CreateAllergy(allergy), Times.Once);
    }

    [Fact]
    public async Task InsertAllergy_ShouldThrowAndLogOnException()
    {
        // Arrange
        _allergiesRepositoryMock.Setup(repo => repo.CreateAllergy(It.IsAny<Allergies>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new InsertAllergiesServices(_allergiesRepositoryMock.Object, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.InsertAllergy(new Allergies()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region UpdateAllergiesServices

    [Fact]
    public async Task UpdateAllergy_ShouldCallRepository()
    {
        // Arrange
        var allergy = new Allergies { Id = Guid.NewGuid(), PatientId = Guid.NewGuid() };
        var service = new UpdateAllergiesServices(_allergiesRepositoryMock.Object, _loggerMock.Object);

        // Act
        await service.UpdateAllergy(allergy);

        // Assert
        _allergiesRepositoryMock.Verify(repo => repo.UpdateAllergy(allergy), Times.Once);
    }

    [Fact]
    public async Task UpdateAllergy_ShouldThrowAndLogOnException()
    {
        // Arrange
        _allergiesRepositoryMock.Setup(repo => repo.UpdateAllergy(It.IsAny<Allergies>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new UpdateAllergiesServices(_allergiesRepositoryMock.Object, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.UpdateAllergy(new Allergies()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region DeleteAllergiesServices

    [Fact]
    public void DeleteAllergy_ShouldCallRepository()
    {
        // Arrange
        var allergy = new Allergies { Id = Guid.NewGuid(), PatientId = Guid.NewGuid() };
        var service = new DeleteAllergiesServices(_allergiesRepositoryMock.Object, _loggerMock.Object);

        // Act
        service.DeleteAllergy(allergy);

        // Assert
        _allergiesRepositoryMock.Verify(repo => repo.DeleteAllergy(allergy), Times.Once);
    }

    [Fact]
    public void DeleteAllergy_ShouldThrowAndLogOnException()
    {
        // Arrange
        _allergiesRepositoryMock.Setup(repo => repo.DeleteAllergy(It.IsAny<Allergies>()))
            .Throws(new Exception("DB error"));
        var service = new DeleteAllergiesServices(_allergiesRepositoryMock.Object, _loggerMock.Object);

        // Act & Assert
        Assert.Throws<Exception>(() => service.DeleteAllergy(new Allergies()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    #endregion
}
