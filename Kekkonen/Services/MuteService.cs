using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Kekkonen.Services
{
    public class MuteService
    {
        private readonly IConfigurationRoot _configuration;
        private readonly DiscordSocketClient _discordSocket;
        private readonly LoggingService _loggingService;

        private readonly ConcurrentBag<MutedUser> _mutedUsers = new ConcurrentBag<MutedUser>();

        public MuteService(IConfigurationRoot configuration, DiscordSocketClient discordSocket,
            LoggingService loggingService)
        {
            _discordSocket = discordSocket;
            _configuration = configuration;
            _loggingService = loggingService;

            Task.Run(async () =>
            {
                while (true)
                {
                    foreach (var mute in _mutedUsers
                        .Where(n => n.MuteTimestamp + n.MuteLength < DateTimeOffset.Now))
                    {
                        await UnmuteUserAsync(mute);
                        await _loggingService.LogAsync(new LogMessage(LogSeverity.Info, "Mute",
                            $"'{mute.User.Username}' unmuted after for {mute.MuteLength} (set at {mute.MuteTimestamp})."));

                        _mutedUsers.TryTake(out var user);
                        await _loggingService.LogAsync(new LogMessage(LogSeverity.Info, "Mute",
                            $"Removed '{user.User.Username}' from bag."));
                    }

                    await Task.Delay(new TimeSpan(0, 0, 0, 1));
                }
            });
        }

        public List<MutedUser> GetMutedUsers()
        {
            return _mutedUsers.ToList();
        }

        public async Task MuteUserAsync(SocketGuildUser user, TimeSpan length, SocketRole role,
            SocketUser owner)
        {
            _mutedUsers.Add(new MutedUser
            {
                User = user,
                MuteLength = length,
                MuteTimestamp = DateTimeOffset.UtcNow,
                MuteRole = role,
                MuteOwner = owner
            });

            await user.AddRoleAsync(role, new RequestOptions
            {
                AuditLogReason = $"Muted by {owner?.Username ?? "Unknown"} for {length}"
            });

            await _loggingService.LogAsync(new LogMessage(LogSeverity.Info, "MuteService",
                $"{user?.Username} muted for {length} by {owner?.Username}."));

        }

        private async Task UnmuteUserAsync(MutedUser mutedUser)
        {
            await mutedUser.User.RemoveRoleAsync(mutedUser.MuteRole);
        }
    }

    public class MutedUser
    {
        public SocketGuildUser User { get; set; }
        public TimeSpan MuteLength { get; set; }
        public DateTimeOffset MuteTimestamp { get; set; }
        public SocketRole MuteRole { get; set; }
        public SocketUser MuteOwner { get; set; }
    }
}