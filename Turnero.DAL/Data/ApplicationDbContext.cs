using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Turnero.DAL.Models;

namespace Turnero.DAL.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Turn>()
        .Property(b => b.DateTurn)
        .HasDefaultValueSql("getdate()");
    }
    public DbSet<Turn> Turns { get; set; }
    public DbSet<Medic> Medics { get; set; }
    public DbSet<TimeTurn> TimeTurns { get; set; }
}
