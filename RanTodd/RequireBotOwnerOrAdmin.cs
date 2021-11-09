using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace RanTodd
{
    public class RequireBotOwnerOrAdmin : SlashCheckBaseAttribute
    {
        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            return (await ctx.Guild.GetMemberAsync(ctx.User.Id)).Permissions.HasPermission(Permissions.Administrator) || ctx.Client.CurrentApplication.Owners.ToList().Contains(ctx.User);
        }
    }
}