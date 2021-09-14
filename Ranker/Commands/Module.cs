using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Ranker.Commands
{
    public class Module : BaseCommandModule
    {
        // Made by @Ahmed605, @Zeealeid, @KojiOdyssey, @itsWindows11 and @SapphireDisD (GitHub). Also dAKirby, Fleuron, Gary and Rip (Discord).

        // If command contains a mention, id or name

        [Command("rank")]
        public async Task RankCommand(CommandContext ctx, DiscordMember member)
        {
            await Rank(ctx, member.Id.ToString());
        }

        // Else...

        [Command("rank")]
        public async Task RankCommand(CommandContext ctx)
        {
            await Rank(ctx, ctx.Message.Author.Id.ToString());
        }

        public async Task Rank(CommandContext ctx, string userId)
        {

            // Fetch database data

            List<Rank> orderedRanks = Program.db.Table<Rank>().ToList().OrderByDescending(x => x.Xp)?.ToList();

            if(!String.IsNullOrEmpty(orderedRanks.Find(x => x.Id == userId)?.Id)) { 
            string[] ranks = { };
            foreach (Rank rankOG in orderedRanks)
            {
                var pre = ranks.ToList();
                pre.Add(rankOG.Id);
                ranks = pre.ToArray();
            }

                // Get user data

                string username = orderedRanks.Find(x => x.Id == userId).Username;
                string discriminator = orderedRanks.Find(x => x.Id == userId).Discriminator;
                string pfpUrl = "https://cdn.discordapp.com/avatars/" + userId + "/" + orderedRanks.Find(x => x.Id == userId).Avatar + ".png?size=2048";

            // Get user rank

            var rankO = orderedRanks.Find(x => x.Id == userId);
                int level = int.Parse(rankO?.Level ?? "0");

                // Get the xp, the xp needed to level up, and the rank

                int gottenXp = int.Parse(rankO?.LevelXp ?? "0");
                int maxXp = Convert.ToInt32(5 * Math.Pow(level, 2) + (50 * level) + 100);
                int rank = Array.IndexOf(ranks, userId) + 1;

                // Image creation

                Image<Rgba32> image = new Image<Rgba32>(934, 282);

                // Image background

                var img = Image.Load("./Images/Background.png");
                image.Mutate(x => x.DrawImage(img, new Point(0, 0), 1));

                // Image rectangle at the left

                var rect = new Rectangle(0, 0, 10, 382);
                image.Mutate(x => x.Fill(Color.FromRgb(0, 166, 234), rect));

                // Load fonts and text settings

                FontCollection fonts = new FontCollection();
                var family1 = fonts.Install("./wwwroot/fonts/selawkb.ttf");
                var family2 = fonts.Install("./wwwroot/fonts/selawk.ttf");

                var font1 = new Font(family1, 38f, FontStyle.Bold);
                var font3 = new Font(family2, 38f);
                var font4 = new Font(family2, 24f);

                DrawingOptions drawingOptions = new DrawingOptions()
                {
                    TextOptions = new TextOptions()
                    {
                        HorizontalAlignment = HorizontalAlignment.Right
                    }
                };

                // Write level and rank text

                image.Mutate(x =>
                    x.DrawText(options: drawingOptions, "Rank #" + rank + "   Level " + level, font1, Color.White, new Point(934 - 107, 193 - 65)));

                // Write username

                image.Mutate(x =>
                x.DrawText(username, font1, Color.White, new Point(51, 215 - 30)));

                // Write discriminator

                var measure = TextMeasurer.Measure(username, new RendererOptions(font1));

                image.Mutate(x =>
                    x.DrawText("#" + discriminator, font3, Color.FromRgb(153, 153, 153), new Point(60 + (int)measure.Width, 215 - 30)));

                // Write their xp

                image.Mutate(x =>
                    x.DrawText(drawingOptions, gottenXp + "/" + maxXp, font4, Color.White, new Point(934 - 47, 223 - 30)));

                // Draw progress bar

                var paths = Extentions.GoodBuildCorners(836, 5, 4).Transform(Matrix3x2.CreateTranslation(51, 179));
                var paths2 = Extentions.GoodBuildCorners((678 * ((gottenXp * 100) / maxXp)) / 100, 5, 4).Transform(Matrix3x2.CreateTranslation(51, 179));

                image.Mutate(x => x.Fill(Color.White, paths));
                image.Mutate(x => x.Fill(Color.FromRgb(0, 166, 234), paths2));

                // Draw profile picture

                var propic = Image.Load(new WebClient().DownloadData(pfpUrl));

                propic.Mutate(x => x.Resize(new ResizeOptions()
                {
                    Mode = ResizeMode.Stretch,
                    Size = new Size(120, 120)
                }));

                Image pfpRound = Extentions.RoundCorners(propic);

                image.Mutate(x => x.DrawImage(pfpRound, new Point(51, 49), 1f));

                // Save as stream

                var stream = new MemoryStream();
                image.SaveAsPng(stream);
                stream.Position = 0;


                // Try to send, if dm is set to true, in dms, else in the channel

                try
                {
                    if (Program.JSON.Dm)
                    {
                        await ctx.Message.DeleteAsync();
                        await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithFile("rank.png", stream));
                    }
                    else
                    {
                        await ctx.RespondAsync(new DiscordMessageBuilder().WithFile("rank.png", stream));
                    }
                }
                catch { }
            } else
            {
                try
                {
                    if (Program.JSON.Dm)
                    {
                        await ctx.Message.DeleteAsync();
                        await ctx.Member.SendMessageAsync("Rank not found...");
                    }
                    else
                    {
                        await ctx.RespondAsync("Rank not found...");
                    }
                }
                catch { }
            }
        }
    }
}
