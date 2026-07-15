using Turnero.Utilities.Utilities;
using Xunit;

namespace Turnero.Test;

public class DateCalculationsTests
{
    [Fact]
    public void CalcularEdad_ShouldReturnYears()
    {
        // Arrange - Person born 25 years ago
        var fechaNacimiento = DateTime.Today.AddYears(-25);

        // Act
        var result = DateCalculations.CalcularEdad(fechaNacimiento);

        // Assert
        Assert.Equal("25 años", result);
    }

    [Fact]
    public void CalcularEdad_ShouldReturnOneYear()
    {
        // Arrange
        var fechaNacimiento = DateTime.Today.AddYears(-1);

        // Act
        var result = DateCalculations.CalcularEdad(fechaNacimiento);

        // Assert
        Assert.Equal("1 año", result);
    }

    [Fact]
    public void CalcularEdad_ShouldReturnMonths()
    {
        // Arrange - Person born 5 months ago
        var fechaNacimiento = DateTime.Today.AddMonths(-5);

        // Act
        var result = DateCalculations.CalcularEdad(fechaNacimiento);

        // Assert
        Assert.Equal("5 meses", result);
    }

    [Fact]
    public void CalcularEdad_ShouldReturnOneMonth()
    {
        // Arrange
        var fechaNacimiento = DateTime.Today.AddMonths(-1);

        // Act
        var result = DateCalculations.CalcularEdad(fechaNacimiento);

        // Assert
        Assert.Equal("1 mes", result);
    }

    [Fact]
    public void CalcularEdad_ShouldReturnWeeks()
    {
        // Arrange - Person born 3 weeks ago
        var fechaNacimiento = DateTime.Today.AddDays(-21);

        // Act
        var result = DateCalculations.CalcularEdad(fechaNacimiento);

        // Assert
        Assert.Equal("3 semanas", result);
    }

    [Fact]
    public void CalcularEdad_ShouldReturnOneWeek()
    {
        // Arrange
        var fechaNacimiento = DateTime.Today.AddDays(-7);

        // Act
        var result = DateCalculations.CalcularEdad(fechaNacimiento);

        // Assert
        Assert.Equal("1 semana", result);
    }

    [Fact]
    public void CalcularEdad_ShouldReturnDays()
    {
        // Arrange - Person born 3 days ago
        var fechaNacimiento = DateTime.Today.AddDays(-3);

        // Act
        var result = DateCalculations.CalcularEdad(fechaNacimiento);

        // Assert
        Assert.Equal("3 días", result);
    }

    [Fact]
    public void CalcularEdad_ShouldReturnOneDay()
    {
        // Arrange
        var fechaNacimiento = DateTime.Today.AddDays(-1);

        // Act
        var result = DateCalculations.CalcularEdad(fechaNacimiento);

        // Assert
        Assert.Equal("1 día", result);
    }

    [Fact]
    public void CalcularEdad_ShouldReturnZeroDays_WhenBornToday()
    {
        // Act
        var result = DateCalculations.CalcularEdad(DateTime.Today);

        // Assert
        Assert.Equal("0 días", result);
    }

    [Fact]
    public void CalcularEdad_ShouldUseReferenceDate_WhenProvided()
    {
        // Arrange
        var fechaNacimiento = new DateTime(2000, 1, 1);
        var fechaReferencia = new DateTime(2025, 1, 1);

        // Act
        var result = DateCalculations.CalcularEdad(fechaNacimiento, fechaReferencia);

        // Assert
        Assert.Equal("25 años", result);
    }

    [Fact]
    public void CalcularEdad_ShouldThrow_WhenBirthDateIsFuture()
    {
        var fechaNacimiento = DateTime.Today.AddDays(1);

        Assert.Throws<ArgumentException>(() => DateCalculations.CalcularEdad(fechaNacimiento));
    }

    [Fact]
    public void CalcularEdad_ShouldHandleBirthdayNotYetOccurredThisYear()
    {
        // Arrange - Born Dec 31, reference is Jan 1 of next year = 1 day old, not 1 year
        var fechaNacimiento = new DateTime(2000, 12, 31);
        var fechaReferencia = new DateTime(2001, 1, 1);

        // Act
        var result = DateCalculations.CalcularEdad(fechaNacimiento, fechaReferencia);

        // Assert
        Assert.Equal("1 día", result);
    }

    [Fact]
    public void CalcularEdad_ShouldHandleExactlyOneYear()
    {
        // Arrange
        var fechaNacimiento = new DateTime(2000, 6, 15);
        var fechaReferencia = new DateTime(2001, 6, 15);

        // Act
        var result = DateCalculations.CalcularEdad(fechaNacimiento, fechaReferencia);

        // Assert
        Assert.Equal("1 año", result);
    }
}
