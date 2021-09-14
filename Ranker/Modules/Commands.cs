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

namespace Ranker
{
    public class Commands : BaseCommandModule
    {
        // Made by @Ahmed605, @Zeealeid, @KojiOdyssey, @itsWindows11 and @SapphireDisD (GitHub)
        private readonly IDatabase _database;
        public Commands(IDatabase database)
        {
            _database = database;
        }


        [Command("rank")]
        public async Task RankCommand(CommandContext ctx, DiscordMember member)
        {
            await Rank(ctx, member.Id);
        }

        [Command("rank")]
        public async Task RankCommand(CommandContext ctx)
        {
            await Rank(ctx, ctx.Message.Author.Id);
        }

        public async Task Rank(CommandContext ctx, ulong userId)
        {
            Rank rank = await _database.GetAsync(userId, ctx.Guild.Id);

            string username = rank.Username;
            string discriminator = rank.Discriminator;
            string pfpUrl = $"{rank.Avatar}";
            ulong level = rank.Level + 1;

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

            var font1 = new Font(family1, 28f, FontStyle.Bold);
            var font2 = new Font(family1, 38f, FontStyle.Bold);
            var font3 = new Font(family2, 38f);
            var font4 = new Font(family2, 24f);

            DrawingOptions drawingOptions = new DrawingOptions()
            {
                TextOptions = new TextOptions()
                {
                    HorizontalAlignment = HorizontalAlignment.Right
                }
            };

            image.Mutate(x =>
                x.DrawText(options: drawingOptions, "Level " + level + "   Rank #" + leader, font1, Color.White, new Point(934 - 47, 193 - 55)));

            image.Mutate(x =>
                x.DrawText(username, font2, Color.White, new Point(51, 215 - 30)));

            var measure = TextMeasurer.Measure(username, new RendererOptions(font2));

            image.Mutate(x =>
                x.DrawText("#" + discriminator, font3, Color.FromRgb(153, 153, 153), new Point(60 + (int)measure.Width, 215 - 30)));

            image.Mutate(x =>
                x.DrawText(drawingOptions, gottenXp + "/" + maxXp, font4, Color.White, new Point(934 - 47, 223 - 30)));

            var paths = Extentions.GoodBuildCorners(836, 5, 4).Transform(Matrix3x2.CreateTranslation(51, 179));
            var paths2 = Extentions.GoodBuildCorners((int)(678 * (gottenXp * 100 / ((level + 1) * 100))) / 100, 5, 4).Transform(Matrix3x2.CreateTranslation(51, 179));

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

            Image pfpRound = Extentions.RoundCorners(propic);

            image.Mutate(x => x.DrawImage(pfpRound, new Point(51, 89), 1f));

            var stream = new MemoryStream();
            image.SaveAsPng(stream);
            stream.Position = 0;

            try
            {
                await ctx.Message.DeleteAsync();
                await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithFile("rank.png", stream));
            }
            catch { }
        }
    }
}
