using Turnero.Utilities.Utilities;
using Xunit;

namespace Turnero.Test;

public class DateCalculationsTests
{
    #region Years

    [Fact]
    public void CalcularEdad_ShouldReturnYears()
    {
        var fechaNacimiento = DateTime.Today.AddYears(-25);
        var result = DateCalculations.CalcularEdad(fechaNacimiento);
        Assert.Equal("25 años", result);
    }

    [Fact]
    public void CalcularEdad_ShouldReturnOneYear()
    {
        var fechaNacimiento = DateTime.Today.AddYears(-1);
        var result = DateCalculations.CalcularEdad(fechaNacimiento);
        Assert.Equal("1 año", result);
    }

    [Fact]
    public void CalcularEdad_ExactBirthday_ShouldReturnExactYears()
    {
        var fechaNacimiento = DateTime.Today.AddYears(-30);
        var result = DateCalculations.CalcularEdad(fechaNacimiento);
        Assert.Equal("30 años", result);
    }

    [Fact]
    public void CalcularEdad_BirthdayOccurredToday_ShouldBeFullYears()
    {
        var fechaNacimiento = DateTime.Today.AddYears(-5);
        var result = DateCalculations.CalcularEdad(fechaNacimiento);
        Assert.Equal("5 años", result);
    }

    [Fact]
    public void CalcularEdad_VeryOldPerson_ShouldReturnYears()
    {
        var fechaNacimiento = DateTime.Today.AddYears(-100);
        var result = DateCalculations.CalcularEdad(fechaNacimiento);
        Assert.Equal("100 años", result);
    }

    [Fact]
    public void CalcularEdad_ShouldHandleExactlyOneYear()
    {
        var fechaNacimiento = new DateTime(2000, 6, 15);
        var fechaReferencia = new DateTime(2001, 6, 15);
        var result = DateCalculations.CalcularEdad(fechaNacimiento, fechaReferencia);
        Assert.Equal("1 año", result);
    }

    [Fact]
    public void CalcularEdad_BirthdayNotYetOccurred_ShouldSubtractOneYear()
    {
        // Born Dec 31, reference Jan 1 next year = 1 day, not 1 year
        var fechaNacimiento = new DateTime(2000, 12, 31);
        var fechaReferencia = new DateTime(2001, 1, 1);
        var result = DateCalculations.CalcularEdad(fechaNacimiento, fechaReferencia);
        Assert.Equal("1 día", result);
    }

    #endregion

    #region Months

    [Fact]
    public void CalcularEdad_ShouldReturnMonths()
    {
        var fechaNacimiento = DateTime.Today.AddMonths(-5);
        var result = DateCalculations.CalcularEdad(fechaNacimiento);
        Assert.Equal("5 meses", result);
    }

    [Fact]
    public void CalcularEdad_ShouldReturnOneMonth()
    {
        var fechaNacimiento = DateTime.Today.AddMonths(-1);
        var result = DateCalculations.CalcularEdad(fechaNacimiento);
        Assert.Equal("1 mes", result);
    }

    [Fact]
    public void CalcularEdad_Exactly11Months_ShouldReturnMonths()
    {
        var fechaNacimiento = DateTime.Today.AddMonths(-11);
        var result = DateCalculations.CalcularEdad(fechaNacimiento);
        Assert.Equal("11 meses", result);
    }

    [Fact]
    public void CalcularEdad_Exactly2Months_ShouldReturnMonths()
    {
        var fechaNacimiento = DateTime.Today.AddMonths(-2);
        var result = DateCalculations.CalcularEdad(fechaNacimiento);
        Assert.Equal("2 meses", result);
    }

    [Fact]
    public void CalcularEdad_ReferenceDate_3MonthsOld_ShouldReturnMonths()
    {
        var fechaNacimiento = new DateTime(2000, 1, 1);
        var fechaReferencia = new DateTime(2000, 4, 1);
        var result = DateCalculations.CalcularEdad(fechaNacimiento, fechaReferencia);
        Assert.Equal("3 meses", result);
    }

    #endregion

    #region Weeks

    [Fact]
    public void CalcularEdad_ShouldReturnWeeks()
    {
        var fechaNacimiento = DateTime.Today.AddDays(-21);
        var result = DateCalculations.CalcularEdad(fechaNacimiento);
        Assert.Equal("3 semanas", result);
    }

    [Fact]
    public void CalcularEdad_ShouldReturnOneWeek()
    {
        var fechaNacimiento = DateTime.Today.AddDays(-7);
        var result = DateCalculations.CalcularEdad(fechaNacimiento);
        Assert.Equal("1 semana", result);
    }

    [Fact]
    public void CalcularEdad_Exactly2Weeks_ShouldReturnWeeks()
    {
        var fechaNacimiento = DateTime.Today.AddDays(-14);
        var result = DateCalculations.CalcularEdad(fechaNacimiento);
        Assert.Equal("2 semanas", result);
    }

    [Fact]
    public void CalcularEdad_10Days_ShouldReturnWeeks()
    {
        // 10 days = 1 week + 3 days → shows as "1 semana"
        var fechaNacimiento = DateTime.Today.AddDays(-10);
        var result = DateCalculations.CalcularEdad(fechaNacimiento);
        Assert.Equal("1 semana", result);
    }

    #endregion

    #region Days

    [Fact]
    public void CalcularEdad_ShouldReturnDays()
    {
        var fechaNacimiento = DateTime.Today.AddDays(-3);
        var result = DateCalculations.CalcularEdad(fechaNacimiento);
        Assert.Equal("3 días", result);
    }

    [Fact]
    public void CalcularEdad_ShouldReturnOneDay()
    {
        var fechaNacimiento = DateTime.Today.AddDays(-1);
        var result = DateCalculations.CalcularEdad(fechaNacimiento);
        Assert.Equal("1 día", result);
    }

    [Fact]
    public void CalcularEdad_ShouldReturnZeroDays_WhenBornToday()
    {
        var result = DateCalculations.CalcularEdad(DateTime.Today);
        Assert.Equal("0 días", result);
    }

    [Fact]
    public void CalcularEdad_6Days_ShouldReturnDays()
    {
        var fechaNacimiento = DateTime.Today.AddDays(-6);
        var result = DateCalculations.CalcularEdad(fechaNacimiento);
        Assert.Equal("6 días", result);
    }

    [Fact]
    public void CalcularEdad_ReferenceDate_ExactlySameDate_ShouldBeZeroDays()
    {
        var fechaNacimiento = new DateTime(2000, 6, 15);
        var fechaReferencia = new DateTime(2000, 6, 15);
        var result = DateCalculations.CalcularEdad(fechaNacimiento, fechaReferencia);
        Assert.Equal("0 días", result);
    }

    [Fact]
    public void CalcularEdad_ReferenceDate_1DayOld_ShouldBeOneDay()
    {
        var fechaNacimiento = new DateTime(2000, 6, 15);
        var fechaReferencia = new DateTime(2000, 6, 16);
        var result = DateCalculations.CalcularEdad(fechaNacimiento, fechaReferencia);
        Assert.Equal("1 día", result);
    }

    #endregion

    #region Reference date

    [Fact]
    public void CalcularEdad_ShouldUseReferenceDate_WhenProvided()
    {
        var fechaNacimiento = new DateTime(2000, 1, 1);
        var fechaReferencia = new DateTime(2025, 1, 1);
        var result = DateCalculations.CalcularEdad(fechaNacimiento, fechaReferencia);
        Assert.Equal("25 años", result);
    }

    [Fact]
    public void CalcularEdad_ReferenceDate_20Years_ShouldReturnYears()
    {
        var fechaNacimiento = new DateTime(2000, 1, 1);
        var fechaReferencia = new DateTime(2020, 1, 1);
        var result = DateCalculations.CalcularEdad(fechaNacimiento, fechaReferencia);
        Assert.Equal("20 años", result);
    }

    #endregion

    #region Error cases

    [Fact]
    public void CalcularEdad_ShouldThrow_WhenBirthDateIsFuture()
    {
        var fechaNacimiento = DateTime.Today.AddDays(1);
        var ex = Assert.Throws<ArgumentException>(() => DateCalculations.CalcularEdad(fechaNacimiento));
        Assert.Contains("futura", ex.Message);
    }

    [Fact]
    public void CalcularEdad_Tomorrow_ShouldThrowArgumentException()
    {
        var fechaNacimiento = DateTime.Today.AddDays(1);
        Assert.Throws<ArgumentException>(() => DateCalculations.CalcularEdad(fechaNacimiento));
    }

    [Fact]
    public void CalcularEdad_FarFuture_ShouldThrowArgumentException()
    {
        var fechaNacimiento = DateTime.Today.AddYears(5);
        Assert.Throws<ArgumentException>(() => DateCalculations.CalcularEdad(fechaNacimiento));
    }

    #endregion
}
