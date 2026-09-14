using Microsoft.EntityFrameworkCore;
using SmartX.Api.Models;
using System.Reflection.Emit;

namespace SmartX.Api.Data
{
    public class SmartXDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<SensorNode> SensorNodes { get; set; }

        public SmartXDbContext(DbContextOptions<SmartXDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.SensorNode)
                .WithOne(n => n.User)
                .HasForeignKey<SensorNode>(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SensorNode>()
                .HasIndex(n => n.MacAddress)
                .IsUnique();

            modelBuilder.Entity<SensorNode>()
                .HasIndex(n => n.Barcode)
                .IsUnique();
        }
    }
}