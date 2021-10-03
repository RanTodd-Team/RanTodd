using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ranker
{
    public class BotService : IHostedService
    {
        DiscordClient client;
        IDatabase database;
        ConfigJson configJson;

        private static BotService _instance;

        public BotService(ConfigJson config, IDatabase database)
        {
            configJson = config;
            this.database = database;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            DiscordConfiguration configuration = new()
            {
                Token = Environment.GetEnvironmentVariable("RANKER_TOKEN"),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.GuildMembers | DiscordIntents.GuildMessages | DiscordIntents.Guilds
            };

            DiscordClient client = new(configuration);

            client.AddExtension(new MessageEvent(database));

            ServiceCollection servCollection = new();
            servCollection.AddSingleton(database);

            var slashCommands = client.UseSlashCommands(new()
            {
                Services = servCollection.BuildServiceProvider()
            });

            slashCommands.SlashCommandErrored += async (s, e) =>
            {
                s.Client.Logger.LogError(e.Exception.ToString());

                bool botOwner = s.Client.CurrentApplication.Owners.Any(x => x.Id == e.Context.User.Id);

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithTitle("Error")
                    .WithColor(DiscordColor.Red)
                    .WithDescription(botOwner ? $"```{e.Exception}```" : "Please contact the bot owner to get more information.");

                await e.Context.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent("Something went wrong!")
                    .AddEmbed(embed));
            };

            slashCommands.RegisterCommands<SlashCommands>(configJson.GuildId);

            var commands = client.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = configJson.Prefixes,
                Services = servCollection.BuildServiceProvider()
            });

            commands.RegisterCommands<NormalCommands>();

            client.Ready += (s, e) =>
            {
                s.Logger.LogInformation("Bot is ready!");
                return Task.CompletedTask;
            };

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
