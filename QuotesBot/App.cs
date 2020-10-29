using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly DiscordCommandHandler _commandHandler;

        public App(DiscordService discordService, IServiceProvider serviceProvider)
        {
            _discordService = discordService;
            _serviceProvider = serviceProvider;

            _commandHandler = _serviceProvider.GetRequiredService<DiscordCommandHandler>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Information("Activating NATS responders");
            _natsResponders = NatsResponder.ActivateAll(_serviceProvider);
            // We need to access the IEnumerable for them to actually activate :(
            Log.Debug("Found {count} responders: {@names}", _natsResponders.Count(), _natsResponders.Select(x => x.GetType().Name));
            
            Log.Debug("Initializing command handler");
            await _commandHandler.InitializeAsync();
            
            Log.Information("Logging in...");
            await _discordService.LoginAndStart();
            Log.Information("Bot started");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discordService.Logout();
        }
    }
}
