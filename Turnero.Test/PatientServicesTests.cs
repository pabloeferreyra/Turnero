using Moq;
using Turnero.DAL.Models;
using Turnero.SL.Services;
using Turnero.SL.Services.PatientServices;
using Turnero.SL.Services.Repositories;
using Xunit;

namespace Turnero.Test;

public class PatientServicesTests
{
    private readonly Mock<LoggerService> _loggerMock;
    private readonly Mock<IPatientRepository> _patientRepositoryMock;

    public PatientServicesTests()
    {
        _loggerMock = new Mock<LoggerService>();
        _patientRepositoryMock = new Mock<IPatientRepository>();
    }

    #region GetPatientService

    [Fact]
    public async Task GetPatients_ShouldReturnList()
    {
        // Arrange
        var patients = new List<PatientDTO> { new() { Id = Guid.NewGuid(), Name = "Test" } };
        _patientRepositoryMock.Setup(repo => repo.GetList()).ReturnsAsync(patients);
        var service = new GetPatientService(_loggerMock.Object, _patientRepositoryMock.Object);

        // Act
        var result = await service.GetPatients();

        // Assert
        Assert.Single(result);
        Assert.Equal("Test", result[0].Name);
    }

    [Fact]
    public async Task GetPatients_ShouldThrowOnException()
    {
        _patientRepositoryMock.Setup(repo => repo.GetList())
            .ThrowsAsync(new Exception("DB error"));
        var service = new GetPatientService(_loggerMock.Object, _patientRepositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.GetPatients());
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void GetAllPatients_ShouldReturnQueryable()
    {
        // Arrange
        var patients = new List<PatientDTO> { new() { Id = Guid.NewGuid(), Name = "Test" } }.AsQueryable();
        _patientRepositoryMock.Setup(repo => repo.GetAll()).Returns(patients);
        var service = new GetPatientService(_loggerMock.Object, _patientRepositoryMock.Object);

        // Act
        var result = service.GetAllPatients();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public void GetAllPatients_ShouldThrowOnException()
    {
        _patientRepositoryMock.Setup(repo => repo.GetAll())
            .Throws(new Exception("DB error"));
        var service = new GetPatientService(_loggerMock.Object, _patientRepositoryMock.Object);

        Assert.Throws<Exception>(() => service.GetAllPatients());
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetPatientById_ShouldReturnPatient()
    {
        // Arrange
        var id = Guid.NewGuid();
        var patient = new Patient { Id = id, Name = "Test" };
        _patientRepositoryMock.Setup(repo => repo.GetById(id)).ReturnsAsync(patient);
        var service = new GetPatientService(_loggerMock.Object, _patientRepositoryMock.Object);

        // Act
        var result = await service.GetPatientById(id);

        // Assert
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public async Task GetPatientById_ShouldThrowOnInvalidOperation()
    {
        _patientRepositoryMock.Setup(repo => repo.GetById(It.IsAny<Guid>()))
            .ThrowsAsync(new InvalidOperationException("Not found"));
        var service = new GetPatientService(_loggerMock.Object, _patientRepositoryMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetPatientById(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetPatientById_ShouldThrowAndLogOnException()
    {
        _patientRepositoryMock.Setup(repo => repo.GetById(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new GetPatientService(_loggerMock.Object, _patientRepositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.GetPatientById(Guid.NewGuid()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SearchPatients_ShouldReturnQueryable()
    {
        // Arrange
        var patients = new List<PatientDTO> { new() { Id = Guid.NewGuid(), Name = "Juan" } }.AsQueryable();
        _patientRepositoryMock.Setup(repo => repo.SearchByNameOrDni("Juan")).ReturnsAsync(patients);
        var service = new GetPatientService(_loggerMock.Object, _patientRepositoryMock.Object);

        // Act
        var result = await service.SearchPatients("Juan");

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task SearchPatients_ShouldThrowOnException()
    {
        _patientRepositoryMock.Setup(repo => repo.SearchByNameOrDni(It.IsAny<string>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new GetPatientService(_loggerMock.Object, _patientRepositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.SearchPatients("test"));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region InsertPatientService

    [Fact]
    public async Task InsertPatient_ShouldCallRepository()
    {
        // Arrange
        var patient = new Patient { Id = Guid.NewGuid(), Name = "Test" };
        _patientRepositoryMock.Setup(repo => repo.NewPatient(It.IsAny<Patient>()))
            .Returns(Task.CompletedTask);
        var service = new InsertPatientService(_loggerMock.Object, _patientRepositoryMock.Object);

        // Act
        await service.InsertPatient(patient);

        // Assert
        _patientRepositoryMock.Verify(repo => repo.NewPatient(It.Is<Patient>(p =>
            p.Name == "Test" &&
            p.PersonalBackground != null &&
            p.PerinatalBackground != null &&
            p.Parent != null &&
            p.CongErrors != null)), Times.Once);
    }

    [Fact]
    public async Task InsertPatient_ShouldSetNewIdAndDefaults()
    {
        // Arrange
        var originalId = Guid.NewGuid();
        var patient = new Patient { Id = originalId, Name = "Test" };
        _patientRepositoryMock.Setup(repo => repo.NewPatient(It.IsAny<Patient>()))
            .Returns(Task.CompletedTask);
        var service = new InsertPatientService(_loggerMock.Object, _patientRepositoryMock.Object);

        // Act
        await service.InsertPatient(patient);

        // Assert - Id should be reassigned to a new GUID
        Assert.NotEqual(originalId, patient.Id);
        Assert.NotNull(patient.PersonalBackground);
        Assert.NotNull(patient.PerinatalBackground);
        Assert.NotNull(patient.Parent);
        Assert.NotNull(patient.CongErrors);
    }

    [Fact]
    public async Task InsertPatient_ShouldThrowAndLogOnException()
    {
        _patientRepositoryMock.Setup(repo => repo.NewPatient(It.IsAny<Patient>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new InsertPatientService(_loggerMock.Object, _patientRepositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.InsertPatient(new Patient()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region UpdatePatientService

    [Fact]
    public async Task UpdatePatient_ShouldCallRepository()
    {
        // Arrange
        var patient = new Patient { Id = Guid.NewGuid(), Name = "Test" };
        var service = new UpdatePatientService(_loggerMock.Object, _patientRepositoryMock.Object);

        // Act
        await service.UpdatePatient(patient);

        // Assert
        _patientRepositoryMock.Verify(repo => repo.UpdatePatient(patient), Times.Once);
    }

    [Fact]
    public async Task UpdatePatient_ShouldThrowAndLogOnException()
    {
        _patientRepositoryMock.Setup(repo => repo.UpdatePatient(It.IsAny<Patient>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new UpdatePatientService(_loggerMock.Object, _patientRepositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.UpdatePatient(new Patient()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeletePatient_ShouldCallRepository()
    {
        // Arrange
        var id = Guid.NewGuid();
        var patient = new Patient { Id = id, Name = "Test" };
        _patientRepositoryMock.Setup(repo => repo.GetById(id)).ReturnsAsync(patient);
        var service = new UpdatePatientService(_loggerMock.Object, _patientRepositoryMock.Object);

        // Act
        await service.DeletePatient(id);

        // Assert
        _patientRepositoryMock.Verify(repo => repo.DeletePatient(patient), Times.Once);
    }

    [Fact]
    public async Task DeletePatient_ShouldThrowWhenNotFound()
    {
        _patientRepositoryMock.Setup(repo => repo.GetById(It.IsAny<Guid>()))
            .ReturnsAsync((Patient?)null);
        var service = new UpdatePatientService(_loggerMock.Object, _patientRepositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.DeletePatient(Guid.NewGuid()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    #endregion
}
