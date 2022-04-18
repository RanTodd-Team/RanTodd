using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace RanTodd
{
    public class RequireBotOwnerOrAdmin : SlashCheckBaseAttribute
    {
        public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            return Task.Run(() => ctx.Member.Permissions.HasPermission(Permissions.Administrator) || ctx.Client.CurrentApplication.Owners.ToList().Contains(ctx.User));
        }
    }
}