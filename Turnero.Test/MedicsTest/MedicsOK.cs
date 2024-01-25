using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Turnero.Test.MedicsTest;

public class MedicsOK
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private ApplicationDbContext _dbContext;
    private List<Medic> _testData;

    public MedicsOK()
    {
        _dbContext = CreateInMemoryDbContext();
        _mapperMock = new Mock<IMapper>();
        _cacheMock = new Mock<IMemoryCache>();

        _testData =
        [
            new() { Id = new Guid(), Name = "Cosme Fulanito 1" },
            new() { Id = new Guid(), Name = "Cosme Fulanito 2" },
            new() { Id = new Guid(), Name = "Cosme Fulanito 3" }
        ];
    }

    [Fact]
    public void GetMedics()
    {
        var repository = new MedicRepository(_dbContext, _mapperMock.Object, _cacheMock.Object);


        _dbContext.AddRange(_testData);
        _dbContext.SaveChanges();

        var result = repository.FindAll().ToList();

        Assert.Equal(_testData.Count, result.Count);
        Assert.Contains(result, q => q.Id == _testData[0].Id);
        Assert.Contains(result, q => q.Id == _testData[1].Id);
        Assert.Contains(result, q => q.Id == _testData[2].Id);
    }

    [Fact]
    public void DeleteMedics()
    {
        var repository = new MedicRepository(_dbContext, _mapperMock.Object, _cacheMock.Object);

        _dbContext.AddRange(_testData);
        _dbContext.SaveChanges();

        repository.DeleteMedic(_testData[0]);
        _dbContext.SaveChanges();

        var result = repository.FindAll();

        Assert.DoesNotContain(_testData[0], result);
    }

    [Fact]
    public void CreateMedics()
    {
        var repository = new MedicRepository(_dbContext, _mapperMock.Object, _cacheMock.Object);

        var testData = new Medic { Id = new Guid("dad06826-6c2e-48a3-95be-883bf26fe6c4"), Name = "Cosme Fulanito" };

        _dbContext.Add(testData);
        _dbContext.SaveChanges();

        var result = repository.FindAll().ToList();

        Assert.True(result.Select(q => q.Id == new Guid("dad06826-6c2e-48a3-95be-883bf26fe6c4")).Any());
        Assert.True(result.Select(q => q.Name == testData.Name).Any());
    }

    private static ApplicationDbContext CreateInMemoryDbContext()
    {
        // Create an instance of ApplicationDbContext with an in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "MedicsTestListOK")
            .Options;

        return new ApplicationDbContext(options);
    }
}