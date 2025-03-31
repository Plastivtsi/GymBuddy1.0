using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DAL.Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Training> Trainings { get; set; }
        public DbSet<Exercise> Exercises { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.User1)
                .WithMany(u => u.FriendshipsAsUser1)
                .HasForeignKey(f => f.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.User2)
                .WithMany(u => u.FriendshipsAsUser2)
                .HasForeignKey(f => f.User2Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Training>()
                .HasOne(t => t.User)
                .WithMany(u => u.Trainings)
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<Exercise>()
                .HasOne(e => e.Training)
                .WithMany(t => t.Exercises)
                .HasForeignKey(e => e.TrainingId);

            modelBuilder.Entity<Training>()
                .Property(t => t.Id)
                .ValueGeneratedOnAdd(); 

            modelBuilder.Entity<Exercise>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd(); 

            modelBuilder.Entity<Training>()
                .Property(t => t.Date)
                .IsRequired(false);
        }
    }

    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql("Host=breakdatabase.postgres.database.azure.com;Port=5432;Database=GymBuddy;Username=postgres;Password=12345678bp!");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}