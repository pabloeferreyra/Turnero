using Turnero.Application.Common;
using Xunit;

namespace Turnero.Test;

public class EnumExtensionsTests
{
    [Theory]
    [InlineData(BloodType.A_Positive, "A+")]
    [InlineData(BloodType.A_Negative, "A-")]
    [InlineData(BloodType.AB_Positive, "AB+")]
    [InlineData(BloodType.O_Negative, "O-")]
    public void GetDisplayName_WithDisplayAttribute_ShouldReturnAttributeName(BloodType value, string expected)
    {
        // Act
        var result = value.GetDisplayName();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(Severity.Critica, "Critica")]
    [InlineData(Severity.Fatal, "Fatal")]
    [InlineData(AllergyType.Ambiental, "Ambiental")]
    [InlineData(Occurrency.Sporadica, "Sporadica")]
    public void GetDisplayName_WithoutDisplayAttribute_ShouldReturnEnumName(Enum value, string expected)
    {
        // Act
        var result = value.GetDisplayName();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetDisplayName_OnEveryBloodType_ShouldNeverReturnFieldName()
    {
        // Todos los miembros de BloodType tienen [Display(Name=...)] definido.
        var fieldNames = Enum.GetValues<BloodType>();

        foreach (var value in fieldNames)
        {
            var displayName = value.GetDisplayName();
            Assert.NotEqual(value.ToString(), displayName);
        }
    }

    [Fact]
    public void GetDisplayName_OnEverySeverity_ShouldReturnPlainName()
    {
        foreach (var value in Enum.GetValues<Severity>())
        {
            Assert.Equal(value.ToString(), value.GetDisplayName());
        }
    }
}
