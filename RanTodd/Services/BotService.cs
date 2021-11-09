using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RanTodd
{
    public class BotService : IHostedService
    {
        DiscordClient client;
        IRanToddRepository database;
        ConfigJson configJson;

        public BotService(ConfigJson config, IRanToddRepository database)
        {
            configJson = config;
            this.database = database;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            DiscordConfiguration configuration = new()
            {
                Token = Environment.GetEnvironmentVariable("RANTODD_TOKEN"),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.GuildMembers | DiscordIntents.GuildMessages | DiscordIntents.Guilds
            };

            client = new(configuration);

            client.AddExtension(new Events(database));

            ServiceCollection servCollection = new();
            servCollection.AddSingleton(database);

            var slashCommands = client.UseSlashCommands(new()
            {
                Services = servCollection.BuildServiceProvider()
            });

            slashCommands.SlashCommandErrored += async (s, e) =>
            {
                s.Client.Logger.LogError(e.Exception.ToString());
                
                await e.Context.DeleteResponseAsync();

                await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, 
                    new DiscordInteractionResponseBuilder()
                        .WithContent("Something went wrong! Please contact the bot owner to get more information.")
                        .AsEphemeral(true));
            };

            slashCommands.RegisterCommands<SlashCommands>(configJson.GuildId);

            var commands = client.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = configJson.Prefixes,
                Services = servCollection.BuildServiceProvider()
            });

            commands.RegisterCommands<NormalCommands>();

            await client.ConnectAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (client == null)
                return;

            await client.DisconnectAsync();
            client.Dispose();
        }
    }
}
