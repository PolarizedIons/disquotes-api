using System;
using System.Threading.Tasks;
using Quartz;
using QuotesApi.Services;
using Serilog;

namespace QuotesApi.Schedules
{
    [DisallowConcurrentExecution]
    public class UpdateDiscordUsers : IJob
    {
        private readonly DiscordService _discordService;
        private readonly UserService _userService;

        public static DateTime? LastExecuteTime;

        public UpdateDiscordUsers(DiscordService discordService, UserService userService)
        {
            _discordService = discordService;
            _userService = userService;
        }
        
        public async Task Execute(IJobExecutionContext context)
        {
            LastExecuteTime = DateTime.UtcNow;

            if (!_discordService.IsLoggedIn)
            {
                Log.Debug("Discord not logged in yet...");
                return;
            }

            foreach (var user in _userService.FindAllUsers())
            {
                Log.Debug("Updating {DiscordUser} ({Id} - {DiscordId})", user.Username + "#" + user.Discriminator, user.Id, user.DiscordId);
                await _userService.UpdateUser(user, await _discordService.GetUser(ulong.Parse(user.DiscordId)));
            }
        }
    }
}
