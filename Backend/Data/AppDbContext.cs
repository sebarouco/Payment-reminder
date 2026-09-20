using Microsoft.EntityFrameworkCore;
using PaymentReminder.Api.Models;

namespace PaymentReminder.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Reminder> Reminders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasDefaultValue("User");
            });

            // Payment configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Payments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Reminder configuration
            modelBuilder.Entity<Reminder>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Payment)
                    .WithMany(p => p.Reminders)
                    .HasForeignKey(e => e.PaymentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}