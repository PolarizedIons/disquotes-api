using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using QuotesBot.Services;
using QuotesLib.Nats;
using Serilog;

namespace QuotesBot
{
    public class App : IHostedService
    {
        private readonly DiscordService _discordService;
        private readonly IServiceProvider _serviceProvider;
        private IEnumerable<NatsResponder> _natsResponders;

        public App(DiscordService discordService, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _discordService = discordService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Information("Activating NATS responders");
            _natsResponders = NatsResponder.ActivateAll(_serviceProvider);
            // We need to access the IEnumerable for them to actually activate :(
            Log.Debug("Found {count} responders: {@names}", _natsResponders.Count(), _natsResponders.Select(x => x.GetType().Name));

            Log.Information("Logging in...");
            await _discordService.LoginAndStart();
            Log.Information("Bot started");
            await Task.Delay(-1, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discordService.Logout();
        }
    }
}
