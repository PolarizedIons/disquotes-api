using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuotesCore.Database;
using QuotesLib.Nats;
using Serilog;

namespace QuotesCore
{
    public class App
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

        public async Task Run()
        {
            Log.Information("Migrating Database...");
            _databaseContext.Database.Migrate();
            
            Log.Information("Activating NATS responders");
            _natsResponders = NatsResponder.ActivateAll(_serviceProvider);
            // We need to access the IEnumerable for them to actually activate :(
            Log.Debug("Found {count} responders", _natsResponders.Count());
            
            Log.Information("Ready :)");

            await Task.Delay(-1);
        }
    }
}
