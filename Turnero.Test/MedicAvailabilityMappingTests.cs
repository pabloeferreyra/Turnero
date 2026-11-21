using Microsoft.EntityFrameworkCore;
using Turnero.Domain.Entities;
using Turnero.Infrastructure.Data;
using Xunit;

namespace Turnero.Test;

public class MedicAvailabilityMappingTests
{
    [Fact]
    public void AvailabilityHasSeparateRestrictedTimeRelationships()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=turnero_test")
            .Options;
        using var context = new ApplicationDbContext(options);
        var entity = context.Model.FindEntityType(typeof(MedicAvailability))!;

        foreach (var name in new[] { nameof(MedicAvailability.TimeFrom), nameof(MedicAvailability.TimeTo) })
        {
            var foreignKey = entity.FindNavigation(name)!.ForeignKey;
            Assert.Equal(typeof(TimeTurn), foreignKey.PrincipalEntityType.ClrType);
            Assert.Equal(name + "Id", Assert.Single(foreignKey.Properties).Name);
            Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
        }

        var medic = entity.FindNavigation(nameof(MedicAvailability.Medic))!.ForeignKey;
        Assert.Equal(typeof(Medic), medic.PrincipalEntityType.ClrType);
        Assert.Equal(nameof(MedicAvailability.MedicId), Assert.Single(medic.Properties).Name);
    }
}
