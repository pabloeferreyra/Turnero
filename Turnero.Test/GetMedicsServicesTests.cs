using Moq;
using Turnero.DAL.Models;
using Turnero.SL.Services;
using Turnero.SL.Services.MedicServices;
using Turnero.SL.Services.Repositories;
using Xunit;

namespace Turnero.Test;

public class GetMedicsServicesTests
{
    private readonly Mock<LoggerService> _loggerMock;
    private readonly Mock<IMedicRepository> _medicRepositoryMock;
    private readonly GetMedicsServices _getMedicsServices;

    public GetMedicsServicesTests()
    {
        _loggerMock = new Mock<LoggerService>();
        _medicRepositoryMock = new Mock<IMedicRepository>();
        _getMedicsServices = new GetMedicsServices(_loggerMock.Object, _medicRepositoryMock.Object);
    }

    [Fact]
    public async Task GetMedicsDto_ShouldReturnMedicsDtoList()
    {
        // Arrange
        var medicsDto = new List<MedicDto> { new() { Id = Guid.NewGuid(), Name = "Medic1" } };
        _medicRepositoryMock.Setup(repo => repo.GetListDto()).ReturnsAsync(medicsDto);

        // Act
        var result = await _getMedicsServices.GetMedicsDto();

        // Assert
        Assert.Single(result);
        Assert.Equal("Medic1", result[0].Name);
    }

    [Fact]
    public async Task GetMedics_ShouldReturnMedicsList()
    {
        // Arrange
        var medics = new List<Medic> {
            new() { Id = Guid.NewGuid(), Name = "Medic1" },
            new() { Id = Guid.NewGuid(), Name = "Medic2" }
        };
        _medicRepositoryMock.Setup(repo => repo.GetList()).ReturnsAsync(medics);

        // Act
        var result = await _getMedicsServices.GetMedics();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal("Medic1", result[0].Name);
    }

    [Fact]
    public async Task GetMedicById_ShouldReturnMedic()
    {
        // Arrange
        var medicId = Guid.NewGuid();
        Medic medic = new() { Id = medicId, Name = "Medic1" };
        _medicRepositoryMock.Setup(repo => repo.GetById(medicId)).ReturnsAsync(medic);

        // Act
        var result = await _getMedicsServices.GetMedicById(medicId);

        // Assert
        Assert.Equal("Medic1", result.Name);
    }

    [Fact]
    public async Task GetMedicByUserId_ShouldReturnMedic()
    {
        // Arrange
        var userId = "user123";
        var medic = new Medic { Id = Guid.NewGuid(), Name = "Medic1", UserGuid = userId };
        _medicRepositoryMock.Setup(repo => repo.GetByUserId(userId)).ReturnsAsync(medic);

        // Act
        var result = await _getMedicsServices.GetMedicByUserId(userId);

        // Assert
        Assert.Equal(userId, result.UserGuid);
    }

    [Fact]
    public void ExistMedic_ShouldReturnTrueIfExists()
    {
        // Arrange
        var medicId = Guid.NewGuid();
        _medicRepositoryMock.Setup(repo => repo.Exists(medicId)).Returns(true);

        // Act
        var result = _getMedicsServices.ExistMedic(medicId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetCachedMedics_ShouldReturnCachedMedicsDtoList()
    {
        // Arrange
        var cachedMedicsDto = new List<MedicDto> { new() { Id = Guid.NewGuid(), Name = "Medic1" } };
        _medicRepositoryMock.Setup(repo => repo.GetCachedMedics()).ReturnsAsync(cachedMedicsDto);

        // Act
        var result = await _getMedicsServices.GetCachedMedics();

        // Assert
        Assert.Single(result);
        Assert.Equal("Medic1", result[0].Name);
    }
}
