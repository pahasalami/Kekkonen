using System.Linq;
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
        private readonly LoggingService _loggingService;

        public LoginService(DiscordSocketClient discordSocket, CommandService commandService,
            IConfigurationRoot configuration, LoggingService loggingService)
        {
            _discordSocket = discordSocket;
            _commandService = commandService;
            _configuration = configuration;
            _loggingService = loggingService;
        }

        public async Task StartAsync()
        {
            var api = _configuration.GetSection("api").GetChildren().Where(n => n.Key == "discord")
                .Select(x => x.Value).FirstOrDefault();
            
            await _discordSocket.LoginAsync(TokenType.Bot, api);
            await _discordSocket.StartAsync();

            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly());

            await _discordSocket.SetGameAsync("Hearts of Iron IV");
        }
    }
}