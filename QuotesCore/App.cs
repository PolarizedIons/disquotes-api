using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using QuotesCore.Database;
using QuotesLib.Nats;
using Serilog;

namespace QuotesCore
{
    public class App : IHostedService
    {
        private readonly IConfiguration _config;
        private readonly DatabaseContext _databaseContext;
        private readonly IServiceProvider _serviceProvider;
        private IEnumerable<NatsResponder> _natsResponders;

        public App(IConfiguration config, DatabaseContext databaseContext, IServiceProvider serviceProvider)
        {
            _config = config;
            _databaseContext = databaseContext;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Information("Migrating Database...");
            await _databaseContext.Database.MigrateAsync(cancellationToken);
            
            Log.Information("Activating NATS responders");
            _natsResponders = NatsResponder.ActivateAll(_serviceProvider);
            // We need to access the IEnumerable for them to actually activate :(
            Log.Debug("Found {count} responders: {@names}", _natsResponders.Count(), _natsResponders.Select(x => x.GetType().Name));
            
            Log.Information("Ready :)");

            await Task.Delay(-1, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
