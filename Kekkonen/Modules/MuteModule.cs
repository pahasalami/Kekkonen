using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kekkonen.Services;
using Microsoft.Extensions.Configuration;

namespace Kekkonen.Modules
{
    [Name("Mute")]
    [RequireContext(ContextType.Guild)]
    public class MuteModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfigurationRoot _configuration;
        private readonly LoggingService _loggingService;
        private readonly MuteService _muteService;

        private readonly Regex _regex = new Regex("(?<Number>[0-9]{1,2})(?<TimeUnit>[smhd]{1})");

        public MuteModule(IConfigurationRoot configuration, MuteService muteService, LoggingService loggingService)
        {
            _configuration = configuration;
            _muteService = muteService;
            _loggingService = loggingService;
        }

        [Command("mute")]
        [RequireUserPermission(GuildPermission.SendMessages)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task MuteUser([Remainder] string input)
        {
            // todo convert this to an attribute
            // get mute channel from config
            var adminChannel = _configuration.GetSection("mute").GetChildren().Where(n => n.Key == "channel")
                .Select(x => x.Value).FirstOrDefault();

            // if message channel is not the admin channel
            if (Context.Channel.Name != adminChannel) return;

            // get mute role from config
            var configRole = _configuration.GetSection("mute").GetChildren().Where(n => n.Key == "role")
                .Select(x => x.Value).FirstOrDefault();

            // map config mute role from string to server role
            var role = Context.Guild.Roles.FirstOrDefault(n => n.Name == configRole);
            if (role == null)
            {
                // server role not found
                await ReplyAsync($"Unable to mute, role '{configRole}' has not been set.");
                return;
            }

            // get mentioned users in message
            var users = Context.Message.MentionedUsers;

            // parse regexp to time unit and numeric value
            var parsedInput = _regex.Match(input);
            if (parsedInput.Success == false)
            {
                // regex was not a match
                await ReplyAsync("Unable to parse time unit format.");
                return;
            }

            // parse string to numeric value
            var unit = parsedInput.Groups["TimeUnit"].Value;
            if (int.TryParse(parsedInput.Groups["Number"].Value, out var number) == false)
            {
                await ReplyAsync($"'{parsedInput.Groups["Number"].Value}' is not a valid number.");
                return;
            }

            var timespan = new TimeSpan();
            switch (unit)
            {
                case "s":
                    // seconds
                    timespan = timespan.Add(new TimeSpan(0, 0, 0, number));
                    break;
                case "m":
                    // minutes
                    timespan = timespan.Add(new TimeSpan(0, 0, number, 0));
                    break;
                case "h":
                    // hours
                    timespan = timespan.Add(new TimeSpan(0, number, 0, number));
                    break;
                case "d":
                    // days
                    timespan = timespan.Add(new TimeSpan(number, 0, 0, 0));
                    break;
                default:
                    // default 10m
                    timespan = timespan.Add(new TimeSpan(0, 0, 10, 0));
                    break;
            }


            foreach (var user in users)
                await _muteService.MuteUserAsync((SocketGuildUser) user, timespan, role, Context.User);
        }

        [Command("mutelist")]
        [RequireUserPermission(GuildPermission.SendMessages)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task GetMuteList()
        {
            var users = _muteService.GetMutedUsers();
            if (users.Count == 0)
            {
                await ReplyAsync("There are no muted users.");
                return;
            }

            var str = new StringBuilder();

            // build list output
            str.AppendLine("```");
            foreach (var user in users)
                str.AppendLine(
                    $"{user.User.Username} for {user.MuteLength} at {user.MuteTimestamp} by {user.MuteOwner.Username}");
            str.AppendLine("```");


            await ReplyAsync(str.ToString());
        }
    }
}