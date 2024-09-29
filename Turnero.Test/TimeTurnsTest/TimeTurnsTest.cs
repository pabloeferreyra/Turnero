using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turnero.Test.TimeTurnsTest
{
    public class TimeTurnsTest
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IMemoryCache> _cacheMock;
        private ApplicationDbContext _dbContext;
        private List<TimeTurn> _testData;
        private TimeTurnRepository _repository;

        public TimeTurnsTest() {
            _dbContext = CreateInMemoryDbContext();
            _mapperMock = new Mock<IMapper>();
            _cacheMock = new Mock<IMemoryCache>();
            _testData =
            [
                new() { Id = new Guid(), Time = "08:00" },
                new() { Id = new Guid(), Time = "08:05" },
                new() { Id = new Guid(), Time = "08:10" }
            ];
            _repository = new TimeTurnRepository(_dbContext, _mapperMock.Object, _cacheMock.Object);

            _dbContext.AddRange(_testData);
            _dbContext.SaveChanges();
        }
        [Fact]
        public void GetList()
        {
            var result = _repository.FindAll().ToList();

            Assert.Contains(result, q => q.Id == _testData[0].Id);
            Assert.Contains(result, q => q.Id == _testData[1].Id);
            Assert.Contains(result, q => q.Id == _testData[2].Id);
        }

        

        [Fact]
        public void Exists_Ok()
        {
            var result = _repository.Exists(_testData[0].Id);

            Assert.True(result);
        }

        [Fact]
        public void Exists_Fail()
        {
            var result = _repository.Exists(new Guid());

            Assert.False(result);
        }

        private static ApplicationDbContext CreateInMemoryDbContext()
        {
            // Create an instance of ApplicationDbContext with an in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TimeTurnsTest", new InMemoryDatabaseRoot())
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}
