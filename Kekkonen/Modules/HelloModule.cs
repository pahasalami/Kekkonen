using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Kekkonen.Modules
{
    [Name("Hello")]
    [RequireContext(ContextType.Guild)]
    public class HelloModule : ModuleBase<SocketCommandContext>
    {
        [Command("hello")]
        [RequireUserPermission(GuildPermission.SendMessages)]
        public async Task Hello()
        {
            await ReplyAsync($"hello {Context.User.Mention} :wave:");
        }
    }
}