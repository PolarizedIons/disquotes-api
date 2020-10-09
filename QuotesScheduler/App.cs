using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace QuotesScheduler
{
    public class App : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Information("Running!");
            await Task.Delay(-1, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Information("Shutting down");
            return Task.CompletedTask;
        }
    }
}
