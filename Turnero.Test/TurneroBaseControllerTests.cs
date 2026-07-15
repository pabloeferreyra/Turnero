using Microsoft.AspNetCore.Mvc.Rendering;
using Turnero.Controllers;
using Turnero.DAL.Models;
using Xunit;

namespace Turnero.Test;

public class TurneroBaseControllerTests
{
    /// <summary>
    /// Concrete implementation of the abstract TurneroBaseController for testing.
    /// </summary>
    private class TestableTurneroController : TurneroBaseController
    {
        // Expose protected static methods for testing
        public static List<SelectListItem> PublicEnumToSelectList<TEnum>(
            Func<TEnum, string>? textSelector = null) where TEnum : struct, Enum
            => EnumToSelectList(textSelector);

        public static List<SelectListItem> PublicEnumNamesToSelectList<TEnum>(
            Func<string, string>? textTransform = null) where TEnum : struct, Enum
            => EnumNamesToSelectList<TEnum>(textTransform);
    }

    private enum TestEnum
    {
        First = 0,
        Second = 1,
        Third = 2
    }

    #region EnumToSelectList

    [Fact]
    public void EnumToSelectList_ShouldReturnAllValues()
    {
        var result = TestableTurneroController.PublicEnumToSelectList<TestEnum>();
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void EnumToSelectList_ShouldUseEnumNamesByDefault()
    {
        var result = TestableTurneroController.PublicEnumToSelectList<TestEnum>();

        Assert.Contains(result, x => x.Text == "First");
        Assert.Contains(result, x => x.Text == "Second");
        Assert.Contains(result, x => x.Text == "Third");
    }

    [Fact]
    public void EnumToSelectList_ShouldUseIntegerValues()
    {
        var result = TestableTurneroController.PublicEnumToSelectList<TestEnum>();

        Assert.Contains(result, x => x.Value == "0");
        Assert.Contains(result, x => x.Value == "1");
        Assert.Contains(result, x => x.Value == "2");
    }

    [Fact]
    public void EnumToSelectList_ShouldUseCustomTextSelector()
    {
        var result = TestableTurneroController.PublicEnumToSelectList<TestEnum>(
            e => $"Custom_{e}");

        Assert.Contains(result, x => x.Text == "Custom_First");
        Assert.Contains(result, x => x.Text == "Custom_Second");
        Assert.Contains(result, x => x.Text == "Custom_Third");
    }

    [Fact]
    public void EnumToSelectList_WithSeverity_ShouldReturnAllValues()
    {
        var result = TestableTurneroController.PublicEnumToSelectList<Severity>();

        Assert.NotEmpty(result);
        Assert.All(result, item =>
        {
            Assert.NotNull(item.Value);
            Assert.NotNull(item.Text);
        });
    }

    #endregion

    #region EnumNamesToSelectList

    [Fact]
    public void EnumNamesToSelectList_ShouldReturnAllNames()
    {
        var result = TestableTurneroController.PublicEnumNamesToSelectList<TestEnum>();
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void EnumNamesToSelectList_ShouldUseNamesAsValues()
    {
        var result = TestableTurneroController.PublicEnumNamesToSelectList<TestEnum>();

        Assert.Contains(result, x => x.Value == "First");
        Assert.Contains(result, x => x.Value == "Second");
        Assert.Contains(result, x => x.Value == "Third");
    }

    [Fact]
    public void EnumNamesToSelectList_ShouldUseNamesAsTextByDefault()
    {
        var result = TestableTurneroController.PublicEnumNamesToSelectList<TestEnum>();

        Assert.Contains(result, x => x.Text == "First");
        Assert.Contains(result, x => x.Text == "Second");
        Assert.Contains(result, x => x.Text == "Third");
    }

    [Fact]
    public void EnumNamesToSelectList_WithVaccinesEnum_ShouldReturnAllValues()
    {
        var result = TestableTurneroController.PublicEnumNamesToSelectList<VaccinesEnum>();

        Assert.NotEmpty(result);
        Assert.All(result, item =>
        {
            Assert.NotNull(item.Value);
            Assert.NotNull(item.Text);
        });
    }

    #endregion
}
