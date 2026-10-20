using Microsoft.EntityFrameworkCore;
using BTL_WEB.Models.Entities;

namespace BTL_WEB.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<Food> Foods => Set<Food>();
        public DbSet<DailyLog> DailyLogs => Set<DailyLog>();
        public DbSet<LogItem> LogItems => Set<LogItem>();
        public DbSet<FridgeItem> FridgeItems => Set<FridgeItem>();
        public DbSet<WeightLog> WeightLogs => Set<WeightLog>();
        public DbSet<InAppNotification> Notifications => Set<InAppNotification>();
        public DbSet<WorkoutLog> WorkoutLogs => Set<WorkoutLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User constraints
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // UserProfile 1-to-1 with User
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasOne(p => p.User)
                      .WithOne(u => u.Profile)
                      .HasForeignKey<UserProfile>(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Food search index
            modelBuilder.Entity<Food>(entity =>
            {
                entity.HasIndex(f => f.Name);
            });

            // DailyLog relationships
            modelBuilder.Entity<DailyLog>(entity =>
            {
                entity.HasOne(d => d.User)
                      .WithMany(u => u.DailyLogs)
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // LogItem relationships
            modelBuilder.Entity<LogItem>(entity =>
            {
                entity.HasOne(li => li.DailyLog)
                      .WithMany(d => d.LogItems)
                      .HasForeignKey(li => li.DailyLogId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(li => li.Food)
                      .WithMany(f => f.LogItems)
                      .HasForeignKey(li => li.FoodId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // FridgeItem relationships
            modelBuilder.Entity<FridgeItem>(entity =>
            {
                entity.HasOne(fi => fi.User)
                      .WithMany(u => u.FridgeItems)
                      .HasForeignKey(fi => fi.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(fi => fi.Food)
                      .WithMany()
                      .HasForeignKey(fi => fi.FoodId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // WeightLog relationships
            modelBuilder.Entity<WeightLog>(entity =>
            {
                entity.HasOne(w => w.User)
                      .WithMany(u => u.WeightLogs)
                      .HasForeignKey(w => w.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Notifications
            modelBuilder.Entity<InAppNotification>(entity =>
            {
                entity.HasOne(n => n.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
