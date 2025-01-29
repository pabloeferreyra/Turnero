using Moq;
using Turnero.SL.Services.Interfaces;
using Turnero.SL.Services.Repositories;
using Turnero.SL.Services;
using Turnero.DAL.Models;
using Xunit;

namespace Turnero.Test;

public class InsertMedicServicesTests
{
    private readonly Mock<ILoggerServices> _loggerMock;
    private readonly Mock<IMedicRepository> _medicRepositoryMock;
    private readonly InsertMedicServices _insertMedicServices;

    public InsertMedicServicesTests()
    {
        _loggerMock = new Mock<ILoggerServices>();
        _medicRepositoryMock = new Mock<IMedicRepository>();
        _insertMedicServices = new InsertMedicServices(_loggerMock.Object, _medicRepositoryMock.Object);
    }

    [Fact]
    public async Task Create_ShouldCallNewMedicOnRepository()
    {
        // Arrange
        var medic = new Medic { Id = Guid.NewGuid(), Name = "Medic1" };

        // Act
        await _insertMedicServices.Create(medic);

        // Assert
        _medicRepositoryMock.Verify(repo => repo.NewMedic(medic), Times.Once);
    }

    [Fact]
    public async Task Create_ShouldLogInfoMessage()
    {
        // Arrange
        var medic = new Medic { Id = Guid.NewGuid(), Name = "Medic1" };

        // Act
        await _insertMedicServices.Create(medic);

        // Assert
        _loggerMock.Verify(logger => logger.Info(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Create_ShouldHandleException()
    {
        // Arrange
        var medic = new Medic { Id = Guid.NewGuid(), Name = "Medic1" };
        _medicRepositoryMock.Setup(repo => repo.NewMedic(It.IsAny<Medic>())).Throws(new Exception("Test exception"));

        // Act
        await _insertMedicServices.Create(medic);

        // Assert
        _loggerMock.Verify(logger => logger.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
    }
}
