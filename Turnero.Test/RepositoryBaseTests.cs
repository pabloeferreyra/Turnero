using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Turnero.SL.Services.Repositories;
using Xunit;

namespace Turnero.Test
{
    public class RepositoryBaseTests
    {
        private readonly Mock<IRepositoryBase<TestEntity>> _repositoryMock;

        public RepositoryBaseTests()
        {
            _repositoryMock = new Mock<IRepositoryBase<TestEntity>>();
        }

        [Fact]
        public void FindAll_ShouldReturnAllEntities()
        {
            // Arrange
            var entities = new List<TestEntity> { new(), new() }.AsQueryable();
            _repositoryMock.Setup(repo => repo.FindAll()).Returns(entities);

            // Act
            var result = _repositoryMock.Object.FindAll();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void FindByCondition_ShouldReturnFilteredEntities()
        {
            // Arrange
            var entities = new List<TestEntity>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            }.AsQueryable();
            _repositoryMock.Setup(repo => repo.FindByCondition(It.IsAny<Expression<Func<TestEntity, bool>>>()))
                           .Returns((Expression<Func<TestEntity, bool>> expr) => entities.Where(expr));

            // Act
            var result = _repositoryMock.Object.FindByCondition(e => e.Id == 1);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result.First().Id);
        }

        [Fact]
        public void Create_ShouldAddEntity()
        {
            // Arrange
            var entity = new TestEntity();
            _repositoryMock.Setup(repo => repo.Create(entity));

            // Act
            _repositoryMock.Object.Create(entity);

            // Assert
            _repositoryMock.Verify(repo => repo.Create(entity), Times.Once);
        }

        [Fact]
        public void Update_ShouldModifyEntity()
        {
            // Arrange
            var entity = new TestEntity();
            _repositoryMock.Setup(repo => repo.Update(entity));

            // Act
            _repositoryMock.Object.Update(entity);

            // Assert
            _repositoryMock.Verify(repo => repo.Update(entity), Times.Once);
        }

        [Fact]
        public void Delete_ShouldRemoveEntity()
        {
            // Arrange
            var entity = new TestEntity();
            _repositoryMock.Setup(repo => repo.Delete(entity));

            // Act
            _repositoryMock.Object.Delete(entity);

            // Assert
            _repositoryMock.Verify(repo => repo.Delete(entity), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddEntityAsync()
        {
            // Arrange
            var entity = new TestEntity();
            _repositoryMock.Setup(repo => repo.CreateAsync(entity)).Returns(Task.CompletedTask);

            // Act
            await _repositoryMock.Object.CreateAsync(entity);

            // Assert
            _repositoryMock.Verify(repo => repo.CreateAsync(entity), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyEntityAsync()
        {
            // Arrange
            var entity = new TestEntity();
            _repositoryMock.Setup(repo => repo.UpdateAsync(entity)).Returns(Task.CompletedTask);

            // Act
            await _repositoryMock.Object.UpdateAsync(entity);

            // Assert
            _repositoryMock.Verify(repo => repo.UpdateAsync(entity), Times.Once);
        }

        [Fact]
        public async Task GetCachedData_ShouldReturnCachedData()
        {
            // Arrange
            var cacheKey = "testKey";
            var cachedData = new List<string> { "data1", "data2" };
            _repositoryMock.Setup(repo => repo.GetCachedData(cacheKey, It.IsAny<Func<Task<List<string>>>>()))
                           .ReturnsAsync(cachedData);

            // Act
            var result = await _repositoryMock.Object.GetCachedData(cacheKey, () => Task.FromResult(new List<string>()));

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void CallStoredProcedure_ShouldReturnData()
        {
            // Arrange
            var procedureName = "TestProcedure";
            var parameters = new object[] { 1, "param" };
            var data = new List<TestEntity> { new TestEntity(), new TestEntity() };
            _repositoryMock.Setup(repo => repo.CallStoredProcedure(procedureName, parameters)).Returns(data);

            // Act
            var result = _repositoryMock.Object.CallStoredProcedure(procedureName, parameters);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void CallStoredProcedureDTO_ShouldReturnData()
        {
            // Arrange
            var connectionString = "TestConnectionString";
            var procedureName = "TestProcedure";
            var data = new List<TestEntity> { new TestEntity(), new TestEntity() }.AsQueryable();
            _repositoryMock.Setup(repo => repo.CallStoredProcedureDTO(connectionString, procedureName)).Returns(data);

            // Act
            var result = _repositoryMock.Object.CallStoredProcedureDTO(connectionString, procedureName);

            // Assert
            Assert.Equal(2, result.Count());
        }
    }

    public class TestEntity
    {
        public int Id { get; set; }
    }
}