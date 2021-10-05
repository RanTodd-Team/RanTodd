using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ranker
{
    public class SQLiteRankerRepository : IRankerRepository
    {
        private SQLiteConnection db;

        public SQLiteRankerRepository(string path)
        {
            db = new SQLiteConnection(path);
        }

        public IRanksRepository Ranks => new SQLiteRanksRepository(db);

        public IRolesRepository Roles => new SQLiteRolesRepository(db);
    }
}
