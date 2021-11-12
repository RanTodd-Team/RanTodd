namespace RanTodd
{
    public interface ISettingsRepository
    {
        /// <summary>
        /// Gets settings for a specific guild.
        /// </summary>
        Task<Settings> GetAsync(ulong guildId);

        /// <summary>
        /// Modify existing settings or add new settings.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        /// <param name="newRank">The new rank.</param>
        Task UpsertAsync(ulong guildId, Settings newSettings);
    }
}
