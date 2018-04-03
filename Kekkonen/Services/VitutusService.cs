using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Kekkonen.Services
{
    public class VitutusService
    {
        private readonly ulong _channelId = 123;
        private readonly IConfigurationRoot _configuration;

        private readonly DiscordSocketClient _discordSocket;

        public VitutusService(IConfigurationRoot configuration, DiscordSocketClient discordSocket)
        {
            _discordSocket = discordSocket;
            _configuration = configuration;

            Task.Run(async () =>
            {
                while (true)
                {
                    DeleteExpiredMessagesAsync(new TimeSpan(1, 0, 0, 0));
                    await Task.Delay(new TimeSpan(0, 0, 0, 5));
                }
            });
        }

        private async void DeleteExpiredMessagesAsync(TimeSpan expiry)
        {
            var channel = (ISocketMessageChannel) _discordSocket.GetChannel(_channelId);
            var messages = await channel.GetMessagesAsync().Flatten();

            // todo: remove messages
        }
    }
}