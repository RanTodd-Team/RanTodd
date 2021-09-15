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
                int newXp = new Random().Next(15, 26);
                rank.Xp += newXp;
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
