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
                Log.Debug("Updating {DiscordUser} ({Id} - {DiscordId})", user.Username + "#" + user.Discriminator, user.Id, user.DiscordId);
                await _natsUserService.UpdateUser(user.Id, await _natsDiscordService.GetUser(ulong.Parse(user.DiscordId)));
            }
        }
    }
}
