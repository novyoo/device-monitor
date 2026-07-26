using Microsoft.EntityFrameworkCore;
using DeviceOptimizer.Api.Models;

namespace DeviceOptimizer.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Device> Devices => Set<Device>();
        public DbSet<CheckIn> CheckIns => Set<CheckIn>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>()
                .Property(d => d.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Device>()
                .HasIndex(d => d.ApiKey)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
