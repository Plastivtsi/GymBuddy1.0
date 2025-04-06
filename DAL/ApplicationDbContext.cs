using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL.Models
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        //public DbSet<User> Users { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Training> Trainings { get; set; }
        public DbSet<Exercise> Exercises { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().ToTable("Users");
            // Вказуємо, що User мапиться на таблицю "Users"
            modelBuilder.Entity<User>().ToTable("Users");
            // Налаштування для ролей (якщо потрібно)
            modelBuilder.Entity<IdentityRole<int>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
            modelBuilder.Entity("DAL.Models.User", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("integer");
                NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                b.HasKey("Id");
                b.ToTable("Users");
            });

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

            modelBuilder.Entity<User>()
                .Property(u => u.Name)
                .IsRequired()
            .HasMaxLength(100); // Додаємо максимальну довжину для імені

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255); // Для email можна додати максимальну довжину

            modelBuilder.Entity<User>()
                .Property(u => u.Weight)
                .IsRequired(); // Вага обов'язкова для кожного користувача

            modelBuilder.Entity<User>()
                .Property(u => u.Height)
                .IsRequired(); // Зріст також

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email) // Створюємо індекс на Email для покращення пошуку
                .IsUnique();
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