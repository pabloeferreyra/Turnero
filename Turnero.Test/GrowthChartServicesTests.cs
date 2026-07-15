using Moq;
using Turnero.DAL.Models;
using Turnero.SL.Services.GrowthChartServices;
using Turnero.SL.Services.Repositories;
using Xunit;

namespace Turnero.Test;

public class GrowthChartServicesTests
{
    private readonly Mock<IGrowthChartRepository> _repositoryMock;

    public GrowthChartServicesTests()
    {
        _repositoryMock = new Mock<IGrowthChartRepository>();
    }

    #region GetGrowthChartService

    [Fact]
    public async Task Get_ShouldReturnList()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var charts = new List<GrowthChart> { new() { Id = Guid.NewGuid(), PatientId = patientId } };
        _repositoryMock.Setup(repo => repo.GetByPatientId(patientId)).ReturnsAsync(charts);
        var service = new GetGrowthChartService(_repositoryMock.Object);

        // Act
        var result = await service.Get(patientId);

        // Assert
        Assert.Single(result);
        Assert.Equal(patientId, result[0].PatientId);
    }

    [Fact]
    public async Task GetById_ShouldReturnChart()
    {
        // Arrange
        var id = Guid.NewGuid();
        var chart = new GrowthChart { Id = id, PatientId = Guid.NewGuid() };
        _repositoryMock.Setup(repo => repo.GetById(id)).ReturnsAsync(chart);
        var service = new GetGrowthChartService(_repositoryMock.Object);

        // Act
        var result = await service.GetById(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNullWhenNotFound()
    {
        _repositoryMock.Setup(repo => repo.GetById(It.IsAny<Guid>())).ReturnsAsync((GrowthChart?)null);
        var service = new GetGrowthChartService(_repositoryMock.Object);

        var result = await service.GetById(Guid.NewGuid());

        Assert.Null(result);
    }

    #endregion

    #region InsertGrowthChartService

    [Fact]
    public async Task Create_ShouldCallRepository()
    {
        // Arrange
        var chart = new GrowthChart { Id = Guid.NewGuid(), PatientId = Guid.NewGuid() };
        var service = new InsertGrowthChartService(_repositoryMock.Object);

        // Act
        await service.Create(chart);

        // Assert
        _repositoryMock.Verify(repo => repo.Insert(chart), Times.Once);
    }

    #endregion

    #region UpdateGrowthChartService

    [Fact]
    public async Task Edit_ShouldCallRepository()
    {
        // Arrange
        var chart = new GrowthChart { Id = Guid.NewGuid(), PatientId = Guid.NewGuid() };
        var service = new UpdateGrowthChartService(_repositoryMock.Object);

        // Act
        await service.Edit(chart);

        // Assert
        _repositoryMock.Verify(repo => repo.Edit(chart), Times.Once);
    }

    #endregion

    #region DeleteGrowthChartService

    [Fact]
    public async Task Delete_ShouldCallRepository()
    {
        // Arrange
        var id = Guid.NewGuid();
        var service = new DeleteGrowthChartService(_repositoryMock.Object);

        // Act
        await service.Delete(id);

        // Assert
        _repositoryMock.Verify(repo => repo.Remove(id), Times.Once);
    }

    #endregion
}
