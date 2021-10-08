using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ranker
{
    public class SQLiteSettingsRepository : ISettingsRepository
    {
        private SQLiteConnection db;

        #region Data
        [Table("Settings")]
        private class SQLiteData
        {
            public SQLiteData()
            { }

            public SQLiteData(Settings settings)
            {
                Guild = settings.Guild.ToString();
                MinRange = settings.MinRange.ToString();
                MaxRange = settings.MaxRange.ToString();
                Banner = settings.Banner;
                ExcludedChannels = JsonConvert.SerializeObject(settings.ExcludedChannels);
            }

            [PrimaryKey]
            public string Guild { get; set; }

            public string MinRange { get; set; }

            public string MaxRange { get; set; }

            public string Banner { get; set; }

            public string ExcludedChannels { get; set; }

            public Settings ToSettings()
            {
                return new Settings()
                {
                    Guild = ulong.Parse(Guild),
                    MinRange = int.Parse(MinRange),
                    MaxRange = int.Parse(MaxRange),
                    Banner = Banner,
                    ExcludedChannels = JsonConvert.DeserializeObject<List<ulong>>(ExcludedChannels)
                };
            }
        }
        #endregion

        public SQLiteSettingsRepository(SQLiteConnection db)
        {
            this.db = db;
            db.CreateTable<SQLiteData>();
        }

        public Task<Settings> GetAsync(ulong guildId)
        {
            return Task.Run(() =>
            {
                var rank = db.Table<SQLiteData>().ToList().Find(f => f.Guild == guildId.ToString())?.ToSettings() ?? new() {
                    Guild = guildId,
                    MinRange = 15,
                    MaxRange = 26,
                    Banner = null,
                    ExcludedChannels = new List<ulong>()
                };
                return rank;
            });
        }

        public Task UpsertAsync(ulong guildId, Settings newSettings)
        {
            return Task.Run(() =>
            {
                var list = db.Table<SQLiteData>().ToList();
                if (list.Any(f => f.Guild == guildId.ToString()))
                {
                    db.Update(new SQLiteData(newSettings));
                }
                else
                {
                    db.Insert(new SQLiteData(newSettings));
                }
            });
        }
    }
}
