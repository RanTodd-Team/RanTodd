namespace RanTodd
{
    public interface IRolesRepository
    {
        /// <summary>
        /// Gets all roles.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        Task<List<Role>> GetAsync(ulong guildId);

        /// <summary>
        /// Modify an existing role or add a new role.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        /// <param name="level">The role level.</param>
        /// <param name="roleId">The role id.</param>
        /// <param name="roleName">The role name.</param>
        Task UpsertAsync(ulong guildId, ulong level, ulong roleId, string roleName);

        /// <summary>
        /// Remove an existing role.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        /// <param name="level">The role level.</param>
        Task RemoveAsync(ulong guildId, ulong level);

        /// <summary>
        /// Empties roles.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        Task Empty(ulong guildId);
    }
}
