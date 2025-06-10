using Microsoft.EntityFrameworkCore;
using LimedikaWebApp.Models;

namespace LimedikaWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ClientInfo> Clients { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // Unique index to avoid duplicate client entries
            modelBuilder.Entity<ClientInfo>()
                .HasIndex(c => new {c.ClientName, c.Address})
                .IsUnique();
        }
    }
}
