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
    }

    public DbSet<Turn> Turns { get; set; }
    public DbSet<Medic> Medics { get; set; }
    public DbSet<TimeTurn> TimeTurns { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<ContactInfo> ContactInfo { get; set; }
    public DbSet<History> Histories { get; set; }
    public DbSet<GeneralHistory> GeneralHistories { get; set; }
    public DbSet<FamilyBackground> FamilyBackgrounds { get; set; }
    public DbSet<Familiar> Familiar { get; set; }
    public DbSet<Lifestyle> Lifestyle { get; set; }
    public DbSet<Allergies> Allergies { get; set; }
    public DbSet<ExamsGenHis> ExamsGenHis { get; set; }
    public DbSet<Visit> Visits { get; set; }
}
