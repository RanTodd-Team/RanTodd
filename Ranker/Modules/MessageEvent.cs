using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ranker
{
    public class MessageEvent : BaseExtension
    {
        private readonly IDatabase _database;

        public MessageEvent(IDatabase database)
        {
            _database = database;
        }

        protected override void Setup(DiscordClient client)
        {
            client.GuildMemberAdded += Client_GuildMemberAdded;
            client.GuildMemberUpdated += Client_GuildMemberUpdated;
            client.MessageCreated += Client_MessageCreated;
        }

        private async Task Client_GuildMemberUpdated(DiscordClient sender, DSharpPlus.EventArgs.GuildMemberUpdateEventArgs e)
        {
            Rank rank = await _database.GetAsync(e.Member.Id, e.Guild.Id);
            rank.Avatar = e.Member.GuildAvatarUrl;
            rank.Discriminator = e.Member.Discriminator;
            rank.Username = e.Member.Username;
            await _database.UpsertAsync(e.Member.Id, e.Guild.Id, rank);
        }

        private async Task Client_GuildMemberAdded(DiscordClient sender, DSharpPlus.EventArgs.GuildMemberAddEventArgs e)
        {
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
                    rank.Xp -= rank.NextXp;
                    rank.NextXp = Convert.ToUInt64(5 * Math.Pow(rank.Level, 2) + (50 * rank.Level) + 100);
                    rank.Level += 1;
                    try
                    {
                        List<Role> roles = await _database.GetRolesAsync();
                        int currentRoleIndex = roles.FindIndex(x => x.Level == rank.Level);
                        ulong roleId = roles[currentRoleIndex]?.Id ?? 0;
                        DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);
                        DiscordRole newRole = e.Guild.GetRole(roleId);
                        DiscordRole oldRole = e.Guild.GetRole(roles[currentRoleIndex - 1]?.Id ?? 0);
                        await member.GrantRoleAsync(newRole);
                        await member.RevokeRoleAsync(oldRole);
                    } catch { }
                }
            }

            await _database.UpsertAsync(e.Author.Id, e.Guild.Id, rank);
        }
    }
}
