using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Turnero.Test.MedicsTest;

public class MedicsTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private ApplicationDbContext _dbContext;
    private List<Medic> _testData;
    private MedicRepository _repository;
    public MedicsTest()
    {
        _dbContext = CreateInMemoryDbContext();
        _mapperMock = new Mock<IMapper>();
        _cacheMock = new Mock<IMemoryCache>();
        _repository = new MedicRepository(_dbContext, _mapperMock.Object, _cacheMock.Object);
        _testData =
        [
            new() { Id = new Guid(), Name = "Cosme Fulanito 1" },
            new() { Id = new Guid(), Name = "Cosme Fulanito 2" },
            new() { Id = new Guid(), Name = "Cosme Fulanito 3" }
        ];
        _dbContext.AddRange(_testData);
        _dbContext.SaveChanges();
    }

    [Fact]
    public void GetMedics()
    {
        var result = _repository.FindAll().ToList();

        Assert.Equal(_testData.Count, result.Count);
        Assert.Contains(result, q => q.Id == _testData[0].Id);
        Assert.Contains(result, q => q.Id == _testData[1].Id);
        Assert.Contains(result, q => q.Id == _testData[2].Id);
    }

    [Fact]
    public void DeleteMedics()
    {
        _repository.DeleteMedic(_testData[0]);
        _dbContext.SaveChanges();

        var result = _repository.FindAll();

        Assert.DoesNotContain(_testData[0], result);
    }

    [Fact]
    public async Task CreateMedics()
    {
        var testData = new Medic { Id = new Guid("dad06826-6c2e-48a3-95be-883bf26fe6c4"), Name = "Cosme Fulanito" };

        await _repository.NewMedic(testData);

        var result = _repository.FindAll().ToList();

        Assert.True(result.Select(q => q.Id == new Guid("dad06826-6c2e-48a3-95be-883bf26fe6c4")).Any());
        Assert.True(result.Select(q => q.Name == testData.Name).Any());
    }

    [Fact]
    public async void GetMedic_Fail()
    {
        var result = await _repository.GetById(new Guid());
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateMedics_Fail()
    {
        _dbContext.Database.EnsureDeleted();

        var testData = new Medic { };
        await _repository.NewMedic(testData);
        var result = _repository.FindAll().ToList();
        Assert.Empty(result);
    }

    private static ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "MedicsTest", new InMemoryDatabaseRoot())
            .Options;

        return new ApplicationDbContext(options);
    }
}