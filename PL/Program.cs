// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="GymBuddy">
//   Copyright (c) GymBuddy. All rights reserved.
// </copyright>
// <summary>
//   Entry point of the GymBuddy application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PL
{
    using PL.Controllers;
    using Serilog;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// The entry point class for the GymBuddy application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point for the GymBuddy application.
        /// This method configures the application and starts the web server.
        /// </summary>
        /// <param name="args">An array of command-line arguments passed to the application.</param>
        public static void Main(string[] args)
        {
            /// <summary>
            /// A builder for the web application.
            /// </summary>
            var builder = WebApplication.CreateBuilder(args);

            // Налаштування Serilog перед створенням `builder`
            LoggingConfig.ConfigureLogging();
            builder.Host.UseSerilog(); // Вказуємо застосовувати Serilog

            // Додаємо сервіси
            builder.Services.AddControllersWithViews();
            // Додаємо підтримку сесій
            builder.Services.AddDistributedMemoryCache(); // Використовуємо пам'ять для збереження сесій
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Тривалість сесії
                options.Cookie.HttpOnly = true; // Захист від XSS-атак
                options.Cookie.IsEssential = true; // Необхідно для роботи в режимі GDPR
            });

            /// <summary>
            /// The web application instance.
            /// </summary>
            var app = builder.Build();

            // Налаштування HTTP-конвеєра
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

            // Логування запуску застосунку
            Log.Information("Застосунок запущено");

            // Запуск застосунку
            app.Run();
        }
    }
}
