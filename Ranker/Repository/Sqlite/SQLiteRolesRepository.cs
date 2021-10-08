using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ranker
{
    public class SQLiteRolesRepository : IRolesRepository
    {
        private SQLiteConnection db;

        #region Class
        [Table("RolesData")]
        private class SQLiteData
        {
            public SQLiteData()
            { }

            public SQLiteData(Role role)
            {
                Guild = role.Guild.ToString();
                Level = role.Level.ToString();
                RoleId = role.RoleId.ToString();
                RoleName = role.RoleName;
            }

            [PrimaryKey]
            public string Id { get => $"{Guild}/{Level}"; set { } }

            public string Guild { get; set; }

            public string Level { get; set; }

            public string RoleId { get; set; }

            public string RoleName { get; set; }

            public Role ToRole()
            {
                return new Role()
                {
                    Guild = ulong.Parse(Guild),
                    Level = ulong.Parse(Level),
                    RoleId = ulong.Parse(RoleId),
                    RoleName = RoleName
                };
            }
        }
        #endregion Class

        public SQLiteRolesRepository(SQLiteConnection db)
        {
            this.db = db;
            db.CreateTable<SQLiteData>();
        }

        public Task<List<Role>> GetAsync(ulong guildId)
        {
            return Task.Run(() => db.Table<SQLiteData>().ToList().FindAll(x => x.Guild == guildId.ToString()).Select(f => f.ToRole()).ToList());
        }

        public Task RemoveAsync(ulong guildId, ulong level)
        {
            return Task.Run(() =>
            {
                db.Delete<SQLiteData>(guildId + "/" + level);
            });
        }

        public Task UpsertAsync(ulong guildId, ulong level, ulong roleId, string roleName)
        {
            return Task.Run(() =>
            {
                Role newRole = new();
                newRole.Guild = guildId;
                newRole.Level = level;
                newRole.RoleId = roleId;
                newRole.RoleName = roleName;
                var list = db.Table<SQLiteData>().ToList().FindAll(x => x.Guild == guildId.ToString());
                if (list.Any(f => ulong.Parse(f.Level) == level))
                {
                    db.Update(new SQLiteData(newRole));
                }
                else
                {
                    db.Insert(new SQLiteData(newRole));
                }
            });
        }

        public Task Empty(ulong guildId)
        {
            return Task.Run(() =>
            {
                var list = db.Table<SQLiteData>().ToList();
                foreach (SQLiteData role in list.FindAll(x => x.Guild == guildId.ToString()))
                {
                    db.Delete<SQLiteData>(role.Id);
                }
            });
        }
    }
}
