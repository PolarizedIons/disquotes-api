using System.Threading.Tasks;
using QuotesBot.Services;
using Serilog;

namespace QuotesBot
{
    public class App
    {
        private readonly DiscordService _discordService;

        public App(DiscordService discordService)
        {
            _discordService = discordService;
        }

        public async Task Run()
        {
            Log.Information("Logging in...");
            await _discordService.LoginAndStart();
            Log.Information("Bot started");
            await Task.Delay(-1);
            await _discordService.Logout();
        }
    }
}
