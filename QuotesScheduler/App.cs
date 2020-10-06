namespace QuotesScheduler
{
    public class App
    {
        public void Run()
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
                    .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromMinutes(int.Parse(Configuration["Scheduler:DiscordUserUpdateInterval"]))).RepeatForever())
                    .WithDescription("Trigger for Discord user updates")
                );
            });

        }
    }
}