using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.Entities;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using DSharpPlus;
using Newtonsoft.Json;

namespace Ranker
{
    public class SlashCommands : ApplicationCommandModule
    {
        private readonly IDatabase _database;
        public SlashCommands(IDatabase database)
        {
            _database = database;
        }

        [SlashCommand("rank", "View the rank of a user.")]
        public async Task RankCommand(InteractionContext ctx, [Option("user", "User to view ranks for")] DiscordUser user = null)
        {
            await ctx.CreateResponseAsync(
                InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral(true));
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
                } else
                {
                    stream = await Commands.RankZeealeid(_database, ctx.Guild, user, rank);
                }

                DiscordWebhookBuilder builder = new DiscordWebhookBuilder();

                builder.AddFile("rank.png", stream);

                await ctx.EditResponseAsync(builder);
            } 
            else if (user.Id == ctx.User.Id) 
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You aren't ranked yet. Send some messages first, then try again."));
            else
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This member isn't ranked yet."));
        }

        [SlashCommand("role", "Configures a level up role.")]
        [SlashRequireUserPermissions(Permissions.ManageGuild, false)]
        public async Task RoleCommand(InteractionContext ctx, [Option("level", "Level to configure")] long level, [Option("role", "Role to configure")] DiscordRole role = null)
        {
            await ctx.CreateResponseAsync(
                InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral(true));
            if (role == null)
            {
                await _database.RemoveAsync(ctx.Guild.Id, (ulong)level);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Role deconfigured!"));
            } else
            {
                await _database.UpsertAsync(ctx.Guild.Id, (ulong)level, role.Id, role.Name);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Role configured!"));
            }
            
        }

        [SlashCommand("fleuron", "Enables or disables Flueron's style.")]
        public async Task FleuronCommand(InteractionContext ctx, [Option("enable", "True for enabling, False for disabling")] bool enabled)
        {
            await ctx.CreateResponseAsync(
                InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral(true));

            Rank rank = await _database.GetAsync(ctx.Member.Id, ctx.Guild.Id);

            if (enabled)
            {
                rank.Fleuron = true;
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Flueron's style enabled!"));
            } else
            {
                rank.Fleuron = false;
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Flueron's style disabled!"));
            }

            await _database.UpsertAsync(ctx.Member.Id, ctx.Guild.Id, rank);
        }

        [SlashCommand("levels", "Send leaderboard.")]
        public async Task LevelsCommand(InteractionContext ctx)
        {
            string domain = JsonConvert.DeserializeObject<ConfigJson>(File.ReadAllText("config.json")).Domain;
            await ctx.CreateResponseAsync(
                InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral(true));
            if (Uri.IsWellFormedUriString(domain, UriKind.Absolute))
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(domain + "/leaderboard/" + ctx.Guild.Id));
            } else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Leaderboard setup not completed!"));
            }
        }

        [SlashCommand("easter", "egg")]
        public async Task EasterCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(
                InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral(true));
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("https://www.youtube.com/watch?v=CBpSGsT7L6s"));
        }
    }
}
