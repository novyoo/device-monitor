using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DeviceOptimizer.Api.Models;

namespace DeviceOptimizer.Api.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Device> Devices => Set<Device>();
        public DbSet<CheckIn> CheckIns => Set<CheckIn>();
        public DbSet<ModelFootprint> ModelFootprints => Set<ModelFootprint>();
        public DbSet<DecisionRecord> DecisionRecords => Set<DecisionRecord>();
        public DbSet<Alert> Alerts => Set<Alert>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>()
                .Property(d => d.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Device>()
                .Property(d => d.Personality)
                .HasConversion<string>();

            modelBuilder.Entity<Device>()
                .HasIndex(d => d.ApiKey)
                .IsUnique();

            modelBuilder.Entity<ModelFootprint>()
                .HasIndex(f => f.Model)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
