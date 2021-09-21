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

namespace Ranker
{
    public class Commands : ApplicationCommandModule
    {
        // Designed by Fleuron
        private readonly IDatabase _database;
        public Commands(IDatabase database)
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
            Rank rank = await _database.GetAsync(user.Id, ctx.Guild.Id);
            if (rank.Fleuron)
            {
                await RankFleuron(ctx, user.Id, rank);
            } else
            {
                await RankZeealeid(ctx, user.Id, rank);
            }
        }

        [SlashCommand("role", "Configures a level up role.")]
        public async Task RoleCommand(InteractionContext ctx, [Option("level", "Level to configure")] long level, [Option("role", "Role to configure")] DiscordRole role = null)
        {
            await ctx.CreateResponseAsync(
                InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral(true));
            if (role == null)
            {
                await _database.UpsertAsync((ulong)level, 0);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Role deconfigured!"));
            } else
            {
                await _database.UpsertAsync((ulong)level, role.Id);
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

        public async Task RankZeealeid(InteractionContext ctx, ulong userId, Rank rank)
        {
            DiscordUser user = await ctx.Client.GetUserAsync(userId);
            string username = rank.Username ?? user.Username;
            string discriminator = rank.Discriminator ?? user.Discriminator;
            string pfpUrl = rank.Avatar ?? user.AvatarUrl;
            ulong level = rank.Level;

            ulong gottenXp = rank.Xp;
            ulong maxXp = rank.NextXp;

            var list = (await _database.GetAsync()).OrderByDescending(f => f.Xp).ToList();

            int leader = list.IndexOf(list.FirstOrDefault(f => f.User == userId)) + 1;

            Image<Rgba32> image = new Image<Rgba32>(934, 282);
            var img = Image.Load("./Images/Background.png");
            image.Mutate(x => x.DrawImage(img, new Point(0, 0), 1));

            var rect = new Rectangle(0, 0, 10, 382);
            image.Mutate(x => x.Fill(Color.FromRgb(0, 166, 234), rect));

            FontCollection fonts = new FontCollection();
            var family1 = fonts.Install("./wwwroot/fonts/selawkb.ttf");
            var family2 = fonts.Install("./wwwroot/fonts/selawk.ttf");

            var font1 = new Font(family1, 38f, FontStyle.Bold);
            var font2 = new Font(family2, 38f);
            var font3 = new Font(family2, 24f);

            DrawingOptions drawingOptions = new DrawingOptions()
            {
                TextOptions = new TextOptions()
                {
                    HorizontalAlignment = HorizontalAlignment.Right
                }
            };

            image.Mutate(x =>
                x.DrawText(options: drawingOptions, "Level " + level + "   Rank #" + leader, font1, Color.White, new Point(934 - 107, 193 - 65)));

            image.Mutate(x =>
                x.DrawText(username, font1, Color.White, new Point(51, 215 - 30)));

            var measure = TextMeasurer.Measure(username, new RendererOptions(font1));

            image.Mutate(x =>
                x.DrawText("#" + discriminator, font2, Color.FromRgb(153, 153, 153), new Point(60 + (int)measure.Width, 215 - 30)));

            image.Mutate(x =>
                x.DrawText(drawingOptions, gottenXp + "/" + maxXp, font3, Color.White, new Point(934 - 47, 223 - 30)));

            var paths = Extentions.GoodBuildCorners(836, 5, 4).Transform(Matrix3x2.CreateTranslation(51, 179));
            var paths2 = Extentions.GoodBuildCorners((int)(678 * (gottenXp * 100 / maxXp)) / 100, 5, 4).Transform(Matrix3x2.CreateTranslation(51, 179));

            image.Mutate(x => x.Fill(Color.White, paths));
            image.Mutate(x => x.Fill(Color.FromRgb(0, 166, 234), paths2));

            var propic = Image.Load(new WebClient().DownloadData("https://cdn.discordapp.com/embed/avatars/1.png"));
            try
            {
                propic = Image.Load(new WebClient().DownloadData(pfpUrl));
            }
            catch { }

            propic.Mutate(x => x.Resize(new ResizeOptions()
            {
                Mode = ResizeMode.Stretch,
                Size = new Size(80, 80)
            }));

            Image pfpRound = Extentions.RoundCorners(propic, 40);

            image.Mutate(x => x.DrawImage(pfpRound, new Point(51, 89), 1f));

            var stream = new MemoryStream();
            image.SaveAsPng(stream);
            stream.Position = 0;

            try
            {
                await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithFile("rank.png", stream));
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I have sent the rank card to you via DM."));
            }
            catch
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Sorry, but I cannot send a DM to you. Can you check if DM from members is enabled?"));
            }
        }


        public async Task RankFleuron(InteractionContext ctx, ulong userId, Rank rank)
        {
           DiscordUser user = await ctx.Client.GetUserAsync(userId);
            string username = rank.Username ?? user.Username;
            string discriminator = rank.Discriminator ?? user.Discriminator;
            string pfpUrl = rank.Avatar ?? user.AvatarUrl;
            ulong level = rank.Level;

            ulong gottenXp = rank.Xp;
            ulong maxXp = rank.NextXp;

            var list = (await _database.GetAsync()).OrderByDescending(f => f.Xp).ToList();

            int leader = list.IndexOf(list.FirstOrDefault(f => f.User == userId)) + 1;

            Image<Rgba32> image = new Image<Rgba32>(934, 282);
            var background = new Rectangle(0, 0, 934, 382);
            image.Mutate(x => x.Fill(Color.Black, background));

            FontCollection fonts = new FontCollection();
            var metropolis = fonts.Install("./Fonts/Metropolis/Metropolis-Regular.ttf");
            var epilogue = fonts.Install("./Fonts/Epilogue/static/Epilogue-Regular.ttf");


            var font1 = new Font(metropolis, 54f);

            var font2 = new Font(metropolis, 38f);

            int measure = (int)TextMeasurer.Measure(username, new RendererOptions(font1)).Width + (int)TextMeasurer.Measure(" #" + discriminator, new RendererOptions(font2)).Width;

            int widthUsernameContainer = ((measure + 180) > 420) ? measure + 180 : 420;

            var usernameContainer = new Rectangle(0, 0, widthUsernameContainer, 82);

            Image userInfoCard = new Image<Rgba32>(usernameContainer.Width, usernameContainer.Height);

            userInfoCard.Mutate(x => x.Fill(Color.FromRgb(50, 169, 229), usernameContainer));
            userInfoCard = Extentions.RoundBottomRight(userInfoCard, 20);

            image.Mutate(x => x.DrawImage(userInfoCard, 1f));

            DrawingOptions drawingOptions = new DrawingOptions()
            {
                TextOptions = new TextOptions()
                {
                    HorizontalAlignment = HorizontalAlignment.Right
                }
            };

            image.Mutate(x => x.DrawText(options: drawingOptions, "#" + discriminator, font2, Color.FromRgb(39, 85, 108), new Point(widthUsernameContainer - 10, 30)));

            var measure2 = TextMeasurer.Measure(" #" + discriminator, new RendererOptions(font2));

            image.Mutate(x => x.DrawText(options: drawingOptions, username, font1, Color.Black, new Point(widthUsernameContainer - (int)measure2.Width, 17)));

            var propic = Image.Load(new WebClient().DownloadData("https://cdn.discordapp.com/embed/avatars/1.png"));
            try
            {
                propic = Image.Load(new WebClient().DownloadData(pfpUrl));
            }
            catch { }

            propic.Mutate(x => x.Resize(new ResizeOptions()
            {
                Mode = ResizeMode.Stretch,
                Size = new Size(130, 130)
            }));

            Image pfpRound = Extentions.RoundCorners(propic, 69);

            image.Mutate(x => x.DrawImage(pfpRound, new Point(18, 18), 1f));

            var stream = new MemoryStream();
            image.SaveAsPng(stream);
            stream.Position = 0;

            try
            {
                await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithFile("rank.png", stream));
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I have sent the rank card to you via DM."));
            }
            catch
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Sorry, but I cannot send a DM to you. Can you check if DM from members is enabled?"));
            }
        }
    }
}
