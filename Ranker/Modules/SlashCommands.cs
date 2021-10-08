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
    public class RequireOwnerOrManageGuild : SlashCheckBaseAttribute
    {
        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            return (await ctx.Guild.GetMemberAsync(ctx.User.Id)).Permissions.HasPermission(Permissions.ManageGuild) || ctx.Client.CurrentApplication.Owners.ToList().Contains(ctx.User);
        }
    }

    [SlashRequireGuild]
    public class SlashCommands : ApplicationCommandModule
    {
        private readonly IRankerRepository _database;
        public SlashCommands(IRankerRepository database)
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
            Rank currentUserRank = await _database.Ranks.GetAsync(ctx.User.Id, ctx.Guild.Id);
            Rank rank = await _database.Ranks.GetAsync(user.Id, ctx.Guild.Id);
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

        [ContextMenu(ApplicationCommandType.UserContextMenu, "Rank")]
        public async Task RankContext(ContextMenuContext ctx) {
            await ctx.CreateResponseAsync(
                InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral(true));

            Rank currentUserRank = await _database.Ranks.GetAsync(ctx.TargetMember.Id, ctx.Guild.Id);
            Rank rank = await _database.Ranks.GetAsync(ctx.TargetMember.Id, ctx.Guild.Id);
            if (rank.Xp > 0)
            {
                 MemoryStream stream;
                if (currentUserRank.Fleuron)
                {
                    stream = await Commands.RankFleuron(_database, ctx.Guild, ctx.TargetMember, rank);
                }
                else
                {
                    stream = await Commands.RankZeealeid(_database, ctx.Guild, ctx.TargetMember, rank);
                }

                DiscordWebhookBuilder builder = new DiscordWebhookBuilder();

                builder.AddFile("rank.png", stream);

                await ctx.EditResponseAsync(builder);
            }
            else await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This member isn't ranked yet."));
        }

        [SlashCommand("role", "Configures a level up role.")]
        [SlashRequireUserPermissions(Permissions.ManageGuild, false)]
        public async Task RoleCommand(InteractionContext ctx, [Option("level", "Level to configure")] long level, [Option("role", "Role to configure")] DiscordRole role = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (role == null)
            {
                await _database.Roles.RemoveAsync(ctx.Guild.Id, (ulong)level);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Role for level " + level.ToString() + " deconfigured!"));
            } else
            {
                await _database.Roles.UpsertAsync(ctx.Guild.Id, (ulong)level, role.Id, role.Name);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Role for level " + level.ToString() + " configured!"));
            }
            
        }

        [SlashCommand("fleuron", "Enables or disables Flueron's style.")]
        public async Task FleuronCommand(InteractionContext ctx, [Option("enable", "True for enabling, False for disabling")] bool enabled)
        {
            await ctx.CreateResponseAsync(
                InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral(true));

            Rank rank = await _database.Ranks.GetAsync(ctx.Member.Id, ctx.Guild.Id);

            if (enabled)
            {
                rank.Fleuron = true;
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Flueron's style enabled!"));
            } else
            {
                rank.Fleuron = false;
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Flueron's style disabled!"));
            }

            await _database.Ranks.UpsertAsync(ctx.Member.Id, ctx.Guild.Id, rank);
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

        [SlashCommand("migrate", "Migrates MEE6's data.")]
        [RequireOwnerOrManageGuild]
        public async Task MigrateCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var continueButton = new DiscordButtonComponent(ButtonStyle.Success, "continue", "Start migration");

            var cancelButton = new DiscordButtonComponent(ButtonStyle.Danger, "cancel", "Cancel");

            DiscordWebhookBuilder webhook = new DiscordWebhookBuilder();

            webhook.WithContent("Running this command will DELETE all ranking and roles data and REPLACE it with the one this server had with MEE6. Do you want to CONTINUE?");

            webhook.AddComponents(continueButton, cancelButton);

            await ctx.EditResponseAsync(webhook);
        }

        [SlashCommand("range", "Changes range of XP winned.")]
        [SlashRequireUserPermissions(Permissions.ManageGuild)]
        public async Task RangeCommand(InteractionContext ctx, [Option("min", "Minimum XP that can be win")] long min, [Option("max", "Maximum XP that can be win")] long max)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if ((max >= min) && (min > -1) && (max > -1))
            {
                Settings settings = await _database.Settings.GetAsync(ctx.Guild.Id);
                settings.MinRange = Convert.ToInt32(min);
                settings.MaxRange = Convert.ToInt32(max) + 1;
                await _database.Settings.UpsertAsync(ctx.Guild.Id, settings);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Range updated successfully!"));
            } else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Max has to be bigger or equal to min! (also, both have to be positive)"));
            }

        }
    }
}
