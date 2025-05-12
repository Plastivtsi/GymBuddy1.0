namespace PL
{
    using PL.Controllers;
    using Serilog;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using BLL.Models;
    using DAL.Models;
    using BLL.Models.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using BLL.Service;
    using BLL.Interfaces;
    using DAL.Interfaces;
    using DAL.Repositorie;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.DataProtection;

    using Azure.Identity;
    using Azure.Security.KeyVault.Secrets;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            LoggingConfig.ConfigureLogging();
            builder.Host.UseSerilog();

            string connectionString;
             
            try
            {
                var keyVaultUrl = "https://gymbuddykey.vault.azure.net/";
                var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
                var dbSecret = await secretClient.GetSecretAsync("DatabaseConnectionString");
                connectionString = dbSecret.Value.Value; // Виправлено: прибрано зайве .Value
                Console.WriteLine("Секрет успішно отримано: {0}", connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка підключення до Key Vault: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                // Резервний рядок підключення для тестування
                connectionString = "Host=breakdatabase.postgres.database.azure.com;Port=5432;Database=GymBuddy;Username=postgres;Password=12345678bp!";
                Console.WriteLine("Використовується резервний рядок підключення.");
                // Прибрано throw, щоб програма продовжила роботу
            }

            // Реєстрація DbContext із рядком підключення
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString, npgsqlOptions => { })
                       .EnableSensitiveDataLogging()
                       .EnableDetailedErrors());

            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")))
                .SetApplicationName("GymBuddy");

            builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
            {
                options.User.AllowedUserNameCharacters = null;
                options.User.RequireUniqueEmail = false;

                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 1;

                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
                options.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultProvider;
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultProvider;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddScoped<DbContext, ApplicationDbContext>();

            builder.Services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Debug);
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            builder.Services.AddControllersWithViews();
            builder.Services.AddAuthorization();
            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromDays(1);
            });

            // Реєстрація репозиторіїв та сервісів
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ITrainingRepository, TrainingRepository>();
            builder.Services.AddScoped<ITrainingService, TrainingService>();
            builder.Services.AddScoped<ITrainingHistoryService, TrainingHistoryService>();
            builder.Services.AddScoped<ITrainingHistoryRepository, TrainingHistoryRepository>();
            builder.Services.AddScoped<ITrainingCalendar, TrainingCalendar>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IFriendshipService, FriendshipService>();
            builder.Services.AddControllersWithViews();

            // Додаємо підтримку сесій
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

                if (!await roleManager.RoleExistsAsync("User"))
                {
                    var role = new IdentityRole<int> { Name = "User" };
                    await roleManager.CreateAsync(role);
                    Log.Information("Роль 'User' створено");
                }
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    var role = new IdentityRole<int> { Name = "Admin" };
                    await roleManager.CreateAsync(role);
                }
                if (!await roleManager.RoleExistsAsync("Blocked"))
                {
                    var role = new IdentityRole<int> { Name = "Blocked" };
                    await roleManager.CreateAsync(role);
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            Log.Information("Застосунок запущено");
            await app.RunAsync();
        }
    }
}