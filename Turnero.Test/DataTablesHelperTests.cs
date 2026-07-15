using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Turnero.Helpers;
using Xunit;

namespace Turnero.Test;

public class DataTablesHelperTests
{
    #region GetDataTableParams

    [Fact]
    public void GetDataTableParams_ShouldParseFormValues()
    {
        var formCollection = new FormCollection(new Dictionary<string, StringValues>
        {
            { "draw", new StringValues("5") },
            { "start", new StringValues("10") },
            { "length", new StringValues("25") }
        });
        var request = new Mock<HttpRequest>();
        request.Setup(r => r.Form).Returns(formCollection);

        var (draw, pageSize, skip) = DataTablesHelper.GetDataTableParams(request.Object);

        Assert.Equal("5", draw);
        Assert.Equal(25, pageSize);
        Assert.Equal(10, skip);
    }

    [Fact]
    public void GetDataTableParams_ShouldUseDefaults_WhenFormIsEmpty()
    {
        var formCollection = new FormCollection(new Dictionary<string, StringValues>());
        var request = new Mock<HttpRequest>();
        request.Setup(r => r.Form).Returns(formCollection);

        var (draw, pageSize, skip) = DataTablesHelper.GetDataTableParams(request.Object);

        Assert.Equal("1", draw);
        Assert.Equal(0, pageSize);
        Assert.Equal(0, skip);
    }

    #endregion

    #region ParsePatientId

    [Fact]
    public void ParsePatientId_ShouldReturnGuid_WhenValid()
    {
        var id = Guid.NewGuid().ToString();
        var result = DataTablesHelper.ParsePatientId(id);
        Assert.NotNull(result);
        Assert.Equal(id, result!.ToString());
    }

    [Fact]
    public void ParsePatientId_ShouldReturnNull_WhenInvalid()
    {
        var result = DataTablesHelper.ParsePatientId("not-a-guid");
        Assert.Null(result);
    }

    [Fact]
    public void ParsePatientId_ShouldReturnNull_WhenNull()
    {
        var result = DataTablesHelper.ParsePatientId(null);
        Assert.Null(result);
    }

    [Fact]
    public void ParsePatientId_ShouldReturnNull_WhenEmpty()
    {
        var result = DataTablesHelper.ParsePatientId("");
        Assert.Null(result);
    }

    #endregion

    #region ApplyPaging

    [Fact]
    public void ApplyPaging_ShouldReturnAll_WhenPageSizeIsMinusOne()
    {
        var data = new List<string> { "a", "b", "c", "d", "e" };
        var result = DataTablesHelper.ApplyPaging(data, -1, 0);
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void ApplyPaging_ShouldSkipAndTake()
    {
        var data = new List<string> { "a", "b", "c", "d", "e" };
        var result = DataTablesHelper.ApplyPaging(data, 2, 1);
        Assert.Equal(2, result.Count);
        Assert.Equal("b", result[0]);
        Assert.Equal("c", result[1]);
    }

    [Fact]
    public void ApplyPaging_ShouldTakeFirstPage()
    {
        var data = new List<string> { "a", "b", "c", "d", "e" };
        var result = DataTablesHelper.ApplyPaging(data, 3, 0);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ApplyPaging_ShouldReturnAll_WhenPageSizeIsZero()
    {
        var data = new List<string> { "a", "b", "c" };
        var result = DataTablesHelper.ApplyPaging(data, 0, 0);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ApplyPaging_ShouldReturnEmpty_WhenNull()
    {
        var result = DataTablesHelper.ApplyPaging<string>(null!, 10, 0);
        Assert.Empty(result);
    }

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

    #region GetPropValue

    [Fact]
    public void GetPropValue_ShouldReturnStringValue()
    {
        var obj = new TestSortEntity { Name = "test", Value = 42 };
        var result = DataTablesHelper.GetPropValue(obj, "Name");
        Assert.Equal("test", result);
    }

    [Fact]
    public void GetPropValue_ShouldReturnIntValue()
    {
        var obj = new TestSortEntity { Name = "test", Value = 42 };
        var result = DataTablesHelper.GetPropValue(obj, "Value");
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetPropValue_ShouldReturnNull_WhenObjIsNull()
    {
        var result = DataTablesHelper.GetPropValue(null, "Name");
        Assert.Null(result);
    }

    [Fact]
    public void GetPropValue_ShouldReturnNull_WhenPropNameIsNull()
    {
        var result = DataTablesHelper.GetPropValue(new TestSortEntity(), null!);
        Assert.Null(result);
    }

    [Fact]
    public void GetPropValue_ShouldReturnNull_WhenPropertyDoesNotExist()
    {
        var result = DataTablesHelper.GetPropValue(new TestSortEntity(), "NonExistent");
        Assert.Null(result);
    }

    [Fact]
    public void GetPropValue_ShouldReturnNullForWhitespacePropName()
    {
        var result = DataTablesHelper.GetPropValue(new TestSortEntity(), "   ");
        Assert.Null(result);
    }

    #endregion

    #region ApplySorting

    [Fact]
    public void ApplySorting_ShouldSortAsc()
    {
        var data = new List<TestSortEntity>
        {
            new() { Name = "Charlie", Value = 3 },
            new() { Name = "Alice", Value = 1 },
            new() { Name = "Bob", Value = 2 }
        };
        var formCollection = new FormCollection(new Dictionary<string, StringValues>
        {
            { "order[0][column]", new StringValues("0") },
            { "columns[0][name]", new StringValues("Name") },
            { "order[0][dir]", new StringValues("asc") }
        });
        var request = new Mock<HttpRequest>();
        request.Setup(r => r.Form).Returns(formCollection);

        var result = DataTablesHelper.ApplySorting(data, request.Object);

        Assert.Equal("Alice", result[0].Name);
        Assert.Equal("Bob", result[1].Name);
        Assert.Equal("Charlie", result[2].Name);
    }

    [Fact]
    public void ApplySorting_ShouldSortDesc()
    {
        var data = new List<TestSortEntity>
        {
            new() { Name = "Charlie", Value = 3 },
            new() { Name = "Alice", Value = 1 },
            new() { Name = "Bob", Value = 2 }
        };
        var formCollection = new FormCollection(new Dictionary<string, StringValues>
        {
            { "order[0][column]", new StringValues("0") },
            { "columns[0][name]", new StringValues("Name") },
            { "order[0][dir]", new StringValues("desc") }
        });
        var request = new Mock<HttpRequest>();
        request.Setup(r => r.Form).Returns(formCollection);

        var result = DataTablesHelper.ApplySorting(data, request.Object);

        Assert.Equal("Charlie", result[0].Name);
        Assert.Equal("Bob", result[1].Name);
        Assert.Equal("Alice", result[2].Name);
    }

    [Fact]
    public void ApplySorting_ShouldSortByIntProperty()
    {
        var data = new List<TestSortEntity>
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
    public void ApplySorting_ShouldReturnUnsorted_WhenNoSortColumn()
    {
        var data = new List<TestSortEntity>
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

    [Fact]
    public void ApplySorting_ShouldReturnUnsorted_WhenNoOrderColumn()
    {
        var data = new List<TestSortEntity>
        {
            new() { Name = "Charlie", Value = 3 },
            new() { Name = "Alice", Value = 1 }
        };
        var formCollection = new FormCollection(new Dictionary<string, StringValues>());
        var request = new Mock<HttpRequest>();
        request.Setup(r => r.Form).Returns(formCollection);

        var result = DataTablesHelper.ApplySorting(data, request.Object);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ApplySorting_ShouldReturnUnsorted_WhenSortColumnDoesNotExist()
    {
        var data = new List<TestSortEntity>
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

        Assert.Equal(2, result.Count);
        Assert.Equal("Charlie", result[0].Name);
    }

    #endregion

    #region ExtractPatientIdAsync

    [Fact]
    public async Task ExtractPatientIdAsync_ShouldUseQueryParameter_WhenProvided()
    {
        var patientId = Guid.NewGuid();
        var request = new Mock<HttpRequest>();

        var result = await DataTablesHelper.ExtractPatientIdAsync(request.Object, patientId);

        Assert.Equal(patientId.ToString(), result);
    }

    [Fact]
    public async Task ExtractPatientIdAsync_ShouldExtractFromFormData()
    {
        var patientId = Guid.NewGuid();
        var formCollection = new FormCollection(new Dictionary<string, StringValues>
        {
            { "patientId", new StringValues(patientId.ToString()) }
        });
        var request = new Mock<HttpRequest>();
        request.Setup(r => r.HasFormContentType).Returns(true);
        request.Setup(r => r.Form).Returns(formCollection);

        var result = await DataTablesHelper.ExtractPatientIdAsync(request.Object);

        Assert.Equal(patientId.ToString(), result);
    }

    [Fact]
    public async Task ExtractPatientIdAsync_ShouldExtractFromQuery()
    {
        var patientId = Guid.NewGuid();
        var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
        {
            { "patientId", new StringValues(patientId.ToString()) }
        });
        var request = new Mock<HttpRequest>();
        request.Setup(r => r.HasFormContentType).Returns(false);
        request.Setup(r => r.Query).Returns(queryCollection);

        var result = await DataTablesHelper.ExtractPatientIdAsync(request.Object);

        Assert.Equal(patientId.ToString(), result);
    }

    #endregion

    public class TestSortEntity
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
    }
}
