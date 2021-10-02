using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using DSharpPlus;
using Newtonsoft.Json;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Ranker
{
    public class NormalCommands : BaseCommandModule
    {
        private readonly IDatabase _database;
        public NormalCommands(IDatabase database)
        {
            _database = database;
        }

        [Command("rank")]
        public async Task RankCommand(CommandContext ctx, DiscordUser user = null)
        {
            if (user == null)
            {
                user = ctx.User;
            }
            Rank currentUserRank = await _database.GetAsync(ctx.User.Id, ctx.Guild.Id);
            Rank rank = await _database.GetAsync(user.Id, ctx.Guild.Id);
            if (rank.Xp > 0) 
            {
                MemoryStream stream;
                if (currentUserRank.Fleuron)
                {
                    stream = await Commands.RankFleuron(_database, ctx.Guild, user, rank);
                }
                else
                {
                    stream = await Commands.RankZeealeid(_database, ctx.Guild, user, rank);
                }

                try
                {
                    await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithFile("rank.png", stream));
                }
                catch
                { }
            } 
            else await ctx.RespondAsync("You aren't ranked yet. Send some messages first, then try again.");

        }
    }
}
