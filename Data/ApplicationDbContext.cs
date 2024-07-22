namespace Turnero.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
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
    public DbSet<Available> Available { get; set; }
}
