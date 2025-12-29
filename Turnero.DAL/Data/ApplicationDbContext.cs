using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Turnero.DAL.Models;

namespace Turnero.DAL.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Turn>()
            .Property(b => b.DateTurn)
            .HasColumnType("date");
        builder.Entity<Patient>()
            .Property(p => p.BirthDate)
            .HasColumnType("date");
        builder.Entity<Visit>()
            .Property(p => p.VisitDate)
            .HasColumnType("date");
        var allergiesDate = typeof(Allergies)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(DateTime?));
        var visitStrings = typeof(Visit)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(string));
        var boolProps = typeof(PersonalBackground)
           .GetProperties()
           .Where(p => p.PropertyType == typeof(bool));
        var stringParents = typeof(ParentsData)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(string));
        var stringCongErrors = typeof(CongErrors)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(string));
        var dateParents = typeof(ParentsData)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(DateOnly));
        var intParents = typeof(ParentsData)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(int));
        var intPernHis = typeof(PerinatalBackground)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(int));
        foreach (var allergies in allergiesDate)
        {
            builder.Entity<Allergies>()
                .Property(allergies.Name)
                .HasColumnType("date")
                .HasDefaultValue(null);
        }
        foreach (var visit in visitStrings)
        {
            builder.Entity<Visit>()
                .Property(visit.Name)
                .HasDefaultValue(string.Empty);
        }
        foreach (var prop in boolProps)
        {
            builder.Entity<PersonalBackground>()
                .Property(prop.Name)
                .HasDefaultValue(false);
        }
        foreach (var prop in stringParents)
        {
            builder.Entity<ParentsData>()
                .Property(prop.Name)
                .HasDefaultValue(string.Empty);
        }
        foreach (var prop in stringCongErrors)
        {
            builder.Entity<CongErrors>()
                .Property(prop.Name)
                .HasDefaultValue(string.Empty);
        }
        foreach (var prop in dateParents)
        {
            builder.Entity<ParentsData>()
                .Property(prop.Name)
                .HasColumnType("date")
                .HasDefaultValue(DateOnly.MinValue);
        }
        foreach (var prop in intParents)
        {
            builder.Entity<ParentsData>()
            .Property(prop.Name)
            .HasDefaultValue(0);
        }
        foreach (var prop in intPernHis)
        {
            builder.Entity<PerinatalBackground>()
            .Property(prop.Name)
            .HasDefaultValue(0);
        }
    }

    public DbSet<Turn> Turns { get; set; }
    public DbSet<Medic> Medics { get; set; }
    public DbSet<TimeTurn> TimeTurns { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<ContactInfo> ContactInfo { get; set; }
    public DbSet<Allergies> Allergies { get; set; }
    public DbSet<Visit> Visits { get; set; }
    public DbSet<ParentsData> ParentsData { get; set; }
    public DbSet<PersonalBackground> PersonalBackground { get; set; }
    public DbSet<PerinatalBackground> PerinatalBackground { get; set; }
    public DbSet<Vaccines> Vaccines { get; set; }
    public DbSet<PermMed> PermMeds { get; set; }
    public DbSet<GrowthChart> GrowthCharts { get; set; }
    public DbSet<CongErrors> CongErrors { get; set; }
}
