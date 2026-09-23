using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Turnero.Domain.Entities;
using Xunit;

namespace Turnero.Test;

/// <summary>
/// Harness mínimo para ejecutar CountAsync/ToListAsync de EF Core sobre datos en memoria,
/// sin base de datos. ToListAsync entra por IAsyncEnumerable; CountAsync llega por
/// IAsyncQueryProvider.ExecuteAsync (solo se soporta Count sobre la fuente cruda,
/// que es justo lo que usa PaginatedList.CreateAsync).
/// </summary>
public class AsyncEnumerableQuery<T> : EnumerableQuery<T>, IQueryable<T>, IAsyncEnumerable<T>, IAsyncQueryProvider
{
    private readonly Expression _queryExpression;

    public AsyncEnumerableQuery(IEnumerable<T> source) : base(source) =>
        _queryExpression = Expression.Constant(this, typeof(IQueryable<T>));

    public AsyncEnumerableQuery(Expression expression) : base(expression) =>
        _queryExpression = expression;

    // La propia clase actúa como proveedor, para que EF detecte soporte async.
    Type IQueryable.ElementType => typeof(T);
    Expression IQueryable.Expression => _queryExpression;
    IQueryProvider IQueryable.Provider => this;

    // CreateQuery devuelve el wrapper async (Skip/Take pasan por acá).
    IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression) =>
        new AsyncEnumerableQuery<TElement>(expression);

    IQueryable IQueryProvider.CreateQuery(Expression expression)
    {
        var elementType = expression.Type.GetGenericArguments()[0];
        var wrapperType = typeof(AsyncEnumerableQuery<>).MakeGenericType(elementType);
        return (IQueryable)Activator.CreateInstance(wrapperType, expression)!;
    }

    // Ejecución sincrónica: se delega al comportamiento estándar de EnumerableQuery.
    // (pragma puntual: la interfaz exige TResult exacto; nunca devuelve null aquí)
    object? IQueryProvider.Execute(Expression expression) =>
        ((IQueryProvider)new EnumerableQuery<T>(expression)).Execute(expression);

#pragma warning disable CS8603 // Posible tipo de valor devuelto de referencia nulo
    TResult IQueryProvider.Execute<TResult>(Expression expression) =>
        ((IQueryProvider)new EnumerableQuery<T>(expression)).Execute<TResult>(expression);
#pragma warning restore CS8603

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        new AsyncEnumerator<T>(((IEnumerable<T>)this).GetEnumerator());

    // Miembro async: solo Count (la operación que usa PaginatedList.CreateAsync).
    TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        if (expression is MethodCallExpression { Method.Name: "Count" or "CountAsync" } call
            && call.Arguments[0] is ConstantExpression { Value: IEnumerable<T> source })
        {
            var count = source.Count();
            return (TResult)(object)Task.FromResult(count);
        }

        throw new NotSupportedException(
            $"El harness solo soporta CountAsync sobre la fuente cruda. Expresión: {expression}");
    }

    private sealed class AsyncEnumerator<TValue>(IEnumerator<TValue> inner) : IAsyncEnumerator<TValue>
    {
        public TValue Current => inner.Current;
        public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(inner.MoveNext());
        public ValueTask DisposeAsync() => default;
    }
}

public static class AsyncQueryableExtensions
{
    public static IQueryable<T> AsAsyncQueryable<T>(this IEnumerable<T> source) =>
        new AsyncEnumerableQuery<T>(source);
}

public class PaginatedListTests
{
    private static List<int> Sequence(int count) => Enumerable.Range(1, count).ToList();

    [Fact]
    public void Create_FirstPage_ShouldReturnRequestedItems()
    {
        // Arrange
        var source = Sequence(45);

        // Act
        var result = PaginatedList<int>.Create(source, pageIndex: 1, pageSize: 20);

        // Assert
        Assert.Equal(20, result.Count);
        Assert.Equal(1, result[0]);
        Assert.Equal(20, result[19]);
    }

    [Fact]
    public void Create_MiddlePage_ShouldSkipCorrectly()
    {
        // Arrange
        var source = Sequence(45);

        // Act
        var result = PaginatedList<int>.Create(source, pageIndex: 2, pageSize: 20);

        // Assert
        Assert.Equal(20, result.Count);
        Assert.Equal(21, result[0]);
        Assert.Equal(40, result[19]);
    }

    [Fact]
    public void Create_LastPartialPage_ShouldReturnRemainingItems()
    {
        // Arrange
        var source = Sequence(45);

        // Act
        var result = PaginatedList<int>.Create(source, pageIndex: 3, pageSize: 20);

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Equal(41, result[0]);
        Assert.Equal(45, result[^1]);
    }

    [Fact]
    public void Create_PageBeyondEnd_ShouldReturnEmpty()
    {
        // Arrange
        var source = Sequence(45);

        // Act
        var result = PaginatedList<int>.Create(source, pageIndex: 10, pageSize: 20);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Create_ShouldCalculateTotalPagesCeiling()
    {
        // 45 items con página de 20 → 3 páginas (techo de 2.25)
        var result = PaginatedList<int>.Create(Sequence(45), pageIndex: 1, pageSize: 20);
        Assert.Equal(3, result.TotalPages);

        // 40 items con página de 20 → exactamente 2 páginas
        var exact = PaginatedList<int>.Create(Sequence(40), pageIndex: 1, pageSize: 20);
        Assert.Equal(2, exact.TotalPages);

        // 1 item con página de 20 → 1 página
        var single = PaginatedList<int>.Create(Sequence(1), pageIndex: 1, pageSize: 20);
        Assert.Equal(1, single.TotalPages);
    }

    [Fact]
    public void Create_EmptySource_ShouldReturnEmptyWithZeroPages()
    {
        // Act
        var result = PaginatedList<int>.Create([], pageIndex: 1, pageSize: 20);

        // Assert
        Assert.Empty(result);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public void HasPreviousPage_ShouldBeTrueOnlyAfterPageOne()
    {
        var page1 = PaginatedList<int>.Create(Sequence(45), pageIndex: 1, pageSize: 20);
        Assert.False(page1.HasPreviousPage);

        var page2 = PaginatedList<int>.Create(Sequence(45), pageIndex: 2, pageSize: 20);
        Assert.True(page2.HasPreviousPage);
    }

    [Fact]
    public void HasNextPage_ShouldBeFalseOnLastPage()
    {
        // Act
        var lastPage = PaginatedList<int>.Create(Sequence(45), pageIndex: 3, pageSize: 20);

        // Assert
        Assert.False(lastPage.HasNextPage);

        var firstPage = PaginatedList<int>.Create(Sequence(45), pageIndex: 1, pageSize: 20);
        Assert.True(firstPage.HasNextPage);
    }

    [Fact]
    public async Task CreateAsync_ShouldPaginateQueryable()
    {
        // Arrange
        var queryable = Sequence(30).AsAsyncQueryable();

        // Act
        var result = await PaginatedList<int>.CreateAsync(queryable, pageIndex: 2, pageSize: 10);

        // Assert
        Assert.Equal(10, result.Count);
        Assert.Equal(11, result[0]);
        Assert.Equal(20, result[^1]);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public async Task CreateAsync_WithStrings_ShouldPreserveOrder()
    {
        // Arrange
        var queryable = new[] { "ana", "beto", "carla", "david", "elena" }.AsAsyncQueryable();

        // Act
        var result = await PaginatedList<string>.CreateAsync(queryable, pageIndex: 1, pageSize: 2);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("ana", result[0]);
        Assert.Equal("beto", result[1]);
        Assert.Equal(3, result.TotalPages);
    }
}
