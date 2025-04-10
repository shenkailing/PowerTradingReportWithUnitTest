using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PowerTradingReport.Services;
using Serilog;
using System;

namespace PowerTradingReport
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/power_trading_report.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
                .CreateLogger();

            try
            {
                

                Log.Information("Starting Power Trading Report Service...");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "The application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IPowerTradeService, PowerTradeService>();
                    services.AddHostedService<Worker>();
                });
    }
}
