# Using a custom database (non-SQLite) with Ranker
**IMPORTANT:** Using a custom database might cause problems with extensions.

## Set up your database
Make sure your database has the following tables that store the following information:
- Rank
  - Id (string)
  - LastCreditDate (DateTimeOffset)
  - Messages (unsigned 64-bit integer)
  - NextXp (unsigned 64-bit integer)
  - Xp (unsigned 64-bit integer)
  - TotalXp (unsigned 64-bit integer)
  - Guild (unsigned 64-bit integer)
  - User (unsigned 64-bit integer)
  - Username (string)
  - Discriminator (string)
  - Avatar (string)
  - Fleuron (boolean)
- Role
  - Id (string)
  - Level (unsigned 64-bit integer)
  - GuildId (unsigned 64-bit integer)
  - RoleId (unsigned 64-bit integer)

## Writing code for your custom database.
Create a new file in the `Ranker\Data` folder with a `.cs` extension. For example, `TestDatabase.cs`.
Open that newly created file and add the below code (replacing `TestDatabase` with the class name you want):
```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Ranker
{
    public class TestDatabase : IDatabase
    {
        public Task<List<Rank>> GetAsync() 
        { }

        public Task<Rank> GetAsync(ulong userId, ulong guildId)
        { }

        public Task<List<Role>> GetRolesAsync()
        { }

        public Task UpsertAsync(ulong userId, ulong guildId, Rank newRank)
        { }
        
        public Task UpsertAsync(ulong guildId, ulong level, ulong roleId, string roleName)
        { }
        
        public Task RemoveAsync(ulong guildId, ulong level)
        { }
    }
}
```
Then, you will need to replace the bodies of those methods with the appropriate C# code for your database.

Open up `Program.cs` and replace the following line:

`IDatabase database = new SQLiteDatabase(Path.Combine(folder, "Ranker.db"));`

... so `database` uses your newly created class instead of `SQLiteDatabase`.

Optionally, delete the `SQLiteDatabase.cs` file in the `Data` folder, but I'd recommend leaving it there just in case something goes awry.

If you ran a test and everything works fine, congratulations, you have conntected a non-SQLite database with Ranker.
