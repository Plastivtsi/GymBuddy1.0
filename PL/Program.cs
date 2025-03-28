namespace PL
{
    using PL.Controllers;
    using Serilog;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using BLL.Models;
    using DAL.Models;
    using BLL.Models.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using BLL.Service; // Додаємо для UserService
    using DAL.Interfaces; // Додаємо для IUserRepository
    using DAL.Repositories; // Додаємо для UserRepository

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            LoggingConfig.ConfigureLogging();
            builder.Host.UseSerilog();

            // Реєстрація DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Реєстрація репозиторію та сервісу
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();

            // Інші сервіси
            builder.Services.AddScoped<ICreateUser, Autorization>();
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