using System.Threading.Tasks;
using Quartz;
using QuotesLib.Services;
using Serilog;

namespace QuotesApi.Schedules
{
    [DisallowConcurrentExecution]
    public class UpdateDiscordUsers : IJob
    {
        private readonly NatsDiscordService _natsDiscordService;
        private readonly NatsUserService _natsUserService;

        public UpdateDiscordUsers(NatsDiscordService natsDiscordService, NatsUserService natsUserService)
        {
            _natsDiscordService = natsDiscordService;
            _natsUserService = natsUserService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (!await _natsDiscordService.IsLoggedIn())
            {
                Log.Debug("Discord not logged in yet...");
                return;
            }

            foreach (var user in await _natsUserService.FindAllUsers())
            {
                var discordUser = await _natsDiscordService.GetUser(ulong.Parse(user.DiscordId));
                if (discordUser == null)
                {
                    Log.Debug("Couldn't update {DiscordUser} ({Id} - {DiscordId})", user.Username + "#" + user.Discriminator, user.Id, user.DiscordId);
                    continue;
                }

                Log.Debug("Updating {DiscordUser} ({Id} - {DiscordId})", user.Username + "#" + user.Discriminator, user.Id, user.DiscordId);
                await _natsUserService.UpdateUser(user.Id, discordUser);
            }
        }
    }
}
