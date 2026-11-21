using Moq;
using Turnero.Domain.Entities;
using Turnero.Application.Common;
using Turnero.Application.Services.MedicServices;
using Turnero.Application.Common.Interfaces;
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

    [Fact]
    public async Task GetAvailability_ReturnsMatchingResults()
    {
        var medicId = Guid.NewGuid();
        var date = new DateOnly(2026, 11, 16);
        var availability = new List<MedicAvailability>
        {
            new() { MedicId = medicId, DateFrom = date, DateTo = date, Day = date.DayOfWeek }
        };
        _medicRepositoryMock.Setup(repo => repo.GetAvailability(medicId, date))
            .ReturnsAsync(availability);

        var result = await _getMedicsServices.GetAvailability(medicId, date);

        Assert.Same(availability, result);
    }

    [Fact]
    public async Task GetAvailability_PropagatesRepositoryFailures()
    {
        var medicId = Guid.NewGuid();
        var date = new DateOnly(2026, 11, 16);
        _medicRepositoryMock.Setup(repo => repo.GetAvailability(medicId, date))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _getMedicsServices.GetAvailability(medicId, date));
        _loggerMock.Verify(logger => logger.Log("Database unavailable"), Times.Once);
    }

    [Fact]
    public async Task GetAvailabilities_ReturnsMedicSchedule()
    {
        var medicId = Guid.NewGuid();
        var availability = new List<MedicAvailability>
        {
            new() { MedicId = medicId, Day = DayOfWeek.Monday }
        };
        _medicRepositoryMock.Setup(repo => repo.GetAvailabilities(medicId))
            .ReturnsAsync(availability);

        var result = await _getMedicsServices.GetAvailabilities(medicId);

        Assert.Same(availability, result);
    }

    [Fact]
    public async Task SaveAvailabilities_PersistsCreatedUpdatedAndUnavailableEntries()
    {
        var medicId = Guid.NewGuid();
        var availabilities = new List<MedicAvailability>
        {
            new() { MedicId = medicId, Day = DayOfWeek.Monday }
        };
        var unavailableIds = new List<Guid> { Guid.NewGuid() };

        await _getMedicsServices.SaveAvailabilities(medicId, availabilities, unavailableIds);

        _medicRepositoryMock.Verify(repo =>
            repo.SaveAvailabilities(medicId, availabilities, unavailableIds), Times.Once);
    }
}
