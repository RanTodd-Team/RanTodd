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
        public static DiscordClient botClient;

        public static async Task MainAsync(string[] args)
            {

            // Start and configure bot

            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = Program.JSON.Token,
            TokenType = TokenType.Bot
            });

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = Program.JSON.Prefixes
        });

            commands.RegisterCommands<Commands.Module>();

            discord.Ready += (s, e) =>
            {
                // Set client info to a variable and start website

                    System.Diagnostics.Debug.WriteLine(((DiscordClient)s).Guilds.Keys);

                    botClient = (DiscordClient)s;
                new Task(() => Program.CreateHostBuilder(args).Build().Run()).Start();
                return Task.CompletedTask;
            };

            discord.GuildCreated += async (s, e) =>
            {
                if (e.Guild.Id != Program.JSON.GuildId)
                {
                        // Leave group if in wrong server

                        await e.Guild.LeaveAsync();
                }
            };

            discord.MessageCreated += async (s, e) =>
            {
                if (e.Message.Author.IsBot) return;

                Rank rankO = Program.db.Table<Rank>().ToList().Find(x => x.Id == e.Message.Author.Id.ToString());

                long time;
                int messages;
                int levelXp;
                int level;
                int xp;
                string username = rankO?.Username;
                string discriminator = rankO?.Discriminator;
                string avatar = rankO?.Avatar;

                // Fetch database data, create if it doesn't exist

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

                int neededXp = Convert.ToInt32(5 * Math.Pow(level, 2) + (50 * level) + 100);

                // XP system

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

                Rank rank = new Rank();

                // Level up system

                if (levelXp > neededXp)
                {
                    levelXp -= neededXp;
                    level++;
                    Dictionary<string, ulong> roles = Program.JSON.Roles;
                    if(roles.ContainsKey(level.ToString()))
                    {
                        try {
                            DiscordMember member = await e.Message.Channel.Guild.GetMemberAsync(e.Message.Author.Id);

                            // Add role

                            DiscordRole role = e.Message.Channel.Guild.GetRole(roles[level.ToString()]);
                            await member.GrantRoleAsync(role);

                            // Remove old role

                            foreach (ulong roleO in Program.JSON.Roles.Values)
                            {
                                if (roleO != roles[level.ToString()])
                                {
                                    DiscordRole roleAO = e.Message.Channel.Guild.GetRole(roleO);
                                    await member.RevokeRoleAsync(roleAO);
                                }
                            }
                        } catch { } // Do nothing if we can't get the member or add the role
                    }
                }

                // Fetch user data (so if they leave the guild, we still have it avaliable)

                username = e.Message.Author.Username;
                discriminator = e.Message.Author.Discriminator;
                avatar = e.Message.Author.AvatarHash;

                // Create and set rank object

                rank.Id = e.Message.Author.Id.ToString();
                rank.TimeR = time.ToString();
                rank.LevelXp = levelXp.ToString();
                rank.Level = level.ToString();
                rank.Xp = xp.ToString();
                rank.NeededXp = neededXp.ToString();
                rank.Username = username;
                rank.Discriminator = discriminator;
                rank.Avatar = avatar;

                if (!String.IsNullOrEmpty(rankO?.Level))
                {
                    Program.db.Update(rank);
                } else
                {
                    Program.db.Insert(rank);
                }
            };

                await discord.ConnectAsync();
                await Task.Delay(-1);
            }
    }
}
