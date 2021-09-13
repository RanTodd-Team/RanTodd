using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.Extensions.Hosting;

namespace Ranker
{
    public class Bot
    {
        public static DiscordUser botUser;

        public static async Task MainAsync(string[] args)
            {
        var discord = new DiscordClient(new DiscordConfiguration()
                {
                    Token = Program.JSON["Token"][0],
                    TokenType = TokenType.Bot
                });

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = Program.JSON["Prefixes"].ToArray()
            });

            commands.RegisterCommands<Commands.Module>();

            discord.Ready += async (s, e) =>
            {
                botUser = ((DiscordClient)s).CurrentUser;
                new Task(() => Program.CreateHostBuilder(args).Build().Run()).Start();
            };

            discord.MessageCreated += async (s, e) =>
            {
                if (e.Message.Author.IsBot) return;

                Rank rankO = Program.db.Table<Rank>().ToList().Find(x => x.UserId == e.Message.Author.Id.ToString() && x.GuildId == e.Message.Channel.GuildId.ToString());

                long time;
                int messages;
                int levelXp;
                int level;
                int xp;
                string username = rankO?.Username;
                string discriminator = rankO?.Discriminator;
                string avatar = rankO?.Avatar;

                string timeR = rankO?.TimeR;
                bool success1 = long.TryParse(timeR, out time);
                if (!success1)
                {
                    time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                }

                string messagesI = rankO?.Messasges;
                bool success2 = int.TryParse(messagesI, out messages);
                if (!success2)
                {
                    messages = 0;
                }

                string levelXpI = rankO?.LevelXp;
                bool success3 = int.TryParse(levelXpI, out levelXp);
                if (!success3)
                {
                    levelXp = 0;
                }

                string levelI = rankO?.Level;
                bool success4 = int.TryParse(levelI, out level);
                if (!success4)
                {
                    level = 0;
                }
                string xpI = rankO?.Xp;
                bool success5 = int.TryParse(xpI, out xp);
                if (!success5)
                {
                    xp = 0;
                }

                if (levelXp == 0)
                {
                    int randomThingy = new Random().Next(15, 25);
                    levelXp += randomThingy;
                    xp += randomThingy;
                }

                if (time + 59999 < (DateTimeOffset.Now.ToUnixTimeMilliseconds()))
                {
                    time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    levelXp += new Random().Next(15, 25);
                    messages += 1;
                }

                int needed = (level + 1) * 100;

                if (levelXp > needed)
                {
                    levelXp -= needed;
                    level++;
                }

                messages += 1;

                username = e.Message.Author.Username;
                discriminator = e.Message.Author.Discriminator;
                avatar = e.Message.Author.AvatarHash;

                Rank rank = new Rank();
                rank.UserId = e.Message.Author.Id.ToString();
                rank.GuildId = e.Message.Channel.GuildId.ToString();
                rank.TimeR = time.ToString();
                rank.LevelXp = levelXp.ToString();
                rank.Level = level.ToString();
                rank.Xp = xp.ToString();
                rank.Username = username;
                rank.Discriminator = discriminator;
                rank.Avatar = avatar;
                var check = rankO;
                if (!String.IsNullOrEmpty(check?.Level))
                {
                    rank.Id = check.Id;
                    Program.db.Update(rank);
                }
                else
                {
                    rank.Id = Guid.NewGuid().ToString();
                    Program.db.Insert(rank);
                }
            };

                await discord.ConnectAsync();
                await Task.Delay(-1);
            }
    }
}
