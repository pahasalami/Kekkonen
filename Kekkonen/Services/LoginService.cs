using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Kekkonen.Services
{
    public class LoginService
    {
        private readonly CommandService _commandService;
        private readonly IConfigurationRoot _configuration;
        private readonly DiscordSocketClient _discordSocket;

        public LoginService(DiscordSocketClient discordSocket, CommandService commandService,
            IConfigurationRoot configuration)
        {
            _discordSocket = discordSocket;
            _commandService = commandService;
            _configuration = configuration;
        }

        public async Task StartAsync()
        {
            await _discordSocket.LoginAsync(TokenType.Bot, _configuration["discord_api"]);
            await _discordSocket.StartAsync();

            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly());
        }
    }
}