using DSharpPlus.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ranker
{
    public class SQLiteRanksRepository : IRanksRepository
    {
        private readonly IRankerRepository _database;

        public SQLiteRanksRepository(IRankerRepository database)
        {
            _database = database;
        }

        private SQLiteConnection db;

        #region Data
        [Table("RankData")]
        private class SQLiteData
        {
            public SQLiteData()
            { }

            public SQLiteData(Rank rank)
            {
                LastCreditDate = rank.LastCreditDate;
                Messages = rank.Messages.ToString();
                NextXp = rank.NextXp.ToString();
                Level = rank.Level.ToString();
                Xp = rank.Xp.ToString();
                User = rank.User.ToString();
                Guild = rank.Guild.ToString();
                TotalXp = rank.TotalXp.ToString();
                Username = rank.Username;
                Discriminator = rank.Discriminator;
                Avatar = rank.Avatar;
                Fleuron = rank.Fleuron.ToString();
            }

            [PrimaryKey]
            public string Id { get => $"{Guild}/{User}"; set { } }

            public DateTimeOffset LastCreditDate { get; set; }

            public string Messages { get; set; }

            public string NextXp { get; set; }

            public string Level { get; set; } = "100";

            public string Xp { get; set; }

            public string User { get; set; }

            public string Guild { get; set; }

            public string Username { get; set; }

            public string Discriminator { get; set; }

            public string Avatar { get; set; }

            public string TotalXp { get; set; }

            public string Fleuron { get; set; }

            public Rank ToRank()
            {
                return new Rank()
                {
                    LastCreditDate = LastCreditDate,
                    Messages = ulong.Parse(Messages),
                    NextXp = ulong.Parse(NextXp),
                    Level = ulong.Parse(Level),
                    Xp = ulong.Parse(Xp),
                    Username = Username,
                    Discriminator = Discriminator,
                    Avatar = Avatar,
                    User = ulong.Parse(User),
                    Guild = ulong.Parse(Guild),
                    TotalXp = ulong.Parse(TotalXp),
                    Fleuron = bool.Parse(Fleuron)
                };
            }
        }
        #endregion

        public SQLiteRanksRepository(SQLiteConnection db)
        {
            this.db = db;
            db.CreateTable<SQLiteData>();
        }

        public Task<List<Rank>> GetAsync(ulong guildId)
        {
            return Task.Run(() => db.Table<SQLiteData>().ToList().FindAll(f => f.Guild == guildId.ToString()).Select(f => f.ToRank()).ToList());
        }

        public Task<Rank> GetAsync(ulong userId, ulong guildId)
        {
            return Task.Run(() =>
            {
                Rank rank;
                var list = db.Table<SQLiteData>().ToList();
                rank = list.Find(f => f.Id == $"{guildId}/{userId}")?.ToRank() ?? new()
                {
                    Guild = guildId,
                    User = userId
                };
                return rank;
            });
        }

        public Task UpsertAsync(ulong userId, ulong guildId, Rank newRank, DiscordGuild? guild)
        {
            return Task.Run(async () =>
            {
                if (newRank.Xp >= newRank.NextXp)
                {
                    newRank.Level += 1;
                    newRank.Xp -= newRank.NextXp;
                    while (newRank.Xp >= newRank.NextXp)
                    {
                        newRank.Level += 1;
                        newRank.Xp -= newRank.NextXp;
                    }
                    newRank.NextXp = Convert.ToUInt64(5 * Math.Pow(newRank.Level, 2) + (50 * (float)newRank.Level) + 100);
                }

                if (guild != null)
                {
                    try
                    {
                        List<Role> roles = await _database.Roles.GetAsync(guild.Id);
                        var filteredRoles = roles.Where(x => x.Level <= newRank.Level).OrderByDescending(x => x.Level);
                        if (filteredRoles.Count != 0)
                        {
                            var selectedRole = filteredRoles[0];
                            ulong roleId = selectedRole.RoleId;
                            DiscordMember member = await guild.GetMemberAsync(userId);
                            if (!member.Roles.Any(f => f.Id == roleId))
                            {
                                DiscordRole newRole = guild.GetRole(roleId);
                                DiscordRole oldRole = guild.GetRole(roles[currentRoleIndex - 1]?.RoleId ?? 0);
                                await member.GrantRoleAsync(newRole);
                                await member.RevokeRoleAsync(oldRole);
                            }
                        }
                    }
                    catch { }
                }

                var list = db.Table<SQLiteData>().ToList();
                if (list.Any(f => f.Id == $"{guildId}/{userId}"))
                {
                    db.Update(new SQLiteData(newRank));
                }
                else
                {
                    db.Insert(new SQLiteData(newRank));
                }
            });
        }

        public Task Empty(ulong guildId)
        {
            return Task.Run(() =>
            {
                var list = db.Table<SQLiteData>().ToList();
                foreach(SQLiteData rank in list.FindAll(x => x.Guild == guildId.ToString()))
                {
                    db.Delete<SQLiteData>(rank.Id);
                }
            });
        }
    }
}
