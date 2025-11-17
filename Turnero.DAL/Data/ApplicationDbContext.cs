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
        builder.Entity<Allergies>()
            .Property(p => p.Begin)
            .HasColumnType("date");
        builder.Entity<Allergies>()
            .Property(p => p.End)
            .HasColumnType("date");
        builder.Entity<ParentsData>()
            .Property(p => p.FatherBirthDate)
            .HasColumnType("date");
        builder.Entity<ParentsData>()
            .Property(p => p.MotherBirthDate)
            .HasColumnType("date");
        builder.Entity<Visit>()
            .Property(b => b.DiagDescription)
            .HasDefaultValue(string.Empty);
        builder.Entity<Visit>()
            .Property(b => b.Treatment)
            .HasDefaultValue(string.Empty);
        builder.Entity<Visit>()
            .Property(b => b.EvolutionNotes)
            .HasDefaultValue(string.Empty);
        builder.Entity<Visit>()
            .Property(b => b.LabResults)
            .HasDefaultValue(string.Empty);
        builder.Entity<Visit>()
            .Property(b => b.OtherStudies)
            .HasDefaultValue(string.Empty);
        builder.Entity<Visit>()
            .Property(b => b.Observations)
            .HasDefaultValue(string.Empty);
        builder.Entity<PersonalBackground>()
            .Property(b => b.Other)
            .HasDefaultValue(string.Empty);
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
}
