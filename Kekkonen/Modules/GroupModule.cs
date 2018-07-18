using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Kekkonen.Modules
{
    public class GroupModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfigurationRoot _configuration;

        public GroupModule(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        [Command("maakunta")]
        [RequireUserPermission(GuildPermission.SendMessages)]
        [RequireContext(ContextType.Guild)]
        public async Task AssignGroup(string message)
        {
            if (Context.Channel.Name != "bot-spam") return;

            var options = _configuration.GetSection("group").GetChildren().FirstOrDefault(n => n.Key == "options")
                ?.GetChildren().Select(x => x.Value).ToList();

            if (options == null)
            {
                // todo logging
                return;
            }

            var user = (IGuildUser) Context.User;
            var roles = Context.Guild.Roles;

            if (string.IsNullOrWhiteSpace(message))
            {
                await ReplyAsync($"{user.Mention}: Vaihtoehdot: {string.Join(", ", options)}");
                return;
            }

            var role =
                (IRole) roles.FirstOrDefault(
                    n => string.Equals(n.Name, message, StringComparison.OrdinalIgnoreCase));

            if (options.Any(n => string.Equals(message, n, StringComparison.OrdinalIgnoreCase)) == false ||
                role == null)
            {
                await ReplyAsync($"{user.Mention}: Vaihtoehdot: {string.Join(", ", options)}");
                return;
            }

            var authorRoles = ((SocketGuildUser) Context.Message.Author).Roles;
            foreach (var authorRole in authorRoles)
                if (options.Contains(authorRole.Name))
                    await user.RemoveRoleAsync(authorRole);

            await user.AddRoleAsync(role);
            await ReplyAsync($"{user.Mention}: Lisätty '{role.Name}'.");
        }
    }
}