using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ranker
{
    public class Events : BaseExtension
    {
        private readonly IDatabase _database;

        public Events(IDatabase database)
        {
            _database = database;
        }

        protected override void Setup(DiscordClient client)
        {
            client.Ready += Client_Ready;
            client.GuildAvailable += Client_GuildAvaliable;
            client.GuildMemberAdded += Client_GuildMemberAdded;
            client.GuildMemberUpdated += Client_GuildMemberUpdated;
            client.GuildRoleUpdated += Client_GuildRoleUpdated;
            client.MessageCreated += Client_MessageCreated;
        }

        private Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            sender.Logger.LogInformation("Bot is ready!");
            return Task.CompletedTask;
        }

        private async Task Client_GuildAvaliable(DiscordClient sender, DSharpPlus.EventArgs.GuildCreateEventArgs e)
        {
            List<Role> roles = await _database.GetRolesAsync(e.Guild.Id);
            roles.ForEach(async role =>
            {
                await _database.UpsertAsync(e.Guild.Id, role.Level, role.RoleId, e.Guild.GetRole(role.RoleId).Name);
            });
        }

    private async Task Client_GuildMemberUpdated(DiscordClient sender, DSharpPlus.EventArgs.GuildMemberUpdateEventArgs e)
        {
            if (e.Member.IsBot) return;

            Rank rank = await _database.GetAsync(e.Member.Id, e.Guild.Id);
            rank.Avatar = e.Member.GuildAvatarUrl;
            rank.Discriminator = e.Member.Discriminator;
            rank.Username = e.Member.Username;
            await _database.UpsertAsync(e.Member.Id, e.Guild.Id, rank);
        }

        private async Task Client_GuildMemberAdded(DiscordClient sender, DSharpPlus.EventArgs.GuildMemberAddEventArgs e)
        {
            if (e.Member.IsBot) return;

            Rank rank = new()
            {
                Avatar = e.Member.AvatarUrl,
                Username = e.Member.Username,
                Discriminator = e.Member.Discriminator,
                LastCreditDate = DateTimeOffset.UnixEpoch
            };
            await _database.UpsertAsync(e.Member.Id, e.Guild.Id, rank);
        }

        private async Task Client_Guild​Role​Update​d(DiscordClient sender, DSharpPlus.EventArgs.Guild​Role​Update​Event​Args e)
        {
            List<Role> roles = await _database.GetRolesAsync(e.Guild.Id);
            Role role = roles.Find(x => x.RoleId == e.RoleAfter.Id);
            if (role != null)
            {
                await _database.UpsertAsync(e.Guild.Id, role.Level, role.RoleId, e.RoleAfter.Name);
            }
        }

        private async Task Client_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            if (e.Author.IsBot) return;

            Rank rank = await _database.GetAsync(e.Author.Id, e.Guild.Id);
            rank.Avatar = e.Author.AvatarUrl;
            rank.Username = e.Author.Username;
            rank.Discriminator = e.Author.Discriminator;
            rank.Messages += 1;
            
            if (e.Message.CreationTimestamp >= rank.LastCreditDate.AddMinutes(1))
            {
                ulong newXp = Convert.ToUInt64(new Random().Next(15, 26));
                rank.Xp += newXp;
                rank.TotalXp += newXp;
                rank.LastCreditDate = e.Message.CreationTimestamp;
                if (rank.Xp >= rank.NextXp)
                {
                    rank.Level += 1;
                    rank.Xp -= rank.NextXp;
                    rank.NextXp = Convert.ToUInt64(5 * Math.Pow(rank.Level, 2) + (50 * rank.Level) + 100);
                    try
                    {
                        List<Role> roles = await _database.GetRolesAsync(e.Guild.Id);
                        int currentRoleIndex = roles.FindIndex(x => x.Level == rank.Level);
                        ulong roleId = roles[currentRoleIndex]?.RoleId ?? 0;
                        DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);
                        DiscordRole newRole = e.Guild.GetRole(roleId);
                        DiscordRole oldRole = e.Guild.GetRole(roles[currentRoleIndex - 1]?.RoleId ?? 0);
                        await member.GrantRoleAsync(newRole);
                        await member.RevokeRoleAsync(oldRole);
                    } catch { }
                }
            }

            await _database.UpsertAsync(e.Author.Id, e.Guild.Id, rank);
        }
    }
}
