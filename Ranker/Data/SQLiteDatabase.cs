using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ranker
{
    public class SQLiteDatabase : IDatabase
    {
        private SQLiteConnection db;

        [Table("RankData")]
        public class SQLiteData
        {
            public SQLiteData()
            { }

            public SQLiteData(Rank rank)
            {
                LastCreditDate = rank.LastCreditDate;
                Messasges = rank.Messasges.ToString();
                NextXp = rank.NextXp.ToString();
                Level = rank.Level.ToString();
                Xp = rank.Xp.ToString();
                User = rank.User.ToString();
                Guild = rank.Guild.ToString();
                Username = rank.Username;
                Discriminator = rank.Discriminator;
                Avatar = rank.Avatar;
            }

            [PrimaryKey]
            public string Id { get => $"{Guild}/{User}" ; set { } }

            public DateTimeOffset LastCreditDate { get; set; }

            public string Messasges { get; set; }

            public string NextXp { get; set; }

            public string Level { get; set; } = "100";

            public string Xp { get; set; }

            public string User { get; set; }

            public string Guild { get; set; }

            public string Username { get; set; }

            public string Discriminator { get; set; }

            public string Avatar { get; set; }

            public Rank ToRank()
            {
                return new Rank()
                {
                    LastCreditDate = LastCreditDate,
                    Messasges = ulong.Parse(Messasges),
                    NextXp = ulong.Parse(NextXp),
                    Level = ulong.Parse(Level),
                    Xp = ulong.Parse(Xp),
                    Username = Username,
                    Discriminator = Discriminator,
                    Avatar = Avatar,
                    User = ulong.Parse(User),
                    Guild = ulong.Parse(Guild)
                };
            }
        }

        public SQLiteDatabase(string path)
        {
            db = new SQLiteConnection(path);
            db.CreateTable<SQLiteData>();
        }

        public Task<List<Rank>> GetAsync()
        {
            return Task.Run(() => db.Table<SQLiteData>().ToList().Select(f => f.ToRank()).ToList());
        }

        public Task<Rank> GetAsync(ulong userId, ulong guildId)
        {
            Rank rank = new();
            var list = db.Table<SQLiteData>().ToList();
            rank = list.Find(f => f.Id == $"{guildId}/{userId}")?.ToRank() ?? new()
            {
                Guild = guildId,
                User = userId
            };
            return Task.Run(() => rank);
        }

        public Task UpsertAsync(ulong userId, ulong guildId, Rank newRank)
        {
            return Task.Run(() =>
            {
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
    }
}
