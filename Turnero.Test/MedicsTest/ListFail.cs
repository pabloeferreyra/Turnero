using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit.Sdk;

namespace Turnero.Test.MedicsTest;

public class ListFail
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private ApplicationDbContext _dbContext;
    private List<Medic> _testData;
    private MedicRepository _medicRepository;

    public ListFail()
    {
        _dbContext = CreateInMemoryDbContext();
        _mapperMock = new Mock<IMapper>();
        _cacheMock = new Mock<IMemoryCache>();
        _medicRepository = new MedicRepository(_dbContext, _mapperMock.Object, _cacheMock.Object);
    }

    [Fact]
    public void GetMedics()
    {
        var result = _medicRepository.FindAll().ToList();
        Assert.Empty(result);
    }

    //[Fact]
    //public void CreateMedics()
    //{

    //    var repository = new MedicRepository(_dbContext, _mapperMock.Object, _cacheMock.Object);

    //    var testData = new Medic { Name = null };
    //    _medicRepository.Create(testData);
    //    var result = repository.FindAll().ToList(); 
    //    Assert.Empty(result);
    //}

    private static ApplicationDbContext CreateInMemoryDbContext()
    {
        // Create an instance of ApplicationDbContext with an in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "MedicsTestListFail")
            .Options;

        return new ApplicationDbContext(options);
    }
}