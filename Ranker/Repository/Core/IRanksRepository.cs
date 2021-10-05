using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ranker
{
    public interface IRanksRepository
    {
        /// <summary>
        /// Gets ranks for a specific guild.
        /// </summary>
        Task<List<Rank>> GetAsync(ulong guildId);

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
