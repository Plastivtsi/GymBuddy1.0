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
    //using DAL.Repositories;
    using DAL.Repositorie;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.DataProtection;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            LoggingConfig.ConfigureLogging();
            builder.Host.UseSerilog();

            // Реєстрація DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
                       .EnableSensitiveDataLogging()
                       .EnableDetailedErrors());
            builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")))
            .SetApplicationName("GymBuddy");

            builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
            {
                options.User.AllowedUserNameCharacters = null; // Дозволяє будь-які символи
                options.User.RequireUniqueEmail = false;

                options.Password.RequiredLength = 6; // Залишаємо мінімальну довжину 6 символів
                options.Password.RequireDigit = false; // Вимикаємо вимогу цифр
                options.Password.RequireLowercase = false; // Вимикаємо вимогу малих букв
                options.Password.RequireUppercase = false; // Вимикаємо вимогу великих букв
                options.Password.RequireNonAlphanumeric = false; // Вимикаємо вимогу спеціальних символів
                options.Password.RequiredUniqueChars = 1; // Вимикаємо вимогу унікальних символів
                // Налаштування часу дії токена
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


            // builder.Services.AddScoped<ICreateUser, Autorization>();
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

                // Перевіряємо, чи існує роль "User"
                if (!await roleManager.RoleExistsAsync("User"))
                {
                    // Створюємо роль "User", якщо її немає
                    var role = new IdentityRole<int> { Name = "User" };
                    var result = await roleManager.CreateAsync(role);
                    Log.Information("Роль 'User' створено");
                }
                // Опціонально: створюємо роль "Admin" (якщо потрібно)
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