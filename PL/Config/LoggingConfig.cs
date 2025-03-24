// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggingConfig.cs" company="GymBuddy">
//   Copyright (c) GymBuddy. All rights reserved.
// </copyright>
// <summary>
//   Provides configuration for Serilog logging.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace PL.Controllers
{
    using Serilog;

    /// <summary>
    /// Provides configuration for Serilog logging.
    /// </summary>
    public static class LoggingConfig
    {
        /// <summary>
        /// Configures Serilog logging for the application.
        /// </summary>
        public static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Seq("http://localhost:5341")
                .Enrich.FromLogContext()
                .CreateLogger();


        }
    }
}