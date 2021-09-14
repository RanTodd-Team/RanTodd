using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ranker
{
    public interface IDatabase
    {
        /// <summary>
        /// Gets ranks for all members.
        /// </summary>
        Task<List<Rank>> GetAsync();

        /// <summary>
        /// Gets rank for a particular member.
        /// </summary>
        /// <param name="userId">The member ID.</param>
        /// <param name="guildId">The guild (server) ID.</param>
        Task<Rank> GetAsync(ulong userId, ulong guildId);

        /// <summary>
        /// Modify an existing rank or add a new rank.
        /// </summary>
        /// <param name="newRank">The new rank.</param>
        Task UpsertAsync(ulong userId, ulong guildId, Rank newRank);
    }
}
