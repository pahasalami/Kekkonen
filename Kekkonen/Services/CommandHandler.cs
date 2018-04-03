using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Kekkonen.Services
{
    public class CommandHandler
    {
        private readonly CommandService _commandService;
        private readonly IConfigurationRoot _configuration;

        private readonly DiscordSocketClient _discordSocket;
        private readonly IServiceProvider _provider;

        public CommandHandler(DiscordSocketClient discordSocket, CommandService commandService,
            IConfigurationRoot configuration, IServiceProvider provider)
        {
            _discordSocket = discordSocket;
            _commandService = commandService;
            _configuration = configuration;
            _provider = provider;

            _discordSocket.MessageReceived += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage socketMessage)
        {
            var message = socketMessage as SocketUserMessage;
            if (message?.Author.Id == _discordSocket.CurrentUser.Id)
            {
                return;
            }

            var context = new SocketCommandContext(_discordSocket, message);

            var pos = 0;
            if (message.HasStringPrefix(_configuration["prefix"], ref pos))
            {
                await _commandService.ExecuteAsync(context, pos, _provider);
            }
        }
    }
}