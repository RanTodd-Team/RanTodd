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
using System.Text.RegularExpressions;
using System.Net.Http;

namespace RanTodd
{
    public class Commands
    {
        // Designed by Zeealeid
        public async static Task<MemoryStream> RankZeealeid(IRanToddRepository _database, DiscordGuild guild, DiscordUser user, Rank rank)
        {
            string prePreUsername = Regex.Replace((rank.Username ?? user.Username), @"[^\u0000-\u007F]+", "?");
            string preUsername = String.Join("", prePreUsername.Take(13).ToArray());
            string username = preUsername + (preUsername != prePreUsername ? "..." : "");
            string discriminator = rank.Discriminator ?? user.Discriminator;
            string pfpUrl = rank.Avatar ?? user.AvatarUrl;
            ulong level = rank.Level;

            ulong gottenXp = rank.Xp;
            ulong maxXp = rank.NextXp;

            var list = (await _database.Ranks.GetAsync(guild.Id)).OrderByDescending(f => f.Xp).ToList();

            int leader = list.IndexOf(list.FirstOrDefault(f => f.User == user.Id)) + 1;

            using (Image<Rgba32> image = new Image<Rgba32>(934, 282))
            {
                var img = Image.Load("./Images/Background.png");
                image.Mutate(x => x.DrawImage(img, new Point(0, 0), 1));

                var rect = new Rectangle(0, 0, 10, 382);
                image.Mutate(x => x.Fill(Color.FromRgb(0, 166, 234), rect));

                FontCollection fonts = new FontCollection();
                var family1 = fonts.Install("./Fonts/Selawik/selawkb.ttf");
                var family2 = fonts.Install("./Fonts/Selawik/selawk.ttf");

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
                    x.DrawText(drawingOptions, gottenXp + " / " + maxXp + " XP", font3, Color.White, new Point(934 - 47, 223 - 30)));

                var paths = Extentions.GoodBuildCorners(836, 5, 4).Transform(Matrix3x2.CreateTranslation(51, 179));
                var paths2 = Extentions.GoodBuildCorners((int)(678 * (gottenXp * 100 / maxXp)) / 100, 5, 4).Transform(Matrix3x2.CreateTranslation(51, 179));

                image.Mutate(x => x.Fill(Color.White, paths));
                image.Mutate(x => x.Fill(Color.FromRgb(0, 166, 234), paths2));

                using (HttpClient httpClient = new HttpClient())
                {
                    var propic = Image.Load(await (await httpClient.GetAsync("https://cdn.discordapp.com/embed/avatars/1.png")).Content.ReadAsByteArrayAsync());
                    try
                    {
                        propic = Image.Load(await (await httpClient.GetAsync(pfpUrl)).Content.ReadAsByteArrayAsync());
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

                    return stream;
                }
            }
        }

        // Designed by Fleuron
        public async static Task<MemoryStream> RankFleuron(IRanToddRepository _database, DiscordGuild guild, DiscordUser user, Rank rank)
        {
            string prePreUsername = Regex.Replace((rank.Username ?? user.Username), @"[^\u0000-\u007F]+", "?");
            string preUsername = String.Join("", prePreUsername.Take(13).ToArray());
            string username = preUsername + (preUsername != prePreUsername ? "..." : "");
            string discriminator = rank.Discriminator ?? user.Discriminator;
            string pfpUrl = rank.Avatar ?? user.AvatarUrl;
            ulong level = rank.Level;

            ulong gottenXp = rank.Xp;
            ulong maxXp = rank.NextXp;

            var list = (await _database.Ranks.GetAsync(guild.Id)).OrderByDescending(f => f.Xp).ToList();

            int leader = list.IndexOf(list.FirstOrDefault(f => f.User == user.Id)) + 1;

            using (Image<Rgba32> image = new Image<Rgba32>(934, 282))
            {
                var background = new Rectangle(0, 0, 934, 382);
                image.Mutate(x => x.Fill(Color.LightGray, background));

                int progressPercentage = (int)((double)rank.Xp / rank.NextXp * 934);

                var progressBarBackground = new Rectangle(0, 0, progressPercentage, 282);
                image.Mutate(x => x.Fill(Color.FromRgb(50, 169, 229), progressBarBackground));

                using (Image frontCard = new Image<Rgba32>(934, 242))
                {
                    var frontCardBackground = new Rectangle(0, 0, 934, 242);
                    frontCard.Mutate(x => x.Fill(Color.Black, frontCardBackground));

                    image.Mutate(x => x.DrawImage(Extentions.RoundBottom(frontCard, 30), 1f));

                    FontCollection fonts = new FontCollection();
                    var metropolis = fonts.Install("./Fonts/Metropolis/Metropolis-Regular.ttf");
                    var metropolisBold = fonts.Install("./Fonts/Metropolis/Metropolis-Bold.ttf");
                    var epilogue = fonts.Install("./Fonts/Epilogue/static/Epilogue-Regular.ttf");

                    var font1 = new Font(metropolis, 54f);

                    var font2 = new Font(metropolis, 38f);

                    var font3 = new Font(metropolisBold, 38f, FontStyle.Bold);

                    var font4 = new Font(epilogue, 38f);

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

                    image.Mutate(x => x.DrawText("Rank " + leader, font3, Color.White, new Point(widthUsernameContainer + 32, 30)));

                    image.Mutate(x => x.DrawText("Level " + level, font3, Color.White, new Point(18, 190)));

                    var measure3 = TextMeasurer.Measure(gottenXp.ToString(), new RendererOptions(font4));

                    var measure4 = TextMeasurer.Measure("|", new RendererOptions(font4));

                    var measure5 = TextMeasurer.Measure(maxXp.ToString(), new RendererOptions(font4));

                    image.Mutate(x => x.DrawText(gottenXp.ToString(), font4, Color.White, new Point(934 - (int)measure3.Width - (int)measure4.Width - (int)measure5.Width - 18, 190)));

                    image.Mutate(x => x.DrawText("|", font4, Color.FromRgb(50, 169, 229), new Point(934 - (int)measure5.Width - 28, 190)));

                    image.Mutate(x => x.DrawText(maxXp.ToString(), font4, Color.White, new Point(934 - (int)measure5.Width - 18, 190)));

                    using (HttpClient httpClient = new HttpClient())
                    {
                        var propic = Image.Load(await (await httpClient.GetAsync("https://cdn.discordapp.com/embed/avatars/1.png")).Content.ReadAsByteArrayAsync());
                        try
                        {
                            propic = Image.Load(await (await httpClient.GetAsync(pfpUrl)).Content.ReadAsByteArrayAsync());
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

                        return stream;
                    }
                }
            }
        }
    }
}
