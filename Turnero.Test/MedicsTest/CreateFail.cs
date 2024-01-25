using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Turnero.Test.MedicsTest;

public class CreateFail
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private ApplicationDbContext _dbContext;

    public CreateFail()
    {
        _dbContext = CreateInMemoryDbContext();
        _mapperMock = new Mock<IMapper>();
        _cacheMock = new Mock<IMemoryCache>();

    }



    private static ApplicationDbContext CreateInMemoryDbContext()
    {
        // Create an instance of ApplicationDbContext with an in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "MedicsTestCreateFail")
            .Options;

        return new ApplicationDbContext(options);
    }
}