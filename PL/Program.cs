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

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            LoggingConfig.ConfigureLogging();
            builder.Host.UseSerilog();

            // Реєстрація DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
                       .EnableSensitiveDataLogging()
                       .EnableDetailedErrors());
            builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
            {
                options.Password.RequiredLength = 6; // Залишаємо мінімальну довжину 6 символів
                options.Password.RequireDigit = false; // Вимикаємо вимогу цифр
                options.Password.RequireLowercase = false; // Вимикаємо вимогу малих букв
                options.Password.RequireUppercase = false; // Вимикаємо вимогу великих букв
                options.Password.RequireNonAlphanumeric = false; // Вимикаємо вимогу спеціальних символів
                options.Password.RequiredUniqueChars = 1; // Вимикаємо вимогу унікальних символів (можна залишити 1)
            }).AddEntityFrameworkStores<ApplicationDbContext>()
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

            // Реєстрація репозиторіїв та сервісів
           
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ITrainingRepository, TrainingRepository>();
            builder.Services.AddScoped<ITrainingService, TrainingService>();
            builder.Services.AddScoped<ITrainingHistoryService, TrainingHistoryService>();
            builder.Services.AddScoped<ITrainingHistoryRepository, TrainingHistoryRepository>();

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
            app.Run();
        }
    }
}