using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace Ranker
{
    public class RequireOwnerOrManageGuild : SlashCheckBaseAttribute
    {
        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            return (await ctx.Guild.GetMemberAsync(ctx.User.Id)).Permissions.HasPermission(Permissions.ManageGuild) || ctx.Client.CurrentApplication.Owners.ToList().Contains(ctx.User);
        }
    }
}