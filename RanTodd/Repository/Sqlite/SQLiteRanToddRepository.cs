using SQLite;

namespace RanTodd
{
    public class SQLiteRanToddRepository : IRanToddRepository
    {
        private SQLiteConnection db;

        public SQLiteRanToddRepository(string path)
        {
            db = new SQLiteConnection(path);
        }

        public IRanksRepository Ranks => new SQLiteRanksRepository(db);

        public IRolesRepository Roles => new SQLiteRolesRepository(db);

        public ISettingsRepository Settings => new SQLiteSettingsRepository(db);
    }
}
