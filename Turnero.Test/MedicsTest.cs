namespace Turnero.Test;

public class MedicsTest
{
      
    [Fact]
    public void Get_Medics_List_Ok()
    {
        var dbContext = CreateInMemoryDbContext();
        var mapperMock = new Mock<IMapper>();
        var cacheMock = new Mock<IMemoryCache>();
        var repository = new MedicRepository(dbContext, mapperMock.Object, cacheMock.Object);
        
        var testData = new List<Medic>
        {
            new() { Id = new Guid(), Name = "Cosme Fulanito 1" },
            new() { Id = new Guid(), Name = "Cosme Fulanito 2" },
            new() { Id = new Guid(), Name = "Cosme Fulanito 3" }
        };
        dbContext.AddRange(testData);
        dbContext.SaveChanges();

        var result = repository.FindAll().ToList();
        
        Assert.Equal(testData.Count, result.Count);
        Assert.Contains(result, q => q.Id == testData[0].Id);
        Assert.Contains(result, q => q.Id == testData[1].Id);
        Assert.Contains(result, q => q.Id == testData[2].Id);
    }

    [Fact]
    public void Create_Medics_Ok()
    {
        var dbContext = CreateInMemoryDbContext();
        var mapperMock = new Mock<IMapper>();
        var cacheMock = new Mock<IMemoryCache>();
        var repository = new MedicRepository(dbContext, mapperMock.Object, cacheMock.Object);

        var testData = new Medic { Id = new Guid("dad06826-6c2e-48a3-95be-883bf26fe6c4"), Name = "Cosme Fulanito" };
           
        dbContext.Add(testData);
        dbContext.SaveChanges();

        var result = repository.FindAll().ToList();

        Assert.True(result.Select(q => q.Id == new Guid("dad06826-6c2e-48a3-95be-883bf26fe6c4")).Any());
        Assert.True(result.Select(q => q.Name == testData.Name).Any());
    }

    [Fact]
    public void Delete_Medics_Ok()
    {
        var dbContext = CreateInMemoryDbContext();
        var mapperMock = new Mock<IMapper>();
        var cacheMock = new Mock<IMemoryCache>();
        var repository = new MedicRepository(dbContext, mapperMock.Object, cacheMock.Object);

        var testData = new List<Medic>
        {
            new() { Id = new Guid(), Name = "Cosme Fulanito1" },
            new() { Id = new Guid(), Name = "Cosme Fulanito2" },
            new() { Id = new Guid(), Name = "Cosme Fulanito3" }
        };
        dbContext.AddRange(testData);
        dbContext.SaveChanges();

        repository.DeleteMedic(testData[0]);
        dbContext.SaveChanges();

        var result = repository.FindAll();

        Assert.DoesNotContain(testData[0], result);
    }

    private static ApplicationDbContext CreateInMemoryDbContext()
    {
        // Create an instance of ApplicationDbContext with an in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "MedicsTest")
            .Options;

        return new ApplicationDbContext(options);
    }
}