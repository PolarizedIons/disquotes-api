using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using QuotesApi.Schedules;
using QuotesLib.Extentions;
using QuotesScheduler;
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
                    services.AddQuartz(q =>
                    {
                        q.SchedulerName = "QuotesApi - Quartz Scheduler";
                
                        q.UseMicrosoftDependencyInjectionScopedJobFactory(options =>
                        {
                            // if we don't have the job in DI, allow fallback to configure via default constructor
                            options.AllowDefaultConstructor = true;
                        });

                        q.UseSimpleTypeLoader();
                        q.UseInMemoryStore();
                        q.UseDefaultThreadPool(tp =>
                        {
                            tp.MaxConcurrency = 10;
                        });
                
                        var updateDiscordUserJobKey = new JobKey("UpdateDiscordUsers", "discord");
                        q.AddJob<UpdateDiscordUsers>(j => j
                            .WithIdentity(updateDiscordUserJobKey)
                            .WithDescription("Updates Discord users in the database")
                        );

                        q.AddTrigger(t => t
                            .WithIdentity("Discord user update trigger")    
                            .ForJob(updateDiscordUserJobKey)
                            .StartNow()
                            .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromMinutes(int.Parse(hostCtx.Configuration["Scheduler:DiscordUserUpdateInterval"]))).RepeatForever())
                            .WithDescription("Trigger for Discord user updates")
                        );
                    });
                    
                    services.DiscoverAndMakeDiServicesAvailable();
                    
                    services.AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });

                    services.AddHostedService<App>();
                })
                .UseSerilog()
                .UseConsoleLifetime();
        }
    }
}
