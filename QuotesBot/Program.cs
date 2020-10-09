using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuotesLib.Extentions;
using Serilog;
using Serilog.Events;

namespace QuotesBot
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Quartz", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                using var host = CreateHostBuilder(args).Build();
                await host.StartAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Fatal exception");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
        
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostCtx, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables();
                })
                .ConfigureServices((hostCtx, services) =>
                {
                    services.DiscoverAndMakeDiServicesAvailable();

                    services.AddHostedService<App>();
                })
                .UseSerilog()
                .UseConsoleLifetime();
        }
    }
}
