using Moq;
using Turnero.DAL.Models;
using Turnero.SL.Services;
using Turnero.SL.Services.VaccinesServices;
using Turnero.SL.Services.Repositories;
using Xunit;

namespace Turnero.Test;

public class VaccinesServicesTests
{
    private readonly Mock<LoggerService> _loggerMock;
    private readonly Mock<IVaccinesRepository> _vaccinesRepositoryMock;

    public VaccinesServicesTests()
    {
        _loggerMock = new Mock<LoggerService>();
        _vaccinesRepositoryMock = new Mock<IVaccinesRepository>();
    }

    #region GetVaccinesServices

    [Fact]
    public async Task Get_ShouldReturnVaccine()
    {
        // Arrange
        var id = Guid.NewGuid();
        var vaccine = new Vaccines { Id = id, PatientId = Guid.NewGuid(), Description = "BCG" };
        _vaccinesRepositoryMock.Setup(repo => repo.Get(id)).ReturnsAsync(vaccine);
        var service = new GetVaccinesServices(_loggerMock.Object, _vaccinesRepositoryMock.Object);

        // Act
        var result = await service.Get(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("BCG", result!.Description);
    }

    [Fact]
    public async Task Get_ShouldReturnNullWhenNotFound()
    {
        _vaccinesRepositoryMock.Setup(repo => repo.Get(It.IsAny<Guid?>())).ReturnsAsync((Vaccines?)null);
        var service = new GetVaccinesServices(_loggerMock.Object, _vaccinesRepositoryMock.Object);

        var result = await service.Get(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task Get_ShouldThrowOnException()
    {
        _vaccinesRepositoryMock.Setup(repo => repo.Get(It.IsAny<Guid?>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new GetVaccinesServices(_loggerMock.Object, _vaccinesRepositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.Get(Guid.NewGuid()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetByPatientId_ShouldReturnList()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var vaccines = new List<Vaccines> { new() { Id = Guid.NewGuid(), PatientId = patientId, Description = "Polio" } };
        _vaccinesRepositoryMock.Setup(repo => repo.GetByPatientId(patientId)).ReturnsAsync(vaccines);
        var service = new GetVaccinesServices(_loggerMock.Object, _vaccinesRepositoryMock.Object);

        // Act
        var result = await service.GetByPatientId(patientId);

        // Assert
        Assert.Single(result);
        Assert.Equal("Polio", result[0].Description);
    }

    [Fact]
    public async Task GetByPatientId_ShouldThrowOnException()
    {
        _vaccinesRepositoryMock.Setup(repo => repo.GetByPatientId(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new GetVaccinesServices(_loggerMock.Object, _vaccinesRepositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.GetByPatientId(Guid.NewGuid()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region InsertVaccinesServices

    [Fact]
    public async Task Insert_ShouldCallRepository()
    {
        // Arrange
        var dto = new VaccinesDto { Description = "BCG", DateApplied = DateOnly.FromDateTime(DateTime.Today), PatientId = Guid.NewGuid() };
        var service = new InsertVaccinesServices(_loggerMock.Object, _vaccinesRepositoryMock.Object);

        // Act
        await service.Insert(dto);

        // Assert
        _vaccinesRepositoryMock.Verify(repo => repo.Insert(It.Is<Vaccines>(v =>
            v.Description == "BCG" && v.PatientId == dto.PatientId)), Times.Once);
    }

    [Fact]
    public async Task Insert_ShouldUseOtherDescriptionWhenDescriptionIsOtra()
    {
        // Arrange
        var dto = new VaccinesDto { Description = "Otra", OtherDescription = "Custom vaccine", DateApplied = DateOnly.FromDateTime(DateTime.Today), PatientId = Guid.NewGuid() };
        var service = new InsertVaccinesServices(_loggerMock.Object, _vaccinesRepositoryMock.Object);

        // Act
        await service.Insert(dto);

        // Assert
        _vaccinesRepositoryMock.Verify(repo => repo.Insert(It.Is<Vaccines>(v =>
            v.Description == "Custom vaccine")), Times.Once);
    }

    [Fact]
    public async Task Insert_ShouldThrowOnException()
    {
        _vaccinesRepositoryMock.Setup(repo => repo.Insert(It.IsAny<Vaccines>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new InsertVaccinesServices(_loggerMock.Object, _vaccinesRepositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.Insert(new VaccinesDto()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region UpdateVaccinesServices

    [Fact]
    public async Task Update_ShouldCallRepository()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new VaccinesDto { Id = id, Description = "Polio", DateApplied = DateOnly.FromDateTime(DateTime.Today), PatientId = Guid.NewGuid() };
        var service = new UpdateVaccinesServices(_loggerMock.Object, _vaccinesRepositoryMock.Object);

        // Act
        await service.Update(dto);

        // Assert
        _vaccinesRepositoryMock.Verify(repo => repo.Update(It.Is<Vaccines>(v =>
            v.Id == id && v.Description == "Polio")), Times.Once);
    }

    [Fact]
    public async Task Update_ShouldUseOtherDescriptionWhenDescriptionIsOtra()
    {
        var dto = new VaccinesDto { Id = Guid.NewGuid(), Description = "Otra", OtherDescription = "Custom", DateApplied = DateOnly.FromDateTime(DateTime.Today), PatientId = Guid.NewGuid() };
        var service = new UpdateVaccinesServices(_loggerMock.Object, _vaccinesRepositoryMock.Object);

        await service.Update(dto);

        _vaccinesRepositoryMock.Verify(repo => repo.Update(It.Is<Vaccines>(v =>
            v.Description == "Custom")), Times.Once);
    }

    [Fact]
    public async Task Update_ShouldThrowOnException()
    {
        _vaccinesRepositoryMock.Setup(repo => repo.Update(It.IsAny<Vaccines>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new UpdateVaccinesServices(_loggerMock.Object, _vaccinesRepositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.Update(new VaccinesDto()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region DeleteVacinesServices

    [Fact]
    public async Task Delete_ShouldCallRepository()
    {
        // Arrange
        var id = Guid.NewGuid();
        var vaccine = new Vaccines { Id = id, PatientId = Guid.NewGuid() };
        _vaccinesRepositoryMock.Setup(repo => repo.Get(id)).ReturnsAsync(vaccine);
        var service = new DeleteVacinesServices(_loggerMock.Object, _vaccinesRepositoryMock.Object);

        // Act
        await service.Delete(id);

        // Assert
        _vaccinesRepositoryMock.Verify(repo => repo.Remove(vaccine), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldThrowWhenNotFound()
    {
        _vaccinesRepositoryMock.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync((Vaccines?)null);
        var service = new DeleteVacinesServices(_loggerMock.Object, _vaccinesRepositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.Delete(Guid.NewGuid()));
    }

    [Fact]
    public async Task Delete_ShouldThrowOnException()
    {
        _vaccinesRepositoryMock.Setup(repo => repo.Get(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("DB error"));
        var service = new DeleteVacinesServices(_loggerMock.Object, _vaccinesRepositoryMock.Object);

        await Assert.ThrowsAsync<Exception>(() => service.Delete(Guid.NewGuid()));
        _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    #endregion
}
