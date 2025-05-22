using Microsoft.EntityFrameworkCore;
using System;
using Wildlife_DAL.Entities;

namespace Wildlife_DAL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<ObservationEntity> Observations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired();
            });

            modelBuilder.Entity<ObservationEntity>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.HasOne(o => o.User)
                      .WithMany(u => u.Observations)
                      .HasForeignKey(o => o.UserId);
            });
        }


    }
}
