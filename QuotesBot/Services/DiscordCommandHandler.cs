using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using QuotesLib.Models;
using Serilog;

namespace QuotesBot.Services
{
    public class DiscordCommandHandler : ISingletonDiService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _services;
        
        public DiscordCommandHandler(DiscordSocketClient discord, CommandService commands, IConfiguration config, IServiceProvider services)
        {
            _discord = discord;
            _commands = commands;
            _config = config;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            var a = await _commands.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: _services
            );

            _discord.MessageReceived += HandleCommandAsync;
            _commands.CommandExecuted += OnCommandExecutedAsync;
        }

        private async Task HandleCommandAsync(SocketMessage socketMessage)
        {
            if (!(socketMessage is SocketUserMessage msg))
            {
                return;
            }

            var argPos = 0;
            if (!msg.HasStringPrefix(_config["Discord:BotPrefix"], ref argPos) || msg.Author.IsBot)
            {
                return;
            }

            var context = new CommandContext(_discord, msg);
            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _services
            );
        }

        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!string.IsNullOrEmpty(result.ErrorReason))
            {
                await context.Channel.SendMessageAsync("Error: " + result.ErrorReason);
                Log.Error("error {@result}", result);
            }
        }
    }
}
