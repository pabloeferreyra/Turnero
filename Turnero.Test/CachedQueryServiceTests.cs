using Microsoft.Extensions.Caching.Memory;
using Turnero.Application.Common.Interfaces;
using Xunit;

namespace Turnero.Test;

public class CachedQueryServiceTests : IDisposable
{
    private readonly MemoryCache _cache;
    private readonly CachedQueryService _service;

    public CachedQueryServiceTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _service = new CachedQueryService(_cache);
    }

    public void Dispose() => _cache.Dispose();

    [Fact]
    public async Task GetCachedData_OnCacheMiss_ShouldCallFactoryAndReturnData()
    {
        // Arrange
        var callCount = 0;
        async Task<List<string>> Factory()
        {
            callCount++;
            return await Task.FromResult(new List<string> { "dato1", "dato2" });
        }

        // Act
        var result = await _service.GetCachedData("key-miss", Factory);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(1, callCount);
        Assert.Equal("dato1", result[0]);
    }

    [Fact]
    public async Task GetCachedData_OnCacheHit_ShouldNotCallFactoryAgain()
    {
        // Arrange
        var callCount = 0;
        async Task<List<string>> Factory()
        {
            callCount++;
            return await Task.FromResult(new List<string> { "dato1" });
        }

        await _service.GetCachedData("key-hit", Factory);

        // Act: segunda llamada con la misma key
        var result = await _service.GetCachedData("key-hit", Factory);

        // Assert
        Assert.Single(result);
        Assert.Equal(1, callCount); // la factory no se volvió a invocar
    }

    [Fact]
    public async Task GetCachedData_DifferentKeys_ShouldCallFactoryForEach()
    {
        // Arrange
        var callCount = 0;
        async Task<List<int>> Factory()
        {
            callCount++;
            return await Task.FromResult(new List<int> { callCount });
        }

        // Act
        var resultA = await _service.GetCachedData("key-a", Factory);
        var resultB = await _service.GetCachedData("key-b", Factory);

        // Assert
        Assert.Equal(1, resultA[0]);
        Assert.Equal(2, resultB[0]);
        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task GetCachedData_ShouldReturnSameInstanceOnHit()
    {
        // Arrange
        async Task<List<string>> Factory() => await Task.FromResult(new List<string> { "dato1" });
        var first = await _service.GetCachedData("key-instance", Factory);

        // Act
        var second = await _service.GetCachedData("key-instance", Factory);

        // Assert: al venir de IMemoryCache, es la misma referencia
        Assert.Same(first, second);
    }

    [Fact]
    public async Task GetCachedData_EmptyList_ShouldStillBeCached()
    {
        // Arrange
        var callCount = 0;
        async Task<List<string>> Factory()
        {
            callCount++;
            return [];
        }

        // Act
        var first = await _service.GetCachedData("key-empty", Factory);
        var second = await _service.GetCachedData("key-empty", Factory);

        // Assert
        Assert.Empty(first);
        Assert.Empty(second);
        Assert.Equal(1, callCount); // lista vacía también cachea
    }

    [Fact]
    public async Task GetCachedData_FactoryThrows_ShouldNotCacheException()
    {
        // Arrange
        var callCount = 0;
        Task<List<string>> Factory()
        {
            callCount++;
            throw new InvalidOperationException("fallo de DB");
        }

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetCachedData("key-throw", Factory));

        // El segundo intento vuelve a invocar la factory (nada quedó cacheado)
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetCachedData("key-throw", Factory));
        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task GetCachedData_WithComplexType_ShouldCacheCorrectly()
    {
        // Arrange
        var patient = new Patient { Id = Guid.NewGuid(), Name = "Juan" };
        async Task<List<Patient>> Factory() =>
            await Task.FromResult(new List<Patient> { patient });

        // Act
        var result = await _service.GetCachedData("key-patient", Factory);

        // Assert
        Assert.Single(result);
        Assert.Equal("Juan", result[0].Name);
        Assert.Equal(patient.Id, result[0].Id);
    }
}
