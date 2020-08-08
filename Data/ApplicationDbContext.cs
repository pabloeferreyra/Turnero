using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Turnero.Models;

namespace Turnero.Data
{
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
            builder.Entity<Medic>()
            .HasMany(m => m.Turns)
            .WithOne(t => t.Medic);
            builder.Entity<TimeTurnViewModel>()
                .HasMany(t => t.Turns)
                .WithOne(v => v.Time);
        }
        public DbSet<Turn> Turns { get; set; }
        public DbSet<Medic> Medics { get; set; }
        public DbSet<TimeTurnViewModel> TimeTurns { get; set; }
    }
}
