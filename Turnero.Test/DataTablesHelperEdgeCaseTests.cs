using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Turnero.Helpers;
using Xunit;

namespace Turnero.Test;

public class DataTablesHelperEdgeCaseTests
{
    #region ApplyPaging edge cases

    [Fact]
    public void ApplyPaging_ShouldReturnEmpty_WhenSkipExceedsDataCount()
    {
        var data = new List<string> { "a", "b", "c" };
        var result = DataTablesHelper.ApplyPaging(data, 10, 100);
        Assert.Empty(result);
    }

    [Fact]
    public void ApplyPaging_ShouldReturnRemaining_WhenPageSizeExceedsData()
    {
        var data = new List<string> { "a", "b", "c" };
        var result = DataTablesHelper.ApplyPaging(data, 100, 0);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ApplyPaging_ShouldReturnEmpty_WhenDataIsEmpty()
    {
        var data = new List<string>();
        var result = DataTablesHelper.ApplyPaging(data, 10, 0);
        Assert.Empty(result);
    }

    [Fact]
    public void ApplyPaging_ShouldReturnSingleItem_WhenPageSizeIsOne()
    {
        var data = new List<string> { "a", "b", "c", "d", "e" };
        var result = DataTablesHelper.ApplyPaging(data, 1, 2);
        Assert.Single(result);
        Assert.Equal("c", result[0]);
    }

    #endregion

    #region ApplySorting edge cases

    [Fact]
    public void ApplySorting_ShouldReturnUnsorted_WhenSortColumnDoesNotExist()
    {
        var data = new List<SortTestEntity>
        {
            new() { Name = "Charlie", Value = 3 },
            new() { Name = "Alice", Value = 1 }
        };

        var formCollection = new FormCollection(new Dictionary<string, StringValues>
        {
            { "order[0][column]", new StringValues("0") },
            { "columns[0][name]", new StringValues("NonExistent") },
            { "order[0][dir]", new StringValues("asc") }
        });
        var request = new Mock<HttpRequest>();
        request.Setup(r => r.Form).Returns(formCollection);

        var result = DataTablesHelper.ApplySorting(data, request.Object);

        // Should return data unsorted (original order preserved)
        Assert.Equal(2, result.Count);
        Assert.Equal("Charlie", result[0].Name);
    }

    [Fact]
    public void ApplySorting_ShouldSortByIntProperty()
    {
        var data = new List<SortTestEntity>
        {
            new() { Name = "C", Value = 30 },
            new() { Name = "A", Value = 10 },
            new() { Name = "B", Value = 20 }
        };

        var formCollection = new FormCollection(new Dictionary<string, StringValues>
        {
            { "order[0][column]", new StringValues("0") },
            { "columns[0][name]", new StringValues("Value") },
            { "order[0][dir]", new StringValues("asc") }
        });
        var request = new Mock<HttpRequest>();
        request.Setup(r => r.Form).Returns(formCollection);

        var result = DataTablesHelper.ApplySorting(data, request.Object);

        Assert.Equal(10, result[0].Value);
        Assert.Equal(20, result[1].Value);
        Assert.Equal(30, result[2].Value);
    }

    [Fact]
    public void ApplySorting_ShouldReturnUnsorted_WhenNoOrderColumn()
    {
        var data = new List<SortTestEntity>
        {
            new() { Name = "Charlie", Value = 3 },
            new() { Name = "Alice", Value = 1 }
        };

        var formCollection = new FormCollection(new Dictionary<string, StringValues>
        {
            { "columns[0][name]", new StringValues("Name") },
            { "order[0][dir]", new StringValues("asc") }
        });
        var request = new Mock<HttpRequest>();
        request.Setup(r => r.Form).Returns(formCollection);

        var result = DataTablesHelper.ApplySorting(data, request.Object);

        Assert.Equal(2, result.Count);
    }

    #endregion

    #region GetPropValue edge cases

    [Fact]
    public void GetPropValue_ShouldReturnIntProperty()
    {
        var obj = new SortTestEntity { Name = "test", Value = 42 };
        var result = DataTablesHelper.GetPropValue(obj, "Value");
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetPropValue_ShouldReturnNullForWhitespacePropName()
    {
        var result = DataTablesHelper.GetPropValue(new SortTestEntity(), "   ");
        Assert.Null(result);
    }

    #endregion

    public class SortTestEntity
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
    }
}
