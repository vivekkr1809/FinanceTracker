using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace ReceiptTracker.Services.Logging;

/// <summary>
/// Configures Serilog logging for the application
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Configures Serilog with standard settings for the application
    /// </summary>
    /// <param name="isDevelopment">Whether the application is running in development mode</param>
    /// <returns>Configured Serilog logger</returns>
    public static Serilog.ILogger ConfigureSerilog(bool isDevelopment = false)
    {
        var logLevel = isDevelopment ? LogEventLevel.Debug : LogEventLevel.Information;

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(logLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithMachineName()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/receipt-tracker-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ThreadId}] {Message:lj}{NewLine}{Exception}");

        if (isDevelopment)
        {
            loggerConfiguration.WriteTo.Debug();
        }

        return loggerConfiguration.CreateLogger();
    }

    /// <summary>
    /// Adds Serilog to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="isDevelopment">Whether the application is running in development mode</param>
    public static void AddSerilog(this IServiceCollection services, bool isDevelopment = false)
    {
        var logger = ConfigureSerilog(isDevelopment);
        Log.Logger = logger;

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(logger, dispose: true);
        });
    }
}
