using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ranker
{
    public class MessageEvent : BaseExtension
    {
        private readonly IDatabase _database;
        private readonly ConfigJson _config;

        public MessageEvent(IDatabase database, ConfigJson config)
        {
            _database = database;
            _config = config;
        }

        protected override void Setup(DiscordClient client)
        {
            client.GuildMemberAdded += Client_GuildMemberAdded;
            client.GuildMemberUpdated += Client_GuildMemberUpdated;
            client.MessageCreated += Client_MessageCreated;
            client.GuildCreated += Client_GuildCreated;
        }

        private async Task Client_GuildCreated(DiscordClient sender, DSharpPlus.EventArgs.GuildCreateEventArgs e)
        {
            // Checks if:
            // 1. There is a server ID specified.
            // 2. If the specified server ID is valid.
            // 3. The joined server is the same one we want to.
            if (_config.GuildId != null && _config.GuildId >= 4194304 && _config.GuildId != e.Guild.Id)
            {
                // We entered into a wrong server, leave it.
                await e.Guild.LeaveAsync();
                e.Handled = true;
            }

            // Otherwise add non-bot members
            await _database.AddNonExistentMembers(e.Guild.Members.Where(f => !f.Value.IsBot).Select(f => f.Value));
        }

        private async Task Client_GuildMemberUpdated(DiscordClient sender, DSharpPlus.EventArgs.GuildMemberUpdateEventArgs e)
        {
            if (e.Member.IsBot)
                return;

            Rank rank = await _database.GetAsync(e.Member.Id, e.Guild.Id);
            rank.Avatar = e.Member.GuildAvatarUrl;
            rank.Discriminator = e.Member.Discriminator;
            rank.Username = e.Member.Username;
            await _database.UpsertAsync(e.Member.Id, e.Guild.Id, rank);
        }

        private async Task Client_GuildMemberAdded(DiscordClient sender, DSharpPlus.EventArgs.GuildMemberAddEventArgs e)
        {
            if (e.Member.IsBot)
                return;

            Rank rank = new()
            {
                Avatar = e.Member.AvatarUrl,
                Username = e.Member.Username,
                Discriminator = e.Member.Discriminator,
                LastCreditDate = DateTimeOffset.UnixEpoch
            };
            await _database.UpsertAsync(e.Member.Id, e.Guild.Id, rank);
        }

        private async Task Client_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            if (_config.IgnoredChannelIds != null && _config.IgnoredChannelIds.Contains(e.Channel.Id))
            {
                // Let's just ignore this channel.
                return;
            }

            if (e.Author.IsBot)
            {
                // Author is bot, ignore it.
                return;
            }

            Rank rank = await _database.GetAsync(e.Author.Id, e.Guild.Id);
            rank.Avatar = e.Author.AvatarUrl;
            rank.Username = e.Author.Username;
            rank.Discriminator = e.Author.Discriminator;
            rank.Messasges += 1;
            
            if (e.Message.CreationTimestamp >= rank.LastCreditDate.AddMinutes(1))
            {
                ulong newXp = Convert.ToUInt64(new Random().Next(15, 26));
                rank.Xp += newXp;
                rank.TotalXp += newXp;
                rank.LastCreditDate = e.Message.CreationTimestamp;
                if (rank.Xp >= rank.NextXp)
                {
                    rank.Level += 1;
                    rank.Xp = 0;
                    rank.NextXp = Convert.ToUInt64(5 * Math.Pow(rank.Level, 2) + (50 * rank.Level) + 100);
                }
            }

            await _database.UpsertAsync(e.Author.Id, e.Guild.Id, rank);
        }
    }
}
