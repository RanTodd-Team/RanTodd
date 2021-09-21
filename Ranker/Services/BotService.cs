using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ranker
{
    public class BotService : IHostedService
    {
        DiscordClient client;

        private readonly IDatabase _database;
        private readonly ConfigJson _configJson;

        public BotService(IDatabase database, ConfigJson configJson)
        {
            _database = database;
            _configJson = configJson;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            DiscordConfiguration configuration = new()
            {
                // We may drop JSON token.
                Token = Environment.GetEnvironmentVariable("RANKER_TOKEN") ?? _configJson.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.GuildMembers | DiscordIntents.GuildMessages | DiscordIntents.Guilds
            };

            client = new(configuration);

            client.AddExtension(new MessageEvent(_database));

            var servCollection = new ServiceCollection();
            servCollection.AddSingleton(_database);

            var slashCommands = client.UseSlashCommands(new()
            {
                Services = servCollection.BuildServiceProvider()
            });

            slashCommands.RegisterCommands<Commands>(_configJson.GuildId);

            await client.ConnectAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                client?.Dispose();
                client = null;
            });
        }
    }
}
