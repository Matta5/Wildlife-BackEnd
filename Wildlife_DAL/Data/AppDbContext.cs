using Microsoft.EntityFrameworkCore;
using Wildlife_DAL.Entities;

namespace Wildlife_DAL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<ObservationEntity> Observations { get; set; }
        public DbSet<SpeciesEntity> Species { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired();
                entity.Property(u => u.Email).IsRequired();
                entity.Property(u => u.PasswordHash).IsRequired();
            });

            modelBuilder.Entity<SpeciesEntity>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.ScientificName).IsRequired();
                entity.Property(s => s.CommonName).IsRequired();
                entity.Property(s => s.ImageUrl).IsRequired();
            });

            modelBuilder.Entity<ObservationEntity>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.HasOne(o => o.User)
                      .WithMany(u => u.Observations)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(o => o.Species)
                      .WithMany(s => s.Observations)
                      .HasForeignKey(o => o.SpeciesId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
