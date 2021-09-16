﻿using System;
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
        // Made by @Ahmed605, @Zeealeid, @KojiOdyssey, @itsWindows11 and @SapphireDisD (GitHub)
        private readonly IDatabase _database;
        public Commands(IDatabase database)
        {
            _database = database;
        }


        [SlashCommand("rank", "Check your rank or a user's")]
        public async Task RankCommand(InteractionContext ctx, [Option("user", "User to view ranks for")] DiscordUser user = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (user == null)
                user = ctx.User;
            await Rank(ctx, user.Id);
        }

        public async Task Rank(InteractionContext ctx, ulong userId)
        {
            Rank rank = await _database.GetAsync(userId, ctx.Guild.Id);

            string username = rank.Username ?? ctx.User.Username;
            string discriminator = rank.Discriminator ?? ctx.User.Discriminator;
            string pfpUrl = $"{rank.Avatar ?? ctx.User.AvatarUrl}";
            string username = rank.Username;
            string discriminator = rank.Discriminator;
            string pfpUrl = rank.Avatar;
            ulong level = rank.Level;

            ulong gottenXp = rank.Xp;
            ulong maxXp = rank.NextXp;

            var list = (await _database.GetAsync()).OrderByDescending(f => f.Xp).ToList();

            int leader = list.IndexOf(list.FirstOrDefault(f => f.User == userId)) + 1;

            Image<Rgba32> image = new Image<Rgba32>(934, 282);
            /*var img = Image.Load("./Images/Background.png");
            image.Mutate(x => x.DrawImage(img, new Point(0, 0), 1));*/

            var rect = new Rectangle(0, 0, 10, 382);
            image.Mutate(x => x.Fill(Color.FromRgb(0, 166, 234), rect));
            var background = new Rectangle();
            image.Mutate(x => x.Fill(Color.Black, background));

            FontCollection fonts = new FontCollection();
            var metropolis = fonts.Install("./Fonts/metropolis/Metropolis-Regular.ttf");
            var epilogue = fonts.Install("./Fonts/Epilogue/static/Epilogue-Regular.ttf");

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

            Image pfpRound = Extentions.RoundCorners(propic);

            image.Mutate(x => x.DrawImage(pfpRound, new Point(18, 18), 1f));

            var stream = new MemoryStream();
            image.SaveAsPng(stream);
            stream.Position = 0;

            try
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddFile("rank.png", stream));
            }
            catch { }
        }
    }
}
